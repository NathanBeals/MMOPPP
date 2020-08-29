using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;
using MMOPPPLibrary;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace MMOPPPLibrary
{
    public class ProtobufTCPMessageHandler
    {
        public delegate bool ParseFunction(List<Byte> RawBytes, IPEndPoint Sender);

        enum ERecievingState
        {
            Frame,
            Message
        }

        //TODO: can be refactored more, the packet size is almost unimportant now
        static public void HandleMessage(UdpClient Client, ParseFunction DataParser)
        {
            if (Client.Available == 0)
                return;

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            byte[] lengthData = new byte[Constants.HeaderSize];
            int dataAvailable = Client.Available;
            int messageSize;

            while (dataAvailable != 0)
            {
                byte[] buffer = Client.Receive(ref RemoteIpEndPoint);

                // Get size of message from header
                Array.Copy(buffer, 0, lengthData, 0, Constants.HeaderSize);
                if (Constants.SystemIsLittleEndian != Constants.MessageIsLittleEndian)
                    lengthData.Reverse();
                messageSize = BitConverter.ToInt32(lengthData, 0);
                if (messageSize > dataAvailable - Constants.HeaderSize)
                    throw new System.Exception("UDP Datagram incomplete");

                // Put the message bytes into a data object
                List<byte> data = new List<byte>();
                data.AddRange(buffer.SubArray(Constants.HeaderSize, messageSize));

                // Parse the message bytes and add it to the inputs list
                if (!DataParser(data, RemoteIpEndPoint))
                    break;

                dataAvailable -= messageSize + Constants.HeaderSize;
            }
        }
    }
}
