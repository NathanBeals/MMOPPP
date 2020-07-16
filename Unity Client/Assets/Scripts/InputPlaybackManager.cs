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
  float start = 0;
  float end = 0;

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
   // start = Time.realtimeSinceStartup;

    if (Inputs.Count != 0)
      Debug.Log("Time To: " + ((Inputs[Inputs.Count - 1].SentTime - Inputs[0].SentTime) / 1000.0f));
    //Debug.Log("delay" + (Time.realtimeSinceStartup - faultTime)); //WORKING HERE: TODO: .1 seconds delay (no input) local, almost .5 seconds delay (no input) remote
  }

  IEnumerator PlaybackInputs()
  {
    float uniformWaitTime = ((MMOPPPLibrary.Constants.ServerTickRate - 50.0f) / m_Inputs.Count) / 1000.0f;
    start = Time.realtimeSinceStartup;
    float timespent = 0;
    while (m_Inputs.Count != 0)
    {
      float startUpdate = Time.realtimeSinceStartup;
      if (m_CurrentInput != null)
      {
        yield return new WaitForSecondsRealtime(uniformWaitTime - (Time.realtimeSinceStartup- startUpdate));

        var delay = m_Inputs.Peek().SentTime - m_CurrentInput.SentTime;
        MMOPPPLibrary.CharacterController.ApplySingleInput(
        m_CurrentInput,
        new System.Numerics.Vector3(transform.position.x, transform.position.y, transform.position.z),
        delay, // miliseconds
        OnPositionCalculated);

        if (m_Inputs.Count != 1)
          m_CurrentInput = m_Inputs.Dequeue();

        // Debug
        if (m_Inputs.Count == 1)
        {
          Debug.Log("Time Spent Real: " + ((Time.realtimeSinceStartup - start) * 1000));
          Debug.Log("Time Spent: " + timespent);
          yield break;
        }

        //var ddiv = delay / 1000.0f;
        timespent += delay;
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
