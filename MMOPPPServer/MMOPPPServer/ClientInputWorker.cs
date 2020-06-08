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

namespace MMOPPPServer
{
    class ClientInputWorker
    {
        Thread m_MessageHandlingThread;
        Thread m_ConnectionHandlingThread;

        List<TcpClient> m_Clients = new List<TcpClient>(); //TODO: mutex guard
        List<PlayerInput> m_Inputs = new List<PlayerInput>(); //TODO: mutex guard

        public ClientInputWorker()
        {
            m_ConnectionHandlingThread = new Thread(HandleConnections);
            m_ConnectionHandlingThread.Start();

            m_MessageHandlingThread = new Thread(HandleMessages);
            m_MessageHandlingThread.Start();
        }

        ~ClientInputWorker()
        {
            m_MessageHandlingThread.Abort(); //TODO: there's a better way to handle this
        }

        public List<PlayerInput> GetInputs()
        {
            Monitor.Enter(m_Inputs);
            var InputsCopy = new List<PlayerInput>(m_Inputs);
            Monitor.Exit(m_Inputs);

            return InputsCopy;
        }

        public void ClearInputs()
        {
            Monitor.Enter(m_Inputs);
            m_Inputs.Clear();
            Monitor.Exit(m_Inputs);
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

                    Monitor.Enter(m_Clients); // it's failing here
                    m_Clients.Add(client);
                    Monitor.Exit(m_Clients);

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
                Monitor.Enter(m_Clients);

                foreach (var client in m_Clients)
                    HandleMessage(client);

                Monitor.Exit(m_Clients);
            }
        }

        //TODO: I'm still worried about the spliting packets problem
        public void HandleMessage(TcpClient Client)
        {
                ERecievingState recievingState = ERecievingState.Frame;
                NetworkStream stream = Client.GetStream();

                Int32 messageSize = 0;
                byte[] buffer = new byte[Constants.TCPBufferSize];
                byte[] lengthData = new byte[Constants.HeaderSize];

                int i;
                while ((i = stream.Read(buffer, 0, Constants.TCPBufferSize)) != 0) { };

                var remainingBufferToParse = Constants.TCPBufferSize;
            while (remainingBufferToParse > 0)
            {
                switch (recievingState)
                {
                    case ERecievingState.Frame:
                        {
                            if (buffer.Length > Constants.HeaderSize)
                            {
                                // Get size of message from header
                                Array.Copy(buffer, lengthData, Constants.HeaderSize);
                                if (Constants.SystemIsLittleEndian != Constants.MessageIsLittleEndian)
                                    lengthData.Reverse();
                                messageSize = BitConverter.ToInt32(lengthData);

                                // No more messages signal, early exit
                                if (messageSize == 0)
                                    break;

                                // Remove header from buffer
                                Array.Copy(buffer, Constants.HeaderSize, buffer, 0, buffer.Length - Constants.HeaderSize);

                                remainingBufferToParse -= Constants.HeaderSize;
                                recievingState = ERecievingState.Message;
                            }
                        }
                        break;

                    case ERecievingState.Message:
                        {
                            // Put the message bytes into a data object
                            List<byte> data = new List<byte>();
                            data.AddRange(buffer);
                            data.RemoveRange(messageSize, buffer.Length - messageSize);

                            // Parse the message bytes and add it to the inputs list
                            Monitor.Enter(m_Inputs);
                            m_Inputs.Add(PlayerInput.Parser.ParseFrom(data.ToArray()));
                            Monitor.Exit(m_Inputs);

                            // Remove message from buffer
                            Array.Copy(buffer, messageSize, buffer, 0, buffer.Length - messageSize);

                            remainingBufferToParse -= messageSize;
                            messageSize = 0;
                            recievingState = ERecievingState.Frame;
                        }
                        break;
                }
            }
        }
    }
}
