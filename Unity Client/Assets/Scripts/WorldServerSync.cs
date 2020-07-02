using Google.Protobuf.MMOPPP.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

using V3 = UnityEngine.Vector3;
using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;
using UnityEditor;
using TMPro;

public class WorldServerSync : MonoBehaviour
{
  public static WorldServerSync s_Instance;

  public GameObject m_PlayerPlaceholder;
  public bool m_DisplayLocalPlayerStamps = false;

  WorldUpdate m_QueuedWorldUpdate = null;
  Queue<List<CharacterDownlinkData>> m_DataFromServer = new Queue<List<CharacterDownlinkData>>();

  public void QueueNewUpdate(WorldUpdate Update)
  {
    m_QueuedWorldUpdate = Update;
    Debug.Log(Update.ToString());
  }

  // What will store the results from the server, TODO: likely to be replaced by the constructed Json stuff
  public struct CharacterDownlinkData
  {
    public string m_Name; // TODO: consider replacing with ID instead of name
    public V3 m_Location; // Location in worldspace
    public V3 m_Rotation; // Direction of the body

    // TODO: use these for client side interpolation
    public V3 m_MovementInput;
    public V3 m_CameraRotation; // Direction of the body
  }

  public static void QueueNewUpdateData(List<CharacterDownlinkData> Data)
  {
    s_Instance.m_DataFromServer.Enqueue(Data);
  }

  void SyncTransformData()
  {
    if (m_DataFromServer.Count <= 0)
      return;

    var updateData = m_DataFromServer.Peek();

    foreach (var characterData in updateData)
    {
      // If the character does not exist locally, spawn it
      // HACK: slows the main loop
      if (!CharacterManager.GetCharacters().ContainsKey(characterData.m_Name) || CharacterManager.GetCharacters()[characterData.m_Name] == null)
      {
        CharacterManager.GetCharacters().Remove(characterData.m_Name);
        CharacterSpawner.SpawnCharacter(characterData.m_Name);
      }

      Character test = CharacterManager.GetCharacters()[characterData.m_Name];

      UpdateCharacter(test, characterData);
    }

    m_DataFromServer.Dequeue();
  }

  //TODO: add banding mechanism
  void UpdateCharacter(Character UpdateCharacter, CharacterDownlinkData Data)
  {
    var cGameObjTrans = UpdateCharacter.gameObject.transform;

    cGameObjTrans.position = Data.m_Location;
    cGameObjTrans.rotation = Quaternion.Euler(Data.m_Rotation);
  }

  private void Update()
  {
    if (m_QueuedWorldUpdate != null)
    {
      foreach (var entity in m_QueuedWorldUpdate.Updates)
      {
        var localCharacter = CharacterManager.GetLocalCharacter();
        if (entity.Id.Name == localCharacter.m_ID)
        {
          localCharacter.gameObject.transform.position = new V3(entity.Position.X, entity.Position.Y + localCharacter.m_CharacterHalfHeight, entity.Position.Z);
          //TODO: smoothing
          if (m_DisplayLocalPlayerStamps)
            CreatePlayerStamp(entity);
        }
        else
        {
          CreatePlayerStamp(entity);
        }
      }

      m_QueuedWorldUpdate = null;
    }
  }

  public void Awake()
  {
    if (s_Instance == null)
    {
      s_Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else if (s_Instance != this)
      Destroy(gameObject);
  }

  public void FixedUpdate()
  {
    //TODO: convert to an attempt to sync, axing on a try lock
    SyncTransformData();
  }

  public void CreatePlayerStamp(EntityUpdate Update)
  {
    var newStamp = Instantiate(m_PlayerPlaceholder,
    new V3(Update.Position.X, Update.Position.Y, Update.Position.Z),
    Quaternion.Euler(0.0f, Update.PredictiveInputs.EulerRotation.Y, 0.0f));

    newStamp.GetComponentInChildren<TextMeshPro>()?.SetText(Update.Id.Name);

    Destroy(newStamp, 1);
  }
}
