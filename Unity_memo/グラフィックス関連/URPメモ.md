# URPメモ

```csharp
&& baseCamera.targetTexture == null && //カメラスタッキングは`baseCamera`の`targetTexture`(nullの場合はバックバッファ)をスタック内で共通して使う？
```

- `static UniversalRenderPipelineAsset UniversalRenderPipeline.asset`: `{get => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;}`

- `static ScriptableRenderer uRP.GetRenderer(camera, additionalCameraData)`
    ```csharp (UniversalRenderPipeline.cs:1324)
    static ScriptableRenderer GetRenderer(Camera camera, UniversalAdditionalCameraData additionalCameraData)
    {
        var renderer = additionalCameraData != null ? additionalCameraData.scriptableRenderer : null;
        if (renderer == null || camera.cameraType == CameraType.SceneView)
            renderer = asset.scriptableRenderer;
        return renderer;
    }
    ```

- `override RenderPipeline uRPA:CreatePipeline()`
    ```csharp (UniversalRenderPipelineAsset.cs:790)
    protected override RenderPipeline CreatePipeline()
    {
        DestroyRenderers(); //=> for(..) m_Renderers[i].Dispose()
        var pipeline = new UniversalRenderPipeline(this);
        CreateRenderers(); //=> for(..) m_Renderers[i] = m_RendererDataList[i]❰SRD❱.InternalCreateRenderer() => uRD.Create() => UR.ctor(uRD):SR.ctor(sRD)
        return pipeline;
    }
    ```
  - `URP.ctor(this)`
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
    ```csharp (ScriptableRenderer.cs:471)
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

- `override void uRP.Render(ctx, cams)`
    ```csharp  (UniversalRenderPipeline.cs:425)
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        int cameraCount = cameras.Count;

        SetupScreenMSAASamplesState(cameraCount); //`canOptimizeScreenMSAASamples`: Codex:「URP のメイン描画が中間ターゲットで MSAA されているなら、最終バックバッファ側の MSAA は不要だから 1 にする」という最適化です。

        using var profScope = new ProfilingScope(ProfilingSampler.Get(URPProfileId.UniversalRenderTotal));

        using (new ContextRenderingScope(context, cameras)) //`public static event Action<ScriptableRenderContext, List<Camera>> ⟪begin¦end⟫ContextRendering`を実行する。
        {
            GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            GraphicsSettings.lightsUseColorTemperature = true;
            SetupPerFrameShaderConstants(); //主にLODCrossFadeの`ditheringTexture`などを設定。

            SortCameras(cameras);
            int lastBaseCameraIndex = GetLastBaseCameraIndex(cameras); //最後の`CameraRenderType.Base`である`camera`のindexを取得する

            for (int i = 0; i < cameraCount; ++i)
            {
                var camera = cameras[i];
                bool isLastBaseCamera = i == lastBaseCameraIndex;
                if (IsGameCamera(camera)) //`camera.cameraType == CameraType.⟪Game¦VR⟫`
                {
                    RenderCameraStack(context, camera, isLastBaseCamera);
                }
                else
                {
                    using (new CameraRenderingScope(context, camera))
                    {
                        VFX.VFXManager.PrepareCamera(camera);
                        UpdateVolumeFramework(camera, null); //非⟪Game¦VR⟫は毎フレーム更新(`VolumeManager.instance.Update(trigger, layerMask)`)する

                        RenderSingleCameraInternal(context, camera, isLastBaseCamera);
                    }
                }
            }

            s_RenderGraph.EndFrame();
            s_RTHandlePool.PurgeUnusedResources(Time.frameCount); //void RTHandleResourcePool.PurgeUnusedResources(int currentFrameIndex)
        }
    }
    ```
  - `static void uRP:RenderCameraStack(context, camera, isLastBaseCamera)`
    :大体、**カメラ毎**に、`uCD`を**作成**して`RenderSingleCamera(ctx, uCD)`を呼んでいるだけ (**uCD作成**(`CreateCameraData`,`InitializeAdditionalCameraData`) と `UpdateVolumeFramework`,`VFX.VFXManager.PrepareCamera`)
    ```csharp  (UniversalRenderPipeline.cs:907)
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

                //UpdateVolumeFramework(overlayCamera, data);が抜けている気がする
                stackedOverlayCamerasRequireDepthForPostProcessing |= VolumeManager.instance.stack.GetComponent<⟪DepthOfField¦MotionBlur⟫>().IsActive() ? true : false;
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
    - `static void uRP:RenderSingleCameraInternal(context, camera, isLastBaseCamera)`
        ```csharp (UniversalRenderPipeline.cs:686)
        internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera, ., bool isLastBaseCamera = true)
        {
            //`additionalCameraData == null`なので以下書き換え
            var frameData = GetRenderer(camera, null).frameData;
            var cameraData = CreateCameraData(frameData, camera, null);
            InitializeAdditionalCameraData(camera, null, true, isLastBaseCamera, cameraData);
            RenderSingleCamera(context, cameraData);
        }
        ```
  - `static void uRP:RenderSingleCamera(context, ＠⟪base¦overlay⟫CameraData)`
    ```csharp (UniversalRenderPipeline.cs:734)
    static void RenderSingleCamera(ScriptableRenderContext context, UniversalCameraData cameraData)
    {
        Camera camera = cameraData.camera;
        ScriptableRenderer renderer = cameraData.renderer;
        using ContextContainer frameData = renderer.frameData; // 注: この変数がスコープを抜けるときに frameData を破棄します。
        if (!TryGetCullingParameters(cameraData, out var cullingParameters)) return; //`return cameraData.camera.TryGetCullingParameters(., out cullingParameters);`

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, CameraMetadataCache.GetCached(camera).sampler)) // CommandBuffer cmd に "BeginSample" コマンドをエンキューします
        {
            using (new ProfilingScope(Profiling.Pipeline.Renderer.setupCullingParameters))
            {
                var legacyCameraData = new CameraData(frameData);
                renderer.OnPreCullRenderPasses(in legacyCameraData);
                renderer.SetupCullingParameters(ref cullingParameters, ref legacyCameraData);
            }

            context.ExecuteCommandBuffer(cmd); cmd.Clear(); //`cmd`記録開始========

            SetupPerCameraShaderConstants(cmd);
            //`ProbeReferenceVolume.instance.～(～);群` //APV更新
            RTHandles.SetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);

            //`frameData`を初期化する====
            UniversalLightData lightData; UniversalShadowData shadowData; CullContextData cullData;
            // cullResults のコピーを避けるため、UniversalRenderingData はここで作成する必要があります。
            var data = frameData.Create<UniversalRenderingData>();
            data.cullResults = context.Cull(ref cullingParameters);
            RenderingMode? renderingMode = (cameraData.renderer as UniversalRenderer)?.renderingModeActual;
            using (new ProfilingScope(Profiling.Pipeline.initializeRenderingData))
            {
                CreateUniversalResourceData(frameData);
                lightData = CreateLightData(frameData, asset, data.cullResults.visibleLights, renderingMode);
                shadowData = CreateShadowData(frameData, asset, renderingMode);
                CreatePostProcessingData(frameData, asset);
                CreateRenderingData(frameData, asset, cmd, renderingMode, cameraData.renderer);
                cullData = CreateCullContextData(frameData, context);
            }
            CreateShadowAtlasAndCullShadowCasters(lightData, shadowData, cameraData, ref data.cullResults, ref context);

            //`renderer.AddRenderPasses(.)`で`.EnqueuePass(.)`して、`RecordAndExecuteRenderGraph(..)`を実行する====
            RenderingData legacyRenderingData = new RenderingData(frameData);
            renderer.AddRenderPasses(ref legacyRenderingData);
            UniversalRenderPipeline.renderTextureUVOriginStrategy = RenderTextureUVOriginStrategy.BottomLeft;
            RecordAndExecuteRenderGraph(s_RenderGraph, context, renderer, cmd, cameraData.camera, UniversalRenderPipeline.renderTextureUVOriginStrategy);
            renderer.FinishRenderGraphRendering(cmd);
        }

        context.ExecuteCommandBuffer(cmd); CommandBufferPool.Release(cmd); //`cmd`記録終了========
        using (new ProfilingScope(Profiling.Pipeline.Context.submit)) context.Submit();
    }
    ```
    - `renderer.sR:AddRenderPasses(legacyRenderingData)`
        ```csharp (ScriptableRenderer.cs:1094)
        internal void AddRenderPasses(ref RenderingData renderingData)
        {
            using var profScope = new ProfilingScope(Profiling.addRenderPasses);
            // `sRF.AddRenderPasses(this❰renderer❱, renderingData)`から`renderer.EnqueuePass(sRPass)`で`sRPass`を`m_ARPQ`に追加する
            for (int i = 0; i < rendererFeatures.Count; ++i)
            {
                if (!rendererFeatures[i].isActive) continue;
                rendererFeatures[i].AddRenderPasses(this, ref renderingData);
            }
        }
        ```

