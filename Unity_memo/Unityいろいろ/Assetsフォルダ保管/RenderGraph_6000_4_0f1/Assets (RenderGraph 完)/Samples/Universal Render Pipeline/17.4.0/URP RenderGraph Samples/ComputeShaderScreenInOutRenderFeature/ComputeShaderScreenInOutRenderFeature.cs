using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using Vector2 = UnityEngine.Vector2;

// この RendererFeature は、Compute Shader を RenderGraph に統合する方法を示します。
// このサンプルでは、Compute Shader の出力を使って CameraColor テクスチャを変更します。
// さらに、CameraColor テクスチャが更新された後、それを別の Compute Shader パスの入力として使います。

// このサンプルは、Unity Insider Program (https://unity.com/unity-insiders) に参加している
// Git-Amend 氏の動画 https://www.youtube.com/watch?v=v_WkGKn601M を元にしています。
// 元のサンプルでは、Compute Shader の出力画像は CameraColor テクスチャではなく RenderTexture に適用されています。

public class ComputeShaderScreenInOutRenderFeature : ScriptableRendererFeature 
{
    class HeatmapPass : ScriptableRenderPass 
    {
        // Compute Shader プログラムです。
        ComputeShader m_HeatmapComputeShader;
        ComputeShader m_HeatmapBrightnessComputeShader;
        
        // 各 Compute Shader のカーネルです。
        int m_KernelHeatMapComputeShader;
        int m_KernelHeatmapBrightnessComputeShader;

        // ヒートマップ用の Compute Shader です。Compute Shader を使って、動き回る敵の集団をシミュレートします。
        BufferHandle m_EnemyBuffer;
        Vector2[] m_EnemyPositions;
        const int k_EnemyCount = 64;

        // 後で RenderGraph から使うための TextureHandle です。
        TextureHandle m_HeatmapTextureHandle;
        TextureHandle m_HeatmapBrightnessTextureHandle;

        public void Setup(ComputeShader heatmapCS, ComputeShader heatmapBrightnessCS)
        {
            // ここで 2 つの Compute Shader を設定します。
            // 1 つ目の Compute Shader は、CameraColor に格納される出力を生成します。
            // 2 つ目の Compute Shader は、その CameraColor を入力として受け取り、さらに処理して最終結果を生成します。
            m_HeatmapComputeShader = heatmapCS;
            m_HeatmapBrightnessComputeShader = heatmapBrightnessCS;
            m_KernelHeatMapComputeShader = heatmapCS.FindKernel("CSMain");
            m_KernelHeatmapBrightnessComputeShader = heatmapBrightnessCS.FindKernel("CSMain");
            
            // 敵の位置を初期化します。
            m_EnemyPositions = new Vector2[k_EnemyCount];
        }

        // Compute パス用のデータです。
        // 2 つの Compute Shader の両方で使います。
        class ComputePassData 
        {
            public ComputeShader computeShader;
            public int kernel;
            public int enemyCount;
            public Vector2[] positions;// これにより、パス内で位置情報を使えるようになります。
            public BufferHandle enemyHandle;
            public TextureHandle input;
            public TextureHandle output;
            public int width;
            public int height;
        }
        
        void UpdateEnemyPositions(int width, int height)
        {
            for (int i = 0; i < k_EnemyCount; i++) {
                float t = Time.time * 0.5f + i * 0.1f;
                float x = Mathf.PerlinNoise(t, i * 1.31f) * width;
                float y = Mathf.PerlinNoise(i * 0.91f, t) * height;
                m_EnemyPositions[i] = new Vector2(x, y);
            }
        }
        
        // ここが RenderGraph システムの中心で、Compute Shader パスが毎フレーム実行されます。
        // Compute Shader パスの目的は、次の 3 ステップに要約できます。
        
