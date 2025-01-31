# MeshFilter と Mesh

## MeshFilter (Component継承)

### Instance変数

- `Mesh mesh`:
  - >新しい`Mesh`か既存の`Mesh`の**複製**を返し、それを`MeshFilter`に割り当てます。
  - 恐らく、`class Renderer`の`Material material`プロパティと**同じ挙動**と思われる。**複製の仕方**は`Instantiate(mesh)`みたい
- `Mesh sharedMesh`
  - >`MeshFilter`に割り当てられた共有Meshを返します
  - 恐らく、`class Renderer`の`Material sharedMaterial`プロパティと**同じ挙動**と思われる。

## Mesh (UnityObject継承)

### Static関数

- **C# Job System**による`Mesh(Data)`の処理
  大体、
  `struct C#_Job_System{[Read] AcquireReadOnlyMeshData(meshes), [Write] writable_MeshDataArray = AllocateWritableMeshData(meshCount)};`
      `ApplyAndDisposeWritableMeshData(writable_MeshDataArray, Mesh[] meshes); return meshes;`
  みたいな感じ。
  - `Mesh.MeshDataArray AcquireReadOnlyMeshData(⟪Mesh＠❰[]❱¦List<Mesh>⟫ meshes)`: (`meshes`は`isReadable==true`であること) (`Acquire(アクワイア)`: 獲得する)
    - `Mesh[] meshes`から**スナップショット**(`⟪Vertex¦Index⟫バッファ`のコピー?)を取り、`ReadOnly`の`struct Mesh.MeshData`の`struct Mesh.MeshDataArray`を作る
    - **C# Job System**でマルチスレッドで回すことができる。`Mesh.MeshDataArray`は`.Dispose()`を使用して**破棄**する必要がある。
  - `Mesh.MeshDataArray AllocateWritableMeshData(int meshCount)`:
    - `meshCount`分の`Writable`の`struct Mesh.MeshData`の`struct Mesh.MeshDataArray`を作る
    - **C# Job System**の出力用データの`Allocate`と思われる
  - `ApplyAndDisposeWritableMeshData(Mesh.MeshDataArray data, ⟪Mesh＠❰[]❱¦List<Mesh>⟫ meshes, MeshUpdateFlags flags)`:
    - ↑の`AllocateWritableMeshData(..)`で作成した`Writable`な`Mesh.MeshDataArray`の`data`を`Mesh[] meshes`に`Apply`し、その後`data`を`.dispose()`して**破棄**する

### .ctor

- `Mesh()`
  - >空の`Mesh`を作成します。(`⟪Vertex¦Index⟫バッファ`も無い?)

### Instance変数

- **Vertex と Index バッファ**
  - `int vertexCount`: >`Mesh`の**頂点数** (Read-Only)
  - `int subMeshCount`:
    - >`Mesh`内の`SubMesh`の数。(`SubMesh`は、`Renderer`にセットされた＠❰複数の❱`Material`に対応する)
    - `この値`を**以前より小さい値に変更**すると、`IndexBufferData`?❰triangles?❱のサイズが**小さくなる**
  - **旧API**
    - **Vertex Attribute**
      各Attribute(`∮:Semantics∮`)の**Get/Set**。Attributeが無い場合は空配列
      >`Attribute`はデータの**コピーを返す(Allocする)** (コピー(Get) => 編集 => 代入(Set))
      - `Color＠❰32❱[] colors＠❰32❱`: `∮:Semantics∮`の`COLOR`
      - `Vector3[] normals`:         `∮:Semantics∮`の`NORMAL`
      - `Vector4[] tangents`:        `∮:Semantics∮`の`TANGENT`
      - `Vector2[] uv＠⟪2～8⟫`:      `∮:Semantics∮`の`TEXCOORD⟪0～7⟫`
      - `BoneWeight[] boneWeights`   `∮:Semantics∮`は分からない。
        - `struct BoneWeight`: `boneIndex⟪0～3⟫`, `weight⟪0～3⟫` (`skinWeightBufferLayout`で`⟪0～3⟫`の数を指定?)
      - `Vector3[] vertices`:        `∮:Semantics∮`の`POSITION`
        - (**配列サイズを変更**すると**全てのAttribute**も**リサイズ**される。初めてSetするとき`RecalculateBounds`が自動的に処理される)
    - **インデックス**
      - `int[] triangles`: `三角形リスト`なので**3の倍数**であること。全ての`SubMesh`に属する`三角形リスト`が**含まれる**
  - **Advanced Mesh API (新API) と その他**
    - **Vertex**
      - `int vertexAttributeCount`: この`Mesh`の`Attribute`の**数** (Read-Only)
        - `mesh.SetVertexBufferParams(..)`の引数内で指定する、 `VertexAttributeDescriptor(..)`の**数**
        - `GetVertexAttribute(⟪0～vertexAttributeCount-1⟫)`で`VertexAttributeDescriptor`を**取得**できる
      - `int vertexBufferCount`: **Vertexバッファの数**(`VertexAttributeDescriptor.stream`数)
      - `GraphicsBuffer.Target vertexBufferTarget`: **Vertexバッファの使用目的**を設定。(`stream`毎に指定できない?) (デフォルトは`Vertex`)
        - `enum GraphicsBuffer.Target`: `ComputeShader`からアクセスしたい場合は`|= GraphicsBuffer.Target.Raw`する。など..
    - **Index**
      - `IndexFormat indexFormat`: `Indexバッファ`の`Format` (デフォルトは`Uint16`(`Uint32`をサポートしていないデバイスがあるため))
        - `enum IndexFormat`: ⟪`Uint16`¦`Uint32`⟫
      - `GraphicsBuffer.Target indexBufferTarget`: **Indexバッファの使用目的**を設定。(デフォルトは`Index`) (`vertexBufferTarget`と同じ感じ)
