using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// このサンプルは、現在アクティブなカラーテクスチャを新しいテクスチャへコピーします。
// これは API の説明用サンプルなので、新しいテクスチャはこのフレーム内の他の場所では使われません。
// 内容は Frame Debugger で確認できます。
public class CopyRenderFeature : ScriptableRendererFeature
{
    class CopyRenderPass : ScriptableRenderPass
    {
        public CopyRenderPass()
        {
            // このパスは現在のカラーテクスチャを読み取ります。
            // そのためには中間テクスチャが必要で、BackBuffer を入力テクスチャとして使うことはサポートされていません。
            // このプロパティを設定すると、URP が自動的に中間テクスチャを作成します。
            // これを RenderFeature 側ではなくここで設定するのがよい実践です。
            // そうすることで、このパス自体が自己完結になり、
            // RenderFeature を使わずに MonoBehaviour から直接このパスを enqueue できるようになります。
            requiresIntermediateTexture = true;
        }

        // ここで RenderGraph ハンドルにアクセスできます。
        // 各 ScriptableRenderPass は、この RenderGraph ハンドルを使って
        // Render Graph に複数のレンダーパスを追加できます。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // UniversalResourceData には、レンダラーが使うすべてのテクスチャハンドルが含まれます。
            // これには、現在アクティブなカラー/深度テクスチャも含まれます。
            // それらはカメラが描き込むメインのカラー/深度バッファです。
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // ここで出力先テクスチャを作成します。
            // このテクスチャは、アクティブなカラーテクスチャと同じサイズで作られます。
            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{passName}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);  
           
            if (RenderGraphUtils.CanAddCopyPassMSAA()) // renderGraph.AddCopyPass() で、現在のプラットフォームの MSAA コピー対応状況を確認します。
            {
                // このシンプルなパスは、アクティブなカラーテクスチャを新しいテクスチャへコピーします。
                renderGraph.AddCopyPass(resourceData.activeColorTexture, destination, passName: "アクティブカラーを一時テクスチャへコピー");

                // コピー結果をどこからも読まないと、このパスはカリングされてしまいます。
                // そのため、ここでは元へコピーし戻しています。これはあくまで説明用の処理です。
                renderGraph.AddCopyPass(destination, resourceData.activeColorTexture, passName: "一時テクスチャをアクティブカラーへコピーし戻し");
            }
            else
            {
                Debug.Log("MSAA のため、コピーパスを追加できません。");
            }
        }
    }

    CopyRenderPass m_CopyRenderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_CopyRenderPass = new CopyRenderPass();

        // このレンダーパスをどこに差し込むか設定します。
        m_CopyRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_CopyRenderPass);
    }
}
