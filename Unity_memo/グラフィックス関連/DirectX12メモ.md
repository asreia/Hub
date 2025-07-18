# DirectX12メモ

- フォーマット
  - `＄R_＝❰ID3D12❱`『特別に`R_`は`∫R_∫`とする。(`R_`は**Resource**の意味)
  - `＄I_＝❰IDXGI❱`『特別に`I_`は`∫I_∫`とする。(`I_`は**Infrastructure**の意味)
  - `＄D_＝❰D3D12_❱`『特別に`D_`は`∫D_∫`とする。(`D_`は**Descriptor**の意味)
  - 基本的に`⟪R_Device¦I_Factory4⟫->関数名(型名, 型名, .., out 型名)`の形式

- ビュー
  - **頂点リソース**
    - `VBV`: アトリビュート、セマンティック
    - `IBV`: インデックス
  - **シェーダーリソース**: `DescriptorTable`と`R_DescriptorHeap`の組み合わせでセット
    - `CBV`: Bufferに特化して**高速**
    - `SRV`: **読み取り専用**
    - `UAV`: **読み書き(RW)**
    - `Sampler`: サンプラー
  - **ターゲット**: `R_GraphicsCommandList->OMSetRenderTarget(..)`でセット
    - `RTV`: カラーターゲット
    - `DSV`: デプス,ステンシル

- 考察メモ
  - 効率的な描画: ((((`InstancingDraw`) => `SubMesh`) => `VBV,IBV`) => `Shader`) => `RT` (右に行くほど**切り替えコスト**が高い)
    - "=>"は優先順位で、左辺で描画するモノがなくなったら右辺を切り替えて描画する
  - CPU,GPU並列処理
    1. Fence
    2. Present (スワップ)
    3. _cmdQueue->ExecuteCommandLists
  - `SetGraphicsRoot⟪CB¦SR¦UA⟫View`でセットする場合は、`R_Resource`のある範囲(View)ではなく、`R_Resource`の**全範囲がバインド**される
  - **Buffer**は**シェーダー側**で**型を定義**するが、**Texture**は**DXGI_FORMAT**で**型を定義**
  - `Texture2D<float4>`は`DXGI_FORMAT`の型が`float4`にキャストされると考えて良い？  (変換順:`R8G8B8A8` => `UNORM` => `<float4>`かな。代入(`float4 = Texture2D`)と何が違う?)
    GPT:シェーダー側で `float4` にキャストされるのは正しい認識ですが、その際に `DXGI_FORMAT` に応じた適切な変換が行われる
    - 例 1: `DXGI_FORMAT_R8G8B8A8_UNORM`
      - フォーマット: `8 ビット`の `RGBA 値`（各チャネルは `0～255 の整数値`）を格納します。
      - シェーダー側での解釈: `Texture2D<float4>` でアクセスすると、各チャネルは `0.0f ～ 1.0f` の範囲の `float4` に正規化されます（`0` が `0.0f`、`255` が `1.0f`）。
    - 例 2: `DXGI_FORMAT_R32G32B32A32_FLOAT`
      - フォーマット: 各チャネルに `32 ビット`の浮動小数点値を直接格納します。
      - シェーダー側での解釈: `Texture2D<float4>` でアクセスすると、格納されている浮動小数点値がそのまま `float4` としてシェーダーに返されます。
    - 例 3: `DXGI_FORMAT_R16G16B16A16_SNORM`
      - フォーマット: `16 ビット`の符号付き正規化整数値を格納します（`-32768 ～ 32767` の範囲）。
      - シェーダー側での解釈: `Texture2D<float4>` でアクセスすると、各チャネルが `-1.0f ～ 1.0f` の範囲の `float4` に正規化されます。
  - `UINT SubResource`: (TextureArrayインデックス * MipMapLv数) + MipMapLvインデックス

## DirectXコンテキスト

### I_Factory4

- ☆**CreateDXGIFactory2**`(out I_Factory4)`:
  - `I_Factory4`: `R_Device`(Direct3D)よりもハードウェアに近い**低レベル**なオブジェクトを生成する
- ☆`I_Factory4->`**EnumAdapters**`(UINT, out I_Adapter)`: `I_Adapter`の取得
  - `UINT`: 刺さってる グラフィックボード のインデックスと思われる
- ☆`I_Factory4->`**CreateSwapChainForHwnd**`(..)`: スワップチェイン生成(`## I_SwapChain4`参照)

### R_Device

- ☆**D3D12CreateDevice**`(I_Adapter, D3D_FEATURE_LEVEL, out R_Device)`:
  - `I_Adapter`: グラフィックボード
  - **enum D3D_FEATURE_LEVEL**: `D3D_FEATURE_LEVEL～`:
    - `_⟪9～12⟫_⟪0～3⟫`: DirectXのバージョン
  - `out R_Device`: `I_Adapter`と`D3D_FEATURE_LEVEL`から生成された**DirectXのデバイス**

## リソース系 (Unity.drawio/ページ35)

### リソース

#### I_SwapChain4

- ☆`I_Factory4->`**CreateSwapChainForHwnd**`(*IUnknown pDevice, HWND, DXGI_SWAP_CHAIN_DESC1,.. out I_SwapChain4)`:
  `R_Device`と`HWND`を関連付けて`I_SwapChain4`を**作成**
  - ***IUnknown pDevice**: 本来は`R_Device`だが、**DirectX11との互換性**のために`R_CommandQueue`に設定しているらしい
  - **HWND**: Windowsのウインドウのハンドル?(`HWND CreateWindow(..)`)
  - **struct DXGI_SWAP_CHAIN_DESC1**:
    - `UINT ⟪Width¦Height⟫`
    - **struct DXGI_SAMPLE_DESC** `SampleDesc`:
      - `UINT Count`: **サンプル点**の数。`⟪1～32⟫`。(多くのハードウェアは**2の累乗に最適化**され`MSAAx8`までサポート)
      - `UINT Quality`: その`Count`数についての**品質レベル**。(通常は`0`が設定され、品質は**ハードウェア依存**)
    - `UINT BufferCount`: 通常は**バックバッファー**と**フロントバッファー**で`2`が設定される
    - `DXGI_FORMAT Format`: `DXGI_FORMAT_R8G8B8A8_UNORM✖❰_SRGB❱`『`_SRGB`を付けないのは、**シェーダーガンマ**してディスプレイに送ってディスプレイ逆ガンマするみたい
      - `DXGI_FORMAT_⟪R16G16B16A16_FLOAT¦『HDR10?』R10G10B10A2_UNORM⟫`は**HDRディスプレイ**
    - `BOOL Stereo`: `TRUE`にすると**XR用**に**バッファ数が2倍**になる
    - その他: `DXGI_USAGE BufferUsage; DXGI_SCALING Scaling; DXGI_SWAP_EFFECT SwapEffect; DXGI_ALPHA_MODE AlphaMode; UINT Flags;`
  - その他の引数には、**フルスクリーンモード**や**ディスプレイの指定**ができる
  - **マルチディスプレイ**する場合は`I_SwapChain4`と`HWND`をディスプレイ毎に生成する
- ☆`I_SwapChain4->`**Present**`(UINT SyncInterval, UINT Flags)`:
  呼び出し時に`GetCurrentBackBufferIndex()`を**更新**し、スワップ時に`BackBuffer`と`FrontBuffer`を**スワップ**しディスプレイが`新しいFrontBuffer`を表示する
  - `UINT SyncInterval`: 待つ**VSyncの数**。`0`は`VSync`なしで**ティアリングが発生**する可能性がある
  - `UINT Flags`: `DXGI_PRESENT～`:
    - `_ALLOW_TEARING`: `SyncInterval`が`0`の時、**ティアリングを許容**するためのフラグであり、指定しない場合はティアリングを抑制することがあるが完全に防ぐものではない
    - `_DO_NOT_WAIT`: 空いているバックバッファが無い場合でも**待機しない**(CPUをブロックしない)
- ☆`I_SwapChain4->`**GetCurrentBackBufferIndex**`()`:
  現在の描画対象の`BackBuffer`の`Index`を取得する
- ☆`I_SwapChain4->`**GetBuffer**`(UINT, out R_Resource)`:
  `I_SwapChain4`の**バッファー取得**
  - `I_SwapChain4`から`UINT`番目(`BufferCount`未満)の`R_Resource`を取得

#### R_Resource

- **アライメント**
  - **リソース**: Alignment (`D_⟪DEFAULT＠❰MSAA❱¦SMALL⟫_RESOURCE_PLACEMENT_ALIGNMENT`)
    `4MB`: **MSAA**
    `64KB`: テクスチャ(一般)
    `4KB`: CreatePlacedResource(..)を使った**小さいテクスチャ(64KB以下)**
    `4KB`: バッファ (`D_RESOURCE_DIMENSION_BUFFER`)
    - **バッファ** (1リソース = 複数バッファ 可能)
      - **Vertexバッファ(VBV)**
        - `任意Byte`(Address)
          - `任意Byte`(struct): (`IASetVertexBuffers(..)`: `StrideInBytes`)
            - `4Byte`(member): (`DXGI_FORMAT`) (class_Mesh.md/G:126 を参照)
      - **Indexバッファ(IBV)**
        - `⟪2¦4⟫Byte`(Address)
          - `⟪2¦4⟫Byte`(member): (`DXGI_FORMAT_R⟪32¦16⟫_UINT`)
      - **Constantバッファ(CBV)**
        - `256Byte`(Address): (`D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT`)
          - `256Byte`(struct): cbuffer
            - `2～16Byte`(member): **cbufferアライメントルール**
      - **Structuredバッファ(SRV,UAV)**
        - `16Byte`(Address): (自動適用?)
          - `16Byte`(struct): (`D_BUFFER_⟪SR¦UA¦RT⟫V`: `StructureByteStride`)
            - `2～16Byte`(member): **Structuredアライメントルール**
      - **Rawバッファ(SRV,UAV)**
        - `16Byte`(Address): (`D3D12_RAW_UAV_SRV_BYTE_ALIGNMENT`)
          - `4Byte`(member)
    - **テクスチャ(SRV,UAV)** (1リソース = 1テクスチャ)
      - `512Byte`: サブリソース＆プレーン (`D3D12_TEXTURE_DATA_PLACEMENT_ALIGNMENT`)
        - `256Byte`: 行(width, RowPitch) (`D3D12_TEXTURE_DATA_PITCH_ALIGNMENT`)
          - `1～16Byte`: ピクセル (`DXGI_FORMAT`)
