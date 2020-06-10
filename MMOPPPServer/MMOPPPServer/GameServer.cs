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
        List<PlayerInput> m_Inputs = new List<PlayerInput>();
        List<Character> m_Characters = new List<Character>();

        float m_ServerTickRate = 100; //Miliseconds
        float m_TimeSinceLastTick = 0;


        public void InitWorld()
        {
            m_ClientManager = new ClientsManager();
        }

        public void WorldUpdate(float DeltaTime)
        {
            m_TimeSinceLastTick += DeltaTime;
            if (m_TimeSinceLastTick > m_ServerTickRate)
            {
                m_TimeSinceLastTick -= m_ServerTickRate;

                // Take inputs from the client manager
                m_Inputs = m_ClientManager.GetInputs(); // Blocking Call
                m_ClientManager.ClearInputs(); // Also a blocking Call //TODO: combine in one function so there's no chance of dropping inputs

                PhysicsUpdate();
            }
        }


        public void PhysicsUpdate()
        {
            foreach (var input in m_Inputs)
            {
                //TODO: add characters if they aren't in the scene (attempt to read database for existing characters)
                //TODO: remove characters that have disconected clients
                //TODO: move the characters
            }

            BroadcastWorldUpdate();
        }

        // Sends a world update to all connected clients
        public void BroadcastWorldUpdate()
        {
            WorldUpdate worldUpdate = new WorldUpdate();
            foreach (var character in m_Characters)
                worldUpdate.Updates.Add(character.ToEnityUpdate());

            m_ClientManager.QueueWorldUpdate(worldUpdate);
        }

        // for headed server debugging
        public void PrintInputs()
        {
            m_ClientManager.GetInputs();
        }
    }
}
