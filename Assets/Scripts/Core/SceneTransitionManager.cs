using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    
    [Header("Scene Names")]
    public string scene1Name = "Scene1_Classroom";
    public string scene2Name = "Scene2_Warehouse";
    
    [Header("Transition Settings")]
    public GameObject transitionPanel;
    public float transitionDuration = 1f;
    
    private bool canTransitionToScene2 = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Determine current scene and set appropriate UI
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == scene1Name)
        {
            UIManager.Instance.SetSceneLabel("Scene 1: Safety Briefing");
            UIManager.Instance.ShowTimer(false);
        }
        else if (currentScene == scene2Name)
        {
            UIManager.Instance.SetSceneLabel("Scene 2: Warehouse Simulation");
            UIManager.Instance.ShowTimer(true);
            
            // Start warehouse timer
            TimerSystem.Instance.StartTimer(300f); // 5 minutes
        }
    }
    
    public void EnableScene2Transition()
    {
        canTransitionToScene2 = true;
    }
    
    public void LoadScene2()
    {
        if (canTransitionToScene2)
        {
            StartCoroutine(TransitionToScene(scene2Name));
        }
        else
        {
            Debug.Log("Scene 2 not yet available. Complete all inspections first.");
        }
    }
    
    public void LoadScene1()
    {
        StartCoroutine(TransitionToScene(scene1Name));
    }
    
    IEnumerator TransitionToScene(string sceneName)
    {
        // Show transition panel
        if (transitionPanel != null)
            transitionPanel.SetActive(true);
            
        // Wait for transition
        yield return new WaitForSeconds(transitionDuration);
        
        // Load new scene
        SceneManager.LoadScene(sceneName);
        
        // Hide transition panel (will be handled by new scene)
        if (transitionPanel != null)
            transitionPanel.SetActive(false);
    }
}