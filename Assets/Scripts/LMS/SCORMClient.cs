using UnityEngine;
using System.Runtime.InteropServices;

public class SCORMClient : MonoBehaviour, ILMSClient
{
    [Header("SCORM Settings")]
    public bool enableSCORM = true;
    public bool debugMode = true;
    
    private bool isInitialized = false;
    private string learnerName = "";
    private float lastReportedProgress = 0f;
    
    // JavaScript bridge functions for WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool SCORM_Initialize();
    
    [DllImport("__Internal")]
    private static extern bool SCORM_SetValue(string element, string value);
    
    [DllImport("__Internal")]
    private static extern string SCORM_GetValue(string element);
    
    [DllImport("__Internal")]
    private static extern bool SCORM_Commit();
    
    [DllImport("__Internal")]
    private static extern bool SCORM_Terminate();
#endif
    
    public void Initialize()
    {
        if (!enableSCORM)
        {
            LogDebug("SCORM disabled");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            isInitialized = SCORM_Initialize();
            
            if (isInitialized)
            {
                // Get learner name
                learnerName = SCORM_GetValue("cmi.core.student_name");
                if (string.IsNullOrEmpty(learnerName))
                    learnerName = "Trainee";
                    
                LogDebug($"SCORM initialized successfully. Learner: {learnerName}");
                
                // Set initial values
                SCORM_SetValue("cmi.core.lesson_status", "incomplete");
                SCORM_SetValue("cmi.core.score.min", "0");
                SCORM_SetValue("cmi.core.score.max", "100");
                SCORM_Commit();
            }
            else
            {
                LogDebug("SCORM initialization failed");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"SCORM error: {e.Message}");
            isInitialized = false;
        }
#else
        // Desktop simulation
        isInitialized = true;
        learnerName = "Desktop User";
        LogDebug("SCORM simulation mode (Desktop)");
#endif
    }
    
    public void ReportProgress(float progress)
    {
        if (!isInitialized || !enableSCORM) return;
        
        // Only report if progress increased significantly
        if (progress - lastReportedProgress < 0.05f) return;
        
        lastReportedProgress = progress;
        int progressPercent = Mathf.RoundToInt(progress * 100);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            SCORM_SetValue("cmi.core.score.raw", progressPercent.ToString());
            SCORM_SetValue("cmi.core.lesson_location", $"progress_{progressPercent}");
            SCORM_Commit();
            
            LogDebug($"Progress reported: {progressPercent}%");
        }
        catch (System.Exception e)
        {
            LogDebug($"Progress report error: {e.Message}");
        }
#else
        LogDebug($"[DESKTOP SIMULATION] Progress: {progressPercent}%");
#endif
    }
    
    public void ReportCompletion(bool passed, int score)
    {
        if (!isInitialized || !enableSCORM) return;
        
        string status = passed ? "passed" : "failed";
        int scorePercent = Mathf.RoundToInt((float)score / 3f * 100);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            SCORM_SetValue("cmi.core.lesson_status", status);
            SCORM_SetValue("cmi.core.score.raw", scorePercent.ToString());
            SCORM_SetValue("cmi.core.exit", "");
            SCORM_Commit();
            
            LogDebug($"Completion reported: {status}, Score: {score}/3 ({scorePercent}%)");
        }
        catch (System.Exception e)
        {
            LogDebug($"Completion report error: {e.Message}");
        }
#else
        LogDebug($"[DESKTOP SIMULATION] Completion: {status}, Score: {score}/3 ({scorePercent}%)");
#endif
    }
    
    public void SetLearnerName(string name)
    {
        learnerName = name;
        
        // Update certificate with learner name
        if (CertificateUI.Instance != null)
        {
            CertificateUI.Instance.SetLearnerName(name);
        }
    }
    
    public void Terminate()
    {
        if (!isInitialized || !enableSCORM) return;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            SCORM_Terminate();
            LogDebug("SCORM terminated");
        }
        catch (System.Exception e)
        {
            LogDebug($"SCORM termination error: {e.Message}");
        }
#else
        LogDebug("[DESKTOP SIMULATION] SCORM terminated");
#endif
        
        isInitialized = false;
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[SCORM] {message}");
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && isInitialized)
        {
            // App resumed, commit current state
#if UNITY_WEBGL && !UNITY_EDITOR
            SCORM_Commit();
#endif
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isInitialized)
        {
            // App lost focus, save current state
#if UNITY_WEBGL && !UNITY_EDITOR
            SCORM_Commit();
#endif
        }
    }
}