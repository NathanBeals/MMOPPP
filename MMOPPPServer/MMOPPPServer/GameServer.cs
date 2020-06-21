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
    using V3 = System.Numerics.Vector3;
    using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;

    class GameServer
    { 
        SQLDB m_Database = new SQLDB();
        ClientManager m_ClientManager = null;
        List<PlayerInput> m_Inputs = new List<PlayerInput>();
        Dictionary<string, Character> m_Characters = new Dictionary<string, Character>();

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

                    m_Inputs = m_ClientManager.GetInputs(); // Blocking Call
                    m_ClientManager.ClearInputs(); // Also a blocking Call //TODO: combine in one function so there's no chance of dropping inputs

                    UpdateDisconnects(DeltaTime);
                    PhysicsUpdate(DeltaTime);
                    SaveCharacters();

                    Console.WriteLine("Tick");
                }

                m_Inputs.Clear();
            }
        }

        // Remove all Characters that have not had an update in TimeToDC miliseconds
        void UpdateDisconnects(float DeltaTime)
        {
            foreach (var pair in m_Characters.Where(kv => kv.Value.m_TimeSinceLastUpdate > Constants.TimeToDC).ToList())
                m_Characters.Remove(pair.Key);
            foreach (var pair in m_Characters)
                pair.Value.m_TimeSinceLastUpdate += DeltaTime;
        }

        // Save all characters to the database
        void SaveCharacters()
        {
            foreach (var pair in m_Characters)
                m_Database.SaveCharacterData(pair.Value.m_Name, Character.Vector3ToGVector3(pair.Value.m_Location), Character.Vector3ToGVector3(pair.Value.m_Rotation));
        }

        // The first time an input is found for a new character, load that characters position
        void TryAddCharacterFromInput(PlayerInput Input)
        {
            if (!m_Characters.ContainsKey(Input.Id.Name))
            {
                var character = new Character(Input.Id.Name);
                m_Characters[Input.Id.Name] = character;
                GV3 loc, rot;
                m_Database.LoadCharacterData(Input.Id.Name, out loc, out rot);
                character.m_Location = Character.GVector3ToVector3(loc);
                character.m_Rotation = Character.GVector3ToVector3(rot);
            }
        }


        //TODO: I'm going to avoid this for now, but there is a functional issue with this code,
        // I am interpretting events 1 past how they should be interpreted.
        // aka I should handle messages like, last update said they were moving forward, this update says they are stopping, move forward for the time difference.
        // as of now I'm doing it, they said forward, move forward for the time since the last message, which is just wrong, but I have other things I have to get done before fixing this.
        void PhysicsUpdate(float DeltaTime)
        {
            var lastframe = new Timestamp { };
            foreach (var input in m_Inputs)
            {
                TryAddCharacterFromInput(input);

                float clientDeltaTime = 0.0f;

                var character = m_Characters[input.Id.Name];
                if (character.m_TimeOfLastUpdate == -1) //first update sent
                {
                    character.m_TimeOfLastUpdate = input.SentTime.Nanos; //TODO: move to constants, also swap out the timestamp class for just raw miliseconds since epoc 
                    character.m_TimeSinceLastUpdate = 0;
                    continue;
                }
                else
                {
                    clientDeltaTime = (input.SentTime.Nanos - character.m_TimeOfLastUpdate) / 1000000;
                    lastframe = input.SentTime;
                    character.m_TimeOfLastUpdate = input.SentTime.Nanos; //TODO: effeciency
                    character.m_TimeSinceLastUpdate = 0;
                    character.Update(input, clientDeltaTime);
                }

                //Debug Line
                //Console.WriteLine(input.ToString());
            }

            BroadcastWorldUpdate();
        }

        // Sends a world update to all connected clients
        void BroadcastWorldUpdate()
        {
            WorldUpdate worldUpdate = new WorldUpdate();
            foreach (var character in m_Characters)
                worldUpdate.Updates.Add(character.Value.ToEntityUpdate());

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
