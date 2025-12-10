using UnityEngine;

public interface IInteractable
{
    string GetInteractionText();
    Color GetInteractionColor();
    void Interact(PlayerController player);
}
