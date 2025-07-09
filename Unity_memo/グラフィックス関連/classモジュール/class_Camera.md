# Camera (Behaviour継承)

主に、**depth**, **cameraType**, **cullingMask**, **Sort**, **GetAllCameras(..)**,
     **⟪target¦active⟫Texture**, **clearFlags**, **depthTextureMode**, **allow⟪HDR¦MSAA¦DynamicResolution⟫**, **描画位置(ViewPort?)**,
     **⟪worldToCamera¦projection⟫Matrix**, **cullingMatrix**

## Static変数

- **Camera**
  - ●`Camera[] allCameras`: `Scene`内の有効(`enabled`)な`Camera`を全て返す。
  - `int allCamerasCount`: 現在の`Scene`の`Camera`数。(↑の長さ)
  - `Camera current`: 現在レンダリングに使用(`on～Render`時の`Camera`?)している、低レベルレンダリングを管理するカメラ（Read-Only）。
  - `Camera main`: `MainCamera`と**タグ付け**された、最初に有効化されたCameraコンポーネント。(Read-Only).
- **on～Renderデリゲート版(Built-in)**
  ●`URP`は`RenderPipelineManager`を使用
  - `delegate void CameraCallback(Camera cam)`
    - `Camera.CameraCallback onPreCull`:`Camera`が`Scene`を切り取る(`Cull`)**前**に、カスタムコードを実行するために使用できる`Delegate`です。
    - `Camera.CameraCallback onPreRender`: `Camera`が`Scene`をレンダリングする**前**に、カスタムコードを実行するために使用できる`Delegate`です。
    - `Camera.CameraCallback onPostRender`: `Camera`が`Scene`をレンダリングした**後**に、カスタムコードを実行するために使用できる`Delegate`です。
- **物理カメラ**
  - `float k⟪Max¦Min⟫Aperture`: 最大許容絞り。
  - `int k⟪Max¦Min⟫BladeCount`: 絞りダイアフラムの最大ブレード数。

## Static関数

- **Camera**
  - `int GetAllCameras(Camera[] cameras)`: `Scene`内の全ての`Camera`を配列に埋めてもらう(戻り値は`Camera`の数)(`allCameras`のNoAlloc版)
- **物理カメラ**
  - `CalculateProjectionMatrixFromPhysicalProperties(..)`: `物理カメラ各種パラメータ`から`投影行列`を**計算**します。
  - `float ⟪｡⟦FocalLength⟧To⟦FieldOfView⟧｡⟫(float ⟪focalLength¦fieldOfView⟫, float sensorSize)`: ⟪｡⟦`FocalLength`(焦点距離)⟧を⟦`FieldOfView`(視野)⟧｡⟫に**変換**する。
  - `⟪｡⟦Horizontal⟧To⟦Vertical⟧｡⟫FieldOfView(float ⟪horizontal¦Vertical⟫FieldOfView, float aspectRatio)`:
    - `aspectRatio`に基づいて、⟪｡⟦`Horizontal`⟧方向を⟦`Vertical`⟧方向⟧｡⟫の視野(`FoV`)に**変換**する。

## Instance変数

- **レンダリング**
  - `float` **depth**: `Camera`のレンダリング順序。**昇順でレンダリング**される。(Unity.drawio/ページ21 `SortCameras(cams)`参照)
  - `CameraType` **cameraType**: >どの種類の`Camera`であるかを識別する。(Unity.drawio/ページ21 `IsGameCamera(cam)`参照)
    - `enum CameraType`: ⟪`Game`¦`SceneView`¦`Preview`¦`VR`¦`Reflection`⟫
  - `bool useOcclusionCulling`: **オクルージョンカリング**を使用するかどうか。
  - `int` **cullingMask**: `gameObject.layer`との**論理積**でその`gameObject`を**描画するか**決める
  - `Scene scene`: NULLでない場合、カメラは指定されたSceneの内容のみをレンダリングする。(EditorSceneManager.NewPreviewSceneで作成されたSceneのみサポート)
  - **Sort**
    - `enum OpaqueSortMode opaqueSortMode`: オブジェクトを不透明にするソーティングモード
      - `⟪Default¦FrontToBack¦NoDistanceSort⟫`
    - `ResetTransparencySortSettings()`でリセット
      - `enum TransparencySortMode transparencySortMode`: ソートモードの透明オブジェクト。
        - `⟪Default¦Perspective¦Orthographic¦↓CustomAxis(or GraphicsSettings)⟫`
      - `Vector3` `transparencySortAxis`: オブジェクト(`Renderer`)を描画順のソートする時に使用する軸(`Axis`) (**Built-inかも?**)
  - **Built-In,レガシー機能**
    - RenderingPath
      - `RenderingPath actualRenderingPath`: 現在使われているレンダリングパス(Read-Only)。
        - `enum RenderingPath`: ⟪UsePlayerSettings¦VertexLit¦Forward¦DeferredShading⟫
      - `RenderingPath renderingPath`: 可能な場合、レンダリングパスが使用されます。
    - `bool clearStencilAfterLightingPass`: カメラが Deferred Light Path の後にステンシルバッファをクリーンにする必要があるかどうか
    - `bool renderCloudsInSceneView`: Falseの場合、このカメラのシーンビューに雲は描画されない。
    - `int commandBufferCount`: コマンドバッファ数をこのカメラに設定(取得?)します (Read-Only)

