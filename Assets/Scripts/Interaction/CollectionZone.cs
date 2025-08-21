using UnityEngine;
using System.Collections.Generic;
public class CollectionZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public Material highlightMaterial;
    public Material normalMaterial;

    private HashSet<string> collectedItemIDs = new HashSet<string>();
    private Renderer zoneRenderer;

    void Start()
    {
        zoneRenderer = GetComponent<Renderer>();
        if (zoneRenderer != null && normalMaterial != null)
            zoneRenderer.material = normalMaterial;
    }

    void Update()
    {
        UpdateZoneVisual();
    }

    void UpdateZoneVisual()
    {
        ItemPickupController pickup = FindObjectOfType<ItemPickupController>();
        if (pickup != null && pickup.IsCarryingItem())
        {
            float distance = Vector3.Distance(transform.position, pickup.transform.position);
            bool playerNearby = distance <= 3f;

            if (zoneRenderer != null)
            {
                if (playerNearby && highlightMaterial != null)
                    zoneRenderer.material = highlightMaterial;
                else if (normalMaterial != null)
                    zoneRenderer.material = normalMaterial;
            }
        }
        else
        {
            if (zoneRenderer != null && normalMaterial != null)
                zoneRenderer.material = normalMaterial;
        }
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