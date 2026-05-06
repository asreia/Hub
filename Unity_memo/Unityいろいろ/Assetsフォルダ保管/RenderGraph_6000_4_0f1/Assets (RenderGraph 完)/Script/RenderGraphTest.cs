using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "RenderGraphTest_PiplineAsset.asset", menuName = "Rendering/RenderGraph Test PipelineAsset", order = 2)]
public class RenderGraphTestAsset : RenderPipelineAsset<RenderGraphTestAsset.RenderGraphTestPipline>
{
    public Material drawMeshMaterial;
    public Shader drawMeshShader;
    public Material fBBlitMaterial;
    public ComputeShader multiplyByIndexCS;
    public Texture2D stark;
    public Texture2D starkDango;
    public Texture2D frieren;
    public Texture2D fern;
    public Texture2D all;
    protected override RenderPipeline CreatePipeline()
    {
        return new RenderGraphTestPipline(this);
    }
    public class RenderGraphTestPipline : RenderPipeline
    {
        RenderGraphTestAsset asset;
        RenderGraph renderGraph;
        Material DrawStark1_Mat;
        int textureID = Shader.PropertyToID("_texture");
        int textureID1 = Shader.PropertyToID("_addTexture");
        int frierenTexID = Shader.PropertyToID("_frierenTex");
        int fernTexID = Shader.PropertyToID("_fernTex");
        Mesh planeMesh;
        GlobalKeyword frameBufferFetch_gKey = GlobalKeyword.Create("_FB_BLIT");
        GlobalKeyword fb_2_merge_gKey = GlobalKeyword.Create("_FB_2_MERGE");
        GlobalKeyword fb_2_plus_1_merge_gKey = GlobalKeyword.Create("_FB_2_PLUS_1_MERGE");
        GlobalKeyword fb_3_merge_gKey = GlobalKeyword.Create("_FB_3_MERGE");
        GlobalKeyword fb_MRT_gKey = GlobalKeyword.Create("_FB_MRT");
        GlobalKeyword addTex_gKey = GlobalKeyword.Create("_ADD_TEX");
        GlobalKeyword _uav_gKey = GlobalKeyword.Create("_UAV");
        // TextureHandle starkHandle; //外部に保存したらだめ。必ず`BeginRecording`～`EndRecordingAndExecute`の間で毎回セットする必要がある。
        // int RenderCount = 0;
        public class DrawMeshPassData //PassDataは初期化されずフレーム間で使いまわされる
        {
            public Mesh mesh;
            public Shader shader;
            public Material material;
            public Material material1;
            public ComputeShader computeShader;
            public Camera camera;
            public Vector4[] vec4Data;
            public BufferHandle buffer;
            public int nameID; public TextureHandle texture; public RenderTexture rt;
            public int nameID1; public TextureHandle texture1;
            public List<RendererListHandle> rendererListHandleList = new List<RendererListHandle>();
            public Matrix4x4 modelMatrix;
            public GlobalKeyword fB_gKey;
            public GlobalKeyword fB_gKey1;
        }
        public RenderGraphTestPipline(RenderGraphTestAsset asset)
        {
            this.asset = asset;
            renderGraph = new RenderGraph("RenderGraph_Test");
            // renderGraph.nativeRenderPassesEnabled = false;
            planeMesh = CreatePlaneMesh(new Rect(-0.25f, -0.5f, 0.5f, 1.0f));
            DrawStark1_Mat = new Material(asset.drawMeshShader){name = "Draw stark 1 Mat"};
        }
        protected override void Render(ScriptableRenderContext ctx, List<Camera> cameras)
        {
            foreach(Camera cam in cameras){if(cam.cameraType != CameraType.Game) return;} //エラー消えた

            CommandBuffer cmd = CommandBufferPool.Get("cmd");
            CommandBuffer cmd1 = CommandBufferPool.Get("cmd1");

            cmd.SetupCameraProperties(cameras[0]);
            ctx.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            try
            {
                var backbufferInfo = new RenderTargetInfo
                {
                    width       = cameras[0].pixelWidth,
                    height      = cameras[0].pixelHeight,
                    volumeDepth = 1,
                    msaaSamples = 1,
                    bindMS      = false,
                    format      = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR),
                };
                var CameraDepthInfo = backbufferInfo;
                CameraDepthInfo.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);

