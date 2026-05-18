# URPメモ

```csharp
&& baseCamera.targetTexture == null && //カメラスタッキングは`baseCamera`の`targetTexture`(nullの場合はバックバッファ)をスタック内で共通して使う？
```

- `static UniversalRenderPipelineAsset UniversalRenderPipeline.asset`: `{get => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;}`
- `class UniversalRenderPipelineAsset`
    ```csharp (UniversalRenderPipelineAsset.cs:790)
    protected override RenderPipeline CreatePipeline()
    {
        DestroyRenderers(); //=> for(..) m_Renderers[i].Dispose()
        var pipeline = new UniversalRenderPipeline(this);
        CreateRenderers(); //=> for(..) m_Renderers[i] = m_RendererDataList[i]❰SRD❱.InternalCreateRenderer() => uRD.Create() => UR.ctor(uRD):SR.ctor(sRD)
        return pipeline;
    }
    ```
  - `new UniversalRenderPipeline(this)`
    ```csharp (UniversalRenderPipeline.cs:226)
    public UniversalRenderPipeline(UniversalRenderPipelineAsset asset)
    {
        pipelineAsset = asset;

        m_GlobalSettings = UniversalRenderPipelineGlobalSettings.instance; //実体は`Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`

        //`UniversalRenderPipelineGlobalSettings.instance`から`IRenderPipelineResources:IRenderPipelineGraphicsSettings`を設定
        runtimeTextures = GraphicsSettings.GetRenderPipelineSettings<UniversalRenderPipelineRuntimeTextures>();
        var shaders = GraphicsSettings.GetRenderPipelineSettings<UniversalRenderPipelineRuntimeShaders>();
            Blitter.Initialize(shaders.coreBlitPS, shaders.coreBlitColorAndDepthPS);

        //`SupportedRenderingFeatures.active`を設定
        SetSupportedRenderingFeatures(pipelineAsset);

        // RTHandle システムの初期状態です。
        RTHandles.Initialize(Screen.width, Screen.height);

        // グローバルシェーダーキーワードを初期化します
        ShaderGlobalKeywords.InitializeShaderGlobalKeywords(); //大量の`ShaderGlobalKeywords.～ = GlobalKeyword.Create(ShaderKeywordStrings.～)`

        QualitySettings.antiAliasing = asset.msaaSampleCount; //設定してもURPは`asset.msaaSampleCount`しか見てないらしい

        //ポスプロのボリューム初期化
        var defaultVolumeProfileSettings = GraphicsSettings.GetRenderPipelineSettings<URPDefaultVolumeProfileSettings>();
            VolumeManager.instance.Initialize(defaultVolumeProfileSettings.volumeProfile, asset.volumeProfile);

        Lightmapping.SetDelegate(lightsDelegate);

        DecalProjector.defaultMaterial = asset.decalMaterial;

        s_RenderGraph = new RenderGraph("URPRenderGraph");

        s_RTHandlePool = new RTHandleResourcePool();

        QualitySettings.enableLODCrossFade = asset.enableLODCrossFade;

        //APV初期化
        if (asset.lightProbeSystem == LightProbeSystem.ProbeVolumes){ProbeReferenceVolume.instance.Initialize(new ProbeVolumeSystemParameters{～})}        
    }
    ```
  - `SR.ctor(sRD data)`
    ```csharp
    public ScriptableRenderer(ScriptableRendererData data)
    {
        foreach (var feature in data.rendererFeatures)
        {
            feature.Create();
            m_RendererFeatures.Add(feature);
        }
        useRenderPassEnabled = data.useNativeRenderPass;
        m_ActiveRenderPassQueue.Clear(); //m_ARPQ.Clear()
    }
    ```
  - `UR.ctor(uRD data)`
  ```csharp (UniversalRenderer.cs:211)
    public UniversalRenderer(UniversalRendererData data) : base(data)
    {
        //`UniversalRenderPipelineGlobalSettings.instance`から`IRenderPipelineResources:IRenderPipelineGraphicsSettings`を設定
        if (GraphicsSettings.TryGetRenderPipelineSettings<UniversalRenderPipelineRuntimeShaders>(out var shadersResources))
        {
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(shadersResources.coreBlitPS);
            m_BlitHDRMaterial = CoreUtils.CreateEngineMaterial(shadersResources.blitHDROverlay);
            m_SamplingMaterial = CoreUtils.CreateEngineMaterial(shadersResources.samplingPS);
            m_BlitOffscreenUICoverMaterial = CoreUtils.CreateEngineMaterial(shadersResources.blitHDROverlay);
        }
        Shader copyDephPS = null;
        if (GraphicsSettings.TryGetRenderPipelineSettings<UniversalRendererResources>(out var universalRendererShaders))
        {
            copyDephPS = universalRendererShaders.copyDepthPS;
            m_StencilDeferredMaterial = CoreUtils.CreateEngineMaterial(universalRendererShaders.stencilDeferredPS);
            m_ClusterDeferredMaterial = CoreUtils.CreateEngineMaterial(universalRendererShaders.clusterDeferred);
            m_CameraMotionVecMaterial = CoreUtils.CreateEngineMaterial(universalRendererShaders.cameraMotionVector);
            m_StencilCrossFadeRenderPass = new StencilCrossFadeRenderPass(universalRendererShaders.stencilDitherMaskSeedPS);
        }

        //`LightCookieManager`生成
        var asset = UniversalRenderPipeline.asset;
        if (asset.supportsLightCookies)
        {
            var settings = LightCookieManager.Settings.Create();
            settings.atlas.⟪format¦resolution⟫ = asset.additionalLightsCookie⟪format¦resolution⟫
            m_LightCookieManager = new LightCookieManager(ref settings);
        }

        //`ForwardLights`生成
        ForwardLights.InitParams forwardInitParams;
        forwardInitParams.lightCookieManager = m_LightCookieManager;
        forwardInitParams.forwardPlus = data.renderingMode == RenderingMode.ForwardPlus;
        m_ForwardLights = new ForwardLights(forwardInitParams);

        //＠❰m_❱フィールドメンバ = data❰:uRD❱.～
        m_DefaultStencilState❰:StencilState❱ = data.defaultStencilState❰:StencilStateData❱
        m_IntermediateTextureMode = data.intermediateTextureMode;
        prepassLayerMask = data.prepassLayerMask;
        opaqueLayerMask = data.opaqueLayerMask;
        transparentLayerMask = data.transparentLayerMask;
        shadowTransparentReceive = data.shadowTransparentReceive;
        stripShadowsOffVariants = data.stripShadowsOffVariants;
        stripAdditionalLightOffVariants = data.stripAdditionalLightOffVariants;
        m_RenderingMode = data.renderingMode;
        m_DepthPrimingMode = data.depthPrimingMode;
        m_CopyDepthMode = data.copyDepthMode;
        m_CameraDepthAttachmentFormat = data.depthAttachmentFormat;
        m_CameraDepthTextureFormat = data.depthTextureFormat;
        useRenderPassEnabled = data.useNativeRenderPass;

        //以下Passの作成

        //`LensFlareCommonSRP`初期化
        LensFlareCommonSRP.mergeNeeded = 0;
        LensFlareCommonSRP.maxLensFlareWithOcclusionTemporalSample = 1;
        LensFlareCommonSRP.Initialize();

        // Note: すべてのカスタムレンダー Pass が先に挿入され、安定ソートが行われるため、
        // 組み込み Pass は before イベントに挿入します。
        m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
        m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);

        m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.opaque, prepassLayerMask);
        m_DepthNormalPrepass = new DepthNormalOnlyPass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.opaque, prepassLayerMask);

        // Editor のワイヤーフレームレンダリングやオフスクリーン深度テクスチャレンダリングで使用するため、Deferred でも常にこの Pass を作成します。
        m_RenderOpaqueForwardPass = new DrawObjectsPass(URPProfileId.DrawOpaqueObjects, true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference); //読んだ
        m_RenderOpaqueForwardWithRenderingLayersPass = new DrawObjectsWithRenderingLayersPass(URPProfileId.DrawOpaqueObjects, true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference);

        bool copyDepthAfterTransparents = m_CopyDepthMode == CopyDepthMode.AfterTransparents;
        RenderPassEvent copyDepthEvent = copyDepthAfterTransparents ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingSkybox;

        m_CopyDepthPass = new CopyDepthPass(
            copyDepthEvent,
            copyDephPS,
            shouldClear: true,
            copyResolvedDepth: RenderingUtils.MultisampleDepthResolveSupported() && copyDepthAfterTransparents); //読んだ

        // モーションベクトルは（コピーされた）深度テクスチャに依存します。モーションベクトルの計算では深度を再投影します。
        m_MotionVectorPass = new MotionVectorRenderPass(copyDepthEvent + 1, m_CameraMotionVecMaterial, data.opaqueLayerMask);

        m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
        m_CopyColorPass = new CopyColorPass(RenderPassEvent.AfterRenderingSkybox, m_SamplingMaterial, m_BlitMaterial);

            m_TransparentSettingsPass = new TransparentSettingsPass(RenderPassEvent.BeforeRenderingTransparents, data.shadowTransparentReceive);
            m_RenderTransparentForwardPass = new DrawObjectsPass(URPProfileId.DrawTransparentObjects, false, RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent, data.transparentLayerMask, m_DefaultStencilState, stencilData.stencilReference);

        m_OnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);

        // "raw color/depth" 用の履歴生成 Pass です。ユーザーが明示的に要求した場合にのみ実行されます。
        // VFX システムのパーティクルがこれらを使用します。RawColorHistory.cs を参照してください。
        m_HistoryRawColorCopyPass = new CopyColorPass(RenderPassEvent.BeforeRenderingPostProcessing, m_SamplingMaterial, m_BlitMaterial, customPassName: "Copy Color Raw History");
        m_HistoryRawDepthCopyPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingPostProcessing, copyDephPS, false, RenderingUtils.MultisampleDepthResolveSupported(), customPassName: "Copy Depth Raw History");

        m_DrawOffscreenUIPass = new DrawScreenSpaceUIPass(RenderPassEvent.BeforeRenderingPostProcessing);
        m_DrawOverlayUIPass = new DrawScreenSpaceUIPass(RenderPassEvent.AfterRendering + k_AfterFinalBlitPassQueueOffset); // m_FinalBlitPass の後
        
        // postProcessData がない場合は、ポストプロセスが無効であることを意味します
            m_PostProcess = new PostProcess(data.postProcessData);
            m_ColorGradingLutPassRenderGraph = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrePasses, data.postProcessData);

        m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
        m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering + k_FinalBlitPassQueueOffset, m_BlitMaterial, m_BlitHDRMaterial);
        m_OffscreenUICoverPrepass = new FinalBlitPass(RenderPassEvent.BeforeRenderingPostProcessing + k_FinalBlitPassQueueOffset, m_BlitMaterial, m_BlitOffscreenUICoverMaterial);
    }
  ```

