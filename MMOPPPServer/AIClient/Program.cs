#define DEBUG

using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;

namespace AIClient
{
    class Debug
    {
        public static void PrintSizeCalc()
        {
#if (DEBUG)
            PlayerInput testInput = new PlayerInput();
            testInput.Id = new Identifier { Name = "Wilbur", Tags = "" };
            testInput.MoveInput = new EntityInput
            {
                Strafe = false,
                Sprint = false,
                EulerRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f },
                DirectionInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f }
            };
            testInput.SentTime = new Timestamp { Seconds = DateTime.Now.Second, Nanos = DateTime.Now.Millisecond / 1000000 };
            Console.WriteLine("I am an AI2 {0}", testInput.CalculateSize());
#endif
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Debug.PrintSizeCalc();

//            Packet<PlayerInput> mypacket = new Packet<PlayerInput> { m_Size = testInput.CalculateSize(), m_Message = testInput };
        }
    }

    //TODO: might be able to rename this just packet
    public class Packet<T> where T : Google.Protobuf.IMessage<T>
    {
        public Int32 m_Size;
        public T m_Message;

        private Int32 m_Length;

        // TODO: there are more efficient ways to do this, I opted for simplicity
       Byte[] ToByteArray()
       {
            byte[] a = BitConverter.GetBytes(m_Size);
            byte[] b = m_Message.ToByteArray();

            byte[] c = new byte[a.Length + b.Length];
            Array.Copy(a, 0, c, 0, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);

            m_Length = c.Length;

            return c;
       }

        public void SendPacket(NetworkStream stream)
        {
            stream.Write(ToByteArray(), 0, m_Length);
        }
    }
}
