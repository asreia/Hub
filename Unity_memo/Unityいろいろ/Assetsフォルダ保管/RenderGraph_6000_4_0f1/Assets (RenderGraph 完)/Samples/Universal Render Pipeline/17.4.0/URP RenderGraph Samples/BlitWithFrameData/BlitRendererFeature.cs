using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// frameData と複数の ScriptableRenderPass を使って Blit 操作を扱う方法のサンプルです。
// BlitStartRenderPass は frameData 内の BlitData を初期化し、カメラのカラーアタッチメントを BlitData 内のテクスチャへコピーします。
// BlitRenderPass は、RendererFeature に渡された各マテリアルごとに Blit を行います。
// BlitEndRenderPass は、BlitData の結果テクスチャをカメラのカラーアタッチメントへコピーし戻します。

public class BlitRendererFeature : ScriptableRendererFeature
{
    // frameData 内に保持されるクラスです。テクスチャリソースの管理を担当します。
    public class BlitData : ContextItem
    {
        // Render Graph のテクスチャハンドルです。
        TextureHandle m_TextureHandleFront;
        TextureHandle m_TextureHandleBack;

        // どちらのテクスチャが最新かを管理する bool 値です。
        bool m_IsFront = true;

        // 直近の Blit 操作後のカラーバッファを保持しているテクスチャです。
        public TextureHandle texture;

        // BlitData を初期化する関数です。各フレームでこのクラスを使い始める前に呼び出します。
        public void Init(RenderGraph renderGraph, TextureDesc targetDescriptor, string textureName = null)
        {
            // テクスチャ名が有効か確認し、無効な場合はデフォルト値を設定します。
            var baseTexName = String.IsNullOrEmpty(textureName) ? "_BlitTextureData" : textureName;
            
            targetDescriptor.filterMode = FilterMode.Bilinear;
            targetDescriptor.wrapMode = TextureWrapMode.Clamp;
            // Blit 操作用に MSAA を無効化します。
            targetDescriptor.msaaSamples = MSAASamples.None;
            // カラーバッファだけを変換するため、深度バッファは無効化します。
            targetDescriptor.depthBufferBits = 0;

            targetDescriptor.name = baseTexName + "Front";
            m_TextureHandleFront = renderGraph.CreateTexture(targetDescriptor);
            
            targetDescriptor.name = baseTexName + "Back";
            m_TextureHandleBack = renderGraph.CreateTexture(targetDescriptor);

            // アクティブなテクスチャをフロントバッファに設定します。
            texture = m_TextureHandleFront;
        }

        // テクスチャハンドルは 1 フレームの間だけ有効なので、
        // 無効なテクスチャハンドルを持ち越さないよう、各フレーム後にリセットする必要があります。
        public override void Reset()
        {
            // 次のフレームへ無効な参照を持ち越さないよう、カラーバッファをリセットします。
            // 前フレームの BlitData テクスチャハンドルは、この時点では無効になっている可能性があります。
            m_TextureHandleFront = TextureHandle.nullHandle;
            m_TextureHandleBack = TextureHandle.nullHandle;
            texture = TextureHandle.nullHandle;
            // アクティブなテクスチャをフロントバッファへ戻します。
            m_IsFront = true;
        }

        // この関数では、前フレームの値が漏れないよう未使用の値をリセットすべきことを示すため、
        // マテリアルを引数として受け取りません。
        public void RecordBlitColor(RenderGraph renderGraph, ContextContainer frameData)
        {
            // frameData から UniversalResourceData を取得し、カメラのアクティブなカラーアタッチメントを取り出します。
            var resourceData = frameData.Get<UniversalResourceData>();
            
            // BlitData のテクスチャが有効か確認し、無効な場合は BlitData を初期化します。
            if (!texture.IsValid())
            {
                // BlitData 用の descriptor を設定します。ここではカメラカラーの descriptor を元にします。
                Init(renderGraph, resourceData.activeColorTexture.GetDescriptor(renderGraph));
            }
            
            // activeColorTexture を現在のフロントテクスチャへコピーします。
            renderGraph.AddCopyPass(resourceData.activeColorTexture, texture, "カメラカラーを BlitData テクスチャへコピー");
        }

