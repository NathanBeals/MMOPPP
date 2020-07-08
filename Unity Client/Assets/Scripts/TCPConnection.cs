using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;
using MMOPPPLibrary;
using System.Threading;

public class TCPConnection : MonoBehaviour
{
  // Threads
  Thread m_SendingInputsThread;
  Thread m_RecievingInputsThread;
  MMOPPPClient m_TestClient;
  static int m_UpdateTickRateMS = 100;

  // Related Components
  Character m_Character;
  Camera m_Camera;

  // Inputs
  PlayerInputActions  m_InputActions; //TODO: get rid of original, it's lived long enough
  bool m_Strafe = false;
  bool m_Sprint = false;
  Vector2 m_MovementInput;
  Vector2 m_MouseInput;

  private void Awake()
  {
    Cursor.lockState = CursorLockMode.Locked;
    m_Character = GetComponent<Character>();

    m_InputActions = new PlayerInputActions();
    //m_InputActions.PlayerControls.Jump.performed += ctx => JumpInput(); //TODO: add in future
    m_InputActions.PlayerControls.Strafe.started += ctx => { m_Strafe = true; };
    m_InputActions.PlayerControls.Strafe.canceled += ctx => { m_Strafe = false; };
    m_InputActions.PlayerControls.Sprint.started += ctx => { m_Sprint = true; };
    m_InputActions.PlayerControls.Sprint.canceled += ctx => { m_Sprint = false; };
    m_InputActions.PlayerControls.Move.performed += ctx => { m_MovementInput = ctx.ReadValue<Vector2>(); };
    m_InputActions.PlayerControls.Rotate.performed += ctx => { m_MouseInput = ctx.ReadValue<Vector2>(); };
  }

  private void Start()
  {
    m_Character = GameObject.FindObjectOfType<Character>();
    if (m_Character == null)
      Debug.Log("No character in scene");

    m_Camera = GameObject.FindObjectOfType<Camera>();
    if (m_Camera == null)
      Debug.Log("No camera in scene");
  }

  public void Update()
  {
    m_TestClient.QueueInput(MMOPPPClient.PackInput(m_Character,
      m_MovementInput,
      m_Character.gameObject.transform.rotation.eulerAngles,
      m_Camera.gameObject.transform.rotation.eulerAngles,
      m_Strafe,
      m_Sprint));

    var serverUpdate = m_TestClient.PopServerUpdate();
    if (serverUpdate != null)
      WorldServerSync.s_Instance.QueueNewUpdate(serverUpdate);
  }

  //TODO: why, besides copying code, is this in a nested class?
  class MMOPPPClient
  {
    public bool m_ThreadsShouldExit = false;
    public List<Packet<ClientInput>> m_QueuedPackets = new List<Packet<ClientInput>>();
    TcpClient m_ServerConnection = new TcpClient();
    List<Byte> m_QueuedData = new List<Byte>();
    List<ServerUpdates> m_ServerUpdates = new List<ServerUpdates>();

    public void Connect(string ServerAddress = MMOPPPLibrary.Constants.ServerPublicAddress, Int32 Port = MMOPPPLibrary.Constants.ServerPort)
    {
      try
      {
        IPAddress[] addresslist = Dns.GetHostAddresses(MMOPPPLibrary.Constants.ServerPublicAddress);
        if (addresslist.Length == 0)
          throw new SocketException();

        m_ServerConnection = new TcpClient(addresslist[0].ToString(), Port);
        NetworkStream stream = m_ServerConnection.GetStream();
        m_ServerConnection.ReceiveBufferSize = Constants.TCPBufferSize;

        while (!m_ThreadsShouldExit)
        {
          SendQueuedPackets(stream);
          Thread.Sleep(m_UpdateTickRateMS);
        }

        stream.Close();
        m_ServerConnection.Close();
      }
      catch (ArgumentNullException e)
      {
        Debug.LogException(e);
      }
      catch (SocketException e)
      {
        Debug.LogException(e);
      }
    }

    public void QueueInput(ClientInput Input) 
    {
      lock (m_QueuedPackets)
      {
        m_QueuedPackets.Add(new Packet<ClientInput>(Input));
      }
    }

    public void SendQueuedPackets(NetworkStream Stream)
    {
      lock (m_QueuedPackets)
      {
        Packet<ClientInput>.SendPacketBatch(Stream, m_QueuedPackets);
        m_QueuedPackets.Clear();
      }
    }

    public static ClientInput PackInput(Character Character, Vector2 MoveInput, UnityEngine.Vector3 BodyRotation, UnityEngine.Vector3 CameraRotation, bool Strafe, bool Spring)
    {
      ClientInput input = new ClientInput();
      input.Name = Character.m_ID;
      input.Input = new Google.Protobuf.MMOPPP.Messages.Input
      {
        Strafe = Strafe,
        Sprint = Spring,
        PlayerMoveInputs = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = MoveInput.x, Y = 0.0f, Z = MoveInput.y },
        EulerBodyRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = BodyRotation.x, Y = BodyRotation.y, Z = BodyRotation.z },
        EulerCameraRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = CameraRotation.x, Y = CameraRotation.y, Z = CameraRotation.z },
        SentTime = new Timestamp { Seconds = (DateTime.UtcNow.Ticks / 10000000) - 11644473600L, Nanos = (int)(DateTime.UtcNow.Ticks % 10000000) * 100 }
    };

      return input;
    }

    public bool ParseServerUpdates(List<Byte> RawBytes, TcpClient ClientConnection)
    {
      lock (m_ServerUpdates)
      {
        try
        {
          m_ServerUpdates.Add(ServerUpdates.Parser.ParseFrom(RawBytes.ToArray()));
          return true;
        }
        catch (Exception e) // If the input fails just clear the entire stream
        {
          Console.WriteLine(e);
        }
      }

      return false;
    }

    public void HandleMessages(int StartDelay)
    {
      Thread.Sleep(StartDelay);
      while (!m_ThreadsShouldExit)
        MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_ServerConnection, m_QueuedData, ParseServerUpdates);
    }

    public ServerUpdates PopServerUpdate()
    {
      ServerUpdates frontUpdate = null;
      lock (m_ServerUpdates)
      {
        if (m_ServerUpdates.Count != 0)
        {
          frontUpdate = m_ServerUpdates[0];
          m_ServerUpdates.RemoveAt(0);
        }
      }
      return frontUpdate;
    }
  }

  public void OnEnable()
  {
    if (m_TestClient == null)
      m_TestClient = new MMOPPPClient();
    m_TestClient.m_ThreadsShouldExit = false;
    m_SendingInputsThread = new Thread(() => m_TestClient.Connect());
    m_SendingInputsThread.Start();
    m_RecievingInputsThread = new Thread(() => m_TestClient.HandleMessages(1000)); // HACK: fixes single player lag bug
    m_RecievingInputsThread.Start();

    m_InputActions.Enable();
  }

  public void OnDisable()
  {
    m_TestClient.m_ThreadsShouldExit = true;

    m_InputActions.Disable();
  }
}
