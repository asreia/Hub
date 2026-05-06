# RenderGraph

- `class_CommandBuffer.md/NativeRenderPass系`,`DirectX12メモ.md/RenderPass`も参照

- [Render Graph Viewer](images\RenderGraphViewer.png) [cmdの分割](images\cmdの分割.png)
- **プラットフォーム差異**は、`UNITY_UV_STARTS_AT_TOP`(`TextureUVOrigin`で調整?), [NRPデプスバグ](images\NRPデプスバグ.png), [LOAD_FB_INPUT(_,ココ)](images\LOAD_FB_INPUT(_,ココ).png)
  - [UVOrigin＆デプスバグ](images\UVOrigin＆デプスバグ.png)
- メモ: [BlitCancel](images\memo_BlitCancel.png), [NRP](images\memo_NRP.png)

- **ResourceHandle**
  - `ResourceHandle`は**抽象ハンドル**。(**リソース**の**生存期間**(生成/破棄)などの**管理**にも使われる)
  - `ResourceHandle`の**有効区間**は、**Recording区間**(`Pass`内含む)。(`builder.CreateTransient⟪Texture¦Buffer⟫(..)`は`Pass`内のみ)
    - `builder.SetRenderFunc((..)=>{`**ココ**`})`のみ**実際に使用**でき`implicit`で各型に変換可能。(`ココ`以外は`RenderGraphResourceRegistry.current`が`null`でエラー)
  - **ResourceHandle renderGraph.～(..)**=>`passData`=>**builder.～(ResourceHandle, AccessFlags)**=>`cmd.～(..)` (`renderGraph`で**ResourceHandle**を生成し、`builder`で**AccessFlags**を付ける)
    - `RTI`=>`TextureHandle renderGraph.Import～(..)`=>`passData`=>`builder.UseTexture(..)`=>`(..)=>`{`cmd.SetGlobalTexture(..)`} (`builder.AllowGlobalStateModification(true)`も必要)

- **NativeRenderPass**
  - **NRPパスマージの条件**は、`解像度`,`MSAAサンプル数`,`VolumeDepth`が**一致**し`デプスバッファ`を**共有**する。1つの`Pass`内で`Set～Attachment～(..)`の**NRP条件**の**一致**が**崩れる**と**エラー**となる。
    ↑(パスカリング後に**NRP条件**から**外れるまで**複数のSubPassを1つのNRPに含め続ける(`RasterRenderPass`のみパスマージされる))[PassBreakReason](images\PassBreakReason.png)
  - `cmd.SetGlobalTexture(..)`しても**NRPパスマージ**は**切れない**
  - **index**について
    - 基本的に`Set⟪Render¦Input⟫Attachment(., index)`の`index`は、`SV_Target##index`と`＠❰LOAD_❱FRAMEBUFFER_INPUT_X＠❰_FLOAT❱(index, ＠❰.❱)`に**一致**していること
    - `index`は`Render`と`Input`の`Attachment`それぞれ`0`から始まる。シェーダー側の**使われない**`index`(FB,MRT)が**定義**してあっても問題なく**動く**
    - `Set⟪Render¦Input⟫Attachment(..)`を**コメントアウト**しても`index`が`0`から詰められてシェーダーに渡り描画する。(とりあえず**動く**)
  - **NRP中**の`cmd.ClearRenderTarget(..)`は*DirectX12*では`DrawIndexedInstance(..)`のフルスクリーン描画に置換される

- **Backbuffer設定** (`BRTT.⟪CameraTarget¦Depth⟫`の`Import⟪Backbuffer¦Texture⟫`)
  ```csharp
      var backbufferInfo = new RenderTargetInfo
      {
          width       = cameras[0].pixelWidth,  //Screen.width, //どちらでも動くが`Camera`の方が正確らしい
          height      = cameras[0].pixelHeight, //Screen.height,
          volumeDepth = 1,
          msaaSamples = 1,
          bindMS      = false,
          format      = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR),
      };
      var CameraDepthInfo = backbufferInfo;
      CameraDepthInfo.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);

      //このメソッドを使うと`Backbuffer`になるぽい
      TextureHandle backbufferDayo = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget, backbufferInfo); 
      TextureHandle cameraDepth = renderGraph.ImportTexture(RTHandles.Alloc(BuiltinRenderTextureType.Depth, "Camera Depth"), CameraDepthInfo);

      using (var builder = renderGraph.AddRasterRenderPass<ClearPassData>("Clear Backbuffer", out var passData)) //普通は`ImportResourceParams`でクリアする
      {
          builder.SetRenderAttachment(backbufferDayo, 0);
          builder.SetRenderAttachmentDepth(cameraDepth);

          builder.SetRenderFunc(static (ClearPassData data, RasterGraphContext RasterCtx) =>
          {
              //DX12: ClearRenderTargetView(..)ではなくDrawIndexedInstanced(6,1)で描画されている..
              RasterCtx.cmd.ClearRenderTarget(RTClearFlags.Color, Color.aquamarine, 1.0f, 0);
          });
      }
  ```

