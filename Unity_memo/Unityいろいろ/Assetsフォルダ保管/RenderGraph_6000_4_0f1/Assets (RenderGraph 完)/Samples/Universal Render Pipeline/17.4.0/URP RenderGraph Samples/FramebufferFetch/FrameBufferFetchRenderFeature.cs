using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// このサンプルは、カスタムマテリアルと framebuffer fetch を使って、前のパスのターゲットを新しいテクスチャへコピーします。
// これは API の説明用サンプルなので、新しいテクスチャはこのフレーム内の他の場所では使われません。
// 内容は Frame Debugger で確認できます。

// Framebuffer fetch は、サブパスが前のサブパスの出力をフレームバッファから直接読めるようにする、高度な TBDR GPU 向け最適化です。
// これにより、帯域使用量を大きく削減できます。
public class FrameBufferFetchRenderFeature : ScriptableRendererFeature
{
    class FrameBufferFetchPass : ScriptableRenderPass
    {
        private Material m_BlitMaterial;
        private Material m_FBFetchMaterial;
        
        public FrameBufferFetchPass( Material fbFetchMaterial)
        {
            m_FBFetchMaterial = fbFetchMaterial;

            // このパスは現在のカラーテクスチャを読み取ります。
            // そのためには中間テクスチャが必要で、BackBuffer を入力テクスチャとして使うことはサポートされていません。
            // このプロパティを設定すると、URP が自動的に中間テクスチャを作成します。
            // ただしパフォーマンスコストがあるため、不要な場合は設定しないでください。
            // これを RenderFeature 側ではなくここで設定するのがよい実践です。
            // そうすることで、このパス自体が自己完結になり、
            // RenderFeature を使わずに MonoBehaviour から直接このパスを enqueue できるようになります。
            requiresIntermediateTexture = true;
        }
        
        // このクラスはパスに必要なデータを保持し、パスを実行するデリゲート関数へ引数として渡されます。
        private class PassData
        {
            internal TextureHandle src;
            internal Material material;
            internal bool useMSAA;
        }
        
        // この static メソッドはパスを実行するために使われ、RenderGraph レンダーパスの RenderFunc デリゲートとして渡されます。
        static void ExecuteFBFetchPass(PassData data, RasterGraphContext context)
        {
            context.cmd.DrawProcedural(Matrix4x4.identity, data.material, data.useMSAA? 1 : 0, MeshTopology.Triangles, 3, 1, null);
        }
        
        private void FBFetchPass(RenderGraph renderGraph, ContextContainer frameData, TextureHandle source, TextureHandle destination, bool useMSAA)
        {
            string passName = "Framebuffer Fetch パス";
            
            // このシンプルなパスは、カスタムマテリアルと framebuffer fetch を使って、前のパスのターゲットを新しいテクスチャへコピーします。
            // これは API の説明用サンプルなので、新しいテクスチャはこのフレーム内の他の場所では使われません。
            // 内容は Frame Debugger で確認できます。

            // 名前と ExecutePass 関数へ渡すデータ型を指定して、RenderGraph に raster render pass を追加します。
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // パスデータを設定します。
                passData.material = m_FBFetchMaterial;
                passData.useMSAA = useMSAA;

                // src を input attachment として宣言します。これは framebuffer fetch に必要です。
                builder.SetInputAttachment(source, 0, AccessFlags.Read);

                // 旧来の cmd.SetRenderTarget に相当する処理として、レンダーターゲットを設定します。
                builder.SetRenderAttachment(destination, 0);
                
                // このサンプルでは説明のために、このパスのカリングを無効化します。
                // 通常は出力先テクスチャが他の場所で使われないため、このパスはカリングされます。
                builder.AllowPassCulling(false);

                // パス実行時に RenderGraph から呼び出されるレンダーパスデリゲートへ ExecutePass 関数を割り当てます。
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) => ExecuteFBFetchPass(data, context));
            }
        }
        
        // ここで RenderGraph ハンドルにアクセスできます。
        // 各 ScriptableRenderPass は、この RenderGraph ハンドルを使って RenderGraph に複数のレンダーパスを追加できます。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // このパスは framebuffer fetch の実装方法を示します。
            // framebuffer fetch は、サブパスが前のサブパスの出力をフレームバッファから直接読めるようにする高度な TBDR GPU 向け最適化で、
            // 帯域使用量を大きく削減できます。
            // 1 つ目の BlitPass は Camera Color を一時レンダーターゲットへ単純にコピーし、
            // 2 つ目の FBFetchPass は framebuffer fetch を使って、その一時レンダーターゲットを別のレンダーターゲットへコピーします。
            // その結果、これらのパスはマージされ、一時レンダーターゲットを破棄できるため帯域使用量が削減されます。
            // この挙動は RenderGraph Visualizer で確認できます。


            // UniversalResourceData には、レンダラーが使うすべてのテクスチャハンドルが含まれます。
            // これには、カメラが描き込むメインのカラー/深度バッファである、アクティブなカラー/深度テクスチャも含まれます。
            var resourceData = frameData.Get<UniversalResourceData>();

            // ここで出力先テクスチャを作成します。
            // このテクスチャは、アクティブなカラーテクスチャと同じサイズで作られます。
            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "Framebuffer Fetch 出力テクスチャ";
            destinationDesc.clearBuffer = false;

            if (destinationDesc.msaaSamples == MSAASamples.None || RenderGraphUtils.CanAddCopyPassMSAA())
            {
                TextureHandle fbFetchDestination = renderGraph.CreateTexture(destinationDesc);

                FBFetchPass(renderGraph, frameData, source, fbFetchDestination, destinationDesc.msaaSamples != MSAASamples.None);

                // Game View で結果を確認しやすくするため、FBF の出力を camera color へコピーし戻します。
                // この copy pass も内部では FBF を使います。
                // これによりすべてのパスがマージされ、出力先アタッチメントは memoryless、つまりメモリの load/store なしになるはずです。
                renderGraph.AddCopyPass(fbFetchDestination, source, passName: "Framebuffer Fetch 出力をコピーし戻し");
            }
            else
            {
                Debug.Log("MSAA のため、FBF パスとコピーパスを追加できません。");
            }
        }
    }

    FrameBufferFetchPass m_FbFetchPass;
    public Material m_FBFetchMaterial;

    /// <inheritdoc/>
    public override void Create()
    {
        m_FbFetchPass = new FrameBufferFetchPass(m_FBFetchMaterial);

        // レンダーパスをどこに差し込むか設定します。
        m_FbFetchPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // マテリアルがない場合は早期終了します。
        if (m_FBFetchMaterial == null)
        {
            Debug.LogWarning(this.name + " のマテリアルが null のため、スキップします。");
            return;
        }

        renderer.EnqueuePass(m_FbFetchPass);
    }
}
