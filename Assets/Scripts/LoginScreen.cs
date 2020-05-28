using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoginScreen : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField UserNameField;
    [SerializeField] UnityEngine.UI.Button LoginButton;

    public void AttemptLogin()
    {
        if (UserNameField.text.Length < MMOPPPConstants.MinNameLength || UserNameField.text.Length > MMOPPPConstants.MaxNameLength)
            return;

        // Username passed validation
        PlayerPrefs.SetString(MMOPPPConstants.CharacterKey, UserNameField.text);

        // Load main scene
        SceneManager.LoadScene(MMOPPPConstants.DefaultLevel);
    }
}
