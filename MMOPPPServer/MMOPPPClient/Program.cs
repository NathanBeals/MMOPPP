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

namespace AIClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MMOPPPClient();
            client.Connect();
        }
    }

    class MMOPPPClient
    {
        public void Connect(string ServerAddress = MMOPPPLibrary.Constants.ServerAddress, Int32 Port = MMOPPPLibrary.Constants.ServerUpPort)
        {
            try
            {
                TcpClient client = new TcpClient(ServerAddress, Port);
                NetworkStream stream = client.GetStream();

                SendQueuedPackets(stream);

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();

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
            Packet<PlayerInput> packet = new Packet<PlayerInput>(testInput);
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
            }; //UTC UTC.Now
            testInput.SentTime = new Timestamp { Seconds = DateTime.UtcNow.ToBinary(),  Nanos = DateTime.Now.Millisecond / 1000000 }; // TODO: this is not what you think it is

            return testInput;
        }
    }
}
