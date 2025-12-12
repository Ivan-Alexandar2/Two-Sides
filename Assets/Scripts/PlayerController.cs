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
        // Use CharacterController's built-in ground check as primary
        isGrounded = controller.isGrounded;

        // Additional raycast check for more reliability
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f - controller.radius + groundCheckOffset);
        bool raycastGrounded = Physics.CheckSphere(spherePosition, controller.radius - 0.05f, groundMask, QueryTriggerInteraction.Ignore);

        // Use both checks for better reliability
        isGrounded = isGrounded || raycastGrounded;

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
        if (objectToTeleport.isInBoat)
        {
            TryDisembark(objectToTeleport);
        }
        else
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
        npc.transform.position = targetBeacon.transform.position;

        targetBeacon.inhabitant = npc.gameObject;
        targetBeacon.isTaken = true;
    }

    private void TryDisembark(NPC npc)
    {
        TeleportBeacon[] targetBank = boat.isOnRightBank ? gameManager.RightBankBeacons : gameManager.LeftBankBeacons;

        TeleportBeacon freeBeacon = FindFreeBeacon(targetBank);

        if (freeBeacon != null)
        {
            Teleport(npc, freeBeacon);
            npc.isInBoat = false;

            FreeUpBeacon(gameManager.BoatBeacons, npc.gameObject);
        }
    }

    private void TryBoard(NPC npc)
    {
        TeleportBeacon freeBoatSeat = FindFreeBeacon(gameManager.BoatBeacons);

        if (freeBoatSeat != null)
        {
            FreeUpBeacon(gameManager.RightBankBeacons, npc.gameObject);
            FreeUpBeacon(gameManager.LeftBankBeacons, npc.gameObject);

            Teleport(npc, freeBoatSeat);
            npc.isInBoat = true;
        }
    }

    private void TryBoard응응응응(NPC npc)
    {
        TeleportBeacon freeBoatSeat = FindFreeBeacon(gameManager.BoatBeacons);

        if (freeBoatSeat != null)
        {
            FreeUpBeacon(gameManager.RightBankBeacons, npc.gameObject);
            FreeUpBeacon(gameManager.LeftBankBeacons, npc.gameObject);

            Teleport(npc, freeBoatSeat);
            npc.isInBoat = true;
        }
    }
    #endregion
}