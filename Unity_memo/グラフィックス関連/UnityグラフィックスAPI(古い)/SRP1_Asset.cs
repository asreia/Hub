using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace CommandBufferDisc{
// 変数
    // 名前	このコマンドバッファーの名前
    // sizeInBytes	このコマンドバッファーのバイトでのサイズ(読み出し専用)
// コンストラクタ
    // CommandBuffer	新しい空の Command Buffer を作成します。
// Public 関数

    // blit	"blit into a render texture" コマンドを追加します。
        //Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice);
        //Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat, int pass);
        //sourceをシェーダーから参照し("_MainTex")destをレンダリングターゲットとしレンダリングパイプラインを使ってクワッドに描画する。
        //scaleとoffsetはST変数に入る?//source|destDepthSliceはテクスチャ配列の要素?//pass	使用するシェーダーパス (デフォルトは -1、意味は"すべてをパス?")。
        //Blit は現在アクティブなレンダーターゲットを変えるということに気を付けてください。Blit を実行した後では、dest がアクティブなレンダーターゲットになります。


    // CopyTexture	テクスチャを別のテクスチャにコピーするコマンドを追加します。
        //同じビット幅のフォーマット間でコピーできる(R16G16 -> R8G8B8A8とか?)
        //スケーリングされないのでsrcとdstのサイズが同じか、srcの部分からdstの部分にコピーされる?
        // ConvertTexture	ソーステクスチャを異なるフォーマットまたはサイズのデスティネーションテクスチャに変換してコピーします。
            //異なるフォーマット、サイズ 間でコピーできる
        //CopyTextureもConvertTextureもGraphics.CopyTextureを使っている?
        //Graphics.CopyTexture -> CopyTexture -> ConvertTexture?

    // EnableShaderKeyword	指定された名前のグローバルキーワードを有効にするコマンドを追加します。
        //EnableShaderKeyword (string keyword);ShaderのShaderKeywordのdefineをスイッチすると思う。
        // DisableShaderKeyword	指定された名前のグローバルシェーダーキーワードを無効にするコマンドを追加します。
            //DisableKeyword を使用する方が効率的です?。

    //http://gottaniprogramming.seesaa.net/article/459277824.html
        // DrawMesh	"Draw Mesh" コマンドをを追加します。ただし、batching は効かない
        // DrawMeshInstanced	「drawmeshwith instancing」コマンドを追加します。Material.enableInstancingがfalseの場合、コマンドはすぐに失敗して例外をスローしませんが、エラーをログに記録し、そのような条件が検出された場合、コマンドが実行されるたびにレンダリングをスキップします。現在のプラットフォームがこのAPIをサポートしていない場合（つまり、GPUインスタンス化が利用できない場合）、InvalidOperationExceptionがスローされます。SystemInfo.supportsInstancingを参照してください。
            //モデル変換Matrixの配列とCountを引数にインスタンシング。 Materialは「Enable GPU Instancing」をチェック, 一度に1024個しかレンダリングできない
        // DrawMeshInstancedIndirect	「間接インスタンス化によるメッシュの描画」コマンドを追加します。
            // DrawMeshInstancedIndirect (Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs);
            // DrawProceduralIndirectと違ってMeshを使える。GraphicsBufferはインスタンス数の長さと、インスタンス毎のデータを入れる?https://shitakami.hatenablog.com/entry/2020/08/23/235908(まだ読んでない)
        // DrawMeshInstancedProcedural	「インスタンス化を使用してメッシュを描画する」コマンドを追加します。手続き型インスタンス化を使用してメッシュを描画します。これはGraphics.DrawMeshInstancedIndirectに似ていますが、インスタンスカウントがスクリプトからわかっている場合、ComputeBuffer経由ではなく、このメソッドを使用して直接提供できる点が異なります。Material.enableInstancingがfalseの場合、コマンドはエラーをログに記録し、コマンドが実行されるたびにレンダリングをスキップします。コマンドがすぐに失敗して例外がスローされることはありません。現在のプラットフォームがこのAPIをサポートしていない場合（たとえば、GPUインスタンス化が利用できない場合）、InvalidOperationExceptionがスローされます。SystemInfo.supportsInstancingを参照してください。
            //DrawMeshInstancedProcedural (Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties);
                //Graphics.DrawMeshInstancedIndirectに似ているが、インスタンスカウントがスクリプトから分かる場合、ComputeBuffer経由ではなく、このメソッドを使用して直接提供できる点が異なります。
                //引数にComputeBufferが無いが?
        // DrawProcedural	"draw procedural geometry" コマンドを追加。
            //DrawProcedural (Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties);
            // 多分、(vertexCount / topology)個のtopologyを組み合わせて一つのジオメトリを作り、それをinstanceCount分インスタンシングする。
                //VertexIDとInstanceIDしか無いので何らかのBufferか配列をVertexIDかInstanceIDで参照するか、ジェネして、Position,Matrixなどを得る。
            // ハードウェアインスタンシング? // MeshTopologyでプリミティブを指定する
            // Unity では Graphics.DrawProcedural() が該当
            // ただし、色々使いづらい仕様になっている
            // Direct3D11 対応環境 / PS4でしか使えない
            // Mesh をそのまま描くことはできない
                // 頂点シェーダの入力は 頂点 ID と インスタンス ID のみ
            // Unity の render queue に載せることができない(cmd版は載る)
                // 呼んだその時点で即座に描かれる (Graphics.DrawMeshNow() と同様)
            // surface shader が使えない
                // 一貫したライティング処理を施すのに工夫が必要
        // DrawProceduralIndirect	"draw procedural geometry" コマンドを追加。
            //DrawProceduralIndirect (Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs);
            //vertexCountやinstanceCountがなく、GraphicsBufferから取得する?、GraphicsBufferの長さで数を増減できる? GPU内で完結できる　Compute -> RenderingPipeline
    // DrawRenderer	Draw Renderer コマンドを追加します。
        // DrawRenderer(Renderer renderer, Material material, int submeshIndex, int shaderPass); //多分MeshRendererなどのRendererコンポーネントを渡してマテリアルを変えて描画する
    // DrawRendererList	「レンダラーリストの描画」コマンドを追加します。SRPContextと関係がある?
    // DrawOcclusionMesh	コマンドバッファにコマンドを追加して、VRデバイスのオクルージョンメッシュを現在のレンダーターゲットに描画します。
        //DrawOcclusionMesh (RectInt normalizedCamViewport);アクティブなVRデバイスによって提供されるオクルージョンメッシュをレンダリングするために使用されます。VRデバイスの表示領域の外側にあるオブジェクトのレンダリングを防ぐために、他のレンダリングメソッドの前にこのメソッドを呼び出します。

    // SetGlobalFloat	"set global shader float property" コマンドを追加します。
        //コマンドバッファが実行されるとき、グローバルシェーダーの float プロパティーが設定される?。効果は Shader.SetGlobalFloat が呼び出された場合と同じです。
        //https://qiita.com/Hirai0827/items/c0a92b3e858b95a06a42 //Properties{}ではなく直接CGPROGRAMのシェーダ内の変数に代入される見たい
        // SetGlobalColor	"set global shader color property" コマンドを追加します。
            //SetGlobalColor (string name, Color value);でColorだが、シェーダ内はfloat4に入る?
        // SetGlobalFloatArray	"set global shader float array property" コマンドを追加します。
        // SetGlobalInt	すべてのシェーダーに特定のプロパティの値を設定するコマンドを追加します。プロパティのタイプはShaderLabコードでIntです。
        // SetGlobalInteger	すべてのシェーダーに指定されたプロパティの値を設定するコマンドを追加します。プロパティは整数です。
        // SetGlobalMatrix	"set global shader matrix property" コマンドを追加します。
        // SetGlobalMatrixArray	"set global shader matrix array property" コマンドを追加します。
        // SetGlobalTexture	RenderTexture を参照する "set global shader texture property" コマンドを追加します。
        // SetGlobalVector	"set global shader vector property" コマンドを追加します。
        // SetGlobalVectorArray	"set global shader vector array property" コマンドを追加します。

    // SetViewport	レンダリングビューポートを設定するコマンドを追加します。レンダリングを指定されたピクセル長方形に制限できます。
        //Scissorとほぼ同じ気がする。Scissor ⊂ SetViewport ?
    // EnableScissorRect	ハードウェアシザー長方形を有効にするコマンドを追加します。
        //EnableScissorRect (Rect scissor); //複数有効にできる?
        // DisableScissorRect	ハードウェアシザー長方形を無効にするコマンドを追加します。
            //DisableScissorRect (); //引数がないのでScissorを全て無効にするか、もともと１つしか有効にできないか(1つだとSetViewportと変わらない)

    // SetViewMatrix	ビューマトリックスを設定するコマンドを追加します。ビューマトリックスは、ワールド空間からカメラ空間に変換するマトリックスです。OpenGL規則に一致するため、負のz軸はカメラの前方です。
        //ビュー変換行列を設定
    // SetProjectionMatrix	射影行列を設定するコマンドを追加します。
        //プロジェクション変換行列を設定
    // SetViewProjectionMatrices	ビューと投影行列を設定するコマンドを追加します。この関数は、組み込みのレンダーパイプラインと互換性があります。SRPと互換性が無い?動いたけど。
        //ビューxプロジェクション変換行列を設定

    // SetInstanceMultiplier	すべての描画呼び出しのインスタンス数に特定の乗数を掛けるコマンドを追加します。
        //インスタンシング? https://docs.unity3d.com/ja/2021.2/Manual/SinglePassInstancing.html
    // SetInvertCulling	「setinvertculling」コマンドをバッファーに追加します。
        //バックフェイスカリングの反転? //RenderDocでは、FrontCounterClockwiseが変わる trueの時true, falseの時false
    // SetShadowSamplingMode	"set global shader matrix property" コマンドを追加します。
        //Shadow(深度?)バッファ?のサンプリング(コピー)方法?

    // ClearRenderTarget	"clear render target" コマンドを追加します。
    // GenerateMips	レンダーテクスチャのミップマップレベルを生成します。
    // GetTemporaryRT	"get a temporary render texture" コマンドを追加します。
    // GetTemporaryRTArray	「一時的なレンダリングテクスチャ配列を取得する」コマンドを追加します。
    // ReleaseTemporaryRT	"release a temporary render texture" コマンドを追加します。
    // SetRenderTarget	"set active render target" コマンドを追加します。
    // ResolveAntiAliasedSurface	アンチエイリアスされたレンダーテクスチャを強制的に解決します。
    // クリア	バッファのすべてのコマンドをクリアします。
    // SetGlobalDepthBias	グローバル深度バイアスを設定するコマンドを追加します。
        //Zファイティングの回避。D3D11よりラスタライザのDepthBiasとSlopeScaledDepthBiasの設定だと思うSetGlobalDepthBias (float bias, float slopeBias); DepthBiasClampは?
    // SetExecutionFlags	コマンドバッファの実行方法を説明するフラグを設定します。
        //互換性のないコマンドが追加された場合に例外をスローします。ScriptableRenderContext|Graphics.ExecuteCommandBuffer"Async"
    // IncrementUpdateCount	TextureのupdateCountプロパティをインクリメントします。
        //https://docs.unity3d.com/ja/2018.4/ScriptReference/Texture.IncrementUpdateCount.html
    // BeginSample	プロファイルのサンプリングを開始するコマンドを追加します。
        //コマンドバッファがこのポイントに達したときに開始するようにパフォーマンスプロファイリングサンプルをスケジュールします。
        //これは、コマンドバッファ内の1つ以上のコマンドによって費やされたCPUおよびGPU時間を測定するのに役立ちます。
            //プロファイラで見れる?
        // EndSample	プロファイルのサンプリングを終了するコマンドを追加します。


    //LateLatch(レイトラッチ?)URPのシェーダープロパティのレイトラッチと言う機能に関するコマンドの様だが、CommandBufferのクラスに存在しない(2021.2の新機能?)上に情報が全く無いので見なかったことにする。
        // MarkLateLatchMatrixShaderPropertyID	グローバルシェーダープロパティIDをレイトラッチするようにマークします。可能なシェーダープロパティには、
            //view、inverseView、viewProjection、およびinverseViewProjectionマトリックスが含まれます。Universal Render Pipeline（URP）は、この関数を使用して、
            //シェーダープロパティのレイトラッチをサポートします。組み込みのUnityレンダリングまたは高解像度レンダリングパイプライン（HDRP）を使用しているときにこの関数を呼び出すと、結果は無視されます。
        // UnmarkLateLatchMatrix	レイトラッチのグローバルシェーダープロパティのマークを外します。マークを外した後、シェーダープロパティはレイトラッチされなくなります。
            //この関数は、ユニバーサルレンダーパイプライン（URP）がレイトラッチシェーダープロパティを指定することを目的としています。
        // SetLateLatchProjectionMatrices	レイトラッチ用に現在のステレオ射影行列を設定します。ステレオ行列は、2つの行列の配列として渡されます。
    //GraphicsFence?GPUFences? GPUのなんらかのコマンドの実行の完了を通知する機能のようだ?
        // CreateGraphicsFence	この呼び出しの前の最後のBlit、Clear、Draw、Dispatch、またはTextureCopyコマンドがGPUで完了した後に渡されるGraphicsFenceを作成します。
        // CreateAsyncGraphicsFence	GraphicsFenceType.AsyncQueueSynchronizationを最初のパラメーターとしてGommandBuffer.CreateGraphicsFenceを呼び出すためのショートカット。
        // WaitOnAsyncGraphicsFence	指定されたGraphicsFenceが渡されるまで待機するようにGPUに指示します。
    //Async GPU Readback https://youtu.be/7tjycAEMJNg?t=4540
        // RequestAsyncReadback	                非同期GPUリードバック要求コマンドをコマンドバッファーに追加します。
        // RequestAsyncReadbackIntoNativeArray	非同期GPUリードバック要求コマンドをコマンドバッファーに追加します。
        // RequestAsyncReadbackIntoNativeSlice	非同期GPUリードバック要求コマンドをコマンドバッファーに追加します。    
        // WaitAllAsyncReadbackRequests	「AsyncGPUReadback.WaitAllRequests」コマンドをCommandBufferに追加します。
    //コンピュートシェーダー
        // SetGlobalConstantBuffer	グローバル定数バッファーをバインドするコマンドを追加します。
            //コンピュートシェーダーのバッファの設定だと思うSetGlobalConstantBuffer (GraphicsBuffer buffer, string name, int offset, int size);
        // SetBufferCounterValue	追加/消費バッファのカウンタ値を設定するコマンドを追加します。
            // ComputeBufferType.Counter|Appendの時の設定?SetBufferCounterValue (GraphicsBuffer buffer, uint counterValue);
        // SetBufferData	配列からの値でバッファを設定するコマンドを追加します。
            // SetBufferData (GraphicsBuffer buffer, Array data);
        // SetGlobalBuffer	「グローバルシェーダーバッファプロパティの設定」コマンドを追加します。
            // SetGlobalBuffer (string name, GraphicsBuffer value);その効果は、Shader.SetGlobalBufferが呼び出されたかのようです。
        // CopyBuffer	1つのGraphicsBufferの内容を別のGraphicsBufferにコピーするコマンドを追加します。
        // CopyCounterValue	ComputeBufferまたはGraphicsBufferのカウンター値をコピーするコマンドを追加します。
    //RandomWrite?
        // ClearRandomWriteTargets	シェーダーモデル4.5レベルのピクセルシェーダーのランダム書き込みターゲットをクリアします。
        // SetRandomWriteTarget	シェーダーモデル4.5レベルのピクセルシェーダーのランダム書き込みターゲットを設定します。
    // SetSinglePassStereo	カメラのシングルパスステレオモードを設定するコマンドを追加します。
        //このプロパティは、バーチャルリアリティが有効になっている場合にのみ使用されます。VRのレンダリング方法をSinglePassStereoMode列挙型で指定?
    // SetKeyword	UnityEngine.Rendering.GlobalKeywordまたはUnityEngine.Rendering.LocalKeywordの状態を設定するコマンドを追加します。
        //CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap")); https://tips.hecomi.com/entry/2019/10/27/152520
        //マテリアルのプロパティに値を設定する?SetGlobal~~の万能型?//cmdに無かった
    // EnableKeyword	UnityEngine.Rendering.GlobalKeywordまたはUnityEngine.Rendering.LocalKeywordを有効にするコマンドを追加します。
        //cmdに無かった
    // DisableKeyword	UnityEngine.Rendering.GlobalKeywordまたはUnityEngine.Rendering.LocalKeywordを無効にするコマンドを追加します。
        //cmdに無かった
}

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

