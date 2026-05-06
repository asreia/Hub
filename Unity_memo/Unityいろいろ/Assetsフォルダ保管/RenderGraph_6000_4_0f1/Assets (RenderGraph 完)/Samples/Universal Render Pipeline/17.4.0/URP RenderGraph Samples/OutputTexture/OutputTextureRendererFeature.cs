using System;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;

// マージ: Inspector で m_PassEvent を After Rendering Opaques に設定し、texture type を Normal に設定すると、
// このパスは Draw Objects Pass や Draw Skybox pass とマージできます。
// このマージは Render Graph Visualizer で確認できます。
// After Rendering Post Processing に設定すると、このパスが何ともマージされないことを確認できます。

// この RenderFeature は、URP で使われる特定のテクスチャを RenderGraph で出力する方法、
// テクスチャを名前でマテリアルへ割り当てる方法、
// そして 2 つのレンダーパスが正しい順序で実行された場合にマージされる方法を示します。
public class OutputTextureRendererFeature : ScriptableRendererFeature
{
    // 出力したいテクスチャを選択するための enum です。
    [Serializable]
    enum TextureType
    {
        OpaqueColor,
        Depth,
        Normal,
        MotionVector,
    }

    // resource data と目的の texture type から、対応するテクスチャを取得する関数です。
    static TextureHandle GetTextureHandleFromType(UniversalResourceData resourceData, TextureType textureType)
    {
        switch (textureType)
        {
            case TextureType.OpaqueColor:
                return resourceData.cameraOpaqueTexture;
            case TextureType.Depth:
                return resourceData.cameraDepthTexture;
            case TextureType.Normal:
                return resourceData.cameraNormalsTexture;
            case TextureType.MotionVector:
                return resourceData.motionVectorColor;
            default:
                return TextureHandle.nullHandle;
        }
    }

    // レンダリング中のテクスチャを出力して確認するためのパスです。
    class OutputTexturePass : ScriptableRenderPass
    {
        // 指定したマテリアルへ TextureHandle をバインドするときに使うテクスチャ名です。
        string m_TextureName;
        // URP から取得したいテクスチャの種類です。
        TextureType m_TextureType;
        // カラー出力へ Blit するために使うマテリアルです。
        Material m_Material;

        // ConfigureInput() を設定し、RendererFeature の設定をレンダーパスへ渡すための関数です。
        public void Setup(string textureName, TextureType textureType, Material material)
        {
            // パス実行時に、対応する各テクスチャが使える状態になるよう設定します。
            if (textureType == TextureType.OpaqueColor)
                ConfigureInput(ScriptableRenderPassInput.Color);
            else if (textureType == TextureType.Depth)
                ConfigureInput(ScriptableRenderPassInput.Depth);
            else if (textureType == TextureType.Normal)
                ConfigureInput(ScriptableRenderPassInput.Normal);
            else if (textureType == TextureType.MotionVector)
                ConfigureInput(ScriptableRenderPassInput.Motion);

            // Blit 時に使うテクスチャ名、種類、マテリアルを設定します。
            // このサンプルでは、Blit 時の入力テクスチャ名にカスタム名を使うマテリアルを使用します。
            // このテクスチャ名は、使用しているマテリアルのテクスチャ入力名と一致している必要があります。
            m_TextureName = String.IsNullOrEmpty(textureName) ? "_BlitTexture" : textureName;
            // Texture type は、カメラから取得したい入力を選択します。
            m_TextureType = textureType;
            // このマテリアルは、テクスチャをカメラのカラーアタッチメントへ Blit するために使います。
            m_Material = material;
        }

        // 選択した URP リソースをカメラのカラーアタッチメントへ Blit する RenderGraph パスを記録します。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // URP のテクスチャハンドルを取得するため、frameData から UniversalResourceData を取得します。
            var resourceData = frameData.Get<UniversalResourceData>();

            // helper 関数で resourceData から正しいハンドルを取得し、入力 TextureHandle として設定します。
            var source = GetTextureHandleFromType(resourceData, m_TextureType);

            if (!source.IsValid())
            {
                Debug.Log("入力テクスチャが作成されていません。パスイベントがリソース作成より前である可能性があるため、OutputTexturePass をスキップします。");
                return;
            }

            RenderGraphUtils.BlitMaterialParameters para = new(source, resourceData.activeColorTexture, m_Material, 0);
            para.sourceTexturePropertyID = Shader.PropertyToID(m_TextureName);
            renderGraph.AddBlitPass(para, passName: "選択したリソースを Blit");                     
        }
    }

    // RendererFeature の設定を変更するための Inspector 入力です。
    [SerializeField]
    RenderPassEvent m_PassEvent = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField]
    string m_TextureName = "_InputTexture";
    [SerializeField]
    TextureType m_TextureType;
    [SerializeField]
    Material m_Material;

    OutputTexturePass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new OutputTexturePass();
        // レンダーパスをどこに差し込むか設定します。
        m_ScriptablePass.renderPassEvent = m_PassEvent;
    }

    // ここでは、レンダラーに 1 つまたは複数のレンダーパスを注入できます。
    // このメソッドは、レンダラーのセットアップ時にカメラごとに呼ばれます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Material == null)
        {
            Debug.LogWarning("マテリアルが null のため、OutputTexturePass をスキップします。");
            return;
        }
        
        // レンダーパス用の正しいデータを設定し、RendererFeature からレンダーパスへデータを渡します。
        m_ScriptablePass.Setup(m_TextureName, m_TextureType, m_Material);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
