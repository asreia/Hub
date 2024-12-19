# Unity URP ShaderVariablesFunctions.hlsl 関数一覧

- 4o版も
- 全ての関数を階層的に分類し分かりやすくまとめてMarkDown形式で書いてください。
そして、フォーマットは以下の形式で書いてください
``` Markdown
# 'Title'

## 'Section'

- 'Sub-Section':
  - '`Type Function(arg,..)`': '関数の説明(特殊な関数の場合はより分かりやすく)'
```

- `Core.hlsl`(`ShaderVariablesFunctions.hlsl`のみ?)でも関数を使うのに要求される`ShaderKeyword`がある(`FOG_EXP`, `_ALPHATEST_ON` など) (`変数`は無い?)

## 座標変換

- **ワールド空間**:
  - Position
    - `float3 GetCameraPositionWS() => _WorldSpaceCameraPos`: **カメラ**の**ワールド空間座標**を返す。
    - `float3 GetCurrentViewPosition() => GetCameraPositionWS()`: 現在の**ビュー**の**ワールド空間座標**を返す。これは、メインカメラやシャドウキャストライトの位置などである可能性がある。
  - ビュー
    - `float3 GetViewForwardDir()`: **ワールド空間**における現在の**ビューの前方**（中心）方向を返す。
    - `float3 GetWorldSpaceViewDir(float3 positionWS)`: 指定された**ワールド空間座標**における、視点に向かう**ワールド空間ビュー方向**を計算する。
    - `half3 GetWorldSpaceNormalizeViewDir(float3 positionWS)`: 指定された**ワールド空間座標**における、視点に向かう**正規化**された**ワールド空間ビュー方向**を計算する。
- **オブジェクト空間**:
  - ビュー
    - `half3 GetObjectSpaceNormalizeViewDir(float3 positionOS)`: 指定された**オブジェクト空間座標**における、視点に向かう**正規化**された**オブジェクト空間ビュー方向**を計算する。
- **その他**:
  - `void GetLeftHandedViewSpaceMatrices(out float4x4 viewMatrix, out float4x4 projMatrix)`: **左手座標系**の**ビュー行列と射影行列**を取得する。

## 法線 と VertexPositionInputs

- VertexPositionInputs
  - `VertexPositionInputs GetVertexPositionInputs(float3 positionOS)`: **オブジェクト空間座標**を入力として、**ワールド空間、ビュー空間、クリップ空間、NDC空間**の座標を含む`VertexPositionInputs`構造体を返す。
- VertexNormalInputs
  - `VertexNormalInputs GetVertexNormalInputs(float3 normalOS)`: **オブジェクト空間法線**を入力として、ワールド空間の接線、従法線、法線を含む`VertexNormalInputs`構造体を返す。
  - `VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS)`: **オブジェクト空間法線**と**接線**を入力として、ワールド空間の接線、従法線、法線を含む`VertexNormalInputs`構造体を返す。
- 法線を正規化
  - `⟪half3¦float3⟫ NormalizeNormalPerVertex(⟪half3¦float3⟫ normalWS)`: **頂点**シェーダーで**法線を正規化**する。
  - `⟪half3¦float3⟫ NormalizeNormalPerPixel(⟪half3¦float3⟫ normalWS)`: **ピクセル**シェーダーで**法線を正規化**する。

## スクリーンパラメータ

- `float4 GetScaledScreenParams() => _ScaledScreenParams`: **スケール**された**スクリーンパラメータ**を返す。
- `float2 GetNormalizedScreenSpaceUV(float⟪2¦4⟫ positionCS)`: **クリップ空間座標**から**正規化**された**スクリーン空間UV座標**を取得する。
- Transform＠❰Normalized❱ScreenUV
  - `void TransformScreenUV(inout float2 uv, float screenHeight)`:      **スクリーンUV座標**を**変換**する。
  - `void TransformScreenUV(inout float2 uv)`:                          **スクリーンUV座標**を**変換**する。
  - `void TransformNormalizedScreenUV(inout float2 uv)`: **正規化**された**スクリーンUV座標**を**変換**する。
