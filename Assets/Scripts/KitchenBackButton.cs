using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenBackButton : MonoBehaviour
{
    public void ReturnToMainScene()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        SceneManager.UnloadSceneAsync("KitchenScene");
    }
}