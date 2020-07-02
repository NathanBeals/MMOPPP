using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
  [SerializeField] public bool m_Local = false;
  [SerializeField] public string m_ID;
  [SerializeField] public float m_CharacterHalfHeight = .5f;

  private void Awake()
  {
    if (m_Local)
      m_ID = PlayerPrefs.GetString(MMOPPPConstants.s_CharacterKey);
  }

  private void Start()
  {
    CharacterManager.AddCharacter(this);
  }
}
