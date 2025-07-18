# CommandBuffer (非UnityObject, UnityEngine.CoreModule.dll)

- `cmd`実行可能なメソッド
  :`⟪ctx¦Graphics⟫.ExecuteCommandBuffer＠❰Async❱(CommandBuffer cmd ＠❰, ComputeQueueType queueType❱)`,
  `camera.AddCommandBuffer(CameraEvent evt, CommandBuffer cmd)`,
  `light.AddCommandBuffer(LightEvent evt, CommandBuffer cmd ＠❰, ShadowMapPass shadowPassMask❱)`
- `ctx.ExecuteCommandBuffer(cmd)`は`cmd`を積むだけ。`ctx.Submit()`で積まれた`cmd`の全ての処理が開始される

- CommandBuffer系
  - `.ctor()`: 新しい空の`cmd`を**作成**する。
  - `string name`: `cmd`の**名前**。RenderDoc,FrameDebugger,Profilerで見れるやつ。
  - `int sizeInBytes`: `cmd`の**バイトサイズ** (Read-Only)
  - `Clear()`: 単にバッファ内に追加された**全てのコマンドを削除**し空の状態に戻すだけ
  - `⟪Begin¦End⟫Sample(⟪string name¦CustomSampler sampler¦ProfilerMarker marker⟫)`
    :**プロファイリング**のコマンドを追加。RenderDoc,FrameDebugger,Profilerで見れるやつ。>CPUやGPUがその後のコマンドに費やした時間を測るのに使えるのですよ。
- ResourceModified系
  - `SetBufferData(GraphicsBuffer buffer, ⟪NativeArray<T>¦List<T>¦Array⟫ data ＠❰, int managedBufferStartIndex, int graphicsBufferStartIndex, int count❱)`
    :`buffer`に`data`を入れる。`GraphicsBuffer.SetData(..)`と同じ。
  - `SetBufferCounterValue(GraphicsBuffer buffer, uint counterValue)`:**カウンターを設定**する。`GraphicsBuffer.SetCounterValue(.)`と同じ。
  - Rayトレーシング
    - `BuildRayTracingAccelerationStructure(RayTracingAccelerationStructure accelerationStructure, Vector3 relativeOrigin)`
      :加速構造(`accelerationStructure`)を構築
