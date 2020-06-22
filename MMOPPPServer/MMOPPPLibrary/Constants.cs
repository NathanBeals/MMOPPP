﻿using System;
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
}
