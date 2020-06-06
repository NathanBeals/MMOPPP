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

        Console.WriteLine("I am an AI {0}", input.CalculateSize());
        }
    }

    class Server
    {

    }
}
