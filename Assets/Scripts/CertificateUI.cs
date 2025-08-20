using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CertificateUI : MonoBehaviour
{
    public static CertificateUI Instance;
    
    [Header("Certificate UI")]
    public GameObject certificatePanel;
    public TextMeshProUGUI learnerNameText;
    public TextMeshProUGUI completionDateText;
    public TextMeshProUGUI courseNameText;
    public TextMeshProUGUI scoreText;
    public Button closeButton;
    public Button downloadButton; // Desktop only
    
    [Header("Certificate Data")]
    public string courseName = "Warehouse Safety Training";
    public string learnerName = "Trainee"; // Could be set from LMS
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (certificatePanel != null)
            certificatePanel.SetActive(false);
            
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCertificate);
            
        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(DownloadCertificate);
            
            // Only show download on desktop builds
#if UNITY_WEBGL
            downloadButton.gameObject.SetActive(false);
#endif
        }
    }
    
    public void ShowCertificate()
    {
        // Populate certificate data
        UpdateCertificateData();
        
        // Show certificate panel
        if (certificatePanel != null)
            certificatePanel.SetActive(true);
            
        // Play completion audio
        AudioManager.Instance.PlayNarration(GetCompletionAudio());
        
        // Disable player movement during certificate display
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
            playerController.SetMovementEnabled(false);
    }
    
    void UpdateCertificateData()
    {
        // Set learner name
        if (learnerNameText != null)
            learnerNameText.text = learnerName;
            
        // Set completion date
        if (completionDateText != null)
            completionDateText.text = DateTime.Now.ToString("MMMM dd, yyyy");
            
        // Set course name
        if (courseNameText != null)
            courseNameText.text = courseName;
            
        // Set score information
        if (scoreText != null && QuizManager.Instance != null)
        {
            int score = QuizManager.Instance.GetScore();
            scoreText.text = $"Final Score: {score}/3";
        }
    }
    
    void CloseCertificate()
    {
        AudioManager.Instance.PlaySFX("button_click");
        
        if (certificatePanel != null)
            certificatePanel.SetActive(false);
            
        // Re-enable player movement
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
            playerController.SetMovementEnabled(true);
    }
    
    void DownloadCertificate()
    {
        AudioManager.Instance.PlaySFX("button_click");
        
#if !UNITY_WEBGL
        // Desktop only - take screenshot of certificate
        StartCoroutine(CaptureAndSaveCertificate());
#endif
    }
    
    System.Collections.IEnumerator CaptureAndSaveCertificate()
    {
        // Wait for end of frame to capture
        yield return new WaitForEndOfFrame();
        
        // Create screenshot
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        // Convert to bytes
        byte[] data = screenshot.EncodeToPNG();
        
        // Save to file
        string filename = $"Certificate_{learnerName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        
        try
        {
            System.IO.File.WriteAllBytes(path, data);
            Debug.Log($"Certificate saved to: {path}");
            
            // Show confirmation (could be a popup)
            if (scoreText != null)
                scoreText.text += $"\nCertificate saved to:\n{path}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save certificate: {e.Message}");
        }
        
        // Clean up
        Destroy(screenshot);
    }
    
    AudioClip GetCompletionAudio()
    {
        // Return completion audio clip
        return null; // Implement with your actual audio clips
    }
    
    public void SetLearnerName(string name)
    {
        learnerName = name;
    }
}
