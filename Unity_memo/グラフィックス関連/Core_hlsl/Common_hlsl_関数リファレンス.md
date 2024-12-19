# Common.hlsl 関数リファレンス

- NoteBookLM: 全ての関数を階層的に分類し分かりやすくまとめてMarkDown形式で書いてください。
関数は実装部以外(戻り値,関数名,引数)を書いてください。
  - 概要:
このソースコードは、Unityシェーダーでよく使われる、汎用的な数学関数、テクスチャサンプリング関数、空間変換関数、その他ユーティリティ関数の集まりです。これらの関数は、シェーダーのコードを簡潔かつ効率的に記述するために、便利な数学的な計算やテクスチャ処理をカプセル化しています。例えば、ビットフィールド操作関数では、特定のビットの抽出、挿入、設定などの処理を簡単に行うための関数を提供しています。また、テクスチャサンプリング関数では、テクスチャから特定の位置のピクセルをサンプリングする際に必要なLOD計算や、さまざまなサンプリングモードをサポートしています。空間変換関数では、世界座標、ビュー座標、スクリーン座標、クリップ座標間の変換を容易に行うための関数を提供しており、シェーダー内部での座標変換を簡略化しています。さらに、このソースコードには、高度な数学関数、デプスエンコーディング/デコーディング関数、アルファ値のシャープ化関数など、シェーダー開発で頻繁に用いられるユーティリティ関数も含まれており、シェーダープログラミングの効率性を高めるために役立ちます。
- 整数,浮動小数点数,ValMax,ValMin,ゼロ除算,Inf,NaN
- Unityが内部で使う為用でごちゃごちゃしている。ので自分で基礎から組み上げれるようにする

## 任意のクワッド内の値を取得 (ピクセルシェーダーのみ)

- `Type dd⟪x¦y⟫＠❰_fine❱()`:
  隣接ピクセルとの差分(微分)を取得する。`＠❰_fine❱`は、**各軸2ピクセル毎**に取得する <http://momose-d.cocolog-nifty.com/blog/2017/10/directx11---ddx.html>
- `float2 GetQuadOffset(int2 screenPos)`
  クワッドの偶数/奇数ピクセルを判定して-1/+1を返す。`dd⟪x¦y⟫_fine()`に**乗算**することで**原点**を左上から**中央に移動**できる
- `float＠⟪3¦4⟫ QuadRead｡＠❰Float⟪3¦4⟫❱｡Across｡⟪X¦Y¦Diagonal⟫｡｡(float＠⟪3¦4⟫ value, int2 screenPos)`:
  クワッド内の`screenPos`の位置から、**⟪X¦Y¦Diagonal⟫軸方向に隣接**する`float＠⟪3¦4⟫ value`値を取得

## 雑多 (CubeMapFaceID, LODFade, SharpenAlpha, Safe⟪Norm¦Div¦Sqrt⟫)

- キューブマップの面ID
  - `float CubeMapFaceID(float3 dir)`:  キューブマップの面IDを取得します。
- LODディザリング遷移 (LODCrossFade):
  - `void LODDitheringTransition(uint2 fadeMaskSeed, float ditherFactor)`:  LODディザリング遷移を処理します。LOD0はditherFactorを1..0、LOD1は-1..0で使用します。これはunity_LODFadeによって提供されるものです。
- SharpenAlpha
  - `float2 SharpenAlpha(float alpha, float alphaClipTreshold)`: アルファ値をシャープにします。アルファカバレッジで使用します。 `alpha` はアルファ値、 `alphaClipTreshold` はアルファクリップ閾値です。
- ベクトル正規化:
  - `float3 SafeNormalize(float3 inVec)`: ゼロベクトルを考慮したベクトル正規化を行います。
  - `half3 SafeNormalize(half3 inVec)`: ゼロベクトルを考慮したベクトル正規化を行います。
  - `bool IsNormalized(float3 inVec)`: ベクトルが正規化されているかどうかを判定します。
  - `bool IsNormalized(half3 inVec)`: ベクトルが正規化されているかどうかを判定します。
- 安全な除算:
  - `real SafeDiv(real numer, real denom)`:  (inf/inf) および (0/0) の場合に 1 を返す除算を行います。入力パラメータのいずれかが NaN の場合、結果は NaN です。
- 安全な平方根:
  - `real SafeSqrt(real x)`: 虚数を回避するための安全な平方根計算を行います。
- 三角関数:
  - `real SinFromCos(real cosX)`: 余弦値から正弦値を計算します。入力は [0, Pi] の範囲であると仮定します。
- 球面座標での内積:
  - `real SphericalDot(real cosTheta1, real phi1, real cosTheta2, real phi2)`: 球面座標系での2つのベクトル間の内積を計算します。
- フルスクリーントライアングルの座標取得:
  - `float2 GetFullScreenTriangleTexCoord(uint vertexID)`: フルスクリーントライアングルに対応するテクスチャ座標を取得します。
  - `float4 GetFullScreenTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)`: フルスクリーントライアングルに対応する頂点位置を取得します。
- クワッドの座標取得:
  - `float2 GetQuadTexCoord(uint vertexID)`: クワッドに対応するテクスチャ座標を取得します。
  - `float4 GetQuadVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)`: クワッドに対応する頂点位置を取得します。
- `TEMPLATE_1_REAL(ClampToFloat16Max, value, return min(value, HALF_MAX))`: 値を 16 ビット浮動小数点数の最大値にクランプします。
- **float2 RepeatOctahedralUV(float u, float v)**: 八面体 UV を繰り返します。 `u` と `v` は UV 座標です。
- フラグチェック:
  - `bool HasFlag(uint bitfield, uint flag)`: ビットフィールドに特定のフラグが設定されているかどうかを判定します。

## 深度エン/デ コード (Linear⟪01¦Eye⟫Depth(depth, zBufferParam))

- 線形深度への変換:
  - `float Linear01DepthFromNear(float depth, float4 zBufferParam)`: Zバッファ値を、ニアプレーンで0、ファープレーンで1となる線形深度に変換します。斜視投影視錐台は正しく処理しません。正投影では動作しません。
  - `float Linear01Depth(float depth, float4 zBufferParam)`: Zバッファ値を、カメラ位置で0、ファープレーンで1となる線形深度に変換します。正投影では動作しません。斜視投影視錐台は正しく処理しません。
  - `float LinearEyeDepth(float depth, float4 zBufferParam)`: Zバッファ値を線形深度に変換します。斜視投影視錐台は正しく処理しません。正投影では動作しません。
  - `float LinearEyeDepth(float2 positionNDC, float deviceDepth, float4 invProjParam)`: Zバッファ値を線形深度に変換します。斜視投影視錐台を正しく処理します。正投影では動作しません。
  - `float LinearEyeDepth(float3 positionWS, float4x4 viewMatrix)`: Zバッファ値を線形深度に変換します。すべてのケースで動作します。通常、これは 'positionWS' をすでに計算している場合、最も安価なバリアントです。 'positionWS' がカメラの前にあると仮定します。
- 対数深度のエンコード/デコード:
  - `float EncodeLogarithmicDepthGeneralized(float z, float4 encodingParams)`: 線形深度値を、一般化された対数深度値にエンコードします。出力は の範囲にクランプされます。
  - `float DecodeLogarithmicDepthGeneralized(float d, float4 decodingParams)`: 対数的にエンコードされた深度値をデコードします。出力は [n, f] の範囲にクランプされます。
  - `float EncodeLogarithmicDepth(float z, float4 encodingParams)`: 線形深度値を対数深度値にエンコードします。出力は の範囲にクランプされます。これは (c = 2) の場合の `EncodeLogarithmicDepthGeneralized()` の最適化バージョンです。
  - `float DecodeLogarithmicDepth(float d, float4 encodingParams)`: 対数的にエンコードされた深度値をデコードします。出力は [n, f] の範囲にクランプされます。これは (c = 2) の場合の `DecodeLogarithmicDepthGeneralized()` の最適化バージョンです。

## MipMapLv計算 (uvからMipMapLv取得, テクスチャからMipMapLv取得)

- テクスチャLODの計算:
  - `float ComputeTextureLOD(float2 uvdx, float2 uvdy, float2 scale, float bias = 0.0)`: テクスチャのLOD (Level of Detail) を計算します。
  - `float ComputeTextureLOD(float2 uv, float bias = 0.0)`: テクスチャのLOD (Level of Detail) を計算します。
  - `float ComputeTextureLOD(float2 uv, float2 texelSize, float bias = 0.0)`: テクスチャのLOD (Level of Detail) を計算します。
  - `float ComputeTextureLOD(float3 duvw_dx, float3 duvw_dy, float3 duvw_dz, float scale, float bias = 0.0)`: テクスチャのLOD (Level of Detail) を計算します。
- ミップマップ数の取得:
  - `uint GetMipCount(TEXTURE2D_PARAM(tex, smp))`: テクスチャのミップマップ数を取得します。

## 特殊な値 (NaN, Inf の判定と修正)

- 特殊な値の判定:
  - `IsNaN(float x)`: 値が NaN (Not a Number) かどうかを判定します。
  - `AnyIsNaN(float2/3/4 v)`: ベクトル内のいずれかの要素が NaN かどうかを判定します。
  - `IsInf(float x)`: 値が無限大かどうかを判定します。
  - `AnyIsInf(float2/3/4 v)`: ベクトル内のいずれかの要素が無限大かどうかを判定します。
  - `IsFinite(float x)`: 値が有限かどうかを判定します。
- 値の修正:
  - `SanitizeFinite(float x)`: 有限な値を返します。無限大または NaN の場合は 0 を返します。
  - `IsPositiveFinite(float x)`: 値が正の有限数かどうかを判定します。
  - `SanitizePositiveFinite(float x)`: 正の有限な値を返します。負、無限大、または NaN の場合は 0 を返します。

## 型毎の近似比較 (2つの浮動小数点数がほぼ等しいかどうかを判定)

- 等価比較:
  - `bool NearlyEqual(float a, float b, float epsilon)`: 2つの浮動小数点数がほぼ等しいかどうかを判定します。
  - `TEMPLATE_2_REAL(NearlyEqual_Real, a, b)`: 2つの実数がほぼ等しいかどうかを判定します。
  - `TEMPLATE_2_FLT(NearlyEqual_Float, a, b)`: 2つの浮動小数点数がほぼ等しいかどうかを判定します。
  - `TEMPLATE_2_HALF(NearlyEqual_Half, a, b)`: 2つの半精度浮動小数点数がほぼ等しいかどうかを判定します。

## ビット操作 (ビット＠❰列❱のSet/Get)

