using UnityEngine;
using UnityEngine.Video;

public class VideoProjectorController : MonoBehaviour
{
    [Header("Video Sources")]
    public VideoClip desktopVideo;
    public string webVideoURL = "https://raw.githubusercontent.com/varunr116/Avatar-ConversationAR/ce4045f158ca8a1e3afd1c54105da3127fd1be89/Assets/Description_Clip.mp4.mp4";
    
    [Header("Screen")]
    public Transform screenTarget;
    public Material screenMaterial;
    
    [Header("Settings")]
    public bool autoPlay = true;
    public bool loopVideo = true;
    
    private VideoPlayer player;
    private RenderTexture videoTexture;
    private Renderer screenRenderer;
    private Material originalMaterial;
    private bool isReady = false;
    
    void Start()
    {
        SetupVideoPlayer();
        SetupScreen();
        
        if (autoPlay)
        {
            Invoke("PlayVideo", 1f);
        }
    }
    
    void SetupVideoPlayer()
    {
        player = GetComponent<VideoPlayer>();
        if (player == null)
            player = gameObject.AddComponent<VideoPlayer>();
        
        videoTexture = new RenderTexture(1920, 1080, 0);
        videoTexture.Create();
        
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = videoTexture;
        player.isLooping = loopVideo;
        player.playOnAwake = false;
        player.waitForFirstFrame = true;
        
        player.prepareCompleted += OnVideoReady;
        player.errorReceived += OnVideoError;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        player.source = VideoSource.Url;
        player.url = webVideoURL;
#else
        if (desktopVideo != null)
        {
            player.source = VideoSource.VideoClip;
            player.clip = desktopVideo;
        }
        else
        {
            player.source = VideoSource.Url;
            player.url = webVideoURL;
        }
#endif
    }
    
    void SetupScreen()
    {
        if (screenTarget == null)
        {
            CreateScreen();
        }
        
        screenRenderer = screenTarget.GetComponent<Renderer>();
        if (screenRenderer != null)
        {
            originalMaterial = screenRenderer.material;
            
            if (screenMaterial == null)
            {
                screenMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
        }
    }
    
    void CreateScreen()
    {
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "VideoScreen";
        screen.transform.position = transform.position + transform.forward * 3f;
        screen.transform.LookAt(transform);
        screen.transform.localScale = Vector3.one * 2f;
        screenTarget = screen.transform;
    }
    
    void OnVideoReady(VideoPlayer vp)
    {
        isReady = true;
        
        if (screenMaterial != null && videoTexture != null)
        {
            screenMaterial.SetTexture("_BaseMap", videoTexture);
            screenMaterial.SetTexture("_MainTex", videoTexture);
            
            if (screenRenderer != null)
                screenRenderer.material = screenMaterial;
        }
        
        player.Play();
    }
    
    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogWarning("Video failed to load: " + message);
        
        if (screenRenderer != null && originalMaterial != null)
            screenRenderer.material = originalMaterial;
    }
    
    public void PlayVideo()
    {
        if (isReady)
        {
            player.Play();
        }
        else
        {
            player.Prepare();
        }
    }
    
    public void StopVideo()
    {
        if (player != null)
            player.Stop();
            
        if (screenRenderer != null && originalMaterial != null)
            screenRenderer.material = originalMaterial;
    }
    
    public void PauseVideo()
    {
        if (player != null)
            player.Pause();
    }
    
    public bool IsPlaying()
    {
        return player != null && player.isPlaying;
    }
    
    void OnDestroy()
    {
        if (videoTexture != null)
        {
            videoTexture.Release();
            DestroyImmediate(videoTexture);
        }
        
        if (screenRenderer != null && originalMaterial != null)
            screenRenderer.material = originalMaterial;
    }
}