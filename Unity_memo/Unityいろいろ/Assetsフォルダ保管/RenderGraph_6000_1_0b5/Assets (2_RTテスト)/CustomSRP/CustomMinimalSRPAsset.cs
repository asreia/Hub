using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Minimal SRP Asset")]
public class CustomMinimalSRPAsset : RenderPipelineAsset<CustomMinimalSRP>
{
    [Header("Fullscreen Pass")]
    public Shader fullscreenShader;
    public Material fullscreenMaterial;

    [Header("RT Settings")]
    [Range(1, 8)] public int msaaSamples = 1;
    public RenderTextureFormat colorFormat = RenderTextureFormat.DefaultHDR;
    public int depthBits = 24;
    public bool useSRGB = true;

    protected override RenderPipeline CreatePipeline()
    {
        if (fullscreenShader == null)
        {
            fullscreenShader = Shader.Find("Hidden/CustomSRP/FullscreenSample");
        }
        return new CustomMinimalSRP(this);
    }
}