- **カリングとレイヤー**
  - **Layer単位のカリング距離**
    - `float[] layerCullDistances`: **gameObject.layer単位**の**カリング距離**。
    - `bool layerCullSpherical`: カメラに対して↑のLayer単位のカリングを**球体**で実行するか。(カメラのnear-farクリップの距離ではないという意味?)
    - `Matrix4x4 cullingMatrix`: `ctx.Cull(..)`で使用する**AABBフラスタムカリング用Matrix**。(`デフォルト: mull(projectionMatrix, worldToCameraMatrix)`)
      - (`ResetCullingMatrix()`でリセット)
  - 現在URPで使われているか分からん
    - `ulong overrideSceneCullingMask`: どのオブジェクトをどのSceneから描画するかを決定するための`CullingMask`を設定します。
      - EditorSceneManager.SetSceneCullingMaskを参照してください。(分からん)
    - `int eventMask`: `Camera`のイベントを発動できるレイヤーを選択するためのマスク。(分からん)

- **カメラとテクスチャに関する情報**
  - **描画位置(ViewPort?)**
    - `Rect ＠❰pixel❱Rect`: 画面上の **⟪正規(0～1)¦ピクセル(解像度)⟫座標**でどこに`Camera`が描画されるか。
    - `int ＠❰scaled❱pixel⟪Width¦Height⟫`: `Camera`の解像度の`⟪Width¦Height⟫`幅。(`ViewPort.⟪x¦y⟫`) (`❰scaled❱`は、**ダイナミック解像度**のスケーリングを**考慮する**) (Read-Only)
  - ●`Vector3` **velocity**: `Camera`のワールド空間での速度を取得します（読み取り専用）。

- **テクスチャ設定**
  - `Camera`の`RenderTexture`
    - メモ
      - `camera.targetTexture = rt;`をセットし`camera.Render();`をすると、
        `camera.activeTexture = camera.targetTexture;`される。(多分)
        `camera.Render()`は、`rt`に描画後`元のRT`に戻すので`RenderTexture.active`は変わらない。
      - 基本的に`camera.targetTexture`と`camera.activeTexture`は`camera.Render()`のみに影響があるが、
        **URPでも互換性**のため、`camera.targetTexture`と`camera.activeTexture`はURPでもサポートされている。
      - `camera.targetTexture = null;`の場合は、`BRTT.CameraTarget`に描画される。
      - **URPでは**、`camera.Render()`は使われず`camera`の情報を使って`cmd.DrawRendererList(｢camera｣, ..)`で描画する。
    - `RenderTexture` **targetTexture**: >移動先のレンダーテクスチャ (**オフスクリーンのRTを"設定"**)
      - `SetTargetBuffers(RenderBuffer＠❰[]❱ colorBuffer, RenderBuffer depthBuffer)`で、**MRT**など詳細に設定できるみたい
    - `RenderTexture` **activeTexture**: >この`Camera`の一時的な`RenderTexture`ターゲットを取得します。(**このCameraの描画先RTを"取得"**)
    - `bool forceIntoRenderTexture`: (**Built-in?**)>`Camera`の描画をRT(`targetTexture`?)に強制されるかどうか。
  - `int targetDisplay`: この`Camera`を指定されたディスプレイにレンダリングする。(`int`にどう設定する?)
  - **allow**
    - `bool allowHDR`: (**Built-in?**)**HDR**の許可。(R16G16B16A16FLOAT?)
    - `bool allowMSAA`: **MSAA**の許可。(rt.msaaSamples)
    - `bool allowDynamicResolution`: **動的解像度**の許可
  - `enum CameraClearFlags` **clearFlags**: >`Camera`をレンダリングするときに**何でクリアするか**を決める
    - `Skybox`: スカイボックスでクリアする
    - `SolidColor`: 背景色をクリアします。
      - `Color backgroundColor`: 背景色
    - `Depth`: **深度バッファのみ**をクリアする
    - `Nothing`: 何もクリアしません
  - `enum DepthTextureMode depthTextureMode`: (**Built-in?**)>どのようにカメラがテクスチャを生成するか。まず生成するのか。(`Depth`と言いつつ`MotionVectors`も生成するの?)
    - `None`: 深度テクスチャを生成しません（デフォルト）
    - `Depth`: 深度テクスチャを生成します
    - `DepthNormals`: 深度と法線を組み合わせたテクスチャを生成します
    - `MotionVectors`: (可能な場合に) モーションベクターをレンダリングするかを指定します。

