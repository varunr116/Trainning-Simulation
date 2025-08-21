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
            
            SCORMClient scorm = FindObjectOfType<SCORMClient>();
            if (scorm != null)
            {
                scorm.TrackItemInspection(itemID);
            }
            
            UpdateProgress();
            
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
            
            SCORMClient scorm = FindObjectOfType<SCORMClient>();
            if (scorm != null)
            {
                scorm.TrackItemCollection(itemID);
            }
            
            UpdateProgress();
            
            if (collectedItems.Count >= requiredItems.Count)
            {
                OnAllItemsCollected();
            }
        }
    }
    
    public void RecordQuizAnswer(int questionIndex, int selectedAnswer, bool isCorrect)
    {
        SCORMClient scorm = FindObjectOfType<SCORMClient>();
        if (scorm != null)
        {
            scorm.TrackQuizAnswer(questionIndex, selectedAnswer, isCorrect);
        }
    }
    
    public void SetQuizScore(int score)
    {
        quizScore = score;
        UpdateProgress();
        
        if (score >= 2)
        {
            OnCourseComplete();
        }
    }
    
    void OnAllItemsInspected()
    {
        scene1Complete = true;
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.EnableScene2Transition();
        }
    }
    
    void OnAllItemsCollected()
    {
        scene2Complete = true;
        
        if (TimerSystem.Instance != null)
        {
            TimerSystem.Instance.StopTimer();
        }
        
        StartQuiz();
    }
    
    void StartQuiz()
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.StartQuiz();
            return;
        }
        
        QuizManager quizManager = FindObjectOfType<QuizManager>();
        if (quizManager != null)
        {
            quizManager.StartQuiz();
            return;
        }
        
        Scene2UIConnector connector = FindObjectOfType<Scene2UIConnector>();
        if (connector != null)
        {
            connector.StartQuizDirectly();
            return;
        }
    }
    
    void OnCourseComplete()
    {
        if (CertificateUI.Instance != null)
        {
            CertificateUI.Instance.ShowCertificate();
        }
        
        if (LMSManager.Instance != null)
        {
            LMSManager.Instance.ReportCompletion();
        }
    }
    
    void UpdateProgress()
    {
        float totalProgress = CalculateProgress();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateProgressBar(totalProgress);
        }
        
        if (LMSManager.Instance != null)
        {
            LMSManager.Instance.ReportProgress(totalProgress);
        }
    }
    
    float CalculateProgress()
    {
        float inspectionProgress = (float)inspectedItems.Count / requiredItems.Count * 0.4f;
        float collectionProgress = (float)collectedItems.Count / requiredItems.Count * 0.4f;
        float quizProgress = (float)quizScore / 3f * 0.2f;
        
        return inspectionProgress + collectionProgress + quizProgress;
    }
    
    public bool IsItemInspected(string itemID) => inspectedItems.Contains(itemID);
    public bool IsItemCollected(string itemID) => collectedItems.Contains(itemID);
    public bool IsScene1Complete() => scene1Complete;
    public bool IsScene2Complete() => scene2Complete;
    public int GetInspectedCount() => inspectedItems.Count;
    public int GetCollectedCount() => collectedItems.Count;
    public float GetProgress() => CalculateProgress();
}