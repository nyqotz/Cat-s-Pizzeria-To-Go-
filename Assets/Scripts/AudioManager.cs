using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip clickSound;
    public AudioClip shutterSound;

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
        musicSource.volume = value / 100f;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value / 100f;
    }

    public void PlayClick()
    {
        sfxSource.PlayOneShot(clickSound, 2f);
    }

    public void PlayShutter()
    {
        StartCoroutine(PlayShortShutter());
    }

    IEnumerator PlayShortShutter()
    {
        sfxSource.clip = shutterSound;
        sfxSource.Play();

        yield return new WaitForSeconds(0.45f);

        sfxSource.Stop();
    }
}