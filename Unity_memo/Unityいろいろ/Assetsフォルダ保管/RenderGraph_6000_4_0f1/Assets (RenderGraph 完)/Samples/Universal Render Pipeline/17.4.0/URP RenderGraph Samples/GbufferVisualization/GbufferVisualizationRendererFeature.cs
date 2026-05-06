using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

// このサンプルは、gBuffer コンポーネントがグローバルではない場合に、RenderPass 内でそれらを使う方法を示します。
// RenderPass はデフォルトで、シーン内のジオメトリ上に Specular Metallic Texture (_GBuffer2) の内容を表示します。
// ただし、サンプルシェーダーを変更することで、サンプリングする gBuffer コンポーネントを変えられます。
// 結果を確認するには、(1) レンダリングパスを Deferred に設定し、(2) シーンに 3D オブジェクトを追加してください。
public class GbufferVisualizationRendererFeature : ScriptableRendererFeature
{
    class GBufferVisualizationRenderPass : ScriptableRenderPass
    {
        Material m_Material;
        string m_PassName = "GBuffer コンポーネントを可視化";
        
        private static readonly int GbufferLightingIndex = 3;
        
        // その他の gBuffer コンポーネントのインデックスです。
        // private static readonly int GBufferNormalSmoothnessIndex = 2;
        // private static readonly int GbufferDepthIndex = 4;
        // private static readonly int GBufferRenderingLayersIndex = 5;

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
            // このサンプルでは、パス内で gBuffer コンポーネントを使います。
            public TextureHandle[] gBuffer;
            public Material material;
        }

        public void Setup(Material material)
        {
            m_Material = material;
        }

        // このメソッドは、シェーダーで要求された gBuffer コンポーネントの内容を描画します。
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // ここではサンプルとして、シェーダーが必要とするのは 1 つだけでも、すべての gBuffer コンポーネントを読み取ります。
            // gBuffer はグローバルにアクセスできないため、明示的に設定する必要があります。
            // そうしないと、シェーダーはデフォルトではアクセスできません。
            for (int i = 0; i < data.gBuffer.Length; i++)
            {
                data.material.SetTexture(s_GBufferShaderPropertyIDs[i], data.gBuffer[i]);
            }

            // シェーダーが要求した gBuffer コンポーネントをジオメトリ上に描画します。
            context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1);
        }

        // RecordRenderGraph では RenderGraph ハンドルにアクセスでき、このハンドルを通してレンダーパスをグラフへ追加できます。
        // FrameData は、URP リソースへアクセスして管理するためのコンテキストコンテナです。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            // gBuffer コンポーネントは deferred モードでのみ使われます。
            if (m_Material == null || universalRenderingData.renderingMode != RenderingMode.Deferred)
                return;

            // resourceData に格納されている gBuffer テクスチャハンドルを取得します。
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle[] gBuffer = resourceData.gBuffer;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_PassName, out var passData))
            {
                passData.material = m_Material;

                // このパスでは、deferred path における gBuffer Lighting コンポーネント (_GBuffer3) である activeColorTexture へ書き込みます。
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                // このパスでは gBuffer コンポーネントを読み取るため、それらに対して UseTexture を呼び出す必要があります。
                // gBuffer がグローバルな場合は builder.UseAllGlobalTexture(true) ですべて読めますが、
                // このパスではグローバルではありません。
                for (int i = 0; i < resourceData.gBuffer.Length; i++)
                {
                    if (i == GbufferLightingIndex)
                    {
                        // これは上で SetRenderAttachment により書き込み先として指定済みです。
                        continue;
                    }

                    builder.UseTexture(resourceData.gBuffer[i]);
                }

                // パスの実行時にアクセスできるよう、パスデータへ gBuffer を設定する必要があります。
                passData.gBuffer = gBuffer;

                // ExecutePass 関数をレンダーパスデリゲートへ割り当てます。これはパス実行時に RenderGraph から呼び出されます。
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }

    GBufferVisualizationRenderPass m_GBufferRenderPass;
    public Material m_Material;

    /// <inheritdoc/>
    public override void Create()
    {
        m_GBufferRenderPass = new GBufferVisualizationRenderPass
        {
            // このパスは deferred lights の描画後、またはそれ以降に差し込む必要があります。
            renderPassEvent = RenderPassEvent.AfterRenderingDeferredLights
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // gBuffer は Deferred rendering path でのみ使われます。
        if (m_Material != null)
        {
            m_GBufferRenderPass.Setup(m_Material);
            renderer.EnqueuePass(m_GBufferRenderPass);
        }
    }
}