- `bool IsPerspectiveProjection() => unity_OrthoParams.w == 0`: 現在のビューが**透視投影を実行するか**どうかを返す。

## フォグ

- **フォグの計算**:
  - `real ComputeFogFactorZ0ToFar(float z)`:               **Z値**から**フォグ係数**を計算する。
  - `real ComputeFogFactor(float zPositionCS)`: **クリップ空間Z値**から**フォグ係数**を計算する。
  - `⟪half¦float⟫ ComputeFogIntensity(⟪half¦float⟫ fogFactor)`: **フォグ係数**から**フォグの強度**を計算する。
- **フォグの適用**:
  - `real InitializeInputDataFog(float4 positionWS, real vertFogFactor)`: フォグの計算に**必要なデータを初期化**する。
  - `⟪half3¦float3⟫ MixFogColor(⟪half3¦float3⟫ fragColor, ⟪half3¦float3⟫ fogColor, ⟪half¦float⟫ fogFactor)`:          **フォグの色**を適用する。
  - `⟪half3¦float3⟫ MixFog｡｡｡｡｡｡(⟪half3¦float3⟫ fragColor, ｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡⟪half¦float⟫ fogFactor)`: グローバル**フォグの色**を適用する。

## 深度

- `⟪half¦float⟫ LinearDepthToEyeDepth(⟪half¦float⟫ rawDepth)`: **線形深度**値を**視点深度**値に変換する。

## アルファ

- **アルファテスト**:
  - `bool IsAlphaToMaskAvailable()`: **アルファテストが有効か**どうかを返す。
  - `⟪half¦real⟫ Alpha⟪Clip¦Discard⟫(⟪half¦real⟫ alpha, ⟪half¦real⟫ cutoff ＠❰, real offset = real(0.0)❱)`:
    - **アルファ値**を**カットオフ値**と**比較**し、カットオフ値未満の場合は**ピクセルを破棄**する。(アルファテスト)
    - Discard: `AlphaClip(alpha, cutoff ❰+ offset❱)`
- **アルファブレンド**:
  - `half OutputAlpha(half alpha, bool isTransparent)`: **最終的**な**アルファ値を出力**する。
  - `half3 AlphaModulate(half3 albedo, half alpha)`: **アルファ値**を使って**アルベドカラーを変調**する。
  - `half3 AlphaPremultiply(half3 albedo, half alpha)`: **アルベドカラー**に**アルファ値を乗算**する。

## サーフェスタイプ

- `bool IsSurfaceType⟪Opaque¦Transparent⟫(half surfaceType)`: 入力が**_＠❰不❱透明サーフェス**を**表すか**どうかを返す。

## その他

- `uint Select4(uint4 v, uint i)`: **uint4型**の変数から**指定されたインデックスの要素**を選択する。
- `uint FIRST_BIT_LOW(uint m)`: **最下位ビット**の**位置**を返す。?
- レンダリングレイヤー
  - `uint GetMeshRenderingLayer()`: **メッシュ**の**レンダリングレイヤー**を取得する。
  - デン/デ コード
    - `float Encode｡MeshRenderingLayer(uint renderingLayer)`:  **メッシュ**の**レンダリングレイヤー**を**エンコード**する。
    - `uint  Decode｡MeshRenderingLayer(float renderingLayer)`: **メッシュ**の**レンダリングレイヤー**を**デコード**する。

## hlslコード

