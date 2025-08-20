using UnityEngine;
using System.Collections.Generic;

public class CollectionZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneTag = "CollectionZone";
    public LayerMask itemLayers = -1;
    
    [Header("Visual Feedback")]
    public GameObject zoneBounds; // Visual indicator of collection area
    public Material highlightMaterial;
    public Material normalMaterial;
    
    private HashSet<string> collectedItemIDs = new HashSet<string>();
    private List<GameObject> itemsInZone = new List<GameObject>();
    private Renderer zoneRenderer;
    
    void Start()
    {
        zoneRenderer = zoneBounds?.GetComponent<Renderer>();
        
        // Set initial material
        if (zoneRenderer != null && normalMaterial != null)
            zoneRenderer.material = normalMaterial;
    }
    
    void OnTriggerEnter(Collider other)
    {
        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && item.isCollectable && !item.IsCollected())
        {
            CollectItem(item);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Optional: Handle item removal from zone
        if (itemsInZone.Contains(other.gameObject))
        {
            itemsInZone.Remove(other.gameObject);
            UpdateZoneVisual();
        }
    }
    
    void CollectItem(InteractableItem item)
    {
        if (!collectedItemIDs.Contains(item.itemData.itemID))
        {
            // Mark as collected
            item.CollectItem();
            collectedItemIDs.Add(item.itemData.itemID);
            itemsInZone.Add(item.gameObject);
            
            // Visual feedback
            UpdateZoneVisual();
            
            // Audio feedback
            AudioManager.Instance.PlaySFX("item_collected");
            AudioManager.Instance.PlayNarration(GetCollectionAudio());
            
            Debug.Log($"Collected: {item.itemData.displayName}");
            
            // Check if all items collected
            CheckCollectionComplete();
        }
    }
    
    void UpdateZoneVisual()
    {
        if (zoneRenderer != null)
        {
            bool hasItems = itemsInZone.Count > 0;
            zoneRenderer.material = hasItems ? highlightMaterial : normalMaterial;
        }
    }
    
    void CheckCollectionComplete()
    {
        int totalRequired = ProgressService.Instance.requiredItems.Count;
        int collected = collectedItemIDs.Count;
        
        Debug.Log($"Collection Progress: {collected}/{totalRequired}");
        
        if (collected >= totalRequired)
        {
            OnAllItemsCollected();
        }
    }
    
    void OnAllItemsCollected()
    {
        Debug.Log("All items collected successfully!");
        
        // Stop timer
        TimerSystem.Instance.StopTimer();
        
        // Play success audio
        AudioManager.Instance.PlayNarration(GetSuccessAudio());
        
        // Start quiz after audio
        StartCoroutine(DelayedQuizStart());
    }
    
    System.Collections.IEnumerator DelayedQuizStart()
    {
        yield return new WaitForSeconds(3f); // Wait for audio
        QuizManager.Instance.StartQuiz();
    }
    
    AudioClip GetCollectionAudio()
    {
        // Return appropriate collection audio clip
        return null; // Implement with your audio clips
    }
    
    AudioClip GetSuccessAudio()
    {
        // Return success audio clip
        return null; // Implement with your audio clips
    }
    
    public int GetCollectedCount()
    {
        return collectedItemIDs.Count;
    }
    
    public bool IsItemCollected(string itemID)
    {
        return collectedItemIDs.Contains(itemID);
    }
}