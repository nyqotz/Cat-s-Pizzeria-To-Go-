using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    public void Login()
{
    string user = usernameInput.text;
    string pass = passwordInput.text;

    if (user.Length < 3)
    {
        feedbackText.text =
            "<color=#FF4444>Username troppo corto</color>";
        return;
    }

    if (pass.Length < 4)
    {
        feedbackText.text =
            "<color=#FF4444>Password troppo corta</color>";
        return;
    }

    feedbackText.text =
        "<color=#00FF88>Bentornato!</color>";

    PlayerPrefs.SetString("username", user);

    StartCoroutine(GoToScene());
}
    IEnumerator GoToScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainScene");
    }
}