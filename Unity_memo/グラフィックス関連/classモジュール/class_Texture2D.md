# Texture2D (Texture継承)

主に、`.ctor`, `TextureFormat format`,
      `Apply(..)`(｡`CPUテクスチャ`(`Texture.isReadable==True`) => `GPUテクスチャ`｡), `ReadPixels(..)`, `⟪Get¦Set⟫Pixels(..)`, `⟪Get¦Set⟫PixelData<T>(..)`

## Static変数

- `⟪black¦＠❰linear❱gray¦normal¦red¦white⟫Texture`:
  - >すべて｢各色｣のピクセルの小さなテクスチャを取得します。

## Static関数

- `static Texture2D CreateExternalTexture(int width, int height, TextureFormat format, bool mipChain, bool linear, IntPtr nativeTex)`:
  - **Unity外部**で作成されたネイティブテクスチャオブジェクトからUnityの`GPU?テクスチャ`を作成します。(`UpdateExternalTexture`も参照)
  - width,heightなどの`Resource Descパラメータ`は`IntPtr nativeTex`のリソースと**一致**している必要がある。(`nativeTex`からコピーするってこと?)
  - `nativeTex`は、各グラフィックAPIの`ID3D12Texture2D*`などを渡す。(`IntPtr Texture.GetNativeTexturePtr()`も同じ種類のポインタ)
- `bool GenerateAtlas(Vector2[] sizes, int padding, int atlasSize, List<Rect> results)`: (`.PackTextures(..)`と似ている)
  - スプライト群（`sizes` 配列）を、パディング（`padding`）分を空けて、正方形のアトラス（`atlasSize`）のテクスチャ内に詰める。(収まらなかったら`false`)

## .ctor

- `.ctor(int width, int height, TextureFormat textureFormat= TextureFormat.RGBA32, ⟪int mipCount= -1¦bool mipChain= true⟫,`
    `bool linear= false, bool createUninitialized= false, MipmapLimitDescriptor mipmapLimitDescriptor)`:
  - 幅, 高さ, フォーマット, ⟪MipMap数(`mipCount`)¦全てのMipMap?(`mipChain`)⟫
    linearかsRGB(`linear`), 未初期化で効率上げるか(`createUninitialized`), MipMapのリミット?多分`mipmapLimitGroup`?(`mipmapLimitDescriptor`)
  - 多分、`CPUテクスチャ`も生成する(`Texture.isReadable`)

## Instance変数

