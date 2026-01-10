using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Unity.Mathematics;
using Mono.Cecil;
using UnityEngine.SocialPlatforms;

[CreateAssetMenu(fileName = "RenderGraphTest_PipelineAsset", menuName = "Rendering/RenderGraph Test PipelineAsset")]
public class RenderTextureTestAsset : RenderPipelineAsset<RenderTextureTestAsset.RenderTextureTestPipeline>
{
    [Header("RenderTexture Settings")]
    public TextureDimension dimension = TextureDimension.Tex2D; //％Tex2D
    public GraphicsFormat colorFormat = GraphicsFormat.B8G8R8A8_SRGB;
    public GraphicsFormat depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
    [Range(1, 8)] public int msaaSamples = 1; //％1
    public bool bindMS = false; //％false
    public bool useMipMap = false; //％false //これをtrueにしないとmipMapが生成されない (mipCount >= 2のみではだめ)
    [Range(-1, 10)] public int mipCount = Texture.GenerateAllMips; //％-1
    public bool autoGenerateMips = true; //％true
    [Range(1, 10)]
    public int volumeDepth = 1; //％1 (1以上を設定)
    [Header("SetRenderTarget")]
    [Range(0, 10)]
    public int mipLevel = 0;
    public CubemapFace cubemapFace = CubemapFace.Unknown;
    [Range(0, 10)]
    public int depthSlice = 0;
    [Header("ClearRenderTarget")]
    public RTClearFlags clearFlags = RTClearFlags.All;
    public Color[] backgroundColors = { Color.bisque };
    [Range(0, 1)]
    public float depth = 1;
    [Range(0, 255)]
    public uint stencil = 0;
    [Header("SetGlobalTexture")]
    public Texture sourceTex;
    [Header("mipAndArrayによる上書き")]
    public TextureDimension dimension_ma = TextureDimension.Tex2DArray;
    public bool useMipMap_ma = true;
    [Range(-1, 10)] public int mipCount_ma = Texture.GenerateAllMips;
    public bool autoGenerateMips_ma = false;
    [Range(1, 20)]
    public int volumeDepth_ma = 1;
    [Range(0, 10)]
    public int mipLevel_ma = 0;
    [Range(0, 10)]
    public int depthSlice_ma = 0;
    [Header("デプスバッファの上書き")]
    public bool overrideDepth_d = false;
    public GraphicsFormat colorFormat_d = GraphicsFormat.B8G8R8A8_SRGB;
    public GraphicsFormat depthStencilFormat_d = GraphicsFormat.D24_UNorm_S8_UInt;
    public TextureDimension dimension_d = TextureDimension.Tex2D;
    public int width_d = 512;
    public int height_d = 512;
    public bool useMipMap_d = false;
    [Range(-1, 10)] public int mipCount_d = 1;
    public bool autoGenerateMips_d = true;
    [Range(1, 20)] public int volumeDepth_d = 1;
    public bool gomi = true;
    [Header("Anisotropic Test")]
    public bool drawAnisotropicTest = true;
    [Header("MSAA Test")]
    public bool drawMsaaTest = true;
    public enum MsaaKeyword
    {
        sample_col,
        sample_average_col,
        sample_index_col,
    }
    [Header("MSAA Sample Keyword")]
    public MsaaKeyword msaaKeyword;
    public MSAASamples msaaSamples1 = MSAASamples.None;
    public Vector2Int rtSize = new Vector2Int(16, 16);
    [Header("TRS Parameters_0")]
    public Vector3 position_0 = new Vector3(-0.45f, 0.0f, 0.4f);
    public Vector3 rotation_0 = Vector3.zero;
    public Vector3 scale_0 = Vector3.one * 0.5f;
    [Header("TRS Parameters_1")]
    public Vector3 position_1 = new Vector3(0.45f, 0.0f, 0.6f);
    public Vector3 rotation_1 = Vector3.zero;
    public Vector3 scale_1 = Vector3.one * 0.5f;
    [Header("TRS Parameters_2")]
    public Vector3 position_2 = new Vector3(0.0f, 0.0f, 0.5f);
    public Vector3 rotation_2 = new Vector3(0.0f, 0.0f, 45.0f);
    public Vector3 scale_2 = Vector3.one * 0.5f;
    [Header("RTHandle Test")]
    public bool drawRTHandleTest = true;
    public Vector2Int initialize = new Vector2Int(128, 128);
    public Vector2 scaleFactor_0 = new Vector2(1.0f, 1.0f);
    public bool useScaling_1 = true;
    public Vector2 scaleFactor_1 = new Vector2(1.0f, 1.0f);
    public RTHandleParameters rTHandleParameters;
    void OnEnable()
    {
        // 既存のアセットをロードするか、新しく作成する
        string assetPath = "Assets/Script/RTHandleParameters.asset";
        rTHandleParameters = UnityEditor.AssetDatabase.LoadAssetAtPath<RTHandleParameters>(assetPath);
        
        if (rTHandleParameters == null)
        {
            // アセットが存在しない場合は新しく作成
            rTHandleParameters = CreateInstance<RTHandleParameters>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(rTHandleParameters, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
    [Header("DrawProcedural")]
    public Shader blitShader;
    public Shader layoutBlitShader;
    public Material AnisotropicTestMaterial_0;
    public Material AnisotropicTestMaterial_1;
    public Material msaaTestMaterial;
    protected override RenderPipeline CreatePipeline()
    {
        Debug.Log("====CreatePipeline====");
        return new RenderTextureTestPipeline(this);
    }

    public class RenderTextureTestPipeline : RenderPipeline
    {
        public RenderTextureTestAsset asset;
        public RenderTextureTestPipeline(RenderTextureTestAsset asset)
        {
            this.asset = asset;
        }
        Camera camera;
        RenderTexture customRT;
        RenderTexture mipAndArrayRT;
        RenderTexture mrtRT;
        RenderTexture depthRT;
        RenderTexture msaa_bindMS_false_RT;
        RenderTexture msaa_bindMS_ture_RT;
        RenderTexture msaa_resolve_RT;
        RTHandleSystem rtHandleSystem;
        RTHandle rtHandleColor_0;
        RTHandle rtHandleColor_1;
        RTHandle rtHandleDepth;
        int sourceTex_id = Shader.PropertyToID("_SourceTex");
        int texDrawPoint_id = Shader.PropertyToID("_texDrawPoint");
        int mipLevelAndDepthSlice_id = Shader.PropertyToID("_mipLevelAndDepthSlice");
        int cellSize_id = Shader.PropertyToID("_cellSize");
        int msaaColor_id = Shader.PropertyToID("_msaaColor");
        int myRTHandleScale_id = Shader.PropertyToID("_MyRTHandleScale");
        int viewportSize_id = Shader.PropertyToID("_viewportSize");
        Material blitMaterial;
        Material layoutBlitMaterial;
        Mesh quadMesh = CreateQuadMesh();
        GlobalKeyword cameraTarget_gkey = GlobalKeyword.Create("_CameraTarget");
        GlobalKeyword msaa_gkey = GlobalKeyword.Create("_MSAA");
        GlobalKeyword msaa_sample_gkey = GlobalKeyword.Create("_MSAA_SAMPLE");
        GlobalKeyword sample_col_gkey = GlobalKeyword.Create("_SAMPLE_COL");
        GlobalKeyword sample_index_col_gkey = GlobalKeyword.Create("_SAMPLE_INDEX_COL");
        GlobalKeyword sample_average_col_gkey = GlobalKeyword.Create("_SAMPLE_AVERAGE_COL");
        GlobalKeyword rTHandle_gkey = GlobalKeyword.Create("_RTHandle");
        GlobalKeyword rTHandle_ClampAndScaleUV_gkey = GlobalKeyword.Create("_RTHandle_ClampAndScaleUV");
        int renderLoopCounter = 0;
        protected override void Render(ScriptableRenderContext ctx, List<Camera> _)
        {
            if (renderLoopCounter == 0)
            {
                camera = GameObject.Find("Main Camera").GetComponent<Camera>();

                Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
                RenderTextureDescriptor desc = new RenderTextureDescriptor
                (
                    (int)ScreenSize.x, (int)ScreenSize.y,
                    asset.colorFormat,
                    asset.depthStencilFormat,
                    asset.mipCount
                )
                {
                    dimension = asset.dimension,
                    msaaSamples = asset.msaaSamples,
                    bindMS = asset.bindMS,
                    useMipMap = asset.useMipMap,
                    autoGenerateMips = asset.autoGenerateMips,
                    volumeDepth = asset.volumeDepth,
                };
                RenderTextureDescriptor mipAndArrayDesc = desc;
                mipAndArrayDesc.width = asset.sourceTex.width / 2;
                mipAndArrayDesc.height = asset.sourceTex.height / 2;
                mipAndArrayDesc.useMipMap = asset.useMipMap_ma;
                mipAndArrayDesc.mipCount = asset.mipCount_ma;
                mipAndArrayDesc.autoGenerateMips = asset.autoGenerateMips_ma;
                mipAndArrayDesc.volumeDepth = asset.volumeDepth_ma;
                mipAndArrayDesc.dimension = asset.dimension_ma;
                mipAndArrayDesc.graphicsFormat = GraphicsFormat.R32G32_SFloat;
                mipAndArrayRT = new RenderTexture(mipAndArrayDesc) { name = "mipAndArrayRT" }; //SetName("CustomRT") RenderDocで表示されるリソース名
                Debug.Log($"mipAndArrayRT.volumeDepth: {mipAndArrayRT.volumeDepth}");
                // mipAndArrayRT.Create();
                RenderTextureDescriptor customDesc = desc;
                customDesc.width = mipAndArrayDesc.width;
                customDesc.height = mipAndArrayDesc.height;
                customDesc.graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm;
                customDesc.useMipMap = false;
                // customDesc.msaaSamples = 2;
                customRT = new RenderTexture(customDesc) { name = "customRT" };
                //==
                RenderTextureDescriptor mrtDesc = desc;
                mrtDesc.width = mipAndArrayDesc.width;
                mrtDesc.height = mipAndArrayDesc.height;
                mrtRT = new RenderTexture(mrtDesc) { name = "mrtRT" };
                //==
                RenderTextureDescriptor depthDesc = desc;
                depthDesc.graphicsFormat = asset.colorFormat_d;
                depthDesc.depthStencilFormat = asset.depthStencilFormat_d;
                depthDesc.stencilFormat = GraphicsFormat.R8_UInt; //RenderTextureSubElement.Stencil用
                depthDesc.width = mipAndArrayDesc.width;
                depthDesc.height = mipAndArrayDesc.height;
                // depthDesc.useMipMap = asset.useMipMap_d;
                // depthDesc.mipCount = asset.mipCount_d;
                // depthDesc.autoGenerateMips = asset.autoGenerateMips_d;
                depthDesc.volumeDepth = asset.volumeDepth_d;
                depthDesc.dimension = asset.dimension_d;
                // depthDesc.msaaSamples = 2;
                depthRT = new RenderTexture(depthDesc) { name = "depthRT" };

                //MSAA Test=========================================================
                RenderTextureDescriptor msaa_bindMS_false_Desc = new
                (
                    asset.rtSize.x, asset.rtSize.y,
                    GraphicsFormat.R16G16B16A16_SFloat,
                    GraphicsFormat.D24_UNorm_S8_UInt
                )
                {
                    dimension = TextureDimension.Tex2D,
                    msaaSamples = (int)asset.msaaSamples1,
                    bindMS = false,
                };
                msaa_bindMS_false_RT = new RenderTexture(msaa_bindMS_false_Desc) { name = "msaa_bindMS_false_RT" };
                msaa_bindMS_false_RT.filterMode = FilterMode.Point;
                RenderTextureDescriptor msaa_bindMS_ture_Desc = msaa_bindMS_false_Desc;
                msaa_bindMS_ture_Desc.bindMS = true;
                msaa_bindMS_ture_RT = new RenderTexture(msaa_bindMS_ture_Desc) { name = "msaa_bindMS_ture_RT" };
                msaa_bindMS_ture_RT.filterMode = FilterMode.Point;
                RenderTextureDescriptor msaa_resolve_Desc = msaa_bindMS_false_Desc;
                msaa_resolve_Desc.msaaSamples = 1;
                msaa_resolve_RT = new RenderTexture(msaa_resolve_Desc) { name = "resolveMsaaRT" };
                msaa_resolve_RT.filterMode = FilterMode.Point;
                msaa_resolve_RT.Create(); //Resolve先はCreateが必要

                //RTHandle Test=========================================================
                rtHandleSystem = new RTHandleSystem();
                rtHandleSystem.Initialize(asset.initialize.x, asset.initialize.y);
                // rtHandleSystem.SetReferenceSize(256, 256);
                RTHandleAllocInfo allocColorInfo = new("allocColorInfo")
                {
                    dimension = TextureDimension.Tex2D,
                    format = GraphicsFormat.R16G16B16A16_SFloat,
                    wrapModeU = TextureWrapMode.Clamp,
                    wrapModeV = TextureWrapMode.Clamp,
                    wrapModeW = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    // filterMode = FilterMode.Point,
                };
                RTHandleAllocInfo allocDepthInfo = new("allocColorInfo")
                {
                    dimension = TextureDimension.Tex2D,
                    format = GraphicsFormat.D24_UNorm_S8_UInt,
                    filterMode = FilterMode.Bilinear,
                };
                rtHandleColor_0 = rtHandleSystem.Alloc(
                    scaleFactor: asset.scaleFactor_0,
                    info: allocColorInfo
                );
                rtHandleDepth = rtHandleSystem.Alloc(
                    scaleFactor: asset.scaleFactor_0,
                    info: allocDepthInfo
                );
                if(asset.useScaling_1)
                {
                    rtHandleColor_1 = rtHandleSystem.Alloc(
                        scaleFactor: asset.scaleFactor_1,
                        info: allocColorInfo
                    );
                }
                else
                {
                    rtHandleColor_1 = rtHandleSystem.Alloc(
                        width: asset.initialize.x, height: asset.initialize.y,
                        info: allocColorInfo
                    );
                    //RTHandleラップTest
                    rtHandleColor_1 = rtHandleSystem.Alloc(rtHandleColor_1);
                    rtHandleColor_1 = rtHandleSystem.Alloc(rtHandleColor_1.rt);
                    rtHandleColor_1 = rtHandleSystem.Alloc(rtHandleColor_1.nameID);
                    // rtHandleColor_1 = rtHandleSystem.Alloc(rtHandleColor_1.externalTexture); //ヌルり
                }

                //Material=========================================================
                blitMaterial = new Material(asset.blitShader);
                layoutBlitMaterial = new Material(asset.layoutBlitShader);
            }

            CommandBuffer cmd = CommandBufferPool.Get("cmd");

            // cmd.SetRenderTarget(depthRT, asset.mipLevel_ma, asset.cubemapFace, asset.depthSlice_ma);
            cmd.SetRenderTarget(new RenderTargetIdentifier[] { customRT, mipAndArrayRT, mrtRT }, depthRT, asset.mipLevel_ma, asset.cubemapFace, asset.depthSlice_ma);
            // cmd.GenerateMips(mipAndArrayRT);
            // cmd.SetRenderTarget(mipAndArrayRT, depthRT);

            // if (!asset.overrideDepth_d)
            // {
            //     RenderTargetIdentifier depthRTI = new(mipAndArrayRT.depthBuffer, 0, CubemapFace.Unknown, 4);//恐らく、カラーバッファにしか効かない
            //     cmd.SetRenderTarget(mipAndArrayRT/*.colorBuffer, mipAndArrayRT.depthBuffer depthRTI*/, asset.mipLevel_ma, asset.cubemapFace, asset.depthSlice_ma);
            // }
            // else
            // {
            //     cmd.SetRenderTarget(mipAndArrayRT, depthRT, asset.mipLevel_ma, asset.cubemapFace, asset.depthSlice_ma);
            // }
            cmd.ClearRenderTarget(asset.clearFlags, asset.backgroundColors, asset.depth, asset.stencil);
            // cmd.SetRenderTarget(new RenderTargetIdentifier[2] { default, default }, default, default, default, default);
            cmd.SetGlobalTexture(sourceTex_id, asset.sourceTex);
            cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);

            cmd.SetupCameraProperties(camera); //_ScreenParamsを設定する
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd.ClearRenderTarget(asset.clearFlags, asset.backgroundColors, 1.0f, 0/*asset.depth, asset.stencil*/);

            if (!asset.autoGenerateMips_ma)
            {
                // cmd.GenerateMips(mipAndArrayRT);
            }
            if(!asset.bindMS)
            {
                // cmd.ResolveAntiAliasedSurface(rt);
            }

            cmd.SetGlobalTexture(sourceTex_id, mipAndArrayRT, RenderTextureSubElement.Default); //.Default(カラー):ok //Array:ok
            // cmd.SetGlobalTexture(sourceTex_id, mipAndArrayRT, RenderTextureSubElement.Color); //.Color:ok //Array:ok
            // cmd.SetGlobalTexture(sourceTex_id, mipAndArrayRT, RenderTextureSubElement.Depth); //.Depth:x
            // cmd.SetGlobalTexture(sourceTex_id, depthRT, RenderTextureSubElement.Depth); //.Depth:ok //Array:ok
            // cmd.SetGlobalTexture(sourceTex_id, depthRT, RenderTextureSubElement.Stencil); //.Stencil:x
            // cmd.SetGlobalTexture(sourceTex_id, depthRT, RenderTextureSubElement.Stencil); //depthDesc.stencilFormat = GraphicsFormat.R8_UInt ＆ .Stencil:ok //Array:ok
            // cmd.SetGlobalTexture(sourceTex_id, mipAndArrayRT, RenderTextureSubElement.Default); //.Default(カラー):ok //Array:ok
            // cmd.SetGlobalTexture(sourceTex_id, depthRT, RenderTextureSubElement.Default); //.Default(デプス):ok //Array:ok
            // cmd.SetGlobalTexture(sourceTex_id, new RenderTargetIdentifier(depthRT, 0, CubemapFace.Unknown, 4), RenderTextureSubElement.Default); //.Default(デプス&カラー→カラー):ok //Array:ok //RTIよるサブリソース指定は意味なし
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(200.0f, 100.0f, 0.0f, 0.0f));
            
            cmd.SetGlobalVector(mipLevelAndDepthSlice_id, new Vector4(asset.mipLevel_ma, asset.depthSlice_ma, 0.0f, 0.0f));

            if(asset.gomi) cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);

            if(asset.drawAnisotropicTest) DrawAnisotropicTest(cmd);

            if(asset.drawMsaaTest) DrawMsaaTest(cmd);

            if(asset.drawRTHandleTest) DrawRTHandleTest(cmd);

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            ctx.Submit();

            renderLoopCounter++;
        }
        void DrawAnisotropicTest(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, asset.AnisotropicTestMaterial_0, 0, MeshTopology.Triangles, 3);
            cmd.DrawProcedural(Matrix4x4.identity, asset.AnisotropicTestMaterial_1, 0, MeshTopology.Triangles, 3);
        }
        void DrawMsaaTest(CommandBuffer cmd)
        {
            cmd.SetRenderTarget(msaa_bindMS_false_RT);
            cmd.ClearRenderTarget(true, true, Color.black); //new Color(0f, 0f, 0f, 1f)
            Matrix4x4 modelMatrix = Matrix4x4.TRS(asset.position_0, Quaternion.Euler(asset.rotation_0), asset.scale_0); 
            cmd.SetGlobalVector(msaaColor_id, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            cmd.DrawMesh(quadMesh, modelMatrix, asset.msaaTestMaterial);
            modelMatrix = Matrix4x4.TRS(asset.position_1, Quaternion.Euler(asset.rotation_1), asset.scale_1); 
            cmd.SetGlobalVector(msaaColor_id, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            cmd.DrawMesh(quadMesh, modelMatrix, asset.msaaTestMaterial);
            modelMatrix = Matrix4x4.TRS(asset.position_2, Quaternion.Euler(asset.rotation_2), asset.scale_2); 
            cmd.SetGlobalVector(msaaColor_id, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            cmd.DrawMesh(quadMesh, modelMatrix, asset.msaaTestMaterial);

            cmd.SetRenderTarget(msaa_bindMS_ture_RT);
            cmd.ClearRenderTarget(true, true, Color.black); //new Color(0f, 0f, 0f, 1f)
            modelMatrix = Matrix4x4.TRS(asset.position_0, Quaternion.Euler(asset.rotation_0), asset.scale_0); 
            cmd.SetGlobalVector(msaaColor_id, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            cmd.DrawMesh(quadMesh, modelMatrix, asset.msaaTestMaterial);
            modelMatrix = Matrix4x4.TRS(asset.position_1, Quaternion.Euler(asset.rotation_1), asset.scale_1); 
            cmd.SetGlobalVector(msaaColor_id, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            cmd.DrawMesh(quadMesh, modelMatrix, asset.msaaTestMaterial);
            modelMatrix = Matrix4x4.TRS(asset.position_2, Quaternion.Euler(asset.rotation_2), asset.scale_2); 
            cmd.SetGlobalVector(msaaColor_id, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            cmd.DrawMesh(quadMesh, modelMatrix, asset.msaaTestMaterial);

            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd.SetGlobalVector(cellSize_id, new Vector4(25.0f, 25.0f, 1.0f, 0.0f));

            switch (asset.msaaKeyword)
            {
                case MsaaKeyword.sample_col:
                    cmd.SetKeyword(sample_index_col_gkey, false);
                    cmd.SetKeyword(sample_average_col_gkey, false);
                    cmd.SetKeyword(sample_col_gkey, true);
                    break;
                case MsaaKeyword.sample_index_col:
                    cmd.SetKeyword(sample_index_col_gkey, true);
                    cmd.SetKeyword(sample_average_col_gkey, false);
                    cmd.SetKeyword(sample_col_gkey, false);
                    break;
                case MsaaKeyword.sample_average_col:
                    cmd.SetKeyword(sample_index_col_gkey, false);
                    cmd.SetKeyword(sample_average_col_gkey, true);
                    cmd.SetKeyword(sample_col_gkey, false);
                    break;
            }
            cmd.SetKeyword(msaa_sample_gkey, true);
            cmd.SetGlobalTexture(sourceTex_id, msaa_bindMS_ture_RT);
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(100.0f, 100.0f, 0.0f, 0.0f));
            cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);
            cmd.SetKeyword(msaa_sample_gkey, false);

            cmd.SetKeyword(msaa_gkey, true);
            cmd.SetGlobalTexture(sourceTex_id, msaa_bindMS_false_RT);
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(615.0f, 100.0f, 0.0f, 0.0f));
            cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);
            cmd.SetKeyword(msaa_gkey, false);
        }

        void DrawRTHandleTest(CommandBuffer cmd)
        {
            cmd.SetKeyword(rTHandle_gkey, true);

            if(asset.rTHandleParameters.resetReferrenceSize)
            {
                asset.rTHandleParameters.resetReferrenceSize = false;
                rtHandleSystem.ResetReferenceSize(asset.rTHandleParameters.setReferenceSize.x, asset.rTHandleParameters.setReferenceSize.y);
            }
            else
            {
                rtHandleSystem.SetReferenceSize(asset.rTHandleParameters.setReferenceSize.x, asset.rTHandleParameters.setReferenceSize.y);
            }

            CoreUtils.SetRenderTarget(cmd, rtHandleColor_0, rtHandleDepth, ClearFlag.All, Color.black);
            cmd.SetGlobalTexture(sourceTex_id, asset.sourceTex);
            cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);

            CoreUtils.SetRenderTarget(cmd, rtHandleColor_1, ClearFlag.All, Color.black);
            cmd.SetGlobalTexture(sourceTex_id, asset.sourceTex);
            cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);

            CoreUtils.SetRenderTarget(cmd, BuiltinRenderTextureType.CameraTarget);

            cmd.SetGlobalTexture(sourceTex_id, rtHandleColor_0);
            cmd.SetGlobalVector(myRTHandleScale_id, rtHandleColor_0.rtHandleProperties.rtHandleScale);
            cmd.SetGlobalVector(viewportSize_id, new Vector4(rtHandleColor_0.rtHandleProperties.currentViewportSize.x, rtHandleColor_0.rtHandleProperties.currentViewportSize.y, 0.0f, 0.0f));
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(50.0f, 300.0f, 0.0f, 0.0f));
            cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);
            cmd.SetKeyword(rTHandle_ClampAndScaleUV_gkey, true);
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(350.0f, 300.0f, 0.0f, 0.0f));
            cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);
            cmd.SetKeyword(rTHandle_ClampAndScaleUV_gkey, false);

            cmd.SetGlobalTexture(sourceTex_id, rtHandleColor_1);
            //`rtHandleProperties`は`_0`と共通の`rtHandleSystem`から取得(つまり要らない)。
            // cmd.SetGlobalVector(myRTHandleScale_id, rtHandleColor_1.rtHandleProperties.rtHandleScale);
            // cmd.SetGlobalVector(viewportSize_id, new Vector4(rtHandleColor_1.rtHandleProperties.currentViewportSize.x, rtHandleColor_1.rtHandleProperties.currentViewportSize.y, 0.0f, 0.0f));
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(50.0f, 10.0f, 0.0f, 0.0f));
            cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);
            cmd.SetKeyword(rTHandle_ClampAndScaleUV_gkey, true);
            cmd.SetGlobalVector(texDrawPoint_id, new Vector4(350.0f, 10.0f, 0.0f, 0.0f));
            cmd.DrawProcedural(Matrix4x4.identity, layoutBlitMaterial, 0, MeshTopology.Triangles, 3);
            cmd.SetKeyword(rTHandle_ClampAndScaleUV_gkey, false);

            cmd.SetKeyword(rTHandle_gkey, false);
        }
        
        static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.SetVertexBufferParams(4, 
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)
            );
            
            Vector3[] vertexData = new Vector3[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(-1.0f,  1.0f, 0.0f),
                new Vector3( 1.0f,  1.0f, 0.0f),
                new Vector3( 1.0f, -1.0f, 0.0f),
            };
            mesh.SetVertexBufferData(vertexData, 0, 0, 4);
            
            int[] indices = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.SetIndexBufferParams(6, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indices, 0, 0, 6);
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, 6));
            
            return mesh;
        }
    }
}
