using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        settingsPanel.SetActive(false);

        float savedMusic =
            PlayerPrefs.GetFloat("MusicVolume", 10f);

        float savedSFX =
            PlayerPrefs.GetFloat("SFXVolume", 10f);

        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(savedMusic);
            AudioManager.Instance.SetSFXVolume(savedSFX);
        }
    }

    public void ToggleSettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ChangeMusicVolume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicSlider.value);
        }

        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.Save();
    }

    public void ChangeSFXVolume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(sfxSlider.value);
        }

        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
    }
}