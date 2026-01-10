# core_hlslメモ

## #define定義

### YとZ

- D3D11系
```C
#define UNITY_UV_STARTS_AT_TOP 1
#define UNITY_REVERSED_Z 1
#define UNITY_NEAR_CLIP_VALUE (1.0)
#define UNITY_RAW_FAR_CLIP_VALUE (0.0)
```
- GLCore,GLES3
```C
#define UNITY_NEAR_CLIP_VALUE (-1.0)
#define UNITY_RAW_FAR_CLIP_VALUE (1.0)
```

### SV_セマンティクス

```C
//ID
#define VERTEXID_SEMANTIC SV_VertexID
#define INSTANCEID_SEMANTIC SV_InstanceID
//FACE
#define FRONT_FACE_SEMANTIC SV_IsFrontFace
#define FRONT_FACE_TYPE bool
#define IS_FRONT_VFACE(VAL, FRONT, BACK) ((VAL) ? (FRONT) : (BACK))
```

### プラットのフォームシェーダー機能

```C
#define PLATFORM_SUPPORTS_EXPLICIT_BINDING
#define PLATFORM_NEEDS_UNORM_UAV_SPECIFIER
#define PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER
#define PLATFORM_SUPPORTS_PRIMITIVE_ID_IN_PIXEL_SHADER
```

### SRPBatcherのcbuffer定義

```C
#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };
```

### テクスチャ

- マルチプラットフォーム(`PLATFORM_`)対応(D3D11) => XR互換(Core) => `_GlobalMipBias`対応(Core)
  - XR互換: `#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)`時:
    - `SLICE_ARRAY_INDEX`対応
      - `#define SLICE_ARRAY_INDEX   unity_StereoEyeIndex`
      - `#define ⟪SAMPLE_¦LOAD_¦GATHER_⟫TEXTURE2D_X＠❰_LOD❱(textureName, samplerName, coord2)`
                `⟪SAMPLE_¦LOAD_¦GATHER_⟫TEXTURE2D_ARRAY＠❰_LOD❱(textureName, samplerName, coord2, SLICE_ARRAY_INDEX)`
    - フレームバッファーフェッチ:
      - 対応時: `#define FRAMEBUFFER_INPUT_X_～(idx)`
                        `FRAMEBUFFER_INPUT_～(idx)`
                `#define LOAD_FRAMEBUFFER_X_INPUT(idx, v2fname)`
                        `LOAD_FRAMEBUFFER_INPUT(idx, v2fname)`
      - 非対応時: `Texture2DArray<T> _UnityFBInput##idx`の`.Load(..)` か `cbuffer ..{half4 hlslcc_fbinput_##idx, ..}`の`half4`定数
        - もしかして`_UnityFBInput##idx`が、前のRT描画結果のバッファーになっている?

#### テクスチャ, サンプラー 宣言

- テクスチャ宣言:
  - `＠⟪TYPED_¦RW_⟫｡｡TEXTURE｡｡⟪2D＠❰_X❱¦CUBE¦3D⟫｡｡＠❰_ARRAY❱｡｡｡｡(＠❰type,❱ textureName)`
```C
//テクスチャ
#define TEXTURE2D_X(textureName)              Texture2D textureName
#define TEXTURE2D_ARRAY(textureName)          Texture2DArray textureName
#define TEXTURECUBE(textureName)              TextureCube textureName
#define TEXTURECUBE_ARRAY(textureName)        TextureCubeArray textureName
#define TEXTURE3D(textureName)                Texture3D textureName
//テクスチャ<タイプ>
#define TYPED_TEXTURE2D_X(type, textureName)     Texture2D<type> textureName
#define TYPED_TEXTURE2D_ARRAY(type, textureName) Texture2DArray<type> textureName
#define TYPED_TEXTURE3D(type, textureName)       Texture3D<type> textureName
//RWテクスチャ<タイプ>
#define RW_TEXTURE2D(type, textureName)          RWTexture2D<type> textureName
#define RW_TEXTURE2D_ARRAY(type, textureName)    RWTexture2DArray<type> textureName
#define RW_TEXTURE3D(type, textureName)          RWTexture3D<type> textureName
```

- サンプラー宣言:
  - `SAMPLER｡｡＠❰_CMP❱｡｡｡｡(samplerName)`
```C
#define SAMPLER(samplerName)                  SamplerState samplerName
#define SAMPLER_CMP(samplerName)              SamplerComparisonState samplerName
```

- テクスチャ＆サンプラー宣言: `＠❰_SHADOW❱_PARAM(textureName, samplerName)`を**テクスチャ宣言**の最後に付けると`, SAMPLER＠❰_CMP❱(samplerName)`が追加される

#### ロード, サンプリング

- ロード:
  - 2D:       `LOAD_TEXTURE2D＠❰_X❱｡｡｡＠⟪_LOD¦_MSAA⟫｡｡｡｡(textureName, unCoord2 ＠⟪,lod¦,sampleIndex⟫)`
  - 2D_ARRAY: `LOAD_TEXTURE2D_ARRAY｡｡＠⟪_LOD¦_MSAA⟫｡｡｡｡(textureName, unCoord2, index ＠⟪,lod¦,sampleIndex⟫)`
  - 3D:       `LOAD_TEXTURE3D｡｡｡｡｡｡｡｡｡＠❰_LOD❱｡｡｡｡｡｡｡｡｡｡｡(textureName, unCoord3 ＠❰,lod❱)`
```C
//2D
#define LOAD_TEXTURE2D_X(textureName, unCoord2)                                 textureName.Load(int3(unCoord2, 0))
#define LOAD_TEXTURE2D_X_LOD(textureName, unCoord2, lod)                        textureName.Load(int3(unCoord2, lod))
#define LOAD_TEXTURE2D_MSAA(textureName, unCoord2, sampleIndex)                 textureName.Load(unCoord2, sampleIndex)
//2D_ARRAY
#define LOAD_TEXTURE2D_ARRAY(textureName, unCoord2, index)                      textureName.Load(int4(unCoord2, index, 0))
#define LOAD_TEXTURE2D_ARRAY_LOD(textureName, unCoord2, index, lod)             textureName.Load(int4(unCoord2, index, lod))
#define LOAD_TEXTURE2D_ARRAY_MSAA(textureName, unCoord2, index, sampleIndex)    textureName.Load(int3(unCoord2, index), sampleIndex)
//3D
#define LOAD_TEXTURE3D(textureName, unCoord3)                                   textureName.Load(int4(unCoord3, 0))
#define LOAD_TEXTURE3D_LOD(textureName, unCoord3, lod)                          textureName.Load(int4(unCoord3, lod))
```
- サンプリング
  - 2D:         `SAMPLE_TEXTURE2D＠❰_X❱｡｡｡｡｡＠⟪_LOD¦_BIAS¦_GRAD⟫｡｡｡｡(textureName, samplerName, coord2 ｡｡｡｡｡｡｡｡｡＠⟪,lod¦,bias¦,dpdx,dpdy⟫)`
  - 2D_ARRAY:   `SAMPLE_TEXTURE2D_ARRAY｡｡｡｡＠⟪_LOD¦_BIAS¦_GRAD⟫｡｡｡｡(textureName, samplerName, coord2, index ｡｡＠⟪,lod¦,bias¦,dpdx,dpdy⟫)`
  - CUBE:       `SAMPLE_TEXTURECUBE｡｡｡｡｡｡｡｡｡＠⟪_LOD¦_BIAS⟫｡｡｡｡｡｡｡｡｡｡(textureName, samplerName, coord3  ｡｡｡｡｡｡｡｡｡＠⟪,lod¦,bias⟫)`
  - CUBE_ARRAY: `SAMPLE_TEXTURECUBE_ARRAY｡｡＠⟪_LOD¦_BIAS⟫｡｡｡｡｡｡｡｡｡｡(textureName, samplerName, coord3, index ｡｡｡＠⟪,lod¦,bias⟫)`
  - 3D:         `SAMPLE_TEXTURE3D｡｡｡｡｡｡｡｡｡｡｡＠⟪_LOD⟫｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡(textureName, samplerName, coord3 ｡｡｡｡｡｡｡｡｡｡＠❰,lod❱)`
