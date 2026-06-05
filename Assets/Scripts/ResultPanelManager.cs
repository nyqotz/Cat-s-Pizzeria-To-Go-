using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPanelManager : MonoBehaviour
{
    [Header("Main References")]
    public CanvasGroup fadeOverlay;
    public RectTransform resultContent;

    [Header("Elements")]
    public Image customerImage;
    public RectTransform pizzaResultContainer;
    public TMP_Text resultText;
    public Image thumbImage;

    [Header("Pizza Size")]
    public Vector2 pizzaVisualSize = new Vector2(250f, 250f);

    [Header("Audio")]
    public AudioClip drumRollClip;
    public AudioClip victoryClip;
    public AudioClip failClip;
    public AudioClip metalClip;

    [Header("Timings")]
    public float fadeDuration = 3f;
    public float returnFadeDuration = 4f;
    public float slideDuration = 0.8f;
    public float drumRollDuration = 4f;
    public float resultHoldDuration = 3f;
    public float successResultHoldDuration = 4.8f;
    public float audioGapAfterDrumRoll = 0.15f;

    [Header("Slide Settings")]
    public float slideOffsetX = 2500f;

    [Header("Fail Animation")]
    public float angryShakeDuration = 2f;
    public float angryShakeSpeed = 20f;
    public float angryShakeAngle = 8f;

    private GameObject pizzaClone;
    private AudioSource resultAudioSource;

    private Vector2 resultContentOriginalPosition;
    private Vector2 customerOriginalPosition;
    private Vector2 pizzaContainerOriginalPosition;
    private Vector2 resultTextOriginalPosition;
    private Vector2 thumbOriginalPosition;

    void Awake()
    {
        SaveOriginalPositions();

        resultAudioSource = GetComponent<AudioSource>();

        if (resultAudioSource == null)
            resultAudioSource = gameObject.AddComponent<AudioSource>();

        resultAudioSource.playOnAwake = false;
        resultAudioSource.loop = false;
        resultAudioSource.spatialBlend = 0f;
    }

    void SaveOriginalPositions()
    {
        if (resultContent != null)
            resultContentOriginalPosition = resultContent.anchoredPosition;

        if (customerImage != null)
            customerOriginalPosition = customerImage.rectTransform.anchoredPosition;

        if (pizzaResultContainer != null)
            pizzaContainerOriginalPosition = pizzaResultContainer.anchoredPosition;

        if (resultText != null)
            resultTextOriginalPosition = resultText.rectTransform.anchoredPosition;

        if (thumbImage != null)
            thumbOriginalPosition = thumbImage.rectTransform.anchoredPosition;
    }

    public void PlayResult(
        bool isCorrect,
        Sprite customerSprite,
        GameObject pizzaVisual,
        System.Action onBlackScreenBeforeReturn,
        System.Action onFinishedAfterFadeIn
    )
    {
        StopAllCoroutines();
        StopResultAudio();

        gameObject.SetActive(true);

        StartCoroutine(
            ResultRoutine(
                isCorrect,
                customerSprite,
                pizzaVisual,
                onBlackScreenBeforeReturn,
                onFinishedAfterFadeIn
            )
        );
    }

    IEnumerator ResultRoutine(
        bool isCorrect,
        Sprite customerSprite,
        GameObject pizzaVisual,
        System.Action onBlackScreenBeforeReturn,
        System.Action onFinishedAfterFadeIn
    )
    {
        PrepareInitialState();

        PauseGameMusic();

        SetupCustomer(customerSprite);
        SetupPizza(pizzaVisual);

        yield return FadeOverlay(0f, 1f, fadeDuration);

        yield return SlideContent(
            resultContentOriginalPosition + new Vector2(slideOffsetX, 0f),
            resultContentOriginalPosition
        );

        PlayDrumRoll();

        yield return new WaitForSeconds(drumRollDuration);

        StopResultAudio();

        yield return new WaitForSeconds(audioGapAfterDrumRoll);

        if (isCorrect)
{
    ShowSuccess();

    yield return new WaitForSeconds(successResultHoldDuration);

    StopResultAudio();
}
else
{
    yield return ShowFail();

    yield return new WaitForSeconds(resultHoldDuration);

    StopResultAudio();
}

        yield return SlideContent(
            resultContentOriginalPosition,
            resultContentOriginalPosition + new Vector2(-slideOffsetX, 0f)
        );

        StopResultAudio();

        if (onBlackScreenBeforeReturn != null)
            onBlackScreenBeforeReturn.Invoke();

        yield return FadeOverlay(1f, 0f, returnFadeDuration);

        ClearPizza();
        StopResultAudio();
        ResumeGameMusic();

        if (onFinishedAfterFadeIn != null)
            onFinishedAfterFadeIn.Invoke();

        gameObject.SetActive(false);
    }

    void PrepareInitialState()
    {
        RestoreOriginalPositions();

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.interactable = false;
        }

        if (resultContent != null)
        {
            resultContent.anchoredPosition =
                resultContentOriginalPosition + new Vector2(slideOffsetX, 0f);

            resultContent.localScale = Vector3.one;
            resultContent.localRotation = Quaternion.identity;
            resultContent.gameObject.SetActive(true);
        }

        if (resultText != null)
            resultText.text = "";

        if (thumbImage != null)
        {
            thumbImage.gameObject.SetActive(false);
            thumbImage.rectTransform.localScale = Vector3.zero;
        }

        ClearPizza();
    }

    void RestoreOriginalPositions()
    {
        if (resultContent != null)
            resultContent.anchoredPosition = resultContentOriginalPosition;

        if (customerImage != null)
            customerImage.rectTransform.anchoredPosition = customerOriginalPosition;

        if (pizzaResultContainer != null)
            pizzaContainerOriginalPosition = pizzaResultContainer.anchoredPosition;

        if (resultText != null)
            resultText.rectTransform.anchoredPosition = resultTextOriginalPosition;

        if (thumbImage != null)
            thumbImage.rectTransform.anchoredPosition = thumbOriginalPosition;
    }

    void SetupCustomer(Sprite customerSprite)
    {
        if (customerImage == null)
            return;

        customerImage.sprite = customerSprite;
        customerImage.preserveAspect = true;
        customerImage.gameObject.SetActive(customerSprite != null);

        RectTransform rect = customerImage.rectTransform;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    void SetupPizza(GameObject pizzaVisual)
    {
        if (pizzaVisual == null || pizzaResultContainer == null)
            return;

        pizzaClone = Instantiate(pizzaVisual, pizzaResultContainer, false);
        pizzaClone.name = "ResultPizzaVisual";

        Canvas[] canvases = pizzaClone.GetComponentsInChildren<Canvas>(true);

        for (int i = 0; i < canvases.Length; i++)
            Destroy(canvases[i]);

        GraphicRaycaster[] raycasters =
            pizzaClone.GetComponentsInChildren<GraphicRaycaster>(true);

        for (int i = 0; i < raycasters.Length; i++)
            Destroy(raycasters[i]);

        RectTransform pizzaRect = pizzaClone.GetComponent<RectTransform>();

        if (pizzaRect != null)
        {
            pizzaRect.anchorMin = new Vector2(0.5f, 0.5f);
            pizzaRect.anchorMax = new Vector2(0.5f, 0.5f);
            pizzaRect.pivot = new Vector2(0.5f, 0.5f);
            pizzaRect.anchoredPosition = Vector2.zero;

            Vector2 originalSize = pizzaRect.rect.size;

            if (originalSize.x <= 0f || originalSize.y <= 0f)
                originalSize = pizzaRect.sizeDelta;

            if (originalSize.x <= 0f || originalSize.y <= 0f)
                originalSize = new Vector2(300f, 300f);

            pizzaRect.sizeDelta = originalSize;

            float scaleX = pizzaVisualSize.x / originalSize.x;
            float scaleY = pizzaVisualSize.y / originalSize.y;

            pizzaRect.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        Image[] pizzaImages = pizzaClone.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < pizzaImages.Length; i++)
        {
            pizzaImages[i].enabled = true;

            Color c = pizzaImages[i].color;
            pizzaImages[i].color = new Color(c.r, c.g, c.b, 1f);
        }

        pizzaClone.transform.SetAsLastSibling();

        DisableRaycasts(pizzaClone);
    }

    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        if (fadeOverlay == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            fadeOverlay.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeOverlay.alpha = to;
    }

    IEnumerator SlideContent(Vector2 from, Vector2 to)
    {
        if (resultContent == null)
            yield break;

        float elapsed = 0f;

        resultContent.anchoredPosition = from;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            resultContent.anchoredPosition = Vector2.Lerp(from, to, t);

            yield return null;
        }

        resultContent.anchoredPosition = to;
    }

    void ShowSuccess()
{
    if (resultText != null)
        resultText.text = "PERFETTA!";

    PlayResultSFX(victoryClip);

    if (thumbImage != null)
    {
        thumbImage.gameObject.SetActive(true);
        StartCoroutine(PopThumb());
    }
}

    IEnumerator ShowFail()
    {
        if (resultText != null)
            resultText.text = "BLEEH!";

        PlayResultSFX(failClip);

        StartCoroutine(ShakeCustomer());

        yield return new WaitForSeconds(0.5f);

        PlayResultSFX(metalClip);
    }

    IEnumerator PopThumb()
    {
        if (thumbImage == null)
            yield break;

        RectTransform rect = thumbImage.rectTransform;

        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            yield return null;
        }

        rect.localScale = Vector3.one;
    }

    IEnumerator ShakeCustomer()
    {
        if (customerImage == null)
            yield break;

        RectTransform rect = customerImage.rectTransform;

        float elapsed = 0f;

        while (elapsed < angryShakeDuration)
        {
            elapsed += Time.deltaTime;

            float angle =
                Mathf.Sin(elapsed * angryShakeSpeed) *
                angryShakeAngle;

            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        rect.localRotation = Quaternion.identity;
    }

    void PlayDrumRoll()
    {
        if (resultAudioSource == null || drumRollClip == null)
            return;

        resultAudioSource.Stop();
        resultAudioSource.loop = true;
        resultAudioSource.clip = drumRollClip;
        resultAudioSource.Play();
    }

    void PlayResultSFX(AudioClip clip)
    {
        if (resultAudioSource == null || clip == null)
            return;

        resultAudioSource.Stop();
        resultAudioSource.loop = false;
        resultAudioSource.clip = clip;
        resultAudioSource.Play();
    }

    void PlayLoopingResultSFX(AudioClip clip)
    {
        if (resultAudioSource == null || clip == null)
            return;

        resultAudioSource.Stop();
        resultAudioSource.loop = true;
        resultAudioSource.clip = clip;
        resultAudioSource.Play();
    }

    void StopResultAudio()
    {
        if (resultAudioSource == null)
            return;

        resultAudioSource.Stop();
        resultAudioSource.loop = false;
        resultAudioSource.clip = null;
    }

    void PauseGameMusic()
    {
        if (AudioManager.Instance == null)
            return;

        if (AudioManager.Instance.musicSource == null)
            return;

        AudioManager.Instance.musicSource.Pause();
    }

    void ResumeGameMusic()
    {
        if (AudioManager.Instance == null)
            return;

        if (AudioManager.Instance.musicSource == null)
            return;

        AudioManager.Instance.musicSource.UnPause();
    }

    void ClearPizza()
    {
        if (pizzaClone != null)
        {
            Destroy(pizzaClone);
            pizzaClone = null;
        }
    }

    void DisableRaycasts(GameObject target)
    {
        Graphic[] graphics = target.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }
}