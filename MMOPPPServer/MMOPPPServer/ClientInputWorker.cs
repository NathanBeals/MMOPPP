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
            m_WorkerThread = new Thread(BackgroundWork);
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

        public static void BackgroundWork()
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
                    //PlayerInput message;
                    byte[] buffer = new byte[256];
                    byte[] lengthData = new byte[4];

                    switch (recievingState)
                    {
                        case ERecievingState.Frame:
                            {
                                while ((i = stream.Read(buffer, 0, 256)) != 0)
                                {
                                    if (buffer.Length > 4) //TODO: make constant
                                    {
                                        Array.Copy(buffer, lengthData, 4);
                                        if (Constants.SystemIsLittleEndian != Constants.MessageIsLittleEndian)
                                            lengthData.Reverse();
                                        messageSize = BitConverter.ToInt32(lengthData);
                                        Array.Copy(buffer, 4, buffer, 0, buffer.Length - 4); // Remove Header

                                        //TODO: more efficient way to do this
                                        List<byte> data = new List<byte>();
                                        data.AddRange(buffer);
                                        data.RemoveRange(messageSize, data.Count - messageSize);

                                        //message
                                        PlayerInput testInput = PlayerInput.Parser.ParseFrom(data.ToArray()); // Currently just throws away the message, not ideal
                                        break;
                                    }
                                }
                            }
                            break;

                        case ERecievingState.Message:
                            {
                                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                                {



                                }
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
