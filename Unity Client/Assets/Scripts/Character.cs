using Google.Protobuf.MMOPPP.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using V3 = UnityEngine.Vector3;
using GV3 = Google.Protobuf.MMOPPP.Messages.Vector3;
using TMPro.Examples;

public class Character : MonoBehaviour
{
  [SerializeField] public bool m_Local = false;
  [SerializeField] public string m_ID;
  [SerializeField] public float m_CharacterHalfHeight = .5f;

  private InputPlaybackManager m_PlaybackManager; // (Used for RPC Positions and animations)
  private List<ClientInput> m_LocalInputs = new List<ClientInput>(); // Inputs stored between server updates (Used for LPC positions)

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
  }

  public InputPlaybackManager GetInputPlaybackManager()
  {
    return m_PlaybackManager;
  }

  public void RecordLocalInput(ClientInput LocalInput)
  {
    m_LocalInputs.Add(LocalInput);
  }

  // HACK: should use the same, the exact same, calculations as the server
  // TODO: instead of copying the code pack it into a function in the library
  public void ApplyLocalInputs()
  {
    if (m_LocalInputs.Count < 1)
      return;

    float timeOfLastUpdate = m_LocalInputs[0].Input.SentTime.Nanos;

    foreach (var input in m_LocalInputs)
    {
      float deltaTime =(input.Input.SentTime.Nanos - timeOfLastUpdate) / 1000000; //HACK: math issue, the first input will be ignored and all deltatimes will be offset by 1
      timeOfLastUpdate = input.Input.SentTime.Nanos;

      if (deltaTime < 0.0f)
        return;

      var moveInputs = GV3ToV3(input.Input.PlayerMoveInputs);
      var bodyRotation = GV3ToV3(input.Input.EulerBodyRotation);
      var cameraRotation = GV3ToV3(input.Input.EulerCameraRotation);

      //forward * rotation * move input
      var forward = moveInputs.z;
      var right = moveInputs.x;
      moveInputs.z = Mathf.Cos(cameraRotation.y * (float)Mathf.PI / 180.0f) * forward;
      moveInputs.x = Mathf.Sin(cameraRotation.y * (float)Mathf.PI / 180.0f) * forward;
      moveInputs.z += Mathf.Cos((cameraRotation.y + 90) * (float)Mathf.PI / 180.0f) * right;
      moveInputs.x += Mathf.Sin((cameraRotation.y + 90) * (float)Mathf.PI / 180.0f) * right;

      moveInputs = moveInputs * MMOPPPLibrary.Constants.CharacterMoveSpeed * deltaTime;
      transform.position = transform.position + moveInputs;
      
      //TODO: rotation
      //ServerUpdatePositionUpdate();
      //ServerUpdateBodyRotationUpdate();
    }
  }

  // HACK: overrides the character controller
  public void PlayInput(ClientInput LPCInput, float DeltaTime)
  {
    float timeOfLastUpdate = m_LocalInputs[0].Input.SentTime.Nanos;

    var moveInputs = GV3ToV3(LPCInput.Input.PlayerMoveInputs);
    var bodyRotation = GV3ToV3(LPCInput.Input.EulerBodyRotation);
    var cameraRotation = GV3ToV3(LPCInput.Input.EulerCameraRotation);

    //forward * rotation * move input
    var forward = moveInputs.z;
    var right = moveInputs.x;
    moveInputs.z = Mathf.Cos(cameraRotation.y * (float)Mathf.PI / 180.0f) * forward;
    moveInputs.x = Mathf.Sin(cameraRotation.y * (float)Mathf.PI / 180.0f) * forward;
    moveInputs.z += Mathf.Cos((cameraRotation.y + 90) * (float)Mathf.PI / 180.0f) * right;
    moveInputs.x += Mathf.Sin((cameraRotation.y + 90) * (float)Mathf.PI / 180.0f) * right;

    moveInputs = moveInputs * MMOPPPLibrary.Constants.CharacterMoveSpeed * DeltaTime * 1000;
    transform.position = transform.position + moveInputs;

    //TODO: rotation
    //ServerUpdatePositionUpdate();
    //ServerUpdateBodyRotationUpdate();
  }

  public void ResetLocalInputs()
  {
    m_LocalInputs.Clear();
  }

  // Helpers
  // Hack: Shared Code / very similar code to the server character
  public static GV3 V3ToGV3(V3 VInput)
  {
    return new GV3 { X = VInput.x, Y = VInput.y, Z = VInput.z };
  }

  public static V3 GV3ToV3(GV3 VInput)
  {
    return new V3(VInput.X, VInput.Y, VInput.Z);
  }
}
