using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChecklistUI : MonoBehaviour
{
    [Header("Checklist Setup")]
    public GameObject checklistPanel;
    public Transform checklistContainer;
    public GameObject checklistItemPrefab;
    public Button proceedButton;
    
    [Header("Item Status")]
    public Sprite uncheckedSprite;
    public Sprite checkedSprite;
    
    private Dictionary<string, ChecklistItem> checklistItems = new Dictionary<string, ChecklistItem>();
    private int totalItems = 0;
    private int checkedItems = 0;
    
    [System.Serializable]
    public class ChecklistItem
    {
        public string itemID;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI statusText;
        public Image statusIcon;
        public bool isChecked = false;
    }
    
    void Start()
    {
        if (proceedButton != null)
        {
            proceedButton.interactable = false;
            proceedButton.onClick.AddListener(OnProceedToScene2);
        }
        
        // Delay setup to ensure ProgressService is ready
        Invoke("SetupChecklist", 0.1f);
    }
    
    void SetupChecklist()
    {
        if (ProgressService.Instance == null)
        {
            Debug.LogError("ProgressService not found! Make sure it's in the scene.");
            return;
        }
        
        // Get required items from progress service
        List<string> requiredItems = ProgressService.Instance.requiredItems;
        totalItems = requiredItems.Count;
        
        Debug.Log($"Setting up checklist with {totalItems} items");
        
        foreach (string itemID in requiredItems)
        {
            CreateChecklistItem(itemID);
        }
        
        UpdateProceedButton();
        
        // Subscribe to progress updates
        if (ProgressService.Instance != null)
        {
            // Check if any items are already inspected
            UpdateAllItemStatuses();
        }
    }
    
    void CreateChecklistItem(string itemID)
{
    if (checklistItemPrefab == null || checklistContainer == null) return;
    
    GameObject itemGO = Instantiate(checklistItemPrefab, checklistContainer);
    ChecklistItem item = new ChecklistItem();
    item.itemID = itemID;
    
    // Find components by exact names
    Transform itemNameTransform = itemGO.transform.Find("ItemName");
    Transform statusTransform = itemGO.transform.Find("Status");
    Transform statusIconTransform = itemGO.transform.Find("StatusIcon");
    
    if (itemNameTransform != null)
        item.nameText = itemNameTransform.GetComponent<TextMeshProUGUI>();
    if (statusTransform != null)
        item.statusText = statusTransform.GetComponent<TextMeshProUGUI>();
    if (statusIconTransform != null)
        item.statusIcon = statusIconTransform.GetComponent<Image>();
    
    // Set initial values
    if (item.nameText != null)
    {
        item.nameText.text = GetDisplayName(itemID);
        item.nameText.fontSize = Mathf.Round(item.nameText.fontSize); // Crisp text
    }
    
    if (item.statusText != null)
    {
        item.statusText.text = "Not Inspected";
        item.statusText.fontSize = Mathf.Round(item.statusText.fontSize); // Crisp text
    }
    
    if (item.statusIcon != null && uncheckedSprite != null)
        item.statusIcon.sprite = uncheckedSprite;
    
    checklistItems[itemID] = item;
    
    Debug.Log($"✅ Created checklist item: {itemID}");
}
    
    string GetDisplayName(string itemID)
    {
        switch (itemID.ToLower())
        {
            case "tape_gun": return "Tape Gun";
            case "barcode_scanner": return "Barcode Scanner";
            case "safety_gloves": return "Safety Gloves";
            case "safety_goggles": return "Safety Goggles";
            default: return itemID.Replace("_", " ");
        }
    }
    
    public void UpdateItemStatus(string itemID, bool inspected)
    {
        if (checklistItems.ContainsKey(itemID))
        {
            ChecklistItem item = checklistItems[itemID];
            
            if (!item.isChecked && inspected)
            {
                item.isChecked = true;
                
                if (item.statusText != null)
                    item.statusText.text = "Inspection Completed";
                if (item.statusIcon != null && checkedSprite != null)
                    item.statusIcon.sprite = checkedSprite;
                
                checkedItems++;
                UpdateProceedButton();
                
                // Play success sound
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("correct_answer");
                
                Debug.Log($"Updated checklist item: {itemID} - COMPLETED");
            }
        }
    }
    
    void UpdateAllItemStatuses()
    {
        foreach (var kvp in checklistItems)
        {
            bool isInspected = ProgressService.Instance.IsItemInspected(kvp.Key);
            if (isInspected)
            {
                UpdateItemStatus(kvp.Key, true);
            }
        }
    }
    
    void UpdateProceedButton()
    {
        bool allChecked = (checkedItems >= totalItems);
        
        if (proceedButton != null)
        {
            proceedButton.interactable = allChecked;
            
            TextMeshProUGUI buttonText = proceedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = allChecked ? "Proceed to Warehouse" : $"Complete Inspection ({checkedItems}/{totalItems})";
            }
        }
        
        Debug.Log($"Proceed button updated: {checkedItems}/{totalItems} - {(allChecked ? "ENABLED" : "DISABLED")}");
    }
    
    void OnProceedToScene2()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("button_click");
            
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene2();
    }
    
    void Update()
    {
        // Constantly check for updates (simple approach for now)
        if (ProgressService.Instance != null)
        {
            foreach (var kvp in checklistItems)
            {
                if (!kvp.Value.isChecked)
                {
                    bool isInspected = ProgressService.Instance.IsItemInspected(kvp.Key);
                    if (isInspected)
                    {
                        UpdateItemStatus(kvp.Key, true);
                    }
                }
            }
        }
    }
    
    public void ShowChecklist(bool show)
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(show);
    }
}