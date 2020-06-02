using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] public bool Local = false;
    [SerializeField] public string Name;

    private void Awake()
    {
        if (Local)
            Name = PlayerPrefs.GetString(MMOPPPConstants.CharacterKey);
    }
}
