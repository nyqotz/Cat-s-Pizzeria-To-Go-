using UnityEngine;

public class CustomerMover : MonoBehaviour
{
    public Transform targetPoint;
    public Transform bubblePoint;

    public float speed = 2f;

    public float walkRotationAmount = 6f;
    public float walkRotationSpeed = 9f;

    public GameObject orderBubble;

    public AudioClip customerAudio;

    public Sprite backSprite;

    private bool isWalking = true;
    private bool isLeaving = false;
    private bool orderTaken = false;
    private bool reachedSlot = false;

    void Update()
    {
        if (targetPoint == null)
            return;

        if (!isWalking && !isLeaving)
            return;

        Vector3 targetPosition = new Vector3(
            targetPoint.position.x,
            targetPoint.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        float angle =
            Mathf.Sin(Time.time * walkRotationSpeed)
            * walkRotationAmount;

        transform.rotation =
            Quaternion.Euler(0, 0, angle);

        if (Vector2.Distance(
            transform.position,
            targetPosition
        ) < 0.05f)
        {
            transform.rotation =
                Quaternion.identity;

            if (isLeaving)
            {
                Destroy(gameObject);
                return;
            }

            isWalking = false;
            reachedSlot = true;
        }
    }

    public void SetSlot(CustomerSlot slot)
    {
        targetPoint = slot.transform;
        bubblePoint = slot.bubblePoint;

        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sortingOrder =
                slot.orderInLayer;
        }

        isWalking = true;
        isLeaving = false;
        reachedSlot = false;
    }

    public void LeaveRestaurant(
        Transform exitPoint
    )
    {
        if (orderBubble != null)
        {
            orderBubble.SetActive(false);
        }

        if (backSprite != null)
        {
            SpriteRenderer sr =
                GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.sprite =
                    backSprite;
            }
        }

        targetPoint = exitPoint;

        isLeaving = true;
        isWalking = true;
    }

    void SetUIPosition(
        GameObject uiObject,
        Transform referencePoint
    )
    {
        if (
            uiObject == null
            || referencePoint == null
        )
            return;

        RectTransform canvasRect =
            uiObject.transform.parent
            .GetComponent<RectTransform>();

        RectTransform uiRect =
            uiObject.GetComponent<RectTransform>();

        Vector2 screenPoint =
            Camera.main
            .WorldToScreenPoint(
                referencePoint.position
            );

        RectTransformUtility
        .ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            Camera.main,
            out Vector2 localPoint
        );

        uiRect.anchoredPosition =
            localPoint;
    }

    void OnMouseDown()
    {
        if (
            isWalking
            || isLeaving
            || orderTaken
            || !reachedSlot
        )
            return;

        RestaurantManager rm =
            FindAnyObjectByType<RestaurantManager>();

        if (rm == null)
            return;

        if (!rm.CanCustomerOrder(this))
            return;

        bool orderAccepted =
            rm.GenerateRandomOrder(this);

        if (!orderAccepted)
            return;

        orderTaken = true;

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnCustomerOrderTaken();

        if (
            AudioManager.Instance != null
            && customerAudio != null
            && AudioManager.Instance.sfxSource != null
        )
        {
            AudioManager.Instance
                .sfxSource
                .PlayOneShot(
                    customerAudio
                );
        }

        SetUIPosition(
            orderBubble,
            bubblePoint
        );

        if (orderBubble != null)
        {
            orderBubble.SetActive(true);
        }
    }
}