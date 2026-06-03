using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StarReputationUI : MonoBehaviour
{
    public Image[] stars;

    public Sprite emptyStarSprite;
    public Sprite fullStarSprite;
    public Sprite halfStarSprite;
    public Sprite goldenStarSprite;

    public float popScale = 1.25f;
    public float popDuration = 0.18f;

    private float currentRating = 3f;
    private Coroutine animationRoutine;

    private void Start()
    {
        SetRating(currentRating, false);
    }

    public void SetRating(float rating, bool animate)
    {
        currentRating = Mathf.Clamp(rating, 0f, 5f);

        RefreshStars();

        if (animate)
        {
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);

            animationRoutine = StartCoroutine(AnimateStars());
        }
    }

    public float GetRating()
    {
        return currentRating;
    }

    private void RefreshStars()
    {
        bool isPerfect = Mathf.Approximately(currentRating, 5f);

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            if (isPerfect && goldenStarSprite != null)
            {
                stars[i].sprite = goldenStarSprite;
                continue;
            }

            float starValue = i + 1;

            if (currentRating >= starValue)
            {
                stars[i].sprite = fullStarSprite;
            }
            else if (currentRating >= starValue - 0.5f && halfStarSprite != null)
            {
                stars[i].sprite = halfStarSprite;
            }
            else
            {
                stars[i].sprite = emptyStarSprite;
            }

            stars[i].rectTransform.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            RectTransform rect = stars[i].rectTransform;

            Vector3 normalScale = Vector3.one;
            Vector3 bigScale = Vector3.one * popScale;

            float elapsed = 0f;

            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / popDuration));
                rect.localScale = Vector3.Lerp(normalScale, bigScale, t);
                yield return null;
            }

            elapsed = 0f;

            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / popDuration));
                rect.localScale = Vector3.Lerp(bigScale, normalScale, t);
                yield return null;
            }

            rect.localScale = normalScale;
        }

        animationRoutine = null;
    }
}