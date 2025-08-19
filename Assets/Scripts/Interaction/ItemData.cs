using UnityEngine;

// Data structure for item information
[CreateAssetMenu(fileName = "ItemData", menuName = "Training/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemID;
    public string displayName;
    public string description;
    
    [Header("Audio")]
    public AudioClip narrationClip;
    
    [Header("Inspection")]
    public Vector3 inspectionPosition = new Vector3(0, 0, 2);
    public Vector3 inspectionRotation = Vector3.zero;
}
