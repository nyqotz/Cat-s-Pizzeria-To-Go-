using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenPanelManager : MonoBehaviour
{
    public RectTransform doughPanel;
    public RectTransform ingredientsPanel;
    public RectTransform ovenPanel;

    public float slideDuration = 0.45f;

    private RectTransform currentPanel;
    private bool isSliding = false;

    void Start()
    {
        currentPanel = doughPanel;

        doughPanel.gameObject.SetActive(true);
        ingredientsPanel.gameObject.SetActive(false);
        ovenPanel.gameObject.SetActive(false);
    }

    public void ShowDoughPanel()
    {
        SlideToPanel(doughPanel, -1);
    }

    public void ShowIngredientsPanel()
    {
        int direction =
            currentPanel == doughPanel
            ? 1
            : -1;

        SlideToPanel(
            ingredientsPanel,
            direction
        );
    }

    public void ShowOvenPanel()
    {
        SlideToPanel(
            ovenPanel,
            1
        );
    }

    public void BackToMainScene()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        SceneManager.UnloadSceneAsync(
            "KitchenScene"
        );
    }

    void SlideToPanel(
        RectTransform nextPanel,
        int direction
    )
    {
        if (
            isSliding
            || nextPanel == currentPanel
        )
            return;

        PlayClick();

        StartCoroutine(
            SlideAnimation(
                nextPanel,
                direction
            )
        );
    }

    IEnumerator SlideAnimation(
        RectTransform nextPanel,
        int direction
    )
    {
        isSliding = true;

        float screenWidth =
            ((RectTransform)
            currentPanel.parent)
            .rect.width;

        Vector2 currentStart =
            Vector2.zero;

        Vector2 currentEnd =
            new Vector2(
                -screenWidth
                * direction,
                0
            );

        Vector2 nextStart =
            new Vector2(
                screenWidth
                * direction,
                0
            );

        Vector2 nextEnd =
            Vector2.zero;

        nextPanel.gameObject
            .SetActive(true);

        nextPanel.anchoredPosition =
            nextStart;

        float elapsed = 0f;

        while (
            elapsed
            < slideDuration
        )
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed
                    / slideDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            currentPanel
                .anchoredPosition =
                Vector2.Lerp(
                    currentStart,
                    currentEnd,
                    t
                );

            nextPanel
                .anchoredPosition =
                Vector2.Lerp(
                    nextStart,
                    nextEnd,
                    t
                );

            yield return null;
        }

        currentPanel.gameObject
            .SetActive(false);

        currentPanel
            .anchoredPosition =
            Vector2.zero;

        nextPanel
            .anchoredPosition =
            Vector2.zero;

        currentPanel =
            nextPanel;

        isSliding = false;
    }

    void PlayClick()
    {
        if (
            AudioManager.Instance
            != null
        )
        {
            AudioManager.Instance
                .PlayClick();
        }
    }
}