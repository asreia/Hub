# Graphics自由メモ

- メモ
  - Unityのレンダリングシステムをコードで複雑度のレベル順に表す
  - スクリプトデバッキングする
  - [【Unity】RenderDocの使い方をステップバイステップで解説～初歩からディープな使い方まで～](https://developers.cyberagent.co.jp/blog/archives/38917/)
  - ContextContainer, RenderingData, ScriptableRenderer, CommandBuffer, ScriptableRenderContext, RTHandles
  - 最終的にRenderPassが書ければ良い?
  - よく使われる、データ構造(フィールドデータ), モジュールの使い方(RTHandle,cmd,ctxなど) を理解する。
    - RenderGraph,XR,NativeRenderPassは保留
    - RenderDoc¦PIX, スクリプトデバッグなどを使う
  - まずは、大まかな流れをdraw.ioする
    - デバッグ系を触る
    - 各モジュールの使い方を覚える 今ココ
      - RTHandle, RT, Texture, RenderTargetBufferSystem, RenderTargetIdentifier, RenderTargetBinding?, MaterialPropertyBlock?
      - cmd, CommandBufferPool, ctx
      - Shader, ShaderKeyword?
      - RenderStateBlock, frameData
      - Material, MeshRenderer, SRPass, ForwardLights, UR
      - UniversalCameraData => ScriptableCullingParameters => CullingResults
      - CommandBuffer
        - cmd.SetGlobalVector, cmd.SetGlobalTexture, cmd.SetKeyword, cmd.SetGlobalConstantBuffer, cmd.SetGlobalInt, cmd.SetViewProjectionMatrices, cmd.SetGlobalMatrix, cmd.SetGlobalVectorArray, cmd.SetRenderTarget, cmd.Clear, cmd.DrawProcedural, cmd.DrawRendererList, cmd.SetViewport, cmd.DrawMesh, cmd.SetGlobalDepthBias, cmd.DisableScissorRect, cmd.SetGlobalMatrixArray, cmd.SetGlobalFloat, cmd.SetWireframe, cmd.SetGlobalInteger, cmd.Blit
    - ●//Playerコマ再生＆スクリプトデバッグで要素を一つづつ変えながらRenderDocする?
    - データ構造を把握する
    - 再度、処理を追う?
  - サブメッシュに対して1マテリアル,1DrawCall?
  - シェーダーキーワードも入力と思う事もできる(PreProcessだけど)
  - cmd.GetTemporaryRT(nameID,..)はnameIDにプールまたは生成したリソースをC#&Shaderバインドし、SetRenderTarget(nameID)でリソース確保される遅延確保で
        ReleaseTemporaryRT(nameID)を呼ばなくてもFrameの最後までにはReleaseしリソースがプールに返され、数フレームしても使用されないとプールからも削除され
        リソースがメモリからアンロードされる (推測)
  - //●●RTHandleからTextureクラスまで含むクラス図を作る。.ctorとGetTemporaryのオーバーロードを言語表現
  - ScriptableRenderContext
    - ExecuteCB, Submit
    - CreateRendererList系, Draw系
    - Cull, SetupCameraProperties
    - NativeRenderPass関係, XR関係
  - DirectX12の新機能
    - DirectX レイトレーシング
    - Mesh Shader
    - VRS(可変解像度シェーディング)(foveated rendering)
      - Per Draw(DrawCall毎), Per Primitive(VertexShaderでSV_ShadingRate設定), Per Tile(R8_UINT(元RTSize/16x16のサイズ))で、何ピクセルを1ピクセルとして描画するか指定できる
    - Sampler Feedback
      - 前のフレームのサンプリング情報をFeedbackして使う?
      - SFSでSVTをGPUドライバレベルで最適化できる。SFS,SVTはテクスチャの遮蔽と距離によるMipLv選択により、必要なテクスチャのタイルのみロードできる。
    - WorkGraphはRenderGraphに似ているが、GPU駆動でありComputeShaderのみ対応で、Passの結果により次に実行するPassを分岐したり止める事ができる?
  - **色の規格**
    - **bit数**
      - bit数が多いほど**光の強度を細かく記憶**できるので、HSVの**Value(明度)**や杆体への刺激が豊かになる。が、
        ディスプレイ側の**nit数**が足りないと、細やかな光の強度を表示できない。(bit数,カーブに対するnit数が規定されているが**基本的にはマップ**される)
        HDRで撮った映像を**Rawデータ**とし、トーンマッピングなどで欲しい**明度を抽出**し、sRGBにデジタル現像することもできる
    - **色空間**
      - 色空間はその規格が表現できる色の範囲を規定する。色空間は**RGB-XYZ変換行列**で表す事ができ、その中のRGBベクトルのx,y成分が**色度図**で確認できる。
        色空間はHSVの**HS平面(鮮やかさ)**や錐体への刺激の豊かさの表現力を表す。
        **カラマネアプリ**は**広い色空間**(bit数も多い?)を持ち、撮ったデータはその色空間に変換し、その広い色空間でtarget規格向けの多彩な編集をすることができる。
        色空間も何もしなければ**targetにマップ**される。
    - **カーブ**
      - カーブはHSVのValue(明度)の関係で、人の目は暗部に敏感で明部に鈍感になる性質に対応して、**暗部の情報量を多く**するためにガンマカーブ作る。
        カーブは**想定している最大nit数**(基本的に想定しているだけ)に応じて**形が変わる**。SDR:100nit, HDR(HLG):1,000nit, HDR(PQ):10,000nit
        カーブは圧縮ファイルのようなもので、実際に使用(色空間変換などで)される前に**逆カーブを掛けLinerに変換**する必要がある。
  - G+FBOカリング(Grid+フラスタム,BackFace,Occlusion)
  - GraphicsModules
    - ok:RenderTexture, ok:RenderTargetIdentifier, ok:RTHandle, o:RenderGraph, ok:CommandBuffer, frameData, ok:ScriptableRenderContext
    - Camera, Light, MeshFilter, MeshRenderer, Material, Shader
    - o:FrameDebugger, o:RenderDoc, o:RenderingViewer, o:Profiler,
    - ●Internal_**Draw**⏎『全て、BatchRendererGroup(BRG)(UnityのMDI機構?)に置き換えれるらしい。(rRG->drawCommandsでUnity描画パイプラインに差し込まれる?)(GPU Instancing(SSBO(unity_DOTSInstanceData <= GraphicsBuffer?))、Job/NativeArray、カメラカリング、SRPBatcherフレンドリー(Meshを固定しInstanceコールする?(**SRPBatcherはCbufferとMeshを変える**))、Hybrid Renderer V2の基盤)(シェーダーはDOTS Instancingをサポートしている必要がある, BatchID毎にInstanceコールしている)(https://blog.virtualcast.jp/blog/2019/10/batchrenderergroup/)
      - bRGにMeshとMaterial(CBuffer,DOTS Instancingシェーダー)と\[メタデータ(PropertyToID)とGraphicsBuffer]を渡し、カメラカリングし、BatchID毎にInstance描画する
      - DOTS Instancingシェーダー: SV_InstanceID -> uniform InstanceIndex = uint property\[SV_InstanceID] -> graphicsBuffer\[InstanceIndex] でInstanceデータにアクセス?
      - >ノート: Shader Graph と、Unity が URP と HDRP で提供するシェーダーは、DOTS Instancing をサポートしています。
      - (https://docs.unity3d.com/ja/2023.2/Manual/batch-renderer-group-creating-draw-commands.html)
      - (https://gist.github.com/AlexMerzlikin/ce756564414985039feff71ee79fefe6)
      - [SRPButcherとBRGとInstancing](https://gamedev.center/how-to-write-a-custom-urp-shader-with-dots-instancing-support/)
      - [GPU インスタンシング対応シェーダーの作成](https://docs.unity3d.com/ja/2023.2/Manual/gpu-instancing-shader.html)
  - 座標系とYupなどの違いは、ある軸を*-1するか軸を入れ替えると左手と右手が入れ替わりYupなども変わる?(ベクトル,行列の基底ベクトル:要素反転か入れ替え、クォータニオン:軸反転)
    - 多分、座標系の違いはCS空間しか受けない(CS空間で違いを吸収する(Z軸反転?))
  - HLSLPROGRAM **API組み込み関数の違い**、**座標系の違い**、**バリアント**、自作メッシュ描画するときAPVなどの機能を取ってこれるか
    - Unityは、uniformとShaderKeywordを設定し、Tags{..}を見てドローコールする
    - 謎マクロ(UNITY_SETUP_STEREO～やDEBUG_DISPLAY)などを無視できるか
    - CgからHLSLで何が変わっているか: ただ**ShaderLibraryが変わっているだけ**
      - HLSL版のライブラリ(#includeファイル)
        - UnityCG.cginc => Core.hlsl
    - マクロ
      - プラットフォーム吸収系 (DirectX系とOpenGL系で、使用する組み込み関数 と 座標系が違う(CS空間のみ?))
        - 座標系の違い吸収
          - uv.y, pos.z の反転
        - API組み込み関数の違い吸収
          - TEXTURE2D(texture)
          - SAMPLER(sampler_texture) //C#のどこで設定している?
          - SAMPLE_TEXTURE2D_X(texture, sampler_texture, uv)
      - Unity組み込みヘルパー系 (マクロ=>関数でも関数=>マクロでもやっていることは変わらん?)
        - インスタンシング Material:GPUインスタンシングを有効にする で、ドローコールがインスタンシングになる?
        - CBuffer
        - BRG
    - 関数
    - [セマンティクス](https://docs.unity3d.com/ja/2023.2/Manual/SL-ShaderSemantics.html)(https://qiita.com/sune2/items/fa5d50d9ea9bd48761b2)
    - #pragma (https://docs.unity3d.com/ja/2023.2/Manual/SL-PragmaDirectives.html)
      - enable_d3d11_debug_symbols: 最適化を無効にして、シェーダーデバッグシンボルを生成
      - バリアント以外、固定値(target 5.0)
    - Tags{..}
      - LightMode (https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.universal@14.0/manual/urp-shaders/urp-shaderlab-pass-tags.html)
      - RenderPipeline
    - [様々なグラフィックス API のシェーダーの作成](https://docs.unity3d.com/ja/2023.2/Manual/SL-PlatformDifferences.html)
    - Lit Shader見る(マクロとバリアント)。Shaderのテンプレートを作る
      - コンパイル時に設定される#defineとランタイム時にしていされるShaderKeywordがある?
    - [#include_with_pragmas: 通常の#include と #pragma も読み込む](https://docs.unity3d.com/ja/2023.2/Manual/shader-include-directives.html)
    - [shader_feature_local_fragment](https://light11.hatenadiary.com/entry/2021/11/18/200645)
    - [バリアント](https://qiita.com/piti5/items/1f8d3bdfe5a64e478c7e)
    - [ShaderGraphコンパイル後のアセンブリコードを読んでみる](https://zenn.dev/r_ngtm/articles/shadergraph-assembly)
    - enable_d3d11_debug_symbolsでHLSLを取得して改良する手はある？
    - [C言語のマクロについてまとめる](https://qiita.com/Yuzu2yan/items/0e7bcf2e8bc1aa1c030b)
    - multi_compile _ KEYWORD1 KEYWORD2 <=> #define ⟪Ø¦KEYWORD1¦KEYWORD2⟫
    - UnityInputマクロ系、Unityヘルパー関数系、サンプリング,デコード,座標系変換系、プラットフォーム吸収系、GetAdditionalLight(index, posWS)
      - 関数やマクロの中で#defineが要求される関数 や シェーダー固有の関数 は使いたくない
    - [positionNDC](https://ny-program.hatenablog.com/entry/2021/10/20/202020)
    - [その70 完全ホワイトボックスなパースペクティブ射影変換行列](http://marupeke296.com/DXG_No70_perspective.html)
    - 構造として理解する。(特にマクロが複雑そうに見せている)
      - 最終的には普通にvertexとfragmentを実行しているに過ぎない
        - APIにより呼び出す**API組み込み関数**が変わる。**座標系の違い**の吸収
      - 頂点単位計算、画素単位計算、投影系、ポストプロセス
      - フレームデバッガーの**ShaderKeyword確認**、追加ライトを付ける(スポットとポイント)
    - #include 『#includeは、深さ探索でどんどんトップレベルへ展開していく。その過程で#define,#ifなどが働く
      - Lit.shader
        『❰#ifあり❱は、実際に使うにはShaderKeywordが必要で、Unity,URP,Shaderの環境に影響を受ける        common系,Random,Sampling,Packing
        - ForwardLit
          - core/ShaderLibrary/FoveatedRenderingKeywords.hlsl
          - universal/ShaderLibrary/RenderingLayers.hlsl
          - universal/ShaderLibrary/ProbeVolumeVariants.hlsl
          - universal/ShaderLibrary/DOTS.hlsl
          - universal/Shaders/**LitInput**.hlsl
            - universal/ShaderLibrary/Core.hlsl
              『Inputと基礎関数群とサンプルなど
              - core/ShaderLibrary/Common.hlsl
              - core/ShaderLibrary/Packing.hlsl
              - core/ShaderLibrary/Version.hlsl
              - universal/ShaderLibrary/Input.hlsl
          - universal/Shaders/**LitForwardPass**.hlsl
            - universal/ShaderLibrary/**Lighting**.hlsl
              『#ifあり『URP/Lit.shaderなどで、実際にフラグメントの色を決める関数がある。(UniversalFragment⟪PBR¦BlinnPhong⟫⊃Lighting⟪Lambert¦Specular¦PhysicallyBased⟫)
              - universal/ShaderLibrary/BRDF.hlsl
                『#ifあり『Lighting.hlslのライティング関数内で使う具体的なライティング関数群がある。(BRDFData, EnvironmentBRDF(..), DirectBRDF(..))
                - core/ShaderLibrary/BSDF.hlsl
                  『PBRの原始的な光の計算をする関数がある。(F_Schlick(..),V_SmithJointGGX(..) (アセンブラレベルで最適化している)) (多分BRDF.hlslでは使われていない)
                  - core/ShaderLibrary/Color.hlsl
                    『色空間変換系の関数がある。(SRGBToLinear(..), RgbToHsv(..), NeutralTonemap())
                    - core/ShaderLibrary/ACES.hlsl
                - core/ShaderLibrary/CommonMaterial.hlsl
                  『BRDF.hlslで使うヘルパーかユーティルな関数が多い?(LerpWhiteTo(..),PerceptualRoughnessToRoughness(..))
                - universal/ShaderLibrary/SurfaceData.hlsl
                  『fragmentでテクスチャ参照して格納する構造体が定義されてるだけ(struct SurfaceData{albedo; specular; metallic; smoothness; normalTS;..})
              - universal/ShaderLibrary/Debug/Debugging3D.hlsl
                『Rendering Debugger使用時、#if defined(DEBUG_DISPLAY)内で使うデバッグのカラーを出力する関数がある
                  『(CanDebugOverrideOutputColor(..)⊃CalculateDebugShadowCascadeColor(..))
              - universal/ShaderLibrary/GlobalIllumination.hlsl
                『間接光に関する関数がある。LightProbe(APV),LightMapの取得も含まれている?
                  『(GlobalIllumination(..), SampleSH⟪Vertex¦Pixel⟫, SampleProbeVolume⟪Vertex¦Pixel⟫, BoxProjectedCubemapDirection(..))
                - core/ShaderLibrary/EntityLighting.hlsl
                - core/ShaderLibrary/ImageBasedLighting.hlsl
                - core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl (APV)
              - universal/ShaderLibrary/RealtimeLights.hlsl
                『struct Light情報を取得する関数だけ。(int GetAdditionalLightsCount(), Light Get⟪Main¦Additional⟫Light(..), LIGHT_LOOP_⟪BEGIN¦END⟫)
                - universal/ShaderLibrary/Input.hlsl"
                  『Unityが設定するCBufferなどがあった。(struct InputData{..}, _MainLightColor,_AdditionalLightsBuffer, _FPParams0, UNITY_MATRIX_M)
                  - universal/ShaderLibrary/UnityInput.hlsl"
                  - core/ShaderLibrary/UnityInstancing.hlsl"
                  - universal/ShaderLibrary/UniversalDOTSInstancing.hlsl"
                - universal/ShaderLibrary/Shadows.hlsl"
                  『Shadowに関する関数がある。(MainLightRealtimeShadow(..), SampleShadowmap(..))
                - universal/ShaderLibrary/LightCookie/LightCookie.hlsl"
                - universal/ShaderLibrary/Clustering.hlsl"
                  『USE_FORWARD_PLUS用関数
              - universal/ShaderLibrary/AmbientOcclusion.hlsl
              - universal/ShaderLibrary/DBuffer.hlsl
            - universal/ShaderLibrary/LODCrossFade.hlsl

- コードメモ

```UnityShader
//https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.universal@14.0/manual/writing-shaders-urp-unlit-texture.html
// このシェーダーはメッシュ上にテクスチャを描画します。
Shader "Example/URPUnlitShaderTexture"
{
    // _BaseMap 変数はマテリアルの Inspector に Base Map フィールドとして
    // 表示されます。
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // uv 変数には特定の頂点のテクスチャにおける UV 座標が
                // 含まれます。
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // uv 変数には特定の頂点のテクスチャにおける UV 座標が
                // 含まれます。
                float2 uv           : TEXCOORD0;
            };

            // このマクロは _BaseMap を Texture2D オブジェクトとして宣言します。
            TEXTURE2D(_BaseMap);
            // このマクロは _BaseMap テクスチャのサンプラーを宣言します。
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                // 以下の行では、フラグメントシェーダーで _BaseMap 変数を
                // 使用できるように _BaseMap_ST 変数を宣言します。タイリングおよび
                // オフセットを機能させるために _ST サフィックスが必要です。
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // TRANSFORM_TEX マクロはタイリングとオフセットの
                // 変換を行います。
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // SAMPLE_TEXTURE2D マクロは指定されたサンプラーでテクスチャを
                // サンプリングします。
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return color;
            }
            ENDHLSL
        }
    }
}
```

```csharp
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

//SRP0_Asset.cs
TestShader = new Material(Shader.Find ("Unlit/TestShader"));
TestShader.SetPass(0);
Graphics.DrawMeshNow(CubeMesh, CubeTransform.position, CubeTransform.rotation);


//SRP0_Asset.cs
[CreateAssetMenu(menuName = "SelfMadeSRP_Asset/SRP0", fileName = "SRP0_Asset_File")]
public class SRP0_Asset : RenderPipelineAsset{
    protected override RenderPipeline CreatePipeline(){return new SRP0();}
    public class SRP0 : RenderPipeline{..}
}

int rtIndex = Shader.PropertyToID("rtIndex");
new CommandBuffer[cmd_CNT];
RTs[i] = new RenderTexture(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm);
Graphics.SetRenderTarget(RTs[0]);
Graphics.Blit(RTs[0], RTs[1]);
//SetRenderTargetが実行される時、初めてRenderTextureのC++側のObjectに領域が確保されGPUのメモリ(VRAM)に記憶される。
    //VRAMに入り切らない(3.0GB)と共有GPUメモリ(メインメモリ)に記憶される。
Graphics.SetRenderTarget(new RenderTexture(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm));
//この場合C++側がリークしUnloadUnusedAssetsしないとDestroyされずGPUメモリを圧迫し続ける。

//Getしただけではまだ0Bの割り当て 後、4分間でPlayModeとEditorModeでGetTemporaryのみ実行し続けたがこれだけで確保される事は無かった。
RenderTexture rt = RenderTexture.GetTemporary(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm); Debug.Log("Done GetTemporary");
//ここで実際にRTにデータが割り当てられる(0.59GB)
Graphics.SetRenderTarget(rt); Debug.Log("Done SetRenderTarget");
//ReleaseするとDestoryされC++側が開放される。(0.59GBが消える)
RenderTexture.ReleaseTemporary(rt); Debug.Log("Done ReleaseTemporary");

//GetTemporary, ReleseTemporary, ctor
//生成と開放と確認 :active, Create, Relese, Graphics.SetRenderTarget, IsCreated, Destory, 
//ResolveAntiAliasedSurface (RenderTexture target//ここにresolved?), <-強制 bindTextureMS
    //DiscardContents <- 破棄?, MarkRestoreExpected <- restore ok?
    //SetRenderTarget (Rendering.RenderTargetIdentifier rt, Rendering.RenderBufferLoadAction loadAction, Rendering.RenderBufferStoreAction storeAction);
        //Rendering.RenderBufferLoadAction.Load //以前RTのロード。一旦RenderTargetから外れてまた描画する時、以前に描画されたデータに上塗りする場合は必要?
        //Rendering.RenderBufferLoadAction.Clear //RTをクリア。何色でクリアされるかは不明(0クリア?)
        //Rendering.RenderBufferLoadAction.DontCare //RTに何もしない C++などでメモリ確保したときに初期化しないのと同じ?
        //Rendering.RenderBufferStoreAction 
    //memorylessMode
//Texture.GetNativeTexturePtr(), RenderTexture.GetNativeDepthBufferPtr(), RenderTexture.colorBuffer|depthBuffer.GetNativeRenderBufferPtr
//imageContentsHash
//updateCount, IncrementUpdateCount
//Texture.isReadable, Instanceate Texture2D.GetPixel, Texture2D.GetPixels, ImageConversion.EncodeToEXR, ImageConversion.EncodeToJPG, ImageConversion.EncodeToPNG, Readableだとメインメモリに圧縮コピーが必要?
//useMipMap /MipMapを有効にする、これをしないとRenderDocでMipLevelsが1のまま
    //autoGenerateMips //trueにするとレンダリングして、アクティブから外れてた時にMipMapが生成される? 
                        //falseの時、個別のMipMapLevelにレンダリングしたり、GenereateMips()が使える?
    //GenereateMips() //autoGenerateMipsがfalseの場合のみ有効で、実行するとその時にMipMapが生成される?
//SetRenderTarget (Rendering.RenderTargetIdentifier rt, int mipLevel); mipMapBias
//sRGB
//currentTextureMemory, totalTextureMemory

//GL.wireframe, GL.Clear, GL.Viewport
//Graphics.ExecuteCommandBuffer, Graphics.SetRenderTarget

addCmd = new CommandBuffer();
for(int i = 0; i < 1000000; i++){
    addCmd.ClearRenderTarget(false, true, Color.blue);//cmdを積んだ分だけデータが増える
}

static CommandBuffer[] cmds = new CommandBuffer[cmd_CNT];
for(int i = 0; i < cmds.Length; i++){
    cmds[i] = CommandBufferPool.Get(); //Getした分だけデータが増える
}
for(int i = 0; i < cmds.Length; i++){
    CommandBufferPool.Release(cmds[i]); 
}
Resources.UnloadUnusedAssets();

cmd.GetTemporaryRT(rtIndex ,1280 * 8, 1920 * 4, 0, FilterMode.Point, GraphicsFormat.R16G16B16A16_UNorm); Debug.Log("Done GetTemporaryRT");
cmd.SetRenderTarget(rtIndex); Debug.Log("Done SetRenderTarget"); 
//Releaseしなくても勝手に解放されるみたい
cmd.ReleaseTemporaryRT(rtIndex); Debug.Log("Done ReleaseTemporaryRT");
context.ExecuteCommandBuffer(cmd); Debug.Log("Done ExecuteCommandBuffer");
context.Submit(); Debug.Log("Done Submit");
cmd.Clear();

protected override void Render(ScriptableRenderContext context, Camera[] cameras){
    context.SetupCameraProperties(camera);
    cmd.SetRenderTarget(camera.targetTexture);

    camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters);
    CullingResults cullingResults = context.Cull(ref cullingParameters);
    SortingSettings sortingSettings = new SortingSettings(camera){criteria = SortingCriteria.CommonOpaque};
    DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRP0Unlit"), sortingSettings);
    FilteringSettings filteringSettings = new FilteringSettings(new RenderQueueRange((int)RenderQueue.Geometry, (int)RenderQueue.GeometryLast));
    context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
}


//SRP1_Asset.cs
Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice);
Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat, int pass);

//CopyTextureもConvertTextureもGraphics.CopyTextureを使っている?
//Graphics.CopyTexture -> CopyTexture -> ConvertTexture?

//EnableShaderKeyword,DisableShaderKeyword,DisableKeyword

//DrawMeshInstanced,DrawProcedural,DrawRenderer

//SetGlobalFloat

//SetViewport,EnableScissorRect

//SetViewProjectionMatrices

// SetRenderTarget  "set active render target" コマンドを追加します。
// ClearRenderTarget    \"clear render target" コマンドを追加します。
// GenerateMips レンダーテクスチャのミップマップレベルを生成します。
// GetTemporaryRT   "get a temporary render texture" コマンドを追加します。
// GetTemporaryRTArray  「一時的なレンダリングテクスチャ配列を取得する」コマンドを追加します。
// ReleaseTemporaryRT   "release a temporary render texture" コマンドを追加します。
// ResolveAntiAliasedSurface    アンチエイリアスされたレンダーテクスチャを強制的に解決します。
// クリア   バッファのすべてのコマンドをクリアします。
// SetGlobalDepthBias   グローバル深度バイアスを設定するコマンドを追加します。
    //Zファイティングの回避。D3D11よりラスタライザのDepthBiasとSlopeScaledDepthBiasの設定だと思うSetGlobalDepthBias (float bias, float slopeBias); DepthBiasClampは?
// SetExecutionFlags    コマンドバッファの実行方法を説明するフラグを設定します。
    //互換性のないコマンドが追加された場合に例外をスローします。ScriptableRenderContext|Graphics.ExecuteCommandBuffer"Async"
// IncrementUpdateCount TextureのupdateCountプロパティをインクリメントします。
    //https://docs.unity3d.com/ja/2018.4/ScriptReference/Texture.IncrementUpdateCount.html
// BeginSample  プロファイルのサンプリングを開始するコマンドを追加します。
    //コマンドバッファがこのポイントに達したときに開始するようにパフォーマンスプロファイリングサンプルをスケジュールします。
    //これは、コマンドバッファ内の1つ以上のコマンドによって費やされたCPUおよびGPU時間を測定するのに役立ちます。
        //プロファイラで見れる?
    // EndSample    プロファイルのサンプリングを終了するコマンドを追加します。

//LateLatch(レイトラッチ?)URPのシェーダープロパティのレイトラッチと言う機能に関するコマンドの様だが、
    //CommandBufferのクラスに存在しない(2021.2の新機能?)上に情報が全く無いので見なかったことにする。
//GraphicsFence?GPUFences? GPUのなんらかのコマンドの実行の完了を通知する機能のようだ?
//Async GPU Readback https://youtu.be/7tjycAEMJNg?t=4540
    // RequestAsyncReadback                 非同期GPUリードバック要求コマンドをコマンドバッファーに追加します。
    // RequestAsyncReadbackIntoNativeArray  非同期GPUリードバック要求コマンドをコマンドバッファーに追加します。
    // RequestAsyncReadbackIntoNativeSlice  非同期GPUリードバック要求コマンドをコマンドバッファーに追加します。    
    // WaitAllAsyncReadbackRequests 「AsyncGPUReadback.WaitAllRequests」コマンドをCommandBufferに追加します。
//コンピュートシェーダー SetGlobalConstantBuffer,CopyBuffer
//RandomWrite?
    // ClearRandomWriteTargets  シェーダーモデル4.5レベルのピクセルシェーダーのランダム書き込みターゲットをクリアします。
    // SetRandomWriteTarget シェーダーモデル4.5レベルのピクセルシェーダーのランダム書き込みターゲットを設定します。
// SetSinglePassStereo  カメラのシングルパスステレオモードを設定するコマンドを追加します。
    //このプロパティは、バーチャルリアリティが有効になっている場合にのみ使用されます。VRのレンダリング方法をSinglePassStereoMode列挙型で指定?
// SetKeyword   UnityEngine.Rendering.GlobalKeywordまたはUnityEngine.Rendering.LocalKeywordの状態を設定するコマンドを追加します。
    //CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap")); https://tips.hecomi.com/entry/2019/10/27/152520
    //マテリアルのプロパティに値を設定する?SetGlobal~~の万能型?//cmdに無かった
// EnableKeyword    UnityEngine.Rendering.GlobalKeywordまたはUnityEngine.Rendering.LocalKeywordを有効にするコマンドを追加します。
    //cmdに無かった
// DisableKeyword   UnityEngine.Rendering.GlobalKeywordまたはUnityEngine.Rendering.LocalKeywordを無効にするコマンドを追加します。
    //cmdに無かった

// AttachmentはRT側? cmd.SetGlobalTexture("_CameraDepthAttachment", source.nameID);違うかも

void Cmd_Context(ScriptableRenderContext context){
    //Unityは通常、GPUへのコマンドを実行するAPIはUnityRenderQueueに載る(遅延実行。多分プロファイラーで見るフレームの最後の方で実行)
        //~Now系や一部のAPIはUnityRenderQueueに載らずに即時実行するのもあるらしい(描画したRTを後でマージする)
    //UnityRenderQueue❰,,.., {GL -> GL -> context_Submit{cmd1{cmd..}, cmd2{cmd..}} -> GL -> context_Submit{cmd1{cmd..}, cmd1{cmd..}, cmd3{cmd..}}}❱

    var cmd1 = new CommandBuffer(){name = "cmd1"};
    cmd1.ClearRenderTarget(RTClearFlags.Color, Color.red, 0, 0);
    /*3*/context.ExecuteCommandBuffer(cmd1); //UIRImmediateRenderer/cmd1 に入る

    /*1*/GL.Clear(false, true, Color.yellow, 0); //Submit前の即時実行なので一番最初に実行される

    var cmd2 = new CommandBuffer(){name = "cmd2"};
    cmd2.ClearRenderTarget(RTClearFlags.Color, Color.blue, 0, 0);
    /*4*/context.ExecuteCommandBuffer(cmd2);　//UIRImmediateRenderer/cmd2 に入る

    /*2*/GL.Clear(false, true, Color.magenta, 0); //Submit前の即時実行で上のGLの次なので二番目に実行

    /*(3~4)*/context.Submit(); //UIRImmediateRendererに戻るだけ

    /*5*/GL.Clear(false, true, Color.cyan, 0); //UIRImmediateRenderer下で実行される。


    cmd1.ClearRenderTarget(RTClearFlags.Color, Color.blue, 0, 0); //cmd1にColor.blue追加
    /*6*/context.ExecuteCommandBuffer(cmd1); //CommamdBufferは使い回せる。//red -> blue

    cmd1.Clear();   //CommandBufferがクリアされてコマンドが無くなる
    cmd1.ClearRenderTarget(RTClearFlags.Color, Color.magenta, 0, 0);
    /*7*/context.ExecuteCommandBuffer(cmd1);

    var cmd3 = new CommandBuffer(){name = "cmd3"};
    cmd3.ClearRenderTarget(RTClearFlags.Color, Color.green, 0, 0);
    /*8*/context.ExecuteCommandBuffer(cmd3); //UIRImmediateRenderer/cmd3 に入る

    /*(6~8)*/context.Submit(); //UIRImmediateRendererに戻るだけ
}
```

- コードメモ
    ```csharp
    //https://learn.microsoft.com/en-us/dotnet/api/system.enum.hasflag?view=net-8.0
        public bool Enum.HasFlag(Enum flag);
    //com.unity.render-pipelines.core@15.0.6\Runtime\Utilities\ResourceReloader.cs
        Resources.GetBuiltinResource(type, path);
        AssetDatabase.GetBuiltinExtraResource(type, path);

    //com.unity.render-pipelines.universal@15.0.6\Runtime\Data\UniversalRenderPipelineAsset.cs
        // Initialize default Renderer(デフォルトのレンダラーを初期化する)
        //class UniversalRenderPipelineEditorResources : ScriptableObject を入れてる
        instance.m_EditorResourcesAsset = instance.editorResources;

        //>与えられたコンテナオブジェクト内のリソースを探し、欠落しているか壊れているものをリロードする。Reload all Null in container
        //System.Objectのフィールドメンバの[ReloadGroup]属性が付いた変数がnullだったら引数無し.ctorで初期化する?
        //[Reload]はパスで指定されたアセットを入れる
        ResourceReloader.ReloadAllNullIn(instance, packagePath);

        //EndNameEditAction
        class CreateUniversalPipelineAsset : EndNameEditAction

        //(Ctrl + P -> グラフィックス関連/image/UniversalRenderPipelineGlobalSettings.png)
        UniversalRenderPipelineGlobalSettings.Ensure();//Ensure:確保する
    
    //com.unity.render-pipelines.universal@15.0.6\Runtime\Data\PostProcessData.cs
        //(Ctrl + P -> image/PostProcessData.png)
        public class PostProcessData : ScriptableObject

    //com.unity.render-pipelines.core@15.0.6\Runtime\Utilities\Blitter.cs
        //SystemInfo
        if (SystemInfo.graphicsShaderLevel < 30)

    //com.unity.render-pipelines.core@15.0.6\Runtime\Utilities\CoreUtils.cs
        //shaderからHideFlags.HideAndDontSaveを付けたMaterialを返す
        Material CreateEngineMaterial(Shader shader)

    //その他
        //ゲームが動作しているプラットフォームを返します（RO）。
        Application.platform;
        //InspectorName
        public enum RenderingMode
        {..
            [InspectorName("Forward+")]
            ForwardPlus = 2,
        ..}
        //↓なにが違う? ShaderTagIdはLightMode指定だっけ
        int Shader.PropertyToID(string name); new ShaderTagId(stringname);
        //ProfilingSampler
        new ProfilingSampler("Render GBuffer");
        //com.unity.render-pipelines.universal@15.0.6\Runtime\DeferredLights.cs G:984
        cmd.DrawMesh(m_FullscreenMesh, Matrix4x4.identity, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[(int)StencilDeferredPasses.SSAOOnly]);
            //(Stencil pass. =>)Lighting pass. G:972 static Mesh CreateHemisphereMesh()
            cmd.DrawMesh(m_HemisphereMesh, vl.localToWorldMatrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[(int)StencilDeferredPasses.PunctualLit]);
        //RenderingUtils.ReAllocateIfNeeded(..) ↓ReAllocateIfNeeded
        RenderingUtils.ReAllocateIfNeeded(ref m_Handle, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_CustomPassHandle");
        //UniversalRenderer.Setup(..) //this.frameData -> cameraData.renderer != this ってこと?
        cameraData.renderer.useDepthPriming = useDepthPriming;


    //RenderGraph
        //CreateRendererList
        public RendererListHandle CreateRendererList(in RendererListParams desc);
        //RenderStateBlockは∮RENDERING_STATE∮?
        static RenderStateBlock[] s_RenderStateBlocks;
        //stateによってkeywordをcmdに設定しているだけ
        public static void CoreUtils.SetKeyword(BaseCommandBuffer cmd, string keyword, bool state);
        //UniversalRenderer.CreateRenderGraphTexture (com.unity.render-pipelines.universal@15.0.6\Runtime\Passes\GBufferPass.cs G:265)
        public static TextureHandle UniversalRenderer.CreateRenderGraphTexture(RenderGraph renderGraph, RenderTextureDescriptor desc, string name, bool clear,
            FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
    ```