```C
//2D
#define SAMPLE_TEXTURE2D_X(textureName, samplerName, coord2)                             textureName.SampleBias(samplerName, coord2, _GlobalMipBias.x)
#define SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, coord2, lod)                    textureName.SampleLevel(samplerName, coord2, lod)
#define SAMPLE_TEXTURE2D_BIAS(textureName, samplerName, coord2, bias)                    textureName.SampleBias(samplerName, coord2,  (bias + _GlobalMipBias.x))
#define SAMPLE_TEXTURE2D_GRAD(textureName, samplerName, coord2, dpdx, dpdy)              textureName.SampleGrad(samplerName, coord2, (dpdx * _GlobalMipBias.y), (dpdy * _GlobalMipBias.y))
//2D_ARRAY
#define SAMPLE_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)                  textureName.SampleBias(samplerName, float3(coord2, index), _GlobalMipBias.x)
#define SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, index, lod)         textureName.SampleLevel(samplerName, float3(coord2, index), lod)
#define SAMPLE_TEXTURE2D_ARRAY_BIAS(textureName, samplerName, coord2, index, bias)       textureName.SampleBias(samplerName, float3(coord2, index), (bias + _GlobalMipBias.x))
#define SAMPLE_TEXTURE2D_ARRAY_GRAD(textureName, samplerName, coord2, index, dpdx, dpdy) textureName.SampleGrad(samplerName, float3(coord2, index), dpdx, dpdy) //_GlobalMipBias.yない?
//CUBE
#define SAMPLE_TEXTURECUBE(textureName, samplerName, coord3)                             textureName.SampleBias(samplerName, coord3, _GlobalMipBias.x)
#define SAMPLE_TEXTURECUBE_LOD(textureName, samplerName, coord3, lod)                    textureName.SampleLevel(samplerName, coord3, lod)
#define SAMPLE_TEXTURECUBE_BIAS(textureName, samplerName, coord3, bias)                  textureName.SampleBias(samplerName, coord3, (bias + _GlobalMipBias.x))
//CUBE_ARRAY
#define SAMPLE_TEXTURECUBE_ARRAY(textureName, samplerName, coord3, index)                textureName.SampleBias(samplerName, float4(coord3, index), _GlobalMipBias.x)
#define SAMPLE_TEXTURECUBE_ARRAY_LOD(textureName, samplerName, coord3, index, lod)       textureName.SampleLevel(samplerName, float4(coord3, index), lod)
#define SAMPLE_TEXTURECUBE_ARRAY_BIAS(textureName, samplerName, coord3, index, bias)     textureName.SampleBias(samplerName, float4(coord3, index), (bias + _GlobalMipBias.x))
//3D
#define SAMPLE_TEXTURE3D(textureName, samplerName, coord3)                               textureName.Sample(samplerName, coord3)
#define SAMPLE_TEXTURE3D_LOD(textureName, samplerName, coord3, lod)                      textureName.SampleLevel(samplerName, coord3, lod)
```
- サンプリング＆Z比較 (MipLv0)
  - 2D:   `SAMPLE_TEXTURE2D｡｡｡｡＠❰_ARRAY❱｡｡_SHADOW｡｡｡｡(textureName, samplerName, coord3 ＠❰,index❱)`
  - CUBE: `SAMPLE_TEXTURECUBE｡｡＠❰_ARRAY❱｡｡_SHADOW｡｡｡｡(textureName, samplerName, coord4 ＠❰,index❱)`
```C
//2D
#define SAMPLE_TEXTURE2D_SHADOW(textureName, samplerName, coord3)                    textureName.SampleCmpLevelZero(samplerName, (coord3).xy, (coord3).z)
#define SAMPLE_TEXTURE2D_ARRAY_SHADOW(textureName, samplerName, coord3, index)       textureName.SampleCmpLevelZero(samplerName, float3((coord3).xy, index), (coord3).z)
//CUBE
#define SAMPLE_TEXTURECUBE_SHADOW(textureName, samplerName, coord4)                  textureName.SampleCmpLevelZero(samplerName, (coord4).xyz, (coord4).w)
#define SAMPLE_TEXTURECUBE_ARRAY_SHADOW(textureName, samplerName, coord4, index)     textureName.SampleCmpLevelZero(samplerName, float4((coord4).xyz, index), (coord4).w)
```
- ギャザー
  - 2D ＠⟪_RED¦_GREEN¦_BLUE¦_ALPHA⟫: `GATHER_｡｡＠⟪_RED¦_GREEN¦_BLUE¦_ALPHA⟫｡｡TEXTURE2D_X｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡(textureName, samplerName, coord2)`
  - 2D_ARRAY:                        `GATHER_｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡TEXTURE2D_ARRAY｡｡｡｡｡｡｡｡｡｡(textureName, samplerName, coord2,   index)`
  - CUBE＠❰_ARRAY❱:                   `GATHER_｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡TEXTURECUBE＠❰_ARRAY❱｡｡｡｡(textureName, samplerName, coord3 ＠❰,index❱)`
```C
//Gather～(..) ギャザー
  //float4 Texture2D.Gather(SamplerState s, float2 texCoord, int component):
    //>Gatherを使ってcomponent=0（赤チャンネル）の値を集める場合、関数は指定された座標の上下左右に位置する4つのピクセルの赤チャンネルの値をfloat4として返します。
    //>第3引数を省略した場合、デフォルトでは「赤チャンネル」が選択されます。この動作はHLSLの仕様
//2D ＠⟪_RED¦_GREEN¦_BLUE¦_ALPHA⟫
#define GATHER_TEXTURE2D_X(textureName, samplerName, coord2)                textureName.Gather(samplerName, coord2)
#define GATHER_RED_TEXTURE2D_X(textureName, samplerName, coord2)            textureName.GatherRed(samplerName, coord2)
#define GATHER_GREEN_TEXTURE2D_X(textureName, samplerName, coord2)          textureName.GatherGreen(samplerName, coord2)
#define GATHER_BLUE_TEXTURE2D_X(textureName, samplerName, coord2)           textureName.GatherBlue(samplerName, coord2)
#define GATHER_ALPHA_TEXTURE2D_X(textureName, samplerName, coord2)          textureName.GatherAlpha(samplerName, coord2)
//2D_ARRAY
#define GATHER_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)   textureName.Gather(samplerName, float3(coord2, index))
//CUBE＠❰_ARRAY❱
#define GATHER_TEXTURECUBE(textureName, samplerName, coord3)              textureName.Gather(samplerName, coord3)
#define GATHER_TEXTURECUBE_ARRAY(textureName, samplerName, coord3, index) textureName.Gather(samplerName, float4(coord3, index))
```

#### フレームバッファーフェッチ

- フェッチ宣言: `FetchInput＠❰_MS❱⟪_HALF¦_FLOAT¦_INT¦_UINT⟫(idx)` //マクロ構想
  - 普通: `FRAMEBUFFER_INPUT_X⟪_HALF¦_FLOAT¦_INT¦_UINT⟫｡｡｡｡｡｡｡(idx)`
  - MS版: `FRAMEBUFFER_INPUT｡｡⟪_HALF¦_FLOAT¦_INT¦_UINT⟫_MS｡｡｡｡(idx)`
```C
//普通
#define FRAMEBUFFER_INPUT_X_HALF(idx)      [[vk::input_attachment_index(idx)]] SubpassInput<half4> hlslcc_fbinput_##idx
#define FRAMEBUFFER_INPUT_X_FLOAT(idx)     [[vk::input_attachment_index(idx)]] SubpassInput<float4> hlslcc_fbinput_##idx
#define FRAMEBUFFER_INPUT_X_INT(idx)       [[vk::input_attachment_index(idx)]] SubpassInput<int4> hlslcc_fbinput_##idx
#define FRAMEBUFFER_INPUT_X_UINT(idx)      [[vk::input_attachment_index(idx)]] SubpassInput<uint4> hlslcc_fbinput_##idx
//MS版
#define FRAMEBUFFER_INPUT_HALF_MS(idx)     [[vk::input_attachment_index(idx)]] SubpassInputMS<half4> hlslcc_fbinput_##idx
#define FRAMEBUFFER_INPUT_FLOAT_MS(idx)    [[vk::input_attachment_index(idx)]] SubpassInputMS<float4> hlslcc_fbinput_##idx
#define FRAMEBUFFER_INPUT_INT_MS(idx)      [[vk::input_attachment_index(idx)]] SubpassInputMS<int4> hlslcc_fbinput_##idx
#define FRAMEBUFFER_INPUT_UINT_MS(idx)     [[vk::input_attachment_index(idx)]] SubpassInputMS<uint4> hlslcc_fbinput_##idx
```
- フェッチ(Load): `Fetch＠❰_MS❱(idx ＠❰,sampleIdx❱, v2fname)`
  - 普通: `LOAD_FRAMEBUFFER_X_INPUT｡｡｡｡｡｡｡(idx ｡｡｡｡｡｡｡｡｡｡｡, v2fname)`
  - MS版: `LOAD_FRAMEBUFFER｡｡_INPUT_MS｡｡｡｡(idx, sampleIdx, v2fname)`
