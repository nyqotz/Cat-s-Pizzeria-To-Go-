using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public int CurrentDay { get; private set; } = 1;
    public float Reputation { get; private set; } = 3f;
    public int Money { get; private set; } = 0;
    public int TotalCustomersServed { get; private set; } = 0;
    public string Username { get; private set; } = "";

    private DocumentReference userDocument;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private bool PrepareUserDocument()
    {
        if (FirebaseManager.Instance == null ||
            !FirebaseManager.Instance.IsReady ||
            FirebaseManager.Instance.CurrentUser == null ||
            FirebaseManager.Instance.Firestore == null)
        {
            Debug.LogWarning("Firebase o utente non pronto.");
            return false;
        }

        string userId = FirebaseManager.Instance.CurrentUser.UserId;
        userDocument = FirebaseManager.Instance.Firestore.Collection("users").Document(userId);
        return true;
    }

    public void LoadPlayerData(Action<bool> callback = null)
    {
        if (!PrepareUserDocument())
        {
            callback?.Invoke(false);
            return;
        }

        userDocument.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Errore caricamento dati: " + task.Exception);
                callback?.Invoke(false);
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            if (!snapshot.Exists)
            {
                Debug.LogWarning("Documento utente non trovato.");
                callback?.Invoke(false);
                return;
            }

            Dictionary<string, object> data = snapshot.ToDictionary();

            Username = GetString(data, "username", "");
            CurrentDay = GetInt(data, "currentDay", 1);
            Reputation = GetFloat(data, "reputation", 3f);
            Money = GetInt(data, "money", 0);
            TotalCustomersServed = GetInt(data, "totalCustomersServed", 0);

            Debug.Log("Dati giocatore caricati.");
            callback?.Invoke(true);
        });
    }

    public void SavePlayerData(Action<bool> callback = null)
    {
        if (!PrepareUserDocument())
        {
            callback?.Invoke(false);
            return;
        }

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "currentDay", CurrentDay },
            { "reputation", Reputation },
            { "money", Money },
            { "totalCustomersServed", TotalCustomersServed },
            { "lastSave", Timestamp.GetCurrentTimestamp() }
        };

        userDocument.UpdateAsync(data).ContinueWithOnMainThread(task =>
        {
            bool success = !task.IsCanceled && !task.IsFaulted;

            if (success)
                Debug.Log("Dati giocatore salvati.");
            else
                Debug.LogError("Errore salvataggio dati: " + task.Exception);

            callback?.Invoke(success);
        });
    }

    public void SetProgress(int currentDay, float reputation, int money, int totalCustomersServed, Action<bool> callback = null)
    {
        CurrentDay = Mathf.Max(1, currentDay);
        Reputation = Mathf.Clamp(reputation, 0f, 5f);
        Money = Mathf.Max(0, money);
        TotalCustomersServed = Mathf.Max(0, totalCustomersServed);

        SavePlayerData(callback);
    }

    public void SetReputation(float value, Action<bool> callback = null)
    {
        Reputation = Mathf.Clamp(value, 0f, 5f);
        SavePlayerData(callback);
    }

    public void AddReputation(float amount, Action<bool> callback = null)
    {
        Reputation = Mathf.Clamp(Reputation + amount, 0f, 5f);
        SavePlayerData(callback);
    }

    public void AddMoney(int amount, Action<bool> callback = null)
    {
        Money = Mathf.Max(0, Money + amount);
        SavePlayerData(callback);
    }

    public void AddServedCustomer(Action<bool> callback = null)
    {
        TotalCustomersServed++;
        SavePlayerData(callback);
    }

    public void CompleteDay(Action<bool> callback = null)
    {
        CurrentDay++;
        SavePlayerData(callback);
    }

    private int GetInt(Dictionary<string, object> data, string key, int defaultValue)
    {
        if (!data.ContainsKey(key) || data[key] == null)
            return defaultValue;

        if (data[key] is long longValue)
            return (int)longValue;

        if (data[key] is int intValue)
            return intValue;

        if (int.TryParse(data[key].ToString(), out int parsedValue))
            return parsedValue;

        return defaultValue;
    }

    private float GetFloat(Dictionary<string, object> data, string key, float defaultValue)
    {
        if (!data.ContainsKey(key) || data[key] == null)
            return defaultValue;

        if (data[key] is double doubleValue)
            return (float)doubleValue;

        if (data[key] is float floatValue)
            return floatValue;

        if (data[key] is long longValue)
            return longValue;

        if (data[key] is int intValue)
            return intValue;

        if (float.TryParse(data[key].ToString(), out float parsedValue))
            return parsedValue;

        return defaultValue;
    }

    private string GetString(Dictionary<string, object> data, string key, string defaultValue)
    {
        if (!data.ContainsKey(key) || data[key] == null)
            return defaultValue;

        return data[key].ToString();
    }
}