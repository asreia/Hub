    // readonly new ProfilingSampler profilingSampler = new ProfilingSampler("Ssf");
    // readonly ShaderTagId ssfDepthShaderTagId = new ShaderTagId("SsfBillboardSphereDepth_");//==context.DrawRenderers用?=============================================
                                                        //===cmd.GetTemporaryRT(PropertyToID(←混乱の元?)nameIDしか無い) -> cmd.SetRenderTarget(RenderTargetIdentifier) か RenderTargetHandle
                                                        //===↓->cmd.SetGlobalTexture("_SsfDepthNormalTexture", depthNormalTargetHandle.id(PropertyToID));
    //             int tempTextureIdentifier = Shader.PropertyToID("_PostEffectTempTexture");           //↑PropertyToIDはintでRenderTargetIdentifierはintからのimplicitがある
    // readonly RenderTargetHandle depthTargetHandle;//=================RenderTargetHandle は一時レンダーテクスチャ(TemporaryRT)の確保・解放の名前(識別子)として利用する値
    // depthTargetHandle.Init("_SsfDepthTexture");      //↑はURPのみ
        // this.renderPassEvent = renderPassEvent;
        // this.material = material;
        // litPass = material.FindPass("SsfLit");
    // var depthTargetDescriptor = new RenderTextureDescriptor(w, h, RenderTextureFormat.RFloat, 0, 0){msaaSamples = 1};
    // cmd.GetTemporaryRT(depthTargetHandle.id, depthTargetDescriptor, FilterMode.Point);//===============================================↓↓
                                                    //普通にpublic static RenderTexture RenderTexture.GetTemporary(int width, int height,,,,);があるが?これでいいんじゃないの?
                                                        //cmdではなく即時実行なので効率悪い?
    // ConfigureTarget(depthTargetHandle.id);
    // ConfigureClear(ClearFlag.All, Color.black);
    // cmd.ReleaseTemporaryRT(depthTargetHandle.id);//=====Getで確保しReleaseで開放?GPU側のメモリの確保と開放をしている?(new RenderTextureとこれで確保してメモリを見る)
                                                    //TemporaryRTはnew RenderTextureと違ってアキュームレータの様な存在?(RenderTexture -> Texture -> UnityObjectだがTemporaryRTはただのID)
                                                    //テクスチャやメッシュはGPU側で確保と開放を自動的に行っている? //↑new RenderTextureはTexture2Dと同じunityObjectなのでnewした直後にGPU転送?
                                                        //UnityObjectのロード,アンロードでGPU側も確保,開放している?(Async Upload Pipeline(AUP)で毎フレーム少量ずつGPUに送る事が出来る)
    
    // var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
    ////ssfDepthShaderTagIdがリストだとマルチパスレンダリングになる?(//Pass1 -> Pass2 -> Pass3 の順で連続で描画してくれる?)
    // var drawSettings = CreateDrawingSettings(ssfDepthShaderTagId, ref renderingData, sortFlags);//==renderingDataはuniformの設定に使われる?
    // drawSettings.perObjectData = PerObjectData.None;
    // context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);//=主にカリングとシェーダパス名で描画対象を選択している==================================
    // cmd.Blit(currentSource.id, currentDestination.id, material, material.FindPass("DownSampling"));//===============================================
    // cmd.SetGlobalMatrix("_MatrixClipToView", GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.projectionMatrix, true).inverse);
    // context.ExecuteCommandBuffer(cmd);//===============================================
    // var cmd = CommandBufferPool.Get(profilingSampler.name);//========================Core RP 多分newするとGC発生するから使いまわしてるだけnameを付けるとFrameDebuggerで見れる確か
    // CommandBufferPool.Release(cmd);
        ////RenderingDataはScriptableRendererがCameraにアタッチされるのでそのCameraによるRenderingDataが入っている?(CullingとかLightとかShadowとか)//===============================================
        // public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
        //     currentPass.SetRenderTarget(renderer.cameraColorTarget);
        //     renderer.EnqueuePass(currentPass);//===============================================
        // }
    // var drawSettings = CreateDrawingSettings(ObstacleShaderTagId, ref renderingData, sortFlags);
    // drawSettings.overrideMaterial = zwriteMaterial;// "UniversalForward"を"Hidden/InternalZWriteOnly"に変えている? //こういうMaterialだけのShaderPassを指定しない系のPassの選択のされ方が謎
    // var height = camera.targetTexture?.height ?? Screen.height;
    // var h = cameraData.camera.scaledPixelHeight / 2;
    // cmd.GetTemporaryRT(Shader.PropertyToID("blurTempRT2"), w, h, 0, FilterMode.Point, RenderTextureFormat.Default);//blurTempRT2は存在しない?存在しなくてもいい?ハッシュ?
    // CommandBuffer commandBuffer = new CommandBuffer();
    // commandBuffer.name = "Clear to Red";
    // commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget//mipLevel,cubemapFace,depthSliceがあるオーバーロードメソッドはRenderTargetIdentifierの生成時に設定された値を上書きする?
                                                                            //↑RenderTargetIdentifierでなくGetTemporaryRTやnew RenderTextureのテクスチャ生成時?
                                                                                //↑いや、ctorのRenderTargetIdentifier(Rendering.RenderTargetIdentifier renderTargetIdentifier, int mipLevel, CubemapFace cubeFace, int depthSlice)かな?
                                                                        //Rendering.RenderBuffer❰Load|Store❱Action.❰Load|Store❱はレンダリング結果情報を❰前にLoad|後にStore❱するかどうか?
                                                                            //でもレンダリング結果は普通は保存され読み込むのでは?(モバイル用?)https://blog.uwa4d.com/archives/TechSharing_198.html
    // commandBuffer.ClearRenderTarget(true, true, Color.red, 1f);//===============================================
    // GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

    // var ssfMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("SampleSsf/Hidden/Ssf"));
    // ssfMaterial.enableInstancing = true;
    // ssfMaterial.SetColor("_Tint", settings.Tint);
    // ssfMaterial.SetColor("_AmbientColor", settings.AmbientColor);
