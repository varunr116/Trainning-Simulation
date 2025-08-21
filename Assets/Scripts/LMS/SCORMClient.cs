using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class SCORMClient : MonoBehaviour, ILMSClient
{
    [Header("SCORM Settings")]
    public bool enableSCORM = true;
    public bool debugMode = true;
    
    private bool isInitialized = false;
    private string learnerName = "";
    private Dictionary<string, string> trackingData = new Dictionary<string, string>();
    
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
        if (!enableSCORM) return;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            isInitialized = SCORM_Initialize();
            if (isInitialized)
            {
                learnerName = SCORM_GetValue("cmi.core.student_name");
                if (string.IsNullOrEmpty(learnerName))
                    learnerName = "Trainee";
                    
                SetupInitialTracking();
            }
        }
        catch (System.Exception e)
        {
           
            isInitialized = false;
        }
#else
        isInitialized = true;
        learnerName = "Desktop User";
        SetupInitialTracking();
#endif
    }
    
    void SetupInitialTracking()
    {
        SetValue("cmi.core.lesson_status", "incomplete");
        SetValue("cmi.core.score.min", "0");
        SetValue("cmi.core.score.max", "100");
        SetValue("cmi.core.entry", "ab-initio");
        
        trackingData["session_start"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        trackingData["training_version"] = "1.0";
        trackingData["unity_version"] = Application.unityVersion;
    }
    
    public void ReportProgress(float progress)
    {
        if (!isInitialized) return;
        
        int progressPercent = Mathf.RoundToInt(progress * 100);
        
        SetValue("cmi.core.score.raw", progressPercent.ToString());
        SetValue("cmi.core.lesson_location", $"progress_{progressPercent}");
        
        if (ProgressService.Instance != null)
        {
            int inspected = ProgressService.Instance.GetInspectedCount();
            int collected = ProgressService.Instance.GetCollectedCount();
            
            SetValue("cmi.suspend_data", $"inspected:{inspected},collected:{collected}");
        }
        
        Commit();
    }
    
    public void ReportCompletion(bool passed, int score)
    {
        if (!isInitialized) return;
        
        string status = passed ? "passed" : "failed";
        int scorePercent = Mathf.RoundToInt((float)score / 3f * 100);
        
        SetValue("cmi.core.lesson_status", status);
        SetValue("cmi.core.score.raw", scorePercent.ToString());
        SetValue("cmi.core.exit", "");
        
        trackingData["completion_time"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        trackingData["final_score"] = score.ToString();
        trackingData["quiz_attempts"] = "1";
        
        string sessionData = BuildSessionData();
        SetValue("cmi.suspend_data", sessionData);
        
        Commit();
    }
    
    public void TrackItemInspection(string itemID)
    {
        if (!isInitialized) return;
        
        trackingData[$"inspected_{itemID}"] = System.DateTime.Now.ToString("HH:mm:ss");
        
        string sessionData = BuildSessionData();
        SetValue("cmi.suspend_data", sessionData);
        Commit();
    }
    
    public void TrackItemCollection(string itemID)
    {
        if (!isInitialized) return;
        
        trackingData[$"collected_{itemID}"] = System.DateTime.Now.ToString("HH:mm:ss");
        
        string sessionData = BuildSessionData();
        SetValue("cmi.suspend_data", sessionData);
        Commit();
    }
    
    public void TrackQuizAnswer(int questionIndex, int selectedAnswer, bool isCorrect)
    {
        if (!isInitialized) return;
        
        trackingData[$"q{questionIndex}_answer"] = selectedAnswer.ToString();
        trackingData[$"q{questionIndex}_correct"] = isCorrect.ToString();
        trackingData[$"q{questionIndex}_time"] = System.DateTime.Now.ToString("HH:mm:ss");
        
        string sessionData = BuildSessionData();
        SetValue("cmi.suspend_data", sessionData);
        Commit();
    }
    
    string BuildSessionData()
    {
        var dataList = new List<string>();
        foreach (var kvp in trackingData)
        {
            dataList.Add($"{kvp.Key}={kvp.Value}");
        }
        return string.Join("|", dataList);
    }
    
    void SetValue(string element, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SCORM_SetValue(element, value);
#else
        if (debugMode)
            Debug.Log($"[SCORM] {element} = {value}");
#endif
    }
    
    void Commit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SCORM_Commit();
#endif
    }
    
    public void SetLearnerName(string name)
    {
        learnerName = name;
        if (CertificateUI.Instance != null)
        {
            CertificateUI.Instance.SetLearnerName(name);
        }
    }
    
    public void Terminate()
    {
        if (!isInitialized) return;
        
        trackingData["session_end"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        string sessionData = BuildSessionData();
        SetValue("cmi.suspend_data", sessionData);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        SCORM_Terminate();
#endif
        
        isInitialized = false;
    }
    
    public bool IsInitialized() => isInitialized;
}