- **アライメントルール**
  - cbufferアライメントルール: (`#pack(cbuffer)`)
    - memberが＄16境界＝❰16Byteアライメント境界❱を**跨ぐ場合**は自動的にパディングが詰められmemberが**_∫16境界∫にアライメント**される。
    - **各データ型**(struct以外)のアライメントは**その型のサイズ**でアライメントされる。
    - CBuffer内の**struct**は、**無条件で∫16境界∫**にアライメントされる。
    - **行列**(float3x4など)は、**行列と各列ベクトル要素**も**無条件で∫16境界∫**にアライメントされる。
    - **配列**は行列と似ていて、**配列と各配列要素**も**無条件で∫16境界∫**にアライメントされる。
    - (全体的に∫16境界∫にアライメントされるのみであって、memberやstructや行列や配列 の**後方**は普通に詰められる(structとかのサイズは特に無い感じ))
  - Structuredアライメントルール: (`#pack(structured)`)
    - **各データ型**(struct以外)のアライメントは**その型のサイズ**でアライメントされる。(**∫16境界∫**のアライメントは**無い**)
  - C#アライメントルール (ShaderLab: `Inspect.Stack(data); Console.WriteLine(sizeof(Data));`)
    - **各データ型**(struct以外)のアライメントは**その型のサイズ**でアライメントされる。
    - `入れ子のstruct`の**アライメント**は、その`入れ子のstruct`の全てのメンバの型の**最大サイズの単位**
    - `struct`の**サイズ**は、`入れ子のstruct`のメンバも含めた全てのメンバの型の**最大サイズの単位**
    - [C#アライメント規則](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUKgZgAIMiBhIgbzyNrtoGdhIBjYIgSSigFMwARAIbBBNenWq5x0ksQBmAGwD2wonGGCCAbiIB6XZgBsYmXUJqlEAEYKeajQBYd+9A5PiAvu/pNW7AMrMEGwQYDxwQiLeEtHS5gwAFkpg7OoiAAzOuumx4uaKKqkamFluUqZmxInJRSLoWQAcufTmXLwCGkQAltx8kYJZRs2VRNUp9iIArFkE6MO0+g455dJeK7JE0AyCcnYkRiQORACygj0AFACU7pKmgX6h4f0TgkQAvES8AO5XWrlcDAADjw2AA6QKCFgAa3OaUElz+6xamAAnOcGF0AF48JRyc73YLAR4RDSXBHuNYeIA)

`R_Resource`の構成: `((((((Component) * ComponentCount) * SubPixelCount) * Resolution) * PlaneSlice) * MipMapLevels) * TextureArray`

- ☆`R_Device->`**CreateCommittedResource**`(D_HEAP_PROPERTIES,., D_RESOURCE_DESC, D_RESOURCE_STATES, D_CLEAR_VALUE, out R_Resource)`:
  `D_HEAP_TYPE`と`D_RESOURCE_DESC`と`D_RESOURCE_STATES`(バリア初期値)と`D_CLEAR_VALUE`を指定して`R_Resource`を作成
  - **struct D_HEAP_PROPERTIES**:
    - **enum D_HEAP_TYPE**: `D_HEAP_TYPE_～`:
      - `DEFAULT`: >**GPU専用メモリ**。リソースは**GPU側で高速にアクセス**。主にRT,Texture,Bufferなどに使用します。(**CUSTOM**:`NOT_AVAILABLE`,`L0`)
      - `UPLOAD`: >**Map**で**CPU=>GPU**に転送するメモリ。**CPUからのデータアップロード**に使用されます。(**CUSTOM**:`WRITE_COMBINE`,`L0`)
      - `READBACK`: >**Map**で**GPU=>CPU**に転送するメモリ。**GPUの計算結果をCPUに読み込む**際に使用します。(**CUSTOM**:`WRITE_BACK`,`L1`)
      - `CUSTOM`: >カスタムヒープタイプ。`CPUPageProperty`と`MemoryPoolPreference`を明示的に指定する。
  - **struct D_RESOURCE_DESC**:
    - `UINT64 Alignment`: `4MB`:MSAA, `64KB`:一般, `4KB`:64KB以下 (`0`は通常、テクスチャ:`64KB`バッファ:`4KB`) (`D_⟪DEFAULT＠❰MSAA❱¦SMALL⟫_RESOURCE_PLACEMENT_ALIGNMENT`)
    - **enum D_RESOURCE_DIMENSION** `Dimension`: `D_RESOURCE_DIMENSION～`
      - `_UNKNOWN`: 未定義
      - `_BUFFER`: **Buffer**。`Buffer`の場合は`DXGI_FORMAT_UNKNOWN`で`Width`に**バイト数**を設定(`sizeof(MyStruct)`など(`256バイト`アライメントらしい?))
      - `_TEXTURE⟪1D¦2D¦3D⟫`: **Texture**
    - `DXGI_FORMAT` `Format`: **フォーマット**
    - `UINT64 Width`,`UINT Height`: **Texture**の場合は`Width`と`Height`を使うが、**Buffer**の場合は`Width`のみ(`Height`は`1`)
    - `UINT16 MipLevels`: **ミップマップ数**
    - `UINT16 DepthOrArraySize`: 3DTextureの**深度** または 2DTexture配列(CubeMapも?)の**配列数**
    - **struct DXGI_SAMPLE_DESC** `SampleDesc`: **MSAAサンプリング設定**
      - `UINT Count`: **サンプル点**の数。`⟪1～32⟫`。(多くのハードウェアは**2の累乗に最適化**され`MSAAx8`までサポート)
      - `UINT Quality`: その`Count`数についての**品質レベル**。(通常は`0`が設定され、品質は**ハードウェア依存**)
    - **enum D_RESOURCE_FLAGS** `Flags`: `D_RESOURCE_FLAG_⟪NONE¦ALLOW_⟪RT¦DS¦UA⟫¦DENY_SR¦..⟫`: この`R_Resource`の**使用目的**。(**バリア**と**View**を制約)
  - **enum D_RESOURCE_STATES**: **バリアの初期状態** (`### D_RESOURCE_BARRIER`を参照)
  - **struct D_CLEAR_VALUE**: `R_Resource`の**値の初期化**。(多分`union{}`は`DXGI_FORMAT`によって`Color`か`DS`か決まる)
    - `DXGI_FORMAT`
    - `union{}`
      - `FLOAT Color[4]`: **初期化**する`Color`
      - `struct D_DEPTH_STENCIL_VALUE`: `FLOAT Depth; UINT8 Stencil;`: **初期化**する`Depth`と`Stencil`
- ☆`D_RESOURCE_DESC = R_Resource->`**GetDesc**`()`:
  `R_Resource`から`D_RESOURCE_DESC`取得
- ☆`R_Resource->`**Map**`(UINT SubResource, D_RANGE pReadRange, out void** ppData)`:
  >`Map(..)`は**GPUリソース**を**CPU仮想アドレス空間**に**マッピング**する。これにより、CPUから直接GPUリソースにアクセスすることが可能
  - `UINT SubResource`: (TextureArrayインデックス * MipMapLv数) + MipMapLvインデックス
  - `struct D_RANGE{SIZE_T Begin; SIZE_T End;} pReadRange`: `SubResource`内の**アクセス範囲**(`nullptr`入れると全範囲)(バイト単位)(`SIZE_T`は符号なし整数)
  - `out void** ppData`: `ppData`の参照(アドレス)を渡して`ppData`の値(ポインタ)を**Map元のアドレス**で書き換える。(これを介して**GPUにアクセス**)
    - **GPUメモリ空間**を**CPU仮想メモリ空間**に**マッピング**する(CPUメモリにアクセスするふりしてGPUメモリにアクセスする(アクセス:**読み**(`READBACK`)**書き**(`UPLOAD`)))
- ☆`R_Resource->`**Unmap**`(UINT SubResource, D_RANGE pReadRange)`:
   >`Map(..)`を使用して**CPU**がリソースに対して**仮想アドレス空間**を得て、データを書き込んだり読み込んだりした後、`Unmap(..)`を使ってその**マッピングを解除**します。
  (Persistent Mapping(持続的なマッピング): `Unmap(..)`しないことで`Map(..)`と`Unmap(..)`のオーバーヘッドを削減し、パフォーマンスを向上させることができる。(`CBuffer`など))
- ☆`D_GPU_VIRTUAL_ADDRESS = R_Resource->`**GetGPUVirtualAddress**`()`:
  `D_GPU_VIRTUAL_ADDRESS`を取得。(**GPU**が**VRAM**のリソースに直接アクセスする**GPU仮想アドレス**)

### R_DescriptorHeap

- **ヒープ**生成
  - ☆`R_Device->`**CreateDescriptorHeap**`(D_DESCRIPTOR_HEAP_DESC, out R_DescriptorHeap)`:
    **ビューの種類**と**数**を指定して`R_DescriptorHeap`を作る
    - **struct D_DESCRIPTOR_HEAP_DESC**:
      - **enum D_DESCRIPTOR_HEAP_TYPE**: `D_DESCRIPTOR_HEAP_TYPE⟪_CBV_SRV_UAV¦_RTV¦_DSV¦_SAMPLER¦_NUM_TYPES⟫`
      - **UINT NumDescriptors**: 格納する**ビューの数**(ビューの型は無いらしい(常に`R_DescriptorHeap`からビューにアクセスする))
      - **enum D_DESCRIPTOR_HEAP_FLAGS** `D_DESCRIPTOR_HEAP_FLAG_⟪NONE¦SHADER_VISIBLE⟫`
- **ハンドル**操作
  - ☆`D_⟪CPU¦GPU⟫_DESCRIPTOR_HANDLE = R_DescriptorHeap->`**Get⟪CPU¦GPU⟫DescriptorHandleForHeapStart**()
    `R_DescriptorHeap`の**_⟪CPU¦GPU⟫先頭アドレス**(`D_⟪CPU¦GPU⟫_DESCRIPTOR_HANDLE`)を取得
  - ☆`D_⟪CPU¦GPU⟫_DESCRIPTOR_HANDLE.ptr += R_Device->`**GetDescriptorHandleIncrementSize**`(D_DESCRIPTOR_HEAP_TYPE)`
    `D_⟪CPU¦GPU⟫_DESCRIPTOR_HANDLE`に**ビューのオフセット**(`D_DESCRIPTOR_HEAP_TYPE`)を足す
- **ビュー**を作成して**ヒープ**の**ハンドル**に詰める
  (`R_Resource`を`D_⟪｡⟪SR¦UA¦RT¦DS¦CB⟫_VIEW_DESC｡¦｡SAMPLER_DESC｡⟫`をもとに**ビューを作り**`D_CPU_DESCRIPTOR_HANDLE`に**詰める**)
  - ☆`R_Device->`**Create⟪SR¦UA¦RT¦DS⟫View**`(R_Resource, 『UAVのみ』＠❰『カウンター』R_Resource❱, D_⟪SR¦UA¦RT¦DS⟫_VIEW_DESC, 『setToAddress』 D_CPU_DESCRIPTOR_HANDLE)`
    - **struct D_⟪SR¦UA¦RT¦DS⟫_VIEW_DESC**:
      ★大体、**DXGI_FORMAT**, `union{`**MipMapLv範囲**, **_ARRAY範囲**, **PlaneSlice**`}`。(つまり、Format, u{SubResource, PlaneSlice} (Format, u{バッファ選択}))
      - `DXGI_FORMAT`
      - `D_⟪SR¦UA¦RT¦DS⟫V_DIMENSION`: **次元**は`R_Resource`の`D_RESOURCE_DIMENSION`と合わせる必要がある
      - `『SRのみ』＠❰UINT Shader4ComponentMapping❱`: ↓の`enum`によって`RGBA`のComponentを**入れ替え**たり、`⟪0¦1⟫`に**固定**したりする
        `Shader4ComponentMapping =`**D_ENCODE_SHADER_4_COMPONENT_MAPPING**`(⟦4┃, ⟧｢D_SHADER_COMPONENT_MAPPING～｣)`という風に設定するには**マクロ**を使う
        - `Shader4ComponentMapping`<=**enum D_SHADER_COMPONENT_MAPPING**: `D_SHADER_COMPONENT_MAPPING～`
          - `_FROM_MEMORY_COMPONENT_⟪0¦1¦2¦3⟫`
          - `_FORCE_VALUE_⟪0¦1⟫`
      - `『DSのみ』＠❰D_DSV_FLAGS❱`: ↓の`enum`で`DSV`を**読み取り専用**にするか決める
        - **enum D_DSV_FLAGS**: `D_DSV_FLAG～`
          - `_NONE`
          - `_READ_ONLY_⟪DEPTH¦STENCIL⟫`
      - ★`union{次元}`『`D_⟪SR¦UA¦RT¦DS⟫V_DIMENSION`により選択される。(**ビューx次元**の組み合わせ数)
        - **struct D_BUFFER_⟪SR¦UA¦RT⟫V**:
          - `UINT FirstElement; UINT NumElements`: `FirstElement` ～ `FirstElement`+`NumElements`-1。`Element`は↓の単位
          - `『⟪SR¦UA⟫のみ』＠❰UINT StructureByteStride ❱`: >構造体の1つの要素のサイズ(アライメント**16Byte単位**)。>`DXGI_FORMAT`は DXGI_FORMAT_UNKNOWN に設定され、無視されます。
          - `『⟪SR¦UA⟫のみ』＠❰enum D_BUFFER_⟪SR¦UA⟫V_FLAGS Flags❱`:
            - `D_BUFFER_UAV_FLAG_NONE`: `StructureByteStride` と シェーダー`RWStructuredBuffer<～>`を使用。`DXGI_FORMAT`は`DXGI_FORMAT_UNKNOWN`を設定
            - `D_BUFFER_UAV_FLAG_RAW`: `StructureByteStride`は不使用。シェーダー`RWByteAddressBuffer`を使用。`DXGI_FORMAT`は`DXGI_FORMAT_R32_TYPELESS`(RenderDocで確認した(ビュー))を設定
              (`FirstElement; NumElements`は**4バイト単位**になる)
          - `『UAのみ』＠❰UINT64 CounterOffsetInBytes❱`: >UAVカウンタのオフセット(バイト単位)。UAVがカウンタを持つ場合、このフィールドはカウンタの位置を指定します。
          - **使い方**: (`CBV`を`SR`や`UA`で**置き換える**ことは可能)
            - `SR`: Structured:`StructuredBuffer<MyStruct> myBuffer`, Raw:`ByteAddressBuffer myBuffer` で参照
            - `UA`: Structured:**RW**`StructuredBuffer<MyStruct> myBuffer`, Raw:**RW**`ByteAddressBuffer myBuffer` で参照
            - `RT`: `R_GraphicsCommandList->OMSetRenderTargets(1, &rtvHandle, FALSE, nullptr)`(1ピクセル1Element。1次元にどう描画するかは少々不明)(`BUFFER`を`RT`として利用する**特殊なケース**)
        - **struct D_TEX⟪1D¦2D⟫＠❰\_ARRAY❱_⟪SR¦UA¦RT¦DS⟫V**: Mip, Array, Plane
          - `『SRのみ』＠❰UINT MostDetailedMip; UINT MipLevels; FLOAT ResourceMinLODClamp❱`: Max(`MostDetailedMip`,`ResourceMinLODClamp`) ～ `MostDetailedMip`+`MipLevels`-1
          - `『SR以外』＠❰UINT MipSlice❱`: >ミップマップチェーン内の**MipMapLv**を指定
          - `『_ARRAYのみ』＠❰UINT FirstArraySlice; UINT ArraySize❱`: `FirstArraySlice` ～ `FirstArraySlice` + `ArraySize`-1 (MRTする場合、`ArraySize = 1`にして個別にRTVを作る必要がある)
          - `『2Dのみ,DS以外』＠❰UINT PlaneSlice❱`: >2Dテクスチャのプレーンインデックス。ビューがどのプレーンにアクセスするかを指定します。(各プレーンは**メモリ空間的に分離**している)
            - 例:`DXGI_FORMAT_NV12` の場合:
              - `PlaneSlice = 0` で**Yプレーン**（輝度チャンネル）にアクセス。
              - `PlaneSlice = 1` で**UVプレーン**（色差チャンネル）にアクセス。
            - 例:`DXGI_FORMAT_D32_FLOAT_S8X24_UINT` の場合:
              - `PlaneSlice = 0` で**32ビット深度プレーン**にアクセス。
              - `PlaneSlice = 1` で**8ビットステンシルプレーン**にアクセス。
            - `DXGI_FORMAT`の**RGBA系**はプレーンを**1つ**しか持たないから指定する意味は無い
        - **struct D_TEX2DMS＠❰\_ARRAY❱_⟪SR¦RT¦DS⟫V**: Array
          - `『_ARRAYのみ』＠❰UINT FirstArraySlice; UINT ArraySize❱`: `FirstArraySlice` ～ `FirstArraySlice` + `ArraySize`-1
        - D_TEX3D
          - **struct D_TEX3D_❰SR❱V**: Mip
            - `UINT MostDetailedMip; UINT MipLevels; FLOAT ResourceMinLODClamp`: Max(`MostDetailedMip`,`ResourceMinLODClamp`) ～ `MostDetailedMip`+`MipLevels`-1
          - **struct D_TEX3D_⟪RT¦UA⟫V**: Mip, Array
            - `UINT MipSlice`: >ミップマップチェーン内の**MipMapLv**を指定
            - `UINT FirstWSlice; UINT WSize`: `FirstWSlice` ～ `FirstWSlice`+`WSize`-1。(`_ARRAY`と似ている。`uvw`の`w`のアクセス範囲)
        - **struct D_TEXCUBE＠❰\_ARRAY❱_❰SR❱V**: Mip, Array
          - `UINT MostDetailedMip; UINT MipLevels; FLOAT ResourceMinLODClamp`: Max(`MostDetailedMip`,`ResourceMinLODClamp`) ～ `MostDetailedMip`+`MipLevels`-1
          - `『_ARRAYのみ』＠❰UINT First2DArrayFace; UINT NumCubes❱`: `First2DArrayFace` ～ `First2DArrayFace`+(`NumCubes`*`6`)-1
        - **struct D_RAYTRACING_ACCELERATION_STRUCTURE_❰SR❱V**:
          - `D_GPU_VIRTUAL_ADDRESS Location;`: >**ASのGPU仮想アドレス**を指定します。このアドレスは、レイトレーシングシェーダーがアクセスするために使用されます。
  - ☆`R_Device->`**Create❰CB❱View**`(D_❰CB❱_VIEW_DESC, 『setToAddress』 D_CPU_DESCRIPTOR_HANDLE)`
    - **struct D_❰CB❱_VIEW_DESC**:
      - `D_GPU_VIRTUAL_ADDRESS BufferLocation`: `+= (sizeof(MaterialForHlsl) + 0xff)&~0xff`で詰めて入れられる
      - `UINT SizeInBytes`
  - ☆`R_Device->`**Create❰Sampler❱**`(D_❰SAMPLER❱_DESC, 『setToAddress』 D_CPU_DESCRIPTOR_HANDLE)`
    - **struct D_❰SAMPLER❱_DESC**:
      - `D_FILTER`: フィルター
      - `UINT MaxAnisotropy`: **最大**異方性レベル
      - `D_TEXTURE_ADDRESS_MODE Address⟪U¦V¦W⟫`
        - `enum D_TEXTURE_ADDRESS_MODE`: `D_TEXTURE_ADDRESS_MODE⟪_WRAP¦_MIRROR¦_CLAMP¦_BORDER¦_MIRROR_ONCE⟫`
      - `FLOAT ⟪Min¦Max⟫LOD`: `clamp(｢MipMapLv｣, Min, Max)`
      - `FLOAT MipLODBias`: `｢MipMapLv｣ + MipLODBias`
      - `D_COMPARISON_fUNC`
        - `enum D_COMPARISON_FUNC`: `D_COMPARISON_FUNC⟪_NEVER¦_LESS¦_EQUAL¦_LESS_EQUAL¦_GREATER¦_NOT_EQUAL¦_GREATER_EQUAL¦_ALWAYS⟫`
        - `sM.z ⟪False¦<¦==¦<=¦>¦!=¦>=¦True⟫ sC.z`。`shadowMap.SampleCmp＠❰LevelZero❱(sampler, shadowCoord.xy, shadowCoord.z)`: `LevelZero`は`MipMapLv`:`0`を使用
      - `FLOAT BorderColor[4]`: `_ADDRESS_MODE_BORDER`時、uv範囲外の色

### R_RootSignature

- `Unity.drawio/ページ35`も参照
- ルートシグネチャーはシェーダーのシェーダーリソースのregister集まりに対して必要なため、通常シェーダー毎にルートシグネチャーが必要

- ディスク。**struct D_ROOT_SIGNATURE_DESC**:
  - **struct D_ROOT_PARAMETER**[]:
    - `enum D_ROOT_PARAMETER_TYPE`: `D_ROOT_PARAMETER_TYPE_⟪DESCRIPTOR_TABLE¦⟪CBV¦SRV¦UAV⟫¦32BIT_CONSTANTS⟫`
    - **enum D_SHADER_VISIBILITY**: `D_SHADER_VISIBILITY⟪_ALL¦_VERTEX¦_HULL¦_DOMAIN¦_GEOMETRY¦_PIXEL¦_AMPLIFICATION¦_MESH⟫`: `D_ROOT_PARAMETER`のシェーダーステージの**可視性**
    - ★`union{D_ROOT_PARAMETER_TYPE}`
      - **struct D_ROOT_DESCRIPTOR_TABLE**: `D_ROOT_PARAMETER_TYPE`が`DESCRIPTOR_TABLE`の時
        `R_GraphicsCommandList->`**SetDescriptorHeaps**`(., R_DescriptorHeap[])`
        `R_GraphicsCommandList->`**SetGraphicsRootDescriptorTable**`(UINT RootParameterIndex, R_DescriptorHeap->GetGPUDescriptorHandleForHeapStart())`
        - **D_DESCRIPTOR_RANGE**[]:
          - **enum D_DESCRIPTOR_RANGE_TYPE**: `D_DESCRIPTOR_RANGE_TYPE～`: `RANGE`に含める**ビュー**のタイプ
            - `⟪_SRV¦_UAV¦_CBV¦_SAMPLER⟫`
          - `UINT BaseShaderRegister`; `UINT NumDescriptors`: `register`のインデックスの範囲
            - `: register`(`⟪b¦t¦u¦s⟫`⟪`BaseShaderRegister` ～ `BaseShaderRegister` + `NumDescriptors`-1⟫)
          - `UINT RegisterSpace`: **register空間**。`: register`(～, `space`｢`RegisterSpace`｣)
          - `UINT OffsetInDescriptorsFromTableStart`: `D_ROOT_DESCRIPTOR_TABLE`に**差し込む**インデックス。(draw.io/ページ35参照)
            - `D_DESCRIPTOR_RANGE_OFFSET_APPEND`を設定すると**前のレンジ**の**直後**にオフセットが設定される
      - **struct D_ROOT_DESCRIPTOR**: **単一**の`DESCRIPTOR`。`D_ROOT_PARAMETER_TYPE`が`⟪CBV¦SRV¦UAV⟫`の時、設定されるので、ビューのタイプは分かっている
        `R_DescriptorHeap`を介さずに`R_Resource->D_GPU_VIRTUAL_ADDRESS`を使って直接`R_Resource`を`D_ROOT_PARAMETER`にバインドする
          `R_GraphicsCommandList->`**SetGraphicsRoot⟪CB¦SR¦UA⟫View**`(UINT RootParameterIndex, R_Resource->D_GPU_VIRTUAL_ADDRESS)`
        - `UINT ShaderRegister`: `register`のインデックス。(`⟪b¦t¦u⟫⟪～⟫`)
        - `UINT RegisterSpace`: **register空間**。`: register`(～, `space`｢`RegisterSpace`｣)
      - **struct D_ROOT_CONSTANTS**: `32BIT_CONSTANTS`の時、選択。`CBV`と同じようにシェーダーで使う
        `R_DescriptorHeap`と`R_Resource`も使わず、`float val[4]`などの**値**を直接`D_ROOT_PARAMETER`にバインドする
          `R_GraphicsCommandList->`**SetGraphicsRoot32BitConstants**`(UINT RootParameterIndex, UINT Num32BitValuesToSet, void* pSrcData, UINT DestOffsetIn32BitValues)`
            `pSrcData = ｢32bitType｣ ｢変数｣[Num32BitValues]`
            設定する範囲:`DestOffsetIn32BitValues ～ DestOffsetIn32BitValues + Num32BitValuesToSet-1`
          `D_RESOURCE_DESC::Width = 4byte(32bit) * Num32BitValues` と似ているが`R_Resource`は使用しない
        - `UINT ShaderRegister`: `register`のインデックス。(`b⟪～⟫`)
        - `UINT RegisterSpace`: **register空間**。`: register`(～, `space`｢`RegisterSpace`｣)
        - `UINT Num32BitValues`: >シェーダーに渡す32ビットの**定数値**の**数**を指定
  - **struct D_STATIC_SAMPLER_DESC**[]:
    - ～**struct D_❰SAMPLER❱_DESC**～を参照
    - `UINT ShaderRegister`: `register`のインデックス。(`s⟪～⟫`)
    - `UINT RegisterSpace`: **register空間**。`: register`(～, `space`｢`RegisterSpace`｣)
    - `D_SHADER_VISIBILITY`: `D_SHADER_VISIBILITY⟪_ALL¦_VERTEX¦_HULL¦_DOMAIN¦_GEOMETRY¦_PIXEL¦_AMPLIFICATION¦_MESH⟫`: `D_STATIC_SAMPLER_DESC`のシェーダーステージの**可視性**
  - **enum D_ROOT_SIGNATURE_FLAGS**: `D_ROOT_SIGNATURE_FLAG～`: `D_ROOT_SIGNATURE_DESC`の構成要素と関係ないフラグもあるが最適化のための**折衷案**かも知れない
    - `_NONE`: 特に指定なし
    - `_DENY_⟪VERTEX¦HULL¦DOMAIN¦GEOMETRY¦PIXEL¦AMPLIFICATION¦MESH⟫_SHADER_ROOT_ACCESS`:
      - 各シェーダーステージ↑が`R_RootSignature`全体に**アクセスしない**場合に、パフォーマンスを**最適化**する
    - `_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT`: `入力アセンブラステージ`で、`INPUT_LAYOUT`を使用 (**通常はこれを設定**)
    - `_ALLOW_STREAM_OUTPUT`: `ストリームアウトステージ`で、`Buffer`の出力(`STREAM_OUTPUT`)を許可
    - `⟪_CBV_SRV_UAV¦_SAMPLER⟫_HEAP_DIRECTLY_INDEXED`: >`R_DescriptorHeap`を特定の構造(連続したメモリ領域など)やヒープの配置,サイズにも注意する代わりに最適化できるらしい
    - `_LOCAL_ROOT_SIGNATURE`: >ローカルルートシグネチャ。レイトレーシングパイプラインで使われ、ルートシグネチャがシェーダーテーブル内のローカルシェーダーで使用されることを指定します。
- ★ルートシグネチャー生成。`D_ROOT_SIGNATURE_DESC`=>`ID3DBlob SignatureBlob`=>`R_RootSignature`
  - ☆**D3D12SerializeRootSignature**`(D_ROOT_SIGNATURE_DESC,., out ID3DBlob SignatureBlob,.)`
    `D_ROOT_SIGNATURE_DESC`を素にシリアライズして`ID3DBlob SignatureBlob`を生成。(`ID3DBlob`噛むのは多分**ディスクに保存**するため)
  - ☆**CreateRootSignature**`(.,void* SignatureBlob->GetBufferPointer(), SIZE_T SignatureBlob->GetBufferSize(), out R_RootSignature)`
    `ID3DBlob SignatureBlob`を素に`R_RootSignature`を生成

## 同期処理系 (Unity.drawio/ページ29)

### R_Fence

`R_Fence`は、GPUの**コマンドの完了(Signal)**を**_⟪CPU¦GPU⟫がチェック**するためのオブジェクト。
`Wait(..)` => `ExecuteCommandLists(..)` => `Signal(..)` の順でキューに積む。

- フェンス作成
  - ☆`R_Device->`**CreateFence**`(UINT64 fenceValue,.,out R_Fence)`
    **初期値**(`fenceValue`)を`R_Fence`に**設定**(`R_Fence = fenceValue`)して`R_Fence`を**作成**する
- GPUシグナル設定
  - ☆`R_CommandQueue->`**Signal**`(R_Fence, UINT64 fenceValue)`
    `R_CommandQueue`が**実行完了**すると`R_Fence = fenceValue`される
  - ☆`R_GraphicsCommandList->`**Signal**`(R_Fence, UINT64 fenceValue)`
    `Signal`の`R_GraphicsCommandList`版。(>より細かいタイミングで GPU 上での操作を完了するタイミングを示すことができます。)
- ⟪CPU¦GPU⟫コンプリート確認
  - ☆`R_Fence->`**GetCompletedValue**`()`
    `R_Fence`の**フェンス値**を取得する。(`Signal`で`R_Fence = fenceValue`されたかを**チェック**するために使う)
  - ☆`R_CommandQueue->`**Wait**`(R_Fence, UINT64 fenceValue)`
    `R_Fence`が`R_Fence`>= `fenceValue`になるまで、`R_CommandQueue`の実行を待機する。(GPUでチェックし待機)
  - ☆`R_Fence->`**SetEventOnCompletion**`(UINT64 fenceValue, HANDLE)`
    `R_Fence`が`R_Fence`>= `fenceValue`になったら、`HANDLE`の**イベント**を実行する。(CPUをブロックしない)

### D_RESOURCE_BARRIER

`R_Resource`への適切な**アクセス権限の管理**を行い、最適化する

- ☆`R_GraphicsCommandList->`**ResourceBarrier**`(D_RESOURCE_BARRIER[] pBarriers)`
  - **struct D_RESOURCE_BARRIER**[] `pBarriers`:
    - **enum D_RESOURCE_BARRIER_TYPE** `Type`: `D_RESOURCE_BARRIER_TYPE～`:
      - `_TRANSITION`: `R_Resource`＠❰の`Subresource`❱の`D_RESOURCE_STATES`(バリア)を**遷移**(TRANSITION)させる
      - `_ALIASING`: >異なるリソースが同じ物理メモリ領域を使用する場合、メモリの競合を防ぐために使われるバリアです。
      - `_UAV`: >UAVの操作を完了させるためのバリアで、特定のリソースの状態に依存しないバリアです。
    - **enum D_RESOURCE_BARRIER_FLAGS** `Flags`: `D_RESOURCE_BARRIER_FLAG⟪％_NONE¦⟪_BEGIN¦_END⟫_ONLY⟫`:
      `R_Resource`の最初と最後に`⟪_BEGIN¦_END⟫_ONLY`を使うことで**最適化**するらしい
    - union{D_RESOURCE_BARRIER_TYPE}: `Transition`のみメモしている
      - **D_RESOURCE_TRANSITION_BARRIER** `Transition`:
        - `R_Resource Resource`: バリアを遷移させる`R_Resource`
        - `UINT Subresource`: バリアを遷移させる`Subresource`。(`D_RESOURCE_BARRIER_ALL_SUBRESOURCES`:全ての`Subresource`)
        - **enum D_RESOURCE_STATES** `State⟪Before¦After⟫`: `Before`から`After`へ**バリアが遷移**する。(`enum`は組合せ可能)
          - `_COMMON`: どこからでもアクセス可能?。初期状態に使用される
          - `_ALL_SHADER_RESOURCE`: `＠❰_NON❱_PIXEL_SHADER_RESOURCE`の組合せ。(全てのシェーダーステージのシェーダーリソース)
          - `＠❰_NON❱_PIXEL_SHADER_RESOURCE`: ＠❰_NON❱ピクセルシェーダー
          - `_GENERIC_READ`: `⟪_VERTEX_AND_CONSTANT¦_INDEX⟫_BUFFER|_COPY_SOURCE|_ALL_SHADER_RESOURCE`の組合せ。(汎用的なリソースの読み取り)
          - `_RENDER_TARGET`: レンダーターゲット
          - `_DEPTH_⟪READ¦WRITE⟫`: デプスバッファ⟪READ¦WRITE⟫
          - `_PRESENT`: プレゼント
          - `_UNORDERED_ACCESS`: UAV
          - `_VERTEX_AND_CONSTANT_BUFFER`: 頂点＆定数
          - `_INDEX_BUFFER`: インデックス
          - `_COPY⟪_SOURCE¦_DEST⟫`: `CopyResource`, `CopyTextureRegion`の⟪_SOURCE¦_DEST⟫
          - `_RESOLVE⟪_SOURCE¦_DEST⟫`: MSAAのリゾルブの⟪_SOURCE¦_DEST⟫
          - `_STREAM_OUT`: ストリーム出力バッファ
          - `_RAYTRACING_ACCELERATION_STRUCTURE`:
          - `_INDIRECT_ARGUMENT`: 引数バッファ (`R_GraphicsCommandList->ExecuteIndirect(..)`)
          - `_SHADING_RATE_SOURCE`: 可変レートシェーディング(VRS)
          - `_PREDICATION`: GPUの予測処理に使用するリソース
          - `_VIDEO_⟪_ENCODE¦_DECODE¦_PROCESS⟫⟪_READ¦_WRITE⟫`:

## パイプライン系

### R_PipelineState (PSO)

- ☆`R_Device->`**CreateGraphicsPipelineState**`(., out R_PipelineState)`
  - **struct D_GRAPHICS_PIPELINE_STATE_DESC**: 主に、シェーダーコード, ∮RenderingState∮(カリング,ADS)。他の所の設定と**重複**しているのは、**最適化**のためらしい(パイプラインをバシッと決める)
    - ルートシグネチャー: **struct R_RootSignature** `pRootSignature`:
      `CreateRootSignature(.., out R_RootSignature)`を**最適化**のためココでも設定(`SetGraphicsRootSignature(..)`でも設定される)。`## ルートシグネチャー`を参照
    - HLSLシェーダーコード: **struct D_SHADER_BYTECODE** `⟪VS¦PS¦DS¦HS¦GS⟫`: コンパイルしたシェーダー(`D3DCompileFromFile(.., out ID3DBlob ppCode,.)`)を設定
      - `void* pShaderBytecode`: `ID3DBlob`の**先頭アドレス**。`ID3DBlob->GetBufferPointer()`を設定する
      - `SIZE_T BytecodeLength`: `ID3DBlob`の**長さ**。`ID3DBlob->GetBufferSize()`を設定する
    - ★ラスタライザ: **struct D_RASTERIZER_DESC** `RasterizerState`:
      - 描画の方法
        - **D_FILL_MODE** `FillMode`: `D_FILL_MODE⟪_WIREFRAME¦_SOLID⟫`: `_WIREFRAME`は**ピクセルシェーダー**がちゃんと**起動**する(固定パイプではない)。**線の太さ**はハードウェア依存
      - ★描画の有無
        - **enum D_CULL_MODE** `CullMode`: `D_CULL_MODE⟪_NONE¦_FRONT¦％_BACK⟫`: `_NONE`:両面描画, `⟪_FRONT¦_BACK⟫`:`⟪_FRONT¦_BACK⟫`をカリング
        - `BOOL FrontCounterClockwise`: `％FALSE`:カメラの視点から**時計回り**のポリゴンの面を`FRONT`とする, `TRUE`:**反時計回り**を`FRONT`とする
        - `BOOL DepthClipEnable`: `FALSE`は、`0.0～1.0`の範囲を**超える**位置にある**ポリゴンを描画**する。(なお、**深度バッファ**は通常通り**0.0～1.0の範囲にクリップ**され書き込まれる)
        - **D_CONSERVATIVE_RASTERIZATION_MODE** `ConservativeRaster`: `D_CONSERVATIVE_RASTERIZATION_MODE⟪_OFF¦_ON⟫`: ![Conservative](\..\画像\D3D12_CONSERVATIVE.png)
      - アンチエイリアス
        - `BOOL AntialiasedLineEnable`: **Line描画**の時、MSAAより軽量なAAを掛けるらしい。(MultisampleEnable❰MSAA❱ = TRUE時はMSAAの方が品質が良いため**この設定は無効**になる)
        - MSAA
          - `BOOL MultisampleEnable`: **MSAA**の有無フラグ
          - `UINT ForcedSampleCount`: 基本的に`R_Resource`の`DXGI_SAMPLE_DESC.Count`を上書きするが、`ForcedSampleCount` > `R_Resource～.Count`の場合は**ハードウェア依存**
      - デプスバイアス
        `MaxDepthSlope = max(abs(ddx(depth)), abs(ddy(depth)))`『ポリゴンの傾き『ピクセルシェーダーではDSVのデプスバッファの値(depth)は取得できない
        `ScaledBias = SlopeScaledDepthBias * MaxDepthSlope`
        `FinalDepthBias = clamp(ScaledBias + DepthBias, -DepthBiasClamp, DepthBiasClamp)`
        `NewDepth = depth + FinalDepthBias`
        設定は深度バッファの解像度(32bitなど)の精度単位(precision unit)で基本的に整数で設定する
        - `INT DepthBias`: **DSVのデプス**に加算する
        - `FLOAT SlopeScaledDepthBias`: カメラの視線に対して**ポリゴンの傾き**に応じて加算する
        - `FLOAT DepthBiasClamp`: 最終的な`Bias`を`-DepthBiasClamp`～`DepthBiasClamp`で`Clamp`する
    - ポリゴン定義
      - ストリップカット**enum D_INDEX_BUFFER_STRIP_CUT_VALUE** `IBStripCutValue`: `D_INDEX_BUFFER_STRIP_CUT_VALUE～`: `D_PRIMITIVE_TOPOLOGY`が`STRIP`の時、**切断する記号**を指定
        **例**: Indexバッファ:`{0, 1, 2, 3, 4, 5, 0xFFFF, 6, 7, 8, 9}`の時、`0xFFFF`でポリゴンが**切断**される。(ストリップなのでプリミティブの倍数でなくても良い)
        - `_DISABLED`: 切断記号を使わない(切断しない)
        - `_0xFFFF`: インデックス形式が**16ビット**の場合の切断記号
        - `_0xFFFFFFFF`: インデックス形式が**32ビット**の場合の切断記号
      - プリミティブトポロジー: **enum D_PRIMITIVE_TOPOLOGY_TYPE** `PrimitiveTopologyType`: `D_PRIMITIVE_TOPOLOGY_TYPE～`:
        `IASetPrimitiveTopology(D_PRIMITIVE_TOPOLOGY)`でも設定されるが**最適化**のためここでも設定される
        - `_UNDEFINED`: エラーになる
        - `_POINT`: ポイント
        - `_LINE`: ライン
        - `_TRIANGLE`: 三角形
        - `_PATCH`: >テッセレーションを使った曲面生成や、GPU側で頂点補間を行うような高度なジオメトリ処理に用いられます。
    - インプットレイアウト: **struct D_INPUT_LAYOUT_DESC** `InputLayout`: `struct InputLayout{DXGI_FORMAT Semantic, ..}`のようなもの
      - **struct D_INPUT_ELEMENT_DESC**[] `pInputElementDescs`:
        - 『変数名』 `LPCSTR SemanticName`; `UINT SemanticIndex`: `: ❰SemanticName❱❰SemanticIndex❱`(例: `: TEXCOORD0`は、`TEXCOORD`==`❰SemanticName❱`, `0`==`❰SemanticIndex❱`(省略可))
        - 『型』     `DXGI_FORMAT Format`: **フォーマット**
        - 『アドレス』`Format`の**位置**
          - `UINT InputSlot`: `VBV`の**Stream**。`IASetVertexBuffers(UINT StartSlot, D_VERTEX_BUFFER_VIEW[i])`の`StartSlot + [i]`という**インデックス**に対応する
          - `UINT AlignedByteOffset`: `VBV`の**ストライド内**のByte単位の**位置**。(`D_APPEND_ALIGNED_ELEMENT`を設定すると**前回**の`D_INPUT_ELEMENT_DESC`の**直後の位置**になる)
        - **enum D_INPUT_CLASSIFICATION** `InputSlotClass`: `D_INPUT_CLASSIFICATION～`: `VBV`が、**頂点毎**か**インスタンス毎**か
          - `_PER_VERTEX_DATA`: `VBV`が**頂点毎**のデータ
          - `_PER_INSTANCE_DATA`: `VBV`が**インスタンス毎**のデータ
        - `UINT InstanceDataStepRate`: `D_INPUT_CLASSIFICATION`が`_PER_INSTANCE_DATA`の時、`VBV`を1つ進める時の**インスタンス数**
    - RTフォーマット,MRT
      - `UINT NumRenderTargets`: `⟪0～8⟫`の値を設定。`0`:デプスのみ出力, `1`:通常の描画, `2以上`:MRT
      - `DXGI_FORMAT`: **ビューと一致**している必要があるらしい
        - `RTVFormats[8]`: RTVの`NumRenderTargets`分の`DXGI_FORMAT`を指定
        - `DSVFormat`: DSVの`DXGI_FORMAT`を指定
    - ★デプスステンシルテスト: **struct D_DEPTH_STENCIL_DESC** `DepthStencilState`:
      - デプステスト
        - `BOOL DepthEnable`: **デプステスト有無**フラグ
        - **enum D_COMPARISON_FUNC** `DepthFunc`: `D_COMPARISON_FUNC⟪_ALWAYS¦_NEVER¦_LESS¦_GREATER¦＠⟪_LESS¦_GREATER¦_NOT⟫_EQUAL⟫`:
          - NewDepth `⟪True¦False¦<¦>¦<=¦>=¦!=¦==⟫` RTDepth。`NewDepth`と`RTDepth`との**デプステスト**
        - **D_DEPTH_WRITE_MASK** `DepthWriteMask`: `D_DEPTH_WRITE_MASK⟪_ALL¦_ZERO⟫`: `⟪_ALL¦_ZERO⟫`:Zライト**有無**
      - ステンシルテスト
        **参照値(Ref)**: `R_GraphicsCommandList->`**OMSetStencilRef**`(`**Ref**`)`
        - `BOOL StencilEnable`: **ステンシルテスト有無**フラグ
        - マスク
          - `UINT8 StencilReadMask`: テスト時、`Stencil` & `StencilReadMask` **StencilFunc** `Ref` & `StencilReadMask` される
          - `UINT8 StencilWriteMask`: `Stencil`**書き込み**時のマスク。(`Stencil = (Stencil & ~StencilWriteMask) | (NewStencil & StencilWriteMask)`)
        - **struct D_DEPTH_STENCILOP_DESC** `⟪Front¦Back⟫Face`: ポリゴンの`⟪Front¦Back⟫Face`に**ステンシルテスト**を設定する
          - **enum D_COMPARISON_FUNC** `StencilFunc`: `DepthFunc`と同じ型: Stencil `⟪True¦False¦<¦>¦<=¦>=¦!=¦==⟫` Ref。`Stencil`と`Ref`との**ステンシルテスト**
          - **enum D_STENCIL_OP** `Stencil⟪＠❰Depth❱Fail¦Pass⟫Op`: `_STENCIL_OP～`: ⟪ステンシル⟪成功¦失敗⟫¦デプスのみ失敗⟫の時の、`Stencil`**更新**方法
            - `_KEEP`: `Stencil = Stencil`
            - `_ZERO`: `Stencil = 0`
            - `_REPLACE`: `Stencil = Ref`
            - `_INVERT`: `Stencil = ~Stencil`
            - `_INCR＠❰_SAT❱`: `Stencil = ++Stencil`(Wrap), `_SAT`:`Stencil = ++Stencil`(上限値でサチる)
            - `_DECR＠❰_SAT❱`: `Stencil = --Stencil`(Wrap), `_SAT`:`Stencil = --Stencil`(下限値でサチる)
    - ★ブレンディング: **struct D_BLEND_DESC** `BlendState`:
      - `BOOL AlphaToCoverageEnable`: Alpha値(0～1の範囲?)によって**MSAAのサンプル点**を有効にしてRTカラーバッファと色を混ぜる<https://youtu.be/htzYbOZ-an0?t=321>
      - `BOOL IndependentBlendEnable`: **TRUE**:各`RenderTarget[⟪0～7⟫]`のブレンドステートを個別に設定。**FALSE**:`RenderTarget[0]`の設定が他の全ての`RenderTarget[]`に適用
      - **struct D_RENDER_TARGET_BLEND_DESC** `RenderTarget[8]`: (`BlendEnable`と`LogicOpEnable`は排他的。同時`TRUE`は不可)
        - `BOOL BlendEnable`: **アルファブレンディング**の有効
        - `BOOL LogicOpEnable`: **ロジック演算**の有効。(SrcとDest間で論理演算する)
        - BlendEnable = TRUE
          - **enum D_BLEND** `⟪Src¦Dest⟫Blend＠❰Alpha❱`: `D_BLEND～`: **ブレンド係数**
            - `⟪_ZERO¦_ONE⟫`: `_ZERO:0`, `_ONE:1`
            - `＠❰_INV❱⟪_SRC＠❰1❱¦_DEST⟫⟪_COLOR¦_ALPHA⟫`: `_INV:1 - ～`, `＠❰1❱:RenderTarget[1]`, `_COLOR:.rgb`, `_ALPHA:.a`
            - `＠❰_INV❱⟪_BLEND¦_ALPHA⟫_FACTOR`: `～_FACTOR`は`OMSetBlendFactor(FLOAT BlendFactor[4])`で設定。`_BLEND:.rgb`, `_ALPHA:.a`
            - `_SRC_ALPHA_SAT`: `min(SRC.a, 1 - DEST.a)`
          - **enum D_BLEND_OP** `BlendOp＠❰Alpha❱`: `D_BLEND_OP～`: **ブレンド演算**
            - `_ADD`: `SRC + DEST`
            - `＠❰_REV❱_SUBTRACT`: `SRC - DEST`。`_REV`は`DEST - SRC`
            - `⟪_MIN¦_MAX⟫`: `⟪min¦max⟫(SRC, DEST)`
        - LogicOpEnable = TRUE
          - **enum D_LOGIC_OP** `LogicOp`: `D_LOGIC_OP～`: `SRC`と`DEST`を**論理演算**
            - `⟪_CLEAR¦_SET⟫`: `_CLEAR:0を出力`, `_SET:1を出力(全てのbitが1)`
            - `_COPY＠❰_INVERTED❱`: `SRC`, `_INVERTED:~SRC`
            - `⟪_NOOP¦_INVERT⟫`: `_NOOP:DEST`, `_INVERT:~DEST`
            - `⟪_AND¦_OR⟫＠⟪_INVERTED¦_REVERSE⟫`: `_INVERTED: ~SRC ⟪&¦|⟫ DEST`, `_REVERSE: SRC ⟪&¦|⟫ ~DEST`
            - `_N⟪AND¦OR⟫`: `_NAND:~(SRC & DEST)`, `_NOR:~(SRC | DEST)`
            - `⟪_XOR¦_EQUIV⟫`: `_XOR:SRC ^ DEST`, `_EQUIV:~(SRC ^ DEST)`
        - ⟪BlendEnable¦LogicOpEnable⟫ = TRUE
          - **enum D_COLOR_WRITE_ENABLE** `RenderTargetWriteMask`: `D_COLOR_WRITE_ENABLE～`: **RT書き込みマスク**
            - `⟪_RED¦_GREEN¦_BLUE¦_ALPHA⟫`: ⟪`.r`¦`.g`¦`.b`¦`.a`⟫それぞれのマスク
            - `_ALL`: `(_RED | _GREEN | _BLUE | _ALPHA)`
    - MSAA
      - `UINT SampleMask`: `MSAA`の**サンプル点**の**有効無効bitフラグ**。(`UINT`==`x32`)
      - **struct DXGI_SAMPLE_DESC** `SampleDesc`: `R_Resource`に設定された`DXGI_SAMPLE_DESC`と一致している必要があるらしい
        - `UINT Count`: **サンプル点**の数。`⟪1～32⟫`。(多くのハードウェアは**2の累乗に最適化**され`MSAAx8`までサポート)
        - `UINT Quality`: その`Count`数についての**品質レベル**。(通常は`0`が設定され、品質は**ハードウェア依存**)
    - ストリームアウトプット: **struct D_STREAM_OUTPUT_DESC** `StreamOutput`: `IASetVertexBuffers(..,VBV[])`から**Stream**としてシェーダーで処理され`SOSetTargets(..,SOBV[])`に出力する
      - **struct D_SO_DECLARATION_ENTRY**[] `pSODeclaration`:
        - 入力
          - `UINT Stream`: Streamを指定(`cmdList->IASetVertexBuffers(..,`**VBV[]**`)`)の`[]`のインデックス
          - `LPCSTR SemanticName`; `UINT SemanticIndex`; `BYTE StartComponent`; `BYTE ComponentCount`: **データ位置**と範囲。(`Component`は`vector.xyzw`の`xyzw`の範囲)
        - 出力
          - `BYTE OutputSlot`: `cmdList->SOSetTargets(..,`**SOBV[]**`)`の`[]`のインデックス
      - `UINT[] pBufferStrides`: `D_SO_DECLARATION_ENTRY.OutputSlot`で指定した**各スロット**の**ストライド**
      - `UINT RasterizedStream`: ラスタライズするStreamを指定。(Geometryシェーダーは複数Streamを出力することがあるが**Vertexシェーダー**は常に**Streamが1**つなので意味をなさない)
    - **struct D_CACHED_PIPELINE_STATE** `CachedPSO`:
      `R_PipelineState->GetCachedBlob(out ID3DBlob)`で`ID3DBlob`を取得し、それをココに設定して、同じ設定の`D_GRAPHICS_PIPELINE_STATE_DESC`にできて効率よく再利用できる
      - `void* pCachedBlob`: `ID3DBlob`の**先頭アドレス**。`ID3DBlob->GetBufferPointer()`を設定する
      - `SIZE_T CachedBlobSizeInBytes`: `ID3DBlob`の**長さ**。`ID3DBlob->GetBufferSize()`を設定する
    - **enum D_PIPELINE_STATE_FLAGS** `Flags`: `D_PIPELINE_STATE_FLAG⟪_NONE¦_TOOL_DEBUG⟫`:
      `_TOOL_DEBUG`: 開発ツール(**RenderDoc**, **PIX**など)によるパイプラインの**デバッグや解析**を行いやすくする

- ☆**D3DCompileFromFile**`(LPCWSTR pFileName, D3D_SHADER_MACRO[],., LPCSTR pEntryPoint, LPCSTR pTarget, UINT Flags1,., out ID3DBlob ppCode,.)`
  『シェーダーファイル(`pFileName`)をコンパイルし`ID3DBlob ppCode`を生成』
  - `LPCWSTR pFileName`: コンパイルする**HLSLファイルのパス**
  - **struct D3D_SHADER_MACRO**[]:
    - `LPCSTR Name`; `LPCSTR Definition`: `#define ｢ Name ｣ ｢ Definition ｣`。(最後はnull終端(`{nullptr, nullptr}`)を入れる)
  - `LPCSTR pEntryPoint`: **エントリポイント関数名**(`vert`や`frag`など)
  - `LPCSTR pTarget`: **ターゲットプロファイル**: `⟪vs¦ps¦gs¦hs¦ds¦cs⟫_⟪4_0¦4_1¦5_0¦5_1¦6_0⟫`
    **シェーダーモデル**
      `4_0`: DirectX 10
      `4_1`: DirectX 10.1
      `5_0`: DirectX 11/12
      `5_1`: DirectX 12
      `6_0`: DirectX 12 (DXR)
  - `UINT Flags1`: **コンパイルオプション**: `D3DCOMPILE_⟪_DEBUG¦_SKIP⟪_VALIDATION¦_OPTIMIZATION⟫¦_OPTIMIZATION_LEVEL⟪0¦1¦2¦3⟫⟫`

### コマンド系

- **enum D_COMMAND_LIST_TYPE**: `D_COMMAND_LIST_TYPE～`: `⟪_DIRECT¦_BUNDLE¦_COMPUTE¦_COPY¦_VIDEO_DECODE¦_VIDEO_PROCESS¦_VIDEO_ENCODE⟫`
  - `✖＄DIRECT＝❰D_COMMAND_LIST_TYPE.D_COMMAND_LIST_TYPE_DIRECT❱`

#### R_CommandQueue

- ☆`R_Device->`**CreateCommandQueue**`(∫DIRECT∫, out R_CommandQueue)`
- ☆`R_CommandQueue->`**ExecuteCommandLists**`(R_CommandList[] ppCommandLists)`: `R_CommandList`を`R_CommandQueue`で**実行**
  - **R_CommandList**[] `ppCommandLists`: `R_⟪Graphics¦Compute⟫CommandList`を設定

#### R_CommandAllocator

- ☆`R_Device->`**CreateCommandAllocator**`(∫DIRECT∫, out R_CommandAllocator)`
- ☆`R_CommandAllocator->`**Reset**`()`: アロケータをリセット

#### R_GraphicsCommandList

- ☆`R_Device->`**CreateCommandList**`(∫DIRECT∫, R_CommandAllocator, R_PipelineState, out R_GraphicsCommandList)`:
  `R_GraphicsCommandList`は`R_CommandAllocator`を内包している
  - `R_PipelineState`: 作成時に**PSO**を設定可能

##### 基本 (Unity.drawio/ページ19 のイメージ)

###### RTV,DSVのSetとClear

- ☆`R_GraphicsCommandList->`**OMSetRenderTargets**`(..)`:
  - `UINT NumRenderTargetDescriptors`: `pRenderTargetDescriptors`の配列の設定要素数 **または** `RTsSingleHandleToDescriptorRange == TRUE`時`R_DescriptorHeap`の設定要素数
  - **struct D_CPU_DESCRIPTOR_HANDLE**[] `pRenderTargetDescriptors`: `RTV[]`を設定。`0`だと**デプス**のみ。`2`以上だと**MRT**
  - `BOOL RTsSingleHandleToDescriptorRange`: `TRUE`:`pRenderTargetDescriptors`に**要素が一つ**の配列を設定して、`R_DescriptorHeap`の範囲を設定する。(`D_DESCRIPTOR_RANGE`とは関係ない)
  - **struct D_CPU_DESCRIPTOR_HANDLE** `pDepthStencilDescriptor`: `DSV`を設定

- ☆`R_GraphicsCommandList->`**ClearDepthStencilView**`(..)`:
  - **struct D_CPU_DESCRIPTOR_HANDLE** `DepthStencilView`: クリアする`DSV`
    - `SIZE_T ptr`: `R_DescriptorHeap`内ポインタ
  - **enum D_CLEAR_FLAGS** `ClearFlags`: `D_CLEAR_FLAG⟪_DEPTH¦_STENCIL⟫`: クリアする`PlaneSlice`。(組合せ可能)
  - `FLOAT Depth`: `_DEPTH`の時、クリアする**デプス値**(0.0～1.0)
  - `UINT8 Stencil`: `_STENCIL`の時、クリアする**ステンシル値**(0～255)
  - **struct D_RECT**[] `pRects`: クリアする**範囲**。(`nullptr`の場合、全範囲)
    - `LONG ❰left, top, right, bottom❱`

- ☆`R_GraphicsCommandList->`**ClearRenderTargetView**`(..)`:
  - **struct D_CPU_DESCRIPTOR_HANDLE** `RenderTargetView`: クリアする`RTV`
  - `FLOAT[4] ColorRGBA`: クリアする**カラー値**(0.0～1.0 x4)
  - **struct D_RECT**[] `pRects`: クリアする**範囲**。(`nullptr`の場合、全範囲)

- ☆`R_GraphicsCommandList->`**ResolveSubresource**`(..)`: MSAAのPixel内の**SubPixelを平均化**して1つのPixelにするだけ。(Unity.drawio/ページ38 参照)
  - `R_Resource p⟪Src¦Dst⟫Resource`; `UINT ⟪Src¦Dst⟫Subresource`: `Src`の`Subresource`から`Dst`の`Subresource`へ**リゾルブ**する
  - `DXGI_FORMAT Format`: リゾルブに使用するフォーマット。(`Src`と`Dst`のフォーマットは一致している必要がある(この引数いる?))

- `Clear⟪RTV¦DSV¦UAV⟪Uint¦Float⟫⟫`

- `OMSetBlendFactor`
- `OMSetStencilRef`

###### PSOと描画範囲の設定

- ☆`R_GraphicsCommandList->`**SetPipelineState**`(..)`
  Unityの**SetPass**にあたる?頻繁に変更するとパフォーマンスが落ちる
  - **R_PipelineState** `pPipelineState`: **PSO**を指定

- `R_GraphicsCommandList->ClearState(..)`: `R_GraphicsCommandList`で設定した**全てのパイプラインステート**(`OMSetRenderTargets`なども)をリセットする。
  - `R_PipelineState`を設定すると、リセット後に`PSO`を設定。(`nullptr`を設定すると`PSO`もリセット)

- ☆`R_GraphicsCommandList->`**RSSetViewports**`(..)`:
  ラスタライズ時に指定したスクリーン空間の範囲に**ストレッチ**する。(Zも)
  - **struct D_VIEWPORT**[] `pViewports`:
    - XY
      - `FLOAT TopLeft⟪X¦Y⟫`: 左上座標
      - `FLOAT ⟪Width¦Height⟫`: 幅
    - Z
      - `FLOAT ⟪Min¦Max⟫Depth`: デプス範囲

- ☆`R_GraphicsCommandList->`**RSSetScissorRects**`(..)`
  ラスタライズ時に指定したスクリーン空間の範囲に**クランプ**する (クランプ外のPixelシェーダーは起動しない)
  - **D_RECT**[] `pRects`

###### シェーダーリソースSet

- ☆`R_GraphicsCommandList->`**SetGraphicsRootSignature**`(..)`
  - **R_RootSignature** `pRootSignature`: **R_RootSignature**を指定

- ☆`R_GraphicsCommandList->`**SetDescriptorHeaps**`(..)`
  `SetGraphicsRootDescriptorTable(..)`を呼び出す前に必ずディスクリプタヒープがバインドされている必要があるらしい
  - **R_DescriptorHeap**[] `ppDescriptorHeaps`: バインドするディスクリプタヒープ配列を指定

- ☆`R_GraphicsCommandList->`**SetGraphicsRootDescriptorTable**`(..)`
  `D_ROOT_DESCRIPTOR_TABLE`(Key)と`R_DescriptorHeap`(Value)をすり合わせてDictを作る
  - `UINT RootParameterIndex`: `SetGraphicsRootSignature(..)`で設定した`R_RootSignature.D_ROOT_PARAMETER[]`のインデックスを指定
  - **D_GPU_DESCRIPTOR_HANDLE** `BaseDescriptor`: `R_DescriptorHeap->GetGPUDescriptorHandleForHeapStart()`を指定

- `Set⟪Compute¦Graphics⟫RootSignature`
- `Set⟪Compute¦Graphics⟫Root⟪DescriptorTable¦32BitConstant＠❰s❱¦CBV¦SRV¦UAV⟫`

###### Topology,VBV,IBVのSet

- ☆`R_GraphicsCommandList->`**IASetPrimitiveTopology**(..)
  - **enum D_PRIMITIVE_TOPOLOGY** `PrimitiveTopology`: `D3D_PRIMITIVE_TOPOLOGY～`
    - `_UNDEFINED`:未定義(エラー)
    - `_POINTLIST`
    - `_⟪LINE¦TRIANGLE⟫⟪LIST¦STRIP⟫＠❰ADJ❱`: `ADJ(Adjacencyアジャセンシー)`:隣接情報(主にジオメトリシェーダーで使用)
    - `_⟪1～32⟫_CONTROL_POINT_PATCHLIST`: >パッチプリミティブ(曲線やサーフェスの描画に使用される)。⟪1～32⟫の制御点を持つパッチリストを指定します。(テッセレーションシェーダーで使用)

- ☆`R_GraphicsCommandList->`**IASetVertexBuffers**`(..)`
  - `UINT StartSlot`: `D_INPUT_ELEMENT_DESC.InputSlot`を指定。(SlotはStream)
  - **struct D_VERTEX_BUFFER_VIEW**[] `pViews`: `StartSlot`から連続する`VBV`を指定
    - `D_GPU_VIRTUAL_ADDRESS BufferLocation`: `R_Resource->GetGPUVirtualAddress()`(GPUアドレス)
    - `UINT SizeInBytes`: `R_Resource`の**サイズ**
    - `UINT StrideInBytes`: `R_Resource`の頂点の**ストライド**

- ☆`R_GraphicsCommandList->`**IASetIndexBuffer**(..)
  - **struct D_INDEX_BUFFER_VIEW** `pView`: `IBV`を指定。(`nullptr`を設定するとバインド解除)
    - `D_GPU_VIRTUAL_ADDRESS BufferLocation`: `R_Resource->GetGPUVirtualAddress()`(GPUアドレス)
    - `UINT SizeInBytes`: `R_Resource`の**サイズ**
    - `DXGI_FORMAT Format`: `R_Resource`のIndexの**フォーマット**(`DXGI_FORMAT_R⟪16¦32⟫_UINT`(`IBV`のストライド))

- `SOSetTargets`

###### ドローコール,ディスパッチ,コピー,破棄

- ☆`R_GraphicsCommandList->`**DrawIndexedInstanced**`(..)`: サブメッシュ毎にInstancingする
  - IBV範囲(サブメッシュ)
    - `UINT StartIndexLocation; UINT IndexCountPerInstance`: ＄サブメッシュ範囲＝❰`StartIndexLocation` ～ `StartIndexLocation` + `IndexCountPerInstance`-1❱
    - `INT BaseVertexLocation`: **IBVtoVBV**(∫サブメッシュ範囲∫) + `BaseVertexLocation`。(**IBVtoVBV**は単なる`IBV`へのインデックス参照(`IBV[∫サブメッシュ範囲∫] == VBV`))
      - `BaseVertexLocation`は`D_INDEX_BUFFER_VIEW.Format`が`DXGI_FORMAT_R16_UINT`の時、`65536以上`の`VBV`の頂点を扱うために**IBVtoVBV**した値に**加算**される
  - Instancing範囲
    - `UINT StartInstanceLocation; UINT InstanceCount`: `StartInstanceLocation` ～ `StartInstanceLocation` + `InstanceCount`-1

- ☆`R_GraphicsCommandList->`**ExecuteIndirect**`(..)`: `R_CommandSignature`を`pArgumentBuffer`を使って`MaxCommandCount`分、実行する。(Unity.drawio/ページ37 参照)
  - **R_CommandSignature** `pCommandSignature`: `CreateCommandSignature`を参照
  - `UINT MaxCommandCount`: **実行されるコマンドセットの数**
  - `R_Resource pArgumentBuffer`: 引数バッファ (`pCommandSignature(コマンドセット)` * `MaxCommandCount` の引数)
  - `UINT64 ArgumentBufferOffset`: `pArgumentBuffer`の開始位置バイトオフセット
  - `R_Resource pCountBuffer`: (オプション)**実行されるコマンドセットの数**をバッファとして渡す。(実行されるコマンド数は`pCountBuffer`<=`MaxCommandCount`という関係)
  - `UINT64 CountBufferOffset`: (オプション) `pCountBuffer`の開始位置バイトオフセット

  - ☆`R_Device->`**CreateCommandSignature**`(.., out R_CommandSignature)`:
    **一連のコマンド実行(コマンドセット)**を設定し、それにより**対応する引数郡**が決まり、その引数郡の間の`ByteStride`を設定する
    - **R_RootSignature** `pRootSignature`: `D_INDIRECT_ARGUMENT_TYPE⟪_CONSTANT¦⟪_C_B¦_S_R¦_U_A⟫_VIEW⟫`の時、`RootParameterIndex`を参照するので、その`R_RootSignature`を設定
    - **struct D_COMMAND_SIGNATURE_DESC** `pDesc`:
      - `UINT ByteStride`: 配列`pArgumentDescs`の各要素の`D_INDIRECT_ARGUMENT_DESC`.`D_INDIRECT_ARGUMENT_TYPE`に対応する**引数**の**合計**を設定する
      - **struct D_INDIRECT_ARGUMENT_DESC**[] `pArgumentDescs`:
        - **enum D_INDIRECT_ARGUMENT_TYPE** `Type`: `D_INDIRECT_ARGUMENT_TYPE～`: ❰=> `(..)`❱は、`R_GraphicsCommandList->`**～**`(..)`の引数(`(..)`)に入るという意味
          - `_DRAW`: `R_GraphicsCommandList->`**DrawInstanced**`(..)`: に対応
            - 引数 => `(..)`
              - **struct D_DRAW_ARGUMENTS**:
                - `UINT VertexCountPerInstance`
                - `UINT InstanceCount`
                - `UINT StartVertexLocation`
                - `UINT StartInstanceLocation`
          - `_DRAW_INDEXED`: `R_GraphicsCommandList->`**DrawIndexedInstanced**`(..)`: に対応
            - 引数 => `(..)`
              - **struct D_DRAW_INDEXED_ARGUMENTS**:
                - `UINT IndexCountPerInstance`
                - `UINT InstanceCount`
                - `UINT StartIndexLocation`
                - `INT BaseVertexLocation`
                - `UINT StartInstanceLocation`
          - `_DISPATCH`: `R_GraphicsCommandList->`**Dispatch**`(..)`: に対応
            - 引数 => `(..)`
              - **struct D_DISPATCH_ARGUMENTS**:
                - `UINT ThreadGroupCountX`
                - `UINT ThreadGroupCountY`
                - `UINT ThreadGroupCountZ`
          - `_VERTEX_BUFFER_VIEW`: `R_GraphicsCommandList->`**IASetVertexBuffers**`(..)`: に対応
            - union{_VERTEX_BUFFER_VIEW} `VertexBuffer` => `(..)`
              - `UINT Slot`
            - 引数 => `(..)`
              - **struct D_VERTEX_BUFFER_VIEW**:
                - `D_GPU_VIRTUAL_ADDRESS BufferLocation`
                - `UINT SizeInBytes`
                - `UINT StrideInBytes`
          - `_INDEX_BUFFER_VIEW`: `R_GraphicsCommandList->`**IASetIndexBuffer**`(..)`: に対応
            - 引数 => `(..)`
              - **struct D_INDEX_BUFFER_VIEW**:
                - `D_GPU_VIRTUAL_ADDRESS BufferLocation`
                - `UINT SizeInBytes`
                - `DXGI_FORMAT Format`
          - `_CONSTANT`: `R_GraphicsCommandList->`**Set⟪Graphics¦Compute⟫Root32BitConstants**`(..)`: に対応
            - union{_CONSTANT} `Constant` => `(..)`
              - `UINT RootParameterIndex`
              - `UINT DestOffsetIn32BitValues`
              - `UINT Num32BitValuesToSet`
            - 引数 => `(..)`
              - `void* pSrcData`: `pSrcData = ｢32bitType｣ ｢変数｣[Num32BitValues]`(32bitプリミティブ型[])
          - `⟪_C_B¦_S_R¦_U_A⟫_VIEW`: `R_GraphicsCommandList->`**Set⟪Graphics¦Compute⟫Root⟪CB¦SR¦UA⟫View**`(..)`: に対応
            - union{⟪_C_B¦_S_R¦_U_A⟫_VIEW} `⟪CB¦SR¦UA⟫View` => `(..)`
              - `UINT RootParameterIndex`
            - 引数 => `(..)`
              - `D_GPU_VIRTUAL_ADDRESS BufferLocation`
          - _DISPATCH⟪_RAYS¦_MESH⟫
        - union{D_INDIRECT_ARGUMENT_TYPE}
          - **struct <無名>** `VertexBuffer`: `_VERTEX_BUFFER_VIEW`時:
            - `UINT Slot`:
          - **struct <無名>** `Constant`: `_CONSTANT`時:
            - `UINT RootParameterIndex`:
            - `UINT DestOffsetIn32BitValues`:
            - `UINT Num32BitValuesToSet`:
          - **struct <無名>** `⟪CB¦SR¦UA⟫View`: `⟪_C_B¦_S_R¦_U_A⟫_VIEW`時:
            - `UINT RootParameterIndex`:

- `Draw＠❰Indexed❱Instanced`

- `Dispatch`

- `Copy⟪｡Resource¦Tiles｡¦｡⟪Buffer¦Texture⟫Region｡⟫`

- `R_GraphicsCommandList->DiscardResource(..)`: `R_Resource`を**破棄**する
  - `R_Resource`: 破棄する`R_Resource`を指定
  - `D_DISCARD_REGION`: 破棄する**矩形**と**SubResource**を指定。(`nullptr`を設定すると全体を破棄)

##### その他

###### コマンドリスト操作

- ☆`R_GraphicsCommandList->`**Close**`()`: `R_GraphicsCommandList`を**クローズ**。検証チェックなどを行う。再び記録するには`Reset()`する

- ☆`R_GraphicsCommandList->`**Reset**`(..)`: `Close()`した後のみ実行でき、リセットして新しいコマンドを記憶できるようにする
  - **R_CommandAllocator** `pAllocator`: リセットの際に`R_CommandAllocator`を再設定
  - **R_PipelineState** `pInitialState`:　リセットの際に`R_PipelineState`を再設定

###### 構造化

- `R_GraphicsCommandList->SetPredication(..)`: バッファの値によりこの次のコマンドを実行するか決める。つまり**ifと同等**。`ExecuteBundle`と組合せられるが、再帰ループはできない
  - `R_Resource pBuffer`: 条件の値を格納するバッファ。(読まれるのは`⟪4¦8⟫バイト`)
  - `UINT64 AlignedBufferOffset`: `pBuffer`の開始位置バイトオフセット(`64バイトアライメント`である必要がある)
  - **enum D_PREDICATION_OP** `Operation`: `D_PREDICATION_OP⟪_EQUAL_ZERO¦_NOT_EQUAL_ZERO⟫`: `_EQUAL_ZERO`:if(`pBuffer` == `0`), `_NOT_EQUAL_ZERO`:if(`pBuffer` != `0`)

- `R_GraphicsCommandList->ExecuteBundle(..)`: `R_GraphicsCommandList`の中に設定された**コマンド郡**を**差し込む**だけ(単に関数(CmdList)を差し込むようなもの)
  - `R_GraphicsCommandList pBundle`: 差し込むコマンド郡。(`Close()`されていること)
    - 含められないコマンド: 再帰不可:❰`SetPredication`, `ExecuteBundle`❱, `Clear⟪RT¦DS¦UA⟫View`, `OMSetRenderTargets`, `ResourceBarrier`

- `R_GraphicsCommandList->`**Signal**`(R_Fence, UINT64 fenceValue)`: `### R_Fence`を参照

###### バリア

- ☆`R_GraphicsCommandList->`**ResourceBarrier**`(D_RESOURCE_BARRIER[] pBarriers)`: **### D_RESOURCE_BARRIER**を参照

###### プロファイリング

- `⟪Begin¦End⟫Event, SetMarker`: コマンドの処理の流れを追跡するマーカー
  - 基本的に**PIXイベント**から呼ばれるみたい。大体呼び出し方は、
    `R_GraphicsCommandList->PIXBeginEvent(color, name)`
    ～処理～
    `R_GraphicsCommandList->PIXEndEvent()`
- `⟪Begin¦End⟫Query, ResolveQueryData`: タイムスタンプクエリとオクルージョンクエリ。(実行時間とジオメトリの隠蔽情報を計測(とその他統計情報)) (プロファイリング要員)

## コード例

CmdList->**ExecuteIndirect**(..), **R_Resource生成**, R_Resource->**⟪Map¦Unmap⟫**(..), CmdList->**ResourceBarrier**(..), CmdList->**CopyBufferRegion**(..)

**ChatGPT o1-preview**へのプロンプト: `DirectX12のExecuteIndirectコマンドで、MaxCommandCountが2以上で、D3D12_INDIRECT_ARGUMENT_DESC.Typeが2種類以上のコード例を書いてください。`

```cpp
// 必要なヘッダーファイル
#include <d3d12.h>
#include <wrl.h>
using namespace Microsoft::WRL;

// デバイスおよびコマンドリストが既に作成されていると仮定します。
ID3D12Device* device = /* デバイスへのポインタ */;
ID3D12GraphicsCommandList* commandList = /* コマンドリストへのポインタ */;

// コマンドシグネチャの引数を定義
D3D12_INDIRECT_ARGUMENT_DESC argumentDescs[2] = {};

// 1つ目の引数: 定数バッファの設定
argumentDescs[0].Type = D3D12_INDIRECT_ARGUMENT_TYPE_CONSTANT;
argumentDescs[0].Constant.RootParameterIndex = 0; // ルートパラメータのインデックス
argumentDescs[0].Constant.DestOffsetIn32BitValues = 0;
argumentDescs[0].Constant.Num32BitValuesToSet = 4; // 4つの32ビット定数を設定

// 2つ目の引数: DrawIndexed
argumentDescs[1].Type = D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED;

// コマンドシグネチャを作成
D3D12_COMMAND_SIGNATURE_DESC commandSignatureDesc = {};
commandSignatureDesc.pArgumentDescs = argumentDescs;
commandSignatureDesc.NumArgumentDescs = _countof(argumentDescs);
commandSignatureDesc.ByteStride = sizeof(UINT) * 4 /* 定数部分 */ + sizeof(D3D12_DRAW_INDEXED_ARGUMENTS);
commandSignatureDesc.NodeMask = 0;

ComPtr<ID3D12CommandSignature> commandSignature;
HRESULT hr = device->CreateCommandSignature(
    &commandSignatureDesc,
    nullptr, // RootSignature、必要に応じて設定
    IID_PPV_ARGS(&commandSignature)
);
if (FAILED(hr)) {
    // エラーハンドリング
}

// 引数バッファを作成（3つのコマンド分）
const UINT MaxCommandCount = 3;
const UINT64 argumentBufferSize = MaxCommandCount * commandSignatureDesc.ByteStride;

ComPtr<ID3D12Resource> argumentBuffer;
// ヒーププロパティ: DEFAULT
D3D12_HEAP_PROPERTIES heapProperties = {};
heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;

// リソースディスクリプタ: バッファ
D3D12_RESOURCE_DESC resourceDesc = {};
resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
resourceDesc.Width = argumentBufferSize;
resourceDesc.Height = 1;
resourceDesc.DepthOrArraySize = 1;
resourceDesc.MipLevels = 1;
resourceDesc.Format = DXGI_FORMAT_UNKNOWN;
resourceDesc.SampleDesc.Count = 1;
resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;

hr = device->CreateCommittedResource(
    &heapProperties,
    D3D12_HEAP_FLAG_NONE,
    &resourceDesc,
    D3D12_RESOURCE_STATE_COPY_DEST,
    nullptr,
    IID_PPV_ARGS(&argumentBuffer)
);
if (FAILED(hr)) {
    // エラーハンドリング
}

// 引数データをCPU側で準備
struct IndirectArguments
{
    UINT Constants[4];
    D3D12_DRAW_INDEXED_ARGUMENTS DrawIndexedArgs;
};

IndirectArguments argumentData[MaxCommandCount];

// 各コマンドの引数を設定
for (UINT i = 0; i < MaxCommandCount; ++i)
{
    // 定数（例としてインデックスを設定）
    argumentData[i].Constants[0] = i;
    argumentData[i].Constants[1] = i + 1;
    argumentData[i].Constants[2] = i + 2;
    argumentData[i].Constants[3] = i + 3;

    // DrawIndexedの引数を設定
    argumentData[i].DrawIndexedArgs.IndexCountPerInstance = 36; // 例: キューブのインデックス数
    argumentData[i].DrawIndexedArgs.InstanceCount = 1;
    argumentData[i].DrawIndexedArgs.StartIndexLocation = 0;
    argumentData[i].DrawIndexedArgs.BaseVertexLocation = 0;
    argumentData[i].DrawIndexedArgs.StartInstanceLocation = 0;
}

// アップロード用バッファを作成
ComPtr<ID3D12Resource> uploadBuffer;
heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;

hr = device->CreateCommittedResource(
    &heapProperties,
    D3D12_HEAP_FLAG_NONE,
    &resourceDesc,
    D3D12_RESOURCE_STATE_GENERIC_READ,
    nullptr,
    IID_PPV_ARGS(&uploadBuffer)
);
if (FAILED(hr)) {
    // エラーハンドリング
}

// アップロードバッファにデータを書き込む
void* mappedData = nullptr;
D3D12_RANGE readRange = {0, 0}; // 読み込まないのでゼロ
hr = uploadBuffer->Map(0, &readRange, &mappedData);
if (FAILED(hr)) {
    // エラーハンドリング
}
memcpy(mappedData, argumentData, argumentBufferSize);
uploadBuffer->Unmap(0, nullptr);

// 引数バッファにデータをコピー
commandList->CopyBufferRegion(argumentBuffer.Get(), 0, uploadBuffer.Get(), 0, argumentBufferSize);

// コピーの完了を待つためにバリアを設定
D3D12_RESOURCE_BARRIER barrier = {};
barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
barrier.Transition.pResource = argumentBuffer.Get();
barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_COPY_DEST;
barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_INDIRECT_ARGUMENT;
barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
commandList->ResourceBarrier(1, &barrier);

// ExecuteIndirectを呼び出す
commandList->ExecuteIndirect(
    commandSignature.Get(),  // コマンドシグネチャ
    MaxCommandCount,         // 実行するコマンド数
    argumentBuffer.Get(),    // 引数バッファ
    0,                       // 引数バッファのオフセット
    nullptr,                 // CountBuffer（必要に応じて設定）
    0                        // CountBufferのオフセット
);

// ここでバリアを戻す（必要に応じて）
barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_INDIRECT_ARGUMENT;
barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_COPY_DEST; // 次の用途に合わせて変更
commandList->ResourceBarrier(1, &barrier);

// コマンドの終了など、他の処理を続ける
```
