# URPレンダリングフローまとめ1

## uR.OnRecordRenderGraph

- `override void OnRecordRenderGraph(renderGraph, context)`
    ```csharp (UniversalRendererRenderGraph.cs)
    internal override void OnRecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        useRenderPassEnabled = renderGraph.nativeRenderPassesEnabled;
        MotionVectorRenderPass.SetRenderGraphMotionVectorGlobalMatrices(renderGraph, cameraData); //『保留

        SetupRenderGraphLights(renderGraph, frameData.Get<UniversalRenderingData>(), cameraData, frameData.Get<UniversalLightData>());

        RequireResults requireResults = CreateCameraRenderTargets(renderGraph, cameraData, frameData.Get<UniversalPostProcessingData>().isEnabled);

        RecordCustomRenderGraphPasses(renderGraph, RenderPassEvent.BeforeRendering);

        TextureHandle target = resourceData.activeColorTexture.IsValid() ? resourceData.activeColorTexture : resourceData.activeDepthTexture;
        SetupRenderGraphCameraProperties(renderGraph, target);

    #if VISUAL_EFFECT_GRAPH_0_0_1_OR_NEWER
        ProcessVFXCameraCommand(renderGraph);
    #endif

        if (requireResults.isCameraTargetOffscreenDepth){OnOffscreenDepthTextureRendering(renderGraph, context, resourceData, cameraData); return;}
        OnBeforeRendering(renderGraph);
        OnMainRendering(renderGraph, context, requireResults.renderPassInputs, requireResults.requirePrepass, requireResults.requireDepthTexture);
        OnAfterRendering(renderGraph, requireResults.applyPostProcessing);
    }
    ```

  - `RequireResults` **CreateCameraRenderTargets**`(renderGraph, cameraData, frameData.Get<UniversalPostProcessingData>().isEnabled)`: 自作の外部関数化コード
    ```csharp (UniversalRendererRenderGraph.cs)
    RequireResults CreateCameraRenderTargets(RenderGraph renderGraph, UniversalCameraData cameraData, bool postProcessingEnabled)
    {
        SetupRenderingLayers(cameraData.cameraTargetDescriptor.msaaSamples);
        bool isCameraTargetOffscreenDepth = IsOffscreenDepthTexture(cameraData.camera.targetTexture);

        RenderPassInputSummary renderPassInputs = GetRenderPassInputs(cameraData.IsTemporalAAEnabled(), postProcessingEnabled, m_RenderingLayerProvidesByDepthNormalPass, activeRenderPassQueue, m_MotionVectorPass);

        bool applyPostProcessing = cameraData.postProcessEnabled && m_PostProcess != null;
        bool requireDepthTexture = RequireDepthTexture(cameraData, in renderPassInputs, applyPostProcessing);
        bool requirePrepassForTextures = RequirePrepassForTextures(cameraData, renderPassInputs);

        /*cameraData.renderer.*/useDepthPriming = IsDepthPrimingEnabledRenderGraph(cameraData, m_DepthPrimingMode);

        bool requirePrepass = requirePrepassForTextures || useDepthPriming;

        // cameraDepthTexture に直接プリパスを行う場合にのみ深度フォーマットを使用します。深度プライミング (つまり activeCameraDepth へのプリパス) を行う場合は、テクスチャへのプリパスは行いません。代わりに、プライミング済みアタッチメントからコピーします。
        bool prepassToCameraDepthTexture = requirePrepassForTextures && !useDepthPriming;
        bool requireCopyFromDepth = requireDepthTexture && !prepassToCameraDepthTexture;

        // これはスタック内の最初のカメラで設定します。オーバーレイカメラはカラー/深度の作成変数を再利用して正しいターゲットを選びます。
        // 中間テクスチャがある場合、オーバーレイカメラはそれらを使用する必要があります。
        if (cameraData.renderType == CameraRenderType.Base)
            s_RequiresIntermediateAttachments = RequiresIntermediateAttachments(cameraData, in renderPassInputs, requireCopyFromDepth, applyPostProcessing);

        CreateRenderGraphCameraRenderTargets(renderGraph, isCameraTargetOffscreenDepth, s_RequiresIntermediateAttachments, prepassToCameraDepthTexture);

        return new RequireResults(renderPassInputs, applyPostProcessing, requireDepthTexture, requirePrepass, isCameraTargetOffscreenDepth);
    }
    ```
    - `RequireResults`
        ```csharp (UniversalRendererRenderGraph.cs)
        private readonly struct RequireResults
        {
            internal readonly RenderPassInputSummary renderPassInputs;
            internal readonly bool applyPostProcessing;
            internal readonly bool requireDepthTexture;
            internal readonly bool requirePrepass;
            internal readonly bool isCameraTargetOffscreenDepth;

            internal RequireResults(RenderPassInputSummary renderPassInputs, bool applyPostProcessing, bool requireDepthTexture, bool requirePrepass, bool isCameraTargetOffscreenDepth)
            {
                this.renderPassInputs = renderPassInputs;
                this.applyPostProcessing = applyPostProcessing;
                this.requireDepthTexture = requireDepthTexture;
                this.requirePrepass = requirePrepass;
                this.isCameraTargetOffscreenDepth = isCameraTargetOffscreenDepth;
            }
        }
        ```
    - `void SetupRenderingLayers(cameraData.cameraTargetDescriptor.msaaSamples)`
        ```csharp
        void SetupRenderingLayers(int msaaSamples)
        {
            // レンダリングレイヤーが必要なレンダーパスイベントとマスクサイズを収集します。
            if(m_RequiresRenderingLayer = RenderingLayerUtils.RequireRenderingLayers(this, rendererFeatures, msaaSamples, out m_RenderingLayersEvent, out m_RenderingLayersMaskSize))
            {
                m_RenderingLayerProvidesRenderObjectPass = m_RenderingLayersEvent == RenderingLayerUtils.Event.Opaque;
                m_RenderingLayerProvidesByDepthNormalPass = m_RenderingLayersEvent == RenderingLayerUtils.Event.DepthNormalPrePass;
            }
            else
            {
                m_RenderingLayerProvidesRenderObjectPass = m_RenderingLayerProvidesByDepthNormalPass = false;
            }
        }
        ```
    - `static bool IsOffscreenDepthTexture(cameraData.camera.targetTexture)`
        ```csharp
        static bool IsOffscreenDepthTexture(RenderTexture rt) => rt != null && rt.format == RenderTextureFormat.Depth; //『rt.graphicsFormat == .None && rt.descriptor.shadowSamplingMode == .None
        ```
    - `static RenderPassInputSummary` **GetRenderPassInputs**`(cameraData.IsTemporalAAEnabled(), postProcessingEnabled, m_RenderingLayerProvidesByDepthNormalPass, activeRenderPassQueue, m_MotionVectorPass)`
        ```csharp
        static RenderPassInputSummary GetRenderPassInputs(bool isTemporalAAEnabled, bool postProcessingEnabled, bool renderingLayerProvidesByDepthNormalPass, List<ScriptableRenderPass> activeRenderPassQueue, MotionVectorRenderPass motionVectorPass)
        {
            RenderPassInputSummary inputSummary = new RenderPassInputSummary
            {
                requiresDepthTextureEarliestEvent = RenderPassEvent.BeforeRenderingPostProcessing //『`DepthTexture`が使われる場所?
                requiresDepthNormalAtEvent = RenderPassEvent.BeforeRenderingOpaques,              //『`DepthNormal`が描画される場所?
            };
            for (int i = 0; i < activeRenderPassQueue.Count; ++i)
            {
                ScriptableRenderPass pass = activeRenderPassQueue[i];
                bool needsNormals = (pass.input & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None;
                bool needsDepth = (pass.input & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None;
                bool needsColor = (pass.input & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None;
                bool needsMotion = (pass.input & ScriptableRenderPassInput.Motion) != ScriptableRenderPassInput.None;

                inputSummary.requiresNormalsTexture |= needsNormals || renderingLayerProvidesByDepthNormalPass;
                inputSummary.requiresDepthTexture |= needsDepth;
                    inputSummary.requiresDepthPrepass |= inputSummary.requiresNormalsTexture || (needsDepth && (pass.renderPassEvent < RenderPassEvent.AfterRenderingOpaques));
                inputSummary.requiresColorTexture |= needsColor;
                inputSummary.requiresMotionVectors |= needsMotion;
                if (needsDepth)                 inputSummary.requiresDepthTextureEarliestEvent = (RenderPassEvent)Mathf.Min((int)pass.renderPassEvent, (int)inputSummary.requiresDepthTextureEarliestEvent);
                if (needsNormals || needsDepth) inputSummary.requiresDepthNormalAtEvent = (RenderPassEvent)Mathf.Min((int)pass.renderPassEvent, (int)inputSummary.requiresDepthNormalAtEvent);
            }

            //『`requiresMotionVectors`と`requiresDepthTexture`追加設定(`= true`)==========================
            // `isTemporalAAEnabled`には、`requiresMotionVectors`が必要です。
            if (isTemporalAAEnabled) inputSummary.requiresMotionVectors = true;
            // `MotionBlurMode.CameraAndObjects`には`requiresMotionVectors`が必要です。
            if (postProcessingEnabled)
            {
                var motionBlur = VolumeManager.instance.stack.GetComponent<MotionBlur>();
                if(motionBlur != null && motionBlur.IsActive() && motionBlur.mode.value == MotionBlurMode.CameraAndObjects) //『`.CameraOnly`の場合はUVとデプスからWSposを復元してそれにVP - prevVPしてるのかな
                    inputSummary.requiresMotionVectors = true;
            }
            // `requiresMotionVectors`は`requiresDepthTexture`を必要とします
            if (inputSummary.requiresMotionVectors)
            {
                inputSummary.requiresDepthTexture = true;
                inputSummary.requiresDepthTextureEarliestEvent = (RenderPassEvent)Mathf.Min((int)motionVectorPass.renderPassEvent, (int)inputSummary.requiresDepthTextureEarliestEvent);
            }

            return inputSummary;
        }
        ```
      - `static bool RequireDepthTexture(cameraData, in renderPassInputs, applyPostProcessing)`: `⟪uRD¦ARPQ⟫`または`uRD.postProcessingRequiresDepthTexture`によって`_CameraDepthTexture`が必要かのフラグ
        ```csharp
        static bool RequireDepthTexture(UniversalCameraData cameraData, in RenderPassInputSummary renderPassInputs, bool applyPostProcessing)
        {
            bool requiresDepthTexture = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture;
            bool cameraHasPostProcessingWithDepth = applyPostProcessing && cameraData.postProcessingRequiresDepthTexture;

            return requiresDepthTexture || cameraHasPostProcessingWithDepth;
        }
        ```
      - `bool RequirePrepassForTextures(cameraData, renderPassInputs)`: `Depth/Normals/RenderingLayers`テクスチャを生成するために`⟪Depth¦DepthNormal⟫Prepass`が必要かのフラグ
        ```csharp
        bool RequirePrepassForTextures(UniversalCameraData cameraData, in RenderPassInputSummary renderPassInputs)
        {
            //`CanCopyDepth`が想定環境で`true`だからcullした`requireDepthTexture && !CanCopyDepth(cameraData);` //『この`CanCopyDepth`はBlit(`CopyDepthPass`の`Blitter.BlitTexture`)ができるかの判定。(できないならば`Prepass`で直接描画する)
            bool requirePrepassForTextures = cameraData.requiresDepthTexture && m_CopyDepthMode == CopyDepthMode.ForcePrepass; //『`uR.m_CopyDepthMode`は`[SerializeField] CopyDepthMode uRD.m_CopyDepthMode = .AfterTransparents;`から取得
            requirePrepassForTextures |= renderPassInputs.requiresDepthPrepass; //『`inputSummary.requiresDepthPrepass |= inputSummary.requiresNormalsTexture`されている
            return requirePrepassForTextures;
        }
        ```
    - `static bool` **IsDepthPrimingEnabledRenderGraph**`(cameraData, m_DepthPrimingMode)`: 早期に`activeDepthTexture`を描画し、早期深度テストを実行して、描画時に`ZTest Equal`にすることでオーバードローを防ぐ。かのフラグ
        ```csharp
        static bool IsDepthPrimingEnabledRenderGraph(UniversalCameraData cameraData, DepthPrimingMode depthPrimingMode)
        {
            bool isNotMSAA = cameraData.cameraTargetDescriptor.msaaSamples == 1;
            return depthPrimingMode == DepthPrimingMode.⟪Auto¦Forced⟫ && cameraData.clearDepth && isNotMSAA && !IsOffscreenDepthTexture(cameraData.targetTexture/*(baseCamera)*/);
        }
        ```
    - `bool` **RequiresIntermediateAttachments**`(cameraData, in renderPassInputs, requireCopyFromDepth, applyPostProcessing)`: `中間RT`が必要かのフラグ (大体必要になる)
        ```csharp
        bool RequiresIntermediateAttachments(UniversalCameraData cameraData, in RenderPassInputSummary renderPassInputs, bool requireCopyFromDepth, bool applyPostProcessing)
        {
            var requireColorTexture = rendererFeatures.Any(feature => feature.isActive) && m_IntermediateTextureMode == IntermediateTextureMode.Always;
            requireColorTexture |= activeRenderPassQueue.Any(pass => pass.requiresIntermediateTexture);
            requireColorTexture |= RequiresIntermediateColorTexture(cameraData, in renderPassInputs, applyPostProcessing);

            return requireColorTexture || requireCopyFromDepth;
        }
        ```
      - `static bool RequiresIntermediateColorTexture(cameraData, in renderPassInputs, applyPostProcessing)`
        ```csharp
        static bool RequiresIntermediateColorTexture(UniversalCameraData cameraData, in RenderPassInputSummary renderPassInputs, bool applyPostProcessing)
        {
            if (!cameraData.resolveFinalTarget) //『この関数は`cameraData.renderType == .Base`で実行される
                return true; //『カメラスタッキングは`中間RT`を作る

            bool requiresOpaqueTexture = cameraData.requiresOpaqueTexture || renderPassInputs.requiresColorTexture;
            bool requiresBlit = applyPostProcessing || requiresOpaqueTexture || cameraData.cameraTargetDescriptor.msaaSamples > 1 || !cameraData.isDefaultViewport;
            if (cameraData.targetTexture != null/*isOffscreenRender*/)
                return requiresBlit;

            bool isScalable = (cameraData.imageScalingMode != ImageScalingMode.None) || IsScalableBufferManagerUsed(cameraData)/*『大体`cameraData.camera.allowDynamicResolution`有無*/;
            bool isNotCompatibleBackbufferTextureDimension = cameraData.cameraTargetDescriptor.dimension != TextureDimension.Tex2D;
            return requiresBlit || isScalable || isNotCompatibleBackbufferTextureDimension || cameraData.requireSrgbConversion/*『通常`false`*/ || cameraData.isHdrEnabled || cameraData.captureActions != null;
        }
        ```
    - `void` **CreateRenderGraphCameraRenderTargets**`(renderGraph, isCameraTargetOffscreenDepth, s_RequiresIntermediateAttachments, prepassToCameraDepthTexture)`: ココまでの結果を素に`uRD`の`TextureHandle`を作る
        ```csharp (UniversalRendererRenderGraph.cs)
        void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, bool isCameraTargetOffscreenDepth, bool requireIntermediateAttachments, bool prepassToCameraDepthTexture)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // レンダーパスのヒストリーリクエストを収集し、ヒストリーテクスチャを更新します。
            UpdateCameraHistory(cameraData);

            var clearCameraParams = GetClearCameraParams(cameraData); //『`struct ClearCameraParams{bool mustClear⟪Color¦Depth⟫, Color clearValue}`を設定

            //『 バックバッファ準備: `TextureHandle resourceData.backBuffer⟪Color¦Depth⟫ <<= RTHandle m_Target⟪Color¦Depth⟫Handle <<= ⟪｡cameraData.targetTexture｡¦｡BRTT.⟪CameraTarget¦Depth⟫｡⟫`という風にインポートしていく。
            SetupTargetHandles(cameraData); //『`RTHandle m_Target⟪Color¦Depth⟫Handle <<= ⟪｡cameraData.targetTexture｡¦｡BRTT.⟪CameraTarget¦Depth⟫｡⟫`
            ImportBackBuffers(renderGraph, cameraData, clearCameraParams.clearValue, isCameraTargetOffscreenDepth);//『`TextureHandle resourceData.backBuffer⟪Color¦Depth⟫ <<= RTHandle m_Target⟪Color¦Depth⟫Handle`

            //『 cameraDescriptorからテクスチャ作成================================================================================================================================================
            GetTextureDesc(in cameraData.cameraTargetDescriptor, out TextureDesc cameraDescriptor); //『`RTDesc`から`TextureDesc`を作成
            //『中間TRまたはバックバッファに直接描画
            if (requireIntermediateAttachments) //『 中間RT
            {
                if (!isCameraTargetOffscreenDepth)
                {
                    cameraDescriptor.format = cameraData.cameraTargetDescriptor.graphicsFormat;
                    CreateIntermediateCameraColorAttachment(renderGraph, cameraData, in cameraDescriptor, clearCameraParams.mustClearColor, clearCameraParams.clearValue);
                }
                CreateIntermediateCameraDepthAttachment(renderGraph, cameraData, in cameraDescriptor, clearCameraParams.mustClearDepth, clearCameraParams.clearValue);
            }
            else
            {
                frameData.Get<UniversalResourceData>().SwitchActiveTexturesToBackbuffer(); //『active⟪Color¦Depth⟫ID = UniversalResourceData.ActiveID.BackBuffer;
            }
            //『`Create～Texture(..)` (大体`format`を変えてるだけ)
            CreateCameraDepthCopyTexture(renderGraph, cameraDescriptor, prepassToCameraDepthTexture, clearCameraParams.clearValue); //『`.cameraDepthTexture`へ`cameraDescriptor`を素に`Create～()`(⟪レンダリング先(デプス)¦コピー先(R32)⟫)
            CreateCameraNormalsTexture(renderGraph, cameraDescriptor); //『`.cameraNormalsTexture`へ`cameraDescriptor`を素に`Create～()`
            CreateMotionVectorTextures(renderGraph, cameraDescriptor); //『`.motionVector⟪Color¦Depth⟫`へ`cameraDescriptor`を素に`Create～()`
            CreateRenderingLayersTexture(renderGraph, cameraDescriptor); //『`.renderingLayersTexture`へ`cameraDescriptor`を素に`Create～()`(`m_RequiresRenderingLayer`で要求されているとき)
            if (cameraData.isHDROutputActive && cameraData.rendersOverlayUI) CreateOffscreenUITexture(renderGraph, cameraDescriptor);
        }
        ```
      - **ユーティリティー**
        - `static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, in TextureDesc desc,..)`
            ```csharp (UniversalRendererRenderGraph.cs)
            internal static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, in TextureDesc desc, string name, bool clear, Color clearColor,
                FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp, bool discardOnLastUse = false)
            {
                TextureDesc outDesc = desc;
                outDesc.name = name;
                outDesc.clearBuffer = clear;
                outDesc.clearColor = clearColor;
                outDesc.filterMode = filterMode;
                outDesc.wrapMode = wrapMode;
                outDesc.discardBuffer = discardOnLastUse;
                return renderGraph.CreateTexture(outDesc);
            }
            ```
        - **DepthFormat**(UniversalRenderer.cs)
            ```csharp (UniversalRendererData.cs)
            public enum DepthFormat
            {
                /// <summary>
                /// AndroidおよびSwitchでは既定形式が<see cref="GraphicsFormat.D24_UNorm_S8_UInt"/>、その他のプラットフォームでは<see cref="GraphicsFormat.D32_SFloat_S8_UInt"/>です
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.All)]
                Default,

                /// <summary>
                /// 深度成分に16ビットの符号なし正規化値を含む形式です。<see cref="GraphicsFormat.D16_UNorm"/>に対応します。
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.Forward | RenderPathCompatibility.ForwardPlus)]
                Depth_16 = GraphicsFormat.D16_UNorm,

                /// <summary>
                /// 深度成分に24ビットの符号なし正規化値を含む形式です。<see cref="GraphicsFormat.D24_UNorm"/>に対応します。
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.Forward | RenderPathCompatibility.ForwardPlus)]
                Depth_24 = GraphicsFormat.D24_UNorm,

                /// <summary>
                /// 深度成分に32ビットの符号付き浮動小数点値を含む形式です。<see cref="GraphicsFormat.D32_SFloat"/>に対応します。
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.Forward | RenderPathCompatibility.ForwardPlus)]
                Depth_32 = GraphicsFormat.D32_SFloat,

                /// <summary>
                /// 深度成分に16ビットの符号なし正規化値、ステンシル成分に8ビットの符号なし整数値を含む形式です。<see cref="GraphicsFormat.D16_UNorm_S8_UInt"/>に対応します。
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.All)]
                Depth_16_Stencil_8 = GraphicsFormat.D16_UNorm_S8_UInt,

                /// <summary>
                /// 深度成分に24ビットの符号なし正規化値、ステンシル成分に8ビットの符号なし整数値を含む形式です。<see cref="GraphicsFormat.D24_UNorm_S8_UInt"/>に対応します。
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.All)]
                Depth_24_Stencil_8 = GraphicsFormat.D24_UNorm_S8_UInt, //『●

                /// <summary>
                /// 深度成分に32ビットの符号付き浮動小数点値、ステンシル成分に8ビットの符号なし整数値を含む形式です。<see cref="GraphicsFormat.D32_SFloat_S8_UInt"/>に対応します。
                /// </summary>
                [RenderPathCompatible(RenderPathCompatibility.All)]
                Depth_32_Stencil_8 = GraphicsFormat.D32_SFloat_S8_UInt, //『●
            }
            ```
          - `GraphicsFormat cameraDepthAttachmentFormat {get => (｢uRD.m_DepthAttachmentFormat｣ != DepthFormat.Default) ? (GraphicsFormat)｢uRD.m_DepthAttachmentFormat｣ : GraphicsFormat.D32_SFloat_S8_UInt;}`: 中間RT
          - `GraphicsFormat cameraDepthTextureFormat    {get => (｢uRD.m_DepthTextureFormat｣   !=  DepthFormat.Default) ? (GraphicsFormat)｢uRD.m_DepthTextureFormat｣   :  GraphicsFormat.D32_SFloat_S8_UInt;}`: 直接レンダリング用(コピーは`R32_SFloat`)
        - `＠⟪k¦m⟫_～Name`は大体`string "_～"`となる
      - `GetClearCameraParams(cameraData)`: `struct ClearCameraParams{bool mustClear⟪Color¦Depth⟫, Color clearValue}`を設定
        ```csharp (UniversalRendererRenderGraph.cs)
        ClearCameraParams GetClearCameraParams(UniversalCameraData cameraData)
        {
            bool clearColor = cameraData.renderType == CameraRenderType.Base;
            bool clearDepth = cameraData.renderType == CameraRenderType.Base || cameraData.clearDepth;
            // カメラ背景タイプが「未初期化」の場合、ユーザーが基礎となる挙動を明確に理解できるよう黄色でクリアします。唯一の例外は、外部テクスチャへレンダリングしている場合です。
            Color clearVal = (cameraData.camera.clearFlags == CameraClearFlags.Nothing && cameraData.targetTexture == null) ? Color.yellow : cameraData.backgroundColor;
            return new ClearCameraParams(clearColor, clearDepth, clearVal); //『`struct ClearCameraParams{bool mustClear⟪Color¦Depth⟫, Color clearValue}`
        }
        ```
      - **バックバッファ準備**: `TextureHandle resourceData.backBuffer⟪Color¦Depth⟫ <<= RTHandle m_Target⟪Color¦Depth⟫Handle <<= ⟪｡cameraData.targetTexture｡¦｡BRTT.⟪CameraTarget¦Depth⟫｡⟫`
        - `SetupTargetHandles(cameraData)`:                                                                         `RTHandle m_Target⟪Color¦Depth⟫Handle <<= ⟪｡cameraData.targetTexture｡¦｡BRTT.⟪CameraTarget¦Depth⟫｡⟫`
            ```csharp (UniversalRendererRenderGraph.cs)
            void SetupTargetHandles(UniversalCameraData cameraData)
            {
                RenderTargetIdentifier targetColorId = cameraData.targetTexture != null ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
                if (m_TargetColorHandle == null) m_TargetColorHandle = RTHandles.Alloc(targetColorId, "Backbuffer color");
                else if (m_TargetColorHandle.nameID != targetColorId) m_TargetColorHandle.SetTexture(targetColorId);

                RenderTargetIdentifier targetDepthId = cameraData.targetTexture != null ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.Depth;
                if (m_TargetDepthHandle == null) m_TargetDepthHandle = RTHandles.Alloc(targetDepthId, "Backbuffer depth");
                else if (m_TargetDepthHandle.nameID != targetDepthId) m_TargetDepthHandle.SetTexture(targetDepthId);
            }
            ```
        - `ImportBackBuffers(renderGraph, cameraData, clearCameraParams.clearValue, isCameraTargetOffscreenDepth)`: `TextureHandle resourceData.backBuffer⟪Color¦Depth⟫ <<= RTHandle m_Target⟪Color¦Depth⟫Handle`
            ```csharp (UniversalRendererRenderGraph.cs)
            void ImportBackBuffers(RenderGraph renderGraph, UniversalCameraData cameraData, Color clearBackgroundColor, bool isCameraTargetOffscreenDepth)
            {
                bool clearBackbufferOnFirstUse = (cameraData.renderType == CameraRenderType.Base) && !s_RequiresIntermediateAttachments; //『`中間RT`が全画面より小さい`Viewport`の可能性があるためクリアできない
                clearBackbufferOnFirstUse |= isCameraTargetOffscreenDepth; // オフスクリーン深度テクスチャへレンダリングしている場合はクリアを強制します。
                bool noStoreOnlyResolveBBColor = !s_RequiresIntermediateAttachments && (cameraData.cameraTargetDescriptor.msaaSamples > 1); //『>MSAA の生データを最後まで store しなくても、 resolve 済み結果だけ残せばいい。
                TextureUVOrigin backbufferTextureUVOrigin = cameraData.targetTexture == null ? TextureUVOrigin.TopLeft : TextureUVOrigin.BottomLeft;//『`.TopLeft`は反転するという意思表示?
                ImportResourceParams importBackbufferColorParams = new ImportResourceParams
                {
                    clearOnFirstUse = clearBackbufferOnFirstUse,
                    clearColor = clearBackgroundColor,
                    discardOnLastUse = noStoreOnlyResolveBBColor,
                    textureUVOrigin = backbufferTextureUVOrigin
                };
                ImportResourceParams importBackbufferDepthParams = new ImportResourceParams
                {
                    clearOnFirstUse = clearBackbufferOnFirstUse,
                    clearColor = clearBackgroundColor,
                    discardOnLastUse = !isCameraTargetOffscreenDepth,
                    textureUVOrigin = backbufferTextureUVOrigin
                };

                RenderTargetInfo importInfo = new RenderTargetInfo();
                RenderTargetInfo importInfoDepth;
                if (cameraData.targetTexture == null /*isBuiltInTexture*/)
                {
                    int numSamples = AdjustAndGetScreenMSAASamples(renderGraph, s_RequiresIntermediateAttachments);
                        int AdjustAndGetScreenMSAASamples(RenderGraph renderGraph, bool s_RequiresIntermediateAttachments)
                        {
                            if (s_RequiresIntermediateAttachments) Screen.SetMSAASamples(1); //『`中間RT`があるならば、バックバッファのMSAAは不要
                            return Screen.msaaSamples;
                        }
                    //『主に`Screen.～`から取得
                    importInfo.width = Screen.width;    //『cameraData.pixel⟪Width¦Height⟫ではない
                    importInfo.height = Screen.height;
                    importInfo.msaaSamples = numSamples;
                    importInfo.volumeDepth = 1;
                    importInfo.format = cameraData.cameraTargetDescriptor.graphicsFormat;

                    importInfoDepth = importInfo;
                    importInfoDepth.format = cameraData.cameraTargetDescriptor.depthStencilFormat;
                }
                else
                {
                    //『`cameraData.targetTexture.～`から取得
                    importInfo.width = cameraData.targetTexture.width;
                    importInfo.height = cameraData.targetTexture.height;
                    importInfo.msaaSamples = cameraData.targetTexture.antiAliasing;
                    importInfo.volumeDepth = cameraData.targetTexture.volumeDepth;
                    importInfo.format = cameraData.targetTexture.graphicsFormat;

                    importInfoDepth = importInfo;
                    importInfoDepth.format = cameraData.targetTexture.depthStencilFormat;
                }

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                if (!isCameraTargetOffscreenDepth)  resourceData.backBufferColor = renderGraph.ImportTexture(m_TargetColorHandle, importInfo,      importBackbufferColorParams);
                                                    resourceData.backBufferDepth = renderGraph.ImportTexture(m_TargetDepthHandle, importInfoDepth, importBackbufferDepthParams);
            }
            ```
      - **cameraDescriptorからテクスチャ作成**
        - `GetTextureDesc(in cameraData.cameraTargetDescriptor, out TextureDesc cameraDescriptor)`: `RTDesc`から`TextureDesc`を作成
            ```csharp (UniversalRendererRenderGraph.cs)
            static void GetTextureDesc(in RenderTextureDescriptor desc, out TextureDesc cameraDescriptor)
            {
                cameraDescriptor = new TextureDesc(desc.width, desc.height)
                {
                    dimension = desc.dimension,
                    format = (desc.depthStencilFormat != GraphicsFormat.None) ? desc.depthStencilFormat : desc.graphicsFormat, //『`CreateRenderGraphCameraRenderTargets(..)`では全て上書きされる
                    msaaSamples = (MSAASamples)desc.msaaSamples, bindTextureMS = desc.bindMS,
                    slices = desc.volumeDepth,
                    isShadowMap = desc.shadowSamplingMode != ShadowSamplingMode.None && desc.depthStencilFormat != GraphicsFormat.None,
                    enableRandomWrite = desc.enableRandomWrite,
                    useDynamicScale = desc.useDynamicScale, useDynamicScaleExplicit = desc.useDynamicScaleExplicit,
                    enableShadingRate = desc.enableShadingRate,
                    vrUsage = desc.vrUsage
                };
            }
            cameraDescriptor.useMipMap = false;
            cameraDescriptor.autoGenerateMips = false;
            cameraDescriptor.mipMapBias = 0;
            cameraDescriptor.anisoLevel = 1;
            ```
        - **requireIntermediateAttachments**: 中間RT
          - `CreateIntermediateCameraColorAttachment(renderGraph, cameraData, in cameraDescriptor, clearCameraParams.mustClearColor, clearCameraParams.clearValue)`: `resourceData.cameraColor`へ`cameraDescriptor`から`Create～()`または`Import～(⟪A¦B⟫)`
            ```csharp (UniversalRendererRenderGraph.cs)
            void CreateIntermediateCameraColorAttachment(RenderGraph renderGraph, UniversalCameraData cameraData, in TextureDesc cameraDescriptor, bool clearColor, Color clearBackgroundColor)
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                var desc = cameraDescriptor;
                desc.filterMode = FilterMode.Bilinear;
                desc.wrapMode = TextureWrapMode.Clamp;

                if (cameraData.resolveFinalTarget && cameraData.renderType == CameraRenderType.Base/*isSingleCamera*/)
                {
                    resourceData.cameraColor = CreateRenderGraphTexture(renderGraph, in desc, _SingleCameraTargetAttachmentName, clearColor, clearBackgroundColor, desc.filterMode, desc.wrapMode, cameraData.resolveFinalTarget);
                    s_CurrentColorHandle = -1;
                }
                else //カメラスタッキング
                {
                    //『`static RTHandle[] s_RenderGraphCameraColorHandles = new RTHandle[]{null, null};`
                    RenderingUtils.ReAllocateHandleIfNeeded(ref s_RenderGraphCameraColorHandles[0], desc, _CameraTargetAttachmentAName); //『`desc`は`baseCamera`から来てるのでスタックレンダリング中は再Allocateされないことを確信している？
                    RenderingUtils.ReAllocateHandleIfNeeded(ref s_RenderGraphCameraColorHandles[1], desc, _CameraTargetAttachmentBName);
                    ImportResourceParams importColorParams = new ImportResourceParams
                    {
                        clearOnFirstUse = clearColor,
                        clearColor = clearBackgroundColor,
                        discardOnLastUse = cameraData.resolveFinalTarget // スタック内の最後のカメラ
                    };
                    if (cameraData.renderType == CameraRenderType.Base) s_CurrentColorHandle = 0; // 決定論的なフレーム結果のため、ベースカメラが常に ColorAttachmentA へのレンダリングから開始するようにします。
                    resourceData.cameraColor = renderGraph.ImportTexture(s_RenderGraphCameraColorHandles[s_CurrentColorHandle]/*currentRenderGraphCameraColorHandle*/, importColorParams);
                }
                resourceData.activeColorID = UniversalResourceData.ActiveID.Camera;
            }
            ```
          - `CreateIntermediateCameraDepthAttachment(renderGraph, cameraData, in cameraDescriptor, clearCameraParams.mustClearDepth, clearCameraParams.clearValue)`: `resourceData.cameraDepth`へ`cameraDescriptor`から`Create～()`または`Import～()`
            ```csharp (UniversalRendererRenderGraph.cs)
            void CreateIntermediateCameraDepthAttachment(RenderGraph renderGraph, UniversalCameraData cameraData, in TextureDesc cameraDescriptor, bool clearDepth, Color clearBackgroundDepth)
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                var desc = cameraDescriptor;
                desc.bindTextureMS = false;
                desc.format = cameraDepthAttachmentFormat;
                desc.filterMode = FilterMode.Point;
                desc.wrapMode = TextureWrapMode.Clamp;

                if (cameraData.resolveFinalTarget && cameraData.renderType == CameraRenderType.Base/*isSingleCamera*/)
                {
                    resourceData.cameraDepth = CreateRenderGraphTexture(renderGraph, desc, _CameraDepthAttachmentName, clearDepth, clearBackgroundDepth, desc.filterMode, desc.wrapMode, cameraData.resolveFinalTarget);
                }
                else
                {
                    RenderingUtils.ReAllocateHandleIfNeeded(ref s_RenderGraphCameraDepthHandle, desc, _CameraDepthAttachmentName); //『`desc`は`baseCamera`から来てるのでスタックレンダリング中は再Allocateされないことを確信している？
                    ImportResourceParams importDepthParams = new ImportResourceParams
                    {
                        clearOnFirstUse = clearDepth,
                        clearColor = clearBackgroundDepth,
                        discardOnLastUse = cameraData.resolveFinalTarget
                    };

                    resourceData.cameraDepth = renderGraph.ImportTexture(s_RenderGraphCameraDepthHandle, importDepthParams);
                }
                resourceData.activeDepthID = UniversalResourceData.ActiveID.Camera;

                // 割り当てられた深度テクスチャに基づいて深度コピーパスを設定します。
                m_CopyDepthPass.CopyToDepth = false; //『`= prepassToCameraDepthTexture;`となっていたが、`true`の場合は`cameraDepthTextureFormat`に描く場合は直接レンダリングするので`m_CopyDepthPass`自体が使われない
                m_CopyDepthPass.MsaaSamples = (int) desc.msaaSamples;
                m_CopyDepthPass.m_CopyResolvedDepth = !desc.bindTextureMS;
            }
            ```
        - **Create～Texture(..)** (大体`format`を変えてるだけ)
          - `CreateCameraDepthCopyTexture(renderGraph, cameraDescriptor, prepassToCameraDepthTexture, clearCameraParams.clearValue)`: `.cameraDepthTexture`へ`cameraDescriptor`から`Create～()`(⟪レンダリング先(デプス)¦コピー先(R32)⟫)
            ```csharp (UniversalRendererRenderGraph.cs)
            void CreateCameraDepthCopyTexture(RenderGraph renderGraph, TextureDesc desc, bool prepassToCameraDepthTexture, Color clearColor)
            {
                desc.msaaSamples = MSAASamples.None;
                if (prepassToCameraDepthTexture)
                {
                    desc.format = cameraDepthTextureFormat;
                    desc.clearBuffer = true; // レンダリング先になります。
                }
                else
                {
                    desc.format = GraphicsFormat.R32_SFloat;
                    desc.clearBuffer = false; // コピー先になります。(全画面だから`false`だと思われる)
                }

                frameData.Get<UniversalResourceData>().cameraDepthTexture = CreateRenderGraphTexture(renderGraph, desc, "_CameraDepthTexture", desc.clearBuffer, clearColor);
            }
            ```
          - `CreateMotionVectorTextures(renderGraph, cameraDescriptor)`: `.cameraNormalsTexture`へ`cameraDescriptor`から`Create～()`
            ```csharp (UniversalRendererRenderGraph.cs)
            void CreateCameraNormalsTexture(RenderGraph renderGraph, TextureDesc desc)
            {
                desc.msaaSamples = MSAASamples.None;
                desc.format = DepthNormalOnlyPass.GetGraphicsFormat(); //『`GraphicsFormat.R8G8B8A8_SNorm`
                frameData.Get<UniversalResourceData>().cameraNormalsTexture = CreateRenderGraphTexture(renderGraph, desc, DepthNormalOnlyPass.k_CameraNormalsTextureName, true, Color.black);
            }
            ```
          - `CreateMotionVectorTextures(renderGraph, cameraDescriptor)`: `.motionVector⟪Color¦Depth⟫`へ`cameraDescriptor`から`Create～()`
            ```csharp (UniversalRendererRenderGraph.cs)
            void CreateMotionVectorTextures(RenderGraph renderGraph, TextureDesc desc)
            {
                desc.msaaSamples = MSAASamples.None;
                desc.format = MotionVectorRenderPass.k_TargetFormat; //『`GraphicsFormat.R16G16_SFloat`
                frameData.Get<UniversalResourceData>().motionVectorColor = CreateRenderGraphTexture(renderGraph, desc, MotionVectorRenderPass.k_MotionVectorTextureName, true, Color.black);
                desc.format = cameraDepthAttachmentFormat;
                frameData.Get<UniversalResourceData>().motionVectorDepth = CreateRenderGraphTexture(renderGraph, desc, MotionVectorRenderPass.k_MotionVectorDepthTextureName, true, Color.black);
            }
            ```
          - `CreateRenderingLayersTexture(renderGraph, cameraDescriptor)`: `.renderingLayersTexture`へ`cameraDescriptor`から`Create～()`(`m_RequiresRenderingLayer`で要求されているとき)
            ```csharp (UniversalRendererRenderGraph.cs)
            void CreateRenderingLayersTexture(RenderGraph renderGraph, TextureDesc desc)
            {
                if (!m_RequiresRenderingLayer) return;

                m_RenderingLayersTextureName = "_CameraRenderingLayersTexture";
                if (!m_RenderingLayerProvidesRenderObjectPass)
                    desc.msaaSamples = MSAASamples.None;
                desc.format = RenderingLayerUtils.GetFormat(m_RenderingLayersMaskSize);

                frameData.Get<UniversalResourceData>().renderingLayersTexture = CreateRenderGraphTexture(renderGraph, desc, m_RenderingLayersTextureName, true, desc.clearColor);
            }
            ```
            - `m_RequiresRenderingLayer`と`m_RenderingLayersMaskSize`
                ```csharp
                //『`class UniversalRenderer`============================================================================
                void SetupRenderingLayers(.)
                {
                    m_RequiresRenderingLayer = RenderingLayerUtils.RequireRenderingLayers(this, rendererFeatures, .., out m_RenderingLayersMaskSize);
                }
                //『`static class RenderingLayerUtils`===================================================================
                enum MaskSize{Bits8,Bits16,Bits24,Bits32,}
                internal static bool RequireRenderingLayers(List<ScriptableRendererFeature> rendererFeatures, .., out MaskSize combinedMaskSize)
                {
                    combinedMaskSize = MaskSize.Bits8;
                    bool result = false;
                    foreach (var rendererFeature in rendererFeatures)
                    {
                            result |= rendererFeature.RequireRenderingLayers(.., out MaskSize rendererMaskSize);
                            combinedMaskSize = Combine(combinedMaskSize, rendererMaskSize);
                    }
                    // URPのグローバル設定で、テクスチャにすべてのレンダリングレイヤーをエンコードするのに十分なビット数があることを確認してください
                    if (UniversalRenderPipelineGlobalSettings.instance)
                        combinedMaskSize = Combine(combinedMaskSize, GetMaskSize(RenderingLayerMask.GetRenderingLayerCount()));

                    return result;
                }
                static MaskSize Combine(MaskSize a, MaskSize b)
                {
                    return (MaskSize)Mathf.Max((int)a, (int)b); //『単なる`Max(..)`
                }
                static MaskSize GetMaskSize(int bits)
                {
                    int bytes = (bits + 7) / 8; //『`bits`から必要`bytes`を計算
                    switch (bytes){case 0:case 1: return MaskSize.Bits8; case 2: return MaskSize.Bits16; case 3: return MaskSize.Bits24; case 4: return MaskSize.Bits32; default: return MaskSize.Bits32;}
                }
                static extern int GetRenderingLayerCount();
                //『`class ScriptableRendererFeature`====================================================================
                internal virtual bool RequireRenderingLayers(.., out RenderingLayerUtils.MaskSize maskSize)
                {
                    maskSize = RenderingLayerUtils.MaskSize.Bits8;
                    return false;
                }
                ```
            - `RenderingLayerUtils.GetFormat(m_RenderingLayersMaskSize)`
                ```csharp
                public static GraphicsFormat GetFormat(MaskSize maskSize)
                {
                    switch (maskSize)
                    {
                        case MaskSize.Bits8:
                            return GraphicsFormat.R8_UInt;
                        case MaskSize.Bits16:
                            return GraphicsFormat.R16_UInt;
                        case MaskSize.Bits24:
                        case MaskSize.Bits32:
                            return GraphicsFormat.R32_UInt;
                    }
                }
                ```
