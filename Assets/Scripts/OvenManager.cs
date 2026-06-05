using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OvenManager : MonoBehaviour
{
    public RectTransform sourcePizzaBase;
    public RectTransform preparedPizzaDragContainer;
    public RectTransform ovenPizzaContainer;

    public PreparedPizzaDragger preparedPizzaDragger;
    public IngredientManager ingredientManager;
    public DoughManager doughManager;

    public Slider bakeProgressBar;
    public TMP_Text bakeStateText;

    public AudioSource ovenLoopSource;
    public AudioClip ovenLoopClip;
    public AudioClip pizzaReadyClip;
    public AudioClip trashClip;

    public float ovenLoopVolume = 0.07f;
    public float sfxVolumeMultiplier = 2f;

    public float mediumTime = 5f;
    public float wellDoneTime = 10f;
    public float burntTime = 15f;

    public Vector2 preparedPizzaSize = new Vector2(300f, 300f);
    public Vector2 ovenPizzaSize = new Vector2(420f, 420f);

    public Color rawTint = Color.white;
    public Color mediumTint = new Color(1f, 0.9f, 0.65f, 1f);
    public Color wellDoneTint = new Color(0.9f, 0.65f, 0.35f, 1f);
    public Color burntTint = new Color(0.35f, 0.22f, 0.15f, 1f);

    private bool pizzaInserted = false;
    private bool isBaking = false;

    private float bakeTimer = 0f;
    private float bakingStartTime = 0f;

    private string currentBakeState = "Cruda";

    private GameObject ovenPizzaClone;
    private GameObject preparedPizzaClone;

    private List<Image> clonedPizzaImages =
        new List<Image>();

    void Start()
    {
        if (ovenLoopSource != null)
        {
            ovenLoopSource.playOnAwake = false;
            ovenLoopSource.loop = true;
            ovenLoopSource.volume = ovenLoopVolume;
            ovenLoopSource.spatialBlend = 0f;
        }

        ResetOven();
        BuildPreparedPizzaPreview();
    }

    void Update()
    {
        if (!isBaking)
            return;

        bakeTimer = Time.time - bakingStartTime;

        if (bakeProgressBar != null)
            bakeProgressBar.value = bakeTimer;

        UpdateBakeState();
    }

    public void BuildPreparedPizzaPreview()
    {
        if (pizzaInserted)
            return;

        if (!PizzaRuntimeData.doughReady || PizzaRuntimeData.pizzaInOven)
        {
            ClearPreparedPizzaPreview();
            return;
        }

        if (sourcePizzaBase == null || preparedPizzaDragContainer == null)
            return;

        ClearPreparedPizzaPreview();

        preparedPizzaClone =
            Instantiate(
                sourcePizzaBase.gameObject,
                preparedPizzaDragContainer,
                false
            );

        preparedPizzaClone.name = "PizzaRealeDaTrascinare";
        preparedPizzaClone.SetActive(true);

        SetupPizzaClone(
            preparedPizzaClone,
            preparedPizzaSize
        );

        DisableRaycasts(preparedPizzaClone);

        if (preparedPizzaDragger != null)
            preparedPizzaDragger.ResetDrag();
    }

    public void InsertPizza()
    {
        if (pizzaInserted)
            return;

        if (!PizzaRuntimeData.doughReady)
            return;

        if (sourcePizzaBase == null || ovenPizzaContainer == null)
            return;

        pizzaInserted = true;
        isBaking = true;

        PizzaRuntimeData.pizzaInOven = true;
        PizzaRuntimeData.pizzaReady = false;

        bakeTimer = 0f;
        bakingStartTime = Time.time;
        currentBakeState = "Cruda";

        ClearOvenPizza();

        ovenPizzaClone =
            Instantiate(
                sourcePizzaBase.gameObject,
                ovenPizzaContainer,
                false
            );

        ovenPizzaClone.name = "PizzaRealeInForno";
        ovenPizzaClone.SetActive(true);

        SetupPizzaClone(
            ovenPizzaClone,
            ovenPizzaSize
        );

        DisableRaycasts(ovenPizzaClone);
        CachePizzaImages();
        ApplyTint(rawTint);

        ClearPreparedPizzaPreview();

        if (preparedPizzaDragger != null)
            preparedPizzaDragger.DisableDrag();

        if (ingredientManager != null)
            ingredientManager.HidePizzaAfterOvenInsert();

        if (doughManager != null)
            doughManager.ResetDoughForNewPizza();

        if (bakeProgressBar != null)
        {
            bakeProgressBar.value = 0;
            bakeProgressBar.maxValue = burntTime;
        }

        StartOvenLoop();
        UpdateOvenText();
    }

    void SetupPizzaClone(GameObject pizzaClone, Vector2 targetSize)
    {
        RectTransform cloneRect =
            pizzaClone.GetComponent<RectTransform>();

        Vector2 originalSize = sourcePizzaBase.rect.size;

        if (originalSize.x <= 0f || originalSize.y <= 0f)
            originalSize = sourcePizzaBase.sizeDelta;

        if (originalSize.x <= 0f || originalSize.y <= 0f)
            originalSize = new Vector2(300f, 300f);

        cloneRect.anchorMin = new Vector2(0.5f, 0.5f);
        cloneRect.anchorMax = new Vector2(0.5f, 0.5f);
        cloneRect.pivot = new Vector2(0.5f, 0.5f);

        cloneRect.anchoredPosition = Vector2.zero;
        cloneRect.sizeDelta = originalSize;

        float scaleX = targetSize.x / originalSize.x;
        float scaleY = targetSize.y / originalSize.y;

        cloneRect.localScale =
            new Vector3(scaleX, scaleY, 1f);
    }

    void UpdateBakeState()
    {
        if (bakeTimer < mediumTime)
            SetBakeState("Cruda", rawTint);
        else if (bakeTimer < wellDoneTime)
            SetBakeState("Cottura media", mediumTint);
        else if (bakeTimer < burntTime)
            SetBakeState("Ben cotta", wellDoneTint);
        else
            SetBakeState("Bruciata", burntTint);
    }

    void SetBakeState(string state, Color tint)
    {
        currentBakeState = state;
        PizzaRuntimeData.bakeState = state;

        ApplyTint(tint);
        UpdateOvenText();
    }

    void ApplyTint(Color tint)
    {
        for (int i = 0; i < clonedPizzaImages.Count; i++)
        {
            if (clonedPizzaImages[i] != null)
                clonedPizzaImages[i].color = tint;
        }
    }

    void CachePizzaImages()
    {
        clonedPizzaImages.Clear();

        if (ovenPizzaClone == null)
            return;

        Image[] images =
            ovenPizzaClone.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
            clonedPizzaImages.Add(images[i]);
    }

    void DisableRaycasts(GameObject target)
    {
        Graphic[] graphics =
            target.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }

    void UpdateOvenText()
    {
        if (bakeStateText == null)
            return;

        string text = "Pizza in forno\n\n";

        text +=
            "Cottura: "
            + currentBakeState
            + "\n\n";

        if (PizzaRuntimeData.hasSugo)
            text += "• Sugo\n";

        if (PizzaRuntimeData.hasMozzarella)
            text += "• Mozzarella\n";

        if (PizzaRuntimeData.hasTonno)
            text += "• Tonno\n";

        if (PizzaRuntimeData.hasCipolla)
            text += "• Cipolla\n";

        if (!PizzaRuntimeData.hasSugo &&
            !PizzaRuntimeData.hasMozzarella &&
            !PizzaRuntimeData.hasTonno &&
            !PizzaRuntimeData.hasCipolla)
        {
            text += "• Solo impasto";
        }

        bakeStateText.text = text;
    }

    public void FinishPizza()
    {
        if (!pizzaInserted)
            return;

        StopOvenLoop();
        PlaySFX(pizzaReadyClip);

        if (currentBakeState == "Bruciata")
        {
            if (bakeStateText != null)
                bakeStateText.text =
                    "Pizza bruciata!\nButtala nel cestino.";

            return;
        }

        isBaking = false;

        PizzaRuntimeData.bakeState = currentBakeState;
        PizzaRuntimeData.pizzaReady = true;
        PizzaRuntimeData.pizzaInOven = false;

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnOvenCompleted();

        RestaurantManager restaurantManager =
            FindAnyObjectByType<RestaurantManager>();

        if (restaurantManager != null && ovenPizzaClone != null)
        {
            restaurantManager.ReceiveReadyPizzaVisual(
                ovenPizzaClone
            );
        }

        if (bakeStateText != null)
        {
            bakeStateText.text =
                "Pizza pronta!\n"
                + currentBakeState;
        }

        SceneManager.UnloadSceneAsync("KitchenScene");
    }

    public void TrashPizza()
    {
        StopOvenLoop();
        PlaySFX(trashClip);

        PizzaRuntimeData.ResetPizza();

        ResetOven();

        if (ingredientManager != null)
            ingredientManager.ResetIngredientsForNewPizza();

        if (doughManager != null)
            doughManager.ResetDoughForNewPizza();

        BuildPreparedPizzaPreview();
    }

    void ResetOven()
    {
        pizzaInserted = false;
        isBaking = false;

        bakeTimer = 0f;
        bakingStartTime = 0f;

        currentBakeState = "Cruda";

        StopOvenLoop();

        ClearOvenPizza();
        ClearPreparedPizzaPreview();

        if (bakeProgressBar != null)
        {
            bakeProgressBar.value = 0;
            bakeProgressBar.maxValue = burntTime;
        }

        if (bakeStateText != null)
            bakeStateText.text = "Metti la pizza nel forno";
    }

    void StartOvenLoop()
    {
        if (ovenLoopSource == null || ovenLoopClip == null)
            return;

        ovenLoopSource.clip = ovenLoopClip;
        ovenLoopSource.loop = true;
        ovenLoopSource.volume = ovenLoopVolume;
        ovenLoopSource.Play();
    }

    void StopOvenLoop()
    {
        if (ovenLoopSource != null)
            ovenLoopSource.Stop();
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        if (AudioManager.Instance != null &&
            AudioManager.Instance.sfxSource != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(
                clip,
                sfxVolumeMultiplier
            );

            return;
        }

        if (ovenLoopSource != null)
        {
            ovenLoopSource.PlayOneShot(
                clip,
                sfxVolumeMultiplier
            );
        }
    }

    void ClearOvenPizza()
    {
        if (ovenPizzaClone != null)
        {
            Destroy(ovenPizzaClone);
            ovenPizzaClone = null;
        }

        clonedPizzaImages.Clear();
    }

    void ClearPreparedPizzaPreview()
    {
        if (preparedPizzaClone != null)
        {
            Destroy(preparedPizzaClone);
            preparedPizzaClone = null;
        }
    }
}