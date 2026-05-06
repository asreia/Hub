using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// URPの影で使用される各種データのコンテナクラスです。
    /// </summary>
    public class UniversalShadowData : ContextItem
    {
        /// <summary>
        /// Main Lightの影が有効な場合は true。
        /// </summary>
        // [設定元] CreateShadowData で SystemInfo.supportsShadows、mainLightShadowsEnabled、Camera の shadowDistance、Main Light の有無/Light.shadows から設定。
        public bool supportsMainLightShadows;

        /// <summary>
        /// URP AssetでMain Lightの影が有効な場合は true
        /// </summary>
        // [設定元] CreateShadowData で UniversalRenderPipelineAsset.supportsMainLightShadows と mainLightRenderingMode == PerPixel から設定。
        internal bool mainLightShadowsEnabled;

        /// <summary>
        /// Main Light Shadow Mapの幅。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.mainLightShadowmapResolution を基準に ShadowData 作成時に設定。
        public int mainLightShadowmapWidth;

        /// <summary>
        /// Main Light Shadow Mapの高さ。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.mainLightShadowmapResolution を基準に ShadowData 作成時に設定。
        public int mainLightShadowmapHeight;

        /// <summary>
        /// Shadow Cascadeの数。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.shadowCascadeCount から設定。
        public int mainLightShadowCascadesCount;

        /// <summary>
        /// Cascade間の分割。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset の cascade split 設定から設定。
        public Vector3 mainLightShadowCascadesSplit;

        /// <summary>
        /// Main Lightの最後のCascadeにおけるShadow Fade Border。
        /// 値は0から1の範囲でShadow Fadeの幅を表します。
        /// 値0はShadow Fadeなしを意味します。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.cascadeBorder から設定。
        public float mainLightShadowCascadeBorder;

        /// <summary>
        /// Additional Lightの影が有効な場合は true。
        /// </summary>
        // [設定元] CreateShadowData で SystemInfo.supportsShadows、additionalLightShadowsEnabled、PerVertex でないこと、Camera の shadowDistance、影を落とす追加ライト有無から設定。
        public bool supportsAdditionalLightShadows;

        /// <summary>
        /// URP Assetで追加ライトの影が有効な場合は true
        /// </summary>
        // [設定元] CreateShadowData で UniversalRenderPipelineAsset.supportsAdditionalLightShadows と追加ライト方式(PerPixel/ForwardPlus)から設定。
        internal bool additionalLightShadowsEnabled;

        /// <summary>
        /// Additional Light Shadow Mapの幅。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.additionalLightsShadowmapResolution を基準に設定。
        public int additionalLightsShadowmapWidth;

        /// <summary>
        /// Additional Light Shadow Mapの高さ。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.additionalLightsShadowmapResolution を基準に設定。
        public int additionalLightsShadowmapHeight;

        /// <summary>
        /// Soft Shadowが有効な場合は true。
        /// </summary>
        // [設定元] CreateShadowData で UniversalRenderPipelineAsset.supportsSoftShadows と有効な Main/Additional Shadow があるかから設定。
        public bool supportsSoftShadows;

        /// <summary>
        /// 使用されるビット数。
        /// </summary>
        // [設定元] UniversalRenderPipeline.CreateShadowData で Shadow Map の depth buffer bit 数として 16 を設定。
        public int shadowmapDepthBufferBits;

        /// <summary>
        /// Shadow Biasのリスト。
        /// </summary>
        // [設定元] VisibleLight ごとの UniversalAdditionalLightData または UniversalRenderPipelineAsset の shadow bias 設定から作成。
        public List<Vector4> bias;

        /// <summary>
        /// Shadow Map解像度のリスト。
        /// </summary>
        // [設定元] VisibleLight ごとの UniversalAdditionalLightData または UniversalRenderPipelineAsset の shadow resolution tier/default から作成。
        public List<int> resolution;

        // [設定元] AdditionalLightsShadowCasterPass の実行結果に応じて Shader Keyword 用に設定。
        internal bool isKeywordAdditionalLightShadowsEnabled;
        // [設定元] MainLightShadowCasterPass/AdditionalLightsShadowCasterPass が Soft Shadows の有効状態に応じて設定。
        internal bool isKeywordSoftShadowsEnabled;
        // [設定元] InitializeMainLightShadowResolution(shadowData) で atlas/cascade 条件から決定。
        internal int mainLightShadowResolution;
        // [設定元] InitializeMainLightShadowResolution(shadowData) で Main Light shadow atlas 幅を決定。
        internal int mainLightRenderTargetWidth;
        // [設定元] InitializeMainLightShadowResolution(shadowData) で Main Light shadow atlas 高さを決定。
        internal int mainLightRenderTargetHeight;

        // [設定元] UniversalRenderPipeline が ShadowCulling.CullShadowCasters(...) の戻り値を設定。
        internal NativeArray<URPLightShadowCullingInfos> visibleLightsShadowCullingInfos;
        // [設定元] UniversalRenderPipeline.BuildAdditionalLightsShadowAtlasLayout(...) の戻り値を設定。
        internal AdditionalLightsShadowAtlasLayout shadowAtlasLayout;

        /// <inheritdoc/>
        public override void Reset()
        {
            supportsMainLightShadows = false;
            mainLightShadowmapWidth = 0;
            mainLightShadowmapHeight = 0;
            mainLightShadowCascadesCount = 0;
            mainLightShadowCascadesSplit = Vector3.zero;
            mainLightShadowCascadeBorder = 0.0f;
            supportsAdditionalLightShadows = false;
            additionalLightsShadowmapWidth = 0;
            additionalLightsShadowmapHeight = 0;
            supportsSoftShadows = false;
            shadowmapDepthBufferBits = 0;
            bias?.Clear();
            resolution?.Clear();

            isKeywordAdditionalLightShadowsEnabled = false;
            isKeywordSoftShadowsEnabled = false;
            mainLightShadowResolution = 0;
            mainLightRenderTargetWidth = 0;
            mainLightRenderTargetHeight = 0;

            visibleLightsShadowCullingInfos = default;
            shadowAtlasLayout = default;
        }
    }
}
