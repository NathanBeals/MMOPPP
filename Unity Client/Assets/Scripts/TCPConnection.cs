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
using TMPro;

//TODO: cleanup
//TODO: this is doing double duty for client inputs and server updates but the server synce code is external (client updates should be external too)

public class TCPConnection : MonoBehaviour //TODO: rename to udp connecton, or something else
{
    // Threads
    Thread m_SendingInputsThread;
    Thread m_RecievingInputsThread;

    // Related Components
    Character m_Character;
    Camera m_Camera;

    // Inputs
    PlayerInputActions m_InputActions; //TODO: get rid of original, it's lived long enough
    bool m_Strafe = false;
    bool m_Sprint = false;
    Vector2 m_MovementInput;
    Vector2 m_MouseInput;

    // Server
    public bool m_ThreadsShouldExit = false;
    public List<Packet<ClientInput>> m_QueuedPackets = new List<Packet<ClientInput>>();
    UdpClient m_UDPToServer; //TODO: bad names
    UdpClient m_UDPFromServer;
    List<ServerUpdates> m_ServerUpdates = new List<ServerUpdates>();
    public int m_RandomUpPort = -1;

    // Lag
    public float m_ArtificialLag = 0;

    public void ConnectionAwake()
    {
        //Generate random up port (allows for testing from same conputer/ip address)
        m_RandomUpPort = UnityEngine.Random.Range(1000, 20000);

        //IPAddress[] addresslist = Dns.GetHostAddresses(MMOPPPLibrary.Constants.ServerPublicAddress); //TODO: put back when I fix my server (physical server)
        //if (addresslist.Length == 0)
        //    throw new SocketException();

        m_UDPToServer = new UdpClient(m_RandomUpPort);
        m_UDPToServer.Connect("192.168.0.105"/*addresslist[0].ToString()*/, MMOPPPLibrary.Constants.ServerPort);

        m_UDPFromServer = new UdpClient(m_RandomUpPort + 1);
        //m_UDPToServer.Connect("192.168.0.105"/*addresslist[0].ToString()*/, m_RandomUpPort);
    }

    public void SendingInputs(string ServerAddress = MMOPPPLibrary.Constants.ServerPublicAddress, Int32 Port = MMOPPPLibrary.Constants.ServerPort)
    {
        try
        {
            while (!m_ThreadsShouldExit)
                SendQueuedPackets();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void QueueInput(ClientInput Input)
    {
        StartCoroutine(SendPacketsLagged(Input));
    }

    public void SendQueuedPackets()
    {
        lock (m_QueuedPackets)
        {
            int size = m_QueuedPackets.Count;
            foreach (var packet in m_QueuedPackets)
                packet.SendPacketUDP(m_UDPToServer);
            m_QueuedPackets.Clear();
            if (m_Character != null && m_QueuedPackets.Count > 0) // Can't do this here
                m_QueuedPackets.Add(new Packet<ClientInput>(CreateTimeStampPacket()));
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
            SentTime = (ulong)DateTime.UtcNow.Ticks / 10000
        };

        return input;
    }

    public bool ParseServerUpdates(List<Byte> RawBytes, IPEndPoint Sender)
    {
        lock (m_ServerUpdates)
        {
            try
            {
                m_ServerUpdates.Add(ServerUpdates.Parser.ParseFrom(RawBytes.ToArray()));
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        return false;
    }

    public void HandleMessages(int StartDelay)
    {
        Thread.Sleep(StartDelay);
        while (!m_ThreadsShouldExit)
        {
           MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_UDPFromServer, ParseServerUpdates);
            Thread.Sleep(100);
        }
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

    public void OnEnable()
    {
        m_ThreadsShouldExit = false;
        m_SendingInputsThread = new Thread(() => SendingInputs());
        m_SendingInputsThread.Start();
        m_RecievingInputsThread = new Thread(() => HandleMessages(1000)); // HACK: fixes single player lag bug
        m_RecievingInputsThread.Start();

        m_InputActions.Enable();
    }

    public void OnDisable()
    {
        m_ThreadsShouldExit = true;
        m_InputActions.Disable();
    }

    private void Awake()
    {
        ConnectionAwake();

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
        Debug.Log("Attempt Queue Input");
        var packedInput = PackInput(m_Character,
          m_MovementInput,
          m_Character.gameObject.transform.rotation.eulerAngles,
          m_Camera.gameObject.transform.rotation.eulerAngles,
          m_Strafe,
          m_Sprint);

        QueueInput(packedInput);
        m_Character.RecordLocalInput(packedInput);

        var serverUpdate = PopServerUpdate();
        if (serverUpdate != null)
            WorldServerSync.s_Instance.QueueNewUpdate(serverUpdate);
    }

    IEnumerator SendPacketsLagged(ClientInput Input)
    {
        yield return new WaitForSeconds(m_ArtificialLag);

        lock (m_QueuedPackets)
            m_QueuedPackets.Add(new Packet<ClientInput>(Input));
    }

    // This creates a packet out of only timestam
    // This is pushed to the fron of every new packet queue to allow the server to use only timestamps to calculate delta times
    // It essentaily exists to be ignored, but because it delivers a timestamp, the deltatime (new update time - prev update time) can be calculated properly for other packets
    public ClientInput CreateTimeStampPacket()
    {
        ClientInput input = new ClientInput();
        input.Name = m_Character.m_ID;
        input.Input = new Google.Protobuf.MMOPPP.Messages.Input
        {
            Strafe = false,
            Sprint = false,
            PlayerMoveInputs = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = 0, Y = 0, Z = 0 },
            EulerBodyRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = 0, Y = 0, Z = 0 },
            EulerCameraRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = 0, Y = 0, Z = 0 },
            SentTime = (ulong)DateTime.UtcNow.Ticks / 10000
        };

        return input;
    }
}
