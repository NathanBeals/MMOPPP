using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoginScreen : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField m_UserNameField = null;
    [SerializeField] UnityEngine.UI.Button m_LoginButton;

    public void AttemptLogin()
    {
        if (m_UserNameField.text.Length < MMOPPPConstants.s_MinNameLength || m_UserNameField.text.Length > MMOPPPConstants.s_MaxNameLength)
            return;

        // Username passed validation
        PlayerPrefs.SetString(MMOPPPConstants.s_CharacterKey, m_UserNameField.text);

        // Load main scene
        SceneManager.LoadScene(MMOPPPConstants.s_DefaultLevel);
    }
}
