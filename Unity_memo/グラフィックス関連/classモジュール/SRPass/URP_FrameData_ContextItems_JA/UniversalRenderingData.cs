namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// 一般的なRenderer設定のデータを含みます。
    /// </summary>
    public class UniversalRenderingData : ContextItem
    {
        /// <summary>
        /// 可視オブジェクト、ライト、プローブへのハンドルを公開するCulling結果を返します。
        /// これを使用して <c>ScriptableRenderContext.DrawRenderers</c> でオブジェクトを描画できます
        /// <see cref="CullingResults"/>
        /// <seealso cref="ScriptableRenderContext"/>
        /// </summary>
        // [設定元] UniversalRenderPipeline が context.Cull(ref cullingParameters) を実行した結果を設定。
        public CullingResults cullResults;

        /// <summary>
        /// パイプラインがDynamic Batchingをサポートしている場合は true。
        /// この設定はShadow Casterの描画時には適用されません。Shadow Casterの描画時、Dynamic Batchingは常に無効です。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.supportsDynamicBatching から設定。
        public bool supportsDynamicBatching;

        /// <summary>
        /// 描画時に要求されるPer-Object Dataを保持します
        /// <see cref="PerObjectData"/>
        /// </summary>
        // [設定元] UniversalRenderPipeline.GetPerObjectLightFlags(...) で UniversalLightData、UniversalRenderPipelineAsset、RenderingMode から設定。
        public PerObjectData perObjectData;

        /// <summary>
        /// 現在のフレームでRendererが使用するRendering Mode。
        /// これはRenderer Assetで設定されている内容と異なる場合があることに注意してください。
        /// たとえば、ハードウェアがDeferred Renderingに対応していない場合や、Wireframe Renderingを行う場合です。
        /// </summary>
        // [設定元] UniversalRenderer.renderingModeActual から設定。
        public RenderingMode renderingMode { get; internal set; }

        /// <summary>
        /// PrepassオブジェクトをフィルタリングするためにRendererへ設定されるLayer Mask。
        /// </summary>
        // [設定元] UniversalRenderer.prepassLayerMask から設定。
        public LayerMask prepassLayerMask { get; internal set; }

        /// <summary>
        /// 不透明オブジェクトをフィルタリングするためにRendererへ設定されるLayer Mask。
        /// </summary>
        // [設定元] UniversalRenderer.opaqueLayerMask から設定。
        public LayerMask opaqueLayerMask { get; internal set; }

        /// <summary>
        /// 透明オブジェクトをフィルタリングするためにRendererへ設定されるLayer Mask。
        /// </summary>
        // [設定元] UniversalRenderer.transparentLayerMask から設定。
        public LayerMask transparentLayerMask { get; internal set; }

        /// <summary>
        /// Stencil LOD Cross Fadeが有効な場合は true。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.enableLODCrossFade と lodCrossFadeDitheringType == Stencil から設定。
        public bool stencilLodCrossFadeEnabled { get; internal set; }

        /// <inheritdoc/>
        public override void Reset()
        {
            cullResults = default;
            supportsDynamicBatching = default;
            perObjectData = default;
            renderingMode = default;
            stencilLodCrossFadeEnabled = default;
            prepassLayerMask = -1;
            opaqueLayerMask = -1;
            transparentLayerMask = -1;
        }
    }
}
