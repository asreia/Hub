# CommandBuffer (非UnityObject, UnityEngine.CoreModule.dll)

`ctx.ExecuteCommandBuffer(cmd)`は`cmd`を積むだけ。`ctx.Submit()`で積まれた`cmd`の全ての処理が開始される

- ResourceModified系
  - `SetBufferData`: 配列の内容をバッファに設定
  - `SetBufferCounterValue`: Append/Consumeバッファのカウンター値を設定
  - `BuildRayTracingAccelerationStructure`: レイトレ用加速構造を構築
  - `ReleaseTemporaryRT`: 一時的なレンダーテクスチャを解放
- **SetPass系**
  - RenderingState系
    - `SetViewport`: ビューポートを設定
    - `EnableScissorRect / DisableScissorRect`: シザー矩形を有効化/無効化
    - `SetInvertCulling`: カリングを反転
    - `SetGlobalDepthBias`: グローバルなデプスバイアスを設定（シェーダー内の深度オフセットのようなもの）
    - `SetWireframe`: ワイヤーフレーム描画を設定
    - `SetShadowSamplingMode`: シャドウサンプリングモードを設定
    - `SetSinglePassStereo`: シングルパスステレオを設定
    - `SetInstanceMultiplier`: インスタンス数に乗算する値を設定
    - FoveatedRendering系
      - `ConfigureFoveatedRendering`: フォービエイテッドレンダリングの構成コマンド
      - `SetFoveatedRenderingMode`: フォービエイテッドレンダリングのモードを設定
      - `SetShadingRateCombiner`: シェーディングレートコンバイナを設定
      - `SetShadingRateFragmentSize`: 基本のシェーディングレートを設定
      - `SetShadingRateImage`: シェーディングレートイメージを設定
      - `ResetShadingRate`: シェーディングレート状態をデフォルトにリセット
  - RenderTarget系
    - `SetRenderTarget`: 描画先レンダリングターゲットを設定
    - `SetRandomWriteTarget`: Shader Model 4.5 対応のピクセルシェーダーにランダム書き込みターゲットを設定
    - NativeRenderPass系 (Unity.Drawio/ページ44 参照) (MetalやVulkanでは前提機能)
      - `BeginRenderPass(int width, int height ＠❰, int volumeDepth❱, int samples,`
        `NativeArray<AttachmentDescriptor> attachments, int depthAttachmentIndex, NativeArray<SubPassDescriptor> subPasses ＠❰, ReadOnlySpan<byte> debugNameUtf8❱)`:
        **NativeRenderPassを開始**し、このパスで使用する`attachments`と`subPasses`を設定する
        - `○⟦, ┃int ⟪width¦height¦＃❰volumeDepth❱¦sample⟫⟧`: `attachments`内の全ての**解像度** (必ず全て一致している必要がある)
          (`volumeDepth`はPixelシェーダで`RT[SV_RenderTargetArrayIndex]`に描画する(Layered Rendering))
        - `NativeArray<AttachmentDescriptor> attachments`: `struct AttachmentDescriptor`: このNativeRenderPassで使用する**全てのアタッチメント**
          - プロパティ
            - `RTI` **loadStoreTarget**: このNativeRenderPassで使う**アタッチメント**
            - `GraphicsFormat graphicsFormat`: `loadStoreTarget`の**ビュー用フォーマット**(Unity.drawio/ページ41 参照)
            - `RenderBufferLoadAction` **loadAction**: `enum RenderBufferLoadAction`: `⟪Load¦Clear¦DontCare⟫`
            - `○⟦, ┃○¦⟪Color¦float¦uint⟫ clear○¦⟪Color¦Depth¦Stencil⟫⟧`: `loadAction.Clear`時のクリア値
            - `RenderBufferStoreAction` **storeAction**: `enum RenderBufferStoreAction`: `⟪Store¦Resolve¦StoreAndResolve¦DontCare⟫`
            - `RTI resolveTarget`: `storeAction.＠❰StoreAnd❱Resolve`時の**リゾルブ先RT**
          - メソッド
            - `ctor(GraphicsFormat format, RTI target ○⟦, bool ⟪loadExistingContents『load～.Load』¦storeResult『store～.Store』¦resolve『store～.＠❰StoreAnd❱Resolve』⟫⟧)`
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
  - ShaderProperty系
    - Parameter系
      - `SetGlobal⟪⟪Float¦Vector¦Matrix⟫＠❰Array❱¦Color¦Int＠❰eger❱¦＠❰Constant❱Buffer¦Texture⟫`: グローバルシェーダープロパティを設定
      - `SetCompute｡｡｡⟪⟪Float¦Int⟫Param＠❰s❱¦⟪⟪Vector¦Matrix⟫＠❰Array❱¦＠❰Constant❱Buffer¦Texture⟫Param⟫`: ComputeShaderの各種パラメータを設定
      - `SetRayTracing⟪⟪Float¦Int⟫Param＠❰s❱¦⟪⟪Vector¦Matrix⟫＠❰Array❱¦＠❰Constant❱Buffer¦Texture⟫Param⟫`: RayTracingShaderの各種パラメータを設定
      - `SetRayTracingAccelerationStructure / SetGlobalRayTracingAccelerationStructure`: 加速構造をシェーダーに設定
      - `SetComputeParamsFromMaterial`: マテリアルからComputeShaderのパラメータを設定
      - `SetViewMatrix / SetProjectionMatrix / SetViewProjectionMatrices`: ビュー/プロジェクション行列を設定
      - `SetupCameraProperties`: カメラ固有のシェーダー変数のセットアップをスケジュール
      - `GetTemporaryRT / GetTemporaryRTArray`: 一時的なレンダーテクスチャ（配列）を取得
    - Keyword系
      - `SetKeyword / EnableKeyword / DisableKeyword`: ローカルまたはグローバルなキーワードを設定/有効化/無効化
      - `EnableShaderKeyword / DisableShaderKeyword`: 名前指定でシェーダーキーワードを有効/無効に
