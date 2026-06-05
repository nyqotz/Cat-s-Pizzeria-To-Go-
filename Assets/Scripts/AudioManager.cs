using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Header("Audio Clips")]
    public AudioClip clickSound;
    public AudioClip shutterSound;

    [Header("SFX Volume Multipliers")]
    [Range(0f, 5f)]
    public float globalSFXMultiplier = 1f;

    [Range(0f, 5f)]
    public float buttonClickMultiplier = 2.3f;

    [Range(0f, 5f)]
    public float shutterMultiplier = 1f;

    private Coroutine shutterRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        UpdateMusicForScene(
            SceneManager.GetActiveScene().name
        );
    }

    void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (scene.name == "KitchenScene")
            return;

        UpdateMusicForScene(scene.name);
    }

    void UpdateMusicForScene(string sceneName)
    {
        AudioClip targetClip = null;

        if (
            sceneName == "StartScene" ||
            sceneName == "StartGameScene" ||
            sceneName == "StartGame" ||
            sceneName == "LoginScene"
        )
        {
            targetClip = menuMusic;
        }
        else if (sceneName == "MainScene")
        {
            targetClip = gameplayMusic;
        }

        if (targetClip == null)
            return;

        if (musicSource == null)
            return;

        if (musicSource.clip == targetClip && musicSource.isPlaying)
            return;

        musicSource.clip = targetClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value / 100f;
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource != null)
            sfxSource.volume = value / 100f;
    }

    public void PlayClick()
    {
        if (sfxSource == null || clickSound == null)
            return;

        sfxSource.PlayOneShot(
            clickSound,
            buttonClickMultiplier * globalSFXMultiplier
        );
    }

    public void PlayShutter()
    {
        if (shutterRoutine != null)
            StopCoroutine(shutterRoutine);

        shutterRoutine =
            StartCoroutine(PlayShortShutter());
    }

    IEnumerator PlayShortShutter()
    {
        if (sfxSource == null || shutterSound == null)
            yield break;

        sfxSource.PlayOneShot(
            shutterSound,
            shutterMultiplier * globalSFXMultiplier
        );

        yield return null;

        shutterRoutine = null;
    }
}