- **カメラ行列**
  - **変換行列**
    - `Matrix4x4 worldToCameraMatrix`: >`World`空間から`Camera`空間に変換する**行列** (`ResetWorldToCameraMatrix()`でリセット)
    - `Matrix4x4 cameraToWorldMatrix`: >`Camera`空間から`World`空間に変換する**行列** (Read-Only)
      - >`Camera`空間は**OpenGLの慣例に一致**していることに注意してください：`Camera`の**前方**は**負のZ軸**です (これがDepthの**nearが1**の理由?)
    - `Matrix4x4 projectionMatrix`: >カスタム**射影行列**を**設定**します。
      - >この行列を変更すると、`Camera`は`fieldOfView`に基づいてレンダリングを更新しなくなります。さらに、カメラの`nearClipPlane`と`farClipPlane`は変更されないので、不整合を避けるために手動で更新する必要があります。これは`ResetProjectionMatrix()`を呼び出すまで続きます。
    - `Matrix4x4 previousViewProjectionMatrix`: >**最後のフレーム**で使用された`ViewProjection`行列を取得します。
    - **ジッター**
      - `Matrix4x4 nonJitteredProjectionMatrix`: >カメラオフセットのない(小刻みな**振動がない**)Rawの`Projection`行列を**取得/設定**します。
      - `bool useJitteredProjectionMatrixForTransparentRendering`: >`Transparent`レンダリングに**ジッター**マトリックスを**使うべきか**？
  - **パラメータ**
    - **平行投影**
      - `bool` **orthographic**: >`Camera`が`orthographic`(true)か、`perspective`(false)か。
        - >`orthographic`の場合、視域は`orthographicSize`で定義される。
        - >`perspective`の場合、視域は`fieldOfView`で定義される。
      - `orthographicSize`: >orthographic モードの場合、カメラの半分のサイズ。(?)
    - **透視投影**
      - `float fieldOfView`: >`Camera`の**垂直視野**（度）。(`シェーダー\透視投影.png`の画角θ/2の**θ**?)
      - `float aspect`: >**アスペクト比**（幅を高さで割った値）。
        - >**変更した場合**は、`camera.ResetAspect()`を呼び出すまで**値が保持**される
      - `float farClipPlane`: >`Camera`からの遠方クリッピング面の距離（ワールド単位）。
      - `float nearClipPlane`: >`Camera`からの近方クリッピング平面の距離（ワールド単位）。
    - **物理カメラ(usePhysicalProperties)**
      - `usePhysicalProperties`: **物理カメラ**を使用する
      - `Vector2 sensorSize`: >カメラセンサーの大きさで、単位はミリメートル。
      - `Camera.GateFitMode gateFit`: カメラにはセンサーゲートと解像度ゲートの2種類があります。センサーゲートは`sensorSize`プロパティ、解像度ゲートはレンダーターゲット領域で定義されます。
      - `Vector2 lensShift`: >カメラのレンズオフセット。レンズシフトはセンサーサイズに対する相対値である。例えば、0.5のレンズシフトは、センサーの水平サイズの半分をオフセットします。
      - `float anamorphism`: >アナモルフィックをシミュレートするためにセンサーを伸ばします。正の値はカメラを垂直に歪ませ、負の値はカメラを水平に歪ませます。
      - `float aperture`: >カメラの絞り。
      - `float barrelClipping`: >カメラバレルクリッピング。
      - `int bladeCount`: >カメラのレンズ内のブレード数。
      - `Vector2 curvature`: >ブレードの曲率。
      - `float focalLength`: >カメラの焦点距離をミリメートルで表します。
      - `float focusDistance`: >レンズの焦点距離。
      - `int iso`: >カメラのセンサ感度。
      - `float shutterSpeed`: >カメラの露光時間（秒）。

