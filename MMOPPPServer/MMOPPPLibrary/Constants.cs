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
    public const Int32 ServerTickRate = 200; // Miliseconds
    public const Int32 TCPBufferSize = 150000; //150kb per second (very intensive)
    public const float TimeToDC = 3000.0f; // Miliseconds
    public const float CharacterMoveSpeed = 8.0f;
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
