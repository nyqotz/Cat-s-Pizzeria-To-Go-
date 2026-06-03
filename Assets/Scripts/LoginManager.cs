using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject forgotPasswordPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Login")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Toggle rememberMeToggle;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button goToRegisterButton;
    [SerializeField] private Button forgotPasswordButton;
    [SerializeField] private TMP_Text loginMessageText;

    [Header("Register")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backToLoginButton;
    [SerializeField] private TMP_Text registerMessageText;

    [Header("Forgot Password")]
    [SerializeField] private TMP_InputField forgotEmailInput;
    [SerializeField] private Button sendResetButton;
    [SerializeField] private Button backFromForgotButton;
    [SerializeField] private TMP_Text forgotMessageText;

    [Header("Scene")]
    [SerializeField] private string sceneAfterLogin = "MainScene";

    private void Start()
    {
        AddButtonListeners();
        ShowLoadingPanel();

        StartCoroutine(WaitForFirebaseReady());
    }

    private void AddButtonListeners()
    {
        loginButton.onClick.AddListener(Login);
        goToRegisterButton.onClick.AddListener(ShowRegisterPanel);
        forgotPasswordButton.onClick.AddListener(ShowForgotPasswordPanel);

        registerButton.onClick.AddListener(Register);
        backToLoginButton.onClick.AddListener(ShowLoginPanel);

        sendResetButton.onClick.AddListener(SendPasswordReset);
        backFromForgotButton.onClick.AddListener(ShowLoginPanel);
    }

    private IEnumerator WaitForFirebaseReady()
    {
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
            yield return null;

        rememberMeToggle.isOn = FirebaseManager.Instance.GetRememberMe();

        if (FirebaseManager.Instance.CurrentUser != null && FirebaseManager.Instance.GetRememberMe())
        {
            LoadPlayerAndEnterGame();
        }
        else
        {
            ShowLoginPanel();
        }
    }

    public void Login()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email))
        {
            SetLoginMessage("<color=#FF4444>Inserisci l'email.</color>");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetLoginMessage("<color=#FF4444>Inserisci la password.</color>");
            return;
        }

        SetButtonsInteractable(false);
        SetLoginMessage("<color=#FFFFFF>Login in corso...</color>");

        FirebaseManager.Instance.Login(email, password, rememberMeToggle.isOn, (success, message) =>
        {
            SetButtonsInteractable(true);

            if (!success)
            {
                SetLoginMessage("<color=#FF4444>" + message + "</color>");
                return;
            }

            SetLoginMessage("<color=#00FF88>Login effettuato!</color>");
            LoadPlayerAndEnterGame();
        });
    }

    public void Register()
    {
        string username = registerUsernameInput.text.Trim();
        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text;

        if (string.IsNullOrEmpty(username) || username.Length < 3)
        {
            SetRegisterMessage("<color=#FF4444>Username troppo corto.</color>");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            SetRegisterMessage("<color=#FF4444>Inserisci l'email.</color>");
            return;
        }

        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            SetRegisterMessage("<color=#FF4444>La password deve avere almeno 6 caratteri.</color>");
            return;
        }

        SetButtonsInteractable(false);
        SetRegisterMessage("<color=#FFFFFF>Creazione account...</color>");

        FirebaseManager.Instance.Register(email, password, username, (success, message) =>
        {
            SetButtonsInteractable(true);

            if (!success)
            {
                SetRegisterMessage("<color=#FF4444>" + message + "</color>");
                return;
            }

            SetRegisterMessage("<color=#00FF88>Account creato!</color>");
            LoadPlayerAndEnterGame();
        });
    }

    public void SendPasswordReset()
    {
        string email = forgotEmailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            SetForgotMessage("<color=#FF4444>Inserisci la tua email.</color>");
            return;
        }

        SetButtonsInteractable(false);
        SetForgotMessage("<color=#FFFFFF>Invio email...</color>");

        FirebaseManager.Instance.SendPasswordReset(email, (success, message) =>
        {
            SetButtonsInteractable(true);

            if (success)
                SetForgotMessage("<color=#00FF88>" + message + "</color>");
            else
                SetForgotMessage("<color=#FF4444>" + message + "</color>");
        });
    }

    private void LoadPlayerAndEnterGame()
    {
        ShowLoadingPanel();

        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager non trovato. Entro comunque in gioco.");
            SceneManager.LoadScene(sceneAfterLogin);
            return;
        }

        PlayerDataManager.Instance.LoadPlayerData(success =>
        {
            if (success)
            {
                Debug.Log("Dati caricati. Entro in gioco.");
            }
            else
            {
                Debug.LogWarning("Dati non caricati. Entro comunque in gioco.");
            }

            SceneManager.LoadScene(sceneAfterLogin);
        });
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        forgotPasswordPanel.SetActive(false);
        loadingPanel.SetActive(false);

        ClearMessages();
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        forgotPasswordPanel.SetActive(false);
        loadingPanel.SetActive(false);

        ClearMessages();
    }

    public void ShowForgotPasswordPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        forgotPasswordPanel.SetActive(true);
        loadingPanel.SetActive(false);

        ClearMessages();
    }

    private void ShowLoadingPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        forgotPasswordPanel.SetActive(false);
        loadingPanel.SetActive(true);
    }

    private void SetButtonsInteractable(bool value)
    {
        loginButton.interactable = value;
        goToRegisterButton.interactable = value;
        forgotPasswordButton.interactable = value;

        registerButton.interactable = value;
        backToLoginButton.interactable = value;

        sendResetButton.interactable = value;
        backFromForgotButton.interactable = value;
    }

    private void ClearMessages()
    {
        SetLoginMessage("");
        SetRegisterMessage("");
        SetForgotMessage("");
    }

    private void SetLoginMessage(string message)
    {
        if (loginMessageText != null)
            loginMessageText.text = message;
    }

    private void SetRegisterMessage(string message)
    {
        if (registerMessageText != null)
            registerMessageText.text = message;
    }

    private void SetForgotMessage(string message)
    {
        if (forgotMessageText != null)
            forgotMessageText.text = message;
    }
}