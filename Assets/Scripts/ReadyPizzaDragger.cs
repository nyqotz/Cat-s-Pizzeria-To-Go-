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
        if (!canDrag)
            return;

        startPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || canvas == null)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canDrag)
            return;

        if (IsOverCustomer(eventData.position))
        {
            if (restaurantManager != null)
            {
                restaurantManager.DeliverReadyPizzaToCustomer();
            }

            canDrag = false;
            return;
        }

        rectTransform.anchoredPosition = startPosition;
    }

    bool IsOverCustomer(Vector2 screenPosition)
    {
        if (customerDropArea == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            customerDropArea,
            screenPosition,
            GetUICamera()
        );
    }

    Camera GetUICamera()
    {
        if (canvas == null)
            return null;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }
}