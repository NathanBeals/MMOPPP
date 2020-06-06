using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf.Examples.AddressBook;
using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;

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
        }
    }

    class Server
    {

    }
}
