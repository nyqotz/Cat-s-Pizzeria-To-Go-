using UnityEngine;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{
    public Image pizzaBaseImage;
    public Image heldIngredientImage;

    public RectTransform pizzaDropArea;
    public RectTransform sauceContainer;

    public Sprite mozzarellaSprite;
    public Sprite cipollaSprite;
    public Sprite tonnoSprite;
    public Sprite sugoSprite;

    public GameObject placedMozzarellaPrefab;
    public GameObject placedCipollaPrefab;
    public GameObject placedTonnoPrefab;
    public GameObject placedSugoDotPrefab;

    public Vector2 heldIngredientSize = new Vector2(45f, 45f);
    public Vector2 normalIngredientSize = new Vector2(40f, 40f);
    public Vector2 sauceDotSize = new Vector2(35f, 35f);

    public float sauceDotSpawnInterval = 0.015f;

    private string heldIngredient = "";
    private bool sauceMode = false;
    private float sauceTimer = 0f;
    private bool tutorialIngredientsCompleted = false;

    void Start()
    {
        ClearHeldIngredient();
        RefreshPizzaVisibility();
    }

    void OnEnable()
    {
        RefreshPizzaVisibility();
    }

    void Update()
    {
        if (sauceTimer > 0f)
            sauceTimer -= Time.deltaTime;
    }

    public void RefreshPizzaVisibility()
    {
        bool shouldShowPizza =
            PizzaRuntimeData.doughReady &&
            !PizzaRuntimeData.pizzaInOven;

        if (pizzaBaseImage != null)
        {
            pizzaBaseImage.gameObject.SetActive(shouldShowPizza);
        }

        if (!shouldShowPizza)
        {
            ClearHeldIngredient();
        }
    }

    public void HidePizzaAfterOvenInsert()
    {
        ClearHeldIngredient();

        if (pizzaBaseImage != null)
        {
            pizzaBaseImage.gameObject.SetActive(false);
        }
    }

    public void ResetIngredientsForNewPizza()
    {
        tutorialIngredientsCompleted = false;

        ClearHeldIngredient();
        ClearPlacedIngredients();

        if (pizzaBaseImage != null)
        {
            pizzaBaseImage.gameObject.SetActive(false);
        }
    }

    public void StartHoldingIngredient(string ingredientName, Vector2 screenPosition)
    {
        if (!PizzaRuntimeData.doughReady || PizzaRuntimeData.pizzaInOven)
            return;

        heldIngredient = ingredientName;
        sauceMode = ingredientName == "SugoPomodoro";
        sauceTimer = 0f;

        if (heldIngredientImage == null)
            return;

        RectTransform heldRect =
            heldIngredientImage.GetComponent<RectTransform>();

        heldIngredientImage.sprite = GetSprite(ingredientName);
        heldIngredientImage.raycastTarget = false;
        heldIngredientImage.gameObject.SetActive(true);

        heldRect.sizeDelta = heldIngredientSize;
        heldRect.localScale = Vector3.one;
        heldRect.position = screenPosition;
    }

    public void MoveHeldIngredient(Vector2 screenPosition)
    {
        if (string.IsNullOrEmpty(heldIngredient))
            return;

        MoveHeldVisual(screenPosition);

        if (!sauceMode)
            return;

        if (!IsInsidePizzaDropArea(screenPosition))
        {
            HideHeldIngredientVisual();
            return;
        }

        ShowHeldIngredientVisual();

        if (sauceTimer <= 0f)
        {
            SpawnSauceDot(screenPosition);
            sauceTimer = sauceDotSpawnInterval;
        }
    }

    public void ReleaseHeldIngredient(Vector2 screenPosition)
    {
        if (string.IsNullOrEmpty(heldIngredient))
            return;

        if (!sauceMode)
        {
            if (IsInsidePizzaDropArea(screenPosition))
            {
                PlaceIngredient(heldIngredient, screenPosition);
            }
        }

        ClearHeldIngredient();
    }

    void MoveHeldVisual(Vector2 screenPosition)
    {
        if (heldIngredientImage == null)
            return;

        RectTransform heldRect =
            heldIngredientImage.GetComponent<RectTransform>();

        heldRect.sizeDelta = heldIngredientSize;
        heldRect.localScale = Vector3.one;
        heldRect.position = screenPosition;
    }

    bool IsInsidePizzaDropArea(Vector2 screenPosition)
    {
        if (pizzaDropArea == null)
            return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                pizzaDropArea,
                screenPosition,
                GetUICamera(),
                out Vector2 localPoint))
        {
            return false;
        }

        float radius =
            Mathf.Min(
                pizzaDropArea.rect.width,
                pizzaDropArea.rect.height
            ) * 0.5f;

        return localPoint.magnitude <= radius;
    }

    void SpawnSauceDot(Vector2 screenPosition)
    {
        if (placedSugoDotPrefab == null || sauceContainer == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sauceContainer,
            screenPosition,
            GetUICamera(),
            out Vector2 localPoint
        );

        GameObject dot =
            Instantiate(
                placedSugoDotPrefab,
                sauceContainer,
                false
            );

        RectTransform dotRect =
            dot.GetComponent<RectTransform>();

        dotRect.anchorMin = new Vector2(0.5f, 0.5f);
        dotRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotRect.pivot = new Vector2(0.5f, 0.5f);

        dotRect.anchoredPosition = localPoint;
        dotRect.sizeDelta = sauceDotSize;
        dotRect.localScale = Vector3.one;

        PizzaRuntimeData.hasSugo = true;

        CheckTutorialIngredientsCompleted();
    }

    void PlaceIngredient(string ingredient, Vector2 screenPosition)
    {
        GameObject prefab = GetPrefab(ingredient);

        if (prefab == null || pizzaDropArea == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pizzaDropArea,
            screenPosition,
            GetUICamera(),
            out Vector2 localPoint
        );

        GameObject placed =
            Instantiate(
                prefab,
                pizzaDropArea,
                false
            );

        RectTransform placedRect =
            placed.GetComponent<RectTransform>();

        placedRect.anchorMin = new Vector2(0.5f, 0.5f);
        placedRect.anchorMax = new Vector2(0.5f, 0.5f);
        placedRect.pivot = new Vector2(0.5f, 0.5f);

        placedRect.anchoredPosition = localPoint;
        placedRect.sizeDelta = normalIngredientSize;
        placedRect.localScale = Vector3.one;

        if (ingredient == "Mozzarella")
            PizzaRuntimeData.hasMozzarella = true;

        if (ingredient == "Cipolla")
            PizzaRuntimeData.hasCipolla = true;

        if (ingredient == "Tonno")
            PizzaRuntimeData.hasTonno = true;

        CheckTutorialIngredientsCompleted();
    }

    void CheckTutorialIngredientsCompleted()
    {
        if (tutorialIngredientsCompleted)
            return;

        if (TutorialManager.Instance == null)
            return;

        if (!TutorialManager.Instance.IsTutorialActive())
            return;

        if (PizzaRuntimeData.hasSugo && PizzaRuntimeData.hasMozzarella)
        {
            tutorialIngredientsCompleted = true;
            TutorialManager.Instance.OnIngredientsCompleted();
        }
    }

    void ClearPlacedIngredients()
    {
        if (sauceContainer != null)
        {
            for (int i = sauceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(sauceContainer.GetChild(i).gameObject);
            }
        }

        if (pizzaDropArea != null)
        {
            for (int i = pizzaDropArea.childCount - 1; i >= 0; i--)
            {
                Transform child = pizzaDropArea.GetChild(i);

                if (sauceContainer != null &&
                    child == sauceContainer)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }
    }

    Sprite GetSprite(string ingredient)
    {
        if (ingredient == "Mozzarella")
            return mozzarellaSprite;

        if (ingredient == "Cipolla")
            return cipollaSprite;

        if (ingredient == "Tonno")
            return tonnoSprite;

        if (ingredient == "SugoPomodoro")
            return sugoSprite;

        return null;
    }

    GameObject GetPrefab(string ingredient)
    {
        if (ingredient == "Mozzarella")
            return placedMozzarellaPrefab;

        if (ingredient == "Cipolla")
            return placedCipollaPrefab;

        if (ingredient == "Tonno")
            return placedTonnoPrefab;

        return null;
    }

    Camera GetUICamera()
    {
        Canvas canvas =
            GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    void ShowHeldIngredientVisual()
    {
        if (heldIngredientImage != null &&
            !heldIngredientImage.gameObject.activeSelf)
        {
            heldIngredientImage.gameObject.SetActive(true);
        }
    }

    void HideHeldIngredientVisual()
    {
        if (heldIngredientImage != null &&
            heldIngredientImage.gameObject.activeSelf)
        {
            heldIngredientImage.gameObject.SetActive(false);
        }
    }

    void ClearHeldIngredient()
    {
        heldIngredient = "";
        sauceMode = false;
        sauceTimer = 0f;

        if (heldIngredientImage != null)
        {
            heldIngredientImage.sprite = null;
            heldIngredientImage.gameObject.SetActive(false);
        }
    }
}