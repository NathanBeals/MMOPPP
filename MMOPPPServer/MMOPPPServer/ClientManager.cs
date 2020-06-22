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
    List<PlayerInput> m_Inputs = new List<PlayerInput>();

    TcpListener m_TCPListener = null;

    object WorldUpdateLock = new object();
    WorldUpdate m_QueuedWorldUpdate = null;

    public ClientManager()
    {
    }

    ~ClientManager()
    {
      Stop();
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
      m_TCPListener = new TcpListener(IPAddress.Parse(Constants.ServerAddress), Constants.ServerUpPort);
      m_TCPListener.Start();

      try
      {
        while (!m_ThreadsShouldExit)
        {
          TcpClient client = m_TCPListener.AcceptTcpClient();

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
        if (e.SocketErrorCode == SocketError.Interrupted)
        {
          // Normal escape on server close
        }
        else
          Console.WriteLine("SocketException: {0}", e);
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
              HandleMessage(i);
          }

          var count = m_Clients.Count();
          m_Clients.RemoveAll(x => !x.Connected); // Clean the list of dead connections
          for (int i = 0; i < count - m_Clients.Count; ++i)
            Console.WriteLine("Disconnected");
        }
      }
    }

    void HandleMessage(int ClientIndex)
    {
      var client = m_Clients[ClientIndex];
      var queuedData = m_QueuedData[ClientIndex];
      if (client.Available == 0)
        return;

      Int32 messageSize = 0;
      byte[] buffer = new byte[Constants.TCPBufferSize];
      byte[] testBuffer = new byte[Constants.TCPBufferSize]; // TODO: remove after testing
      byte[] lengthData = new byte[Constants.HeaderSize];
      Array.Copy(queuedData.ToArray(), buffer, queuedData.Count);
      ERecievingState recievingState = ERecievingState.Frame;
      int dataAvailable = 0;

      try // Make sure to catch if the client is DCed in here
      {
        NetworkStream stream = client.GetStream();
        dataAvailable = client.Available;
        stream.Read(buffer, queuedData.Count, Math.Min(dataAvailable, Constants.TCPBufferSize - queuedData.Count));
      }
      catch (System.IO.IOException) //TODO: look up client dc error
      {
        return;
      }
      queuedData.Clear();

      Array.Copy(buffer, testBuffer, buffer.Length); //TODO: remove after testing

      bool dataNotComplete = false;
      while (!m_ThreadsShouldExit && !dataNotComplete)
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

                recievingState = ERecievingState.Message;
              }
              else // If the remaining data is smaller than the header size, push it onto the data to be parsed later
              {
                m_QueuedData[ClientIndex].AddRange(buffer.SubArray(0, dataAvailable));
                dataNotComplete = true;
              }
            }
            break;

          case ERecievingState.Message:
            {
              if (dataAvailable >= messageSize + Constants.HeaderSize)
              {
                // Remove header from buffer
                Array.Copy(buffer, Constants.HeaderSize, buffer, 0, buffer.Length - Constants.HeaderSize);
                dataAvailable -= Constants.HeaderSize;

                // Put the message bytes into a data object
                List<byte> data = new List<byte>();
                data.AddRange(buffer);
                data.RemoveRange(messageSize, buffer.Length - messageSize);

                // Parse the message bytes and add it to the inputs list
                lock (m_Inputs)
                {
                  //HACK: only for testing, needs to be resolved
                  try
                  {
                    m_Inputs.Add(PlayerInput.Parser.ParseFrom(data.ToArray()));
                  }
                  catch(Exception e) // If the input fails just clear the entire stream
                  {
                    // Data failed to parse, clear client
                    var tempbuffer = new byte[Constants.TCPBufferSize];
                    while (m_Clients[ClientIndex].GetStream().DataAvailable)
                      m_Clients[ClientIndex].GetStream().Read(tempbuffer, 0, tempbuffer.Length);
                    dataAvailable = 0;

                    Console.WriteLine("Critical Failure, Steam Dump Performed");

                    break;
                  }
                }

                // Remove message from buffer
                Array.Copy(buffer, messageSize, buffer, 0, buffer.Length - messageSize);

                dataAvailable -= messageSize;
                messageSize = 0;
                recievingState = ERecievingState.Frame;
              }
              else // If the remaining data is smaller than the message size, push it onto the data to be parsed later
              {
                m_QueuedData[ClientIndex].AddRange(buffer.SubArray(0, dataAvailable));
                dataNotComplete = true;
              }
            }
            break;
        }
      }
    }

    public void QueueWorldUpdate(WorldUpdate Update)
    {
      lock (WorldUpdateLock)
      {
        m_QueuedWorldUpdate = Update;
      }
    }

    void BroadcastWorldUpdate()
    {
      while (true)
      {
        lock (WorldUpdateLock)
        {
          if (m_QueuedWorldUpdate != null)
          {
            lock (m_Clients)
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

    public void Start()
    {
      m_ThreadsShouldExit = false;

      m_ConnectionHandlingThread = new Thread(HandleConnections);
      m_ConnectionHandlingThread.Start();

      m_MessageHandlingThread = new Thread(HandleMessages);
      m_MessageHandlingThread.Start();

      m_BroadcastHandlingThread = new Thread(BroadcastWorldUpdate);
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
