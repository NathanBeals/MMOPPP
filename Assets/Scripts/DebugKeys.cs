using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Linq;

public class DebugKeys : MonoBehaviour
{
    // Settables
    [SerializeField] int TestSpawnCount = 1;

    // Inputs
    PlayerInputActions inputActions;
    float mQuit;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.ExitGame.performed += ctx => ExitGame();
        inputActions.PlayerControls.SpawnRandomCharacter.performed += ctx => SpawnRandomCharacter();
    }

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
        var spawner = UnityEngine.Object.FindObjectOfType<CharacterSpawner>(); //TODO: slow

        System.Random random = new System.Random();

        foreach (var index in Enumerable.Range(1, TestSpawnCount))
            spawner.SpawnCharacter(random.Next(1, 100000).ToString(), false, new Vector3(random.Next(-50, 50), 1.5f, random.Next(-50, 50)));
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
