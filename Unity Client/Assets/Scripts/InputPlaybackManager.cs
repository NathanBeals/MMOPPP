using Google.Protobuf.MMOPPP.Messages;
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
  float m_LocalDeltaTime = 0; // Miliseconds
  float m_BaseTime = 0; // Miliseconds
  float m_LastInputTime = 0; // Miliseconds

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
    m_BaseTime = m_Inputs.Peek().SentTime;
    m_LastInputTime = m_Inputs.Peek().SentTime;
  }

  private void Update()
  {
    if (m_Inputs.Count == 0)
      return;

    float toptime = m_Inputs.Peek().SentTime - m_BaseTime;

    if (toptime / 1000 < m_LocalDeltaTime)
    {
      //if (m_CurrentInput != null)
      //{
      //  MMOPPPLibrary.CharacterController.ApplySingleInput(
      //    m_CurrentInput,
      //    new System.Numerics.Vector3(transform.position.x, transform.position.y, transform.position.z),
      //     ((m_CurrentInput.SentTime.Nanos / 1000000) - m_LastInputTime) /*Time.deltaTime * 1000*/, // Seconds
      //    OnPositionCalculated);

      //  m_LastInputTime = m_CurrentInput.SentTime.Nanos / 1000000;
      //}
      m_CurrentInput = m_Inputs.Pop();
    }

    UpdateAnimationController();

    m_LocalDeltaTime += Time.deltaTime;
  }

  private void UpdateAnimationController()
  {
    if (m_CurrentInput == null)
      return;

    m_InputSystem.m_MovementInput = new Vector2(m_CurrentInput.PlayerMoveInputs.X, m_CurrentInput.PlayerMoveInputs.Z);
    m_InputSystem.m_FalseCamera.transform.eulerAngles = new UnityEngine.Vector3(m_CurrentInput.EulerCameraRotation.X, m_CurrentInput.EulerCameraRotation.Y, m_CurrentInput.EulerCameraRotation.Z);
  }

  public void OnPositionCalculated(System.Numerics.Vector3 CurrentPosition)
  {
    transform.position =  new UnityEngine.Vector3(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Z);
  }
}