//CBTest.cs
        // renderTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0);
        // commandBuffer.SetRenderTarget(new RenderTargetIdentifier(renderTexture));//==RenderTargetIdentifierはctor(type(Builtin),name,nameID,tex,Identifier)しかなく
                                                                                        //、(ctorと同じ数-1と同じimplicitはある)
                                                                                        //cmd.SetRenderTargetやcmd.SetGlobalTextureにRTを指定するために使われる
                                                                                            //とにかくuniformかレンダーターゲットに指定するために使われる?
                                                                                        //RenderTargetIdentifier単なる↑を指定するためのTextureの値?
                                                                                            //implicitがあるため気にする必要がないものかも?
                                                                                                //RenderTargetIdentifier(Rendering.RenderTargetIdentifier renderTargetIdentifier, int mipLevel, CubemapFace cubeFace, int depthSlice)以外は
// CommandBuffer.DrawMesh (
//       mesh,
//       Matrix4x4.TRS (new Vector3 (0, 0, 0), Quaternion.identity, Vector3.one),
//       material //パス指定なし
//     );

// FilterMode.Trilinear FilterModeはWebGLで言うテクスチャパラメータ?
    // Bilinear とほぼ同じだが、ミップマップレベルにおいてブレンドして表示する
    // RenderTexture はミップマップをサポートしていないので、自動的に Bilinear となります。
//enableRandomWrite https://qiita.com/keito_takaishi/items/eccc59ba30ac5e281e28
    //ComputeShaderのRWTextureユニフォーム変数?で使えるらしい
//enum RenderTextureMemoryless{None,Color,Depth,MSAA}
    //RenderTexture.antiAliasingが1に設定されている場合、レンダーテクスチャのカラーピクセルはメモリレスになります。
    //メモリを無くせる?少なくできる?

// var depthTargetDescriptor = new RenderTextureDescriptor(w, h, RenderTextureFormat.RFloat, 0, 0){msaaSamples = 1};
// cmd.GetTemporaryRT(depthTargetHandle.id, depthTargetDescriptor, FilterMode.Point);

//ShaderTagId("LightMode")は複数のPassから選択しうる?(QueueやLODによって選択?)

//new RenderStateBlock(RenderStateMask.Nothing);

//Material, Shader, context, ComandBuffer, GL, Graphics
//＠❰Get❱TemporaryRT, RenderTargetIdentifier, RenderTargetHandle
//ShaderTagIdリスト, 
//Mesh, DrawMesh, MeshRenderer,
//GL.GetGPUProjectionMatrix
//drawSettings.overrideMaterial = zwriteMaterial;
//CoreUtils.CreateEngineMaterial(Shader.Find("SampleSsf/Hidden/Ssf"));
//ScriptableRenderPass.renderPassEvent


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Toguchi.Rendering
{
    [ExecuteInEditMode]
    [CreateAssetMenu(menuName = "ToonRenderPipelineAsset")]
    public class ToonRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private float modelRenderResolutionRate = 0.7f;
        public float ModelRenderResolutionRate => modelRenderResolutionRate;

        protected override RenderPipeline CreatePipeline()
        {
            return new ToonRenderPipeline(this);
        }
    }
}