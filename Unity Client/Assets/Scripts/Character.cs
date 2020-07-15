using Google.Protobuf.MMOPPP.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using V3 = UnityEngine.Vector3;
using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;
using TMPro.Examples;
using System;
using System.Linq;
using Invector.vCharacterController;
using TMPro;
using System.ComponentModel;

public class Character : MonoBehaviour
{
  [SerializeField] public bool m_Local = false;
  [SerializeField] public string m_ID;
  [SerializeField] public float m_CharacterHalfHeight = .5f;

  private InputPlaybackManager m_PlaybackManager; // (Used for RPC Positions and animations)
  private List<ClientInput> m_LocalInputs = new List<ClientInput>(); // Inputs stored between server updates (Used for LPC positions)
  private UnityEngine.Vector3 m_ServerPosition;
  private ulong m_LastServerInputTimestamp;

  // UI 
  public TMPro.TextMeshPro DisplayName;

  private void Awake()
  {
    if (m_Local)
      m_ID = PlayerPrefs.GetString(MMOPPPConstants.s_CharacterKey);
  }

  private void Start()
  {
    if (m_Local)
      CharacterManager.AddCharacter(this);

    m_PlaybackManager = GetComponent<InputPlaybackManager>();
    GetComponentInChildren<vThirdPersonController>().m_ExternallyOverridingMovements = true;
  }

  public InputPlaybackManager GetInputPlaybackManager()
  {
    return m_PlaybackManager;
  }

  public void RecordLocalInput(ClientInput LocalInput)
  {
    m_LocalInputs.Add(LocalInput);
  }

  public void ApplyLocalInputs()
  {
    if (m_LocalInputs.Count < 1)
      return;

    MMOPPPLibrary.CharacterController.ApplyInputs(
      m_LocalInputs, 
      new System.Numerics.Vector3(m_ServerPosition.x, m_ServerPosition.y, m_ServerPosition.z),
      OnPositionCalculated);
  }

  public void OnPositionCalculated(System.Numerics.Vector3 CurrentPosition)
  {
    transform.position = new V3(CurrentPosition.X, CurrentPosition.Y + m_CharacterHalfHeight, CurrentPosition.Z);
  }

  public void ResetLocalInputs()
  {
    m_LocalInputs.Clear();
  }

  public void ServerUpdate(ServerUpdate Update)
  {
    m_ServerPosition = new V3(Update.Location.X, Update.Location.Y, Update.Location.Z);
    m_LastServerInputTimestamp = Update.PastInputs.Last().SentTime;

    m_LocalInputs.RemoveAll((ClientInput a) => { return a.Input.SentTime <= m_LastServerInputTimestamp; });
  }

  // Helpers
  public static GV3 V3ToGV3(V3 VInput)
  {
    return new GV3 { X = VInput.x, Y = VInput.y, Z = VInput.z };
  }

  public static V3 GV3ToV3(GV3 VInput)
  {
    return new V3(VInput.X, VInput.Y, VInput.Z);
  }

  public void Update()
  {
    if (m_Local)
      ApplyLocalInputs();

    UpdateUI();
  }

  public void UpdateUI()
  {
    // HACK: slow
    V3 camPos = new V3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
    V3 textPos = new V3(DisplayName.transform.position.x, 0, DisplayName.transform.position.z);
    V3 relativePos = textPos - camPos;
    DisplayName.transform.rotation = Quaternion.LookRotation(relativePos, V3.up);
    DisplayName.text = m_ID;
  }
}
