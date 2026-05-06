using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// このサンプルは、アクティブなカラーテクスチャを新しいテクスチャへコピーし、その後ソーステクスチャを 2 回ダウンサンプリングします。
// これは API の説明用サンプルなので、新しいテクスチャはこのフレーム内の他の場所では使われません。
// 内容は Frame Debugger で確認できます。
// このサンプルの重要な概念は UnsafePass の使用です。
// この種類のパスは unsafe で、RasterRenderPass とは互換性のない SetRenderTarget() のようなコマンドを使えます。
// UnsafePass を使うと、RenderGraph は NativeRenderPass 内へマージする最適化を試みません。
// 場合によっては UnsafePass を使うのが妥当です。
// たとえば、隣接する一連のパスがマージ不可能だと分かっている場合、複数パスのセットアップを簡略化しつつ RenderGraph のコンパイル時間を短縮できます。
public class UnsafePassRenderFeature : ScriptableRendererFeature
{
    class UnsafePass : ScriptableRenderPass
    {
        // このクラスはパスに必要なデータを保持し、パスを実行するデリゲート関数へ引数として渡されます。
        private class PassData
        {
            internal TextureHandle source;
            internal TextureHandle destination;
            internal TextureHandle destinationHalf;
            internal TextureHandle destinationQuarter;
        }

        // この static メソッドはパスを実行するために使われ、RenderGraph レンダーパスの RenderFunc デリゲートとして渡されます。
        static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            // 各 Blit に対して RenderTarget を手動で設定します。
            // 可能な場合に RenderGraph がパスをマージできるよう設定するなら、各 SetRenderTarget 呼び出しごとに個別の RasterCommandPass が必要になります。
            // このケースでは 3 つのサブパスの RenderTarget サイズが異なるため、マージできないことが分かっています。
            // そのため unsafe pass を使ってコードを簡略化し、RenderGraph の処理時間も節約します。
            
            // 現在のシーンカラーをコピーします。

            //==ココを全てコメントアウトすると真っ黒になった==

            CommandBuffer unsafeCmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
            
            context.cmd.SetRenderTarget(data.destination);
            Blitter.BlitTexture(unsafeCmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
            
            // 2 倍ダウンスケールします。
            
            context.cmd.SetRenderTarget(data.destinationHalf);
            Blitter.BlitTexture(unsafeCmd, data.destination, new Vector4(1, 1, 0, 0), 0, false);
            
            context.cmd.SetRenderTarget(data.destinationQuarter);
            Blitter.BlitTexture(unsafeCmd, data.destinationHalf, new Vector4(1, 1, 0, 0), 0, false);
            
            // 2 倍アップスケールします。
            
            context.cmd.SetRenderTarget(data.destinationHalf);
            Blitter.BlitTexture(unsafeCmd, data.destinationQuarter, new Vector4(1, 1, 0, 0), 0, false);
            
            context.cmd.SetRenderTarget(data.destination);
            Blitter.BlitTexture(unsafeCmd, data.destinationHalf, new Vector4(1, 1, 0, 0), 0, false);
        }

        // ここで RenderGraph ハンドルにアクセスできます。
        // 各 ScriptableRenderPass は、この RenderGraph ハンドルを使って RenderGraph に複数のレンダーパスを追加できます。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            string passName = "Unsafe パス";

            // UniversalResourceData には、レンダラーが使うすべてのテクスチャハンドルが含まれます。
            // これには、カメラが描き込むメインのカラー/深度バッファである、アクティブなカラー/深度テクスチャも含まれます。
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var descriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            // Blit 操作用に MSAA を無効化します。
            descriptor.msaaSamples = MSAASamples.None;
            descriptor.clearBuffer = false;
            // descriptor.disableFallBackToImportedTexture = true; //効果なし
            // descriptor.fallBackToBlackTexture = true; //効果なし

            // Blit 結果を保持する新しい一時テクスチャを作成します。
            descriptor.name = "Unsafe テクスチャ";
            var destination = renderGraph.CreateTexture(descriptor);

            // 名前と ExecutePass 関数へ渡すデータ型を指定して、RenderGraph に unsafe pass を追加します。
            using (var builder = renderGraph.AddUnsafePass<PassData>(passName, out var passData))
            {
                //==`Pass`内を全てコメントアウトしてもパスカルされるだけで出力が真っ黒にならない謎..==
                    //次の`Pass`で描画されていない`TextureHandle`を読もうとすると、適当に?Importされた`TextureHandle`を読もうとする?(次の`Pass`の`disableFallBackToImportedTexture`?)

                // パスに必要なデータを passData に設定します。

                // frame data 経由でアクティブなカラーテクスチャを取得し、Blit 用のソーステクスチャとして設定します。
                passData.source = resourceData.activeColorTexture;

                // ここで出力先テクスチャを作成します。
                // 1 つ目のテクスチャはカラーテクスチャのコピーとして、アクティブなカラーテクスチャと同じサイズで作られますが、深度バッファは持ちません。
                // このサンプルでは multisampled texture が不要なため、MSAA も無効化します。
                // 残り 2 つのテクスチャは、それぞれ前のテクスチャの半分の解像度になります。


                descriptor.width /= 2;
                descriptor.height /= 2;
                descriptor.name = "Unsafe テクスチャ 2";
                var destinationHalf = renderGraph.CreateTexture(descriptor);

                descriptor.width /= 2;
                descriptor.height /= 2;
                descriptor.name = "Unsafe テクスチャ 3";
                var destinationQuarter = renderGraph.CreateTexture(descriptor);

                passData.destination = destination;
                passData.destinationHalf = destinationHalf;
                passData.destinationQuarter = destinationQuarter;

                // src テクスチャを、このパスの入力依存関係として UseTexture() で宣言します。
                builder.UseTexture(passData.source);
                
                // UnsafePass では UseTextureFragment/UseTextureFragmentDepth を使って出力を設定しないため、代わりに UseTexture で書き込みを指定する必要があります。
                builder.UseTexture(passData.destination, AccessFlags.WriteAll);
                builder.UseTexture(passData.destinationHalf, AccessFlags.WriteAll);
                builder.UseTexture(passData.destinationQuarter, AccessFlags.WriteAll);

                // このサンプルでは説明のために、このパスのカリングを無効化します。
                // 通常は出力先テクスチャが他の場所で使われないため、このパスはカリングされます。
                // builder.AllowPassCulling(false);

                // パス実行時に RenderGraph から呼び出されるレンダーパスデリゲートへ ExecutePass 関数を割り当てます。
                builder.SetRenderFunc(static (PassData data, UnsafeGraphContext context) => ExecutePass(data, context));

            }
            resourceData.cameraColor = destination;
        }
    }

    UnsafePass m_UnsafePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_UnsafePass = new UnsafePass();

        // レンダーパスをどこに差し込むか設定します。
        m_UnsafePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_UnsafePass);
    }
}
