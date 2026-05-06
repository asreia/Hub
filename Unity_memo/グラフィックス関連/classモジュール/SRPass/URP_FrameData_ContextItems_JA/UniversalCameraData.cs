using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// カメラに関連する設定を保持するクラスです。
    /// </summary>
    public partial class UniversalCameraData : ContextItem
    {
        // ステレオコンテキストでViewをどのように公開するかまだ確定していないため、内部カメラデータとして扱います。
        // このAPIは近いうちに変更される可能性があります。
        // [設定元] UniversalRenderPipeline.InitializeAdditionalCameraData の SetViewProjectionAndJitterMatrix(...) で Camera.worldToCameraMatrix から設定。
        Matrix4x4 m_ViewMatrix;
        // [設定元] UniversalRenderPipeline.InitializeAdditionalCameraData で projectionMatrix を作成し、SetViewProjectionAndJitterMatrix(...) へ渡して設定。
        Matrix4x4 m_ProjectionMatrix;
        // [設定元] UniversalRenderPipeline.UpdateTemporalAAData で作った jitterMat を InitializeAdditionalCameraData 経由で設定。
        Matrix4x4 m_JitterMatrix;

        internal void SetViewAndProjectionMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            m_ViewMatrix = viewMatrix;
            m_ProjectionMatrix = projectionMatrix;
            m_JitterMatrix = Matrix4x4.identity;
        }

        internal void SetViewProjectionAndJitterMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 jitterMatrix)
        {
            m_ViewMatrix = viewMatrix;
            m_ProjectionMatrix = projectionMatrix;
            m_JitterMatrix = jitterMatrix;
        }

#if ENABLE_VR && ENABLE_XR_MODULE
        // [設定元] UniversalCameraData.PushBuiltinShaderConstantsXR 内で XR の renderIntoTexture 状態をキャッシュ。
        private bool m_CachedRenderIntoTextureXR;
        // [設定元] UniversalCameraData.PushBuiltinShaderConstantsXR 内で XR 定数を初期化済みか管理。
        private bool m_InitBuiltinXRConstants;
#endif
        // ビルトインのステレオ行列とURPのステレオ行列を設定するヘルパー関数です
        internal void PushBuiltinShaderConstantsXR(RasterCommandBuffer cmd, bool renderIntoTexture)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            // マルチパスでは、他のパスが設定した誤ったビュー投影行列を防ぐため、常に更新が必要です
            bool needsUpdate = !m_InitBuiltinXRConstants || m_CachedRenderIntoTextureXR != renderIntoTexture || !xr.singlePassEnabled;
            if (needsUpdate && xr.enabled )
            {
                var projection0 = GetProjectionMatrix();
                var view0 = GetViewMatrix();
                cmd.SetViewProjectionMatrices(view0, projection0);

                if (xr.singlePassEnabled)
                {
                    var projection1 = GetProjectionMatrix(1);
                    var view1 = GetViewMatrix(1);
                    XRBuiltinShaderConstants.UpdateBuiltinShaderConstants(view0, projection0, renderIntoTexture, 0);
                    XRBuiltinShaderConstants.UpdateBuiltinShaderConstants(view1, projection1, renderIntoTexture, 1);
                    XRBuiltinShaderConstants.SetBuiltinShaderConstants(cmd);
                }
                else
                {
                    // マルチパスのワールド空間カメラ位置を更新します
                    Vector3 worldSpaceCameraPos = Matrix4x4.Inverse(GetViewMatrix(0)).GetColumn(3);
                    cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, worldSpaceCameraPos);

                    //マルチパスは通常レンダーと同じ値を使用し、ステレオ用に設定された値は使用しません。
                    //そのため、unity_MatrixInvV のような値を設定する必要があります。
                    //以下の値は、ScriptableRenderer.cs の SetCameraMatrices 関数で設定される値と同じである必要があります。
                    Matrix4x4 gpuProjectionMatrix = GetGPUProjectionMatrix(renderIntoTexture); // TODO: ターゲットの反転ロジックが分岐した経路を持つため、invProjection が実際の投影と一致しない可能性があります（invP*P==I とは限りません）。
                    Matrix4x4 inverseViewMatrix = Matrix4x4.Inverse(view0);
                    Matrix4x4 inverseProjectionMatrix = Matrix4x4.Inverse(gpuProjectionMatrix);
                    Matrix4x4 inverseViewProjection = inverseViewMatrix * inverseProjectionMatrix;

                    // unity_matrixV と unity_WorldToCamera の間には handedness の不整合があります
                    // Unity は unity_WorldToCamera の handedness を変更します（Camera::CalculateMatrixShaderProps を参照）
                    // 既存のシェーダーを壊さないよう、ここでも同じ変更を行います。（case 1257518）
                    Matrix4x4 worldToCameraMatrix = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f)) * view0;
                    Matrix4x4 cameraToWorldMatrix = worldToCameraMatrix.inverse;
                    cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, worldToCameraMatrix);
                    cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, cameraToWorldMatrix);

                    cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, inverseViewMatrix);
                    cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, inverseProjectionMatrix);
                    cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, inverseViewProjection);
                }
                m_CachedRenderIntoTextureXR = renderIntoTexture;
                m_InitBuiltinXRConstants = true;
            }
