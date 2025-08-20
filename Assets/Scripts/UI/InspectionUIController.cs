using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectionUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI instructionText;
    public Button backButton;
    
    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        // Set default instruction text
        if (instructionText != null)
        {
            instructionText.text = "Drag mouse to rotate • Right-click or ESC to return";
        }
    }
    
    void OnEnable()
    {
        // This runs every time the UI is shown
        UpdateItemName();
    }
    
    void UpdateItemName()
    {
        if (itemNameText != null)
        {
            // Try to get the current item being inspected
            if (InteractionManager.Instance != null)
            {
                // For now, we'll set a default - this can be improved later
                itemNameText.text = "Inspecting Item";
            }
        }
    }
    
    void OnBackButtonClicked()
    {
        // Play button sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button_click");
        }
        
        // Tell InteractionManager to end inspection
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.EndItemInspection();
        }
    }
    
    // Public method to set item name (called from InteractionManager)
    public void SetItemName(string itemName)
    {
        if (itemNameText != null)
        {
            itemNameText.text = $"Inspecting: {itemName}";
        }
    }
}