[CreateAssetMenu(menuName = "SelfMadeSRP_Asset/SRP1", fileName = "SRP1_Asset_File")]
public class SRP1_Asset : RenderPipelineAsset{
    protected override RenderPipeline CreatePipeline(){
        return new SRP1();
    }
    public class SRP1 : RenderPipeline{
        public static bool flag = false;
        public static RenderTexture RT_GetTemp = new RenderTexture(2,2,0);
        public static RenderTexture RT_GetTemp1;
        public static RenderTexture new_RT;
        public static bool useMipMap = false;

        [MenuItem("SRP1/MipMap/Toggle_useMipMap")]
        static void Toggle_useMipMap(){
            useMipMap =! useMipMap; Debug.Log($"useMipMap: {useMipMap}");
            new_RT.useMipMap = useMipMap;//すでに作成されたレンダリングテクスチャのミップマップモードの設定はサポートされていません 
        }

        [MenuItem("SRP1/CreateRT/Create_new_RT_useMipMap")]
        static void Create_new_RT_useMipMap(){
            new_RT = new RenderTexture(1024, 1024, 0, GraphicsFormat.R16G16B16A16_UNorm, 4){name = "name_new_RT"};
            new_RT.useMipMap = true; //ok
            new_RT.Create();Debug.Log("new_RT.Create()"); //これをすると600MBメモリ確保される
            // new_RT.useMipMap = true; //RTが生成されてからuseMipMapを設定できない//すでに作成されたレンダリングテクスチャのミップマップモードの設定はサポートされていません 
            new_RT.autoGenerateMips = true;
            // Graphics.SetRenderTarget(new_RT);
            // new_RT.GenerateMips();
        }

