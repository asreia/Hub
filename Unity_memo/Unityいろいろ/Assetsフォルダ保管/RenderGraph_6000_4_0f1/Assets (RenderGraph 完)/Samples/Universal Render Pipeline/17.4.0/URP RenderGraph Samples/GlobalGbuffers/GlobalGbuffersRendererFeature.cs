using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

// このサンプル機能は gBuffer コンポーネントをグローバルに設定します。これ自体は何も描画しません。
// この機能を scriptable renderer に追加すると、それ以降の他のパスから gBuffer へグローバルとしてアクセスできます。
// 動作させるには、rendering path を Deferred に設定してください。

// gBuffer をグローバルに設定すると、パフォーマンスやメモリ使用量が悪化する可能性があります。
// 理想的には、テクスチャを自分で管理し、本当に必要なテクスチャだけに builder.UseTexture を呼び出す方がよいです。
public class GlobalGbuffersRendererFeature : ScriptableRendererFeature
{
    class GlobalGBuffersRenderPass : ScriptableRenderPass
    {
        Material m_Material;
        string m_PassName = "GBuffer コンポーネントをグローバル化";

        private static readonly int GBufferNormalSmoothnessIndex = 2;
        private static readonly int GbufferLightingIndex = 3;
        private static readonly int GBufferRenderingLayersIndex = 5;

        // パイプラインはすでにいくつかの場所で gBuffer の depth コンポーネントをグローバルに設定しているため、必要に応じてこのコードをコメント解除してください。
        // private static readonly int GbufferDepthIndex = 4;

        // optional と示されているコンポーネントは、パイプラインが要求した場合にのみ存在します。
        // たとえば rendering layers texture がない場合、_GBuffer5 には ShadowMask texture が入ります。
        private static readonly int[] s_GBufferShaderPropertyIDs = new int[]
        {
            // Albedo texture を含みます。
            Shader.PropertyToID("_GBuffer0"),

            // Specular Metallic texture を含みます。
            Shader.PropertyToID("_GBuffer1"),

            // Normals と Smoothness を含みます。他のシェーダーでは _CameraNormalsTexture として参照されます。
            Shader.PropertyToID("_GBuffer2"),

            // Lighting texture を含みます。
            Shader.PropertyToID("_GBuffer3"),

            // Depth texture を含みます。他のシェーダーでは _CameraDepthTexture として参照されます (optional)。
            Shader.PropertyToID("_GBuffer4"),

            // Rendering Layers texture を含みます。他のシェーダーでは _CameraRenderingLayersTexture として参照されます (optional)。
            Shader.PropertyToID("_GBuffer5"),

            // ShadowMask texture を含みます (optional)。
            Shader.PropertyToID("_GBuffer6")
        };

        private class PassData
        {
        }

        // 現在のパスの後で、gBuffer コンポーネントをグローバルとして設定します。
        // このパスの後、グローバル化された gBuffer コンポーネントは、
        // 'builder.UseTexture(gBuffer[i])' ではなく 'builder.UseAllGlobalTextures(true)' でアクセスできるようになります。
        // グローバルテクスチャを使うシェーダーは、このパスの ExecutePass 関数で行うような
        // 'material.SetTexture()' の呼び出しなしに、それらを取得できるようになります。
        private void SetGlobalGBufferTextures(IRasterRenderGraphBuilder builder, TextureHandle[] gBuffer)
        {
            // このループにより、_GBufferX テクスチャシェーダー ID を使うすべてのシェーダーから gBuffer にアクセスできるようになります。
            for (int i = 0; i < gBuffer.Length; i++)
            {
                if (i != GbufferLightingIndex && gBuffer[i].IsValid())
                    builder.SetGlobalTextureAfterPass(gBuffer[i], s_GBufferShaderPropertyIDs[i]);
            }

            // 一部のグローバルテクスチャは、URP 内部の特定のシェーダー ID を使ってアクセスされます。
            // その場所で gBuffer を使うには、対応する gBuffer コンポーネントを指すように ID を設定する必要があります。
            if (gBuffer[GBufferNormalSmoothnessIndex].IsValid())
            {
                // このパスの後、_CameraNormalsTexture を使うシェーダーは gBuffer の NormalsSmoothnessTexture コンポーネントを取得します。
                builder.SetGlobalTextureAfterPass(gBuffer[GBufferNormalSmoothnessIndex],
                    Shader.PropertyToID("_CameraNormalsTexture"));
            }
            
            // パイプラインはすでにいくつかの場所で gBuffer の depth コンポーネントをグローバルに設定しているため、必要に応じてこのコードをコメント解除してください。
            // if (GbufferDepthIndex < gBuffer.Length && gBuffer[GbufferDepthIndex].IsValid())
            // {
            //     // このパスの後、_CameraDepthTexture を使うシェーダーは gBuffer の Depth コンポーネントを取得します。
            //     // これは copy depth pass によってもグローバルに設定される点に注意してください。
            //     builder.SetGlobalTextureAfterPass(gBuffer[GbufferDepthIndex],
            //         Shader.PropertyToID("_CameraDepthTexture"));
            // }

            if (GBufferRenderingLayersIndex < gBuffer.Length && gBuffer[GBufferRenderingLayersIndex].IsValid())
            {
                // このパスの後、_CameraRenderingLayersTexture を使うシェーダーは gBuffer の RenderingLayersTexture コンポーネントを取得します。
                builder.SetGlobalTextureAfterPass(gBuffer[GBufferRenderingLayersIndex],
                    Shader.PropertyToID("_CameraRenderingLayersTexture"));
            }
        }

        // RecordRenderGraph では RenderGraph ハンドルにアクセスでき、このハンドルを通してレンダーパスをグラフへ追加できます。
        // FrameData は、URP リソースへアクセスして管理するためのコンテキストコンテナです。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            // gBuffer コンポーネントは deferred モードでのみ使われます。
            if (universalRenderingData.renderingMode != RenderingMode.Deferred)
                return;
            
            // resourceData に格納されている gBuffer テクスチャハンドルを取得します。
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle[] gBuffer = resourceData.gBuffer;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_PassName, out var passData))
            {
                builder.AllowPassCulling(false);
                // このパスの後で gBuffer をグローバルに設定します。
                SetGlobalGBufferTextures(builder, gBuffer);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) => { /* 描画するものはありません。 */ });
            }
        }
    }

    GlobalGBuffersRenderPass m_GlobalGbuffersRenderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_GlobalGbuffersRenderPass = new GlobalGBuffersRenderPass
        {
            // このパスは deferred lights の描画後、またはそれ以降に差し込む必要があります。
            renderPassEvent = RenderPassEvent.AfterRenderingDeferredLights
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_GlobalGbuffersRenderPass);
    }
}
