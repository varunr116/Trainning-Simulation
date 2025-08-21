using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemGlowController : MonoBehaviour
{
    [Header("Glow Settings")]
    public Material urpGlowMaterial;
    public Color glowColor = Color.cyan;
    [Range(0f, 10f)]
    public float glowIntensity = 3f;
    
    [Header("Scale Settings")]
    public float hoverScaleMultiplier = 1.1f;
    public float selectedScaleMultiplier = 1.2f;
    public float scaleSpeed = 3f;
    
    [Header("Pulse Settings")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensityRange = 1f;
    
    
    public OutlineMethod outlineMethod = OutlineMethod.DuplicateMesh;
    
    public enum OutlineMethod
    {
        DuplicateMesh,
        MaterialSwap,
        URPRendererFeature
    }
    
    
    private List<Renderer> targetRenderers; // Gets data from InteractableItem
    private Vector3 originalScale;
    private List<GameObject> outlineObjects = new List<GameObject>();
    private bool isGlowing = false;
    private bool isHovered = false;
    private bool isSelected = false;
    private Coroutine pulseCoroutine;
    private Coroutine scaleCoroutine;
    
    // URP material properties
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    
    void Start()
    {
        originalScale = transform.localScale;
        
        if (urpGlowMaterial == null)
        {
            CreateURPGlowMaterial();
        }
    }
    
    void CreateURPGlowMaterial()
    {
        urpGlowMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        if (urpGlowMaterial.shader == null)
            urpGlowMaterial.shader = Shader.Find("URP/Lit");
        if (urpGlowMaterial.shader == null)
            urpGlowMaterial.shader = Shader.Find("Lit");
        
        urpGlowMaterial.SetColor(BaseColor, glowColor);
        urpGlowMaterial.SetFloat("_Metallic", 0f);
        urpGlowMaterial.SetFloat("_Smoothness", 0.8f);
        urpGlowMaterial.EnableKeyword("_EMISSION");
        urpGlowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        urpGlowMaterial.SetColor(EmissionColor, glowColor * glowIntensity);
    }
    
   
    
    public void SetTargetRenderers(List<Renderer> renderers)
    {
        targetRenderers = renderers;
      
    }
    
    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        
        if (hovered && !isSelected)
        {
            StartGlow(GlowType.Hover);
            StartScale(hoverScaleMultiplier);
        }
        else if (!hovered && !isSelected)
        {
            StopGlow();
            StartScale(1f);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            StartGlow(GlowType.Selected);
            StartScale(selectedScaleMultiplier);
        }
        else
        {
            StopGlow();
            StartScale(1f);
        }
    }
    
    public enum GlowType
    {
        Hover,
        Selected
    }
    
    void StartGlow(GlowType glowType)
    {
        if (targetRenderers == null || targetRenderers.Count == 0)
        {
            
            return;
        }
        
        if (isGlowing) StopGlow();
        
        switch (outlineMethod)
        {
            case OutlineMethod.DuplicateMesh:
                CreateOutlinesForRenderers(glowType);
                break;
            case OutlineMethod.MaterialSwap:
                SwapMaterialsForRenderers();
                break;
        }
        
        isGlowing = true;
        
        if (enablePulse && glowType == GlowType.Selected)
        {
            pulseCoroutine = StartCoroutine(PulseGlow());
        }
    }
    
    void StopGlow()
    {
        if (!isGlowing) return;
        
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        switch (outlineMethod)
        {
            case OutlineMethod.DuplicateMesh:
                DestroyAllOutlines();
                break;
            case OutlineMethod.MaterialSwap:
                // InteractableItem handles material restoration
                break;
        }
        
        isGlowing = false;
    }
    
  
    
    void CreateOutlinesForRenderers(GlowType glowType)
    {
        DestroyAllOutlines();
        
        float intensity = glowType == GlowType.Selected ? glowIntensity : glowIntensity * 0.6f;
        float outlineSize = glowType == GlowType.Selected ? 1.05f : 1.03f;
        
        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer == null) continue;
            
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null) continue;
            
            // Create outline
            GameObject outlineObject = new GameObject($"{renderer.name}_Outline");
            outlineObject.transform.SetParent(renderer.transform);
            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one * outlineSize;
            
            // Copy mesh
            MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
            outlineMeshFilter.mesh = meshFilter.mesh;
            
            // Add glow material
            MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
            Material instanceMaterial = new Material(urpGlowMaterial);
            instanceMaterial.SetColor(EmissionColor, glowColor * intensity);
            outlineRenderer.material = instanceMaterial;
            
            // Optimize rendering
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.sortingOrder = -1;
            
            outlineObjects.Add(outlineObject);
        }
        
       
    }
    
    void DestroyAllOutlines()
    {
        foreach (GameObject outlineObj in outlineObjects)
        {
            if (outlineObj != null)
                DestroyImmediate(outlineObj);
        }
        outlineObjects.Clear();
    }
    
    void SwapMaterialsForRenderers()
    {
        
    }
    
   
    
    IEnumerator PulseGlow()
    {
        List<Material> pulseMaterials = new List<Material>();
        
        foreach (GameObject outlineObj in outlineObjects)
        {
            if (outlineObj != null)
            {
                Renderer outlineRenderer = outlineObj.GetComponent<Renderer>();
                if (outlineRenderer != null)
                    pulseMaterials.Add(outlineRenderer.material);
            }
        }
        
        float baseIntensity = glowIntensity;
        
        while (isGlowing && isSelected)
        {
            // Pulse up
            for (float t = 0; t <= 1; t += Time.deltaTime * pulseSpeed)
            {
                float intensity = Mathf.Lerp(baseIntensity, baseIntensity + pulseIntensityRange, t);
                Color emissionColor = glowColor * intensity;
                
                foreach (Material mat in pulseMaterials)
                {
                    if (mat != null && mat.HasProperty(EmissionColor))
                        mat.SetColor(EmissionColor, emissionColor);
                }
                yield return null;
            }
            
            // Pulse down
            for (float t = 0; t <= 1; t += Time.deltaTime * pulseSpeed)
            {
                float intensity = Mathf.Lerp(baseIntensity + pulseIntensityRange, baseIntensity, t);
                Color emissionColor = glowColor * intensity;
                
                foreach (Material mat in pulseMaterials)
                {
                    if (mat != null && mat.HasProperty(EmissionColor))
                        mat.SetColor(EmissionColor, emissionColor);
                }
                yield return null;
            }
        }
    }
    
    
    
    void StartScale(float targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
            
        scaleCoroutine = StartCoroutine(ScaleToSize(originalScale * targetScale));
    }
    
    IEnumerator ScaleToSize(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;
        float duration = 1f / scaleSpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
    
   
    
    void OnDestroy()
    {
        StopGlow();
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
    }
    
    public void ResetToOriginal()
    {
        StopGlow();
        transform.localScale = originalScale;
    }
}