- **SetPass系**
  :設定は基本的に`ctx.ExecuteCommandBuffer(cmd)`実行後に**リセット**する (個別で設定する場合は、`ShaderLab/∮RenderingState∮`, `RenderStateBlock` を使う)
  - RenderingState系
    - カリング
      - `SetInvertCulling(bool invertCulling)`: **カリング反転**。`invertCulling=true`で**BackFaceカリング**を**反転**。(DirectXの**Z反転対策**は`FrontCCW`で合わせている?)
    - ビューポート、シザー
      - `SetViewport(Rect pixelRect)`: **ビューポート設定**。`pixelRect`(スクリーン空間)に**ストレッチ**する。(**アクティブRT変更時**、その**rt解像度にリセット**される) (1つのみ, depthは無い)
      - `EnableScissorRect(Rect scissor)`: **シザー設定**。`pixelRect`(スクリーン空間)に**クランプ**する。(1つのみ, クランプ外のPixelシェーダーは起動しない)
        - `DisableScissorRect()`: ↑で設定したシザー設定を**無効**にする
    - シャドー関係
      - `SetGlobalDepthBias(float bias, float slopeBias)`:Globalな**デプスバイアス**を設定。設定は深度バッファの解像度(32bitなど)の**精度単位**(precision unit)で基本的に整数で設定する
          :DirectX12に対して`bias`は`DepthBias`、`slopeBias`は`SlopeScaledDepthBias`と同じ。(`DepthBiasClamp`は無い)
      - `SetShadowSamplingMode(RTI shadowmap, ShadowSamplingMode mode)`: `Texture`の**シャドウサンプリングモード**を設定。(**シャドウマップ**を**サンプリング**したい場合に設定(`RawDepth`))
        :`class Texture`が**シャドウマップ**か**通常のテクスチャ**かは、Unity内部で設定される。(`シャドウマップ`になるパターン: `Light`による影の生成, `LightEvent.AfterShadowMap`)
        - `enum ShadowSamplingMode`: DirectX12では`D_SAMPLER_DESC/D_COMPARISON_fUNC`に対応する。
          - `CompareDepths`: **シャドウマップ**のデフォルト。HLSLで`SamplerComparisonState`,`shadowMap.SampleCmp(..)`を使う。
          - `RawDepth`: **シャドウマップ**を**サンプリング**したい場合の設定。HLSLで`SamplerState`,`shadowMap.Sample～(..)`を使う。
          - `None`: **通常のテクスチャ**のデフォルト。
    - ワイヤーフレーム
      - `SetWireframe(bool enable)`: **ワイヤーフレーム描画**を設定。(`D_FILL_MODE⟪_WIREFRAME¦_SOLID⟫`)
    - XR関係
      - `SetSinglePassStereo(SinglePassStereoMode mode)`: シングルパスステレオを設定
      - `SetInstanceMultiplier(uint multiplier)`: インスタンス数に乗算する値を設定
    - FoveatedRendering系
      - `ConfigureFoveatedRendering`: フォービエイテッドレンダリングの構成コマンド
      - `SetFoveatedRenderingMode`: フォービエイテッドレンダリングのモードを設定
      - シェーディングレート
        - `SetShadingRateCombiner`: シェーディングレートコンバイナを設定
        - `SetShadingRateFragmentSize`: 基本のシェーディングレートを設定
        - `SetShadingRateImage`: シェーディングレートイメージを設定
        - `ResetShadingRate`: シェーディングレート状態をデフォルトにリセット
  - RenderTarget系
    - `SetRenderTarget(⟪RTI rt¦RTI＠❰[]❱ color＠❰s❱, RTI depth¦RTB binding⟫,`『**レンダーターゲット**
        `＠○⟦, RenderBuffer○¦⟪Load¦Store⟫Action ＠⟪color¦depth⟫○¦⟪Load¦Store⟫Action⟧,`『⟪Load¦Store⟫Action
        `＠❰int mipLevel＠❰, CubemapFace cubemapFace＠❰, int depthSlice❱❱❱)`『**サブリソース**(無い場合は、RTI(rIt, mipLevel, cubeFace, depthSlice)を使う): >描画先**RTを設定**。
        :`color`バッファと`depth`バッファは**同じ解像度**である必要があるので、`mipLevel != 0`の場合はそれと同じ解像度の`depth`バッファを用意する必要がある。(ミップマップ生成はカラーバッファのみ)
        `cmd`実行中のみ**RT**が切り替わり、`ctx.ExecuteCommandBuffer(cmd)`で`cmd`の実行が終わると**元のRT**に戻る。
    - NativeRenderPass系 (Unity.Drawio/ページ44 参照) (MetalやVulkanでは前提機能)
      - `BeginRenderPass(int width, int height ＠❰, int volumeDepth❱, int samples,`
        `NativeArray<AttachmentDescriptor> attachments, int depthAttachmentIndex, NativeArray<SubPassDescriptor> subPasses ＠❰, ReadOnlySpan<byte> debugNameUtf8❱)`
        :**NativeRenderPassを開始**し、このパスで使用する`attachments`と`subPasses`を設定する
        - `○⟦, ┃int ⟪width¦height¦＃❰volumeDepth❱¦sample⟫⟧`: `attachments`内の全ての**解像度** (必ず全て一致している必要がある)
          :(`volumeDepth`はPixelシェーダで`RT[SV_RenderTargetArrayIndex]`に描画する(Layered Rendering))
        - `NativeArray<AttachmentDescriptor> attachments`: `struct AttachmentDescriptor`: このNativeRenderPassで使用する**全てのアタッチメント**
          - プロパティ
            - `RTI` **loadStoreTarget**: このNativeRenderPassで使う**アタッチメント**
            - `GraphicsFormat graphicsFormat`: `loadStoreTarget`の**ビュー用フォーマット**(Unity.drawio/ページ41 参照)
            - `RenderBufferLoadAction` **loadAction**: `enum RenderBufferLoadAction`: `⟪Load¦Clear¦DontCare⟫`
            - `○⟦, ┃○¦⟪Color¦float¦uint⟫ clear○¦⟪Color¦Depth¦Stencil⟫⟧`: `loadAction.Clear`時のクリア値
            - `RenderBufferStoreAction` **storeAction**: `enum RenderBufferStoreAction`: `⟪Store¦Resolve¦StoreAndResolve¦DontCare⟫`
            - `RTI resolveTarget`: `storeAction.＠❰StoreAnd❱Resolve`時の**リゾルブ先RT**
          - メソッド
            - `.ctor(GraphicsFormat format, RTI target ○⟦, bool ⟪loadExistingContents『load～.Load』¦storeResult『store～.Store』¦resolve『store～.＠❰StoreAnd❱Resolve』⟫⟧)`
            - `ConfigureClear(○⟦, ┃○¦⟪Color¦float¦uint⟫ clear○¦⟪Color¦Depth¦Stencil⟫⟧)`: `loadAction.Clear`時のクリア値 を設定
            - `ConfigureTarget(RTI target, bool loadExistingContents『load～.Load』, bool storeResults『store～.Store』)`: **アタッチメント**を設定 (`bool`:`false`は`DontCare`)
        - `int depthAttachmentIndex`: `attachments`内の**デプスアタッチメント**のIndexを指定
        - `NativeArray<SubPassDescriptor> subPasses`: `struct SubPassDescriptor`: **各SubPass**の**入力**と**出力**の**アタッチメント**を指定する (入出力で同じには出来ない)
          - `AttachmentIndexArray inputs`: **カラー**アタッチメントの**入力**。(**FrameBuffer Fetch**)
          - `AttachmentIndexArray colorOutputs`: **カラー**アタッチメントの**出力**。(**タイルレンダリング**)
            - `struct AttachmentIndexArray`: 単なる`⟪int[]¦NativeArray<int>⟫ attachments`を持っているだけ(`implicit`がある)(最大`8つ`まで(`MaxAttachments`))
          - `SubPassFlags flags`: `enum SubPassFlags`: 主に**デプス**アタッチメントの**入出力**
            - `None`: 特になし
            - `ReadOnly＠❰Depth❱＠❰Stencil❱`: `inputs/colorOutputs`の**デプスアタッチメント**版
            - `UseShadingRateImage`: `FoveatedRendering系`の`ShadingRateImage`の使用 (RenderDoc/Rasterizer/Shading Rate Image を設定する)
        - `ReadOnlySpan<byte> debugNameUtf8`: デバッグ用Name (RenderDocで表示される)
      - `NextSubPass()`: **次のSubPassを実行**し、**入出力のアタッチメント**(`subPasses`)を**切り替える**
      - `EndRenderPass()`: **NativeRenderPassを終了**し、**アタッチメント**を**テクスチャとして参照**できるようになる
      - `static bool ThrowOnSetRenderTarget`:`true`:`cmd.SetRenderTarget(..)`された時に**例外をスロー**する。(主に、**NativeRenderPass**を使用中に実行されるのを防ぐため)
  - Property系
    - ShaderProperty系 (設定は**C#コンパイルも跨ぐ**(GPUバッファだから?))
      - 基本ShaderProperty_Set
        - `SetGlobal⟪⟪Float¦Vector¦Matrix⟫＠❰Array❱¦Color¦Integer¦＠❰Constant❱Buffer¦Texture⟫`
          `(int nameID, ｢Type｣ ｢value｣ ＠❰, ..❱)`: `Global`ShaderPropertyを設定
          - `SetGlobalTexture(int nameID, RTI rt ＠❰, RenderTextureSubElement element❱)`
            - `RenderTextureSubElement element`:`enum RenderTextureSubElement`: >`RenderTexture`が内包するさまざまな種類のデータにアクセスするために使います。
              - `Color`: `RenderBuffer rt.colorBuffer`
              - `Depth`: `RenderBuffer rt.depthBuffer` (`rt`生成時に`GraphicsFormat`を`R～_TYPELESS`にする必要があると思われる(Unity.drawio/ページ41参照))
              - `Stencil`: 基本的に**サンプリング出来ない**
              - `Default`: 基本的に`Color`、無い場合は`Depth`
              - `ShadingRate`: 知らない
          - `SetGlobalConstantBuffer(..)`: `class_GraphicsBuffer.md/- **.Constant**: CBV`を参照
          - `SetRandomWriteTarget(int ❰index❱, ⟪GraphicsBuffer buffer, ＠❰bool preserveCounterValue❱¦RTI rt『enableRandomWrite=true』⟫)`:
              :SM4.5**Pixelシェーダー**で**UAV書き込み**をしたい用。(Computeシェーダーは`SetComputeBufferParam(..)`)
              Pixelシェーダー側: `RW～<T> ⟪_buffer¦_rt⟫ : register(u❰index❱)`
              **使用後**は`cmd.ClearRandomWriteTargets()`を呼ぶ必要がある
            - `bool preserveCounterValue`:カウンター値を⟪％`false`:リセット(0)¦`true`:保持⟫
        - `Set⟪Compute¦RayTracing⟫｡⟪⟪Float¦Int⟫Param＠❰s❱¦⟪⟪Vector¦Matrix⟫＠❰Array❱¦＠❰Constant❱Buffer¦Texture⟫Param⟫`
          `(⟪Compute¦RayTracing⟫Shader shader ＠❰, int kernelIndex ❱, int nameID, ｢Type｣ ｢value｣ ＠❰, ..❱)`: ⟪`Compute`¦`RayTracing`⟫ShaderPropertyを設定
          :`Compute`Shaderで`⟪Buffer¦Texture⟫`を設定する場合は`int kernelIndex`が必要。
          後は大体`SetGlobal`と同じ。
      - VP_Matrix,プレーン (**Draw～(..)系**の描画前に設定する)
        - `SetupCameraProperties(Camera camera)`: `camera`からVP_Matrixとクリッププレーンを設定
          :↓,↓↓の **ビューMatrix**,**プロジェクションMatrix** と **クリッピングプレーン**(`float4 unity_CameraWorldClipPlanes[6]`) を設定
        - `SetViewMatrix(Matrix4x4 view)`: ビューMatrix を設定 (`unity_MatrixV`)
          >Unityの`View`空間はOpenGLの規約と一致していて、カメラの前方方向が **-Z方向** なのです。
        - `SetProjectionMatrix(Matrix4x4 proj)`: プロジェクションMatrix を設定 (`glstate_matrix_projection`)
          設定例: `camera.projectionMatrix`, `Matrix4x4.Perspective(60, 1.777f, 0.1f, 100f)`
        - `SetViewProjectionMatrices(Matrix4x4 view, Matrix4x4 proj)`: ビュープロジェクションMatrixを設定(**Build-inのみ**) (`unity_MatrixVP`)
      - その他
        - `SetComputeParamsFromMaterial(ComputeShader computeShader, int kernelIndex, Material material)`: `material`から**ShaderProperty**を設定 (ShaderKeywordは無理)
      - 一時RT
        - `GetTemporaryRT(int nameID, RenderTextureDescriptor desc ＠❰, FilterMode filter❱)`:
          **一時RT**を`nameID`に**C#＆Shaderバインド**する。
        - `ReleaseTemporaryRT(int nameID)`:
          **一時RT**(`nameID`)を**開放**する。(開放しなくても`ctx.ExecuteCommandBuffer(cmd)`か`ctx.Submit()`で開放される?)
      - Rayトレーシング
        - `Set＠❰Global❱RayTracingAccelerationStructure(⟪RayTracingShader rayTracingShader¦ComputeShader computeShader, int kernelIndex⟫, int nameID, RayTracingAccelerationStructure accelerationStructure)`:
          :加速構造(`accelerationStructure`)をシェーダーに設定
    - ShaderKeyword系 (これは`ctx.ExecuteCommandBuffer(cmd)`でリセット?)
      - `SetKeyword(⟪｡ref GlobalKeyword keyword¦⟪Material material¦ComputeShader computeShader⟫, ref LocalKeyword keyword｡⟫, bool value)`
        :⟪`GlobalKeyword`¦`LocalKeyword`⟫の**ShaderKeyword**の状態(`value`)を設定する
- **Action系**
  - Clear系
    - `ClearRenderTarget(RTClearFlags clearFlags, Color＠❰[]❱ backgroundColor＠❰s❱, float depth = 1.0, uint stencil = 0)`: RTを**クリア**
      - `enum RTClearFlags`: `⟪None¦Color¦Depth¦Stencil¦All¦DepthStencil¦ColorDepth¦ColorStencil¦Color⟪0～7⟫⟫` (`Color`は`Color⟪0～7⟫`全てクリア)
  - DrawCall＆Dispatch＠❰Rays❱系: 別ファイル
  - MipMap生成、リゾルブ
    - `GenerateMips(RTI rt)`: `rt`の**MipMap生成**
      :条件: `rt.useMipMap = true;`, `rt.autoGenerateMips = false;`, 解像度が`2^n正方`, MipMap可能な`GraphicsFormat` (アクティブRTで無くて良い(`cmd.SetRenderTarget(..)`不要))
    - `ResolveAntiAliasedSurface(RenderTexture rt ＠❰, RenderTexture target ❱)`: **リゾルブ**(MSAA解決)。`target`を省略した場合は、`rt`自身にリゾルブ
      :条件: `rt.antiAliasing > 1`, `bindTextureMS = true;`, `rt`と`target`の解像度と`GraphicsFormat`が同じ (アクティブRTで無くて良い)
  - Copy系
    - テクスチャ系
      - `CopyTexture(RTI src ＠○⟦, int src⟪Element＃⟪Mip＃⟪X¦Y¦Width¦Height⟫⟫⟫⟧, RTI dst ＠○⟦, int dst⟪Element＃⟪Mip＃⟪X¦Y⟫⟫⟫⟧)`
        :テクスチャのコピー。`src`と`dst`は**サイズ**と**フォーマット**が一致していること。DirectX12API:`R_GraphicsCommandList->CopyTextureRegion(..)`
        (`src`と`dst`が両方とも`texture.isReadable`が`true`ならば、CPU上でもコピーする可能性がある)
      - `ConvertTexture(RTI src ＠❰, int srcElement❱, RTI dst ＠❰, int dstElement❱)`
        :`src`から`dst`へ**Blitしてコピー**。`src`と`dst`は`＠❰非❱RenderTarget`どちらでも良く、`解像度`と`DXGI_FORMAT`が異なっていても良い。
          (内部的には、`[src]`=`Blit`=>`[dstTempRT]`=`CopyTexture`=>`[dst]`をしている(`Blit`を使っているがURPでも使用可能))
    - バッファ系
      - `CopyBuffer(GraphicsBuffer source, GraphicsBuffer dest)`: >GPUによって効率的にコピーされます。
        :条件: `⟪source¦dest⟫.Target.⟪CopySource¦CopyDestination⟫`が必要。`source`と`dest`で`count * stride`が一致している必要がある。
      - `CopyCounterValue(GraphicsBuffer src, GraphicsBuffer dst, uint dstOffsetBytes)`
        :`src`の**カウンター**を`dst`の`dstOffsetBytes`された位置に**コピー**する。(`GraphicsBuffer.CopyCount(..)`と同じ)
        ((`dst`=`new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, sizeof(uint))`))
  - (テクスチャ更新)
    - `IncrementUpdateCount(RTI dest)`: `texture.updateCount`を**インクリメント**するだけ。(**cmd経由**で`texture`を直接更新した場合に使う)
- synchronize系
  - Fence系
    ```CSharp
    computeCmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute); //Computeコマンドのみ追加可能にする
    //～Computeコマンド追加～
    GraphicsFence fence = computeCmd.CreateAsyncGraphicsFence();            //直前の全てのAction系?の実行の完了を追跡  //->CreateFence(..), ->Signal(..)
    graphicsCmd.WaitOnAsyncGraphicsFence(fence);                            //fenceの完了を待つコマンドを追加         //->Wait(..)
    ctx.ExecuteCommandBufferAsync(computeCmd, ComputeQueueType.Default);    //Computeキューにコマンドが積まれる
    ctx.ExecuteCommandBuffer(graphicsCmd);                                  //Graphicsキューにコマンドが積まれる
      //enum ComputeQueueType: ⟪Default『1フレーム内』¦Background『数フレーム』¦Urgent『Graphicsキューより高優先度』⟫
    ctx.Submit(); //コマンド実行
    if(fence.passed){/*.CreateAsyncGraphicsFence()までのコマンド完了後の処理*/} //GraphicsFenceは.passedしか持っていない。//R_Fence->GetCompletedValue()
    ```
    :Fenceを使わなくてもバリア的な同期は取れるみたい。**ctxのキュー**と**Graphicsクラスのキュー**は内部で**同じキューを使っている**みたい
    - `Create＠❰Async❱GraphicsFence(..)`: DirectX12API: `R_Device->CreateFence(..)`, `R_CommandQueue->Signal(..)`
      **このメソッドが呼ばれる前まで**の**コマンドの完了を追跡**するフェンスを作成する
      `❰Async❱版`は`✖❰Async❱版`の簡易ラッパーで、殆ど`❰Async❱版`しか使わないだろう
      - `Create❰Async❱GraphicsFence(＠❰SynchronizationStage stage❱)`: `enum SynchronizationStage`: `⟪Vertex¦Pixel⟫Processing`: 各ステージの完了を追跡
    - `WaitOnAsyncGraphicsFence(GraphicsFence fence ＠❰, SynchronizationStage stage❱)`: GPUを`fence`で一時停止。DirectX12API: `R_CommandQueue->Wait(..)`
    - `SetExecutionFlags(CommandBufferExecutionFlags frag)`:
      - `enum CommandBufferExecutionFlags`: ⟪`.None`¦`AsyncCompute`⟫: `AsyncCompute`: Computeコマンド以外のコマンドをセットすると例外スロー
  - AsyncReadback系
    - `RequestAsyncReadback＠❰IntoNative⟪Slice¦Array⟫❱`
      `(＠❰ref Native⟪Slice¦Array⟫<T> output,『❰IntoNative⟪Slice¦Array⟫❱の場合』❱`『`✖❰IntoNative⟪Slice¦Array⟫❱`の場合、Unityが一時的に確保して次のフレームで破棄してしまう
      `⟪`
        `GraphicsBuffer src ＠❰, int size, int offset❱『バイト単位`
        `¦Texture src ＠❰｡, int mipIndex ＠❰, int x, int width, int y, int height, int z, int depth❱ ＠❰⟪Texture¦Graphics⟫Format dstFormat❱｡❱`
          『`dstFormat`: `src.graphicsFormat`と違う場合は自動変換(`src: R16G16B16A16_SFloat → dstFormat: R8G8B8A8_UNorm`など) (`DirectXTex`を使っている?)
      `⟫`
      `, Action<AsyncGPUReadbackRequest> callback)`
      :`src`の内容をCPUメモリ(⟪**ユーザー**が用意した`output`¦**Unity**が用意した`NativeArray`⟫)への**読み戻し**(`Readback`)を**リクエスト**し、登録された`callback`を呼び出す。
      - `struct AsyncGPUReadbackRequest`:
        - プロパティ
          - `done`,**hasError**: `done`で非同期の完了をチェック(不要)し、`hasError`で`Readback`が**成功したか**チェックする。[.done](https://youtu.be/7tjycAEMJNg?t=4660)
          - `bool forcePlayerLoopUpdate`: >Editor上で使用され、GPUリクエストが進行中の間に**Playerループを更新し続ける**かどうか。(Playerループで`Update()`を呼ぶ)
          - `width`,`height`: `RequestAsyncReadback～(..)`の`width`,`height`の`値`がそのまま入る。(`GraphicsBuffer`の場合は`width`=`size`)
          - `depth`,`layerCount`: `depth`= `⟪『3D』depth¦『2DArray』1⟫`, `layerCount`= `⟪『3D』1¦『2DArray』depth⟫`
          - `layerDataSize`: `layerDataSize`=`width * height * depth * ⟪src.graphicsFormat¦dstFormat⟫` (総データサイズ = `layerDataSize` * `layerCount`)
        - メソッド
          - `NativeArray<T>` **GetData<T>**`(int layer)`: `done`=true,`hasError`=false 時、`Readback`したデータにアクセスできる。`layer`で`layerCount`のレイヤーを取得する
          - `Update()`: **リクエストが完了したか**をチェックし完了した場合は`AsyncGPUReadbackRequest.done=true`などをするメソッド
          - `WaitForCompletion()`: `WaitAllAsyncReadbackRequests()`と同じで**完了を待機**する(CPUブロックする)
    - `WaitAllAsyncReadbackRequests()`
      :**リクエスト**した全ての`AsyncReadback`の**完了を待機**する(`AsyncGPUReadbackRequest.done`を`true`にする)
      (4oはCPUブロックしないと言っているが`ctx.Submit()`で**CPUブロック**するような気がする)
  - コールバック系
    - `InvokeOnRenderObjectCallbacks()`
      :`ctx.Submit()`時、`MonoBehaviour.OnRenderObject()`コールバックを呼び出す。(この**コマンドを追加した位置**に、`GL系`や`Graphics.ExecuteCommandBuffer(cmd)`などで**描画を差し込める**)
  - IssuePlugin系
    - `IssuePluginEventAndData / IssuePluginEventAndDataWithFlags`: データやフラグ付きでプラグインイベントを送信
    - `IssuePluginCustomBlit`: カスタムBlitイベントをプラグインに送信
    - `IssuePluginCustomTextureUpdateV2`: テクスチャ更新イベントを送信
  - LateLatch系
    - `MarkLateLatchMatrixShaderPropertyID`: 遅延ラッチ対象としてマトリクスプロパティをマーク
    - `UnmarkLateLatchMatrix`: マークされた遅延ラッチプロパティを解除
    - `SetLateLatchProjectionMatrices`: ステレオ用投影行列を遅延ラッチとして設定
- FastMemory
  - `Switch⟪Into¦OutOf⟫FastMemory(RTI rt,..)`: 指定された`rt`を **高速なGPUメモリ**に⟪配置¦除外⟫するコマンドを追加します。(**DirectX12には影響なし**。DirectStorageのことではない)
