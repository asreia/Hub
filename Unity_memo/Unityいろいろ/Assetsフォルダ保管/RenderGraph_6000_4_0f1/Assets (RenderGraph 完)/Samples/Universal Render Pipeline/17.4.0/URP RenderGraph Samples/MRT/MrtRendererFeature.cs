using System;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// このサンプルは、URP で RenderGraph を使って Multiple Render Targets (MRT) を扱う方法を示します。
// 1 つのパスで 4 チャンネルを超えるデータ、つまり単一の RGBA テクスチャに収まらないデータを書き込みたい場合に便利です。
public class MrtRendererFeature : ScriptableRendererFeature
{
    // このパスは MRT を使い、3 つの異なる Render Target へ出力します。
    class MrtPass : ScriptableRenderPass
    {
        // 記録後、レンダー関数へ渡したいデータです。
        class PassData
        {
            // カラー入力用のテクスチャハンドルです。
            public TextureHandle color;
            // マテリアルで使う入力テクスチャ名です。
            public string texName;
            // MRT Pass で使うマテリアルです。
            public Material material;
        }

        // マテリアルで使う入力テクスチャ名です。
        string m_texName;
        // MRT Pass で使うマテリアルです。
        Material m_Material;
        // MRT の出力先用 RTHandle です。
        RTHandle[] m_RTs = new RTHandle[3];
        RenderTargetInfo[] m_RTInfos = new RenderTargetInfo[3];

        // RendererFeature からレンダーパスへマテリアルを渡すための関数です。
        public void Setup(string texName, Material material, RenderTexture[] renderTextures)
        {
            m_Material = material;
            m_texName = String.IsNullOrEmpty(texName) ? "_ColorTexture" : texName;


            // RenderTexture が変更されている場合は、RenderTexture から RTHandle を作成します。
            for (int i = 0; i < 3; i++)
            {
                if (m_RTs[i] == null || m_RTs[i].rt != renderTextures[i])
                {
                    m_RTs[i]?.Release();
                    m_RTs[i] = RTHandles.Alloc(renderTextures[i], $"チャンネルテクスチャ[{i}]");
                    m_RTInfos[i] = new RenderTargetInfo()
                    {
                        format = renderTextures[i].graphicsFormat,
                        height = renderTextures[i].height,
                        width = renderTextures[i].width,
                        bindMS = renderTextures[i].bindTextureMS,
                        msaaSamples = 1,
                        volumeDepth = renderTextures[i].volumeDepth,
                    };
                }
            }
        }

        // 指定されたマテリアルで画面全体を Blit します。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var handles = new TextureHandle[3];
            // TextureHandle を RenderGraph にインポートします。
            for (int i = 0; i < 3; i++)
            {
                handles[i] = renderGraph.ImportTexture(m_RTs[i], m_RTInfos[i]);
            }
            // パス名を指定して RenderGraph パスの記録を開始し、
            // レンダー関数の実行時へデータを渡すための passData を受け取ります。
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("MRT パス", out var passData))
            {
                // カメラのカラーアタッチメントを取り出すため、UniversalResourceData を取得します。
                var resourceData = frameData.Get<UniversalResourceData>();

                // レンダー関数で使う passData を設定します。
                // カメラのカラーアタッチメントを入力として使います。
                passData.color = resourceData.activeColorTexture;
                // マテリアルで使う入力テクスチャ名です。
                passData.texName = m_texName;
                // このパスで使うマテリアルです。
                passData.material = m_Material;


                // 入力アタッチメントを設定します。
                builder.UseTexture(passData.color);
                // カラーアタッチメントを設定します。
                for (int i = 0; i < 3; i++)
                {
                    builder.SetRenderAttachment(handles[i], i);
                }

                // レンダー関数を設定します。
                builder.SetRenderFunc(static (PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
            }
        }

        // ExecutePass は、各 Blit RenderGraph 記録で使うレンダー関数です。
        // 呼び出し元のラムダ外にある変数を使わないようにするのがよい実践です。
        // static にすることで、意図しない挙動につながる可能性があるメンバー変数の利用を避けます。
        static void ExecutePass(PassData data, RasterGraphContext rgContext)
        {
            // MRTPass で使う名前に、入力カラーテクスチャを設定します。
            data.material.SetTexture(data.texName, data.color);
            // MRT シェーダーでフルスクリーントライアングルを描画します。
            rgContext.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);
        }
    }

    [Tooltip("MRT パスで使用するマテリアルです。")]
    public Material mrtMaterial;
    [Tooltip("指定したマテリアルへカメラのカラーアタッチメントを渡すときに使う名前です。")]
    public string textureName = "_ColorTexture";
    [Tooltip("結果の出力先となる RenderTexture です。配列サイズは 3 である必要があります。")]
    public RenderTexture[] renderTextures = new RenderTexture[3];

    MrtPass m_MrtPass;

    // ここでパスを作成して初期化できます。このメソッドはシリアライズが発生するたびに呼ばれます。
    public override void Create()
    {
        m_MrtPass = new MrtPass();

        // レンダーパスをどこに差し込むか設定します。
        m_MrtPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // これらは同じ RenderPassEvent を持つため、enqueue する順序が重要です。

        // マテリアルがない場合は早期終了します。
        if (mrtMaterial == null || renderTextures.Length != 3)
        {
            Debug.LogWarning("マテリアルが null、または renderTextures 配列のサイズが 3 ではないため、MRTPass をスキップします。");
            return;
        }

        foreach (var rt in renderTextures)
        {
            if (rt == null)
            {
                Debug.LogWarning("いずれかの RenderTexture が null のため、MRTPass をスキップします。");
                return;
            }
        }

        // RendererFeature の設定を RenderPass へ渡すため、パスの Setup 関数を呼び出します。
        m_MrtPass.Setup(textureName, mrtMaterial, renderTextures);
        renderer.EnqueuePass(m_MrtPass);
    }
}
