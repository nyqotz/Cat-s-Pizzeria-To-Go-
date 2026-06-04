using UnityEngine;
using TMPro;

public class DaySummaryPanelManager : MonoBehaviour
{
    public TMP_Text servedText;
    public TMP_Text perfectText;
    public TMP_Text wrongText;

    private RestaurantManager restaurantManager;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowSummary(
        int servedCustomers,
        int perfectPizzas,
        int wrongPizzas,
        RestaurantManager manager
    )
    {
        restaurantManager = manager;

        if (servedText != null)
            servedText.text = "Clienti serviti: " + servedCustomers;

        if (perfectText != null)
            perfectText.text = "Pizze perfette: " + perfectPizzas;

        if (wrongText != null)
            wrongText.text = "Pizze sbagliate: " + wrongPizzas;

        gameObject.SetActive(true);

        if (AudioManager.Instance != null &&
            AudioManager.Instance.musicSource != null)
        {
            AudioManager.Instance.musicSource.UnPause();
        }
    }

    public void ConfirmDay()
    {
        gameObject.SetActive(false);

        if (restaurantManager != null)
            restaurantManager.ConfirmDaySummary();
    }
}