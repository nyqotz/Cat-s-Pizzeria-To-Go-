using UnityEngine;

public class StarReputationTester : MonoBehaviour
{
    public StarReputationUI starReputationUI;

    public float currentStars = 3f;
    public float testAmount = 0.1f;

    void Start()
    {
        if (starReputationUI != null)
            starReputationUI.SetRating(currentStars, false);
    }

    public void AddStars()
    {
        currentStars += testAmount;
        currentStars = Mathf.Clamp(currentStars, 0f, 5f);

        if (starReputationUI != null)
            starReputationUI.SetRating(currentStars, true);
    }

    public void RemoveStars()
    {
        currentStars -= testAmount;
        currentStars = Mathf.Clamp(currentStars, 0f, 5f);

        if (starReputationUI != null)
            starReputationUI.SetRating(currentStars, true);
    }
}