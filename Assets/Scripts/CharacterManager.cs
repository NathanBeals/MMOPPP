using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

// Static manager that will store references to all charcters in all loaded levels
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager s_Instance;
    [SerializeField] List<Character> m_Characters;

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

    // On level loaded recalculate the list of characters
    public void OnLevelWasLoaded(int level)
    {
        s_Instance.m_Characters.Clear();
        s_Instance.m_Characters.AddRange(FindObjectsOfType<Character>());
    }

    public static List<Character> GetCharacters() { return s_Instance.m_Characters; }

    public static void AddCharacter(Character character)
    {
        s_Instance.m_Characters.Add(character);
    }

    public static void RemoveCharacter(Character character)
    {
        s_Instance.m_Characters.Remove(character);
    }
}
