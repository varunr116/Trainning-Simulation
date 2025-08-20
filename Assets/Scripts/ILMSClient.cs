public interface ILMSClient
{
    void Initialize();
    void ReportProgress(float progress);
    void ReportCompletion(bool passed, int score);
    void SetLearnerName(string name);
    void Terminate();
    bool IsInitialized();
}