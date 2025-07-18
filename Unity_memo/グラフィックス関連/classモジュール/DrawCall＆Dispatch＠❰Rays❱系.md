# DrawCall＆Dispatch＠❰Rays❱系

- DrawCall＆Dispatch＠❰Rays❱系
  - DrawCall系
    - `Draw`**RendererList**`(RendererList rendererList)`: `RendererList`に含まれる可視な`Rendererコンポーネント`を描画する
      - `struct RendererList`:
        - `bool isValid`: `RendererList`が無効な場合は`false`を返す
        - `static RendererList nullRendererList()`: 空の`RendererList`を返す
      - `RendererList ctx.`**CreateRendererList**`(⟪ RendererListDesc desc¦ref RendererListParams param⟫)`:
        - `struct RendererListDesc`: `RendererListParams`の**簡易版**。(`static RendererListDesc.ConvertToParameters(desc)`で`RendererListParams`に**変換**できる)
        - `struct` **RendererListParams**:
          - `static RendererListParams Invalid`: 空の`RendererListParams`を返す
          - `.ctor(CullingResults cullingResults, DrawingSettings drawSettings, FilteringSettings filteringSettings)`
            :Culling{cullingMask,Occlu⟪der¦dee⟫,CPUの**AABBフラスタムカリング**} => Filtering => Drawing
          - **CullingResults** `cullingResults`: `struct CullingResults`: 描画対象となる**可視{オブジェクト,ライト,プローブ}セット**
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
                メモ: **Forward+**では`タイル毎`?と思ったが、**ComputeShader**で計算し設定され`CullingResults`とは関係ないらしい。が、**C#JobSystem**で計算してた気がする..
                - `NativeArray<int> GetLightIndexMap(Allocator allocator)`: 以前に設定された`lightIndexMap`を取得
                  - `SetLightIndexMap(NativeArray<int> lightIndexMap)`: `オブジェクト毎`に使う`Lightインデックス`を`Lightストライド`で進めながら設定する
                    - 例: `lightIndexCount = 4; lightIndexMap = {obj0_Light{0,1,2,-1}, obj1_Light{1,2,4,5}, obj2_Light{3,1,-1,-1}};`(-1は無効の要素)
                - `NativeArray<int> GetReflectionProbeIndexMap(Allocator allocator)`: 以前に設定された`probeIndexMap`を取得
                  - `SetReflectionProbeIndexMap(NativeArray<int> probeIndexMap)`: `オブジェクト毎`に使う`Probeインデックス`を`Probeストライド`で進めながら設定する
              - SetData (`GraphicsBuffer`)
                - `FillLightAndReflectionProbeIndices(GraphicsBuffer buffer)`: ライト+プローブのIndexBuffer作成
                  `オブジェクト毎`に使う`Lightインデックス`+`Probeインデックス`を`LightAndProbeストライド`で進めながら`buffer`に**SetData**する
                  - 例: `lightIndexCount = 4; reflectionProbeIndexCount = 2; lightAndReflectionProbeIndexCount = lightIndexCount + reflectionProbeIndexCount;`
                        `buffer = {obj0{Light{0,1,2,-1},Probe{2,1}}, obj1{Light{1,2,4,5},Probe{-1,-1}}, obj2{Light{3,1,-1,-1},Probe{2,4}}};`
            - `bool GetShadowCasterBounds(int lightIndex, out Bounds outBounds)`:
              :「その`Lightインデックス`(`lightIndex`)に照らされて影を落とすオブジェクト(*ShadowCaster*)たちを全て囲う**AABB**」を`outBounds`に返す
                 (*ShadowCaster*が一つもない時、`false`を返す)
            - `NativeArray<VisibleLight> visibleOffscreenVertexLights`: **画面外**(Offscreen)の**頂点単位ライト**を集めた`VisibleLight`。(`visibleOffscreenVertexLights`⊂`visibleLights`)
            - `Compute`～`ShadowMatricesAndCullingPrimitives(..)`
              :`activeLightIndex`(`Lightインデックス`)のビュー行列(`viewMatrix`)と投影行列(`projMatrix`)と`ShadowSplitData`を計算する。(`false`の場合、このライトは影を描画しない)
              「`ShadowSplitData`は、ライト視点で影を描くときに“どこまでのオブジェクトを描くか”を決める**カリング範囲データ**だにゃ！(カスケードなので他のシャドウ用途でも使える)」
              - `bool Compute`Spot`ShadowMatricesAndCullingPrimitives(int activeLightIndex, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData)`
              - `bool Compute`Point`ShadowMatricesAndCullingPrimitives(int activeLightIndex, CubemapFace cubemapFace, float fovBias, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData)`
              - `bool Compute`Directional`ShadowMatricesAndCullingPrimitives(int activeLightIndex, int splitIndex, int splitCount, Vector3 splitRatio, int shadowResolution, float shadowNearPlaneOffset, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData)`
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
          - **DrawingSettings** `drawSettings`: `struct DrawingSettings`: オブジェクトの**描画方法**(`パス指定`)と**描画順序**(`SortingSettings`)を指定
            - `static int maxShaderPasses`: >1 回の DrawRenderers 呼び出しで描画できるパスの最大数
            - `.ctor(`**ShaderTagId shaderPassName**`,`**SortingSettings** `sortingSettings)`:
            - **SortingSettings** `sortingSettings`: `struct SortingSettings`: オブジェクトの描画順を設定（必須）
              - `.ctor(Camera camera)`: *⟪⟦Front⟧To⟦Back⟧⟫距離の計算方法*を設定すると思われる
              - **SortingCriteria** `criteria`: `enum SortingCriteria`: 組み合わせ可能。**優先度は上から適用**される
                - `None`:
                - `SortingLayer`: `SortingLayer.value`順
                - `RenderQueue`: `Material.renderQueue`順
                - 距離(`⟪⟦Front⟧To⟦Back⟧⟫`) (これより下は**同一距離**) (**Z Pre Pass**を書いていれば、インスタンシングで効率描画できる？)
                  - `BackToFront`: `far`から`near`順
                  - `QuantizedFrontToBack`: `near`から`far`順 (`Quantized`は**粗い距離でソート**しCPU負荷を削減)
                - `OptimizeStateChanges`: これより下は**同一シェーダ**
                - `CanvasOrder`: Order in Layer❰`sortingOrder`❱順
                - `RendererPriority`: `Renderer.rendererPriority`順
                - 組み合わせ
                  - `CommonOpaque`: (`SortingLayer`|`RenderQueue`|`QuantizedFrontToBack`|`OptimizeStateChanges`|`CanvasOrder`)
                  - `CommonTransparent`: (`SortingLayer`|`RenderQueue`|`BackToFront`|`OptimizeStateChanges`)
              - **⟪⟦Front⟧To⟦Back⟧⟫距離の計算方法** (`class_Camera.md/Sort`も参照)
                - `DistanceMetric distanceMetric`: `enum DistanceMetric`:
                  - `％Perspective`: `Distance(オブジェクト.pos, (Vector3)worldToCameraMatrix.GetColumn(3) * -1)` //`Distance(オブジェクト.pos, cameraPosition)`かも
                  - `Orthographic`: `cameraPosition = new Vector3(0.0, 0.0, 0.0)`, `customAxis = new Vector3(0.0, 0.0, 1.0)` と同じ効果(↓の式を使う)
                  - `CustomAxis`: `Dot((worldToCameraMatrix * オブジェクト.pos) - cameraPosition, customAxis)` //`cameraPosition`意味ないかも
                - `Matrix4x4 worldToCameraMatrix`: `camera.worldToCameraMatrix`と同じと思われる //`DistanceMetric.⟪Orthographic¦CustomAxis⟫`時のみ有効かも
                - `Vector3 cameraPosition, customAxis`: ビュー空間の仮想的な**カメラの位置と向き**(`DistanceMetric.CustomAxis`時のみ有効) //`cameraPosition`はワールド空間かも知れない
            - **パス指定**
              - Tags{"LightMode"}の**優先順位**。(`ShaderTagId[] shaderPassNames`を設定しているだけ) (`.ctor(shaderPassName,..)`⇔`shaderPassNames[0] = shaderPassName`)
                - `ShaderTagId GetShaderPassName(int index)`: `shaderPassNames[index]`
                - `SetShaderPassName(int index, ShaderTagId shaderPassName)`: `shaderPassNames[index] = shaderPassName` (`shaderPassNames[1] = new ShaderTagId("SRPDefaultUnlit")`など)
              - ⟪Material¦Shader⟫関係
                - `Material fallbackMaterial`: `ShaderTagId[] shaderPassNames`にマッチしない時に使用する`Material` (主にデバッグ用)
                - オーバーライド⟪Material¦Shader⟫
                  - メモ
                    🎭 `overrideMaterial` vs. `overrideShader`
                    ・`overrideMaterial` は、すべての`Material`とその`MaterialProperty`を**完全に置き換え**ます。
                    ・`overrideShader` は、**現在のMaterialPropertyを保持**しつつシェーダーだけを変更します。
                    ※ SRP Batcher や BRG では `overrideShader` はサポートされておらず、
                    パフォーマンスにも影響があるため、できれば `overrideMaterial` の方を使ってください。
                    両者を同時に使うことはできません！
                  - `Material overrideMaterial`: オーバーライドする`Material`を設定 (完全に置き換え)
                    - `int overrideMaterialPassIndex`: `ShaderPassIndex`を指定
                  - `Shader overrideShader`: オーバーライドする`Shader`を設定 (現在のMaterialPropertyを保持, BRG サポート外)
                    - `int overrideShaderPassIndex`: `ShaderPassIndex`を指定
            - 機能スイッチ
              - `bool enableInstancing`,`％❰true❱`: GPUインスタンシングの有効化 (`Material.enableInstancing`)
              - `bool enableDynamicBatching`,`％❰false❱`: 動的バッチングの有効化
            - 設定
              - `int lodCrossFadeStencilMask`: Unity2023.1以降(特にHDRP)の**ステンシルバッファ**を使ったLODクロスフェード用32bitステンシルマスクらしい (0で無効)
              - `int mainLightIndex`: >メインライトのオブジェクト毎の**ライトカリング**を実行するために使用。(`VisibleLight[] CullingResults.visibleLights`のインデックスを指定(％自動選択))
              - `PerObjectData perObjectData`: オブジェクト毎の`ShaderProperty`(`UnityPerDraw`,テクスチャ変数,など)をフレーム毎に設定するが、**GPU_RD**ではデータをGPUに常駐させるため**無効**
          - `Nullable<NativeArray<`**RenderStateBlock**`>> stateBlocks`: C#から`∮RenderingState∮`を`mask`してオーバーライドする。(配列なのは、配列内の`mask`が排他的でそれらを合わせるから?)
          - ⟪Pass¦SubShader⟫Tagによる**フィルタリング**。`tagName`の`tagValues`にマッチした`Material`を持つ`Renderer`を描画対象にする
            - `bool isPassTagName`: `isPassTagName`が `true`:`PassTag`,`false`:`SubShaderTag`
            - `ShaderTagId tagName`: `tagName`は`Tags{"ココ"="tagValues"}`
            - `Nullable<NativeArray<ShaderTagId>> tagValues`: `tagValues`は`Tags{"tagName"="ココの候補"}`
      - `RendererList ctx.`**Create**Shadow**RendererList**`(ref ShadowDrawingSettings settings)`:
        - >どのシャドウライト(`lightIndex`)をどの分割設定(`splitData`)で描画するかを設定します。(後は`RendererListParams`より**簡易**)
      - `RendererList ctx.`**Create**Skybox**RendererList**`(Camera camera)`: `camera`と`Material RenderSettings.skybox`を使って**スカイボックス**を描画する
    - `Draw`**Renderer**`(Renderer renderer, Material material, int submeshIndex = 0, int shaderPass = -1)`:
      多分、`renderer`を**Mesh**(`submeshIndex`)と**UnityPerDraw**としてしか使っていない
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
            `＠❰int indexCount, ＠❰int instanceCount ❱❱,`『**Index数**と**Instance数**
          『❰Indirect❱
            `＠❰GraphicsBuffer bufferWithArgs, ＠❰int argsOffset❱❱`『**引数バッファ**。`❰int indexCount, int instanceCount ❱`を詰める
        `)`
      - 引数説明
        - `＠❰Matrix4x4＠❰[]❱ matrix❱`: 多分`UNITY_MATRIX_M`を設定している
        - `int shaderPass`: デフォルト`-1`で**全てのパスを描画**する (URPでもcmdを直接操作しているので描画される)
        - ❰Instanced❱: **インスタンシング**は`Material.enableInstancing`が`true`であること (インスペクターでも設定できる)
        - `＠❰MaterialPropertyBlock properties❱`: 4oはURPでも、**テクスチャや配列でなければSRP Batcherでも使える**と言っている
        - `GraphicsBuffer bufferWithArgs`: `argsOffset`を使って`D_DRAW_INDEXED_ARGUMENTS`を1個の選択できる。`DrawMeshInstancedIndirect(..)`時、`int submeshIndex`との競合は、
          :4o「メッシュバインド用にどのサブメッシュ(`int submeshIndex`)を選ぶかだけ教えてね。あとはバッファ(`bufferWithArgs`)の指示に従うから！」らしい
        - `＠❰GraphicsBuffer indexBuffer❱`: `SV_VertexID`!=`indexBuffer`であり、`SV_VertexID`は単純に**頂点処理の連番**である
          :(`＠❰StructuredBuffer<uint>❱ _indexBuffer[SV_VertexID]`で参照できる)
    - `DrawOcclusionMesh(RectInt normalizedCamViewport)`:
      :ビューポートの範囲(`normalizedCamViewport`)に**VRデバイスが提供するOcclusion Mesh**(見えない部分のメッシュ)を**深度バッファ**に**Nearクリップ面で描画**する
    - `Blit`: **URP非推奨**。`DrawMesh(..)`などを使って**低レベル操作で実現**する。テクスチャを別のレンダーテクスチャにシェーダーを使ってコピー
  - Dispatch系
    - `DispatchCompute(ComputeShader computeShader, int kernelIndex, ⟪｡○⟦, ┃int threadGroups⟪X¦Y¦Z⟫⟧｡¦｡GraphicsBuffer indirectBuffer, uint argsOffset｡⟫)`
      :Computeシェーダーを実行する
      - `ComputeShader computeShader`, `int kernelIndex`: **カーネル**指定 (`computeShader.FindKernel(string name)`で取得)
      - `○⟦, ┃int threadGroups⟪X¦Y¦Z⟫⟧`: **スレッドグループ数**指定
        - `GraphicsBuffer indirectBuffer`, `uint argsOffset`: 間接描画も可能
  - DispatchRays系
    - `SetRayTracingShaderPass`: レイ/ジオメトリ交差シェーダーに使うパスを指定
    - `DispatchRays`: RayTracingShaderを実行
