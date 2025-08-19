using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Item Setup")]
    public ItemData itemData;
    public bool isInspectable = true;
    public bool isCollectable = false;
    
    [Header("Visual Effects")]
    public GameObject outlineEffect; // Simple outline GameObject
    public Material highlightMaterial;
    
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isInspecting = false;
    private bool isCollected = false;
    
    // Original transform data
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    
    // Components
    private Renderer itemRenderer;
    private Material originalMaterial;
    private Collider itemCollider;
    
    void Start()
    {
        // Store original transform
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
        
        // Get components
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null)
            originalMaterial = itemRenderer.material;
            
        itemCollider = GetComponent<Collider>();
        
        // Ensure outline is disabled initially
        if (outlineEffect != null)
            outlineEffect.SetActive(false);
    }
    
    void OnMouseEnter()
    {
        if (!isHovered && !isInspecting && !isCollected)
        {
            SetHovered(true);
            InteractionManager.Instance.OnItemHovered(this);
        }
    }
    
    void OnMouseExit()
    {
        if (isHovered && !isSelected && !isInspecting)
        {
            SetHovered(false);
            InteractionManager.Instance.OnItemUnhovered(this);
        }
    }
    
    void OnMouseDown()
    {
        if (isHovered && !isInspecting && !isCollected)
        {
            InteractionManager.Instance.OnItemClicked(this);
        }
    }
    
    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        
        // Show/hide outline effect
        if (outlineEffect != null)
            outlineEffect.SetActive(hovered);
            
        // Alternative: Change material
        if (highlightMaterial != null && itemRenderer != null)
        {
            itemRenderer.material = hovered ? highlightMaterial : originalMaterial;
        }
    }
    
    public void StartInspection(Transform inspectionParent)
    {
        isInspecting = true;
        isSelected = true;
        SetHovered(false);
        
        // Move to inspection position
        transform.SetParent(inspectionParent);
        transform.localPosition = itemData.inspectionPosition;
        transform.localRotation = Quaternion.Euler(itemData.inspectionRotation);
        
        // Disable collider during inspection
        if (itemCollider != null)
            itemCollider.enabled = false;
            
        // Play narration
        if (itemData.narrationClip != null)
            AudioManager.Instance.PlayNarration(itemData.narrationClip);
    }
    
    public void EndInspection()
    {
        isInspecting = false;
        isSelected = false;
        
        // Return to original position
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        // Re-enable collider
        if (itemCollider != null)
            itemCollider.enabled = true;
            
        // Mark as inspected
        ProgressService.Instance.MarkItemInspected(itemData.itemID);
    }
    
    public void CollectItem()
    {
        if (!isCollectable) return;
        
        isCollected = true;
        SetHovered(false);
        
        // Hide the item
        gameObject.SetActive(false);
        
        // Notify systems
        ProgressService.Instance.MarkItemCollected(itemData.itemID);
        AudioManager.Instance.PlaySFX("item_collected");
    }
    
    public bool IsInspected()
    {
        return ProgressService.Instance.IsItemInspected(itemData.itemID);
    }
    
    public bool IsCollected()
    {
        return isCollected;
    }
}

