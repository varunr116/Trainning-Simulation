using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPickupController : MonoBehaviour
{
    [Header("Settings")]
    public float pickupRange = 2f;
    public float dropRange = 3f;
    public Vector3 holdOffset = new Vector3(0, -0.5f, 1.5f);
    public float moveSpeed = 5f;
    
    [Header("UI References")]
    public GameObject pickupPrompt;
    public GameObject dropPrompt;
    public TextMeshProUGUI pickupText;
    public TextMeshProUGUI dropText;
    
    private Camera playerCamera;
    private InteractableItem currentNearbyItem;
    private InteractableItem heldItem;
    private CollectionZone nearbyCollectionZone;
    private bool isCarryingItem = false;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
            
        SetupUI();
    }
    
    void SetupUI()
    {
        if (pickupPrompt != null) pickupPrompt.SetActive(false);
        if (dropPrompt != null) dropPrompt.SetActive(false);
    }
    
    void Update()
    {
        if (!isCarryingItem)
        {
            CheckForNearbyItems();
            HandlePickupInput();
        }
        else
        {
            UpdateHeldItemPosition();
            CheckForCollectionZone();
            HandleDropInput();
        }
    }
    
    void CheckForNearbyItems()
    {
        InteractableItem closestItem = null;
        float closestDistance = pickupRange;
        
        InteractableItem[] allItems = FindObjectsOfType<InteractableItem>();
        
        foreach (InteractableItem item in allItems)
        {
            if (item.isCollectable && !item.IsCollected() && item.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(playerCamera.transform.position, item.transform.position);
                
                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }
        
        if (closestItem != currentNearbyItem)
        {
            currentNearbyItem = closestItem;
            UpdatePickupUI();
        }
    }
    
    void UpdatePickupUI()
    {
        if (currentNearbyItem != null)
        {
            if (pickupPrompt != null) pickupPrompt.SetActive(true);
            if (pickupText != null) 
                pickupText.text = $"Press P to pick up {currentNearbyItem.itemData.displayName}";
        }
        else
        {
            if (pickupPrompt != null) pickupPrompt.SetActive(false);
        }
    }
    
    void HandlePickupInput()
    {
        if (Input.GetKeyDown(KeyCode.P) && currentNearbyItem != null)
        {
            PickupItem(currentNearbyItem);
        }
    }
    
    void PickupItem(InteractableItem item)
    {
        heldItem = item;
        isCarryingItem = true;
        
        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null) itemCollider.enabled = false;
        
        if (Scene2AudioManager.Instance != null)
        {
            Scene2AudioManager.Instance.OnItemPickedUp(item.itemData.itemID);
        }
        
        if (pickupPrompt != null) pickupPrompt.SetActive(false);
    }
    
    void UpdateHeldItemPosition()
    {
        if (heldItem == null) return;
        
        Vector3 targetPosition = playerCamera.transform.position + 
                                playerCamera.transform.TransformDirection(holdOffset);
        
        heldItem.transform.position = Vector3.Lerp(
            heldItem.transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
        
        heldItem.transform.LookAt(playerCamera.transform);
    }
    
    void CheckForCollectionZone()
    {
        CollectionZone closestZone = null;
        float closestDistance = dropRange;
        
        CollectionZone[] allZones = FindObjectsOfType<CollectionZone>();
        
        foreach (CollectionZone zone in allZones)
        {
            float distance = Vector3.Distance(playerCamera.transform.position, zone.transform.position);
            
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestZone = zone;
            }
        }
        
        if (closestZone != nearbyCollectionZone)
        {
            nearbyCollectionZone = closestZone;
            UpdateDropUI();
        }
    }
    
    void UpdateDropUI()
    {
        if (nearbyCollectionZone != null && isCarryingItem)
        {
            if (dropPrompt != null) dropPrompt.SetActive(true);
            if (dropText != null) 
                dropText.text = $"Press D to drop {heldItem.itemData.displayName}";
        }
        else
        {
            if (dropPrompt != null) dropPrompt.SetActive(false);
        }
    }
    
    void HandleDropInput()
    {
        if (Input.GetKeyDown(KeyCode.D) && nearbyCollectionZone != null)
        {
            DropItem();
        }
    }
    
    void DropItem()
    {
        if (heldItem == null) return;
        
        Collider itemCollider = heldItem.GetComponent<Collider>();
        if (itemCollider != null) itemCollider.enabled = true;
        
        Vector3 dropPosition = CalculateDropPosition();
        heldItem.transform.position = dropPosition;
        heldItem.transform.rotation = Quaternion.identity;
        
        ProgressService.Instance.MarkItemCollected(heldItem.itemData.itemID);
        
        if (Scene2AudioManager.Instance != null)
        {
            Scene2AudioManager.Instance.OnItemDropped(heldItem.itemData.itemID);
        }
        
        CheckAllItemsCollected();
        
        heldItem = null;
        isCarryingItem = false;
        
        if (dropPrompt != null) dropPrompt.SetActive(false);
    }
    
    Vector3 CalculateDropPosition()
    {
        Vector3 basePosition = nearbyCollectionZone.transform.position;
        
        int collectedCount = ProgressService.Instance.GetCollectedCount();
        float spacing = 1.2f;
        
        switch (collectedCount)
        {
            case 0:
                return basePosition + new Vector3(-1.5f, 0.5f, 0);
            case 1:
                return basePosition + new Vector3(-0.5f, 0.5f, 0);
            case 2:
                return basePosition + new Vector3(0.5f, 0.5f, 0);
            case 3:
                return basePosition + new Vector3(1.5f, 0.5f, 0);
            default:
                return basePosition + new Vector3(collectedCount * spacing, 0.5f, 0);
        }
    }
    
    void CheckAllItemsCollected()
    {
        if (ProgressService.Instance != null)
        {
            int collected = ProgressService.Instance.GetCollectedCount();
            int total = ProgressService.Instance.requiredItems.Count;
            
            if (collected >= total)
            {
                if (TimerSystem.Instance != null)
                {
                    TimerSystem.Instance.StopTimer();
                }
                
                StartCoroutine(DelayedQuizStart());
            }
        }
    }
    
    System.Collections.IEnumerator DelayedQuizStart()
    {
        yield return new WaitForSeconds(3f);
        
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.StartQuiz();
        }
    }
    
    public bool IsCarryingItem() => isCarryingItem;
    public InteractableItem GetHeldItem() => heldItem;
}

