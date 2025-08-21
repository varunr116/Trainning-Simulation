using UnityEngine;

public class LMSManager : MonoBehaviour
{
    public static LMSManager Instance;
    
    [Header("LMS Configuration")]
    public bool enableLMSReporting = true;
    public LMSType lmsType = LMSType.SCORM;
    
    [Header("References")]
    public SCORMClient scormClient;
    
    public enum LMSType
    {
        SCORM,
        xAPI,
        Simulation
    }
    
    private ILMSClient currentClient;
    private bool isInitialized = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLMS();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeLMS()
    {
        if (!enableLMSReporting)
        {
            
            return;
        }
        
        switch (lmsType)
        {
            case LMSType.SCORM:
                if (scormClient == null)
                    scormClient = gameObject.AddComponent<SCORMClient>();
                currentClient = scormClient;
                break;
                
            case LMSType.xAPI:
                // Could implement xAPI client here
                
                break;
                
            case LMSType.Simulation:
                currentClient = new SimulationLMSClient();
                break;
        }
        
        if (currentClient != null)
        {
            currentClient.Initialize();
            isInitialized = currentClient.IsInitialized();
            
            if (isInitialized)
            {
               
                
                // Report initial progress
                ReportProgress(0f);
            }
        }
    }
    
    public void ReportProgress(float progress)
    {
        if (isInitialized && currentClient != null)
        {
            currentClient.ReportProgress(progress);
        }
    }
    
    public void ReportCompletion()
    {
        if (isInitialized && currentClient != null)
        {
            bool passed = QuizManager.Instance != null && QuizManager.Instance.HasPassed();
            int score = QuizManager.Instance != null ? QuizManager.Instance.GetScore() : 0;
            
            currentClient.ReportCompletion(passed, score);
        }
    }
    
    public void SetLearnerName(string name)
    {
        if (isInitialized && currentClient != null)
        {
            currentClient.SetLearnerName(name);
        }
    }
    
    void OnApplicationQuit()
    {
        if (isInitialized && currentClient != null)
        {
            currentClient.Terminate();
        }
    }
    
    public bool IsLMSActive()
    {
        return isInitialized && enableLMSReporting;
    }
}