- ビットフィールドの操作:
  - `uint BitFieldExtract(uint data, uint offset, uint numBits)`: 指定されたデータ (`data`) の特定の位置 (`offset`) から、指定されたビット数 (`numBits`) 分のビットを抽出します。
  - `int BitFieldExtractSignExtend(int data, uint offset, uint numBits)`:  `BitFieldExtract` と同様にビットを抽出しますが、抽出されたビット列を符号拡張します。つまり、最上位ビットが1であれば、上位ビットを全て1で埋めます。
  - `uint BitFieldInsert(uint mask, uint src, uint dst)`: `src` のデータのうち、`mask` で指定されたビット位置にあるビットを、`dst` の対応する位置に挿入します。
- 特定ビットの操作:
  - `bool IsBitSet(uint data, uint offset)`: 指定されたデータ (`data`) の特定の位置 (`offset`) のビットが1かどうかを判定します。
  - `void SetBit(inout uint data, uint offset)`: 指定されたデータ (`data`) の特定の位置 (`offset`) のビットを1に設定します。
  - `void ClearBit(inout uint data, uint offset)`: 指定されたデータ (`data`) の特定の位置 (`offset`) のビットを0に設定します。
  - `void ToggleBit(inout uint data, uint offset)`: 指定されたデータ (`data`) の特定の位置 (`offset`) のビットを反転させます (0であれば1に、1であれば0に)。

## 数学関数1 (度<->ラジアン, Fast_Arc⟪Cos¦Sin¦Tan⟫, log_2(x), べき乗計算)

- 角度変換:
  - `real DegToRad(real deg)`: 度をラジアンに変換します。
  - `real RadToDeg(real rad)`: ラジアンを度に変換します。
- 逆三角関数:
  - `real FastACosPos(real inX)`: 高速な逆余弦関数を計算します。入力は、出力は [0, PI/2] です。
  - `real FastACos(real inX)`: 高速な逆余弦関数を計算します。入力は [-1, 1]、出力は [0, PI] です。
  - `real FastASin(real x)`: 高速な逆正弦関数を計算します。入力は [-1, 1]、出力は [-PI/2, PI/2] です。
  - `real FastATanPos(real x)`: 高速な逆正接関数を計算します。入力は [0, infinity]、出力は [0, PI/2] です。
  - `real FastATan(real x)`: 高速な逆正接関数を計算します。入力は [-infinity, infinity]、出力は [-PI/2, PI/2] です。
  - `real FastAtan2(real y, real x)`: 高速な逆正接関数を計算します。2つの引数を取り、y/xの逆正接を計算します。
- 対数関数:
  - `uint FastLog2(uint x)`: 高速な対数関数 (底は2) を計算します。
- 累乗:
  - `TEMPLATE_1_REAL(Sq, x)`: 引数を2乗します。
  - `TEMPLATE_1_INT(Sq, x)`: 引数を2乗します。
  - `bool IsPower2(uint x)`: 引数が2の累乗かどうかを判定します。
- 累乗関数:
  - `TEMPLATE_2_REAL(PositivePow, base, power)`:  底が正または0である場合の累乗関数を計算します。
  - `TEMPLATE_2_FLT(SafePositivePow, base, power)`:        底が正であることを前提とした累乗関数を計算し、0をFLT_EPSにクランプすることでNaNを回避します。
  - `TEMPLATE_2_ONLY_HALF(SafePositivePow, base, power)`:  底が正であることを前提とした累乗関数を計算し、0をHALF_EPSにクランプすることでNaNを回避します。
  - `TEMPLATE_2_FLT(SafePositivePow_float, base, power)`:  底が正であることを前提とした累乗関数を計算し、0をFLT_EPSにクランプすることでNaNを回避します。
  - `TEMPLATE_2_HALF(SafePositivePow_half, base, power)`:  底が正であることを前提とした累乗関数を計算し、0をHALF_EPSにクランプすることでNaNを回避します。

## 数学関数2 (CopySign, Remap01, Smoothstep01, Inverse(float4x4 m))

- 符号操作:
  - `float CopySign(float x, float s, bool ignoreNegZero = true)`: `x` の値を `s` の符号で置き換えます。オプションで、-0 を +0 として扱うことができます。
  - `float FastSign(float s, bool ignoreNegZero = true)`:  `s` の符号を高速に取得します。オプションで、-0 を +0 として扱うことができます。
- ベクトル操作:
  - `real3 Orthonormalize(real3 tangent, real3 normal)`: グラム・シュミットの正規直交化法を使用して接線フレームを正規直交化します。法線は正規化されており、2つのベクトルは同一線上にはないと仮定します。新しい接線を返します（法線は影響を受けません）。
- 線形補間:
  - `TEMPLATE_3_REAL(Remap01, x, rcpLength, startTimesRcpLength)`:  [start, end] を にリマップします。
  - `TEMPLATE_3_REAL(Remap10, x, rcpLength, endTimesRcpLength)`:  [start, end] を にリマップします。
  - `real2 RemapHalfTexelCoordTo01(real2 coord, real2 size)`:  [0.5 / size, 1 - 0.5 / size] を にリマップします。
  - `real2 Remap01ToHalfTexelCoord(real2 coord, real2 size)`:  を [0.5 / size, 1 - 0.5 / size] にリマップします。
- スムーズステップ関数:
  - `real Smoothstep01(real x)`: スムーズステップ関数を計算します。入力は の範囲であると仮定します。
  - `real Smootherstep01(real x)`: より滑らかなスムーズステップ関数を計算します。
  - `real Smootherstep(real a, real b, real t)`:  `a` と `b` の間のスムーズな補間を計算します。
- ベクトル演算:
  - `float3 NLerp(float3 A, float3 B, float t)`: 2つのベクトルを正規化して線形補間します。
  - `float Length2(float3 v)`: ベクトルの長さの2乗を計算します。
  - `real Pow4(real x)`: 引数を4乗します。
  - `TEMPLATE_3_FLT(RangeRemap, min, max, t)`:  `min` と `max` の間の値を にリマップします。
  - `TEMPLATE_3_FLT(RangeRemapFrom01, min, max, t)`:  の値を `min` と `max` の間の値にリマップします。
- 行列演算:
  - `float4x4 Inverse(float4x4 m)`: 4x4行列の逆行列を計算します。
- その他:
  - `float Remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)`:  `origFrom` から `origTo` の範囲を `targetFrom` から `targetTo` の範囲にリマップします。

## アルファ合成, ハイトマップ圧縮, ステンシル取得

- アルファ合成
  - `real4 CompositeOver(real4 front, real4 back)`: 前景の色とアルファ値を使用して、背景の色とアルファ値を合成します。
  - `void CompositeOver(real3 colorFront, real3 alphaFront, real3 colorBack, real3 alphaBack, out real3 color, out real3 alpha)`: 前景の色とアルファ値を使用して、背景の色とアルファ値を合成します。
- ハイトマップ エン/デ コード
  - `real4 PackHeightmap(real height)`: 高度値をテクスチャにパックします。プラットフォームによって実装が異なります。
  - `real UnpackHeightmap(real4 height)`: テクスチャから高度値をアンパックします。
- ステンシル値の取得:
  - **uint GetStencilValue(uint2 stencilBufferVal)**: ステンシルバッファ値を取得します。 `stencilBufferVal` はステンシルバッファ値です。

## 空間変換 (Clip, NDC, View, World, 地理座標変換, 各空間のPos)

- クリップ空間座標の計算:
  - `float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth)`:  NDC座標とデバイス深度値からクリップ空間座標を計算します。
  - `float4 ComputeClipSpacePosition(float3 position, float4x4 clipSpaceTransform = k_identity4x4)`: 指定された座標と変換行列からクリップ空間座標を計算します。
- NDC座標の計算:
  - `float3 ComputeNormalizedDeviceCoordinatesWithZ(float3 position, float4x4 clipSpaceTransform = k_identity4x4)`: 指定された座標と変換行列から、Z値を含むNDC座標を計算します。
  - `float2 ComputeNormalizedDeviceCoordinates(float3 position, float4x4 clipSpaceTransform = k_identity4x4)`: 指定された座標と変換行列からNDC座標を計算します。
- ビュー空間座標の計算:
  - `float3 ComputeViewSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invProjMatrix)`: NDC座標、デバイス深度値、逆投影行列からビュー空間座標を計算します。
- ワールド空間座標の計算:
  - `float3 ComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)`: NDC座標、デバイス深度値、逆ビュー投影行列からワールド空間座標を計算します。
  - `float3 ComputeWorldSpacePosition(float4 positionCS, float4x4 invViewProjMatrix)`: クリップ空間座標と逆ビュー投影行列からワールド空間座標を計算します。
- 方向と緯度経度座標の変換:
  - `float2 DirectionToLatLongCoordinate(float3 unDir)`: 方向ベクトルを緯度経度座標に変換します。
  - `float3 LatlongToDirectionCoordinate(float2 coord)`: 緯度経度座標を方向ベクトルに変換します。

### 各空間のPos❰PositionInputs❱をPosSSから取得

- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, uint2 tileCoord)`: 指定されたスクリーン空間座標、逆スクリーンサイズ、タイル座標から位置入力を取得します。明示的なタイル座標を指定することで、計算評価のためにレーン不変にすることができます。
- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize)`: 指定されたスクリーン空間座標と逆スクリーンサイズから位置入力を取得します。
- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float3 positionWS)`:  (レイトレーシングのみ) 指定されたスクリーン空間座標、逆スクリーンサイズ、ワールド空間座標から位置入力を取得します。この関数はdeviceDepthとlinearDepthを初期化しません。
- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth, float linearDepth, float3 positionWS, uint2 tileCoord)`:  (フォワードレンダリングから) 指定されたスクリーン空間座標、逆スクリーンサイズ、デバイス深度、線形深度、ワールド空間座標、タイル座標から位置入力を取得します。deviceDepthとlinearDepthはSV_Positionの.zwから直接取得されます。
- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth, float linearDepth, float3 positionWS)`:  (フォワードレンダリングから) 指定されたスクリーン空間座標、逆スクリーンサイズ、デバイス深度、線形深度、ワールド空間座標から位置入力を取得します。
- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth, float4x4 invViewProjMatrix, float4x4 viewMatrix, uint2 tileCoord)`:  (デファードレンダリングまたはコンピュートシェーダーから) 指定されたスクリーン空間座標、逆スクリーンサイズ、デバイス深度、逆ビュー投影行列、ビュー行列、タイル座標から位置入力を取得します。depthはraw深度バッファからの深度でなければなりません。これは、逆ビュー投影行列を使用して、あらゆる種類の深度を自動的に処理できるようにします。情報として。Unityでは、深度の範囲は常に0〜1ですが（OpenGLでも）、反転させることができます。
- `PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth, float4x4 invViewProjMatrix, float4x4 viewMatrix)`:  (デファードレンダリングまたはコンピュートシェーダーから) 指定されたスクリーン空間座標、逆スクリーンサイズ、デバイス深度、逆ビュー投影行列、ビュー行列から位置入力を取得します。
- `void ApplyDepthOffsetPositionInput(float3 V, float depthOffsetVS, float3 viewForwardDir, float4x4 viewProjMatrix, inout PositionInputs posInput)`:  ビュー方向 'V' はカメラを向いています。 'depthOffsetVS' は常に反対方向 (-V) に適用されます。