```C
//普通                                   ↓使われてないが
#define LOAD_FRAMEBUFFER_X_INPUT(idx, v2fname) hlslcc_fbinput_##idx.SubpassLoad()
//MS版
#define LOAD_FRAMEBUFFER_INPUT_MS(idx, sampleIdx, v2fname) hlslcc_fbinput_##idx.SubpassLoad(sampleIdx)
```

## インプット

### マトリックス

- 大体、M, V, P

#### 通常のM,V,P変換行列

- `unity_Matrix＠❰Inv❱⟪V¦P¦VP⟫`は、**カメラ**または**ライト**など
- `float4x4`
  `⟪`
    `＠❰Inv❱⟪M¦V¦P¦VP⟫`
    `¦MVP`
    `¦＠❰Inv❱＠❰Trans❱MV`
    `¦Prev＠❰Inv❱M`
  `⟫`
  - 多分、`V`は`Z軸`が**反転**(*-1)している。(プラットフォーム(`UNITY_REVERSED_Z`)関係なし)

```C
//Input.hlsl/G:197, UnityInput.hlsl/G:226, SpaceTransforms.hlsl/G:13
//いろいろ変換行列があるが、C#上で、M, V, P が取れれば全て作れる。複雑そうに見えるが、単なる座標空間移動。と 転置(T)
//M, V, P-------------------↓さらにラップしてm2wとかMとかにしたい
float4x4 GetObjectToWorldMatrix()      =>  #define UNITY_MATRIX_M           ❰float4x4❱ unity_ObjectToWorld     ⇒  CBUFFER_START(UnityPerDraw)
float4x4 GetWorldToViewMatrix()        =>  #define UNITY_MATRIX_V           ❰float4x4❱ unity_MatrixV
float4x4 GetViewToHClipMatrix()        =>  #define UNITY_MATRIX_P           OptimizeProjectionMatrix(❰float4x4❱ glstate_matrix_projection)
//MV, VP, MVP
                                           #define UNITY_MATRIX_MV          mul(UNITY_MATRIX_V, UNITY_MATRIX_M) //シェーダー内でmul(..)演算
float4x4 GetWorldToHClipMatrix()       =>  #define UNITY_MATRIX_VP          ❰float4x4❱ unity_MatrixVP
                                           #define UNITY_MATRIX_MVP         mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)
//Inv [M, V, P]-------------
float4x4 GetWorldToObjectMatrix()      =>  #define UNITY_MATRIX_I_M         ❰float4x4❱ unity_WorldToObject     ⇒  CBUFFER_START(UnityPerDraw)
float4x4 GetViewToWorldMatrix()        =>  #define UNITY_MATRIX_I_V         ❰float4x4❱ unity_MatrixInvV
                                           #define UNITY_MATRIX_I_P         ❰float4x4❱ unity_MatrixInvP
//Trans MV, Inv VP
                                           #define UNITY_MATRIX_T_MV        transpose(UNITY_MATRIX_MV)
                                           #define UNITY_MATRIX_I_VP        ❰float4x4❱ unity_MatrixInvVP
//InvTrans MV
                                           #define UNITY_MATRIX_IT_MV       transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
//Prev [M, Inv M]-----------
float4x4 GetPrevObjectToWorldMatrix()  =>  #define UNITY_PREV_MATRIX_M      ❰float4x4❱ unity_MatrixPreviousM   ⇒  CBUFFER_START(UnityPerDraw)
float4x4 GetPrevWorldToObjectMatrix()  =>  #define UNITY_PREV_MATRIX_I_M    ❰float4x4❱ unity_MatrixPreviousMI  ⇒  CBUFFER_START(UnityPerDraw)
```
`OptimizeProjectionMatrix(float4x4 M)`
```C
float4x4 OptimizeProjectionMatrix(float4x4 M)
{
    // Matrix format (x = non-constant value).
    // Orthographic Perspective  Combined(OR)
    // | x 0 0 x |  | x 0 x 0 |  | x 0 x x |
    // | 0 x 0 x |  | 0 x x 0 |  | 0 x x x |
    // | x x x x |  | x x x x |  | x x x x | <- oblique projection row 斜投影列
    // | 0 0 0 1 |  | 0 0 x 0 |  | 0 0 x x |
    // Notice that some values are always 0.
    // We can avoid loading and doing math with constants.
    // いくつかの値は常に0であることに注意。
    // 定数の読み込みや計算を避けることができる。
    M._21_41 = 0;
    M._12_42 = 0;
    return M;
}
```

#### 常にカメラの変換行列

- 元のカメラの`＠❰Inv❱⟪V¦P⟫`。(例えば、**シャドウ**の描画で`unity_MatrixV`が**ライト**になっていても、`unity_WorldToCamera`は**カメラ**を表す)

```C
//UnityInput.hlsl/G:102
// >カメラの射影行列。これは現在設定されている射影行列とは異なる場合があることに注意してください。
// >例えば、シャドウをレンダリングしている間、以下の行列は依然として元のカメラの射影となります。
float4x4 unity_CameraProjection; //=>元のカメラの OptimizeProjectionMatrix(❰float4x4❱ glstate_matrix_projection) ?
float4x4 unity_CameraInvProjection; //=>元のカメラの unity_MatrixInvP
float4x4 unity_WorldToCamera; //=>元のカメラの unity_MatrixV
float4x4 unity_CameraToWorld; //=>元のカメラの unity_MatrixInvV
```

#### Temporal Anti-aliasing用Matrix

```C
//UnityInput.hlsl/G:271
// >TODO: すべてのアフィン変換行列は3x4にすること。
// >TODO: これらの変数を使用頻度の降順でソートし、よく使う変数をまとめて配置すること。
// >注意: 行列変数を直接参照せず、 UNITY_MATRIX_X マクロを使用してください。
float4x4 _PrevViewProjMatrix;         // non-jittered. Motion vectors.
float4x4 _NonJitteredViewProjMatrix;  // non-jittered.
float4x4 _ViewProjMatrix; // >TODO: URPは現在unity_MatrixVPを使用している。Input.hlslを参照
float4x4 _ViewMatrix;
float4x4 _ProjMatrix;
float4x4 _InvViewProjMatrix;
float4x4 _InvViewMatrix;
float4x4 _InvProjMatrix;

float4   _InvProjParam;
float4   _ScreenSize;       // {w, h, 1/w, 1/h}
float4   _FrustumPlanes[6]; // {(a, b, c) = N, d = -dot(N, P)} [L, R, T, B, N, F]
```

### テクスチャ

- 大体、拡散, 鏡面, 影

- テクスチャ(`＠❰unity_❱`)とサンプラー(`＠❰samplerunity_❱`)
  - GI
    - ライトマップ: `unity_Lightmap＠❰s❱`, 間接光?:`unity_Lightmap＠❰s❱Ind`
    - リアルタイムライトマップ: `unity_DynamicLightmap`, 方向性?:`unity_DynamicDirectionality`
  - リフレクションプローブ: `unity_SpecCube⟪0¦1⟫`: 今は`urp_ReflProbes_Atlas`
  - シャドウマスク: `unity_ShadowMask＠❰s❱`

```C
// Main lightmap //ライトマップ
TEXTURE2D(unity_Lightmap);
SAMPLER(samplerunity_Lightmap);
TEXTURE2D_ARRAY(unity_Lightmaps);
SAMPLER(samplerunity_Lightmaps);

// Dynamic lightmap // リアルタイムライトマップ
TEXTURE2D(unity_DynamicLightmap);
SAMPLER(samplerunity_DynamicLightmap);
// ↑TODO ENLIGHTEN: Instanced GI
// ↓Dual or directional lightmap (always used with unity_Lightmap, so can share sampler)
// ↓デュアルまたはディレクショナルライトマップ（常に unity_Lightmap で使用されるため、サンプラーを共有できます。）
TEXTURE2D(unity_LightmapInd); //間接光?
TEXTURE2D_ARRAY(unity_LightmapsInd); //間接光?
TEXTURE2D(unity_DynamicDirectionality); //方向性?
// TODO ENLIGHTEN: Instanced GI
// TEXTURE2D_ARRAY(unity_DynamicDirectionality);

// Unity specific //リフレクションプローブ //今は、GlossyEnvironmentReflection(..):Texture2D urp_ReflProbes_Atlasから環境鏡面光をサンプリング。がある
TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);
TEXTURECUBE(unity_SpecCube1);
SAMPLER(samplerunity_SpecCube1);

//シャドウマスク
TEXTURE2D(unity_ShadowMask);
SAMPLER(samplerunity_ShadowMask);
TEXTURE2D_ARRAY(unity_ShadowMasks); //Shadows.hlsl/SAMPLE_SHADOWMASK(input.staticLightmapUV)で取得される
SAMPLER(samplerunity_ShadowMasks);
```

