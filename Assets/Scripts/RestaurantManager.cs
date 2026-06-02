using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RestaurantManager : MonoBehaviour
{
    public TMP_Text buttonText;
    public TMP_Text clockText;

    public GameObject openRestaurantButton;

    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public CustomerSlot[] customerSlots;

    public GameObject orderBubble;

    public GameObject orderTicket;

    public TMP_Text ticketTitle;
    public TMP_Text ingredientsText;
    public TMP_Text bakeText;

    public float minSpawnTime = 2f;
    public float maxSpawnTime = 6f;

    public float gameplayDuration = 180f;

    private bool isOpen = false;

    private Coroutine spawnRoutine;
    private Coroutine clockRoutine;

    private List<CustomerMover> activeCustomers =
        new List<CustomerMover>();

    private int lastCustomerIndex = -1;

    private PizzaOrder currentOrder;

    private int orderNumber = 0;

    void Start()
    {
        ResetRestaurant();

        if (orderTicket != null)
            orderTicket.SetActive(false);
    }

    public void OpenRestaurant()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        openRestaurantButton.SetActive(false);

        spawnRoutine =
            StartCoroutine(
                SpawnCustomersRandomly()
            );

        clockRoutine =
            StartCoroutine(
                RunClock()
            );
    }

    IEnumerator RunClock()
    {
        float elapsed = 0f;

        while (elapsed < gameplayDuration)
        {
            elapsed += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed /
                    gameplayDuration
                );

            int totalMinutes =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        12 * 60,
                        15 * 60,
                        normalized
                    )
                );

            int hours =
                totalMinutes / 60;

            int minutes =
                totalMinutes % 60;

            if (clockText != null)
            {
                clockText.text =
                    hours.ToString("00")
                    + ":"
                    + minutes.ToString("00");
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
                Random.Range(
                    minSpawnTime,
                    maxSpawnTime
                );

            yield return
                new WaitForSeconds(
                    waitTime
                );

            TrySpawnCustomer();
        }
    }

    void TrySpawnCustomer()
    {
        if (!isOpen)
            return;

        if (customerPrefabs == null
            || customerPrefabs.Length == 0)
            return;

        if (customerSlots == null
            || customerSlots.Length == 0)
            return;

        if (activeCustomers.Count
            >= customerSlots.Length)
            return;

        int randomIndex =
            GetRandomCustomerIndex();

        GameObject customerObject =
            Instantiate(
                customerPrefabs[randomIndex],
                spawnPoint.position,
                Quaternion.identity
            );

        CustomerMover customer =
            customerObject
            .GetComponent<CustomerMover>();

        if (customer == null)
        {
            Destroy(customerObject);
            return;
        }

        customer.orderBubble =
            orderBubble;

        activeCustomers.Add(customer);

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
                Random.Range(
                    0,
                    customerPrefabs.Length
                );
        }
        while (
            randomIndex
            == lastCustomerIndex
        );

        lastCustomerIndex =
            randomIndex;

        return randomIndex;
    }

    void UpdateCustomerSlots()
    {
        for (
            int i = 0;
            i < activeCustomers.Count;
            i++
        )
        {
            activeCustomers[i]
                .SetSlot(
                    customerSlots[i]
                );
        }
    }

    public void GenerateRandomOrder()
    {
        orderNumber++;

        currentOrder =
            new PizzaOrder();

        currentOrder.sugoPomodoro =
            Random.value > 0.5f;

        currentOrder.mozzarella =
            Random.value > 0.5f;

        currentOrder.tonno =
            Random.value > 0.5f;

        currentOrder.cipolla =
            Random.value > 0.5f;

        currentOrder.mediumBake =
            Random.value > 0.5f;

        string ingredients = "";

        if (currentOrder.sugoPomodoro)
        {
            ingredients +=
                "• Sugo di pomodoro\n";
        }

        if (currentOrder.mozzarella)
        {
            ingredients +=
                "• Mozzarella\n";
        }

        if (currentOrder.tonno)
        {
            ingredients +=
                "• Tonno\n";
        }

        if (currentOrder.cipolla)
        {
            ingredients +=
                "• Cipolla\n";
        }

        if (ingredients == "")
        {
            ingredients =
                "• Margherita semplice\n";
        }

        if (ticketTitle != null)
        {
            ticketTitle.text =
                "ORDINE #"
                + orderNumber;
        }

        if (ingredientsText != null)
        {
            ingredientsText.text =
                ingredients;
        }

        if (bakeText != null)
        {
            bakeText.text =
                currentOrder.mediumBake
                ? "Cottura: Media"
                : "Cottura: Ben cotta";
        }

        if (orderTicket != null)
        {
            orderTicket.SetActive(true);
        }
    }

    public void RemoveCustomer(
        CustomerMover customer
    )
    {
        if (
            activeCustomers.Contains(
                customer
            )
        )
        {
            activeCustomers
                .Remove(customer);

            if (orderBubble != null)
            {
                orderBubble
                    .SetActive(false);
            }

            if (orderTicket != null)
            {
                orderTicket
                    .SetActive(false);
            }

            customer
                .LeaveRestaurant(
                    spawnPoint
                );

            UpdateCustomerSlots();
        }
    }

    public void DebugServeFirstCustomer()
    {
        if (
            activeCustomers.Count
            == 0
        )
            return;

        RemoveCustomer(
            activeCustomers[0]
        );
    }

    void CloseRestaurantAutomatically()
    {
        isOpen = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(
                spawnRoutine
            );

            spawnRoutine = null;
        }

        if (clockRoutine != null)
        {
            StopCoroutine(
                clockRoutine
            );

            clockRoutine = null;
        }

        ResetRestaurant();
    }

    void ResetRestaurant()
    {
        if (clockText != null)
        {
            clockText.text = "12:00";
        }

        orderNumber = 0;

        if (
            openRestaurantButton
            != null
        )
        {
            openRestaurantButton
                .SetActive(true);
        }

        if (buttonText != null)
        {
            buttonText.text =
                "Apri pizzeria";
        }
    }
}