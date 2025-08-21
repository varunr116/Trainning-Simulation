using UnityEngine;
using System.Collections;

public class TimerSystem : MonoBehaviour
{
    public static TimerSystem Instance;
    
    [Header("Timer Settings")]
    public float totalTime = 300f; // 5 minutes in seconds
    public bool autoStart = false;
    
    [Header("Warning Settings")]
    public float warningAt2Minutes = 120f;
    public float warningAt30Seconds = 30f;
    
    private float currentTime;
    private bool isRunning = false;
    private bool hasWarned2Min = false;
    private bool hasWarned30Sec = false;
    private bool timeUp = false;
    
    public delegate void TimerEvent();
    public static event TimerEvent OnTimerWarning2Min;
    public static event TimerEvent OnTimerWarning30Sec;
    public static event TimerEvent OnTimeUp;
    
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
        if (autoStart)
        {
            StartTimer(totalTime);
        }
    }
    
    void Update()
    {
        if (isRunning && !timeUp)
        {
            currentTime -= Time.deltaTime;
            
            // Update UI
            UIManager.Instance.UpdateTimer(currentTime);
            
            // Check for warnings
            CheckWarnings();
            
            // Check if time is up
            if (currentTime <= 0)
            {
                TimeUp();
            }
        }
    }
    
    
    public void StartTimer(float duration)
    {
        totalTime = duration;
        currentTime = duration;
        isRunning = true;
        timeUp = false;
        hasWarned2Min = false;
        hasWarned30Sec = false;

        Debug.Log($"Timer started: {duration} seconds");

        // Show timer UI
        UIManager.Instance.ShowTimer(true);
    }
    
    public void PauseTimer()
    {
        isRunning = false;
    }
    
    public void ResumeTimer()
    {
        if (!timeUp)
            isRunning = true;
    }
    
    public void StopTimer()
    {
        isRunning = false;
        timeUp = true;
        UIManager.Instance.ShowTimer(false);
    }
    
    void CheckWarnings()
{
    // 2 minute warning
    if (!hasWarned2Min && currentTime <= warningAt2Minutes)
    {
        hasWarned2Min = true;
        
        // Play through Scene2AudioManager instead of directly
        if (Scene2AudioManager.Instance != null)
        {
            Scene2AudioManager.Instance.OnTimerWarning(2);
        }
        else
        {
            AudioManager.Instance.PlayNarration(GetAudioClip("09_Timer_Warning_2Min"));
        }
        
        OnTimerWarning2Min?.Invoke();
    }
    
    // 30 second warning
    if (!hasWarned30Sec && currentTime <= warningAt30Seconds)
    {
        hasWarned30Sec = true;
        
        // Play through Scene2AudioManager
        if (Scene2AudioManager.Instance != null)
        {
            Scene2AudioManager.Instance.OnTimerWarning(0); // 0 for 30 seconds
        }
        else
        {
            AudioManager.Instance.PlayNarration(GetAudioClip("10_Timer_Warning_30Sec"));
        }
        
        AudioManager.Instance.PlaySFX("timer_warning");
        OnTimerWarning30Sec?.Invoke();
    }
}
    
   void TimeUp()
{
    isRunning = false;
    timeUp = true;
    currentTime = 0;
    
   
    // Play through Scene2AudioManager
    if (Scene2AudioManager.Instance != null)
    {
        Scene2AudioManager.Instance.OnTimeUp();
    }
    else
    {
        AudioManager.Instance.PlayNarration(GetAudioClip("12_Collection_Failure"));
    }
    
    OnTimeUp?.Invoke();
    
    // Proceed to quiz regardless
    StartCoroutine(DelayedQuizStart());
}
    
    IEnumerator DelayedQuizStart()
    {
        yield return new WaitForSeconds(3f); // Wait for audio to finish
        QuizManager.Instance.StartQuiz();
    }
    
    AudioClip GetAudioClip(string clipName)
    {
        // This would be set up with your actual audio clips
        // For now, return null and handle in AudioManager
        return null;
    }
    
    public float GetTimeRemaining() => currentTime;
    public float GetTimeElapsed() => totalTime - currentTime;
    public bool IsRunning() => isRunning;
    public bool IsTimeUp() => timeUp;
    public float GetProgress() => (totalTime - currentTime) / totalTime;
}
