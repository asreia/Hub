using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using Unity.Collections;

[CreateAssetMenu(fileName = "RenderGraphTest_PipelineAsset", menuName = "Rendering/RenderGraph Test PipelineAsset")]
public class CommandBufferTestAsset : RenderPipelineAsset<CommandBufferTestAsset.CommandBufferTestPipeline>
{
    public Texture2D cat;
    public Texture2D beluga;
    public Texture2D face;
    public Shader cmdTest;
    [Range(0.0f, 0.3f)]
    public float moveOffset; // = 0.0f; //恐らくAsset以外はInspectorに出せない
    public Material instancingMaterial;
    public Rect viewportRect = new Rect(0.20f, 0.1f, 0.6f, 0.7f);
    public Rect scissorRect = new Rect(0.3f, 0.3f, 0.6f, 0.6f);
    public bool wireframe = true;
    public bool invertCullingFlag = false;
    public Vector2 globalDepthBias = new Vector2(1000.0f, 1.0f);
    public bool middleSubmit = true;
    [Range(0, 18)]
    public int faceCount = 4;
    public bool global_keyword_enable = true;
    public bool strip_keyword_enable = false;
    public bool svc_keyword_enable = true;
    public bool vertex_keyword_enable = false;
    public bool asset_0_keyword_enable = true;
    public bool asset_1_keyword_enable = true;
    [Range(0, 18)]
    public int instancingCount = 4;
    public bool matNullSwitch = false;
    public bool sortingLayer = false;
    public bool canvasOrder = false;
    public bool renderQueue = false;
    public bool rendererPriority = false;
    public bool backToFront = false;
    public bool optimizeStateChanges = false;
    [Range(-6.0f, -10.0f)]
    public float cameraPositionZ = -10.0f;
    public bool stopMotion = false;
    public bool excludeMotionVectorObjects = false;
    public bool forceAllMotionVectorObjects = false;
    public bool fallbackMaterial = false;
    public bool overrideMaterial = false;
    public bool depthTest = false;
    void Reset(){ OnValidate(); Debug.Log($"Reset()");}
    protected override RenderPipeline CreatePipeline() { return new CommandBufferTestPipeline(this); }