- `CreateCameraData`
  :Overlay は「出力先や解像度は Base と共有し、描画カメラとしての個別設定だけ後で Overlay のものにする」という流れです。
```csharp
static UniversalCameraData CreateCameraData(ContextContainer frameData, Camera camera, UniversalAdditionalCameraData additionalCameraData)
{
    using var profScope = new ProfilingScope(Profiling.Pipeline.initializeCameraData);

    UniversalCameraData cameraData = frameData.Create<UniversalCameraData>();

    InitializeStackedCameraData(camera, additionalCameraData, cameraData);
    cameraData.camera = camera; //『baseCamera => overlayCamera (あとで上書きされる)
    cameraData.historyManager = additionalCameraData?.historyManager; // 履歴を生成できる挿入済みユーザーレンダーパスがアクセスできるよう、書き込み可能なカメラ履歴への参照を追加します。

    //`cameraTargetDescriptor`設定=============
    cameraData.scaled⟪Width¦Height⟫ = camera.pixel⟪Width¦Height⟫ * cameraData.renderScale;
    int msaaSamples = 1; if (camera.allowMSAA && asset.msaaSampleCount > 1) msaaSamples = (camera.targetTexture != null) ? camera.targetTexture.antiAliasing : asset.msaaSampleCount;
    cameraData.hdrColorBufferPrecision = asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits;
    cameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(camera, cameraData, cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, msaaSamples, Graphics.preserveFramebufferAlpha);
    cameraData.isAlphaOutputEnabled = GraphicsFormatUtility.HasAlphaChannel(cameraData.cameraTargetDescriptor.graphicsFormat);

    return cameraData;
}
```

