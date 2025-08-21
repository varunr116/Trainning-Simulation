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
    
    [Header("Proceed Settings")]
    public int minimumItemsToUnlock = 1;
    
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
        
        Invoke("SetupChecklist", 0.1f);
    }
    
    void SetupChecklist()
    {
        if (ProgressService.Instance == null)
        {
            
            return;
        }
        
        List<string> requiredItems = ProgressService.Instance.requiredItems;
        totalItems = requiredItems.Count;
        
        foreach (string itemID in requiredItems)
        {
            CreateChecklistItem(itemID);
        }
        
        UpdateProceedButton();
        UpdateAllItemStatuses();
    }
    
    void CreateChecklistItem(string itemID)
    {
        if (checklistItemPrefab == null || checklistContainer == null) return;
        
        GameObject itemGO = Instantiate(checklistItemPrefab, checklistContainer);
        ChecklistItem item = new ChecklistItem();
        item.itemID = itemID;
        
        Transform itemNameTransform = itemGO.transform.Find("ItemName");
        Transform statusTransform = itemGO.transform.Find("Status");
        Transform statusIconTransform = itemGO.transform.Find("StatusIcon");
        
        if (itemNameTransform != null)
            item.nameText = itemNameTransform.GetComponent<TextMeshProUGUI>();
        if (statusTransform != null)
            item.statusText = statusTransform.GetComponent<TextMeshProUGUI>();
        if (statusIconTransform != null)
            item.statusIcon = statusIconTransform.GetComponent<Image>();
        
        if (item.nameText != null)
        {
            item.nameText.text = GetDisplayName(itemID);
            item.nameText.fontSize = Mathf.Round(item.nameText.fontSize);
        }
        
        if (item.statusText != null)
        {
            item.statusText.text = "Not Inspected";
            item.statusText.fontSize = Mathf.Round(item.statusText.fontSize);
        }
        
        if (item.statusIcon != null && uncheckedSprite != null)
            item.statusIcon.sprite = uncheckedSprite;
        
        checklistItems[itemID] = item;
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
                
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("correct_answer");
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
        bool canProceed = (checkedItems >= minimumItemsToUnlock);
        
        if (proceedButton != null)
        {
            proceedButton.interactable = canProceed;
            
            TextMeshProUGUI buttonText = proceedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (checkedItems >= totalItems)
                {
                    buttonText.text = "Proceed to Warehouse";
                }
                else if (canProceed)
                {
                    buttonText.text = $"Proceed to Warehouse ({checkedItems}/{totalItems} inspected)";
                }
                else
                {
                    buttonText.text = $"Inspect at least {minimumItemsToUnlock} item to proceed";
                }
            }
        }
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