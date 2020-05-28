using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] GameObject CharacterPrefab;
    List<Character> mCharacters = new List<Character>(); // Change to hashmap (or the c# equivalent)

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void SpawnCharacter(string Name = "Default", bool Player = false, Vector3 Position = new Vector3(), Vector3 Rotation = new Vector3())
    {
        if (mCharacters.Exists(character => character.Name == Name))
            throw new System.ArgumentException("Duplicate Character Name");

        // Init new character
        var newCharacter = Instantiate(CharacterPrefab, Position, Quaternion.Euler(Rotation));
        var newCharacterScript = newCharacter.GetComponent<Character>();

        // Set new character values
        newCharacterScript.Name = Name;
        newCharacterScript.Local = Player;
        if (!Player)
        {
            newCharacterScript.GetComponentInChildren<Camera>().enabled = false;
            newCharacterScript.GetComponentInChildren<AudioListener>().enabled = false;
        }

        mCharacters.Add(newCharacterScript);
    }

    public void DespawnCharacter(string Name)
    {
        var foundCharacter = mCharacters.Find(character => character.Name == Name);
        mCharacters.Remove(foundCharacter);
        Destroy(foundCharacter.gameObject);
    }

    
}
