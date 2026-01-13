using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region MOVEMENT SETTINGS
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravityMultiplier = 2f;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    [Range(0f, 90f)] public float verticalLookLimit = 90f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask = 1;
    public float groundCheckOffset = 0.1f;

    [Header("Jump Settings")]
    public float jumpCooldown = 0.1f;
    public float airControl = 0.5f;

    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private float currentSpeed;
    private float lastJumpTime;

    // Input cache
    private float horizontalInput;
    private float verticalInput;
    private bool jumpInput;
    private bool sprintInput;

    // Constants
    private const float TERMINAL_VELOCITY = 50f;
    private const float GROUNDED_VELOCITY = -2f;
    #endregion

    // Non-movement related
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoatController boat;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactionText;
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public ParticleSystem poofEffectPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] poofSounds;
    public AudioClip[] footstepSounds; 
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    private float footstepTimer = 0f;

    void Start()
    {
        mainMenu.SetActive(true);
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>().transform;
        }

        // Initialize velocity
        velocity = Vector3.zero;

        interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        GetInput();
        HandleMouseLook();
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        HandleGravity();

        HandleInteraction();
        HandleFootsteps();
    }

    #region MOVEMENT
    void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetButtonDown("Jump");
        sprintInput = Input.GetKey(KeyCode.LeftShift);
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate camera vertically and clamp to prevent over-rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    void HandleGroundCheck()
    {
        // Cast from center of controller downward
        Vector3 rayOrigin = transform.position;
        float rayDistance = (controller.height / 2f) + 0.2f; // Extra margin

        // Do multiple checks for reliability
        bool centerCheck = Physics.Raycast(rayOrigin, Vector3.down, rayDistance, groundMask, QueryTriggerInteraction.Ignore);

        // Sphere check as backup
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f - controller.radius);
        bool sphereCheck = Physics.CheckSphere(spherePosition, controller.radius + 0.15f, groundMask, QueryTriggerInteraction.Ignore);

        isGrounded = centerCheck || sphereCheck;

        // If grounded and moving downward, reset vertical velocity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = GROUNDED_VELOCITY;
        }
    }

    void HandleMovement()
    {
        // Set speed based on sprint input (only when grounded or with reduced air control)
        currentSpeed = sprintInput ? sprintSpeed : walkSpeed;

        // Get movement direction relative to player's rotation
        Vector3 moveDirection = (transform.right * horizontalInput + transform.forward * verticalInput).normalized;

        // Apply air control reduction if in air
        float effectiveSpeed = currentSpeed;
        float controlFactor = 1f;

        if (!isGrounded)
        {
            controlFactor = airControl;
            effectiveSpeed = Mathf.Lerp(walkSpeed, currentSpeed, airControl);
        }

        // Move the controller
        Vector3 movement = moveDirection * effectiveSpeed * controlFactor * Time.deltaTime;
        controller.Move(movement);
    }

    void HandleJump()
    {
        // Check if we can jump
        bool canJump = isGrounded &&
                      jumpInput &&
                      Time.time >= lastJumpTime + jumpCooldown;

        if (canJump)
        {
            // Calculate jump velocity using physics formula: v = sqrt(2gh)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            lastJumpTime = Time.time;
            isGrounded = false; // Immediately set to false to prevent double jumps
        }
    }

    void HandleGravity()
    {
        // Only apply gravity if not grounded
        if (!isGrounded)
        {
            // Apply gravity with multiplier
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

            // Clamp terminal velocity
            velocity.y = Mathf.Clamp(velocity.y, -TERMINAL_VELOCITY, TERMINAL_VELOCITY);
        }

        // Apply vertical velocity
        controller.Move(velocity * Time.deltaTime);
    }

    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Public methods for external access
    public bool IsGrounded() => isGrounded;
    public bool IsSprinting() => sprintInput && isGrounded;
    public float GetCurrentSpeed() => currentSpeed;

    void HandleFootsteps()
    {
        if (!isGrounded)
        {
            footstepTimer = 0; // Reset when in air
            return;
        }

        // Check if actually moving - use input as primary indicator
        bool isMoving = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                PlayRandomFootstep();
                footstepTimer = sprintInput ? sprintStepInterval : walkStepInterval;
            }
        }
        else
        {
            footstepTimer = 0;
        }
    }

    void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;

        int index = Random.Range(0, footstepSounds.Length);

        // slight volume variance for realism
        float originalVolume = audioSource.volume;
        audioSource.PlayOneShot(footstepSounds[index], originalVolume * Random.Range(0.02f, 0.05f));
    }
    #endregion

    #region GAMEPLAY

    void HandleInteraction()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        bool foundInteractable = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                foundInteractable = true; // found something

                // Set Text
                interactionText.text = interactable.GetInteractionText();
                interactionText.color = interactable.GetInteractionColor();
                interactionText.gameObject.SetActive(true);

                if (Input.GetKeyDown(interactKey))
                    interactable.Interact(this);
            }
        }

        if (!foundInteractable)
            interactionText.gameObject.SetActive(false);

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance,
                      foundInteractable ? Color.green : Color.white);
    }

    public void MoveEntity(NPC objectToTeleport)
    {
        if (objectToTeleport.isInBoat && !boat.isMoving)
        {
            TryDisembark(objectToTeleport);
        }
        else if (!objectToTeleport.isInBoat && !boat.isMoving)
        {
            TryBoard(objectToTeleport);
        }
    }

    private TeleportBeacon FindFreeBeacon(TeleportBeacon[] beacons)
    {
        foreach (var beacon in beacons)
        {
            if (!beacon.isTaken && beacon.inhabitant == null)
            {
                return beacon;
            }
        }
        return null; // No space
    }

    // Frees up space
    private void FreeUpBeacon(TeleportBeacon[] beacons, GameObject objectToRemove)
    {
        foreach (var beacon in beacons)
        {
            if (beacon.inhabitant == objectToRemove)
            {
                beacon.inhabitant = null;
                beacon.isTaken = false;
                return;
            }
        }
    }

    // The teleportation execution
    private void Teleport(NPC npc, TeleportBeacon targetBeacon)
    {
        if(!boat.isMoving)
        {
            // particles
            Instantiate(poofEffectPrefab, npc.transform.position + Vector3.up, Quaternion.identity);

            // audio
            int index = Random.Range(0, poofSounds.Length);
            audioSource.PlayOneShot(poofSounds[index]);

            npc.transform.position = targetBeacon.transform.position;

            targetBeacon.inhabitant = npc.gameObject;
            targetBeacon.isTaken = true;

            Instantiate(poofEffectPrefab, npc.transform.position + Vector3.up, Quaternion.identity);
        }    
    }

    private void TryDisembark(NPC npc)
    {
        TeleportBeacon[] targetBank = boat.isOnRightBank ? gameManager.RightBankBeacons : gameManager.LeftBankBeacons;

        TeleportBeacon freeBeacon = FindFreeBeacon(targetBank);

        if (freeBeacon != null && !boat.isMoving)
        {
            Teleport(npc, freeBeacon);
            npc.isInBoat = false;

            FreeUpBeacon(gameManager.BoatBeacons, npc.gameObject);
        }
    }

    private void TryBoard(NPC npc)
    {
        // SECURITY CHECK: Is the boat on the same side as the NPC?
        if (boat.isOnRightBank)
        {
            // Boat is Right -> NPC must be on Right Bank
            if (!IsOnSpecificBank(npc, gameManager.RightBankBeacons))
            {
                Debug.Log("Cannot board: Boat is on Right Bank, but NPC is on Left!");
                return; // STOP HERE
            }
        }
        else
        {
            // Boat is Left -> NPC must be on Left Bank
            if (!IsOnSpecificBank(npc, gameManager.LeftBankBeacons))
            {
                Debug.Log("Cannot board: Boat is on Left Bank, but NPC is on Right!");
                return; // STOP HERE
            }
        }

        TeleportBeacon freeBoatSeat = FindFreeBeacon(gameManager.BoatBeacons);

        if (freeBoatSeat != null && !boat.isMoving)
        {
            FreeUpBeacon(gameManager.RightBankBeacons, npc.gameObject);
            FreeUpBeacon(gameManager.LeftBankBeacons, npc.gameObject);

            Teleport(npc, freeBoatSeat);
            npc.isInBoat = true;
        }
    }

    private bool IsOnSpecificBank(NPC npc, TeleportBeacon[] bankBeacons)
    {
        foreach (var beacon in bankBeacons)
        {
            if (beacon.inhabitant == npc.gameObject)
            {
                return true; // Found them on this bank
            }
        }
        return false; // They are not here
    }
    #endregion
}