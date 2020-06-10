using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using System.Linq.Expressions;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Reflection;
using MMOPPPShared;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace MMOPPPServer
{
    class ClientsManager //TODO: this will also control the downlinks so... bad name again
    {
        Thread m_MessageHandlingThread;
        Thread m_ConnectionHandlingThread;
        Thread m_BroadcastHandlingThread;

        List<TcpClient> m_Clients = new List<TcpClient>(); //TODO: mutex guard
        List<List<Byte>> m_QueuedData = new List<List<byte>>();
        List<PlayerInput> m_Inputs = new List<PlayerInput>(); //TODO: mutex guard

        object WorldUpdateLock = new object();
        WorldUpdate m_QueuedWorldUpdate = null;

        public ClientsManager()
        {
            m_ConnectionHandlingThread = new Thread(HandleConnections);
            m_ConnectionHandlingThread.Start();

            m_MessageHandlingThread = new Thread(HandleMessages);
            m_MessageHandlingThread.Start();

            m_BroadcastHandlingThread = new Thread(BroadcastWorldUpdate);
            m_BroadcastHandlingThread.Start();
        }

        ~ClientsManager()
        {
            lock (m_Clients)
            {
                foreach (var client in m_Clients)
                    client.Close();
                m_Clients.Clear();
            }

            //TODO: there's a better way to handle this
            m_ConnectionHandlingThread.Abort();
            m_MessageHandlingThread.Abort();
            m_BroadcastHandlingThread.Abort();
        }

        public List<PlayerInput> GetInputs()
        {
            List<PlayerInput> InputsCopy;
            lock (m_Inputs)
            {
                InputsCopy = new List<PlayerInput>(m_Inputs);
            }

            return InputsCopy;
        }

        public void ClearInputs()
        {
            lock (m_Inputs)
            {
                m_Inputs.Clear();
            }
        }

        enum ERecievingState
        {
            Frame,
            Message
        }
        
        public void HandleConnections()
        {
            TcpListener connectionServer = new TcpListener(IPAddress.Parse(Constants.ServerAddress), Constants.ServerUpPort);
            connectionServer.Start();

            try
            {
                while (true)
                {
                    TcpClient client = connectionServer.AcceptTcpClient();

                    lock (m_Clients)
                    {
                        m_Clients.Add(client);
                        m_QueuedData.Add(new List<byte>());
                    }

                    Console.WriteLine("Connected!");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                connectionServer.Stop();
            }
        }

        public void HandleMessages() 
        {
            while (true)
            {
                lock (m_Clients)
                {
                    for (int i = 0; i < m_Clients.Count; ++i)
                    {
                        if (!m_Clients[i].Connected) //TODO: remove disconnected from the list
                            continue;
                        HandleMessage(i);
                    }

                    m_Clients.RemoveAll(x => !x.Connected); // Clean the list of dead connections
                }
            }
        }

        //TODO: consider adding locks to inputs and clients inside this function, it's private so it can be handled easy enough for now, but hmmm... granularity
        void HandleMessage(int ClientIndex)
        {
            var client = m_Clients[ClientIndex];
            var queuedData = m_QueuedData[ClientIndex];

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
            catch(System.IO.IOException) //TODO: look up client dc error
            {
                return;
            }
            queuedData.Clear();


            while (true)
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
                                m_QueuedData[ClientIndex] = buffer.ToList(); // TODO: investigate the impact of the array to list conversions on performance... and just usage in general (not sure how to use)
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

                                // Parse the message bytes and add it to the inputs list
                                lock (m_Inputs)
                                {
                                    m_Inputs.Add(PlayerInput.Parser.ParseFrom(data.ToArray()));
                                }

                                // Remove message from buffer
                                Array.Copy(buffer, messageSize, buffer, 0, buffer.Length - messageSize);

                                dataAvailable -= messageSize;
                                messageSize = 0;
                                recievingState = ERecievingState.Frame;
                            }
                            else // If the remaining data is smaller than the message size, push it onto the data to be parsed later
                            {
                                m_QueuedData[ClientIndex] = buffer.ToList();
                            }
                        }
                        break;
                }
            }
        }
    
        public void QueueWorldUpdate(WorldUpdate Update)
        {
            lock(WorldUpdateLock)
            {
                m_QueuedWorldUpdate = Update;
            }
        }

        void BroadcastWorldUpdate()
        {
            while(true)
            {
                lock(WorldUpdateLock)
                {
                    if (m_QueuedWorldUpdate != null)
                    {
                        lock(m_Clients)
                        {
                            foreach (var client in m_Clients)
                            {
                                Packet<WorldUpdate> packet = new Packet<WorldUpdate>(m_QueuedWorldUpdate);
                                try
                                {
                                    packet.SendPacket(client.GetStream());
                                }
                                catch { }
                            }
                        }
                        m_QueuedWorldUpdate = null;
                    }
                }
            }
        }
    }
}
