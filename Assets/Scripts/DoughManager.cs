using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DoughManager : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public Image doughImage;
    public Sprite rawDoughSprite;
    public Sprite stretchedDoughSprite;

    public Vector2 stretchedDoughSize = new Vector2(700f, 700f);

    public RectTransform leftPaw;
    public RectTransform rightPaw;

    public Image leftPawImage;
    public Image rightPawImage;

    public Sprite leftPawHoldingSprite;
    public Sprite rightPawHoldingSprite;

    public AudioSource doughAudioSource;
    public AudioClip kneadingMusic;
    public float kneadingMusicVolume = 0.10f;

    public float pawPressDistance = 25f;
    public float pawPressDuration = 0.12f;
    public float pawReturnDuration = 0.18f;

    public float pawCircleMoveMultiplier = 0.08f;
    public float pawCircleMaxDistance = 20f;

    private int tapCount = 0;
    private int circleCount = 0;

    private bool doughReady = false;
    private bool kneadingMusicStarted = false;
    private bool useLeftPaw = true;

    private float accumulatedRotation = 0f;
    private Vector2 previousDirection = Vector2.zero;

    private Vector2 leftPawStart;
    private Vector2 rightPawStart;

    private Vector2 doughStartSize;
    private Vector3 doughStartScale;

    private Sprite leftPawStartSprite;
    private Sprite rightPawStartSprite;

    private const int requiredTaps = 10;
    private const int requiredCircles = 4;

    void Start()
    {
        SaveStartValues();
        ResetDoughForNewPizza();
    }

    void SaveStartValues()
    {
        if (doughImage != null)
        {
            doughStartSize = doughImage.rectTransform.sizeDelta;
            doughStartScale = doughImage.rectTransform.localScale;
        }

        if (leftPaw != null)
            leftPawStart = leftPaw.anchoredPosition;

        if (rightPaw != null)
            rightPawStart = rightPaw.anchoredPosition;

        if (leftPawImage != null)
            leftPawStartSprite = leftPawImage.sprite;

        if (rightPawImage != null)
            rightPawStartSprite = rightPawImage.sprite;
    }

    void OnDisable()
    {
        StopKneadingMusic();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (doughReady)
            return;

        if (!kneadingMusicStarted)
        {
            StartKneadingMusic();
            kneadingMusicStarted = true;
        }

        previousDirection = GetDirectionFromDoughCenter(eventData);

        if (tapCount < requiredTaps)
        {
            tapCount++;
            AnimatePawTap();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (doughReady)
            return;

        if (tapCount < requiredTaps)
            return;

        if (!kneadingMusicStarted)
        {
            StartKneadingMusic();
            kneadingMusicStarted = true;
        }

        AnimatePawCircle(eventData.delta);

        Vector2 currentDirection =
            GetDirectionFromDoughCenter(eventData);

        if (currentDirection == Vector2.zero)
            return;

        if (previousDirection != Vector2.zero)
        {
            float angle =
                Vector2.SignedAngle(
                    previousDirection,
                    currentDirection
                );

            if (Mathf.Abs(angle) < 2f)
            {
                previousDirection = currentDirection;
                return;
            }

            accumulatedRotation += Mathf.Abs(angle);

            if (accumulatedRotation >= 360f)
            {
                circleCount++;
                accumulatedRotation = 0f;

                SwitchPaw();

                if (circleCount >= requiredCircles)
                {
                    CompleteDough();
                }
            }
        }

        previousDirection = currentDirection;
    }

    Vector2 GetDirectionFromDoughCenter(PointerEventData eventData)
    {
        if (doughImage == null)
            return Vector2.zero;

        RectTransform doughRect =
            doughImage.rectTransform;

        Camera uiCamera =
            eventData.pressEventCamera;

        Vector2 localPoint;

        bool converted =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                doughRect,
                eventData.position,
                uiCamera,
                out localPoint
            );

        if (!converted)
            return Vector2.zero;

        if (localPoint.sqrMagnitude < 25f)
            return Vector2.zero;

        return localPoint.normalized;
    }

    void AnimatePawTap()
    {
        RectTransform paw =
            useLeftPaw ? leftPaw : rightPaw;

        Vector2 start =
            useLeftPaw ? leftPawStart : rightPawStart;

        if (paw != null)
        {
            StartCoroutine(PawPressAnimation(paw, start));
        }

        SwitchPaw();
    }

    void AnimatePawCircle(Vector2 delta)
    {
        RectTransform paw =
            useLeftPaw ? leftPaw : rightPaw;

        if (paw == null)
            return;

        Vector2 start =
            useLeftPaw ? leftPawStart : rightPawStart;

        paw.anchoredPosition += delta * pawCircleMoveMultiplier;

        if (Vector2.Distance(paw.anchoredPosition, start) > pawCircleMaxDistance)
        {
            paw.anchoredPosition =
                start +
                (paw.anchoredPosition - start).normalized *
                pawCircleMaxDistance;
        }
    }

    System.Collections.IEnumerator PawPressAnimation(
        RectTransform paw,
        Vector2 start
    )
    {
        Vector2 down =
            start + new Vector2(0f, -pawPressDistance);

        float time = 0f;

        while (time < pawPressDuration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    time / pawPressDuration
                );

            paw.anchoredPosition =
                Vector2.Lerp(start, down, t);

            yield return null;
        }

        time = 0f;

        while (time < pawReturnDuration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    time / pawReturnDuration
                );

            paw.anchoredPosition =
                Vector2.Lerp(down, start, t);

            yield return null;
        }

        paw.anchoredPosition = start;
    }

    void SwitchPaw()
    {
        useLeftPaw = !useLeftPaw;
    }

    void CompleteDough()
    {
        doughReady = true;

        PizzaRuntimeData.doughReady = true;
        PizzaRuntimeData.pizzaInOven = false;

        if (doughImage != null && stretchedDoughSprite != null)
        {
            doughImage.sprite = stretchedDoughSprite;
            doughImage.rectTransform.sizeDelta = stretchedDoughSize;
            doughImage.raycastTarget = true;

            doughImage.transform.SetAsLastSibling();
        }

        if (leftPawImage != null && leftPawHoldingSprite != null)
            leftPawImage.sprite = leftPawHoldingSprite;

        if (rightPawImage != null && rightPawHoldingSprite != null)
            rightPawImage.sprite = rightPawHoldingSprite;

        if (leftPaw != null)
            leftPaw.anchoredPosition = leftPawStart;

        if (rightPaw != null)
            rightPaw.anchoredPosition = rightPawStart;

        StopKneadingMusic();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnDoughCompleted();
    }

    public void ResetDoughForNewPizza()
    {
        tapCount = 0;
        circleCount = 0;

        doughReady = false;
        kneadingMusicStarted = false;
        useLeftPaw = true;

        accumulatedRotation = 0f;
        previousDirection = Vector2.zero;

        StopKneadingMusic();

        if (doughImage != null)
        {
            if (rawDoughSprite != null)
                doughImage.sprite = rawDoughSprite;

            doughImage.rectTransform.sizeDelta = doughStartSize;
            doughImage.rectTransform.localScale = doughStartScale;
            doughImage.raycastTarget = true;

            doughImage.transform.SetSiblingIndex(0);
        }

        if (leftPaw != null)
            leftPaw.anchoredPosition = leftPawStart;

        if (rightPaw != null)
            rightPaw.anchoredPosition = rightPawStart;

        if (leftPawImage != null)
            leftPawImage.sprite = leftPawStartSprite;

        if (rightPawImage != null)
            rightPawImage.sprite = rightPawStartSprite;
    }

    void StartKneadingMusic()
    {
        if (AudioManager.Instance != null &&
            AudioManager.Instance.musicSource != null)
        {
            AudioManager.Instance.musicSource.Pause();
        }

        if (doughAudioSource == null || kneadingMusic == null)
            return;

        doughAudioSource.clip = kneadingMusic;
        doughAudioSource.loop = true;
        doughAudioSource.volume = kneadingMusicVolume;
        doughAudioSource.Play();
    }

    void StopKneadingMusic()
    {
        if (doughAudioSource != null)
            doughAudioSource.Stop();

        if (AudioManager.Instance != null &&
            AudioManager.Instance.musicSource != null)
        {
            AudioManager.Instance.musicSource.UnPause();
        }
    }
}