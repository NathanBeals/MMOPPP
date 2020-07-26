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
using System.Drawing;

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
      m_Message = Message;
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

    public void SendPacket(NetworkStream Stream)
    {
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
