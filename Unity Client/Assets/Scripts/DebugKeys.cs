using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using UnityEngine;

public class DebugKeys : MonoBehaviour
{
  [SerializeField] int m_TestSpawnCount = 1;
  [SerializeField] List<WorldServerSync.CharacterDownlinkData> m_CharacterData = new List<WorldServerSync.CharacterDownlinkData>();

  // Inputs
  PlayerInputActions inputActions;

  void ExitGame()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
  }

  void SpawnRandomCharacter()
  {
    System.Random random = new System.Random();

    foreach (var index in Enumerable.Range(1, m_TestSpawnCount))
      CharacterSpawner.SpawnCharacter(random.Next(1, 100000).ToString(), false, new Vector3(random.Next(-50, 50), 1.5f, random.Next(-50, 50)));
  }

  void ReturnToTitle()
  {
    SceneManager.LoadScene("UI");
  }

  void Awake()
  {
    DontDestroyOnLoad(gameObject);

    inputActions = new PlayerInputActions();
    inputActions.DebugControls.ExitGame.performed += ctx => ExitGame();
    inputActions.DebugControls.SpawnRandomCharacter.performed += ctx => SpawnRandomCharacter();
    inputActions.DebugControls.ReturnToTitle.performed += ctx => ReturnToTitle();
    inputActions.DebugControls.IncrementLag.performed += ctx => ModifyLag(.1f);
    inputActions.DebugControls.DecrementLag.performed += ctx => ModifyLag(-.1f);
    inputActions.DebugControls.ToggleMouseLock.performed += ctx => ToggleMouseLock();
  }

  private void ModifyLag(float value)
  {
    var connection = FindObjectOfType<TCPConnection>();
    if (connection)
      connection.m_ArtificialLag = Mathf.Max(0, connection.m_ArtificialLag + value);
    Debug.Log($"Current Lag = {connection.m_ArtificialLag}s");
  }

  private void OnEnable()
  {
    inputActions.Enable();
  }

  private void OnDisable()
  {
    inputActions.Disable();
  }

  private void ToggleMouseLock()
  {
    if (UnityEngine.Cursor.lockState == CursorLockMode.Locked)
      UnityEngine.Cursor.lockState = CursorLockMode.None;
    else
      UnityEngine.Cursor.lockState = CursorLockMode.Locked;

    UnityEngine.Cursor.visible = UnityEngine.Cursor.lockState != CursorLockMode.Locked;
  }
}
