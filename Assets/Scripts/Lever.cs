using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    public AudioSource audioSource;
    public AudioClip leverPull;

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

        audioSource.PlayOneShot(leverPull);

        // Boat movement logic
        if (!boat.isMoving)
        {
            // Check if has driver
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
