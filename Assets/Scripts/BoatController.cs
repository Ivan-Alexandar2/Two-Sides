using UnityEngine;

public class BoatController : MonoBehaviour
{
    public bool isOnRightBank = true;
    public bool isMoving = false;

    [Header("Movement Settings")]
    public float moveDuration = 3f;
    public float moveDistance = 10f;
    public float moveSpeed = 3f;

    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    private float startTime;

    public ParticleSystem wakeParticles;
    public ParticleSystem foamParticles;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + -transform.right * moveDistance;
        transform.position = startPosition;
    }

    private void Update()
    {
        if (isMoving)
        {
            float timePassed = Time.time - startTime;
            float progress = timePassed / moveDuration;

            // Determine the actual start and end points for the current trip
            Vector3 currentStart = isOnRightBank ? startPosition : endPosition;
            Vector3 currentEnd = isOnRightBank ? endPosition : startPosition;

            transform.position = Vector3.Lerp(currentStart, currentEnd, progress);

            if (!wakeParticles.isPlaying) wakeParticles.Play();
            if (!foamParticles.isPlaying) foamParticles.Play();

            if (progress >= 1f)
            {
                isMoving = false;
                transform.position = currentEnd;
                isOnRightBank = !isOnRightBank;

                if (wakeParticles.isPlaying) wakeParticles.Stop();
                if (foamParticles.isPlaying) foamParticles.Stop();
            }
        }
    }

    public void StartMove()
    {
        if (!isMoving)
        {
            isMoving = true;
            startTime = Time.time;
        }
    }
}
