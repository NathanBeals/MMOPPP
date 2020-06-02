﻿using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public static CharacterSpawner s_Instance;

    [SerializeField] GameObject m_CharacterPrefab;

    public void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (s_Instance != this)
            Destroy(gameObject);
    }

    static public void SpawnCharacter(string Name = "Default", bool Player = false, Vector3 Position = new Vector3(), Vector3 Rotation = new Vector3())
    {
        if (CharacterManager.GetCharacters().Exists(character => character.Name == Name))
            throw new System.ArgumentException("Duplicate Character Name");

        // Init new character
        var newCharacter = Instantiate(s_Instance.m_CharacterPrefab, Position, Quaternion.Euler(Rotation));
        var newCharacterScript = newCharacter.GetComponent<Character>();

        // Set new character values
        newCharacterScript.Name = Name;
        newCharacterScript.Local = Player;

        CharacterManager.AddCharacter(newCharacterScript);
    }

    static public void DespawnCharacter(string Name)
    {
        var foundCharacter = CharacterManager.GetCharacters().Find(character => character.Name == Name);
        CharacterManager.RemoveCharacter(foundCharacter);
        Destroy(foundCharacter.gameObject);
    }
}
