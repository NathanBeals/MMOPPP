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

namespace MMOPPPServer
{
    class ClientInputWorker
    {
        Thread m_WorkerThread;

        public ClientInputWorker()
        {
            m_WorkerThread = new Thread(AcceptClients);
            m_WorkerThread.Start();
        }

        ~ClientInputWorker()
        {
            m_WorkerThread.Abort(); //TODO: there's a better way to handle this
        }

        enum ERecievingState
        {
            Frame,
            Message
        }

        public static void AcceptClients() //TODO: the connections and the processing of the messages needs to be separated
        {
            ERecievingState recievingState = ERecievingState.Frame;

            TcpListener connectionServer = new TcpListener(IPAddress.Parse(Constants.ServerAddress), Constants.ServerUpPort);
            connectionServer.Start();

            try
            {
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    TcpClient client = connectionServer.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    NetworkStream stream = client.GetStream();

                    int i;
                    Int32 messageSize = 0;
                    byte[] buffer = new byte[Constants.TCPBufferSize];
                    byte[] lengthData = new byte[Constants.HeaderSize];

                    while ((i = stream.Read(buffer, 0, Constants.TCPBufferSize)) != 0) { };

                    switch (recievingState)
                    {
                        case ERecievingState.Frame:
                            {
                                if (buffer.Length > Constants.HeaderSize)
                                {
                                    Array.Copy(buffer, lengthData, Constants.HeaderSize);
                                    if (Constants.SystemIsLittleEndian != Constants.MessageIsLittleEndian)
                                        lengthData.Reverse();
                                    messageSize = BitConverter.ToInt32(lengthData);
                                    Array.Copy(buffer, Constants.HeaderSize, buffer, 0, buffer.Length - Constants.HeaderSize); // Remove Header

                                    //TODO: more efficient way to do this
                                    List<byte> data = new List<byte>();
                                    data.AddRange(buffer);
                                    data.RemoveRange(messageSize, data.Count - messageSize);

                                    //message
                                    PlayerInput testInput = PlayerInput.Parser.ParseFrom(data.ToArray()); // Currently just throws away the message, not ideal

                                    recievingState = ERecievingState.Message;
                                }
                            }
                            break;

                        case ERecievingState.Message:
                            {

                            }
                            break;
                    }
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

            Console.WriteLine("Server End");
        }
    }
}
