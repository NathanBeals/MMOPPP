﻿using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;

namespace MMOPPPShared
{
    public class Packet<T> where T : Google.Protobuf.IMessage<T>
    {
        public Int32 m_Size;
        public T m_Message;

        private Int32 m_Length;

        // TODO: there are more efficient ways to do this, I opted for simplicity
        Byte[] ToByteArray()
        {
            byte[] sizeInBytes = BitConverter.GetBytes(m_Size);
            byte[] messageInBytes = m_Message.ToByteArray();

            byte[] packetInBytes = new byte[sizeInBytes.Length + messageInBytes.Length];
            Array.Copy(sizeInBytes, 0, packetInBytes, 0, sizeInBytes.Length);
            Array.Copy(messageInBytes, 0, packetInBytes, sizeInBytes.Length, messageInBytes.Length);

            m_Length = packetInBytes.Length;

            return packetInBytes;
        }

        public void SendPacket(NetworkStream stream)
        {
            stream.Write(ToByteArray(), 0, m_Length);
        }
    }
}
