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
using TMPro;
using System.Linq;

public class WorldServerSync : MonoBehaviour
{
  public static WorldServerSync s_Instance;

  public GameObject m_PlayerStamp;
  public GameObject m_PlayerPrefab;
  public bool m_DisplayLocalPlayerStamps = false;

  ServerUpdates m_QueuedServerUpdates = null;
  Queue<List<CharacterDownlinkData>> m_DataFromServer = new Queue<List<CharacterDownlinkData>>();

  public const float mTeleportThreshold = 5.0f; // If the reconcilliation distance is greater than this, just teleport to resolve instead of lerp

  public void QueueNewUpdate(ServerUpdates Update)
  {
    m_QueuedServerUpdates = Update;
    //Debug.Log(Update.ToString());
  }

  //HACK: rework for new messages
  public struct CharacterDownlinkData
  {
    public string m_Name;
    public V3 m_Location; // Location in worldspace
    public V3 m_Rotation; // Direction of the body

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

  void UpdateCharacter(Character UpdateCharacter, CharacterDownlinkData Data)
  {
    var cGameObjTrans = UpdateCharacter.gameObject.transform;

    cGameObjTrans.position = Data.m_Location;
    cGameObjTrans.rotation = Quaternion.Euler(Data.m_Rotation);
  }

  private void Update()
  {
    if (m_QueuedServerUpdates != null)
    {
      foreach (var entity in m_QueuedServerUpdates.Updates)
      {
        if (entity.BodyRotation == null || entity.Location == null)
          continue;

        if (UpdateLPC(entity))
          continue;

        if (UpdateRPC(entity))
          continue;
      }

      m_QueuedServerUpdates = null;
    }
  }

  bool UpdateLPC(ServerUpdate entity)
  {
    var localCharacter = CharacterManager.GetLocalCharacter();

    if (localCharacter == null || entity.Name != localCharacter.m_ID)
      return false;

    localCharacter.StopAllCoroutines();
    localCharacter.StartCoroutine(ReconcilePosition(localCharacter.gameObject,
       localCharacter.gameObject.transform.position,
       new V3(entity.Location.X, entity.Location.Y + localCharacter.m_CharacterHalfHeight, entity.Location.Z),
       MMOPPPLibrary.Constants.ServerTickRate / 1000.0f));

    if (m_DisplayLocalPlayerStamps)
      CreatePlayerStamp(entity);

    return true;
  }

  bool UpdateRPC(ServerUpdate entity)
  {
    Character character = null;
    CharacterManager.GetCharacters().TryGetValue(entity.Name, out character);

    if (!character)
    {
      GameObject newCharacterBody = Instantiate(m_PlayerPrefab);
      character = newCharacterBody.GetComponent<Character>();
      character.m_ID = entity.Name;
      CharacterManager.AddCharacter(character);
    }

    character.StopAllCoroutines();
    character.StartCoroutine(ReconcilePosition(character.gameObject,
      character.gameObject.transform.position,
      new V3(entity.Location.X, entity.Location.Y + character.m_CharacterHalfHeight, entity.Location.Z),
      MMOPPPLibrary.Constants.ServerTickRate / 1000.0f));
    character.transform.eulerAngles = new V3(entity.BodyRotation.X, entity.BodyRotation.Y, entity.BodyRotation.Z);

    Debug.Log(entity.Name + " : " + entity.Location);

    //TODO: working here
    character.GetInputPlaybackManager()?.UpdateReplayInputs(entity.PastInputs.ToList());

    return true;
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

  public void CreatePlayerStamp(ServerUpdate Update)
  {
    var newStamp = Instantiate(m_PlayerStamp,
    new V3(Update.Location.X, Update.Location.Y, Update.Location.Z),
    Quaternion.Euler(0.0f, Update.PastInputs[Update.PastInputs.Count - 1].EulerBodyRotation.Y, 0.0f));

    newStamp.GetComponentInChildren<TextMeshPro>()?.SetText(Update.Name);

    Destroy(newStamp, 1);
  }

  IEnumerator ReconcilePosition(GameObject Body, V3 OldPosition, V3 NewPosition, float Duration)
  {
    V3 difference = NewPosition - OldPosition;

    if (difference.magnitude > mTeleportThreshold) // For extremes just teleport //TODO: make this a constant
    {
      Body.transform.position = NewPosition;
      yield break;
    }

    float elapsedTime = 0;
    V3 differencePerFrame = difference * Time.fixedDeltaTime / Duration;

    do
    {
      yield return new WaitForFixedUpdate();
      elapsedTime += Time.fixedDeltaTime;
      Body.transform.position += differencePerFrame;
    } while (elapsedTime < Duration);

    yield return null;
  }
}
