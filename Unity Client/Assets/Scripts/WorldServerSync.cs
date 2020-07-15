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

  public const float mTeleportThreshold = 5.0f; // If the reconcilliation distance is greater than this, just teleport to resolve instead of lerp

  public void QueueNewUpdate(ServerUpdates Update)
  {
    m_QueuedServerUpdates = Update;
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

  private void Update()
  {
    if (m_QueuedServerUpdates != null)
    {
      Debug.Log("Server DownLink Up");

      var characters = new List<Character>(CharacterManager.GetCharacters().Values);

      foreach (var entity in m_QueuedServerUpdates.Updates)
      {
        if (entity.BodyRotation == null || entity.Location == null)
          continue;

        characters.RemoveAll((Character a) => { return a.m_ID == entity.Name; });// Remove disconnects //HACK: slow

        if (UpdateLPC(entity))
          continue;

        if (UpdateRPC(entity))
          continue;
      }

      foreach (var character in characters) // Disables the character after one frame, deletes it after 2
      {
        if (character != CharacterManager.GetLocalCharacter())
        {
          CharacterManager.RemoveCharacter(character);
          Destroy(character.gameObject);
        }
      }

      m_QueuedServerUpdates = null;
    }
  }

  bool UpdateLPC(ServerUpdate entity)
  {
    var localCharacter = CharacterManager.GetLocalCharacter();

    if (localCharacter == null || entity.Name != localCharacter.m_ID)
      return false;

    // TODO: should be embedded into the server update of local character
    //localCharacter.StopAllCoroutines();
    //localCharacter.StartCoroutine(ReconcilePosition(localCharacter.gameObject,
    //   localCharacter.gameObject.transform.position,
    //   new V3(entity.Location.X, entity.Location.Y + localCharacter.m_CharacterHalfHeight, entity.Location.Z),
    //   MMOPPPLibrary.Constants.ServerTickRate / 1000.0f));

    localCharacter.ServerUpdate(entity);

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

    //character.StopAllCoroutines();
    //character.StartCoroutine(ReconcilePosition(character.gameObject,
    //  character.gameObject.transform.position,
    //  new V3(entity.Location.X, entity.Location.Y + character.m_CharacterHalfHeight, entity.Location.Z),
    //  MMOPPPLibrary.Constants.ServerTickRate / 1000.0f));
   // character.transform.position = new V3(entity.Location.X, entity.Location.Y + character.m_CharacterHalfHeight, entity.Location.Z);
    //character.transform.eulerAngles = new V3(entity.BodyRotation.X, entity.BodyRotation.Y, entity.BodyRotation.Z); // TODO: remove, calculated locally by input replay

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
