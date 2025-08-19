using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Sources")]
    public AudioSource narrationSource;
    public AudioSource sfxSource;
    public AudioSource backgroundSource;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float narrationVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 0.6f;
    
    [Header("SFX Clips")]
    public AudioClip itemCollectedClip;
    public AudioClip buttonClickClip;
    public AudioClip correctAnswerClip;
    public AudioClip wrongAnswerClip;
    public AudioClip timerWarningClip;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetupAudioSources()
    {
        // Create audio sources if not assigned
        if (narrationSource == null)
        {
            GameObject narrationGO = new GameObject("NarrationSource");
            narrationGO.transform.SetParent(transform);
            narrationSource = narrationGO.AddComponent<AudioSource>();
        }
        
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }
        
        // Configure audio sources
        narrationSource.volume = narrationVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
        
        narrationSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
    }
    
    public void PlayNarration(AudioClip clip)
    {
        if (clip != null && narrationSource != null)
        {
            StopNarration(); // Stop any currently playing narration
            narrationSource.clip = clip;
            narrationSource.Play();
            
            Debug.Log($"Playing narration: {clip.name}");
        }
    }
    
    public void StopNarration()
    {
        if (narrationSource != null && narrationSource.isPlaying)
        {
            narrationSource.Stop();
        }
    }
    
    public void PlaySFX(string clipName)
    {
        AudioClip clip = GetSFXClip(clipName);
        if (clip != null)
        {
            PlaySFX(clip);
        }
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    AudioClip GetSFXClip(string clipName)
    {
        switch (clipName.ToLower())
        {
            case "item_collected": return itemCollectedClip;
            case "button_click": return buttonClickClip;
            case "correct_answer": return correctAnswerClip;
            case "wrong_answer": return wrongAnswerClip;
            case "timer_warning": return timerWarningClip;
            default: 
                Debug.LogWarning($"SFX clip not found: {clipName}");
                return null;
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    void UpdateVolumes()
    {
        if (narrationSource != null)
            narrationSource.volume = narrationVolume * masterVolume;
            
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }
    
    public bool IsNarrationPlaying()
    {
        return narrationSource != null && narrationSource.isPlaying;
    }
}