        // BlitData の現在のフロントテクスチャをカメラのカラーアタッチメントへ Blit し戻す RenderGraph パスを記録します。
        public void RecordBlitBackToColor(RenderGraph renderGraph, ContextContainer frameData)
        {
            // BlitData のテクスチャが有効か確認します。無効なら、未初期化またはエラー発生済みです。
            if (!texture.IsValid()) return;
            
            // frameData から UniversalResourceData を取得し、カメラのアクティブなカラーアタッチメントを取り出します。
            var resourceData = frameData.Get<UniversalResourceData>();
            
            // 現在のフロントテクスチャを activeColorTexture へコピーし戻します。
            renderGraph.AddCopyPass(texture, resourceData.activeColorTexture, "BlitData テクスチャをカメラカラーへコピーし戻し");
        }

        // 指定されたマテリアルで画面全体を Blit します。
        public void RecordFullScreenPass(RenderGraph renderGraph, string passName, Material material)
        {
            // データが事前に初期化されているか、マテリアルが有効かを確認します。
            if (!texture.IsValid() || material == null)
            {
                Debug.LogWarning("入力テクスチャハンドルが無効なため、フルスクリーンパスをスキップします。");
                return;
            }
            
            // アクティブなテクスチャハンドルを切り替えて、不要な Blit を避けます。
            // 最新のテクスチャが必要な場合は、texture 変数を参照すれば分かります。
            m_IsFront = !m_IsFront;

            // アクティブなテクスチャを入れ替えます。
            var destination = m_IsFront ? m_TextureHandleFront : m_TextureHandleBack;

            // レンダー関数の実行時に使うデータを設定します。
            var blitMaterialParameters = new RenderGraphUtils.BlitMaterialParameters(texture, destination, material, 0);
            
            // Blit を追加します。
            renderGraph.AddBlitPass(blitMaterialParameters, passName);
            
            // 切り替え後のテクスチャを更新します。
            texture = destination;
        }
    }

    // RendererFeature の最初のレンダーパスです。
    // frameData 内のデータを初期化し、Blit による変換を行えるよう、
    // カメラのカラーアタッチメントを BlitData 内のテクスチャへコピーします。
    class BlitStartRenderPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // frameData 内に BlitData を作成します。
            var blitTextureData = frameData.Create<BlitData>();
            // カメラのカラーアタッチメントを BlitData 内のテクスチャへコピーします。
            blitTextureData.RecordBlitColor(renderGraph, frameData);
        }
    }

    // RendererFeature に渡された各マテリアルごとに Blit を行うレンダーパスです。
    class BlitRenderPass : ScriptableRenderPass
    {
        List<Material> m_Materials;

        // RendererFeature からマテリアルを受け取るための Setup 関数です。
        public void Setup(List<Material> materials)
        {
            m_Materials = materials;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 現在のフレームから BlitData を取得します。
            var blitTextureData = frameData.Get<BlitData>();
            foreach(var material in m_Materials)
            {
                // マテリアルが null の場合、変換が発生せず Blit する必要がないため、このループをスキップします。
                if (material == null) continue;
                // マテリアルを使った Blit パスを記録します。
                blitTextureData.RecordFullScreenPass(renderGraph, $"{material.name} の Blit パス", material);
            }    
        }
    }

    // テクスチャをカメラのカラーアタッチメントへコピーし戻す最後のレンダーパスです。
    class BlitEndRenderPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 現在のフレームから BlitData を取得し、カメラのカラーアタッチメントへ再度 Blit し戻します。
            var blitTextureData = frameData.Get<BlitData>();
            blitTextureData.RecordBlitBackToColor(renderGraph, frameData);
        }
    }

    [SerializeField]
    [Tooltip("Blit に使用するマテリアルです。リスト内の順序どおり、インデックス 0 から順番に Blit されます。")]
    List<Material> m_Materials;

    BlitStartRenderPass m_StartPass;
    BlitRenderPass m_BlitPass;
    BlitEndRenderPass m_EndPass;
    
    // ここでパスを作成して初期化できます。このメソッドはシリアライズが発生するたびに呼ばれます。
    public override void Create()
    {
        m_StartPass = new BlitStartRenderPass();
        m_BlitPass = new BlitRenderPass();
        m_EndPass = new BlitEndRenderPass();

        // レンダーパスをどこに差し込むか設定します。
        m_StartPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        m_BlitPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        m_EndPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Blit するテクスチャがない場合は早期 return します。
        if (m_Materials == null || m_Materials.Count == 0) return;

        // Blit レンダーパスへマテリアルを渡します。
        m_BlitPass.Setup(m_Materials);

        // これらは同じ RenderPassEvent を持つため、enqueue する順序が重要です。
        renderer.EnqueuePass(m_StartPass);
        renderer.EnqueuePass(m_BlitPass);
        renderer.EnqueuePass(m_EndPass);
    }
}