- `RenderGraph`の**API早見**
  - **区間**: `RenderGraph`,`Recording`,`NRP`,`Pass`
  - **Pass**: `＠❰Add❱⟪Raster＠❰Render❱¦Compute¦Unsafe⟫⟪Pass¦GraphContext⟫`
  - **リソース**: `＠⟪Create¦Import¦Use⟫⟪Texture¦Buffer¦RendererList⟫＠⟪Desc¦Handle⟫`
  - **Attachment**: `Set⟪Input¦Render¦RandomAccess⟫Attachment＠❰Depth❱`

## RenderGraph

- `string name {get;}`: `renderGraph`の名前
- `static List<RenderGraph> GetRegisteredRenderGraphs()`: >登録されている全ての`RenderGraph`の`List`を取得します。
- `bool` **nativeRenderPassesEnabled** `{get; set;}`: >`AddRasterRenderPass()`の従来の`SetRenderTarget(..)`の代わりに、**NRPの使用を有効**(`BeginRenderPass(..)`)にします(6000.3以降デフォルトで有効)。(`false`にすると`DirectX12`が従来に戻る)
- `enum RenderTextureUVOriginStrategy renderTextureUVOriginStrategy {get;}`: `BeginRecording(RenderGraphParameters parameters)`で設定した値。
- `static bool isRenderGraphViewerActive {get;}`: >`true`の場合、*Render Graph Viewer*はアクティブです。

## ライフサイクル

- **.ctor**`(string name = "RenderGraph")`
- `void` **BeginRecording**`(RenderGraphParameters parameters)`: >`RenderGraph`の**記録を開始**します。
  - `struct RenderGraphParameters parameters`:
    - `CommandBuffer` **cmd**, `ScriptableRenderContext` **ctx**: `cmd`は`Rendering`毎に別々にして`ctx.ExecuteCommandBuffer(cmdN)`することも可能
    - `int currentFrameIndex`: **Time.frameCount**を設定する (`EndFrame()`で使われる)
    - `bool generateDebugData`: `true`にして↓を設定すると*Render Graph Viewer*が表示される
    - `EntityId executionId`: `cameras[i].GetEntityId()`を設定する
    - `bool rendererListCulling`: >`RendererList`のカリングを有効にするかどうかを制御します。? `CullingResults`の時点でカリングされているが?->gpt:RendererList が空 → Pass 自体を消す
    - `enum RenderTextureUVOriginStrategy renderTextureUVOriginStrategy`: >`renderGraph`がグラフ内の`TextureUVOriginSelection.Unknown`(`CreateTexture(..)`)の**UV原点の位置**の戦略
      - ％`BottomLeft`: >RenderTextures は常に左下方向として扱われます。
      - `PropagateAttachmentOrientation`: >RenderTextures は`Attachment`読み取り経由でのみ使用される場合、バックバッファ`Attachment`の方向を継承することがあります。
- `I⟪Raster¦Compute¦Unsafe⟫RenderGraph`**Builder** `Add`**⟪RasterRender¦Compute¦Unsafe⟫Pass**`<PassData>(string passName, out PassData passData ＠❰, ProfilingSampler sampler❱)`[AddRasterRenderPass](images\AddRasterRenderPass.png)
- `void` **EndRecordingAndExecute**`()`: `Recording`を**コンパイル**して**実行**する。[EndRecordingAndExecute](images\EndRecordingAndExecute.png)
- `void` **EndFrame**`()`: 恐らく基本的に**10フレーム間使われなかったリソースを解放**。[EndFrame](images\EndFrame.png)
- `bool ResetGraphAndLogException(Exception e)`: >グラフの記録または実行中に発生する可能性のある例外をキャッチして記録します
  - これを入れないと不安定になる気がする
    ```csharp
    try
    {
        renderGraph.BeginRecording(rgParams);
        /*～Recording～*/
        renderGraph.EndRecordingAndExecute();

        renderGraph.BeginRecording(rgParams);
        /*～Recording～*/
        renderGraph.EndRecordingAndExecute();

        //..
    }
    catch (Exception e)
    {
        if(renderGraph.ResetGraphAndLogException(e)) throw;
    }
    ```
