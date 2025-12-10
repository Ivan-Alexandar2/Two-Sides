using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour, IInteractable
{
    public bool isOnRightBank;
    public bool isInBoat;
    public bool isOnLeftBank;

    public NavMeshAgent agent;
    public Animator animator;

    private TeleportBeacon reservedSeat;
    private bool isBoarding = false;

    // Interface Implementation
    public string GetInteractionText() { return "Move NPC"; }
    public Color GetInteractionColor() { return Color.yellow; }

    public void Interact(PlayerController player)
    {
        // We tell the player to decide what to do with us
        player.MoveEntity(this);
    }

    // --- WALKING LOGIC CALLED BY PLAYER CONTROLLER ---
    public void GoToBoat(Vector3 boardingPos, TeleportBeacon seat)
    {
        reservedSeat = seat;

        agent.enabled = true;
        agent.SetDestination(boardingPos);

        isBoarding = true;
        if (animator) animator.SetBool("isWalking", true);
    }

    private void Update()
    {
        if (isBoarding)
        {
            // Check if we have reached the edge of the dock
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                EnterBoat();
            }
        }
    }

    private void EnterBoat()
    {
        isBoarding = false;

        // Disable NavMesh so we can manually snap them into the boat
        agent.enabled = false;
        if (animator) animator.SetBool("isWalking", false);

        // Snap to the reserved seat
        transform.position = reservedSeat.transform.position;
        transform.parent = reservedSeat.transform;

        // Finalize state
        isInBoat = true;
    }
}