- **Mesh設定**
  - `bool isReadable`:
    - >`true`は、**読み"書き"可能**。(CPU側データ)(`Meshモデル`?をUnityに**インポート**するときは`Read/Write Enabledチェックボックス`を使う)
    - `Mesh`を操作する必要がない場合は`false`にして**高速化**する (多分、`Mesh`にデータを持たない)
- **Meshデータ**
  - `Bounds` **bounds**: **Local空間**の`Bounds`。(`Renderer.bounds`は**World空間**)
  - bindposes
    - `Matrix4x4[] bindposes`: >バインドポーズ。各Indexの`bindposes`は同じIndexのボーンを参照します。?
    - `int bindposeCount`: >`bindposes`の数。
  - `SkinWeights skinWeightBufferLayout`: `BoneWeightBuffer`内のデータレイアウト(1Vertexあたりの`Bone`数)。`boneWeights`?や`Mesh.GetBoneWeightBuffer(SkinWeights)`に関連
  - `int blendShapeCount`: >`Mesh`の `BlendShape`数を返します

### Instance関数

- **Clear**と**Upload**
  - **Clear**`(bool keepVertexLayout= true)`: 旧/新APIで呼ばれている(<https://youtu.be/u51C_sNZsyA?t=202>)
    - >全ての`Vertexデータ`と`Indexデータ`を**削除**します。(`isReadable == false`にはしないらしい)
    - `keepVertexLayout`: `SetVertexBufferParams(int vertexCount, VertexAttributeDescriptor[] attributes)`で設定される、
      **頂点レイアウト**(`attributes`)の**構成**を**維持**する (DirectX12 `D_INPUT_LAYOUT_DESC`)
    - [Mesh_ClearとisReadable](\..\..\Unityいろいろ\グラフィックス\レンダーパイプライン\Mesh_ClearとisReadable.png)
  - `UploadMeshData(bool markNoLongerReadable)`: こっちは即時でなければ呼ばなくてもいいみたい?
    - この`Mesh`で**変更した内容**を**即時**に**グラフィックAPI**(GPU?)に`Upload`する。(DirectX12 `R_Resource->Map(..)`して書き込み?)
    - `markNoLongerReadable`: `Mesh`のデータを**開放**する。(CPU側データを破棄し、`isReadable == false`にする)
      `mesh.Clear(..)`とは違いGPU側バッファ(`R_Resource`)は破棄しない

- **Mesh結合**
  - `CombineMeshes(CombineInstance[] combine, bool mergeSubMeshes= true, bool useMatrices= true, bool hasLightmapData= false)`:
    - >この`Mesh`に複数の`Mesh`を**結合**します
    - 今ではGPU Resident Drawer(のGPU draw submission modes)?では、**静的バッチ**すら**推奨されない**

- **Vertex Attribute**の**NoAlloc版**(コピーはする) (雑にまとめたから多少違うかも、`Triangles`と`Indices`とか)
  - `＄Attribute ＝ ⟪Colors¦Normals¦Tangents¦UVs¦Vertices¦Triangles¦Indices⟫`
    `Get∫Attribute∫(List<｢Type｣> value)`
    `Set∫Attribute∫(.., ⟪List<｢Type｣>¦NativeArray<｢Type｣>¦｢Type｣[]⟫, ..)`

- **⟪Vertex¦Index⟫バッファ**の**最適化**
  - `Optimize()`:
    - **GPUの処理時**、**キャッシュ率を向上**させるために、`Mesh`の`Indexバッファ`と`Vertexバッファ`を**最適化**する
    - 実際には`OptimizeIndexBuffers()` => `OptimizeReorderVertexBuffer()` を実行している
    - プロシージャルに生成した`Mesh`のみに使用し、`MeshAsset`の場合はインポート時`メッシュの最適化`が有効になっているとその時呼び出される
  - `OptimizeIndexBuffers()`:
    - `Indexバッファ`を**最適化**
  - `OptimizeReorderVertexBuffer()`:
    - `Vertexバッファ`を**最適化**

- **Mark**
  - `MarkDynamic()`:
    - `Mesh`を**毎フレーム?更新**する場合、これを呼ぶとパフォーマンスが向上する
    - 内部的はに**グラフィックAPI**で`Dynamic Buffer`を使用するようにする
  - `MarkModified()`:
    - `Set～(..)系`で`MeshUpdateFlags.DontNotifyMeshUsers`が**設定**された場合、`Renderer`への通知がスキップされるので、**これで通知**する

- **Set⟪Vertex¦Index⟫Buffer⟪Params¦Data⟫(..)**
  - **SetVertexBufferParams**`(int vertexCount, ⟪params ∫VAD∫[]¦NativeArray<∫VAD∫>⟫ attributes)『＄VAD＝❰VertexAttributeDescriptor❱`:
    - この`Mesh`の**Vertexバッファ[]を生成**するための、**頂点数**(`vertexCount`)と**頂点レイアウト**(`attributes`)を指定する
      `Attribute`は`Stream`内で`enum VertexAttribute`の定義順に**並べられる**らしい。(`attributes`の定義順ではない)
      `Attribute`の**アライメント**は**4バイト**単位。((format=VAF.Float16 * dimension=3) / 8 = 6バイト は、できない)
    - [struct **VertexAttributeDescriptor**](https://docs.unity3d.com/ja/2023.2/ScriptReference/Rendering.VertexAttributeDescriptor.html)
      - コンストラクタ
        - `.ctor(VertexAttribute attribute, VertexAttributeFormat format = .Float32, int dimension = 3, int stream = 0)`
      - フィールド (`format[dimension] val :attribute` と `stream`)
        - `VertexAttribute` **attribute**: `∮:Semantics∮`: ⟪`Position`¦`Normal`¦`Tangent`¦`Color`¦`TexCoord⟪0～7⟫`¦`Blend⟪Weight¦Indices⟫`⟫
        - `VertexAttributeFormat format`: `VertexAttribute`の**フォーマット**(デフォルトは`.Float32`)
          - `VertexAttributeFormat`: `⟪｡Float¦⟪U¦S⟫⟪Norm¦Int⟫｡⟫⟪8¦16¦32⟫`
        - `int dimension`: `VertexAttribute`の**次元数**(`1～4`。デフォルトは`3`)
        - `int` **stream**: `Vertexバッファ`,**インターリーブ** の **単位**。(Unityは4つまでサポートしている。デフォルト:`stream=0`)
  - `SetVertexBufferData(⟪｡⟪NativeArray¦List⟫<T>¦T[]｡⟫ data, int dataStart, int vertexBufferStart, int count, int stream, MeshUpdateFlags flags)`:
    - この`Mesh`の`Vertexバッファ`に`data`を設定します。(`dataStart`:`data`開始位置, `count`:`data`の要素数, `vertexBufferStart`:`Vertexバッファ`の開始位置)
  - `SetIndexBufferParams(int indexCount, IndexFormat format)`:
    - この`Mesh`の**Indexバッファを生成**するための、**Index数**(`indexCount`)と**IndexFormat**(`format`)を指定する
    - `enum IndexFormat`: ⟪`UInt16`¦`UInt32`⟫
  - `SetIndexBufferData(⟪｡⟪NativeArray¦List⟫<T>¦T[]｡⟫ data, int dataStart, int indexBufferStart, int count, MeshUpdateFlags flags)`:
    - この`Mesh`の`Indexバッファ`に`data`を設定します。(`dataStart`:`data`開始位置, `count`:`data`の要素数, `indexBufferStart`:`Indexバッファ`の開始位置)
  - 補足:`⟪｡⟪NativeArray¦List⟫<T>¦T[]｡⟫` **Get**`VertexBufferData(stream,..)`のようなGPU側から**stream**を取得する系は無い..
    `Set⟪Vertex¦Index⟫BufferData(⟪｡⟪NativeArray¦List⟫<T>¦T[]｡⟫ data,..)`は、`data`を**即時?にGPUに転送**(`R_Resource->Map(..)`)しているらしい。(新APIは`isReadable`関係なし)
    [.Dispose()している](https://youtu.be/u51C_sNZsyA?t=214), [SetVertexBufferData](\..\..\Unityいろいろ\グラフィックス\レンダーパイプライン\SetVertexBufferData.png)

- **SubMesh**系
  - **SetSubMesh＠❰es❱(..)**
    - **SetSubMesh**`(int subMeshIndex, SubMeshDescriptor desc, MeshUpdateFlags flags)`:
      - `subMeshIndex`に`desc`を設定して、`Indexバッファ`から`SubMesh`を**定義**する。`flags`は、`Mesh`が編集された時の**検証チェックの省略** や `descの要素`の**自動生成**
        (`subMeshIndex`は、`subMeshCount`以下であること)
      - `struct` **SubMeshDescriptor**: (**topology**と**indexStart**と**indexCount**だけ指定すればよい)
        - `MeshTopology topology`: この`SubMesh`の`MeshTopology`を指定
          - `enum MeshTopology`: ⟪`Triangles`¦`Quads`¦`Lines`¦`LineStrip`¦`Points`⟫
        - `Indexバッファ`の**範囲**指定
          - `int indexStart`: この`SubMesh`を構成する`Indexバッファ`の**開始位置**
          - `int indexCount`: この`SubMesh`を構成する`Indexバッファ`の**カウント**
        - `Vertexバッファ`の**範囲**指定 (通常は**自動計算**される)
          - `int firstVertex`: この`SubMesh`を構成する`Vertexバッファ`の**開始位置**
          - `int vertexCount`: この`SubMesh`を構成する`Vertexバッファ`の**カウント**
        - `Bounds bounds`: この`SubMesh`?を包む`Bounds` (通常は**自動計算**される)
        - `int baseVertex`: `Indexバッファ`の**要素**に**加算**し`Vertexバッファ`を**参照**する値。↓(DirectX12 `INT BaseVertexLocation`)
          - (`IndexFormat.Uint16`の時に`SubMesh`単位で`最大65535頂点`を使えるようにするため。グラフィックAPIでサポート)
      - `enum` **MeshUpdateFlags**:
        このFlagは、`Mesh`**編集時**に様々な**検証,処理,自動計算**を**スキップ**して**効率化**することが目的。(後でまとめて処理するとか)
        関数によって使うFlagが異なる。**設定可能な関数**は、`SetSubMesh(..), Set⟪Vertex¦Index⟫BufferData(..)`
        (フラグは論理和(`|`)で結合できる)
        - `Default`:
          - 様々な**検証,処理,自動計算**を**スキップしない**。
        - `DontValidateIndices`:
          - `mesh.SetIndexBufferData(..)`時、`Indexバッファ`が`Vertexバッファ`の**レンジ内のIndex**を指しているか検証する。事をしない。
        - `DontRecalculateBounds`:
          - `mesh.SetSubMesh(..)`時、`SubMeshDescriptor.⟪bounds¦firstVertex¦vertexCount⟫`を**自動計算**する。事をしない。
        - `DontNotifyMeshUsers`:
          - `SetSubMesh(..), Set⟪Vertex¦Index⟫BufferData(..)`時、`mesh.bounds`?の変更を`Renderer`に**通知しない**。後で`mesh.MarkModified`で通知できる
        - `DontResetBoneBounds`:
          - `Set⟪Vertex¦Index⟫BufferData(..)`時、`Skinned Mesh`?の`BoneBounds`(`Bone`による変形後の`Bounds`)?をリセットして再計算しない
    - `SetSubMeshes(⟪∫SMD∫[]¦⟪List¦NativeArray⟫<∫SMD∫>⟫ desc, ＠❰int start, int count,❱ MeshUpdateFlags flags)『＄SMD＝❰SubMeshDescriptor❱`:
      - ⟪`全て`¦`start`から`start`+`count`⟫の`desc[]`を設定して、`Indexバッファ`から`SubMesh`を**定義**する。
  - **GetSubMesh(..)** 系
    - `SubMeshDescriptor` **GetSubMesh**`(int subMeshIndex)`:
      - `subMeshIndex`から`SubMeshDescriptor`を**取得(Get)**する。(これがあれば↓要らないかも)
    - `MeshTopology GetTopology(int subMesh)`:
      - `SetSubMesh(int subMesh, SubMeshDescriptor desc, ..)`の`desc.topology`の値を**取得**する
    - `uint GetBaseVertex(int subMesh)`:
      - `SetSubMesh(int subMesh, SubMeshDescriptor desc, ..)`の`desc.baseVertex`の値を**取得**する
    - `subMesh`の`Indexバッファ`の**範囲**
      - `uint GetIndexStart (int subMesh)`: >`subMesh`の`Index`開始位置を**取得**します。(`desc.indexStart`?)
      - `uint GetIndexCount(int subMesh)`: >`subMesh`の`Index`数を**取得**します。(`desc.indexCount`?)

- **Attribute**の**情報取得**系
  - **VertexAttributeDescriptor**の**取得**
    - `VertexAttributeDescriptor GetVertexAttribute(int attributeIndex)`:
      - `attributeIndex`から`VertexAttributeDescriptor`を**取得**する
      - `attributeIndex`は、`⟪0～mesh.vertexAttributeCount-1⟫`の範囲
    - `VertexAttributeDescriptor[] GetVertexAttributes()`:
      - この`Mesh`に存在する全ての`Attribute`の`VertexAttributeDescriptor`を取得する。(NoAlloc版のリストのオーバーロードもある)
  - **Attribute**の**存在**
    - `bool HasVertexAttribute(VertexAttribute attribute)`:
      - この`Mesh`に`attribute`が**存在**する場合は`true`を返す
      - `VertexAttribute`: ⟪`Position`¦`Normal`¦`Tangent`¦`Color`¦`TexCoord⟪0～7⟫`¦`Blend⟪Weight¦Indices⟫`⟫
  - **Attribute**の**型** (`.format`, `.dimension`)
    - `VertexAttributeFormat GetVertexAttributeFormat(VertexAttribute attribute)`:
      - `attribute`の`VertexAttributeDescriptor.format`を**取得**する
    - `int GetVertexAttributeDimension(VertexAttribute attribute)`:
      - `attribute`の`VertexAttributeDescriptor.dimension`を**取得**する
  - **Attribute**の**位置特定**系 (`Vertexバッファ`内) (`stream`->`Stride`->`ByteOffset`)
    - `int GetVertexAttributeStream(VertexAttribute attribute)`:
      - `attribute`の`VertexAttributeDescriptor.stream`を**取得**する
    - `int GetVertexBufferStride(int stream)`:
      - `stream`内の`1頂点`の**バイトサイズ**(`Stride`)を**取得**する。[例参照](https://docs.unity3d.com/ja/2023.2/ScriptReference/Mesh.GetVertexBufferStride.html)
      - `Unity.drawio`/`ページ29`/`頂点バッファと頂点レイアウト`の**ストライド**だと思われる
    - `int GetVertexAttributeOffset(VertexAttribute attribute)`:
      - `attribute`の`stream`内の`1頂点内`の**バイトオフセット**を**取得**する。[例参照](https://docs.unity3d.com/ja/2023.2/ScriptReference/Mesh.GetVertexAttributeOffset.html)
      - `Unity.drawio`/`ページ29`/`頂点バッファと頂点レイアウト`の**Aligned Byte Offset**だと思われる

- **Stream**から**GraphicsBuffer**の**取得**
  - `GraphicsBuffer GetVertexBuffer(int stream)`:
    - `stream`から`Vertexバッファ`の`GraphicsBuffer`を**取得**する。[例:ComputeShaderへのSet](https://docs.unity3d.com/ja/2023.2/ScriptReference/Mesh.GetVertexBuffer.html)
  - `GraphicsBuffer GetIndexBuffer()`:
    - `Indexバッファ`の`GraphicsBuffer`を**取得**する。(ComputeShaderへのSetは↑`GetVertexBuffer`と同じ)

- `⟪Vertex¦Index⟫バッファ`の**IntPtr**: [GetNative～Ptr()はR_Resource](\..\..\Unityいろいろ\グラフィックス\レンダーパイプライン\GetNative～Ptr()はR_Resource.png)
  - `IntPtr GetNativeIndexBufferPtr()`:
    - `Indexバッファ`を**ネイティブプラグインから編集**するための`IntPtr`を**取得**する
  - `IntPtr GetNativeVertexBufferPtr(int stream)`:
    - `stream`の`Vertexバッファ`を**ネイティブプラグインから編集**するための`IntPtr`を**取得**する

- **Recalculate**(再計算)
  - `RecalculateBounds(MeshUpdateFlags flags = .Default)`:
    - >`Vertexバッファ`を使って`Mesh`とその全ての`SubMesh`の`Bounds`を**再計算**します。(ちょっと再計算できる条件があるみたい)
  - `RecalculateNormals(MeshUpdateFlags flags = .Default)`:
    - >三角形と頂点から`Mesh`の`Normal`を**再計算**します。
    - (`SetVertexBufferParams(..)`で**独自に定義**された`頂点レイアウト`で正しく設定されるかは分からない)
  - `RecalculateTangents(MeshUpdateFlags flags = .Default)`:
    - `Normal`と`UV座標`(.TexCoord0?)から`Mesh`の`Tangent`を**再計算**します。

- UVDistributionMetric
  - `float GetUVDistributionMetric(int uvSetIndex)`:
    - `uvSetIndex`は、`VertexAttribute.TexCoord⟪0～7⟫`の`⟪0～7⟫`?。`(三角形の面積 / UVの面積)の平均`を返す。
    - [必要なMipMapLvを計算に使えるらしい](https://docs.unity3d.com/ja/2023.2/ScriptReference/Mesh.GetUVDistributionMetric.html)
  - `RecalculateUVDistributionMetric(int uvSetIndex, float uvAreaThreshold)`:
    - 頂点とUV座標からMeshのUV分布メトリックを**再計算**します。`uvAreaThreshold`:>考慮すべきUV領域の最小値。(デフォルトは`1e-9f`)
  - `RecalculateUVDistributionMetrics(float uvAreaThreshold)`
    - 全ての?頂点とUV座標からMeshのUV分布メトリックを**再計算**します。

- **Bone系**
  - `NativeArray<byte> GetBonesPerVertex()`
    - >各頂点の非ゼロボーンウェイトの数。(配列は頂点インデックス順にソート)
  - `GraphicsBuffer GetBoneWeightBuffer(SkinWeights layout)`:
    - >GPUボーンウェイトデータへの直接の読み書きアクセスを提供するGraphicsBufferを取得します。
  - `NativeArray<BoneWeight1> GetAllBoneWeights()`:
    - >Meshのボーンウェイトを取得します。
  - `GetBoneWeights(List<BoneWeight> boneWeights)`:
    - >Meshのボーンウェイトを取得します。
  - `SetBoneWeights(NativeArray<byte> bonesPerVertex, NativeArray<BoneWeight1> weights)`:
    - >Meshのボーンウェイトを設定します。
  - `NativeArray<Matrix4x4> GetBindposes()`:
    - メッシュのバインドポーズを取得します。
  - `SetBindposes(NativeArray<Matrix4x4> poses)`:
    - Meshのバインドポーズを設定します。

- **BlendShape**
  - `ClearBlendShapes()`:
    - >メッシュからすべてのブレンドシェイプをクリアします
  - `BlendShapeBufferRange GetBlendShapeBufferRange(int blendShapeIndex)`:
    - >指定されたブレンドシェイプの頂点データの位置を取得する。
  - `GraphicsBuffer GetBlendShapeBuffer(＠❰BlendShapeBufferLayout layout❱)`:
    - >GPUブレンドシェイプ頂点データへの直接読み書きアクセスを提供するGraphicsBufferを取得します。
  - `AddBlendShapeFrame(string shapeName, float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)`:
    - >新しいブレンドシェイプのフレームを追加します
  - `int GetBlendShapeFrameCount(int shapeIndex)`:
    - >ブレンドシェイプのフレーム数を返します
  - `GetBlendShapeFrameVertices(int shapeIndex, int frameIndex, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)`:
    - >ブレンドシェイプフレームの deltaTangents、deltaTangents、deltaTangents を取得します
  - `float GetBlendShapeFrameWeight (int shapeIndex, int frameIndex)`:
    - >ブレンドシェイプフレームの重みを返します
  - `int GetBlendShapeIndex(string blendShapeName)`:
    - >指定インデックスの BlendShape のインデックスを返します。
  - `string GetBlendShapeName (int shapeIndex)`:
    - >指定インデックスの BlendShape 名を返します
