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

        List<ClientInput> m_Inputs = new List<ClientInput>();

        TcpListener m_TCPListener = null;
        UdpClient m_UDPClientIncoming = null;
        UdpClient m_UDPClientOutgoing = null;
        List<IPEndPoint> m_Clients = new List<IPEndPoint>();

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
                while (m_UDPClientIncoming.Available != 0)
                {
                    MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_UDPClientIncoming, ParseClientUpdate);
                    Console.WriteLine("Inputs Recieved");
                }
            }
        }

        public bool ParseClientUpdate(List<Byte> RawBytes, IPEndPoint Sender)
        {
            lock (m_Inputs)
            {
                try
                {
                    lock (m_Clients)
                    {
                        if (!m_Clients.Contains(Sender)) // HACK: costly depending on connections
                            m_Clients.Add(Sender);
                    }

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
            //Packet<ServerUpdates> packet = new Packet<ServerUpdates>();
            ServerUpdates greg = new ServerUpdates();
            ServerUpdate a = new ServerUpdate();
            a.Name = "gerg";
            greg.Updates.Add(a);
            Packet<ServerUpdates> packet = new Packet<ServerUpdates>(greg/*m_QueuedServerUpdates*/);
            foreach (var connectionInfo in m_Clients)
            {
                packet.SendPacketUDP(m_UDPClientOutgoing, new IPEndPoint(connectionInfo.Address, connectionInfo.Port + 1)); //instead just use set 3333?
            }

            m_QueuedServerUpdates = null;
        }

        public void Start()
        {
            m_ThreadsShouldExit = false;

            m_UDPClientIncoming = new UdpClient(MMOPPPLibrary.Constants.ServerPort);
            m_UDPClientOutgoing = new UdpClient(MMOPPPLibrary.Constants.ServerPort + 1);

            m_MessageHandlingThread = new Thread(HandleMessages);
            m_MessageHandlingThread.Name = "Message Handling";
            m_MessageHandlingThread.Start();

            m_BroadcastHandlingThread = new Thread(BroadcastWorldUpdate);
            m_BroadcastHandlingThread.Name = "Broadcasting";
            m_BroadcastHandlingThread.Start();
        }

        public void Stop()
        {
            if (m_TCPListener != null)
            {
                m_TCPListener.Stop();
                m_TCPListener = null;
            }

            m_ThreadsShouldExit = true;
        }
    }
}
