using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MMOPPPLibrary
{
  public class Packet<T> where T : Google.Protobuf.IMessage<T>, new()
  {
    T m_Message;
    Int32 m_MessageSize;
    Int32 m_PacketSize;

    public Packet(T Message)
    {
      m_Message = new T();
      m_Message = Message; //HACK: I'm not sure if this is copying or not
      m_MessageSize = Message.CalculateSize();
    }

    byte[] ToByteArray()
    {
      byte[] sizeInBytes = BitConverter.GetBytes(m_MessageSize);
      // INFO: protobuf handles the endianess of it's messages, only the size needs to be swapped.
      if (Constants.MessageIsLittleEndian != Constants.SystemIsLittleEndian)
        sizeInBytes.Reverse();

      byte[] messageInBytes = m_Message.ToByteArray();

      byte[] packetInBytes = new byte[sizeInBytes.Length + messageInBytes.Length];
      Array.Copy(sizeInBytes, 0, packetInBytes, 0, sizeInBytes.Length);
      Array.Copy(messageInBytes, 0, packetInBytes, sizeInBytes.Length, messageInBytes.Length);

      m_PacketSize = packetInBytes.Length;

      return packetInBytes;
    }

    int MTUSize = 1000; // Closer to 1474
    public void SendPacket(NetworkStream Stream)
    {
      var byteArray = ToByteArray();

      //if (byteArray.Length > MTUSize)
      //{
      //  List<byte> runningByteArray = new List<byte>(byteArray);

      //  Console.WriteLine("Forcing Split");

      //  while (runningByteArray.Count > 0)
      //  {
      //    List<byte> subArray = new List<byte>(runningByteArray.Take(MTUSize));
      //    Stream.Write(subArray.ToArray(), 0, subArray.Count);
      //    runningByteArray.RemoveRange(0, Math.Min(MTUSize, subArray.Count));
      //    Thread.Sleep(1);
      //  }
      //}
      //else
        Stream.Write(ToByteArray(), 0, m_PacketSize);
    }

    public static void SendPacketBatch(NetworkStream Stream, List<Packet<T>> Messages)
    {
      List<Byte> batch = new List<byte>();
      foreach (var message in Messages)
        batch.AddRange(message.ToByteArray());

      Stream.Write(batch.ToArray(), 0, batch.Count());
    }

    public Int32 GetPacketSize()
    {
      return m_MessageSize + 4;
    }
  }
}
