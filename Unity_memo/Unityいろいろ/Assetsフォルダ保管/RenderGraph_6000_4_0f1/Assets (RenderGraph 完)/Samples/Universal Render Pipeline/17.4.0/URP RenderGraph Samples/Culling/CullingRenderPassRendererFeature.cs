using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CullingRenderPassRendererFeature : ScriptableRendererFeature
{
    // RendererList に入れるオブジェクトをフィルタリングするためのレイヤーマスクです。
    public LayerMask m_LayerMask;
    private CullingRenderPass m_CullingRenderPass;

    public override void Create()
    {
        m_CullingRenderPass = new CullingRenderPass(m_LayerMask);

        // レンダーパスをどこに差し込むか設定します。
        m_CullingRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_CullingRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_CullingRenderPass = null;
    }
}

public class CullingRenderPass : ScriptableRenderPass
{
    private LayerMask m_LayerMask;

    // RendererList を構築するために使うシェーダータグのリストです。
    private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    public CullingRenderPass(LayerMask layerMask)
    {
        m_LayerMask = layerMask;
    }

    class PassData
    {
        public RendererListHandle rendererListHandle;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var passName = "カリング結果から RendererList を描画";

        // このシンプルなパスは、現在のアクティブなカラーテクスチャをクリアした後、
        // カリング結果を使って m_LayerMask レイヤーに関連するシーンジオメトリを描画します。
        // 独自のカスタムレイヤーにシーンジオメトリを追加し、Render Feature UI でレイヤーマスクを切り替えて試してみてください。
        // パスの出力は Frame Debugger で確認できます。
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
        {
            // UniversalResourceData には、レンダラーが使うすべてのテクスチャハンドルが含まれます。
            // これには、カメラが描き込むメインのカラー/深度バッファである、アクティブなカラー/深度テクスチャも含まれます。
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var cameraData = frameData.Get<UniversalCameraData>();

            // CullContextData にはカリング API が含まれます。
            var cullContextData = frameData.Get<CullContextData>();

            // 使用中のカメラ用のカリングパラメーターを取得します。
            cameraData.camera.TryGetCullingParameters(false, out var cullingParameters);

            // CullContextData API を使ってカリングを実行します。
            var cullingResults = cullContextData.Cull(ref cullingParameters);

            // パスに必要なデータを passData に設定します。
            InitRendererLists(cullingResults, frameData, ref passData, renderGraph);

            // RendererList が有効であることを確認します。
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

    // この static メソッドはパスを実行するために使われ、RenderGraph レンダーパスの RenderFunc デリゲートとして渡されます。
    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.green, 1, 0);

        context.cmd.DrawRendererList(data.rendererListHandle);
    }

    // RenderGraph API を使って RendererList を作成する方法を示すサンプル用ユーティリティメソッドです。
    private void InitRendererLists(CullingResults cullResults, ContextContainer frameData, ref PassData passData, RenderGraph renderGraph)
    {
        // Universal Render Pipeline から関連する frame data にアクセスします。
        var universalRenderingData = frameData.Get<UniversalRenderingData>();
        var cameraData = frameData.Get<UniversalCameraData>();
        var lightData = frameData.Get<UniversalLightData>();

        var sortFlags = cameraData.defaultOpaqueSortFlags;
        var renderQueueRange = RenderQueueRange.opaque;
        var filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

        var forwardOnlyShaderTagIds = new ShaderTagId[]
        {
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("SRPDefaultUnlit"), // 後方互換性のため、gbuffer パスを持たないレガシーシェーダーは forward-only と見なされます。
                new ShaderTagId("LightweightForward") // 後方互換性のため、gbuffer パスを持たないレガシーシェーダーは forward-only と見なされます。
        };

        m_ShaderTagIdList.Clear();

        foreach (ShaderTagId sid in forwardOnlyShaderTagIds)
            m_ShaderTagIdList.Add(sid);

        var drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

        var param = new RendererListParams(cullResults, drawSettings, filterSettings);
        passData.rendererListHandle = renderGraph.CreateRendererList(param);
    }
}
