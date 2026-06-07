using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KitchenTutorialManager : MonoBehaviour
{
    public static KitchenTutorialManager Instance;

    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    public Button nextButton;

    public RectTransform tutorialArrow;
    public float arrowPulseScale = 1.25f;
    public float arrowPulseSpeed = 4f;

    public RectTransform doughTarget;
    public RectTransform nextToIngredientsButton;
    public RectTransform nextToOvenButton;

    private Vector3 arrowBaseScale;

    void Awake()
    {
        Instance = this;

        if (tutorialArrow != null)
            arrowBaseScale = tutorialArrow.localScale;
    }

    void Start()
    {
        if (
            TutorialManager.Instance == null ||
            !TutorialManager.Instance.IsTutorialActive()
        )
        {
            HideTutorial();
            return;
        }

        DisableRaycasts();
        ShowDoughStep();
    }

    void Update()
    {
        PulseArrow();
    }

    void DisableRaycasts()
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
            Image img =
                nextButton.GetComponent<Image>();

            if (img != null)
                img.raycastTarget = true;

            TMP_Text buttonText =
                nextButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.raycastTarget = false;
        }

        if (tutorialArrow != null)
        {
            Image img =
                tutorialArrow.GetComponent<Image>();

            if (img != null)
                img.raycastTarget = false;
        }
    }

   void ShowDoughStep()
{
    ShowText(
        "Clicca sull'impasto e completalo con 10 tap e 2 movimenti circolari."
    );

    ShowNextButton(false);

    PointArrowTo(
        doughTarget,
        new Vector2(-95f, -140f),
        90f
    );
}

    public void OnDoughPanelOpened()
    {
        HideTutorial();
    }

    public void OnDoughCompleted()
    {
        ShowText(
            "Perfetto! Ora passa agli ingredienti."
        );

        ShowNextButton(false);

        PointArrowTo(
            nextToIngredientsButton,
            new Vector2(0f, -120f),
            90f
        );
    }

    public void OnIngredientsPanelOpened()
    {
        HideTutorial();
    }

    public void OnIngredientsCompleted()
    {
        ShowText(
            "Ottimo! Ora passa al forno."
        );

        ShowNextButton(false);

        PointArrowTo(
            nextToOvenButton,
            new Vector2(0f, -120f),
            90f
        );
    }

    public void OnOvenPanelOpened()
    {
        HideTutorial();
    }

    public void OnOvenCompleted()
    {
        ShowText(
            "Pizza pronta! Torna al bancone e trascinala sul cliente."
        );

        ShowNextButton(false);
        HideArrow();
    }

    public void CloseKitchenTutorial()
    {
        HideTutorial();
    }

    public void HideTutorialPanel()
    {
        HideTutorial();
    }

    void ShowText(string message)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.SetAsLastSibling();
        }

        if (tutorialText != null)
            tutorialText.text = message;
    }

    void ShowNextButton(bool show)
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(show);
    }

    void PointArrowTo(
        RectTransform target,
        Vector2 offset,
        float rotationZ
    )
    {
        if (
            tutorialArrow == null ||
            target == null ||
            tutorialPanel == null ||
            tutorialPanel.transform.parent == null
        )
        {
            HideArrow();
            return;
        }

        RectTransform canvasRect =
            tutorialPanel.transform.parent
            .GetComponent<RectTransform>();

        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(
                null,
                target.position
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                null,
                out Vector2 localPoint
            );

        tutorialArrow.gameObject.SetActive(true);
        tutorialArrow.SetParent(
            tutorialPanel.transform.parent,
            false
        );
        tutorialArrow.SetAsLastSibling();

        tutorialArrow.anchorMin =
            new Vector2(0.5f, 0.5f);

        tutorialArrow.anchorMax =
            new Vector2(0.5f, 0.5f);

        tutorialArrow.pivot =
            new Vector2(0.5f, 0.5f);

        tutorialArrow.anchoredPosition =
            localPoint + offset;

        tutorialArrow.localRotation =
            Quaternion.Euler(0f, 0f, rotationZ);

        tutorialArrow.localScale =
            arrowBaseScale;
    }

    void HideArrow()
    {
        if (tutorialArrow != null)
            tutorialArrow.gameObject.SetActive(false);
    }

    void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        HideArrow();
    }

    void PulseArrow()
    {
        if (tutorialArrow == null)
            return;

        if (!tutorialArrow.gameObject.activeSelf)
            return;

        float pulse =
            1f +
            Mathf.Sin(
                Time.unscaledTime * arrowPulseSpeed
            ) *
            (arrowPulseScale - 1f);

        tutorialArrow.localScale =
            arrowBaseScale * pulse;
    }
}