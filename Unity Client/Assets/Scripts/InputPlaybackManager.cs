using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GInput = Google.Protobuf.MMOPPP.Messages.Input;

public class InputPlaybackManager : MonoBehaviour
{
  Stack<GInput> m_Inputs = new Stack<GInput>();
  GInput m_CurrentInput;
  float m_LocalDeltaTime = 0; // Seconds
  float m_BaseTime = 0; // Seconds
  Google.Protobuf.WellKnownTypes.Timestamp m_LastUpdateHandled = null;


  vThirdPersonInput m_InputSystem;

  public void Start()
  {
    m_InputSystem = GetComponent<vThirdPersonInput>();
  }

  public void UpdateReplayInputs(List<GInput> Inputs)
  {
    if (Inputs.Count == 0)
      return;

    m_Inputs = new Stack<GInput>(Inputs);
    m_BaseTime = m_Inputs.Peek().SentTime.Nanos / 1000000000;
  }

  private void Update()
  {
    ProcessInput();

    if (m_Inputs.Count == 0)
      return;

    float toptime = m_Inputs.Peek().SentTime.Nanos / 1000000000 - m_BaseTime;

    if (toptime < m_LocalDeltaTime)
      m_CurrentInput = m_Inputs.Pop();

    m_LocalDeltaTime += Time.deltaTime;
  }

  private void ProcessInput()
  {
    if (m_CurrentInput == null)
      return;

    if (m_LastUpdateHandled == null)
      m_LastUpdateHandled = m_CurrentInput.SentTime;

    ApplySingleInput();

    m_InputSystem.m_MovementInput = new Vector2(m_CurrentInput.PlayerMoveInputs.X, m_CurrentInput.PlayerMoveInputs.Z);
    m_InputSystem.m_FalseCamera.transform.eulerAngles = new Vector3(m_CurrentInput.EulerCameraRotation.X, m_CurrentInput.EulerCameraRotation.Y, m_CurrentInput.EulerCameraRotation.Z);
  }

  void ApplySingleInput()
  {
    var timeStampInput = m_CurrentInput;
    timeStampInput.SentTime = m_LastUpdateHandled;

    MMOPPPLibrary.CharacterController.ApplyInputs(
      new List<Google.Protobuf.MMOPPP.Messages.ClientInput> { timeStampInput, m_CurrentInput },
      new System.Numerics.Vector3(m_ServerPosition.x, m_ServerPosition.y, m_ServerPosition.z),
      OnPositionCalculated);
  }

  public void OnPositionCalculated(System.Numerics.Vector3 CurrentPosition)
  {
    transform.position = new Vector3(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Z);
  }

  public void
}
