// ============================================================================
// COMPLETE VIDEO PROJECTOR CONTROLLER - Full Implementation
// ============================================================================

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VideoProjectorController : MonoBehaviour
{
    [Header("Video Setup")]
    public VideoClip instructionVideo;
    public RenderTexture videoTexture; // This will be automatically created
    
    [Header("Projector Hardware")]
    public Transform projectorBody; // Your 3D projector model
    public Transform screenTarget; // The projection screen/wall
    public Transform projectorLens; // Where light comes from (optional)
    
    [Header("Projection Settings")]
    public ProjectionMethod projectionMethod = ProjectionMethod.BothMethods;
    public LayerMask projectionLayers = -1;
    public float projectionDistance = 10f;
    public float fieldOfView = 60f;
    public Color projectorTint = Color.white;
    [Range(0f, 1f)]
    public float projectionIntensity = 0.8f;
    
    [Header("Video Settings")]
    public bool autoPlayOnStart = true;
    public bool loopVideo = true;
    public bool playAudioFromProjector = true;
    [Range(0f, 1f)]
    public float videoVolume = 0.7f;
    
    [Header("Lighting Effects")]
    public bool enableProjectorLight = true;
    public Color lightColor = new Color(1f, 0.95f, 0.8f); // Warm white
    public float lightIntensity = 2f;
    public bool enableDustParticles = true;
    
    [Header("Screen Materials")]
    public Material screenOffMaterial; // When projector is off
    public Material projectorShaderMaterial; // For Unity projector
    
    public enum ProjectionMethod
    {
        UnityProjector,     // Uses Unity's Projector component
        ScreenMaterial,     // Direct material replacement
        BothMethods         // Both for maximum effect
    }
    
    // Private components - All automatically created
    private VideoPlayer videoPlayer;
    private RenderTexture videoRenderTexture;
    private Projector unityProjector;
    private Light projectorLight;
    private AudioSource audioSource;
    private ParticleSystem dustParticles;
    
    // Materials - All automatically created
    private Material projectorMaterial;
    private Material screenMaterial;
    private Material originalScreenMaterial;
    
    // State tracking
    private bool isInitialized = false;
    private bool isPlaying = false;
    private bool isPaused = false;
    private Renderer screenRenderer;
    
    // Video texture settings
    private int videoTextureWidth = 1920;
    private int videoTextureHeight = 1080;
    
    void Start()
    {
        StartCoroutine(InitializeProjectorSystem());
    }
    
    // ============================================================================
    // COMPLETE INITIALIZATION SYSTEM
    // ============================================================================
    
    IEnumerator InitializeProjectorSystem()
    {
        Debug.Log("=== COMPLETE VIDEO PROJECTOR INITIALIZATION ===");
        
        // Step 1: Validate setup
        if (!ValidateSetup())
        {
            Debug.LogError("Projector setup validation failed!");
            yield break;
        }
        
        // Step 2: Create video render texture
        CreateVideoRenderTexture();
        yield return null;
        
        // Step 3: Setup video player
        SetupVideoPlayer();
        yield return null;
        
        // Step 4: Create all materials
        CreateAllMaterials();
        yield return null;
        
        // Step 5: Setup projection methods
        SetupProjectionSystems();
        yield return null;
        
        // Step 6: Setup lighting effects
        SetupLightingEffects();
        yield return null;
        
        // Step 7: Setup particle effects
        if (enableDustParticles)
        {
            SetupDustParticles();
            yield return null;
        }
        
        // Step 8: Setup audio
        SetupProjectorAudio();
        yield return null;
        
        // Step 9: Final configuration
        ConfigureScreenTarget();
        yield return null;
        
        isInitialized = true;
        Debug.Log("✅ Video Projector System FULLY INITIALIZED");
        
        // Step 10: Auto-start if enabled
        if (autoPlayOnStart)
        {
            yield return new WaitForSeconds(1f); // Brief delay
            PlayVideo();
        }
    }
    
    bool ValidateSetup()
    {
        bool isValid = true;
        
        if (instructionVideo == null)
        {
            Debug.LogError("❌ No instruction video assigned!");
            isValid = false;
        }
        
        if (screenTarget == null)
        {
            Debug.LogWarning("⚠️ No screen target assigned - will create default");
            CreateDefaultScreen();
        }
        
        Debug.Log($"Validation result: {(isValid ? "✅ PASSED" : "❌ FAILED")}");
        return isValid;
    }
    
    void CreateDefaultScreen()
    {
        // Create a default projection screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Auto_ProjectionScreen";
        screen.transform.position = transform.position + transform.forward * 5f;
        screen.transform.LookAt(transform);
        screen.transform.localScale = new Vector3(4f, 3f, 1f);
        
        screenTarget = screen.transform;
        Debug.Log("✅ Created default projection screen");
    }
    
    // ============================================================================
    // VIDEO RENDER TEXTURE CREATION - COMPLETE
    // ============================================================================
    
    void CreateVideoRenderTexture()
    {
        Debug.Log("Creating video render texture...");
        
        // Determine optimal texture size
        if (instructionVideo != null)
        {
            videoTextureWidth = Mathf.NextPowerOfTwo((int)instructionVideo.width);
            videoTextureHeight = Mathf.NextPowerOfTwo((int)instructionVideo.height);
            
            // Clamp to reasonable sizes for performance
            videoTextureWidth = Mathf.Clamp(videoTextureWidth, 512, 2048);
            videoTextureHeight = Mathf.Clamp(videoTextureHeight, 512, 2048);
        }
        
        // Create the render texture
        videoRenderTexture = new RenderTexture(videoTextureWidth, videoTextureHeight, 0, RenderTextureFormat.ARGB32)
        {
            name = "ProjectorVideoTexture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            useMipMap = false,
            autoGenerateMips = false
        };
        
        videoRenderTexture.Create();
        
        // Assign to public field for inspector reference
        videoTexture = videoRenderTexture;
        
        Debug.Log($"✅ Video render texture created: {videoTextureWidth}x{videoTextureHeight}");
    }
    
    // ============================================================================
    // VIDEO PLAYER SETUP - COMPLETE
    // ============================================================================
    
    void SetupVideoPlayer()
    {
        Debug.Log("Setting up video player...");
        
        // Create or get video player component
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }
        
        // Configure video player completely
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = instructionVideo;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        
        // Subscribe to all video events
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnVideoLooped;
        videoPlayer.errorReceived += OnVideoError;
        
        Debug.Log("✅ Video player configured");
    }
    
    // ============================================================================
    // MATERIAL CREATION - COMPLETE
    // ============================================================================
    
    void CreateAllMaterials()
    {
        Debug.Log("Creating all materials...");
        
        CreateProjectorMaterial();
        CreateScreenMaterial();
        StoreOriginalScreenMaterial();
        
        Debug.Log("✅ All materials created");
    }
    
    void CreateProjectorMaterial()
    {
        // Try URP Decal shader first
        Shader projectorShader = Shader.Find("Universal Render Pipeline/Decal");
        
        // Fallback shaders
        if (projectorShader == null)
            projectorShader = Shader.Find("Legacy Shaders/Projector/Multiply");
        if (projectorShader == null)
            projectorShader = Shader.Find("Projector/Multiply");
        if (projectorShader == null)
            projectorShader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
        
        projectorMaterial = new Material(projectorShader)
        {
            name = "ProjectorMaterial_Auto"
        };
        
        // Configure material properties
        projectorMaterial.SetTexture("_MainTex", videoRenderTexture);
        projectorMaterial.SetColor("_Color", projectorTint);
        
        if (projectorMaterial.HasProperty("_Alpha"))
            projectorMaterial.SetFloat("_Alpha", projectionIntensity);
        if (projectorMaterial.HasProperty("_Intensity"))
            projectorMaterial.SetFloat("_Intensity", projectionIntensity);
        
        Debug.Log($"✅ Projector material created with shader: {projectorShader.name}");
    }
    
    void CreateScreenMaterial()
    {
        // Create emissive screen material
        Shader screenShader = Shader.Find("Universal Render Pipeline/Lit");
        if (screenShader == null)
            screenShader = Shader.Find("Standard");
        
        screenMaterial = new Material(screenShader)
        {
            name = "ScreenMaterial_Auto"
        };
        
        // Configure for video display
        screenMaterial.SetTexture("_BaseMap", videoRenderTexture);
        screenMaterial.SetTexture("_MainTex", videoRenderTexture);
        screenMaterial.SetColor("_BaseColor", Color.white);
        
        // Enable emission for glow effect
        screenMaterial.EnableKeyword("_EMISSION");
        screenMaterial.SetTexture("_EmissionMap", videoRenderTexture);
        screenMaterial.SetColor("_EmissionColor", Color.white * 0.5f);
        screenMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        Debug.Log("✅ Screen material created");
    }
    
    void StoreOriginalScreenMaterial()
    {
        if (screenTarget != null)
        {
            screenRenderer = screenTarget.GetComponent<Renderer>();
            if (screenRenderer != null)
            {
                originalScreenMaterial = screenRenderer.material;
                Debug.Log("✅ Original screen material stored");
            }
        }
    }
    
    // ============================================================================
    // PROJECTION SYSTEMS SETUP - COMPLETE
    // ============================================================================
    
    void SetupProjectionSystems()
    {
        Debug.Log("Setting up projection systems...");
        
        if (projectionMethod == ProjectionMethod.UnityProjector || projectionMethod == ProjectionMethod.BothMethods)
        {
            SetupUnityProjector();
        }
        
        if (projectionMethod == ProjectionMethod.ScreenMaterial || projectionMethod == ProjectionMethod.BothMethods)
        {
            SetupScreenMaterialProjection();
        }
        
        Debug.Log("✅ Projection systems configured");
    }
    
    void SetupUnityProjector()
    {
        // Create Unity Projector component
        unityProjector = GetComponent<Projector>();
        if (unityProjector == null)
        {
            unityProjector = gameObject.AddComponent<Projector>();
        }
        
        // Configure projector settings
        unityProjector.orthographic = false;
        unityProjector.fieldOfView = fieldOfView;
        unityProjector.nearClipPlane = 0.1f;
        unityProjector.farClipPlane = projectionDistance;
        unityProjector.ignoreLayers = ~projectionLayers;
        unityProjector.material = projectorMaterial;
        
        // Point toward screen
        if (screenTarget != null)
        {
            transform.LookAt(screenTarget.position);
        }
        
        Debug.Log("✅ Unity Projector configured");
    }
    
    void SetupScreenMaterialProjection()
    {
        if (screenRenderer != null)
        {
            // This will be applied when video starts playing
            Debug.Log("✅ Screen material projection ready");
        }
    }
    
    // ============================================================================
    // LIGHTING EFFECTS - COMPLETE
    // ============================================================================
    
    void SetupLightingEffects()
    {
        if (!enableProjectorLight) return;
        
        Debug.Log("Setting up projector lighting...");
        
        // Create spotlight
        projectorLight = GetComponent<Light>();
        if (projectorLight == null)
        {
            projectorLight = gameObject.AddComponent<Light>();
        }
        
        // Configure light
        projectorLight.type = LightType.Spot;
        projectorLight.color = lightColor;
        projectorLight.intensity = lightIntensity;
        projectorLight.range = projectionDistance;
        projectorLight.spotAngle = fieldOfView + 10f; // Slightly wider than projection
        projectorLight.innerSpotAngle = fieldOfView - 10f;
        projectorLight.shadows = LightShadows.Soft;
        projectorLight.enabled = false; // Will be enabled when video plays
        
        Debug.Log("✅ Projector lighting configured");
    }
    
    // ============================================================================
    // DUST PARTICLE EFFECTS - COMPLETE
    // ============================================================================
    
    void SetupDustParticles()
    {
        Debug.Log("Setting up dust particle effects...");
        
        GameObject particleGO = new GameObject("ProjectorDustParticles");
        particleGO.transform.SetParent(transform);
        particleGO.transform.localPosition = Vector3.zero;
        
        dustParticles = particleGO.AddComponent<ParticleSystem>();
        
        // Configure particle system
        var main = dustParticles.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.5f;
        main.startSize = 0.02f;
        main.startColor = new Color(1f, 1f, 1f, 0.1f);
        main.maxParticles = 100;
        
        var emission = dustParticles.emission;
        emission.rateOverTime = 20f;
        
        var shape = dustParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = fieldOfView * 0.5f;
        shape.radius = 0.1f;
        shape.length = projectionDistance * 0.8f;
        
        var velocityOverLifetime = dustParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        
        var sizeOverLifetime = dustParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0.1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        dustParticles.Stop(); // Will start when video plays
        
        Debug.Log("✅ Dust particle effects configured");
    }
    
    // ============================================================================
    // AUDIO SETUP - COMPLETE
    // ============================================================================
    
    void SetupProjectorAudio()
    {
        if (!playAudioFromProjector) return;
        
        Debug.Log("Setting up projector audio...");
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure audio source
        audioSource.volume = 0;
        audioSource.spatialBlend = 1f; // Full 3D
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.playOnAwake = false;
        
        // Connect to video player
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        
        Debug.Log("✅ Projector audio configured");
    }
    
    // ============================================================================
    // SCREEN TARGET CONFIGURATION - COMPLETE
    // ============================================================================
    
    void ConfigureScreenTarget()
    {
        if (screenTarget == null) return;
        
        Debug.Log("Configuring screen target...");
        
        // Ensure screen has proper components
        if (screenTarget.GetComponent<Collider>() == null)
        {
            MeshCollider collider = screenTarget.gameObject.AddComponent<MeshCollider>();
            collider.convex = false;
        }
        
        // Set up layer for projection
        if (projectionLayers != 0)
        {
            int targetLayer = GetFirstSetBit(projectionLayers);
            if (targetLayer >= 0)
            {
                screenTarget.gameObject.layer = targetLayer;
            }
        }
        
        Debug.Log("✅ Screen target configured");
    }
    
    int GetFirstSetBit(LayerMask mask)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
                return i;
        }
        return 0;
    }
    
    // ============================================================================
    // VIDEO CONTROL METHODS - COMPLETE
    // ============================================================================
    
    public void PlayVideo()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Projector not initialized yet!");
            return;
        }
        
        if (instructionVideo == null)
        {
            Debug.LogError("No video assigned!");
            return;
        }
        
        Debug.Log("🎬 Starting video playback...");
        
        if (isPaused)
        {
            videoPlayer.Play();
            isPaused = false;
        }
        else
        {
            videoPlayer.Prepare();
        }
    }
    
    public void StopVideo()
    {
        if (!isInitialized || !isPlaying) return;
        
        Debug.Log("⏹️ Stopping video...");
        
        videoPlayer.Stop();
        isPlaying = false;
        isPaused = false;
        
        // Turn off effects
        TurnOffProjectorEffects();
        
        // Restore original screen material
        RestoreScreenMaterial();
    }
    
    public void PauseVideo()
    {
        if (!isPlaying) return;
        
        Debug.Log("⏸️ Pausing video...");
        videoPlayer.Pause();
        isPaused = true;
    }
    
    public void ResumeVideo()
    {
        if (!isPaused) return;
        
        Debug.Log("▶️ Resuming video...");
        videoPlayer.Play();
        isPaused = false;
    }
    
    // ============================================================================
    // PROJECTOR EFFECTS CONTROL - COMPLETE
    // ============================================================================
    
    void TurnOnProjectorEffects()
    {
        // Enable projector light
        if (projectorLight != null)
        {
            projectorLight.enabled = true;
        }
        
        // Start dust particles
        if (dustParticles != null)
        {
            dustParticles.Play();
        }
        
        // Apply screen material
        if (projectionMethod == ProjectionMethod.ScreenMaterial || projectionMethod == ProjectionMethod.BothMethods)
        {
            ApplyScreenMaterial();
        }
        
        Debug.Log("✅ Projector effects turned ON");
    }
    
    void TurnOffProjectorEffects()
    {
        // Disable projector light
        if (projectorLight != null)
        {
            projectorLight.enabled = false;
        }
        
        // Stop dust particles
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }
        
        Debug.Log("✅ Projector effects turned OFF");
    }
    
    void ApplyScreenMaterial()
    {
        if (screenRenderer != null && screenMaterial != null)
        {
            screenRenderer.material = screenMaterial;
            Debug.Log("✅ Screen material applied");
        }
    }
    
    void RestoreScreenMaterial()
    {
        if (screenRenderer != null && originalScreenMaterial != null)
        {
            screenRenderer.material = originalScreenMaterial;
            Debug.Log("✅ Original screen material restored");
        }
    }
    
    // ============================================================================
    // VIDEO EVENT HANDLERS - COMPLETE
    // ============================================================================
    
    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("📹 Video prepared, starting playback...");
        videoPlayer.Play();
    }
    
    void OnVideoStarted(VideoPlayer vp)
    {
        isPlaying = true;
        isPaused = false;
        
        Debug.Log("🎬 Video started playing!");
        
        // Turn on all projector effects
        TurnOnProjectorEffects();
        
        // Notify other systems
        if (GameController.Instance != null)
        {
            // You can add events here
        }
    }
    
    void OnVideoLooped(VideoPlayer vp)
    {
        Debug.Log("🔄 Video loop completed");
    }
    
    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"❌ Video error: {message}");
    }
    
    // ============================================================================
    // UTILITY METHODS - COMPLETE
    // ============================================================================
    
    public bool IsVideoPlaying() => isPlaying && !isPaused;
    public bool IsVideoPaused() => isPaused;
    public bool IsProjectorInitialized() => isInitialized;
    
    public float GetVideoProgress()
    {
        if (videoPlayer != null && instructionVideo != null)
        {
            return (float)(videoPlayer.time / instructionVideo.length);
        }
        return 0f;
    }
    
    public void SetProjectionIntensity(float intensity)
    {
        projectionIntensity = Mathf.Clamp01(intensity);
        
        if (projectorMaterial != null)
        {
            if (projectorMaterial.HasProperty("_Alpha"))
                projectorMaterial.SetFloat("_Alpha", projectionIntensity);
        }
        
        if (projectorLight != null)
        {
            projectorLight.intensity = lightIntensity * projectionIntensity;
        }
    }
    
    // ============================================================================
    // CLEANUP - COMPLETE
    // ============================================================================
    
    void OnDestroy()
    {
        Debug.Log("🧹 Cleaning up video projector...");
        
        // Unsubscribe from events
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.loopPointReached -= OnVideoLooped;
            videoPlayer.errorReceived -= OnVideoError;
        }
        
        // Release render texture
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            DestroyImmediate(videoRenderTexture);
        }
        
        // Destroy created materials
        if (projectorMaterial != null)
            DestroyImmediate(projectorMaterial);
        if (screenMaterial != null)
            DestroyImmediate(screenMaterial);
        
        // Restore original screen material
        RestoreScreenMaterial();
    }
    
    // ============================================================================
    // DEBUG METHODS - COMPLETE
    // ============================================================================
    
    [ContextMenu("▶️ Play Video")]
    public void DEBUG_PlayVideo() => PlayVideo();
    
    [ContextMenu("⏹️ Stop Video")]
    public void DEBUG_StopVideo() => StopVideo();
    
    [ContextMenu("⏸️ Pause Video")]
    public void DEBUG_PauseVideo() => PauseVideo();
    
    [ContextMenu("🔄 Test Loop")]
    public void DEBUG_TestLoop()
    {
        if (videoPlayer != null)
        {
            videoPlayer.time = videoPlayer.length - 2f; // Jump near end
        }
    }
    
    [ContextMenu("📊 Show Status")]
    public void DEBUG_ShowStatus()
    {
        Debug.Log("=== PROJECTOR STATUS ===");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"Playing: {isPlaying}");
        Debug.Log($"Paused: {isPaused}");
        Debug.Log($"Video Progress: {GetVideoProgress():F2}");
        Debug.Log($"Render Texture: {(videoRenderTexture != null ? "✅" : "❌")}");
        Debug.Log($"Materials Created: {(projectorMaterial != null && screenMaterial != null ? "✅" : "❌")}");
    }
}