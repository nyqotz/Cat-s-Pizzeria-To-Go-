using UnityEngine;

public static class PizzaRuntimeData
{
    public static bool doughReady = false;
    public static bool pizzaInOven = false;
    public static bool pizzaReady = false;

    public static bool hasSugo = false;
    public static bool hasMozzarella = false;
    public static bool hasTonno = false;
    public static bool hasCipolla = false;

    public static string bakeState = "Cruda";

    public static void ResetPizza()
    {
        doughReady = false;
        pizzaInOven = false;
        pizzaReady = false;

        hasSugo = false;
        hasMozzarella = false;
        hasTonno = false;
        hasCipolla = false;

        bakeState = "Cruda";
    }
}