using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// このサンプルは、現在のアクティブなカラーテクスチャをクリアした後、m_LayerMask レイヤーに関連するシーンジオメトリを描画します。
// 独自のカスタムレイヤーにシーンジオメトリを追加し、Render Feature UI でレイヤーマスクを切り替えて試してみてください。
// パスの出力は Frame Debugger で確認できます。
public class RendererListRenderFeature : ScriptableRendererFeature
{
    class RendererListPass : ScriptableRenderPass
    {
        // RendererList に入れるオブジェクトをフィルタリングするためのレイヤーマスクです。
        private LayerMask m_LayerMask;
        
        // RendererList を構築するために使うシェーダータグのリストです。
        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        public RendererListPass(LayerMask layerMask)
        {
            m_LayerMask = layerMask;
        }
        
        // このクラスはパスに必要なデータを保持し、パスを実行するデリゲート関数へ引数として渡されます。
        private class PassData
        {
            public RendererListHandle rendererListHandle;
        }

        // RenderGraph API を使って RendererList を作成する方法を示すサンプル用ユーティリティメソッドです。
        private void InitRendererLists(ContextContainer frameData, ref PassData passData, RenderGraph renderGraph)
        {
            // Universal Render Pipeline から関連する frame data にアクセスします。
            UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            
            var sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
            FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);
            
            ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
            {
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("SRPDefaultUnlit"), // 後方互換性のため、gbuffer パスを持たないレガシーシェーダーは forward-only と見なされます。
                new ShaderTagId("LightweightForward") // 後方互換性のため、gbuffer パスを持たないレガシーシェーダーは forward-only と見なされます。
            };
            
            m_ShaderTagIdList.Clear();
            
            foreach (ShaderTagId sid in forwardOnlyShaderTagIds)
                m_ShaderTagIdList.Add(sid);
            
            DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

            var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
            passData.rendererListHandle = renderGraph.CreateRendererList(param);
        }

        // この static メソッドはパスを実行するために使われ、RenderGraph レンダーパスの RenderFunc デリゲートとして渡されます。
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.green, 1,0);
            
            context.cmd.DrawRendererList(data.rendererListHandle);
        }
        
        // ここで RenderGraph ハンドルにアクセスできます。
        // 各 ScriptableRenderPass は、この RenderGraph ハンドルを使って RenderGraph に複数のレンダーパスを追加できます。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            string passName = "RendererList 描画パス";
            
            // このシンプルなパスは、現在のアクティブなカラーテクスチャをクリアした後、
            // m_LayerMask レイヤーに関連するシーンジオメトリを描画します。
            // 独自のカスタムレイヤーにシーンジオメトリを追加し、Render Feature UI でレイヤーマスクを切り替えて試してみてください。
            // パスの出力は Frame Debugger で確認できます。

            // 名前と ExecutePass 関数へ渡すデータ型を指定して、RenderGraph に raster render pass を追加します。
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // UniversalResourceData には、レンダラーが使うすべてのテクスチャハンドルが含まれます。
                // これには、カメラが描き込むメインのカラー/深度バッファである、アクティブなカラー/深度テクスチャも含まれます。
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                
                // パスに必要なデータを passData に設定します。
                InitRendererLists(frameData, ref passData, renderGraph);
                
                // rendererList が有効か確認する任意チェックです。
                // 無効な場合、このパスは実行されません。これにより RenderGraph がエラーを投げる可能性を避けます。
                if (!passData.rendererListHandle.IsValid())
                    return;
                
                // 作成した RendererList を、このパスの入力依存関係として UseRendererList() で宣言します。
                builder.UseRendererList(passData.rendererListHandle);
                
                // 旧来の cmd.SetRenderTarget(color, depth) に相当する処理として、
                // SetRenderAttachment と SetRenderAttachmentDepth を使ってレンダーターゲットを設定します。
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                // パス実行時に RenderGraph から呼び出されるレンダーパスデリゲートへ ExecutePass 関数を割り当てます。
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }

    RendererListPass m_ScriptablePass;
    public LayerMask m_LayerMask;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new RendererListPass(m_LayerMask);

        // レンダーパスをどこに差し込むか設定します。
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