```C
#ifndef UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED
#define UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED

//UnityのUniform変数やShaderkeyWordに関わる関数

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.deprecated.hlsl" //旧式
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingCommon.hlsl" //defined(DEBUG_DISPLAY)

VertexPositionInputs GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;

    // mikkts space compliant. only normalize when extracting normal at frag.
    real sign = real(tangentOS.w) * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = real3(TransformObjectToWorldDir(tangentOS.xyz));
    tbn.bitangentWS = real3(cross(tbn.normalWS, float3(tbn.tangentWS))) * sign;
    return tbn;
}

float4 GetScaledScreenParams()
{
    return _ScaledScreenParams;
}

// Returns 'true' if the current view performs a perspective projection.
bool IsPerspectiveProjection()
{
    return (unity_OrthoParams.w == 0);
}

float3 GetCameraPositionWS()
{
    // Currently we do not support Camera Relative Rendering so
    // we simply return the _WorldSpaceCameraPos until then
    return _WorldSpaceCameraPos;

    // We will replace the code above with this one once
    // we start supporting Camera Relative Rendering
    //#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
    //    return float3(0, 0, 0);
    //#else
    //    return _WorldSpaceCameraPos;
    //#endif
}

// Could be e.g. the position of a primary camera or a shadow-casting light.
float3 GetCurrentViewPosition()
{
    // Currently we do not support Camera Relative Rendering so
    // we simply return the _WorldSpaceCameraPos until then
    return GetCameraPositionWS();

    // We will replace the code above with this one once
    // we start supporting Camera Relative Rendering
    //#if defined(SHADERPASS) && (SHADERPASS != SHADERPASS_SHADOWS)
    //    return GetCameraPositionWS();
    //#else
    //    // This is a generic solution.
    //    // However, for the primary camera, using '_WorldSpaceCameraPos' is better for cache locality,
    //    // and in case we enable camera-relative rendering, we can statically set the position is 0.
    //    return UNITY_MATRIX_I_V._14_24_34;
    //#endif
}

// Returns the forward (central) direction of the current view in the world space.
//ワールド空間における現在のビューの前方（中心）方向を返す。
float3 GetViewForwardDir()
{
    float4x4 viewMat = GetWorldToViewMatrix();
    return -viewMat[2].xyz;
}

// Computes the world space view direction (pointing towards the viewer).
float3 GetWorldSpaceViewDir(float3 positionWS)
{
    if (IsPerspectiveProjection())
    {
        // Perspective
        return GetCurrentViewPosition() - positionWS;
    }
    else
    {
        // Orthographic
        return -GetViewForwardDir();
    }
}

// Computes the object space view direction (pointing towards the viewer).
half3 GetObjectSpaceNormalizeViewDir(float3 positionOS)
{
    if (IsPerspectiveProjection())
    {
        // Perspective
        float3 V = TransformWorldToObject(GetCurrentViewPosition()) - positionOS;
        return half3(normalize(V));
    }
    else
    {
        // Orthographic
        return half3(TransformWorldToObjectNormal(-GetViewForwardDir()));
    }
}

half3 GetWorldSpaceNormalizeViewDir(float3 positionWS)
{
    if (IsPerspectiveProjection())
    {
        // Perspective
        float3 V = GetCurrentViewPosition() - positionWS;
        return half3(normalize(V));
    }
    else
    {
        // Orthographic
        return half3(-GetViewForwardDir());
    }
}

// UNITY_MATRIX_V defines a right-handed view space with the Z axis pointing towards the viewer.
// This function reverses the direction of the Z axis (so that it points forward),
// making the view space coordinate system left-handed.
void GetLeftHandedViewSpaceMatrices(out float4x4 viewMatrix, out float4x4 projMatrix)
{
    viewMatrix = UNITY_MATRIX_V;
    viewMatrix._31_32_33_34 = -viewMatrix._31_32_33_34;

    projMatrix = UNITY_MATRIX_P;
    projMatrix._13_23_33_43 = -projMatrix._13_23_33_43;
}

// Constants that represent material surface types
//
// These are expected to align with the commonly used "_Surface" material property
static const half kSurfaceTypeOpaque = 0.0;
static const half kSurfaceTypeTransparent = 1.0;

// Returns true if the input value represents an opaque surface
bool IsSurfaceTypeOpaque(half surfaceType)
{
    return (surfaceType == kSurfaceTypeOpaque);
}

// Returns true if the input value represents a transparent surface
bool IsSurfaceTypeTransparent(half surfaceType)
{
    return (surfaceType == kSurfaceTypeTransparent);
}

// Only define the alpha clipping helpers when the alpha test define is present.
// This should help identify usage errors early.
#if defined(_ALPHATEST_ON)
// Returns true if AlphaToMask functionality is currently available
// NOTE: This does NOT guarantee that AlphaToMask is enabled for the current draw. It only indicates that AlphaToMask functionality COULD be enabled for it.
//       In cases where AlphaToMask COULD be enabled, we export a specialized alpha value from the shader.
//       When AlphaToMask is enabled:     The specialized alpha value is combined with the sample mask
//       When AlphaToMask is not enabled: The specialized alpha value is either written into the framebuffer or dropped entirely depending on the color write mask
bool IsAlphaToMaskAvailable()
{
    return (_AlphaToMaskAvailable != 0.0);
}

// When AlphaToMask is available:     Returns a modified alpha value that should be exported from the shader so it can be combined with the sample mask
// When AlphaToMask is not available: Terminates the current invocation if the alpha value is below the cutoff and returns the input alpha value otherwise
half AlphaClip(half alpha, half cutoff)
{
    // If the user has specified zero as the cutoff threshold, the expectation is that the shader will function as if alpha-clipping was disabled.
    // Ideally, the user should just turn off the alpha-clipping feature in this case, but in order to make this case work as expected, we force alpha
    // to 1.0 here to ensure that alpha-to-coverage never throws away samples when its active. (This would cause opaque objects to appear transparent)
    alpha = (cutoff <= 0.0) ? 1.0 : alpha;

    // Produce 0.0 if the input value would be clipped by traditional alpha clipping and produce the original input value otherwise.
    // WORKAROUND: The alpha parameter in this ternary expression MUST be converted to a float in order to work around a known HLSL compiler bug.
    //             See Fogbugz 934464 for more information
    half clippedAlpha = (alpha >= cutoff) ? float(alpha) : 0.0;

    // Calculate a specialized alpha value that should be used when alpha-to-coverage is enabled
    half alphaToCoverageAlpha = SharpenAlpha(alpha, cutoff);

    // When alpha-to-coverage is available:     Use the specialized value which will be exported from the shader and combined with the MSAA coverage mask.
    // When alpha-to-coverage is not available: Use the "clipped" value. A clipped value will always result in thread termination via the clip() logic below.
    alpha = IsAlphaToMaskAvailable() ? alphaToCoverageAlpha : clippedAlpha;

    // Terminate any threads that have an alpha value of 0.0 since we know they won't contribute anything to the final image
    clip(alpha - 0.0001);

    return alpha;
}
#endif

// Terminates the current invocation if the input alpha value is below the specified cutoff value and returns an updated alpha value otherwise.
// When provided, the offset value is added to the cutoff value during the comparison logic.
// The return value from this function should be exported as the final alpha value in fragment shaders so it can be combined with the MSAA coverage mask.
//
// When _ALPHATEST_ON is defined:     The returned value follows the behavior noted in the AlphaClip function
// When _ALPHATEST_ON is not defined: The returned value is equal to the original alpha input parameter
//
// NOTE: When _ALPHATEST_ON is not defined, this function is effectively a no-op.
real AlphaDiscard(real alpha, real cutoff, real offset = real(0.0))
{
#ifdef _ALPHATEST_ON
    if (IsAlphaDiscardEnabled())
        alpha = AlphaClip(alpha, cutoff + offset);
#endif

    return alpha;
}

half OutputAlpha(half alpha, bool isTransparent)
{
    if (isTransparent)
    {
        return alpha;
    }
    else
    {
#if defined(_ALPHATEST_ON)
        // Opaque materials should always export an alpha value of 1.0 unless alpha-to-coverage is available
        return IsAlphaToMaskAvailable() ? alpha : 1.0;
#else
        return 1.0;
#endif
    }
}

half3 AlphaModulate(half3 albedo, half alpha)
{
    // Fake alpha for multiply blend by lerping albedo towards 1 (white) using alpha.
    // Manual adjustment for "lighter" multiply effect (similar to "premultiplied alpha")
    // would be painting whiter pixels in the texture.
    // This emulates that procedure in shader, so it should be applied to the base/source color.
#if defined(_ALPHAMODULATE_ON)
    return lerp(half3(1.0, 1.0, 1.0), albedo, alpha);
#else
    return albedo;
#endif
}

half3 AlphaPremultiply(half3 albedo, half alpha)
{
    // Multiply alpha into albedo only for Preserve Specular material diffuse part.
    // Preserve Specular material (glass like) has different alpha for diffuse and specular lighting.
    // Logically this is "variable" Alpha blending.
    // (HW blend mode is premultiply, but with alpha multiply in shader.)
#if defined(_ALPHAPREMULTIPLY_ON)
    return albedo * alpha;
#endif
    return albedo;
}

// Normalization used to depend on SHADER_QUALITY
// Currently we always normalize to avoid lighting issues
// and platform inconsistencies.
half3 NormalizeNormalPerVertex(half3 normalWS)
{
    return normalize(normalWS);
}

float3 NormalizeNormalPerVertex(float3 normalWS)
{
    return normalize(normalWS);
}

half3 NormalizeNormalPerPixel(half3 normalWS)
{
// With XYZ normal map encoding we sporadically sample normals with near-zero-length causing Inf/NaN
#if defined(UNITY_NO_DXT5nm) && defined(_NORMALMAP)
    return SafeNormalize(normalWS);
#else
    return normalize(normalWS);
#endif
}

float3 NormalizeNormalPerPixel(float3 normalWS)
{
#if defined(UNITY_NO_DXT5nm) && defined(_NORMALMAP)
    return SafeNormalize(normalWS);
#else
    return normalize(normalWS);
#endif
}



real ComputeFogFactorZ0ToFar(float z)
{
    #if defined(FOG_LINEAR)
    // factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
    float fogFactor = saturate(z * unity_FogParams.z + unity_FogParams.w);
    return real(fogFactor);
    #elif defined(FOG_EXP) || defined(FOG_EXP2)
    // factor = exp(-(density*z)^2)
    // -density * z computed at vertex
    return real(unity_FogParams.x * z);
    #else
        return real(0.0);
    #endif
}

real ComputeFogFactor(float zPositionCS)
{
    float clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(zPositionCS);
    return ComputeFogFactorZ0ToFar(clipZ_0Far);
}

half ComputeFogIntensity(half fogFactor)
{
    half fogIntensity = half(0.0);
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #if defined(FOG_EXP)
            // factor = exp(-density*z)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor));
        #elif defined(FOG_EXP2)
            // factor = exp(-(density*z)^2)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor * fogFactor));
        #elif defined(FOG_LINEAR)
            fogIntensity = fogFactor;
        #endif
    #endif
    return fogIntensity;
}

// Force enable fog fragment shader evaluation
#define _FOG_FRAGMENT 1
real InitializeInputDataFog(float4 positionWS, real vertFogFactor)
{
    real fogFactor = 0.0;
#if defined(_FOG_FRAGMENT)
    #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        // Compiler eliminates unused math --> matrix.column_z * vec
        float viewZ = -(mul(UNITY_MATRIX_V, positionWS).z);
        // View Z is 0 at camera pos, remap 0 to near plane.
        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
        fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
    #endif
#else
    fogFactor = vertFogFactor;
#endif
    return fogFactor;
}

float ComputeFogIntensity(float fogFactor)
{
    float fogIntensity = 0.0;
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #if defined(FOG_EXP)
            // factor = exp(-density*z)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor));
        #elif defined(FOG_EXP2)
            // factor = exp(-(density*z)^2)
            // fogFactor = density*z compute at vertex
            fogIntensity = saturate(exp2(-fogFactor * fogFactor));
        #elif defined(FOG_LINEAR)
            fogIntensity = fogFactor;
        #endif
    #endif
    return fogIntensity;
}

half3 MixFogColor(half3 fragColor, half3 fogColor, half fogFactor)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        half fogIntensity = ComputeFogIntensity(fogFactor);
        fragColor = lerp(fogColor, fragColor, fogIntensity);
    #endif
    return fragColor;
}

float3 MixFogColor(float3 fragColor, float3 fogColor, float fogFactor)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    if (IsFogEnabled())
    {
        float fogIntensity = ComputeFogIntensity(fogFactor);
        fragColor = lerp(fogColor, fragColor, fogIntensity);
    }
    #endif
    return fragColor;
}

half3 MixFog(half3 fragColor, half fogFactor)
{
    return MixFogColor(fragColor, unity_FogColor.rgb, fogFactor);
}

float3 MixFog(float3 fragColor, float fogFactor)
{
    return MixFogColor(fragColor, unity_FogColor.rgb, fogFactor);
}

// Linear depth buffer value between [0, 1] or [1, 0] to eye depth value between [near, far]
half LinearDepthToEyeDepth(half rawDepth)
{
    #if UNITY_REVERSED_Z
        return half(_ProjectionParams.z - (_ProjectionParams.z - _ProjectionParams.y) * rawDepth);
    #else
        return half(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * rawDepth);
    #endif
}

float LinearDepthToEyeDepth(float rawDepth)
{
    #if UNITY_REVERSED_Z
        return _ProjectionParams.z - (_ProjectionParams.z - _ProjectionParams.y) * rawDepth;
    #else
        return _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * rawDepth;
    #endif
}

void TransformScreenUV(inout float2 uv, float screenHeight)
{
    #if UNITY_UV_STARTS_AT_TOP
    uv.y = screenHeight - (uv.y * _ScaleBiasRt.x + _ScaleBiasRt.y * screenHeight);
    #endif
}

void TransformScreenUV(inout float2 uv)
{
    #if UNITY_UV_STARTS_AT_TOP
    TransformScreenUV(uv, GetScaledScreenParams().y);
    #endif
}

void TransformNormalizedScreenUV(inout float2 uv)
{
    #if UNITY_UV_STARTS_AT_TOP
    TransformScreenUV(uv, 1.0);
    #endif
}

float2 GetNormalizedScreenSpaceUV(float2 positionCS)
{
    float2 normalizedScreenSpaceUV = positionCS.xy * rcp(GetScaledScreenParams().xy);
    TransformNormalizedScreenUV(normalizedScreenSpaceUV);
    return normalizedScreenSpaceUV;
}

float2 GetNormalizedScreenSpaceUV(float4 positionCS)
{
    return GetNormalizedScreenSpaceUV(positionCS.xy);
}

// Select uint4 component by index.
// Helper to improve codegen for 2d indexing (data[x][y])
// Replace:
// data[i / 4][i % 4];
// with:
// select4(data[i / 4], i % 4);
uint Select4(uint4 v, uint i)
{
    // x = 0 = 00
    // y = 1 = 01
    // z = 2 = 10
    // w = 3 = 11
    uint mask0 = uint(int(i << 31) >> 31);
    uint mask1 = uint(int(i << 30) >> 31);
    return
        (((v.w & mask0) | (v.z & ~mask0)) & mask1) |
        (((v.y & mask0) | (v.x & ~mask0)) & ~mask1);
}

#if SHADER_TARGET < 45
uint URP_FirstBitLow(uint m)
{
    // http://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightFloatCast
    return (asuint((float)(m & asuint(-asint(m)))) >> 23) - 0x7F;
}
#define FIRST_BIT_LOW URP_FirstBitLow
#else
#define FIRST_BIT_LOW firstbitlow
#endif

#define UnityStereoTransformScreenSpaceTex(uv) uv

uint GetMeshRenderingLayer()
{
    return asuint(unity_RenderingLayer.x);
}

float EncodeMeshRenderingLayer(uint renderingLayer)
{
    // Force any bits above max to be skipped
    renderingLayer &= _RenderingLayerMaxInt;

    // This is copy of "real PackInt(uint i, uint numBits)" from com.unity.render-pipelines.core\ShaderLibrary\Packing.hlsl
    // Differences of this copy:
    // - Pre-computed rcpMaxInt
    // - Returns float instead of real
    float rcpMaxInt = _RenderingLayerRcpMaxInt;
    return saturate(renderingLayer * rcpMaxInt);
}

uint DecodeMeshRenderingLayer(float renderingLayer)
{
    // This is copy of "uint UnpackInt(real f, uint numBits)" from com.unity.render-pipelines.core\ShaderLibrary\Packing.hlsl
    // Differences of this copy:
    // - Pre-computed maxInt
    // - Parameter f is float instead of real
    uint maxInt = _RenderingLayerMaxInt;
    return (uint)(renderingLayer * maxInt + 0.5); // Round instead of truncating
}

#endif // UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED

```
