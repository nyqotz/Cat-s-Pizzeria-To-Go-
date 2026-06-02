using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text orderText;
    public TMP_Text scoreText;
    public TMP_Text feedbackText;

    private string currentOrder;
    private int score = 0;

    private string[] foods = { "Latte", "Pesce", "Crocchette" };

    void Start()
    {
        GenerateOrder();
        UpdateScore();
        feedbackText.text = "";
    }

    void GenerateOrder()
    {
        currentOrder = foods[Random.Range(0, foods.Length)];
        orderText.text = "Il gattino vuole: " + currentOrder;
    }

    public void ChooseFood(string food)
    {
        if (food == currentOrder)
        {
            score += 10;
            feedbackText.text = "<color=green>Miao! Ordine giusto!</color>";
        }
        else
        {
            score -= 5;
            feedbackText.text = "<color=red>Grrr... ordine sbagliato!</color>";
        }

        UpdateScore();
        GenerateOrder();
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }
}