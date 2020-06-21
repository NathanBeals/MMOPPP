using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.InteropServices.ComTypes;

namespace MMOPPPLibrary
{
    public static class Constants
    {
        public static readonly bool MessageIsLittleEndian = true; // Little Endian
        public static readonly bool SystemIsLittleEndian = BitConverter.IsLittleEndian;
        public const string ServerAddress = "192.168.0.105";
        public const Int32 ServerUpPort = 6000;
        public const Int32 ServerDownPort = 6001;
        public static readonly Int32 HeaderSize = 4;
        public static Int32 TCPBufferSize = 65536;
        public const float TimeToDC = 2000.0f; // Miliseconds
        public const float CharacterMoveSpeed = 6.0f / 1000;
    }

    public static class Helpers
    {
        //[DllImport("kernel32.dll")]
        //public static extern void GetSystemTimeAsFileTime(out FILETIME lpSystemTimeAsFileTime);

        //static void GetPrecisionTimeStamp()
        //{
        //    FILETIME ft;
        //    GetSystemTimeAsFileTime(&ft);
        //    UINT64 ticks = (((UINT64)ft.dwHighDateTime) << 32) | ft.dwLowDateTime;

        //    // A Windows tick is 100 nanoseconds. Windows epoch 1601-01-01T00:00:00Z
        //    // is 11644473600 seconds before Unix epoch 1970-01-01T00:00:00Z.
        //    Timestamp timestamp;
        //    timestamp.set_seconds((INT64)((ticks / 10000000) - 11644473600LL));
        //    timestamp.set_nanos((INT32)((ticks % 10000000) * 100));
        //}
    }
}
