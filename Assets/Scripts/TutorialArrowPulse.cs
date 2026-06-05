using UnityEngine;

public class TutorialArrowPulse : MonoBehaviour
{
    public RectTransform arrowRect;

    public float pulseScale = 1.25f;
    public float pulseSpeed = 4f;

    private Vector3 baseScale;

    void Awake()
    {
        if (arrowRect == null)
            arrowRect = GetComponent<RectTransform>();

        baseScale = arrowRect.localScale;
    }

    void Update()
    {
        if (arrowRect == null)
            return;

        float pulse =
            1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * (pulseScale - 1f);

        arrowRect.localScale = baseScale * pulse;
    }
}