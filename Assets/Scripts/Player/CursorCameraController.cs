
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CursorCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    
    [Header("Camera Look Settings")]
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 80f;
    public float smoothTime = 0.1f;
    
    [Header("References")]
    public Camera playerCamera;
    
    private CharacterController characterController;
    private float verticalRotation = 0;
    private Vector3 moveDirection;
    
    // Camera control state
    private bool isLookingAround = false;
    private Vector3 lastMousePosition;
    private float targetVerticalRotation = 0;
    private float currentVerticalVelocity = 0;
    private float targetHorizontalRotation = 0;
    private float currentHorizontalVelocity = 0;
    
    // Movement and input control
    private bool movementEnabled = true;
    private bool cameraControlEnabled = true;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // If no camera assigned, use main camera
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // Store initial rotations
        targetVerticalRotation = playerCamera.transform.localEulerAngles.x;
        targetHorizontalRotation = transform.eulerAngles.y;
        
        // Always show cursor - never lock it
        SetCursorVisible(true);
        
        
    }

    void Update()
    {
        if (movementEnabled)
        {
            HandleMovement();
        }
        
        if (cameraControlEnabled)
        {
            HandleCameraLook();
        }
    }
    
    
    
    void HandleCameraLook()
    {
        // Don't handle camera look if in inspection mode
        if (InteractionManager.Instance != null && InteractionManager.Instance.IsInspectionMode())
        {
            // If we were looking around, stop it
            if (isLookingAround)
            {
                StopLookingAround();
            }
            return;
        }
        
        // Check for click-drag to look around
        if (Input.GetMouseButtonDown(0))
        {
            // Start looking around only if not clicking on UI
            if (!IsPointerOverUI())
            {
                StartLookingAround();
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            StopLookingAround();
        }
        
        // Handle camera rotation while dragging
        if (isLookingAround)
        {
            UpdateCameraRotation();
        }
        
        // Apply smooth camera movements
        ApplySmoothCameraRotation();
    }
    
    void StartLookingAround()
    {
        // Don't start looking around if in inspection mode
        if (InteractionManager.Instance != null && InteractionManager.Instance.IsInspectionMode())
        {
            return;
        }
        
        isLookingAround = true;
        lastMousePosition = Input.mousePosition;
        
        
    }
    
    void StopLookingAround()
    {
        isLookingAround = false;
        
        
    }
    
    void UpdateCameraRotation()
    {
        // Calculate mouse delta
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        
        // Convert to rotation values
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;
        
        // Update target rotations
        targetHorizontalRotation += mouseX;
        targetVerticalRotation -= mouseY;
        
        // Clamp vertical rotation
        targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -verticalLookLimit, verticalLookLimit);
        
        // Store current mouse position for next frame
        lastMousePosition = Input.mousePosition;
    }
    
    void ApplySmoothCameraRotation()
    {
        // Smooth horizontal rotation (player body)
        float currentY = transform.eulerAngles.y;
        float newY = Mathf.SmoothDampAngle(currentY, targetHorizontalRotation, ref currentHorizontalVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0, newY, 0);
        
        // Smooth vertical rotation (camera)
        float newX = Mathf.SmoothDamp(verticalRotation, targetVerticalRotation, ref currentVerticalVelocity, smoothTime);
        verticalRotation = newX;
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
   
    
    void HandleMovement()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S
        
        // Check for run modifier
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // Calculate movement direction relative to player rotation
        Vector3 forward = transform.forward * vertical;
        Vector3 right = transform.right * horizontal;
        
        moveDirection = (forward + right).normalized * currentSpeed;

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
    
    
    
    bool IsPointerOverUI()
    {
        // Check if mouse is over UI elements
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
    
   
    
    void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    
   
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        
    }
    
    public void SetCameraControlEnabled(bool enabled)
    {
        cameraControlEnabled = enabled;
        
        if (!enabled)
        {
            // Stop any current look around
            StopLookingAround();
        }
        
       
    }
    
    public void SetCursorLocked(bool locked)
    {
        // For compatibility - but we never actually lock the cursor
        // Instead, we disable camera control
        SetCameraControlEnabled(!locked);
        
       
    }
    
    public bool IsMovementEnabled()
    {
        return movementEnabled;
    }
    
   
    
    [Header("Alternative Controls")]
    public bool enableArrowKeyLook = true;
    public float arrowKeyLookSpeed = 50f;
    
    void HandleAlternativeLook()
    {
        if (!enableArrowKeyLook || !cameraControlEnabled) return;
        
        // Don't handle arrow key look if in inspection mode
        if (InteractionManager.Instance != null && InteractionManager.Instance.IsInspectionMode())
        {
            return;
        }
        
        // Arrow key looking (for accessibility)
        float arrowX = 0f;
        float arrowY = 0f;
        
        if (Input.GetKey(KeyCode.LeftArrow)) arrowX = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) arrowX = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) arrowY = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) arrowY = -1f;
        
        if (arrowX != 0f || arrowY != 0f)
        {
            targetHorizontalRotation += arrowX * arrowKeyLookSpeed * Time.deltaTime;
            targetVerticalRotation += arrowY * arrowKeyLookSpeed * Time.deltaTime;
            targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -verticalLookLimit, verticalLookLimit);
        }
    }
    
   
  
    
    // Call this in Update() if you want arrow key support
    void Update_WithArrowKeys()
    {
        if (movementEnabled)
        {
            HandleMovement();
        }
        
        if (cameraControlEnabled)
        {
            HandleCameraLook();
            HandleAlternativeLook(); // Add this line
        }
    }
}