- `void` **Cleanup**`()`: >`renderGraph`が内部で使用している**全てのリソースを解放**します。

## リソース準備

- **Create系** (`renderGraph`**内部**でリソースが**作成**され、**生存期間**を`renderGraph`が**管理**する)[ImportTexture](images\ImportTexture.png)
  - TextureHandle
    - `TextureHandle` **CreateTexture**`(⟪ TextureDesc desc¦ TextureHandle texture ＠❰, string name, bool clear = false❱⟫)`
    - `void CreateTextureIfInvalid(TextureDesc desc, ref TextureHandle texture)`:
      `texture`が無効(`.IsValid()`)の時、新たに`texture`に`desc`を素に`TextureHandle`を作成する
  - BufferHandle: `BufferHandle` **CreateBuffer**`(⟪BufferDesc desc¦BufferHandle graphicsBuffer⟫)`
  - RendererListHandle
    - `RendererListHandle` **CreateRendererList**`(RendererList⟪Params¦Desc⟫ desc)`
      - `RendererListHandle Create`Shadow``    RendererList(ShadowDrawingSettings shadowDrawingSettings)`
      - `RendererListHandle Create`Skybox``    RendererList(Camera camera ＠❰＠❰, Matrix4x4 projMatrixL, Matrix4x4 viewMatrixL❱, Matrix4x4 projMatrixR, Matrix4x4 viewMatrixR❱)`
      - `RendererListHandle Create`Gizmo``     RendererList(Camera camera, GizmoSubset gizmoSubset)`
      - `RendererListHandle Create`UIOverlay`` RendererList(Camera camera, UISubset uiSubset)`
      - `RendererListHandle Create`WireOverlay`RendererList(Camera camera)`
- **Import系** (`renderGraph`**外部**からリソースを**取り込み**、フレームを**跨ぐ**リソースを扱える。インポートマークが付く) (恐らく基本的に、`Import系`への**出力**は**パスカリングされない**)
  - TextureHandle
    - `TextureHandle` **ImportBackbuffer**`(RenderTargetIdentifier rt ＠❰, RenderTargetInfo info, ImportResourceParams importParams = default❱)`
      :この**Backbuffer**に**繋がらないPass**は基本的に**パスカリング**される。あと、*Render Graph Viewer*に表示される**名前**が`Backbuffer`となる (追記:`ImportTexture`とは違って`m_CurrentBackbuffer`に代入している。が、何も使われていない..)
      - `struct RenderTargetInfo info`: ＠❰`RTHandle rt`の中身が❱`RTI`だと不透明なため、`RenderGraph`に必要な**最小限の情報セット**を提供する。(多分NRP関係のため)
        - `GraphicsFormat format`
        - `int msaaSamples`,`bool bindMS`
        - `○⟦, ┃int ⟪width¦height⟫⟧`
        - `int volumeDepth`
      - `struct ImportResourceParams importParams`: >`Import`されたテクスチャの動作を記述するヘルパー構造体。[textureUVOrigin以外は`TextureDesc`を設定する](images\ImportParams.png)
        - `bool clearOnFirstUse`,`Color clearColor`: `clearOnFirstUse=true`の時、`Recording`で初めて使用されるとき`Import`された**テクスチャをクリア**(`clearColor`)します。
          (`RasterRenderPass`の**NRP**の場合は、`D_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR`でクリアされる。**それ以外**の場合は`ClearRenderTargetView(..)`でクリアされる)
        - `bool discardOnLastUse`: `true`:`Recording`の**最後に使用**時に**破棄**(`StoreAction.DontCare`)する。*MSAA*の場合はMSAAテクスチャのみが破棄される。
        - `enum TextureUVOrigin textureUVOrigin`: >`Import`されたテクスチャで使用される**UV方向**(アクティブなグラフィックAPIとは独立)。(`RasterCtx.GetTextureUVOrigin(data.texture)`に伝搬するみたい)
          - `BottomLeft`: OpenGL
          - `TopLeft`: Vulkan, DirectX, Metal,...
    - `TextureHandle` **ImportTexture**   `(RTHandle rt           ＠❰｡＠❰, RenderTargetInfo info❱, ImportResourceParams importParams = default｡❱)` (追記:恐らく基本的に`ImportBackbuffer`と同じ挙動)
  - BufferHandle: `BufferHandle` **ImportBuffer**`(GraphicsBuffer graphicsBuffer)`
  - その他
    - `RayTracingAccelerationStructureHandle ImportRayTracingAccelerationStructure(RayTracingAccelerationStructure accelStruct, string name = null)`
    - `TextureHandle ImportShadingRateImageTexture(RTHandle rt)`

