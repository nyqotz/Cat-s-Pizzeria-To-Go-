using UnityEngine;
using UnityEngine.EventSystems;

public class ReadyPizzaDragger : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public RestaurantManager restaurantManager;
    public RectTransform customerDropArea;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 startPosition;
    private bool canDrag = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (rectTransform != null)
            startPosition = rectTransform.anchoredPosition;
    }

    public void ResetDrag()
    {
        canDrag = true;

        if (rectTransform != null)
            rectTransform.anchoredPosition = startPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canDrag || rectTransform == null)
            return;

        startPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || rectTransform == null || canvas == null)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canDrag || rectTransform == null)
            return;

        if (IsOverCustomer(eventData))
        {
            if (restaurantManager != null)
                restaurantManager.DeliverReadyPizzaToCustomer();

            canDrag = false;
            return;
        }

        rectTransform.anchoredPosition = startPosition;
    }

    bool IsOverCustomer(PointerEventData eventData)
    {
        if (customerDropArea == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            customerDropArea,
            eventData.position,
            eventData.pressEventCamera
        );
    }
}