using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }
    public FirebaseUser CurrentUser { get; private set; }

    public bool IsReady { get; private set; }

    private const string RememberMeKey = "RememberMe";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus status = task.Result;

            if (status == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                Firestore = FirebaseFirestore.DefaultInstance;
                CurrentUser = Auth.CurrentUser;

                Auth.StateChanged += OnAuthStateChanged;

                IsReady = true;
                Debug.Log("Firebase pronto.");
            }
            else
            {
                IsReady = false;
                Debug.LogError("Firebase dependencies non disponibili: " + status);
            }
        });
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        CurrentUser = Auth.CurrentUser;
    }

    public bool GetRememberMe()
    {
        return PlayerPrefs.GetInt(RememberMeKey, 0) == 1;
    }

    public void SetRememberMe(bool value)
    {
        PlayerPrefs.SetInt(RememberMeKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void Register(string email, string password, string username, Action<bool, string> callback)
    {
        if (!IsReady)
        {
            callback?.Invoke(false, "Firebase non è ancora pronto.");
            return;
        }

        Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(false, GetErrorMessage(task.Exception));
                return;
            }

            CurrentUser = task.Result.User;

            CreateUserDocument(CurrentUser.UserId, email, username, success =>
            {
                if (success)
                    callback?.Invoke(true, "Account creato!");
                else
                    callback?.Invoke(false, "Account creato, ma errore nel salvataggio dati.");
            });
        });
    }

    public void Login(string email, string password, bool rememberMe, Action<bool, string> callback)
    {
        if (!IsReady)
        {
            callback?.Invoke(false, "Firebase non è ancora pronto.");
            return;
        }

        Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(false, GetErrorMessage(task.Exception));
                return;
            }

            CurrentUser = task.Result.User;
            SetRememberMe(rememberMe);

            callback?.Invoke(true, "Login effettuato!");
        });
    }

    public void SendPasswordReset(string email, Action<bool, string> callback)
    {
        if (!IsReady)
        {
            callback?.Invoke(false, "Firebase non è ancora pronto.");
            return;
        }

        Auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(false, GetErrorMessage(task.Exception));
                return;
            }

            callback?.Invoke(true, "Email di recupero inviata.");
        });
    }

    public void Logout()
    {
        SetRememberMe(false);

        if (Auth != null)
            Auth.SignOut();

        CurrentUser = null;
    }

    private void CreateUserDocument(string userId, string email, string username, Action<bool> callback)
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "email", email },
            { "username", username },
            { "currentDay", 1 },
            { "reputation", 3 },
            { "money", 0 },
            { "totalCustomersServed", 0 },
            { "createdAt", Timestamp.GetCurrentTimestamp() },
            { "lastSave", Timestamp.GetCurrentTimestamp() }
        };

        Firestore.Collection("users").Document(userId).SetAsync(data).ContinueWithOnMainThread(task =>
        {
            callback?.Invoke(!task.IsCanceled && !task.IsFaulted);
        });
    }

    private string GetErrorMessage(AggregateException exception)
    {
        if (exception == null)
            return "Errore sconosciuto.";

        Exception inner = exception.Flatten().InnerException;

        if (inner == null)
            return exception.Message;

        return inner.Message;
    }

    private void OnDestroy()
    {
        if (Auth != null)
            Auth.StateChanged -= OnAuthStateChanged;
    }
}