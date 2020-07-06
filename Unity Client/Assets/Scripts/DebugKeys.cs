using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Linq;

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

    void SaveWorldState()
    {
        m_CharacterData.Clear();

        foreach (var character in CharacterManager.GetCharacters())
        {
            if (character.Value == null)
                continue;

            var saveData = new WorldServerSync.CharacterDownlinkData();
            saveData.m_Name = character.Value.m_ID;
            saveData.m_Location = character.Value.gameObject.transform.position;
            saveData.m_Rotation = character.Value.gameObject.transform.rotation.eulerAngles;
            m_CharacterData.Add(saveData);
        }
    }

    void LoadWorldState()
    {
        WorldServerSync.QueueNewUpdateData(m_CharacterData);
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        inputActions = new PlayerInputActions();
        inputActions.DebugControls.ExitGame.performed += ctx => ExitGame();
        inputActions.DebugControls.SpawnRandomCharacter.performed += ctx => SpawnRandomCharacter();
        inputActions.DebugControls.SaveWorldState.performed += ctx => SaveWorldState();
        inputActions.DebugControls.LoadWorldState.performed += ctx => LoadWorldState();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