```csharp
static void InitializeStackedCameraData(Camera baseCamera, UniversalAdditionalCameraData baseAdditionalCameraData, UniversalCameraData cameraData)
{
    using var profScope = new ProfilingScope(Profiling.Pipeline.initializeStackedCameraData);

    cameraData.targetTexture = baseCamera.targetTexture;
    cameraData.cameraType = baseCamera.cameraType;

    // 環境とポストプロセス設定////////////////////
    if (baseAdditionalCameraData != null)
    {
        cameraData.volumeLayerMask = baseAdditionalCameraData.volumeLayerMask;
        cameraData.volumeTrigger = baseAdditionalCameraData.volumeTrigger == null ? baseCamera.transform : baseAdditionalCameraData.volumeTrigger;
        cameraData.isStopNaNEnabled = baseAdditionalCameraData.stopNaN && SystemInfo.graphicsShaderLevel >= 35;
        cameraData.isDitheringEnabled = baseAdditionalCameraData.dithering;
        cameraData.antialiasing = baseAdditionalCameraData.antialiasing;
        cameraData.antialiasingQuality = baseAdditionalCameraData.antialiasingQuality;
        cameraData.xrRendering = baseAdditionalCameraData.allowXRRendering && XRSystem.displayActive;
        cameraData.allowHDROutput = baseAdditionalCameraData.allowHDROutput;
    }

    // カメラの出力を制御する設定////////////////////
    //HDR
    cameraData.isHdrEnabled = baseCamera.allowHDR && asset.supportsHDR;
    cameraData.allowHDROutput &= asset.supportsHDR;

    //解像度関係
    cameraData.pixel⟪Width¦Height⟫ = baseCamera.pixel⟪Width¦Height⟫;
    cameraData.aspectRatio = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
    cameraData.renderScale = cameraData.cameraType == CameraType.Game ? asset.renderScale : 1.0f;
    cameraData.isDefaultViewport = !(Math.Abs(baseCamera.rect.x) > 0.0f || Math.Abs(baseCamera.rect.y) > 0.0f || Math.Abs(baseCamera.rect.width) < 1.0f || Math.Abs(baseCamera.rect.height) < 1.0f);
    cameraData.pixelRect = baseCamera.pixelRect; //『シザー?

    //『デフォルト不透明`SortingCriteria`
    var commonOpaqueFlags = SortingCriteria.CommonOpaque;
    var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
    bool canSkipFrontToBackSorting = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && SystemInfo.hasHiddenSurfaceRemovalOnGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
    cameraData.defaultOpaqueSortFlags = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : commonOpaqueFlags;

    // パイプラインアセットのアップスケーリングフィルター選択を画像アップスケーリングフィルターに変換します 『(ImageUpscalingFilter <= UpscalingFilterSelection)
    cameraData.upscalingFilter = ResolveUpscalingFilterSelection(new Vector2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.renderScale, asset.upscalingFilter);
    cameraData.imageScalingMode = ｢ImageScalingMode.⟪None¦Upscaling¦Downscaling⟫:`cameraData.⟪renderScale¦cameraType¦upscalingFilter⟫`で決められる｣
    //『FSR
    cameraData.fsrOverrideSharpness = asset.fsrOverrideSharpness;
    cameraData.fsrSharpness = asset.fsrSharpness;
    //『xr
    cameraData.xr = XRSystem.emptyPass;
    //『キャプチャー
    cameraData.captureActions = Unity.RenderPipelines.Core.Runtime.Shared.CameraCaptureBridge.GetCachedCaptureActionsEnumerator(baseCamera);
}
```

