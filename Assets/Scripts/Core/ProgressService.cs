using UnityEngine;

using System.Collections.Generic;

public class ProgressService : MonoBehaviour
{
    public static ProgressService Instance;
    
    [Header("Progress Tracking")]
    public List<string> requiredItems = new List<string>();
    
    private HashSet<string> inspectedItems = new HashSet<string>();
    private HashSet<string> collectedItems = new HashSet<string>();
    private int quizScore = 0;
    private bool scene1Complete = false;
    private bool scene2Complete = false;
    
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
    
    public void MarkItemInspected(string itemID)
    {
        if (!inspectedItems.Contains(itemID))
        {
            inspectedItems.Add(itemID);
            UpdateProgress();
            
            // Check if all items inspected
            if (inspectedItems.Count >= requiredItems.Count)
            {
                OnAllItemsInspected();
            }
        }
    }
    
    public void MarkItemCollected(string itemID)
    {
        if (!collectedItems.Contains(itemID))
        {
            collectedItems.Add(itemID);
            UpdateProgress();
            
            // Check if all items collected
            if (collectedItems.Count >= requiredItems.Count)
            {
                OnAllItemsCollected();
            }
        }
    }
    
    public void SetQuizScore(int score)
    {
        quizScore = score;
        UpdateProgress();
        
        if (score >= 2) // Pass threshold
        {
            OnCourseComplete();
        }
    }
    
    void OnAllItemsInspected()
    {
        scene1Complete = true;
        Debug.Log("Scene 1 Complete - All items inspected!");
        
        // Enable scene transition
        SceneTransitionManager.Instance.EnableScene2Transition();
    }
    
    void OnAllItemsCollected()
    {
        scene2Complete = true;
        Debug.Log("Scene 2 Complete - All items collected!");
        
        // Start quiz
        QuizManager.Instance.StartQuiz();
    }
    
    void OnCourseComplete()
    {
        Debug.Log("Course Complete!");
        
        // Show certificate
        CertificateUI.Instance.ShowCertificate();
        
        // Report to LMS
        if (LMSManager.Instance != null)
        {
            LMSManager.Instance.ReportCompletion();
        }
    }
    
    void UpdateProgress()
    {
        float totalProgress = CalculateProgress();
        
        // Update progress bar
        UIManager.Instance.UpdateProgressBar(totalProgress);
        
        // Report to LMS
        if (LMSManager.Instance != null)
        {
            LMSManager.Instance.ReportProgress(totalProgress);
        }
    }
    
    float CalculateProgress()
    {
        float inspectionProgress = (float)inspectedItems.Count / requiredItems.Count * 0.4f; // 40%
        float collectionProgress = (float)collectedItems.Count / requiredItems.Count * 0.4f; // 40%
        float quizProgress = (float)quizScore / 3f * 0.2f; // 20%
        
        return inspectionProgress + collectionProgress + quizProgress;
    }
    
    // Public getters
    public bool IsItemInspected(string itemID) => inspectedItems.Contains(itemID);
    public bool IsItemCollected(string itemID) => collectedItems.Contains(itemID);
    public bool IsScene1Complete() => scene1Complete;
    public bool IsScene2Complete() => scene2Complete;
    public int GetInspectedCount() => inspectedItems.Count;
    public int GetCollectedCount() => collectedItems.Count;
    public float GetProgress() => CalculateProgress();
}