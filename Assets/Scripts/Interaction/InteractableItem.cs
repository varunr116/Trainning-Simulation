using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractableItem : MonoBehaviour
{[Header("Item Setup")]
    public ItemData itemData;
    public bool isInspectable = true;
    public bool isCollectable = false;
    
    [Header("Inspection Settings")]
    public float inspectionDistance = 1.5f;
    public Vector3 inspectionOffset = Vector3.zero;
    public float lerpSpeed = 2f;
    
    [Header("Multi-Mesh Support")]
    public bool includeChildrenMeshes = true;
    
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isInspecting = false;
    private bool isCollected = false;
    private bool isLerping = false;
    
    // Transform storage
    private Vector3 originalLocalPosition;
    private Vector3 originalWorldPosition;
    private Quaternion originalLocalRotation;
    private Quaternion originalWorldRotation;
    private Vector3 originalLocalScale;
    private Transform originalParent;
    
    // Inspection positions
    private Vector3 targetInspectionPosition;
    private Quaternion targetInspectionRotation;
    
    // SINGLE SOURCE OF TRUTH for multi-mesh data
    private List<Renderer> allRenderers = new List<Renderer>();
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    private Dictionary<Renderer, Material[]> originalMaterialArrays = new Dictionary<Renderer, Material[]>();
    
    // Components
    private Collider itemCollider;
    private ItemGlowController glowController;
    
     void Start()
    {
        StoreOriginalTransform();
        SetupRenderersAndMaterials(); // ONLY here!
        SetupGlowController();
        SetupCollider();
    }
    
    void StoreOriginalTransform()
    {
        originalLocalPosition = transform.localPosition;
        originalWorldPosition = transform.position;
        originalLocalRotation = transform.localRotation;
        originalWorldRotation = transform.rotation;
        originalLocalScale = transform.localScale;
        originalParent = transform.parent;
    }
    
    void SetupRenderersAndMaterials()
    {
        allRenderers.Clear();
        originalMaterials.Clear();
        originalMaterialArrays.Clear();
        
        if (includeChildrenMeshes)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            allRenderers.AddRange(renderers);
        }
        else
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                allRenderers.Add(renderer);
        }
        
        // Store original materials
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                originalMaterials[renderer] = renderer.material;
                originalMaterialArrays[renderer] = (Material[])renderer.materials.Clone();
            }
        }
        
        Debug.Log($"InteractableItem: Found {allRenderers.Count} renderers for {name}");
    }
    
    void SetupGlowController()
    {
        glowController = GetComponent<ItemGlowController>();
        if (glowController == null)
            glowController = gameObject.AddComponent<ItemGlowController>();
            
        // PASS renderer data to glow controller
        glowController.SetTargetRenderers(allRenderers);
    }
    
  void SetupCollider()
    {
        itemCollider = GetComponent<Collider>();
        if (itemCollider == null)
            itemCollider = GetComponentInChildren<Collider>();
    }
    void Update()
    {
        if (isLerping)
        {
            HandleInspectionLerp();
        }
    }
    
    void HandleInspectionLerp()
    {
        transform.position = Vector3.Lerp(transform.position, targetInspectionPosition, lerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetInspectionRotation, lerpSpeed * Time.deltaTime);
        
        float distance = Vector3.Distance(transform.position, targetInspectionPosition);
        if (distance < 0.01f)
        {
            transform.position = targetInspectionPosition;
            transform.rotation = targetInspectionRotation;
            isLerping = false;
            isInspecting = true;
            
            Debug.Log($"Item {itemData.displayName} reached inspection position");
        }
    }
    

    void OnMouseEnter()
    {
        if (!isHovered && !isInspecting && !isCollected && !isLerping)
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
        if (isHovered && !isInspecting && !isCollected && !isLerping)
        {
            InteractionManager.Instance.OnItemClicked(this);
        }
    }
    
  
    
    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        
        if (glowController != null)
            glowController.SetHovered(hovered);
    }
    
   public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (glowController != null)
            glowController.SetSelected(selected);
    }
    
    public void StartInspection(Camera playerCamera)
    {
        isSelected = true;
        isLerping = true;
        isInspecting = false;
        SetHovered(false);
        SetSelected(false); // Activate selection glow
        
        // Calculate target inspection position
        Vector3 cameraForward = playerCamera.transform.forward;
        targetInspectionPosition = playerCamera.transform.position + (cameraForward * inspectionDistance) + inspectionOffset;
        targetInspectionRotation = playerCamera.transform.rotation;
        
        // Remove from parent hierarchy during inspection
        transform.SetParent(null);
        
        // Disable colliders during inspection
        DisableColliders();
        
        // Play narration
        if (itemData != null && itemData.narrationClip != null)
            AudioManager.Instance.PlayNarration(itemData.narrationClip);
            
        Debug.Log($"Item {itemData.displayName} starting inspection");
    }
    
    public void EndInspection()
    {
        SetSelected(false); // Deactivate selection glow
        StartCoroutine(ReturnToOriginalPosition());
    }
    
    IEnumerator ReturnToOriginalPosition()
    {
        isInspecting = false;
        isLerping = true;
        
        Vector3 targetPosition;
        Quaternion targetRotation;
        
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            targetPosition = originalParent.TransformPoint(originalLocalPosition);
            targetRotation = originalParent.rotation * originalLocalRotation;
        }
        else
        {
            targetPosition = originalWorldPosition;
            targetRotation = originalWorldRotation;
        }
        
        // Smooth lerp back to original position
        float lerpTimer = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        while (lerpTimer < 1f)
        {
            lerpTimer += lerpSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, lerpTimer);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, lerpTimer);
            yield return null;
        }
        
        // Final snap to exact position
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
            transform.localScale = originalLocalScale;
        }
        else
        {
            transform.position = originalWorldPosition;
            transform.rotation = originalWorldRotation;
        }
        
        isLerping = false;
        isSelected = false;
        
        // Re-enable colliders
        EnableColliders();
        
        // Mark as inspected
        ProgressService.Instance.MarkItemInspected(itemData.itemID);
        
        Debug.Log($"Item {itemData.displayName} returned to original position");
    }
    
 
    
    void DisableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        Debug.Log($"Disabled {colliders.Length} colliders");
    }
    
    void EnableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        Debug.Log($"Enabled {colliders.Length} colliders");
    }
    

    
    public void RotateItem(Vector2 mouseDelta)
    {
        if (isInspecting && !isLerping)
        {
            transform.Rotate(Vector3.up, -mouseDelta.x * 0.5f, Space.World);
            transform.Rotate(Vector3.right, mouseDelta.y * 0.5f, Space.World);
        }
    }
    
    public void CollectItem()
    {
        if (!isCollectable) return;
        
        isCollected = true;
        SetHovered(false);
        SetSelected(false);
        
        // Reset glow before hiding
        if (glowController != null)
        {
            glowController.ResetToOriginal();
        }
        
        gameObject.SetActive(false);
        
        ProgressService.Instance.MarkItemCollected(itemData.itemID);
        AudioManager.Instance.PlaySFX("item_collected");
    }
    
    
    public void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            Renderer renderer = kvp.Key;
            Material originalMaterial = kvp.Value;
            
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
        }
        
        foreach (var kvp in originalMaterialArrays)
        {
            Renderer renderer = kvp.Key;
            Material[] originalMaterials = kvp.Value;
            
            if (renderer != null && originalMaterials != null)
            {
                renderer.materials = originalMaterials;
            }
        }
    }
    
    public bool IsInspected()
    {
        return ProgressService.Instance.IsItemInspected(itemData.itemID);
    }
    
    public bool IsCollected()
    {
        return isCollected;
    }
    
    public bool IsCurrentlyInspecting()
    {
        return isInspecting;
    }
    
    public bool IsLerping()
    {
        return isLerping;
    }
    
    public List<Renderer> GetAllRenderers()
    {
        return new List<Renderer>(allRenderers);
    }
    
}