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

namespace MMOPPPServer
{
    class GameServer
    {
        ClientsManager m_ClientManager = null;
        

        public void InitWorld()
        {
            m_ClientManager = new ClientsManager();
        }

        public void WorldUpdate(float DeltaTime)
        {


        }

        public void CalculatePhysics()
        {

        }

        public void BroadcastWorldUpdate()
        {
            Google.Protobuf.MMOPPP.Messages.WorldUpdate worldUpdate;

            
        }

        // for headed server debugging
        public void PrintInputs()
        {
            m_ClientManager.GetInputs();
        }
    }
}
