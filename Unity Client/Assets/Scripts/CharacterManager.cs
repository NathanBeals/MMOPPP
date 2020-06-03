using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

// Static manager that will store references to all charcters in all loaded levels
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager s_Instance;
    [SerializeField] Dictionary<string, Character> m_Characters = new Dictionary<string, Character>();

    public static Dictionary<string, Character> GetCharacters() { return s_Instance.m_Characters; }

    public static void AddCharacter(Character NewCharacter)
    {
        s_Instance.m_Characters.Add(NewCharacter.m_ID, NewCharacter);
    }

    public static void RemoveCharacter(Character NewCharacter)
    {
        s_Instance.m_Characters.Remove(NewCharacter.m_ID);
    }

    void ResetCharacterList()
    {
        s_Instance.m_Characters.Clear();
        foreach (var character in FindObjectsOfType<Character>())
            m_Characters.Add(character.m_ID, character);
    }

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

    public void Start()
    {
        ResetCharacterList();
    }

    public void OnLevelWasLoaded(int level)
    {
        ResetCharacterList();
    }
}