```csharp
protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
{
    int cameraCount = cameras.Count;
    // 状況によっては Render Graph で帯域幅を最適化します
    SetupScreenMSAASamplesState(cameraCount); //`canOptimizeScreenMSAASamples`: Codex:「URP のメイン描画が中間ターゲットで MSAA されているなら、最終バックバッファ側の MSAA は不要だから 1 にする」という最適化です。

    using var profScope = new ProfilingScope(ProfilingSampler.Get(URPProfileId.UniversalRenderTotal));

    //`public static event Action<ScriptableRenderContext, List<Camera>> ⟪begin¦end⟫ContextRendering`を実行する。
    using (new ContextRenderingScope(renderContext, cameras))
    {
        GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        GraphicsSettings.lightsUseColorTemperature = true;
        SetupPerFrameShaderConstants(); //主にLODCrossFadeの`ditheringTexture`などを設定。

        SortCameras(cameras);
        int lastBaseCameraIndex = GetLastBaseCameraIndex(cameras); //`CameraRenderType.Base`であるような最後の`camera`のindexを取得する

        for (int i = 0; i < cameraCount; ++i)
        {
            // カメラは Base カメラまたは Overlay カメラのどちらにもなり得ます
            var camera = cameras[i];
            bool isLastBaseCamera = i == lastBaseCameraIndex;
            if (IsGameCamera(camera)) //`camera.cameraType == CameraType.⟪Game¦VR⟫`
            {
                // カメラが Base カメラの場合のみスタックをレンダリングします
                RenderCameraStack(renderContext, camera, isLastBaseCamera);
            }
            else
            {
                using (new CameraRenderingScope(renderContext, camera))
                {
#if VISUAL_EFFECT_GRAPH_0_0_1_OR_NEWER
                // マテリアルを準備するため、カリング前に呼び出す必要があります。VisualEffect コンポーネントが存在しない場合、このメソッドは何もしません。
                // 注: この段階では XR カメラを想定していません
                VFX.VFXManager.PrepareCamera(camera);
#endif
                    UpdateVolumeFramework(camera, null); //非⟪Game¦VR⟫は毎フレーム更新(`VolumeManager.instance.Update(trigger, layerMask)`)する
                    // カメラが Base カメラの場合のみレンダリングします
                    RenderSingleCameraInternal(renderContext, camera, isLastBaseCamera);
                }
            }
        }

        s_RenderGraph.EndFrame();
        s_RTHandlePool.PurgeUnusedResources(Time.frameCount); //void RTHandleResourcePool.PurgeUnusedResources(int currentFrameIndex)
    }
}
```

