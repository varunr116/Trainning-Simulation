using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float mouseSensitivity = 2f;
    public float upDownRange = 80f;

    [Header("References")]
    public Camera playerCamera;
    
    private CharacterController characterController;
    private float verticalRotation = 0;
    private Vector3 moveDirection;
    
    // Movement and input control
    private bool movementEnabled = true;
    private bool cursorLocked = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // If no camera assigned, use main camera
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        // Initialize cursor state
        SetCursorLocked(true);
    }

    void Update()
    {
        if (movementEnabled)
        {
            HandleMouseLook();
            HandleMovement();
        }
        
        // Only handle ESC if movement is enabled (not in inspection)
        if (movementEnabled)
        {
            HandleCursorToggle();
        }
    }

    void HandleMouseLook()
    {
        if (!cursorLocked) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(0, mouseX, 0);

        // Rotate camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Calculate movement direction relative to player rotation
        Vector3 forward = transform.forward * vertical;
        Vector3 right = transform.right * horizontal;
        
        moveDirection = (forward + right).normalized * walkSpeed;

        // Apply basic gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= 9.81f * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -1f; // Keep grounded
        }

        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleCursorToggle()
    {
        // Press ESC to toggle cursor lock (only when movement enabled)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    public void ToggleCursorLock()
    {
        SetCursorLocked(!cursorLocked);
    }
    
    public void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        
        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        Debug.Log($"Cursor locked: {cursorLocked}");
    }

    // Public method to enable/disable movement (called from InteractionManager)
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        Debug.Log($"Player movement: {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    public bool IsMovementEnabled()
    {
        return movementEnabled;
    }
}