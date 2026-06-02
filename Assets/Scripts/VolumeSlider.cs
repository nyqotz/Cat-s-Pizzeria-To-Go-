using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider slider;

    public void UpdateMusicVolume()
    {
        AudioManager.Instance.SetMusicVolume(slider.value);
    }
}