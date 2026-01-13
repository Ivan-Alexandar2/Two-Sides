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
    public ParticleSystem poof;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] villagerIdleSounds;
    public AudioClip[] killerIdleSounds;

    // Private timer variables
    private float idleTimer = 0f;
    private float timeUntilNextSound = 0f;
    private bool hasPlayedSoundInCycle = false;
    private const float CYCLE_DURATION = 15f;
    void Update()
    {
        HandleIdleAudio();
    }

    void Start()
    {
        timeUntilNextSound = Random.Range(2f, CYCLE_DURATION - 1f);
    }

    // Interface Implementation
    public string GetInteractionText() { return "Move NPC"; }
    public Color GetInteractionColor() { return Color.yellow; }

    public void Interact(PlayerController player)
    {
        // We tell the player to decide what to do with us
        player.MoveEntity(this);
    }

    private void HandleIdleAudio()
    {
        // 1. Increase timer
        idleTimer += Time.deltaTime;

        // 2. Check if it is time to play the sound
        if (!hasPlayedSoundInCycle && idleTimer >= timeUntilNextSound)
        {
            PlayRandomIdleSound();
            hasPlayedSoundInCycle = true; // Mark as done so it only plays once
        }

        // 3. Reset the cycle after 15 seconds
        if (idleTimer >= CYCLE_DURATION)
        {
            idleTimer = 0f;
            hasPlayedSoundInCycle = false;

            // Pick a new random time for the next 15-second block
            timeUntilNextSound = Random.Range(2f, CYCLE_DURATION - 1f);
        }
    }

    private void PlayRandomIdleSound()
    {
        // Determine which array to use based on components
        AudioClip[] clipsToPlay = null;

        if (GetComponent<Killer>() != null)
        {
            clipsToPlay = killerIdleSounds;
        }
        else
        {
            clipsToPlay = villagerIdleSounds;
        }

        // Safety check: Make sure there are sounds assigned!
        if (clipsToPlay != null && clipsToPlay.Length > 0)
        {
            // Pick random sound
            int index = Random.Range(0, clipsToPlay.Length);

            // Randomize pitch slightly (0.9 to 1.1 is natural, 0.8 to 1.2 is crazier)
            audioSource.pitch = Random.Range(0.9f, 1.1f);

            // Play
            audioSource.PlayOneShot(clipsToPlay[index]);
        }
    }
}