## Builder

⟪`IRaster`¦`IUnsafe`⟫->`IRenderAttachment`->`IBase`, `ICompute`->`IBase`

- **Passの入出力** (`PassData`は**初期化されず**使い回される)
  - `enum`**AccessFlags**: [AccessFlags](images\AccessFlags.png)。`ResourceHandle`に`AccessFlags`で使用方法を指定し`renderGraph`に**パスカリング**や**生存期間**などの**管理のヒント**を与える。
    - メモ:`Recording`内で有効な`Pass`の`ResourceHandle`に最低1つの`AccessFlags`が無いと、その`Recording`区間が*Render Graph Viewer*に表示されない。
      **AccessFlags**によって`⟪Load¦Store⟫Action`と**パスカリング**に**影響を与える**ことを確認。絶対正しくない`AccessFlags`を設定しても大体エラーにはならず*Render Graph Viewer*に反映される(**リソース管理用途**であり`cmd.～`の動作には関与しない)
    - `None`: >このパスはリソースに一切アクセスしません。
    - `Read`: `renderGraph`に`リソース`を`Read`することを伝える。(恐らく最後の`Read`の`10フレーム`後に破棄)
      (RTリソースが`.Read`**のみ**で書き込まないと**パスカリング**されることを確認)
    - `Write`: `renderGraph`に`リソース`を`Write`することを伝える。(恐らく最初の`Write`で確保)
    - `Discard`: `renderGraph`に**未初期化**(`.DontCare`)な`リソース`に`アクセス`することを伝える。(恐らく`Discard`のみの場合はダミー用)
    - `WriteAll`=`Write|Discard`: `renderGraph`に**未初期化**な`リソース`を`Write`することを伝える。(つまり**全データ**を`Write`する用)
      (このフラグ以前の`Pass`に書き込まれて**一度も読まれ無い**時、その`Pass`がカリングされることを確認した。(その`Pass`を`.DontCare`にさせることも確認))
    - `ReadWrite`=`Read|Write`: >`Read|Write`のショートカットです。
  - **Use系** (`TextureHandle`を引数にとる`cmd.～`で、`BaseCB.ValidateTextureHandle(rt)`を実行して`TextureHandle`が`Read`|`Write`|`Transient`であることを**要請**する)
    - `IBase`
      - TextureHandle:        `void` **UseTexture**     `(TextureHandle input, AccessFlags flags = AccessFlags.Read)`: `UnsafePass`の`cmd.SetRenderTarget(..)`では`.UseTexture(., AccessFlags.Write＠❰All❱)`を使う
      - BufferHandle: `BufferHandle` **UseBuffer**      `(BufferHandle  input, AccessFlags flags = AccessFlags.Read)`
        >戻り値:'input'に渡された値。返された値は将来削除されるため、使用しないでください。?
      - RendererListHandle:   `void` **UseRendererList**`(RendererListHandle input)`: 読み取り専用(`IRenderGraphResource`では**無い**) : 忘れると**エラーにならず**描画されない(`RendererList.nullRendererList`かな?)
    - `IRenderAttachment`, `cmd.SetRandomWriteTarget(..)`と対応してるぽい。戻り値は謎
      - `TextureHandle SetRandomAccessAttachment(TextureHandle tex, int index,                                 AccessFlags flags = AccessFlags.ReadWrite)`
        - **NRP**とは**無関係** (*RenderDoc*で`BeginRenderPass(..)`などには現れなかった)
        - `builder.AllowGlobalStateModification(true)`は**不要**
        - `enableRandomWrite = true`にする必要がある。[UAV対応テクスチャ(`SRV`,`RTV` + **UAV候補**)](images\uav.png)
      - `BufferHandle  UseBufferRandomAccess    (BufferHandle tex,  int index ＠❰, bool preserveCounterValue ❱, AccessFlags flags = AccessFlags.Read)`
  - **Attachment系** (**NativeRenderPass**を参照、`SetRenderTarget(..)`相当)
    - `IRenderAttachment` (`cmd.ClearRenderTarget`,`cmd.Draw～系`で`BaseCB.ThrowIfRasterNotAllowed()`が実行され、`.SetRenderAttachment＠❰Depth❱`されることを**要請**する)
      - `void` **SetRenderAttachment**     `(TextureHandle tex, int index, AccessFlags flags = AccessFlags.Write     ＠❰, int mipLevel, int depthSlice ❱)`:
        >ブレンディングなど読み取る場合は`AccessFlags.ReadWrite`にする必要がある。フルスクリーンパスなどで完全に上書きする場合`AccessFlags.WriteAll`にするとパフォーマンスが良くなる
        - `int index`: MRTスロット(`SV_Target##index`), `int depthSlice`: >`-1`は全てのスライスをバインド(Layered Rendering?)
      - `void` **SetRenderAttachment**Depth`(TextureHandle tex,            AccessFlags flags = AccessFlags.ReadWrite ＠❰, int mipLevel, int depthSlice ❱)`
        >`Write`:`ZWrite On`, `Read`:`Ztest`が`Disabled`,`Never`,`Always`以外のとき。デプスバッファにMipMapは作れないはずだが?..
    - `IRaster`
      - `void` **Set**Input**Attachment**  `(TextureHandle tex, int index, AccessFlags flags = AccessFlags.Read      ＠❰, int mipLevel, int depthSlice ❱)`
        - `int index`: `FRAMEBUFFER_INPUT_X_FLOAT(index)`,`LOAD_FRAMEBUFFER_INPUT_X(index)`
  - CreateTransient系:`IBase`: この**Pass内でのみ使用可能**(％`.ReadWrite`(`.Use⟪Texture¦Buffer⟫(..)`の宣言省略可能))
    (本質的に↑これだけの機能のようで、`RasterRenderPass`の＠❰*NRP*の❱**中間テクスチャバッファ**としては**使えず**(`Set～Attachment～(..)`できるが**意味ない**)、主に`UAV`か`UnsafePass`(`SetRT(..)`)の用途しかないみたい..)
    - TextureHandle: `TextureHandle CreateTransientTexture(⟪TextureDesc desc¦TextureHandle texture⟫)`
    - BufferHandle:   `BufferHandle CreateTransientBuffer (⟪BufferDesc  desc¦BufferHandle  buffer⟫)`
  - **Global＠❰Texture❱系**:`IBase`
    - `void` **AllowGlobalStateModification**`(bool value)`: `true`:(`UnsafePass`**以外**の)**cmd.⟪Gloabl¦Local⟫Keyword**,**cmd.GlobalProperty**を`cmd.～(..)`で**設定**するときに**必要**(`BaseCB.ThrowIfGlobalStateNotAllowed()`で検証)
      そして`AllowPassCulling(false)`される。(`MPB`か`material`の`⟪Gloabl¦Local⟫⟪Keyword¦Property⟫`経由なら問題ない。というよりRenderGraphシステムとは無関係) (`.SetGlobalTextureAfterPass(..)`,`Set～Attachment～(..)`系では**必要ではない**)
      (ココで**グローバルな状態を変更**し、それ以降のどの`Pass`で**影響を受けるか把握できない**ため**パスカリングできない**ということだと思う。(>このパス以降のパスは、このパスより前に**リオーダーしない**))
    - `void` **SetGlobalTextureAfterPass**`(in TextureHandle input, int propertyId)`: 現在の`Pass`の**直後**に`cmd.SetGlobalTexture(propertyId, input)`し、*Render Graph Viewer*に**地球儀のマーク**を表示する
    - `void` **UseGlobalTexture**`(int propertyId, AccessFlags flags = AccessFlags.Read)`: `SetGlobalTextureAfterPass(..)`でセットした`propertyId`を**使用**(`Use`)する。(`SetGlobalTextureAfterPass(..)`と**必ず対**である必要がある)
    - `void UseAllGlobalTextures(bool enable)`: `SetGlobalTextureAfterPass(..)`された**全て**の`propertyId`を**使用**する(`AccessFlags.Read`) (`UseGlobalTexture`の全て使用する版) [UseAllGlobalTextures](images\UseAllGlobalTextures.png)
