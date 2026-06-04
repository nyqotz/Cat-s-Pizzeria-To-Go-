using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenPanelManager : MonoBehaviour
{
    public GameObject kitchenHub;
    public GameObject backToMainButton;

    public RectTransform doughPanel;
    public RectTransform ingredientsPanel;
    public RectTransform ovenPanel;

    public OvenManager ovenManager;

    public float slideDuration = 0.2f;

    private RectTransform currentPanel;
    private bool isSliding = false;
    private bool panelOpen = false;

    void Start()
    {
        ShowKitchenHub();
    }

    public void ShowKitchenHub()
    {
        panelOpen = false;
        currentPanel = null;

        if (backToMainButton != null)
            backToMainButton.SetActive(true);

        if (kitchenHub != null)
            kitchenHub.SetActive(true);

        if (doughPanel != null)
            doughPanel.gameObject.SetActive(false);

        if (ingredientsPanel != null)
            ingredientsPanel.gameObject.SetActive(false);

        if (ovenPanel != null)
            ovenPanel.gameObject.SetActive(false);
    }

    public void OpenDoughStation()
    {
        OpenPanelDirectly(doughPanel);
    }

    public void OpenIngredientsStation()
    {
        OpenPanelDirectly(ingredientsPanel);
    }

    public void OpenOvenStation()
    {
        if (ovenManager != null)
            ovenManager.BuildPreparedPizzaPreview();

        OpenPanelDirectly(ovenPanel);
    }

    void OpenPanelDirectly(RectTransform panel)
    {
        if (panel == null)
            return;

        PlayClick();

        if (backToMainButton != null)
            backToMainButton.SetActive(false);

        if (kitchenHub != null)
            kitchenHub.SetActive(false);

        if (doughPanel != null)
            doughPanel.gameObject.SetActive(false);

        if (ingredientsPanel != null)
            ingredientsPanel.gameObject.SetActive(false);

        if (ovenPanel != null)
            ovenPanel.gameObject.SetActive(false);

        panel.gameObject.SetActive(true);
        panel.anchoredPosition = Vector2.zero;

        currentPanel = panel;
        panelOpen = true;
    }

    public void ShowDoughPanel()
    {
        if (!panelOpen)
        {
            OpenDoughStation();
            return;
        }

        SlideToPanel(doughPanel, -1);
    }

    public void ShowIngredientsPanel()
    {
        if (!panelOpen)
        {
            OpenIngredientsStation();
            return;
        }

        int direction =
            currentPanel == doughPanel
            ? 1
            : -1;

        SlideToPanel(ingredientsPanel, direction);
    }

    public void ShowOvenPanel()
    {
        if (ovenManager != null)
            ovenManager.BuildPreparedPizzaPreview();

        if (!panelOpen)
        {
            OpenOvenStation();
            return;
        }

        SlideToPanel(ovenPanel, 1);
    }

    public void BackToKitchenHub()
    {
        PlayClick();
        ShowKitchenHub();
    }

    public void BackToMainScene()
    {
        PlayClick();
        SceneManager.UnloadSceneAsync("KitchenScene");
    }

    void SlideToPanel(RectTransform nextPanel, int direction)
    {
        if (isSliding || nextPanel == null || nextPanel == currentPanel)
            return;

        PlayClick();

        if (backToMainButton != null)
            backToMainButton.SetActive(false);

        StartCoroutine(
            SlideAnimation(nextPanel, direction)
        );
    }

    IEnumerator SlideAnimation(RectTransform nextPanel, int direction)
    {
        isSliding = true;

        float screenWidth =
            ((RectTransform)currentPanel.parent).rect.width;

        Vector2 currentStart = Vector2.zero;
        Vector2 currentEnd = new Vector2(-screenWidth * direction, 0);

        Vector2 nextStart = new Vector2(screenWidth * direction, 0);
        Vector2 nextEnd = Vector2.zero;

        nextPanel.gameObject.SetActive(true);
        nextPanel.anchoredPosition = nextStart;

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / slideDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            currentPanel.anchoredPosition =
                Vector2.Lerp(currentStart, currentEnd, t);

            nextPanel.anchoredPosition =
                Vector2.Lerp(nextStart, nextEnd, t);

            yield return null;
        }

        currentPanel.gameObject.SetActive(false);
        currentPanel.anchoredPosition = Vector2.zero;

        nextPanel.anchoredPosition = Vector2.zero;
        currentPanel = nextPanel;

        isSliding = false;
    }

    void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();
    }
}