        [MenuItem("SRP1/ReleaseRT/Release_new_RT")]
        static void Release_new_RT(){
            Debug.Log($"IsCreated:{new_RT.IsCreated()}"); //=> true;
            new_RT.Release();Debug.Log("new_RT.Release()");//これをすると600MBメモリ開放される
            Debug.Log($"IsCreated:{new_RT.IsCreated()}"); //=>false;
        }

        [MenuItem("SRP1/CreateRT/Create_new_RT")]
        static void Create_new_RT(){
            new_RT = new RenderTexture(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm){name = "name_new_RT"};
            Debug.Log($"IsCreated:{new_RT.IsCreated()}"); //=> false
            // new_RT.useMipMap = true; //ok
            new_RT.Create();Debug.Log("new_RT.Create()"); //これをすると600MBメモリ確保される
            // new_RT.useMipMap = true; //RTが生成されてからuseMipMapを設定できない//すでに作成されたレンダリングテクスチャのミップマップモードの設定はサポートされていません 
            Debug.Log($"IsCreated:{new_RT.IsCreated()}"); //=> true;
            // new_RT.useMipMap = false; 
            // new_RT.useMipMap = true; 
            // new_RT.autoGenerateMips = false;
            // new_RT.GenerateMips();
        }

        [MenuItem("SRP1/SetRT/Graphics_SetRT_new_RT")]
        static void Graphics_SetRT_new_RT(){
            Debug.Log($"IsCreated:{new_RT.IsCreated()}"); //=> false
            Graphics.SetRenderTarget(new_RT);Debug.Log("Graphics.SetRenderTarget(new_RT)");//生成されてない時これをすると600MBメモリ確保される
            Debug.Log($"IsCreated:{new_RT.IsCreated()}"); //=> true
        }

