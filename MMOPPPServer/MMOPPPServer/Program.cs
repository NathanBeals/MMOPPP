using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using System.Linq.Expressions;
using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Reflection;
using MMOPPPShared;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MMOPPPServer;
using System.Runtime.CompilerServices;

namespace MMOPPP
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            GameServer server = new GameServer();

            server.InitWorld();

            long deltaTime;
            while (true)
            {
                deltaTime = stopWatch.ElapsedTicks;
                stopWatch.Restart();
                
                server.WorldUpdate(deltaTime / 10000.0f); // Expensive? 

                stopWatch.Stop();
            }
        }
    }
}
