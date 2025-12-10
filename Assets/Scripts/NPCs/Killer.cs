using UnityEngine;

public class Killer : NPC, IInteractable
{
    public Color GetInteractionColor()
    {
        return Color.red;
    }

    public string GetInteractionText()
    {
        return "E to move Killer";
    }

    public void Interact(PlayerController player)
    {
        player.MoveEntity(this);
        FindObjectOfType<GameManager>().PeopleInBoat++;
    }
}
