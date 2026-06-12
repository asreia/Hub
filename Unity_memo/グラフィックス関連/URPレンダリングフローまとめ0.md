# URPレンダリングフローまとめ0

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
- [RenderPipelineGlobalSettings](images\RenderPipelineGlobalSettings.png)

## uRPA.CreatePipeline

- `override RenderPipeline uRPA.CreatePipeline()`
    ```csharp (UniversalRenderPipelineAsset.cs:790)
    protected override RenderPipeline CreatePipeline()
    {
        DestroyRenderers(); //=> for(..) m_Renderers[i].Dispose()
        var pipeline = new UniversalRenderPipeline(this);
        CreateRenderers(); //=> for(..) m_Renderers[i] = m_RendererDataList[i]❰SRD❱.InternalCreateRenderer() => uRD.Create() => UR.ctor(uRD):SR.ctor(sRD)
        return pipeline;
    }
    ```
  - `uRP.ctor(uRPA this)`
    ```csharp (UniversalRenderPipeline.cs:226)
    public UniversalRenderPipeline(UniversalRenderPipelineAsset asset)
    {
        pipelineAsset = asset;

        m_GlobalSettings = UniversalRenderPipelineGlobalSettings.instance; //実体は`Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`

        //`UniversalRenderPipelineGlobalSettings.instance`(`.GetRenderPipelineSettings`)から`IRenderPipelineResources:IRenderPipelineGraphicsSettings`(`<T>`)を取得し設定
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
  - `sR.ctor(sRD data)`
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
  - `uR.ctor(uRD data)`
    ```csharp (UniversalRenderer.cs:211)
        public UniversalRenderer(UniversalRendererData data) : base(data)
        {
            //`UniversalRenderPipelineGlobalSettings.instance`(`.TryGetRenderPipelineSettings`)から`IRenderPipelineResources:IRenderPipelineGraphicsSettings`(`<T>`)を取得し設定
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

            //＠❰m_❱フィールドメンバ = data<:uRD>.～
            m_DefaultStencilState<:StencilState> = data.defaultStencilState<:StencilStateData>
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

## uRP.Render

- `override void uRP.`**Render**`(context, cameras)`
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
  - `static void URP:`**RenderCameraStack**`(context, camera, isLastBaseCamera)`: 大体、**カメラ毎**に、`uCD`を**作成**して`RenderSingleCamera(context, uCD)`を呼んでいるだけ (あと`UpdateVolumeFramework`,`VFX.VFXManager.PrepareCamera`)
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
                baseCameraData.postProcessingRequiresDepthTexture<:bool> |= stackedOverlayCamerasRequireDepthForPostProcessing;

                baseCameraData.stackAnyPostProcessingEnabled<:bool> = stackAnyPostProcessingEnabled;
                baseCameraData.stackLastCameraOutputToHDR<:bool> = asset.supportsHDR && HDROutputForMainDisplayIsActive() && baseCamera.targetTexture == null && baseCamera.cameraType == CameraType.⟪Game¦VR⟫ && baseCameraData.allowHDROutput;

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
                    overlayCameraData.baseCamera<:Camera> = baseCamera;
                    overlayCameraData.camera<:Camera> = overlayCamera; //`CreateCameraData(..)`から上書き

                    using (new CameraRenderingScope(context, overlayCamera))
                    {
                        // マテリアルを準備するため、カリング前に呼び出す必要があります。VisualEffect コンポーネントが存在しない場合、このメソッドは何もしません。
                        VFX.VFXManager.PrepareCamera(overlayCamera, cameraXRSettings);

                        UpdateVolumeFramework(overlayCamera, overlayAdditionalCameraData);

                        bool isLastOverlayCamera = i == lastActiveOverlayCameraIndex;
                        InitializeAdditionalCameraData(overlayCamera, overlayAdditionalCameraData, isLastOverlayCamera, isLastBaseCamera, overlayCameraData);

                        overlayCameraData.stackAnyPostProcessingEnabled<:bool> = stackAnyPostProcessingEnabled;

                        RenderSingleCamera(context, overlayCameraData);
                    }
                }
            }
        }
        XRSystem.EndLayout();
    }
    ```
    - `static void URP:RenderSingleCameraInternal(context, camera, isLastBaseCamera)`
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
  - `static void URP:`**RenderSingleCamera**`(context, ＠⟪base¦overlay⟫CameraData)`
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
            UniversalLightData lightData; UniversalShadowData shadowData;
            // cullResults のコピーを避けるため、UniversalRenderingData はここで作成する必要があります。
            var data = frameData.Create<UniversalRenderingData>();
            data.cullResults = context.Cull(ref cullingParameters);
            using (new ProfilingScope(Profiling.Pipeline.initializeRenderingData))
            {
                CreateUniversalResourceData(frameData);
                lightData = CreateLightData(frameData, asset, data.cullResults.visibleLights);
                shadowData = CreateShadowData(frameData, asset);
                CreatePostProcessingData(frameData, asset);
                CreateRenderingData(frameData, asset, cameraData.renderer);
                CreateCullContextData(frameData, context);
            }
            CreateShadowAtlasAndCullShadowCasters(lightData, shadowData, cameraData, ref data.cullResults, ref context);

            //`renderer.AddRenderPasses(.)`で`.EnqueuePass(.)`して、`RecordAndExecuteRenderGraph(..)`を実行する====
            RenderingData legacyRenderingData = new RenderingData(frameData);
            renderer.AddRenderPasses(ref legacyRenderingData);
            UniversalRenderPipeline.renderTextureUVOriginStrategy = RenderTextureUVOriginStrategy.BottomLeft;
            RecordAndExecuteRenderGraph(s_RenderGraph, renderer, context, cmd);
            renderer.FinishRenderGraphRendering(cmd);
        }

        context.ExecuteCommandBuffer(cmd); CommandBufferPool.Release(cmd); //`cmd`記録終了========
        using (new ProfilingScope(Profiling.Pipeline.Context.submit)) context.Submit();
    }
    ```
    - `void renderer.sR:AddRenderPasses(legacyRenderingData)`
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
    - `void RecordAndExecuteRenderGraph(s_RenderGraph, renderer, context, cmd)`
        ```csharp (UniversalRenderPipelineRenderGraph.cs:6)
        static void RecordAndExecuteRenderGraph(RenderGraph renderGraph, ScriptableRenderer renderer, ScriptableRenderContext context, CommandBuffer cmd)
        {
            Camera camera = renderer.frameData.Get<UniversalCameraData>().camera;

            RenderGraphParameters rgParams = new RenderGraphParameters
            {
                executionId = camera.GetEntityId(),
                generateDebugData = camera.cameraType != CameraType.Preview && !camera.isProcessingRenderRequest,
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount,
                renderTextureUVOriginStrategy = UniversalRenderPipeline.renderTextureUVOriginStrategy, //『`.BottomLeft`
            };

            try
            {
                renderGraph.BeginRecording(rgParams);
                renderer.RecordRenderGraph(renderGraph, context);
                renderGraph.EndRecordingAndExecute();
            }
            catch (Exception e)
            {
                if (renderGraph.ResetGraphAndLogException(e))
                    throw;
            }
        }
        ```
      - `void renderer.RecordRenderGraph(renderGraph, context)`
        ```csharp (ScriptableRenderer.cs:901)
        internal void RecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context)
        {
            using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RecordRenderGraph)))
            {
                /*Begin*/OnBeginRenderGraphFrame(); //『`frameData.Get<UniversalResourceData>().InitFrame();` => `isAccessible = true;`

                using (new ProfilingScope(Profiling.sortRenderPasses))
                {
                    SortStable(m_ActiveRenderPassQueue); //『Codex: `RenderPassEvent`を昇順に並べる、安定な挿入ソート
                }

                /*Init*/InitRenderGraphFrame(renderGraph); //『`.AddUnsafePass`:`ShaderKeywrod`と`ShaderProperty(Time)`の初期化(⟦～⟧❰cmd.SetKeyword(ShaderGlobalKeywords.～, false)❱と⟦～⟧❰cmd.SetGlobalVector(ShaderPropertyId.～Time～, new Vector4(～)❱)

                using (new ProfilingScope(Profiling.recordRenderGraph))
                {
                    OnRecordRenderGraph(renderGraph, context);
                }

                /*End*/OnEndRenderGraphFrame(); //『`frameData.Get<UniversalResourceData>().EndFrame();` => `isAccessible = false;`
            }
        }
        ```
        - `void InitRenderGraphFrame(renderGraph)`
            ```csharp (ScriptableRenderer.cs:575)
            private void InitRenderGraphFrame(RenderGraph renderGraph)
            {
                using (var builder = renderGraph.AddUnsafePass<PassData>(Profiling.initRenderGraphFrame.name, out var passData, Profiling.initRenderGraphFrame))
                {
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc(static (PassData data, UnsafeGraphContext rgContext) =>
                    {
                        ClearRenderingState(rgContext.cmd);
                        static void ClearRenderingState(IBaseCommandBuffer cmd)
                        {
                            using var profScope = new ProfilingScope(Profiling.clearRenderingState);

                            // カメラごとのシェーダーキーワードをリセットします。これらは実行されるレンダーパスに応じて有効化されます。
                            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadows, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadowCascades, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.AdditionalLightsVertex, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.AdditionalLightsPixel, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ClusterLightLoop, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ForwardPlus, false); // 後方互換性のため。6.1 で非推奨。
                            cmd.SetKeyword(ShaderGlobalKeywords.AdditionalLightShadows, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeBlending, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeBoxProjection, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeAtlas, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadows, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsLow, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsMedium, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsHigh, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.MixedLightingSubtractive, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.LightmapShadowMixing, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ShadowsShadowMask, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.LinearToSRGBConversion, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.LightLayers, false);
                            cmd.SetKeyword(ShaderGlobalKeywords.ScreenSpaceOcclusion, false);
                            cmd.SetGlobalVector(ScreenSpaceAmbientOcclusionPass.s_AmbientOcclusionParamID, Vector4.zero);
                        }

                        SetShaderTimeValues(rgContext.cmd, Time.time, Time.deltaTime, Time.smoothDeltaTime);
                        static void SetShaderTimeValues(IBaseCommandBuffer cmd, float time, float deltaTime, float smoothDeltaTime)
                        {
                            float timeEights = time / 8f;
                            float timeFourth = time / 4f;
                            float timeHalf = time / 2f;
                            float lastTime = time - ShaderUtils.PersistentDeltaTime;

                            cmd.SetGlobalVector(ShaderPropertyId.time, time * new Vector4(1f / 20f, 1f, 2f, 3f));
                            cmd.SetGlobalVector(ShaderPropertyId.sinTime, new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time)));
                            cmd.SetGlobalVector(ShaderPropertyId.cosTime, new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time)));
                            cmd.SetGlobalVector(ShaderPropertyId.deltaTime, new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime));
                            cmd.SetGlobalVector(ShaderPropertyId.timeParameters, new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f));
                            cmd.SetGlobalVector(ShaderPropertyId.lastTimeParameters, new Vector4(lastTime, Mathf.Sin(lastTime), Mathf.Cos(lastTime), 0.0f));
                        }
                    });
                }
            }
            ```
        - `override void OnRecordRenderGraph(renderGraph, context)`: 別ファイル
    - `void renderer.FinishRenderGraphRendering(cmd)`
        ```csharp (ScriptableRenderer.cs:940)
        internal void FinishRenderGraphRendering(CommandBuffer cmd)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            {//『InternalFinishRenderingCommon(..)
                using (new ProfilingScope(Profiling.internalFinishRenderingCommon))
                {
                    for (int i = 0; i < m_ActiveRenderPassQueue.Count; ++i)
                        m_ActiveRenderPassQueue[i].OnCameraCleanup(cmd);
                    m_ActiveRenderPassQueue.Clear();
                    if (cameraData.resolveFinalTarget) m_IsPipelineExecuting = false;
                }
            }
        }
        ```

