using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;

    public void SetVolume(float value)
    {
        musicSource.volume = value / 10f;
    }
}