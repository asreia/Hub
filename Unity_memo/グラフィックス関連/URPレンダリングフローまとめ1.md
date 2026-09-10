# URPレンダリングフローまとめ1

## uR.OnRecordRenderGraph

- `override void OnRecordRenderGraph(renderGraph, context)`
    ```csharp (UniversalRendererRenderGraph.cs)
    internal override void OnRecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        useRenderPassEnabled = renderGraph.nativeRenderPassesEnabled;
        MotionVectorRenderPass.SetRenderGraphMotionVectorGlobalMatrices(renderGraph, cameraData);

        /*☆*/m_ForwardLights.SetupRenderGraphLights(renderGraph, frameData.Get<UniversalRenderingData>(), cameraData, frameData.Get<UniversalLightData>());

        /*☆*/RequireResults requireResults = CreateCameraRenderTargets(renderGraph, cameraData, frameData.Get<UniversalPostProcessingData>().isEnabled);

        RecordCustomRenderGraphPasses(renderGraph, RenderPassEvent.BeforeRendering);

        TextureHandle activeTargetForIsYFlipped = resourceData.activeColorTexture.IsValid() ? resourceData.activeColorTexture : resourceData.activeDepthTexture;
        /*☆*/SetupRenderGraphCameraProperties(renderGraph, activeTargetForIsYFlipped);

        #if VISUAL_EFFECT_GRAPH_0_0_1_OR_NEWER //『↓コードが有効でない(灰色)なのは、このUnityプロジェクトに`Visual Effect Graph パッケージ`が入っていない
                    ProcessVFXCameraCommand(renderGraph); //『AddUnsafePass =>
                        //『CommandBufferHelpers.VFXManager_ProcessCameraCommand(cmd, camera, cullResults) =>extern
        #endif

        if (requireResults.isCameraTargetOffscreenDepth){OnOffscreenDepthTextureRendering(renderGraph, context, resourceData, cameraData); return;}
        OnBeforeRendering(renderGraph);
        OnMainRendering(renderGraph, context, requireResults.renderPassInputs, requireResults.requirePrepass, requireResults.requireDepthTexture);
        OnAfterRendering(renderGraph, requireResults.applyPostProcessing);
    }
    ```
  - `void MotionVectorRenderPass.SetRenderGraphMotionVectorGlobalMatrices(renderGraph, cameraData)`
    ```csharp (images\URPレンダリングフローまとめ\まとめ1\SetRenderGraphMotionVectorGlobalMatrices.png)
    static void SetRenderGraphMotionVectorGlobalMatrices(RenderGraph renderGraph, UniversalCameraData cameraData)
    {
        if (cameraData.camera.TryGetComponent<UniversalAdditionalCameraData>(out var additionalCameraData))
        {
            using (var builder = renderGraph.AddRasterRenderPass<MotionMatrixPassData>(s_SetMotionMatrixProfilingSampler.name, out var passData, s_SetMotionMatrixProfilingSampler))
            {
                passData.motionData = additionalCameraData.motionVectorsPersistentData;
                passData.xr = cameraData.xr;

                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (MotionMatrixPassData data, RasterGraphContext context) =>
                {
                    data.motionData.SetGlobalMotionMatrices(context.cmd, data.xr);
                        //『class MotionVectorsPersistentData
                            //『var passID/*`0`*/ = GetXRMultiPassId(xr); //『`xr.enabled==false`なので`0`が返る
                            //『cmd.SetGlobalMatrix(ShaderPropertyId.previousViewProjectionNoJitter/*_PrevViewProjMatrix*/, previousViewProjectionStereo[passID]);
                            //『cmd.SetGlobalMatrix(ShaderPropertyId.viewProjectionNoJitter/*_NonJitteredViewProjMatrix*/, viewProjectionStereo[passID]);

                });
            }
        }
    }
    ```
  - `void m_ForwardLights.`**SetupRenderGraphLights**`(renderGraph, frameData.Get<UniversalRenderingData>(), cameraData, frameData.Get<UniversalLightData>())`
    ```csharp (images\URPレンダリングフローまとめ\まとめ1\ForwardLights\SetupLights.png)
    void SetupRenderGraphLights(RenderGraph renderGraph, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
    {
        using (var builder = renderGraph.AddUnsafePass<SetupLightPassData>(s_SetupForwardLights.name, out var passData, s_SetupForwardLights))
        {
            passData.renderingData = renderingData;
            passData.cameraData = cameraData;
            passData.lightData = lightData;

            builder.AllowPassCulling(false);

            builder.SetRenderFunc((SetupLightPassData data, UnsafeGraphContext rgContext) =>
            {
                SetupLights(rgContext.cmd, data.renderingData, data.cameraData, data.lightData);
                void SetupLights(UnsafeCommandBuffer cmd, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
                {
                    using (new ProfilingScope(m_ProfilingSampler))
                    {
                        //『Forward+機構 と ReflectionProbeの`.UpdateGpuData`
                        if (m_UseForwardPlus)
                        {
                            if (lightData.reflectionProbeAtlas)
                            {
                                m_ReflectionProbeManager.UpdateGpuData(CommandBufferHelpers.GetNativeCommandBuffer(cmd), ref renderingData.cullResults);
                            }

                            using (new ProfilingScope(m_ProfilingSamplerFPComplete))
                            {
                                m_CullingHandle.Complete();
                            }

                            using (new ProfilingScope(m_ProfilingSamplerFPUpload))
                            {
                                //『画像参照
                                m_ZBinsBuffer.SetData(m_ZBins.Reinterpret<float4>(UnsafeUtility.SizeOf<uint>()));
                                m_TileMasksBuffer.SetData(m_TileMasks.Reinterpret<float4>(UnsafeUtility.SizeOf<uint>()));
                                cmd.SetGlobalConstantBuffer(m_ZBinsBuffer, "urp_ZBinBuffer", 0, UniversalRenderPipeline.maxZBinWords * 4);
                                cmd.SetGlobalConstantBuffer(m_TileMasksBuffer, "urp_TileBuffer", 0, UniversalRenderPipeline.maxTileWords * 4);
                            }

                            //『 viewZ=dot(ViewForward, PosWS - CameraPositionWS), ZBinIndex=(Perspective ? log2(viewZ) : viewZ) * x + y, z=ProbeBegin
                            cmd.SetGlobalVector("_FPParams0", math.float4(m_ZBinScale, m_ZBinOffset, m_LightCount, m_DirectionalLightCount));
                            //『 TileXY=uint2(ScreenUV * xy),  TileIndex=TileY * z + TileX,  TileWordsOffset=TileIndex * w
                            cmd.SetGlobalVector("_FPParams1", math.float4(cameraData.pixelRect.size / m_ActualTileWidth, m_TileResolution.x, m_WordsPerTile));
                            //『 ZBinOffset=min(ZBinIndex, x - 1) * (WordsPerTile + 2❰header❱),  y=TotalTileCount
                            cmd.SetGlobalVector("_FPParams2", math.float4(m_BinCount, m_TileResolution.x * m_TileResolution.y, 0, 0));
                            //『 ClusterInit(ScreenUV, PosWS, h)EntityIndex=⟪h=0:0❰Light❱¦h=1:ProbeBegin⟫, WordInMask=EntityIndex/32, BitInWord=EntityIndex%32
                            //『 wordIndex=⟪TileWordsOffset¦ZBinOffset + 2❰header❱⟫+WordInMask   (直感的にはこんな感じ)
                            //『 urp_⟪ZBin ∩ Tile⟫Buffer[wordIndex/4][wordIndex%4]>>BitInWord ～ ⟪<<ProbeBegin¦<<(WordsPerTile*32 - ProbeBegin)⟫
                        }
                        cmd.SetKeyword(ShaderGlobalKeywords.ClusterLightLoop, m_UseForwardPlus);
                        //『`lightData.visibleLights`から`cmd`で`._MainLight～`と`_AdditionalLights～[]`を設定
                        SetupShaderLightConstants(cmd, ref renderingData.cullResults, lightData);
                        //『AdditionalLightsPixel
                        bool lightCountCheck = (cameraData.renderer.stripAdditionalLightOffVariants/*`％true`*/ && lightData.supportsAdditionalLights) || lightData.additionalLightsCount > 0;
                        cmd.SetKeyword(ShaderGlobalKeywords.AdditionalLightsPixel, lightCountCheck);//『`.～Vertex`はcullした

                        //『Mixed Lighting
                        bool isShadowMask = lightData.supportsMixedLighting && m_MixedLightingSetup == MixedLightingSetup.ShadowMask;
                        bool isShadowMaskAlways = isShadowMask && QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask;
                        bool isSubtractive = lightData.supportsMixedLighting && m_MixedLightingSetup == MixedLightingSetup.Subtractive;
                        cmd.SetKeyword(ShaderGlobalKeywords.LightmapShadowMixing, isSubtractive || isShadowMaskAlways);
                        cmd.SetKeyword(ShaderGlobalKeywords.ShadowsShadowMask, isShadowMask);
                        cmd.SetKeyword(ShaderGlobalKeywords.MixedLightingSubtractive, isSubtractive); // 後方互換性のため。

                        //『Reflection Probe
                        cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeBlending, lightData.reflectionProbeBlending);
                        cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeBoxProjection, lightData.reflectionProbeBoxProjection);
                        cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeAtlas, lightData.reflectionProbeAtlas && m_UseForwardPlus && lightData.reflectionProbeBlending); // シェーダーストリッピングの条件と一致させる必要があります。
                        if (GraphicsSettings.TryGetRenderPipelineSettings<URPReflectionProbeSettings>(out var reflectionProbeSettings))
                            cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeRotation, reflectionProbeSettings.UseReflectionProbeRotation);
                        else
                            cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeRotation, false);

                        //『Light Layers
                        cmd.SetKeyword(ShaderGlobalKeywords.LightLayers, lightData.supportsLightLayers);


                        //『APV
                        var asset = UniversalRenderPipeline.asset;
                        bool apvIsEnabled = asset != null && asset.lightProbeSystem == LightProbeSystem.ProbeVolumes;
                        ProbeVolumeSHBands probeVolumeSHBands = asset.probeVolumeSHBands; //『`[SerializeField]`初期値は`.SphericalHarmonicsL1`
                        cmd.SetKeyword(ShaderGlobalKeywords.ProbeVolumeL1, apvIsEnabled && probeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL1);
                        cmd.SetKeyword(ShaderGlobalKeywords.ProbeVolumeL2, apvIsEnabled && probeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2);
                        var shMode = PlatformAutoDetect.ShAutoDetect(asset.shEvalMode); //『`ShAutoDetect(..)`:`％.Auto`のとき、モバイル:`.PerVertex`,非モバイル:`.PerPixel`
                        cmd.SetKeyword(ShaderGlobalKeywords.EVALUATE_SH_MIXED, shMode == ShEvalMode.Mixed);
                        cmd.SetKeyword(ShaderGlobalKeywords.EVALUATE_SH_VERTEX, shMode == ShEvalMode.PerVertex);
                        var stack = VolumeManager.instance.stack;
                        bool enableProbeVolumes = ProbeReferenceVolume.instance.UpdateShaderVariablesProbeVolumes( //『これもAPVっぽい
                            CommandBufferHelpers.GetNativeCommandBuffer(cmd),
                            stack.GetComponent<ProbeVolumesOptions>(),
                            cameraData.IsTemporalAAEnabled() ? Time.frameCount : 0,
                            lightData.supportsLightLayers);
                        cmd.SetGlobalInt("_EnableProbeVolumes", enableProbeVolumes ? 1 : 0);

                        //『Cookie
                        if (m_LightCookieManager != null)
                            m_LightCookieManager.Setup(CommandBufferHelpers.GetNativeCommandBuffer(cmd), lightData);
                        else
                            cmd.SetKeyword(ShaderGlobalKeywords.LightCookies, false);

                        //『Light Map
                        if (GraphicsSettings.TryGetRenderPipelineSettings<LightmapSamplingSettings>(out var lightmapSamplingSettings))
                            cmd.SetKeyword(ShaderGlobalKeywords.LIGHTMAP_BICUBIC_SAMPLING, lightmapSamplingSettings.useBicubicLightmapSampling);
                        else
                            cmd.SetKeyword(ShaderGlobalKeywords.LIGHTMAP_BICUBIC_SAMPLING, false);
                    }
                }
            });
        }
    }
    ```
    - `void m_ReflectionProbeManager.UpdateGpuData(CommandBufferHelpers.GetNativeCommandBuffer(cmd), ref renderingData.cullResults)`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\ForwardLights\ReflectionProbeManager\UpdateGpuData.png)
        struct ReflectionProbeManager : IDisposable
        {
            int2 m_Resolution; //『現在のAtlasテクスチャの解像度
            RenderTexture m_AtlasTexture0; //『`probe.texture`を八面体で詰めるAtlasテクスチャ
            RenderTexture m_AtlasTexture1; //『一時的にAtlasテクスチャ(`m_AtlasTexture0`)の拡張時に使われるswap用変数
            BuddyAllocator m_AtlasAllocator; //『Atlasテクスチャの領域管理に使用する住所システム。(`level`は区画の大きさ(MipLvと相対的), `index`はその`level`内のindex)
            Dictionary<EntityId, CachedProbe> m_Cache; //『これに含まれてるならば、Atlasテクスチャにその`cachedProbe`は含まれている
            List<EntityId> m_NeedsUpdate; //『Atlasテクスチャの更新に使用する`probe`(`id = probe.reflectionProbe.GetEntityId()`)
            List<EntityId> m_NeedsRemove;

            // 定数バッファへ値を設定(`cmd`)する際に使用する、事前確保済みの配列 //『恐らく事前確保のためにフィールドに出してるだけ
            Vector4[] m_～;

            const int k_MaxMipCount = 7;
            const string k_ReflectionProbeAtlasName = "URP Reflection Probe Atlas";

            unsafe struct CachedProbe //『Atlasテクスチャ(`m_AtlasTexture0`)の更新を管理するための`probe`のキャッシュ
            {
                public uint updateCount; //『`probe.texture.updateCount`
                public int size; //『`probe.texture.width`
                public int mipCount; //『`math.min(math.ceillog2(probe.texture.width * 4) + 1, k_MaxMipCount)`
                //『`m_AtlasAllocator.TryAllocate(mipLevel, out var allocation)`。(`BuddyAllocation allocation`構造体を各フィールドに分けて格納する)
                public fixed int dataIndices[k_MaxMipCount];
                public fixed int levels[k_MaxMipCount];
                public Texture texture; //『`probe.texture`
                public int lastUsed;
                public Vector4 hdrData;
            }

            public static ReflectionProbeManager Create()
            {
                var instance = new ReflectionProbeManager();
                instance.Init();
                return instance;
            }

            void Init()
            {
                var maxProbes = UniversalRenderPipeline.maxVisibleReflectionProbes;
                m_Resolution = 1;
                var format = GraphicsFormat.B10G11R11_UFloatPack32;
                if (!SystemInfo.IsFormatSupported(format, GraphicsFormatUsage.Render)) { format = GraphicsFormat.R16G16B16A16_SFloat; }
                m_AtlasTexture0 = new RenderTexture(new RenderTextureDescriptor
                {
                    width = m_Resolution.x,
                    height = m_Resolution.y,
                    volumeDepth = 1,
                    dimension = TextureDimension.Tex2D,
                    graphicsFormat = format,
                    useMipMap = false,
                    msaaSamples = 1
                })
                {
                    name = k_ReflectionProbeAtlasName, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave
                };
                m_AtlasTexture0.Create();

                m_AtlasTexture1 = new RenderTexture(m_AtlasTexture0.descriptor)
                {
                    name = k_ReflectionProbeAtlasName, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave
                };

                // 確保可能な最小解像度は4x4とする。レベル数は次の式で計算する。
                // log2(最大値) - log2(4) = log2(最大値) - 2
                m_AtlasAllocator = new BuddyAllocator(math.floorlog2(SystemInfo.maxTextureSize) - 2, 2);
                m_Cache = new Dictionary<EntityId, CachedProbe>(maxProbes);
                m_Needs～ = new List<EntityId>(maxProbes);
                m_～ = new Vector4[maxProbes];
            }

            //『`m_Cache`に含まれる`probe = cullResults.visibleReflectionProbes[～]`の`probe.texture`を八面体にしてAtlasテクスチャ(`m_AtlasTexture0`)に詰め、`probe`の情報とAtlasテクスチャを`cmd`でシェーダーに送る。
            public unsafe void UpdateGpuData(CommandBuffer cmd, ref CullingResults cullResults)
            {
                var probes = cullResults.visibleReflectionProbes;
                var probeCount = math.min(probes.Length, UniversalRenderPipeline.maxVisibleReflectionProbes/*64*/);
                var frameIndex = Time.renderedFrameCount;

                //『変更または古い`cachedProbe`の区画解放＆`m_Cache`から削除
                foreach (var (id, cachedProbe) in m_Cache)
                {
                    // 1フレームを超えて使用されていない、テクスチャが存在しなくなった、またはサイズが変わったプローブをキャッシュから破棄する。
                    if (Math.Abs(cachedProbe.lastUsed - frameIndex) > 1 || //『最後に使用したフレームから2フレーム後に破棄される
                        !cachedProbe.texture ||
                        cachedProbe.size != cachedProbe.texture.width)
                    {
                        m_NeedsRemove.Add(id);
                        for (var i = 0; i < k_MaxMipCount; i++)
                        {
                            if (cachedProbe.dataIndices[i] != -1) m_AtlasAllocator.Free(new BuddyAllocation(cachedProbe.levels[i], cachedProbe.dataIndices[i]));
                        }
                    }
                }
                foreach (var probeIndex in m_NeedsRemove)
                {
                    m_Cache.Remove(probeIndex);
                }
                m_NeedsRemove.Clear();

                var requiredAtlasSize = math.int2(0, 0);

                //『`probes[]`(`probe`)を素に`m_Cache[]`(`cachedProbe`)を更新する
                for (var probeIndex = 0; probeIndex < probeCount; probeIndex++)
                {
                    VisibleReflectionProbe probe = probes[probeIndex];
                    Texture texture = probe.texture;
                    EntityId id = probe.reflectionProbe.GetEntityId();
                    bool wasCached = m_Cache.TryGetValue(id, out var cachedProbe);

                    if (!texture) continue;

                    if (!wasCached) //『`m_Cache`に登録されていない`probe`情報を`cachedProbe`に設定する。(後で`m_Cache[id] = cachedProbe`される)
                    {
                        cachedProbe.size = texture.width;
                        var mipCount = math.ceillog2(cachedProbe.size * 4) + 1; //『←↓`*4`とか`+2`とか深く考えない
                        var level = m_AtlasAllocator.levelCount + 2 - mipCount;
                        cachedProbe.mipCount = math.min(mipCount, k_MaxMipCount);
                        cachedProbe.texture = texture;

                        var mip = 0;
                        for (; mip < cachedProbe.mipCount; mip++)
                        {
                            // 最大レベルに制限する。これは64x64以下の場合に関係し、その場合は1x1ミップにも有効な内容が存在する。
                            // 八面体のサイズは面サイズの2倍なので、最終的に2x2となる。境界を考慮すると、八面体用に2x2テクセルを
                            // 残すため、最終ミップは4x4でなければならない。
                            var mipLevel = math.min(level + mip, m_AtlasAllocator.levelCount - 1);
                            if (!m_AtlasAllocator.TryAllocate(mipLevel, out var allocation)) break;
                            // C#では構造体型の固定長配列を使用できないため、allocation構造体を各フィールドに分けて格納する :(
                            cachedProbe.levels[mip] = allocation.level;
                            cachedProbe.dataIndices[mip] = allocation.index;
                            //『`.level`と`index`から`ScaleOffset(UV空間0～1)`を算出し、そこにAtlas解像度(`m_Resolution.xyxy`)を乗算して、AtlasテクスチャPixel空間(`scaleOffset`)にする
                            var scaleOffset = (int4)(GetScaleOffset(mipLevel, allocation.index, true, false) * m_Resolution.xyxy); //『`mipLevel == allocation.level`
                            requiredAtlasSize = math.max(requiredAtlasSize, scaleOffset.zw + scaleOffset.xy); //『Atlasテクスチャに詰め込む八面体の右上(.zw + .xy)の最大位置
                        }

                        // アトラスの空き領域が不足したか確認する。//『`.TryAllocate(..)`で`break`したか?
                        if (mip < cachedProbe.mipCount)
                        {
                            for (var i = 0; i < mip; i++) m_AtlasAllocator.Free(new BuddyAllocation(cachedProbe.levels[i], cachedProbe.dataIndices[i]));
                            for (var i = 0; i < k_MaxMipCount; i++) cachedProbe.dataIndices[i] = -1; //『←↑全ての`mip`が`.TryAllocate(..)`できないならば全て解放する
                            continue;
                        }

                        for (; mip < k_MaxMipCount; mip++)
                        {
                            cachedProbe.dataIndices[mip] = -1; //『`cachedProbe.mipCount`を超える残りの`mip`範囲(`k_MaxMipCount`まで)を未使用(`-1`)にする
                        }
                    }

                    var needsUpdate = !wasCached || cachedProbe.updateCount != texture.updateCount || cachedProbe.hdrData != probe.hdrData;

                    if (needsUpdate)
                    {
                        cachedProbe.updateCount = texture.updateCount;
                        m_NeedsUpdate.Add(id);
                    }

                    // プローブが毎フレーム更新に設定されている場合、次のフレームで破棄されるよう最終使用フレームを-1にする。
                    if (probe.reflectionProbe.mode == ReflectionProbeMode.Realtime && probe.reflectionProbe.refreshMode == ReflectionProbeRefreshMode.EveryFrame)
                        cachedProbe.lastUsed = -1;
                    else
                        cachedProbe.lastUsed = frameIndex;

                    cachedProbe.hdrData = probe.hdrData;
                    m_Cache[id] = cachedProbe;
                }

                // 現在の割り当てを収容できる大きさがなければ、アトラスを拡張する。
                if (math.any(m_Resolution < requiredAtlasSize)) //『`int2.xy`の片方が`true`なら`true`
                {
                    requiredAtlasSize = math.max(m_Resolution, math.ceilpow2(requiredAtlasSize)); //『`int2.xy`のどちらかの最大値をとるので`max(..)`を使う
                    m_AtlasTexture1.width = requiredAtlasSize.x; m_AtlasTexture1.height = requiredAtlasSize.y;
                    m_AtlasTexture1.Create();

                    if (m_AtlasTexture0.width != 1)
                    {
                        Graphics.CopyTexture(m_AtlasTexture0, 0, 0, 0, 0, m_Resolution.x, m_Resolution.y, m_AtlasTexture1, 0, 0, 0, 0);
                    }

                    m_AtlasTexture0.Release();
                    (m_AtlasTexture0, m_AtlasTexture1) = (m_AtlasTexture1, m_AtlasTexture0); //『0⇆1 swap
                    m_Resolution = requiredAtlasSize;
                }

                var skipCount = 0;
                //『`probes[probeIndex]`から`cmd`設定用の`m_～[dataIndex]`へセットする
                for (var probeIndex = 0; probeIndex < probeCount; probeIndex++)
                {
                    VisibleReflectionProbe probe = probes[probeIndex];
                    EntityId id = probe.reflectionProbe.GetEntityId();
                    int dataIndex = probeIndex - skipCount;
                        //『Q:シェーダー側で`probeIndex`と`dataIndex`が異なっても正しく参照できるのか?
                        //『A:基本的にForward+機構の並べ替えによって合わせられるが、`.TryAllocate`失敗時は不整合になり得る。つまりindexがForward+機構側と連携できていない。
                            //『`probes[probeIndex - skipCount] = probe;`をクラスタ構築前に行う必要がある(ムリ)
                    if (!m_Cache.TryGetValue(id, out var cachedProbe) || !probe.texture)
                    {
                        skipCount++;
                        continue;
                    }
                    m_BoxMax[dataIndex] = new Vector4(probe.bounds.max.x, probe.bounds.max.y, probe.bounds.max.z, probe.blendDistance); //『Blend Distance
                    m_BoxMin[dataIndex] = new Vector4(probe.bounds.min.x, probe.bounds.min.y, probe.bounds.min.z, probe.importance); //『Importance
                    m_ProbePosition[dataIndex] = new Vector4(probe.localToWorldMatrix.m03, probe.localToWorldMatrix.m13, probe.localToWorldMatrix.m23, (probe.isBoxProjection ? 1 : -1) * (cachedProbe.mipCount));//『Box Projection有無 + ミップ数
                    //『`atlasUV = probeUV(八面体) * scaleOffset.xy + scaleOffset.zw;`: Probe内のUVをAtlas全体のUVへ変換します。
                    for (var i = 0; i < cachedProbe.mipCount; i++) m_MipScaleOffset[dataIndex * k_MaxMipCount + i] = GetScaleOffset(cachedProbe.levels[i], cachedProbe.dataIndices[i], false, false);
                    var rot = Quaternion.Inverse(probe.reflectionProbe.transform.rotation);
                    m_Rotations[dataIndex] = new Vector4(rot.x, rot.y, rot.z, rot.w);
                }

                //『`m_NeedsUpdate`の`cachedProbe`を八面体にしてAtlasテクスチャ(`m_AtlasTexture0`)へ描画し、`m_～[dataIndex]`にセットされた値を`cmd`に設定しシェーダーへ送る
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UpdateReflectionProbeAtlas)))
                {
                    cmd.SetRenderTarget(m_AtlasTexture0);

                    foreach (var probeId in m_NeedsUpdate)
                    {
                        var cachedProbe = m_Cache[probeId];
                        for (var mip = 0; mip < cachedProbe.mipCount; mip++)
                        {
                            var level = cachedProbe.levels[mip];
                            var dataIndex = cachedProbe.dataIndices[mip];
                            // Y反転が必要な場合は、更新頻度の低いアトラス側を反転する。これにより参照座標が正しくなる。
                            // そのため、シェーダーコードで参照座標をY反転する必要がなくなる。
                            var scaleBias = GetScaleOffset(level, dataIndex, true, !SystemInfo.graphicsUVStartsAtTop); //『`.level`と`index`から`ScaleOffset(UV空間0～1)`を算出 (多分VertexShaderでクワッド範囲(`positionCS`)に使用)
                            var sizeWithoutPadding = (1 << (m_AtlasAllocator.levelCount + 1 - level)) - 2; //『多分PixelShaderで`cachedProbe.texture`サンプリング時に使用
                            //『`cachedProbe.texture`を八面体にして`m_AtlasTexture0`へ描画
                            Blitter.BlitCubeToOctahedral2DQuadWithPadding(cmd, cachedProbe.texture, new Vector2(sizeWithoutPadding, sizeWithoutPadding), scaleBias, mip, true, 2, cachedProbe.hdrData);
                        }
                    }
                    //『`m_～[dataIndex]`にセットされた値を`cmd`に設定しシェーダーへ送る
                    cmd.SetGlobalVectorArray(Shader.PropertyToID("urp_ReflProbes_BoxMin"), m_BoxMin);
                    cmd.SetGlobalVectorArray(Shader.PropertyToID("urp_ReflProbes_BoxMax"), m_BoxMax);
                    cmd.SetGlobalVectorArray(Shader.PropertyToID("urp_ReflProbes_ProbePosition"), m_ProbePosition);
                    cmd.SetGlobalVectorArray(Shader.PropertyToID("urp_ReflProbes_MipScaleOffset"), m_MipScaleOffset);
                    cmd.SetGlobalVectorArray(Shader.PropertyToID("urp_ReflProbes_Rotation"), m_Rotations);
                    cmd.SetGlobalFloat(Shader.PropertyToID("urp_ReflProbes_Count"), probeCount - skipCount); //『シェーダーで使われていない
                    cmd.SetGlobalTexture(Shader.PropertyToID("urp_ReflProbes_Atlas"), m_AtlasTexture0);
                }

                m_NeedsUpdate.Clear();
            }

            float4 GetScaleOffset(int level, int dataIndex, bool includePadding, bool yflip) //『`.level`と`index`から`ScaleOffset(UV空間0～1)`を算出
            {
                // level = m_AtlasAllocator.levelCount + 2 - (log2(size) + 1) <=>
                // log2(size) + 1 = m_AtlasAllocator.levelCount + 2 - level <=>
                // log2(size) = m_AtlasAllocator.levelCount + 1 - level <=>
                // size = 2^(m_AtlasAllocator.levelCount + 1 - level)
                var size = (1 << (m_AtlasAllocator.levelCount + 1 - level));
                var coordinate = SpaceFillingCurves.DecodeMorton2D((uint)dataIndex);
                var scale = (size - (includePadding ? 0 : 2)) / ((float2)m_Resolution);
                var bias = ((float2) coordinate * size + (includePadding ? 0 : 1)) / (m_Resolution);
                if (yflip) bias.y = 1.0f - bias.y - scale.y;
                return math.float4(scale, bias);
            }

            public void Dispose(){～}
        }
        ```
    - `void SetupShaderLightConstants(cmd, ref renderingData.cullResults, lightData)`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\ForwardLights\SetupShaderLightConstants\SetupShaderLightConstants.png)
        void SetupShaderLightConstants(UnsafeCommandBuffer cmd, ref CullingResults cullResults, UniversalLightData lightData)
        {
            m_MixedLightingSetup = MixedLightingSetup.None;

            SetupMainLightConstants(cmd, lightData);
            SetupAdditionalLightConstants(cmd, lightData);
        }
        ```
      - `void SetupMainLightConstants(cmd, lightData)`
        ```csharp
        void SetupMainLightConstants(UnsafeCommandBuffer cmd, UniversalLightData lightData)
        {
            InitializeLightConstants
            (
                lightData.visibleLights,
                lightData.mainLightIndex,
                lightData.supportsLightLayers,
                out Vector4 lightPos,
                out Vector4 lightColor, out Vector4 _, out Vector4 _,
                out Vector4 lightOcclusionChannel,
                out uint lightLayerMask,
                out bool isSubtractive
            );
            lightColor.w = isSubtractive ? 0f : 1f;

            cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, lightPos);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, lightColor);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightOcclusionProbesChannel, lightOcclusionChannel);
            if (lightData.supportsLightLayers) cmd.SetGlobalInt(LightConstantBuffer._MainLightLayerMask, (int)lightLayerMask);
        }
        ```
      - `void SetupAdditionalLightConstants(cmd, lightData)`
        ```csharp
        void SetupAdditionalLightConstants(UnsafeCommandBuffer cmd, UniversalLightData lightData)
        {
            if (lightData.additionalLightsCount > 0)
            {
                if (m_UseStructuredBuffer)
                {
                    //『現在は`false`なので中身cull。codex→5.6Sol:「StructuredBuffer 自体に対応しているかどうか」ではなく、
                        //『プラットフォームごとの性能・Vulkan のバインディング問題・D3D のシェーダー分岐問題があるので、今は一律 false にして UBO 系の経路を使っている
                }
                else
                {
                    for (int i = 0, lightIter = 0; i < lightData.visibleLights.Length && lightIter < UniversalRenderPipeline.maxVisibleAdditionalLights/*256*/; ++i)
                    {
                        if (lightData.mainLightIndex != i)
                        {
                            InitializeLightConstants //『中身はcull。見たかったらその時に参照すれば良い
                            (
                                lightData.visibleLights,
                                i,
                                lightData.supportsLightLayers,
                                out m_AdditionalLightPositions[lightIter], //『配列も`UniversalRenderPipeline.maxVisibleAdditionalLights/*256*/`で確保されている
                                out m_AdditionalLightColors[lightIter],
                                out m_AdditionalLightAttenuations[lightIter],
                                out m_AdditionalLightSpotDirections[lightIter],
                                out m_AdditionalLightOcclusionProbeChannels[lightIter],
                                out uint lightLayerMask,
                                out var isSubtractive
                            );
                            if (lightData.supportsLightLayers) m_AdditionalLightsLayerMasks[lightIter] = math.asfloat(lightLayerMask);
                            m_AdditionalLightColors[lightIter].w = isSubtractive ? 1f : 0f;
                            lightIter++;
                        }
                    }

                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition, m_AdditionalLightPositions);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor, m_AdditionalLightColors);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsAttenuation, m_AdditionalLightAttenuations);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsSpotDir, m_AdditionalLightSpotDirections);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightOcclusionProbeChannel, m_AdditionalLightOcclusionProbeChannels);
                    if (lightData.supportsLightLayers) cmd.SetGlobalFloatArray(LightConstantBuffer._AdditionalLightsLayerMasks, m_AdditionalLightsLayerMasks);
                }
            }
        }
        ```
  - `RequireResults` **CreateCameraRenderTargets**`(renderGraph, cameraData, frameData.Get<UniversalPostProcessingData>().isEnabled)`: 自作の外部関数化コード
    ```csharp (UniversalRendererRenderGraph.cs) (images\URPレンダリングフローまとめ\まとめ1\CreateCameraRenderTargets\CreateCameraRenderTargets.png)
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
            bool isNotMSAA = cameraData.cameraTargetDescriptor.msaaSamples == 1; //『この条件を外したい。問題ない気がする..
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
        ```csharp (UniversalRendererRenderGraph.cs) (images\URPレンダリングフローまとめ\まとめ1\CreateCameraRenderTargets\CreateRenderGraphCameraRenderTargets.png)
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
                frameData.Get<UniversalResourceData>().SwitchActiveTexturesToBackbuffer(); //『active⟪Color¦Depth⟫ID = UniversalResourceData.ActiveID.BackBuffer; (`TextureHandle active⟪Color¦Depth⟫Texture`の`switch(.active～ID)`で使用)
            }
            //『`Create～Texture(..)` (大体`format`を変えてるだけ)
            CreateCameraDepthCopyTexture(renderGraph, cameraDescriptor, prepassToCameraDepthTexture, clearCameraParams.clearValue); //『`.cameraDepthTexture`へ`cameraDescriptor`を素に`Create～()`(⟪レンダリング先(デプス)¦コピー先(R32)⟫)
            CreateCameraNormalsTexture(renderGraph, cameraDescriptor); //『`.cameraNormalsTexture`へ`cameraDescriptor`を素に`Create～()`
            CreateMotionVectorTextures(renderGraph, cameraDescriptor); //『`.motionVector⟪Color¦Depth⟫`へ`cameraDescriptor`を素に`Create～()`
            CreateRenderingLayersTexture(renderGraph, cameraDescriptor); //『`.renderingLayersTexture`へ`cameraDescriptor`を素に`Create～()`(`m_RequiresRenderingLayer`で要求されているとき)
            if (cameraData.isHDROutputActive && cameraData.rendersOverlayUI) CreateOffscreenUITexture(renderGraph, cameraDescriptor);
        }
        ```
      - 構成:`resourceData.active⟪Color¦Depth⟫Texture`
        - `resourceData.activeColorTexture` (`.SwitchActiveTexturesToBackbuffer()`で`BackBuffer`切替)
          - `resourceData.backBufferColor` (`.activeColorID = URD.ActiveID.BackBuffer`)
            - `cameraData.targetTexture`
            - `BRTT.CameraTarget` (↑`null`時)
          - `resourceData.cameraColor` (`.activeColorID = URD.ActiveID.Camera`)
            - シングルカメラ
              - `CreateRenderGraphTexture(.., _SingleCameraTargetAttachmentName, ..)`
            - カメラスタッキング
              - `RenderingUtils.ReAllocateHandleIfNeeded(.., _CameraTargetAttachmentAName)`
              - `RenderingUtils.ReAllocateHandleIfNeeded(.., _CameraTargetAttachmentBName)`
        - `resourceData.activeDepthTexture`
          - `resourceData.backBufferDepth` (`.activeDepthID = URD.ActiveID.BackBuffer`)
            - `cameraData.targetTexture`
            - `BRTT.Depth` (↑`null`時)
          - `resourceData.cameraDepth` (`.activeDepthID = URD.ActiveID.Camera`)
            - シングルカメラ
              - `CreateRenderGraphTexture(.., _CameraDepthAttachmentName, ..)`
            - カメラスタッキング
              - `RenderingUtils.ReAllocateHandleIfNeeded(.., _CameraDepthAttachmentName)`
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
                //『環境がDirectX系であるとき、`.TopLeft`(BRTT.⟪CameraTarget¦Depth⟫)は標準の座標系であり、スクリーンに映しても反転していない`textureHandle`。に対して`.BottomLeft`(アセットのテクスチャ,中間RT)はDirectX系では反転した`textureHandle`。
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
                    //『`static RTHandle[] s_RenderGraphCameraColorHandles = new RTHandle[]{null, null};`。(>Post Processing などで同じターゲットを読み書きできない場合に`nextRenderGraphCameraColorHandle`が呼ばれ、A/B が切り替わります)
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
          - `CreateCameraNormalsTexture(renderGraph, cameraDescriptor)`: `.cameraNormalsTexture`へ`cameraDescriptor`から`Create～()`
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
  - `void` **RecordCustomRenderGraphPasses**`(renderGraph, RenderPassEvent.BeforeRendering)`
    ```csharp (関数圧縮:https://chatgpt.com/c/6a94ec55-3984-83ee-bb66-9a9bc2fb3209) (images\URPレンダリングフローまとめ\まとめ1\RecordCustomRenderGraphPasses.png)
    internal void RecordCustomRenderGraphPasses(
        RenderGraph renderGraph,
        RenderPassEvent startInjectionPoint,
        RenderPassEvent? endInjectionPoint = null)
    {
        RenderPassEvent end = endInjectionPoint ?? startInjectionPoint;
        int range = ScriptableRenderPass.GetRenderPassEventRange(end);
        RenderPassEvent eventEnd = end + range;

        foreach (ScriptableRenderPass pass in m_ActiveRenderPassQueue)
        {
            if (pass.renderPassEvent >= startInjectionPoint &&
                pass.renderPassEvent < eventEnd)
            {
                pass.RecordRenderGraph(renderGraph, m_frameData);
            }
        }
    }
    ```
  - `void` **SetupRenderGraphCameraProperties**`(renderGraph, activeTargetForIsYFlipped)`
    ```csharp (images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\Input_hlsl.png)
    internal void SetupRenderGraphCameraProperties(RenderGraph renderGraph, TextureHandle targetForIsYFlipped)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(Profiling.setupCamera.name, out var passData,
            Profiling.setupCamera))
        {
            passData.renderer = this;
            passData.cameraData = frameData.Get<UniversalCameraData>();
            passData.cameraTargetSizeCopy = new Vector2Int(passData.cameraData.cameraTargetDescriptor.width, passData.cameraData.cameraTargetDescriptor.height);
            passData.targetForIsYFlipped = targetForIsYFlipped;

            builder.AllowGlobalStateModification(true);

            builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
            {
                bool isTargetYFlipped = SystemInfo.graphicsUVStartsAtTop && RenderingUtils.IsHandleYFlipped(context, in data.targetForIsYFlipped);

                if (data.cameraData.renderType == CameraRenderType.Base)
                {
                    context.cmd.SetupCameraProperties(data.cameraData.camera);
                    data.renderer.SetPerCameraShaderVariables(context.cmd, data.cameraData, data.cameraTargetSizeCopy, isTargetYFlipped);
                }
                else //『`overlayCamera`は、`cmd.SetupCameraProperties(..)`を使わずに設定可能。
                {
                    data.renderer.SetPerCameraShaderVariables(context.cmd, data.cameraData, data.cameraTargetSizeCopy, isTargetYFlipped);
                    data.renderer.SetPerCameraClippingPlaneProperties(context.cmd, in data.cameraData, isTargetYFlipped);
                    data.renderer.SetPerCameraBillboardProperties(context.cmd, data.cameraData);
                }

                // `SetupCameraProperties(..)`で上書きされたシェーダー時間変数をリセットします。これを行わないと、シャドウとメインレンダリングの間で不一致が発生する可能性があります。
                SetShaderTimeValues(context.cmd, Time.time, Time.deltaTime, Time.smoothDeltaTime); //『`Time.time`は、エディターで`Application.isPlaying==false`ならば`Time.realtimeSinceStartup`を使う
            });
        }
    }
    ```
  - `void ProcessVFXCameraCommand(renderGraph)`: (images\URPレンダリングフローまとめ\まとめ1\ProcessVFXCameraCommand.png) (実際のエフェクト描画は`OnMainRendering(..)`の`cmd.DrawRendererList(rendererList)`)
    - `void context.cmd.SetupCameraProperties(data.cameraData.camera)`: (classモジュール\images\SetupCameraProperties.png)
    - `void data.renderer.`**SetPerCameraShaderVariables**`(context.cmd, data.cameraData, data.cameraTargetSizeCopy, isTargetYFlipped)`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\SetPerCameraShaderVariables.png)
        void SetPerCameraShaderVariables(RasterCommandBuffer cmd, UniversalCameraData cameraData, Vector2Int cameraTargetSizeCopy, bool isTargetYFlipped)
        {
            using var profScope = new ProfilingScope(Profiling.setPerCameraShaderVariables);

            Camera camera = cameraData.camera;

            //『カメラターゲットサイズ-------------(`cameraTargetSizeCopy.x = cameraData.scaledWidth = baseCamera.pixelWidth * cameraData.renderScale`)
            float scaledCameraTargetWidth = (float)cameraTargetSizeCopy.x * (camera.allowDynamicResolution ? ScalableBufferManager.widthScaleFactor : 1.0f);
            float scaledCameraTargetHeight = (float)cameraTargetSizeCopy.y * (camera.allowDynamicResolution ? ScalableBufferManager.heightScaleFactor : 1.0f);
            // オーバーレイカメラはビューポートを持ちません。カメラのビューポートではなく、計算済み/継承されたビューポートを使用する必要があります。
                //『分岐削除(`overlayCamera`も`baseCamera`の値を使う)
            float cameraWidth = (float)cameraData.pixelWidth; //『(`baseCamera.pixelWidth`)
            float cameraHeight = (float)cameraData.pixelHeight;

            //『`camera`パラメータ---------------------------------------------------------------------------------
            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;
            float invNear = Mathf.Approximately(near, 0.0f) ? 0.0f : 1.0f / near;
            float invFar = Mathf.Approximately(far, 0.0f) ? 0.0f : 1.0f / far;
            float isOrthographic = camera.orthographic ? 1.0f : 0.0f;

            //『各種GlobalPropertyを設定================================================================================
            //『Cameraパラメータ===============----------------------------------
            if (cameraData.renderType == CameraRenderType.Overlay)
            {
                // `projectionFlipSign`は GfxDevice::SetInvertProjectionMatrix の深部にあり、これは`overlayCamera`のゲームビュー向け。それ以外は`cmd.SetupCameraProperties(..)`で正しく設定される。
                float projectionFlipSign = isTargetYFlipped ? -1.0f : 1.0f;
                Vector4 projectionParams = new Vector4(projectionFlipSign, near, far, 1.0f * invFar);
                /*☆*/cmd.SetGlobalVector(ShaderPropertyId.projectionParams, projectionParams);
            }
            // https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html に記載されているCameraとScreenの変数 (images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\UnityShaderVariables.png)
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, cameraData.worldSpaceCameraPos);
            cmd.SetGlobalVector(ShaderPropertyId.zBufferParams, GetZBufferParams(far, invNear, invFar));
            cmd.SetGlobalVector(ShaderPropertyId.orthoParams, new Vector4(camera.orthographicSize * cameraData.aspectRatio, camera.orthographicSize, 0.0f, isOrthographic));

            //『Screenサイズ===============----------------------------------
            cmd.SetGlobalVector(ShaderPropertyId.screenParams, new Vector4(cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));
            //『`_ScaledScreenParams`,`_ScreenSize`は、`cameraData.cameraTargetDescriptor.width/height`を使用。(`camera.allowDynamicResolution==true`の場合に`ScalableBufferManager.width/heightScaleFactor`を乗算)
            cmd.SetGlobalVector(ShaderPropertyId.scaledScreenParams, new Vector4(scaledCameraTargetWidth, scaledCameraTargetHeight, 1.0f + 1.0f / scaledCameraTargetWidth, 1.0f + 1.0f / scaledCameraTargetHeight));
            /*☆*/cmd.SetGlobalVector(ShaderPropertyId.screenSize, new Vector4(scaledCameraTargetWidth, scaledCameraTargetHeight, 1.0f / scaledCameraTargetWidth, 1.0f / scaledCameraTargetHeight));
            //『ScreenCoordOverride----------------------------------
            cmd.SetKeyword(ShaderGlobalKeywords.SCREEN_COORD_OVERRIDE, cameraData.useScreenCoordOverride); //『(images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\SCREEN_COORD_OVERRIDE.png)
            cmd.SetGlobalVector(ShaderPropertyId.screenSizeOverride, cameraData.screenSizeOverride);
            cmd.SetGlobalVector(ShaderPropertyId.screenCoordScaleBias, cameraData.screenCoordScaleBias);
            //『_RTHandleScale----------------------------------
            //『現状この箇所では固定値 // TODO(@sandy-carter): 動的スケーリングの準備ができたら RTHandles.rtHandleProperties.rtHandleScale に設定する
            cmd.SetGlobalVector(ShaderPropertyId.rtHandleScale, Vector4.one);

            //『☆_GlobalMipBias===============----------------------------------
            //『5.6Sol:現在のレンダリング解像度ではなく、最終的な画像解像度に基づいてmipLvを補正する (`scaledCameraTargetWidth / cameraWidth`は単位変換)
            // ダウンサンプリング時に画像のディテールを減らしてしまわないよう、この値を 0.0 以下にクランプ(Math.Min(., 0.0f))します。
            float mipBias = Math.Min((float)Math.Log(scaledCameraTargetWidth / cameraWidth, 2.0f), 0.0f);
            float taaMipBias = Math.Min(cameraData.taaSettings.mipBias, 0.0f); mipBias = Math.Min(mipBias, taaMipBias); // Temporal Anti-aliasing (気にしなくてよい)
            cmd.SetGlobalVector(ShaderPropertyId.globalMipBias, new Vector2(mipBias, Mathf.Pow(2.0f, mipBias)));

            //『SetCameraMatrices===============----------------------------------
            SetCameraMatrices(cmd, cameraData, isTargetYFlipped);
        }
        //『(images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\ZBufferParams.png)
        static Vector4 GetZBufferParams(float far, float invNear, float invFar)
        {
            float zc0 = 1.0f - far * invNear;
            float zc1 = far * invNear;
            Vector4 zBufferParams = new Vector4(zc0, zc1, zc0 * invFar, zc1 * invFar);
            if (SystemInfo.usesReversedZBuffer)
            {
                zBufferParams.y += zBufferParams.x;
                zBufferParams.x = -zBufferParams.x;
                zBufferParams.w += zBufferParams.z;
                zBufferParams.z = -zBufferParams.z;
            }
            return zBufferParams;
        }
        ```
      - `static void` **SetCameraMatrices**`(cmd, cameraData, isTargetYFlipped)`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\SetCameraMatrices.png)
        static void SetCameraMatrices(RasterCommandBuffer cmd, UniversalCameraData cameraData, bool isTargetYFlipped)
        {
            //『⟪V¦P¦VP⟫------------------------
            // 注意: URP のデフォルトのメイン ビュー/プロジェクション 行列は CameraData のビュー/プロジェクション行列です。
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
            Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(); // ジッター適用済み、非 GPU
            // デフォルトのビュー/プロジェクションを設定します。注意: projectionMatrix はレンダリング用に GPU プロジェクション (gfx API 調整済み) として設定されます。
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix); //『**全ての順方向Matrix**を設定 (`glstate_matrix_projection`,`unity_Matrix⟪V¦VP⟫`)

            //InverseMatrices====================================================

            //『Camera版はZ反転を適用している------------------------
            Matrix4x4 worldToCameraMatrix = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f)) * viewMatrix;
            Matrix4x4 cameraToWorldMatrix = worldToCameraMatrix.inverse;
            cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, worldToCameraMatrix);
            cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, cameraToWorldMatrix);
            //『❰Inv❱⟪V¦P¦VP⟫------------------------
            // TODO: ターゲット反転ロジックの分岐が異なるため、`inverseProjectionMatrix`は、実際のPの`glstate_matrix_projection`のInverseではない。つまり、`invP*P==I`と一致しない可能性があります。
            Matrix4x4 gpuProjectionMatrix = cameraData.GetGPUProjectionMatrix(isTargetYFlipped);
            Matrix4x4 inverseViewMatrix = Matrix4x4.Inverse(viewMatrix);
            Matrix4x4 inverseProjectionMatrix = Matrix4x4.Inverse(gpuProjectionMatrix);
            Matrix4x4 inverseViewProjection = inverseViewMatrix * inverseProjectionMatrix; //『これらを↓に移動
            cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, inverseViewMatrix);
            cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, inverseProjectionMatrix);
            cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, inverseViewProjection);

            // TODO: SetViewAndProjectionMatrices が y 反転 / ワインディング順の問題を引き起こす理由を調査する。当面は cmd.SetViewProjectionMatrices を使用する
            //SetViewAndProjectionMatrices(cmd, viewMatrix, cameraData.GetDeviceProjectionMatrix()); //『将来的にこれ一括で＠❰Inv❱⟪V¦P¦VP⟫を設定する予定?
            // TODO: オーバーレイカメラでしばらく正しく動作することを確認できたら、ここに SetPerCameraClippingPlaneProperties を追加する
        }
        ```
    - `void data.renderer.SetPerCameraClippingPlaneProperties(context.cmd, in data.cameraData, isTargetYFlipped)`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\SetPerCameraClippingPlaneProperties.png)
        private void SetPerCameraClippingPlaneProperties(RasterCommandBuffer cmd, in UniversalCameraData cameraData, bool isTargetYFlipped)
        {
            Matrix4x4 projectionMatrix = cameraData.GetGPUProjectionMatrix(isTargetYFlipped);
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();

            Matrix4x4 viewProj = CoreMatrixUtils.MultiplyProjectionMatrix(projectionMatrix, viewMatrix, cameraData.camera.orthographic);
            Plane[] planes = s_Planes;
            GeometryUtility.CalculateFrustumPlanes(viewProj, planes); //『=>extern

            Vector4[] cameraWorldClipPlanes = s_VectorPlanes;
            for (int i = 0; i < planes.Length; ++i) //『6面分
                cameraWorldClipPlanes[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);

            cmd.SetGlobalVectorArray(ShaderPropertyId.cameraWorldClipPlanes, cameraWorldClipPlanes);
        }
        ```
    - `void data.renderer.SetPerCameraBillboardProperties(context.cmd, data.cameraData)`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\SetupRenderGraphCameraProperties\SetPerCameraBillboardProperties.png)
        void SetPerCameraBillboardProperties(RasterCommandBuffer cmd, UniversalCameraData cameraData)
        {
            cmd.SetKeyword(ShaderGlobalKeywords.BillboardFaceCameraPos, QualitySettings.billboardsFaceCameraPosition);

            CalculateBillboardProperties(cameraData.GetViewMatrix(), out Vector3 billboardTangent, out Vector3 billboardNormal, out float cameraXZAngle); //『C#実装あり

            cmd.SetGlobalVector(ShaderPropertyId.billboardNormal, new Vector4(billboardNormal.x, billboardNormal.y, billboardNormal.z, 0.0f));
            cmd.SetGlobalVector(ShaderPropertyId.billboardTangent, new Vector4(billboardTangent.x, billboardTangent.y, billboardTangent.z, 0.0f)); //『多分TBN行列のようなものを作る
            Vector3 cameraPos = cameraData.worldSpaceCameraPos;
            cmd.SetGlobalVector(ShaderPropertyId.billboardCameraParams, new Vector4(cameraPos.x, cameraPos.y, cameraPos.z, cameraXZAngle)); //『`cameraXZAngle`によって画像を差し替える
        }
        ```
  - `void OnBeforeRendering(renderGraph)`: (images\URPレンダリングフローまとめ\まとめ1\OnBeforeRendering.png)
    - `void m_ForwardLights.`**PreSetup**`(renderingData, cameraData, lightData)`
        ```csharp
        void PreSetup(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
        {
            //『`NativeArray`と`GraphicsBuffer`を初期化、cameraの○⟦と ┃⟪`View¦`Projection⟫Matrix`⟧を設定して`ScheduleClusteringJobs(..)`を呼ぶだけ
            if (m_UseForwardPlus)
            {
                using var _ = new ProfilingScope(m_ProfilingSamplerFPSetup);

                if (!m_CullingHandle.IsCompleted)
                {
                    throw new InvalidOperationException("Forward+ のジョブがまだ完了していません。");
                }

                if (m_TileMasks.Length != UniversalRenderPipeline.maxTileWords)
                {
                    m_ZBins.Dispose();
                    m_ZBinsBuffer.Dispose();
                    m_TileMasks.Dispose();
                    m_TileMasksBuffer.Dispose();
                    //『CreateForwardPlusBuffers();
                    m_ZBins = new NativeArray<uint>(UniversalRenderPipeline.maxZBinWords, Allocator.Persistent);
                    m_ZBinsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, UniversalRenderPipeline.maxZBinWords / 4, UnsafeUtility.SizeOf<float4>());
                    m_ZBinsBuffer.name = "URP Z-Bin Buffer";
                    m_TileMasks = new NativeArray<uint>(UniversalRenderPipeline.maxTileWords, Allocator.Persistent);
                    m_TileMasksBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, UniversalRenderPipeline.maxTileWords / 4, UnsafeUtility.SizeOf<float4>());
                    m_TileMasksBuffer.name = "URP Tile Buffer";
                }
                else
                {
                    unsafe
                    {
                        //『`NativeArray`をゼロ初期化
                        UnsafeUtility.MemClear(m_ZBins.GetUnsafePtr(), m_ZBins.Length * sizeof(uint));
                        UnsafeUtility.MemClear(m_TileMasks.GetUnsafePtr(), m_TileMasks.Length * sizeof(uint));
                    }
                }

                var viewCount = 1;
                var worldToViews = new Fixed2<float4x4>(cameraData.GetViewMatrix(0), cameraData.GetViewMatrix(0)); //『←↓`[0]`しか参照しない
                var viewToClips = new Fixed2<float4x4>(cameraData.GetProjectionMatrix(0), cameraData.GetProjectionMatrix(0));

                m_CullingHandle = ScheduleClusteringJobs(
                    lightData.mainLightIndex != -1,
                    lightData.supportsAdditionalLights,
                    lightData.visibleLights,
                    renderingData.cullResults.visibleReflectionProbes,
                    m_ZBins,
                    m_TileMasks,
                    worldToViews,
                    viewToClips,
                    viewCount,
                    math.int2(cameraData.pixelWidth, cameraData.pixelHeight),
                    cameraData.camera.nearClipPlane,
                    cameraData.camera.farClipPlane,
                    cameraData.camera.orthographic,
                    out m_LightCount,
                    out m_DirectionalLightCount,
                    out m_BinCount,
                    out m_ZBinScale,
                    out m_ZBinOffset,
                    out m_TileResolution,
                    out m_ActualTileWidth,
                    out m_WordsPerTile
                );

                JobHandle.ScheduleBatchedJobs();
            }
        }
        ```
      - `static JobHandle ScheduleClusteringJobs(lightData.mainLightIndex != -1, lightData.supportsAdditionalLights, lightData.visibleLights, renderingData.cullResults.visibleReflectionProbes, m_ZBins, m_TileMasks, worldToViews, viewToClips, viewCount, math.int2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.camera.nearClipPlane, cameraData.camera.farClipPlane, cameraData.camera.orthographic, out m_LightCount, out m_DirectionalLightCount, out m_BinCount, out m_ZBinScale, out m_ZBinOffset, out m_TileResolution, out m_ActualTileWidth, out m_WordsPerTile);`
        ```csharp (images\URPレンダリングフローまとめ\まとめ1\ForwardLights\Forward+機構\Forward+機構0.png)
        static JobHandle ScheduleClusteringJobs(
            bool hasMainLight,
            bool supportsAdditionalLights,
            NativeArray<VisibleLight> lights,
            NativeArray<VisibleReflectionProbe> probes,
            NativeArray<uint> zBins,
            NativeArray<uint> tileMasks,
            Fixed2<float4x4> worldToViews,
            Fixed2<float4x4> viewToClips,
            int viewCount,
            int2 screenResolution,
            float nearClipPlane,
            float farClipPlane,
            bool isOrthographic,
            out int localLightCount,
            out int directionalLightCount,
            out int binCount,
            out float zBinScale,
            out float zBinOffset,
            out int2 tileResolution,
            out int actualTileWidth,
            out int wordsPerTile
        )
        {
            localLightCount = supportsAdditionalLights ? lights.Length: 0;
            // lights 配列には、最初に Directional Light、その後にローカルライトが格納されています。
            // リストを走査して、最初のローカルライトのインデックスを探します。//『(`Forward+機構1.png`参照)
            var firstLocalLightIdx = 0;
            while (firstLocalLightIdx < localLightCount && lights[firstLocalLightIdx].lightType == LightType.Directional)
            {
                firstLocalLightIdx++;
            }
            localLightCount -= firstLocalLightIdx;

            // Directional Light が 1 つ以上ある場合、そのうちの 1 つがメインライトである可能性があります。
            if (firstLocalLightIdx > 0)
            {

                directionalLightCount = firstLocalLightIdx;
                if (hasMainLight)
                    directionalLightCount -= 1;
            }
            else
            {
                directionalLightCount = 0;
            }

            var localLights = lights.GetSubArray(firstLocalLightIdx, localLightCount); //『directionalを除いた`NativeArray<VisibleLight>`

            var reflectionProbeCount = math.min(probes.Length, UniversalRenderPipeline.maxVisibleReflectionProbes);
            // テクスチャのない Reflection Probe が使用されないようにします。
            for (var i = 0; i < probes.Length; i++)
            {
                if (!probes[i].texture)
                    reflectionProbeCount--;
            }

            var itemsPerTile = localLights.Length + reflectionProbeCount; //『⟪1Tile¦1bin⟫の`localLights + probes`数(全ての⟪1Tile¦1bin⟫に全てのitemを入れる)
            wordsPerTile = (itemsPerTile + 31) / 32; //『⟪1Tile¦1bin⟫のuint(32,words)数

            actualTileWidth = 8 >> 1;
            do //『`.maxTileWords`に収まるようにTileサイズ(`actualTileWidth`)を2の累乗で大きくしていき`screenResolution`から`tileResolution`を決定する
            {
                actualTileWidth <<= 1;
                tileResolution = (screenResolution + actualTileWidth - 1) / actualTileWidth;
            }
            while ((tileResolution.x * tileResolution.y * wordsPerTile * viewCount) > UniversalRenderPipeline.maxTileWords);

            if (!isOrthographic) //『シェーダーのビュー空間で`log2(viewZ)`から`binIndex`を計算するための係数群。(`Forward+機構2.png`参照)
            {
                //『`float viewZ = dot(GetViewForwardDir(), positionWS - GetCameraPositionWS())`: ビューベクトルからカメラフォワードへの射影
                // binIndex = log2(viewZ) * zBinScale + zBinOffset の計算に使用します。
                zBinScale = (UniversalRenderPipeline.maxZBinWords / viewCount) / ((math.log2(farClipPlane) - math.log2(nearClipPlane)) * (2 + wordsPerTile));
                zBinOffset = -math.log2(nearClipPlane) * zBinScale;
                binCount = (int)(math.log2(farClipPlane) * zBinScale + zBinOffset);
            }
            else
            {
                // binIndex = z * zBinScale + zBinOffset の計算に使用します。
                zBinScale = (UniversalRenderPipeline.maxZBinWords / viewCount) / ((farClipPlane - nearClipPlane) * (2 + wordsPerTile));
                zBinOffset = -nearClipPlane * zBinScale;
                binCount = (int)(farClipPlane * zBinScale + zBinOffset);
            }

            // エディターで farClipPlane が Infinity に設定されたとき、ビン数が負になるのを防ぐために必要です。
            binCount = Math.Max(binCount, 0);

            // probe を otherProbe より後に配置すべきか判定します。
            static bool IsProbeGreater(VisibleReflectionProbe probe, VisibleReflectionProbe otherProbe)
            {
                return otherProbe.texture != null && (probe.texture == null || probe.importance < otherProbe.importance ||
                    (probe.importance == otherProbe.importance && probe.bounds.extents.sqrMagnitude > otherProbe.bounds.extents.sqrMagnitude));
            }

            // 関連度の高いプローブが使用されるよう、probes.Length を使って確認します。//『優先順位順に並び替えるだけ
            for (var i = 1; i < probes.Length; i++)
            {
                var probe = probes[i];
                var j = i - 1;
                while (j >= 0 && IsProbeGreater(probes[j], probe))
                {
                    probes[j + 1] = probes[j];
                    j--;
                }

                probes[j + 1] = probe;
            }

            var minMaxZs = new NativeArray<float2>(itemsPerTile * viewCount, Allocator.TempJob);

            //『Codex: 各 item がどの range に入るかを決める幾何学的な判定は、ビュー空間またはビュー空間から投影した座標を基準にしています。

            //『各`localLight`のz軸方向の最小(min)と最大(max)を記録する(float2)。(`Forward+機構3.png`参照)
            var lightMinMaxZJob = new LightMinMaxZJob
            {
                worldToViews = worldToViews,
                lights = localLights,
                minMaxZs = minMaxZs.GetSubArray(0, localLightCount * viewCount)
            };
            // 内部ループのバッチ数 32 に特別な意味はありません。スケジューリングのオーバーヘッドが大きくなりすぎず、並列度も低くなりすぎないようにした、おおよその値です。
            var lightMinMaxZHandle = lightMinMaxZJob.ScheduleParallel(localLightCount * viewCount, 32, new JobHandle());

            var reflectionProbeRotation = GraphicsSettings.TryGetRenderPipelineSettings<URPReflectionProbeSettings>(out var reflectionProbeSettings) ? reflectionProbeSettings.UseReflectionProbeRotation : true;

            //『各`reflectionProbe`のz軸方向の最小(min)と最大(max)を記録する(float2)。(`Forward+機構3.png`参照)
            var reflectionProbeMinMaxZJob = new ReflectionProbeMinMaxZJob
            {
                worldToViews = worldToViews,
                reflectionProbes = probes,
                reflectionProbeRotation = reflectionProbeRotation,
                minMaxZs = minMaxZs.GetSubArray(localLightCount * viewCount, reflectionProbeCount * viewCount)
            };
            var reflectionProbeMinMaxZHandle = reflectionProbeMinMaxZJob.ScheduleParallel(reflectionProbeCount * viewCount, 32, lightMinMaxZHandle);


            var zBinningBatchCount = (binCount + ZBinningJob.batchSize - 1) / ZBinningJob.batchSize;
            //『記録した`minMaxZs`から係数(`zBin⟪Scale¦Offset⟫`)を使ってindexを算出して`bins`にそのitemのビットを立てる。(`Forward+機構4.png`参照)
            var zBinningJob = new ZBinningJob
            {
                bins = zBins,
                minMaxZs = minMaxZs,
                zBinScale = zBinScale,
                zBinOffset = zBinOffset,
                binCount = binCount,
                wordsPerTile = wordsPerTile,
                lightCount = localLightCount,
                reflectionProbeCount = reflectionProbeCount,
                batchCount = zBinningBatchCount,
                viewCount = viewCount,
                isOrthographic = isOrthographic
            };
            var zBinningHandle = zBinningJob.ScheduleParallel(zBinningBatchCount * viewCount, 1, reflectionProbeMinMaxZHandle);

            reflectionProbeMinMaxZHandle.Complete(); //『なぜかz軸方向(`minMaxZs`)だけ先に`.Complete()`される。(`items`を変更されたくないなら`TilingJob`(`tileRanges`)も同じなはず)

            GetViewParams(isOrthographic, viewToClips[0], out float viewPlaneBottom0, out float viewPlaneTop0, out float4 viewToViewportScaleBias0);
            GetViewParams(isOrthographic, viewToClips[1], out float viewPlaneBottom1, out float viewPlaneTop1, out float4 viewToViewportScaleBias1);

            // 各ライトには Y 用の範囲が 1 つと、行ごとの範囲が必要です。偽共有を避けるため、128 バイト境界に揃えます。
            var rangesPerItem = AlignByteCount((1 + tileResolution.y) * UnsafeUtility.SizeOf<InclusiveRange>(), 128) / UnsafeUtility.SizeOf<InclusiveRange>();
            var tileRanges = new NativeArray<InclusiveRange>(rangesPerItem * itemsPerTile * viewCount, Allocator.TempJob);
            //『各itemからタイルx影響範囲をタイルy毎に`tileRanges`に記録する。(`Forward+機構5.png`参照)
            var tilingJob = new TilingJob
            {
                lights = localLights,
                reflectionProbes = probes,
                reflectionProbeRotation = reflectionProbeRotation,
                tileRanges = tileRanges,
                itemsPerTile = itemsPerTile,
                rangesPerItem = rangesPerItem,
                worldToViews = worldToViews,
                tileScale = (float2)screenResolution / actualTileWidth,
                tileScaleInv = actualTileWidth / (float2)screenResolution,
                viewPlaneBottoms = new Fixed2<float>(viewPlaneBottom0, viewPlaneBottom1),
                viewPlaneTops = new Fixed2<float>(viewPlaneTop0, viewPlaneTop1),
                viewToViewportScaleBiases = new Fixed2<float4>(viewToViewportScaleBias0, viewToViewportScaleBias1),
                tileCount = tileResolution,
                near = nearClipPlane,
                isOrthographic = isOrthographic
            };

            var tileRangeHandle = tilingJob.ScheduleParallel(itemsPerTile * viewCount, 1, reflectionProbeMinMaxZHandle);

            //『記録した`tileRanges`からタイル毎にそのitemのビットを`tileMasks`に立てる。(`Forward+機構6.png`参照)
            var expansionJob = new TileRangeExpansionJob
            {
                tileRanges = tileRanges,
                tileMasks = tileMasks,
                rangesPerItem = rangesPerItem,
                itemsPerTile = itemsPerTile,
                wordsPerTile = wordsPerTile,
                tileResolution = tileResolution,
            };

            var tilingHandle = expansionJob.ScheduleParallel(tileResolution.y * viewCount, 1, tileRangeHandle);
            JobHandle cullingHandle = JobHandle.CombineDependencies(
                minMaxZs.Dispose(zBinningHandle),
                tileRanges.Dispose(tilingHandle));
            return cullingHandle;
        }
        ```
