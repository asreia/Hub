# Light (Behaviour継承)

主に、**Lightパラメータ**, **GlobalIllumination**, **Shadow**

## Instance変数

- **ライティング方法(Built-in?)**
  - `LightRenderMode renderMode`: `Light`のレンダリングする`Mode`を設定する。(⟪`Pixel`¦`Vertex`⟫単位ライティング)
    - `enum LightRenderMode`: ⟪`Auto`¦`Force`⟪`Pixel`¦`Vertex`⟫⟫

- **Culling**
  - `bool useBoundingSphereOverride`: `true`にすると↓で`Override`する
    - `Vector4 boundingSphereOverride`: (フラスタム?)カリングの`boundingSphere`の`Override` (forward+のクラスタとのコリジョンは円錐?)
  - `int cullingMask`: `GameObject.layer`との論理積で1以上の時、この`Light`による**ライティングを受ける**
  - `renderingLayerMask`: ↑の`renderingLayer`版?(`renderer.renderingLayerMask`との論理積) `Inspector`には↑が無くて、`Shadow`版もあったが?..

- **Lightパラメータ**
  - `LightType type`: `Light`の**種類**を設定する。
    - `enum LightType`: ⟪｡Directional¦Point¦『Spot系』⟪Spot¦Box¦Pyramid⟫¦『GI』⟪Rectangle¦Disc¦Tube⟫｡⟫
  - **Lightの色と明るさ** (color x colorTemperature x intensity?)
    - `Color color`: `Light`の**色**を設定する。
    - `bool useColorTemperature`: ↓色温度を使用するには`true`を設定する。
      - `float colorTemperature`: >光の**色温度**。相関色温度（CCTと略される）は、光源の最終的な色を計算する際に**カラーフィルター(color?)と掛け合わされる**。
    - `float intensity`: `Light`の**強度**
  - **Lightの形**
  - `float spotAngle`: `Spot`の**外側**の円錐の角度(度)
  - `float innerSpotAngle`: `Spot`の**内側**の円錐の角度(度)
  - `float range`: `Light`の**範囲**。
    - >**エリアライト**は1点ではなく発光面を持つため、光の累積範囲はこの特性よりも**大きくなる**。
    - >この大きな範囲は、`Light.dilatedRange`プロパティから読み取ることができる。
    - >**非エリアライト**の場合、`Light.range`と`Light.dilatedRange`は**同じ**値を返します。

- **GlobalIllumination**
  - `LightBakingOutput bakingOutput`: GIの**ベイク情報**
    - `struct LightBakingOutput`:
      - `bool isBaked`: >この`Light`は⟪`Lightmap`¦`Lightprobe`⟫に**ベイクされているか**
      - `LightmapBakeType lightmapBakeType`: **ベイク**の**タイプ**
        - `enum lightmapBakeType`:
          - `Realtime`: リアルタイム
          - `Baked`: ベイク
          - `Mixed`: ミックス
      - 以下`Mixed`の**場合**
        - `MixedLightingMode mixedLightingMode`: **Mixed**の**タイプ**
          - `enum MixedLightingMode`:
            - `IndirectOnly`: 間接光だけベイクし、直接光とシャドーはリアルタイム
            - `Shadowmask`: 間接光とシャドーをベイクし、直接光はリアルタイム (`Distance Shadowmask`: 近距離のみリアルタイムシャドー)
            - `Subtractive`: 全てのライトパスをベイク? (スペキュラーが死ぬ。影の色を調整する)
        - `int occlusionMaskChannel`: >使用する`OcclusionMaskChannel`のインデックス?
        - `int probeOcclusionLightIndex`: >`OcclusionProbe`の視点から見た`Light`のインデックス?
  - `lightShadowCasterMode`: `mixedLightingMode == Shadowmask`時、`Light`毎にグローバルShadowmaskモードを**オーバーライド**できる。
    - `enum LightShadowCasterMode`: `ShadowMap`へのレンダリング設定っぽい
  - `LightmapBakeType lightmapBakeType`: ↑↑と同じと思われる。フィールド直下版?
  - `Vector2 areaSize`: >エリアライトの大きさ (Editor only).
  - `float bounceIntensity`: バウンスライト(**ベイク**)の`Intensity`。(1が物理的に正しい)
  - `float dilatedRange`: エリアライトの場合はその**範囲**(非エリアライトの場合は`range`と`dilatedRange`は同じ)