## 以下、要らない?============================================================

### ウェーブ操作

- `TEMPLATE_1_REAL(WaveReadLaneFirst, scalarValue)`: GPUのウェーブ操作において、最初のレーンのスカラー値を読み取ります。ウェーブとは、GPUが並列処理を行う単位です。
- `TEMPLATE_1_INT(WaveReadLaneFirst, scalarValue)`: GPUのウェーブ操作において、最初のレーンのスカラー値を読み取ります。ウェーブとは、GPUが並列処理を行う単位です。

### 算術演算

- 整数演算:
  - `TEMPLATE_2_INT(Mul24, a, b)`: 2 つの整数を乗算します。結果は24ビット精度になります。
  - `TEMPLATE_3_INT(Mad24, a, b, c)`: 2 つの整数を乗算し、その結果に3番目の整数を加算します。結果は24ビット精度になります。
- 最小・最大・平均:
  - `TEMPLATE_3_REAL(Min3, a, b, c)`: 3 つの値のうち、最小値を返します。
  - `TEMPLATE_3_INT(Min3, a, b, c)`: 3 つの値のうち、最小値を返します。
  - `TEMPLATE_3_REAL(Max3, a, b, c)`: 3 つの値のうち、最大値を返します。
  - `TEMPLATE_3_INT(Max3, a, b, c)`: 3 つの値のうち、最大値を返します。
  - `TEMPLATE_3_REAL(Avg3, a, b, c)`: 3 つの値の平均値を返します。

### テクスチャ形式サンプリング

- 1Dテクスチャ:
  - `float4 tex1D(sampler1D_f x, float v)`: 1Dテクスチャをサンプリングします。
  - `float4 tex1Dbias(sampler1D_f x, in float4 t)`: バイアス値を使用して1Dテクスチャをサンプリングします。
  - `float4 tex1Dlod(sampler1D_f x, in float4 t)`: 特定のLODレベルで1Dテクスチャをサンプリングします。
  - `float4 tex1Dgrad(sampler1D_f x, float t, float dx, float dy)`: グラデーション値を使用して1Dテクスチャをサンプリングします。
  - `float4 tex1D(sampler1D_f x, float t, float dx, float dy)`: グラデーション値を使用して1Dテクスチャをサンプリングします。
  - `float4 tex1Dproj(sampler1D_f s, in float2 t)`: 射影座標を使用して1Dテクスチャをサンプリングします。
  - `float4 tex1Dproj(sampler1D_f s, in float4 t)`: 射影座標を使用して1Dテクスチャをサンプリングします。
- 2Dテクスチャ:
  - `float4 tex2D(sampler2D_f x, float2 v)`: 2Dテクスチャをサンプリングします。
  - `float4 tex2Dbias(sampler2D_f x, in float4 t)`: バイアス値を使用して2Dテクスチャをサンプリングします。
  - `float4 tex2Dlod(sampler2D_f x, in float4 t)`: 特定のLODレベルで2Dテクスチャをサンプリングします。
  - `float4 tex2Dgrad(sampler2D_f x, float2 t, float2 dx, float2 dy)`: グラデーション値を使用して2Dテクスチャをサンプリングします。
  - `float4 tex2D(sampler2D_f x, float2 t, float2 dx, float2 dy)`: グラデーション値を使用して2Dテクスチャをサンプリングします。
  - `float4 tex2Dproj(sampler2D_f s, in float3 t)`: 射影座標を使用して2Dテクスチャをサンプリングします。
  - `float4 tex2Dproj(sampler2D_f s, in float4 t)`: 射影座標を使用して2Dテクスチャをサンプリングします。
