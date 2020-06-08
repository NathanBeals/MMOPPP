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
        List<Thread> m_WorkerThreads;
        List<Byte> m_BytesToParse;
        List<PlayerInput> m_QueuedPlayerInputs = new List<PlayerInput>();
        Mutex m_PlayerInputMutex = new Mutex();

        Int32 m_ClientInputWorkerThreadCount = 1; //TODO: test with more
        List<ClientInputWorker> m_ClientInputWorkers = new List<ClientInputWorker>();

        void RecievedPlayerInput(PlayerInput Input)
        {
            //m_PlayerInputMutex.wa
        }

        public void InitWorld()
        {
            for (int i = 0; i < m_ClientInputWorkerThreadCount; ++i) // Definately worse than a normal for loop
                m_ClientInputWorkers.Add(new ClientInputWorker());
        }

        public void WorldUpdate(float DeltaTime)
        {
            // Not implemented
        }
    }
}
