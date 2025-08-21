

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        
        Instance.UpdateSceneSettings(this);
        Destroy(gameObject);
    }
}
    
    void Start()
    {
        
        StartCoroutine(DelayedInitialization());
    }
    public void UpdateSceneSettings(GameController sceneController)
{
    // Copy settings from the scene's controller
    this.isScene1 = sceneController.isScene1;
    this.isScene2 = sceneController.isScene2;
    this.enableDebugMode = sceneController.enableDebugMode;
    
    // Reinitialize with new settings
    InitializeScene();
}
    IEnumerator DelayedInitialization()
    {
        // Wait a frame for all Awake() methods to complete
        yield return null;
        
        // Wait another frame for all Start() methods to complete
        yield return null;
        
       
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
        
        // Initialize LMS after scene setup
        StartCoroutine(InitializeLMSAfterDelay());
    }
    
    IEnumerator InitializeLMSAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        if (LMSManager.Instance != null && !LMSManager.Instance.IsLMSActive())
        {
            Debug.Log("Retrying LMS initialization...");
        }
    }
    
    void InitializeScene1()
    {
       
        
        // Find and setup UI Manager
        SetupUIManager();
        
        // Set scene label and UI state
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetSceneLabel("Scene 1: Safety Briefing");
            UIManager.Instance.ShowTimer(false);
        }
        
        // Setup items for inspection
        SetupScene1Items();
        
        // Start projector presentation
        StartProjectorPresentation();
        
        // Show checklist
        SetupChecklistUI();
        
       
    }
    
    void InitializeScene2()
    {
        
        
        // CRITICAL: Find and setup all managers for Scene 2
        SetupUIManager();
        SetupTimerSystem();
        SetupProgressService();
        SetupAudioManager();
        SetupScene2Audio();
        
        // Set scene label and show timer
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetSceneLabel("Scene 2: Warehouse Simulation");
            UIManager.Instance.ShowTimer(true);
            
            // Update progress bar with current progress
            if (ProgressService.Instance != null)
            {
                float currentProgress = ProgressService.Instance.GetProgress();
                UIManager.Instance.UpdateProgressBar(currentProgress);
               
            }
        }
        
        // Setup item spawning and collection
        SetupScene2Items();
        SetupCollectionZone();
        
        // Start timer system
        StartTimerSystem();
        
        // Play welcome audio
        PlayScene2WelcomeAudio();
        
       
    }
    
    
    
    void SetupUIManager()
    {
        if (UIManager.Instance == null)
        {
           
            
            // Try to find UIManager in scene
            UIManager uiManager = FindObjectOfType<UIManager>();
            
        }
        else
        {
           
        }
    }
    
    void SetupTimerSystem()
    {
        if (TimerSystem.Instance == null)
        {
           
            
            // Try to find existing TimerSystem
            TimerSystem timer = FindObjectOfType<TimerSystem>();
            if (timer == null)
            {
                // Create new TimerSystem
                GameObject timerGO = new GameObject("TimerSystem");
                timer = timerGO.AddComponent<TimerSystem>();
               
            }
        }
        else
        {
            Debug.Log("✅ TimerSystem found and ready");
        }
    }
    
    void SetupProgressService()
    {
        if (ProgressService.Instance == null)
        {
            
            
            // ProgressService should be DontDestroyOnLoad, so this is concerning
            ProgressService progress = FindObjectOfType<ProgressService>();
            if (progress != null)
            {
                Debug.Log(" Found ProgressService in scene but Instance is null");
            }
        }
        else
        {
            
        }
    }
    
    void SetupAudioManager()
    {
        if (AudioManager.Instance == null)
        {
           
            
            AudioManager audio = FindObjectOfType<AudioManager>();
            if (audio != null)
            {
                Debug.Log(" Found AudioManager in scene but Instance is null");
            }
        }
        else
        {
           
        }
    }
    
    void SetupScene2Audio()
    {
        Scene2AudioManager scene2Audio = FindObjectOfType<Scene2AudioManager>();
        if (scene2Audio == null)
        {
           
            
            // Optionally create one
            GameObject audioGO = new GameObject("Scene2AudioManager");
            scene2Audio = audioGO.AddComponent<Scene2AudioManager>();
          
        }
        else
        {
          
        }
    }
    
    void SetupScene2Items()
    {
        // Setup item spawning
        ItemSpawner spawner = FindObjectOfType<ItemSpawner>();
        if (spawner != null)
        {
            spawner.SpawnItems();
            
        }
        else
        {
           
            
            // Fallback: setup existing items in scene for collection
            SetupExistingItemsForCollection();
        }
    }
    
    void SetupExistingItemsForCollection()
    {
        InteractableItem[] items = FindObjectsOfType<InteractableItem>();
        
        foreach (InteractableItem item in items)
        {
            // Configure for Scene 2
            item.isInspectable = false;  
            item.isCollectable = true;   
            
            
        }
        
       
    }
    
    void SetupCollectionZone()
    {
        CollectionZone zone = FindObjectOfType<CollectionZone>();
        if (zone == null)
        {
            
            
            // Could create a basic one here if needed
            CreateBasicCollectionZone();
        }
        else
        {
            
        }
    }
    
    void CreateBasicCollectionZone()
    {
        GameObject zoneGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zoneGO.name = "CollectionZone_Auto";
        zoneGO.transform.position = Vector3.zero;
        zoneGO.transform.localScale = new Vector3(3, 1, 3);
        
        // Make it a trigger
        Collider collider = zoneGO.GetComponent<Collider>();
        collider.isTrigger = true;
        
        // Add CollectionZone script
        CollectionZone zone = zoneGO.AddComponent<CollectionZone>();
        
        // Make it semi-transparent
        Renderer renderer = zoneGO.GetComponent<Renderer>();
        Material mat = renderer.material;
        mat.color = new Color(0, 1, 0, 0.3f); // Green, semi-transparent
        
      
    }
    
    void StartTimerSystem()
    {
        if (TimerSystem.Instance != null)
        {
            TimerSystem.Instance.StartTimer(300f); // 5 minutes
            
        }
        else
        {
           
        }
    }
    
    void PlayScene2WelcomeAudio()
    {
        if (Scene2AudioManager.Instance != null)
        {
            // Scene2AudioManager will handle welcome audio automatically
          
        }
        else if (AudioManager.Instance != null)
        {
            // Fallback to general warehouse intro
            AudioClip introClip = GetWarehouseIntroAudio();
            if (introClip != null)
            {
                AudioManager.Instance.PlayNarration(introClip);
                
            }
        }
        else
        {
           
        }
    }
    
   
    
    void SetupChecklistUI()
    {
        ChecklistUI checklistUI = FindObjectOfType<ChecklistUI>();
        if (checklistUI != null)
        {
            checklistUI.ShowChecklist(true);
           
        }
      
    }
    
    void StartProjectorPresentation()
    {
        VideoProjectorController projector = FindObjectOfType<VideoProjectorController>();
        if (projector != null)
        {
            // Start welcome audio first, then projector
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayNarration(GetWelcomeAudio());
            }
            
            // Start projector after welcome audio (delay)
            Invoke("StartProjectorVideo", 3f);
           
        }
       
    }
    
    void StartProjectorVideo()
    {
        VideoProjectorController projector = FindObjectOfType<VideoProjectorController>();
        if (projector != null)
        {
            projector.PlayVideo();
          
        }
    }
    
    void SetupScene1Items()
    {
        InteractableItem[] items = FindObjectsOfType<InteractableItem>();
        
        foreach (InteractableItem item in items)
        {
            item.isInspectable = true;
            item.isCollectable = false;
        }
        
        
    }
    
   
    
    AudioClip GetWelcomeAudio()
    {
        // Return welcome audio clip for Scene 1
        return null; 
    }
    
    AudioClip GetWarehouseIntroAudio()
    {
        // Return warehouse intro audio clip for Scene 2
        return null; 
    }
    
    
    
    void Update()
    {
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
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene2();
            }
        }
        
        // Complete all inspections
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (ProgressService.Instance != null)
            {
                foreach (string itemID in ProgressService.Instance.requiredItems)
                {
                    ProgressService.Instance.MarkItemInspected(itemID);
                }
            }
        }
        
        // Start quiz
        if (Input.GetKeyDown(KeyCode.F4))
        {
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.StartQuiz();
            }
        }
        
        // Show certificate
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (CertificateUI.Instance != null)
            {
                CertificateUI.Instance.ShowCertificate();
            }
        }
        
        // Debug Scene 2 setup
        if (Input.GetKeyDown(KeyCode.F6))
        {
            
            InitializeScene2();
        }
    }
}