- **Passの実行**
  - SetRenderFunc:`IRaster`,`ICompute`,`IUnsafe`
    - `void` **SetRenderFunc**`<PassData>(BaseRenderFunc<`**PassData**`,`**⟪Raster¦Compute¦Unsafe⟫GraphContext**`> renderFunc) where PassData : class, new()`: 必ず1つの`Pass`に**1つのみ必要**。無いとエラー
      - `PassData` **data**: 任意型(継承必要なし)
      - `⟪struct Raster¦class ⟪Compute¦Unsafe⟫⟫GraphContext ～Ctx`
        - `⟪Raster¦Compute¦Unsafe⟫CommandBuffer` **cmd**
        - `RenderGraphObjectPool` *renderGraphPool* `{get;}`: アロケート回避用プール
          - `T[] GetTempArray<T>(int size)`: 任意型の配列を取得
          - `MaterialPropertyBlock GetTempMaterialPropertyBlock()`: 初期化済みの*MPB*を返す
        - `RenderGraphDefaultResources defaultResources {get;}`: デフォルトリソース
          - `TextureHandle ⟪black¦clear¦magenta¦white¦defaultShadow⟫＠❰UInt❱Texture＠❰＠⟪3D¦Array⟫XR❱`
        - `TextureUVOrigin GetTextureUVOrigin(TextureHandle textureHandle)`: >RenderGraphテクスチャの*TextureUVOrigin*を`textureHandle`から取得します。
  - IBase
    - `void` **AllowPassCulling**`(bool value)`: `false`:**パスカリング**を**無効**にする。(`AccessFlags`による依存関係によりこれに繋がる前の`Pass`も実行される)
    - `void` EnableAsyncCompute`(bool value)`: `true`:`⟪Compute¦Unsafe⟫Pass`で呼ぶと**非同期**Computeになる(`RasterRenderPass`で呼ぶとUnity**落ちる**)。[EnableAsyncCompute](images\EnableAsyncCompute.png)
      (非同期で`renderGraph`に渡した`cmd`とは**別**の`cmd`になっていて`ComputePass`側の`cmd`は`renderGraph`内部で`ctx.ExecuteCommandBuffer(cmd)`されている[](images\EnableAsyncCompute1.png))
      **同期ポイント**は、`AccessFlags`を見て作られると思われる。
    - `void EnableFoveatedRasterization(bool value)`: >`true`:このパスの**中心窩レンダリング**を有効にします。
