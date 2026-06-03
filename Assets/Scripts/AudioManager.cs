using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

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
        }
        else
        {
            Destroy(gameObject);
        }
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

        shutterRoutine = StartCoroutine(PlayShortShutter());
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