                var importParams = new ImportResourceParams
                {
                    clearOnFirstUse = true, //こっちは効く
                    clearColor = Color.bisque,
                    textureUVOrigin = TextureUVOrigin.BottomLeft
                };

                var rgParams = new RenderGraphParameters
                {
                    scriptableRenderContext = ctx,
                    commandBuffer = cmd,
                    currentFrameIndex = Time.frameCount,
                    generateDebugData = true,
                    executionId = cameras[0].GetEntityId(),
                    // renderTextureUVOriginStrategy = RenderTextureUVOriginStrategy.PropagateAttachmentOrientation
                };
                renderGraph.BeginRecording(rgParams);
                // if(RenderCount < 10){RenderCount++; starkHandle = renderGraph.ImportTexture(RTHandles.Alloc(asset.stark));}

                TextureHandle backbufferDayo = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget, backbufferInfo, importParams);
                TextureHandle cameraDepth = renderGraph.ImportTexture(RTHandles.Alloc(BuiltinRenderTextureType.Depth, "Camera Depth"), CameraDepthInfo);

                TextureHandle starkHandle = renderGraph.ImportTexture(RTHandles.Alloc(asset.stark));
                TextureHandle starkDangoHandle = renderGraph.ImportTexture(RTHandles.Alloc(asset.starkDango));
                TextureHandle frierenHandle = renderGraph.ImportTexture(RTHandles.Alloc(asset.frieren));
                TextureHandle fernHandle = renderGraph.ImportTexture(RTHandles.Alloc(asset.fern));

