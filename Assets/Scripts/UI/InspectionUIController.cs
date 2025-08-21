using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectionUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI instructionText;
    public Button backButton;
    
    private InteractableItem currentItem;
    
    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        if (instructionText != null)
        {
            instructionText.text = "Click and drag to rotate • Right-click or ESC to return";
        }
    }
    
    void OnEnable()
    {
        UpdateItemInfo();
    }
    
    void UpdateItemInfo()
    {
        if (InteractionManager.Instance != null)
        {
            currentItem = InteractionManager.Instance.GetCurrentlyInspecting();
            
            if (currentItem != null && currentItem.itemData != null)
            {
                SetItemName(currentItem.itemData.displayName);
                SetInstructionText(currentItem.itemData.description);
            }
            else
            {
                SetItemName("Unknown Item");
                SetInstructionText("Click and drag to rotate");
            }
        }
    }
    
    void OnBackButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button_click");
        }
        
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.EndItemInspection();
        }
    }
    
    public void SetItemName(string itemName)
    {
        if (itemNameText != null)
        {
            itemNameText.text = $"Inspecting: {itemName}";
        }
    }
    
    public void SetInstructionText(string instruction)
    {
        if (instructionText != null)
        {
            if (string.IsNullOrEmpty(instruction))
            {
                instructionText.text = "Click and drag to rotate • Right-click or ESC to return";
            }
            else
            {
                instructionText.text = instruction;
            }
        }
    }
}