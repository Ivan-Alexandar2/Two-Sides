using UnityEngine;
public class Person : NPC, IInteractable
{
    public string GetInteractionText()
    {
        return "E to move Person";
    }

    public Color GetInteractionColor()
    {
        return Color.blue;
    }

    public void Interact(PlayerController player)
    {
        GameManager gm = FindObjectOfType<GameManager>();

        if (gm.TotalBoatPopulation <= 2)
        {
            player.MoveEntity(this);
            gm.KillersInBoat++;
        }
    }
}
