//6000_0_0b13
//●←検索。HLSL組み込み関数をまとめる?rcp(..)とか。 unity_ObjectToWorld とかを #define O2w unity_ObjectToWorld にしたい
//関数名[\s]*\(.*\).*?[\n]?\{
//APV関連===============================================================================================================
    //APVのbakedGIは、uniform Texture3D _APVResL⟪0¦1⟫からuvwでサンプリングしSHEvalLinearL1(..)で算出していた (詳しくは全然追ってない(L2は使ってないみたい))
        //bakedGI  <==❰SHEvalLinearL1(normal, APVSample.L1⟪R¦G¦B⟫) + APVSample.L0❱==  struct APVSample{half3 L⟪0¦1⟫}  
        //<==❰ APVResources.L⟪0¦1⟫.SampleLevel(.., uvw,..)❱==  struct APVResources{Texture3D L⟪0¦1⟫}  <=  uniform❰Texture3D _APVResL⟪0¦1⟫❱
    //uniform========================================================================================
        //core/Runtime/Lighting/ProbeVolume/ShaderVariablesProbeVolumes.cs.hlsl====================
        cbuffer ShaderVariablesProbeVolumes
        {
            float4 _Offset_IndirectionEntryDim; // インデクシングエントリの次元とオフセット
            float4 _Weight_MinLoadedCellInEntries; // エントリ内の最小ロードセルの重み
            float4 _PoolDim_MinBrickSize; // プールの次元と最小ブリックサイズ
            float4 _RcpPoolDim_XY; // プールの逆次元(XY)
            float4 _MinEntryPos_Noise; // エントリの最小位置とノイズ
            float4 _IndicesDim_IndexChunkSize; // インデックスの次元とインデックスチャンクサイズ
            float4 _Biases_NormalizationClamp; // バイアスと正規化クランプ
            float4 _LeakReduction_SkyOcclusion; // リークリダクションとスカイオクルージョン
            float4 _MaxLoadedCellInEntries_FrameIndex; // フレームインデックス内の最大ロードセル
        };
        //core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl====================
        StructuredBuffer < int > _APVResIndex; // APVリソースのインデックスを格納する構造化バッファ
        StructuredBuffer < uint3 > _APVResCellIndices; // APVリソースのセルインデックスを格納する構造化バッファ
        StructuredBuffer < float3 > _SkyPrecomputedDirections; // スカイの事前計算された方向を格納する構造化バッファ
        
        Texture3D _APVResL0_L1Rx; // APVリソースのL0とL1のR成分を格納するテクスチャ
        Texture3D _APVResL1G_L1Ry; // APVリソースのL1のG成分とL1のR成分のY成分を格納するテクスチャ
        Texture3D _APVResL1B_L1Rz; // APVリソースのL1のB成分とL1のR成分のZ成分を格納するテクスチャ
        Texture3D _APVResL2_0; // APVリソースのL2の0成分を格納するテクスチャ
        Texture3D _APVResL2_1; // APVリソースのL2の1成分を格納するテクスチャ
        Texture3D _APVResL2_2; // APVリソースのL2の2成分を格納するテクスチャ
        Texture3D _APVResL2_3; // APVリソースのL2の3成分を格納するテクスチャ
        
        Texture3D _APVResValidity; // APVリソースの有効性を格納するテクスチャ
        
        Texture3D _SkyOcclusionTexL0L1; // スカイのオクルージョンテクスチャ（L0とL1）
        Texture3D _SkyShadingDirectionIndicesTex; // スカイのシェーディング方向のインデックスを格納するテクスチャ
        uint _EnableProbeVolumes; //APV有効フラグ
    //struct==========================================================================================
        //core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
        struct APVResources
        {
            StructuredBuffer < int > index;
            StructuredBuffer < float3 > SkyPrecomputedDirections;

            Texture3D L0_L1Rx;  //= _APVResL0_L1Rx
            Texture3D L1G_L1Ry; //= _APVResL1G_L1Ry
            Texture3D L1B_L1Rz; //= _APVResL1B_L1Rz
            Texture3D L2_0;
            Texture3D L2_1;
            Texture3D L2_2;
            Texture3D L2_3;

            Texture3D Validity;

            Texture3D SkyOcclusionL0L1;
            Texture3D SkyShadingDirectionIndices;
        };
        SamplerState s_linear_clamp_sampler; 
        //core/Runtime/Lighting/ProbeVolume/DecodeSH.hlsl
        half3 DecodeSH(half l0, half3 l1)
        {
            return (l1 - 0.5) * (2.0 * 2.0 * l0);
        }
        float3 EncodeSH(float l0, float3 l1)
        {
            return l0 == 0.0f ? 0.5f : l1 * rcp(l0)/ (2.0f * 2.0) + 0.5f;
        }
        //core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
        struct APVSample
        {
            half3 L0;   //= L0_L1Rx.xyz
            half3 L1_R; //= L0_L1R⟪x¦y¦z⟫.w    ❰apvSample.L1_R = half3(L0_L1Rx.w, L1G_L1Ry.w, L1B_L1Rz.w)❱
            half3 L1_G; //= L1G_L1Ry.xyz
            half3 L1_B; //= L1B_L1Rz.xyz
            float4 skyOcclusionL0L1;
            float3 skyShadingDirection;
            int status;
            void Decode()
            {
                if(status == 0)
                {
                    L1_R = DecodeSH(L0.r, L1_R);
                    L1_G = DecodeSH(L0.g, L1_G);
                    L1_B = DecodeSH(L0.b, L1_B);
                    status = 1;
                }
            }

            void Encode()
            {
                if(status == 1)
                {
                    L1_R = EncodeSH(L0.r, L1_R);
                    L1_G = EncodeSH(L0.g, L1_G);
                    L1_B = EncodeSH(L0.b, L1_B);
                    status = 0;
                }
            }
        };
//クラスター関係==========================================================================================================
    //uniform============================================
        float4 _FPParams0;
        float4 _FPParams1;
        // float4 _FPParams2; //使ってない?
        cbuffer urp_ZBinBuffer
        {
            float4 urp_ZBins[1024];
        };
        cbuffer urp_TileBuffer
        {
            float4 urp_Tiles[4096];
        };
    //struct==============================================
        struct ClusterIterator
        {
            uint tileOffset;
            uint zBinOffset;
            uint tileMask;
            uint entityIndexNextMax;
        };
