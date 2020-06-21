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
        TcpClient m_ServerConnection = new TcpClient();
        List<Byte> m_QueuedData = new List<Byte>();
        bool m_ThreadsShouldExit = false; // Hack, never set

        List<WorldUpdate> m_WorldUpdates = new List<WorldUpdate>();
        Thread m_MessageHandlingThread;

        enum ERecievingState
        {
            Frame,
            Message
        }

        public void HandleMessages()
        {
            while (!m_ThreadsShouldExit)
            {
                HandleMessage(m_ServerConnection);
            }
        }

        public void HandleMessage(TcpClient Client)
        {
            var client = Client;
            var queuedData = m_QueuedData;
            if (client.Available == 0)
                return;

            Int32 messageSize = 0;
            byte[] buffer = new byte[Constants.TCPBufferSize];
            byte[] lengthData = new byte[Constants.HeaderSize];
            Array.Copy(queuedData.ToArray(), buffer, queuedData.Count);
            ERecievingState recievingState = ERecievingState.Frame;
            int dataAvailable = 0;

            try // Make sure to catch if the client is DCed in here
            {
                NetworkStream stream = client.GetStream();
                dataAvailable = client.Available;
                stream.Read(buffer, queuedData.Count, dataAvailable);
            }
            catch (System.IO.IOException) //TODO: look up client dc error
            {
                return;
            }
            queuedData.Clear();

            while (!m_ThreadsShouldExit)
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
                                messageSize = BitConverter.ToInt32(lengthData);

                                // Remove header from buffer
                                Array.Copy(buffer, Constants.HeaderSize, buffer, 0, buffer.Length - Constants.HeaderSize);

                                recievingState = ERecievingState.Message;
                                dataAvailable -= Constants.HeaderSize;
                            }
                            else // If the remaining data is smaller than the header size, push it onto the data to be parsed later
                            {
                                m_QueuedData = buffer.ToList(); // TODO: investigate the impact of the array to list conversions on performance... and just usage in general (not sure how to use)
                            }
                        }
                        break;

                    case ERecievingState.Message:
                        {
                            if (dataAvailable >= messageSize)
                            {
                                // Put the message bytes into a data object
                                List<byte> data = new List<byte>();
                                data.AddRange(buffer);
                                data.RemoveRange(messageSize, buffer.Length - messageSize);

                                //// Parse the message bytes and add it to the inputs list
                                lock (m_WorldUpdates)
                                {
                                    m_WorldUpdates.Add(WorldUpdate.Parser.ParseFrom(data.ToArray()));
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
                                m_QueuedData = buffer.ToList();
                            }
                        }
                        break;
                }
            }
        }

        public void Connect(string ServerAddress = MMOPPPLibrary.Constants.ServerAddress, Int32 Port = MMOPPPLibrary.Constants.ServerUpPort)
        {
            try
            {
                m_ServerConnection = new TcpClient(ServerAddress, Port);
                NetworkStream stream = m_ServerConnection.GetStream();

                m_MessageHandlingThread = new Thread(HandleMessages);
                m_MessageHandlingThread.Start();

                SendQueuedPackets(stream);

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();

                stream.Close(); // TODO: shouldn't close here, after one message, this whole block should loop and wait for input
                m_ServerConnection.Close();
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
            };
            DateTimeOffset now = DateTime.UtcNow;
            testInput.SentTime = new Timestamp { Seconds = (now.Ticks / 10000000) - 11644473600L, Nanos = (int)(now.Ticks % 10000000) * 100};

            return testInput;
        }
    }
}
