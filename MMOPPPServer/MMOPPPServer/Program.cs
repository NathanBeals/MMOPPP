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
using MMOPPPLibrary;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MMOPPPServer;


namespace MMOPPP
{
  class Program
  {
    static void Main(string[] args)
    {
      var stopWatch = new Stopwatch();
      GameServer server = new GameServer();
      bool bExit = false;
      server.Start();

      long deltaTime;
      while (!bExit)
      {
        deltaTime = stopWatch.ElapsedTicks;
        stopWatch.Restart();

        if (Console.KeyAvailable) // HACK: these don't work consistently, the faster the tick the less they work
        {
          if (Console.ReadKey(true).Key == ConsoleKey.Escape)
            bExit = true;
          if (Console.ReadKey(true).Key == ConsoleKey.S)
            server.Start();
          if (Console.ReadKey(true).Key == ConsoleKey.H)
            server.Stop();
        }

        server.WorldUpdate(deltaTime / 10000.0f);
        Thread.Sleep(1);

        stopWatch.Stop();
      }

      server.Stop();

    }
  }
}
