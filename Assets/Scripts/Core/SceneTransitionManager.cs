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
    
    [Header("Scene 2 Access")]
    public int minimumInspectionsRequired = 1;
    
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
            
            if (TimerSystem.Instance != null)
            {
                TimerSystem.Instance.StartTimer(300f);
            }
        }
    }
    
    public void EnableScene2Transition()
    {
        canTransitionToScene2 = true;
    }
    
    public void LoadScene2()
    {
        if (CanAccessScene2())
        {
            StartCoroutine(TransitionToScene(scene2Name));
        }
        else
        {
            int inspected = ProgressService.Instance != null ? ProgressService.Instance.GetInspectedCount() : 0;
           
        }
    }
    
    bool CanAccessScene2()
    {
        if (ProgressService.Instance == null) return false;
        
        int inspectedCount = ProgressService.Instance.GetInspectedCount();
        return inspectedCount >= minimumInspectionsRequired;
    }
    
    public void LoadScene1()
    {
        StartCoroutine(TransitionToScene(scene1Name));
    }
    
    IEnumerator TransitionToScene(string sceneName)
    {
        if (transitionPanel != null)
            transitionPanel.SetActive(true);
            
        yield return new WaitForSeconds(transitionDuration);
        
        SceneManager.LoadScene(sceneName);
        
        if (transitionPanel != null)
            transitionPanel.SetActive(false);
    }
}