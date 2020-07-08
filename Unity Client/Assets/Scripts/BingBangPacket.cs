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
using MMOPPPLibrary;

public class PacketIt<T> where T : Google.Protobuf.IMessage<T>, new()
{
  T m_Message;
  Int32 m_MessageSize;
  Int32 m_PacketSize;

  public PacketIt(T Message)
  {
    m_Message = new T();
    m_Message = Message; //HACK: I'm not sure if this is copying or not
    m_MessageSize = Message.CalculateSize();
  }

  Byte[] ToByteArray()
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
    var packet = ToByteArray();
    if (Stream.CanWrite)
      Stream.Write(ToByteArray(), 0, packet.Length);
    if (packet.Length != m_PacketSize)
      Debug.WriteLine("???????????????????????????????????");
  }

  public static void SendPacketBatch(NetworkStream Stream, List<PacketIt<T>> Messages)
  {
    List<Byte> batch = new List<byte>();
    foreach (var message in Messages)
      batch.AddRange(message.ToByteArray());

    Stream.Write(batch.ToArray(), 0, batch.Count());
  }
}