```csharp
internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera, /*ref UniversalAdditionalCameraData additionalCameraData == null*/, bool isLastBaseCamera = true)
{
    var frameData = GetRenderer(camera, additionalCameraData).frameData;
    var cameraData = CreateCameraData(frameData, camera, additionalCameraData);
    InitializeAdditionalCameraData(camera, additionalCameraData, true, isLastBaseCamera, cameraData);
    RenderSingleCamera(context, cameraData);
}
```

```csharp
static ScriptableRenderer GetRenderer(Camera camera, UniversalAdditionalCameraData additionalCameraData)
{
    var renderer = additionalCameraData != null ? additionalCameraData.scriptableRenderer : null;
    if (renderer == null || camera.cameraType == CameraType.SceneView)
        renderer = asset.scriptableRenderer;
    return renderer;
}
```

大体、**カメラ毎**に、`uCD`を**作成**して`RenderSingleCamera(ctx, uCD)`を呼んでいるだけ (**uCD作成**(`CreateCameraData`,`InitializeAdditionalCameraData`) と `UpdateVolumeFramework`,`VFX.VFXManager.PrepareCamera`)
```csharp
        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera, bool isLastBaseCamera)
        {
            using var profScope = new ProfilingScope(ProfilingSampler.Get(URPProfileId.RenderCameraStack));

            baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var baseCameraAdditionalData);
            var renderer = GetRenderer(baseCamera, baseCameraAdditionalData);

            if (baseCameraAdditionalData.renderType == CameraRenderType.Overlay) return; //`CameraRenderType.Base`な`Camera`のみ通す
            List<Camera> stackedOverlayCameras = baseCameraAdditionalData.cameraStack; //スタックカメラ取得

            // スタック内のいずれかのカメラでポストプロセスが有効かどうかを確認するために、この bool を使用します
            bool stackAnyPostProcessingEnabled = baseCameraAdditionalData.renderPostProcessing;

            // レンダリング時に画面へ解決できるように、スタック内の最後のアクティブカメラを知る必要があります。
            int lastActiveOverlayCameraIndex = -1;
            if (stackedOverlayCameras != null)
            {
                stackedOverlayCamerasRequireDepthForPostProcessing = false;

                for (int i = 0; i < stackedOverlayCameras.Count; ++i)
                {
                    Camera overlayCamera = stackedOverlayCameras[i];

                    if (!overlayCamera.isActiveAndEnabled) continue;

                    overlayCamera.TryGetComponent<UniversalAdditionalCameraData>(out var data);
                    stackAnyPostProcessingEnabled |= data.renderPostProcessing;

                    lastActiveOverlayCameraIndex = i;

                    //UpdateVolumeFramework(overlayCamera, data);が抜けている気がする↓
                    stackedOverlayCamerasRequireDepthForPostProcessing |= CheckPostProcessForDepth(); //if(VolumeManager.instance.stack.GetComponent<⟪DepthOfField¦MotionBlur⟫>().IsActive()) return true
                }
            }

            bool isStackedRendering = lastActiveOverlayCameraIndex != -1;

            var xrLayout = XRSystem.NewLayout();
            xrLayout.AddCamera(baseCamera, baseCameraAdditionalData?.allowXRRendering ?? true);
            foreach ((Camera _, XRPass xrPass) in xrLayout.GetActivePasses()) // XR multi-pass が有効な場合、各カメラは異なるパラメーターで複数回レンダリングされる可能性があります
            {
                // Base カメラのレンダリング==================================================
                using (new CameraRenderingScope(context, baseCamera)) //`public static event Action<ScriptableRenderContext, Camera> ⟪begin¦end⟫CameraRendering`を実行する。
                {
                    // 追加カメラデータを初期化する前に Volume Framework を更新します
                    UpdateVolumeFramework(baseCamera, baseCameraAdditionalData);

                    ContextContainer frameData = renderer.frameData;
                    UniversalCameraData baseCameraData = CreateCameraData(frameData, baseCamera, baseCameraAdditionalData);
                    InitializeAdditionalCameraData(baseCamera, baseCameraAdditionalData, !isStackedRendering, isLastBaseCamera, baseCameraData);

                    // マテリアルを準備するため、カリング前に呼び出す必要があります。VisualEffect コンポーネントが存在しない場合、このメソッドは何もしません。
                    VFX.VFXCameraXRSettings cameraXRSettings;cameraXRSettings.view～ = baseCameraData.xr.～;
                    VFX.VFXManager.PrepareCamera(baseCamera, cameraXRSettings); //=>`static extern void PrepareCamera_Injected(IntPtr cam, [In] ref VFXCameraXRSettings camXRSettings)`
                        //Codex:VisualEffectGraph用に、そのカメラ向けのマテリアルなどのVFX描画準備をするAPIです。

                    // フレーム後半で Overlay カメラが必要とする場合にシーン深度が保存されるよう、Base カメラのフラグを更新します
                    baseCameraData.postProcessingRequiresDepthTexture |= stackedOverlayCamerasRequireDepthForPostProcessing;

                    RenderSingleCamera(context, baseCameraData);
                }

                // Overlay カメラのレンダリング==================================================
                if (isStackedRendering)
                {
                    for (int i = 0; i < stackedOverlayCameras.Count; ++i)
                    {
                        var overlayCamera = stackedOverlayCameras[i];
                        if (!overlayCamera.isActiveAndEnabled) continue;
                        overlayCamera.TryGetComponent<UniversalAdditionalCameraData>(out var overlayAdditionalCameraData);
                        if (overlayAdditionalCameraData == null) continue;

                        ContextContainer overlayFrameData = GetRenderer(overlayCamera, overlayAdditionalCameraData).frameData;

                        UniversalCameraData overlayCameraData = CreateCameraData(overlayFrameData, baseCamera, baseCameraAdditionalData);       //`baseCamera`を使う
                        InitializeAdditionalCameraData(overlayCamera, overlayAdditionalCameraData, false, isLastBaseCamera, overlayCameraData); //`overlayCamera`を使う //Codex:かなり冗長/怪しい
                        overlayCameraData.baseCamera = baseCamera;
                        overlayCameraData.camera = overlayCamera;

                        using (new CameraRenderingScope(context, overlayCamera))
                        {
                            // マテリアルを準備するため、カリング前に呼び出す必要があります。VisualEffect コンポーネントが存在しない場合、このメソッドは何もしません。
                            VFX.VFXManager.PrepareCamera(overlayCamera, cameraXRSettings);

                            UpdateVolumeFramework(overlayCamera, overlayAdditionalCameraData);

                            bool isLastOverlayCamera = i == lastActiveOverlayCameraIndex;
                            InitializeAdditionalCameraData(overlayCamera, overlayAdditionalCameraData, isLastOverlayCamera, isLastBaseCamera, overlayCameraData);

                            overlayCameraData.stackAnyPostProcessingEnabled = stackAnyPostProcessingEnabled;

                            RenderSingleCamera(context, overlayCameraData);
                        }
                    }
                }
            }
            XRSystem.EndLayout();
        }
```

```csharp

```
