using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    public string GetInteractionText()
    {
        return "Press E to pull Lever";
    }

    public Color GetInteractionColor()
    {
        return Color.gray;
    }

    public void Interact(PlayerController player)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        BoatController boat = FindObjectOfType<BoatController>();

        // Логиката за пускане на лодката
        if (!boat.isMoving)
        {
            // Проверка дали има шофьор
            bool hasDriver = false;
            foreach (var beacon in gm.BoatBeacons)
            {
                if (beacon.inhabitant != null) hasDriver = true;
            }

            if (hasDriver)
            {
                boat.StartMove();
            }
            else
            {
                Debug.Log("Boat needs a driver");
            }
        }
    }
}
