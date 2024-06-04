using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "SelfMadeSRP_Asset/SRP0", fileName = "SRP0_Asset_File")]
public class SRP0_Asset : RenderPipelineAsset{
    [MenuItem("Create/SRP/SRP0")]
    static void CreateAssetFile(){
        var so = ScriptableObject.CreateInstance<SRP0_Asset>();
        AssetDatabase.CreateAsset(so, "Assets/Script/MI_SRP0_Asset_File.asset");
    }
    protected override RenderPipeline CreatePipeline(){
        return new SRP0();
    }

    public class SRP0 : RenderPipeline{
        static int num = 0;
        const int RT_CNT = 4;
        static RenderTexture[] RTs = new RenderTexture[RT_CNT];
        const int cmd_CNT = 100000;
        const int addCmd_CNT = 1000000;
        static CommandBuffer addCmd;
        static CommandBuffer[] cmds = new CommandBuffer[cmd_CNT];
        [MenuItem("APITest/RT/CreateRTs")]
        static void CreateRTs(){
            Debug.Log("CreateRTs");
            for(int i = 0; i < RT_CNT; i++){
                RTs[i] = new RenderTexture(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm);
                //(1280, 1920, 0, FilterMode.Point, GraphicsFormat.R8G8B8A8_UNorm, 2, false, RenderTextureMemoryless.None, false);
            }
            RTs[0].name = "SetRT"; 
            Graphics.SetRenderTarget(RTs[0]); //[0],[0] 同じRTなら倍に増えない
            RTs[1].name = "SetRT"; 
            Graphics.SetRenderTarget(RTs[1]); 
            // Graphics.Blit(RTs[0], RTs[1]);
        }
        [MenuItem("APITest/RT/CreateRTs1")]
        static void CreateRTs1(){
            Debug.Log("CreateRTs1");
            //SetRenderTargetが実行される時、初めてRenderTextureのC++側のObjectに領域が確保されGPUのメモリ(VRAM)に記憶される。
                //VRAMに入り切らない(3.0GB)と共有GPUメモリ(メインメモリ)に記憶される。
            Graphics.SetRenderTarget(new RenderTexture(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm));
            //この場合C++側がリークしUnloadUnusedAssetsしないとDestroyされずGPUメモリを圧迫し続ける。
        }
        [MenuItem("APITest/RT/CreateRTs2")]
        static void CreateRTs2(){
            Debug.Log("CreateRTs2");
            var m = Object.FindObjectOfType<MonoBehaviour>();
            m.StartCoroutine(CreateRT2Coroutine());
            static IEnumerator CreateRT2Coroutine(){    //rtのC++が存在している時は常にDontDestoryOnLoadになっている
                //Getしただけではまだ0Bの割り当て 後、4分間でPlayModeとEditorModeでGetTemporaryのみ実行し続けたがこれだけで確保される事は無かった。
                RenderTexture rt = RenderTexture.GetTemporary(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm); Debug.Log("Done GetTemporary");
                // yield break;
                rt.name = "CreateRT2";
                yield return new WaitForSecondsRealtime(3.0f);
                //ここで実際にRTにデータが割り当てられる(0.59GB)
                Graphics.SetRenderTarget(rt); Debug.Log("Done SetRenderTarget");
                yield return new WaitForSecondsRealtime(3.0f);
                //ReleaseするとDestoryされC++側が開放される。(0.59GBが消える)
                RenderTexture.ReleaseTemporary(rt); Debug.Log("Done ReleaseTemporary");

                //ReleaseしたrtにSetRenderTargetしてもエラーも何も起こらない(DestroyされC++側が開放されてるから?)
                yield return new WaitForSecondsRealtime(3.0f);
                Graphics.SetRenderTarget(rt); Debug.Log("Done SetRenderTarget");
            }
        }
        static ScriptableRenderContext? cachedContex = null;
        static bool executeCreateRTs3 = false;
        [MenuItem("APITest/RT/CreateRTs3")]
        static void CreateRTs3(){
            Debug.Log("CreateRTs3");
            executeCreateRTs3 = true;
            // var m = Object.FindObjectOfType<MonoBehaviour>();
            // m.StartCoroutine(CreateRT2Coroutine());
            static IEnumerator CreateRT2Coroutine(){
                //InvalidOperationException: ScriptableRenderContextのインスタンスが無効です。これは、デフォルトコンストラクタを使用してインスタンスを構築した場合に発生します。
                // var contex = new ScriptableRenderContext();
                var cmd = new CommandBuffer();
                //Getしただけではまだ0Bの割り当て
                int rtIndex = Shader.PropertyToID("rtIndex");
                cmd.GetTemporaryRT(rtIndex ,1280 * 8, 1920 * 4, 0, FilterMode.Point, GraphicsFormat.R16G16B16A16_UNorm); Debug.Log("Done GetTemporaryRT");
                yield return new WaitForSecondsRealtime(3.0f);
                //ここで実際にRTにデータが割り当てられる(0.59GB)
                cmd.SetRenderTarget(rtIndex); Debug.Log("Done SetRenderTarget");
                yield return new WaitForSecondsRealtime(3.0f);
                //ReleaseするとDestoryされC++側が開放される。(0.59GBが消える)
                cmd.ReleaseTemporaryRT(rtIndex); Debug.Log("Done ReleaseTemporaryRT");
                yield return new WaitForSecondsRealtime(3.0f);
                //InvalidOperationExceptionです。NativeArrayは解放されており、アクセスすることはできません。
                cachedContex?.ExecuteCommandBuffer(cmd); Debug.Log("Done ExecuteCommandBuffer");
            }
        }
        //GetTemporary, ReleseTemporary, ctor
        //生成と開放と確認 :active, Create, Relese, Graphics.SetRenderTarget, IsCreated, Destory, 
        //ResolveAntiAliasedSurface (RenderTexture target//ここにresolved?), <-強制 bindTextureMS
            //DiscardContents, MarkRestoreExpected
            //SetRenderTarget (Rendering.RenderTargetIdentifier rt, Rendering.RenderBufferLoadAction loadAction, Rendering.RenderBufferStoreAction storeAction);
            //memorylessMode
        //Texture.GetNativeTexturePtr(), RenderTexture.GetNativeDepthBufferPtr(), RenderTexture.colorBuffer|depthBuffer.GetNativeRenderBufferPtr
        //imageContentsHash
        //updateCount, IncrementUpdateCount
        //Texture.isReadable, Instanceate Texture2D.GetPixel, Texture2D.GetPixels, ImageConversion.EncodeToEXR, ImageConversion.EncodeToJPG, ImageConversion.EncodeToPNG, Readableだとメインメモリに圧縮コピーが必要?
        //useMipMap, autoGenerateMips, GenereateMips, SetRenderTarget (Rendering.RenderTargetIdentifier rt, int mipLevel); mipMapBias
        //sRGB
        //currentTextureMemory, totalTextureMemory
        //Renderdoc

