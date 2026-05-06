//AIに書かせたが一発で完璧(AddBlitPassPatternRendererFeature.cs, AddBlitPassPatternViewer.shader, codex://threads/019d917f-e660-74a3-80d4-dca1de923d6f)

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public sealed class AddBlitPassPatternRendererFeature : ScriptableRendererFeature
{
    const string k_DisplayShaderName = "Hidden/RenderGraph/AddBlitPassPatternViewer";

    static readonly int s_GoodMipTargetID = Shader.PropertyToID("_GoodMipTarget");
    static readonly int s_GoodArrayTargetID = Shader.PropertyToID("_GoodArrayTarget");
    static readonly int s_BadSelectedSliceTargetID = Shader.PropertyToID("_BadSelectedSliceTarget");
    static readonly int s_BadArrayCopyTargetID = Shader.PropertyToID("_BadArrayCopyTarget");

    [SerializeField] RenderPassEvent m_RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField, Range(16, 256)] int m_TextureSize = 64;
    [SerializeField] bool m_LogLegendOnce = true;

    Material m_DisplayMaterial;
    PatternPass m_Pass;

    public override void Create()
    {
        m_Pass = new PatternPass();
        m_Pass.renderPassEvent = m_RenderPassEvent;
        EnsureDisplayMaterial();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Pass == null)
            Create();

        if (!EnsureDisplayMaterial())
        {
            Debug.LogWarning($"{nameof(AddBlitPassPatternRendererFeature)}: display shader '{k_DisplayShaderName}' was not found.");
            return;
        }

        m_Pass.renderPassEvent = m_RenderPassEvent;
        m_Pass.Setup(m_DisplayMaterial, Mathf.Max(16, m_TextureSize), m_LogLegendOnce);
        renderer.EnqueuePass(m_Pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_DisplayMaterial);
        m_DisplayMaterial = null;
    }

    bool EnsureDisplayMaterial()
    {
        if (m_DisplayMaterial != null)
            return true;

        Shader shader = Shader.Find(k_DisplayShaderName);
        if (shader == null)
            return false;

        m_DisplayMaterial = CoreUtils.CreateEngineMaterial(shader);
        return m_DisplayMaterial != null;
    }

    sealed class PatternPass : ScriptableRenderPass
    {
        static readonly Color k_Clear = Color.black;
        static readonly Color k_Red = new Color(1f, 0f, 0f, 1f);
        static readonly Color k_Green = new Color(0f, 1f, 0f, 1f);
        static readonly Color k_Cyan = new Color(0f, 1f, 1f, 1f);

        Material m_DisplayMaterial;
        int m_TextureSize = 64;
        bool m_LogLegendOnce = true;
        bool m_HasLoggedLegend;
        bool m_HasWarnedBackBuffer;

        public PatternPass()
        {
            requiresIntermediateTexture = true;
        }

        public void Setup(Material displayMaterial, int textureSize, bool logLegendOnce)
        {
            m_DisplayMaterial = displayMaterial;
            m_TextureSize = textureSize;
            m_LogLegendOnce = logLegendOnce;
        }

        class InitPassData
        {
            public TextureHandle goodMipSource;
            public TextureHandle goodArraySource;
            public TextureHandle arraySource;
            public TextureHandle goodMipTarget;
            public TextureHandle goodArrayTarget;
            public TextureHandle badSelectedSliceTarget;
            public TextureHandle badArrayCopyTarget;
        }

        class DisplayPassData
        {
            public TextureHandle goodMipTarget;
            public TextureHandle goodArrayTarget;
            public TextureHandle badSelectedSliceTarget;
            public TextureHandle badArrayCopyTarget;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_DisplayMaterial == null)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                if (!m_HasWarnedBackBuffer)
                {
                    Debug.LogWarning($"{nameof(AddBlitPassPatternRendererFeature)}: active target is the back buffer. Move the pass earlier or keep the intermediate color texture enabled.");
                    m_HasWarnedBackBuffer = true;
                }
                return;
            }

            if (m_LogLegendOnce && !m_HasLoggedLegend)
            {
                Debug.Log(
                    "AddBlitPass pattern viewer:\n" +
                    "Top-left: Tex2D source, sourceSlice 0, numSlices 1, numMips 2. Expected mip1 = green.\n" +
                    "Top-middle: Tex2D source -> Texture2DArray destination slice 1. Expected = cyan.\n" +
                    "Top-right: Texture2DArray source slice 1 -> Tex2D. Expected if supported = green; mismatch shows the source-slice problem.\n" +
                    "Bottom-left: Texture2DArray source -> Texture2DArray destination slice 0. Expected = red.\n" +
                    "Bottom-middle: Texture2DArray source -> Texture2DArray destination slice 1. Expected = green; mismatch shows the array-source problem.\n" +
                    "Each panel's bottom strip is the expected color.");
                m_HasLoggedLegend = true;
            }

            int size = Mathf.Max(16, m_TextureSize);

            TextureHandle goodMipSource = renderGraph.CreateTexture(Create2DDesc(size, size, "ABP Good Source Tex2D Mips", true));
            TextureHandle goodMipTarget = renderGraph.CreateTexture(Create2DDesc(size, size, "ABP Good Target Tex2D Mips", true));

            TextureHandle goodArraySource = renderGraph.CreateTexture(Create2DDesc(size, size, "ABP Good Source Tex2D", false));
            TextureHandle goodArrayTarget = renderGraph.CreateTexture(CreateArrayDesc(size, size, "ABP Good Target Array", false));

            TextureHandle arraySource = renderGraph.CreateTexture(CreateArrayDesc(size, size, "ABP Bad Source Array", false));
            TextureHandle badSelectedSliceTarget = renderGraph.CreateTexture(Create2DDesc(size, size, "ABP Bad Selected Slice Target", false));
            TextureHandle badArrayCopyTarget = renderGraph.CreateTexture(CreateArrayDesc(size, size, "ABP Bad Array Copy Target", false));

            AddInitPass(renderGraph, goodMipSource, goodArraySource, arraySource, goodMipTarget, goodArrayTarget, badSelectedSliceTarget, badArrayCopyTarget);

            renderGraph.AddBlitPass(
                goodMipSource,
                goodMipTarget,
                Vector2.one,
                Vector2.zero,
                sourceSlice: 0,
                destinationSlice: 0,
                numSlices: 1,
                sourceMip: 0,
                destinationMip: 0,
                numMips: 2, //Goodだよ
                filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest,
                passName: "ABP Good Tex2D Mip Chain");

            renderGraph.AddBlitPass(
                goodArraySource,
                goodArrayTarget,
                Vector2.one,
                Vector2.zero,
                sourceSlice: 0,
                destinationSlice: 1, //Goodだよ
                numSlices: 1,
                sourceMip: 0,
                destinationMip: 0,
                numMips: 1,
                filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest,
                passName: "ABP Good Tex2D To Array Destination Slice");

            renderGraph.AddBlitPass(
                arraySource, //`.Tex2DArray`にした時点で死
                badSelectedSliceTarget,
                Vector2.one,
                Vector2.zero,
                sourceSlice: 1, //Badだよ
                destinationSlice: 0,
                numSlices: 1,
                sourceMip: 0,
                destinationMip: 0,
                numMips: 1,
                filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest,
                passName: "ABP Bad Array Source Slice To Tex2D");

            renderGraph.AddBlitPass(
                arraySource, //`.Tex2DArray`にした時点で死
                badArrayCopyTarget,
                Vector2.one,
                Vector2.zero,
                sourceSlice: 0,
                destinationSlice: 0,
                numSlices: 2, //Badだよ
                sourceMip: 0,
                destinationMip: 0,
                numMips: 1,
                filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest,
                passName: "ABP Bad Array Source All Slices");

            AddDisplayPass(renderGraph, resourceData.activeColorTexture, goodMipTarget, goodArrayTarget, badSelectedSliceTarget, badArrayCopyTarget, m_DisplayMaterial);
        }

        static TextureDesc Create2DDesc(int width, int height, string name, bool useMipMap)
        {
            return new TextureDesc(width, height)
            {
                name = name,
                format = GraphicsFormat.R8G8B8A8_UNorm,
                clearBuffer = true,
                clearColor = k_Clear,
                dimension = TextureDimension.Tex2D,
                slices = 1,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                msaaSamples = MSAASamples.None,
                useMipMap = useMipMap,
                autoGenerateMips = false
            };
        }

        static TextureDesc CreateArrayDesc(int width, int height, string name, bool useMipMap)
        {
            TextureDesc desc = Create2DDesc(width, height, name, useMipMap);
            desc.dimension = TextureDimension.Tex2DArray; //`.Tex2DArray`にした時点で死
            desc.slices = 2;
            return desc;
        }

        static void AddInitPass(
            RenderGraph renderGraph,
            TextureHandle goodMipSource,
            TextureHandle goodArraySource,
            TextureHandle arraySource,
            TextureHandle goodMipTarget,
            TextureHandle goodArrayTarget,
            TextureHandle badSelectedSliceTarget,
            TextureHandle badArrayCopyTarget)
        {
            using (var builder = renderGraph.AddUnsafePass<InitPassData>("ABP Init Test Textures", out var passData))
            {
                passData.goodMipSource = goodMipSource;
                passData.goodArraySource = goodArraySource;
                passData.arraySource = arraySource;
                passData.goodMipTarget = goodMipTarget;
                passData.goodArrayTarget = goodArrayTarget;
                passData.badSelectedSliceTarget = badSelectedSliceTarget;
                passData.badArrayCopyTarget = badArrayCopyTarget;

                builder.UseTexture(goodMipSource, AccessFlags.WriteAll);
                builder.UseTexture(goodArraySource, AccessFlags.WriteAll);
                builder.UseTexture(arraySource, AccessFlags.WriteAll);
                builder.UseTexture(goodMipTarget, AccessFlags.WriteAll);
                builder.UseTexture(goodArrayTarget, AccessFlags.WriteAll);
                builder.UseTexture(badSelectedSliceTarget, AccessFlags.WriteAll);
                builder.UseTexture(badArrayCopyTarget, AccessFlags.WriteAll);

                builder.SetRenderFunc(static (InitPassData data, UnsafeGraphContext context) => ExecuteInitPass(data, context));
            }
        }

        static void ExecuteInitPass(InitPassData data, UnsafeGraphContext context)
        {
            Clear(context.cmd, data.goodMipSource, 0, 0, k_Red);
            Clear(context.cmd, data.goodMipSource, 1, 0, k_Green);

            Clear(context.cmd, data.goodArraySource, 0, 0, k_Cyan);

            Clear(context.cmd, data.arraySource, 0, 0, k_Red);
            Clear(context.cmd, data.arraySource, 0, 1, k_Green);

            Clear(context.cmd, data.goodMipTarget, 0, 0, k_Clear);
            Clear(context.cmd, data.goodMipTarget, 1, 0, k_Clear);
            Clear(context.cmd, data.goodArrayTarget, 0, 0, k_Clear);
            Clear(context.cmd, data.goodArrayTarget, 0, 1, k_Clear);
            Clear(context.cmd, data.badSelectedSliceTarget, 0, 0, k_Clear);
            Clear(context.cmd, data.badArrayCopyTarget, 0, 0, k_Clear);
            Clear(context.cmd, data.badArrayCopyTarget, 0, 1, k_Clear);
        }

        static void Clear(UnsafeCommandBuffer cmd, TextureHandle texture, int mip, int slice, Color color)
        {
            cmd.SetRenderTarget(texture, mip, CubemapFace.Unknown, slice);
            cmd.ClearRenderTarget(false, true, color);
        }

        static void AddDisplayPass(
            RenderGraph renderGraph,
            TextureHandle cameraColor,
            TextureHandle goodMipTarget,
            TextureHandle goodArrayTarget,
            TextureHandle badSelectedSliceTarget,
            TextureHandle badArrayCopyTarget,
            Material material)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DisplayPassData>("ABP Display Pattern Results", out var passData))
            {
                passData.goodMipTarget = goodMipTarget;
                passData.goodArrayTarget = goodArrayTarget;
                passData.badSelectedSliceTarget = badSelectedSliceTarget;
                passData.badArrayCopyTarget = badArrayCopyTarget;
                passData.material = material;

                builder.UseTexture(goodMipTarget, AccessFlags.Read);
                builder.UseTexture(goodArrayTarget, AccessFlags.Read);
                builder.UseTexture(badSelectedSliceTarget, AccessFlags.Read);
                builder.UseTexture(badArrayCopyTarget, AccessFlags.Read);
                builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (DisplayPassData data, RasterGraphContext context) => ExecuteDisplayPass(data, context));
            }
        }

        static void ExecuteDisplayPass(DisplayPassData data, RasterGraphContext context)
        {
            data.material.SetTexture(s_GoodMipTargetID, data.goodMipTarget);
            data.material.SetTexture(s_GoodArrayTargetID, data.goodArrayTarget);
            data.material.SetTexture(s_BadSelectedSliceTargetID, data.badSelectedSliceTarget);
            data.material.SetTexture(s_BadArrayCopyTargetID, data.badArrayCopyTarget);

            context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);
        }
    }
}
