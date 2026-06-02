using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OvenManager : MonoBehaviour
{
    public RectTransform sourcePizzaBase;
    public RectTransform ovenPizzaContainer;

    public Slider bakeProgressBar;
    public TMP_Text bakeStateText;

    public float mediumTime = 5f;
    public float wellDoneTime = 10f;
    public float burntTime = 15f;

    public Vector2 ovenPizzaSize = new Vector2(300f, 300f);

    public float pizzaChildrenScaleMultiplier = 2.2f;

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

    private List<Image> clonedPizzaImages =
        new List<Image>();

    void Start()
    {
        ResetOven();
    }

    void Update()
    {
        if (!isBaking)
            return;

        bakeTimer =
            Time.time - bakingStartTime;

        if (bakeProgressBar != null)
        {
            bakeProgressBar.value =
                bakeTimer;
        }

        UpdateBakeState();
    }

    public void InsertPizza()
    {
        if (pizzaInserted)
            return;

        if (sourcePizzaBase == null ||
            ovenPizzaContainer == null)
            return;

        pizzaInserted = true;
        isBaking = true;

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

        ovenPizzaClone.name =
            "PizzaRealeInForno";

        RectTransform cloneRect =
            ovenPizzaClone.GetComponent<RectTransform>();

        cloneRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        cloneRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        cloneRect.pivot =
            new Vector2(0.5f, 0.5f);

        cloneRect.anchoredPosition =
            Vector2.zero;

        cloneRect.sizeDelta =
            ovenPizzaSize;

        cloneRect.localScale =
            Vector3.one;

        ScalePizzaChildren(
            ovenPizzaClone.transform,
            pizzaChildrenScaleMultiplier
        );

        DisableRaycasts(
            ovenPizzaClone
        );

        CachePizzaImages();

        ApplyTint(rawTint);

        if (bakeProgressBar != null)
        {
            bakeProgressBar.value = 0;
            bakeProgressBar.maxValue = burntTime;
        }

        UpdateOvenText();
    }

    void UpdateBakeState()
    {
        if (bakeTimer < mediumTime)
        {
            SetBakeState(
                "Cruda",
                rawTint
            );
        }
        else if (bakeTimer < wellDoneTime)
        {
            SetBakeState(
                "Cottura media",
                mediumTint
            );
        }
        else if (bakeTimer < burntTime)
        {
            SetBakeState(
                "Ben cotta",
                wellDoneTint
            );
        }
        else
        {
            SetBakeState(
                "Bruciata",
                burntTint
            );
        }
    }

    void SetBakeState(
        string state,
        Color tint
    )
    {
        currentBakeState = state;

        PizzaRuntimeData.bakeState =
            state;

        ApplyTint(tint);

        UpdateOvenText();
    }

    void ApplyTint(Color tint)
    {
        for (int i = 0; i < clonedPizzaImages.Count; i++)
        {
            if (clonedPizzaImages[i] != null)
            {
                clonedPizzaImages[i].color =
                    tint;
            }
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
        {
            clonedPizzaImages.Add(images[i]);
        }
    }

    void ScalePizzaChildren(
        Transform parent,
        float multiplier
    )
    {
        RectTransform parentRect =
            parent.GetComponent<RectTransform>();

        RectTransform[] children =
            parent.GetComponentsInChildren<RectTransform>(true);

        foreach (RectTransform child in children)
        {
            if (child == parentRect)
                continue;

            child.sizeDelta *= multiplier;
            child.anchoredPosition *= multiplier;
        }
    }

    void DisableRaycasts(GameObject target)
    {
        Graphic[] graphics =
            target.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = false;
        }
    }

    void UpdateOvenText()
    {
        if (bakeStateText == null)
            return;

        string text =
            "Pizza in forno\n\n";

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

        if (
            !PizzaRuntimeData.hasSugo &&
            !PizzaRuntimeData.hasMozzarella &&
            !PizzaRuntimeData.hasTonno &&
            !PizzaRuntimeData.hasCipolla
        )
        {
            text += "• Solo impasto";
        }

        bakeStateText.text = text;
    }

    public void FinishPizza()
    {
        if (!pizzaInserted)
            return;

        if (currentBakeState == "Bruciata")
        {
            if (bakeStateText != null)
            {
                bakeStateText.text =
                    "Pizza bruciata!\nButtala nel cestino.";
            }

            return;
        }

        isBaking = false;

        PizzaRuntimeData.bakeState =
            currentBakeState;

        if (bakeStateText != null)
        {
            bakeStateText.text =
                "Pizza pronta!\n"
                + currentBakeState;
        }

        SceneManager.UnloadSceneAsync(
            "KitchenScene"
        );
    }

    public void TrashPizza()
    {
        PizzaRuntimeData.ResetPizza();

        ResetOven();
    }

    void ResetOven()
    {
        pizzaInserted = false;
        isBaking = false;

        bakeTimer = 0f;
        bakingStartTime = 0f;

        currentBakeState = "Cruda";

        ClearOvenPizza();

        if (bakeProgressBar != null)
        {
            bakeProgressBar.value = 0;
            bakeProgressBar.maxValue = burntTime;
        }

        if (bakeStateText != null)
        {
            bakeStateText.text =
                "Metti la pizza nel forno";
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
}