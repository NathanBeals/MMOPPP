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

namespace MMOPPPServer
{
    class GameServer
    {
        SQLDB m_Database = new SQLDB();
        ClientManager m_ClientManager = null;
        List<PlayerInput> m_Inputs = new List<PlayerInput>();
        List<Character> m_Characters = new List<Character>();

        float m_ServerTickRate = 1000; //Miliseconds
        float m_TimeSinceLastTick = 0;

        bool m_ServerRunning = false;

        ~GameServer()
        {
            Stop();
        }

        void Initialize()
        {
            m_ClientManager = new ClientManager();
            m_Database.Initialize();
        }

        public void WorldUpdate(float DeltaTime)
        {
            if (m_ServerRunning)
            {
                m_TimeSinceLastTick += DeltaTime;
                if (m_TimeSinceLastTick > m_ServerTickRate)
                {
                    m_TimeSinceLastTick -= m_ServerTickRate;

                    // Take inputs fro the client manager
                    m_Inputs = m_ClientManager.GetInputs(); // Blocking Call
                    m_ClientManager.ClearInputs(); // Also a blocking Call //TODO: combine in one function so there's no chance of dropping inputs

                    PhysicsUpdate();

                    Console.WriteLine("Tick");
                }

                foreach (var input in m_Inputs)
                {
                    m_Database.SaveCharacterData(input.Id.Name, input.MoveInput.DirectionInputs, input.MoveInput.EulerRotation, true); // TODO: on disconnect, only set online to false
                }

                m_Inputs.Clear();
            }
        }


        void PhysicsUpdate()
        {
            foreach (var input in m_Inputs)
            {
                //Debug Line
                Console.WriteLine(input.ToString());
                //TODO: add characters if they aren't in the scene (attempt to read database for existing characters)
                //TODO: remove characters that have disconected clients
                //TODO: move the characters
            }

            BroadcastWorldUpdate();
        }

        // Sends a world update to all connected clients
        void BroadcastWorldUpdate()
        {
            WorldUpdate worldUpdate = new WorldUpdate();
            foreach (var character in m_Characters)
                worldUpdate.Updates.Add(character.ToEnityUpdate());

            m_ClientManager.QueueWorldUpdate(worldUpdate);
        }

        public void Start()
        {
            if (!m_ServerRunning)
            {
                Initialize();
                m_ClientManager.Start();
                m_ServerRunning = true;
            }
        }

        public void Stop()
        {
            if (m_ServerRunning)
            {
                m_ClientManager.Stop();
                m_ServerRunning = false;
            }
        }
    }
}
