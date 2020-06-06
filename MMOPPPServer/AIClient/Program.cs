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
    class Program
    {
        static void Main(string[] args)
        {
            PlayerInput input = new PlayerInput
            {
                Id = new Identifier
                {
                    Name = "nate",
                    Tags = "na"
                },
                MoveInput = new EntityInput
                {
                    DirectionInputs = new Vector3
                    {
                        X = 0.0f,
                        Y = 0.0f,
                        Z = 0.0f
                    },
                    EulerRotation = new Vector3
                    {
                        X = 0.0f,
                        Y = 0.0f,
                        Z = 0.0f
                    },
                    Sprint = false,
                    Strafe = false
                },
                SentTime = new Timestamp() // TODO: this field is problematic, I want milisecond precision but it only takes whole seconds or nanoseconds... strang, might have to alter the message.
                {
                    Seconds = DateTime.Now.Second,
                    Nanos = DateTime.Now.Millisecond / 1000000 //TODO: this converts mili to nanoseconds, this can't be right
                }
            };

            PlayerInput input2 = new PlayerInput();
            input2.Id = new Identifier { Name = "Wilbur", Tags = ""};
            input2.MoveInput = new EntityInput { 
                Strafe = false, 
                Sprint = false, 
                EulerRotation = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f }, 
                DirectionInputs = new Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f } };
            input2.SentTime = new Timestamp { Seconds = DateTime.Now.Second, Nanos = DateTime.Now.Millisecond / 1000000 };

            Console.WriteLine("I am an AI1 {0}", input.CalculateSize());
            Console.WriteLine("I am an AI2 {0}", input2.CalculateSize());

            input2.ToByteArray();

            TCPPacket<PlayerInput> mypacket = new TCPPacket<PlayerInput> { m_Size = input2.CalculateSize(), m_Message = input2 };
        }
    }

    //TODO: might be able to rename this just packet
    public class TCPPacket<T> where T : Google.Protobuf.IMessage<T>
    {
        public Int32 m_Size;
        public T m_Message;

        // TODO: there are more efficient ways to do this, I opted for simplicity
       Byte[] ToByteArray()
       {
            byte[] a = BitConverter.GetBytes(m_Size);
            byte[] b = m_Message.ToByteArray();

            byte[] c = new byte[a.Length + b.Length];
            Array.Copy(a, 0, c, 0, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);
            return c;
       }

        public void SendPacket(NetworkStream stream)
        {
            stream.Write(ToByteArray(), 0, ToByteArray().Length);
        }
    }
}