#endif
        }

        /// <summary>
        /// カメラのビュー行列を返します。
        /// </summary>
        /// <param name="viewIndex"> ステレオレンダリング時のビューインデックスです。既定では <c>viewIndex</c> は 0 に設定されます。 </param>
        /// <returns> カメラのビュー行列。 </returns>
        public Matrix4x4 GetViewMatrix(int viewIndex = 0)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            if (xr.enabled)
                return xr.GetViewMatrix(viewIndex);
#endif
            return m_ViewMatrix;
        }

        /// <summary>
        /// カメラの投影行列を返します。Temporal系機能のためにジッターが適用されている場合があります。
        /// </summary>
        /// <param name="viewIndex"> ステレオレンダリング時のビューインデックスです。既定では <c>viewIndex</c> は 0 に設定されます。 </param>
        /// <returns> カメラの投影行列。 </returns>
        public Matrix4x4 GetProjectionMatrix(int viewIndex = 0)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            if (xr.enabled)
                return m_JitterMatrix * xr.GetProjMatrix(viewIndex);
#endif
            return m_JitterMatrix * m_ProjectionMatrix;
        }

        internal Matrix4x4 GetProjectionMatrixNoJitter(int viewIndex = 0)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            if (xr.enabled)
                return xr.GetProjMatrix(viewIndex);
