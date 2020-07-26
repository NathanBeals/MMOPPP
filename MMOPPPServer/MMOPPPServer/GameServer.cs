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
using System.Threading.Tasks;

namespace MMOPPPServer
{
  using V3 = System.Numerics.Vector3;
  using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;
  using CCHelper = MMOPPPLibrary.CharacterController;

  class GameServer
  {
    SQLDB m_Database = new SQLDB();
    ClientManager m_ClientManager = null;
    List<ClientInput> m_Inputs = new List<ClientInput>();
    Dictionary<string, Character> m_Characters = new Dictionary<string, Character>();

    float m_ServerTickRate = Constants.ServerTickRate; //Miliseconds
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
          m_TimeSinceLastTick = 0;//-= m_ServerTickRate;

          m_Inputs = m_ClientManager.GetInputs(); // Blocking Call
          m_ClientManager.ClearInputs(); // Also a blocking Call //TODO: combine in one function so there's no chance of dropping inputs

          UpdateDisconnects(m_ServerTickRate);
          PhysicsUpdate(m_ServerTickRate);
          SaveCharacters();

          //Console.WriteLine("Tick");
          //m_ClientManager.DebugClientCount();
          //m_ClientManager.DebugThreadsUp();
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
        m_Database.SaveCharacterData(pair.Value.m_Name, CCHelper.V3ToGV3(pair.Value.m_Location), CCHelper.V3ToGV3(pair.Value.m_CameraRotation));
    }

    // The first time an input is found for a new character, load that characters position
    void TryAddCharacterFromInput(ClientInput Input)
    {
      if (!m_Characters.ContainsKey(Input.Name))
      {
        var character = new Character(Input.Name);
        m_Characters[Input.Name] = character;
        GV3 loc, rot;
        m_Database.LoadCharacterData(Input.Name, out loc, out rot);
        character.m_Location = CCHelper.GV3ToSNV3(loc);
        character.m_BodyRotation = CCHelper.GV3ToSNV3(rot);
      }
    }

    void PhysicsUpdate(float DeltaTime)
    {
      var inputGroups = m_Inputs.GroupBy(x => x.Name);

      foreach (var characterInputs in inputGroups)
      {
        TryAddCharacterFromInput(characterInputs.First());
        m_Characters[characterInputs.First().Name].Update(characterInputs.ToList());
      }

      BroadcastWorldUpdate();

      PrintPhysicsUpdateDebug();
    }

    // Sends a world update to all connected clients
    void BroadcastWorldUpdate()
    {
      ServerUpdates worldUpdate = new ServerUpdates();
      foreach (var character in m_Characters)
      {
        worldUpdate.Updates.Add(character.Value.GetServerUpdate());
        character.Value.ServerUpdateReset();
      }

      Task.Run(() => m_ClientManager.QueueWorldUpdate(worldUpdate));
      //m_ClientManager.QueueWorldUpdate(worldUpdate);
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

    public void PrintPhysicsUpdateDebug()
    {
      if (m_Characters.Count > 0)
        Console.Write("Online:");
      foreach (var character in m_Characters)
        Console.Write($" {character.Value.m_Name}, ");
      if (m_Characters.Count > 0)
        Console.WriteLine("");
    }
  }
}