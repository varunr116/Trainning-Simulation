using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Progress Bar")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    
    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public GameObject timerPanel;
    
    [Header("Interaction Prompts")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    [Header("Scene Labels")]
    public TextMeshProUGUI sceneLabel;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Initialize UI
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        if (timerPanel != null)
            timerPanel.SetActive(false);
            
        UpdateProgressBar(0f);
    }
    
    public void UpdateProgressBar(float progress)
    {
        progress = Mathf.Clamp01(progress);
        
        if (progressBar != null)
            progressBar.value = progress;
            
        if (progressText != null)
            progressText.text = $"Progress: {Mathf.RoundToInt(progress * 100)}%";
    }
    
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Change color when time is low
            if (timeRemaining <= 30f)
                timerText.color = Color.red;
            else if (timeRemaining <= 60f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }
    }
    
    public void ShowTimer(bool show)
    {
        if (timerPanel != null)
            timerPanel.SetActive(show);
    }
    
    public void ShowInteractionPrompt(string message)
    {
        if (interactionPrompt != null && promptText != null)
        {
            promptText.text = message;
            interactionPrompt.SetActive(true);
        }
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    public void SetSceneLabel(string sceneName)
    {
        if (sceneLabel != null)
            sceneLabel.text = sceneName;
    }
}