//型============================================================================================================================================================================
//universal/ShaderLibrary/BRDF.hlsl
struct BRDFData //struct SurfaceDataから作る
{
    half3 albedo;
    half3 diffuse;
    half3 specular;
    half reflectivity;
    half perceptualRoughness;
    half roughness;
    half roughness2;
    half grazingTerm;
    half normalizationTerm;
    half roughness2MinusOne;
};
//universal/ShaderLibrary/RealtimeLights.hlsl
struct Light //●
{
    half3 direction;
    half3 color;
    float distanceAttenuation;
    half shadowAttenuation;
    uint layerMask;
};
//universal/ShaderLibrary/Lighting.hlsl
struct LightingData //各種ライティング結果を保存している
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 vertexLightingColor;
    half3 emissionColor;
};
//universal/ShaderLibrary/Input.hlsl
struct InputData //●
{
    float3 positionWS;
    float4 positionCS;
    float3 normalWS;
    half3 viewDirectionWS;
    float4 shadowCoord;
    half fogCoord;
    half3 vertexLighting;
    half3 bakedGI;
    float2 normalizedScreenSpaceUV;
    half4 shadowMask;
    half3x3 tangentToWorld;
};
float4 _ScaledScreenParams;
//universal/ShaderLibrary/SurfaceData.hlsl
struct SurfaceData //●
{
    half3 albedo;
    half3 specular;
    half metallic;
    half smoothness;
    half3 normalTS;
    half3 emission;
    half occlusion;
    half alpha;
    half clearCoatMask;
    half clearCoatSmoothness;
};
//universal/ShaderLibrary/Core.hlsl
struct VertexPositionInputs //●
{
    float3 positionWS; // ワールドスペースでの頂点位置
    float3 positionVS; // ビュースペースでの頂点位置
    float4 positionCS; // クリップスペースでの頂点位置
    float4 positionNDC; // 正規化デバイス座標での頂点位置
};
//universal/ShaderLibrary/Core.hlsl 
struct VertexNormalInputs //●
{
    float3 tangentWS;
    float3 bitangentWS;
    float3 normalWS;
};
//universal/ShaderLibrary/AmbientOcclusion.hlsl
struct AmbientOcclusionFactor //SSAO (_ScreenSpaceOcclusionTexture, _AmbientOcclusionParam.w)
{
    half indirectAmbientOcclusion; //間接ao
    half directAmbientOcclusion; //直接ao
};
//universal/ShaderLibrary/Shadows.hlsl
struct ShadowSamplingData
{
    half4 shadowOffset0; //ソフトシャドウのサンプリング時にオフセットする
    half4 shadowOffset1;
    float4 shadowmapSize; //使ってない
    half softShadowQuality; //ソフトシャドウの品質
};
//Input系=======================================================================================================================================================================
    //universal/ShaderLibrary/UnityInput.hlsl====================

    //座標変換行列とカメラ位置 //●
    float4x4 unity_MatrixV;
    float4x4 unity_MatrixInvV;
    float4x4 unity_MatrixVP;
    float3 _WorldSpaceCameraPos;
    //カメラパラムス //●
    float4 _ProjectionParams;
    float4 unity_OrthoParams; //.w == 0 なら Perspective

    cbuffer UnityPerDraw{
        //座標変換行列とモーションベクトル関係 //●
        float4x4 unity_ObjectToWorld; // オブジェクトのワールド座標変換行列
        float4x4 unity_WorldToObject; // ワールドからオブジェクトへの座標変換行列
        float4x4 unity_MatrixPreviousM; // 前フレームのモデル行列
        float4x4 unity_MatrixPreviousMI; // 前フレームのモデル行列の逆行列
        float4 unity_MotionVectorsParams; // モーションベクトルのパラメータ
        float4 unity_WorldTransformParams; //.wは、 unity_ObjectToWorld の行列式が負だと-1?// ワールド変換のパラメータ

        //レンダリングレイヤーと視錐台の範囲? //●
        float4 unity_RenderingLayer; //レンダリングレイヤー
        float4 unity_RendererBounds_Min; // レンダラーの境界ボックスの最小値
        float4 unity_RendererBounds_Max; // レンダラーの境界ボックスの最大値

        //その他
        float4 unity_LODFade; // LODのフェードパラメータ
        half4 unity_LightData; // ライトデータ
        half4 unity_LightIndices[2]; // ライトインデックス

        //shadowMask
        float4 unity_ProbesOcclusion; // プローブのオクルージョン

        //リフレクションプローブ関係(使われていない。代わりに cbuffer urp_ReflectionProbeBuffer が使われている)
        float4 unity_SpecCube0_HDR; // スペキュラキューブ0のHDRデータ
        float4 unity_SpecCube0_BoxMax; // スペキュラキューブ0の境界ボックスの最大値
        float4 unity_SpecCube0_BoxMin; // スペキュラキューブ0の境界ボックスの最小値
        float4 unity_SpecCube0_ProbePosition; // スペキュラキューブ0のプローブ位置
        float4 unity_SpecCube1_HDR; // スペキュラキューブ1のHDRデータ
        float4 unity_SpecCube1_BoxMax; // スペキュラキューブ1の境界ボックスの最大値
        float4 unity_SpecCube1_BoxMin; // スペキュラキューブ1の境界ボックスの最小値
        float4 unity_SpecCube1_ProbePosition; // スペキュラキューブ1のプローブ位置

        //ライトマップ関係
        float4 unity_LightmapST; // ライトマップのテクスチャ座標変換行列
        float4 unity_DynamicLightmapST; // ダイナミックライトマップのテクスチャ座標変換行列
        
        //ライトプローブ関係
        float4 unity_SHAr; // 環境光のシーンハーモニクス係数
        float4 unity_SHAg;
        float4 unity_SHAb;
        float4 unity_SHBr;
        float4 unity_SHBg;
        float4 unity_SHBb;
        float4 unity_SHC;
        
        //スプライト関係
        float4 unity_SpriteColor; // スプライトのカラー
        float4 unity_SpriteProps; // スプライトのプロパティ
    };
    uniform float4 _ScaleBiasRt;
    float4 unity_FogColor;
    //universal/Shaders/LitInput.hlsl====================
    cbuffer UnityPerMaterial{
        float4 _BaseMap_ST; // ベースマップのテクスチャ座標変換行列
        float4 _DetailAlbedoMap_ST; // ディテールアルベドマップのテクスチャ座標変換行列
        half4 _BaseColor; // ベースカラー
        half4 _SpecColor; // スペキュラカラー
        half4 _EmissionColor; // エミッションカラー
        half _Cutoff; // カットオフ値
        half _Smoothness; // スムーズネス
        half _Metallic; // メタリック
        half _BumpScale; // バンプスケール
        half _Parallax; // パララックス
        half _OcclusionStrength; // オクルージョンの強度
        half _ClearCoatMask; // クリアコートマスク
        half _ClearCoatSmoothness; // クリアコートのスムーズネス
        half _DetailAlbedoMapScale; // ディテールアルベドマップのスケール
        half _DetailNormalMapScale; // ディテールノーマルマップのスケール
        half _Surface; // サーフェス
        float4 unity_MipmapStreaming_DebugTex_ST; // ミップマップストリーミングのデバッグテクスチャ座標変換行列
        float4 unity_MipmapStreaming_DebugTex_TexelSize; // ミップマップストリーミングのデバッグテクスチャのテクセルサイズ
        float4 unity_MipmapStreaming_DebugTex_MipInfo; // ミップマップストリーミングのデバッグテクスチャのミップ情報
        float4 unity_MipmapStreaming_DebugTex_StreamInfo; // ミップマップストリーミングのデバッグテクスチャのストリーム情報
    };
    //universal/ShaderLibrary/Shadows.hlsl====================
    cbuffer LightShadows{
        //カスケード関連
        float4x4 _MainLightWorldToShadow[4 + 1]; // メインライトのワールド座標からシャドウマップへの変換行列
        float4 _CascadeShadowSplitSpheres0; // カスケードシャドウの分割球の情報
        float4 _CascadeShadowSplitSpheres1;
        float4 _CascadeShadowSplitSpheres2;
        float4 _CascadeShadowSplitSpheres3;
        float4 _CascadeShadowSplitSphereRadii; // カスケードシャドウの分割球の半径
        //メインライトシャドウ関連
        float4 _MainLightShadowOffset0; //ソフトシャドウのサンプリング時にオフセットする
        float4 _MainLightShadowOffset1;
        float4 _MainLightShadowParams;  //shadowStrength(.x)とソフトシャドウの品質(.y) とシャドーフェイド(.zw)
        float4 _MainLightShadowmapSize; // メインライトのシャドウマップのサイズ //使ってない
        //追加ライトシャドウ関連
        float4 _AdditionalShadowOffset0; // 追加ライトのシャドウオフセット
        float4 _AdditionalShadowOffset1;
        float4 _AdditionalShadowFadeParams; // 追加ライトのシャドウフェードパラメータ
        float4 _AdditionalShadowmapSize; // 追加ライトのシャドウマップのサイズ
        float4 _AdditionalShadowParams[(256)]; // 追加ライトのシャドウパラメータ
        float4x4 _AdditionalLightsWorldToShadow[(256)]; // 追加ライトのワールド座標からシャドウマップへの変換行列
    };
    Texture2D _MainLightShadowmapTexture;
    Texture2D _AdditionalLightsShadowmapTexture;
    SamplerComparisonState sampler_LinearClampCompare;
    //universal/ShaderLibrary/Input.hlsl====================

    //リフレクションプローブ関係
    cbuffer urp_ReflectionProbeBuffer //ClusterInit(..),ClusterNext(..,index)で取得される
    {
        float4 urp_ReflProbes_BoxMax[64]; // 反射プローブの境界ボックスの最大値
        float4 urp_ReflProbes_BoxMin[64]; // 反射プローブの境界ボックスの最小値
        float4 urp_ReflProbes_ProbePosition[64]; // 反射プローブの位置
        float4 urp_ReflProbes_MipScaleOffset[64 * 7]; // 反射プローブのミップマップのスケールとオフセット
    };
    Texture2D urp_ReflProbes_Atlas; // 反射プローブのアトラステクスチャ
    half4 _GlossyEnvironmentCubeMap_HDR; // 光沢環境キューブマップのHDRデータ
    TextureCube _GlossyEnvironmentCubeMap; // 光沢環境キューブマップ
    SamplerState sampler_GlossyEnvironmentCubeMap; // 光沢環境キューブマップのサンプラーステート

    //メインライト情報 //●
    float4 _MainLightPosition;
    half4 _MainLightColor;
    half4 _MainLightOcclusionProbes; 
    uint _MainLightLayerMask;
    //追加ライト情報 //●
    cbuffer AdditionalLights //ClusterInit(..),ClusterNext(..,index)で取得される
    {
        float4 _AdditionalLightsPosition[(256)]; // 追加ライトの位置
        
        half4 _AdditionalLightsColor[(256)]; // 追加ライトの色
        half4 _AdditionalLightsAttenuation[(256)]; // 追加ライトの減衰
        half4 _AdditionalLightsSpotDir[(256)]; // 追加ライトのスポットライトの方向
        half4 _AdditionalLightsOcclusionProbes[(256)]; // 追加ライトのオクルージョンプローブ
        float _AdditionalLightsLayerMasks[(256)]; // 追加ライトのレイヤーマスク
    };

    //universal/ShaderLibrary/SurfaceInput.hlsl====================
    //テクスチャー
    Texture2D _BaseMap;
    SamplerState sampler_BaseMap;
    Texture2D _BumpMap;
    SamplerState sampler_BumpMap;
    Texture2D _EmissionMap;
    SamplerState sampler_EmissionMap;

    //SSAO関連================================================================================================================================================
        //universal/ShaderLibrary/Input.hlsl====================
        half4 _AmbientOcclusionParam; //SSAOの強度?
        //universal/ShaderLibrary/AmbientOcclusion.hlsl====================
        Texture2D _ScreenSpaceOcclusionTexture;

    //グローバルな、Mipのバイアスとサンプラー(.SampleBias(..,_GlobalMipBias))
        //universal/ShaderLibrary/Input.hlsl====================
        float2 _GlobalMipBias;
        //core/ShaderLibrary/GlobalSamplers.hlsl====================
        SamplerState sampler_LinearClamp;

//定数==========================================================================================================================================================================
    //universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
    static const half kSurfaceTypeTransparent = 1.0;

//Level:7～12 Shadow と ProbeVolume しか無い
//Level:12=======================================================================================================================================================================
//core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl
float SampleShadow_GetTriangleTexelArea(float triangleHeight)
{
    return triangleHeight - 0.5;
}
//Level:11=======================================================================================================================================================================
//core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
void SampleShadow_GetTexelAreas_Tent_3x3(float offset, out float4 computedArea, out float4 computedAreaUncut)
{
    float offset01SquaredHalved = (offset + 0.5) * (offset + 0.5) * 0.5;
    computedAreaUncut.x = computedArea.x = offset01SquaredHalved - offset;
    computedAreaUncut.w = computedArea.w = offset01SquaredHalved;
    computedAreaUncut.y = SampleShadow_GetTriangleTexelArea(1.5 - offset);
    float clampedOffsetLeft = min(offset, 0);
    float areaOfSmallLeftTriangle = clampedOffsetLeft * clampedOffsetLeft;
    computedArea.y = computedAreaUncut.y - areaOfSmallLeftTriangle;
    computedAreaUncut.z = SampleShadow_GetTriangleTexelArea(1.5 + offset);
    float clampedOffsetRight = max(offset, 0);
    float areaOfSmallRightTriangle = clampedOffsetRight * clampedOffsetRight;
    computedArea.z = computedAreaUncut.z - areaOfSmallRightTriangle;
}
//Level:10=======================================================================================================================================================================
//core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl
void SampleShadow_GetTexelWeights_Tent_5x5(float offset, out float3 texelsWeightsA, out float3 texelsWeightsB)
{
    float4 computedArea_From3texelTriangle;
    float4 computedAreaUncut_From3texelTriangle;
    SampleShadow_GetTexelAreas_Tent_3x3(offset, computedArea_From3texelTriangle, computedAreaUncut_From3texelTriangle);
    texelsWeightsA.x = 0.16 * (computedArea_From3texelTriangle.x);
    texelsWeightsA.y = 0.16 * (computedAreaUncut_From3texelTriangle.y);
    texelsWeightsA.z = 0.16 * (computedArea_From3texelTriangle.y + 1);
    texelsWeightsB.x = 0.16 * (computedArea_From3texelTriangle.z + 1);
    texelsWeightsB.y = 0.16 * (computedAreaUncut_From3texelTriangle.z);
    texelsWeightsB.z = 0.16 * (computedArea_From3texelTriangle.w);
}
//core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl
void SampleShadow_GetTexelWeights_Tent_7x7(float offset, out float4 texelsWeightsA, out float4 texelsWeightsB)
{
float4 computedArea_From3texelTriangle;
float4 computedAreaUncut_From3texelTriangle;
SampleShadow_GetTexelAreas_Tent_3x3(offset, computedArea_From3texelTriangle, computedAreaUncut_From3texelTriangle);
texelsWeightsA.x = 0.081632 * (computedArea_From3texelTriangle.x);
texelsWeightsA.y = 0.081632 * (computedAreaUncut_From3texelTriangle.y);
texelsWeightsA.z = 0.081632 * (computedAreaUncut_From3texelTriangle.y + 1);
texelsWeightsA.w = 0.081632 * (computedArea_From3texelTriangle.y + 2);
texelsWeightsB.x = 0.081632 * (computedArea_From3texelTriangle.z + 2);
texelsWeightsB.y = 0.081632 * (computedAreaUncut_From3texelTriangle.z + 1);
texelsWeightsB.z = 0.081632 * (computedAreaUncut_From3texelTriangle.z);
texelsWeightsB.w = 0.081632 * (computedArea_From3texelTriangle.w);
}
//Level:9========================================================================================================================================================================
//core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl
void SampleShadow_ComputeSamples_Tent_5x5(float4 shadowMapTexture_TexelSize, float2 coord, out float fetchesWeights[9], out float2 fetchesUV[9])
{
    float2 tentCenterInTexelSpace = coord.xy * shadowMapTexture_TexelSize.zw;
    float2 centerOfFetchesInTexelSpace = floor(tentCenterInTexelSpace + 0.5);
    float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace;
    //#line 151
    float3 texelsWeightsU_A, texelsWeightsU_B;
    float3 texelsWeightsV_A, texelsWeightsV_B;
    SampleShadow_GetTexelWeights_Tent_5x5(offsetFromTentCenterToCenterOfFetches.x, texelsWeightsU_A, texelsWeightsU_B);
    SampleShadow_GetTexelWeights_Tent_5x5(offsetFromTentCenterToCenterOfFetches.y, texelsWeightsV_A, texelsWeightsV_B);
    //#line 157
    float3 fetchesWeightsU = float3(texelsWeightsU_A.xz, texelsWeightsU_B.y) + float3(texelsWeightsU_A.y, texelsWeightsU_B.xz);
    float3 fetchesWeightsV = float3(texelsWeightsV_A.xz, texelsWeightsV_B.y) + float3(texelsWeightsV_A.y, texelsWeightsV_B.xz);
    //#line 161
    float3 fetchesOffsetsU = float3(texelsWeightsU_A.y, texelsWeightsU_B.xz)/ fetchesWeightsU.xyz + float3(- 2.5, - 0.5, 1.5);
    float3 fetchesOffsetsV = float3(texelsWeightsV_A.y, texelsWeightsV_B.xz)/ fetchesWeightsV.xyz + float3(- 2.5, - 0.5, 1.5);
    fetchesOffsetsU *= shadowMapTexture_TexelSize.xxx;
    fetchesOffsetsV *= shadowMapTexture_TexelSize.yyy;

    float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * shadowMapTexture_TexelSize.xy;
    fetchesUV[0] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.x);
    fetchesUV[1] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.x);
    fetchesUV[2] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.x);
    fetchesUV[3] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.y);
    fetchesUV[4] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.y);
    fetchesUV[5] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.y);
    fetchesUV[6] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.z);
    fetchesUV[7] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.z);
    fetchesUV[8] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.z);

    fetchesWeights[0] = fetchesWeightsU.x * fetchesWeightsV.x;
    fetchesWeights[1] = fetchesWeightsU.y * fetchesWeightsV.x;
    fetchesWeights[2] = fetchesWeightsU.z * fetchesWeightsV.x;
    fetchesWeights[3] = fetchesWeightsU.x * fetchesWeightsV.y;
    fetchesWeights[4] = fetchesWeightsU.y * fetchesWeightsV.y;
    fetchesWeights[5] = fetchesWeightsU.z * fetchesWeightsV.y;
    fetchesWeights[6] = fetchesWeightsU.x * fetchesWeightsV.z;
    fetchesWeights[7] = fetchesWeightsU.y * fetchesWeightsV.z;
    fetchesWeights[8] = fetchesWeightsU.z * fetchesWeightsV.z;
}
//core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl
void SampleShadow_ComputeSamples_Tent_7x7(float4 shadowMapTexture_TexelSize, float2 coord, out float fetchesWeights[16], out float2 fetchesUV[16])
{

float2 tentCenterInTexelSpace = coord.xy * shadowMapTexture_TexelSize.zw;
float2 centerOfFetchesInTexelSpace = floor(tentCenterInTexelSpace + 0.5);
float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace;
//#line 197
float4 texelsWeightsU_A, texelsWeightsU_B;
float4 texelsWeightsV_A, texelsWeightsV_B;
SampleShadow_GetTexelWeights_Tent_7x7(offsetFromTentCenterToCenterOfFetches.x, texelsWeightsU_A, texelsWeightsU_B);
SampleShadow_GetTexelWeights_Tent_7x7(offsetFromTentCenterToCenterOfFetches.y, texelsWeightsV_A, texelsWeightsV_B);
//#line 203
float4 fetchesWeightsU = float4(texelsWeightsU_A.xz, texelsWeightsU_B.xz) + float4(texelsWeightsU_A.yw, texelsWeightsU_B.yw);
float4 fetchesWeightsV = float4(texelsWeightsV_A.xz, texelsWeightsV_B.xz) + float4(texelsWeightsV_A.yw, texelsWeightsV_B.yw);
//#line 207
float4 fetchesOffsetsU = float4(texelsWeightsU_A.yw, texelsWeightsU_B.yw)/ fetchesWeightsU.xyzw + float4(- 3.5, - 1.5, 0.5, 2.5);
float4 fetchesOffsetsV = float4(texelsWeightsV_A.yw, texelsWeightsV_B.yw)/ fetchesWeightsV.xyzw + float4(- 3.5, - 1.5, 0.5, 2.5);
fetchesOffsetsU *= shadowMapTexture_TexelSize.xxxx;
fetchesOffsetsV *= shadowMapTexture_TexelSize.yyyy;

float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * shadowMapTexture_TexelSize.xy;
fetchesUV[0] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.x);
fetchesUV[1] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.x);
fetchesUV[2] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.x);
fetchesUV[3] = bilinearFetchOrigin + float2(fetchesOffsetsU.w, fetchesOffsetsV.x);
fetchesUV[4] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.y);
fetchesUV[5] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.y);
fetchesUV[6] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.y);
fetchesUV[7] = bilinearFetchOrigin + float2(fetchesOffsetsU.w, fetchesOffsetsV.y);
fetchesUV[8] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.z);
fetchesUV[9] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.z);
fetchesUV[10] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.z);
fetchesUV[11] = bilinearFetchOrigin + float2(fetchesOffsetsU.w, fetchesOffsetsV.z);
fetchesUV[12] = bilinearFetchOrigin + float2(fetchesOffsetsU.x, fetchesOffsetsV.w);
fetchesUV[13] = bilinearFetchOrigin + float2(fetchesOffsetsU.y, fetchesOffsetsV.w);
fetchesUV[14] = bilinearFetchOrigin + float2(fetchesOffsetsU.z, fetchesOffsetsV.w);
fetchesUV[15] = bilinearFetchOrigin + float2(fetchesOffsetsU.w, fetchesOffsetsV.w);

