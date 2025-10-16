# Texture (UnityObject継承)

主に、**ResourceDesc**, **TextureSampler**, `isReadable`, `GraphicsTexture`

## Static変数

- `int GenerateAllMips`: 全ての`MipMapLv`を生成することを示す(-1)。`RenderTexture`の`.ctor`などに使用する
- `bool allowThreadedTextureCreation`: >`true`マルチスレッド(デフォルト), `false`レンダースレッド、でテクスチャを生成する
- **メモリー** (半分よく分からない)
  - `ulong currentTextureMemory`: >シーン内のすべてのTextureが使用するメモリ量。
  - `ulong targetTextureMemory`: >Unityがメモリバジェットを適用し、Textureのロードを終了した後、シーン内のTextureに割り当てるTextureメモリの総量。
    - >`targetTextureMemory`はミップマップのストリーミング設定も考慮する。
    - >この値には、Texture2D と CubeMap Textures のインスタンスのみが含まれます。
    - >この値には、他のTextureタイプや、Unityが内部で作成する2DやCubeMap Textureは含まれません。
  - `ulong desiredTextureMemory`: >他の制約がない場合にUnityがロードするTexturesの合計サイズ（バイト単位）。
    - >UnityはTextureをロードする前にメモリバジェットを適用し、Textureのサイズがその値を超えた場合、ロードされたTextureの解像度を下げます。
    - >望ましい`TextureMemory`値は、Unityが要求した、または手動で設定したミップマップレベルを考慮します。
    - >例えば、Textureが遠かったり、要求されたミップマップレベルが0より大きかったりするため、UnityがTextureをフル解像度でロードしない場合、
      - >Unityは必要なメモリ総量に合うように`dishiredTextureMemory`値を減らします。`dishiredTextureMemory`値は`Texture.targetTextureMemory`値より大きくすることができます。
  - `ulong totalTextureMemory`: >UnityがすべてのTextureをミップマップ**レベル0**でロードした場合に使用するTextureメモリの総量。
    - >これは理論値であり、ストリーミングシステムからの入力や、手動で`Texture2D.requestedMipmapLevel`を設定した場合などの他の入力は考慮されていません。
    - >入力を考慮したテクスチャメモリの値を見るには、`desiredTextureMemory`を使います。
    - >`totalTextureMemory`はTexture2DとCubeMap Textureのインスタンスのみを含む。
    - >この値には、他のTextureタイプや、Unityが内部で作成する2DやCubeMap Textureは含まれません。
  - `ulong nonStreamingTextureMemory`: >Unityがシーン内の**非ストリーミング**Textureに割り当てるメモリ量。
    - >これには、Texture2D と CubeMap Textures のインスタンスのみが含まれます。
    - >これには、他のTextureタイプや、Unityが内部で作成する2DやCubeMap Textureは含まれません。
