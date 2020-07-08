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
    public const string ServerLocalAddress = "0.0.0.0";
    public const string ServerPublicAddress = "natelovespizza.ddns.net"; 
    public const Int32 ServerPort = 6969; 
    public const Int32 HeaderSize = 4;
    public const Int32 HeaderChecksumSize = 4;
    public const Int32 HeaderChecksumValue = 0x7FFFFFFF;
    public const Int32 ServerTickRate = 100; // Miliseconds
    public const Int32 TCPBufferSize = (int)((ServerTickRate / 1000.0f) * 1000000); //10mb per second (very intensive)
    public const float TimeToDC = 100000.0f; // Miliseconds
    public const float CharacterMoveSpeed = 8.0f / 1000;
  }

  public static class Extensions // From https://www.techiedelight.com/get-subarray-of-array-csharp/
  {
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
      T[] result = new T[length];
      Array.Copy(array, offset, result, 0, length);
      return result;
    }
  }
}
