using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;
    
    [Header("Inspection Setup")]
    public Transform inspectionPoint; // Where items go during inspection
    public GameObject inspectionUI; // UI with "Back to Tray" button
    
    [Header("Interaction Settings")]
    public LayerMask interactionLayers = -1;
    public float interactionRange = 5f;
    
    private Camera playerCamera;
    private InteractableItem currentlyInspecting;
    private InteractableItem hoveredItem;
    private bool isInspectionMode = false;
    
    // Mouse rotation for inspection
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
        
        if (isRotating && currentlyInspecting != null)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // Rotate item based on mouse movement
            currentlyInspecting.transform.Rotate(Vector3.up, -mouseDelta.x * 0.5f, Space.World);
            currentlyInspecting.transform.Rotate(Vector3.right, mouseDelta.y * 0.5f, Space.World);
            
            lastMousePosition = Input.mousePosition;
        }
    }
    
    public void OnItemHovered(InteractableItem item)
    {
        hoveredItem = item;
        // Show interaction prompt UI here if needed
    }
    
    public void OnItemUnhovered(InteractableItem item)
    {
        if (hoveredItem == item)
            hoveredItem = null;
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
        
        // Move item to inspection point
        item.StartInspection(inspectionPoint);
        
        // Show inspection UI
        if (inspectionUI != null)
            inspectionUI.SetActive(true);
            
        // Disable player movement during inspection
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
            playerController.SetMovementEnabled(false);
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
            
        // Re-enable player movement
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
            playerController.SetMovementEnabled(true);
    }
    
    public bool IsInspectionMode()
    {
        return isInspectionMode;
    }
}