### ライト

- 大体、TBNP, カラー, レイヤー

- **⟪メイン¦アディショナル⟫ライト**
  - ライトカラー:             `⟪_MainLight¦_AdditionalLights⟫｡Color` : >**Forward+**の時、`.a`にライトの**subtractiveモード**の使用有無を格納(0,1かな)
  - 減衰カラー:               `⟪_MainLight¦_AdditionalLights⟫｡OcclusionProbes`: ライト遮蔽時に減衰カラーを乗算する?
  - ライト位置:               `⟪_MainLight¦_AdditionalLights⟫｡Position` : ディレクショナルライトの時`.w`は`0`だった気がする
- **❰アディショナル❱ライト**: 距離と角度
  配列(`[MAX_VISIBLE_LIGHTS]`)は、**Forward+機構**から`lightIndex`で参照される。(`ClusterInit(inputData.normalizedScreenSpaceUV, inputData.positionWS, 0)`)
  - スポットライトの角度:              `_AdditionalLights｡SpotDir`
  - ライトの距離と角度の減衰パラメータ: `_AdditionalLights｡Attenuation`: float attenuation = DistanceAttenuation(..) * AngleAttenuation(..) で使用
- **レンダリングレイヤー**: `⟪_MainLight¦_AdditionalLights⟫｡LayerMasks`: (～LayerMask＠❰s❱ & unity_RenderingLayer)!= 0 で**合致したライト**をライティングする

メインライト
```C
half4 _MainLightColor; //ライトカラー
half4 _MainLightOcclusionProbes; //減衰カラー
float4 _MainLightPosition; //ライト位置
uint _MainLightLayerMask; //レンダリングレイヤー
```
アディショナルライト
```C
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA //SRVの次元:バッファーかな
    StructuredBuffer<LightData> _AdditionalLightsBuffer; //struct LightData は、 CBUFFER_START(AdditionalLights)の要素の型を全て含む型
    StructuredBuffer<int> _AdditionalLightsIndices; //USE_FORWARD_PLUSではないときのlightIndexを変換している?
#else
    CBUFFER_START(AdditionalLights)
        half4 _AdditionalLightsColor[MAX_VISIBLE_LIGHTS]; //ライトカラー
        half4 _AdditionalLightsOcclusionProbes[MAX_VISIBLE_LIGHTS]; //減衰カラー
        float4 _AdditionalLightsPosition[MAX_VISIBLE_LIGHTS]; //ライト位置
        half4 _AdditionalLightsSpotDir[MAX_VISIBLE_LIGHTS]; //スポットライトの角度
        half4 _AdditionalLightsAttenuation[MAX_VISIBLE_LIGHTS]; //ライトの距離と角度の減衰パラメータ
        float _AdditionalLightsLayerMasks[MAX_VISIBLE_LIGHTS]; //レンダリングレイヤー
    CBUFFER_END
#endif
half4 _AdditionalLightsCount; // シーン内の追加ライトの数を格納するパラメータ (多分 _AdditionalLightsCount <= MAX_VISIBLE_LIGHTS (<=:以下))
```
`struct LightData`
```C
struct LightData
{
    float4 color; //ライトカラー
    float4 occlusionProbeChannels; //減衰カラー
    float4 position; //ライト位置
    float4 spotDirection; //スポットライトの角度
    float4 attenuation; //ライトの距離と角度の減衰パラメータ
    uint layerMask; //レンダリングレイヤー
};
```

### Unity Per Draw

```C
#ifndef DOTS_INSTANCING_ON // Hybrid Rendererが存在しない場合の UnityPerDraw cbuffer

    // SRP Batcherによりブロックレイアウトが尊重されるべき
    CBUFFER_START(UnityPerDraw)
        // 空間
        float4x4 unity_ObjectToWorld; // オブジェクトからワールド座標への変換行列
        float4x4 unity_WorldToObject; // ワールド座標からオブジェクトへの変換行列
        float4 unity_LODFade; // xは[0,1]の範囲内でフェード値。yは16レベルに量子化されたx
        real4 unity_WorldTransformParams; //●// wは通常1.0、または負スケール変換の場合-1.0

        // レンダリングレイヤー
        float4 unity_RenderingLayer; // 最初のチャネル（x）のみ有効なデータを含み、floatはasuint()で再解釈して元の32ビット値を抽出する必要があります。

        // ライトインデックス // ?
        // RendererConfigurationのリクエストに応じてエンジン内部で設定されます。
        half4 unity_LightData;
        half4 unity_LightIndices[2]; // BatchRendererGroup ではサポートされていません。

        float4 unity_ProbesOcclusion;  //Shadows.hlsl/#elif !defined (LIGHTMAP_ON) #define SAMPLE_SHADOWMASK(uv) unity_ProbesOcclusion; //多分、従来のプローブ (非APV)

        // ライトマップ
        float4 unity_LightmapST;
        float4 unity_DynamicLightmapST;

        // Velocity
        float4x4 unity_MatrixPreviousM;
        float4x4 unity_MatrixPreviousMI;
        // X: 前フレーム位置を使用（現在はスキンメッシュのみ対象）
        // Y: モーションなしを強制
        // Z: Zバイアス値
        // W: カメラのみ
        float4 unity_MotionVectorsParams;

        //以下使わない==========================================================================================================

        // レンダラーバウンディングボックス // BatchRendererGroup ではサポートされていません。
        float4 unity_RendererBounds_Min;
        float4 unity_RendererBounds_Max;

        // SH（球面調和）//今はAPV使う
        real4 unity_SHAr;
        real4 unity_SHAg;
        real4 unity_SHAb;
        real4 unity_SHBr;
        real4 unity_SHBg;
        real4 unity_SHBb;
        real4 unity_SHC;

        // リフレクションプローブ //今は`urp_ReflProbes_Atlas`
        real4 unity_SpecCube0_HDR; // HDR環境マップのデコード指示
        // 以下7個 BatchRendererGroup ではサポートされていません。
        real4 unity_SpecCube1_HDR;
        float4 unity_SpecCube0_BoxMax;          // wはブレンド距離を含む
        float4 unity_SpecCube0_BoxMin;          // wは線形補間値を含む
        float4 unity_SpecCube0_ProbePosition;   // wはボックス投影の場合1に設定
        float4 unity_SpecCube1_BoxMax;          // wはブレンド距離を含む
        float4 unity_SpecCube1_BoxMin;          // wは(SpecCube0.importance - SpecCube1.importance)の符号を含む
        float4 unity_SpecCube1_ProbePosition;   // wはボックス投影の場合1に設定

        // スプライト
        float4 unity_SpriteColor;
        // X: FlipX（左右反転）
        // Y: FlipY（上下反転）
        // Z: 将来の使用のために予約済み
        // W: 将来の使用のために予約済み
        float4 unity_SpriteProps;
    CBUFFER_END

#endif // UNITY_DOTS_INSTANCING_ENABLED
```

### 雑多インプット

