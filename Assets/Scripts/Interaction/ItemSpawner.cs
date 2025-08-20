using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints = new List<Transform>();
    public List<GameObject> itemPrefabs = new List<GameObject>();
    
    [Header("Randomization")]
    public bool randomizeOnStart = true;
    public float itemSpacing = 1f;
    
    private List<GameObject> spawnedItems = new List<GameObject>();
    
    void Start()
    {
        if (randomizeOnStart)
        {
            SpawnItems();
        }
    }
    
    public void SpawnItems()
    {
        // Clear any existing items
        ClearSpawnedItems();
        
        if (spawnPoints.Count < itemPrefabs.Count)
        {
            Debug.LogWarning("Not enough spawn points for all items!");
            return;
        }
        
        // Create a shuffled list of spawn points
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        ShuffleList(availableSpawnPoints);
        
        // Spawn each item at a random point
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (i < availableSpawnPoints.Count)
            {
                GameObject spawnedItem = Instantiate(itemPrefabs[i], availableSpawnPoints[i].position, availableSpawnPoints[i].rotation);
                
                // Set up for collection
                InteractableItem itemComponent = spawnedItem.GetComponent<InteractableItem>();
                if (itemComponent != null)
                {
                    itemComponent.isCollectable = true;
                    itemComponent.isInspectable = false; // Can't inspect in warehouse
                }
                
                spawnedItems.Add(spawnedItem);
                
                Debug.Log($"Spawned {itemPrefabs[i].name} at {availableSpawnPoints[i].name}");
            }
        }
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    void ClearSpawnedItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        spawnedItems.Clear();
    }
    
    public int GetTotalItemsToCollect()
    {
        return itemPrefabs.Count;
    }
    
    public List<GameObject> GetSpawnedItems()
    {
        return new List<GameObject>(spawnedItems);
    }
}