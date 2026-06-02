using UnityEngine;

public static class PizzaRuntimeData
{
    public static bool hasSugo;
    public static bool hasMozzarella;
    public static bool hasTonno;
    public static bool hasCipolla;

    public static string bakeState = "Cruda";

    public static void ResetPizza()
    {
        hasSugo = false;
        hasMozzarella = false;
        hasTonno = false;
        hasCipolla = false;
        bakeState = "Cruda";
    }
}