using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;
    
    [Header("Interaction Settings")]
    public LayerMask interactionLayers = -1;
    public float interactionRange = 5f;
    
    [Header("UI References")]
    public GameObject inspectionUI;
    
    private Camera playerCamera;
    private InteractableItem currentlyInspecting;
    private InteractableItem hoveredItem;
    private bool isInspectionMode = false;
    private FirstPersonController playerController;
    
    // Mouse rotation tracking
    private bool isRotating = false;
    private Vector3 lastMousePosition;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        playerCamera = Camera.main;
        playerController = FindObjectOfType<FirstPersonController>();
        
        if (inspectionUI != null)
            inspectionUI.SetActive(false);
    }
    
    void Update()
    {
        if (isInspectionMode)
        {
            HandleInspectionInput();
        }
        else
        {
            HandleNormalInteraction();
        }
    }
    
    void HandleNormalInteraction()
    {
        // Only do raycasting if cursor is locked (normal play mode)
        if (Cursor.lockState != CursorLockMode.Locked) return;
        
        // Cast ray from camera to detect interactable items
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayers))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null && item != hoveredItem)
            {
                // New item hovered
                if (hoveredItem != null)
                    hoveredItem.SetHovered(false);
                    
                hoveredItem = item;
                item.SetHovered(true);
            }
        }
        else
        {
            // No item hovered
            if (hoveredItem != null)
            {
                hoveredItem.SetHovered(false);
                hoveredItem = null;
            }
        }
    }
    
    void HandleInspectionInput()
    {
        if (currentlyInspecting == null) return;
        
        // Don't allow rotation if item is still lerping
        if (currentlyInspecting.IsLerping()) return;
        
        // Mouse rotation for item inspection
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
        
        if (isRotating)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            currentlyInspecting.RotateItem(mouseDelta);
            lastMousePosition = Input.mousePosition;
        }
        
        // ESC or right-click to exit inspection (higher priority than FirstPersonController)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            EndItemInspection();
        }
    }
    
    public void OnItemHovered(InteractableItem item)
    {
        hoveredItem = item;
        // Show interaction prompt
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInteractionPrompt($"Click to inspect {item.itemData.displayName}");
        }
    }
    
    public void OnItemUnhovered(InteractableItem item)
    {
        if (hoveredItem == item)
        {
            hoveredItem = null;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractionPrompt();
            }
        }
    }
    
    public void OnItemClicked(InteractableItem item)
    {
        if (item.isInspectable && !isInspectionMode)
        {
            StartItemInspection(item);
        }
        else if (item.isCollectable)
        {
            item.CollectItem();
        }
    }
    
    void StartItemInspection(InteractableItem item)
    {
        currentlyInspecting = item;
        isInspectionMode = true;
        
        // DISABLE PLAYER MOVEMENT FIRST
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        
        // UNLOCK CURSOR FOR UI INTERACTION
        if (playerController != null)
        {
            playerController.SetCursorLocked(false);
        }
        
        // Start item inspection (with lerp animation)
        item.StartInspection(playerCamera);
        
        // Show inspection UI and set item name
        if (inspectionUI != null)
        {
            inspectionUI.SetActive(true);
            
            // Set the item name in the UI
            InspectionUIController uiController = inspectionUI.GetComponent<InspectionUIController>();
            if (uiController != null)
            {
                uiController.SetItemName(item.itemData.displayName);
            }
        }
        
        Debug.Log($"Started inspecting: {item.itemData.displayName}");
    }
    
    public void EndItemInspection()
    {
        if (currentlyInspecting != null)
        {
            currentlyInspecting.EndInspection();
            currentlyInspecting = null;
        }
        
        isInspectionMode = false;
        
        // Hide inspection UI
        if (inspectionUI != null)
            inspectionUI.SetActive(false);
            
        // RE-ENABLE PLAYER MOVEMENT
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
        
        // LOCK CURSOR BACK
        if (playerController != null)
        {
            playerController.SetCursorLocked(true);
        }
        
        Debug.Log("Ended item inspection");
    }
    
    public bool IsInspectionMode()
    {
        return isInspectionMode;
    }
}