- その他
  - `IBase`: `void GenerateDebugData(bool value)`: `false`:*Render Graph Viewer*でこれを実行した`Pass`の列が消える
  - `IRaster`
    - ShadingRate (`cmd`に対応する項目がある)
      - `void SetShadingRateCombiner(ShadingRateCombinerStage stage, ShadingRateCombiner combiner)`: >シェーディングレートコンバイナーを設定します。(各*ShadingRate*の比較)
      - `void SetShadingRateFragmentSize(ShadingRateFragmentSize shadingRateFragmentSize)`: >シェーディングレートのフラグメントサイズを設定します。(画面全体)
      - `void SetShadingRateImageAttachment(in TextureHandle tex)`: >現在のラスタライズパスで Variable Rate Shading（VRS）を有効化します。(テクスチャで指定)
    - `void SetExtendedFeatureFlags(ExtendedFeatureFlags extendedFeatureFlags)`: **＠❰Meta❱ XR**関係の**最適化**設定。(>プラットフォーム固有の最適化)
      - `enum ExtendedFeatureFlags extendedFeatureFlags`
        - `MultisampledShaderResolve`: >**Meta XR**上で、このフラグを設定すると、レンダーパスの最後のサブパスでMSAA のシェーダーリゾルブを使用できます。
        - `MultiviewRenderRegionsCompatible`: >**XR**で、Multiview Render Regions に対応したパスに設定できるフラグです。
        - `None`: >拡張機能が何も有効になっていないデフォルト状態。
        - `TileProperties`: >**Meta XR**上で、最も多くの 3D レンダリングを行うパスに設定することで、より高いパフォーマンスを得られる可能性があります。

## リソース