- 3Dテクスチャ:
  - `float4 tex3D(sampler3D_f x, float3 v)`: 3Dテクスチャをサンプリングします。
  - `float4 tex3Dbias(sampler3D_f x, in float4 t)`: バイアス値を使用して3Dテクスチャをサンプリングします。
  - `float4 tex3Dlod(sampler3D_f x, in float4 t)`: 特定のLODレベルで3Dテクスチャをサンプリングします。
  - `float4 tex3Dgrad(sampler3D_f x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用して3Dテクスチャをサンプリングします。
  - `float4 tex3D(sampler3D_f x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用して3Dテクスチャをサンプリングします。
  - `float4 tex3Dproj(sampler3D_f s, in float4 t)`: 射影座標を使用して3Dテクスチャをサンプリングします。
- キューブマップ:
  - `float4 texCUBE(samplerCUBE_f x, float3 v)`: キューブマップをサンプリングします。
  - `float4 texCUBEbias(samplerCUBE_f x, in float4 t)`: バイアス値を使用してキューブマップをサンプリングします。
  - `float4 texCUBElod(samplerCUBE_f x, in float4 t)`: 特定のLODレベルでキューブマップをサンプリングします。
  - `float4 texCUBEgrad(samplerCUBE_f x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用してキューブマップをサンプリングします。
  - `float4 texCUBE(samplerCUBE_f x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用してキューブマップをサンプリングします。
  - `float4 texCUBEproj(samplerCUBE_f s, in float4 t)`: 射影座標を使用してキューブマップをサンプリングします。
- 半精度浮動小数点数テクスチャ:
  - 1Dテクスチャ:
    - `min16float4 tex1D(sampler1D_h x, float v)`: 1Dテクスチャをサンプリングします。
    - `min16float4 tex1Dbias(sampler1D_h x, in float4 t)`: バイアス値を使用して1Dテクスチャをサンプリングします。
    - `min16float4 tex1Dlod(sampler1D_h x, in float4 t)`: 特定のLODレベルで1Dテクスチャをサンプリングします。
    - `min16float4 tex1Dgrad(sampler1D_h x, float t, float dx, float dy)`: グラデーション値を使用して1Dテクスチャをサンプリングします。
    - `min16float4 tex1D(sampler1D_h x, float t, float dx, float dy)`: グラデーション値を使用して1Dテクスチャをサンプリングします。
    - `min16float4 tex1Dproj(sampler1D_h s, in float2 t)`: 射影座標を使用して1Dテクスチャをサンプリングします。
    - `min16float4 tex1Dproj(sampler1D_h s, in float4 t)`: 射影座標を使用して1Dテクスチャをサンプリングします。
  - 2Dテクスチャ:
    - `min16float4 tex2D(sampler2D_h x, float2 v)`: 2Dテクスチャをサンプリングします。
    - `min16float4 tex2Dbias(sampler2D_h x, in float4 t)`: バイアス値を使用して2Dテクスチャをサンプリングします。
    - `min16float4 tex2Dlod(sampler2D_h x, in float4 t)`: 特定のLODレベルで2Dテクスチャをサンプリングします。
    - `min16float4 tex2Dgrad(sampler2D_h x, float2 t, float2 dx, float2 dy)`: グラデーション値を使用して2Dテクスチャをサンプリングします。
    - `min16float4 tex2D(sampler2D_h x, float2 t, float2 dx, float2 dy)`: グラデーション値を使用して2Dテクスチャをサンプリングします。
    - `min16float4 tex2Dproj(sampler2D_h s, in float3 t)`: 射影座標を使用して2Dテクスチャをサンプリングします。
    - `min16float4 tex2Dproj(sampler2D_h s, in float4 t)`: 射影座標を使用して2Dテクスチャをサンプリングします。
  - 3Dテクスチャ:
    - `min16float4 tex3D(sampler3D_h x, float3 v)`: 3Dテクスチャをサンプリングします。
    - `min16float4 tex3Dbias(sampler3D_h x, in float4 t)`: バイアス値を使用して3Dテクスチャをサンプリングします。
    - `min16float4 tex3Dlod(sampler3D_h x, in float4 t)`: 特定のLODレベルで3Dテクスチャをサンプリングします。
    - `min16float4 tex3Dgrad(sampler3D_h x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用して3Dテクスチャをサンプリングします。
    - `min16float4 tex3D(sampler3D_h x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用して3Dテクスチャをサンプリングします。
    - `min16float4 tex3Dproj(sampler3D_h s, in float4 t)`: 射影座標を使用して3Dテクスチャをサンプリングします。
  - キューブマップ:
    - `min16float4 texCUBE(samplerCUBE_h x, float3 v)`: キューブマップをサンプリングします。
    - `min16float4 texCUBEbias(samplerCUBE_h x, in float4 t)`: バイアス値を使用してキューブマップをサンプリングします。
    - `min16float4 texCUBElod(samplerCUBE_h x, in float4 t)`: 特定のLODレベルでキューブマップをサンプリングします。
    - `min16float4 texCUBEgrad(samplerCUBE_h x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用してキューブマップをサンプリングします。
    - `min16float4 texCUBE(samplerCUBE_h x, float3 t, float3 dx, float3 dy)`: グラデーション値を使用してキューブマップをサンプリングします。
    - `min16float4 texCUBEproj(samplerCUBE_h s, in float4 t)`: 射影座標を使用してキューブマップをサンプリングします。

### hlslコード

```C
    #ifndef INTRINSIC_BITFIELD_EXTRACT
    // Unsigned integer bit field extraction.
    // Note that the intrinsic itself generates a vector instruction.
    // Wrap this function with WaveReadLaneFirst() to get scalar output.
    // 符号なし整数のビットフィールド抽出。
    // この関数自体がベクトル命令を生成することに注意してください。
    // スカラ出力を得るには、この関数をWaveReadLaneFirst()でラップしてください。

    // 指定されたビット位置から特定のビット数を抽出して返す（引数: data=対象データ, offset=開始位置, numBits=ビット数、戻り値: 抽出されたビット値）//●
    uint BitFieldExtract(uint data, uint offset, uint numBits)
    {
        uint mask = (1u << numBits) - 1u;
        return (data >> offset) & mask;
    }
    #endif // INTRINSIC_BITFIELD_EXTRACT

    #ifndef INTRINSIC_BITFIELD_EXTRACT_SIGN_EXTEND
    // Integer bit field extraction with sign extension.
    // Note that the intrinsic itself generates a vector instruction.
    // Wrap this function with WaveReadLaneFirst() to get scalar output.
    int BitFieldExtractSignExtend(int data, uint offset, uint numBits)
    {
        int  shifted = data >> offset;      // Sign-extending (arithmetic) shift
        int  signBit = shifted & (1u << (numBits - 1u));
        uint mask    = (1u << numBits) - 1u;

        return -signBit | (shifted & mask); // Use 2-complement for negation to replicate the sign bit
    }
    #endif // INTRINSIC_BITFIELD_EXTRACT_SIGN_EXTEND

    #ifndef INTRINSIC_BITFIELD_INSERT
    // Inserts the bits indicated by 'mask' from 'src' into 'dst'.
    uint BitFieldInsert(uint mask, uint src, uint dst)
    {
        return (src & mask) | (dst & ~mask);
    }
    #endif // INTRINSIC_BITFIELD_INSERT

    bool IsBitSet(uint data, uint offset)
    {
        return BitFieldExtract(data, offset, 1u) != 0;
    }

    void SetBit(inout uint data, uint offset)
    {
        data |= 1u << offset;
    }

    void ClearBit(inout uint data, uint offset)
    {
        data &= ~(1u << offset);
    }

    void ToggleBit(inout uint data, uint offset)
    {
        data ^= 1u << offset;
    }

    #ifndef INTRINSIC_WAVEREADFIRSTLANE
        // Warning: for correctness, the argument's value must be the same across all lanes of the wave.
        TEMPLATE_1_REAL(WaveReadLaneFirst, scalarValue, return scalarValue)
        TEMPLATE_1_INT(WaveReadLaneFirst, scalarValue, return scalarValue)
    #endif

    #ifndef INTRINSIC_MUL24
        TEMPLATE_2_INT(Mul24, a, b, return a * b)
    #endif // INTRINSIC_MUL24

    #ifndef INTRINSIC_MAD24
        TEMPLATE_3_INT(Mad24, a, b, c, return a * b + c)
    #endif // INTRINSIC_MAD24

    #ifndef INTRINSIC_MINMAX3
        TEMPLATE_3_REAL(Min3, a, b, c, return min(min(a, b), c))
        TEMPLATE_3_INT(Min3, a, b, c, return min(min(a, b), c))
        TEMPLATE_3_REAL(Max3, a, b, c, return max(max(a, b), c))
        TEMPLATE_3_INT(Max3, a, b, c, return max(max(a, b), c))
    #endif // INTRINSIC_MINMAX3

    TEMPLATE_3_REAL(Avg3, a, b, c, return (a + b + c) * 0.33333333)

    // Important! Quad functions only valid in pixel shaders!
        //偶数/奇数ピクセルを判定して-1/+1を返す
        float2 GetQuadOffset(int2 screenPos)
        {
            return float2(float(screenPos.x & 1) * 2.0 - 1.0, float(screenPos.y & 1) * 2.0 - 1.0);
        }

    #ifndef INTRINSIC_QUAD_SHUFFLE
        float QuadReadAcrossX(float value, int2 screenPos)
        {
            return value - (ddx_fine(value) * (float(screenPos.x & 1) * 2.0 - 1.0));
        }

        float QuadReadAcrossY(float value, int2 screenPos)
        {
            return value - (ddy_fine(value) * (float(screenPos.y & 1) * 2.0 - 1.0));
        }

        float QuadReadAcrossDiagonal(float value, int2 screenPos)
        {
            float2 quadDir = GetQuadOffset(screenPos);
            float dX = ddx_fine(value);
            float X = value - (dX * quadDir.x);
            return X - (ddy_fine(X) * quadDir.y);
        }
    #endif

        float3 QuadReadFloat3AcrossX(float3 val, int2 positionSS)
        {
            return float3(QuadReadAcrossX(val.x, positionSS), QuadReadAcrossX(val.y, positionSS), QuadReadAcrossX(val.z, positionSS));
        }

        float4 QuadReadFloat4AcrossX(float4 val, int2 positionSS)
        {
            return float4(QuadReadAcrossX(val.x, positionSS), QuadReadAcrossX(val.y, positionSS), QuadReadAcrossX(val.z, positionSS), QuadReadAcrossX(val.w, positionSS));
        }

        float3 QuadReadFloat3AcrossY(float3 val, int2 positionSS)
        {
            return float3(QuadReadAcrossY(val.x, positionSS), QuadReadAcrossY(val.y, positionSS), QuadReadAcrossY(val.z, positionSS));
        }

        float4 QuadReadFloat4AcrossY(float4 val, int2 positionSS)
        {
            return float4(QuadReadAcrossY(val.x, positionSS), QuadReadAcrossY(val.y, positionSS), QuadReadAcrossY(val.z, positionSS), QuadReadAcrossY(val.w, positionSS));
        }

        float3 QuadReadFloat3AcrossDiagonal(float3 val, int2 positionSS)
        {
            return float3(QuadReadAcrossDiagonal(val.x, positionSS), QuadReadAcrossDiagonal(val.y, positionSS), QuadReadAcrossDiagonal(val.z, positionSS));
        }

        float4 QuadReadFloat4AcrossDiagonal(float4 val, int2 positionSS)
        {
            return float4(QuadReadAcrossDiagonal(val.x, positionSS), QuadReadAcrossDiagonal(val.y, positionSS), QuadReadAcrossDiagonal(val.z, positionSS), QuadReadAcrossDiagonal(val.w, positionSS));
        }

    TEMPLATE_SWAP(Swap) // Define a Swap(a, b) function for all types

    #define CUBEMAPFACE_POSITIVE_X 0
    #define CUBEMAPFACE_NEGATIVE_X 1
    #define CUBEMAPFACE_POSITIVE_Y 2
    #define CUBEMAPFACE_NEGATIVE_Y 3
    #define CUBEMAPFACE_POSITIVE_Z 4
    #define CUBEMAPFACE_NEGATIVE_Z 5

    #ifndef INTRINSIC_CUBEMAP_FACE_ID
    float CubeMapFaceID(float3 dir) //●
    {
        float faceID;

        if (abs(dir.z) >= abs(dir.x) && abs(dir.z) >= abs(dir.y))
        {
            faceID = (dir.z < 0.0) ? CUBEMAPFACE_NEGATIVE_Z : CUBEMAPFACE_POSITIVE_Z;
        }
        else if (abs(dir.y) >= abs(dir.x))
        {
            faceID = (dir.y < 0.0) ? CUBEMAPFACE_NEGATIVE_Y : CUBEMAPFACE_POSITIVE_Y;
        }
        else
        {
            faceID = (dir.x < 0.0) ? CUBEMAPFACE_NEGATIVE_X : CUBEMAPFACE_POSITIVE_X;
        }

        return faceID;
    }
    #endif // INTRINSIC_CUBEMAP_FACE_ID

    // Intrinsic isnan can't be used because it require /Gic to be enabled on fxc that we can't do. So use AnyIsNan instead
    bool IsNaN(float x) //●
    {
        return (asuint(x) & 0x7FFFFFFF) > 0x7F800000;
    }

    bool AnyIsNaN(float2 v)
    {
        return (IsNaN(v.x) || IsNaN(v.y));
    }

    bool AnyIsNaN(float3 v)
    {
        return (IsNaN(v.x) || IsNaN(v.y) || IsNaN(v.z));
    }

    bool AnyIsNaN(float4 v)
    {
        return (IsNaN(v.x) || IsNaN(v.y) || IsNaN(v.z) || IsNaN(v.w));
    }

    bool IsInf(float x)
    {
        return (asuint(x) & 0x7FFFFFFF) == 0x7F800000;
    }

    bool AnyIsInf(float2 v)
    {
        return (IsInf(v.x) || IsInf(v.y));
    }

    bool AnyIsInf(float3 v)
    {
        return (IsInf(v.x) || IsInf(v.y) || IsInf(v.z));
    }

    bool AnyIsInf(float4 v)
    {
        return (IsInf(v.x) || IsInf(v.y) || IsInf(v.z) || IsInf(v.w));
    }

    bool IsFinite(float x)
    {
        return (asuint(x) & 0x7F800000) != 0x7F800000;
    }

    float SanitizeFinite(float x)
    {
        return IsFinite(x) ? x : 0;
    }

    bool IsPositiveFinite(float x)
    {
        return asuint(x) < 0x7F800000;
    }

    float SanitizePositiveFinite(float x)
    {
        return IsPositiveFinite(x) ? x : 0;
    }

    // ----------------------------------------------------------------------------
    // Common math functions
    // ----------------------------------------------------------------------------

    real DegToRad(real deg) //●
    {
        return deg * (PI / 180.0);
    }

    real RadToDeg(real rad)
    {
        return rad * (180.0 / PI);
    }

    // Square functions for cleaner code
    TEMPLATE_1_REAL(Sq, x, return (x) * (x))
    TEMPLATE_1_INT(Sq, x, return (x) * (x))

    bool IsPower2(uint x)
    {
        return (x & (x - 1)) == 0;
    }

    // Input [0, 1] and output [0, PI/2]
    // 9 VALU
    real FastACosPos(real inX)
    {
        real x = abs(inX);
        real res = (0.0468878 * x + -0.203471) * x + 1.570796; // p(x)
        res *= sqrt(1.0 - x);

        return res;
    }

    // Ref: https://seblagarde.wordpress.com/2014/12/01/inverse-trigonometric-functions-gpu-optimization-for-amd-gcn-architecture/
    // Input [-1, 1] and output [0, PI]
    // 12 VALU
    real FastACos(real inX)
    {
        real res = FastACosPos(inX);

        return (inX >= 0) ? res : PI - res; // Undo range reduction
    }

    // Same cost as Acos + 1 FR
    // Same error
    // input [-1, 1] and output [-PI/2, PI/2]
    real FastASin(real x)
    {
        return HALF_PI - FastACos(x);
    }

    // max absolute error 1.3x10^-3
    // Eberly's odd polynomial degree 5 - respect bounds
    // 4 VGPR, 14 FR (10 FR, 1 QR), 2 scalar
    // input [0, infinity] and output [0, PI/2]
    real FastATanPos(real x)
    {
        real t0 = (x < 1.0) ? x : 1.0 / x;
        real t1 = t0 * t0;
        real poly = 0.0872929;
        poly = -0.301895 + poly * t1;
        poly = 1.0 + poly * t1;
        poly = poly * t0;
        return (x < 1.0) ? poly : HALF_PI - poly;
    }

    // 4 VGPR, 16 FR (12 FR, 1 QR), 2 scalar
    // input [-infinity, infinity] and output [-PI/2, PI/2]
    real FastATan(real x)
    {
        real t0 = FastATanPos(abs(x));
        return (x < 0.0) ? -t0 : t0;
    }

    real FastAtan2(real y, real x)
    {
        return FastATan(y / x) + real(y >= 0.0 ? PI : -PI) * (x < 0.0);
    }

    #if (SHADER_TARGET >= 45)
    uint FastLog2(uint x)
    {
        return firstbithigh(x);
    }
    #endif

    // Using pow often result to a warning like this
    // "pow(f, e) will not work for negative f, use abs(f) or conditionally handle negative values if you expect them"
    // PositivePow remove this warning when you know the value is positive or 0 and avoid inf/NAN.
    // Note: https://msdn.microsoft.com/en-us/library/windows/desktop/bb509636(v=vs.85).aspx pow(0, >0) == 0
    TEMPLATE_2_REAL(PositivePow, base, power, return pow(abs(base), power))

    // SafePositivePow: Same as pow(x,y) but considers x always positive and never exactly 0 such that
    // SafePositivePow(0,y) will numerically converge to 1 as y -> 0, including SafePositivePow(0,0) returning 1.
    //
    // First, like PositivePow, SafePositivePow removes this warning for when you know the x value is positive or 0 and you know
    // you avoid a NaN:
    // ie you know that x == 0 and y > 0, such that pow(x,y) == pow(0, >0) == 0
    // SafePositivePow(0, y) will however return close to 1 as y -> 0, see below.
    //
    // Also, pow(x,y) is most probably approximated as exp2(log2(x) * y), so pow(0,0) will give exp2(-inf * 0) == exp2(NaN) == NaN.
    //
    // SafePositivePow avoids NaN in allowing SafePositivePow(x,y) where (x,y) == (0,y) for any y including 0 by clamping x to a
    // minimum of FLT_EPS. The consequences are:
    //
    // -As a replacement for pow(0,y) where y >= 1, the result of SafePositivePow(x,y) should be close enough to 0.
    // -For cases where we substitute for pow(0,y) where 0 < y < 1, SafePositivePow(x,y) will quickly reach 1 as y -> 0, while
    // normally pow(0,y) would give 0 instead of 1 for all 0 < y.
    // eg: if we #define FLT_EPS  5.960464478e-8 (for fp32),
    // SafePositivePow(0, 0.1)   = 0.1894646
    // SafePositivePow(0, 0.01)  = 0.8467453
    // SafePositivePow(0, 0.001) = 0.9835021
    //
    // Depending on the intended usage of pow(), this difference in behavior might be a moot point since:
    // 1) by leaving "y" free to get to 0, we get a NaNs
    // 2) the behavior of SafePositivePow() has more continuity when both x and y get closer together to 0, since
    // when x is assured to be positive non-zero, pow(x,x) -> 1 as x -> 0.
    //
    // TL;DR: SafePositivePow(x,y) avoids NaN and is safe for positive (x,y) including (x,y) == (0,0),
    //        but SafePositivePow(0, y) will return close to 1 as y -> 0, instead of 0, so watch out
    //        for behavior depending on pow(0, y) giving always 0, especially for 0 < y < 1.
    //
    // Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/bb509636(v=vs.85).aspx
    TEMPLATE_2_FLT(SafePositivePow, base, power, return pow(max(abs(base), float(FLT_EPS)), power))
    TEMPLATE_2_ONLY_HALF(SafePositivePow, base, power, return pow(max(abs(base), half(HALF_EPS)), power))

    // Helpers for making shadergraph functions consider precision spec through the same $precision token used for variable types
    TEMPLATE_2_FLT(SafePositivePow_float, base, power, return pow(max(abs(base), float(FLT_EPS)), power))
    TEMPLATE_2_HALF(SafePositivePow_half, base, power, return pow(max(abs(base), half(HALF_EPS)), power))

    float Eps_float() { return FLT_EPS; }
    float Min_float() { return FLT_MIN; }
    float Max_float() { return FLT_MAX; }
    half Eps_half() { return HALF_EPS; }
    half Min_half() { return HALF_MIN; }
    half Max_half() { return HALF_MAX; }

    // Compute the 'epsilon equal' relative to the scale of 'a' & 'b'.
    // Farther to 0.0f 'a' or 'b' are, larger epsilon have to be.
    bool NearlyEqual(float a, float b, float epsilon)
    {
        return abs(a - b) / (abs(a) + abs(b)) < epsilon;
    }

    TEMPLATE_2_REAL(NearlyEqual_Real, a, b, return abs(a - b) / (abs(a) + abs(b)) < real(REAL_EPS))
    TEMPLATE_2_FLT(NearlyEqual_Float, a, b, return abs(a - b) / (abs(a) + abs(b)) < real(FLT_EPS))
    TEMPLATE_2_HALF(NearlyEqual_Half, a, b, return abs(a - b) / (abs(a) + abs(b)) < real(HALF_EPS))

    // Composes a floating point value with the magnitude of 'x' and the sign of 's'.
    // See the comment about FastSign() below.
    float CopySign(float x, float s, bool ignoreNegZero = true)
    {
        if (ignoreNegZero)
        {
            return (s >= 0) ? abs(x) : -abs(x);
        }
        else
        {
            uint negZero = 0x80000000u;
            uint signBit = negZero & asuint(s);
            return asfloat(BitFieldInsert(negZero, signBit, asuint(x)));
        }
    }

    // Returns -1 for negative numbers and 1 for positive numbers.
    // 0 can be handled in 2 different ways.
    // The IEEE floating point standard defines 0 as signed: +0 and -0.
    // However, mathematics typically treats 0 as unsigned.
    // Therefore, we treat -0 as +0 by default: FastSign(+0) = FastSign(-0) = 1.
    // If (ignoreNegZero = false), FastSign(-0, false) = -1.
    // Note that the sign() function in HLSL implements signum, which returns 0 for 0.
    float FastSign(float s, bool ignoreNegZero = true)
    {
        return CopySign(1.0, s, ignoreNegZero);
    }

    // Orthonormalizes the tangent frame using the Gram-Schmidt process.
    // We assume that the normal is normalized and that the two vectors
    // aren't collinear.
    // Returns the new tangent (the normal is unaffected).
    real3 Orthonormalize(real3 tangent, real3 normal)
    {
        // TODO: use SafeNormalize()?
        return normalize(tangent - dot(tangent, normal) * normal);
    }

    // [start, end] -> [0, 1] : (x - start) / (end - start) = x * rcpLength - (start * rcpLength)
    TEMPLATE_3_REAL(Remap01, x, rcpLength, startTimesRcpLength, return saturate(x * rcpLength - startTimesRcpLength))

    // [start, end] -> [1, 0] : (end - x) / (end - start) = (end * rcpLength) - x * rcpLength
    TEMPLATE_3_REAL(Remap10, x, rcpLength, endTimesRcpLength, return saturate(endTimesRcpLength - x * rcpLength))

    // Remap: [0.5 / size, 1 - 0.5 / size] -> [0, 1]
    real2 RemapHalfTexelCoordTo01(real2 coord, real2 size)
    {
        const real2 rcpLen              = size * rcp(size - 1);
        const real2 startTimesRcpLength = 0.5 * rcp(size - 1);

        return Remap01(coord, rcpLen, startTimesRcpLength);
    }

    // Remap: [0, 1] -> [0.5 / size, 1 - 0.5 / size]
    real2 Remap01ToHalfTexelCoord(real2 coord, real2 size)
    {
        const real2 start = 0.5 * rcp(size);
        const real2 len   = 1 - rcp(size);

        return coord * len + start;
    }

    // smoothstep that assumes that 'x' lies within the [0, 1] interval.
    real Smoothstep01(real x) //●
    {
        return x * x * (3 - (2 * x));
    }

    real Smootherstep01(real x)
    {
    return x * x * x * (x * (x * 6 - 15) + 10);
    }

    real Smootherstep(real a, real b, real t)
    {
        real r = rcp(b - a);
        real x = Remap01(t, r, a * r);
        return Smootherstep01(x);
    }

    float3 NLerp(float3 A, float3 B, float t)
    {
        return normalize(lerp(A, B, t));
    }

    float Length2(float3 v)
    {
        return dot(v, v);
    }

    #ifndef BUILTIN_TARGET_API
    real Pow4(real x)
    {
        return (x * x) * (x * x);
    }
    #endif

    TEMPLATE_3_FLT(RangeRemap, min, max, t, return saturate((t - min) / (max - min)))
    TEMPLATE_3_FLT(RangeRemapFrom01, min, max, t,  return (max - min) * t + min)

    float4x4 Inverse(float4x4 m) //●
    {
        float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
        float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
        float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
        float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

        float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
        float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
        float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
        float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

        float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
        float idet = 1.0f / det;

        float4x4 ret;

        ret[0][0] = t11 * idet;
        ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
        ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
        ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

        ret[1][0] = t12 * idet;
        ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
        ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
        ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

        ret[2][0] = t13 * idet;
        ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
        ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
        ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

        ret[3][0] = t14 * idet;
        ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
        ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
        ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

        return ret;
    }

    float Remap(float origFrom, float origTo, float targetFrom, float targetTo, float value) //●
    {
        return lerp(targetFrom, targetTo, (value - origFrom) / (origTo - origFrom));
    }

    // ----------------------------------------------------------------------------
    // Texture utilities
    // ----------------------------------------------------------------------------

    float ComputeTextureLOD(float2 uvdx, float2 uvdy, float2 scale, float bias = 0.0) //●
    {
        float2 ddx_ = scale * uvdx;
        float2 ddy_ = scale * uvdy;
        float  d    = max(dot(ddx_, ddx_), dot(ddy_, ddy_));

        return max(0.5 * log2(d) - bias, 0.0);
    }

    float ComputeTextureLOD(float2 uv, float bias = 0.0)
    {
        float2 ddx_ = ddx(uv);
        float2 ddy_ = ddy(uv);

        return ComputeTextureLOD(ddx_, ddy_, 1.0, bias);
    }

    // x contains width, w contains height
    float ComputeTextureLOD(float2 uv, float2 texelSize, float bias = 0.0)
    {
        uv *= texelSize;

        return ComputeTextureLOD(uv, bias);
    }

    // LOD clamp is optional and happens outside the function.
    float ComputeTextureLOD(float3 duvw_dx, float3 duvw_dy, float3 duvw_dz, float scale, float bias = 0.0)
    {
        float d = Max3(dot(duvw_dx, duvw_dx), dot(duvw_dy, duvw_dy), dot(duvw_dz, duvw_dz));

        return max(0.5f * log2(d * (scale * scale)) - bias, 0.0);
    }

    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D12) || defined(SHADER_API_D3D11_9X) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL)
        #define MIP_COUNT_SUPPORTED 1
    #endif
        // TODO: Bug workaround, switch defines GLCORE when it shouldn't
    #if ((defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_VULKAN) || defined(SHADER_API_WEBGPU)) && !defined(SHADER_STAGE_COMPUTE)
        // OpenGL only supports textureSize for width, height, depth
        // textureQueryLevels (GL_ARB_texture_query_levels) needs OpenGL 4.3 or above and doesn't compile in compute shaders
        // tex.GetDimensions converted to textureQueryLevels
        #define MIP_COUNT_SUPPORTED 1
    #endif
        // Metal doesn't support high enough OpenGL version

    uint GetMipCount(TEXTURE2D_PARAM(tex, smp))
    {
    #if defined(MIP_COUNT_SUPPORTED)
        uint mipLevel, width, height, mipCount;
        mipLevel = width = height = mipCount = 0;
        tex.GetDimensions(mipLevel, width, height, mipCount);
        return mipCount;
    #else
        return 0;
    #endif
    }

    // ----------------------------------------------------------------------------
    // Texture format sampling
    // ----------------------------------------------------------------------------

    // DXC no longer supports DX9-style HLSL syntax for sampler2D, tex2D and the like.
    // These are emulated for backwards compatibility using our own small structs and functions which manually combine samplers and textures.
    #if defined(UNITY_COMPILER_DXC) && !defined(DXC_SAMPLER_COMPATIBILITY)
    #define DXC_SAMPLER_COMPATIBILITY 1

    // On DXC platforms which don't care about explicit sampler precison we want the emulated types to work directly e.g without needing to redefine 'sampler2D' to 'sampler2D_f'
    #if !defined(SHADER_API_GLES3) && !defined(SHADER_API_VULKAN) && !defined(SHADER_API_METAL) && !defined(SHADER_API_SWITCH) && !defined(SHADER_API_WEBGPU)
        #define sampler1D_f sampler1D
        #define sampler2D_f sampler2D
        #define sampler3D_f sampler3D
        #define samplerCUBE_f samplerCUBE
    #endif

    struct sampler1D_f      { Texture1D<float4> t; SamplerState s; };
    struct sampler2D_f      { Texture2D<float4> t; SamplerState s; };
    struct sampler3D_f      { Texture3D<float4> t; SamplerState s; };
    struct samplerCUBE_f    { TextureCube<float4> t; SamplerState s; };

    float4 tex1D(sampler1D_f x, float v)        { return x.t.Sample(x.s, v); } //●
    float4 tex2D(sampler2D_f x, float2 v)       { return x.t.Sample(x.s, v); }
    float4 tex3D(sampler3D_f x, float3 v)       { return x.t.Sample(x.s, v); }
    float4 texCUBE(samplerCUBE_f x, float3 v)   { return x.t.Sample(x.s, v); }

    float4 tex1Dbias(sampler1D_f x, in float4 t)        { return x.t.SampleBias(x.s, t.x, t.w); }
    float4 tex2Dbias(sampler2D_f x, in float4 t)        { return x.t.SampleBias(x.s, t.xy, t.w); }
    float4 tex3Dbias(sampler3D_f x, in float4 t)        { return x.t.SampleBias(x.s, t.xyz, t.w); }
    float4 texCUBEbias(samplerCUBE_f x, in float4 t)    { return x.t.SampleBias(x.s, t.xyz, t.w); }

    float4 tex1Dlod(sampler1D_f x, in float4 t)     { return x.t.SampleLevel(x.s, t.x, t.w); }
    float4 tex2Dlod(sampler2D_f x, in float4 t)     { return x.t.SampleLevel(x.s, t.xy, t.w); }
    float4 tex3Dlod(sampler3D_f x, in float4 t)     { return x.t.SampleLevel(x.s, t.xyz, t.w); }
    float4 texCUBElod(samplerCUBE_f x, in float4 t) { return x.t.SampleLevel(x.s, t.xyz, t.w); }

    float4 tex1Dgrad(sampler1D_f x, float t, float dx, float dy)        { return x.t.SampleGrad(x.s, t, dx, dy); }
    float4 tex2Dgrad(sampler2D_f x, float2 t, float2 dx, float2 dy)     { return x.t.SampleGrad(x.s, t, dx, dy); }
    float4 tex3Dgrad(sampler3D_f x, float3 t, float3 dx, float3 dy)     { return x.t.SampleGrad(x.s, t, dx, dy); }
    float4 texCUBEgrad(samplerCUBE_f x, float3 t, float3 dx, float3 dy) { return x.t.SampleGrad(x.s, t, dx, dy); }

    float4 tex1D(sampler1D_f x, float t, float dx, float dy)        { return x.t.SampleGrad(x.s, t, dx, dy); }
    float4 tex2D(sampler2D_f x, float2 t, float2 dx, float2 dy)     { return x.t.SampleGrad(x.s, t, dx, dy); }
    float4 tex3D(sampler3D_f x, float3 t, float3 dx, float3 dy)     { return x.t.SampleGrad(x.s, t, dx, dy); }
    float4 texCUBE(samplerCUBE_f x, float3 t, float3 dx, float3 dy) { return x.t.SampleGrad(x.s, t, dx, dy); }

    float4 tex1Dproj(sampler1D_f s, in float2 t)        { return tex1D(s, t.x / t.y); }
    float4 tex1Dproj(sampler1D_f s, in float4 t)        { return tex1D(s, t.x / t.w); }
    float4 tex2Dproj(sampler2D_f s, in float3 t)        { return tex2D(s, t.xy / t.z); }
    float4 tex2Dproj(sampler2D_f s, in float4 t)        { return tex2D(s, t.xy / t.w); }
    float4 tex3Dproj(sampler3D_f s, in float4 t)        { return tex3D(s, t.xyz / t.w); }
    float4 texCUBEproj(samplerCUBE_f s, in float4 t)    { return texCUBE(s, t.xyz / t.w); }

    // Half precision emulated samplers used instead the sampler.*_half unity types
    struct sampler1D_h      { Texture1D<min16float4> t; SamplerState s; };
    struct sampler2D_h      { Texture2D<min16float4> t; SamplerState s; };
    struct sampler3D_h      { Texture3D<min16float4> t; SamplerState s; };
    struct samplerCUBE_h    { TextureCube<min16float4> t; SamplerState s; };

    min16float4 tex1D(sampler1D_h x, float v)       { return x.t.Sample(x.s, v); }
    min16float4 tex2D(sampler2D_h x, float2 v)      { return x.t.Sample(x.s, v); }
    min16float4 tex3D(sampler3D_h x, float3 v)      { return x.t.Sample(x.s, v); }
    min16float4 texCUBE(samplerCUBE_h x, float3 v)  { return x.t.Sample(x.s, v); }

    min16float4 tex1Dbias(sampler1D_h x, in float4 t)       { return x.t.SampleBias(x.s, t.x, t.w); }
    min16float4 tex2Dbias(sampler2D_h x, in float4 t)       { return x.t.SampleBias(x.s, t.xy, t.w); }
    min16float4 tex3Dbias(sampler3D_h x, in float4 t)       { return x.t.SampleBias(x.s, t.xyz, t.w); }
    min16float4 texCUBEbias(samplerCUBE_h x, in float4 t)   { return x.t.SampleBias(x.s, t.xyz, t.w); }

    min16float4 tex1Dlod(sampler1D_h x, in float4 t)        { return x.t.SampleLevel(x.s, t.x, t.w); }
    min16float4 tex2Dlod(sampler2D_h x, in float4 t)        { return x.t.SampleLevel(x.s, t.xy, t.w); }
    min16float4 tex3Dlod(sampler3D_h x, in float4 t)        { return x.t.SampleLevel(x.s, t.xyz, t.w); }
    min16float4 texCUBElod(samplerCUBE_h x, in float4 t)    { return x.t.SampleLevel(x.s, t.xyz, t.w); }

    min16float4 tex1Dgrad(sampler1D_h x, float t, float dx, float dy)           { return x.t.SampleGrad(x.s, t, dx, dy); }
    min16float4 tex2Dgrad(sampler2D_h x, float2 t, float2 dx, float2 dy)        { return x.t.SampleGrad(x.s, t, dx, dy); }
    min16float4 tex3Dgrad(sampler3D_h x, float3 t, float3 dx, float3 dy)        { return x.t.SampleGrad(x.s, t, dx, dy); }
    min16float4 texCUBEgrad(samplerCUBE_h x, float3 t, float3 dx, float3 dy)    { return x.t.SampleGrad(x.s, t, dx, dy); }

    min16float4 tex1D(sampler1D_h x, float t, float dx, float dy)           { return x.t.SampleGrad(x.s, t, dx, dy); }
    min16float4 tex2D(sampler2D_h x, float2 t, float2 dx, float2 dy)        { return x.t.SampleGrad(x.s, t, dx, dy); }
    min16float4 tex3D(sampler3D_h x, float3 t, float3 dx, float3 dy)        { return x.t.SampleGrad(x.s, t, dx, dy); }
    min16float4 texCUBE(samplerCUBE_h x, float3 t, float3 dx, float3 dy)    { return x.t.SampleGrad(x.s, t, dx, dy); }

    min16float4 tex1Dproj(sampler1D_h s, in float2 t)       { return tex1D(s, t.x / t.y); }
    min16float4 tex1Dproj(sampler1D_h s, in float4 t)       { return tex1D(s, t.x / t.w); }
    min16float4 tex2Dproj(sampler2D_h s, in float3 t)       { return tex2D(s, t.xy / t.z); }
    min16float4 tex2Dproj(sampler2D_h s, in float4 t)       { return tex2D(s, t.xy / t.w); }
    min16float4 tex3Dproj(sampler3D_h s, in float4 t)       { return tex3D(s, t.xyz / t.w); }
    min16float4 texCUBEproj(samplerCUBE_h s, in float4 t)   { return texCUBE(s, t.xyz / t.w); }
    #endif

    float2 DirectionToLatLongCoordinate(float3 unDir)
    {
        float3 dir = normalize(unDir);
        // coordinate frame is (-Z, X) meaning negative Z is primary axis and X is secondary axis.
        return float2(1.0 - 0.5 * INV_PI * atan2(dir.x, -dir.z), asin(dir.y) * INV_PI + 0.5);
    }

    float3 LatlongToDirectionCoordinate(float2 coord)
    {
        float theta = coord.y * PI;
        float phi = (coord.x * 2.f * PI - PI*0.5f);

        float cosTheta = cos(theta);
        float sinTheta = sqrt(1.0 - min(1.0, cosTheta*cosTheta));
        float cosPhi = cos(phi);
        float sinPhi = sin(phi);

        float3 direction = float3(sinTheta*cosPhi, cosTheta, sinTheta*sinPhi);
        direction.xy *= -1.0;
        return direction;
    }

    // ----------------------------------------------------------------------------
    // Depth encoding/decoding
    // ----------------------------------------------------------------------------

    // Z buffer to linear 0..1 depth (0 at near plane, 1 at far plane).
    // Does NOT correctly handle oblique view frustums.
    // Does NOT work with orthographic projection.
    // zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
    float Linear01DepthFromNear(float depth, float4 zBufferParam)
    {
        return 1.0 / (zBufferParam.x + zBufferParam.y / depth);
    }

    // Z buffer to linear 0..1 depth (0 at camera position, 1 at far plane).
    // Does NOT work with orthographic projections.
    // Does NOT correctly handle oblique view frustums.
    // zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
    float Linear01Depth(float depth, float4 zBufferParam) //●
    {
        return 1.0 / (zBufferParam.x * depth + zBufferParam.y);
    }

    // Z buffer to linear depth.
    // Does NOT correctly handle oblique view frustums.
    // Does NOT work with orthographic projection.
    // zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
    float LinearEyeDepth(float depth, float4 zBufferParam)
    {
        return 1.0 / (zBufferParam.z * depth + zBufferParam.w);
    }

    // Z buffer to linear depth.
    // Correctly handles oblique view frustums.
    // Does NOT work with orthographic projection.
    // Ref: An Efficient Depth Linearization Method for Oblique View Frustums, Eq. 6.
    float LinearEyeDepth(float2 positionNDC, float deviceDepth, float4 invProjParam)
    {
        float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
        float  viewSpaceZ = rcp(dot(positionCS, invProjParam));

        // If the matrix is right-handed, we have to flip the Z axis to get a positive value.
        return abs(viewSpaceZ);
    }

    // Z buffer to linear depth.
    // Works in all cases.
    // Typically, this is the cheapest variant, provided you've already computed 'positionWS'.
    // Assumes that the 'positionWS' is in front of the camera.
    float LinearEyeDepth(float3 positionWS, float4x4 viewMatrix)
    {
        float viewSpaceZ = mul(viewMatrix, float4(positionWS, 1.0)).z;

        // If the matrix is right-handed, we have to flip the Z axis to get a positive value.
        return abs(viewSpaceZ);
    }

    // 'z' is the view space Z position (linear depth).
    // saturate(z) the output of the function to clamp them to the [0, 1] range.
    // d = log2(c * (z - n) + 1) / log2(c * (f - n) + 1)
    //   = log2(c * (z - n + 1/c)) / log2(c * (f - n) + 1)
    //   = log2(c) / log2(c * (f - n) + 1) + log2(z - (n - 1/c)) / log2(c * (f - n) + 1)
    //   = E + F * log2(z - G)
    // encodingParams = { E, F, G, 0 }
    float EncodeLogarithmicDepthGeneralized(float z, float4 encodingParams)
    {
        // Use max() to avoid NaNs.
        return encodingParams.x + encodingParams.y * log2(max(0, z - encodingParams.z));
    }

    // 'd' is the logarithmically encoded depth value.
    // saturate(d) to clamp the output of the function to the [n, f] range.
    // z = 1/c * (pow(c * (f - n) + 1, d) - 1) + n
    //   = 1/c * pow(c * (f - n) + 1, d) + n - 1/c
    //   = 1/c * exp2(d * log2(c * (f - n) + 1)) + (n - 1/c)
    //   = L * exp2(d * M) + N
    // decodingParams = { L, M, N, 0 }
    // Graph: https://www.desmos.com/calculator/qrtatrlrba
    float DecodeLogarithmicDepthGeneralized(float d, float4 decodingParams)
    {
        return decodingParams.x * exp2(d * decodingParams.y) + decodingParams.z;
    }

    // 'z' is the view-space Z position (linear depth).
    // saturate(z) the output of the function to clamp them to the [0, 1] range.
    // encodingParams = { n, log2(f/n), 1/n, 1/log2(f/n) }
    // This is an optimized version of EncodeLogarithmicDepthGeneralized() for (c = 2).
    float EncodeLogarithmicDepth(float z, float4 encodingParams)
    {
        // Use max() to avoid NaNs.
        // TODO: optimize to (log2(z) - log2(n)) / (log2(f) - log2(n)).
        return log2(max(0, z * encodingParams.z)) * encodingParams.w;
    }

    // 'd' is the logarithmically encoded depth value.
    // saturate(d) to clamp the output of the function to the [n, f] range.
    // encodingParams = { n, log2(f/n), 1/n, 1/log2(f/n) }
    // This is an optimized version of DecodeLogarithmicDepthGeneralized() for (c = 2).
    // Graph: https://www.desmos.com/calculator/qrtatrlrba
    float DecodeLogarithmicDepth(float d, float4 encodingParams)
    {
        // TODO: optimize to exp2(d * y + log2(x)).
        return encodingParams.x * exp2(d * encodingParams.y);
    }

    real4 CompositeOver(real4 front, real4 back)
    {
        return front + (1 - front.a) * back;
    }

    void CompositeOver(real3 colorFront, real3 alphaFront,
                    real3 colorBack,  real3 alphaBack,
                    out real3 color,  out real3 alpha)
    {
        color = colorFront + (1 - alphaFront) * colorBack;
        alpha = alphaFront + (1 - alphaFront) * alphaBack;
    }

    // ----------------------------------------------------------------------------
    // Space transformations
    // ----------------------------------------------------------------------------

    static const float3x3 k_identity3x3 = {1, 0, 0,
                                        0, 1, 0,
                                        0, 0, 1};

    static const float4x4 k_identity4x4 = {1, 0, 0, 0,
                                        0, 1, 0, 0,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1};

    float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth)
    {
        float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);

    #if UNITY_UV_STARTS_AT_TOP
        // Our world space, view space, screen space and NDC space are Y-up.
        // Our clip space is flipped upside-down due to poor legacy Unity design.
        // The flip is baked into the projection matrix, so we only have to flip
        // manually when going from CS to NDC and back.
        positionCS.y = -positionCS.y;
    #endif

        return positionCS;
    }

    // Use case examples:
    // (position = positionCS) => (clipSpaceTransform = use default)
    // (position = positionVS) => (clipSpaceTransform = UNITY_MATRIX_P)
    // (position = positionWS) => (clipSpaceTransform = UNITY_MATRIX_VP)
    float4 ComputeClipSpacePosition(float3 position, float4x4 clipSpaceTransform = k_identity4x4)
    {
        return mul(clipSpaceTransform, float4(position, 1.0));
    }

    // The returned Z value is the depth buffer value (and NOT linear view space Z value).
    // Use case examples:
    // (position = positionCS) => (clipSpaceTransform = use default)
    // (position = positionVS) => (clipSpaceTransform = UNITY_MATRIX_P)
    // (position = positionWS) => (clipSpaceTransform = UNITY_MATRIX_VP)
    float3 ComputeNormalizedDeviceCoordinatesWithZ(float3 position, float4x4 clipSpaceTransform = k_identity4x4)
    {
        float4 positionCS = ComputeClipSpacePosition(position, clipSpaceTransform);

    #if UNITY_UV_STARTS_AT_TOP
        // Our world space, view space, screen space and NDC space are Y-up.
        // Our clip space is flipped upside-down due to poor legacy Unity design.
        // The flip is baked into the projection matrix, so we only have to flip
        // manually when going from CS to NDC and back.
        positionCS.y = -positionCS.y;
    #endif

        positionCS *= rcp(positionCS.w);
        positionCS.xy = positionCS.xy * 0.5 + 0.5;

        return positionCS.xyz;
    }

    // Use case examples:
    // (position = positionCS) => (clipSpaceTransform = use default)
    // (position = positionVS) => (clipSpaceTransform = UNITY_MATRIX_P)
    // (position = positionWS) => (clipSpaceTransform = UNITY_MATRIX_VP)
    float2 ComputeNormalizedDeviceCoordinates(float3 position, float4x4 clipSpaceTransform = k_identity4x4)
    {
        return ComputeNormalizedDeviceCoordinatesWithZ(position, clipSpaceTransform).xy;
    }

    float3 ComputeViewSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invProjMatrix)
    {
        float4 positionCS = ComputeClipSpacePosition(positionNDC, deviceDepth);
        float4 positionVS = mul(invProjMatrix, positionCS);
        // The view space uses a right-handed coordinate system.
        positionVS.z = -positionVS.z;
        return positionVS.xyz / positionVS.w;
    }

    float3 ComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)
    {
        float4 positionCS  = ComputeClipSpacePosition(positionNDC, deviceDepth);
        float4 hpositionWS = mul(invViewProjMatrix, positionCS);
        return hpositionWS.xyz / hpositionWS.w;
    }

    float3 ComputeWorldSpacePosition(float4 positionCS, float4x4 invViewProjMatrix)
    {
        float4 hpositionWS = mul(invViewProjMatrix, positionCS);
        return hpositionWS.xyz / hpositionWS.w;
    }

    // ----------------------------------------------------------------------------
    // PositionInputs
    // ----------------------------------------------------------------------------

    // Note: if you modify this struct, be sure to update the CustomPassFullscreenShader.template
    struct PositionInputs
    {
        float3 positionWS;  // World space position (could be camera-relative)
        float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
        uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
        uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
        float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
        float  linearDepth; // View space Z coordinate                              : [Near, Far]
    };

    // This function is use to provide an easy way to sample into a screen texture, either from a pixel or a compute shaders.
    // This allow to easily share code.
    // If a compute shader call this function positionSS is an integer usually calculate like: uint2 positionSS = groupId.xy * BLOCK_SIZE + groupThreadId.xy
    // else it is current unormalized screen coordinate like return by SV_Position
    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, uint2 tileCoord)   // Specify explicit tile coordinates so that we can easily make it lane invariant for compute evaluation.
    {
        PositionInputs posInput;
        ZERO_INITIALIZE(PositionInputs, posInput);

        posInput.positionNDC = positionSS;
    #if defined(SHADER_STAGE_COMPUTE) || defined(SHADER_STAGE_RAY_TRACING)
        // In case of compute shader an extra half offset is added to the screenPos to shift the integer position to pixel center.
        posInput.positionNDC.xy += float2(0.5, 0.5);
    #endif
        posInput.positionNDC *= invScreenSize;
        posInput.positionSS = uint2(positionSS);
        posInput.tileCoord = tileCoord;

        return posInput;
    }

    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize)
    {
        return GetPositionInput(positionSS, invScreenSize, uint2(0, 0));
    }

    // For Raytracing only
    // This function does not initialize deviceDepth and linearDepth
    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float3 positionWS)
    {
        PositionInputs posInput = GetPositionInput(positionSS, invScreenSize, uint2(0, 0));
        posInput.positionWS = positionWS;

        return posInput;
    }

    // From forward
    // deviceDepth and linearDepth come directly from .zw of SV_Position
    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth, float linearDepth, float3 positionWS, uint2 tileCoord)
    {
        PositionInputs posInput = GetPositionInput(positionSS, invScreenSize, tileCoord);
        posInput.positionWS = positionWS;
        posInput.deviceDepth = deviceDepth;
        posInput.linearDepth = linearDepth;

        return posInput;
    }

    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth, float linearDepth, float3 positionWS)
    {
        return GetPositionInput(positionSS, invScreenSize, deviceDepth, linearDepth, positionWS, uint2(0, 0));
    }

    // From deferred or compute shader
    // depth must be the depth from the raw depth buffer. This allow to handle all kind of depth automatically with the inverse view projection matrix.
    // For information. In Unity Depth is always in range 0..1 (even on OpenGL) but can be reversed.
    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth,
        float4x4 invViewProjMatrix, float4x4 viewMatrix,
        uint2 tileCoord)
    {
        PositionInputs posInput = GetPositionInput(positionSS, invScreenSize, tileCoord);
        posInput.positionWS = ComputeWorldSpacePosition(posInput.positionNDC, deviceDepth, invViewProjMatrix);
        posInput.deviceDepth = deviceDepth;
        posInput.linearDepth = LinearEyeDepth(posInput.positionWS, viewMatrix);

        return posInput;
    }

    PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth,
                                    float4x4 invViewProjMatrix, float4x4 viewMatrix)
    {
        return GetPositionInput(positionSS, invScreenSize, deviceDepth, invViewProjMatrix, viewMatrix, uint2(0, 0));
    }

    // The view direction 'V' points towards the camera.
    // 'depthOffsetVS' is always applied in the opposite direction (-V).
    void ApplyDepthOffsetPositionInput(float3 V, float depthOffsetVS, float3 viewForwardDir, float4x4 viewProjMatrix, inout PositionInputs posInput)
    {
        posInput.positionWS += depthOffsetVS * (-V);
        posInput.deviceDepth = ComputeNormalizedDeviceCoordinatesWithZ(posInput.positionWS, viewProjMatrix).z;

        // Transform the displacement along the view vector to the displacement along the forward vector.
        // Use abs() to make sure we get the sign right.
        // 'depthOffsetVS' applies in the direction away from the camera.
        posInput.linearDepth += depthOffsetVS * abs(dot(V, viewForwardDir));
    }

    // ----------------------------------------------------------------------------
    // Terrain/Brush heightmap encoding/decoding
    // ----------------------------------------------------------------------------

    #if defined(SHADER_API_VULKAN) || defined(SHADER_API_GLES3) || defined(SHADER_API_WEBGPU)

    // For the built-in target this is already a defined symbol
    #ifndef BUILTIN_TARGET_API
    real4 PackHeightmap(real height)
    {
        uint a = (uint)(65535.0 * height);
        return real4((a >> 0) & 0xFF, (a >> 8) & 0xFF, 0, 0) / 255.0;
    }

    real UnpackHeightmap(real4 height)
    {
        return (height.r + height.g * 256.0) / 257.0; // (255.0 * height.r + 255.0 * 256.0 * height.g) / 65535.0
    }
    #endif

    #else

    // For the built-in target this is already a defined symbol
    #ifndef BUILTIN_TARGET_API
    real4 PackHeightmap(real height)
    {
        return real4(height, 0, 0, 0);
    }

    real UnpackHeightmap(real4 height)
    {
        return height.r;
    }
    #endif

    #endif

    // ----------------------------------------------------------------------------
    // Misc utilities
    // ----------------------------------------------------------------------------

    // Simple function to test a bitfield
    bool HasFlag(uint bitfield, uint flag)
    {
        return (bitfield & flag) != 0;
    }

    // Normalize that account for vectors with zero length
    float3 SafeNormalize(float3 inVec) //●
    {
        float dp3 = max(FLT_MIN, dot(inVec, inVec));
        return inVec * rsqrt(dp3);
    }

    half3 SafeNormalize(half3 inVec)
    {
        half dp3 = max(HALF_MIN, dot(inVec, inVec));
        return inVec * rsqrt(dp3);
    }

    bool IsNormalized(float3 inVec)
    {
        float squaredLength = dot(inVec, inVec);
        return 0.9998 < squaredLength && squaredLength < 1.0002001;
    }

    bool IsNormalized(half3 inVec)
    {
        half squaredLength = dot(inVec, inVec);
        return 0.998 < squaredLength && squaredLength < 1.002;
    }

    // Division which returns 1 for (inf/inf) and (0/0).
    // If any of the input parameters are NaNs, the result is a NaN.
    real SafeDiv(real numer, real denom)
    {
        return (numer != denom) ? numer / denom : 1;
    }

    // Perform a square root safe of imaginary number.
    real SafeSqrt(real x)
    {
        return sqrt(max(0, x));
    }

    // Assumes that (0 <= x <= Pi).
    real SinFromCos(real cosX)
    {
        return sqrt(saturate(1 - cosX * cosX));
    }

    // Dot product in spherical coordinates.
    real SphericalDot(real cosTheta1, real phi1, real cosTheta2, real phi2)
    {
        return SinFromCos(cosTheta1) * SinFromCos(cosTheta2) * cos(phi1 - phi2) + cosTheta1 * cosTheta2;
    }

    // Generates a triangle in homogeneous clip space, s.t.
    // v0 = (-1, -1, 1), v1 = (3, -1, 1), v2 = (-1, 3, 1).
    float2 GetFullScreenTriangleTexCoord(uint vertexID)
    {
    #if UNITY_UV_STARTS_AT_TOP
        return float2((vertexID << 1) & 2, 1.0 - (vertexID & 2));
    #else
        return float2((vertexID << 1) & 2, vertexID & 2);
    #endif
    }

    float4 GetFullScreenTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
    {
        // note: the triangle vertex position coordinates are x2 so the returned UV coordinates are in range -1, 1 on the screen.
        float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
        float4 pos = float4(uv * 2.0 - 1.0, z, 1.0);
    #ifdef UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION
        pos = ApplyPretransformRotation(pos);
    #endif
        return pos;
    }


    // draw procedural with 2 triangles has index order (0,1,2)  (0,2,3)

    // 0 - 0,0
    // 1 - 0,1
    // 2 - 1,1
    // 3 - 1,0

    float2 GetQuadTexCoord(uint vertexID)
    {
        uint topBit = vertexID >> 1;
        uint botBit = (vertexID & 1);
        float u = topBit;
        float v = (topBit + botBit) & 1; // produces 0 for indices 0,3 and 1 for 1,2
    #if UNITY_UV_STARTS_AT_TOP
        v = 1.0 - v;
    #endif
        return float2(u, v);
    }

    // 0 - 0,1
    // 1 - 0,0
    // 2 - 1,0
    // 3 - 1,1
    float4 GetQuadVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
    {
        uint topBit = vertexID >> 1;
        uint botBit = (vertexID & 1);
        float x = topBit;
        float y = 1 - (topBit + botBit) & 1; // produces 1 for indices 0,3 and 0 for 1,2
        float4 pos = float4(x, y, z, 1.0);
    #ifdef UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION
        pos = ApplyPretransformRotation(pos);
    #endif
        return pos;
    }

    #if !defined(SHADER_STAGE_RAY_TRACING)

    // LOD dithering transition helper
    // LOD0 must use this function with ditherFactor 1..0
    // LOD1 must use this function with ditherFactor -1..0
    // This is what is provided by unity_LODFade
    void LODDitheringTransition(uint2 fadeMaskSeed, float ditherFactor)
    {
        // Generate a spatially varying pattern.
        // Unfortunately, varying the pattern with time confuses the TAA, increasing the amount of noise.
        float p = GenerateHashedRandomFloat(fadeMaskSeed);

        // This preserves the symmetry s.t. if LOD 0 has f = x, LOD 1 has f = -x.
        float f = ditherFactor - CopySign(p, ditherFactor);
        clip(f);
    }

    #endif

    // The resource that is bound when binding a stencil buffer from the depth buffer is two channel. On D3D11 the stencil value is in the green channel,
    // while on other APIs is in the red channel. Note that on some platform, always using the green channel might work, but is not guaranteed.
    // デプスバッファからステンシルバッファをバインドする際にバインドされるリソースは2チャンネルです。
    // D3D11ではステンシル値がグリーンチャンネルに格納されますが、他のAPIではレッドチャンネルに格納されます。
    // 一部のプラットフォームでは常にグリーンチャンネルを使用しても動作する場合がありますが、それは保証されていません。
    uint GetStencilValue(uint2 stencilBufferVal)
    {
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GAMECORE)
        return stencilBufferVal.y;
    #else
        return stencilBufferVal.x;
    #endif
    }

    // Sharpens the alpha of a texture to the width of a single pixel
    // テクスチャのアルファ値を1ピクセルの幅にシャープにします。
    // Used for alpha to coverage
    // source: https://medium.com/@bgolus/anti-aliased-alpha-test-the-esoteric-alpha-to-coverage-8b177335ae4f
    // GPT-4o>SharpenAlpha 関数の目的は、アルファテストを行う際にエイリアシングを軽減するためにアルファ値を調整すること
      // アルファ値が0.5の場合、ピクセル内のサブピクセルの半分だけが描画されるようにするなど、アルファ値を「部分的なカバレッジ」に変換します。
      // これがアルファテストらしい→if (color.a < alphaThreshold){discard;} だとしたら、discardによる描画の有無でジャギーが発生するが? (MSAAカバレッジはdiscard要らない?)
    //float _AlphaToMaskAvailable; // Alpha-to-coverageモード: Pass{AlphaToMask On 『MSAAで使用することを目的』} の時、1.0になる?
      //AlphaToCoverageEnable(DirectX12メモ.md/G:421)と、SharpenAlpha(..)(Common.hlsl/G:1773)を使っている?<https://youtu.be/htzYbOZ-an0?t=321>
        //>URPではAlpha-to-Coverageをそのまま使うのではなく、輪郭のピクセルだけに限定して適用するような処理が組まれています。
    //つまり、MSAA＆Pass{AlphaToMask On}にしてSharpenAlpha(..)でalpha値を調整してSV_Targetに出力しているだけってことかな?
    float SharpenAlpha(float alpha, float alphaClipTreshold)
    {
        //alpha値に急激な変化がある場合、しきい値との差が小さくなるようスケールする。その結果、値が0.5付近になる。(サブピクセルの半分だけが描画)
        return saturate((alpha - alphaClipTreshold) / max(fwidth(alpha), 0.0001) + 0.5); //fwidth(x) == abs(ddx(x)) + abs(ddy(x)) //マンハッタン距離
    }

    // These clamping function to max of floating point 16 bit are use to prevent INF in code in case of extreme value
    // 浮動小数点16ビットの最大値に対するこれらのクランプ関数は、極端な値の場合にコードのINFを防止するために使用されます。
    TEMPLATE_1_REAL(ClampToFloat16Max, value, return min(value, HALF_MAX))

    #if SHADER_API_MOBILE || SHADER_API_GLES3
    #pragma warning (enable : 3205) // conversion of larger type to smaller
    #endif

    float2 RepeatOctahedralUV(float u, float v)
    {
        float2 uv;

        if (u < 0.0f)
        {
            if (v < 0.0f)
                uv = float2(1.0f + u, 1.0f + v);
            else if (v < 1.0f)
                uv = float2(-u, 1.0f - v);
            else
                uv = float2(1.0f + u, v - 1.0f);
        }
        else if (u < 1.0f)
        {
            if (v < 0.0f)
                uv = float2(1.0f - u, -v);
            else if (v < 1.0f)
                uv = float2(u, v);
            else
                uv = float2(1.0f - u, 2.0f - v);
        }
        else
        {
            if (v < 0.0f)
                uv = float2(u - 1.0f, 1.0f + v);
            else if (v < 1.0f)
                uv = float2(2.0f - u, 1.0f - v);
            else
                uv = float2(u - 1.0f, v - 1.0f);
        }

        return uv;
    }

    #endif // UNITY_COMMON_INCLUDED

```