- `TextureFormat format`: `TextureFormat`なので**ディスクに保存**するためのフォーマットを**指定**する? (`CPUテクスチャ`のフォーマットでもある)
- **TextureImporter**
  - `bool alphaIsTransparency`: >このテクスチャが`TextureImporter.alphaIsTransparency`を**有効**にして**インポート**されたかどうかを示します。(Editor-Only)
    - [多分テクスチャを加工して白いノイズを無くすらしい](https://nakamura001.hatenablog.com/entry/20130722/1374499269)
  - `bool vtOnly`: >このテクスチャが`TextureImporter.vtOnly`を**有効**にして**インポート**されたかどうかを示します。(Editor-Only)
    - > `Virtual Texturing` 用の Texture Stack と組み合わせてのみ使用できます(スクリプトからアクセス不可)
- **MipMapストリーミング**
  - `bool streamingMipmaps`: >この Texture に対して**ミップマップストリーミング**が**有効かどうか**を決定します。(QualitySettingsとの論理積っぽい)
  - `int streamingMipmapsPriority`: >メモリバジェット内に収まるようにメモリサイズを減らすときに、このテクスチャの**相対的な優先順位**を**設定**します。
  - `int requestedMipmapLevel`: >ロードするミップマップレベル。ミップマップレベルが**強制的にロード**され、**ミップマップストリーミングシステム**を**上書き**します。
  - `int minimumMipmapLevel`: >ミップマップストリーミングシステムをこの Texture の**最小ミップレベル**を**制限**する。
  - `bool ignoreMipmapLimit`: >このプロパティは、テクスチャがすべてのテクスチャの**ミップマップ制限設定**を**無視**するようにします。
  - `string mipmapLimitGroup`: >このテクスチャが関連付けられているテクスチャの**ミップマップ制限グループ**の**名前**。 (Read-Only)
  - `int desiredMipmapLevel`: >`メモリバジェット`が**適用される前**にストリーミングシステムがロードするミップマップレベル。
  - `int activeMipmapLimit`: >Unityが**GPUにアップロードしない**、テクスチャからの高解像度**ミップマップレベルの数**。 (Read-Only)
  - `int loadedMipmapLevel`: >ストリーミングシステムが**現在ロード**している**ミップマップレベル**。
  - `int loadingMipmapLevel`: >ミップマップ・ストリーミング・システムが**ロード中**の**ミップマップレベル。**
  - `int calculatedMipmapLevel`: >ストリーミングシステムによって計算されたミップマップレベルで、ストリーミングカメラとこのTextureを含むオブジェクトの位置を考慮する。?
    - これは `requestedMipmapLevel` や `minimumMipmapLevel` には影響されない。

## Instance関数

- **操作**
  - `Apply(bool updateMipmaps = true, bool makeNoLongerReadable = false)`: >`CPUテクスチャ`に`SetPixel系`で加えた**変更**を`GPUテクスチャ`に**コピー**します。
    - `bool updateMipmaps`: `true`の場合、`MipMapLv`:`0`を元に全ての`MipMapLv`を**再生成**する
    - `bool makeNoLongerReadable`: `true`の場合、`CPUテクスチャ`(メインメモリ)を削除して、`Texture.isReadable`を`false`にする。
      - (↑をした場合`ディスク`から**再ロード**は**できない**(**ディスク** `Import`?=> **CPUテクスチャ** <=`ReadPixels`? `Apply`=> **GPUテクスチャ**))
  - `bool Reinitialize(int width, int height, ⟪GraphicsFormat¦TextureFormat⟫ format, bool hasMipMap)`:
    - 引数の指定で`CPUテクスチャ`を**作り直し＆0クリア**し、`GPUテクスチャ`は`Apply(..)`時に作り直される?
    - 戻り値boolは成功したか。`{TextureName}_`⟪`TexelSize`¦`HDR`⟫は自動的に更新しない
  - `Compress(bool highQuality)`: >**実行時**に`CPU?テクスチャ`を`DXT/BCn`または`ETC`フォーマットに**圧縮**します。
    - `highQuality`: `ETC`以外で、**ディザアーティファクト**が掛かるのを**減らす**が、遅くなる
    - Unityがテクスチャを**圧縮するフォーマット**は、`プラットフォーム`と`テクスチャのプロパティ`に**依存**する
    - 既に圧縮されている場合は何もしない。**エディターで操作**する場合は、`EditorUtility.CompressTexture(..)`もある
  - `Rect[] PackTextures(Texture2D[] textures, int padding, int maximumAtlasSize, bool makeNoLongerReadable)`:
    - >`Atlasテクスチャ`に`複数のテクスチャ`を**パック**します(<https://youtu.be/7tjycAEMJNg?t=2746>)
    - 戻り値Rect[]は、`Atlasテクスチャ`内の`スプライト群`のUV座標を表す
    - `textures`: パックする`スプライト群`。`padding`: テクスチャの間隔。`makeNoLongerReadable`: `Apply(..)`と同じ
    - `maximumAtlasSize`: `Atlasテクスチャ`の**最大サイズ**を指定する。`textures`が収まらない場合は縮小される
    - `Atlasテクスチャ`が圧縮されるか?は複雑そうな条件がある[リファレンス参照](https://docs.unity3d.com/ja/2023.2/ScriptReference/Texture2D.PackTextures.html)
  - `UpdateExternalTexture(IntPtr nativeTex)`: >異なるネイティブのテクスチャオブジェクトを使うために Unity のテクスチャを更新します。
    - **Unity外部**(ネイティブコードプラグイン)のテクスチャの`IntPtr`を渡し、そこへUnityの`GPU?テクスチャ`を**コピー**して更新する?(`CreateExternalTexture`も参照)
- **⟪Get¦Set⟫Pixels** (`Color型`: `Color＠❰32❱`, `CPUテクスチャ`: `TextureFormat`)
  - **CPUテクスチャ <= GPUのRT**
    - `ReadPixels(Rect source, int destX, int destY, bool recalculateMipMaps= true)`:
      - 現在の**レンダーターゲット**(`BRTT.CameraTarget`,`RenderTexture.active`,`GraphicsTexture.active`)
        から、`Rect source`の範囲をこの`CPUテクスチャ`の`int dest⟪X¦Y⟫`にコピーする。`GPUテクスチャ`を更新するために`Apply(..)`を呼ぶ必要がある
        - この`TextureFormat format`を`⟪RT¦GT⟫.active`の`GraphicsFormat Texture.graphicsTexture.descriptor.format`と合わせる必要がある?自動変換される?
            >RTのフォーマットが自動的に変換され、CPU側で適切な `TextureFormat` にマッピングされます。
      - この関数はGPU処理が完了するまで待機するので**遅い**。代わりに`AsyncGPUReadback.RequestIntoNativeArray(..)`で**非同期にコピー**する(<>)
      - `recalculateMipMaps`で、`MipMap`を生成することもできるが、`Apply(bool updateMipmaps)`で`GPUテクスチャ`のみ?に生成することもできる
  - **Color型 <=Get Set=> CPUテクスチャ**
    - `⟪『Set』void¦『Get』Color＠❰＠❰32❱[]❱⟫ ⟪Get¦Set⟫Pixel＠❰s＠❰32❱❱(｡＠❰『✖❰s❱』int x, int y,❱＠❰『Set』⟪Color color,¦Color＠❰32❱[] colors,⟫❱ int miplevel= 0｡)`
      - **Get**:`CPUテクスチャ`(isReadable==true)のピクセル＠❰範囲❱を＠❰解凍し❱`Color＠❰32❱`に変換して取得する
        - 左下隅が`(0,0)`、範囲外の座標は`TextureWrapMode`に従う (`int x, int y`は解像度)
          - `Pixels`の場合は、`CPUテクスチャ`の左下から行ごとにピクセルが格納される (>配列のサイズは、`miplevel`の 幅 × 高さ)
      - **Set**:`Get`のほぼ**逆方向**と思われる。`GPUテクスチャ`を更新するために`Apply(..)`を呼ぶ必要がある
    - `Color GetPixelBilinear(float u, float v, int mipLevel= 0)`:
      - >`正規化座標❰0～1❱ (u, v)` で**バイリニアフィルタリング**されたピクセルの色を**取得**します。(テクスチャサンプリングのような)
      - 後は`GetPixel＠❰s＠❰32❱❱(..)`と大体同じと思われる
    - **生NativeArray版**: **圧縮**(`TF.BC7`など)されている可能性があるので、解凍処理は手動で行う必要がある。`GPUテクスチャ`を更新するために`Apply(..)`を呼ぶ必要がある
      - `NativeArray<T> GetPixelData<T>(int mipLevel)`
        - `CPUテクスチャ`の`mipLevel`のバッファを指す`NativeArray<T>`を取得してデータを**直接、読み書き**できる
        - `CPUテクスチャ`が、`16x16ピクセル`,`TF.RGBA32`,`Texture.mipmapCount:3`だとして、`GetPixelData(1)`で取得する場合
          `NativeArray<T>`は、`8x8ピクセル`=`64要素の配列`で`<T>`は`TF.RGBA32`と同じ型(`Color32`など)にする必要がある
      - `void SetPixelData<T>(NativeArray<T> data, int mipLevel, int sourceDataStartIndex = 0)`
        - `CPUテクスチャ`の`mipLevel`のバッファを`data`配列の`sourceDataStartIndex`～`sourceDataStartIndex`+`mipLevel`のバッファサイズ の要素で**コピー**する
      - `⟪『Load』void¦『Get』NativeArray<T>⟫ ⟪Get¦Load⟫RawTextureData<T>(＠❰『Load』NativeArray<T> data❱)`:
        - `⟪Get¦Set⟫GetPixelData<T>(..)`の全ての`mipLevel`含んでいる版
- **MipMapストリーミング**
  - `ClearMinimumMipmapLevel()`: >`minimumMipmapLevel`フィールドをリセットする。
  - `ClearRequestedMipmapLevel()`: >`requestedMipmapLevel`フィールドをリセットする。
  - `bool IsRequestedMipmapLevelLoaded()`: >`requestedMipmapLevel`によって設定されたミップマップ・レベルのロードが終了したかどうかを確認します。
