using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using System.Linq.Expressions;
using Google.Protobuf.Examples.AddressBook;
using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Reflection;
using MMOPPPShared;
using System.Diagnostics;
using System.IO;
using System.Linq;

// going to be sending udp packets and accepting packet loss as a reality
// maybe also recieve a tcp connection?

// Need a connection thread 
namespace MMOPPP
{
    class Server
    {
        List<Thread> m_WorkerThreads;

        List<Byte> m_BytesToParse; // TODO: move

        static void Main(string[] args)
        {
            Thread thread = new Thread(Server.BackgroundWork);
            thread.Start();

            //TODO: remove this whole section it's bad
            int x = 0;
            while(true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Main wait");
            }

            thread.Abort();
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

            EntityInput message;

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

                    switch(recievingState)
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
                                        PlayerInput testInput = PlayerInput.Parser.ParseFrom(data.ToArray());
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
