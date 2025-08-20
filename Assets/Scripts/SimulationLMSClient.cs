using UnityEngine;
public class SimulationLMSClient : ILMSClient
{
    private bool initialized = false;
    private string learnerName = "Test User";

    public void Initialize()
    {
        initialized = true;
        Debug.Log("[LMS SIMULATION] Initialized");
    }

    public void ReportProgress(float progress)
    {
        int percent = Mathf.RoundToInt(progress * 100);
        Debug.Log($"[LMS SIMULATION] Progress: {percent}%");
    }

    public void ReportCompletion(bool passed, int score)
    {
        string status = passed ? "PASSED" : "FAILED";
        Debug.Log($"[LMS SIMULATION] Completion: {status}, Score: {score}/3");
    }

    public void SetLearnerName(string name)
    {
        learnerName = name;
        Debug.Log($"[LMS SIMULATION] Learner: {name}");
    }

    public void Terminate()
    {
        initialized = false;
        Debug.Log("[LMS SIMULATION] Terminated");
    }

    public bool IsInitialized()
    {
        return initialized;
    }
}