        [MenuItem("APITest/RT/RemoveRefRTs")]
        static void RemoveRefRTs(){
            Debug.Log("RemoveRefRTs");
            for(int i = 0; i < RT_CNT; i++){
                DestroyImmediate(RTs[i]); 
                RTs[i] = null;  //nullを入れてもオブジェクトがまだGCされてない
            }
        }
        [MenuItem("APITest/AddCmd/Add_Cmd_ClearRenderTarget")]
        static void Add_Cmd_ClearRenderTarget(){
            Debug.Log("Add_Cmd_ClearRenderTarget");
            // addCmd = CommandBufferPool.Get(); 
            addCmd = new CommandBuffer();
            for(int i = 0; i < addCmd_CNT; i++){
                addCmd.ClearRenderTarget(false, true, Color.blue);//cmdを積んだ分だけデータが増える
            }
        }
        [MenuItem("APITest/AddCmd/Release_addCmd")]
        static void Release_addCmd(){
            Debug.Log("Release_addCmd");
            CommandBufferPool.Release(addCmd);
        }
        [MenuItem("APITest/CommandBufferPool_Get")]
        static void CommandBufferPool_Get(){
            Debug.Log("CommandBufferPool_Get");
            for(int i = 0; i < cmds.Length; i++){
                cmds[i] = CommandBufferPool.Get(); //Getした分だけデータが増える
            }
        }
        [MenuItem("APITest/CommandBufferPool.Release")]
        static void CommandBufferPool_Release(){
            Debug.Log("CommandBufferPool.Release");
            for(int i = 0; i < cmds.Length; i++){
                CommandBufferPool.Release(cmds[i]); 
            }
        }
        [MenuItem("APITest/UnloadUnusedAssets")]
        static void UnloadUnusedAssets(){
            Debug.Log("UnloadUnusedAssets");
            Resources.UnloadUnusedAssets();
        }
        //EditorではOnValidateで実行?//PlayModeでは毎フレーム実行(多分)
        //Editorでは↑の時に?cameraのカラーバッファをblackにするClearRenderTargetが入っている?
        void CreateRTs3(ScriptableRenderContext context, Camera camera){
            if(!executeCreateRTs3) return;
            executeCreateRTs3 = false;
            Debug.Log("executeCreateRTs3");
            var cmd = new CommandBuffer();
            int rtIndex = Shader.PropertyToID("rtIndex");
            //GetTemporaryRTだけでも何回か実行すると一瞬確保された跡が見える..(cmd.GetTemporaryRTの場合は常にこれをしただけで確保される?)(PlayModeとEditorModeで確認)
            cmd.GetTemporaryRT(rtIndex ,1280 * 8, 1920 * 4, 0, FilterMode.Point, GraphicsFormat.R16G16B16A16_UNorm); Debug.Log("Done GetTemporaryRT");
            //何回か実行すると一瞬確保された跡が見える      //(1280 * 8 *1920 * 4 * 8) / (1024^3) = 0.5859375GB
            cmd.SetRenderTarget(rtIndex); Debug.Log("Done SetRenderTarget"); 
            //Releaseしなくても勝手に解放されるみたい
            cmd.ReleaseTemporaryRT(rtIndex); Debug.Log("Done ReleaseTemporaryRT");
            context.ExecuteCommandBuffer(cmd); Debug.Log("Done ExecuteCommandBuffer");
            context.Submit(); Debug.Log("Done Submit"); //忘れてた..
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras){
            // CreateRTs3(context, cameras[0]); return;
            cachedContex = context; //多分contextを変なタイミングで使うとエラー出る
            for(int i = 0; i < cameras.Length; i++){
                Camera camera = cameras[i];
                context.SetupCameraProperties(camera);
                // CommandBuffer cmd = CommandBufferPool.Get("CBP0");
                CommandBuffer cmd1 = CommandBufferPool.Get("CBP0");
                // Debug.Log($"cmd == cmd1: {cmd == cmd1}");//=>false nameは同じでも違うcmd
                
                // int renderTargetHandle = Shader.PropertyToID("renderTargetHandle");
                // cmd.GetTemporaryRT(renderTargetHandle, 1280, 1920, 0, FilterMode.Point, GraphicsFormat.R8G8B8A8_UNorm, 2, false, RenderTextureMemoryless.None, false);
                // context.ExecuteCommandBuffer(cmd);

                // cmd.SetRenderTarget(camera.targetTexture);
                // cmd.SetRenderTarget(renderTargetHandle);
                // cmd.ClearRenderTarget(false, true, Color.gray);

                // cmd1.SetRenderTarget(camera.targetTexture);
                cmd1.ClearRenderTarget(false, true, Color.blue);
                // context.ExecuteCommandBuffer(cmd);
                // context.ExecuteCommandBuffer(cmd1);
                // if(num % 2 == 0){
                    // context.ExecuteCommandBuffer(cmd);
                    // context.ExecuteCommandBuffer(cmd1);
                // }else 
                // if(Input.GetKey(KeyCode.F))
                { //何故かGetKeyDownだと反応しない
                    // Debug.Log("f"); 
                    context.ExecuteCommandBuffer(cmd1);
                    // context.ExecuteCommandBuffer(cmd);
                }
                num++;
                // CommandBufferPool.Release(cmd);
                CommandBufferPool.Release(cmd1);
                // cmd.Clear();
                // cmd.SetRenderTarget(camera.targetTexture);
                // context.ExecuteCommandBuffer(cmd);
                camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters);
                CullingResults cullingResults = context.Cull(ref cullingParameters);
                SortingSettings sortingSettings = new SortingSettings(camera){criteria = SortingCriteria.CommonOpaque};
                DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRP0Unlit"), sortingSettings);
                FilteringSettings filteringSettings = new FilteringSettings(new RenderQueueRange((int)RenderQueue.Geometry, (int)RenderQueue.GeometryLast));
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
                // context.ExecuteCommandBuffer(cmd);
                // cmd.ReleaseTemporaryRT(renderTargetHandle);

                // context.ExecuteCommandBuffer(cmd);
            }
            context.Submit();
        }
    }
}