fetchesWeights[0] = fetchesWeightsU.x * fetchesWeightsV.x;
fetchesWeights[1] = fetchesWeightsU.y * fetchesWeightsV.x;
fetchesWeights[2] = fetchesWeightsU.z * fetchesWeightsV.x;
fetchesWeights[3] = fetchesWeightsU.w * fetchesWeightsV.x;
fetchesWeights[4] = fetchesWeightsU.x * fetchesWeightsV.y;
fetchesWeights[5] = fetchesWeightsU.y * fetchesWeightsV.y;
fetchesWeights[6] = fetchesWeightsU.z * fetchesWeightsV.y;
fetchesWeights[7] = fetchesWeightsU.w * fetchesWeightsV.y;
fetchesWeights[8] = fetchesWeightsU.x * fetchesWeightsV.z;
fetchesWeights[9] = fetchesWeightsU.y * fetchesWeightsV.z;
fetchesWeights[10] = fetchesWeightsU.z * fetchesWeightsV.z;
fetchesWeights[11] = fetchesWeightsU.w * fetchesWeightsV.z;
fetchesWeights[12] = fetchesWeightsU.x * fetchesWeightsV.w;
fetchesWeights[13] = fetchesWeightsU.y * fetchesWeightsV.w;
fetchesWeights[14] = fetchesWeightsU.z * fetchesWeightsV.w;
fetchesWeights[15] = fetchesWeightsU.w * fetchesWeightsV.w;
}
//Level:8========================================================================================================================================================================
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
bool LoadCellIndexMetaData(int cellFlatIdx, out int chunkIndex, out int stepSize, out int3 minRelativeIdx, out int3 maxRelativeIdxPlusOne)
{
    bool cellIsLoaded = false;
    uint3 metaData = _APVResCellIndices[cellFlatIdx];

    if(metaData.x != 0xFFFFFFFF)
    {
        chunkIndex = metaData.x & 0x1FFFFFFF;
        stepSize = pow(3,(metaData.x >> 29) & 0x7);

        minRelativeIdx.x = metaData.y & 0x3FF;
        minRelativeIdx.y = (metaData.y >> 10) & 0x3FF;
        minRelativeIdx.z = (metaData.y >> 20) & 0x3FF;

        maxRelativeIdxPlusOne.x = metaData.z & 0x3FF;
        maxRelativeIdxPlusOne.y = (metaData.z >> 10) & 0x3FF;
        maxRelativeIdxPlusOne.z = (metaData.z >> 20) & 0x3FF;
        cellIsLoaded = true;
    }
    else
    {
        chunkIndex = - 1;
        stepSize = - 1;
        minRelativeIdx = - 1;
        maxRelativeIdxPlusOne = - 1;
    }

    return cellIsLoaded;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
float ProbeDistance(uint subdiv)
{
return pow(3, subdiv) * _PoolDim_MinBrickSize.w / 3.0f;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
half ProbeDistanceHalf(uint subdiv)
{
return pow(half(3), half(subdiv)) * half(_PoolDim_MinBrickSize.w)/ 3.0;
}
//universal/ShaderLibrary/Shadows.hlsl
float SampleShadowmapFilteredLowQuality(Texture2D ShadowMap, SamplerComparisonState sampler_ShadowMap, float4 shadowCoord, ShadowSamplingData samplingData)
{
    float4 attenuation4;
    attenuation4.x = float(ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(shadowCoord.xyz + float3(samplingData.shadowOffset0.xy, 0)).xy,(shadowCoord.xyz + float3(samplingData.shadowOffset0.xy, 0)).z));
    attenuation4.y = float(ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(shadowCoord.xyz + float3(samplingData.shadowOffset0.zw, 0)).xy,(shadowCoord.xyz + float3(samplingData.shadowOffset0.zw, 0)).z));
    attenuation4.z = float(ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(shadowCoord.xyz + float3(samplingData.shadowOffset1.xy, 0)).xy,(shadowCoord.xyz + float3(samplingData.shadowOffset1.xy, 0)).z));
    attenuation4.w = float(ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(shadowCoord.xyz + float3(samplingData.shadowOffset1.zw, 0)).xy,(shadowCoord.xyz + float3(samplingData.shadowOffset1.zw, 0)).z));
    return dot(attenuation4, float(0.25));
}
//universal/ShaderLibrary/Shadows.hlsl
float SampleShadowmapFilteredMediumQuality(Texture2D ShadowMap, SamplerComparisonState sampler_ShadowMap, float4 shadowCoord, ShadowSamplingData samplingData)
{
    float fetchesWeights[9];
    float2 fetchesUV[9];
    SampleShadow_ComputeSamples_Tent_5x5(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    return fetchesWeights[0] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[0].xy, shadowCoord.z)).xy,(float3(fetchesUV[0].xy, shadowCoord.z)).z)
    + fetchesWeights[1] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[1].xy, shadowCoord.z)).xy,(float3(fetchesUV[1].xy, shadowCoord.z)).z)
    + fetchesWeights[2] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[2].xy, shadowCoord.z)).xy,(float3(fetchesUV[2].xy, shadowCoord.z)).z)
    + fetchesWeights[3] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[3].xy, shadowCoord.z)).xy,(float3(fetchesUV[3].xy, shadowCoord.z)).z)
    + fetchesWeights[4] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[4].xy, shadowCoord.z)).xy,(float3(fetchesUV[4].xy, shadowCoord.z)).z)
    + fetchesWeights[5] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[5].xy, shadowCoord.z)).xy,(float3(fetchesUV[5].xy, shadowCoord.z)).z)
    + fetchesWeights[6] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[6].xy, shadowCoord.z)).xy,(float3(fetchesUV[6].xy, shadowCoord.z)).z)
    + fetchesWeights[7] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[7].xy, shadowCoord.z)).xy,(float3(fetchesUV[7].xy, shadowCoord.z)).z)
    + fetchesWeights[8] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[8].xy, shadowCoord.z)).xy,(float3(fetchesUV[8].xy, shadowCoord.z)).z);
}
//universal/ShaderLibrary/Shadows.hlsl
float SampleShadowmapFilteredHighQuality(Texture2D ShadowMap, SamplerComparisonState sampler_ShadowMap, float4 shadowCoord, ShadowSamplingData samplingData)
{
    float fetchesWeights[16];
    float2 fetchesUV[16];
    SampleShadow_ComputeSamples_Tent_7x7(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    return fetchesWeights[0] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[0].xy, shadowCoord.z)).xy,(float3(fetchesUV[0].xy, shadowCoord.z)).z)
    + fetchesWeights[1] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[1].xy, shadowCoord.z)).xy,(float3(fetchesUV[1].xy, shadowCoord.z)).z)
    + fetchesWeights[2] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[2].xy, shadowCoord.z)).xy,(float3(fetchesUV[2].xy, shadowCoord.z)).z)
    + fetchesWeights[3] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[3].xy, shadowCoord.z)).xy,(float3(fetchesUV[3].xy, shadowCoord.z)).z)
    + fetchesWeights[4] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[4].xy, shadowCoord.z)).xy,(float3(fetchesUV[4].xy, shadowCoord.z)).z)
    + fetchesWeights[5] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[5].xy, shadowCoord.z)).xy,(float3(fetchesUV[5].xy, shadowCoord.z)).z)
    + fetchesWeights[6] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[6].xy, shadowCoord.z)).xy,(float3(fetchesUV[6].xy, shadowCoord.z)).z)
    + fetchesWeights[7] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[7].xy, shadowCoord.z)).xy,(float3(fetchesUV[7].xy, shadowCoord.z)).z)
    + fetchesWeights[8] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[8].xy, shadowCoord.z)).xy,(float3(fetchesUV[8].xy, shadowCoord.z)).z)
    + fetchesWeights[9] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[9].xy, shadowCoord.z)).xy,(float3(fetchesUV[9].xy, shadowCoord.z)).z)
    + fetchesWeights[10] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[10].xy, shadowCoord.z)).xy,(float3(fetchesUV[10].xy, shadowCoord.z)).z)
    + fetchesWeights[11] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[11].xy, shadowCoord.z)).xy,(float3(fetchesUV[11].xy, shadowCoord.z)).z)
    + fetchesWeights[12] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[12].xy, shadowCoord.z)).xy,(float3(fetchesUV[12].xy, shadowCoord.z)).z)
    + fetchesWeights[13] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[13].xy, shadowCoord.z)).xy,(float3(fetchesUV[13].xy, shadowCoord.z)).z)
    + fetchesWeights[14] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[14].xy, shadowCoord.z)).xy,(float3(fetchesUV[14].xy, shadowCoord.z)).z)
    + fetchesWeights[15] * ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(float3(fetchesUV[15].xy, shadowCoord.z)).xy,(float3(fetchesUV[15].xy, shadowCoord.z)).z);
}
//Level:7========================================================================================================================================================================
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
uint GetIndexData(APVResources apvRes, float3 posWS)
{
    float3 entryPos = floor(posWS / _Offset_IndirectionEntryDim.w);
    float3 topLeftEntryWS = entryPos * _Offset_IndirectionEntryDim.w;

    bool isALoadedCell = all(entryPos >= _Weight_MinLoadedCellInEntries.yzw) && all(entryPos <= _MaxLoadedCellInEntries_FrameIndex.xyz);
    //#line 311
    int3 entryPosInt = (int3)(entryPos - _MinEntryPos_Noise.xyz);

    int flatIdx = dot(entryPosInt, int3(1,(int)_IndicesDim_IndexChunkSize.xyz.x,((int)_IndicesDim_IndexChunkSize.xyz.x * (int)_IndicesDim_IndexChunkSize.xyz.y)));

    int stepSize = 0;
    int3 minRelativeIdx, maxRelativeIdxPlusOne;
    int chunkIdx = - 1;
    bool isValidBrick = false;
    int locationInPhysicalBuffer = 0;
    //#line 322
   [branch] if(isALoadedCell)
    {
        if(LoadCellIndexMetaData(flatIdx, chunkIdx, stepSize, minRelativeIdx, maxRelativeIdxPlusOne))
        {
            float3 residualPosWS = posWS - topLeftEntryWS;
            int3 localBrickIndex = floor(residualPosWS / (_PoolDim_MinBrickSize.w * stepSize));
            //#line 330
            isValidBrick = all(localBrickIndex >= minRelativeIdx) && all(localBrickIndex < maxRelativeIdxPlusOne);

            int3 sizeOfValid = maxRelativeIdxPlusOne - minRelativeIdx;

            int3 localRelativeIndexLoc = (localBrickIndex - minRelativeIdx);
            int flattenedLocationInCell = dot(localRelativeIndexLoc, int3(sizeOfValid.y, 1, sizeOfValid.x * sizeOfValid.y));

            locationInPhysicalBuffer = chunkIdx * (int)_IndicesDim_IndexChunkSize.w + flattenedLocationInCell;
        }
    }

    uint result = 0xffffffff;
    //#line 344
   [branch] if(isValidBrick)
    {
        result = apvRes.index[locationInPhysicalBuffer];
    }

    return result;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
float3 GetSnappedProbePosition(float3 posWS, uint subdiv)
{
    float3 distBetweenProbes = ProbeDistance(subdiv);
    float3 dividedPos = posWS / distBetweenProbes;
    return (dividedPos - frac(dividedPos)) * distBetweenProbes;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
uint3 GetSampleOffset(uint i)
{
    return uint3(i, i >> 1, i >> 2) & 1;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
half GetValidityWeight(uint offset, uint validityMask)
{
    uint mask = 1U << offset;
    return (validityMask & mask)> 0 ? 1 : 0;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
half GetNormalWeightHalf(uint3 offset, float3 posWS, float3 sample0Pos, float3 normalWS, uint subdiv)
{

    half3 samplePos = (half3)(sample0Pos - posWS) + (half3)offset * ProbeDistanceHalf(subdiv);
    half3 vecToProbe = normalize(samplePos);
    half weight = saturate(dot(vecToProbe,(half3)normalWS) -(half)_LeakReduction_SkyOcclusion.y);
    return weight;
}
//universal/ShaderLibrary/Shadows.hlsl
float SampleShadowmapFiltered(Texture2D ShadowMap, SamplerComparisonState sampler_ShadowMap, float4 shadowCoord, ShadowSamplingData samplingData)
{
    float attenuation = float(1.0);

    if(samplingData.softShadowQuality == half(1.0))
    {
        attenuation = SampleShadowmapFilteredLowQuality(ShadowMap, sampler_ShadowMap, shadowCoord, samplingData);
    }
    else if(samplingData.softShadowQuality == half(2.0))
    {
        attenuation = SampleShadowmapFilteredMediumQuality(ShadowMap, sampler_ShadowMap, shadowCoord, samplingData);
    }
    else
    {
        attenuation = SampleShadowmapFilteredHighQuality(ShadowMap, sampler_ShadowMap, shadowCoord, samplingData);
    }

    return attenuation;
}
//core/ShaderLibrary/CommonMaterial.hlsl
float LerpWhiteTo(float b, float t)
{
    float oneMinusT = 1.0 - t;
    return oneMinusT + b * t;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float4x4 GetWorldToViewMatrix()
{
    return unity_MatrixV;
}
//Level:6========================================================================================================================================================================
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
bool TryToGetPoolUVWAndSubdiv(APVResources apvRes, float3 posWSForSample, out float3 uvw, out uint subdiv)
{
    uint packed_pool_idx = GetIndexData(apvRes, posWSForSample.xyz);
    subdiv = (packed_pool_idx >> 28) & 15;
    float cellSize = pow(3.0, subdiv);

    float flattened_pool_idx = packed_pool_idx &((1 << 28) - 1);
    float3 pool_idx;
    pool_idx.z = floor(flattened_pool_idx * _RcpPoolDim_XY.w);
    flattened_pool_idx -= (pool_idx.z * (_PoolDim_MinBrickSize.xyz.x * _PoolDim_MinBrickSize.xyz.y));
    pool_idx.y = floor(flattened_pool_idx * _RcpPoolDim_XY.xyz.x);
    pool_idx.x = floor(flattened_pool_idx -(pool_idx.y * _PoolDim_MinBrickSize.xyz.x));
    float3 posRS = posWSForSample.xyz / _PoolDim_MinBrickSize.w;
    float3 offset = frac(posRS / (float)cellSize);
    uvw = (pool_idx + 0.5 + (3.0 * offset)) * _RcpPoolDim_XY.xyz;
    return packed_pool_idx != 0xffffffffu;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
void WarpUVWLeakReduction(APVResources apvRes, float3 posWS, float3 normalWS, uint subdiv, float3 biasedPosWS, inout float3 uvw, out float3 normalizedOffset, out float validityWeights[8])
{
    float3 texCoordFloat = uvw * _PoolDim_MinBrickSize.xyz - 0.5f;
    int3 texCoordInt = texCoordFloat;
    half3 texFrac = half3(frac(texCoordFloat));
    half3 oneMinTexFrac = 1.0 - texFrac;
    uint validityMask = apvRes.Validity.Load(int4(texCoordInt, 0)).x * 255.0;

    half4 weights[2];
    half totalW = 0.0;
    uint i = 0;
    float3 positionCentralProbe = GetSnappedProbePosition(biasedPosWS, subdiv);

   [unroll]
    for(i = 0; i < 8; ++ i)
    {
        uint3 offset = GetSampleOffset(i);
        half trilinearW =
       ((offset.x == 1)? texFrac.x : oneMinTexFrac.x) *
       ((offset.y == 1)? texFrac.y : oneMinTexFrac.y) *
       ((offset.z == 1)? texFrac.z : oneMinTexFrac.z);

        half validityWeight = GetValidityWeight(i, validityMask);
        validityWeights[i] = validityWeight;

        half geoW = GetNormalWeightHalf(offset, posWS, positionCentralProbe, normalWS, subdiv);

        half weight = saturate(trilinearW * (geoW * validityWeight));

        weights[i / 4][i % 4] = weight;
        totalW += weight;
    }

    half rcpTotalW = rcp(max(0.0001, totalW));
    weights[0] *= rcpTotalW;
    weights[1] *= rcpTotalW;

    half3 fracOffset = - texFrac;

   [unroll]
    for(i = 0; i < 8; ++ i)
    {
        uint3 offset = GetSampleOffset(i);
        fracOffset += (half3)offset * weights[i / 4][i % 4];
    }

    normalizedOffset = (float3)(fracOffset + texFrac);

    uvw = uvw + (float3)fracOffset * _RcpPoolDim_XY.xyz;
}
//core/ShaderLibrary/SphericalHarmonics.hlsl
//SHEvalLinearL0L1(..)は頂点単位、SHEvalLinearL2(..)は画素単位に処理を分けることもある?
    //最終的に、L0L1❰｡x1❰shA❱｡❱ + L2❰｡x2❰shB❱ + x3❰shC❱｡❱を計算している (L⟪0¦1¦2⟫とは何?)
float3 SHEvalLinearL0L1(float3 N, float4 shAr, float4 shAg, float4 shAb)
{
    float4 vA = float4(N, 1.0);

    float3 x1;

    //恐らく、shA⟪r¦g¦b⟫とvA==normalとの内積類似度を取り、その色が強い方向にnormalが向いていたら、その分だけその色が結果となる
    x1.r = dot(shAr, vA);
    x1.g = dot(shAg, vA);
    x1.b = dot(shAb, vA);

    return x1;
}
//core/ShaderLibrary/SphericalHarmonics.hlsl
float3 SHEvalLinearL2(float3 N, float4 shBr, float4 shBg, float4 shBb, float4 shC)
{
    float3 x2;

    //恐らく、考え方はSHEvalLinearL0L1(..)と同じだが、v⟪B¦C⟫の作られ方が意味不明
    float4 vB = N.xyzz * N.yzzx;
    x2.r = dot(shBr, vB);
    x2.g = dot(shBg, vB);
    x2.b = dot(shBb, vB);
    float vC = N.x * N.x - N.y * N.y;
    float3 x3 = shC.rgb * vC;

    return x2 + x3;
}
//universal/ShaderLibrary/Shadows.hlsl
ShadowSamplingData GetMainLightShadowSamplingData()
{
    ShadowSamplingData shadowSamplingData;
    shadowSamplingData.shadowOffset0 = half4(_MainLightShadowOffset0);
    shadowSamplingData.shadowOffset1 = half4(_MainLightShadowOffset1);
    shadowSamplingData.shadowmapSize = _MainLightShadowmapSize;
    shadowSamplingData.softShadowQuality = half(_MainLightShadowParams.y);

    return shadowSamplingData;
}
//universal/ShaderLibrary/Shadows.hlsl
half4 GetMainLightShadowParams()
{
    return half4(_MainLightShadowParams);
}
//universal/ShaderLibrary/Shadows.hlsl
float SampleShadowmap(Texture2D ShadowMap, SamplerComparisonState sampler_ShadowMap, float4 shadowCoord, ShadowSamplingData samplingData, half4 shadowParams, bool isPerspectiveProjection = true) //追加もメインも同じ関数を使っている
{
    if(isPerspectiveProjection)
        shadowCoord.xyz /= shadowCoord.w;

    float attenuation;
    float shadowStrength = shadowParams.x;
    if(shadowParams.y > half(0.0))
    {
        attenuation = SampleShadowmapFiltered(ShadowMap, sampler_ShadowMap, shadowCoord, samplingData);
    }
    else
    {
        attenuation = float(ShadowMap.SampleCmpLevelZero(sampler_ShadowMap,(shadowCoord.xyz).xy,(shadowCoord.xyz).z));
    }
    attenuation = LerpWhiteTo(attenuation, shadowStrength);
    return shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0 ? 1.0 : attenuation;
}
//core/ShaderLibrary/ImageBasedLighting.hlsl
float PerceptualRoughnessToMipmapLevel(float perceptualRoughness, uint maxMipLevel)
{
    perceptualRoughness = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness);

    return perceptualRoughness * maxMipLevel;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
float3 GetViewForwardDir()
{
    float4x4 viewMat = GetWorldToViewMatrix();
    return - viewMat[2].xyz;    //●マイナス(-)が付いている。 unity_MatrixV は Z が逆?
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
float3 GetCameraPositionWS()
{
    return _WorldSpaceCameraPos;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
bool IsPerspectiveProjection()
{
    return (unity_OrthoParams.w == 0);
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
uint Select4(uint4 v, uint i)
{
    uint mask0 = uint(int(i << 31)>> 31);
    uint mask1 = uint(int(i << 30)>> 31);
    return
       (((v.w & mask0)|(v.z & ~ mask0)) & mask1)|
       (((v.y & mask0)|(v.x & ~ mask0)) & ~ mask1);
}
//universal/ShaderLibrary/Shadows.hlsl
ShadowSamplingData GetAdditionalLightShadowSamplingData(int index)
{
    ShadowSamplingData shadowSamplingData = (ShadowSamplingData)0;
    shadowSamplingData.shadowOffset0 = _AdditionalShadowOffset0;
    shadowSamplingData.shadowOffset1 = _AdditionalShadowOffset1;
    shadowSamplingData.shadowmapSize = _AdditionalShadowmapSize;
    shadowSamplingData.softShadowQuality = _AdditionalShadowParams[index].y;
    return shadowSamplingData;
}
//universal/ShaderLibrary/Shadows.hlsl
half4 GetAdditionalLightShadowParams(int lightIndex)
{
    return _AdditionalShadowParams[lightIndex];
}
//core/ShaderLibrary/Common.hlsl
float CubeMapFaceID(float3 dir)
{
    float faceID;

    if(abs(dir.z)>= abs(dir.x) && abs(dir.z)>= abs(dir.y))
    {
        faceID = (dir.z < 0.0)? 5 : 4;
    }
    else if(abs(dir.y)>= abs(dir.x))
    {
        faceID = (dir.y < 0.0)? 3 : 2;
    }
    else
    {
        faceID = (dir.x < 0.0)? 1 : 0;
    }

    return faceID;
}
//core/ShaderLibrary/Common.hlsl
 float4 PositivePow(float4 base, float4 power){ return pow(abs(base), power); }
//Level:5========================================================================================================================================================================
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
bool TryToGetPoolUVWAndSubdiv(APVResources apvRes, float3 posWS, float3 normalWS, float3 viewDirWS, out float3 uvw, out uint subdiv, out float3 biasedPosWS)
{
    biasedPosWS = (posWS + normalWS * _Biases_NormalizationClamp.x) + viewDirWS * _Biases_NormalizationClamp.y;
    return TryToGetPoolUVWAndSubdiv(apvRes, biasedPosWS, uvw, subdiv);
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
void WarpUVWLeakReduction(APVResources apvRes, float3 posWS, float3 normalWS, uint subdiv, float3 biasedPosWS, inout float3 uvw)
{
    float3 normalizedOffset;
    float validityWeights[8];
    WarpUVWLeakReduction(apvRes, posWS, normalWS, subdiv, biasedPosWS, uvw, normalizedOffset, validityWeights);
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
APVSample SampleAPV(APVResources apvRes, float3 uvw)
{
    APVSample apvSample;
    half4 L0_L1Rx = half4(apvRes.L0_L1Rx.SampleLevel(s_linear_clamp_sampler, uvw, 0).rgba);
    half4 L1G_L1Ry = half4(apvRes.L1G_L1Ry.SampleLevel(s_linear_clamp_sampler, uvw, 0).rgba);
    half4 L1B_L1Rz = half4(apvRes.L1B_L1Rz.SampleLevel(s_linear_clamp_sampler, uvw, 0).rgba);

    apvSample.L0 = L0_L1Rx.xyz;
    apvSample.L1_R = half3(L0_L1Rx.w, L1G_L1Ry.w, L1B_L1Rz.w);
    apvSample.L1_G = L1G_L1Ry.xyz;
    apvSample.L1_B = L1B_L1Rz.xyz;
    if(_LeakReduction_SkyOcclusion.z > 0)
        apvSample.skyOcclusionL0L1 = apvRes.SkyOcclusionL0L1.SampleLevel(s_linear_clamp_sampler, uvw, 0).rgba;
    else
        apvSample.skyOcclusionL0L1 = float4(0, 0, 0, 0);

    if(_LeakReduction_SkyOcclusion.w > 0)
    {

        float3 texCoordFloat = uvw * _PoolDim_MinBrickSize.xyz - 0.5f;
        int3 texCoordInt = texCoordFloat;
        uint index = apvRes.SkyShadingDirectionIndices.Load(int4(texCoordInt, 0)).x * 255.0;

    if(index == 255)
        apvSample.skyShadingDirection = float3(0, 0, 0);
    else
        apvSample.skyShadingDirection = apvRes.SkyPrecomputedDirections[index].rgb;
    }
    else
        apvSample.skyShadingDirection = float3(0, 0, 0);

    apvSample.status = 0;

    return apvSample;
}
//core/ShaderLibrary/SphericalHarmonics.hlsl
float3 SHEvalLinearL1(float3 N, float3 shAr, float3 shAg, float3 shAb)
{
    float3 x1;
    x1.r = dot(shAr, N);
    x1.g = dot(shAg, N);
    x1.b = dot(shAb, N);

    return x1;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
float EvalSHSkyOcclusion(float3 dir, APVSample apvSample)
{
    float4 temp = float4(0.28209479177387814347f, 0.48860251190291992159f * dir.x, 0.48860251190291992159f * dir.y, 0.48860251190291992159f * dir.z);
    return _LeakReduction_SkyOcclusion.z * dot(temp, apvSample.skyOcclusionL0L1);
}
//core/ShaderLibrary/AmbientProbe.hlsl
float3 EvaluateAmbientProbe(float3 normalWS)
{
    float3 res = SHEvalLinearL0L1(normalWS, unity_SHAr, unity_SHAg, unity_SHAb);
    res += SHEvalLinearL2(normalWS, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);

    return res;
}
//core/ShaderLibrary/CommonMaterial.hlsl
float PerceptualSmoothnessToPerceptualRoughness(float perceptualSmoothness)
{
    return (1.0 - perceptualSmoothness);
}
//core/ShaderLibrary/CommonMaterial.hlsl
float PerceptualRoughnessToRoughness(float perceptualRoughness)
{
    return perceptualRoughness * perceptualRoughness;
}
//universal/ShaderLibrary/AmbientOcclusion.hlsl
half SampleAmbientOcclusion(float2 normalizedScreenSpaceUV)
{
    float2 uv = normalizedScreenSpaceUV;
    return half(_ScreenSpaceOcclusionTexture.SampleBias(sampler_LinearClamp, uv, _GlobalMipBias.x).x);
}
//universal/ShaderLibrary/Shadows.hlsl
half MainLightRealtimeShadow(float4 shadowCoord)
{
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();
    return SampleShadowmap(_MainLightShadowmapTexture, sampler_LinearClampCompare, shadowCoord, shadowSamplingData, shadowParams, false);
}
//universal/ShaderLibrary/Shadows.hlsl
half GetMainLightShadowFade(float3 positionWS)
{
    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

    float fade = saturate(distanceCamToPixel2 * float(_MainLightShadowParams.z) + float(_MainLightShadowParams.w));
    return half(fade);
}
//universal/ShaderLibrary/Shadows.hlsl
half MixRealtimeAndBakedShadows(half realtimeShadow, half bakedShadow, half shadowFade) //追加もメインも同じ関数を使っている
{
    return lerp(realtimeShadow, bakedShadow, shadowFade);
}
//core/ShaderLibrary/ImageBasedLighting.hlsl
float PerceptualRoughnessToMipmapLevel(float perceptualRoughness)
{
    return PerceptualRoughnessToMipmapLevel(perceptualRoughness, 6);
}
//universal/ShaderLibrary/Clustering.hlsl 
ClusterIterator ClusterInit(float2 normalizedScreenSpaceUV, float3 positionWS, int headerIndex) //全く理解できない
{
    ClusterIterator state = (ClusterIterator)0;
    uint2 tileId = uint2(normalizedScreenSpaceUV * ((float2)_FPParams1.xy)); //.xy単位で割る
    state.tileOffset = tileId.y * ((uint)_FPParams1.z) + tileId.x; //.zは多分1/x
    state.tileOffset *= ((uint)_FPParams1.w);   //.tileOffsetは多分、SS空間上の一次元位置IDをスケール(w)したもの

    float viewZ = dot(GetViewForwardDir(), positionWS - GetCameraPositionWS());
    uint zBinBaseIndex = (uint)((IsPerspectiveProjection()? log2(viewZ): viewZ) * (_FPParams0.x) + (_FPParams0.y));
    zBinBaseIndex = zBinBaseIndex * (2 + ((uint)_FPParams1.w));
    zBinBaseIndex = min(zBinBaseIndex, 4 * 1024 -(2 + ((uint)_FPParams1.w)));

    uint zBinHeaderIndex = zBinBaseIndex + headerIndex;
    state.zBinOffset = zBinBaseIndex + 2;
    uint header = Select4(asuint(urp_ZBins[zBinHeaderIndex / 4]), zBinHeaderIndex % 4);
    state.entityIndexNextMax = header;
    return state;
}
//universal/ShaderLibrary/Clustering.hlsl
bool ClusterNext(inout ClusterIterator it, out uint entityIndex) //全く理解できない
{
    uint maxIndex = it.entityIndexNextMax >> 16;
   [loop] while(it.tileMask == 0 &&(it.entityIndexNextMax & 0xFFFF)<= maxIndex)
    {
        uint wordIndex = ((it.entityIndexNextMax & 0xFFFF)>> 5);
        uint tileIndex = it.tileOffset + wordIndex;
        uint zBinIndex = it.zBinOffset + wordIndex;
        it.tileMask =
            Select4(asuint(urp_Tiles[tileIndex / 4]), tileIndex % 4) &    //asuint: >戻り値は、符号なし整数として解釈される入力。
            Select4(asuint(urp_ZBins[zBinIndex / 4]), zBinIndex % 4) &
           (0xFFFFFFFFu <<(it.entityIndexNextMax & 0x1F)) &(0xFFFFFFFFu >>(31 - min(31, maxIndex - wordIndex * 32)));
        it.entityIndexNextMax = (it.entityIndexNextMax + 32) & ~ 31;
    }

    bool hasNext = it.tileMask != 0;
    uint bitIndex = firstbitlow(it.tileMask); //firstbitlow: >戻り値は、最初のセット ビットの位置。
    it.tileMask ^= (1 << bitIndex);
    entityIndex = (((it.entityIndexNextMax - 32) &(0xFFFF & ~ 31))) + bitIndex;
    return hasNext;
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
float CalculateProbeWeight(float3 positionWS, float4 probeBoxMin, float4 probeBoxMax)
{
    float blendDistance = probeBoxMax.w;
    float3 weightDir = min(positionWS - probeBoxMin.xyz, probeBoxMax.xyz - positionWS)/ blendDistance;
    return saturate(min(weightDir.x, min(weightDir.y, weightDir.z)));
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
half3 BoxProjectedCubemapDirection(half3 reflectionWS, float3 positionWS, float4 cubemapPositionWS, float4 boxMin, float4 boxMax)
{
    if(cubemapPositionWS.w > 0.0f)
    {
        float3 boxMinMax = (reflectionWS > 0.0f)? boxMax.xyz : boxMin.xyz;
        half3 rbMinMax = half3(boxMinMax - positionWS)/ reflectionWS;

        half fa = half(min(min(rbMinMax.x, rbMinMax.y), rbMinMax.z));

        half3 worldPos = half3(positionWS - cubemapPositionWS.xyz);

        half3 result = worldPos + reflectionWS * fa;
        return result;
    }
    else
    {
        return reflectionWS;
    }
}
//core/ShaderLibrary/Packing.hlsl
float2 PackNormalOctQuadEncode(float3 n) //これはCubeマップの参照で組み込みでやっていた処理?
{
    n *= rcp(max(dot(abs(n), 1.0), 1e-6));
    float t = saturate(- n.z);
    return n.xy + float2(n.x >= 0.0 ? t : - t, n.y >= 0.0 ? t : - t);
}
//core/ShaderLibrary/EntityLighting.hlsl
float3 DecodeHDREnvironment(float4 encodedIrradiance, float4 decodeInstructions)
{
    float alpha = max(decodeInstructions.w * (encodedIrradiance.a - 1.0) + 1.0, 0.0);
    return (decodeInstructions.x * PositivePow(alpha, decodeInstructions.y)) * encodedIrradiance.rgb;
}
//core/ShaderLibrary/Common.hlsl
float3 SafeNormalize(float3 inVec)
{
    float dp3 = max(1.175494351e-38, dot(inVec, inVec));
    return inVec * rsqrt(dp3); //ゼロ除算回避?
}
//universal/ShaderLibrary/RealtimeLights.hlsl
float DistanceAttenuation(float distanceSqr, half2 distanceAttenuation)
{
    float lightAtten = rcp(distanceSqr);
    float2 distanceAttenuationFloat = float2(distanceAttenuation);
    half factor = half(distanceSqr * distanceAttenuationFloat.x);
    half smoothFactor = saturate(half(1.0) - factor * factor);
    smoothFactor = smoothFactor * smoothFactor;

    return lightAtten * smoothFactor;
}
//core/ShaderLibrary/CommonLighting.hlsl
float AngleAttenuation(float cosFwd, float lightAngleScale, float lightAngleOffset)
{
    return saturate(cosFwd * lightAngleScale + lightAngleOffset);
}
//universal/ShaderLibrary/Shadows.hlsl
half AdditionalLightRealtimeShadow(int lightIndex, float3 positionWS, half3 lightDirection)
{
    ShadowSamplingData shadowSamplingData = GetAdditionalLightShadowSamplingData(lightIndex);

    half4 shadowParams = GetAdditionalLightShadowParams(lightIndex);

    int shadowSliceIndex = shadowParams.w;
    if(shadowSliceIndex < 0)
    return 1.0;

    half isPointLight = shadowParams.z;

   [branch]
    if(isPointLight)
    {

    float cubemapFaceId = CubeMapFaceID(- lightDirection);
    shadowSliceIndex += cubemapFaceId;
    }
    float4 shadowCoord = mul(_AdditionalLightsWorldToShadow[shadowSliceIndex], float4(positionWS, 1.0));
    return SampleShadowmap(_AdditionalLightsShadowmapTexture, sampler_LinearClampCompare, shadowCoord, shadowSamplingData, shadowParams, true);
}
//universal/ShaderLibrary/Shadows.hlsl
half GetAdditionalLightShadowFade(float3 positionWS)
{
    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

    float fade = saturate(distanceCamToPixel2 * float(_AdditionalShadowFadeParams.x) + float(_AdditionalShadowFadeParams.y));
    return half(fade);
}
//Level:4========================================================================================================================================================================
//core/ShaderLibrary/SpaceTransforms.hlsl
float4x4 GetViewToWorldMatrix()
{
    return unity_MatrixInvV;
}
//core/ShaderLibrary/Random.hlsl
float InterleavedGradientNoise(float2 pixCoord, int frameCount)
{
    const float3 magic = float3(0.06711056f, 0.00583715f, 52.9829189f);
    float2 frameMagicScale = float2(2.083f, 4.867f);
    pixCoord += frameCount * frameMagicScale;
    return frac(magic.z * frac(dot(pixCoord, magic.xy)));
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl //############################################################
APVSample SampleAPV(APVResources apvRes, float3 posWS, float3 biasNormalWS, float3 viewDir)
{
    APVSample outSample;

    posWS -= _Offset_IndirectionEntryDim.xyz;

    float3 pool_uvw;
    uint subdiv;
    float3 biasedPosWS;
    if(TryToGetPoolUVWAndSubdiv(apvRes, posWS, biasNormalWS, viewDir, pool_uvw, subdiv, biasedPosWS)) //uvwの取得を試みる
    {
        if(_LeakReduction_SkyOcclusion.x != 0)
        {
            WarpUVWLeakReduction(apvRes, posWS, biasNormalWS, subdiv, biasedPosWS, pool_uvw);
        }
        outSample = SampleAPV(apvRes, pool_uvw); //Texture3D L⟪0¦1⟫からuvwでサンプリング

    }
    else
    {
        outSample = (APVSample)0;;
        outSample.status = - 1; //uvwの取得に失敗したらAPVを無効
    }

    return outSample;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
APVResources FillAPVResources()
{
    APVResources apvRes;
    apvRes.index = _APVResIndex;

    apvRes.L0_L1Rx = _APVResL0_L1Rx;

    apvRes.L1G_L1Ry = _APVResL1G_L1Ry;
    apvRes.L1B_L1Rz = _APVResL1B_L1Rz;

    apvRes.L2_0 = _APVResL2_0;
    apvRes.L2_1 = _APVResL2_1;
    apvRes.L2_2 = _APVResL2_2;
    apvRes.L2_3 = _APVResL2_3;

    apvRes.Validity = _APVResValidity;
    apvRes.SkyOcclusionL0L1 = _SkyOcclusionTexL0L1;
    apvRes.SkyShadingDirectionIndices = _SkyShadingDirectionIndicesTex;
    apvRes.SkyPrecomputedDirections = _SkyPrecomputedDirections;

    return apvRes;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
void EvaluateAPVL1(APVSample apvSample, float3 N, out float3 diffuseLighting)
{
    diffuseLighting = SHEvalLinearL1(N, apvSample.L1_R, apvSample.L1_G, apvSample.L1_B);
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
float3 EvaluateOccludedSky(APVSample apvSample, float3 N)
{
    float occValue = EvalSHSkyOcclusion(N, apvSample);
    float3 shadingNormal = N;

    if(_LeakReduction_SkyOcclusion.w > 0)
    {
        shadingNormal = apvSample.skyShadingDirection;
        float normSquared = dot(shadingNormal, shadingNormal);
        if(normSquared < 0.2f)
        shadingNormal = N;
        else
        {
            shadingNormal = shadingNormal * rsqrt(normSquared);
        }
    }
    return occValue * EvaluateAmbientProbe(shadingNormal);
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
void TransformScreenUV(inout float2 uv, float screenHeight) //意味が分からない
{
    uv.y = screenHeight - (uv.y * _ScaleBiasRt.x + _ScaleBiasRt.y * screenHeight);
}
//universal/ShaderLibrary/BRDF.hlsl
half OneMinusReflectivityMetallic(half metallic)
{
    half oneMinusDielectricSpec = half4(0.04, 0.04, 0.04, 1.0 - 0.04).a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec; //0.96 - metallic * 0.96 = 0.96～0
}
//universal/ShaderLibrary/BRDF.hlsl
inline void InitializeBRDFDataDirect(half3 albedo, half3 diffuse, half3 specular, half reflectivity, half oneMinusReflectivity, half smoothness, inout half alpha, out BRDFData outBRDFData)
{
    outBRDFData = (BRDFData)0;
    outBRDFData.albedo = albedo;
    outBRDFData.diffuse = diffuse;
    outBRDFData.specular = specular;
    outBRDFData.reflectivity = reflectivity;

    outBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    outBRDFData.roughness = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), 0.0078125);
    outBRDFData.roughness2 = max(outBRDFData.roughness * outBRDFData.roughness, 6.103515625e-5);
    outBRDFData.grazingTerm = saturate(smoothness + reflectivity);
    outBRDFData.normalizationTerm = outBRDFData.roughness * half(4.0) + half(2.0);
    outBRDFData.roughness2MinusOne = outBRDFData.roughness2 - half(1.0);
}
//universal/ShaderLibrary/AmbientOcclusion.hlsl
AmbientOcclusionFactor GetScreenSpaceAmbientOcclusion(float2 normalizedScreenSpaceUV)
{
    AmbientOcclusionFactor aoFactor;

    float ssao = saturate(SampleAmbientOcclusion(normalizedScreenSpaceUV) + (1.0 - _AmbientOcclusionParam.x));
    aoFactor.indirectAmbientOcclusion = ssao;
    aoFactor.directAmbientOcclusion = lerp(half(1.0), ssao, _AmbientOcclusionParam.w);
    return aoFactor;
}
//universal/ShaderLibrary/RealtimeLights.hlsl
Light GetMainLight()
{
    Light light;
    light.direction = half3(_MainLightPosition.xyz);
    light.distanceAttenuation = 1.0;
    light.shadowAttenuation = 1.0;
    light.color = _MainLightColor.rgb;

    light.layerMask = _MainLightLayerMask;

    return light;
}
//universal/ShaderLibrary/Shadows.hlsl
half MainLightShadow(float4 shadowCoord, float3 positionWS, half4 shadowMask, half4 occlusionProbeChannels)
{
    half realtimeShadow = MainLightRealtimeShadow(shadowCoord);
    half bakedShadow = half(1.0);
    half shadowFade = GetMainLightShadowFade(positionWS);
    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade);
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
half3 CalculateIrradianceFromReflectionProbes(half3 reflectVector, float3 positionWS, half perceptualRoughness, float2 normalizedScreenSpaceUV)
{
    half3 irradiance = half3(0.0h, 0.0h, 0.0h);
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);

    float totalWeight = 0.0f;
    uint probeIndex;
    ClusterIterator it = ClusterInit(normalizedScreenSpaceUV, positionWS, 1);
   [loop] while(ClusterNext(it, probeIndex) && totalWeight < 0.99f)
    {
        probeIndex -= ((uint)_FPParams0.z);

        float weight = CalculateProbeWeight(positionWS, urp_ReflProbes_BoxMin[probeIndex], urp_ReflProbes_BoxMax[probeIndex]);
        weight = min(weight, 1.0f - totalWeight);

        half3 sampleVector = reflectVector;
                            //↓ボックスプロジェクション
        sampleVector = BoxProjectedCubemapDirection(reflectVector, positionWS, urp_ReflProbes_ProbePosition[probeIndex], urp_ReflProbes_BoxMin[probeIndex], urp_ReflProbes_BoxMax[probeIndex]);
        //#line 245
        uint maxMip = (uint)abs(urp_ReflProbes_ProbePosition[probeIndex].w) - 1;
        half probeMip = min(mip, maxMip);   //↓Cubeマップをベクトルで参照するのをuvにしている?
        float2 uv = saturate(PackNormalOctQuadEncode(sampleVector) * 0.5 + 0.5);

        float mip0 = floor(probeMip);
        float mip1 = mip0 + 1;
        float mipBlend = probeMip - mip0;
        float4 scaleOffset0 = urp_ReflProbes_MipScaleOffset[probeIndex * 7 + (uint)mip0];
        float4 scaleOffset1 = urp_ReflProbes_MipScaleOffset[probeIndex * 7 + (uint)mip1];
                                        //↓Texture2D urp_ReflProbes_Atlasからサンプリングしている (TextureCubeではない)
        half3 irradiance0 = half4(urp_ReflProbes_Atlas.SampleLevel(sampler_LinearClamp, uv * scaleOffset0.xy + scaleOffset0.zw, 0.0)).rgb;
        half3 irradiance1 = half4(urp_ReflProbes_Atlas.SampleLevel(sampler_LinearClamp, uv * scaleOffset1.xy + scaleOffset1.zw, 0.0)).rgb;
        irradiance += weight * lerp(irradiance0, irradiance1, mipBlend);
        totalWeight += weight;
    }
    //#line 313
    if(totalWeight < 0.99f)
    {
        half4 encodedIrradiance = half4(_GlossyEnvironmentCubeMap.SampleLevel(sampler_GlossyEnvironmentCubeMap, reflectVector, mip));

        irradiance += (1.0f - totalWeight) * DecodeHDREnvironment(encodedIrradiance, _GlossyEnvironmentCubeMap_HDR);
    }

    return irradiance;
}
//universal/ShaderLibrary/BRDF.hlsl
half3 EnvironmentBRDFSpecular(BRDFData brdfData, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm)/*F0～F90 を fresnelTerm で補間*/);
}
//universal/ShaderLibrary/BRDF.hlsl 
half DirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));

    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;

    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);
    return specularTerm;
}
//universal/ShaderLibrary/RealtimeLights.hlsl
Light GetAdditionalPerObjectLight(int perObjectLightIndex, float3 positionWS)
{
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    uint lightLayerMask = asuint(_AdditionalLightsLayerMasks[perObjectLightIndex]);
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), 6.103515625e-5);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));

    float attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = 1.0;
    light.color = color;
    light.layerMask = lightLayerMask;

    return light;
}
//universal/ShaderLibrary/Shadows.hlsl
half AdditionalLightShadow(int lightIndex, float3 positionWS, half3 lightDirection, half4 shadowMask, half4 occlusionProbeChannels)
{
    half realtimeShadow = AdditionalLightRealtimeShadow(lightIndex, positionWS, lightDirection);
    half bakedShadow = half(1.0);
    half shadowFade = GetAdditionalLightShadowFade(positionWS);
    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade);
}
//Level:3========================================================================================================================================================================
//core/ShaderLibrary/SpaceTransforms.hlsl
float4x4 GetObjectToWorldMatrix()
{
    return unity_ObjectToWorld;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float4x4 GetWorldToHClipMatrix()
{
    return unity_MatrixVP;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float4x4 GetWorldToObjectMatrix()
{
    return unity_WorldToObject;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
float AlphaDiscard(float alpha, float cutoff, float offset = float(0.0))
{
    return alpha;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
float3 AddNoiseToSamplingPosition(float3 posWS, float2 positionSS, float3 direction)
{
    float3 right = mul((float3x3)GetViewToWorldMatrix(), float3(1.0, 0.0, 0.0));
    float3 top = mul((float3x3)GetViewToWorldMatrix(), float3(0.0, 1.0, 0.0));
    float noise01 = InterleavedGradientNoise(positionSS, _MaxLoadedCellInEntries_FrameIndex.w); //スクリーンスペースとフレーム番号でノイズをつくる
    float noise02 = frac(noise01 * 100.0);
    float noise03 = frac(noise01 * 1000.0);
    direction += top * (noise02 - 0.5) + right * (noise03 - 0.5);
    return _MinEntryPos_Noise.w > 0 ? posWS + noise01 * _MinEntryPos_Noise.w * direction : posWS;
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
APVSample SampleAPV(float3 posWS, float3 biasNormalWS, float3 viewDir)
{
    APVResources apvRes = FillAPVResources();
    return SampleAPV(apvRes, posWS, biasNormalWS, viewDir);
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
void EvaluateAdaptiveProbeVolume(APVSample apvSample, float3 normalWS, out float3 bakeDiffuseLighting)
{
    if(apvSample.status != - 1)
    {
        apvSample.Decode();
        EvaluateAPVL1(apvSample, normalWS, bakeDiffuseLighting); //L1を計算してbakeDiffuseLightingに出力
        bakeDiffuseLighting += apvSample.L0; //↑にL0を加算
        if(_LeakReduction_SkyOcclusion.z > 0)
            bakeDiffuseLighting += EvaluateOccludedSky(apvSample, normalWS); //↑に従来のライトプローブ(EvaluateAmbientProbe(normalWS))を重みを掛けて足してるみたい
        {                                                                           //↑訂正: bDL += EvalSHOccludedSky(N, apvSample) * EvaluateAmbientProbe(normalWS)❰従来❱
            bakeDiffuseLighting = bakeDiffuseLighting * _Weight_MinLoadedCellInEntries.x; //最後に重みを掛ける (ココはifの外)
        }
    }
    else //APVSampleが有効でない場合は従来のライトプローブ
    {
        bakeDiffuseLighting = EvaluateAmbientProbe(normalWS);
    }
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
float4 GetScaledScreenParams()
{
    return _ScaledScreenParams;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
void TransformNormalizedScreenUV(inout float2 uv)
{
    TransformScreenUV(uv, 1.0);
}
//universal/ShaderLibrary/BRDF.hlsl
inline void InitializeBRDFData(half3 albedo, half metallic, half3 specular, half smoothness, inout half alpha, out BRDFData outBRDFData)
{
    half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic); //0.96 - metallic * 0.96 = 0.96～0 (metallic: 0～1)
    half reflectivity = half(1.0) - oneMinusReflectivity; //= 0.04～1.0
    half3 brdfDiffuse = albedo * oneMinusReflectivity/*0.96～0*/;       //brdfDiffuse と brdfSpecular は足して1.0を超えない
    half3 brdfSpecular = lerp(half4(0.04, 0.04, 0.04, 1.0 - 0.04).rgb, albedo, metallic); //0.04～albedo
    InitializeBRDFDataDirect(albedo, brdfDiffuse, brdfSpecular, reflectivity, oneMinusReflectivity, smoothness, alpha, outBRDFData);
}
//universal/ShaderLibrary/AmbientOcclusion.hlsl
AmbientOcclusionFactor CreateAmbientOcclusionFactor(float2 normalizedScreenSpaceUV, half occlusion)
{
    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);

    aoFactor.indirectAmbientOcclusion = min(aoFactor.indirectAmbientOcclusion, occlusion);
    return aoFactor;
}
//universal/ShaderLibrary/RealtimeLights.hlsl
Light GetMainLight(float4 shadowCoord, float3 positionWS, half4 shadowMask)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightShadow(shadowCoord, positionWS, shadowMask, _MainLightOcclusionProbes);
    return light;
}
//universal/ShaderLibrary/Debug/DebuggingCommon.hlsl
bool IsLightingFeatureEnabled(uint bitMask)
{
    return true;
}
//core/ShaderLibrary/Common.hlsl
float Pow4(float x)
{
return (x * x) * (x * x);
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion, float2 normalizedScreenSpaceUV)
{
    half3 irradiance; //イラディアンス 放射照度
    irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness, normalizedScreenSpaceUV);
    return irradiance * occlusion;
}
//universal/ShaderLibrary/BRDF.hlsl
half3 EnvironmentBRDF(BRDFData brdfData, half3 indirectDiffuse, half3 indirectSpecular, half fresnelTerm)
{
    half3 c = indirectDiffuse/*bakedGI*/ * brdfData.diffuse;
    c += indirectSpecular/*GlossyEnvironmentReflection*/ * EnvironmentBRDFSpecular(brdfData, fresnelTerm);
    return c;
}
//universal/ShaderLibrary/Debug/DebuggingCommon.hlsl
bool IsOnlyAOLightingFeatureEnabled()
{
    return false;
}
//universal/ShaderLibrary/Lighting.hlsl
half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, float lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    half clearCoatMask, bool specularHighlightsOff)
{
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half3 radiance = lightColor * (lightAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;

   [branch] if(! specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
    }
    return brdf * radiance;
}
//universal/ShaderLibrary/RealtimeLights.hlsl
Light GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
{
    int lightIndex = i;
    Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
    half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[lightIndex];

    light.shadowAttenuation = AdditionalLightShadow(lightIndex, positionWS, light.direction, shadowMask, occlusionProbeChannels);
    return light;
}
//universal/ShaderLibrary/Lighting.hlsl
half3 CalculateLightingColor(LightingData lightingData, half3 albedo)
{
    half3 lightingColor = 0;

    if(IsOnlyAOLightingFeatureEnabled())
    {
        return lightingData.giColor;
    }

    if(IsLightingFeatureEnabled((1)))
    {
        lightingColor += lightingData.giColor;
    }

    if(IsLightingFeatureEnabled((2)))
    {
        lightingColor += lightingData.mainLightColor;
    }

    if(IsLightingFeatureEnabled((4)))
    {
        lightingColor += lightingData.additionalLightsColor;
    }

    if(IsLightingFeatureEnabled((8)))
    {
        lightingColor += lightingData.vertexLightingColor;
    }

    lightingColor *= albedo;

    if(IsLightingFeatureEnabled((16)))
    {
        lightingColor += lightingData.emissionColor;
    }

    return lightingColor;
}
//Level:2========================================================================================================================================================================
//core/ShaderLibrary/SpaceTransforms.hlsl
float3 TransformObjectToWorld(float3 positionOS)
{
    return mul(GetObjectToWorldMatrix(), float4(positionOS, 1.0)).xyz;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float3 TransformWorldToView(float3 positionWS)
{
    return mul(GetWorldToViewMatrix(), float4(positionWS, 1.0)).xyz;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float4 TransformWorldToHClip(float3 positionWS)
{
    return mul(GetWorldToHClipMatrix(), float4(positionWS, 1.0));
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float GetOddNegativeScale()
{
    return unity_WorldTransformParams.w >= 0.0 ? 1.0 : - 1.0;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float3 TransformObjectToWorldNormal(float3 normalOS, bool doNormalize = true)
{
    float3 normalWS = mul(normalOS,(float3x3)GetWorldToObjectMatrix());
    if(doNormalize)
        return SafeNormalize(normalWS);

    return normalWS;
}
//core/ShaderLibrary/SpaceTransforms.hlsl
float3 TransformObjectToWorldDir(float3 dirOS, bool doNormalize = true)
{
    float3 dirWS = mul((float3x3)GetObjectToWorldMatrix(), dirOS);        //●↑↑と行列と行列の掛ける順序が違うだけ(意味ある?)
    if(doNormalize)
        return SafeNormalize(dirWS);

    return dirWS;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
float3 GetCurrentViewPosition()
{
    return GetCameraPositionWS();
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
half3 SampleProbeVolumeVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir)
{
    return half3(0, 0, 0);
}
//universal/ShaderLibrary/SurfaceInput.hlsl
half4 SampleAlbedoAlpha(float2 uv, Texture2D albedoAlphaMap, SamplerState sampler_albedoAlphaMap)
{
    return half4(albedoAlphaMap.SampleBias(sampler_albedoAlphaMap, uv, _GlobalMipBias.x));
        //.SampleBias //●
            //bias: 適用するミップマップバイアス値。正の値はより低解像度のミップマップレベルを、負の値はより高解像度のミップマップレベルを選択するようにバイアスをかけます。
            //ミップマップはテクスチャの異なる解像度のレベルであり、レベル0が元の解像度のテクスチャ、レベル1がその半分の解像度（幅と高さがそれぞれ1/2）といった具合に、各レベルで解像度が次第に低くなっていきます。
            //バイアス値の影響 (-16.0 から 15.99(最大2^16=65536ピクセル四方))
                //正のバイアス: バイアス値が正の場合、選択されるミップマップレベルが通常よりも高い（低解像度）レベルになります。たとえば、バイアス値が 1.0 なら、通常選択されるレベルよりも1つ上のミップマップレベルが選択されます。
                //負のバイアス: バイアス値が負の場合、選択されるミップマップレベルが通常よりも低い（高解像度）レベルになります。たとえば、バイアス値が -1.0 なら、通常選択されるレベルよりも1つ下のミップマップレベルが選択されます。
}
//universal/ShaderLibrary/SurfaceInput.hlsl
half Alpha(half albedoAlpha, half4 color, half cutoff)
{
    half alpha = albedoAlpha * color.a;
    alpha = AlphaDiscard(alpha, cutoff);

    return alpha;
}
//universal/Shaders/LitInput.hlsl
half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;
    specGloss.rgb = _Metallic.rrr;
    specGloss.a = _Smoothness;
    return specGloss;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
half3 AlphaModulate(half3 albedo, half alpha)
{
    return albedo;
}
//universal/ShaderLibrary/SurfaceInput.hlsl
half3 SampleNormal(float2 uv, Texture2D bumpMap, SamplerState sampler_bumpMap, half scale = half(1.0))
{
    return half3(0.0h, 0.0h, 1.0h);
}
//universal/Shaders/LitInput.hlsl
half SampleOcclusion(float2 uv)
{
    return half(1.0);
}
//universal/ShaderLibrary/SurfaceInput.hlsl
half3 SampleEmission(float2 uv, half3 emissionColor, Texture2D emissionMap, SamplerState sampler_emissionMap)
{
    return 0;
}
//universal/ShaderLibrary/Shadows.hlsl
half ComputeCascadeIndex(float3 positionWS)
{
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);

    return half(4.0) - dot(weights, half4(4, 3, 2, 1));
}
//core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl
void EvaluateAdaptiveProbeVolume(in float3 posWS, in float3 normalWS, in float3 viewDir, in float2 positionSS, out float3 bakeDiffuseLighting)
{
    bakeDiffuseLighting = float3(0.0, 0.0, 0.0); //bakedGI。大体L0,L1を加算する

    posWS = AddNoiseToSamplingPosition(posWS, positionSS, viewDir); //posWS += (viewDir.⟪x¦y⟫ + ノイズ) * ノイズ をしている

    APVSample apvSample = SampleAPV(posWS, normalWS, viewDir); //最終的にTexture3D L⟪0¦1⟫をuvwでサンプリングするが超複雑そう
    EvaluateAdaptiveProbeVolume(apvSample, normalWS, bakeDiffuseLighting); //L1 + L0 + if{EvalSH"OccludedSky"(N, apvSample) * EvaluateAmbientProbe(normalWS)❰従来❱}
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl 
float2 GetNormalizedScreenSpaceUV(float2 positionCS) //●positionCS.xy * 0.5 + 0.5 じゃだめなの?
{                                                           //↓レンダーターゲットの解像度らしい
    float2 normalizedScreenSpaceUV = positionCS.xy * rcp(GetScaledScreenParams().xy);
    TransformNormalizedScreenUV(normalizedScreenSpaceUV);
    return normalizedScreenSpaceUV;
}
//universal/ShaderLibrary/BRDF.hlsl
inline void InitializeBRDFData(inout SurfaceData surfaceData, out BRDFData brdfData)
{
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
}
//universal/ShaderLibrary/BRDF.hlsl
BRDFData CreateClearCoatBRDFData(SurfaceData surfaceData, inout BRDFData brdfData)
{
    BRDFData brdfDataClearCoat = (BRDFData)0;
    return brdfDataClearCoat;
}
//universal/ShaderLibrary/RealtimeLights.hlsl
half4 CalculateShadowMask(InputData inputData)
{
    half4 shadowMask = unity_ProbesOcclusion;
    return shadowMask;
}
//universal/ShaderLibrary/AmbientOcclusion.hlsl
AmbientOcclusionFactor CreateAmbientOcclusionFactor(InputData inputData, SurfaceData surfaceData)
{
    return CreateAmbientOcclusionFactor(inputData.normalizedScreenSpaceUV, surfaceData.occlusion);
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
uint GetMeshRenderingLayer()
{
    return asuint(unity_RenderingLayer.x);
}
//universal/ShaderLibrary/RealtimeLights.hlsl
Light GetMainLight(InputData inputData, half4 shadowMask, AmbientOcclusionFactor aoFactor)
{
    Light light = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    if(IsLightingFeatureEnabled((32)))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
    return light;
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
void MixRealtimeAndBakedGI(inout Light light, half3 normalWS, inout half3 bakedGI)
{
}
//universal/ShaderLibrary/Lighting.hlsl
LightingData CreateLightingData(InputData inputData, SurfaceData surfaceData)
{
    LightingData lightingData;

    lightingData.giColor = inputData.bakedGI;
    lightingData.emissionColor = surfaceData.emission;
    lightingData.vertexLightingColor = 0;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;

    return lightingData;
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
half3 GlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
    half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS, float2 normalizedScreenSpaceUV)
{
    half3 reflectVector = reflect(- viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;    //↓ボックスプロジェクションとTexture2D urp_ReflProbes_Atlasを2回サンプリングし補間している (ほぼ理解してない)
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    if(IsOnlyAOLightingFeatureEnabled()) //アンビエントオクルージョンだけ表示する
    {
        color = half3(1, 1, 1);
    }
    return color * occlusion; //ssao
}
//core/ShaderLibrary/CommonLighting.hlsl
bool IsMatchingLightLayer(uint lightLayers, uint renderingLayers)
{
    return (lightLayers & renderingLayers)!= 0;
}
//universal/ShaderLibrary/Lighting.hlsl
half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff)
{
    return LightingPhysicallyBased(brdfData, brdfDataClearCoat, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff);
}
//universal/ShaderLibrary/RealtimeLights.hlsl
int GetAdditionalLightsCount()
{
    return 0;
}
//universal/ShaderLibrary/RealtimeLights.hlsl
Light GetAdditionalLight(uint i, InputData inputData, half4 shadowMask, AmbientOcclusionFactor aoFactor)
{
    Light light = GetAdditionalLight(i, inputData.positionWS, shadowMask);
    if(IsLightingFeatureEnabled((32)))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
    return light;
}
//universal/ShaderLibrary/Lighting.hlsl
half4 CalculateFinalColor(LightingData lightingData, half alpha)
{
    half3 finalColor = CalculateLightingColor(lightingData, 1);

    return half4(finalColor, alpha);
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
half3 MixFogColor(half3 fragColor, half3 fogColor, half fogFactor)
{
    return fragColor;
}
//Level:1========================================================================================================================================================================
//Varyings LitPassVertex(Attributes input)==========================================================
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
VertexPositionInputs GetVertexPositionInputs(float3 positionOS) //●
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
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS) //●
{
    VertexNormalInputs tbn;
    //[【Unity】接空間について](https://coposuke.hateblo.jp/entry/2020/12/21/144327#itangentw-%E3%81%A8-unity_WorldTransformParamsw)
        //(t,bはU,Vの座標系に一致している。接空間は右手系? tangentOS.w が通常は-1なのは左手→右手系の変換? 基本的にはUV(tb)平面にNormalMapを張っている)
        //(tは必ずUの向きに一致し、bは必ずtに直行しVに一致するが、Vがどっちに向いているか分からない為、tangentOS.wにV(b)の向きを保存している)
    float sign = float(tangentOS.w) * GetOddNegativeScale();    //tangentOS.w(通常-1)はUVの反転(+1)によるY軸=b=Vの反転、～Params.wはTransformの負のscaleによる反転
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = float3(TransformObjectToWorldDir(tangentOS.xyz));
    tbn.bitangentWS = float3(cross(tbn.normalWS, float3(tbn.tangentWS))) * sign;
    return tbn;
}
//universal/ShaderLibrary/Lighting.hlsl
half3 VertexLighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);
    return vertexLightColor;
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
half3 GetWorldSpaceNormalizeViewDir(float3 positionWS) //●
{
    if(IsPerspectiveProjection())
    {
        float3 V = GetCurrentViewPosition() - positionWS;
        return half3(normalize(V));
    }
    else
    {
        return half3(- GetViewForwardDir()); //平行投影は、カメラが点ではなく面だから? //透視投影でコレにしたら透視投影だけど2D(イラスト)っぽくなるかも?
    }
}
//universal/ShaderLibrary/GlobalIllumination.hlsl
half3 SampleProbeSHVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir)
{
    return SampleProbeVolumeVertex(absolutePositionWS, normalWS, viewDir);
}

//void LitPassFragment(Varyings input, out half4 outColor : SV_Target0)=============================== 
//universal/Shaders/LitInput.hlsl
inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, _BaseMap, sampler_BaseMap);
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, _BumpMap, sampler_BumpMap, _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, _EmissionMap, sampler_EmissionMap);
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
}
    //void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)===============
    //universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
    float3 NormalizeNormalPerPixel(float3 normalWS)
    {
        return normalize(normalWS);
    }
    //universal/ShaderLibrary/Shadows.hlsl
    float4 TransformWorldToShadowCoord(float3 positionWS)
    {
        half cascadeIndex = ComputeCascadeIndex(positionWS);
        float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
        
        return float4(shadowCoord.xyz, 0);
    }
    //universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
    float InitializeInputDataFog(float4 positionWS, float vertFogFactor)
    {
        float fogFactor = 0.0;
        return fogFactor;
    }
    //core/ShaderLibrary/SpaceTransforms.hlsl
    float3 GetAbsolutePositionWS(float3 positionRWS)
    {
        return positionRWS;
    }
    //universal/ShaderLibrary/GlobalIllumination.hlsl
    half3 SampleProbeVolumePixel(in half3 vertexValue, in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir, in float2 positionSS)
    {
        half3 bakedGI;
        if(_EnableProbeVolumes) //APV
        {
            EvaluateAdaptiveProbeVolume(absolutePositionWS, normalWS, viewDir, positionSS, bakedGI); //positionCS.xyだからpositionSS(スクリーンスペース)なのかな
        }
        else //従来のライトプローブ?
        {
            bakedGI = EvaluateAmbientProbe(normalWS);
        }
        return bakedGI;
    }
    //universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
    float2 GetNormalizedScreenSpaceUV(float4 positionCS)
    {
        return GetNormalizedScreenSpaceUV(positionCS.xy);
    }
//universal/ShaderLibrary/Lighting.hlsl
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData)
{
    bool specularHighlightsOff = false;

    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor); //シャドウの計算もある
        //uniformからLight情報取得 と light.shadowAttenuationを_MainLightShadowmapTextureをshadowCoordでサンプリング＆比較して取得
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    //環境拡散光:inputData.bakedGI と 環境鏡面光:urp_ReflProbes_AtlasとinputData で 合算してlightingData.giColorを算出
    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
    inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
    inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);

    if(IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers)) //return (lightLayers & renderingLayers)!= 0;
    {
        lightingData.mainLightColor = LightingPhysicallyBased
           (brdfData, brdfDataClearCoat, mainLight,
            inputData.normalWS, inputData.viewDirectionWS,
            surfaceData.clearCoatMask, specularHighlightsOff);
    }
    uint pixelLightCount = GetAdditionalLightsCount();
   [loop] for(uint lightIndex = 0; lightIndex < min(((uint)_FPParams0.w),(256)); lightIndex ++) //非クラスター (多分オブジェクト単位)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor); //lightIndex で cbuffer AdditionalLights から取得 //シャドウの計算もある
        if(IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased
               (brdfData, brdfDataClearCoat, light,
                inputData.normalWS, inputData.viewDirectionWS,
                surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    {
//==============================Forward+な部分。(リフレクションも同じ機構を利用している(urp_ReflProbes_ProbePosition[probeIndex]))
        uint lightIndex; //このインデックスで配列要素にアクセスしている
        ClusterIterator _urp_internal_clusterIterator = ClusterInit(inputData.normalizedScreenSpaceUV, inputData.positionWS, 0);
       [loop] while(ClusterNext(_urp_internal_clusterIterator, lightIndex)) //クラスター (なぜ↑↑とコレと2つある?) //●
        {
            lightIndex += ((uint)_FPParams0.w);
//===========================================
            Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor); //lightIndex で cbuffer AdditionalLights から取得
            if(IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            {
                lightingData.additionalLightsColor += LightingPhysicallyBased
                   (brdfData, brdfDataClearCoat, light,
                    inputData.normalWS, inputData.viewDirectionWS,
                    surfaceData.clearCoatMask, specularHighlightsOff);
            }
        } 
    }
    return CalculateFinalColor(lightingData, surfaceData.alpha);
}
//MixFog
half3 MixFog(half3 fragColor, half fogFactor)
{
    return MixFogColor(fragColor, half3(unity_FogColor.rgb), fogFactor);
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
bool IsSurfaceTypeTransparent(half surfaceType)
{
    return (surfaceType == kSurfaceTypeTransparent);
}
//universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
half OutputAlpha(half alpha, bool isTransparent)
{
    if(isTransparent)
    {
        return alpha;
    }
    else
    {
        return 1.0;
    }
}
//Level:0========================================================================================================================================================================
//"C:/UnityProject/6000_0_0b13/Library/PackageCache/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
struct Attributes //●Vertexへのin の セマンティクス として認識される?
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    float2 dynamicLightmapUV : TEXCOORD2;
};

struct Varyings //●Vertexからout、Fragmentへのin の セマンティクス として認識される?
{
    float2 uv : TEXCOORD0; //((input.texcoord.xy) * _BaseMap_ST.xy + _BaseMap_ST.zw)
    float3 positionWS : TEXCOORD1; //TransformObjectToWorld(positionOS)
    float3 normalWS : TEXCOORD2; //TransformObjectToWorldNormal(normalOS)
    half fogFactor : TEXCOORD5; //0
    half3 vertexSH : TEXCOORD8; //half3(0, 0, 0)
    float4 positionCS : SV_POSITION; //TransformWorldToHClip(input.positionWS)
};

//inputData.positionWS,normalWS,viewDirectionWS,shadowCoord,fogCoord,bakedGI,normalizedScreenSpaceUV,shadowMask
void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    inputData.normalWS = input.normalWS;
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
    inputData.bakedGI = SampleProbeVolumePixel(input.vertexSH, GetAbsolutePositionWS(inputData.positionWS), inputData.normalWS, inputData.viewDirectionWS, input.positionCS.xy);
    ////Forward+に使うパラメータ?普通のテクスチャ参照でも使ってた(_ScreenSpaceOcclusionTexture.SampleBias(..))
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = unity_ProbesOcclusion;;
}
Varyings LitPassVertex(Attributes input) //●戻り値はoutセマンティクス、引数は基本的にinセマンティクスだがoutをつけるとoutセマンティクス
{
    Varyings output = (Varyings)0; //attIn, varOut, varIn

    //↓,↓↓使ってない処理された変数が多いが最適化で消える?
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    output.uv = ((input.texcoord.xy) * _BaseMap_ST.xy + _BaseMap_ST.zw);
    output.normalWS = normalInput.normalWS;
    output.vertexSH.xyz = SampleProbeSHVertex(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS));
    output.fogFactor = fogFactor;
    output.positionWS = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

    return output;
}
void LitPassFragment(Varyings input, out half4 outColor : SV_Target0) //●
{
    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);
    InputData inputData;
    //inputData.positionWS,normalWS,viewDirectionWS,shadowCoord,fogCoord,bakedGI,normalizedScreenSpaceUV,shadowMask
    InitializeInputData(input, surfaceData.normalTS, inputData);
    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord); //フォグ使ってないので効果なし(color がそのまま返る)
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface)); //_Surface が Transparent だったら color.a がそのまま返る、不透明だったら1.0

    outColor = color;
}