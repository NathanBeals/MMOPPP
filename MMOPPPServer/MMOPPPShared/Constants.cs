using System;
using System.Collections.Generic;
using System.Text;

namespace MMOPPPShared
{
    public static class Constants
    {
        public static readonly bool MessageIsLittleEndian = true; // Little Endian
        public static readonly bool SystemIsLittleEndian = BitConverter.IsLittleEndian;
        public const string ServerAddress = "127.0.0.1";
        public const Int32 ServerUpPort = 6000;
        public const Int32 ServerDownPort = 6001;
    }
}