                TextureHandle starkRT_fernMRT = renderGraph.CreateTexture(new TextureDesc(backbufferInfo.width, backbufferInfo.height){name = "starkRT_fernMRT", clearBuffer = false, clearColor = Color.chartreuse.linear, format = backbufferInfo.format});
                TextureHandle starkBlitRT = renderGraph.CreateTexture(new TextureDesc(backbufferInfo.width, backbufferInfo.height){name = "starkBlitRT", clearBuffer = false, clearColor = Color.chocolate.linear, format = GraphicsFormat.R16G16B16A16_SFloat, slices = 1, msaaSamples = MSAASamples.None, memoryless = RenderTextureMemoryless.None, discardBuffer = true});
                TextureHandle test = renderGraph.CreateTexture(new TextureDesc(1, 1){format= GraphicsFormat.R8G8B8A8_SRGB, dimension = TextureDimension.Tex2DArray, slices = 2}); //slices>1はdimensionを忘れただけだった
                TextureHandle frierenRT = renderGraph.CreateTexture(new TextureDesc(backbufferInfo.width, backbufferInfo.height){name = "frierenRT", clearBuffer = true, clearColor = Color.chocolate.linear, format = backbufferInfo.format, enableRandomWrite = true });
                TextureHandle fernRT = renderGraph.CreateTexture(new TextureDesc(backbufferInfo.width, backbufferInfo.height){name = "fernRT", clearBuffer = true, clearColor = Color.chocolate.linear, format = backbufferInfo.format});
                TextureHandle starkAndFrierenRT = renderGraph.CreateTexture(new TextureDesc(backbufferInfo.width, backbufferInfo.height){name = "starkAndFrierenRT", clearBuffer = true, clearColor = Color.chocolate.linear, format = backbufferInfo.format});
                // TextureHandle fernMRT = renderGraph.CreateTexture(new TextureDesc(backbufferInfo.width, backbufferInfo.height){name = "fernMRT", clearBuffer = true, clearColor = Color.chocolate.linear, format = backbufferInfo.format});
                TextureHandle offScreenDepthTex = renderGraph.CreateTexture(new TextureDesc(CameraDepthInfo.width, CameraDepthInfo.height){name = "OffScreen Depth Texture", format = CameraDepthInfo.format});
                BufferHandle bufferHandle;

                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw stark 0", out var passData))
                {
                    builder.SetRenderAttachment(starkRT_fernMRT, 0);
                    // builder.SetRenderAttachmentDepth(cameraDepth);
                    builder.SetRenderAttachmentDepth(offScreenDepthTex); //コメントアウトするとNULLになってた//==

                    // builder.AllowPassCulling(false);
                    // builder.AllowGlobalStateModification(true); //除去//これを潜ったら`renderGraph.EndRecordingAndExecute()`に繋がった..
                        //コメントアウトすると >InvalidOperationException: スターク 0 の描画: このコマンドバッファからのグローバル状態の変更は許可されていません。レンダリンググラフパスがグローバル状態の変更を許可していることを確認してください。
                    builder.UseTexture(starkHandle, AccessFlags.Read); //`AccessFlags`を変えるとそれに応じて表示が変わる。パスカルや最適化に影響があると思われる。
                        //⟪コメントアウト¦.Discard⟫すると >例外: パス 'Draw stark 0' が、そのビルダーによって登録されていないテクスチャをコマンドバッファにバインドしようとしています。テクスチャの使用方法（UseTexture/CreateTransientTexture）をパスビルダーに指定してください。

                    passData.mesh = planeMesh;
                    passData.material = new Material(asset.drawMeshShader){name = "Draw stark 0 Mat"};
                    passData.nameID = textureID; passData.texture = starkHandle;
                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3((float)Screen.height/Screen.width, 1f, 1f));

                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        // RasterCtx.cmd.ClearRenderTarget(true, true, Color.blueViolet);
                        data.material.SetTexture(data.nameID, data.texture);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                    });
                }

                bool multiplyByIndexCSPassEnabled = false;
                using(var builder = renderGraph.AddComputePass<DrawMeshPassData>("Multiply by index CS", out var passData))
                {
                    builder.EnableAsyncCompute(true); //ctx.ExecuteCommandBuffer(cmd)をコメントアウトしても`computeCtx.cmd.DispatchCompute(..)`は実行される。
                      //(非同期で別々のcmdになっていて`ComputePass`側の`cmd`は`renderGraph`内部で`ctx.ExecuteCommandBuffer(cmd)`されていると思われる) 
                    // builder.AllowPassCulling(false);
                    Vector4[] vec4Data = new Vector4[]{new Vector4(0.1f, 0.1f, 0.1f, 0.1f), new Vector4(0.1f, 0.1f, 0.1f, 0.1f)};
                    // GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 2, 16);
                    // BufferHandle bufferHandle =  renderGraph.ImportBuffer(buffer);
                    bufferHandle =  renderGraph.CreateBuffer(new BufferDesc(2, 16, GraphicsBuffer.Target.Structured){name = "vec4Buffer"}); //Pass内で`renderGraph.CreateBuffer(..)`可能!? (Pass内で`renderGraph`が使える)
                    // ((GraphicsBuffer)bufferHandle).SetData(data); //`.SetRenderFunc`外なので不可
                    builder.UseBuffer(bufferHandle, AccessFlags.ReadWrite);
                    passData.computeShader = asset.multiplyByIndexCS;
                    passData.vec4Data = vec4Data;
                    passData.buffer = bufferHandle;
                    builder.SetRenderFunc(/*static*/ (DrawMeshPassData data, ComputeGraphContext computeCtx) =>
                    {
                        multiplyByIndexCSPassEnabled = true;
                        // ((GraphicsBuffer)data.buffer).SetData(data.vec4Data); //←↓どっちでも可
                        computeCtx.cmd.SetBufferData(data.buffer, data.vec4Data);
                        computeCtx.cmd.SetComputeBufferParam(data.computeShader, data.computeShader.FindKernel("CSMain"), "data", data.buffer); //これは`builder.AllowGlobalStateModification(true)`要らない (Global的では無いから)
                        computeCtx.cmd.DispatchCompute(data.computeShader, data.computeShader.FindKernel("CSMain"), 1, 1, 1);
                        Vector4[] vec4Data = new Vector4[2];
                        ((GraphicsBuffer)data.buffer).GetData(vec4Data);
                        foreach(Vector4 eVec in vec4Data){Debug.Log(eVec.ToString());} //okちゃんとCS動作してる
                    });
                }
                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw stark 1", out var passData))
                {
                    // builder.GenerateDebugData(false); //*Render Graph Wiewer*で"Draw stark 1"のPassの列が消える
                    // builder.EnableAsyncCompute(true);
                    // builder.AllowGlobalStateModification(true); //除去
                    builder.SetInputAttachment(starkRT_fernMRT, 0); //コメントアウトしてFBFを切ってもNRPのバッチが切れない！ //==

                    builder.SetRenderAttachment(starkBlitRT, 0); //`.Discard`はエラー //`.Read`にして`.AllowGlobalStateModification(true)`も無効化するとパスカリングされることを確認(stark_dangoも消える)
                    // builder.SetRenderAttachmentDepth(cameraDepth);
                    builder.SetRenderAttachmentDepth(offScreenDepthTex);

                    // builder.UseTexture(starkHandle, AccessFlags.Read);

                    builder.UseTexture(starkDangoHandle, AccessFlags.Read); //普通のテクスチャを追加してもNRPのバッチが切れない！

                    passData.mesh = planeMesh;
                    passData.shader = asset.drawMeshShader;
                    passData.material = DrawStark1_Mat;
                    passData.nameID = textureID1; passData.texture = starkDangoHandle;

                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0.2f), Quaternion.identity, new Vector3((float)Screen.height/Screen.width, 1f, 1f));
                        //デプスバグは違うUnityバージョン、違うプラットフォームでも起きる
                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        data.material.SetTexture(data.nameID, data.texture);
                        data.material.SetKeyword(new LocalKeyword(data.shader, "_FB_BLIT"), true);
                        data.material.SetKeyword(new LocalKeyword(data.shader, "_ADD_TEX"), true);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw frieren 0", out var passData))
                {
                    builder.SetRenderAttachment(frierenRT, 0);
                    // builder.SetRenderAttachmentDepth(TextureHandle.nullHandle); //前のPassのデプス解除できなかった..

                    // builder.AllowGlobalStateModification(true); //除去
                    // builder.SetInputAttachment(offScreenDepthTex, 0); //`Read-Only DSV`だから読めるかなと思ったけどエラー
                    builder.UseTexture(frierenHandle, AccessFlags.Read);
                    builder.UseBuffer(bufferHandle, AccessFlags.Read); //コメントアウトすると`Multiply by index CS`Passがパスカリングされる
                    passData.buffer = bufferHandle;

                    builder.SetGlobalTextureAfterPass(frierenRT, frierenTexID); //`builder.AllowGlobalStateModification(true)`は必要ではない!//=========================

                    passData.mesh = planeMesh;
                    passData.material = new Material(asset.drawMeshShader){name = "Draw frieren 0 Mat"};
                    passData.material1 = DrawStark1_Mat;
                    passData.nameID = textureID; passData.texture = frierenHandle;
                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3((float)Screen.height/Screen.width, 1f, 1f));

                    builder.SetRenderFunc(/*static*/ (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        if(/*data.buffer.IsValid()*/multiplyByIndexCSPassEnabled){
                            data.material1.SetBuffer("buffer", data.buffer); Debug.Log("刈られてない!");}
                        data.material.SetTexture(data.nameID, data.texture);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                    });
                }

                // offScreenColorTex_2を通常のテクスチャとしてサンプルするのでパスマージが切れる (このPassを切ると全て繋がる)
                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw frieren UAV", out var passData))
                {
                    builder.SetRenderAttachment(starkRT_fernMRT, 0);
                    builder.SetRandomAccessAttachment(frierenRT, 1); //☆これをコメントアウトするとパスカリングされる//％AccessFlags.ReadWrite //↓↓の`builder.SetRenderAttachment(starkRT_fernMRT, 1, AccessFlags.WriteAll)`も設定
                    passData.material = asset.fBBlitMaterial;
                    passData.fB_gKey = _uav_gKey;

                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        RasterCtx.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw fern 0", out var passData))
                {
                    builder.SetRenderAttachment(fernRT, 0);

                    // builder.AllowGlobalStateModification(true); //除去
                    builder.UseTexture(fernHandle, AccessFlags.Read); //`.UseTexture(..)`は`.SetGlobalTexture(..)`によって要求される

                    builder.SetGlobalTextureAfterPass(fernRT, fernTexID);

                    // passData.nameID1 = frierenTexID; passData.texture1 = frierenRT;
                    // builder.UseTexture(offScreenColorTex_2, AccessFlags.Read); //このパスでは使っていないが`.SetGlobalTexture(..)`によって要求される。あとNRPが切れる

                    passData.mesh = planeMesh;
                    passData.material = new Material(asset.drawMeshShader){name = "Draw fern 0 Mat"};//asset.drawMeshMaterial;
                    passData.nameID = textureID; passData.texture = fernHandle;
                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3((float)Screen.height/Screen.width, 1f, 1f));

                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        data.material.SetTexture(data.nameID, data.texture);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw 2 marge & MRT", out var passData))
                {
                    // builder.AllowGlobalStateModification(true); //除去
                    builder.SetRenderAttachment(starkAndFrierenRT, 0);
                    builder.SetRenderAttachment(starkRT_fernMRT, 1, AccessFlags.WriteAll); //`.WriteAll`(.Discard)で`Draw frieren UAV`が`.Load`が`.DontCare`になるのを確認。パスカリングも起こすことが可能
                    builder.SetInputAttachment(starkBlitRT, 0);  //==
                    builder.SetInputAttachment(frierenRT, 1);
                    builder.SetInputAttachment(fernRT, 2);

                    passData.mesh = planeMesh;
                    passData.shader = asset.drawMeshShader;
                    passData.material =  new Material(asset.drawMeshShader){name = "Draw 2 marge & MRT Mat"};//asset.drawMeshMaterial;
                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3((float)Screen.height/Screen.width * 2, 1f, 1f));

                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        data.material.SetKeyword(new LocalKeyword(data.shader, "_FB_2_MERGE"), true); data.material.SetKeyword(new LocalKeyword(data.shader, "_FB_MRT"), true);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw 2+1 marge", out var passData))
                {
                    // builder.AllowGlobalStateModification(true); //除去
                    builder.SetInputAttachment(starkAndFrierenRT, 0);
                    builder.SetInputAttachment(starkRT_fernMRT, 1);

                    builder.SetRenderAttachment(backbufferDayo, 0);

                    passData.mesh = planeMesh;
                    passData.shader = asset.drawMeshShader;
                    passData.material =  new Material(asset.drawMeshShader){name = "Draw 2+1 marge Mat"};//asset.drawMeshMaterial;
                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(-0.26f, 0, 0), Quaternion.identity, new Vector3((float)Screen.height/Screen.width * 3, 1f, 1f));

                    Debug.Log($"renderGraph.renderTextureUVOriginStrategy: {renderGraph.renderTextureUVOriginStrategy}");
                    // passData.texture = backbufferDayo;
                    passData.texture = starkRT_fernMRT;

                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        data.material.SetKeyword(new LocalKeyword(data.shader, "_FB_2_PLUS_1_MERGE"), true);
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                        Debug.Log($"RasterCtx.GetTextureUVOrigin(data.texture): {RasterCtx.GetTextureUVOrigin(data.texture)}");
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<DrawMeshPassData>("Draw 3 marge", out var passData))
                {
                    // builder.AllowGlobalStateModification(true); //除去
                    builder.SetInputAttachment(starkBlitRT, 0); //==
                    // builder.SetInputAttachment(offScreenColorTex_2, 1);
                    // builder.SetInputAttachment(offScreenColorTex_3, 2);

                    builder.SetRenderAttachment(backbufferDayo, 0);

                    // builder.UseGlobalTexture(frierenTexID, AccessFlags.Read); //builder.SetGlobalTextureAfterPass(..)と必ず対である必要がある
                    // builder.UseGlobalTexture(fernTexID, AccessFlags.Read);
                    builder.UseAllGlobalTextures(true); //全ての`builder.SetGlobalTextureAfterPass(..)`に対して`builder.UseGlobalTexture(｢全てのTexID｣, AccessFlags.Read)`をするのと同等の挙動。

                    passData.mesh = planeMesh;
                    passData.shader = asset.drawMeshShader;
                    passData.material = new Material(asset.drawMeshShader){name = "Draw 3 marge Mat"};//asset.drawMeshMaterial;
                    passData.modelMatrix = Matrix4x4.TRS(new Vector3(+0.26f, 0, 0), Quaternion.identity, new Vector3((float)Screen.height/Screen.width * 3, 1f, 1f));

                    builder.SetRenderFunc(static (DrawMeshPassData data, RasterGraphContext RasterCtx) =>
                    {
                        data.material.SetKeyword(new LocalKeyword(data.shader, "_FB_3_MERGE"), true);
                        // data.material.SetTexture(frierenTexID, fernRT); //`Properties{..}`と`⟪Global¦Local⟫Property`の関係を確認した
                        RasterCtx.cmd.DrawMesh(data.mesh, data.modelMatrix, data.material, 0, 0);
                    });
                }

                renderGraph.EndRecordingAndExecute();

                var rgParams1 = new RenderGraphParameters
                {
                    scriptableRenderContext = ctx,
                    // commandBuffer = cmd,
                    commandBuffer = cmd1, //=>`ctx.ExecuteCommandBuffer(cmd1)` //`cmd`を分けることも可能
                    currentFrameIndex = Time.frameCount,
                    generateDebugData = true,
                    executionId = cameras[1].GetEntityId(), //cameras[1]
                };
                renderGraph.BeginRecording(rgParams1);
                importParams.clearOnFirstUse = false;
                importParams.clearColor = Color.cyan;
                TextureHandle backbufferDayo1 = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget, backbufferInfo, importParams);
                TextureHandle allHandle = renderGraph.ImportTexture(RTHandles.Alloc(asset.all));
                RenderTexture drawRendererRT = new RenderTexture((int)(backbufferInfo.width * 0.5f), (int)(backbufferInfo.height * 0.5f), 0, backbufferInfo.format){name = "DrawRendererList RT"};
                if(!cameras[1].TryGetCullingParameters(out var cullingParams)){Debug.LogError("カリングパラメータの取得に失敗");}
                CullingResults cullingResults = ctx.Cull(ref cullingParams);
                DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), new SortingSettings(cameras[1]));
                FilteringSettings sphereFilteringSettings = new FilteringSettings(RenderQueueRange.all){renderingLayerMask = RenderingLayerMask.GetMask("Sphere")};
                FilteringSettings planeFilteringSettings = new FilteringSettings(RenderQueueRange.all){renderingLayerMask = RenderingLayerMask.GetMask("Plane")};
                RendererListHandle sphereRendererListHandle = renderGraph.CreateRendererList(new RendererListParams(cullingResults, drawingSettings, sphereFilteringSettings));
                RendererListHandle planeRendererListHandle = renderGraph.CreateRendererList(new RendererListParams(cullingResults, drawingSettings, planeFilteringSettings));
                using(var builder = renderGraph.AddUnsafePass<DrawMeshPassData>("Double DrawRendererList", out var passData))
                {
                    // builder.AllowPassCulling(false);
                    passData.camera = cameras[1];
                    passData.rt = drawRendererRT;
                    passData.texture = backbufferDayo1;
                    passData.texture1 = allHandle;
                    passData.rendererListHandleList.Clear(); //PassDataは初期化されない
                    passData.rendererListHandleList.Add(sphereRendererListHandle);
                    passData.rendererListHandleList.Add(planeRendererListHandle);

                    builder.UseTexture(backbufferDayo1, AccessFlags.Write); //`.AllowPassCulling(false)`でもこっちは無くても大丈夫
                    builder.UseTexture(allHandle, AccessFlags.Read);
                    builder.UseRendererList(sphereRendererListHandle); //無いとエラーにはならないが描画されない(`RendererList.nullRendererList`かな?)
                    builder.UseRendererList(planeRendererListHandle);
                    builder.SetRenderFunc(static (DrawMeshPassData data, UnsafeGraphContext unsafeCtx) =>
                    {
                        unsafeCtx.cmd.SetupCameraProperties(data.camera);
                        unsafeCtx.cmd.SetRenderTarget(data.rt);
                        unsafeCtx.cmd.ClearRenderTarget(false, true, Color.turquoise.linear);
                        unsafeCtx.cmd.SetGlobalTexture("_BaseMap", data.texture1);
                        unsafeCtx.cmd.SetGlobalVector("_SunColor", new Vector4(1.0f, 0.9f, 0.7f, 0.0f));
                        unsafeCtx.cmd.DrawRendererList(data.rendererListHandleList[0]);

                        unsafeCtx.cmd.SetRenderTarget(data.texture);//←↓どっちでもいい
                        // unsafeCtx.cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget); //RT直で呼べる
                        unsafeCtx.cmd.SetGlobalTexture("_BaseMap", data.rt);
                        unsafeCtx.cmd.DrawRendererList(data.rendererListHandleList[1]);
                    });
                }
                using(var builder = renderGraph.AddUnsafePass<DrawMeshPassData>("Something Pass", out var passData))
                {
                    // builder.AllowPassCulling(false);
                    // builder.AllowGlobalStateModification(true); //`UnsafePass`のみ`.AllowGlobalStateModification(true)`無しで動く
                    builder.SetRenderFunc(static (DrawMeshPassData data, UnsafeGraphContext unsafeCtx) =>
                    {
                        unsafeCtx.cmd.SetGlobalVector("_SomethingColor", new Vector4(1.0f, 0.9f, 0.7f, 0.0f));
                    });
                }
                renderGraph.EndRecordingAndExecute();
            }
            catch (Exception e)
            {
                if(renderGraph.ResetGraphAndLogException(e)) throw;
            }

            renderGraph.EndFrame();

            ctx.ExecuteCommandBuffer(cmd);
            ctx.ExecuteCommandBuffer(cmd1); //消すと`cmd1`の描画が消える
            CommandBufferPool.Release(cmd);
            CommandBufferPool.Release(cmd1);

            ctx.Submit();
        }
        protected override void Dispose(bool disposing)
        {
            renderGraph.Cleanup();
            renderGraph = null;
        }
        Mesh CreatePlaneMesh(Rect scaleRect)
        {
            Mesh mesh = new Mesh();

            // 頂点属性の定義
            mesh.SetVertexBufferParams(4, new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0)
            });

            // 頂点データ（Position, TexCoord0） スケール適用
            var vertexData = new Vertex[]
            {
                new Vertex { position = new Vector3(scaleRect.xMin, scaleRect.yMin, 0), uv = new Vector2(0, 0) },
                new Vertex { position = new Vector3(scaleRect.xMin, scaleRect.yMax, 0) , uv = new Vector2(0, 1) },
                new Vertex { position = new Vector3(scaleRect.xMax, scaleRect.yMax, 0)  , uv = new Vector2(1, 1) },
                new Vertex { position = new Vector3(scaleRect.xMax, scaleRect.yMin, 0) , uv = new Vector2(1, 0) }
            };
            mesh.SetVertexBufferData(vertexData, 0, 0, vertexData.Length);

            // インデックスデータ
            var indices = new ushort[] { 0, 1, 2, 2, 3, 0 }; //時計回り(CW)
            mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);
            mesh.SetIndexBufferData(indices, 0, 0, indices.Length);

            // サブメッシュ設定
            var subMesh = new SubMeshDescriptor(0, indices.Length, MeshTopology.Triangles);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, subMesh);

            return mesh;
        }
        struct Vertex
        {
            public Vector3 position;
            public Vector2 uv;
        }
    }
}