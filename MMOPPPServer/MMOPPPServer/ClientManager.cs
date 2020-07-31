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
using MMOPPPLibrary;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using System.Runtime.CompilerServices;

namespace MMOPPPServer
{
    class ClientManager
    {
        Thread m_MessageHandlingThread;
        //Thread m_ConnectionHandlingThread;
        Thread m_BroadcastHandlingThread;
        bool m_ThreadsShouldExit = false;

        List<TcpClient> m_Clients = new List<TcpClient>();
        List<List<Byte>> m_QueuedData = new List<List<byte>>();
        List<ClientInput> m_Inputs = new List<ClientInput>();

        TcpListener m_TCPListener = null;
        List<UdpClient> m_UDPClientsOutgoing = new List<UdpClient>(); // Need to record IP addresses in addition to the udp down port (for systems sharing ip addresses)
        UdpClient m_UDPClientIncoming = null;

        object WorldUpdateLock = new object();
        ServerUpdates m_QueuedServerUpdates = null;

        public ClientManager()
        {
        }

        ~ClientManager()
        {
            Stop();
        }

        public List<ClientInput> GetInputs()
        {
            List<ClientInput> InputsCopy;
            lock (m_Inputs)
            {
                InputsCopy = new List<ClientInput>(m_Inputs);
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

        public void HandleMessages()
        {
            while (!m_ThreadsShouldExit)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0); // Allows recieving from any port
                Byte[] recieved = m_UDPClientIncoming.Receive(ref RemoteIpEndPoint);
                if (recieved.Length > 0)
                    Console.WriteLine("success");
                while (m_UDPClientIncoming.Available != 0)
                {
                    Byte[] recieved = m_UDPClientIncoming.Receive(ref RemoteIpEndPoint); // TODO: recieve async
                    MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_UDPClientIncoming, ParseClientUpdate);

                    // Handle Message
                }
            }
        }

        public bool ParseClientUpdate(List<Byte> RawBytes)
        {
            lock (m_Inputs)
            {
                try
                {
                    m_Inputs.Add(ClientInput.Parser.ParseFrom(RawBytes.ToArray()));
                    return true;
                }
                catch (Exception e) // If the input fails just clear the entire stream
                {
                    Console.WriteLine(e);
                }
            }

            return false;
        }

        public void QueueWorldUpdate(ServerUpdates Update)
        {
            lock (WorldUpdateLock)
            {
                m_QueuedServerUpdates = Update;
            }
        }

        void BroadcastWorldUpdate()
        {
            while (true)
            {
                lock (WorldUpdateLock)
                {
                    lock (m_Clients)
                    {
                        BroadcaseWorldUpdateLocked();
                    }
                }
            }
        }

        private void BroadcaseWorldUpdateLocked()
        {
            if (m_QueuedServerUpdates == null)
                return;

            // TODO: for each connection
            Packet<ServerUpdates> packet = new Packet<ServerUpdates>(m_QueuedServerUpdates);
            packet.SendPacketUDP(m_UDPClientsOutgoing[0]); //TODO: establish connection to remote 

            m_QueuedServerUpdates = null;
        }

        public void Start()
        {
            m_ThreadsShouldExit = false;

            m_UDPClientIncoming = new UdpClient(MMOPPPLibrary.Constants.ServerPort);

            m_UDPClientsOutgoing.Add(new UdpClient()); //Also get the address of the incoming connection
            m_UDPClientsOutgoing[0].Connect("127.0.0.1", MMOPPPLibrary.Constants.TempClientPort);

            m_MessageHandlingThread = new Thread(HandleMessages);
            m_MessageHandlingThread.Name = "Message Handling";
            m_MessageHandlingThread.Start();

            m_BroadcastHandlingThread = new Thread(BroadcastWorldUpdate);
            m_BroadcastHandlingThread.Name = "Broadcasting";
            m_BroadcastHandlingThread.Start();
        }

        public void Stop()
        {
            lock (m_Clients)
            {
                foreach (var client in m_Clients)
                    client.Close();
                m_Clients.Clear();
            }

            if (m_TCPListener != null)
            {
                m_TCPListener.Stop();
                m_TCPListener = null;
            }

            m_ThreadsShouldExit = true;
        }
    }
}
