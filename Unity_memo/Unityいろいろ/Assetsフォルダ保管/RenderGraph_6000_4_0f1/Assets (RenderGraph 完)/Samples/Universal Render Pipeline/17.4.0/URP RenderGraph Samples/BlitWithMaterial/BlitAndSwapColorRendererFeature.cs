using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;

// このサンプルは、アクティブな CameraColor を新しいテクスチャへ Blit します。
// マテリアルを使った Blit の方法と、ResourceData を使ってアクティブなカラーターゲットへ再度 Blit し戻す処理を避ける方法を示します。
// これは API の説明用サンプルです。

// このパスは、指定されたマテリアルで画面全体を一時テクスチャへ Blit し、
// UniversalResourceData.cameraColor をその一時テクスチャへ差し替えます。
// そのため、次に cameraColor を参照するパスは、この新しい一時テクスチャを cameraColor として参照し、Blit を 1 回節約できます。
// ResourceData を使うと、cameraColor 専用だった SwapColorBuffer API のような専用 API に頼らず、自分でリソースの差し替えを管理できます。
// これにより、避けられるコピーや Blit の追加コストなしに、より疎結合なパスを書けます。
public class BlitAndSwapColorPass : ScriptableRenderPass
{
    const string m_PassName = "マテリアル Blit とカラー差し替えパス";

    // Blit 操作で使うマテリアルです。
    Material m_BlitMaterial;

    // RendererFeature からレンダーパスへマテリアルを渡すための関数です。
    public void Setup(Material mat)
    {
        m_BlitMaterial = mat;

        // このパスは現在のカラーテクスチャを読み取ります。
        // そのためには中間テクスチャが必要で、BackBuffer を入力テクスチャとして使うことはサポートされていません。
        // このプロパティを設定すると、URP が自動的に中間テクスチャを作成します。
        // ただしパフォーマンスコストがあるため、不要な場合は設定しないでください。
        // これを RenderFeature 側ではなくここで設定するのがよい実践です。
        // そうすることで、このパス自体が自己完結になり、
        // RenderFeature を使わずに MonoBehaviour から直接このパスを enqueue できるようになります。
        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // UniversalResourceData には、レンダラーが使うすべてのテクスチャハンドルが含まれます。
        // これには、カメラが描き込むメインのカラー/深度バッファである、アクティブなカラー/深度テクスチャも含まれます。
        var resourceData = frameData.Get<UniversalResourceData>();

        // m_Pass.requiresIntermediateTexture = true を設定しているため、通常ここには入りません。
        // ただし、BackBuffer しか存在しない AfterRendering に render event を設定した場合は例外です。
        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError($"レンダリングパスをスキップします。BlitAndSwapColorRendererFeature では中間的な ColorTexture が必要ですが、BackBuffer をテクスチャ入力として使用することはできません。");
            return;
        }

        // ここで出力先テクスチャを作成します。
        // このテクスチャは、アクティブなカラーテクスチャと同じサイズで作られます。
        var source = resourceData.activeColorTexture;
        
        var destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = $"CameraColor-{m_PassName}";
        destinationDesc.clearBuffer = false;
        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_BlitMaterial, 0);
        renderGraph.AddBlitPass(para, passName: m_PassName);

        // FrameData では、内部パイプラインバッファを取得および設定できます。
        // ここでは、このパスで書き込んだテクスチャへ CameraColorBuffer を更新しています。
        // RenderGraph がパイプラインリソースと依存関係を管理するため、後続パスは正しいカラーバッファを使えます。
        // この最適化にはいくつか注意点があります。
        // カメラスタッキングのように、カラーバッファがフレーム間や別カメラ間で永続化される場合は注意が必要です。
        // そのような場合は、テクスチャが RTHandle であることと、そのライフサイクルを適切に管理していることを確認してください。
        resourceData.cameraColor = destination;
    }
}

public class BlitAndSwapColorRendererFeature : ScriptableRendererFeature
{    
    [Tooltip("Blit 操作で使用するマテリアルです。")]
    public Material material;

    [Tooltip("このパスを差し込む RenderPassEvent です。")]
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    BlitAndSwapColorPass m_Pass;

    // ここでパスを作成して初期化できます。このメソッドはシリアライズが発生するたびに呼ばれます。
    public override void Create()
    {
        m_Pass = new BlitAndSwapColorPass();

        // レンダーパスをどこに差し込むか設定します。
        m_Pass.renderPassEvent = renderPassEvent;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // マテリアルがない場合は早期終了します。
        if (material == null)
        {
            Debug.LogWarning(this.name + " のマテリアルが null のため、スキップします。");
            return;
        }

        m_Pass.Setup(material);
        renderer.EnqueuePass(m_Pass);
    }
}
