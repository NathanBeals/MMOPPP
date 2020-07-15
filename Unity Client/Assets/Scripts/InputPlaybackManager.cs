using Google.Protobuf.MMOPPP.Messages;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GInput = Google.Protobuf.MMOPPP.Messages.Input;

public class InputPlaybackManager : MonoBehaviour
{
  Queue<GInput> m_Inputs = new Queue<GInput>();
  GInput m_CurrentInput;
  float m_LocalDeltaTime = 0; // Miliseconds
  ulong m_BaseTime = 0; // Miliseconds
  UnityEngine.Vector3 m_OldServerLocation = UnityEngine.Vector3.zero;
  UnityEngine.Vector3 m_OldServerRotation = UnityEngine.Vector3.zero;

  vThirdPersonInput m_InputSystem;

  // Debug
  float faultTime = 0;

  public void Start()
  {
    m_InputSystem = GetComponent<vThirdPersonInput>();
  }

  public void UpdateReplayInputs(List<GInput> Inputs)
  {
    if (Inputs.Count == 0)
      return;

    StopAllCoroutines();

    foreach (var x in Inputs)
      m_Inputs.Enqueue(x);
    m_LocalDeltaTime = 0;

    if (m_Inputs.Count > 0)
      m_BaseTime = m_Inputs.Peek().SentTime;

    StartCoroutine(PlaybackInputs());
    //Debug.Log("delay" + (Time.realtimeSinceStartup - faultTime)); //WORKING HERE: TODO: .1 seconds delay (no input) local, almost .5 seconds delay (no input) remote
  }

  IEnumerator PlaybackInputs()
  {
    while (m_Inputs.Count != 0)
    {
      if (m_CurrentInput != null)
      {
        var delay = m_Inputs.Peek().SentTime - m_CurrentInput.SentTime;
        yield return new WaitForSeconds(delay / 1000);

        MMOPPPLibrary.CharacterController.ApplySingleInput(
        m_CurrentInput,
        new System.Numerics.Vector3(transform.position.x, transform.position.y, transform.position.z),
        delay, // miliseconds
        OnPositionCalculated);

        m_CurrentInput = m_Inputs.Dequeue();
      }
      else
        m_CurrentInput = m_Inputs.Dequeue();
    }

    yield break;
  }

  private void Update()
  {
    if (m_Inputs.Count == 0)
      return;

    //var timeSinceUpdate = (m_Inputs.Peek().SentTime - m_BaseTime);

    //if (m_Inputs.Count != 0 && timeSinceUpdate / 1000 < m_LocalDeltaTime)
    //{
    //  timeSinceUpdate = (m_Inputs.Peek().SentTime - m_BaseTime);
    //  if (m_CurrentInput != null)
    //  {
    //    var deltaTime = (m_Inputs.Peek().SentTime - m_CurrentInput.SentTime);
    //    MMOPPPLibrary.CharacterController.ApplySingleInput(
    //      m_CurrentInput,
    //      new System.Numerics.Vector3(transform.position.x, transform.position.y, transform.position.z),
    //      deltaTime, // miliseconds
    //      OnPositionCalculated);
    //  }

    //  m_CurrentInput = m_Inputs.Pop();
    //}

    //if (m_Inputs.Peek().SentTime - m_BaseTime > m_LocalDeltaTime * 1000)
    //{
    //  if (m_CurrentInput != null)
    //  {
    //    MMOPPPLibrary.CharacterController.ApplySingleInput(
    //      m_CurrentInput,
    //      new System.Numerics.Vector3(transform.position.x, transform.position.y, transform.position.z),
    //      (m_Inputs.Peek().SentTime - m_CurrentInput.SentTime), // miliseconds
    //      OnPositionCalculated);
    //  }

    //  m_CurrentInput = m_Inputs.Pop();
    //}

    //if (m_Inputs.Count > 0)
    //{
    //  m_CurrentInput = m_Inputs.Dequeue();
    //  if (m_Inputs.Count > 0)
    //  {
    //    MMOPPPLibrary.CharacterController.ApplySingleInput(
    //      m_CurrentInput,
    //      new System.Numerics.Vector3(transform.position.x, transform.position.y, transform.position.z),
    //      (m_Inputs.Peek().SentTime - m_CurrentInput.SentTime), // miliseconds
    //      OnPositionCalculated);
    //  }

    //  faultTime = Time.realtimeSinceStartup;
    //}

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
    transform.position = new UnityEngine.Vector3(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Z);
  }

  public UnityEngine.Vector3 GetOldServerLocation()
  {
    return m_OldServerLocation;
  }
  public void SetOldServerLocation(UnityEngine.Vector3 Location)
  {
    m_OldServerLocation = Location;
  }
  public UnityEngine.Vector3 GetOldServerRotation()
  {
    return m_OldServerRotation;
  }
  public void SetOldServerRotation(UnityEngine.Vector3 Location)
  {
    m_OldServerRotation = Location;
  }

}
