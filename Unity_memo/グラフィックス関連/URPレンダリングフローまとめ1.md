# URPレンダリングフローまとめ1

## uR.OnRecordRenderGraph

- `override void OnRecordRenderGraph(renderGraph, context)`

- `CreateRenderGraphCameraRenderTargets(renderGraph, isCameraTargetOffscreenDepth, s_RequiresIntermediateAttachments, prepassToCameraDepthTexture)`
    ```csharp (UniversalRendererRenderGraph.cs)
    void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, bool isCameraTargetOffscreenDepth, bool requireIntermediateAttachments, bool depthTextureIsDepthFormat)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // レンダーパスのヒストリーリクエストを収集し、ヒストリーテクスチャを更新します。
        UpdateCameraHistory(cameraData);

        var clearCameraParams = GetClearCameraParams(cameraData);

        // RG にインポートする前にバックバッファ RTHandle をセットアップします。
        SetupTargetHandles(cameraData); //『`RTHandle m_Target⟪Color¦Depth⟫Handle = ⟪｡cameraData.targetTexture｡¦｡BRTT.⟪CameraTarget¦Depth⟫｡⟫`
            void SetupTargetHandles(UniversalCameraData cameraData)
            {
                RenderTargetIdentifier targetColorId = cameraData.targetTexture != null ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
                if (m_TargetColorHandle == null) m_TargetColorHandle = RTHandles.Alloc(targetColorId, "Backbuffer color");
                else if (m_TargetColorHandle.nameID != targetColorId) m_TargetColorHandle.SetTexture(targetColorId);

                RenderTargetIdentifier targetDepthId = cameraData.targetTexture != null ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.Depth;
                if (m_TargetDepthHandle == null) m_TargetDepthHandle = RTHandles.Alloc(targetDepthId, "Backbuffer depth");
                else if (m_TargetDepthHandle.nameID != targetDepthId) m_TargetDepthHandle.SetTexture(targetDepthId);
            }

        // バックバッファを Render Graph にインポートします。
        ImportBackBuffers(renderGraph, cameraData, clearCameraParams.clearValue, isCameraTargetOffscreenDepth);
            void ImportBackBuffers(RenderGraph renderGraph, UniversalCameraData cameraData, Color clearBackgroundColor, bool isCameraTargetOffscreenDepth)
            {
                bool clearBackbufferOnFirstUse = (cameraData.renderType == CameraRenderType.Base) && !s_RequiresIntermediateAttachments; //『`中間RT`が全画面より小さい`Viewport`の可能性があるためクリアできない
                clearBackbufferOnFirstUse |= isCameraTargetOffscreenDepth; // オフスクリーン深度テクスチャへレンダリングしている場合はクリアを強制します。
                bool noStoreOnlyResolveBBColor = !s_RequiresIntermediateAttachments && (cameraData.cameraTargetDescriptor.msaaSamples > 1); //『>MSAA の生データを最後まで store しなくても、 resolve 済み結果だけ残せばいい。
                TextureUVOrigin backbufferTextureUVOrigin = cameraData.targetTexture == null ? TextureUVOrigin.TopLeft : TextureUVOrigin.BottomLeft;
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

        TextureDesc cameraDescriptor;
        GetTextureDesc(in cameraData.cameraTargetDescriptor, out cameraDescriptor);
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

        if (requireIntermediateAttachments)
        {
            if (!isCameraTargetOffscreenDepth)
            {
                cameraDescriptor.format = cameraData.cameraTargetDescriptor.graphicsFormat;
                CreateIntermediateCameraColorAttachment(renderGraph, cameraData, in cameraDescriptor, clearCameraParams.mustClearColor, clearCameraParams.clearValue);
                    void CreateIntermediateCameraColorAttachment(RenderGraph renderGraph, UniversalCameraData cameraData, in TextureDesc cameraDescriptor, bool clearColor, Color clearBackgroundColor)
                    {
                        var resourceData = frameData.Get<UniversalResourceData>();

                        var desc = cameraDescriptor;
                        desc.filterMode = FilterMode.Bilinear;
                        desc.wrapMode = TextureWrapMode.Clamp;

                        if (cameraData.resolveFinalTarget && cameraData.renderType == CameraRenderType.Base/*isSingleCamera*/)
                        {
                            resourceData.cameraColor = CreateRenderGraphTexture(/*☆*/renderGraph, in desc, _SingleCameraTargetAttachmentName, clearColor, clearBackgroundColor, desc.filterMode, desc.wrapMode, cameraData.resolveFinalTarget);
                            s_CurrentColorHandle = -1;
                        }
                        else //カメラスタッキング
                        {
                            RenderingUtils.ReAllocateHandleIfNeeded(ref s_RenderGraphCameraColorHandles[0], desc, _CameraTargetAttachmentAName);
                            RenderingUtils.ReAllocateHandleIfNeeded(ref s_RenderGraphCameraColorHandles[1], desc, _CameraTargetAttachmentBName);
                            ImportResourceParams importColorParams = new ImportResourceParams
                            {
                                clearOnFirstUse = clearColor,
                                clearColor = clearBackgroundColor,
                                discardOnLastUse = cameraData.resolveFinalTarget // スタック内の最後のカメラ
                            };
                            if (cameraData.renderType == CameraRenderType.Base)
                                s_CurrentColorHandle = 0; // 決定論的なフレーム結果のため、ベースカメラが常に ColorAttachmentA へのレンダリングから開始するようにします。
                            resourceData.cameraColor = renderGraph.ImportTexture(currentRenderGraphCameraColorHandle/*☆*/, importColorParams);
                        }
                        resourceData.activeColorID = UniversalResourceData.ActiveID.Camera;
                    }
            }
            cameraDescriptor.format = cameraData.cameraTargetDescriptor.depthStencilFormat; //↓↓で書き換えられてるから要らないかも
            CreateIntermediateCameraDepthAttachment(renderGraph, cameraData, in cameraDescriptor, clearCameraParams.mustClearDepth, clearCameraParams.clearValue, depthTextureIsDepthFormat/*☆*/);
                void CreateIntermediateCameraDepthAttachment(RenderGraph renderGraph, UniversalCameraData cameraData, in TextureDesc cameraDescriptor, bool clearDepth, Color clearBackgroundDepth, bool depthTextureIsDepthFormat)
                {
                    var resourceData = frameData.Get<UniversalResourceData>();

                    var desc = cameraDescriptor;
                    desc.bindTextureMS = false;
                    desc.format = cameraDepthAttachmentFormat/*☆*/;
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
                    m_CopyDepthPass.CopyToDepth = false; //『`= depthTextureIsDepthFormat;`となっていたが、`true`の場合は`cameraDepthTextureFormat`に描く場合は直接レンダリングするので`m_CopyDepthPass`自体が使われない
                    m_CopyDepthPass.MsaaSamples = (int) desc.msaaSamples;
                    m_CopyDepthPass.m_CopyResolvedDepth = !desc.bindTextureMS;
                }
        }
        else
        {
            frameData.Get<UniversalResourceData>().SwitchActiveTexturesToBackbuffer(); //『active⟪Color¦Depth⟫ID = UniversalResourceData.ActiveID.BackBuffer;
        }

        //『`Create～Texture(..)`
        CreateCameraDepthCopyTexture(renderGraph, cameraDescriptor, depthTextureIsDepthFormat, clearCameraParams.clearValue);
            void CreateCameraDepthCopyTexture(RenderGraph renderGraph, TextureDesc desc, bool depthTextureIsDepthFormat, Color clearColor)
            {
                desc.msaaSamples = MSAASamples.None;
                if (depthTextureIsDepthFormat)
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
        CreateCameraNormalsTexture(renderGraph, cameraDescriptor);
            void CreateCameraNormalsTexture(RenderGraph renderGraph, TextureDesc desc)
            {
                desc.msaaSamples = MSAASamples.None;
                desc.format = DepthNormalOnlyPass.GetGraphicsFormat(); //『`GraphicsFormat.R8G8B8A8_SNorm`
                frameData.Get<UniversalResourceData>().cameraNormalsTexture = CreateRenderGraphTexture(renderGraph, desc, DepthNormalOnlyPass.k_CameraNormalsTextureName, true, Color.black);
            }
        CreateMotionVectorTextures(renderGraph, cameraDescriptor);
            void CreateMotionVectorTextures(RenderGraph renderGraph, TextureDesc desc)
            {
                desc.msaaSamples = MSAASamples.None;
                desc.format = MotionVectorRenderPass.k_TargetFormat; //『`GraphicsFormat.R16G16_SFloat`
                frameData.Get<UniversalResourceData>().motionVectorColor = CreateRenderGraphTexture(renderGraph, desc, MotionVectorRenderPass.k_MotionVectorTextureName, true, Color.black);
                desc.format = cameraDepthAttachmentFormat;
                frameData.Get<UniversalResourceData>().motionVectorDepth = CreateRenderGraphTexture(renderGraph, desc, MotionVectorRenderPass.k_MotionVectorDepthTextureName, true, Color.black);
            }
        CreateRenderingLayersTexture(renderGraph, cameraDescriptor);
            void CreateRenderingLayersTexture(RenderGraph renderGraph, TextureDesc desc)
            {
                if (!m_RequiresRenderingLayer) return;

                m_RenderingLayersTextureName = "_CameraRenderingLayersTexture";
                if (!m_RenderingLayerProvidesRenderObjectPass)
                    desc.msaaSamples = MSAASamples.None;
                desc.format = RenderingLayerUtils.GetFormat(m_RenderingLayersMaskSize/*☆*/);

                frameData.Get<UniversalResourceData>().renderingLayersTexture = CreateRenderGraphTexture(renderGraph, desc, m_RenderingLayersTextureName, true, desc.clearColor);
            }
        if (cameraData.isHDROutputActive && cameraData.rendersOverlayUI) CreateOffscreenUITexture(renderGraph, cameraDescriptor);
    }
    ```
