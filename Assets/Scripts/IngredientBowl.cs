using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientBowl : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public IngredientManager ingredientManager;
    public string ingredientName;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ingredientManager == null)
            return;

        ingredientManager.StartHoldingIngredient(
            ingredientName,
            eventData.position
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ingredientManager == null)
            return;

        ingredientManager.MoveHeldIngredient(
            eventData.position
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (ingredientManager == null)
            return;

        ingredientManager.ReleaseHeldIngredient(
            eventData.position
        );
    }
}