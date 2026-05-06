using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


// この RendererFeature は、Compute Shader を RenderGraph と組み合わせて使う方法を示します。

// このサンプルでは明示していませんが、Compute Shader はレンダーパスと一緒に実行できます。
// Compute Shader がレンダーパスでも使われるリソースを使用している場合、
// 2 つのレンダーパスの場合と同じように、パス間の依存関係が作成されます。
public class ComputeRendererFeature : ScriptableRendererFeature
{
    // Compute パスも通常の Scriptable Render Pass として扱います。
    class ComputePass : ScriptableRenderPass
    {
        // Compute Shader です。
        ComputeShader m_ComputeShader;

        // Compute バッファです。
        BufferHandle m_InputBufferHandle;
        BufferHandle m_OutputBufferHandle;

        // Compute Shader へ渡す入力データです。
        private List<int> inputData = new List<int>();

        // コンストラクタで入力データを初期化します。
        public ComputePass()
        {
            for (int i = 0; i < 20; i++)
            {
                inputData.Add(i);
            }
        }

        // RendererFeature からレンダーパスへ Compute Shader を渡すための Setup 関数です。
        public void Setup(ComputeShader cs)
        {
            m_ComputeShader = cs;
        }

        // PassData は、記録時のデータをパスの実行時へ渡すために使います。
        class PassData
        {
            // Compute Shader です。
            public ComputeShader cs;
            
            // Compute バッファ用のバッファハンドルです。
            public BufferHandle input;
            public BufferHandle output;
            public List<int> bufferData;
        }

        // ReadbackPassData は、指定した bufferHandle から非同期でデータを読み戻すために使います。
        class ReadbackPassData
        {
            public BufferHandle bufferHandle;
        }

        // RenderGraph に Compute パスと読み戻しパスを記録します。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // バッファを作成します。
            var bufferDesc = new BufferDesc()
            {
                name = "入力バッファ",
                count = 20,
                stride = sizeof(int),
                target = GraphicsBuffer.Target.Structured
            };
            m_InputBufferHandle = renderGraph.CreateBuffer(bufferDesc);
            
            bufferDesc.name = "出力バッファ";
            m_OutputBufferHandle = renderGraph.CreateBuffer(bufferDesc);

            // パス名を指定して RenderGraph パスの記録を開始し、
            // レンダー関数の実行時へデータを渡すための passData を受け取ります。
            // Compute を扱うため、ここでは "AddComputePass" を使います。
            using (var builder = renderGraph.AddComputePass("Compute パス", out PassData passData))
            {
                // 記録時から実行時へデータを渡せるよう、passData を設定します。
                passData.cs = m_ComputeShader;
                passData.input = m_InputBufferHandle;
                passData.output = m_OutputBufferHandle;
                passData.bufferData = inputData;
                
                // 処理前後を確認できるよう、入力データをコンソールへ出力します。
                Debug.Log($"入力データ: {string.Join(",", inputData)}");
                
                // UseBuffer は、読み書きフラグとともに RenderGraph の依存関係を設定するために使います。
                builder.UseBuffer(passData.input, AccessFlags.Read);
                builder.UseBuffer(passData.output, AccessFlags.Write);

                // builder.EnableAsyncCompute(true); //追加した

                // Compute パスでも、実行関数は SetRenderFunc で指定します。
                builder.SetRenderFunc(static (PassData data, ComputeGraphContext cgContext) => ExecutePass(data, cgContext));
            }

            // BufferHandle は RenderGraph に管理されるため、RenderGraph の実行完了後に直接データへアクセスすることはできません。
            // Compute Shader の出力データを使いたい場合は、出力バッファから読み取るためのパスを追加する必要があります。
            using (var builder = renderGraph.AddUnsafePass("読み戻しパス", out ReadbackPassData passData))
            {
                builder.AllowPassCulling(false);

                // 読み取り元のバッファを指定します。
                passData.bufferHandle = m_OutputBufferHandle;
                builder.UseBuffer(passData.bufferHandle, AccessFlags.Read);
                builder.SetRenderFunc(static (ReadbackPassData data, UnsafeGraphContext ctx) =>
                {
                    ctx.cmd.RequestAsyncReadback(data.bufferHandle, (AsyncGPUReadbackRequest request) =>
                    {
                        var result = request.GetData<int>();
                        Debug.Log($"出力データ: {string.Join(",", result)}");
                    });
                });
            }
        }

        // ExecutePass は、RenderGraph の記録時に設定するレンダー関数です。
        // 呼び出し元のラムダ外にある変数を使わないようにするのがよい実践です。
        // static にすることで、意図しない挙動につながる可能性があるメンバー変数の利用を避けます。
        static void ExecutePass(PassData data, ComputeGraphContext cgContext)
        {
            // Compute バッファを設定します。
            cgContext.cmd.SetBufferData(data.input, data.bufferData);
            cgContext.cmd.SetComputeBufferParam(data.cs, data.cs.FindKernel("CSMain"), "inputData", data.input);
            cgContext.cmd.SetComputeBufferParam(data.cs, data.cs.FindKernel("CSMain"), "outputData", data.output);
            // 指定したカーネルをエントリポイントとして Compute Shader を Dispatch します。
            // スレッドグループ数により、そのカーネルを何グループ実行するかが決まります。
            cgContext.cmd.DispatchCompute(data.cs, data.cs.FindKernel("CSMain"), 1, 1, 1);
        }
    }

    [SerializeField]
    ComputeShader computeShader;
    ComputePass m_ComputePass;

    /// <inheritdoc/>
    public override void Create()
    {
        // Compute パスを初期化します。
        m_ComputePass = new ComputePass();
        // レンダリング前に実行されるよう、RendererFeature を設定します。
        m_ComputePass.renderPassEvent = RenderPassEvent.BeforeRendering;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // システムが Compute Shader をサポートしているか確認し、サポートしていなければ早期終了します。
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogWarning("このデバイスは Compute Shader をサポートしていないため、パスをスキップします。");
            return;
        }
        // Compute Shader が null の場合は、このレンダーパスをスキップします。
        if (computeShader == null)
        {
            Debug.LogWarning("Compute Shader が null のため、パスをスキップします。");
            return;
        }
        // レンダーパスの Setup を呼び出し、Compute Shader を渡します。
        m_ComputePass.Setup(computeShader);
        // Compute パスを enqueue します。
        renderer.EnqueuePass(m_ComputePass);
    }
}