- **MipMapストリーミング** (`Scene`ロード時や`Camera`からの距離と`メモリバジェット`に応じて、`MipMap`の**大きいMipMapLv**の**ロードを優先**する)
  [MipMapとMipMapストリーミング。これ見れば大体分かる](https://www.youtube.com/watch?v=knqdwREIM2U)
  - **Count**
    - `ulong nonStreamingTextureCount`: >シーン内の非ストリーミングテクスチャの数。
      - これには、Texture2D と CubeMap Texture のインスタンスが含まれます。その他の Texture タイプや、Unity が内部的に作成する 2D および CubeMap Texture は含まれません。
    - `ulong streamingTextureCount`: >ストリーミング・テクスチャーの数。
    - `ulong streamingTextureLoadingCount`: >現在ロード中のミップマップを持つストリーミングテクスチャの数。
    - `ulong streamingTexturePendingLoadCount`: >ロードされる未処理のミップマップを持つストリーミング・テクスチャの数。?
    - `ulong streamingRendererCount`: >テクスチャストリーミングシステムに登録されている`Renderer`の数。
    - `ulong streamingMipmapUploadCount`: >テクスチャのミップマップ・ストリーミングにより、テクスチャが何回アップロードされたか。
  - `bool streamingTextureDiscardUnusedMips`:
    - >このプロパティは、ストリーミングテクスチャシステムに、テクスチャのメモリバジェットを超えるまで、未使用のミップマップをキャッシュする代わりにすべて破棄させます。
    - >これは、予測可能な Texture のセットをメモリに保持するために、プロファイルを作成したり、テストを書いたりするときに便利です。
  - `bool streamingTextureForceLoadAll`: >ストリーミング・テクスチャに**全て**の`MipMapLv`をロードさせる。

## Static関数

- `SetGlobalAnisotropicFilteringLimits(int forcedMin, int globalMax)`: 異方性のリミットを設定します
  - `forcedMin`は強制された場合の最小。 `globalMax` は最大。 任意の場合に -1 を設定することでデフォルト値が使用されます。
- `SetStreamingTextureMaterialDebugProperties()`: このTexture を mipmap ストリーミングシステムで使用するマテリアルの mipmap ストリーミング**デバッグShaderPropertyを設定**します。

## Instance変数

- **ResourceDesc**
  - `enum TextureDimension dimension`: >Textureの**次元数** (Read-Only)
    - ⟪`Unknown`¦`None`¦`Any`¦⟪`Tex2D`¦`Cube`⟫＠❰`Array`❱¦`Tex3D`⟫
  - `GraphicsFormat graphicsFormat`: ピクセルの**型** (`Texture2D`の場合`GPUテクスチャ`のフォーマット?)
    - `GraphicsFormat`: `R8G8B8A8_SRGB`や`R16G16B16A16_SFloat`とかのやつ
  - **解像度** (`PlayerSettings.mipStripping`が有効な場合、許容する最高解像度での`Texture`の⟪幅¦高さ⟫)
    - `int width`: >**幅** (Read-Only).
    - `int height`: >**高さ** (Read-Only).
  - `int mipmapCount`: >**MipMapレベル数** (Read-Only).
- **TextureSampler**
  - `FilterMode filterMode`: >`Texture`の**フィルタリングモード**。
    - `enum FilterMode`: ⟪`Point`¦％?`Bilinear`¦`Trilinear`⟫(<https://someiyoshino.info/entry/2022/01/30/171912>)
  - `enum TextureWrapMode wrapMode`: >テクスチャ座標の折り返しモード(Texture Addressing Mode)。**Set**:全ての軸に設定。**Get**:`wrapModeU`と同じ
    - ⟪`Repeat`¦`Clamp`¦`Mirror`¦`MirrorOnce`⟫
    - `TextureWrapMode wrapMode⟪U¦V¦W⟫`: >テクスチャ⟪U¦V¦W⟫軸座標の折り返しモード。
  - `float mipMapBias`: >Textureの**ミップマップバイアス**。(算出された`MipMapLv`に加算される)
  - `anisoLevel`: >**異方性フィルタリングレベル**を設定します。(鋭角な`視線の方向`の**解像度が上がる**)
    - (`Texture`との`視線の角度`によって、サンプリングする範囲を変える(鋭角ほど広い)(`Level`は範囲中のサンプリング数))
    - (>`Direct3D 11 API`には、異方性フィルタリングを使用すると`Trilinear`が**有効**になってしまうという**制限**があります。)
    - (anisoLevel 値が `1` から `9` の間で、**Quality Settings** の `Anisotropic Filtering` が `Forced On` に設定されている場合、Unity は anisoLevel を **9** に設定します。(`0`は無効))
    - (考察: `Trilinear`＆`anisoLevel`の場合、**サンプリング数** = 2(`Bilinear`) x 2(`MipMap補間`) x anisoLevel数 ?)
- **GraphicsTexture** (Unity 2022.1 以降に導入された**新しい機能**)
  - `class GraphicsTexture` `graphicsTexture`: >**GPUにアップロード**された**R_Resource**の**ビュー**を表す。(Read-Only).
    - `static GraphicsTexture active`: **現在アクティブ**な`GraphicsTexture`。`static RenderTexture active`のようなもの
      - (**Set**するにはテクスチャ作成時に`GraphicsTexture.descriptor.flags`で`GraphicsTextureDescFlags.RenderTarget`を有効にする)
    - `GraphicsTextureDescriptor descriptor`: UnityがGraphicsTextureを作成するために使用するすべての情報が含まれています。(`D3D12_RESOURCE_DESC`。RTDescにも似ている)
      - Resource Desc (↑↑参照)
        - `TextureDimension dimension`: >Textureの**次元数** (Read-Only)
        - `GraphicsFormat format`: ピクセルの**型**
        - `width`: >Unityが**GPUにアップロード**するときのGraphicsTextureの`幅`をピクセル単位で指定します。
        - `height`: >Unityが**GPUにアップロード**するときのGraphicsTextureの`高さ`をピクセル単位で指定します。
        - `int mipCount`: >**MipMapレベル数**
      - `int depth`: `TextureDimension.Tex3D`の時の深さ (多分テクスチャ枚数)
      - `int arrayLength`: `TextureDimension.`⟪`Tex2D`¦`Cube`⟫`Array`の時の要素数 (CubeはCube数 * 6 ?)
      - `int numSamples`: **MSAA**のサンプル数
        - 2以上でMSAA有効。`GraphicsTextureDescriptorFlags.RenderTarget`を設定する必要がある。`GraphicsTextureDescriptorFlags.RandomWriteTarget`と互換性は無い
      - `enum` **GraphicsTextureDescriptorFlags** `flags`: >`GraphicsTexture`の**レンダリング**および**読み取り/書き込みアクセス** モード。
        - `None`: >デフォルト。この`GraphicsTexture`からサンプリングできる。(`class Texture2D`,`SRV`)
        - `RenderTarget`: >この`GraphicsTexture`を**レンダーターゲット**として**設定**し、レンダリングできるようにします。(`class RenderTexture`,`RTV`)
        - `RandomWriteTarget`: `ShaderModel5.0`のシェーダーで、この`GraphicsTexture`への**ランダム書き込みアクセス**を**許可**します。(`class RenderTexture.enableRandomWrite`,`UAV`)
    - `enum GraphicsTextureState state`: >`GraphicsTexture` の**現在の状態**。レンダリング スレッドで構築、初期化、または破棄される `GraphicsTexture` の状態を記述します。
      (↓は上から順に実行される感じがするがよく分からない(.png => Texture.ctor => 作成キュー => .pngをDXT等に変換＆作成完了 => 破棄キュー => 破棄 かな?))
      - `Constructed`: >`GraphicsTexture` **コンストラクタ**(.ctor?)が実行を開始しました。
      - `Initializing`: >ディスクリプタは初期化され、`GraphicsTexture`を**作成する作業**はレンダースレッドで**キューに入れられた**。
      - `InitializedOnRenderThread`: >`GraphicsTexture`がレンダースレッド上で**作成を完了**した。
      - `DestroyQueued`: >`GraphicsTexture`はレンダリングスレッドで**破壊のキュー**に入っているが、まだ完了していない。
      - `Destroyed`: >`GraphicsTexture`がレンダースレッド上で**完全に破壊**された。
- **その他情報**
  - `bool isReadable`: `Texture2D.GetPixels(..)`など**CPU上で操作**する場合にtrueである必要がある(GPU上の操作は関係ない(新API))
    - インポートのデフォルトでは`false`。スクリプトからでは`true`。`false`にするには`Texture2D.Apply(.., makeNoLongerReadable:false)`をする
  - `bool isDataSRGB`: >テクスチャが**sRGB色空間**の場合、`true` (Read-Only) (`GraphicsFormat.～_SRGB`になって描画時、ガンマ<=>リニア変換されると思われる)
  - `uint updateCount`: >このカウンターはTextureが**更新**されると**インクリメント**される。
    - >GPU 側から更新を実行する場合は、カウンターを自分で増分する必要があります。(たとえば、 RenderTexture にブリットする場合)。(IncrementUpdateCount を参照)。
  - `Hash128 imageContentsHash`: >Textureのハッシュ値。(Unityがテクスチャを変更するときハッシュも更新される) (Editor-Only)

## Instance関数

- `IntPtr GetNativeTexturePtr()`: [GetNative～Ptr()はR_Resource](\..\..\Unityいろいろ\グラフィックス\レンダーパイプライン\GetNative～Ptr()はR_Resource.png)
  (Draw.io/RenderTexture/bindings/`GetNativeDepthBufferPtr()`のColor版だと思われる)
- `IncrementUpdateCount()`: `uint updateCount`をインクリメントする。