- **XR関係**
  - `float stereoConvergence`: >仮想の目が収束する点までの距離。
  - `Camera.MonoOrStereoscopicEye stereoActiveEye`: **現在レンダリングしている目**を返します。ステレオが有効になっていないときに呼び出された場合は、Camera.MonoOrStereoscopicEye.Mono を返します。OnRenderImage などのカメラレンダリングコールバック中に呼び出された場合、現在レンダリングしている目を返します。レンダリングコールバック以外で呼び出され、ステレオが有効になっている場合、デフォルトの目（Camera.MonoOrStereoscopicEye.Left）を返します。
  - `bool stereoEnabled`: 立体レンダリング。
  - `float stereoSeparation`: 仮想の目の間隔。現在の目の間隔を照会または設定する場合に使用します。ほとんどのVRデバイスはこの値を提供しますが、その場合、この値を設定しても効果はありません。
  - `bool areVRStereoViewMatricesWithinSingleCullTolerance`: ステレオビュー行列が、シングルパスカルを可能にするのに適しているかどうかを判断します。
  - `StereoTargetEyeMask stereoTargetEye`: VR ディスプレイのどちらの目にカメラがレンダリングするかを決定します。

## Instance関数

- **Copy, Culling, Frustum**
  - `CopyFrom(Camera other)`: `other`の設定パラメータをこの`Camera`に**コピー**する(`Transform`と`Layer`もコピーされる)
  - `bool TryGetCullingParameters(＠❰bool stereoAware,❱ out ScriptableCullingParameters cullingParameters)`:
    - `Camera`の`ScriptableCullingParameters`を取得する。(`ctx.Cull(ref cullParams)`のようなもの?)
  - `CalculateFrustumCorners(Rect viewport, float z, Camera.MonoOrStereoscopicEye eye, Vector3[] outCorners)`:
    - `viewport`座標と`Camera`の深度(`z`)が与えられると、`View`空間の`Frustum`の4つの`Corner`(クラスタの面のようなもの)を計算して`outCorners`に入れる。(Forward+のクラスタ?)

- **CommandBuffer, Shader, Render**
  - **⟪Add¦Get¦Remove⟫CommandBuffer**
    - `AddCommandBuffer(CameraEvent evt, CommandBuffer buffer)`: >指定された`CameraEvent`で実行されるように`cmd`を追加します。
    - `AddCommandBufferAsync(CameraEvent evt, CommandBuffer buffer, ComputeQueueType queueType)`: ↑のGPU非同期コンピュートキュー版
    - `CommandBuffer[] GetCommandBuffers(CameraEvent evt)`: >指定された`CameraEvent`で実行される`cmd`を取得します。
    - `RemoveCommandBuffer＠❰s❱(CameraEvent evt ＠❰, CommandBuffer buffer❱)`: >指定された`CameraEvent`で実行から＠❰全ての❱`cmd`を削除します。
      - `RemoveAllCommandBuffers()`: この`Camera`に設定される全ての`cmd`を削除します。
  - **Shader**
    - `SetReplacementShader(Shader shader, string replacementTag)`: >`Camera`は`Shader`を置き換えたビューをレンダリングします。
      - `ResetReplacementShader()`: `Camera`で変更された`Shader`を削除します。(↑の設定のリセット)
  - **Render**
    - ●`SubmitRenderRequest(RequestData renderRequest)`: `URP`の`SR.SubmitRenderRequest(..)`を呼び、単一の`Camera`を指定した`RT`へ**描画**する?
    - `Render()`: >手動でカメラをレンダリングします。(`RenderTexture.active`に対して↑の`AddCmd(..)`された`cmd`で描画する?)
    - `RenderToCubemap(Cubemap cubemap, int faceMask)`: `cubemap`の`faceMask`に描画する
    - `RenderWithShader(Shader shader, string replacementTag)`: `Shader`を交換して`Camera`をレンダリングします。