    public class CommandBufferTestPipeline : RenderPipeline
    {
        CommandBufferTestAsset asset;
        public Texture2D cat => asset.cat;
        public Texture2D beluga => asset.beluga;
        public Texture2D face => asset.face;
        public CommandBufferTestPipeline(CommandBufferTestAsset asset)
        {
            this.asset = asset;
        }
        Material material;
        Material shaderKeywordTestMaterial;
        Material shaderKeywordTestMaterial_Asset_0;
        Material shaderKeywordTestMaterial_Asset_1;
        Mesh plane;
        Mesh viewportPlane;
        Mesh scissorPlane;
        Mesh facePlane;
        Camera camera;
        float aspect;
        int aspect_id = Shader.PropertyToID("_aspect");
        int face_id = Shader.PropertyToID("_face"); int faceCount_id = Shader.PropertyToID("_spritCount");
        int atlasRanges_id = Shader.PropertyToID("_atlasRanges");
        int move_id = Shader.PropertyToID("_move");
        int renderLoopCounter = 0;
        static int staticRenderLoopCounter = 0;
        // RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        protected override void Render(ScriptableRenderContext ctx, List<Camera> _)
        {
            aspect = (float)Screen.width / Screen.height;
            CommandBuffer cmd = CommandBufferPool.Get("cmd0");

            //RenderingState系============================================================================================
            if (renderLoopCounter == 0)
            {
                // PlaneMeshの初期化
                plane = CreatePlaneMesh(new Rect(new Vector2(-0.5f, -0.5f), new Vector2(1.0f, 1.0f)));
                viewportPlane = CreatePlaneMesh(ToPlaneRect(asset.viewportRect));
                scissorPlane = CreatePlaneMesh(ToPlaneRect(asset.scissorRect));
                facePlane = CreatePlaneMesh(new Rect(new Vector2(0.0f, 0.0f), ApplyAspect(new Vector2(0.25f * asset.faceCount, 0.25f), aspect)));
                // マテリアルの初期化
                material = new Material(asset.cmdTest);
                shaderKeywordTestMaterial = new Material(Shader.Find("Custom/ShaderKeywordTestShader"));
                shaderKeywordTestMaterial_Asset_0 = Resources.Load<Material>("ShaderKeywordTestMaterial_Asset_0");
                shaderKeywordTestMaterial_Asset_1 = Resources.Load<Material>("ShaderKeywordTestMaterial_Asset_1");
                //カメラの取得
                camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            }
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            // cmd.SetRenderTarget(rt); cmd.SetInvertCulling(true); //catのcmd.SetInvertCulling(.)をコメントアウト
            cmd.ClearRenderTarget(RTClearFlags.ColorDepth, Color.brown.linear);

            using (cmd.WithWireframe(asset.wireframe))
            {
                using (cmd.WithKeyword(material, new LocalKeyword(asset.cmdTest, "_VIEWPORT")))
                {
                    //Build And Runのとき、`viewportPlane`が破棄される。恐らく、SceneリロードしてC#ドメインリロードしないから、このクラスの`renderLoopCounter`が初期化されない
                        //`RenderGraphTest_PipelineAsset`の*Inspector*を触るとすぐ直る
                    cmd.DrawMesh(viewportPlane, Matrix4x4.identity, material);
                }
                using (cmd.WithKeyword(material, new LocalKeyword(asset.cmdTest, "_SCISSOR")))
                {
                    cmd.DrawMesh(scissorPlane, Matrix4x4.identity, material);
                }
            }

            Matrix4x4 move;
            var screenSize = new Vector2(Screen.width, Screen.height);
            using (cmd.WithViewport(new Rect(asset.viewportRect.position * screenSize, asset.viewportRect.size * screenSize)))
            {
                using (cmd.WithScissorRect(new Rect(asset.scissorRect.position * screenSize, asset.scissorRect.size * screenSize)))
                {
                    cmd.SetGlobalFloat(aspect_id, aspect);

                    material.mainTexture = cat;
                    move = Matrix4x4.TRS(new Vector3(-0.45f + asset.moveOffset, 0.25f, 0.1f), Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
                    cmd.SetInvertCulling(asset.invertCullingFlag);
                    cmd.DrawMesh(plane, move, material);
                    cmd.SetInvertCulling(false);

                    // cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget); //cmd.SetViewport(.)がSetされたrtにリセットされる
                    if (asset.middleSubmit)
                    {
                        ctx.ExecuteCommandBuffer(cmd); //⟪Begin¦End⟫Event(cmd0)が自動的に挿入される
                        ctx.Submit();                  //cmd群実行。LocalProperty設定が反映される単位  //これを超えてもcmd.SetRenderTarget(rt)が元のRTに戻ることはない
                        cmd.Clear();
                    }

                    material.mainTexture = beluga; //コメントアウトするとダブルbelugaになる (ctx.Submit()を超えてもLocalProperty設定が維持される)
                    move = Matrix4x4.TRS(new Vector3(0.45f - asset.moveOffset, -0.25f, 0.1f), Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
                    cmd.SetGlobalDepthBias(asset.globalDepthBias.x, asset.globalDepthBias.y);
                    cmd.DrawMesh(plane, move, material);
                    cmd.SetGlobalDepthBias(0.0f, 0.0f);
                }
            }
            //=========================================================================================================

            DrawShaderKeywordTest(cmd);

            {//cmd.DrawMeshInstancedProcedural(..)
                move = Matrix4x4.TRS(new Vector3(-0.93f, -0.87f, 0.2f), Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
                cmd.SetGlobalMatrix(move_id, move);
                cmd.DrawMeshInstancedProcedural(plane, 0, asset.instancingMaterial, 0, asset.instancingCount);
            }

            DrawRenderListTest(ctx, cmd);

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            ctx.Submit();
            if (renderLoopCounter == 0) //ctx.Submit()でmaterial.enabledKeywordsに設定される
            {
                Debug.Log($"staticRenderLoopCounter: {staticRenderLoopCounter} ============================================================");
                staticRenderLoopCounter++;
                // 有効なキーワード (LocalKeyword[]) //C#リロードで更新 または Materialに保存されている
                var enabledKeywords = shaderKeywordTestMaterial.enabledKeywords;
                string enabledStr = string.Join(", ", enabledKeywords.Select(k => k.name));
                Debug.Log($"material.enabledKeywords: {enabledStr}");

                // シェーダーに定義されている全てのキーワード (LocalKeyword[]) //C#リロードで更新
                var shaderKeywords = shaderKeywordTestMaterial.shader.keywordSpace.keywords;
                string allKeywordsStr = string.Join(", ", shaderKeywords.Select(k => k.name));
                Debug.Log($"shader.keywordSpace: {allKeywordsStr}");

                // Shader.enableGlobalKeywords (GlobalKeyword[]) //これのリセットはEditor再起動 (C#リロード貫通) //明示的に設定すれば反映される
                var enabledGlobalKeywords = Shader.enabledGlobalKeywords;
                string enabledGlobalKeywordsStr = string.Join(", ", enabledGlobalKeywords.Select(k => k.name));
                Debug.Log($"Shader.enableGlobalKeywords: {enabledGlobalKeywordsStr}");

                // Shader.globalKeywords (GlobalKeyword[]) //これのリセットはEditor再起動 (C#リロード貫通)
                var globalKeywords = Shader.globalKeywords;
                string globalKeywordsStr = string.Join(", ", globalKeywords.Select(k => k.name));
                Debug.Log($"Shader.globalKeywords: {globalKeywordsStr}");
            }
            renderLoopCounter++;
        }

        GlobalKeyword global_keyword = GlobalKeyword.Create("_GLOBAL");
        GlobalKeyword strip_keyword = GlobalKeyword.Create("_STRIP");
        LocalKeyword stereo_keyword_Asset, stereo_keyword, svc_keyword, vertex_keyword, default_keyword, asset_0_keyword, asset_1_keyword;
        void DrawShaderKeywordTest(CommandBuffer cmd)
        {
            if (renderLoopCounter == 0)
            {
                svc_keyword = new LocalKeyword(shaderKeywordTestMaterial.shader, "_SVC");
                vertex_keyword = new LocalKeyword(shaderKeywordTestMaterial.shader, "_VERTEX");
                default_keyword = new LocalKeyword(shaderKeywordTestMaterial_Asset_0.shader, "_DEFAULT");
                asset_0_keyword = new LocalKeyword(shaderKeywordTestMaterial_Asset_0.shader, "_ASSET_0");
                asset_1_keyword = new LocalKeyword(shaderKeywordTestMaterial_Asset_0.shader, "_ASSET_1");
                stereo_keyword_Asset = new LocalKeyword(shaderKeywordTestMaterial_Asset_0.shader, "STEREO_INSTANCING_ON");
                stereo_keyword = new LocalKeyword(shaderKeywordTestMaterial.shader, "STEREO_INSTANCING_ON");
            }
            //これらのcmd.SetKeyword(..)を↑のif(.)に含めても結果は変わらない。つまり、ctx.Submit()を超えてもShaderKeywordの設定は維持される。(GPUキャッシュ?)
            //GlobalKeywordの設定
            cmd.SetKeyword(global_keyword, asset.global_keyword_enable);
            cmd.SetKeyword(strip_keyword, asset.strip_keyword_enable);
            //LocalKeywordの設定
            cmd.SetKeyword(shaderKeywordTestMaterial, svc_keyword, asset.svc_keyword_enable);
            cmd.SetKeyword(shaderKeywordTestMaterial, vertex_keyword, asset.vertex_keyword_enable);
            cmd.SetKeyword(shaderKeywordTestMaterial_Asset_0, default_keyword, !asset.asset_0_keyword_enable);
            cmd.SetKeyword(shaderKeywordTestMaterial_Asset_0, asset_0_keyword, asset.asset_0_keyword_enable);
            cmd.SetKeyword(shaderKeywordTestMaterial_Asset_1, default_keyword, !asset.asset_1_keyword_enable);
            cmd.SetKeyword(shaderKeywordTestMaterial_Asset_1, asset_1_keyword, asset.asset_1_keyword_enable);
            //STEREO_INSTANCING_ON を false
            cmd.SetKeyword(shaderKeywordTestMaterial_Asset_1, stereo_keyword_Asset, false);
            cmd.SetKeyword(shaderKeywordTestMaterial_Asset_0, stereo_keyword_Asset, false);
            cmd.SetKeyword(shaderKeywordTestMaterial, stereo_keyword, false);

            cmd.SetGlobalVectorArray(atlasRanges_id, new Vector4[]
            {
                new Vector4(0.0f, 0.0f, 0.5f, 0.5f), // 左上
                new Vector4(0.5f, 0.0f, 1.0f, 0.5f), // 右上
                new Vector4(0.0f, 0.5f, 0.5f, 1.0f), // 左下
                new Vector4(0.5f, 0.5f, 1.0f, 1.0f)  // 右下
            });
            cmd.SetGlobalInteger(faceCount_id, asset.faceCount);
            cmd.SetGlobalTexture(face_id, face);
            float posX = 1.0f - (0.25f * asset.faceCount / aspect);
            Matrix4x4 ObjectToWorld = Matrix4x4.TRS(new Vector3(posX, -1.00f, 0.0f), Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
            cmd.DrawMesh(facePlane, ObjectToWorld, shaderKeywordTestMaterial);
            ObjectToWorld = Matrix4x4.TRS(new Vector3(posX, -0.75f, 0.2f), Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
            cmd.DrawMesh(facePlane, ObjectToWorld, shaderKeywordTestMaterial_Asset_1);
            ObjectToWorld = Matrix4x4.TRS(new Vector3(posX, -0.50f, 0.2f), Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
            cmd.DrawMesh(facePlane, ObjectToWorld, shaderKeywordTestMaterial_Asset_0);
        }

        GameObject rendererObj_0;
        GameObject rendererObj_1;
        GameObject rendererObj_2;
        GameObject rendererObj_3;
        GameObject rendererMotionObj;
        Transform rendererMotionObj_transform;
        Material rendererListFallbackMaterial;
        Material rendererListOverrideMaterial;
        Shader rendererListOverrideShader;
        static float motionTimer = 0.0f;
        void DrawRenderListTest(ScriptableRenderContext ctx, CommandBuffer cmd)
        {
            if (renderLoopCounter == 0)
            {
                rendererObj_0 = GameObject.Find("RendererObj_0");
                rendererObj_1 = GameObject.Find("RendererObj_1");
                rendererObj_2 = GameObject.Find("RendererObj_2");
                rendererObj_3 = GameObject.Find("RendererObj_3");
                rendererMotionObj = GameObject.Find("RendererMotionObj");
                Renderer rendererObj_renderer_0 = rendererObj_0.GetComponent<Renderer>();
                Renderer rendererObj_renderer_1 = rendererObj_1.GetComponent<Renderer>();
                Renderer rendererObj_renderer_2 = rendererObj_2.GetComponent<Renderer>();
                Renderer rendererObj_renderer_3 = rendererObj_3.GetComponent<Renderer>();
                Transform rendererObj_transform_0 = rendererObj_0.GetComponent<Transform>();
                Transform rendererObj_transform_1 = rendererObj_1.GetComponent<Transform>();
                Transform rendererObj_transform_2 = rendererObj_2.GetComponent<Transform>();
                Transform rendererObj_transform_3 = rendererObj_3.GetComponent<Transform>();
                rendererMotionObj_transform = rendererMotionObj.GetComponent<Transform>();
                var rendererListMaterial = Resources.Load<Material>("Custom_RendererListShader");
                var rendererListMaterial_1 = Resources.Load<Material>("Custom_RendererListShader_1");
                var rendererListMotionMaterial = Resources.Load<Material>("Custom_RendererListMotionShader");
                rendererListFallbackMaterial = Resources.Load<Material>("Custom_RendererListFallbackShader");
                rendererListOverrideMaterial = Resources.Load<Material>("Custom_RendererListOverrideShader");
                rendererListOverrideShader = Resources.Load<Shader>("RendererListOverrideShader");

                if (!asset.matNullSwitch)
                {
                    //同一シェーダーバリアント同一メッシュなのにInstancingに成っていない
                    rendererObj_renderer_0.material =
                    rendererObj_renderer_1.material =
                    rendererObj_renderer_2.material = rendererListMaterial;
                    rendererObj_renderer_3.material = rendererListMaterial_1;
                    rendererMotionObj.GetComponent<Renderer>().material = rendererListMotionMaterial;

                    rendererListMaterial.enableInstancing = true;
                }
                else
                {
                    rendererObj_renderer_0.material =
                    rendererObj_renderer_1.material =
                    rendererObj_renderer_2.material =
                    rendererObj_renderer_3.material =
                    rendererMotionObj.GetComponent<Renderer>().material = null;
                }

                //renderer.sortingLayer
                if (asset.sortingLayer)
                {
                    rendererObj_renderer_0.sortingLayerName = "First Layer";  //0
                    rendererObj_renderer_1.sortingLayerName = "Second Layer"; //1
                    rendererObj_renderer_2.sortingLayerName = "Third Layer";  //2
                    rendererObj_renderer_3.sortingLayerName = "Fourth Layer"; //3
                    // Debug.Log($"SortingLayer.GetLayerValueFromName(\"Third Layer\"): {SortingLayer.GetLayerValueFromName("Third Layer")}"); //=>3 (SortingLayer.value)
                }
                else
                {
                    rendererObj_renderer_0.sortingLayerName =
                    rendererObj_renderer_1.sortingLayerName =
                    rendererObj_renderer_2.sortingLayerName =
                    rendererObj_renderer_3.sortingLayerName = "Default";
                }

                //renderer.sortingOrder
                if (asset.canvasOrder)
                {
                    rendererObj_renderer_0.sortingOrder = 2; //3
                    rendererObj_renderer_1.sortingOrder = 0; //1
                    rendererObj_renderer_2.sortingOrder = 1; //2
                    rendererObj_renderer_3.sortingOrder = -1; //0
                }
                else
                {
                    rendererObj_renderer_0.sortingOrder =
                    rendererObj_renderer_1.sortingOrder =
                    rendererObj_renderer_2.sortingOrder =
                    rendererObj_renderer_3.sortingOrder = 0;
                }

                //material.renderQueue
                if (!asset.matNullSwitch)
                {
                    if (asset.renderQueue)
                    {
                        //SubShader{Tags{"Queue" = "AlphaTest+100"『2550』}}
                        rendererObj_renderer_0.sharedMaterial = new Material(rendererObj_renderer_0.sharedMaterial) { renderQueue = -1 };   //3
                        rendererObj_renderer_1.sharedMaterial = new Material(rendererObj_renderer_1.sharedMaterial) { renderQueue = 2549 }; //2
                        rendererObj_renderer_2.sharedMaterial = new Material(rendererObj_renderer_2.sharedMaterial) { renderQueue = 2548 }; //1
                        rendererObj_renderer_3.sharedMaterial = new Material(rendererObj_renderer_3.sharedMaterial) { renderQueue = 2547 }; //0
                    }
                    else
                    {
                        //↓これらをコメントアウトしてもInstancingされない
                        rendererObj_renderer_0.sharedMaterial = new Material(rendererObj_renderer_0.sharedMaterial) { renderQueue = -1 }; //3
                        rendererObj_renderer_1.sharedMaterial = new Material(rendererObj_renderer_1.sharedMaterial) { renderQueue = -1 }; //2
                        rendererObj_renderer_2.sharedMaterial = new Material(rendererObj_renderer_2.sharedMaterial) { renderQueue = -1 }; //1
                        rendererObj_renderer_3.sharedMaterial = new Material(rendererObj_renderer_3.sharedMaterial) { renderQueue = -1 }; //0
                    }
                    // Debug.Log($"renderQueue: {rendererObj_renderer_0.material.renderQueue}, rawRenderQueue: {rendererObj_renderer_0.material.rawRenderQueue}, shader.renderQueue: {rendererObj_renderer_0.material.shader.renderQueue}");
                }

                //renderer.rendererPriority
                if (asset.rendererPriority)
                {
                    rendererObj_renderer_0.rendererPriority = 1;  //2
                    rendererObj_renderer_1.rendererPriority = 2;  //3
                    rendererObj_renderer_2.rendererPriority = 0;  //1
                    rendererObj_renderer_3.rendererPriority = -1; //0
                }
                else
                {
                    rendererObj_renderer_0.rendererPriority =
                    rendererObj_renderer_1.rendererPriority =
                    rendererObj_renderer_2.rendererPriority =
                    rendererObj_renderer_3.rendererPriority = 0;
                }

                //gameObject.Transform
                if (asset.backToFront)
                {
                    //.backToFront用
                    rendererObj_transform_0.localPosition = new Vector3(0.0f, 1.5f, -7.0f);  //0
                    rendererObj_transform_1.localPosition = new Vector3(0.5f, 1.0f, -8.0f);  //2
                    rendererObj_transform_2.localPosition = new Vector3(0.0f, 0.5f, -7.5f);  //1
                    rendererObj_transform_3.localPosition = new Vector3(-0.5f, 1.0f, -8.5f); //3

                    //.QuantizedFrontToBack用
                    // rendererObj_transform_0.localPosition = new Vector3(0.0f, 1.5f, -7.0f);   //0
                    // rendererObj_transform_1.localPosition = new Vector3(0.5f, 1.0f, 90.0f);   //2
                    // rendererObj_transform_2.localPosition = new Vector3(0.0f, 0.5f, 40.0f);   //1
                    // rendererObj_transform_3.localPosition = new Vector3(-0.5f, 1.0f, 990.0f); //3
                }
                else
                {
                    rendererObj_transform_0.localPosition = new Vector3(0.0f, 1.5f, -7.0f);
                    rendererObj_transform_1.localPosition = new Vector3(0.5f, 1.0f, -7.0f);
                    rendererObj_transform_2.localPosition = new Vector3(0.0f, 0.5f, -7.0f);
                    rendererObj_transform_3.localPosition = new Vector3(-0.5f, 1.0f, -7.0f);
                }

                //SortingCriteria.OptimizeStateChanges (稀にしか変化を観測できないが、しっかり機能している)
                LocalKeyword optimizeStateChanges_Key = new LocalKeyword(rendererListMaterial.shader, "_OptimizeStateChanges_Key");
                LocalKeyword optimizeStateChanges_Key_1 = new LocalKeyword(rendererListMaterial_1.shader, "_OptimizeStateChanges_Key");
                if (!asset.matNullSwitch)
                {
                    if (asset.optimizeStateChanges)
                    {
                        rendererObj_renderer_0.sharedMaterial.SetKeyword(optimizeStateChanges_Key, true);    //赤
                        rendererObj_renderer_1.sharedMaterial.SetKeyword(optimizeStateChanges_Key, true);    //赤
                        rendererObj_renderer_2.sharedMaterial.SetKeyword(optimizeStateChanges_Key, false);   //橙
                        rendererObj_renderer_3.sharedMaterial.SetKeyword(optimizeStateChanges_Key_1, false); //橙
                    }
                    else
                    {
                        //↓これらをコメントアウトしてもInstancingされない
                        rendererObj_renderer_0.sharedMaterial.SetKeyword(optimizeStateChanges_Key, false);
                        rendererObj_renderer_1.sharedMaterial.SetKeyword(optimizeStateChanges_Key, false);
                        rendererObj_renderer_2.sharedMaterial.SetKeyword(optimizeStateChanges_Key, false);
                        rendererObj_renderer_3.sharedMaterial.SetKeyword(optimizeStateChanges_Key_1, false);
                    }
                }
            }

            cmd.SetupCameraProperties(camera);

            //CullingResults
            camera.TryGetCullingParameters(out ScriptableCullingParameters scriptableCullingParameters);
            CullingResults cullingResults = ctx.Cull(ref scriptableCullingParameters);

            //FilteringSettings
            var filteringSettings = FilteringSettings.defaultValue; //フィルタリングをしない設定の値
            filteringSettings.sortingLayerRange = new SortingLayerRange(0, 4);
            filteringSettings.renderQueueRange = new RenderQueueRange(2000, 3000);
            //`Camera.cullingMask`=>`camera.TryGetCullingParameters(..)～`からさらにフィルタリングしている
            filteringSettings.layerMask = LayerMask.GetMask("six", "seven", "eight", "nine");
            filteringSettings.renderingLayerMask = RenderingLayerMask.GetMask("Ren1", "Ren2", "Ren3", "Ren4", "RenShared");

            //SortingSettings
            var sortingSettings = new SortingSettings(camera);

            sortingSettings.criteria = SortingCriteria.SortingLayer | SortingCriteria.CanvasOrder | SortingCriteria.RenderQueue | SortingCriteria.RendererPriority | SortingCriteria.BackToFront | SortingCriteria.OptimizeStateChanges;

            sortingSettings.cameraPosition = new Vector3(0.0f, 1.0f, asset.cameraPositionZ);

            //DrawingSettings
            var drawingSettings = new DrawingSettings(ShaderTagId.none , sortingSettings);
            drawingSettings.SetShaderPassName(9, new ShaderTagId("RendererListShaderTag"));
            // drawingSettings.SetShaderPassName(3, new ShaderTagId("RendererListFallbackShaderTag")); //効果ない (`.fallbackMaterial`効かない)
            if (!asset.fallbackMaterial)
            {
                drawingSettings.SetShaderPassName(1, new ShaderTagId("RendererListShaderTag_1")); //重複して描画される(意味は無さそう)
                drawingSettings.SetShaderPassName(2, new ShaderTagId("RendererListShaderTag_1"));
            }

            drawingSettings.fallbackMaterial = rendererListFallbackMaterial; //フォールバックされなかった

            rendererListOverrideMaterial.SetKeyword(new LocalKeyword(rendererListOverrideShader, "_OverrideShaderKeyword"), true); //効く (MaterialPropertyも効く)
            if (asset.overrideMaterial)
            {
                drawingSettings.overrideMaterial = rendererListOverrideMaterial; 
            }

            //`⟪drawingSettings¦material⟫.enableInstancing = true`、同一`Mesh`、同一`シェーダーバリアント`、同列`SortingCriteria` で、効くはず..
                //(`drawingSettings.enableInstancing`と`material.enableInstancing`の`2^2パターン`。`multi_compile_instancing`もやった。あと色々。試したけど変化なし)
            drawingSettings.enableInstancing = true; 
            // Debug.Log($"drawingSettings.enableInstancing: {drawingSettings.enableInstancing}"); //=>true (デフォルト)

            var rendererListParams = new RendererListParams(cullingResults, drawingSettings, filteringSettings);

            //各`ShaderTagId`(`tagValues`)毎の`∮RenderingState∮`(`stateBlocks`)を**オーバーライド**
            rendererListParams.isPassTagName = false; //falseは、SubShaderのTags{..}を見る
            // Debug.Log($"rendererListParams.isPassTagName: {rendererListParams.isPassTagName}"); //=>false (デフォルト)
            rendererListParams.tagName = new ShaderTagId("SubShaderTagName");
            rendererListParams.tagValues = new NativeArray<ShaderTagId>(2, Allocator.Temp)
            {
                [0] = new ShaderTagId("TagValue_0"), //RendererListShaderTag
                [1] = new ShaderTagId("TagValue_1")
            };
            rendererListParams.stateBlocks = new NativeArray<RenderStateBlock>(2, Allocator.Temp)
            {
                [0] = new RenderStateBlock(RenderStateMask.Depth)
                {
                    depthState = new DepthState(true, asset.depthTest? CompareFunction.LessEqual : CompareFunction.Always)
                },
                [1] = new RenderStateBlock(RenderStateMask.Depth | RenderStateMask.Blend)
                {
                    depthState = new DepthState(true, asset.depthTest? CompareFunction.LessEqual : CompareFunction.Always),
                    blendState = new BlendState()
                    {
                        blendState0 = new RenderTargetBlendState()
                        {
                            writeMask = ColorWriteMask.Blue | ColorWriteMask.Green,
                            sourceColorBlendMode = BlendMode.SrcAlpha,
                            destinationColorBlendMode = BlendMode.OneMinusSrcAlpha,
                            colorBlendOperation = BlendOp.Add,
                            sourceAlphaBlendMode = BlendMode.SrcAlpha,
                            destinationAlphaBlendMode = BlendMode.OneMinusSrcAlpha,
                            alphaBlendOperation = BlendOp.Add,
                        }
                    }
                }
            };

            RendererList rendererList = ctx.CreateRendererList(ref rendererListParams);

            cmd.DrawRendererList(rendererList);


            //new ShaderTagId("MotionVectors")===============================================================
            if (!asset.stopMotion) motionTimer += Time.deltaTime;
            motionTimer %= 2.0f; //0.0f～2.0fの範囲に収める
            var pos = rendererMotionObj_transform.localPosition;
            pos.x = Mathf.Sin(motionTimer * Mathf.PI); //-1.0f～1.0fの範囲に収める
            rendererMotionObj_transform.localPosition = pos;

            var motionDrawingSettings = new DrawingSettings(new ShaderTagId("MotionVectors"), sortingSettings);
            motionDrawingSettings.perObjectData = PerObjectData.MotionVectors;
                //現在の drawSettings では PerObjectMotionVectors が設定されていないため、FilterSettings.forceAllMotionVectorObjects は無視されます。
            filteringSettings.excludeMotionVectorObjects = asset.excludeMotionVectorObjects;
            filteringSettings.forceAllMotionVectorObjects = asset.forceAllMotionVectorObjects;
            var motionRendererListParams = new RendererListParams(cullingResults, motionDrawingSettings, filteringSettings);
            RendererList motionRendererList = ctx.CreateRendererList(ref motionRendererListParams);

            cmd.DrawRendererList(motionRendererList);
        }

        Vector2 ApplyAspect(Vector2 pos, float aspect)
        {
            if (aspect > 1.0)
            {
                pos.x /= aspect;
            }
            else
            {
                pos.y *= aspect;
            }
            return pos;
        }
        Rect ToPlaneRect(Rect rect)
        {
            Vector2 position = rect.position * 2.0f - Vector2.one; // 左下原点に変換
            Vector2 size = rect.size * 2.0f; // サイズを2倍
            return new Rect(position, size);
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

static class CommandBufferExtensions
{
    // AIコード補完=======================================================
    public class WireframeScope : IDisposable
    {
        CommandBuffer cmd;
        public WireframeScope(CommandBuffer cmd)
        {
            this.cmd = cmd;
            cmd.SetWireframe(true);
        }
        public void Dispose()
        {
            cmd.SetWireframe(false);
        }
    }
    public class NullScope : IDisposable
    {
        public void Dispose(){ }
    }
    public static IDisposable WithWireframe(this CommandBuffer cmd, bool enable = true)
    {
        if(enable)
            return new WireframeScope(cmd);
        else
            return new NullScope();
    }
    // ==================================================================

    // AI自動コード生成(SetKeywordとEnableScissorRect版も作ってください)====
    public class KeywordScope : IDisposable
    {
        CommandBuffer cmd;
        Material material;
        LocalKeyword keyword;
        public KeywordScope(CommandBuffer cmd, Material material, LocalKeyword keyword)
        {
            this.cmd = cmd;
            this.material = material;
            this.keyword = keyword;
            cmd.SetKeyword(material, keyword, true);
        }
        public void Dispose()
        {
            cmd.SetKeyword(material, keyword, false);
        }
    }
    public static KeywordScope WithKeyword(this CommandBuffer cmd, Material material, LocalKeyword keyword)
    {
        return new KeywordScope(cmd, material, keyword);
    }

    public class ScissorScope : IDisposable
    {
        CommandBuffer cmd;
        public ScissorScope(CommandBuffer cmd, Rect rect)
        {
            this.cmd = cmd;
            cmd.EnableScissorRect(rect);
        }
        public void Dispose()
        {
            cmd.DisableScissorRect();
        }
    }
    public static ScissorScope WithScissorRect(this CommandBuffer cmd, Rect rect)
    {
        return new ScissorScope(cmd, rect);
    }

    public class ViewportScope : IDisposable
    {
        CommandBuffer cmd;
        Rect prevViewport;
        public ViewportScope(CommandBuffer cmd, Rect rect)
        {
            this.cmd = cmd;
            cmd.SetViewport(rect);
        }
        public void Dispose()
        {
            cmd.SetViewport(new Rect(0, 0, Screen.width, Screen.height));
        }
    }
    public static ViewportScope WithViewport(this CommandBuffer cmd, Rect rect)
    {
        return new ViewportScope(cmd, rect);
    }
    // ================================================================
}