using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void GoToKitchen()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        SceneManager.LoadScene(
            "KitchenScene",
            LoadSceneMode.Additive
        );
    }

    public void CloseKitchen()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        SceneManager.UnloadSceneAsync("KitchenScene");
    }
}