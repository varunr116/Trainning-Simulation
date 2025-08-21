using UnityEngine;
using System.Collections;

public class Scene1AudioManager : MonoBehaviour
{
    public static Scene1AudioManager Instance;

    [Header("Scene 1 Audio Clips")]
    public AudioClip welcomeToTraining;
    public AudioClip safetyBriefingIntro;
    public AudioClip inspectionInstructions;
    public AudioClip videoIntroduction;
    public AudioClip checklistExplanation;
    
    [Header("Item Inspection Audio")]
    public AudioClip firstItemInspected;
    public AudioClip halfItemsInspected;
    public AudioClip allItemsInspected;
    
    [Header("Transition Audio")]
    public AudioClip readyForWarehouse;
    public AudioClip proceedToScene2;
    
    private bool hasPlayedWelcome = false;
    private bool hasPlayedHalfway = false;
    private int itemsInspected = 0;
    private int totalItems = 4;

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
        Invoke("PlayWelcomeSequence", 1f);
    }

    void PlayWelcomeSequence()
    {
        if (!hasPlayedWelcome && welcomeToTraining != null)
        {
            hasPlayedWelcome = true;
            AudioManager.Instance.PlayNarration(welcomeToTraining);
            
            float delay = welcomeToTraining.length + 1f;
            Invoke("PlaySafetyBriefing", delay);
        }
    }

    void PlaySafetyBriefing()
    {
        if (safetyBriefingIntro != null)
        {
            AudioManager.Instance.PlayNarration(safetyBriefingIntro);
            
            float delay = safetyBriefingIntro.length + 1f;
            Invoke("PlayInspectionInstructions", delay);
        }
    }

    void PlayInspectionInstructions()
    {
        if (inspectionInstructions != null)
        {
            AudioManager.Instance.PlayNarration(inspectionInstructions);
        }
    }

    public void PlayVideoIntroduction()
    {
        if (videoIntroduction != null)
        {
            AudioManager.Instance.PlayNarration(videoIntroduction);
        }
    }

    public void PlayChecklistExplanation()
    {
        if (checklistExplanation != null)
        {
            AudioManager.Instance.PlayNarration(checklistExplanation);
        }
    }

    public void OnItemInspected(string itemID)
    {
        itemsInspected++;
        
        if (itemsInspected == 1 && firstItemInspected != null)
        {
            Invoke("PlayFirstItemAudio", 2f);
        }
        else if (itemsInspected == totalItems / 2 && !hasPlayedHalfway)
        {
            hasPlayedHalfway = true;
            Invoke("PlayHalfwayAudio", 2f);
        }
        else if (itemsInspected >= totalItems)
        {
            Invoke("PlayAllItemsAudio", 2f);
        }
    }

    void PlayFirstItemAudio()
    {
        if (firstItemInspected != null)
        {
            AudioManager.Instance.PlayNarration(firstItemInspected);
        }
    }

    void PlayHalfwayAudio()
    {
        if (halfItemsInspected != null)
        {
            AudioManager.Instance.PlayNarration(halfItemsInspected);
        }
    }

    void PlayAllItemsAudio()
    {
        if (allItemsInspected != null)
        {
            AudioManager.Instance.PlayNarration(allItemsInspected);
            
            float delay = allItemsInspected.length + 1f;
            Invoke("PlayReadyForWarehouse", delay);
        }
    }

    void PlayReadyForWarehouse()
    {
        if (readyForWarehouse != null)
        {
            AudioManager.Instance.PlayNarration(readyForWarehouse);
        }
    }

    public void PlayProceedToScene2()
    {
        if (proceedToScene2 != null)
        {
            AudioManager.Instance.PlayNarration(proceedToScene2);
        }
    }

    public void ResetProgress()
    {
        itemsInspected = 0;
        hasPlayedHalfway = false;
        hasPlayedWelcome = false;
    }
}