- **座標変換系**
  - **Matrix**
    - Stereo
      - `GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye eye)`: >`eye`で指定された目の非ジッター投影行列を取得する。
      - `GetStereoProjectionMatrix(Camera.StereoscopicEye eye )`: >`eye`で指定された目の投影行列を取得する。
      - `SetStereoProjectionMatrix(Camera.StereoscopicEye eye, Matrix4x4 matrix)`: `eye`で指定された目を`matrix`で設定する。(通常はVR SDK が提供する投影Matrixを使用することを推奨)
        - (`ResetStereoProjectionMatrix()`でリセット)
      - `GetStereoViewMatrix(Camera.StereoscopicEye eye)`: >`eye`で指定された目のビュー行列を取得する。
      - `SetStereoViewMatrix(Camera.StereoscopicEye eye, Matrix4x4 matrix)`: >`eye`で指定された目を`matrix`で設定する。(通常はVR SDK が提供するビューMatrixを使用することを推奨)
      - `ResetStereoViewMatrices()`: >↑の`SetStereoViewMatrix(..)`で設定された`matrix`を`VR SDK が提供するビューMatrix`に戻す
      - `CopyStereoDeviceProjectionMatrixToNonJittered(Camera.StereoscopicEye eye)`: >VR SDK から取得した非ジッター投影行列を設定します。(`ResetStereoViewMatrices()`と何が違う?)
    - `Matrix4x4 CalculateObliqueMatrix(Vector4 clipPlane)`: 計算し、斜めの近平面射影行列?を返します。
  - **座標変換**
    - `Vector3 WorldToViewportPoint(Vector3 position ＠❰, Camera.MonoOrStereoscopicEye eye❱)`: `World`空間の`position`を`Viewport`空間に変換します。
    - `Vector3 WorldToScreenPoint (Vector3 position ＠❰, Camera.MonoOrStereoscopicEye eye❱)`: `World`空間の`position`を`Screen`空間に変換します。
    - `Vector3 ScreenToViewportPoint(Vector3 position)`: `Screen`空間の`position`を`Viewport`空間に変換します。
    - `Vector3 ScreenToWorldPoint(Vector3 position, ＠❰, Camera.MonoOrStereoscopicEye eye❱)`: `Screen`空間の`position`を`World`空間に変換します。
    - `Vector3 ViewportToScreenPoint(Vector3 position)`: `Viewport`空間の`position`を`Screen`空間に変換します。
    - `Vector3 ViewportToWorldPoint(Vector3 position)`: `Viewport`空間の`position`を`World`空間に変換します。
- **Ray**
  - `Ray ScreenPointToRay(Vector3 pos ＠❰, Camera.MonoOrStereoscopicEye eye❱)`: `camera.transform?`から`ScreenPoint`を通して`Ray`を飛ばす
  - `Ray ViewportPointToRay(Vector3 pos ＠❰, Camera.MonoOrStereoscopicEye eye❱)`: `camera.transform?`から`ViewportPoint`を通して`Ray`を飛ばす

- GateFit?
  - `float GetGateFittedFieldOfView()`: ゲートフィットを含むカメラの有効垂直視野を取得します。
    - センサーゲートと分解能ゲートの取り付けは、最終的な視野に影響を与える。
    - センサーゲートのアスペクト比が解像度ゲートのアスペクト比と同じ場合、またはカメラが物理モードでない場合、このメソッドは `fieldofview` プロパティと同じ値を返す。
  - `Vector2 GetGateFittedLensShift()`: GateFit を含む、カメラの有効なレンズオフセットを取得します。
    - センサーゲートと解像度ゲートの取り付けは、投影の最終的な斜度に影響を与える。
    - センサーゲートのアスペクト比が解像度ゲートのアスペクト比と同じ場合、このメソッドは`lensShift`プロパティと同じ値を返す。
    - カメラが物理モードでない場合、このメソッドはVector2.zeroを返す。

- **Reset**
  - `Reset()`: すべての`Camera`パラメータをデフォルトに戻す。
  - `ResetWorldToCameraMatrix()`: `worldToCameraMatrix`への設定を`Reset`する
  - `ResetProjectionMatrix()`: `projectionMatrix`への設定を`Reset`する
  - `ResetStereoProjectionMatrices()`: `SetStereoProjectionMatrix`への設定を`Reset`する
  - `ResetCullingMatrix()`: `cullingMatrix`への設定を`Reset`する
  - `ResetAspect()`: `aspect`への設定を`Reset`する
  - `ResetTransparencySortSettings()`: `transparencySort⟪Axis¦Mode⟫`への設定を`Reset`(GraphicsSettings)する

## Message

- **On～Renderメッセージ版(Built-in)**
  `URP`は`RenderPipelineManager`を使用
  - `OnPreCull`: `Camera`が❰`scene`を切り取る(`Cull`)**前**❱に呼び出すイベント関数。
  - `OnWillRenderObject`: ❰`Camera`から`Object`が可視状態❱
  - `OnPreRender`: ❰`Camera`が`Scene`をレンダリングする**前**❱
  - `OnRenderObject`: ❰`Camera`が`Scene`をレンダリングした**後**❱(Object単位?)
  - `OnPostRender`: ❰`Camera`が`Scene`をレンダリングした**後**❱
  - `OnRenderImage(RenderTexture source,RenderTexture destination)`: UnityがCameraのレンダリング終了後に呼び出すイベント関数で、Cameraの最終画像を変更できます。
