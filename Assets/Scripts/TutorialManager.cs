using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI MainScene")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    public Button nextButton;

    [Header("Canvas")]
    public Canvas canvas;

    [Header("Arrow")]
    public RectTransform tutorialArrow;
    public float arrowPulseScale = 1.25f;
    public float arrowPulseSpeed = 4f;

    [Header("Targets")]
    public RectTransform openRestaurantButton;
    public RectTransform kitchenButton;
    public RectTransform orderTicket;

    [Header("Optional")]
    public GameObject pauseButton;

    private int step = 0;
    private bool tutorialActive = false;
    private Vector3 arrowBaseScale;

    private RectTransform highlightedTarget;
    private Transform highlightedOriginalParent;
    private int highlightedOriginalSiblingIndex = -1;

    void Awake()
    {
        Instance = this;

        if (tutorialArrow != null)
            arrowBaseScale = tutorialArrow.localScale;
    }

    void Start()
    {
        DisableTutorialRaycasts();

        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            Time.timeScale = 1f;
            HideTutorial();
            return;
        }

        StartTutorial();
    }

    void Update()
    {
        PulseArrow();
    }

    void DisableTutorialRaycasts()
    {
        if (tutorialPanel != null)
        {
            Graphic[] graphics =
                tutorialPanel.GetComponentsInChildren<Graphic>(true);

            for (int i = 0; i < graphics.Length; i++)
                graphics[i].raycastTarget = false;
        }

        if (nextButton != null)
        {
            Image img = nextButton.GetComponent<Image>();

            if (img != null)
                img.raycastTarget = true;

            TMP_Text buttonText =
                nextButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.raycastTarget = false;
        }

        if (tutorialArrow != null)
        {
            Image img = tutorialArrow.GetComponent<Image>();

            if (img != null)
                img.raycastTarget = false;
        }
    }

    void StartTutorial()
    {
        Time.timeScale = 1f;

        tutorialActive = true;
        step = 0;

        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.SetAsLastSibling();
        }

        SetText("Benvenuto in Cat's Pizzeria To Go!\nPremi Avanti per iniziare il tutorial.");
        ShowNextButton(true);
        HideArrow();
    }

    public bool IsTutorialActive()
    {
        return tutorialActive;
    }

    public bool ShouldUseTutorialOrder()
    {
        return tutorialActive;
    }

    public bool ShouldDelayRestaurantOpening()
    {
        return tutorialActive && step == 1;
    }

    public void NextStep()
    {
        if (!tutorialActive)
            return;

        if (step == 0)
        {
            step = 1;

            Time.timeScale = 1f;

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
                tutorialPanel.transform.SetAsLastSibling();
            }

            SetText("Premi il tasto Apri Pizzeria per iniziare la giornata.");
            ShowNextButton(false);

            PointArrowTo(
                openRestaurantButton,
                new Vector2(-160f, 0f),
                0f
            );

            return;
        }

        if (step == 2)
        {
            Time.timeScale = 1f;

            HideTutorial();

            RestaurantManager restaurantManager =
                FindAnyObjectByType<RestaurantManager>();

            if (restaurantManager != null)
                restaurantManager.StartRestaurantAfterTutorialWait();

            return;
        }

        if (step == 3)
        {
            step = 4;

            Time.timeScale = 1f;

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
                tutorialPanel.transform.SetAsLastSibling();
            }

            SetText("Ora premi Cucina per preparare la pizza.");
            ShowNextButton(false);

            PointArrowTo(
                kitchenButton,
                new Vector2(-160f, 0f),
                0f
            );

            return;
        }

        if (step == 8)
        {
            CompleteTutorial();
            return;
        }
    }

    public void OnOpenRestaurantPressed()
    {
        if (!tutorialActive || step != 1)
            return;

        RestoreHighlightedTarget();

        step = 2;

        Time.timeScale = 1f;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.SetAsLastSibling();
        }

        SetText("Perfetto! Ora attendi l'arrivo del primo cliente.\nQuando arriva, toccalo per prendere l'ordine.");
        ShowNextButton(true);
        HideArrow();
    }

    public void OnCustomerOrderTaken()
    {
        if (!tutorialActive || step != 2)
            return;

        step = 3;

        Time.timeScale = 0f;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.SetAsLastSibling();
        }

        SetText(
            "Qui hai lo scontrino.\n" +
            "Ci sono gli ingredienti e cottura richiesti.\n" +
            "Nel tutorial preparerai una pizza con Sugo e Mozzarella con Cottura media."
        );

        ShowNextButton(true);
        HideArrow();
    }

    public void OnKitchenOpened()
    {
        if (!tutorialActive || step != 4)
            return;

        RestoreHighlightedTarget();

        Time.timeScale = 1f;
        HideTutorial();
    }

    public void OnDoughCompleted()
    {
        if (!tutorialActive)
            return;

        if (KitchenTutorialManager.Instance != null)
            KitchenTutorialManager.Instance.OnDoughCompleted();
    }

    public void OnIngredientsCompleted()
    {
        if (!tutorialActive)
            return;

        if (KitchenTutorialManager.Instance != null)
            KitchenTutorialManager.Instance.OnIngredientsCompleted();
    }

    public void OnOvenCompleted()
    {
        if (!tutorialActive)
            return;

        if (KitchenTutorialManager.Instance != null)
            KitchenTutorialManager.Instance.OnOvenCompleted();
    }

    public void OnPizzaDelivered()
    {
        if (!tutorialActive)
            return;

        RestoreHighlightedTarget();

        step = 8;

        Time.timeScale = 0f;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.SetAsLastSibling();
        }

        SetText("Ottimo lavoro! Hai completato il tutorial.\nPremi Avanti per continuare a giocare normalmente.");
        ShowNextButton(true);
        HideArrow();
    }

    public void CompleteTutorial()
    {
        Time.timeScale = 1f;

        RestoreHighlightedTarget();

        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        tutorialActive = false;

        HideTutorial();

        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        HideArrow();
    }

    void SetText(string message)
    {
        if (tutorialText != null)
            tutorialText.text = message;
    }

    void ShowNextButton(bool show)
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(show);
    }

    void PointArrowTo(RectTransform target, Vector2 offset, float rotationZ)
    {
        if (tutorialArrow == null || target == null || canvas == null)
        {
            HideArrow();
            return;
        }

        RestoreHighlightedTarget();

        highlightedTarget = target;
        highlightedOriginalParent = target.parent;
        highlightedOriginalSiblingIndex = target.GetSiblingIndex();

        target.SetParent(canvas.transform, true);
        target.SetAsLastSibling();

        tutorialArrow.gameObject.SetActive(true);
        tutorialArrow.SetParent(target, false);

        tutorialArrow.anchorMin = new Vector2(0.5f, 0.5f);
        tutorialArrow.anchorMax = new Vector2(0.5f, 0.5f);
        tutorialArrow.pivot = new Vector2(0.5f, 0.5f);

        tutorialArrow.anchoredPosition = offset;
        tutorialArrow.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        tutorialArrow.localScale = arrowBaseScale;
    }

    void RestoreHighlightedTarget()
    {
        if (highlightedTarget == null)
            return;

        if (tutorialArrow != null)
        {
            tutorialArrow.gameObject.SetActive(false);

            if (canvas != null)
                tutorialArrow.SetParent(canvas.transform, false);
        }

        if (highlightedOriginalParent != null)
        {
            highlightedTarget.SetParent(highlightedOriginalParent, true);

            if (highlightedOriginalSiblingIndex >= 0)
            {
                int safeIndex =
                    Mathf.Clamp(
                        highlightedOriginalSiblingIndex,
                        0,
                        highlightedOriginalParent.childCount - 1
                    );

                highlightedTarget.SetSiblingIndex(safeIndex);
            }
        }

        highlightedTarget = null;
        highlightedOriginalParent = null;
        highlightedOriginalSiblingIndex = -1;
    }

    void HideArrow()
    {
        if (tutorialArrow == null)
            return;

        tutorialArrow.gameObject.SetActive(false);

        if (canvas != null)
            tutorialArrow.SetParent(canvas.transform, false);
    }

    void PulseArrow()
    {
        if (tutorialArrow == null)
            return;

        if (!tutorialArrow.gameObject.activeSelf)
            return;

        float pulse =
            1f + Mathf.Sin(Time.unscaledTime * arrowPulseSpeed)
            * (arrowPulseScale - 1f);

        tutorialArrow.localScale = arrowBaseScale * pulse;
    }

    public void ResetTutorialForTesting()
    {
        Time.timeScale = 1f;

        RestoreHighlightedTarget();

        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
    }
}