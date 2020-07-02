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
  PlayerInputActions2 m_InputActions;
  bool m_Strafe = false;
  bool m_Sprint = false;
  Vector2 m_MovementInput;
  Vector2 m_MouseInput;

  private void Awake()
  {
    Cursor.lockState = CursorLockMode.Locked;
    m_Character = GetComponent<Character>();

    m_InputActions = new PlayerInputActions2();
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
      m_Camera.gameObject.transform.rotation.eulerAngles, // HACK: allowing client to be authoritative on input
      m_Strafe,
      m_Sprint));

    var wUpdate = m_TestClient.PopWorldUpdate();
    if (wUpdate != null)
      WorldServerSync.s_Instance.QueueNewUpdate(wUpdate);
  }

  class MMOPPPClient
  {
    public bool m_ThreadsShouldExit = false;
    public List<Packet<PlayerInput>> m_QueuedPackets = new List<Packet<PlayerInput>>();
    TcpClient m_ServerConnection = new TcpClient();
    List<Byte> m_QueuedData = new List<Byte>();
    List<WorldUpdate> m_WorldUpdates = new List<WorldUpdate>();

    public void Connect(string ServerAddress = MMOPPPLibrary.Constants.ServerPublicAddress, Int32 Port = MMOPPPLibrary.Constants.ServerPort)
    {
      try
      {
        m_ServerConnection = new TcpClient(ServerAddress, Port);
        NetworkStream stream = m_ServerConnection.GetStream();
        m_ServerConnection.ReceiveBufferSize = Constants.TCPBufferSize;

        while (!m_ThreadsShouldExit)
        {
          SendQueuedPackets(stream);
          Debug.Log("I'm Alive");
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

    public void QueueInput(PlayerInput Input) 
    {
      lock (m_QueuedPackets)
      {
        m_QueuedPackets.Add(new Packet<PlayerInput>(Input));
      }
    }

    public void SendQueuedPackets(NetworkStream Stream)
    {
      lock (m_QueuedPackets)
      {
        foreach (var packet in m_QueuedPackets)
        {
          packet.SendPacket(Stream);
        }

        m_QueuedPackets.Clear();
      }
    }

    public static PlayerInput PackInput(Character Character, Vector2 MoveInput, UnityEngine.Vector3 Rotation, bool Strafe, bool Spring)
    {
      PlayerInput input = new PlayerInput();
      input.Id = new Identifier { Name = Character.m_ID, Tags = "Default" };
      input.MoveInput = new EntityInput
      {
        Strafe = Strafe,
        Sprint = Spring,
        EulerRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = Rotation.x, Y = Rotation.y, Z = Rotation.z },
        DirectionInputs = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = MoveInput.x, Y = 0.0f, Z = MoveInput.y }
      };
      DateTimeOffset now = DateTime.UtcNow;
      input.SentTime = new Timestamp { Seconds = (now.Ticks / 10000000) - 11644473600L, Nanos = (int)(now.Ticks % 10000000) * 100 };

      return input;
    }

    enum ERecievingState
    {
      Frame,
      Message
    }

    public void HandleMessages(int StartDelay)
    {
      Thread.Sleep(StartDelay);
      while (!m_ThreadsShouldExit)
        HandleMessage(m_ServerConnection);
    }

    public void HandleMessage(TcpClient Client) // HACK: duplicated code, see client manager in server code
    {
      var client = Client;
      var queuedData = m_QueuedData;
      if (client.Available == 0)
        return;

      Int32 messageSize = 0;
      byte[] buffer = new byte[Constants.TCPBufferSize];
      byte[] lengthData = new byte[Constants.HeaderSize];
      ERecievingState recievingState = ERecievingState.Frame;
      int dataAvailable = 0;

      try // Make sure to catch if the client is DCed in here
      {
        NetworkStream stream = client.GetStream();
        dataAvailable = client.Available;
        stream.Read(buffer, queuedData.Count, Math.Min(dataAvailable, Constants.TCPBufferSize - queuedData.Count));
      }
      catch (System.IO.IOException) //TODO: look up client dc error
      {
        return;
      }
      Array.Copy(queuedData.ToArray(), buffer, queuedData.Count);
      queuedData.Clear();

      bool dataNotComplete = false;
      while (!m_ThreadsShouldExit && !dataNotComplete)
      {
        //Normal Exit, data source exhausted
        if (dataAvailable == 0)
          break;

        switch (recievingState)
        {
          case ERecievingState.Frame:
            {
              if (dataAvailable > Constants.HeaderSize)
              {
                // Get size of message from header
                Array.Copy(buffer, lengthData, Constants.HeaderSize);
                if (Constants.SystemIsLittleEndian != Constants.MessageIsLittleEndian)
                  lengthData.Reverse();
                messageSize = BitConverter.ToInt32(lengthData, 0);

                recievingState = ERecievingState.Message;
              }
              else
              {
                m_QueuedData.AddRange(buffer.SubArray(0, dataAvailable));
                dataNotComplete = true;
              }
            }
            break;

          case ERecievingState.Message:
            {
              if (dataAvailable >= messageSize + Constants.HeaderSize)
              {
                // Remove header from buffer
                Array.Copy(buffer, Constants.HeaderSize, buffer, 0, buffer.Length - Constants.HeaderSize);
                dataAvailable -= Constants.HeaderSize;

                // Put the message bytes into a data object
                List<byte> data = new List<byte>();
                data.AddRange(buffer.SubArray(0, messageSize));

                // Parse the message bytes and add it to the inputs list
                lock (m_WorldUpdates)
                {
                  try
                  {
                    m_WorldUpdates.Add(WorldUpdate.Parser.ParseFrom(data.ToArray()));
                  }
                  catch (Exception e) // If the input fails just clear the entire stream
                  {
                    Console.WriteLine("Malformed world update message.");
                    break;
                  }
                }

                // Remove message from buffer
                Array.Copy(buffer, messageSize, buffer, 0, buffer.Length - messageSize);

                dataAvailable -= messageSize;
                messageSize = 0;
                recievingState = ERecievingState.Frame;

                //Debbuging
                Console.WriteLine($"World Updated {WorldUpdate.Parser.ParseFrom(data.ToArray()).ToString()}");
              }
              else // If the remaining data is smaller than the message size, push it onto the data to be parsed later
              {
                m_QueuedData.AddRange(buffer.SubArray(0, dataAvailable));
                dataNotComplete = true;
              }
            }
            break;
        }
      }
    }

    public WorldUpdate PopWorldUpdate()
    {
      WorldUpdate frontUpdate = null;
      lock (m_WorldUpdates)
      {
        if (m_WorldUpdates.Count != 0)
        {
          frontUpdate = m_WorldUpdates[0];
          m_WorldUpdates.RemoveAt(0);
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
    m_RecievingInputsThread = new Thread(() => m_TestClient.HandleMessages(2000)); // HACK: fixes single player lag bug
    m_RecievingInputsThread.Start();

    m_InputActions.Enable();
  }

  public void OnDisable()
  {
    m_TestClient.m_ThreadsShouldExit = true;

    m_InputActions.Disable();
  }
}