## frameData 作成

- `Universal`**Camera**`Data`作成 (呼び出し元:`RenderCameraStack`)
  - `static UCD CreateCameraData(＠❰overlay❱FrameData, baseCamera, baseCameraAdditionalData)`: `overlayCamera`時も`baseCamera`で設定する
    ```csharp (UniversalRenderPipeline.cs:1338)
    static UniversalCameraData CreateCameraData(ContextContainer frameData, Camera camera, UniversalAdditionalCameraData additionalCameraData)
    {
        using var profScope = new ProfilingScope(Profiling.Pipeline.initializeCameraData);

        UniversalCameraData cameraData = frameData.Create<UniversalCameraData>();

        InitializeStackedCameraData(camera, additionalCameraData, cameraData);
        cameraData.camera<:Camera> = camera; //『baseCamera => overlayCamera (あとで上書きされる)
        cameraData.historyManager<:UniversalCameraHistory> = additionalCameraData?.historyManager; // 履歴を生成できる挿入済みユーザーレンダーパスがアクセスできるよう、書き込み可能なカメラ履歴への参照を追加します。

        //`cameraTargetDescriptor`設定=============
        cameraData.scaled⟪Width¦Height⟫<:int> = camera.pixel⟪Width¦Height⟫ * cameraData.renderScale;
        int msaaSamples = 1; if (camera.allowMSAA && asset.msaaSampleCount > 1) msaaSamples = (camera.targetTexture != null) ? camera.targetTexture.antiAliasing : asset.msaaSampleCount;
        cameraData.hdrColorBufferPrecision<:HDRColorBufferPrecision> = asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits;
        cameraData.cameraTargetDescriptor<:RTDesc> = CreateRenderTextureDescriptor(camera, cameraData, cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, msaaSamples, Graphics.preserveFramebufferAlpha);
        cameraData.isAlphaOutputEnabled<:bool> = GraphicsFormatUtility.HasAlphaChannel(cameraData.cameraTargetDescriptor.graphicsFormat);

        return cameraData;
    }
    ```
    - `static void InitializeStackedCameraData(camera, additionalCameraData, 『out』cameraData)`
        ```csharp (UniversalRenderPipeline.cs:1344)
        static void InitializeStackedCameraData(Camera baseCamera, UniversalAdditionalCameraData baseAdditionalCameraData, UniversalCameraData cameraData)
        {
            using var profScope = new ProfilingScope(Profiling.Pipeline.initializeStackedCameraData);

            cameraData.targetTexture<:RenderTexture> = baseCamera.targetTexture;
            cameraData.cameraType<:CameraType> = baseCamera.cameraType;

            // 環境とポストプロセス設定////////////////////
            if (baseAdditionalCameraData != null)
            {
                cameraData.volumeLayerMask<:LayerMask> = baseAdditionalCameraData.volumeLayerMask;
                cameraData.volumeTrigger<:Transform> = baseAdditionalCameraData.volumeTrigger == null ? baseCamera.transform : baseAdditionalCameraData.volumeTrigger;
                cameraData.isStopNaNEnabled<:bool> = baseAdditionalCameraData.stopNaN && SystemInfo.graphicsShaderLevel >= 35;
                cameraData.isDitheringEnabled<:bool> = baseAdditionalCameraData.dithering;
                cameraData.antialiasing<:AntialiasingMode> = baseAdditionalCameraData.antialiasing;
                cameraData.antialiasingQuality<:AntialiasingQuality> = baseAdditionalCameraData.antialiasingQuality;
                cameraData.xrRendering<:bool> = baseAdditionalCameraData.allowXRRendering && XRSystem.displayActive;
                cameraData.allowHDROutput<:bool> = baseAdditionalCameraData.allowHDROutput;
            }

            // カメラの出力を制御する設定////////////////////
            //HDR
            cameraData.isHdrEnabled<:bool> = baseCamera.allowHDR && asset.supportsHDR;
            cameraData.allowHDROutput<:bool> &= asset.supportsHDR;

            //解像度関係
            cameraData.pixel⟪Width¦Height⟫<:int> = baseCamera.pixel⟪Width¦Height⟫;
            cameraData.aspectRatio<:float> = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
            cameraData.renderScale<:float> = cameraData.cameraType == CameraType.Game ? asset.renderScale : 1.0f;
            cameraData.isDefaultViewport<:bool> = !(Math.Abs(baseCamera.rect.x) > 0.0f || Math.Abs(baseCamera.rect.y) > 0.0f || Math.Abs(baseCamera.rect.width) < 1.0f || Math.Abs(baseCamera.rect.height) < 1.0f);
            cameraData.pixelRect<:Rect> = baseCamera.pixelRect; //『シザー?

            //『デフォルト不透明`SortingCriteria`
            var commonOpaqueFlags = SortingCriteria.CommonOpaque;
            var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
            bool canSkipFrontToBackSorting = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && SystemInfo.hasHiddenSurfaceRemovalOnGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
            cameraData.defaultOpaqueSortFlags<:SortingCriteria> = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : commonOpaqueFlags;

            // パイプラインアセットのアップスケーリングフィルター選択を画像アップスケーリングフィルターに変換します 『(ImageUpscalingFilter <= UpscalingFilterSelection)
            cameraData.upscalingFilter<:ImageUpscalingFilter> = ResolveUpscalingFilterSelection(new Vector2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.renderScale, asset.upscalingFilter);
            cameraData.imageScalingMode<:ImageScalingMode> = ｢ImageScalingMode.⟪None¦Upscaling¦Downscaling⟫:`cameraData.⟪renderScale¦cameraType¦upscalingFilter⟫`で決められる｣
            //『FSR
            cameraData.fsrOverrideSharpness<:bool> = asset.fsrOverrideSharpness;
            cameraData.fsrSharpness<:float> = asset.fsrSharpness;
            //『xr
            cameraData.xr<:XRPass> = XRSystem.emptyPass;
            //『キャプチャー
            cameraData.captureActions<:IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>>> = Unity.RenderPipelines.Core.Runtime.Shared.CameraCaptureBridge.GetCachedCaptureActionsEnumerator(baseCamera);
        }
        ```
    - `static RTDesc CreateRenderTextureDescriptor(camera, cameraData, cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, msaaSamples, Graphics.preserveFramebufferAlpha)`
        ```csharp (UniversalRenderPipelineCore.cs:1532)
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
  - `static void InitializeAdditionalCameraData(⟪base¦overlay⟫Camera, ⟪base¦overlay⟫AdditionalCameraData, isLastBaseCamera, ⟪!isStackedRendering¦isLastOverlayCamera⟫, 『out』⟪base¦overlay⟫CameraData)`
    ```csharp (UniversalRenderPipeline.cs:1548)
    static void InitializeAdditionalCameraData(Camera camera, UniversalAdditionalCameraData additionalCameraData, bool isLastBaseCamera, bool isLastOverlayCamera, UniversalCameraData cameraData)
    {
        using var profScope = new ProfilingScope(Profiling.Pipeline.initializeAdditionalCameraData);

        cameraData.isLastBaseCamera<:bool> = isLastBaseCamera;
        cameraData.resolveFinalTarget<:bool> = isLastOverlayCamera;
        cameraData.renderer<:ScriptableRenderer> = GetRenderer(camera, additionalCameraData);
        cameraData.useGPUOcclusionCulling<:bool> = GPUResidentDrawer.IsInstanceOcclusionCullingEnabled() && cameraData.renderer.supportsGPUOcclusion;

        if (additionalCameraData != null)
        {
            cameraData.renderType<:CameraRenderType> = additionalCameraData.renderType;
            cameraData.postProcessEnabled<:bool> = additionalCameraData.renderPostProcessing;

            cameraData.clearDepth<:bool> = additionalCameraData.renderType == CameraRenderType.Base ? true : additionalCameraData.clearDepth;
            cameraData.requiresOpaqueTexture<:bool> = cameraData.renderType == CameraRenderType.Overlay ? false : additionalCameraData.requiresColorTexture;
            cameraData.requiresDepthTexture<:bool> = additionalCameraData.requiresDepthTexture | cameraData.useGPUOcclusionCulling;
            cameraData.postProcessingRequiresDepthTexture<:bool> = cameraData.postProcessEnabled && VolumeManager.instance.stack.GetComponent<⟪DepthOfField¦MotionBlur⟫>().IsActive() ? true : false;

            cameraData.useScreenCoordOverride<:bool> = additionalCameraData.useScreenCoordOverride;
            cameraData.screenSizeOverride<:Vector4> = additionalCameraData.screenSizeOverride;
            cameraData.screenCoordScaleBias<:Vector4> = additionalCameraData.screenCoordScaleBias;
        }

        float dist = Mathf.Min(asset.shadowDistance, camera.farClipPlane);
        cameraData.maxShadowDistance<:float> = additionalCameraData.renderShadows && asset.supports⟪Main¦Additional⟫LightShadows && dist >= camera.nearClipPlane ? dist : 0.0f;

        UpdateTemporalAAData(cameraData, additionalCameraData); //『cameraData.taaHistory<:TaaHistory> = GetHistoryForWrite<TaaHistory>(); cameraData.stpHistory<:StpHistory> = GetHistoryForWrite<StpHistory>(); cameraData.taaSettings<:TemporalAA.Settings> = additionalCameraData.taaSettings;

        Matrix4x4 projectionMatrix = camera.projectionMatrix;
        // Overlay カメラは Base から viewport を継承します。両者の viewport が異なる場合、Overlay カメラでオブジェクトをレンダリングしたときにつぶれるのを防ぐため、アスペクト比行列を調整するように projection へパッチを当てる必要がある場合があります。
        if (cameraData.renderType == CameraRenderType.Overlay && !camera.orthographic && cameraData.pixelRect != camera.pixelRect)
            projectionMatrix.m00 *= camera.aspect / cameraData.aspectRatio; //m00. = (cotangent / .aspect=>.aspectRatio)
        Matrix4x4 jitterMatrix = TemporalAA.CalculateJitterMatrix(cameraData, cameraData.IsSTPEnabled() ? StpUtils.s_JitterFunc : TemporalAA.s_JitterFunc);
        cameraData.SetViewProjectionAndJitterMatrix(viewMatrix:camera.worldToCameraMatrix, projectionMatrix, jitterMatrix);
            //『cameraData.m_ViewMatrix<:Matrix4x4> = viewMatrix;
            //『cameraData.m_ProjectionMatrix<:Matrix4x4> = projectionMatrix;
            //『cameraData.m_JitterMatrix<:Matrix4x4> = jitterMatrix;
        cameraData.worldSpaceCameraPos<:Vector3> = camera.transform.position;

        cameraData.backgroundColor<:Color> = CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor); //『return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color.linear : color;
        cameraData.isAlphaOutputEnabled<:bool> &= !cameraData.postProcessEnabled || (cameraData.postProcessEnabled && asset.allowPostProcessAlphaOutput);
    }
    ```
- `Universal`**Resource**`Data`作成 (呼び出し元:`RenderSingleCamera`)
  - `static URD CreateUniversalResourceData(frameData)`
    ```csharp  (UniversalRenderPipeline.cs:1904)
    static UniversalResourceData CreateUniversalResourceData(ContextContainer frameData)
    {
        return frameData.Create<UniversalResourceData>();
    }
    ```
- `Universal`**Light**`Data`作成 (呼び出し元:`RenderSingleCamera`)
  - `static ULD CreateLightData(frameData, asset, data.cullResults.visibleLights)`
    ```csharp (UniversalRenderPipeline.cs:2019)
    static UniversalLightData CreateLightData(ContextContainer frameData, UniversalRenderPipelineAsset asset, NativeArray<VisibleLight> visibleLights)
    {
        using var profScope = new ProfilingScope(Profiling.Pipeline.initializeLightData);

        UniversalLightData lightData = frameData.Create<UniversalLightData>();
        lightData.visibleLights<:NativeArray<VisibleLight>> = visibleLights;
        lightData.mainLightIndex<:int> = GetMainLightIndex(visibleLights);
        lightData.additionalLightsCount<:int> = Math.Min((lightData.mainLightIndex != -1) ? visibleLights.Length - 1 : visibleLights.Length, maxVisibleAdditionalLights/*『==256*/);
        lightData.maxPerObjectAdditionalLightsCount<:int> = Math.Min(asset.maxAdditionalLightsCount, maxPerObjectLights/*『==8*/);
        lightData.supportsAdditionalLights<:bool> = asset.additionalLightsRenderingMode != LightRenderingMode.Disabled; //『=>true
        lightData.shadeAdditionalLightsPerVertex<:bool> = asset.additionalLightsRenderingMode == LightRenderingMode.PerVertex; //『=>false
        lightData.supportsMixedLighting<:bool> = asset.supportsMixedLighting; //『=>true
        lightData.supportsLightLayers<:bool> = asset.useRenderingLayers;
        lightData.reflectionProbeBoxProjection<:bool> = asset.reflectionProbeBoxProjection;
        lightData.reflectionProbeBlending<:bool> = asset.reflectionProbeBlending;
        lightData.reflectionProbeAtlas<:bool> = (asset.reflectionProbeBlending && asset.reflectionProbeAtlas) || asset.gpuResidentDrawerMode != GPUResidentDrawerMode.Disabled;

        return lightData;
    }
    ```
    - `static int GetMainLightIndex(visibleLights)`
        ```csharp (UniversalRenderPipeline.cs:2220)
        static int GetMainLightIndex(NativeArray<VisibleLight> visibleLights)
        {
            using var profScope = new ProfilingScope(Profiling.Pipeline.getMainLightIndex);

            //『static int GetBrightestDirectionalLightIndex(NativeArray<VisibleLight> visibleLights)を展開
            int brightestDirectionalLightIndex = -1;
            float brightestLightIntensity = 0.0f;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                VisibleLight currVisibleLight = visibleLights[i];

                if (currVisibleLight.lightType == LightType.Directional)
                {
                    if (currVisibleLight.light == RenderSettings.sun) //『`RenderSettings`？
                        return i;

                    // Sun light が存在しない場合は、最も明るい directional light を返します
                    if (currVisibleLight.light.intensity > brightestLightIntensity)
                    {
                        brightestLightIntensity = currVisibleLight.light.intensity;
                        brightestDirectionalLightIndex = i;
                    }
                }
            }
            return brightestDirectionalLightIndex;
        }
        ```
- `Universal`**Shadow**`Data`作成 (呼び出し元:`RenderSingleCamera`)
  - `static USD CreateShadowData(frameData, asset)`
    ```csharp (UniversalRenderPipeline.cs:1798)
    static UniversalShadowData CreateShadowData(ContextContainer frameData, UniversalRenderPipelineAsset asset)
    {
        using var profScope = new ProfilingScope(Profiling.Pipeline.initializeShadowData);

        // 初期セットアップ
        // ------------------------------------------------------
        UniversalShadowData shadowData = frameData.Create<UniversalShadowData>();

        m_ShadowBiasData.Clear(); m_ShadowResolutionData.Clear();

        shadowData.shadowmapDepthBufferBits<:int> = 16;
        shadowData.mainLightShadowmap⟪Width¦Height⟫<:int> = asset.mainLightShadowmapResolution;
        shadowData.additionalLightsShadowmap⟪Width¦Height⟫<:int> = asset.additionalLightsShadowmapResolution;

        shadowData.mainLightShadowCascadeBorder<:float> = asset.cascadeBorder;
        shadowData.mainLightShadowCascadesCount<:int> = asset.shadowCascadeCount;
        shadowData.mainLightShadowCascadesSplit<:Vector3> = GetMainLightCascadeSplit(shadowData.mainLightShadowCascadesCount, asset);


        // これは AdditionalLightsShadowCasterPass でセットアップされます。
        shadowData.isKeywordAdditionalLightShadowsEnabled<:bool> = false;
        shadowData.isKeywordSoftShadowsEnabled<:bool> = false;

        //『↓,↓↓は後で`CreateShadowAtlasAndCullShadowCasters`で設定される
        // これらのフィールドは RenderingData に対して ApplyAdaptivePerformance が呼び出された後にセットアップする必要があります。
        // この関数が現在 mainLightShadowmapWidth、mainLightShadowmapHeight、mainLightShadowCascadesCount を変更できるためです。
        // これらのフィールドの計算には 3 つのパラメーターすべてが必要なため、初期化は InitializeMainLightShadowResolution まで遅延されます。
        shadowData.mainLightShadowResolution<:int> = 0;
        shadowData.mainLightRenderTarget⟪Width¦Height⟫<:int> = 0;
        // これら 2 つのフィールドは ShadowData を使用して初期化する必要がありますが、ShadowData はこの関数 (InitializeRenderingData) の直後に ApplyAdaptivePerformance によって変更される可能性があります。
        // そのため、これらの初期化は ShadowData が完全に初期化された後の時点まで遅延されます。
        shadowData.shadowAtlasLayout<:AdditionalLightsShadowAtlasLayout> = default;
        shadowData.visibleLightsShadowCullingInfos<:NativeArray<URPLightShadowCullingInfos>> = default;

        // ライトの反復が必要なデータをセットアップします
        // ------------------------------------------------------

        // カメラの Render Shadows トグルが無効な場合、maxShadowDistance は 0.0f に設定されます
        bool cameraRenderShadows = frameData.Get<UniversalCameraData>().maxShadowDistance > 0.0f;

        shadowData.mainLightShadowsEnabled<:bool> = asset.supportsMainLightShadows && asset.mainLightRenderingMode == LightRenderingMode.PerPixel;
        shadowData.supportsMainLightShadows<:bool> = SystemInfo.supportsShadows && shadowData.mainLightShadowsEnabled && cameraRenderShadows;

        shadowData.additionalLightShadowsEnabled<:bool> = asset.supportsAdditionalLightShadows && (asset.additionalLightsRenderingMode == LightRenderingMode.PerPixel);
        shadowData.supportsAdditionalLightShadows<:bool> = SystemInfo.supportsShadows && shadowData.additionalLightShadowsEnabled && !lightData.shadeAdditionalLightsPerVertex && cameraRenderShadows;

        // シャドウがレンダリングされない場合は早期終了します...
        if (!shadowData.supportsMainLightShadows && !shadowData.supportsAdditionalLightShadows)
            return shadowData;

        UniversalLightData lightData = frameData.Get<UniversalLightData>();

        shadowData.supportsMainLightShadows<:bool> &= lightData.visibleLights[lightData.mainLightIndex].light.shadows != LightShadows.None;

        if (shadowData.supportsAdditionalLightShadows)
        {
            // シャドウを投影する追加ライトが少なくとも 1 つあるかを確認します...
            bool additionalLightsCastShadows = false;
            for (int i = 0; i < lightData.visibleLights.Length; ++i)
            {
                if (i == lightData.mainLightIndex)
                    continue;

                if (lightData.visibleLights[i].lightType == LightType.⟪Spot¦Point⟫ && lightData.visibleLights[i].light.shadows != LightShadows.None)
                {
                    additionalLightsCastShadows = true;
                    break;
                }
            }
            shadowData.supportsAdditionalLightShadows<:bool> &= additionalLightsCastShadows;
        }

        // 早期終了できるかを再度確認します... //『`.light.shadows != LightShadows.None`の条件を追加して再度チェック (enum LightShadows{None, Hard, Soft})
        if (!shadowData.supportsMainLightShadows && !shadowData.supportsAdditionalLightShadows)
            return shadowData;

        for (int i = 0; i < lightData.visibleLights.Length; ++i)
        {
            if ((!shadowData.supportsMainLightShadows && i == mainLightIndex) || (!shadowData.supportsAdditionalLightShadows && i != mainLightIndex))
            {
                m_ShadowBiasData.Add(Vector4.zero);
                m_ShadowResolutionData.Add(0);
                continue;
            }


            //＄UALD＝❰UniversalAdditionalLightData❱, ＄ALSRTier＝❰AdditionalLightsShadowResolutionTier❱
            lightData.visibleLights[i].light.gameObject.TryGetComponent(out UALD uALD);

            if (!uALD.usePipelineSettings)
                m_ShadowBiasData.Add(new Vector4(lightData.visibleLights[i].light.shadowBias, lightData.visibleLights[i].light.shadowNormalBias, 0.0f, 0.0f));
            else
                m_ShadowBiasData.Add(new Vector4(asset.shadowDepthBias, asset.shadowNormalBias, 0.0f, 0.0f));

            if ((uALD.ALSRTier == UALD.ALSRTier❰Custom❱))
                m_ShadowResolutionData.Add((int)lightData.visibleLights[i].light.shadowResolution);
            else
                m_ShadowResolutionData.Add(asset.GetAdditionalLightsShadowResolution(Mathf.Clamp(uALD.ALSRTier, UALD.ALSRTier❰Low❱, UALD.ALSRTier❰High❱)));
        }

        shadowData.bias<:List<Vector4>> = m_ShadowBiasData;
        shadowData.resolution<:List<int>> = m_ShadowResolutionData;
        shadowData.supportsSoftShadows<:bool> = asset.supportsSoftShadows && (shadowData.supportsMainLightShadows || shadowData.supportsAdditionalLightShadows);

        return shadowData;
    }
    ```
  - `static void CreateShadowAtlasAndCullShadowCasters(lightData, shadowData, cameraData, ref data.cullResults, ref context)`
    ```csharp (UniversalRenderPipeline.cs:885)
    private static void CreateShadowAtlasAndCullShadowCasters(UniversalLightData lightData, UniversalShadowData shadowData, UniversalCameraData cameraData, ref CullingResults cullResults, ref ScriptableRenderContext context)
    {
        //codex://threads/019e790b-3ab8-74c3-8517-0ab231e541c9
        // この関数は、実際にシャドウマップへ描画する場所ではありません。
        // 後続の MainLightShadowCasterPass / AdditionalLightsShadowCasterPass が
        // 「どのライトの影を、どの大きさで、どのアトラス領域へ、どのライト視点から描くか」
        // を判断できるように、 shadowData に影描画のための実行計画を詰める場所です。
        //
        // ここに来る前の CreateShadowData では、URP Asset、カメラ設定、ライト設定などから
        // supportsMainLightShadows / supportsAdditionalLightShadows などの基本的な可否だけが決まっています。
        // しかし、その時点ではまだ次のような「描画に必要な具体情報」は確定していません。
        //
        // - メインライトのカスケード 1 枚あたりの解像度
        // - メインライト用シャドウ RenderTarget の実際の幅と高さ
        // - 追加ライト用シャドウアトラスの中に、各 Spot / Point Light の影をどう詰めるか
        // - ライトごとの shadow view / projection matrix
        // - 各シャドウスライスで使う ShadowSplitData と、スライスが有効かどうか
        // - 通常のカメラカリングとは別に、ライト視点で影を落とす Renderer(恐らくコンポーネント) を絞り込むための情報
        //
        // つまり、この関数を通った後の shadowData は単なる「影設定」ではなく、
        // 後続の ShadowCasterPass がそのまま読める「このフレームの影の実行計画書」になります。

        // メインライト影も追加ライト影も使わないなら、 shadowData に追加で詰める影描画情報はありません。
        // ここで返る場合、後続の ShadowCasterPass は影なし、または空のシャドウ設定として扱います。
        if (!shadowData.supportsMainLightShadows && !shadowData.supportsAdditionalLightShadows)
            return;

        // メインライト、通常は Directional Light のリアルタイム影が有効な場合の準備です。
        //
        // CreateShadowData の段階では、URP Asset に設定された
        // mainLightShadowmapWidth / mainLightShadowmapHeight / mainLightShadowCascadesCount は入っています。
        // ただし、カスケードシャドウでは 1 枚のシャドウマップを複数の区画に分けるため、
        // 「1 カスケードを何 px の正方形として描けるか」はここで決め直す必要があります。
        //
        // 例:
        // - 2048x2048 のメインライトシャドウマップ
        // - カスケード数 4
        // なら、内部的には 1024x1024 の区画を 2x2 に並べるような考え方になります。
        //
        // InitializeMainLightShadowResolution は主に次を shadowData に設定します。
        // - mainLightShadowResolution  : 1 カスケード、つまり 1 スライスあたりの解像度
        // - mainLightRenderTargetWidth : メインライト用シャドウ RenderTarget の幅
        // - mainLightRenderTargetHeight: メインライト用シャドウ RenderTarget の高さ
        //
        // これらは後で MainLightShadowCasterPass が
        // 「各カスケードをどの viewport に描くか」を決めるために使います。
        if (shadowData.supportsMainLightShadows)
            InitializeMainLightShadowResolution(shadowData);
                //『shadowData.mainLightShadowResolution<:int> = ShadowUtils.GetMaxTileResolutionInAtlas(shadowData.mainLightShadowmapWidth, shadowData.mainLightShadowmapHeight, shadowData.mainLightShadowCascadesCount);
                //『shadowData.mainLightRenderTargetWidth<:int> = shadowData.mainLightShadowmapWidth;
                //『shadowData.mainLightRenderTargetHeight<:int> = (shadowData.mainLightShadowCascadesCount == 2) ? shadowData.mainLightShadowmapHeight >> 1 : shadowData.mainLightShadowmapHeight;

        // 追加ライト、つまり Spot Light / Point Light のリアルタイム影が有効な場合の準備です。
        //
        // 追加ライトの影は、ライトごとに個別の RenderTexture を作るのではなく、
        // 1 枚の additional lights shadow atlas の中に複数の影スライスを詰めます。
        //
        // Spot Light は 1 方向だけを見ればよいので、基本的に 1 スライスです。
        // Point Light は全方向へ影を落とすため、キューブマップ相当の 6 スライスが必要です。
        //
        // AdditionalLightsShadowAtlasLayout は、可視ライト一覧、各ライトの要求解像度、
        // ソフトシャドウかどうか、Point か Spot か、カメラからの距離、アトラスサイズなどを見て、
        // 次のような「配置計画」を shadowData.shadowAtlasLayout に作ります。
        //
        // - どの追加ライトが今回シャドウを描けるか
        // - 各ライトの各スライスをアトラスのどの x/y(Rect?) 位置へ置くか
        // - 要求解像度をそのまま使うか、アトラスに収めるため縮小するか
        // - 入りきらないライトや、縮小しすぎて見た目が破綻しそうなスライスを外すか
        //
        // ここで作っているのはあくまで「アトラス内の置き場所の計画」です。
        // 実際の描画は、この後の AdditionalLightsShadowCasterPass がこの layout を読んで行います。
        if (shadowData.supportsAdditionalLightShadows)
            shadowData.shadowAtlasLayout<:AdditionalLightsShadowAtlasLayout> = new AdditionalLightsShadowAtlasLayout(lightData, shadowData, cameraData);

        // ここが最終的な影用カリング情報の作成です。
        //
        // すでに通常のカメラカリングは context.Cull(ref cullingParameters) で終わっていて、
        // cullResults.visibleLights には「このカメラから見て関係するライト」が入っています。
        // しかし、カメラから見える Renderer(コンポーネント?) と、ライトから見て影を落とす Renderer は同じではありません。
        //
        // 例:
        // - カメラには映っていない建物でも、ライト方向から見ると画面内の地面へ影を落とすかもしれない
        // - カメラから見えるライトでも、シャドウアトラスに入らなければ今回は影を描けない
        // - Directional Light はカスケードごとに別の範囲を持つ
        // - Point Light は 6 方向ぶんのライト視点が必要になる
        //
        // そのため、 ShadowCulling.CullShadowCasters ではライトごと、スライスごとに
        // ShadowSliceData を作り、さらに ScriptableRenderContext.CullShadowCasters を呼んで
        // ライト視点の影キャスターカリングを実行します。
        //
        // 戻り値の visibleLightsShadowCullingInfos は visibleLights と同じ index で参照できる配列です。
        // 各要素には、そのライトが持つシャドウスライス群が入ります。
        //
        // Directional Light:
        // - カスケード数ぶんの slices
        // - 各 slice に viewMatrix / projectionMatrix / shadowTransform / splitData
        //
        // Spot Light:
        // - 1 つの slice
        //
        // Point Light:
        // - CubemapFace ごとの 6 つの slices
        //
        // 後続の MainLightShadowCasterPass / AdditionalLightsShadowCasterPass は、
        // shadowData.visibleLightsShadowCullingInfos からこれらの slice 情報を読み、
        // 実際にどの viewport へ、どの行列で、どの RendererList を使って影を描くかを決めます。
        shadowData.visibleLightsShadowCullingInfos<:NativeArray<URPLightShadowCullingInfos>> = ShadowCulling.CullShadowCasters(ref context, shadowData, ref shadowData.shadowAtlasLayout, ref cullResults);
    }
    ```
- `Universal`**PostProcessing**`Data`作成 (呼び出し元:`RenderSingleCamera`)
  - `static UPD CreatePostProcessingData(frameData, asset)`
    ```csharp (UniversalRenderPipeline.cs:1980)
    static UniversalPostProcessingData CreatePostProcessingData(ContextContainer frameData, UniversalRenderPipelineAsset asset)
    {
        UniversalPostProcessingData postProcessingData = frameData.Create<UniversalPostProcessingData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        postProcessingData.isEnabled<:bool> = cameraData.stackAnyPostProcessingEnabled;

        postProcessingData.lutSize<:int> = asset.colorGradingLutSize;
        postProcessingData.gradingMode<:ColorGradingMode> =
            cameraData.stackLastCameraOutputToHDR ? ColorGradingMode.HighDynamicRange :
            asset.supportsHDR ? asset.colorGradingMode :
            ColorGradingMode.LowDynamicRange;
        postProcessingData.useFastSRGBLinearConversion<:bool> = asset.useFastSRGBLinearConversion;

        postProcessingData.supportScreenSpaceLensFlare<:bool> = asset.supportScreenSpaceLensFlare;
        postProcessingData.supportDataDrivenLensFlare<:bool> = asset.supportDataDrivenLensFlare;

        return postProcessingData;
    }
    ```
- `Universal`**Rendering**`Data`作成 (呼び出し元:`RenderSingleCamera`)
  - `static URenD CreateRenderingData(frameData, asset, cmd, cameraData.renderer)`
    ```csharp (UniversalRenderPipeline.cs:1776)
    static UniversalRenderingData CreateRenderingData(ContextContainer frameData, UniversalRenderPipelineAsset asset, ScriptableRenderer renderer)
    {
        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

        renderingData.perObjectData<:PerObjectData> = GetPerObjectLightFlags(asset);
        renderingData.stencilLodCrossFadeEnabled<:bool> = asset.enableLODCrossFade && asset.lodCrossFadeDitheringType == LODCrossFadeDitheringType.Stencil;
        renderingData.supportsDynamicBatching<:bool> = asset.supportsDynamicBatching;

        UniversalRenderer universalRenderer = renderer as UniversalRenderer;
        renderingData.renderingMode<:RenderingMode> = universalRenderer.renderingModeActual;
        renderingData.prepassLayerMask<:LayerMask> = universalRenderer.prepassLayerMask;
        renderingData.opaqueLayerMask<:LayerMask> = universalRenderer.opaqueLayerMask;
        renderingData.transparentLayerMask<:LayerMask> = universalRenderer.transparentLayerMask;

        return renderingData;
    }
    ```
    - `static PerObjectData GetPerObjectLightFlags(asset)`
        ```csharp (UniversalRenderPipeline.cs:2152)
        static PerObjectData GetPerObjectLightFlags(UniversalRenderPipelineAsset asset)
        {
            using var profScope = new ProfilingScope(Profiling.Pipeline.getPerObjectLightFlags);

            PerObjectData configuration = PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask;
            if (!/*asset.ShouldUseReflectionProbeBlending*/(asset.gpuResidentDrawerMode != GPUResidentDrawerMode.Disabled ? true : asset.reflectionProbeBlending))
                configuration |= PerObjectData.ReflectionProbes;

            return configuration;
        }
        ```
- **CullContext**`Data`作成 (呼び出し元:`RenderSingleCamera`)
  - `static CullContextData CreateCullContextData(frameData, context)`
    ```csharp (UniversalRenderPipeline.cs:1955)
    static CullContextData CreateCullContextData(ContextContainer frameData, ScriptableRenderContext context)
    {
        var cullData = frameData.Create<CullContextData>();
        cullData.SetRenderContext(context); //『cullData.m_RenderContext<:ScriptableRenderContext?> = context;
        return cullData;
    }
    ```
