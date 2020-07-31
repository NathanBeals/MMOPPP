using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using MMOPPPLibrary;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Data;

namespace AIClient
{
    class ConnectionManager
    {
        static List<string> s_ClientNames = new List<string>();
        UdpClient m_UDPClient = new UdpClient(420);// MMOPPPLibrary.Constants.ServerPort); // TODO: client port number?
        bool m_ThreadsShouldExit = false; // Hack: never set
        Thread m_MessageHandlingThread;
        static ulong s_InputsPerCharacter = 12;

        public void InitGameLoop()
        {
            s_ClientNames.Add(Console.ReadLine());
            m_UDPClient.Connect(MMOPPPLibrary.Constants.ServerPublicAddress, 6969);

            Console.WriteLine("Server Connection Successful");

            IPAddress[] addresslist = Dns.GetHostAddresses(MMOPPPLibrary.Constants.ServerPublicAddress);
            if (addresslist.Length == 0)
                throw new SocketException();

            m_MessageHandlingThread = new Thread(HandleMessages);
            m_MessageHandlingThread.Start();

            while (true)
                SendQueuedPackets(s_ClientNames[0]); // TODO: remove, just for testing

            //GameLoop();
        }

        void HandleMessages()
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0); // Allows recieving from any port
            while (m_UDPClient.Available != 0)
            {
                Byte[] recieved = m_UDPClient.Receive(ref RemoteIpEndPoint); // TODO: recieve async
                MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_UDPClient, ParseWorldUpdate);
            }
        }

        bool ParseWorldUpdate(List<Byte> RawBytes)
        {
            try
            {
                Google.Protobuf.MMOPPP.Messages.ServerUpdates.Parser.ParseFrom(RawBytes.ToArray());
                Console.WriteLine("World Update Parsed");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        void SendQueuedPackets(string Name)
        {
            //Debug code, making a packet
            ClientInput testInput = CreatePlayerInput(Name);
            Packet<ClientInput> packet = new Packet<ClientInput>(testInput);

            //Debug
            Console.WriteLine("Packet is this size: {0}", testInput.CalculateSize());

            for (var x = 0; x < 1000; x++)
                packet.SendPacketUDP(m_UDPClient);

            Console.WriteLine("any key to send next batch");
            Console.ReadLine();
        }

        ClientInput CreatePlayerInput(string Name)
        {
            ClientInput testInput = new ClientInput();
            testInput.Name = Name;
            testInput.Input = new Input
            {
                Strafe = false,
                Sprint = false,
                PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
                EulerBodyRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
                EulerCameraRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
                SentTime = (ulong)DateTime.UtcNow.Ticks / 10000
            };

            return testInput;
        }

        void GameLoop()
        {
            float rot = 0;
            while (true)
            {
                List<Packet<ClientInput>> inputs = new List<Packet<ClientInput>>();
                ulong sendtime = ((ulong)DateTime.UtcNow.Ticks / 10000);
                foreach (var name in s_ClientNames)
                {
                    for (ulong x = 0; x < s_InputsPerCharacter; ++x)
                    {
                        if (x == 0)
                        {
                            var timestampinput = CreatePlayerInput(name);
                            timestampinput.Input.SentTime = sendtime;
                            timestampinput.Input.PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 1.0f };
                            timestampinput.Input.EulerBodyRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
                            timestampinput.Input.EulerCameraRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
                            var timestamppacket = new Packet<ClientInput>(timestampinput);
                            inputs.Add(timestamppacket);
                            timestamppacket.SendPacketUDP(m_UDPClient);

                            Thread.Sleep(1);
                        }

                        var playerInput = CreatePlayerInput(name);
                        playerInput.Input.SentTime = sendtime + (ulong)(16.66667 * (x + 1));
                        playerInput.Input.PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 1.0f };
                        playerInput.Input.EulerBodyRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
                        playerInput.Input.EulerCameraRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
                        var newPacket = new Packet<ClientInput>(playerInput);
                        inputs.Add(newPacket);
                        newPacket.SendPacketUDP(m_UDPClient);

                        Thread.Sleep(1);
                    }
                }

                int sum = 0;
                foreach (var packet in inputs)
                    sum += packet.GetPacketSize();

                rot += 20;
                if (rot > 360)
                    rot = 0;

                Console.WriteLine("!!! Size: " + sum + " !!!");
                Thread.Sleep(MMOPPPLibrary.Constants.ServerTickRate);
            }
        }
    }
}
