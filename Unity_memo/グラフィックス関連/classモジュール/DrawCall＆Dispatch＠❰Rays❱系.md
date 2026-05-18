# DrawCall＆Dispatch＠❰Rays❱系

- DrawCall＆Dispatch＠❰Rays❱系
  - DrawCall系
    - `Draw`**RendererList**`(RendererList rendererList)`: `RendererList`に含まれる可視な`Rendererコンポーネント`を描画する
      - `struct RendererList`: (**オブジェクト**(`Rendererコンポーネント`)が**描画順にソート**され、オブジェクトに適用する`ライト`,`プローブ`を持っている?)。`RendererList`は一度だけ`cmd.DrawRendererList(..)`で使用可能であり、**再利用不可**。
        - `bool isValid`: `RendererList`が無効な場合は`false`を返す
        - `static RendererList nullRendererList()`: 空の`RendererList`を返す
      - `RendererList ctx.`**CreateRendererList**`(⟪ RendererListDesc desc¦ref RendererListParams param⟫)`:
        - `struct RendererListDesc`: `RendererListParams`の**簡易版**。(`static RendererListDesc.ConvertToParameters(desc)`で`RendererListParams`に**変換**できる)
        - `struct` **RendererListParams**:
          - `static RendererListParams Invalid`: 空の`RendererListParams`を返す
          - `.ctor(CullingResults cullingResults, DrawingSettings drawSettings, FilteringSettings filteringSettings)`
            :*Culling*{camera.cullingMask,Occlu⟪der¦dee⟫,CPUの**AABBフラスタムカリング**,`オブジェクト`(index)との`ライト`/`プローブ`(value)衝突 の配列} => *Filtering* => *Drawing*
            - **CullingResults** `cullingResults`: `struct CullingResults`: 描画対象となる**可視{オブジェクト,ライト,プローブ}セット** (`buffer={obj{Light{..},Probe{..}}..}`)
              - メモ: `ctx.Cull(ref ScriptableCullingParameters『カメラ』)`を設定。
                      `Rendererコンポーネント`(オブジェクト)はフラスタムカリングされているが、**公開されていない** (恐らく`CreateRendererList(..)`の内部で使われるのみ)
                      カメラ ⊃ {オブジェクト}, オブジェクト ⊃ {カメラ,ライト,プローブ}, ライト ⊃ {オブジェクト}
              - **ライト+プローブのIndexBuffer作成** <https://chatgpt.com/c/686fbda4-1458-8013-a2ce-ab6dda899dcd>
                - 実体
                  メモ:`Visible⟪Light¦ReflectionProbe⟫`は、**C#JobSystem**のために良く使うメンバを持った**struct**になっている。(`⟪Light¦ReflectionProbe⟫`そのものへの参照(class)もある)
                  - `NativeArray<VisibleLight> visibleLights`: フラスタムカリングされた`VisibleLight`の配列。(`Lightインデックス`で参照)
                  - `NativeArray<VisibleReflectionProbe> visibleReflectionProbes`: フラスタムカリングされた`VisibleReflectionProbe`の配列。(`Probeインデックス`で参照)
                - ストライド
                  - `int lightIndexCount`: `SetLightIndexMap(.)`で使う`Lightストライド`
                  - `int reflectionProbeIndexCount`: `SetReflectionProbeIndexMap(.)`で使う`Probeストライド`
                  - `int lightAndReflectionProbeIndexCount`: `FillLightAndReflectionProbeIndices(.)`で使う`LightAndProbeストライド`
                - 再マップ
                  メモ: **Forward+**では`objN`が`タイル毎`?と思ったが、**ComputeShader**で計算し設定され`CullingResults`とは関係ないらしい。が、**C#JobSystem**で計算してた気がする..
                  - `NativeArray<int> GetLightIndexMap(Allocator allocator)`: 以前に設定された`lightIndexMap`を取得
                    - `SetLightIndexMap(NativeArray<int> lightIndexMap)`: `オブジェクト毎`に使う`Lightインデックス`を`Lightストライド`で進めながら設定する
                      - 例: `lightIndexCount = 4; lightIndexMap = {obj0_Light{0,1,2,-1}, obj1_Light{1,2,4,5}, obj2_Light{3,1,-1,-1}};`(-1は無効の要素)
                  - `NativeArray<int> GetReflectionProbeIndexMap(Allocator allocator)`: 以前に設定された`probeIndexMap`を取得
                    - `SetReflectionProbeIndexMap(NativeArray<int> probeIndexMap)`: `オブジェクト毎`に使う`Probeインデックス`を`Probeストライド`で進めながら設定する
                - SetData (`GraphicsBuffer`)
                  - `FillLightAndReflectionProbeIndices(GraphicsBuffer buffer)`: ライト+プローブのIndexBuffer作成 (メモ: **Forward+**はこの`buffer`を`ComputeShader`で設定している?)
                    `オブジェクト毎`に使う`Lightインデックス`+`Probeインデックス`を`LightAndProbeストライド`で進めながら`buffer`に**SetData**する
                    - 例: `lightIndexCount = 4; reflectionProbeIndexCount = 2; lightAndReflectionProbeIndexCount = lightIndexCount + reflectionProbeIndexCount;`
                          `buffer = {obj0{Light{0,1,2,-1},Probe{2,1}}, obj1{Light{1,2,4,5},Probe{-1,-1}}, obj2{Light{3,1,-1,-1},Probe{2,4}}};`
              - `bool GetShadowCasterBounds(int lightIndex, out Bounds outBounds)`:
                :「その`Lightインデックス`(`lightIndex`)に照らされて影を落とすオブジェクト(*ShadowCaster*)たちを全て囲う**AABB**」を`outBounds`に返す
                  (*ShadowCaster*が一つもない時、`false`を返す)
              - `NativeArray<VisibleLight> visibleOffscreenVertexLights`: **画面外**(Offscreen)の**頂点単位ライト**を集めた`VisibleLight`
                (`visibleOffscreenVertexLights`⊂`visibleLights`)
              - `Compute`～`ShadowMatricesAndCullingPrimitives(..)`
                :`activeLightIndex`(`Lightインデックス`)のビュー行列(`viewMatrix`)と投影行列(`projMatrix`)と`ShadowSplitData`を計算する。(`false`の場合、このライトは影を描画しない)
                「`ShadowSplitData`は、ライト視点で影を描くときに“どこまでのオブジェクトを描くか”を決める**カリング範囲データ**だにゃ！(カスケードなので他のシャドウ用途でも使える)」
                - `bool Compute`Spot`ShadowMatricesAndCullingPrimitives(int activeLightIndex, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData)`
                - `bool Compute`Point`ShadowMatricesAndCullingPrimitives(int activeLightIndex, CubemapFace cubemapFace, float fovBias, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData)`
                - `bool Compute`Directional`ShadowMatricesAndCullingPrimitives(int activeLightIndex, int splitIndex, int splitCount, Vector3 splitRatio, int shadowResolution, float shadowNearPlaneOffset, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData)`
            - **FilteringSettings** `filteringSettings`: `struct FilteringSettings`: **可視Obj**のフィル方法 (主に`＠❰rendering❱LayerMask`,`⟪renderQueue¦sortingLayer⟫Range`) *ok*
              - `static FilteringSettings defaultValue`: フィルタリングをしない設定の値(全て含む) (`new FilteringSettings()`は**既定のstruct**による**0初期化の.ctor**が呼ばれるので**注意**)
              - `.ctor(Nullable<RenderQueueRange> renderQueueRange = RenderQueueRange.all, int layerMask, uint renderingLayerMask, int excludeMotionVectorObjects 『⟪％❰0❱¦1⟫』)`:
              - `uint batchLayerMask`: **Unity側が構築したBRG**の**Batchレイヤー**の**ビットマスク**。BRGのBatchをスイッチして簡単に最適化できるが、Unity側が作ったBRGがよく分からない
              - `Tags{"LightMode" = "MotionVectors"}`がある`Material`での**描画有無**。(動いているかは、`UNITY_MATRIX_MV`が**前回のフレームと違うか**を確認する?)
                  (試したが、よく分からない。常に動いている判定されているような挙動だった) (Unityは`"MotionVectors"`を**特別扱い**してコレが入っていると他の`LightMode`の**描画**が**されなくなる**)
                - `bool excludeMotionVectorObjects`: `true`: **動いている**オブジェクトを**除外** //これは、描画されなくなることを確認した。*ok*。しかし、止まっている時も除去される..
                - `bool forceAllMotionVectorObjects`: `true`: **止まっている**オブジェクトを**強制描画** //こっちは、通常から止まっていても描画されていて、違いが観測されなかった。
              - `int layerMask`: **gameObject.layer**をフィルタリングするビットマスク(`Camera.cullingMask`=>`camera.TryGetCullingParameters(..)～`からさらにフィルタリングしている)
                - **struct LayerMask**: 32bit(int)のビットフィールド: **staticメソッド**: `int GetMask(params string[] layerNames)`, `⟪⟦Name⟧To⟦Layer⟧⟫`,`int`=>`LayerMask`へのimplicitあり。**プロパティ**: value
              - `uint renderingLayerMask`: **renderer.renderingLayerMask**をフィルタリングするビットマスク
                - **struct RenderingLayerMask**: 大体`LayerMask`と同じ。しかしこちらは`renderer`に**複数bitを立てる**事ができる。
              - `RenderQueueRange renderQueueRange`: `struct RenderQueueRange`: **material.renderQueue**の範囲をフィルタリングする
                - `○¦＠❰.ctor(❱int lowerBound, int upperBound○¦＠❰)❱, ○⟦, ┃static ⟪RenderQueueRange ⟪all¦opaque¦transparent⟫¦int ⟪minimumBound¦maximumBound⟫⟫⟧`
              - `SortingLayerRange sortingLayerRange`: `struct SortingLayerRange`: **sortingLayer.value**の範囲をフィルタリングする (`PS/Tags and Layers/Sorting Layers` 参照)
                - `○¦＠❰.ctor(❱short lowerBound, short upperBound○¦＠❰)❱`, `static SortingLayerRange all`
            - **DrawingSettings** `drawSettings`: `struct DrawingSettings`: オブジェクトの**描画方法**(`LightMode`)と**描画順序**(`SortingSettings`)を指定 *大体ok*
              - `static int maxShaderPasses`: >1 回の DrawRenderers 呼び出しで描画できるパスの最大数
              - `.ctor(`**ShaderTagId shaderPassName**`,`**SortingSettings** `sortingSettings)`:
              - **SortingSettings** `sortingSettings`: `struct SortingSettings`: オブジェクトの**描画順**を設定（必須）
                - `.ctor(Camera camera)`: `camera`は*⟪⟦Front⟧To⟦Back⟧⟫距離の計算方法*を設定する (`criteria`は設定**しない**)
                - **SortingCriteria** `criteria`: `enum SortingCriteria`: 組み合わせ可能。**優先度は上から適用**される *ok*
                  - `None`:
                  - `SortingLayer`: `renderer.sortingLayer⟪Name¦ID⟫`順 [`static SortingLayer[] SortingLayer.layers`](https://docs.unity3d.com/ja/2023.2/ScriptReference/SortingLayer.html)
                  - `CanvasOrder`: `renderer.sortingOrder`順 (Order in Layer) ⟪-N～+N⟫(％0)
                  - `RenderQueue`: `material.renderQueue`順  ⟪0～5000⟫(`Background`(1000),`％Geometry`(2000),`AlphaTest`(2450),`Transparent`(3000),`Overlay`(4000))
                  - `RendererPriority`: `renderer.rendererPriority`順 ⟪-N～+N⟫(％0)
                  - 距離(`⟪⟦Front⟧To⟦Back⟧⟫`) (これより下は**同一距離**) (**Z Pre Pass**を書いていれば、インスタンシングで効率描画できる？)
                    - `BackToFront`: `far`から`near`順
                    - `QuantizedFrontToBack`: `near`から`far`順 (`Quantized`は**粗い距離でソート**しCPU負荷を削減 (`2unit`は粗いことを確認した))
                  - `OptimizeStateChanges`: **同一シェーダー** (シェーダーを切り替えない)
                  - 組み合わせ
                    - `CommonOpaque`: (`SortingLayer`|`RenderQueue`|`QuantizedFrontToBack`|`OptimizeStateChanges`|`CanvasOrder`)
                    - `CommonTransparent`: (`SortingLayer`|`RenderQueue`|`BackToFront`|`OptimizeStateChanges`) (半透明の**破綻回避**のために`CanvasOrder`を外してあるらしい)
                - **⟪⟦Front⟧To⟦Back⟧⟫距離の計算方法** *ok*
                  :`.ctor(camera)`のときの各メンバの設定のされかた
                  - `DistanceMetric distanceMetric`: ｢`camera.transparencySortMode`から｣設定される (`class_Camera.md/transparencySortMode`参照)
                    - `enum DistanceMetric`:
                      `pos = gameObject.transform.position` 以下、それぞれの**距離**の計算方法
                      - `Perspective`: `Vector3.Distance(pos, cameraPosition)`『単純に`camera`との距離
                      - `Orthographic`: `Vector3.Dot(worldToCameraMatrix.MultiplyPoint(pos), Vector3.forward)`『**デプスバッファ的**距離
                      - `CustomAxis`: `Vector3.Dot(pos, customAxis)`『ワールド空間上の特定方向(`customAxis`)の距離
                  - `Matrix4x4 worldToCameraMatrix`: `camera.worldToCameraMatrix`(⇔Transformと同期) が設定される
                  - `Vector3 cameraPosition`: `camera.transform.position` が設定される
                  - `Vector3 customAxis`: `camera.transparencySortAxis` が設定される
              - **パス指定**
                - Tags{"LightMode"}の描画の指定。(`ShaderTagId[] shaderPassNames`を設定しているだけ) (`.ctor(shaderPassName,..)`⇔`shaderPassNames[0] = shaderPassName`)
                  :描画順は`SortingSettings`で決めているので`index`は**描画順に関係ない**。`LightMode`が`index`間で重複している場合は**重複して描画**される(意味は無さそう)。
                  `.ctor(ShaderTagId.none,.)`にして後からこれで設定することも可能。 (`new ShaderTagId("SRPDefaultUnlit")`は`Tags{"LightMode"}`が**無い**シェーダーにマッチする値)
                  - `ShaderTagId GetShaderPassName(int index)`: `shaderPassNames[index]`
                  - `SetShaderPassName(int index, ShaderTagId shaderPassName)`: `shaderPassNames[index] = shaderPassName`
                - ⟪Material¦Shader⟫関係
                  - `Material fallbackMaterial`: `ShaderTagId[] shaderPassNames`にマッチしない時に使用する`Material` (主にデバッグ用)
                    :試したけど、**フォールバックされなかった**。(`.SetShaderPassName(3, new ShaderTagId("RendererListFallbackShaderTag"))`も設定したけど効果なし)
                  - オーバーライド⟪Material¦Shader⟫
                    - メモ
                      🎭 `overrideMaterial` vs. `overrideShader`
                      ・`overrideMaterial` は、すべての`Material`を**完全に置き換え**ます。(`MaterialProperty`と`LocalKeyword`も効く)
                      ・`overrideShader` は、**現在のMaterialを保持**しつつシェーダーだけを変更します。
                      ※ SRP Batcher や BRG では `overrideShader` はサポートされておらず、
                      パフォーマンスにも影響があるため、できれば `overrideMaterial` の方を使ってください。
                      両者を同時に使うことはできません！
                    - `Material overrideMaterial`: オーバーライドする`Material`を設定 (完全に置き換え) *ok*
                      :試したけ所、`SetShaderPassName(..)`の**重複**は、重複して描画**されなくなる**。`SortingSettings`で順序が特に決まらない(同列)時、順序が適当に変わることがある。
                      - `int overrideMaterialPassIndex`: `ShaderPassIndex`を指定 (こっちは試してない)
                    - `Shader overrideShader`: オーバーライドする`Shader`を設定 (現在のMaterialを保持, SRPBatcher/BRGサポート外)
                      - `int overrideShaderPassIndex`: `ShaderPassIndex`を指定
              - 機能スイッチ
                - `bool enableInstancing`,`％❰true❱`: GPUインスタンシングの有効化 (`Material.enableInstancing`)
                  :試したけど、**全く違いが出なかった**。(`⟪drawingSettings¦material⟫.enableInstancing = true`、同一`Mesh`、同一`シェーダーバリアント`、同列`SortingCriteria` で、効くはず..)
                  (`drawingSettings.enableInstancing`と`material.enableInstancing`の`2^2パターン`を試したけど変化なし。`multi_compile_instancing`もやった。あと色々。)
                - `bool enableDynamicBatching`,`％❰false❱`: 動的バッチングの有効化
              - その他設定
                - `int lodCrossFadeStencilMask`: Unity2023.1以降(特にHDRP)の**ステンシルバッファ**を使ったLODクロスフェード用32bitステンシルマスクらしい (0で無効)
                - `int mainLightIndex`: >メインライトのオブジェクト毎の**ライトカリング**を実行するために使用。(`VisibleLight[] CullingResults.visibleLights`のインデックスを指定(％自動選択))
                - `PerObjectData perObjectData`: オブジェクト毎の`ShaderProperty`(`UnityPerDraw`,テクスチャ変数,など)をフレーム毎に設定するが、**GPU_RD**ではデータをGPUに常駐させるため**無効**
          - 各`ShaderTagId`毎の`∮RenderingState∮`を**オーバーライド**
            :`stateBlocks`と`tagValues`の**要素数は一致**している必要がある (`tagValues`の`∮RenderingState∮`を`stateBlocks`で**オーバーライド**する)
            - `Nullable<NativeArray<`**RenderStateBlock**`>> stateBlocks`: *C#*から`mask`の`∮RenderingState∮`で**オーバーライド**する。
            - **オーバーライド**する`ShaderTagId`を**指定**する。`tagName`の`tagValues`にマッチした`Shader`の`∮RenderingState∮`を**オーバーライド**する
              - `bool isPassTagName`: `isPassTagName`が `true`:`PassTag`,`％false`:`SubShaderTag`
              - `ShaderTagId tagName`: `tagName`は`Tags{"ココ"="tagValues"}`
              - `Nullable<NativeArray<ShaderTagId>> tagValues`: `tagValues`は`Tags{"tagName"="ココの候補"}` (追記:`ShaderTagId.none`であれば無条件にマッチするらしい(Codex)(RenderingUtils.cs:233))
      - `RendererList ctx.`**Create**Shadow**RendererList**`(ref ShadowDrawingSettings settings)`:
        - シャドウライト(`lightIndex`)＠❰と(`splitIndex`)❱を指定してシャドウマップを描画する。(後は`RendererListParams`より**簡易**)
      - `RendererList ctx.`**Create**Skybox**RendererList**`(Camera camera)`: `camera`と`Material RenderSettings.skybox`を使って**スカイボックス**を描画する
    - `Draw`**Renderer**`(Renderer renderer, Material material, int submeshIndex = 0, int shaderPass = -1)`:
      多分、`renderer`を**Mesh**(`submeshIndex`)と**UnityPerDraw**としてしか使っていない
    - **Draw⟪Mesh¦Procedural⟫**
      - `Draw`**Mesh**`＠❰Instanced＠❰⟪Indirect¦Procedural⟫❱❱`『DrawMesh, DrawMeshInstancedProcedural *ok*
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
            `＠❰int indexCount, ＠❰int instanceCount ❱❱,`『**Index数**と**Instance数**
          『❰Indirect❱
            `＠❰GraphicsBuffer bufferWithArgs, ＠❰int argsOffset❱❱`『**引数バッファ**。`❰int indexCount, int instanceCount ❱`を詰める
        `)`
      - 引数説明
        - `＠❰Matrix4x4＠❰[]❱ matrix❱`: 多分`UNITY_MATRIX_M`を設定している
        - `int shaderPass`: デフォルト`-1`で**全てのパスを描画**する (URPでもcmdを直接操作しているので描画される)
        - ❰Instanced❱: **インスタンシング**は`Material.enableInstancing`が`true`であること (インスペクターでも設定できる) (追記:`.enableInstancing = false`でも普通に動く)
        - `＠❰MaterialPropertyBlock properties❱`: 4oはURPでも、**テクスチャや配列でなければSRP Batcherでも使える**と言っている
        - `GraphicsBuffer bufferWithArgs`: `argsOffset`を使って`D_DRAW_INDEXED_ARGUMENTS`を1個の選択できる。`DrawMeshInstancedIndirect(..)`時、`int submeshIndex`との競合は、
          :4o「メッシュバインド用にどのサブメッシュ(`int submeshIndex`)を選ぶかだけ教えてね。あとはバッファ(`bufferWithArgs`)の指示に従うから！」らしい
        - `＠❰GraphicsBuffer indexBuffer❱`: `SV_VertexID`!=`indexBuffer`であり、`SV_VertexID`は単純に**頂点処理の連番**である
          :(`＠❰StructuredBuffer<uint>❱ _indexBuffer[SV_VertexID]`で参照できる)
    - `DrawOcclusionMesh(RectInt normalizedCamViewport)`:
      :ビューポートの範囲(`normalizedCamViewport`)に**VRデバイスが提供するOcclusion Mesh**(見えない部分のメッシュ)を**深度バッファ**に**Nearクリップ面で描画**する
    - `Blit`*ok*: **URP非推奨**。`DrawMesh(..)`などを使って**低レベル操作で実現**する。テクスチャを別のレンダーテクスチャにシェーダーを使ってコピー
      `cmd.Blit(rt, BRTT.CameraTarget)`は動いた。(`cmd.ConvertTexture(..)`が使えないときは有用かも)
  - Dispatch系
    - `DispatchCompute(ComputeShader computeShader, int kernelIndex, ⟪｡○⟦, ┃int threadGroups⟪X¦Y¦Z⟫⟧｡¦｡GraphicsBuffer indirectBuffer, uint argsOffset｡⟫)`
      :Computeシェーダーを実行する
      - `ComputeShader computeShader`, `int kernelIndex`: **カーネル**指定 (`computeShader.FindKernel(string name)`で取得)
      - `○⟦, ┃int threadGroups⟪X¦Y¦Z⟫⟧`: **スレッドグループ数**指定
        - `GraphicsBuffer indirectBuffer`, `uint argsOffset`: 間接描画も可能
  - DispatchRays系
    - `SetRayTracingShaderPass`: レイ/ジオメトリ交差シェーダーに使うパスを指定
    - `DispatchRays`: RayTracingShaderを実行

`new RendererListParams(cullingResults, drawingSettings, filteringSettings)`
```Csharp
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
```
