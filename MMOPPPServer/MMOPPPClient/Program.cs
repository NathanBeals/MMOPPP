using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using MMOPPPShared;

namespace AIClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MMOPPPCLient();
            client.Connect();
        }
    }

    class MMOPPPCLient
    {
        public void Connect(string ServerAddress = MMOPPPShared.Constants.ServerAddress, Int32 Port = MMOPPPShared.Constants.ServerUpPort)
        {
            try
            {
                TcpClient client = new TcpClient(ServerAddress, Port);
                NetworkStream stream = client.GetStream();

                SendQueuedPackets(stream);

                stream.Close(); // TODO: shouldn't close here, after one message, this whole block should loop and wait for input
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    
        public void SendQueuedPackets(NetworkStream stream)
        {
            //Debug code, making a packet
            PlayerInput testInput = CreatePlayerInput("Nate");
            Packet<PlayerInput> packet = new Packet<PlayerInput>
            {
                m_Size = testInput.CalculateSize(),
                m_Message = testInput
            };

            Console.WriteLine("Packet is this size: {0}", testInput.CalculateSize()); //TODO: remove

            packet.SendPacket(stream);
        }

        public PlayerInput CreatePlayerInput(string Name)
        {
            PlayerInput testInput = new PlayerInput();
            testInput.Id = new Identifier { Name = Name, Tags = "Default" };
            testInput.MoveInput = new EntityInput
            {
                Strafe = false,
                Sprint = false,
                EulerRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
                DirectionInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f }
            };
            testInput.SentTime = new Timestamp { Seconds = DateTime.Now.Second, Nanos = DateTime.Now.Millisecond / 1000000 };

            return testInput;
        }
    }
}