- **Action系**
  - DrawCall系 (別ファイルにする?)
    - **Draw⟪Mesh¦Procedural⟫**
      - `Draw`**Mesh**`＠❰Instanced＠❰⟪Indirect¦Procedural⟫❱❱`
        `(`
          『基本セット
            `Mesh mesh, int submeshIndex, Material material, int shaderPass,`『**Mesh**と**Material**
            `＠❰Matrix4x4＠❰[]❱ matrix❱, ＠❰MaterialPropertyBlock properties❱`『**ShaderProperty**。⟪Indirect¦Procedural⟫は`matrix`が無い
          『❰Instanced＠❰Procedural❱❱
            `＠❰int count❱`『**Instance数**。❰Procedural❱は❰Indirect❱の`bufferWithArgs`を`count`に変えたもの
          『❰Instanced❰Indirect❱❱
            `＠❰GraphicsBuffer bufferWithArgs, ＠❰int argsOffset❱❱`『**引数バッファ**
        `)`
      - `Draw`**Procedural**`＠❰Indirect❱`
        `(`
          『基本セット
            `MeshTopology topology, Material material, int shaderPass,`『**トポロジー**と**Material**
            `Matrix4x4 matrix, ＠❰MaterialPropertyBlock properties❱`『**ShaderProperty**
            `＠❰GraphicsBuffer indexBuffer❱,`『**Indexバッファ**
          『✖❰Indirect❱
            `＠❰int indexCount, ＠❰int instanceCount❱❱,`『**Index数**と**Instance数**
          『❰Indirect❱
            `＠❰GraphicsBuffer bufferWithArgs, ＠❰int argsOffset❱❱`『**引数バッファ**。`❰int indexCount, int instanceCount❱`を詰める
        `)`
      - 引数説明
        - `＠❰Matrix4x4＠❰[]❱ matrix❱`: 多分`UNITY_MATRIX_M`を設定している
        - `int shaderPass`: デフォルト`-1`で**全てのパスを描画**する (URPでもcmdを直接操作しているので描画される)
        - ❰Instanced❱: **インスタンシング**は`Material.enableInstancing`が`true`であること (インスペクターでも設定できる)
        - `＠❰MaterialPropertyBlock properties❱`: 4oはURPでも、**テクスチャや配列でなければSRP Batcherでも使える**と言っている
        - `GraphicsBuffer bufferWithArgs`: `argsOffset`を使って`D_DRAW_INDEXED_ARGUMENTS`を1個の選択できる。`DrawMeshInstancedIndirect(..)`時、`int submeshIndex`との競合は、
          4o「メッシュバインド用にどのサブメッシュ(`int submeshIndex`)を選ぶかだけ教えてね。あとはバッファ(`bufferWithArgs`)の指示に従うから！」らしい
        - `＠❰GraphicsBuffer indexBuffer❱`: `SV_VertexID`!=`indexBuffer`であり、`SV_VertexID`は単純に**頂点処理の連番**である
          (`＠❰StructuredBuffer<uint>❱ _indexBuffer[SV_VertexID]`で参照できる)
    - `Draw`**Renderer**`(Renderer renderer, Material material, int submeshIndex = 0, int shaderPass = -1)`:
      多分、`renderer`を**Mesh**(`submeshIndex`)と**UnityPerDraw**としてしか使っていない
    - `Draw`**RendererList**`(Rendering.RendererList rendererList)`: >RendererList に含まれる可視な GameObject を描画するスケジュールを行います。
      - `struct RendererList`:
        - `bool isValid`: `RendererList`が無効な場合は`false`を返す
        - `static RendererList nullRendererList()`: 空の`RendererList`を返す
      - `RendererList ctx.`**CreateRendererList**`(⟪RendererListDesc desc¦ref RendererListParams param⟫)`:
        - `struct RendererListDesc`:
        - `struct` **RendererListParams**:
          - `.ctor(CullingResults cullingResults, DrawingSettings drawSettings, FilteringSettings filteringSettings)`:
            Culling{cullingMask,Occlu⟪der¦dee⟫,CPUの**AABBフラスタムカリング**} => Filtering => Drawing
          - **CullingResults** `cullingResults`: `struct CullingResults`: 描画対象となる**可視オブジェクトセット**。`ctx.Cull(ref ScriptableCullingParameters『カメラ』)`を設定
          - **FilteringSettings** `filteringSettings`: `struct FilteringSettings`: **可視オブジェクトセット**のフィルタリング方法
            - `static FilteringSettings defaultValue`: フィルタリングをしない設定の値
            - `.ctor(Nullable<RenderQueueRange> renderQueueRange = RenderQueueRange.all, int layerMask, uint renderingLayerMask, int excludeMotionVectorObjects 『⟪％❰0❱¦1⟫』)`:
            - `uint batchLayerMask`: **Unity側が構築したBRG**の**Batchレイヤー**の**ビットマスク**。BRGのBatchをスイッチして簡単に最適化できるが、Unity側が作ったBRGがよく分からない
            - `Tags{"LightMode" = "MotionVectors"}`がある`Material`での**描画有無**。(動いているかは、`UNITY_MATRIX_MV`が**前回のフレームと違うか**を確認する)
              - `bool excludeMotionVectorObjects`: `true`: **動いている**オブジェクトを**除外**
              - `bool forceAllMotionVectorObjects`: `true`: **止まっている**オブジェクトを**強制描画**
            - `int layerMask`: `Camera.cullingMask`(**GameObject.layer**)をさらにフィルタリングするビットマスク
            - `uint renderingLayerMask`: `Renderer`**.renderingLayerMask**をフィルタリングするビットマスク
            - `RenderQueueRange renderQueueRange`: `struct RenderQueueRange`: `⟪Material`**.renderQueue**`¦Tags{"Queue"}⟫`の範囲をフィルタリングする
              - `○¦＠❰.ctor(❱int lowerBound, int upperBound○¦＠❰)❱, ○⟦, ┃static ⟪RenderQueueRange ⟪all¦opaque¦transparent⟫¦int ⟪minimumBound¦maximumBound⟫⟫⟧`
            - `SortingLayerRange sortingLayerRange`: `struct SortingLayerRange`: **SortingLayer**`.value`の範囲をフィルタリングする (PS/Tags and Layers/Sorting Layers 参照)
              - `○¦＠❰.ctor(❱short lowerBound, short upperBound○¦＠❰)❱`, `static SortingLayerRange all`
          - **DrawingSettings** `drawSettings`: `struct DrawingSettings`: オブジェクトの描画方法
            - `static int maxShaderPasses`: 1 回の DrawRenderers 呼び出しでレンダリングできるパスの最大数
            - `.ctor(ShaderTagId shaderPassName, SortingSettings sortingSettings)`:
            - `SortingSettings sortingSettings`: `struct SortingSettings`: ソート順設定（必須）
              - `SortingCriteria criteria`: `enum SortingCriteria`: 組み合わせ可能。**優先度は上から適用**される
                - `None`:
                - `SortingLayer`: `SortingLayer.value`順
                - `RenderQueue`: `Material.renderQueue`順
                - 距離 (これより下は**同一距離**) (**Z Pre Pass**を書いていれば、インスタンシングで効率描画できる？)
                  - `BackToFront`: `far`から`near`順
                  - `QuantizedFrontToBack`: `near`から`far`順 (`Quantized`は**粗い距離でソート**しCPU負荷を削減)
                - `OptimizeStateChanges`: これより下は**同一シェーダ**
                - `CanvasOrder`: Order in Layer❰`sortingOrder`❱順
                - `RendererPriority`: `Renderer.rendererPriority`順
                - 組み合わせ
                  - `CommonOpaque`: (`SortingLayer`|`RenderQueue`|`QuantizedFrontToBack`|`OptimizeStateChanges`|`CanvasOrder`)
                  - `CommonTransparent`: (`SortingLayer`|`RenderQueue`|`BackToFront`|`OptimizeStateChanges`)
              - `Vector3 cameraPosition`:
              - `Vector3 customAxis`:
              - `DistanceMetric distanceMetric`: `enum DistanceMetric`:
              - `Matrix4x4 worldToCameraMatrix`:
            - `bool enableDynamicBatching`,`％❰false❱`: 動的バッチングの有効化
            - `bool enableInstancing`,`％❰true❱`: GPUインスタンシングの有効化
            - `Material fallbackMaterial`: (`.ctor(ShaderTagId shaderPassName, .)`との?)条件不一致時に使うフォールバックマテリアル
            - `int lodCrossFadeStencilMask`: LODクロスフェード用ステンシルマスク（32bit int）
            - `int mainLightIndex`: メインライトとして使うライトのインデックス（-1で無効）
            - `Material overrideMaterial`: マテリアルをオーバーライドする場合のマテリアル
            - `int overrideMaterialPassIndex`: overrideMaterialのどのパスを使用するか（例：0 = Base Pass）
            - `Shader overrideShader`: シェーダーをオーバーライドする場合のシェーダー //new Material(overrideShader) ?
            - `int overrideShaderPassIndex`: overrideShaderのどのパスを使用するか
            - `PerObjectData perObjectData`: per-objectごとに取得するデータ（Transform, LightProbe, LightIndex等）
            - `ShaderTagId GetShaderPassName(int index)`: シェーダ パスの名前を取得します
            - `SetShaderPassName(int index, ShaderTagId shaderPassName)`: シェーダ パスの名前を設定します
          - `isPassTagName`:
          - `stateBlocks`:
          - `tagName`:
          - `tagValues`:
          - `static RendererListParams Invalid`: 空の`RendererListParams`を返す
    - `DrawOcclusionMesh(RectInt normalizedCamViewport)`:
      ビューポートの範囲(`normalizedCamViewport`)に**VRデバイスが提供するOcclusion Mesh**(見えない部分のメッシュ)を**深度バッファ**に**Nearクリップ面で描画**する
    - _
    - `DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex = 0, int shaderPass = -1, ＠❰MaterialPropertyBlock properties❱)`
    - `DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, ＠❰int count❱, ＠❰MaterialPropertyBlock properties❱)`
    - `DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, ＠❰int argsOffset❱, ＠❰MaterialPropertyBlock properties❱)`
    - `DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, ＠❰MaterialPropertyBlock properties❱)`
    - `DrawOcclusionMesh(RectInt normalizedCamViewport)`
    - `DrawProcedural(＠❰GraphicsBuffer indexBuffer❱, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, ＠❰int instanceCount❱, ＠❰MaterialPropertyBlock properties❱)`
    - `DrawProceduralIndirect(＠❰GraphicsBuffer indexBuffer❱, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, ＠❰int argsOffset❱, ＠❰MaterialPropertyBlock properties❱)`
    - `DrawRenderer(Renderer renderer, Material material, int submeshIndex = 0, int shaderPass = -1)`
    - `void DrawRendererList(Rendering.RendererList rendererList)`
    - `DrawMesh`: メッシュを描画するコマンドを追加
    - `DrawMeshInstanced`: インスタンシングを使用してメッシュを描画
    - `DrawMeshInstancedIndirect`: インスタンシング（間接）でメッシュを描画
    - `DrawMeshInstancedProcedural`: Procedural Instancing によりメッシュ描画
    - `DrawRenderer`: Renderer を描画するコマンド
    - `DrawRendererList`: RendererList を描画するコマンド
    - `DrawOcclusionMesh`: VRデバイスのオクルージョンメッシュを描画
    - `DrawProcedural`: 手続き型ジオメトリを描画
    - `DrawProceduralIndirect`: 手続き型ジオメトリ（間接）を描画
    - `Blit`: **URP非推奨**。`DrawMesh(..)`などを使って**低レベル操作で実現**する。テクスチャを別のレンダーテクスチャにシェーダーを使ってコピー
  - Dispatch系
    - `DispatchCompute`: ComputeShaderを実行するコマンドを追加（スレッドグループ指定）
  - DispatchRays系
    - `SetRayTracingShaderPass`: レイ/ジオメトリ交差シェーダーに使うパスを指定
    - `DispatchRays`: RayTracingShaderを実行
  - Copy系
    - `CopyTexture(RTI src ＠○⟦, int src⟪Element＃⟪Mip＃⟪X¦Y¦Width¦Height⟫⟫⟫⟧, RTI dst ＠○⟦, int dst⟪Element＃⟪Mip＃⟪X¦Y⟫⟫⟫⟧)`:
      テクスチャのコピー。`src`と`dst`は**サイズ**と**フォーマット**が一致していること。DirectX12API:`R_GraphicsCommandList->CopyTextureRegion(..)`
    - `ConvertTexture(RTI src ＠❰, int srcElement❱, RTI dst ＠❰, int dstElement❱)`:
      `src`から`dst`へ**Blitしてコピー**。`src`と`dst`は`＠❰非❱RenderTarget`どちらでも良く、`解像度`と`DXGI_FORMAT`が異なっていても良い。
        (内部的には、`[src]`=`Blit`=>`[dstTempRT]`=`CopyTexture`=>`[dst]`をしている(`Blit`を使っているがURPでも使用可能))
    - `CopyBuffer`: GraphicsBufferの内容を別のバッファへコピー
    - `CopyCounterValue`: ComputeBufferまたはGraphicsBufferのカウンタ値をコピー
  - Clear系
    - `ClearRenderTarget`: レンダリングターゲットをクリア
    - `ClearRandomWriteTargets`: ランダム書き込みターゲットを解除（Shader Model 4.5向け）
  - other
    - `ResolveAntiAliasedSurface`: アンチエイリアス済みテクスチャを解決
    - `GenerateMips`: レンダーテクスチャのミップマップを生成
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
    Fenceを使わなくてもバリア的な同期は取れるみたい。**ctxのキュー**と**Graphicsクラスのキュー**は内部で**同じキューを使っている**みたい
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
      `, Action<AsyncGPUReadbackRequest> callback)`:
      `src`の内容をCPUメモリ(⟪**ユーザー**が用意した`output`¦**Unity**が用意した`NativeArray`⟫)への**読み戻し**(`Readback`)を**リクエスト**し、登録された`callback`を呼び出す。
      - `struct AsyncGPUReadbackRequest`:
        - プロパティ
          - `done`,`hasError`: `done`で非同期の完了をチェック(不要)し、`hasError`で`Readback`が**成功したか**チェックする。[.done](https://youtu.be/7tjycAEMJNg?t=4660)
          - `bool forcePlayerLoopUpdate`: >Editor上で使用され、GPUリクエストが進行中の間に**Playerループを更新し続ける**かどうか。(Playerループで`Update()`を呼ぶ)
          - `width`,`height`: `RequestAsyncReadback～(..)`の`width`,`height`の`値`がそのまま入る。(`GraphicsBuffer`の場合は`width`=`size`)
          - `depth`,`layerCount`: `depth`= `⟪『3D』depth¦『2DArray』1⟫`, `layerCount`= `⟪『3D』1¦『2DArray』depth⟫`
          - `layerDataSize`: `layerDataSize`=`width * height * depth * ⟪src.graphicsFormat¦dstFormat⟫` (総データサイズ = `layerDataSize` * `layerCount`)
        - メソッド
          - `NativeArray<T> GetData<T>(int layer)`: `done`=true,`hasError`=false 時、`Readback`したデータにアクセスできる。`layer`で`layerCount`のレイヤーを取得する
          - `Update()`: **リクエストが完了したか**をチェックし完了した場合は`AsyncGPUReadbackRequest.done=true`などをするメソッド
          - `WaitForCompletion()`: `WaitAllAsyncReadbackRequests()`と同じで**完了を待機**する(CPUブロックする)
    - `WaitAllAsyncReadbackRequests()`:
      **リクエスト**した全ての`AsyncReadback`の**完了を待機**する(`AsyncGPUReadbackRequest.done`を`true`にする)
      (4oはCPUブロックしないと言っているが`ctx.Submit()`で**CPUブロック**するような気がする)
  - LateLatch系
    - `MarkLateLatchMatrixShaderPropertyID`: 遅延ラッチ対象としてマトリクスプロパティをマーク
    - `UnmarkLateLatchMatrix`: マークされた遅延ラッチプロパティを解除
    - `SetLateLatchProjectionMatrices`: ステレオ用投影行列を遅延ラッチとして設定
  - IssuePlugin系
    - `IssuePluginEventAndData / IssuePluginEventAndDataWithFlags`: データやフラグ付きでプラグインイベントを送信
    - `IssuePluginCustomBlit`: カスタムBlitイベントをプラグインに送信
    - `IssuePluginCustomTextureUpdateV2`: テクスチャ更新イベントを送信
- CommandBuffer系
  - `Clear`: コマンドバッファ内のすべてのコマンドをクリア
  - `IncrementUpdateCount`: テクスチャの updateCount プロパティをインクリメント（更新を強制する用途など）
  - `InvokeOnRenderObjectCallbacks`:
    `ctx.Submit()`時、`MonoBehaviour.OnRenderObject()`コールバックを呼び出す。(この**コマンドを追加した位置**に、`GL系`や`Graphics.ExecuteCommandBuffer(cmd)`などで**描画を差し込める**)
  - `BeginSample`: プロファイリングの開始コマンドを追加
  - `EndSample`: プロファイリングの終了コマンドを追加

- 🟩 基本描画・処理制御
- 🟥 描画コマンド関連
- 🟦 テクスチャとバッファ操作
- 🟨 シェーダーパラメータ設定
- 🟧 レンダリング設定・ステート
- 🟫 フォービエイテッドレンダリング／可変レート関連
- 🟪 RayTracing 関連
- 🟥 プラグイン・イベント関連
  - IssuePluginEvent ネイティブプラグインにイベントを送信
- 🔷 ステレオレンダリング・Late Latch
- 🔸 同期関連

- 🟩 基本描画・処理制御
  - `BeginRenderPass`: ネイティブレンダーパスを開始するコマンドを追加
  - `EndRenderPass`: アクティブなネイティブレンダーパスを終了
  - `NextSubPass`: BeginRenderPassで定義された次のサブパスを開始
  - `BeginSample`: プロファイリングの開始コマンドを追加
  - `EndSample`: プロファイリングの終了コマンドを追加
  - `Clear`: コマンドバッファ内のすべてのコマンドをクリア
  - `SetExecutionFlags`: コマンドバッファの実行方法に関する意図を示すフラグを設定
  - `InvokeOnRenderObjectCallbacks`: MonoBehaviour.OnRenderObject()コールバックを呼び出すようスケジュールする
- 🟥 描画コマンド関連
  - `DrawMesh`: メッシュを描画するコマンドを追加
  - `DrawMeshInstanced`: インスタンシングを使用してメッシュを描画
  - `DrawMeshInstancedIndirect`: インスタンシング（間接）でメッシュを描画
  - `DrawMeshInstancedProcedural`: Procedural Instancing によりメッシュ描画
  - `DrawRenderer`: Renderer を描画するコマンド
  - `DrawRendererList`: RendererList を描画するコマンド
  - `DrawOcclusionMesh`: VRデバイスのオクルージョンメッシュを描画
  - `DrawProcedural`: 手続き型ジオメトリを描画
  - `DrawProceduralIndirect`: 手続き型ジオメトリ（間接）を描画
  - `Blit`: テクスチャを別のレンダーテクスチャにシェーダーを使ってコピー
  - `DispatchCompute`: ComputeShaderを実行するコマンドを追加（スレッドグループ指定）
- 🟦 テクスチャとバッファ操作
  - `ConvertTexture`: テクスチャを別形式に変換してコピー
  - `CopyTexture`: テクスチャからテクスチャへピクセルデータをコピー
  - `CopyBuffer`: GraphicsBufferの内容を別のバッファへコピー
  - `CopyCounterValue`: ComputeBufferまたはGraphicsBufferのカウンタ値をコピー
  - `SetBufferData`: 配列の内容をバッファに設定
  - `SetBufferCounterValue`: Append/Consumeバッファのカウンター値を設定
  - `GetTemporaryRT / GetTemporaryRTArray`: 一時的なレンダーテクスチャ（配列）を取得
  - `ReleaseTemporaryRT`: 一時的なレンダーテクスチャを解放
  - `ResolveAntiAliasedSurface`: アンチエイリアス済みテクスチャを解決
  - `GenerateMips`: レンダーテクスチャのミップマップを生成
  - `RequestAsyncReadback`: 非同期GPUリードバック要求を追加
  - `RequestAsyncReadbackIntoNativeArray`: NativeArray<T>への非同期GPUリードバックをリクエスト
  - `RequestAsyncReadbackIntoNativeSlice`: NativeSlice<T>への非同期GPUリードバックをリクエスト
  - `SetRandomWriteTarget`: Shader Model 4.5 対応のピクセルシェーダーにランダム書き込みターゲットを設定
  - `ClearRandomWriteTargets`: ランダム書き込みターゲットを解除（Shader Model 4.5向け）
  - `IncrementUpdateCount`: テクスチャの updateCount プロパティをインクリメント（更新を強制する用途など）
- 🟨 シェーダーパラメータ設定
  - `SetGlobal⟪⟪Float¦Vector¦Matrix⟫＠❰Array❱¦Color¦Int＠❰eger❱¦＠❰Constant❱Buffer¦Texture⟫`: グローバルシェーダープロパティを設定
  - `SetCompute｡｡｡⟪⟪Float¦Int⟫Param＠❰s❱¦⟪⟪Vector¦Matrix⟫＠❰Array❱¦＠❰Constant❱Buffer¦Texture⟫Param⟫`: ComputeShaderの各種パラメータを設定
  - `SetRayTracing⟪⟪Float¦Int⟫Param＠❰s❱¦⟪⟪Vector¦Matrix⟫＠❰Array❱¦＠❰Constant❱Buffer¦Texture⟫Param⟫`: RayTracingShaderの各種パラメータを設定
  - `SetComputeParamsFromMaterial`: マテリアルからComputeShaderのパラメータを設定
  - `SetKeyword / EnableKeyword / DisableKeyword`: ローカルまたはグローバルなキーワードを設定/有効化/無効化
  - `EnableShaderKeyword / DisableShaderKeyword`: 名前指定でシェーダーキーワードを有効/無効に
  - `SetupCameraProperties`: カメラ固有のシェーダー変数のセットアップをスケジュール
- 🟧 レンダリング設定・ステート
  - `SetRenderTarget`: 描画先レンダリングターゲットを設定
  - `ClearRenderTarget`: レンダリングターゲットをクリア
  - `SetViewMatrix / SetProjectionMatrix / SetViewProjectionMatrices`: ビュー/プロジェクション行列を設定
  - `SetViewport`: ビューポートを設定
  - `EnableScissorRect / DisableScissorRect`: シザー矩形を有効化/無効化
  - `SetInvertCulling`: カリングを反転
  - `SetGlobalDepthBias`: グローバルなデプスバイアスを設定（シェーダー内の深度オフセットのようなもの）
  - `SetWireframe`: ワイヤーフレーム描画を設定
  - `SetShadowSamplingMode`: シャドウサンプリングモードを設定
  - `SetSinglePassStereo`: シングルパスステレオを設定
  - `SetInstanceMultiplier`: インスタンス数に乗算する値を設定
- 🟫 フォービエイテッドレンダリング／可変レート関連
  - `ConfigureFoveatedRendering`: フォービエイテッドレンダリングの構成コマンド
  - `SetFoveatedRenderingMode`: フォービエイテッドレンダリングのモードを設定
  - `SetShadingRateCombiner`: シェーディングレートコンバイナを設定
  - `SetShadingRateFragmentSize`: 基本のシェーディングレートを設定
  - `SetShadingRateImage`: シェーディングレートイメージを設定
  - `ResetShadingRate`: シェーディングレート状態をデフォルトにリセット
- 🟪 RayTracing 関連
  - `BuildRayTracingAccelerationStructure`: レイトレ用加速構造を構築
  - `SetRayTracingAccelerationStructure / SetGlobalRayTracingAccelerationStructure`: 加速構造をシェーダーに設定
  - `DispatchRays`: RayTracingShaderを実行
  - `SetRayTracingShaderPass`: レイ/ジオメトリ交差シェーダーに使うパスを指定
- 🟥 プラグイン・イベント関連
  - IssuePluginEvent ネイティブプラグインにイベントを送信
  - `IssuePluginEventAndData / IssuePluginEventAndDataWithFlags`: データやフラグ付きでプラグインイベントを送信
  - `IssuePluginCustomBlit`: カスタムBlitイベントをプラグインに送信
  - `IssuePluginCustomTextureUpdateV2`: テクスチャ更新イベントを送信
- 🔷 ステレオレンダリング・Late Latch
  - `MarkLateLatchMatrixShaderPropertyID`: 遅延ラッチ対象としてマトリクスプロパティをマーク
  - `UnmarkLateLatchMatrix`: マークされた遅延ラッチプロパティを解除
  - `SetLateLatchProjectionMatrices`: ステレオ用投影行列を遅延ラッチとして設定
- 🔸 同期関連
  - `CreateGraphicsFence / CreateAsyncGraphicsFence`: GPUフェンスを作成
  - `WaitOnAsyncGraphicsFence`: GPUをフェンスで一時停止
  - `WaitAllAsyncReadbackRequests`: 全ての非同期GPUリードバック完了を待機

CommandBufferExtensions: Switch⟪Into¦OutOf⟫FastMemory
