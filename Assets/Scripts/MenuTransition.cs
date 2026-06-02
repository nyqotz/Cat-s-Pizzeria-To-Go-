using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuTransition : MonoBehaviour
{
    public RectTransform background;
    public RectTransform playButton;
    public RectTransform settingsButton;
    public RectTransform playGameText;

    private bool isAnimating = false;

    public void PlayGame()
    {
        if (isAnimating) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
            AudioManager.Instance.PlayShutter();
        }

        StartCoroutine(SlideUp());
    }

    IEnumerator SlideUp()
    {
        isAnimating = true;

        float duration = 0.6f;
        float time = 0f;

        Vector2 bgStart = background.anchoredPosition;
        Vector2 bgEnd = bgStart + new Vector2(0, Screen.height);

        Vector2 btnStart = playButton.anchoredPosition;
        Vector2 btnEnd = btnStart + new Vector2(0, Screen.height);

        Vector2 settingsStart = settingsButton.anchoredPosition;
        Vector2 settingsEnd = settingsStart + new Vector2(0, Screen.height);

        Vector2 textStart = playGameText.anchoredPosition;
        Vector2 textEnd = textStart + new Vector2(0, Screen.height);

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            background.anchoredPosition =
                Vector2.Lerp(bgStart, bgEnd, t);

            playButton.anchoredPosition =
                Vector2.Lerp(btnStart, btnEnd, t);

            settingsButton.anchoredPosition =
                Vector2.Lerp(settingsStart, settingsEnd, t);

            playGameText.anchoredPosition =
                Vector2.Lerp(textStart, textEnd, t);

            yield return null;
        }

        SceneManager.LoadScene("LoginScene");
    }
}