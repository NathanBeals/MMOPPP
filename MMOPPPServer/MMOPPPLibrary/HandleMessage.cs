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

namespace MMOPPPLibrary
{
  public class ProtobufTCPMessageHandler
  {
    public delegate bool ParseFunction(List<Byte> RawBytes);

    enum ERecievingState
    {
      Frame,
      Message
    }

    static public void HandleMessage(UdpClient Client, ParseFunction DataParser)
    {
      if (Client.Available == 0)
        return;

      var client = Client;
      Int32 messageSize = 0;
      byte[] buffer = new byte[Constants.TCPBufferSize];
      byte[] lengthData = new byte[Constants.HeaderSize];
      int dataAvailable;
      int dataHead = 0;

      dataAvailable = client.Available;

      while (dataAvailable != 0)
      {
        if (dataAvailable >= Constants.HeaderSize)
        {
          // Get size of message from header
          Array.Copy(buffer, dataHead, lengthData, 0, Constants.HeaderSize);
          if (Constants.SystemIsLittleEndian != Constants.MessageIsLittleEndian)
            lengthData.Reverse();
          messageSize = BitConverter.ToInt32(lengthData, 0);
        }

        if (dataAvailable >= messageSize + Constants.HeaderSize)
        {
          // Put the message bytes into a data object
          List<byte> data = new List<byte>();
          data.AddRange(buffer.SubArray(dataHead + Constants.HeaderSize, messageSize));

          // Parse the message bytes and add it to the inputs list
          if (!DataParser(data))
            break;

          // Increment the head
          dataHead += messageSize + Constants.HeaderSize;
          dataAvailable -= messageSize + Constants.HeaderSize;

          continue;
        }

        if (dataAvailable != 0 && dataAvailable < messageSize + Constants.HeaderSize) // If the remaining data is smaller than the message size, push it onto the data to be parsed later
        {
          Debug.Assert(true);
          break;
        }
      }
    }
  }
}
