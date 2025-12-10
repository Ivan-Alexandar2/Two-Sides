using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGuy : MonoBehaviour, IInteractable
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip tutorialClip;

    private bool isSpeaking = false;

    public string GetInteractionText()
    {
        return "Press E for Tutorial";
    }

    public Color GetInteractionColor()
    {
        return Color.yellow;
    }

    public void Interact(PlayerController player)
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        // Prevent interaction if already talking
        if (isSpeaking) return;

        StartCoroutine(PlayTutorialSequence());
    }

    private IEnumerator PlayTutorialSequence()
    {
        isSpeaking = true;

        // Play Audio
        audioSource.PlayOneShot(tutorialClip);

        // Start Talking Animation
        animator.SetBool("isTalking", true);

        // Wait for the exact duration of the audio clip
        yield return new WaitForSeconds(tutorialClip.length);

        // Stop Animation (Return to Idle)
        animator.SetBool("isTalking", false);

        isSpeaking = false;
    }
}
