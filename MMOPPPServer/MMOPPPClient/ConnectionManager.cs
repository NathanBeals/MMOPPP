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
using System.ComponentModel;

namespace AIClient
{
    class ConnectionManager
    {
        static List<string> s_ClientNames = new List<string>();
        UdpClient m_UDPClientUp;
        UdpClient m_UDPClientDown;
        Thread m_MessageHandlingThread;
        static ulong s_InputsPerCharacter = 12;

        public void InitGameLoop()
        {
            var rand = new Random();
            var port = rand.Next(1000, 2000);
            m_UDPClientUp = new UdpClient(port);

            IPAddress[] addresslist = Dns.GetHostAddresses(MMOPPPLibrary.Constants.ServerPublicAddress);
            if (addresslist.Length == 0)
                throw new SocketException();

            m_UDPClientUp.Connect("192.168.0.105", 6969); // actual host address not working again, just use my local ip for now
            Console.WriteLine("Server Connection Successful");

            m_UDPClientDown = new UdpClient(port + 1);

            s_ClientNames.Add(Console.ReadLine());

           // while (true)
           SendQueuedPackets(s_ClientNames[0]); // TODO: remove, just for testing

            //while(true)
            //{
            //    Thread.Sleep(10);
            //    //Console.WriteLine("running");
            //    //don't exit
            //    //Console.Clear();
            //}

            //m_MessageHandlingThread = new Thread(HandleMessages);
            //m_MessageHandlingThread.Start();
            HandleMessages();
        }

        void HandleMessages()
        {
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0); // Allows recieving from any port
                while (m_UDPClientDown.Available != 0)
                {
                    Byte[] recieved = m_UDPClientDown.Receive(ref RemoteIpEndPoint); // TODO: recieve async
                    MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_UDPClientDown, ParseWorldUpdate);
                }

                Thread.Sleep(1000);
            }
        }

        bool ParseWorldUpdate(List<Byte> RawBytes, IPEndPoint Sender)
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
                packet.SendPacketUDP(m_UDPClientUp);

            //Console.WriteLine("any key to send next batch");
            //s_ClientNames.Clear();
            //s_ClientNames.Add(Console.ReadLine());
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
                            timestamppacket.SendPacketUDP(m_UDPClientUp);

                            Thread.Sleep(1);
                        }

                        var playerInput = CreatePlayerInput(name);
                        playerInput.Input.SentTime = sendtime + (ulong)(16.66667 * (x + 1));
                        playerInput.Input.PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 1.0f };
                        playerInput.Input.EulerBodyRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
                        playerInput.Input.EulerCameraRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
                        var newPacket = new Packet<ClientInput>(playerInput);
                        inputs.Add(newPacket);
                        newPacket.SendPacketUDP(m_UDPClientUp);

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