- **TextureHandle**
  - `struct` **TextureHandle**: `renderGraph`によって**有効期間や使用方法など**を管理され、`renderGraph`外でアクセスしてはいけない。[概要](images\TextureHandle概要.png)
    - `static TextureHandle nullHandle {get;}`
    - `bool IsValid()`: `TextureHandle.nullHandle`の時`false`になるぽい。なので、そうでない時、**実際のリソースを保持**しているかは**分からない**。
      (`if(data.buffer.IsValid()) {RasterCtx.cmd.SetGlobalBuffer("buffer", data.buffer);}`のように**使えない**(自前のフラグで管理する(`multiplyByIndexCSPassEnabled`など)))
    - `TextureDesc GetDescriptor(RenderGraph renderGraph)`: `renderGraph.GetTextureDesc(TextureHandle texture)`を呼ぶだけ。(本体は`renderGraph`にあるらしい)
    - `static implicit operator ⟪RTI¦RenderTexture¦RTHandle¦Texture⟫(TextureHandle texture)`: `SetRenderFunc(..)`内で**各型に変換**される
  - `struct` **TextureDesc**: `TextureHandle`を作成するために使用される説明
    - 取得:`TextureDesc` Get**TextureDesc**`(TextureHandle texture)`
      - `RenderTargetInfo GetRenderTargetInfo(TextureHandle texture)`: `RTI`が不透明なため、`RenderGraph`に必要な**最小限の情報セット**を提供する。を取得する
    - コンストラクタ
      - `TextureDesc(⟪○⟦, ┃int ⟪width¦height⟫⟧¦Vector2 scale¦ScaleFunc func⟫, bool dynamicResolution = false, bool xrReady = false)`
      - `TextureDesc(⟪RenderTexture＠❰Descriptor❱¦TextureDesc⟫ input)`
    - フィールド
      - 名前:`string name`
      - RTDesc
        - ResourceDesc
          - `TextureDimension dimension`
          - `GraphicsFormat format`
          - `MSAASamples msaaSamples`,`bool bindTextureMS`
          - `enum TextureSizeMode sizeMode`: >テクスチャのサイズを決定するモード。
            - `Explicit`: `○⟦, ┃int ⟪width¦height⟫⟧`
            - `Functor`: `ScaleFunc func`
            - `Scale`: `Vector2 scale`
          - `bool useMipMap`,`bool autoGenerateMips`
          - `int slices`: 普通に`volumeDepth`と同じ
        - `bool isShadowMap`
        - `bool enableRandomWrite`
        - `bool useDynamicScale`,`bool useDynamicScaleExplicit`
        - `VRTextureUsage vrUsage`
        - `RenderTextureMemoryless memoryless`: DirectX12では無効[Memoryless](images\Memoryless.png)
      - TextureSampler
        - `FilterMode filterMode`
        - `int anisoLevel`
        - `TextureWrapMode wrapMode`
        - `float mipMapBias`
      - RTHandle
        - `bool enableShadingRate`: >テクスチャをシェーディング レート イメージとして使用する場合は true に設定します。(`○⟦, ┃int ⟪width¦height⟫⟧`はタイル単位になります)
      - TextureHandle
        - クリア:`bool clearBuffer`,`Color clearColor`: 初めて使用するときにテクスチャをクリアする必要があります。(`clearBuffer=false`でもクリアされる..(`Discard`でない時、未初期化を避けるためクリアかな?)(*Render Graph Viewer*は`clearBuffer`を表示))
        - `bool discardBuffer`: **Import時**に`Recording`の**最後に使用**時に**破棄**(`StoreAction.DontCare`)する。*MSAA*の場合はMSAAテクスチャのみが破棄される。
        - フォールバック
          - `bool fallBackToBlackTexture`: >テクスチャに書き込まずに読み取った場合に、テクスチャを黒のテクスチャにフォールバックするかどうかを決定します。
          - `bool disableFallBackToImportedTexture`: >テクスチャに書き込まれるすべてのパスがダイナミックレンダーパスカリングによってカリングされた場合、自動的に類似の事前割り当てテクスチャにフォールバックされます。割り当てを強制するには、これをtrueに設定します。
            - あるテクスチャ T を“生成するはず”だったけど、生成パスが全部不要判定で消えた。それでも後段が T を参照する（または外に出す）都合がある。そのとき RenderGraph が **「じゃあ同等っぽい既存（imported/preallocated）に差し替えて辻褄を合わせる」**ことがある
            - 本当にその挙動っぽい。(参照:`Assets\Samples\Universal Render Pipeline\17.4.0\URP RenderGraph Samples\UnsafePass\UnsafePassRenderFeature.cs`)
        - `FastMemoryDesc fastMemoryDesc`: >テクスチャがそれをサポートするプラットフォーム上の高速メモリにどのように配置されるかを決定する記述子。
- **BufferHandle**
  - `struct` **BufferHandle**:
    - `static BufferHandle nullHandle {get;}`
    - `bool IsValid()`
    - `static implicit operator GraphicsBuffer(BufferHandle buffer)`: `SetRenderFunc(..)`内で**変換**される
  - `struct` **BufferDesc**: `BufferHandle`を作成するために使用される説明
    - 取得:`BufferDesc` Get**BufferDesc**`(BufferHandle graphicsBuffer)`
    - コンストラクタ
      - `BufferDesc(int count, int stride ＠❰, GraphicsBuffer.Target target❱)`
    - フィールド
      - `string name`
      - `GraphicsBuffer.Target target`
      - `int count`
      - `int stride`
      - `GraphicsBuffer.UsageFlags usageFlags`
- **RendererListHandle**
  - `struct` **RendererListHandle**
    - `bool IsValid()`
    - `static implicit operator ⟪RendererList¦int⟫(RendererListHandle rendererList)`: `SetRenderFunc(..)`内で**各型に変換**される
  - `RendererList⟪Params¦Desc⟫`

## CommandBuffer