- **cookie, flare**
  - `Texture cookie`: `cookie`の`Texture`。`Point Light`の場合は`CubeMap`になる。ベイクの場合は`EditorSettings.enableCookiesInLightmapper`を有効にする
  - `float cookieSize`: >`Directional Light`の`cookie`の`Size`
  - `Flare flare`: >`Light`に`flare`を使用する。
    - `class Flare (UnityObject 継承)`
      - フィールドなし。`Inspector`で設定する。

- **Shadow**
  - 基本
    - `LightShadows shadows`: `Shadow`の掛け方
      - `enum LightShadows`: ⟪％None¦Hard¦Soft⟫
    - `float shadowStrength`: `Shadow`の**強さ**
  - Shadow解像度
    - `LightShadowResolution shadowResolution`: `ShadowMap`の**解像度**。
      - `enum LightShadowResolution`: ⟪％`FromQualitySettings`¦`Low`¦`Medium`¦`High`¦`VeryHigh`⟫
    - `int shadowCustomResolution`: (**Built-inのみ**)`ShadowMap`のカスタムの**解像度**。
  - カリング系
    - `bool useViewFrustumForShadowCasterCul`: `Light`がビュー・フラストラムの外側にある場合に、この`Light`の**Shadow**を**Culling**するかどうか。
    - `float[] layerShadowCullDistances`: **Layer単位**の**シャドー**(キャスト)の**カリング距離**。
      - (`Directional Light`のみ。 `Camera.layerCullDistances`のシャドー版。`Camera.layerCullSpherical`は共有される)
    - フラスタムカリング?
      - `useShadowMatrixOverride`: ↓のオーバーライド許可
        - `Matrix4x4 shadowMatrixOverride`: >`Shadow`の(フラスタム?)カリング時に、通常の**投影行列**を**オーバーライド**する。
      - `float shadowNearPlane`: **Shadow視錐台**に使用する`NearPlane`との距離?(これもカリング?)
  - ベイク?
    - `float shadowAngle`: `DirectionalLight`によって投影される`Shadow`のエッジに適用される人工的な柔らかさの量を制御します。 (Editor only).
    - `float shadowRadius`: ポイントライトまたはスポットライトによって投影される影のエッジに適用される人工的な柔らかさの量を制御します。 (Editor only).
  - バイアス
    - `float shadowBias`: `ShadowMap`比較時の`Bias`
    - `float shadowNormalBias`: `ShadowMap`比較時の`NormalBias`

- **CommandBuffer**
  - `int commandBufferCount`: この`Light`を設定する`cmd`の数 (Read Only)

## Instance関数

- **Reset**
  - `Reset()`: 全ての`Light`パラメーターをデフォルトに戻す。
- **Dirty**
  - `SetLightDirty()`: `Lightベーキング`のバックエンドに内部ライト表現を更新するよう通知するために、`Light`のダーティを設定する。(Editor only)
- **CommandBuffer**
  - `AddCommandBuffer(LightEvent evt, CommandBuffer buffer ＠❰, ShadowMapPass shadowPassMask❱)`: >指定された`LightEvent`で実行されるように`cmd`を追加します。
    - `enum ShadowMapPass`: ↑の`LightEvent.⟪Before¦After⟫ShadowMapPass`の時、より細かくどの`Light`でどの`CubeMap`の面の描画なのか指定できる
  - `AddCommandBufferAsync(LightEvent evt, CommandBuffer buffer ＠❰, ShadowMapPass shadowPassMask❱, ComputeQueueType queueType)`: ↑のGPU非同期コンピュートキュー版
  - `CommandBuffer[] GetCommandBuffers(LightEvent evt)`: >指定された`LightEvent`で実行される`cmd`を取得します。
  - `RemoveCommandBuffer＠❰s❱(LightEvent evt, ＠❰CommandBuffer buffer❱)`: >指定された`LightEvent`で実行から＠❰全ての❱`cmd`を削除します。
    - `RemoveAllCommandBuffers()`: この`Camera`に設定される全ての`cmd`を削除します。
