using UnityEngine;
using UnityEngine.EventSystems;

public class PreparedPizzaDragger : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public OvenManager ovenManager;
    public RectTransform ovenDropArea;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 startAnchoredPosition;
    private bool canDrag = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (rectTransform != null)
            startAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canDrag || rectTransform == null)
            return;

        if (ovenManager != null)
            ovenManager.BuildPreparedPizzaPreview();

        startAnchoredPosition = rectTransform.anchoredPosition;
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

        if (IsOverOven(eventData))
        {
            if (ovenManager != null)
                ovenManager.InsertPizza();

            DisableDrag();
        }
        else
        {
            rectTransform.anchoredPosition = startAnchoredPosition;
        }
    }

    public void ResetDrag()
    {
        canDrag = true;
        gameObject.SetActive(true);

        if (rectTransform != null)
            rectTransform.anchoredPosition = startAnchoredPosition;
    }

    public void DisableDrag()
    {
        canDrag = false;
        gameObject.SetActive(false);
    }

    bool IsOverOven(PointerEventData eventData)
    {
        if (ovenDropArea == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            ovenDropArea,
            eventData.position,
            eventData.pressEventCamera
        );
    }
}