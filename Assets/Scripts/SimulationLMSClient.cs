using UnityEngine;
public class SimulationLMSClient : ILMSClient
{
    private bool initialized = false;
    private string learnerName = "Test User";

    public void Initialize()
    {
        initialized = true;
        
    }

    public void ReportProgress(float progress)
    {
        int percent = Mathf.RoundToInt(progress * 100);
       
    }

    public void ReportCompletion(bool passed, int score)
    {
        string status = passed ? "PASSED" : "FAILED";
       
    }

    public void SetLearnerName(string name)
    {
        learnerName = name;
     
    }

    public void Terminate()
    {
        initialized = false;

    }

    public bool IsInitialized()
    {
        return initialized;
    }
}