using System;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// ポストプロセスで使用される設定。
    /// </summary>
    public class UniversalPostProcessingData : ContextItem
    {
        /// <summary>
        /// Camera Stackのレンダリング中にポストプロセス効果が有効な場合は true。
        /// </summary>
        // [設定元] Camera Stack 内で Post Processing が一つでも有効かを示す cameraData.stackAnyPostProcessingEnabled から設定。
        public bool isEnabled;

        /// <summary>
        /// 使用する <c>ColorGradingMode</c>。
        /// </summary>
        /// <seealso cref="ColorGradingMode"/>
        // [設定元] UniversalRenderPipelineAsset の HDR/Color Grading 設定から設定し、HDR 出力時は HDR mode に補正。
        public ColorGradingMode gradingMode;

        /// <summary>
        /// Look Up Table（LUT）のサイズ
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.colorGradingLutSize から設定。
        public int lutSize;

        /// <summary>
        /// sRGBとLinear色空間の間で変換するときに高速近似関数を使用する場合は true。それ以外の場合は false。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.useFastSRGBLinearConversion から設定。
        public bool useFastSRGBLinearConversion;

        /// <summary>
        /// このAssetがScreen Space Lens Flareをサポートしている場合は true、それ以外の場合は false を返します。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.supportScreenSpaceLensFlare から設定。
        public bool supportScreenSpaceLensFlare;

        /// <summary>
        /// このAssetがData Driven Lens Flareをサポートしている場合は true、それ以外の場合は false を返します。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.supportDataDrivenLensFlare から設定。
        public bool supportDataDrivenLensFlare;

        /// <summary>
        /// アクティブなUpscalerがない場合は null を返します
        /// </summary>
#if ENABLE_UPSCALER_FRAMEWORK
        // [設定元] IUpscaler 使用時に upscaling.GetActiveUpscaler() から設定し、それ以外は null。
        public IUpscaler activeUpscaler;
#endif

        /// <summary>
        /// IDisposableインターフェース用に追加された空の関数。
        /// </summary>
        public override void Reset()
        {
            isEnabled = default;
            gradingMode = ColorGradingMode.LowDynamicRange;
            lutSize = 0;
            useFastSRGBLinearConversion = false;
            supportScreenSpaceLensFlare = false;
            supportDataDrivenLensFlare = false;
        }
    }
}