4oでコメント書いたから間違ってるかも知れない
```C
//uint DecodeMeshRenderingLayer(float renderingLayer)でテクスチャーに書かれた0～1のrenderingLayerをデコード(_RenderingLayerMaxInt を乗算)するために使われる
uint _RenderingLayerMaxInt; // int Get＠❰Defined❱RenderingLayerCount(): 定義されたレンダリングレイヤーの数 ?
float _RenderingLayerRcpMaxInt; // ↑の逆数

uint _EnableProbeVolumes; // SampleProbeVolumePixel(..)で使う、APV有無フラグ。(無効なら、従来のライトプローブ)

// x = Mip Bias
// y = 2.0 ^ [Mip Bias]
float2 _GlobalMipBias; // Mipmapのバイアスを指定するパラメータ (## #define定義/### テクスチャ/- サンプリング/G:140 参照)

// スクリーン座標の編集系? //以下の上２つは ScreenCoordOverride.hlsl で使われていた
float4 _ScreenCoordScaleBias; // ((positionCS.xy*0.5+0.5) * _ScreenCoordScaleBias.xy) + _ScreenCoordScaleBias.zw するもの?
float4 _ScreenSizeOverride; // 「スクリーン座標オーバーライド」がアクティブな場合に使用されるスクリーンサイズを指定するためのパラメータ
float4 _ScaledScreenParams; // スクリーンパラメータ（スケーリングされたスクリーンの解像度やアスペクト比に関する情報）//`_ScreenParams`のRTHandle版らしい?

float _AlphaToMaskAvailable; // Alpha-to-coverageモード: Pass{AlphaToMask On 『MSAAで使用することを目的』} の時、1.0になる?
  //AlphaToCoverageEnable(DirectX12メモ.md/G:421)と、SharpenAlpha(..)(Common.hlsl/G:1773)を使っている?<https://youtu.be/htzYbOZ-an0?t=321>
    //>URPではAlpha-to-Coverageをそのまま使うのではなく、輪郭のピクセルだけに限定して適用するような処理が組まれています。

half4 _AdditionalLightsCount; // シーン内の追加ライトの数を格納するパラメータ (### ライト/アディショナルライト/G:378 参照)

#define _InvCameraViewProj unity_MatrixInvVP // カメラのビュープロジェクション行列の逆行列

// スカイのCubeMap
half4 _GlossyEnvironmentCubeMap_HDR; // 光沢反射環境キューブマップのHDR情報 //DecodeHDREnvironment(,, _GlossyEnvironmentCubeMap_HDR)で使われている
TEXTURECUBE(_GlossyEnvironmentCubeMap); // 環境光の光沢を反射するためのキューブマップテクスチャ //`unity_SpecCube⟪0¦1⟫`ではなく、スカイのCubeMap
SAMPLER(sampler_GlossyEnvironmentCubeMap); // 環境光の光沢用キューブマップテクスチャをサンプリングするためのサンプラー
    // half3 CalculateIrradianceFromReflectionProbes(half3 reflectVector,..)
    // {
    //     ～～
    //     if (totalWeight < 0.99f)
    //     {
    //         half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_GlossyEnvironmentCubeMap, sampler_GlossyEnvironmentCubeMap, reflectVector, mip));

    //         irradiance += (1.0f - totalWeight) * DecodeHDREnvironment(encodedIrradiance, _GlossyEnvironmentCubeMap_HDR);
    //     }
    //     return irradiance;
    // }
half4 _GlossyEnvironmentColor; // 環境の光沢反射の色を表すパラメータ //環境鏡面マップを使わない場合に使う//GlossyEnvironmentReflection(..){return _GlossyEnvironmentColor.rgb * occlusion;}
half4 _SubtractiveShadowColor; // 影によって減算される色を表すパラメータ //defined(_MIXED_LIGHTING_SUBTRACTIVE)時に使い、多分ライトマップを修正するやつ

// xyz: 現在未使用
// w: directLightStrength
half4 _AmbientOcclusionParam; // 現在はw成分のみ使用されており、直接光の強度を表す //SSAOの強度? (GetScreenSpaceAmbientOcclusion(..)で使用)
```
これだけ使う?
```C
// タイム (t = time since current level load) values from Unity
float4 _Time; // (t/20, t, t*2, t*3)
float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)
float4 _CosTime; // cos(t/8), cos(t/4), cos(t/2), cos(t)
float4 unity_DeltaTime; // dt, 1/dt, smoothdt, 1/smoothdt
float4 _TimeParameters; // t, sin(t), cos(t)
float4 _LastTimeParameters; // t, sin(t), cos(t)

#if !defined(USING_STEREO_MATRICES)
float3 _WorldSpaceCameraPos;
#endif

// x = 1 or -1 (投影がY軸で反転(している場合は-1) (`new RT()`は`-1`、`BRTT.Cam`は`1`)
// y = near plane
// z = far plane
// w = 1/far plane
float4 _ProjectionParams;

// x = orthographic カメラの width
// y = orthographic カメラの height
// z = unused
// w = カメラがOrthoの場合は1.0、perspectiveの場合は0.0
float4 unity_OrthoParams;

// x = width
// y = height
// z = 1 + 1.0/width //1倍 + テクセル ?
// w = 1 + 1.0/height
float4 _ScreenParams; //`cmd.SetupCameraProperties(camera)`で設定される事を確認した(`camera`が描画する`rt`のサイズ) (`cmd.SetRednerTarget(..)`では設定されない)

// Zバッファーのリニアライズに使用される値 (http://www.humus.name/temp/Linearize%20depth.txt)
  //グラフィックス関連/images/_ZBufferParams_Zバッファーのリニアライズに使用される値.png を参照
// x = 1-far/near
// y = far/near
// z = x/far
// w = y/far
// または深度バッファが逆の場合 (UNITY_REVERSED_Z is 1)
// x = -1+far/near
// y = 1
// z = x/far
// w = 1/far
float4 _ZBufferParams;

// scaleBias.x = flipSign
// scaleBias.y = scale
// scaleBias.z = bias
// scaleBias.w = unused
uniform float4 _ScaleBias;
uniform float4 _ScaleBiasRt;

// { w / RTHandle.maxWidth, h / RTHandle.maxHeight } : xy = currFrame, zw = prevFrame
// V / RT (サンプリング時にUVをScaleする値) (uvは、(RT.w,RT.h)を(1.0,1.0)とする)
uniform float4 _RTHandleScale; //恐らく、`RTHandles`

//平面の方程式: Ax + By + Cz + D = 0 (float4には(A,B,C,D)が格納されている)
  //法線(N) = (A,B,C) (多分、正規化されてる), 原点から3次元平面までの距離(d) = -D
  //N * d = 平面の中心位置 (または、原点から平面が法線方向にどのくらいオフセットされているか)
  //(A,B,C,D)・(Position, 1) = 平面との距離(正は法線(N)側) (N(A,B,C)は正規化されていること) (ABCへのPositionの射影 から Dを足す(引く))
float4 unity_CameraWorldClipPlanes[6];
```
Lightingウインドウの項目?
```C
//アンビエント
real4 glstate_lightmodel_ambient;
//スカイボックスの代わりのやつ
real4 unity_AmbientSky;
real4 unity_AmbientEquator;
real4 unity_AmbientGround;
//間接スペキュラ
real4 unity_IndirectSpecColor;
//フォグ
float4 unity_FogParams;
real4  unity_FogColor;
//シャドウ
real4 unity_ShadowColor;
```

## HLSLライブラリ

- マクロや関数でmul(P, V, M, vector) とかやりたい。m =M=> w =V=> v =P=> c
  - inv(M) //逆行列
  - iV //ビュー行列の逆行列
  - tM //転置
  - t_iMiV // trans(mul(inv(M), inv(V)))
  - m2c //モデル空間からクリップ空間
  - M2c(vector) //関数版
  - lerp, clamp, map, random
  - posOS, viewDirWS

- 既存`#include`ファイルの**オーバーライド**:
  - 上位の.hlslで、`#define ～_INCLUDED`を定義して、`～_INCLUDED`を含む`#include`ファイルを`#include`すれば、`～_INCLUDED`ファイル**全体**を**オーバーライド**する
    `#ifndef ～_INCLUDED`
    `#define ～_INCLUDED` (コピガ)
    ～ (`～_INCLUDED`ファイルの全内容)
    `#endif`
    - 例:
      `#include "Custom～.hlsl"` (`.hlsl`の中で↓に含まれる`～_INCLUDED`を`#define ～_INCLUDED`で定義して**オーバーライド**)
      `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/～.hlsl"`
    - `Lit.shader`をまるまるコピーして独自の`カスタム#include`を上に噛ませる
  - 下位の.hlslで、`#if !defined(OVERRIDE_関数名) ～関数名～ #endif`とし、上位の.hlslで`#define OVERRIDE_関数名`を定義し、
    下位の.hlslを`#include`すれば、下位の関数名の処理をオーバーライドできる。(既存コードに`#if`プリプロセスを追加する必要がある)

