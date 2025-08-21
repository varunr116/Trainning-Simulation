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
    public Button downloadButton;
    
    [Header("Certificate Data")]
    public string courseName = "Warehouse Safety Training";
    public string learnerName = "Trainee";
    
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
        
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCertificate);
            
        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(DownloadCertificate);
            
#if UNITY_WEBGL
            downloadButton.gameObject.SetActive(false);
#endif
        }
    }
    
    public void ShowCertificate()
    {
        
        
        UpdateCertificateData();
        
        if (certificatePanel != null)
        {
            certificatePanel.SetActive(true);
            
        }
        else
        {
            
        }
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayNarration(GetCompletionAudio());
        
        CursorCameraController playerController = FindObjectOfType<CursorCameraController>();
        if (playerController != null)
            playerController.SetMovementEnabled(false);
    }
    
    void UpdateCertificateData()
    {
        if (learnerNameText != null)
            learnerNameText.text = learnerName;
            
        if (completionDateText != null)
            completionDateText.text = DateTime.Now.ToString("MMMM dd, yyyy");
            
        if (courseNameText != null)
            courseNameText.text = courseName;
            
        if (scoreText != null)
        {
            Scene2UIConnector connector = FindObjectOfType<Scene2UIConnector>();
            if (connector != null)
            {
                scoreText.text = $"Final Score: {connector.GetCorrectAnswers()}/3";
            }
            else if (QuizManager.Instance != null)
            {
                int score = QuizManager.Instance.GetScore();
                scoreText.text = $"Final Score: {score}/3";
            }
            else
            {
                scoreText.text = "Final Score: -/3";
            }
        }
    }
    
    void CloseCertificate()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("button_click");
        
        if (certificatePanel != null)
            certificatePanel.SetActive(false);
            
        CursorCameraController playerController = FindObjectOfType<CursorCameraController>();
        if (playerController != null)
            playerController.SetMovementEnabled(true);
    }
    
    void DownloadCertificate()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("button_click");
        
#if !UNITY_WEBGL
        StartCoroutine(CaptureAndSaveCertificate());
#endif
    }
    
    System.Collections.IEnumerator CaptureAndSaveCertificate()
    {
        yield return new WaitForEndOfFrame();
        
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        byte[] data = screenshot.EncodeToPNG();
        
        string filename = $"Certificate_{learnerName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        
        try
        {
            System.IO.File.WriteAllBytes(path, data);
           
            
            if (scoreText != null)
                scoreText.text += $"\nCertificate saved to:\n{path}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save certificate: {e.Message}");
        }
        
        Destroy(screenshot);
    }
    
    AudioClip GetCompletionAudio()
    {
        return null;
    }
    
    public void SetLearnerName(string name)
    {
        learnerName = name;
    }
}