#endif
            return m_ProjectionMatrix;
        }

        internal Matrix4x4 GetGPUProjectionMatrix(bool renderIntoTexture, int viewIndex = 0)
        {
            return GL.GetGPUProjectionMatrix(GetProjectionMatrix(viewIndex), renderIntoTexture);
        }

        /// <summary>
        /// Cameraコンポーネント。
        /// </summary>
        // [設定元] UniversalRenderPipeline.CreateCameraData の camera 引数から設定。
        public Camera camera;

        /// <summary>
        /// Cameraのスケール適用後の幅を返します
        /// カメラの pixelWidth を取得し、Render Scale を考慮します
        /// 最小寸法は 1 です。
        /// </summary>
        // [設定元] InitializeScaledDimensions で Camera.pixelWidth / pixelHeight と renderScale から設定し、XR/IUpscaler で再調整される場合あり。
        public int scaledWidth;

        /// <summary>
        /// Cameraのスケール適用後の高さを返します
        /// カメラの pixelHeight を取得し、Render Scale を考慮します
        /// 最小寸法は 1 です。
        /// </summary>
        // [設定元] InitializeScaledDimensions で Camera.pixelWidth / pixelHeight と renderScale から設定し、XR/IUpscaler で再調整される場合あり。
        public int scaledHeight;

        // NOTE: 古い CameraData 互換プロパティで ref return を許可するため、private ではなく internal にしています。
        // それが削除されたら、これは private にできます。
        //
        // 内部および注入されたレンダーパス用の、完全に書き込み可能なカメラ履歴への非所有参照です。
        // パイプライン内部で実行されるパス/コードだけがアクセスすべきです。
        // アクセスするには下の "historyManager" プロパティを使います。
        // [設定元] UniversalAdditionalCameraData.historyManager から取得され、historyManager プロパティ経由でも差し替え可能。
        internal UniversalCameraHistory m_HistoryManager;

        /// <summary>
        /// カメラ履歴テクスチャマネージャー。ScriptableRenderPass からカメラ履歴へアクセスするために使います。
        /// </summary>
        /// <seealso cref="ScriptableRenderPass"/>
        // [設定元] UniversalAdditionalCameraData.historyManager 由来の m_HistoryManager を返す公開アクセサ。
        public UniversalCameraHistory historyManager { get => m_HistoryManager; set => m_HistoryManager = value; }

        /// <summary>
        /// カメラスタッキングで使用されるカメラのレンダータイプ。
        /// <see cref="CameraRenderType"/>
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.renderType から設定し、SceneView などでは Base に補正。
        public CameraRenderType renderType;

        /// <summary>
        /// カメラの最終ターゲットテクスチャを制御します。null の場合、カメラは画面へレンダリング結果を解決します。
        /// </summary>
        // [設定元] Base Camera の Camera.targetTexture から設定。
        public RenderTexture targetTexture;

        /// <summary>
        /// レンダリング用の中間カメラテクスチャを作成するために使用されるRender Texture設定。
        /// </summary>
        // [設定元] UniversalRenderPipeline.CreateRenderTextureDescriptor で Camera と UniversalRenderPipelineAsset 設定から生成。
        public RenderTextureDescriptor cameraTargetDescriptor;
        // [設定元] Camera.pixelRect から設定し、XR 有効時は XRSystem.UpdateCameraData で調整される場合あり。
        internal Rect pixelRect;
        // [設定元] UniversalAdditionalCameraData.useScreenCoordOverride から設定し、未指定時は false。
        internal bool useScreenCoordOverride;
        // [設定元] UniversalAdditionalCameraData があれば screenSizeOverride、なければ cameraData.pixelRect.size から設定。
        internal Vector4 screenSizeOverride;
        // [設定元] UniversalAdditionalCameraData があれば screenCoordScaleBias、なければ Vector2.one から設定。
        internal Vector4 screenCoordScaleBias;
        // [設定元] Camera.pixelWidth から設定し、XR 有効時は XRSystem.UpdateCameraData で調整される場合あり。
        internal int pixelWidth;
        // [設定元] Camera.pixelHeight から設定し、XR 有効時は XRSystem.UpdateCameraData で調整される場合あり。
        internal int pixelHeight;
        // [設定元] Camera.aspect から設定し、XR 有効時は XRSystem.UpdateCameraData で調整される場合あり。
        internal float aspectRatio;

        /// <summary>
        /// カメラテクスチャ作成時に適用するRender Scale。スケール後の範囲は整数へ切り捨てられます。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.renderScale から設定し、Scene/Preview/Reflection などでは 1 に補正。
        public float renderScale;
        // [設定元] UniversalRenderPipeline.InitializeStackedCameraData で renderScale と upscaling 設定から決定。
        internal ImageScalingMode imageScalingMode;
        // [設定元] UniversalRenderPipelineAsset.upscalingFilter から設定し、利用条件に応じて補正。
        internal ImageUpscalingFilter upscalingFilter;
        // [設定元] UniversalRenderPipelineAsset.fsrOverrideSharpness から設定。
        internal bool fsrOverrideSharpness;
        // [設定元] UniversalRenderPipelineAsset.fsrSharpness から設定。
        internal float fsrSharpness;
        // [設定元] UniversalRenderPipelineAsset.hdrColorBufferPrecision から設定。
        internal HDRColorBufferPrecision hdrColorBufferPrecision;

        /// <summary>
        /// このカメラが深度バッファをクリアする場合は true。この設定は <c>CameraRenderType.Overlay</c> 型のカメラにのみ適用されます
        /// <seealso cref="CameraRenderType"/>
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.clearDepth から設定し、Base/Scene では true。
        public bool clearDepth;

        /// <summary>
        /// カメラの種類。
        /// <seealso cref="UnityEngine.CameraType"/>
        /// </summary>
        // [設定元] Camera.cameraType から設定。
        public CameraType cameraType;

        /// <summary>
        /// このカメラが画面全体に対応するビューポートへ描画している場合は true。
        /// </summary>
        // [設定元] Camera.rect がデフォルトViewportかどうかから設定。
        public bool isDefaultViewport;

        /// <summary>
        /// このカメラがHDRカラーターゲットへレンダリングする場合は true。
        /// </summary>
        // [設定元] Camera.allowHDR と UniversalRenderPipelineAsset.supportsHDR から設定。
        public bool isHdrEnabled;

        /// <summary>
        /// このカメラがHDRディスプレイ向けの色変換とエンコードを許可する場合は true。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.allowHDROutput と UniversalRenderPipelineAsset の HDR 対応から設定。
        public bool allowHDROutput;

        /// <summary>
        /// このカメラがアルファチャンネルを書き込める場合は true。ポストプロセスがこれを使用します。カラーターゲットにアルファチャンネルが必要です。
        /// </summary>
        // [設定元] RenderTextureDescriptor の alpha bit と post-process alpha 設定から設定。
        public bool isAlphaOutputEnabled;

        /// <summary>
        /// このカメラが _CameraDepthTexture への書き込みを必要とする場合は true。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.requiresDepthTexture または UniversalRenderPipelineAsset.supportsCameraDepthTexture から設定し、GPU occlusion 要件も反映。
        public bool requiresDepthTexture;

        /// <summary>
        /// このカメラがカメラカラーTextureを _CameraOpaqueTexture へコピーする必要がある場合は true。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.requiresColorTexture または UniversalRenderPipelineAsset.supportsCameraOpaqueTexture から設定し、Overlay では false。
        public bool requiresOpaqueTexture;

        /// <summary>
        /// ポストプロセスパスが深度Textureを必要とする場合に true を返します。
        /// </summary>
        // [設定元] UniversalRenderPipeline.CheckPostProcessForDepth(cameraData) の結果から設定。
        public bool postProcessingRequiresDepthTexture;

        /// <summary>
        /// XRレンダリングが有効な場合に true を返します。
        /// </summary>
        // [設定元] SceneView は false、UniversalAdditionalCameraData があれば allowXRRendering && XRSystem.displayActive、なければ XRSystem.displayActive から設定。
        public bool xrRendering;

        // このカメラをレンダリングするときにGPU Occlusion Cullingを使用する場合は true。
        // [設定元] UniversalRenderPipeline.InitializeAdditionalCameraData で GPUResidentDrawer の Occlusion Culling 有効条件から設定。
        internal bool useGPUOcclusionCulling;

        // [設定元] 代入ではなく XR renderTarget と Display の sRGB 状態から算出。
        internal bool requireSrgbConversion
        {
            get
            {
#if ENABLE_VR && ENABLE_XR_MODULE
                // 一部のXRプラットフォームではSRGBでエンコードする必要がありますが、_SRGB形式のTextureは使えません。8bit/チャンネルの32bit形式でのみ必要です。
                if (xr.enabled)
                    return !xr.renderTargetDesc.sRGB && (xr.renderTargetDesc.graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || xr.renderTargetDesc.graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm) && (QualitySettings.activeColorSpace == ColorSpace.Linear);
#endif

                return targetTexture == null && Display.main.requiresSrgbBlitToBackbuffer;
            }
        }

        /// <summary>
        /// 通常のゲーム内カメラレンダリングの場合は true。
        /// </summary>
        // [設定元] 代入ではなく cameraType から Game Camera かを算出。
        public bool isGameCamera => cameraType == CameraType.Game;

        /// <summary>
        /// エディターのSceneビュー用カメラレンダリングの場合は true。
        /// </summary>
        // [設定元] 代入ではなく cameraType から SceneView Camera かを算出。
        public bool isSceneViewCamera => cameraType == CameraType.SceneView;

        /// <summary>
        /// エディターのPreviewウィンドウ用カメラレンダリングの場合は true。
        /// </summary>
        // [設定元] 代入ではなく cameraType から Preview Camera かを算出。
        public bool isPreviewCamera => cameraType == CameraType.Preview;

        // [設定元] 代入ではなく cameraType から Game/Reflection Camera かを見て Native Render Pass 対応対象を算出。
        internal bool isRenderPassSupportedCamera => (cameraType == CameraType.Game || cameraType == CameraType.Reflection);

        // [設定元] 代入ではなく targetTexture、resolveFinalTarget、cameraType/Game または camera.cameraType/VR から最終出力先が画面かを算出。
        internal bool resolveToScreen => targetTexture == null && resolveFinalTarget && (cameraType == CameraType.Game || camera.cameraType == CameraType.VR);

        /// <summary>
        /// CameraがHDRディスプレイへ出力する場合は true。
        /// </summary>
        // [設定元] 代入ではなく UniversalRenderPipeline.HDROutputForMainDisplayIsActive()/XR の HDR 状態、allowHDROutput、resolveToScreen から算出。
        public bool isHDROutputActive
        {
            get
            {
                bool hdrDisplayOutputActive = UniversalRenderPipeline.HDROutputForMainDisplayIsActive();
#if ENABLE_VR && ENABLE_XR_MODULE
                // XRへレンダリングしている場合は、メインの非XRディスプレイではなくXR Displayを見る必要があります。
                if (xr.enabled)
                    hdrDisplayOutputActive = xr.isHDRDisplayOutputActive;
#endif
                return hdrDisplayOutputActive && allowHDROutput && resolveToScreen;
            }
        }

        /// <summary>
        /// スタック内の最後のカメラがHDRスクリーンへ出力する場合は true
        /// </summary>
        // [設定元] InitializeAdditionalCameraData で isHDROutputActive から設定し、Camera Stack では最終出力HDR状態で上書き。
        internal bool stackLastCameraOutputToHDR;

        /// <summary>
        /// このカメラがレンダリングしている現在のディスプレイのHDR Display情報。
        /// </summary>
        // [設定元] 代入ではなく XR 有効時は xr.hdrDisplayOutputInformation、通常時は HDROutputSettings.main から算出。
        public HDROutputUtils.HDRDisplayInformation hdrDisplayInformation
        {
            get
            {
                HDROutputUtils.HDRDisplayInformation displayInformation;
#if ENABLE_VR && ENABLE_XR_MODULE
                // XRへレンダリングしている場合は、メインの非XRディスプレイではなくXR Displayを見る必要があります。
                if (xr.enabled)
                {
                    displayInformation = xr.hdrDisplayOutputInformation;
                }
                else
#endif
                {
                    HDROutputSettings displaySettings = HDROutputSettings.main;
                    displayInformation = new HDROutputUtils.HDRDisplayInformation(displaySettings.maxFullFrameToneMapLuminance,
                        displaySettings.maxToneMapLuminance,
                        displaySettings.minToneMapLuminance,
                        displaySettings.paperWhiteNits);
                }

                return displayInformation;
            }
        }

        /// <summary>
        /// HDR Displayの色域
        /// </summary>
        // [設定元] 代入ではなく XR 有効時は xr.hdrDisplayOutputColorGamut、通常時は HDROutputSettings.main.displayColorGamut から算出。
        public ColorGamut hdrDisplayColorGamut
        {
            get
            {
#if ENABLE_VR && ENABLE_XR_MODULE
                // XRへレンダリングしている場合は、メインの非XRディスプレイではなくXR Displayを見る必要があります。
                if (xr.enabled)
                {
                    return xr.hdrDisplayOutputColorGamut;
                }
                else
#endif
                {
                    HDROutputSettings displaySettings = HDROutputSettings.main;
                    return displaySettings.displayColorGamut;
                }
            }
        }

        /// <summary>
        /// CameraがオーバーレイUIをレンダリングする場合は true。
        /// </summary>
        // [設定元] 代入ではなく UI Overlay 対応状態と resolveToScreen から算出。
        public bool rendersOverlayUI => SupportedRenderingFeatures.active.rendersUIOverlay && resolveToScreen;

        /// <summary>
        /// HDR出力に必要なオフスクリーンのオーバーレイUIをCameraにレンダリングさせます。
        /// 最初のBase Cameraがそれをレンダリングすると、URPはカメラ間でオフスクリーンTextureを共有します。
        /// </summary>
        // [設定元] UniversalRenderPipeline.RenderCameraStack の HDR Overlay UI 分岐で設定。
        internal bool rendersOffscreenUI;

        /// <summary>
        /// HDR出力用のオフスクリーンオーバーレイUIカバーをCameraにBlitさせます。
        /// オフスクリーンUIカバーのプリパスは、複数カメラのビューポートを合成しても画面全体を満たさない場合でも、オーバーレイUIがディスプレイ全体を覆うことを保証します。
        /// </summary>
        // [設定元] UniversalRenderPipeline.RenderCameraStack の HDR Overlay UI 合成分岐で設定。
        internal bool blitsOffscreenUICover;

        /// <summary>
        /// ハンドルの内容がY軸方向に反転している場合は true。
        /// これは特定のRendering APIでのみ発生します。
        /// それらのプラットフォームでは、Backbufferへレンダリングする場合を除き、どのハンドルの内容も反転します。ただし、
        /// Scene Viewは常に反転します。
        /// 反転空間から非反転空間へ、またはその逆へ遷移する場合、内容を反転する必要があります
        /// シェーダー内で:
        /// shouldPerformYFlip = IsHandleYFlipped(source) != IsHandleYFlipped(target)
        /// </summary>
        /// <param name="handle">反転状態を確認するハンドル。</param>
        /// <returns>内容がY方向に反転している場合は true。</returns>
        public bool IsHandleYFlipped(RTHandle handle)
        {
            if (!SystemInfo.graphicsUVStartsAtTop)
                return true;

            if (cameraType == CameraType.SceneView || cameraType == CameraType.Preview)
                return true;

            var handleID = new RenderTargetIdentifier(handle.nameID, 0, CubemapFace.Unknown, 0);
            bool isBackbuffer = handleID == BuiltinRenderTextureType.CameraTarget || handleID == BuiltinRenderTextureType.Depth;
#if ENABLE_VR && ENABLE_XR_MODULE
            if (xr.enabled)
                isBackbuffer |= handleID == new RenderTargetIdentifier(xr.renderTarget, 0, CubemapFace.Unknown, 0);
#endif
            return !isBackbuffer;
        }

        /// <summary>
        /// Render Targetの投影行列が反転している場合は true。これはパイプラインがレンダリングしているときに発生します
        /// OpenGL以外のプラットフォームでRender Textureへレンダリングするときです。カメラTextureをコピーするカスタムBlitパスを行う場合、
        /// （_CameraColorTexture、_CameraDepthAttachment）行列を反転すべきかを判断するため、このフラグを確認する必要があります
        /// cmd.Draw* でレンダリングし、カメラTextureから読み取るときに。
        /// </summary>
        /// <param name="color">行列が反転しているか確認するColor Render Target。</param>
        /// <param name="depth">color が null の場合に使用されるDepth Render Target。既定では <c>depth</c> は null に設定されます。</param>
        /// <returns> Render Targetの投影行列が反転している場合は true。 </returns>
        public bool IsRenderTargetProjectionMatrixFlipped(RTHandle color, RTHandle depth = null)
        {
            if (!SystemInfo.graphicsUVStartsAtTop)
                return true;

            return targetTexture != null || IsHandleYFlipped(color ?? depth);
        }

        /// <summary>
        /// Temporal Anti-Aliasingが要求されている場合に true を返します
        /// 実行時にTAAが有効か確認するには IsTemporalAAEnabled() を使用してください
        /// </summary>
        /// <returns>TAAが要求されている場合は true</returns>
        internal bool IsTemporalAARequested()
        {
            return antialiasing == AntialiasingMode.TemporalAntiAliasing;
        }

        /// <summary>
        /// パイプラインと指定されたカメラが、Temporal Anti-Aliasingのポストプロセスを有効にしてレンダリングするよう設定されている場合に true を返します
        ///
        /// TAAを選択すると、実行にはパイプライン側の前提条件がいくつか必要になります。その多くはカメラ自体に関するものです。
        /// </summary>
        /// <returns>TAAが有効な場合は true</returns>
        internal bool IsTemporalAAEnabled()
        {
            UniversalAdditionalCameraData additionalCameraData;
            camera.TryGetComponent(out additionalCameraData);

            return IsTemporalAARequested()                                                                                            // 要求済み
                   && postProcessEnabled                                                                                              // ポストプロセス有効
                   && (taaHistory != null)                                                                                            // 初期化済み
                   && (cameraTargetDescriptor.msaaSamples == 1)                                                                       // MSAAなし
                   && !(additionalCameraData?.renderType == CameraRenderType.Overlay || additionalCameraData?.cameraStack.Count > 0)  // Camera Stackなし
                   && !camera.allowDynamicResolution                                                                                  // Dynamic Resolutionなし
                   && renderer.SupportsMotionVectors();                                                                               // Motion Vector実装済み
        }

        /// <summary>
        /// STP Upscalerが要求されている場合に true を返します
        /// 実行時にSTP Upscalerが有効か確認するには IsSTPEnabled() を使用してください。これにはTAAの前処理が必要です
        /// </summary>
        /// <returns>STPが要求されている場合は true</returns>
        internal bool IsSTPRequested()
        {
            return (imageScalingMode == ImageScalingMode.Upscaling) && (upscalingFilter == ImageUpscalingFilter.STP);
        }

        /// <summary>
        /// パイプラインと指定されたカメラがSTP Upscalerでレンダリングするよう設定されている場合に true を返します
        ///
        /// STPの実行時は、URPネイティブTAAが提供する既存のTAA基盤の多くに依存します。そのため、URPはAnti-Aliasing Modeを
        /// STPが要求されたときにTAAへ強制し、TAAロジックの大部分が有効なままになるようにします。この挙動の副作用として、STPは同じ設定上の制約をすべて引き継ぎます
        /// TAAと同じ制約であり、IsTemporalAAEnabled() が false を返す場合は実質的に実行できません。STPを実行するポストプロセスパスのロジックがこの
        /// 状況を処理するため、実行時にTAAのサポート要件が満たされない場合、STPはTAAと同じように振る舞うはずです。
        /// </summary>
        /// <returns>STPが有効な場合は true</returns>
        internal bool IsSTPEnabled()
        {
            return IsSTPRequested() && IsTemporalAAEnabled();
        }

        /// <summary>
        /// URP内部のレンダーパスが不透明オブジェクトを描画するときに使用するソート基準。
        /// GPUがHidden Surface Removalをサポートしている場合、URPはその情報に基づき、不透明オブジェクトを前面から背面へソートすることを避け、
        /// より最適なStatic Batchingの恩恵を得ます。
        /// </summary>
        /// <seealso cref="SortingCriteria"/>
        // [設定元] Camera.opaqueSortMode と SystemInfo.hasHiddenSurfaceRemovalOnGPU から設定。
        public SortingCriteria defaultOpaqueSortFlags;

        /// <summary>
        /// XRPassはRender Target情報とXRViewのリストを保持します。
        /// XRViewにはレンダリングに必要なパラメーター（投影行列、ビュー行列、ビューポートなど）が含まれます
        /// </summary>
        // [設定元] XRSystem.emptyPass で初期化し、XR Camera Stack の描画時に XRPass を設定。
        public XRPass xr { get; internal set; }

        // [設定元] 代入ではなく xr を XRPassUniversal にキャストして返す補助プロパティ。
        internal XRPassUniversal xrUniversal => xr as XRPassUniversal;

        /// <summary>
        /// カメラから見える最大シャドウ距離。0に設定すると、そのカメラでは影が無効になります。
        /// </summary>
        // [設定元] UniversalRenderPipelineAsset.shadowDistance と Camera.farClipPlane の小さい方から設定し、Camera の shadowDistance 設定で無効化される場合あり。
        public float maxShadowDistance;

        /// <summary>
        /// このカメラでポストプロセスが有効な場合は true。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.renderPostProcessing または SceneView の CoreUtils.ArePostProcessesEnabled から設定。
        public bool postProcessEnabled;

        /// <summary>
        /// このカメラのStack内にあるいずれかのカメラでポストプロセスが有効な場合は true。
        /// </summary>
        // [設定元] 現在の Camera の postProcessEnabled から始まり、Camera Stack 内の有効な PostProcess を反映。
        internal bool stackAnyPostProcessingEnabled;

        /// <summary>
        /// カメラキャプチャ用に、レンダーループの最後で実行されるアクション群をRendererへ提供します。
        /// </summary>
        // [設定元] CameraCaptureBridge.GetCachedCaptureActionsEnumerator(baseCamera) から設定。
        public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions;

        /// <summary>
        /// カメラのVolume Layer Mask。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.volumeLayerMask または SceneView/Default の Volume Layer から設定。
        public LayerMask volumeLayerMask;

        /// <summary>
        /// カメラのVolume Trigger。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.volumeTrigger があればそれ、なければ Camera.transform から設定。
        public Transform volumeTrigger;

        /// <summary>
        /// trueに設定すると、統合ポストプロセススタックはポストプロセス前のレンダーパスで生成されたNaNを黒/ゼロに置き換えます。
        /// このオプションを有効にすると、目に見えるパフォーマンス影響があります。NaN問題を特定するため、開発モード中に使用してください。
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.stopNaN から設定。
        public bool isStopNaNEnabled;

        /// <summary>
        /// trueに設定すると、ディザリングを適用するための最終ポストプロセスパスが適用されます。
        /// これはポストプロセスのAnti-Aliasingと組み合わせられます。
        /// <seealso cref="antialiasing"/>
        /// </summary>
        // [設定元] UniversalAdditionalCameraData.dithering から設定。
        public bool isDitheringEnabled;

        /// <summary>
        /// 統合ポストプロセススタックが使用するAnti-Aliasing Modeを制御します。
        /// <c>AntialiasingMode.None</c> 以外の値を選択すると、Anti-Aliasingを適用するための最終ポストプロセスパスが適用されます。
        /// このパスはディザリングと組み合わせられます。
        /// <see cref="AntialiasingMode"/>
        /// <seealso cref="isDitheringEnabled"/>
        /// </summary>
        // [設定元] UniversalAdditionalCameraData があれば antialiasing、SceneView/未指定時は AntialiasingMode.None から設定。
        public AntialiasingMode antialiasing;

        /// <summary>
        /// Anti-Aliasing ModeのAnti-Aliasing品質を制御します。
        /// <see cref="antialiasingQuality"/>
        /// <seealso cref="AntialiasingMode"/>
        /// </summary>
        // [設定元] UniversalAdditionalCameraData があれば antialiasingQuality、SceneView/未指定時は AntialiasingQuality.High から設定。
        public AntialiasingQuality antialiasingQuality;

        /// <summary>
        /// このカメラが使用する現在のRendererを返します。
        /// <see cref="ScriptableRenderer"/>
        /// </summary>
        // [設定元] UniversalRenderPipeline.GetRenderer(camera, additionalCameraData) から設定。
        public ScriptableRenderer renderer;

        /// <summary>
        /// このカメラが最終的なCamera Render Targetへレンダリング結果を解決している場合は true。
        /// Camera Stackをレンダリングする場合、Stack内の最後のカメラだけがCamera Targetへ解決します。
        /// </summary>
        // [設定元] UniversalRenderPipeline.InitializeAdditionalCameraData の resolveFinalTarget 引数から設定。
        public bool resolveFinalTarget;

        /// <summary>
        /// ワールド空間でのカメラ位置。
        /// </summary>
        // [設定元] Camera.transform.position から設定。
        public Vector3 worldSpaceCameraPos;

        /// <summary>
        /// アクティブな色空間での最終背景色。
        /// </summary>
        // [設定元] Camera.backgroundColor をアクティブな ColorSpace に合わせて変換して設定。
        public Color backgroundColor;

        /// <summary>
        /// 主に蓄積Texture用の永続的なTAAデータ。
        /// </summary>
        // [設定元] UpdateTemporalAAData で UniversalCameraHistory から TaaHistory を取得して設定。
        internal TaaHistory taaHistory;

        /// <summary>
        /// STP履歴データ。永続状態とTextureの両方を含みます。
        /// </summary>
        // [設定元] UpdateTemporalAAData で UniversalCameraHistory から StpHistory を取得して設定。
        internal StpHistory stpHistory;

        // TAA設定。
        // [設定元] UniversalAdditionalCameraData.taaSettings から取得し、Debug 設定で上書きされる場合あり。
        internal TemporalAA.Settings taaSettings;

        // このカメラに対してポストプロセス履歴のリセットがトリガーされています。
        // [設定元] 代入ではなく taaSettings.resetHistoryFrames が残っているかから算出。
        internal bool resetHistory
        {
            get => taaSettings.resetHistoryFrames != 0;
        }

        /// <summary>
        /// Overlay Camera Stackの先頭にあるCamera。Stackがない場合は、上の camera フィールドと同じです。
        /// </summary>
        // [設定元] Camera Stack の Overlay Camera 処理で Base Camera を設定し、通常の Base Camera では null。
        public Camera baseCamera;

        /// <summary>
        /// baseCamera フィールドが、そのフレームへレンダリングされる最後のBase Cameraである場合に true を返します。
        /// Camera Stack内の最後のカメラは最後のOverlay Cameraを意味しますが、これはすべての入力Base Cameraのうち最後のものを示します。
        /// </summary>
        // [設定元] UniversalRenderPipeline.InitializeAdditionalCameraData の isLastBaseCamera 引数から設定。
        internal bool isLastBaseCamera;

        ///<inheritdoc/>
        public override void Reset()
        {
            m_ViewMatrix = default;
            m_ProjectionMatrix = default;
            m_JitterMatrix = default;
#if ENABLE_VR && ENABLE_XR_MODULE
            m_CachedRenderIntoTextureXR = false;
            m_InitBuiltinXRConstants = false;
#endif
            camera = null;
            renderType = CameraRenderType.Base;
            targetTexture = null;
            cameraTargetDescriptor = default;
            pixelRect = default;
            useScreenCoordOverride = false;
            screenSizeOverride = default;
            screenCoordScaleBias = default;
            pixelWidth = 0;
            pixelHeight = 0;
            aspectRatio = 0.0f;
            renderScale = 1.0f;
            imageScalingMode = ImageScalingMode.None;
            upscalingFilter = ImageUpscalingFilter.Point;
            fsrOverrideSharpness = false;
            fsrSharpness = 0.0f;
            hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
            clearDepth = false;
            cameraType = CameraType.Game;
            isDefaultViewport = false;
            isHdrEnabled = false;
            allowHDROutput = false;
            isAlphaOutputEnabled = false;
            requiresDepthTexture = false;
            requiresOpaqueTexture = false;
            postProcessingRequiresDepthTexture = false;
            xrRendering = false;
            useGPUOcclusionCulling = false;
            defaultOpaqueSortFlags = SortingCriteria.None;
            xr = default;
            maxShadowDistance = 0.0f;
            postProcessEnabled = false;
            captureActions = default;
            volumeLayerMask = 0;
            volumeTrigger = default;
            isStopNaNEnabled = false;
            isDitheringEnabled = false;
            antialiasing = AntialiasingMode.None;
            antialiasingQuality = AntialiasingQuality.Low;
            renderer = null;
            resolveFinalTarget = false;
            worldSpaceCameraPos = default;
            backgroundColor = Color.black;
            taaHistory = null;
            stpHistory = null;
            taaSettings = default;
            baseCamera = null;
            isLastBaseCamera = false;
            stackAnyPostProcessingEnabled = false;
            stackLastCameraOutputToHDR = false;
            rendersOffscreenUI = false;
            blitsOffscreenUICover = false;
        }
    }
}