        // 1- Perlin ノイズを使って敵の位置を更新し、それを GPU バッファへアップロードします。
        // 2- RenderGraph に 2 つの Compute Shader パスを設定します。
        //    1 つ目は敵の位置を元にヒートマップテクスチャを生成し、
        //    2 つ目は別の Compute Shader でその結果テクスチャへ少し明るさを加えて追加処理します。
        // 3- ある Compute Shader パスの結果テクスチャを次のパスへ渡し、最後にカメラのカラーバッファへ割り当てて描画します。
        public override void RecordRenderGraph(RenderGraph graph, ContextContainer context) 
        {
            // Universal Resource Data を取得します。
            // ここには、アクティブなカラーテクスチャや深度テクスチャなど、すべてのテクスチャリソースが含まれます。
            var resourceData = context.Get<UniversalResourceData>();
            
            // カメラカラーからサイズを取得します。
            var width = resourceData.cameraColor.GetDescriptor(graph).width;
            var height = resourceData.cameraColor.GetDescriptor(graph).height;
            
            // 敵の位置を更新します。
            UpdateEnemyPositions(width, height);

            // activeColorTexture の descriptor 値を元にテクスチャ descriptor を作成します。
            // このテクスチャ descriptor は、m_HeatmapTextureHandle と
            // m_HeatmapBrightnessTextureHandle の両方で使います。
            var heatmapDesc = resourceData.activeColorTexture.GetDescriptor(graph);
            
            // descriptor の属性をいくつか設定します。
            heatmapDesc.name = "ヒートマップ";
            heatmapDesc.enableRandomWrite = true;   // Compute Shader でテクスチャへ効率よく書き込むために使います。
                                                    // 逐次的なタイル書き込みではなく、ランダムなタイルアクセスを有効にします。
            heatmapDesc.msaaSamples = MSAASamples.None;
            
            // カメラカラーの descriptor を元に、m_HeatmapTextureHandle 用のテクスチャを作成します。
            m_HeatmapTextureHandle = graph.CreateTexture(heatmapDesc);
            
            // 先ほど作成した heatmapDesc を再利用し、今回は名前だけを変更します。
            heatmapDesc.name = "明るさ調整ヒートマップ";
            
            // カメラカラーの descriptor を元に、m_HeatmapBrightnessTextureHandle 用のテクスチャを作成します。
            m_HeatmapBrightnessTextureHandle = graph.CreateTexture(heatmapDesc);
            
            // バッファを作成します。
            var bufferDesc = new BufferDesc()
            {
                name = "敵位置バッファ",
                stride = sizeof(float) * 2,
                count = k_EnemyCount,
                target = GraphicsBuffer.Target.Structured
            };
            
            // これを RenderGraph に追加します。
            m_EnemyBuffer = graph.CreateBuffer(bufferDesc);

            // ここで Compute Shader のレンダーパスを定義し、
            // Compute Shader パスで処理するデータを割り当てます。
            using (var builder = graph.AddComputePass<ComputePassData>("ヒートマップ生成 Compute パス", out var passData))
            {
                // Compute Shader のデータへ値を割り当てます。
                passData.computeShader = m_HeatmapComputeShader;
                passData.kernel = m_KernelHeatMapComputeShader;
                passData.output = m_HeatmapTextureHandle;
                passData.enemyHandle = m_EnemyBuffer;
                passData.enemyCount = k_EnemyCount;
                passData.positions = m_EnemyPositions;
                passData.width = width;
                passData.height = height;

                // builder を使って、このパス内でのリソース使用を宣言します。
                builder.UseTexture(passData.output, AccessFlags.Write);
                builder.UseBuffer(passData.enemyHandle, AccessFlags.Read);

                // Compute Shader パスを実行する関数を設定します。パフォーマンス向上のため static を使っています。
                builder.SetRenderFunc(static(ComputePassData data, ComputeGraphContext ctx) =>
                {
                    // SetBufferData は、コマンドバッファを使って敵の位置データを
                    // passData.enemyHandle から passData.positions へ送ります。
                    ctx.cmd.SetBufferData(data.enemyHandle, data.positions); // data.enemyPositions を使うことで、
                                                                             // レンダー関数のスコープ内に収まるようにします。
                    ctx.cmd.SetComputeIntParam(data.computeShader, "k_EnemyCount", data.enemyCount);
                    ctx.cmd.SetComputeBufferParam(data.computeShader, data.kernel, "m_EnemyPositions", data.enemyHandle);
                    ctx.cmd.SetComputeTextureParam(data.computeShader, data.kernel, "heatmapTexture", data.output);
                    ctx.cmd.DispatchCompute(data.computeShader, data.kernel, Mathf.CeilToInt(data.width / 8f), Mathf.CeilToInt(data.height / 8f), 1);
                });
            }
            // resourceData.cameraColor = m_HeatmapTextureHandle;
            // ここで resourceData.cameraColor = m_HeatmapTextureHandle を設定し、2 つ目のパスをコメントアウトすると、
            // Compute パスの結果を 2 つ目のパスで再利用せずに、そのまま直接取得できます。

            // これは 2 つ目の Compute Shader レンダーパスです。
            // このパスでは、現在の `m_HeatmapTextureHandle` を入力とし、
            // brightness 用 Compute Shader で処理した後の出力を `m_HeatmapBrightnessTextureHandle` に格納します。
            using (var builder = graph.AddComputePass<ComputePassData>("ヒートマップ明るさ調整 Compute パス", out var passData))
            {
                // Compute Shader のデータへ値を割り当てます。
                passData.computeShader = m_HeatmapBrightnessComputeShader;
                passData.kernel = m_KernelHeatmapBrightnessComputeShader;
                passData.input = m_HeatmapTextureHandle;
                passData.output = m_HeatmapBrightnessTextureHandle;
                passData.width = width;
                passData.height = height;

                // builder を使って、このパス内でのリソース使用を宣言します。
                builder.UseTexture(passData.input, AccessFlags.Read);
                builder.UseTexture(passData.output, AccessFlags.Write);

                // Compute Shader パスを実行する関数を設定します。
                builder.SetRenderFunc(static(ComputePassData data, ComputeGraphContext ctx) =>
                {
                    ctx.cmd.SetComputeTextureParam(data.computeShader, data.kernel, "heatmapTexture", data.input);
                    ctx.cmd.SetComputeTextureParam(data.computeShader, data.kernel, "result", data.output);
                    ctx.cmd.DispatchCompute(data.computeShader, data.kernel, Mathf.CeilToInt(data.width / 8f), Mathf.CeilToInt(data.height / 8f), 1);
                });
            }
            
            // 最後の Compute Shader パスで得られたテクスチャを、現在の Camera Color へ割り当てます。
            resourceData.cameraColor = m_HeatmapBrightnessTextureHandle;
        }
    }
    
    // RendererFeature の Inspector 用フィールドです。
    [SerializeField] ComputeShader HeatmapComputeShader;
    [SerializeField] ComputeShader HeatmapBrightnessComputeShader;

    // HeatmapPass のインスタンスです。
    HeatmapPass heatmapPass;

    public override void Create() 
    {
        heatmapPass = new HeatmapPass
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) 
    {
        if (HeatmapComputeShader == null || HeatmapBrightnessComputeShader == null)
        {
            Debug.Log("ComputeShaderRendererFeature に 2 つのシェーダーを設定してください。");
            return;
        }
        
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.Log(
                "このシステムは Compute Shader をサポートしていないため、ComputeShaderRendererFeature を追加できません。");
        }

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            heatmapPass.Setup(HeatmapComputeShader, HeatmapBrightnessComputeShader);
            renderer.EnqueuePass(heatmapPass);
        }
    }
}
