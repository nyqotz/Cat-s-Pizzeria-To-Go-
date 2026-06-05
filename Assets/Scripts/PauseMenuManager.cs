using System.Collections;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseButton;
    public GameObject pausePanel;
    public CanvasGroup fadeOverlay;
    public RestaurantManager restaurantManager;

    public float fadeDuration = 1.5f;

    private bool isPaused = false;
    private bool isRestarting = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(false);
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.interactable = false;
        }

        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void ShowPauseButton()
    {
        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void HidePauseButton()
    {
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void TogglePause()
    {
        if (isRestarting)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isRestarting)
            return;

        isPaused = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseButton != null)
            pauseButton.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        Time.timeScale = 1f;

        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void RestartCurrentDay()
    {
        if (isRestarting)
            return;

        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        isRestarting = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(false);

        Time.timeScale = 0f;

        yield return Fade(0f, 1f);

        Time.timeScale = 1f;

        if (restaurantManager != null)
            restaurantManager.RestartCurrentDayFromPause();

        Time.timeScale = 0f;

        yield return Fade(1f, 0f);

        Time.timeScale = 1f;

        isPaused = false;
        isRestarting = false;

        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeOverlay == null)
            yield break;

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.blocksRaycasts = true;
        fadeOverlay.interactable = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            fadeOverlay.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeOverlay.alpha = to;

        if (to == 0f)
        {
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.gameObject.SetActive(false);
        }
    }
}