- **無いもの**: `Blit`、`ResolveAntiAliasedSurface`、NativeRenderPass系、Fence系、`CopyBuffer`、`⟪Get¦Release⟫TemporaryRT`、FastMemory
- **IBase**==================================================================================
  - **カリング**:`SetInvertCulling`、**ビューポート**:`SetViewport`、**シザー**:`⟪Enable¦Disable⟫ScissorRect`、シャドー`SetShadowSamplingMode`
  - *基本ShaderProperty_Set*:`SetGlobal`⟪`Texture(.,`**TextureHandle**`,.)`¦`⟪⟪Float¦Vector¦Matrix⟫＠❰Array❱¦Color¦Integer¦＠❰Constant❱Buffer⟫`⟫と`SetGlobalDepthBias`
    - VP_Matrix,クリッププレーン:`SetupCameraProperties`,`SetViewProjectionMatrices`
  - **ShaderKeyword系**:`SetKeyword`
  - その他
    - IssuePlugin系:`IssuePluginEvent＠❰AndData❱`,`IssuePluginCustomBlit`,`IssuePluginCustomTextureUpdateV2`
    - LateLatch系:`MarkLateLatchMatrixShaderPropertyID`,`UnmarkLateLatchMatrix`,`SetLateLatchProjectionMatrices`
    - CommandBuffer系:`⟪Begin¦End⟫Sample`
    - その他:`IncrementUpdateCount`,`InvokeOnRenderObjectCallbacks`,`SetSinglePassStereo`
- **IRaster** : `IBase`======================================================================
  - **DrawCall系**:`DrawRenderer＠❰List❱`,`DrawMesh＠❰Instanced＠❰⟪Indirect¦Procedural⟫❱❱`,`DrawProcedural＠❰Indirect❱`,`DrawOcclusionMesh`,`DrawMultipleMeshes`
    - **Clear系**:`ClearRenderTarget`
  - ワイヤーフレーム:`SetWireframe`
  - XR関係:`SetInstanceMultiplier`
    - FoveatedRendering系:`SetFoveatedRenderingMode`,`ConfigureFoveatedRendering`
      - シェーディングレート:`SetShadingRateCombiner`,`SetShadingRateFragmentSize`
- **ICompute** : `IBase`=====================================================================
  - **Dispatch＠❰Rays❱系**:`Dispatch⟪Compute¦Rays⟫`,`SetRayTracingShaderPass`
  - *基本ShaderProperty_Set*:`Set⟪Compute¦RayTracing⟫`⟪`TextureParam(..,`**TextureHandle**`,..)`¦`⟪⟪Float¦Int⟫Param＠❰s❱¦⟪｡⟪Vector¦Matrix⟫＠❰Array❱¦＠❰Constant❱Buffer｡⟫Param⟫`⟫
    - `SetComputeParamsFromMaterial`,`SetRayTracingAccelerationStructure`
  - ResourceModified系:`SetBufferData`,`⟪SetBuffer¦Copy⟫CounterValue`,`BuildRayTracingAccelerationStructure`
- **IUnsafe** : `IBase`, `IRaster`, `ICompute`===============================================
  `static CommandBuffer CommandBufferHelpers.GetNativeCommandBuffer(UnsafeCommandBuffer baseBuffer)`で生の`CommandBuffer`を取得可能(なんでもできる)
  - **RenderTarget系**:`SetRenderTarget`,`⟪Set¦Clear⟫RandomWriteTarget` (`SetRenderTarget`は、`RIT`ではなく**TextureHandle**の場合、恐らく`implicit`が挟む時に**検証**され、`builder.UseTexture(textureHandle, AccessFlags.Write)`の設定が必要な場合がある)
  - **Copy系**:`CopyTexture`
  - *基本ShaderProperty_Set*:`SetGlobalTexture(.,`**RTI**`,.)`,`Set⟪Compute¦RayTracing⟫TextureParam(..,`**RTI**`,..)` (継承しているので`TextureHandle`版もある(両方使ったok))
  - MipMap生成:`GenerateMips`
  - **AsyncReadback系**:`RequestAsyncReadback＠❰IntoNativeArray❱`
  - Clear:`Clear`

## その他

- `class RenderGraphDefaultResources defaultResources {get;}`: >Pass中にデフォルトのリソースにアクセスできるようにするヘルパークラス。
  - `TextureHandle ⟪black¦clear¦magenta¦white¦defaultShadow⟫＠❰UInt❱Texture＠❰＠⟪3D¦Array⟫XR❱`
- `void ＠❰Un❱RegisterDebug(DebugUI.Panel panel = null)`: >`RenderGraph`をデバッグウィンドウに⟪登録¦解除⟫します。