```csharp
internal static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, UniversalCameraData cameraData, bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, int msaaSamples, bool needsAlpha)
{
    RenderTextureDescriptor desc;

    if (camera.targetTexture == null)
    {
        desc = new RenderTextureDescriptor(cameraData.scaledWidth, cameraData.scaledHeight); //『scaled

        static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, bool needsAlpha) //理解のためにインナー化した
        {
            if (isHdrEnabled)
            {
                if (!needsAlpha && requestHDRColorBufferPrecision != HDRColorBufferPrecision._64Bits && SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
                    return GraphicsFormat.B10G11R11_UFloatPack32;
                if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend)) //『指定された用途`GraphicsFormatUsage`に対して、指定された`GraphicsFormat`がサポートされているか検証します。
                    return GraphicsFormat.R16G16B16A16_SFloat; //『ほぼこれになると思われる
                return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
            }
            return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
        }
        //『基本的に、`.depthBufferBits`も`.sRGB`も`.⟪depthStencil¦graphics⟫Format`に整合する値にはなっていて、`new RenderTexture(desc)`でRTを作成しても、URP内部RTとの差異は基本的に無く、大丈夫なようにはなっている。
            //『Codex:`.depthBufferBits`,`.sRGB`は、旧APIや`RenderTextureDescriptor`互換のためにも埋めている補助情報です。(codex://threads/019e4af7-5842-77b1-b4b4-8f39e075e4d5)
        /*color*/   desc.graphicsFormat = MakeRenderTextureGraphicsFormat(isHdrEnabled, requestHDRColorBufferPrecision, needsAlpha);
        /*depth*/   desc.depthStencilFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
        /*補助情報*/ desc.depthBufferBits = (int)CoreUtils.GetDefaultDepthBufferBits(); desc.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
    }
    else
    {
        // 注:`camera.targetTexture`は最終的な出力先ですが、`cameraTargetDescriptor`は`targetTexture.descriptor`の生コピーではありません。 //後でコメント消去予定
        // これは URP がこのフレームで描画する領域や、必要に応じて作る中間テクスチャのための作業用`descriptor`です。
        //`targetTexture`がある場合は、色形式などの外部テクスチャ固有の設定を引き継ぐため、まず`targetTexture.descriptor`を土台にします。
        // その後、renderScale、カメラ viewport、アップスケーラー、MSAA など URP 側の現在の描画条件に合わせてサイズなどを上書きします。
            //URPは直接`camera.targetTexture`に描画しているわけではなく、`中間RT`を自動的に生成して、`中間RT`=Blitなど=>`camera.targetTexture`のようにレンダリングしているわけですね？
            //その`中間RT`(`cameraData.cameraTargetDescriptor`)を`camera.targetTexture.descriptor`を土台にして作っているわけですね？
            //`backBufferColor`<=`camera.targetTexture`, `cameraColor`<=`cameraData.cameraTargetDescriptor` だったらしい
        desc = camera.targetTexture.descriptor; //『`color`,`depth`などは`targetTexture`から継承
        desc.⟪width¦height⟫ = cameraData.scaled⟪Width¦Height⟫; //『scaled
    }

    desc.msaaSamples = msaaSamples;
    desc.enableRandomWrite = false;
    desc.bindMS = false;
    desc.useDynamicScale = camera.allowDynamicResolution;

    return desc;
}
```

```csharp
InitializeAdditionalCameraData
```