        [MenuItem("SRP1/ReleaseRT/Release_RT_GetTemp")]
        static void Release_RT_GetTemp(){
            Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}");//=> true;
            //GetTemporaryへの後の呼び出しは、可能であれば以前に作成されたRenderTextureを再利用します。誰も一時的なRenderTextureを要求しない状態が数フレーム続くと、それは破棄されます。
                //同じサイズか同じ設定で生成すると再利用される?
            RenderTexture.ReleaseTemporary(RT_GetTemp); Debug.Log("RenderTexture.ReleaseTemporary(RT_GetTemp)");
            Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}");//=> true; //600MB開放されているがfalseにならない

            RT_GetTemp.Release(); Debug.Log("RT_GetTemp.Release()");
            Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}");//=> false; //600MB開放されfalseになる
        }

        [MenuItem("SRP1/IsCreatedRT/IsCreated_RT_GetTemp")]
        static void IsCreated_RT_GetTemp(){
            Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}");//ReleaseTemporaryしたRTは数フレームたつと勝手にtrueになる。
        }

        [MenuItem("SRP1/CreateRT/Create_RT_GetTemp")]
        static void Create_RT_GetTemp(){
            Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}");
            RT_GetTemp = RenderTexture.GetTemporary(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm);
            RT_GetTemp.name = "name_RT_GetTemp";//"name_RT_GetTemp"がRenderDocでResource名になっている。
            Debug.Log("RT_GetTemp = RenderTexture.GetTemporary(..)");
            Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}"); //=> true;
        }
        static bool DiscardRTFlag = false;
        [MenuItem("SRP1/DiscardRT/DiscardRT")]
        static void DiscardRTSwitch(){
            DiscardRTFlag =! DiscardRTFlag;
        }
        static void DiscardRT(){
            if(new_RT == null || !new_RT.IsCreated())Create_new_RT();
            new_RT.DiscardContents();Debug.Log("DiscardContents"); //なしありでRenderDocに違いはない。
            Graphics_SetRT_new_RT();
            // new_RT.MarkRestoreExpected(); //旧形式、[非推奨]と出た
        }
        void CreateRT2(ScriptableRenderContext context){
            flag= false;
            // Debug.Log("CreateRT2");
            // rt = RenderTexture.GetTemporary(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm); Debug.Log("Done GetTemporary");
            // rt.name = "CreateRT2";
            // rt.Create();
            int rtIndex1 = Shader.PropertyToID("prtIndex1"); //"prtIndex1"がRenderDocでResource名になっている。
            var cmd = CommandBufferPool.Get("CBP11");
            cmd.GetTemporaryRT(rtIndex1 ,1280 * 8, 1920 * 4, 0, FilterMode.Point, GraphicsFormat.R16G16B16A16_UNorm);
            cmd.SetRenderTarget(rtIndex1);
            // cmd.SetRenderTarget(name_RT_GetTemp);//生成されていても使われないとRenderDocに表示されない。NullはGameViewがsetされる
            // cmd.SetRenderTarget(new_RT);
            cmd.ReleaseTemporaryRT(rtIndex1);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);


            // var m = Object.FindObjectOfType<MonoBehaviour>();
            // m.StartCoroutine(CreateRT2Coroutine());
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

        void new_RT_bindTextureMS_useMipMap(){
        //bindTextureMS
            //以下antiAliasingが2以上の場合のみ
                //bindTextureMS = false(デフォルト)にするとResolveSubresource()がactiveRTから外れる直前に呼ばれる (マルチサンプルリソースをシングルサンプルリソースにコピーする．らしい)
                    //ResolveAntiAliasedSurface()を呼ぶとResolveSubresource()が即時呼ばれる。
                //bindTextureMS = true にするとResolveSubresource()が一切呼ばれなくなる。
                //bindTextureMS = false はTexture2Dで、trueはTexture2DMSにバインドする?
        //useMipMap
            //useMipMap = trueにするとMipMap付きRenderTextureが生成される。falseだとmipCountを設定してもMipMapが付かない。
            //以下useMipMap = trueの場合のみ
                //autoGenerateMips = trueの時、direct_xのGenerateMips()がactiveRTから外れたとき自動的に実行してMipMapが描画される。
                //autoGenerateMips =falseの時、GenerateMips()を実行すると即時direct_xのGenerateMips()が実行する。trueのときこれを実行するとエラーがでる

            //RenderTextureの第三引数に0以外入れると深度バッファが出来る。RenderTextureは深度バッファ付きだったのか..
            if(new_RT == null) new_RT = new RenderTexture(1024, 1024, 16, GraphicsFormat.R16G16B16A16_UNorm, 4){
                name = "name_new_RT", useMipMap = true, autoGenerateMips = true, antiAliasing = 4, bindTextureMS = false}; 
                //useMipMapをtrueにするだけでMiscFlags:D3D11_RESOURCE_MISC_GENERATE_MIPSが付く
                //autoGenerateMipsがtrueの時、direct_xのGenerateMips()がactiveRTから外れたとき自動的に実行してMipMapが描画される。
                //antiAliasingを4に設定するとDXGI_SAMPLE_DESCのCount:4,Quality:0になりMipMapが無効になる Qualityの設定はどこ
            Graphics.SetRenderTarget(new_RT);//Debug.Log("Graphics.SetRenderTarget(new_RT)");//Captureするframe内で使われていないとRenderDocで表示されない。
            new_RT.ResolveAntiAliasedSurface();//bindTextureMS = falseの時のみResolveSubresource()を強制的に呼んでいる?
            GL.Clear(false, true, Color.grey, 0.0f);//0.0fだとClearDepthStencilView(1.00, 0)、1.0fだと0.00になる謎
            // new_RT.GenerateMips();//autoGenerateMipsがfalseの時、実行すると即時direct_xのGenerateMips()が実行する。trueのときこれを実行するとエラーがでる
            RenderTexture.active = null;
            Graphics.SetRenderTarget(new_RT);//antiAliasingが有効でbindTextureMSがfalseの時、activeRTから外れる直前にResolveSubresource()が呼ばれる
            RenderTexture.active = null;

            // Material TestShader = new Material(Shader.Find ("Unlit/TestShader"));
            // Mesh CubeMesh = GameObject.Find("Cube").GetComponent<MeshFilter>().sharedMesh; //Debug.Log($"CubeMesh == null: {CubeMesh == null}");
            // Transform CubeTransform = GameObject.Find("Cube").GetComponent<Transform>();
            // TestShader.SetPass(0);
            // Graphics.DrawMeshNow(CubeMesh, CubeTransform.position, CubeTransform.rotation,0);//うまくいかなかった
        }
        
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

        void CmdPool(ScriptableRenderContext context){
            //CommandBufferPoolはEngine内の内部的な最適化の挙動なので挙動は普通のCommandBufferと変わらない?
            //のように見えたが、
            //Releaseはもう使わないcmdのメモリ領域をPoolに返しGetはPoolからメモリ領域を取得している。多分
            //ReleaseされたcmdはこれからGetされたcmdによって書き換わる可能性が非常に高いので、Releaseされたcmdは使わないのが鉄則かな

            CommandBuffer cmdPool = CommandBufferPool.Get("cmdPool1"); //←はnew CommandBuffer(){name = "cmdPool1"};と同じ様に働くように見える。
                                                                        //ReleaseされてまだC#的に開放されていないメモリ領域を再利用している。

            cmdPool.ClearRenderTarget(RTClearFlags.Color, Color.red, 0, 0);
            context.ExecuteCommandBuffer(cmdPool); //red

            CommandBufferPool.Release(cmdPool); //ReleaseはCommandBuffer.Clearと同じ挙動をする様に見える。
                                                 //使用したメモリ領域をClearしPoolに戻している?

            context.ExecuteCommandBuffer(cmdPool); //なし

            CommandBuffer cmdPool_ = CommandBufferPool.Get("cmdPool2"); //引数の文字列でnew CommandBuffer(){name = "cmdPool1"};と挙動が変わることは無い
                // Debug.Log($"cmdPool_.name: {cmdPool_.name}"); //=> cmdPool2;
            cmdPool_.ClearRenderTarget(RTClearFlags.Color, Color.magenta, 0, 0);
            context.ExecuteCommandBuffer(cmdPool_); //magentaのみ
            context.ExecuteCommandBuffer(cmdPool); //↑のReleaseをするとred -> magenta, cmdPool1 -> cmdPool2に変わっている!! //つまり、Get(~)は同じメモリ領域を指すポインタを返している
            CommandBufferPool.Release(cmdPool_);

            context.Submit();
        }

        static int frameCount = -1;
        [MenuItem("SRP1/Test/RT_GetTemp_Test")]
        static void clearFrameCount(){
            frameCount = 0;
        }
        void RT_GetTemp_Test(){
            //GetTemporaryの説明
                // この関数は、一時的な計算を行うために素早くRenderTextureが必要な場合に最適です。使い終わったらすぐに ReleaseTemporary を使って解放し、必要に応じて別の呼び出しで再利用できるようにします。
                // Unity は内部的に一時的なレンダリング テクスチャのプールを保持しているため、GetTemporary を呼び出すと、ほとんどの場合、（サイズとフォーマットが一致する場合）すでに作成されたものが返されます。これらの一時的なレンダリングテクスチャは、数フレーム使用されないと実際に破棄されます。
                // 一連のポストプロセス「ブリット」を行う場合、1つまたは2つのレンダリングテクスチャを前もって取得して再利用するのではなく、ブリットごとに一時的なレンダリングテクスチャを取得してリリースするのがパフォーマンス的に最適です。これは主に、モバイル（タイルベース）やマルチ GPU システムに有効です。GetTemporary は内部的に DiscardContents 呼び出しを行い、以前のレンダリング テクスチャ コンテンツに対するコストのかかる復元操作を回避します。
                // GetTemporary 関数から取得した RenderTexture の特定のコンテンツに依存?することはできません。プラットフォームによっては、それがゴミであったり、何らかの色にクリアされているかもしれません。

            //CmdPoolの様にReleaseTemporaryされたRTをSetRTして再利用されているかチェックしようとしたがSetRTされずうまくいかなかった。
            //GetTemporaryへの後の呼び出しは、可能であれば以前に作成され(その後ReleaseTemporaryされ?)たRenderTextureを再利用します。
                //誰も一時的なRenderTextureを要求しない状態が数フレーム続くと、それは破棄されます。
                    //同じサイズ(widthとheight?)とフォーマット(GraphicsFormat?)で生成すると再利用される
                    //1フレーム内で生成と破棄を繰り返す場合に有効?
            switch(frameCount){
                case 0: Debug.Log("case 0");
                        RT_GetTemp = RenderTexture.GetTemporary(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm);
                        RT_GetTemp.name = "name_RT_GetTemp";
                        Graphics.SetRenderTarget(RT_GetTemp);
                        GL.Clear(false, true, Color.red, 0);
                        break;
                case 1: Debug.Log("case 1");
                        RenderTexture.ReleaseTemporary(RT_GetTemp);
                        Debug.Log($"RT_GetTemp.IsCreated(): {RT_GetTemp.IsCreated()}");//ReleaseTemporaryはfalseにならない
                        break;
                case 2: Debug.Log("case 2");
                        RT_GetTemp1 = RenderTexture.GetTemporary(1280 * 8, 1920 * 4, 0, GraphicsFormat.R16G16B16A16_UNorm);
                        RT_GetTemp1.name = "name_RT_GetTemp_1";
                        Graphics.SetRenderTarget(RT_GetTemp1);
                        GL.Clear(false, true, Color.blue, 0);
                        Graphics.SetRenderTarget(RT_GetTemp); //ReleaseTemporaryしたRTはSetRTされない
                        break;
                case 3:Debug.Log("case 3");
                        RenderTexture.ReleaseTemporary(RT_GetTemp1);
                        break;
                default: frameCount = 0;
                         return;
            }
            frameCount++;
        }
        GameObject cube;
        void DrawMesh(ScriptableRenderContext context, Camera camera){
            if(cube == null) cube = GameObject.Find("Cube");
            // Debug.Log($"cube: {cube.GetComponent<MeshFilter>().sharedMesh}");
            var cmd = CommandBufferPool.Get("DrawMesh");
            cmd.SetInvertCulling(true); //これが必要なのはD3D11_COMPARISON_GREATER_EQUALが原因?(裏面を描画していた訳では無かった)いや、単にtrueが表でfalseが裏になるだけ?
                                        //1.奥を表示(GREATER_EQUAL)で表面描画(CullBack)だと密閉されたジオメトリは全て裏面しか見えず描画されないはず?..
                                        //そうすると2.手前を表示で裏面描画でも表示されない?あれ?
                                        //Cull -> depth　ならおｋ?
                                        //そうすると、1.表面表示だけ残し、奥を表示(奥は裏面なのですでにない)
                                        //2.裏面表示を残し、手前を表示(手前は表面なのですでにない)
                                            //単純に裏表どちらかCullすると [カメラ< |表||   |裏|| のどっちか消えてdepthの比較の意味がなくなるはず
            cmd.ClearRenderTarget(RTClearFlags.ColorDepth, Color.gray, 1.0f, 0);//Unity上では0がfarで1がnearでDirectXでは逆に変換されるがdepthテストが逆..と言うことだと思う
                //1の時 RenderDoc: ClearDepthStencilView(0.00)映る     //UnityAPIと逆になっている
                //0の時 RenderDoc: 984 ClearDepthStencilView(1.00)映らない //DirectXは左手座標系なのに逆
                    //DepthFunc D3D11_COMPARISON_GREATER_EQUAL           //↑と思ったらdepthテストがGREATER_EQUALになっていたｗ これの変え方は?..
            // cmd.SetViewMatrix(camera.transform.localToWorldMatrix.inverse); //camera.transformで設定するのは正しい?
            // cmd.SetProjectionMatrix(camera.projectionMatrix);
            cmd.SetViewProjectionMatrices(camera.transform.localToWorldMatrix.inverse, camera.projectionMatrix); //SRPと互換性が無い?動いたけど。
            cmd.DrawMesh(cube.GetComponent<MeshFilter>().sharedMesh, cube.transform.localToWorldMatrix , cube.GetComponent<MeshRenderer>().sharedMaterial);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            RenderTexture.active = null;
            // GL.Clear(true, true, Color.gray, 1.0f); //ClearDepthStencilView(0.00, 0)映る (cmd.ClearRenderTarget無し時)
            context.Submit();
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras){
            /*if(flag)*/ CreateRT2(context); 

            context.Submit();RenderTexture.active = null;

            // context.SetupCameraProperties(cameras[0]);

            // new_RT_bindTextureMS_useMipMap();

            // Cmd_Context(context);

            // RT_GetTemp_Test();

            // CmdPool(context);

            // if(DiscardRTFlag) DiscardRT();

            DrawMesh(context, cameras[0]);
                                                                                            //多分、context.Submit()の直前にGPUに送られている
            for(int i = 0; i < cameras.Length; i++){

            }
            context.Submit();
        }
    }
}
