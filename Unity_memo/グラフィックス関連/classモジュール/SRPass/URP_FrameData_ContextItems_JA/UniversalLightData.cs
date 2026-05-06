using System;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// ライトに関連する設定を保持するクラスです。
    /// </summary>
    public class UniversalLightData : ContextItem
    {
        /// <summary>
        /// Cullingで返された <c>VisibleLight</c> リスト内のMain Lightインデックスを保持します。シーンにMain Lightがない場合、<c>mainLightIndex</c> は -1 に設定されます。
        /// Main Lightは、ライト設定でSun Sourceとして割り当てられたDirectional Light、または最も明るいDirectional Lightです。
        /// <seealso cref="CullingResults"/>
        /// </summary>
        // [設定元] UniversalRenderPipeline.GetMainLightIndex(settings, visibleLights) の結果から設定。
        public int mainLightIndex;

        /// <summary>
        /// カメラから見えるAdditional Lightの数。
        /// </summary>
        // [設定元] additionalLightsRenderingMode が有効な場合、VisibleLight 数から Main Light を除き UniversalRenderPipeline.maxVisibleAdditionalLights で制限して設定。
        public int additionalLightsCount;

        /// <summary>
        /// オブジェクトごとにシェーディング可能なライトの最大数。この値はForward Renderingにのみ影響します。
        /// </summary>
        // [設定元] additionalLightsRenderingMode が有効な場合、UniversalRenderPipelineAsset.maxAdditionalLightsCount と UniversalRenderPipeline.maxPerObjectLights の小さい方から設定。
        public int maxPerObjectAdditionalLightsCount;

        /// <summary>
        /// Cullingによって返された可視ライトのリスト。
        /// </summary>
        // [設定元] UniversalRenderingData.cullResults.visibleLights から設定。
        public NativeArray<VisibleLight> visibleLights;

        /// <summary>
        /// Additional LightをVertex Shaderでシェーディングする場合は true。それ以外の場合、Additional LightはPixel単位でシェーディングされます。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.additionalLightsRenderingMode が PerVertex かどうかから設定。
        public bool shadeAdditionalLightsPerVertex;

        /// <summary>
        /// Mixed Lightingがサポートされている場合は true。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.supportsMixedLighting から設定。
        public bool supportsMixedLighting;

        /// <summary>
        /// Reflection ProbeでBox Projectionが有効な場合は true。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.reflectionProbeBoxProjection から設定。
        public bool reflectionProbeBoxProjection;

        /// <summary>
        /// Reflection ProbeでBlendingが有効な場合は true。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.ShouldUseReflectionProbeBlending() から設定。
        public bool reflectionProbeBlending;

        /// <summary>
        /// Reflection Probe Atlasが有効な場合は true。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.ShouldUseReflectionProbeAtlasBlending(renderingMode) から設定。
        public bool reflectionProbeAtlas;

        /// <summary>
        /// Light Layerが有効な場合は true。
        /// </summary>
        // [設定元] SystemInfo の LightLayer 対応と UniversalRenderPipelineAsset.useRenderingLayers から設定。
        public bool supportsLightLayers;

        /// <summary>
        /// Additional Lightが有効な場合は true。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.additionalLightsRenderingMode が Disabled でないかから設定。
        public bool supportsAdditionalLights;

        /// <inheritdoc/>
        public override void Reset()
        {
            mainLightIndex = -1;
            additionalLightsCount = 0;
            maxPerObjectAdditionalLightsCount = 0;
            visibleLights = default;
            shadeAdditionalLightsPerVertex = false;
            supportsMixedLighting = false;
            reflectionProbeBoxProjection = false;
            reflectionProbeBlending = false;
            supportsLightLayers = false;
            supportsAdditionalLights = false;
        }
    }
}
