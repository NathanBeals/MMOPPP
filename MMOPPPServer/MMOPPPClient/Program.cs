﻿using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using MMOPPPLibrary;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Data;

namespace AIClient
{
  class Program
  {
    static void Main(string[] args)
    {
      var client = new MMOPPPClient();
      client.Connect();
    }
  }

  class MMOPPPClient
  {
    static List<string> s_ClientNames = new List<string>();

    TcpClient m_ServerConnection = new TcpClient();
    List<Byte> m_QueuedData = new List<Byte>();
    bool m_ThreadsShouldExit = false; // Hack, never set

    List<ServerUpdates> m_WorldUpdates = new List<ServerUpdates>();
    Thread m_MessageHandlingThread;

    enum ERecievingState
    {
      Frame,
      Message
    }

    public void HandleMessages()
    {
      while (!m_ThreadsShouldExit)
      {
        HandleMessage(m_ServerConnection);
        Thread.Sleep(5);
      }
    }

    public void HandleMessage(TcpClient Client)
    {
      var client = Client;
      var queuedData = m_QueuedData;
      if (client.Available == 0)
        return;

      Int32 messageSize = 0;
      byte[] buffer = new byte[Constants.TCPBufferSize];
      byte[] lengthData = new byte[Constants.HeaderSize];
      Array.Copy(queuedData.ToArray(), buffer, queuedData.Count);
      ERecievingState recievingState = ERecievingState.Frame;
      int dataAvailable = 0;

      try // Make sure to catch if the client is DCed in here
      {
        NetworkStream stream = client.GetStream();
        dataAvailable = client.Available;
        stream.Read(buffer, queuedData.Count, dataAvailable);
      }
      catch (System.IO.IOException) //TODO: look up client dc error
      {
        return;
      }
      queuedData.Clear();

      while (!m_ThreadsShouldExit)
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

                // Remove header from buffer
                Array.Copy(buffer, Constants.HeaderSize, buffer, 0, buffer.Length - Constants.HeaderSize);

                recievingState = ERecievingState.Message;
                dataAvailable -= Constants.HeaderSize;
              }
              else // If the remaining data is smaller than the header size, push it onto the data to be parsed later
              {
                m_QueuedData = buffer.ToList(); // TODO: investigate the impact of the array to list conversions on performance... and just usage in general (not sure how to use)
              }
            }
            break;

          case ERecievingState.Message:
            {
              if (dataAvailable >= messageSize)
              {
                // Put the message bytes into a data object
                List<byte> data = new List<byte>();
                data.AddRange(buffer);
                data.RemoveRange(messageSize, buffer.Length - messageSize);

                //// Parse the message bytes and add it to the inputs list
                lock (m_WorldUpdates)
                {
                  m_WorldUpdates.Add(ServerUpdates.Parser.ParseFrom(data.ToArray()));
                }

                // Remove message from buffer
                Array.Copy(buffer, messageSize, buffer, 0, buffer.Length - messageSize);

                dataAvailable -= messageSize;
                messageSize = 0;
                recievingState = ERecievingState.Frame;

                //Debbuging
               // Console.WriteLine($"World Updated {ServerUpdates.Parser.ParseFrom(data.ToArray()).ToString()}");
              }
              else // If the remaining data is smaller than the message size, push it onto the data to be parsed later
              {
                m_QueuedData = buffer.ToList();
              }
            }
            break;
        }
      }
    }

    public void Connect(string ServerAddress = MMOPPPLibrary.Constants.ServerPublicAddress, Int32 Port = MMOPPPLibrary.Constants.ServerPort)
    {
      s_ClientNames.Add(Console.ReadLine());

      try
      {
        IPAddress[] addresslist = Dns.GetHostAddresses(MMOPPPLibrary.Constants.ServerPublicAddress);
        if (addresslist.Length == 0)
          throw new SocketException();

        m_ServerConnection = new TcpClient(addresslist[0].ToString(), Port);
        m_ServerConnection.ReceiveBufferSize = Constants.TCPBufferSize;
        NetworkStream stream = m_ServerConnection.GetStream();

        m_MessageHandlingThread = new Thread(HandleMessages);
        m_MessageHandlingThread.Start();

        m_ServerConnection.ReceiveBufferSize = Constants.TCPBufferSize;
        //SendQueuedPackets(stream);

        //Console.WriteLine("Press enter to exit");
        //Console.ReadLine();

        GameLoop(stream);

        stream.Close(); // TODO: shouldn't close here, after one message, this whole block should loop and wait for input
        m_ServerConnection.Close();
      }
      catch (ArgumentNullException e)
      {
        Console.WriteLine("ArgumentNullException: {0}", e);
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException: {0}", e);
      }
    }

    public void SendQueuedPackets(NetworkStream stream)
    {
      //Debug code, making a packet
      ClientInput testInput = CreatePlayerInput("Nate");
      Packet<ClientInput> packet = new Packet<ClientInput>(testInput);

      //Debug
      Console.WriteLine("Packet is this size: {0}", testInput.CalculateSize());

      while (true)
      {
        for (var x = 0; x < 1000; x++)
          packet.SendPacket(stream);

        Console.WriteLine("any key to send next batch");
        Console.ReadLine();
      }
    }

    public ClientInput CreatePlayerInput(string Name)
    {
      ClientInput testInput = new ClientInput();
      testInput.Name = Name;
      testInput.Input = new Input
      {
        Strafe = false,
        Sprint = false,
        PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
        EulerBodyRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
        EulerCameraRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
        SentTime = (ulong)DateTime.UtcNow.Ticks / 10000
      };

      return testInput;
    }

    // HACK: yeah this is the roughest code in the whole project and that says something
    static ulong s_InputsPerCharacter = 12;
    public void GameLoop(NetworkStream SendStream)
    {
      float rot = 0;
      while (true)
      {
        List<Packet<ClientInput>> inputs = new List<Packet<ClientInput>>();
        ulong sendtime = ((ulong)DateTime.UtcNow.Ticks / 10000);
        foreach (var name in s_ClientNames)
        {
          for (ulong x = 0; x < s_InputsPerCharacter; ++x)
          {
            if (x == 0)
            {
              var timestampinput = CreatePlayerInput(name);
              timestampinput.Input.SentTime = sendtime;
              timestampinput.Input.PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 1.0f };
              timestampinput.Input.EulerBodyRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
              timestampinput.Input.EulerCameraRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
              var timestamppacket = new Packet<ClientInput>(timestampinput);
              inputs.Add(timestamppacket);
              timestamppacket.SendPacket(SendStream);

              Thread.Sleep(1);
            }

            var playerInput = CreatePlayerInput(name);
            playerInput.Input.SentTime = sendtime + (ulong)(16.66667 * (x + 1));
            playerInput.Input.PlayerMoveInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 1.0f };
            playerInput.Input.EulerBodyRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
            playerInput.Input.EulerCameraRotation = new Vector3 { X = 0.0f, Y = rot, Z = 0.0f };
            var newPacket = new Packet<ClientInput>(playerInput);
            inputs.Add(newPacket);
            newPacket.SendPacket(SendStream);

            Thread.Sleep(1);
          }
        }

        int sum = 0;
        foreach (var packet in inputs)
          sum += packet.GetPacketSize();

        rot += 20;
        if (rot > 360)
          rot = 0;

        Console.WriteLine("!!! Size: " + sum + " !!!");
        Thread.Sleep(MMOPPPLibrary.Constants.ServerTickRate);
      }
    }
  }
}
