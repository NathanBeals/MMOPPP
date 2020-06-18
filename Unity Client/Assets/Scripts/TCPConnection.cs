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
using MMOPPPShared;
using System.Threading;

public class TCPConnection : MonoBehaviour
{
    Thread m_SendingInputsThread;
    MMOPPPClient m_TestClient;

    #region Variables       
    // Related Components
    Character m_Character;

    // Inputs
    PlayerInputActions2 m_InputActions;
    bool m_Strafe = false;
    bool m_Sprint = false;
    Vector2 m_MovementInput;
    Vector2 m_MouseInput;

    #endregion

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
    }

    public void Update()
    {
         m_TestClient.QueueInput(MMOPPPClient.PackInput(m_Character, m_MovementInput, m_MouseInput, m_Strafe, m_Sprint));
    }

    class MMOPPPClient
    {
        public bool m_ThreadShouldExit = false;
        public List<Packet<PlayerInput>> m_QueuedPackets = new List<Packet<PlayerInput>>();

        public void Connect(string ServerAddress = MMOPPPShared.Constants.ServerAddress, Int32 Port = MMOPPPShared.Constants.ServerUpPort)
        {
            try
            { 
                TcpClient client = new TcpClient(ServerAddress, Port);
                NetworkStream stream = client.GetStream();

                while (!m_ThreadShouldExit)
                {
                    SendQueuedPackets(stream);
                    Debug.Log("I'm Alive");
                    Thread.Sleep(1000);
                }

                stream.Close(); // TODO: shouldn't close here, after one message, this whole block should loop and wait for input
                client.Close();
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
                    packet.SendPacket(Stream);

                m_QueuedPackets.Clear();
            }
        }

        public static PlayerInput CreateTestInput(string Name)
        {
            PlayerInput testInput = new PlayerInput();
            testInput.Id = new Identifier { Name = Name, Tags = "Default" };
            testInput.MoveInput = new EntityInput
            {
                Strafe = false,
                Sprint = false,
                EulerRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
                DirectionInputs = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f }
            };
            testInput.SentTime = new Timestamp { Seconds = DateTime.Now.Second, Nanos = DateTime.Now.Millisecond / 1000000 };

            return testInput;
        }

        public static PlayerInput PackInput(Character Character, Vector2 MoveInput, Vector2 MouseInput, bool Strafe, bool Spring)
        {
            PlayerInput input = new PlayerInput();
            input.Id = new Identifier { Name = Character.m_ID, Tags = "Default" };
            input.MoveInput = new EntityInput
            {
                Strafe = Strafe,
                Sprint = Spring,
                EulerRotation = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = MouseInput.x, Y = MouseInput.y, Z = 0.0f },
                DirectionInputs = new Google.Protobuf.MMOPPP.Messages.Vector3 { X = MoveInput.x, Y = MoveInput.y, Z = 0.0f }
            };
            input.SentTime = new Timestamp { Seconds = DateTime.Now.Second, Nanos = DateTime.Now.Millisecond / 1000000 };

            return input;
        }
    }

    public void OnEnable()
    {
        if (m_TestClient == null)
            m_TestClient = new MMOPPPClient();
        m_TestClient.m_ThreadShouldExit = false;
        m_SendingInputsThread = new Thread(() => m_TestClient.Connect());
        m_SendingInputsThread.Start();

        m_InputActions.Enable();
    }

    public void OnDisable()
    {
        m_TestClient.m_ThreadShouldExit = true;

        m_InputActions.Disable();
    }
}