- **シェーダー作成方法**
  - **カスタマイズ**
    - `Lit.shaderなど`～`主要Lighting.hlsl`を利用して既存のシェーダーをカスタマイズする
      - `_BaseMap`とか`_Cutoff`などは、その名前を利用する関数があるので名前を変更しない
  - **1から作る**
    - `Unity defined keywords`と`GPU Instancing`を必要に応じて設定する
    - その他、MaterialのUIやバリアントなども全て独自に作る
  - **.hlslシェーダー関数の利用**
    - `Core.hlsl`を`#include`する。(**Input系**を入れる) (`Properties`,`Material Keywords`,`Universal Pipeline keywords`,`Input系`は、`_`プレフィックスが付く)
    - `\b_[A-Z]+(?:_[A-Z0-9]+)*\b`を正規表現(と大文字と小文字を区別するもチェック)で検索して、**ShaderKeyword分岐制御系**を調べる。(`Lit.shaderなど`～`主要Lighting.hlsl`に存在)
      - (ShaderKeyword分岐制御系 <=> `Material Keywords`＆`Universal Pipeline keywords`)
  - **これだけ気をつけてコード書く**: `UNITY_REVERSED_Z`, `UNITY_UV_STARTS_AT_TOP`, `UNITY⟪_NEAR¦_RAW_FAR⟫_CLIP_VALUE`

- **#defineKeywordの種類**
  - **Unity直接#define**: `BuiltinShaderDefine(UNITY_～)`,その他(`SHADER_API_D3D11など`)
  - #pragma `Shader_feature`,`multi_compile`
    - **Renderer,Material**: **ShaderKeyword分岐制御系**(`Material Keywords`,`Universal Pipeline keywords`)
    - **シーン構成,ProjectSettings**: `Unity defined keywords`,`GPU Instancing`
  - **.hlsl内**: .hlsl内部の設定Keyword(品質など), Keywordが別のKeywordを定義

```C
//バリアントを減らしたい場合に例えば、
  //#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
  //を、
  //#pragma multi_compile_fragment _ _SHADOWS_SOFT_MEDIUM
  //にしたり、
  //#define _SHADOWS_SOFT_MEDIUM
  //にしたり、
  // //#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
  //としてコメントアウトしたり、すると効果的
//逆に
  //#pragma shader_feature_local_fragment _ALPHATEST_ON
  //を
  //#pragma multi_compile_local_fragment _ALPHATEST_ON
  //に、する手もある
// #pragma target 4.5 PROBE_VOLUMES_L1
  // と、
  // #if defined(PROBE_VOLUMES_L1) #pragma target 4.5 #endif
  // は、同じ

//目的の関数がある.hlslとそのShaderKeyword と Core.hlsl を入れて目的の関数が動くかどうか
//基本的に 関数内のShaderKeyword分岐制御系 以外は、Unity側ShaderKeyword または Core.hlslがShaderKeywordやInput を用意してくれる
// uniform(とShaderKeyword分岐制御系)には_をプレフィックスとして付け、_LowercaseThenCamelCaseで記述します。(Common.hlsl)
//多分、.shader直下に近いの.hlsl(Shadersフォルダ,Lighting.hlslなど)は ShaderKeyword分岐制御系を含む関数 が多いだけ
  //そこは、独自の.hlslにすれば、シンプルにできるかも (MaterialUIも独自にできる)
//案外、Forward+機構 や GlossyEnvironmentReflection(..) や TexEvaluateAdaptiveProbeVolume(..) は利用できるかも知れない

//❰\b_[A-Z]+(?:_[A-Z0-9]+)*\b❱を正規表現(と大文字と小文字を区別するもチェック)で検索してShaderKeyword分岐制御系を調べる //正規表現は4oで教えてもらった
  //_BaseMapとか_Cutoffなどは、その名前を利用する他のパスや便利関数があるので変えないほうがいい
  //Unityが定義してた❰Shadersフォルダ(Lighting.hlsl直前まで)❱を改造する または その部分を一から作り直してMaterialのUIやバリアントを独自に定義するか
  //Unity defined keywordsも入れる (keywordがコードに見つからなければバリアントは生成されない?) (#includeファイルにまとめる?Core.hlsl,独自マクロ定義M2wとかも)
  //Core.hlslと目的の関数がある.hlslを#includeする

//#defineKeyword の種類
  //Unity側が#defineするKeyword: SHADER_API_D3D11など
    //と、multi_compile の Unity defined keywords: LOD_FADE_CROSSFADE, multi_compile_instancingなど
  //.hlslのライブラリ内で#defineするKeyword: USE_FORWARD_PLUS, UNITY_UV_STARTS_AT_TOPなど
  //.shaderでshader_feature, multi_compileで#defineするShaderKeyword: _ALPHATEST_ONなど (Unity defined keywordsを除く)
```

