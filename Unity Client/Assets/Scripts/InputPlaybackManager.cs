using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPlaybackManager : MonoBehaviour
{
  Stack<Google.Protobuf.MMOPPP.Messages.Input> m_Inputs;
  float LocalDeltaTime = 0;


  private void Update()
  {
    LocalDeltaTime += Time.deltaTime;
   // m_Inputs.Peek().
  }
}
