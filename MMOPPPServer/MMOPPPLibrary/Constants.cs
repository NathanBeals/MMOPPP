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
