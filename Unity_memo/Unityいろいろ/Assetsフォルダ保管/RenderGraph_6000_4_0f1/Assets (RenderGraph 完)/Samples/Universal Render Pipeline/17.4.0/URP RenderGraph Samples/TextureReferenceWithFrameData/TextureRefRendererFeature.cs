using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;

// このサンプルでは、後続パスで使う参照を保持するため、
// frameData 内にテクスチャ参照用の ContextItem を作成します。
// これは、カメラのカラーアタッチメントへコピーし戻したり再度コピーしたりする追加の Blit 操作を避けるのに便利です。
// Blit 操作後にコピーし戻す代わりに、Blit の出力先を参照するよう更新し、それを後続パスで使えます。
// パス間でリソースを共有するには frameData を使う方法が推奨されます。
// 以前はこの用途にグローバルテクスチャを使うことが一般的でしたが、可能な限り避ける方がよいです。
public class TextureRefRendererFeature : ScriptableRendererFeature
{
    // テクスチャ参照を保存するために使う ContextItem です。
    public class TexRefData : ContextItem
    {
        // テクスチャ参照用の変数です。
        public TextureHandle texture = TextureHandle.nullHandle;

        // ContextItem に必要な Reset 関数です。
        // 次のフレームへ持ち越さないすべての変数をリセットする必要があります。
        public override void Reset()
        {
            // テクスチャハンドルは現在のフレームでのみ有効なため、常にリセットする必要があります。
            texture = TextureHandle.nullHandle;
        }
    }

    // このパスは、マテリアルとカメラのカラーアタッチメントを使って Blit 操作を行う際に参照を更新します。
    class UpdateRefPass : ScriptableRenderPass
    {
        // Blit 操作で使うマテリアルです。
        Material[] m_DisplayMaterials;

        // RendererFeature からレンダーパスへマテリアルを渡すための関数です。
        public void Setup(Material[] materials)
        {
            m_DisplayMaterials = materials;
        }

        // 指定されたマテリアルで画面全体を Blit します。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            foreach (var mat in m_DisplayMaterials)
            {
                // マテリアルが null の場合はスキップします。
                if (mat == null)
                {
                    Debug.LogWarning($"未割り当てのマテリアルのため、レンダーパスをスキップします。");
                    continue;
                }
                
                var texRefExist = frameData.Contains<TexRefData>();
                var texRef = frameData.GetOrCreate<TexRefData>();

                // このパスの初回実行時は、アクティブなカラーバッファから参照を取得します。
                if (!texRefExist)
                {
                    var resourceData = frameData.Get<UniversalResourceData>();
                    // 初回はこの参照を使います。
                    texRef.texture = resourceData.activeColorTexture;
                }
                
                // このパス用の出力先テクスチャを作成します。
                var descriptor = texRef.texture.GetDescriptor(renderGraph);
                // Blit 操作用に MSAA を無効化します。
                descriptor.msaaSamples = MSAASamples.None;
                descriptor.name = $"Blit マテリアル参照テクスチャ_{mat.name}";
                descriptor.clearBuffer = false;

                // Blit 結果を保持する新しい一時テクスチャを作成します。
                var destination = renderGraph.CreateTexture(descriptor);
                
                // レンダー関数で使うパスデータを設定します。
                var blitPassParams = new RenderGraphUtils.BlitMaterialParameters(texRef.texture, destination, mat, 0);
                renderGraph.AddBlitPass(blitPassParams, $"参照更新パス_{mat.name}");
                
                // texRef を更新します。
                texRef.texture = destination;
            }
        }
    }

    // 参照を更新した後、その結果を使うためにカメラのカラーアタッチメントへコピーし戻す必要があります。
    class CopyBackRefPass : ScriptableRenderPass
    {
        // この関数は、参照先をカメラのカラーアタッチメントへ Blit し戻します。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // frameData 内に TexRefData が存在しない場合、コピーし戻すものがないため早期終了します。
            if (!frameData.Contains<TexRefData>()) return;

            // カメラのアクティブなカラーテクスチャを取得するため、UniversalResourceData を取得します。
            var resourceData = frameData.Get<UniversalResourceData>();
            // テクスチャ参照を取得するため、TexRefData を取得します。
            var texRef = frameData.Get<TexRefData>();
            
            renderGraph.AddBlitPass(texRef.texture, resourceData.activeColorTexture, Vector2.one, Vector2.zero, passName: "Blit し戻しパス");
        }
    }

    [Tooltip("Blit 操作で使用するマテリアルです。")]
    public Material[] displayMaterials = new Material[1];

    UpdateRefPass m_UpdateRefPass;
    CopyBackRefPass m_CopyBackRefPass;

    // ここでパスを作成して初期化できます。このメソッドはシリアライズが発生するたびに呼ばれます。
    public override void Create()
    {
        m_UpdateRefPass = new UpdateRefPass();
        m_CopyBackRefPass = new CopyBackRefPass();

        // レンダーパスをどこに差し込むか設定します。
        m_UpdateRefPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        m_CopyBackRefPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // マテリアルがない場合は早期終了します。
        if (displayMaterials == null || displayMaterials.Length == 0)
        {
            Debug.LogWarning("TextureRefRendererFeature のマテリアル配列が null または 要素数0 のため、スキップします。");
            return;
        }

        // これらは同じ RenderPassEvent を持つため、enqueue する順序が重要です。
        m_UpdateRefPass.Setup(displayMaterials);
        renderer.EnqueuePass(m_UpdateRefPass);
        renderer.EnqueuePass(m_CopyBackRefPass);
    }
}
