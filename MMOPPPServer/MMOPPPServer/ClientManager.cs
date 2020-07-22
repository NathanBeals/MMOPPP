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
    Thread m_ConnectionHandlingThread;
    Thread m_BroadcastHandlingThread;
    bool m_ThreadsShouldExit = false;

    List<TcpClient> m_Clients = new List<TcpClient>();
    List<List<Byte>> m_QueuedData = new List<List<byte>>();
    List<ClientInput> m_Inputs = new List<ClientInput>();

    TcpListener m_TCPListener = null;

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

    public void HandleConnections()
    {
      m_TCPListener = new TcpListener(IPAddress.Parse(Constants.ServerLocalAddress), Constants.ServerPort);
      m_TCPListener.Start();

      try
      {
        while (!m_ThreadsShouldExit)
        {
          TcpClient client = m_TCPListener.AcceptTcpClient();
          client.ReceiveBufferSize = Constants.TCPBufferSize;

          lock (m_Clients)
          {
            m_Clients.Add(client);
            m_QueuedData.Add(new List<byte>());
          }

          Console.WriteLine("Connected!");
        }
      }
      catch (Exception e)
      {
          Console.WriteLine("Exception: {0}", e);
      }
      finally
      {
        if (m_TCPListener != null)
          m_TCPListener.Stop();
      }
    }

    public void HandleMessages()
    {
      while (!m_ThreadsShouldExit)
      {
        lock (m_Clients)
        {
          for (int i = 0; i < m_Clients.Count; ++i)
          {
            if (m_Clients[i].Connected && m_Clients[i].Available != 0)
              MMOPPPLibrary.ProtobufTCPMessageHandler.HandleMessage(m_Clients[i], m_QueuedData[i], ParseClientUpdate);
          }

          var count = m_Clients.Count();
          m_Clients.RemoveAll(x => !x.Connected); // Clean the list of dead connections
          for (int i = 0; i < count - m_Clients.Count; ++i)
            Console.WriteLine("Disconnected");
        }
      }
    }

    public bool ParseClientUpdate(List<Byte> RawBytes, TcpClient ClientConnection)
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
          ClientConnection.GetStream().FlushAsync();
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

      foreach (var client in m_Clients)
      {
        Packet<ServerUpdates> packet = new Packet<ServerUpdates>(m_QueuedServerUpdates);
        try
        {
          packet.SendPacket(client.GetStream());
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }
      m_QueuedServerUpdates = null;
    }

    public void Start()
    {
      m_ThreadsShouldExit = false;

      m_ConnectionHandlingThread = new Thread(HandleConnections);
      m_ConnectionHandlingThread.Name = "Connection Handling";
      m_ConnectionHandlingThread.Start();

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
