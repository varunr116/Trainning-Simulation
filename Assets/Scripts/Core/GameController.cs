using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    
    [Header("Scene Configuration")]
    public bool isScene1 = true;
    public bool isScene2 = false;
    
    [Header("Debug")]
    public bool enableDebugMode = false;
    
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
        InitializeScene();
    }
    
    void InitializeScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName.Contains("Classroom") || isScene1)
        {
            InitializeScene1();
        }
        else if (sceneName.Contains("Warehouse") || isScene2)
        {
            InitializeScene2();
        }
        
        // Initialize LMS
        if (LMSManager.Instance != null && !LMSManager.Instance.IsLMSActive())
        {
            // LMS might not be ready yet, try again
            Invoke("RetryLMSInit", 1f);
        }
    }
    
    void InitializeScene1()
    {
        Debug.Log("Initializing Scene 1: Classroom");
        
        // Set UI
        UIManager.Instance.SetSceneLabel("Scene 1: Safety Briefing");
        UIManager.Instance.ShowTimer(false);
        
        // Setup items for inspection
        SetupScene1Items();
        
        // Play welcome audio
        AudioManager.Instance.PlayNarration(GetWelcomeAudio());
        
        // Show checklist
        ChecklistUI checklistUI = FindObjectOfType<ChecklistUI>();
        if (checklistUI != null)
        {
            checklistUI.ShowChecklist(true);
        }
    }
    
    void InitializeScene2()
    {
        Debug.Log("Initializing Scene 2: Warehouse");
        
        // Set UI
        UIManager.Instance.SetSceneLabel("Scene 2: Warehouse Simulation");
        UIManager.Instance.ShowTimer(true);
        
        // Setup item spawning
        ItemSpawner spawner = FindObjectOfType<ItemSpawner>();
        if (spawner != null)
        {
            spawner.SpawnItems();
        }
        
        // Start timer
        TimerSystem.Instance.StartTimer(300f); // 5 minutes
        
        // Play warehouse intro audio
        AudioManager.Instance.PlayNarration(GetWarehouseIntroAudio());
    }
    
    void SetupScene1Items()
    {
        // Find all interactable items and set them up for inspection
        InteractableItem[] items = FindObjectsOfType<InteractableItem>();
        
        foreach (InteractableItem item in items)
        {
            item.isInspectable = true;
            item.isCollectable = false;
        }
        
        Debug.Log($"Setup {items.Length} items for inspection");
    }
    
    void RetryLMSInit()
    {
        // Second attempt at LMS initialization
        Debug.Log("Retrying LMS initialization...");
    }
    
    AudioClip GetWelcomeAudio()
    {
        // Return welcome audio clip
        return null; // Implement with your actual audio clips
    }
    
    AudioClip GetWarehouseIntroAudio()
    {
        // Return warehouse intro audio clip  
        return null; // Implement with your actual audio clips
    }
    
    void Update()
    {
        // Debug controls
        if (enableDebugMode)
        {
            HandleDebugInput();
        }
    }
    
    void HandleDebugInput()
    {
        // Skip to Scene 2
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneTransitionManager.Instance.LoadScene2();
        }
        
        // Complete all inspections
        if (Input.GetKeyDown(KeyCode.F3))
        {
            foreach (string itemID in ProgressService.Instance.requiredItems)
            {
                ProgressService.Instance.MarkItemInspected(itemID);
            }
        }
        
        // Start quiz
        if (Input.GetKeyDown(KeyCode.F4))
        {
            QuizManager.Instance.StartQuiz();
        }
        
        // Show certificate
        if (Input.GetKeyDown(KeyCode.F5))
        {
            CertificateUI.Instance.ShowCertificate();
        }
    }
}