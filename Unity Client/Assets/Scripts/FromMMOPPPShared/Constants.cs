using System;
using System.Collections.Generic;
using System.Text;

namespace MMOPPPShared
{
    public static class Constants
    {
        public static readonly bool MessageIsLittleEndian = true; // Little Endian
        public static readonly bool SystemIsLittleEndian = BitConverter.IsLittleEndian;
        public const string ServerAddress = "192.168.0.107";
        public const Int32 ServerUpPort = 6000;
        public const Int32 ServerDownPort = 6001;
        public static readonly Int32 HeaderSize = 4;
        public static Int32 TCPBufferSize = 65535;
    }
}
