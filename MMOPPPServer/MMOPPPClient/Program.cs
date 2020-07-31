using System;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

using Google.Protobuf.MMOPPP.Messages;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Net.NetworkInformation;
using MMOPPPLibrary;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Data;

namespace AIClient
{
  class Program
  {
    static void Main(string[] args)
    {
      var connectionManager = new ConnectionManager();
      connectionManager.InitGameLoop();
    }
  }
}
