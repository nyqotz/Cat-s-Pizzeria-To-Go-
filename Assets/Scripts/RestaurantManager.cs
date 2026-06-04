using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestaurantManager : MonoBehaviour
{
    public TMP_Text buttonText;
    public TMP_Text clockText;
    public TMP_Text dayText;

    public GameObject openRestaurantButton;

    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public CustomerSlot[] customerSlots;

    public GameObject orderBubble;
    public Image ingredientImage;

    public Sprite sugoSprite;
    public Sprite mozzarellaSprite;
    public Sprite tonnoSprite;
    public Sprite cipollaSprite;

    public GameObject orderTicket;

    public TMP_Text ticketTitle;
    public TMP_Text ingredientsText;
    public TMP_Text bakeText;

    public RectTransform readyPizzaContainer;
    public Vector2 readyPizzaSize = new Vector2(180f, 180f);

    public StarReputationUI starReputationUI;
    public float startingStars = 3f;
    public float correctPizzaStars = 0.1f;
    public float wrongPizzaStars = -0.2f;
    public float timeoutPenaltyStars = -0.3f;

    public float customerPatienceTime = 40f;

    public ResultPanelManager resultPanelManager;

    public AudioClip doorBellClip;
    public AudioClip dayEndClip;
    public float doorBellVolume = 0.3f;
    public float dayEndVolume = 0.5f;

    public float minSpawnTime = 2f;
    public float maxSpawnTime = 6f;
    public float gameplayDuration = 180f;

    public float orderIngredientDelay = 0.75f;
    public float lastIngredientHoldTime = 0.75f;

    private bool isOpen = false;
    private bool isShowingResult = false;
    private bool dayEndedWaitingForNextDay = false;

    private Coroutine spawnRoutine;
    private Coroutine clockRoutine;
    private Coroutine orderRevealRoutine;
    private Coroutine patienceRoutine;

    private List<CustomerMover> activeCustomers =
        new List<CustomerMover>();

    private int lastCustomerIndex = -1;
    private PizzaOrder currentOrder;

    private int orderNumber = 0;
    private int currentDay = 1;
    private float starRating = 3f;

    private GameObject readyPizzaClone;

    void Start()
    {
        ResetRestaurant();

        if (orderTicket != null)
            orderTicket.SetActive(false);

        if (orderBubble != null)
            orderBubble.SetActive(false);

        HideBubbleIngredient();

        ClearReadyPizzaVisual();

        starRating = startingStars;

        if (starReputationUI != null)
            starReputationUI.SetRating(starRating, false);

        UpdateDayText();
    }

    public void OpenRestaurant()
    {
        if (isOpen)
            return;

        if (dayEndedWaitingForNextDay && activeCustomers.Count > 0)
            return;

        if (dayEndedWaitingForNextDay)
        {
            currentDay++;
            dayEndedWaitingForNextDay = false;
            UpdateDayText();
        }

        isOpen = true;
        orderNumber = 0;
        lastCustomerIndex = -1;

        if (clockText != null)
            clockText.text = "12:00";

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (openRestaurantButton != null)
            openRestaurantButton.SetActive(false);

        spawnRoutine = StartCoroutine(SpawnCustomersRandomly());
        clockRoutine = StartCoroutine(RunClock());
    }

    IEnumerator RunClock()
    {
        float elapsed = 0f;

        while (elapsed < gameplayDuration)
        {
            elapsed += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(elapsed / gameplayDuration);

            int totalMinutes =
                Mathf.RoundToInt(
                    Mathf.Lerp(12 * 60, 15 * 60, normalized)
                );

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            if (clockText != null)
            {
                clockText.text =
                    hours.ToString("00") +
                    ":" +
                    minutes.ToString("00");
            }

            yield return null;
        }

        CloseRestaurantAutomatically();
    }

    IEnumerator SpawnCustomersRandomly()
    {
        while (isOpen)
        {
            float waitTime =
                Random.Range(minSpawnTime, maxSpawnTime);

            yield return new WaitForSeconds(waitTime);

            TrySpawnCustomer();
        }
    }

    void TrySpawnCustomer()
    {
        if (!isOpen)
            return;

        if (customerPrefabs == null || customerPrefabs.Length == 0)
            return;

        if (customerSlots == null || customerSlots.Length == 0)
            return;

        if (activeCustomers.Count >= customerSlots.Length)
            return;

        int randomIndex = GetRandomCustomerIndex();

        GameObject customerObject =
            Instantiate(
                customerPrefabs[randomIndex],
                spawnPoint.position,
                Quaternion.identity
            );

        CustomerMover customer =
            customerObject.GetComponent<CustomerMover>();

        if (customer == null)
        {
            Destroy(customerObject);
            return;
        }

        customer.orderBubble = orderBubble;

        activeCustomers.Add(customer);

        PlaySFX(doorBellClip, doorBellVolume);

        UpdateCustomerSlots();
    }

    int GetRandomCustomerIndex()
    {
        if (customerPrefabs.Length == 1)
            return 0;

        int randomIndex;

        do
        {
            randomIndex =
                Random.Range(0, customerPrefabs.Length);
        }
        while (randomIndex == lastCustomerIndex);

        lastCustomerIndex = randomIndex;

        return randomIndex;
    }

    void UpdateCustomerSlots()
    {
        for (int i = 0; i < activeCustomers.Count; i++)
            activeCustomers[i].SetSlot(customerSlots[i]);
    }

    public bool CanCustomerOrder(CustomerMover customer)
    {
        if (customer == null)
            return false;

        if (isShowingResult)
            return false;

        if (currentOrder != null)
            return false;

        if (activeCustomers.Count == 0)
            return false;

        return activeCustomers[0] == customer;
    }

    public bool GenerateRandomOrder(CustomerMover customer)
    {
        if (!CanCustomerOrder(customer))
            return false;

        GenerateRandomOrderInternal();
        return true;
    }

    public void GenerateRandomOrder()
    {
        if (activeCustomers.Count == 0)
            return;

        GenerateRandomOrder(activeCustomers[0]);
    }

    void GenerateRandomOrderInternal()
    {
        if (orderRevealRoutine != null)
        {
            StopCoroutine(orderRevealRoutine);
            orderRevealRoutine = null;
        }

        StopPatienceTimer();

        orderNumber++;

        currentOrder = new PizzaOrder();

        currentOrder.sugoPomodoro = Random.value > 0.5f;
        currentOrder.mozzarella = Random.value > 0.5f;
        currentOrder.tonno = Random.value > 0.5f;
        currentOrder.cipolla = Random.value > 0.5f;
        currentOrder.mediumBake = Random.value > 0.5f;

        if (ticketTitle != null)
            ticketTitle.text = "ORDINE #" + orderNumber;

        if (ingredientsText != null)
            ingredientsText.text = "";

        if (bakeText != null)
        {
            bakeText.text =
                currentOrder.mediumBake
                ? "Cottura: Media"
                : "Cottura: Ben cotta";
        }

        if (orderTicket != null)
            orderTicket.SetActive(true);

        if (orderBubble != null)
            orderBubble.SetActive(true);

        HideBubbleIngredient();

        orderRevealRoutine =
            StartCoroutine(RevealOrderIngredients());

        StartPatienceTimer();
    }

    IEnumerator RevealOrderIngredients()
    {
        string ticketIngredients = "";

        List<OrderIngredientVisual> ingredients =
            GetRequestedIngredientVisuals();

        if (ingredients.Count == 0)
        {
            yield return new WaitForSeconds(orderIngredientDelay);

            ticketIngredients = "• Margherita semplice\n";

            if (ingredientsText != null)
                ingredientsText.text = ticketIngredients;

            HideBubbleIngredient();

            if (orderBubble != null)
                orderBubble.SetActive(false);

            yield break;
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            ShowBubbleIngredient(ingredients[i].sprite);

            ticketIngredients +=
                "• " + ingredients[i].displayName + "\n";

            if (ingredientsText != null)
                ingredientsText.text = ticketIngredients;

            yield return new WaitForSeconds(orderIngredientDelay);
        }

        yield return new WaitForSeconds(lastIngredientHoldTime);

        HideBubbleIngredient();

        if (orderBubble != null)
            orderBubble.SetActive(false);
    }

    List<OrderIngredientVisual> GetRequestedIngredientVisuals()
    {
        List<OrderIngredientVisual> result =
            new List<OrderIngredientVisual>();

        if (currentOrder == null)
            return result;

        if (currentOrder.sugoPomodoro)
            result.Add(new OrderIngredientVisual("Sugo di pomodoro", sugoSprite));

        if (currentOrder.mozzarella)
            result.Add(new OrderIngredientVisual("Mozzarella", mozzarellaSprite));

        if (currentOrder.tonno)
            result.Add(new OrderIngredientVisual("Tonno", tonnoSprite));

        if (currentOrder.cipolla)
            result.Add(new OrderIngredientVisual("Cipolla", cipollaSprite));

        return result;
    }

    void ShowBubbleIngredient(Sprite sprite)
    {
        if (ingredientImage == null)
            return;

        ingredientImage.sprite = sprite;
        ingredientImage.preserveAspect = true;
        ingredientImage.gameObject.SetActive(sprite != null);
    }

    void HideBubbleIngredient()
    {
        if (ingredientImage == null)
            return;

        ingredientImage.sprite = null;
        ingredientImage.gameObject.SetActive(false);
    }

    void StartPatienceTimer()
    {
        StopPatienceTimer();

        patienceRoutine =
            StartCoroutine(CustomerPatienceRoutine());
    }

    void StopPatienceTimer()
    {
        if (patienceRoutine != null)
        {
            StopCoroutine(patienceRoutine);
            patienceRoutine = null;
        }
    }

    IEnumerator CustomerPatienceRoutine()
    {
        yield return new WaitForSeconds(customerPatienceTime);

        if (isShowingResult)
            yield break;

        CustomerTimedOut();
    }

    void CustomerTimedOut()
    {
        if (activeCustomers.Count == 0)
            return;

        CustomerMover customer = activeCustomers[0];

        ChangeStars(timeoutPenaltyStars);

        ClearReadyPizzaVisual();

        if (orderBubble != null)
            orderBubble.SetActive(false);

        HideBubbleIngredient();

        if (orderTicket != null)
            orderTicket.SetActive(false);

        if (orderRevealRoutine != null)
        {
            StopCoroutine(orderRevealRoutine);
            orderRevealRoutine = null;
        }

        PizzaRuntimeData.ResetPizza();
        currentOrder = null;

        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
            customer.LeaveRestaurant(spawnPoint);
            UpdateCustomerSlots();
        }

        TryShowOpenButtonAfterDayEnd();
    }

    public void ReceiveReadyPizzaVisual(GameObject pizzaVisual)
    {
        if (pizzaVisual == null || readyPizzaContainer == null)
            return;

        ClearReadyPizzaVisual();

        readyPizzaClone =
            Instantiate(
                pizzaVisual,
                readyPizzaContainer,
                false
            );

        readyPizzaClone.name = "ReadyPizzaOnCounter";

        RectTransform pizzaRect =
            readyPizzaClone.GetComponent<RectTransform>();

        if (pizzaRect != null)
        {
            pizzaRect.anchorMin = new Vector2(0.5f, 0.5f);
            pizzaRect.anchorMax = new Vector2(0.5f, 0.5f);
            pizzaRect.pivot = new Vector2(0.5f, 0.5f);

            Vector2 originalSize = pizzaRect.rect.size;

            if (originalSize.x <= 0f || originalSize.y <= 0f)
                originalSize = pizzaRect.sizeDelta;

            if (originalSize.x <= 0f || originalSize.y <= 0f)
                originalSize = new Vector2(300f, 300f);

            pizzaRect.anchoredPosition = Vector2.zero;
            pizzaRect.sizeDelta = originalSize;

            float scaleX = readyPizzaSize.x / originalSize.x;
            float scaleY = readyPizzaSize.y / originalSize.y;

            pizzaRect.localScale =
                new Vector3(scaleX, scaleY, 1f);
        }

        DisableRaycasts(readyPizzaClone);

        readyPizzaContainer.gameObject.SetActive(true);

        ReadyPizzaDragger dragger =
            readyPizzaContainer.GetComponent<ReadyPizzaDragger>();

        if (dragger != null)
            dragger.ResetDrag();
    }

    public void DeliverReadyPizzaToCustomer()
    {
        if (isShowingResult)
            return;

        if (activeCustomers.Count == 0)
            return;

        if (!PizzaRuntimeData.pizzaReady)
            return;

        CustomerMover customer = activeCustomers[0];

        bool correct = IsPizzaCorrect();

        Sprite customerSprite = GetCustomerSprite(customer);

        GameObject pizzaVisualForResult = readyPizzaClone;

        isShowingResult = true;

        StopPatienceTimer();

        if (resultPanelManager != null)
        {
            resultPanelManager.PlayResult(
                correct,
                customerSprite,
                pizzaVisualForResult,
                () =>
                {
                    PrepareSceneAfterResult(correct);
                },
                () =>
                {
                    FinishCustomerExit(customer);
                }
            );
        }
        else
        {
            PrepareSceneAfterResult(correct);
            FinishCustomerExit(customer);
        }
    }

    void PrepareSceneAfterResult(bool correct)
    {
        if (correct)
            ChangeStars(correctPizzaStars);
        else
            ChangeStars(wrongPizzaStars);

        ClearReadyPizzaVisual();

        if (orderBubble != null)
            orderBubble.SetActive(false);

        HideBubbleIngredient();

        if (orderTicket != null)
            orderTicket.SetActive(false);

        if (orderRevealRoutine != null)
        {
            StopCoroutine(orderRevealRoutine);
            orderRevealRoutine = null;
        }

        PizzaRuntimeData.ResetPizza();
        currentOrder = null;
    }

    void FinishCustomerExit(CustomerMover customer)
    {
        if (customer != null &&
            activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
            customer.LeaveRestaurant(spawnPoint);
            UpdateCustomerSlots();
        }

        isShowingResult = false;

        TryShowOpenButtonAfterDayEnd();
    }

    Sprite GetCustomerSprite(CustomerMover customer)
    {
        if (customer == null)
            return null;

        SpriteRenderer spriteRenderer =
            customer.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            return null;

        return spriteRenderer.sprite;
    }

    void ClearReadyPizzaVisual()
    {
        if (readyPizzaClone != null)
        {
            Destroy(readyPizzaClone);
            readyPizzaClone = null;
        }

        if (readyPizzaContainer != null)
            readyPizzaContainer.gameObject.SetActive(false);
    }

    void DisableRaycasts(GameObject target)
    {
        Graphic[] graphics =
            target.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }

    bool IsPizzaCorrect()
    {
        if (currentOrder == null)
            return false;

        if (!PizzaRuntimeData.pizzaReady)
            return false;

        if (currentOrder.sugoPomodoro != PizzaRuntimeData.hasSugo)
            return false;

        if (currentOrder.mozzarella != PizzaRuntimeData.hasMozzarella)
            return false;

        if (currentOrder.tonno != PizzaRuntimeData.hasTonno)
            return false;

        if (currentOrder.cipolla != PizzaRuntimeData.hasCipolla)
            return false;

        bool bakeCorrect =
            currentOrder.mediumBake
            ? PizzaRuntimeData.bakeState == "Cottura media"
            : PizzaRuntimeData.bakeState == "Ben cotta";

        return bakeCorrect;
    }

    public void DebugServeFirstCustomer()
    {
        DeliverReadyPizzaToCustomer();
    }

    public void RemoveCustomer(CustomerMover customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);

            if (orderBubble != null)
                orderBubble.SetActive(false);

            HideBubbleIngredient();

            if (orderTicket != null)
                orderTicket.SetActive(false);

            if (orderRevealRoutine != null)
            {
                StopCoroutine(orderRevealRoutine);
                orderRevealRoutine = null;
            }

            StopPatienceTimer();

            customer.LeaveRestaurant(spawnPoint);

            UpdateCustomerSlots();

            TryShowOpenButtonAfterDayEnd();
        }
    }

    void CloseRestaurantAutomatically()
    {
        isOpen = false;
        dayEndedWaitingForNextDay = true;

        PlaySFX(dayEndClip, dayEndVolume);

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        if (clockRoutine != null)
        {
            StopCoroutine(clockRoutine);
            clockRoutine = null;
        }

        StopPatienceTimer();

        for (int i = activeCustomers.Count - 1; i >= 1; i--)
        {
            CustomerMover customer = activeCustomers[i];

            activeCustomers.RemoveAt(i);

            if (customer != null)
                customer.LeaveRestaurant(spawnPoint);
        }

        UpdateCustomerSlots();

        if (activeCustomers.Count == 0)
        {
            TryShowOpenButtonAfterDayEnd();
        }
        else
        {
            if (openRestaurantButton != null)
                openRestaurantButton.SetActive(false);

            if (buttonText != null)
                buttonText.text = "Completa ultimo ordine";
        }
    }

    void TryShowOpenButtonAfterDayEnd()
    {
        if (!dayEndedWaitingForNextDay)
            return;

        if (activeCustomers.Count > 0)
            return;

        if (openRestaurantButton != null)
            openRestaurantButton.SetActive(true);

        if (buttonText != null)
            buttonText.text = "Apri pizzeria";
    }

    void ChangeStars(float amount)
    {
        starRating += amount;
        starRating = Mathf.Clamp(starRating, 0f, 5f);

        if (starReputationUI != null)
            starReputationUI.SetRating(starRating, true);
    }

    void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = "GIORNO " + currentDay;
    }

    void ResetRestaurant()
    {
        if (clockText != null)
            clockText.text = "12:00";

        orderNumber = 0;

        if (openRestaurantButton != null)
            openRestaurantButton.SetActive(true);

        if (buttonText != null)
            buttonText.text = "Apri pizzeria";
    }

    void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        if (AudioManager.Instance == null)
            return;

        if (AudioManager.Instance.sfxSource == null)
            return;

        AudioManager.Instance.sfxSource.PlayOneShot(
            clip,
            volume
        );
    }

    private class OrderIngredientVisual
    {
        public string displayName;
        public Sprite sprite;

        public OrderIngredientVisual(
            string displayName,
            Sprite sprite
        )
        {
            this.displayName = displayName;
            this.sprite = sprite;
        }
    }
}