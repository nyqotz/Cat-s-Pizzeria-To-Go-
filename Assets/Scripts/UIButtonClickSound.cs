using UnityEngine;
using UnityEngine.UI;

public class UIButtonClickSound : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();

        if (btn != null)
        {
            btn.onClick.AddListener(PlayClick);
        }
    }

    void PlayClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }
}