- **主要HLSL** (`#include "Packages/com.unity.render-pipelines.⟪core¦universal⟫/⟪ShaderLibrary¦Shaders⟫/｢ファイル名｣.hlsl"`)
  - **Unity機構系**
    - **インスタンシング機構**
      - UnityInstancing.hlsl: インスタンシング機構
        - UnityDOTSInstancing.hlsl: DOTSインスタンシング機構(使い方参考:`LitInput.hlsl`): `UNITY_ACCESS_DOTS_INSTANCED_PROP`や`LoadDOTSInstancedData`などの関数を通して
          `ByteAddressBuffer unity_DOTSInstanceData`にアクセス(マクロがカオス！(でも頑張ればいけるかも知れないが優先度低))
      - DOTS.hlsl: `#include_with_pragmas`するやつ (`#pragma multi_compile _ DOTS_INSTANCING_ON`10行)
    - **Forward+機構**
      - Clustering.hlsl: `ClusterInit(..)`でiterを生成して`ClusterNext(..)`でindexを取得 (`LIGHT_LOOP_⟪BEGIN¦END⟫`は`RealtimeLights.hlsl`にある)
    - **LODクロスフェード機構**
      - LODCrossFade.hlsl: `LODFadeCrossFade(float4 positionCS){..unity_LODFade.x..clip(d);}`
  - **URP機構系**
    - **主要URPライティング機構**
      - Lighting.hlsl: 照明関数:`half4 UniversalFragment⟪PBR¦BlinnPhong¦BakedLit⟫(InputData, SurfaceData) => LightingPhysicallyBased(..) => DirectBRDFSpecular(..)`
        - BRDF.hlsl: 直接光:`struct BRDFData{..}, InitializeBRDFData(..), ⟪Environment¦Direct⟫BRDF＠❰Specular❱(..)`
        - GlobalIllumination.hlsl: 間接光:`GlobalIllumination(..) => GlossyEnvironmentReflection(..), SAMPLE_GI(..) => ⟪SampleProbeVolumePixel(..)¦SampleLightmap(..){Single版呼ぶ}⟫`
        - RealtimeLights.hlsl: Light取得:`struct Light{..}, Get⟪Main¦Additional⟫Light(..) => GetAdditionalPerObjectLight(..) => ⟪Distance¦Angle⟫Attenuation(..), LIGHT_LOOP_⟪BEGIN¦END⟫`
          - Shadows.hlsl: 影減衰量:`⟪Main¦Additional⟫Light＠❰Realtime❱Shadow(..), SampleShadowmap(..), MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade)`
    - **ツール**
      - SpaceTransforms.hlsl: 空間変換の関数群:`Transform～To～＠❰Dir❱(..), Get～To～Matrix(), GetOddNegativeScale(), CreateTangentToWorld(..)`
      - ShaderVariablesFunctions.hlsl: `Core.hlsl`のInput系などを使った関数群
      - EntityLighting.hlsl: ライトマップのサンプリング:`Sample⟪Single¦Directional⟫Lightmap(..), SampleProbeVolumeSH⟪9¦4⟫(..)(URP使かてないぽい。APVはSampleAPV(..)を使っている)`
      - MotionVectorsCommon.hlsl: モーションベクトル計算:`float2❰velocity❱ CalcNdcMotionVectorFromCsPositions(posCS, prevPosCS), ApplyMotionVectorZBias(positionCS)`
      - SSAO.hlsl: `half4 SSAO(Varyings input) : SV_Target`デプスバッファから、周辺のピクセルの正規化された法線を再構成することで、AO値を計算。後、`⟪Bilateral¦Gaussian¦Kawase⟫Blur`
    - **ShaderGraph**
      - ShaderGraphFunctions.hlsl(Universal,ShaderGraph): ShaderGraphのノードから情報取得すると思われる`shadergraph_LW⟪SampleScene⟪Color¦Depth¦Normals⟫¦BakedGI¦ReflectionProbe¦Fog⟫(..)`
    - **=Input,構造体系=**
      - Input.hlsl, UnityInput.hlsl, ShaderVariables.hlsl, ShaderVariablesMatrixDefsLegacyUnity.hlsl: `Core_hlslメモ.md/## インプット`参照
        - UniversalDOTSInstancing.hlsl: `#ifndef DOTS_INSTANCING_ON CBUFFER_START(UnityPerDraw)～`の部分の`DOTInstancing`による差し替え。追加された要素もある(`unity_EntityId`など)
      - SurfaceInput.hlsl: テクスチャ参照:`⟪TEXTURE2D¦SAMPLER⟫(＠❰sampler❱⟪_BaseMap＠⟪_TexelSize¦_MipInfo⟫¦_BumpMap¦_EmissionMap⟫), Sample⟪AlbedoAlpha¦Normal¦Emission⟫(..), Alpha(..)`
      - **テクスチャーサンプリング**
        - Declare⟪Normals¦Opaque¦Depth¦RenderingLayer⟫Texture.hlsl: `float3 ⟪Sample¦Load⟫Scene～(⟪float2¦uint2⟫ uv)`: `Load`と`DepthのLoadとSample`は、RTHandleのScale＆Clampをしていない
          - DynamicScalingClamping.hlsl: RTHandleによるScale＆Clampを行う:`float2 Clamp＠❰AndScale❱UV(float2 UV, float2 texelSize, float numberOfTexels, float2 scale)`
      - **型,サンプラー定義 のみ**
        - GlobalSamplers.hlsl: `SAMPLER(sampler_⟪Point¦Linear⟫⟪Clamp¦Repeat⟫)`
        - ShaderTypes.cs.hlsl: `struct LightData{position; color; attenuation; spotDirection; occlusionProbeChannels; layerMask;};`
        - SurfaceData.hlsl: `struct SurfaceData{albedo; specular; metallic; smoothness; normalTS; emission; occlusion; alpha; clearCoatMask; clearCoatSmoothness;};`
  - **ライブラリ系**(`#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"`は必要)
    - **Debug系**
      - Debug.hlsl: 色や数字ツール と ❰MipMapLv,オーバードロー❱などの可視化関数 `real3 GetIndexColor(int index), bool SampleDebugFont＠Number⟪2¦3¦All⟫Digits(int2 pixCoord, uint number)`
      - ShaderDebugPrint.hlsl: マウスの左ボタンが押されたとき、マウスカーソル下のピクセルの値をコンソールに出力。(｢tag｣: ｢value｣ のように)
        `ShaderDebugPrint＠❰Mouse＠❰Button❱Over❱(＠❰int2 pixelPos,❱ ＠❰uint tag❰ShaderDebugTag(..)❱,❱ ｢Type｣ value)`
    - **マクロ系**
      - Macros.hlsl: `～PI～, ｢TYPE｣⟪_INF¦_NAN¦_EPS¦_MAX¦_MIN⟫, TRANSFORM_TEX(tex, name), GET_TEXELSIZE_NAME(name), COMPARE_DEVICE_DEPTH_CLOSER＠❰EQUAL❱(shadowMapDepth, zDevice)`
        `TEMPLATE_⟪～⟫_｢TYPE｣(FunctionName, ⟦～┃,⟧❰ParameterN❱, FunctionBody) \, TEMPLATE_SWAP(FunctionName) \`
        `#ifdef  UNITY_NO_CUBEMAP_ARRAY⏎ SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(..)『CubeMapが使えないプラットフォーム時にTEXTURE2D_ARRAYにフォールバックするマクロ`
    - **数学,圧縮 系**
      - Common.hlsl: 便利関数群
      - Random.hlsl: **❰乱数,ハッシュ,ノイズ❱生成** (`InterleavedGradientNoise(..), ＠❰Jenkins❱Hash(..), ConstructFloat(..)『[0, 1)』, GenerateHashedRandomFloat(..), XorShift(..)`)
      - SpaceFillingCurves.hlsl: **Mortonコード化**と復号化(高速な空間探索) (`uint＠⟪2¦3⟫ ⟪Encode¦Decode⟫Morton⟪2D¦3D⟫(uint＠⟪2¦3⟫ ⟪coord¦code⟫) => uint ⟪Part¦Compact⟫1By⟪1¦2⟫(uint x)`)
      - Packing.hlsl: **パッキング**(Normal,HDR,Quaternion,Integer,Float,Color)`Pack～(..)`と`Unpack～(..)`のペアが定義されている
    - **フィルタリング系**
      - Filtering.  hlsl: **バイリニアフィルタリングの進化版**
      - Coverage.hlsl: `⟪Triangle¦Line⟫CoverageMask(..)`に⟪三角形¦線分⟫の頂点座標を渡すと、**8x8グリッド内でどのピクセルをカバーしているか**をビットマスクとして返す
    - **カラー系**
      - Color.hlsl: **色空間変換**や**色加工系**の関数群がある
      - HDROutput.hlsl: **HDRディスプレイ出力**(`カーブ`,`色空間変換`,`EOTF?`など)
      - PhysicalCamera.hlsl: **物理カメラ** (`Compute⟪EV100⟪ToExposure¦FromAvgLuminance⟫¦ISO¦LuminanceAdaptation⟫(..)`)
    - **ライティング系**
      - **照明**
        - BSDF.hlsl: **基礎ライティング関数群** (`--Fresnel term--`,`--⟪Diffuse¦Specular⟫ BRDF--`,`--Iridescence(イリデッセンス)(虹玉)--`,`--Fabric(生地)--`,`--Hair--`)
        - ImageBasedLighting.hlsl: **IBL**。**環境マップ**(球面,CubMapなど)を使って**周囲の光からサーフェスをライティング**する。(拡散反射,鏡面反射)
        - CommonLighting.hlsl: **ライティングや影など** (`--Helper--`,`--Attenuation--`,`--IES--`,`--Lighting--`)
        - AreaLighting.hlsl: **エリアライトのライティング**
        - SixWayLighting.hlsl: **テクスチャに対してライティング**する (`real3 GetSixWayDiffuseContributions(real3 rightTopBack, real3 leftBottomFront, real4 baseColor, real3 L0,..)`)
      - Refraction.hlsl: **球体と平面の屈折** (`RefractionModelResult RefractionModel⟪Sphere¦Box⟫(..)`)
      - SphericalHarmonics.hlsl: **球面調和関数(SH)のサンプリング関数**など (`float3 SampleSH9(float4 SH～[..], float3 N) => SHEvalLinear⟪＠❰L0❱L1¦L2⟫(real3 N, real4 shAr,..)`)
    - **ジオメトリ系**
      - MostRepresentativePoint.hlsl: エリアライトなどの**面積**を持つ面と**位置座標**(positionWS)との**最近傍点(MRP)**を算出する
      - SDF2D.hlsl: **2DのSDF** (`float ⟪Circle¦Ellipse¦Rectangle⟫SDF(position)`)
      - **交差**
        - GeometricTools.hlsl: レイと⟪Sphere¦Box⟫の**交差検出**やその他**汎用的な幾何学関数** (`⟪Sphere¦Box⟫RayIntersect＠❰Simple❱(..)`, その他、幾何学的関数がある)
        - VertexCubeSlicing.hlsl: **立方体と平面の交点**を計算 (Beat Saber?)(`float3 ComputeCubeSliceVertexPositionRWS(cameraViewDirection, planeDistance, vertexId)のみ`)
      - **法線**
        - NormalReconstruction.hlsl: 現在のピクセルの**近傍ピクセル**の**デプス**を取得して、**差分から法線を近似**する (`ReconstructNormalTap⟪3¦4¦5⟫『⟪3¦4¦5⟫はデプスのサンプリング数`)
        - NormalSurfaceGradient.hlsl: 法線やUV座標などから**サーフェスの勾配**を算出 (バンプマッピングなど凹凸を表現するために使用される)
      - **視差**
        - ParallaxMapping.hlsl: **視差マッピング** (`float2 ParallaxMapping(heightMap, viewDirTS,., uv)『uvの視差オフセットを計算`)
        - PerPixelDisplacement.hlsl: **視差オクルージョンマッピング（POM）**(`real2 ParallaxOcclusionMapping(..)`のみ)

- `全Core.hlsl`
  - `Common.hlsl`
    - `⟪GameCore¦XBoxOne¦ps⟪4¦5⟫/../PssL¦D3D11¦Metal¦Vulkan¦Switch¦GLCore¦GLES3¦WebGPU⟫.hlsl`
    - `Macros.hlsl`
    - `Random.hlsl`
  - `Packing.hlsl`
  - `Input.hlsl`
    - `ShaderTypes.cs.hlsl`
    - `UnityInput.hlsl`
    - `UnityInstancing.hlsl`
      - `UnityDOTSInstancing.hlsl`
    - `UniversalDOTSInstancing.hlsl`
    - `SpaceTransforms.hlsl`
  - `ShaderVariablesFunctions.hlsl`

