using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq.Expressions;
using Google.Protobuf.Examples.AddressBook;
using Google.Protobuf.MMOPPP.Messages;

// going to be sending udp packets and accepting packet loss as a reality
// maybe also recieve a tcp connection?

// Need a connection thread 
namespace MMOPPP
{
    class Program
    {
        static bool s_ApplicationQuit = false;
        static Int32 s_ServerPort = 2999;
        static IPAddress s_ServerAddress = IPAddress.Parse("127.0.0.1");

        List<Thread> m_WorkerThreads;

        static void Main(string[] args)
        {
            Thread thread = new Thread(Program.BackgroundWork);
            thread.Start();

            //AddressBook book = new AddressBook();
            //int myBook = book.CalculateSize();

        }

        public static void BackgroundWork()
        {
            TcpListener connectionServer = new TcpListener(s_ServerAddress, s_ServerPort);
            connectionServer.Start();

            try
            {
                while (true)
                {
                    if (s_ApplicationQuit) // wont work clients is a waiting call
                        break;

                    Console.Write("Waiting for a connection... ");
                    TcpClient client = connectionServer.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // Loop to receive all the data sent by the client.
                    NetworkStream stream = client.GetStream();
                    Byte[] bytes = new Byte[256];
                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) 
                    {
                        //if (stream.Length > 4)
                        //stream.Length;
                    }

                    Console.WriteLine("Message Recieved");
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
    }
}
