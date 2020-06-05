using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq.Expressions;
using Google.Protobuf.Examples.AddressBook;

// going to be sending udp packets and accepting packet loss as a reality
// maybe also recieve a tcp connection?

// Need a connection thread 
namespace MMOPPPServer
{
    class Program
    {
        static Int32 s_ServerPort = 2999;
        static IPAddress s_ServerAddress = IPAddress.Parse("127.0.0.1");

        List<Thread> m_WorkerThreads;

        static void Main(string[] args)
        {
            Thread thread = new Thread(Program.BackgroundWork);
            thread.Start();
        }

        public static void BackgroundWork()
        {
            TcpListener connectionServer = new TcpListener(s_ServerAddress, s_ServerPort);
            connectionServer.Start();

            try
            {
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = connectionServer.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    Byte[] bytes = new Byte[256];
                    String data = null; // TODO: replace this type with an expected data type

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    AddressBook book = new AddressBook();
                    int myBook = book.CalculateSize();


                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // //TODO: constider the endianess of the transfer //TODO: look at how paul did it

                        // // Translate data bytes to a ASCII string.
                        //// data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        // Console.WriteLine("Received: {0}", data);

                        // // Process the data sent by the client.
                        // //data = data.ToUpper();

                        // byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // // Send back a response.
                        // stream.Write(msg, 0, msg.Length);
                        // Console.WriteLine("Sent: {0}", data);
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