- `主要Lighting.hlsl (Lit.shader)`
  - `BRDF.hlsl`
    - `BSDF.hlsl`
    - `CommonMaterial.hlsl`
    - `SurfaceData.hlsl`
  - `Debug/Debugging3D.hlsl`
  - `GlobalIllumination.hlsl`
    - `EntityLighting.hlsl`
    - `ImageBasedLighting.hlsl`
    - `RealtimeLights.hlsl(Light)`
    - `ProbeVolume.hlsl`
      - `SphericalHarmonics.hlsl`
  - `RealtimeLights.hlsl`
    - `AmbientOcclusion.hlsl`
    - `Input.hlsl`
    - `Shadows.hlsl`
    - `LightCookie.hlsl`
    - `Clustering.hlsl`
  - `AmbientOcclusion.hlsl`
  - `DBuffer.hlsl`

### #defineKeyword

#### Core.hlsl Keyword

- ShaderKeyword
  - `SHADER_API⟪_METAL¦_GLES3¦_MOBILE¦¦¦⟫`: グラフィックAPI
  - `UNITY_COMPILER_DXC`: コンパイラ
  - `SHADER_STAGE⟪_FRAGMENT¦_COMPUTE⟫`: シェーダーステージ
  - `UNITY_REVERSED_Z`, `UNITY_UV_STARTS_AT_TOP`, `UNITY⟪_NEAR¦_RAW_FAR⟫_CLIP_VALUE`: これだけ気をつけてコード書く
  - 効率的な描画
    - `PLATFORM_SUPPORTS_NATIVE_RENDERPASS`: Native Render Pass
    - `DOTS_INSTANCING_ON`, `UNITY_DOTS_INSTANCING_ENABLED`: Instancing
    - `USE_FORWARD_PLUS`, `_FORWARD_PLUS`: Light, ReflectionProbe
  - `PLATFORM_SUPPORTS_PRIMITIVE_ID_IN_PIXEL_SHADER`: SV_PrimitiveID ?
  - LIGHT
    - `USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA`
    - `MAX_VISIBLE_LIGHTS` ⟪16¦32¦256⟫
    - `MAX_VISIBLE_LIGHTS_UBO` 32
    - `MAX_VISIBLE_LIGHTS_SSBO` 256
  - WAVE
    - `PLATFORM_SUPPORTS_WAVE_INTRINSICS`
    - `UNITY_HW_SUPPORTS_WAVE`
  - `DEBUG_DISPLAY`
  - `BUILTIN_TARGET_API`
  - `UNITY_NO_DXT5nm`, `_NORMALMAP`
  - `UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION` ApplyPretransformRotation(pos)
  - `UNITY_PLATFORM_IOS`

#### BuiltinShaderDefine

ターゲットプラットフォームとGraphicsTierに基づいて、シェーダーをコンパイルするときにエディタによって設定される#define。

`UNITY_NO_DXT5nm`: DXT5NMをサポートしないプラットフォームでシェーダーをコンパイルする際に設定され、法線マップがRGBでエンコードされることを意味します。
`UNITY_NO_RGBM`: RGBMをサポートしないプラットフォームでシェーダーをコンパイルする際に設定され、dLDRが代わりに使用されます。
`UNITY_ENABLE_REFLECTION_BUFFERS`: 遅延シェーディングが遅延モードで反射プローブをレンダリングする際に設定され、このオプションが有効な場合、反射はピクセルごとのバッファにレンダリングされます。この方法は、ライトがピクセルごとのバッファにレンダリングされる方法に似ています。遅延シェーディングを使用している場合UNITY_ENABLE_REFLECTION_BUFFERSはデフォルトでオンですが、グラフィックス設定で「Deferred Reflections」シェーダーオプションを「No support」に設定することでオフにできます。設定がオフの場合、反射プローブはフォワードレンダリングと同様にオブジェクトごとにレンダリングされます。
`UNITY_FRAMEBUFFER_FETCH_AVAILABLE`: フレームバッファフェッチが利用可能な可能性のあるプラットフォームでシェーダーをコンパイルする際に設定されます。
`UNITY_ENABLE_NATIVE_SHADOW_LOOKUPS`: OpenGL ES 2.0で組み込みのシャドウ比較サンプラーを使用可能にします。
`UNITY_METAL_SHADOWS_USE_POINT_FILTERING`: iOS Metalでシャドウサンプラーがポイントフィルタリングを使用する場合に設定されます。
`UNITY_NO_SCREENSPACE_SHADOWS`: スクリーンスペースのカスケードシャドウマップが無効になっている場合に設定されます。
`UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS`: 半透明シャドウが有効な場合に設定されます。
`UNITY_PBS_USE_BRDF1`: Standard ShaderのBRDF1を使用する場合に設定されます。
`UNITY_PBS_USE_BRDF2`: Standard ShaderのBRDF2を使用する場合に設定されます。
`UNITY_PBS_USE_BRDF3`: Standard ShaderのBRDF3を使用する場合に設定されます。
`UNITY_SPECCUBE_BOX_PROJECTION`: リフレクションプローブのボックスプロジェクションが有効な場合に設定されます。
`UNITY_SPECCUBE_BLENDING`: リフレクションプローブのブレンディングが有効な場合に設定されます。
`UNITY_ENABLE_DETAIL_NORMALMAP`: ディテール法線マップが割り当てられている場合にサンプリングを行うために設定されます。
`SHADER_API_MOBILE`: モバイルプラットフォーム用にシェーダーをコンパイルする際に設定されます。
`SHADER_API_DESKTOP`: 「デスクトップ」プラットフォーム用にシェーダーをコンパイルする際に設定されます。
`UNITY_HARDWARE_TIER1`: GraphicsTier.Tier1用にシェーダーをコンパイルする際に設定されます。
`UNITY_HARDWARE_TIER2`: GraphicsTier.Tier2用にシェーダーをコンパイルする際に設定されます。
`UNITY_HARDWARE_TIER3`: GraphicsTier.Tier3用にシェーダーをコンパイルする際に設定されます。
`UNITY_COLORSPACE_GAMMA`: ガンマカラースペース用にシェーダーをコンパイルする際に設定されます。
`UNITY_LIGHT_PROBE_PROXY_VOLUME`: Light Probe Proxy Volume機能が現在のグラフィックスAPIでサポートされ、Graphics Tier設定で有効になっている場合に設定されます。この設定は、ビルトインレンダーパイプラインでのみ設定できます。
`UNITY_LIGHTMAP_DLDR_ENCODING`: ライトマップテクスチャがdLDRエンコーディングを使用してテクスチャ内に値を格納する際に設定されます。
`UNITY_LIGHTMAP_RGBM_ENCODING`: ライトマップテクスチャがRGBMエンコーディングを使用してテクスチャ内に値を格納する際に設定されます。
`UNITY_LIGHTMAP_FULL_HDR`: ライトマップテクスチャがエンコーディングなしで値を格納する際に設定されます。
`UNITY_VIRTUAL_TEXTURING`: このプラットフォームで仮想テクスチャリングが有効かつサポートされている場合です。
`UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION`: Vulkanのプリトランスフォームが有効で、ターゲットビルドプラットフォームでサポートされている場合にUnityが有効にします。
`UNITY_ASTC_NORMALMAP_ENCODING`: Android、iOS、またはtvOSでDXT5nm形式の法線マップが使用される場合、Unityが有効にします。
`SHADER_API_GLES30`: グラフィックスAPIがOpenGL ES 3で、最小サポートバージョンがOpenGL ES 3.0である場合に設定されます。
`UNITY_UNIFIED_SHADER_PRECISION_MODEL`: プレイヤー設定でシェーダー精度モデルを統一に設定した場合、Unityが設定します。
`UNITY_PLATFORM_SUPPORTS_WAVE_32`: ターゲットプラットフォームが32のウェーブサイズでのウェーブレベルシェーダー操作をサポートしていることが知られている場合にUnityが設定します。
`UNITY_PLATFORM_SUPPORTS_WAVE_64`: ターゲットプラットフォームが64のウェーブサイズでのウェーブレベルシェーダー操作をサポートしていることが知られている場合にUnityが設定します。
`UNITY_NEEDS_RENDERPASS_FBFETCH_FALLBACK`: フレームバッファフェッチが利用できない可能性があるため、RenderPassのフォールバックを生成する必要がある場合に設定されます。
`UNITY_PLATFORM_SUPPORTS_DEPTH_FETCH`: RenderPassがその深度アタッチメントを入力として使用できる場合に設定されます。
