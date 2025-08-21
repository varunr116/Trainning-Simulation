using UnityEngine;
using System.Collections.Generic;
public class Scene2AudioManager : MonoBehaviour
{
    public static Scene2AudioManager Instance;

    [Header("Audio Data")]
    public Scene2AudioData audioData;

    [Header("Settings")]
    public bool playPickupAudio = true;
    public bool playDropAudio = true;
    public bool playProgressAudio = true;

    private int itemsCollected = 0;
    private int totalItems = 4;
    private bool hasPlayedHalfway = false;
    private bool hasPlayedAlmostFinished = false;

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
        
        PlayWelcomeAudio();

       
    }

  

    void PlayWelcomeAudio()
    {
        if (audioData.welcomeToWarehouse != null)
        {
            AudioManager.Instance.PlayNarration(audioData.welcomeToWarehouse);

            // Play instructions after welcome (with delay)
            Invoke("PlayInstructionsAudio", GetAudioLength(audioData.welcomeToWarehouse) + 1f);
        }
    }

    void PlayInstructionsAudio()
    {
        if (audioData.generalInstructions != null)
        {
            AudioManager.Instance.PlayNarration(audioData.generalInstructions);
        }
    }

    
    public void PlayPickupAudio(string itemID)
    {
        if (!playPickupAudio) return;

        AudioClip clip = GetPickupClip(itemID);
        if (clip != null)
        {
            AudioManager.Instance.PlayNarration(clip);
        }
    }

    AudioClip GetPickupClip(string itemID)
    {
        switch (itemID.ToLower())
        {
            case "tape_gun": return audioData.pickupTapeGun;
            case "barcode_scanner": return audioData.pickupBarcodeScanner;
            case "safety_gloves": return audioData.pickupSafetyGloves;
            case "safety_goggles": return audioData.pickupSafetyGoggles;
            default: return audioData.pickupGeneric;
        }
    }

    

    public void PlayDropAudio(string itemID)
    {
        if (!playDropAudio) return;

        AudioClip clip = GetDropClip(itemID);
        if (clip != null)
        {
            AudioManager.Instance.PlayNarration(clip);
        }

        // Update progress after drop
        itemsCollected++;
        CheckProgressAudio();
    }

    AudioClip GetDropClip(string itemID)
    {
        switch (itemID.ToLower())
        {
            case "tape_gun": return audioData.dropTapeGun;
            case "barcode_scanner": return audioData.dropBarcodeScanner;
            case "safety_gloves": return audioData.dropSafetyGloves;
            case "safety_goggles": return audioData.dropSafetyGoggles;
            default: return audioData.dropGeneric;
        }
    }

   

    void CheckProgressAudio()
    {
        if (!playProgressAudio) return;

        if (itemsCollected == 1 && audioData.firstItemCollected != null)
        {
            // First item collected
            Invoke("PlayProgressAudio", 2f); // Delay after drop audio
        }
        else if (itemsCollected == totalItems / 2 && !hasPlayedHalfway)
        {
            // Halfway complete
            hasPlayedHalfway = true;
            Invoke("PlayHalfwayAudio", 2f);
        }
        else if (itemsCollected == totalItems - 1 && !hasPlayedAlmostFinished)
        {
            // Almost finished
            hasPlayedAlmostFinished = true;
            Invoke("PlayAlmostFinishedAudio", 2f);
        }
        else if (itemsCollected >= totalItems)
        {
            // All items collected
            Invoke("PlayAllItemsCollectedAudio", 2f);
        }
    }

    void PlayProgressAudio()
    {
        if (audioData.firstItemCollected != null)
        {
            AudioManager.Instance.PlayNarration(audioData.firstItemCollected);
        }
    }

    void PlayHalfwayAudio()
    {
        if (audioData.halfwayComplete != null)
        {
            AudioManager.Instance.PlayNarration(audioData.halfwayComplete);
        }
    }

    void PlayAlmostFinishedAudio()
    {
        if (audioData.almostFinished != null)
        {
            AudioManager.Instance.PlayNarration(audioData.almostFinished);
        }
    }

    void PlayAllItemsCollectedAudio()
    {
        if (audioData.allItemsCollected != null)
        {
            AudioManager.Instance.PlayNarration(audioData.allItemsCollected);
        }
    }

 
    public void PlayTimerWarning(int remainingMinutes)
    {
        AudioClip clip = null;

        if (remainingMinutes == 2 && audioData.twoMinutesRemaining != null)
        {
            clip = audioData.twoMinutesRemaining;
        }
        else if (remainingMinutes == 0 && audioData.thirtySecondsRemaining != null)
        {
            clip = audioData.thirtySecondsRemaining;
        }

        if (clip != null)
        {
            AudioManager.Instance.PlayNarration(clip);
        }
    }

    public void PlayTimeUpAudio()
    {
        if (audioData.timeUp != null)
        {
            AudioManager.Instance.PlayNarration(audioData.timeUp);
        }
    }

   

    float GetAudioLength(AudioClip clip)
    {
        return clip != null ? clip.length : 0f;
    }

    public void ResetProgress()
    {
        itemsCollected = 0;
        hasPlayedHalfway = false;
        hasPlayedAlmostFinished = false;
    }

  
    public void OnItemPickedUp(string itemID)
    {
        PlayPickupAudio(itemID);
    }

    public void OnItemDropped(string itemID)
    {
        PlayDropAudio(itemID);
    }

    // Call these from your TimerSystem
    public void OnTimerWarning(int minutes)
    {
        PlayTimerWarning(minutes);
    }

    public void OnTimeUp()
    {
        PlayTimeUpAudio();
    }
}