//cmd.DrawProcedural(Matrix4x4.identity, asset.AnisotropicTestMaterial, 0, MeshTopology.Triangles, 3) で描画
Shader "Custom/AnisotropicTestShader"
{
    Properties
    {
        [NoScaleOffset] _SourceTex ("SourceTex", 2D) = "white" {}
        [Header(Draw Position and Scale)]
        _texDrawPointX ("texDrawPoint X", Range(0, 1920)) = 0
        _texDrawPointY ("texDrawPoint Y", Range(0, 1080)) = 0
        _texDrawScaleX ("texDrawScale X", Range(0, 2.0)) = 1
        _texDrawScaleY ("texDrawScale Y", Range(0, 2.0)) = 1
        [Space(10)]
        [IntRange] _AnisoLevel ("Aniso Level", Range(1, 16)) = 16
        [KeywordEnum(anisotropic, sample, load)] _use ("Use Anisotropic Sample", Float) = 0 //｢PropertyName｣_｢Keyword｣(を大文字化)
        [HideInInspector][HDR] _Emission ("Emission", Color) = (1,1,1,1) //テスト
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _USE_ANISOTROPIC _USE_SAMPLE _USE_LOAD

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Script/AnisotropicSample.hlsl"

            Texture2D<float4> _SourceTex;
            SamplerState sampler_SourceTex;
            float4 _SourceTex_TexelSize; // x=1/width, y=1/height, z=width, w=height

            float _texDrawPointX; float _texDrawPointY;
            float _texDrawScaleX; float _texDrawScaleY;
            float _AnisoLevel;

            float4 vert (uint vertexID : SV_VERTEXID) : SV_POSITION
            {
                float4 pos;
                float2 uv = float2(vertexID & 2, (vertexID << 1) & 2);
                pos = float4(uv * 2.0 - 1.0, 0, 1);
                pos.y *= _ProjectionParams.x;
                return pos;
            }

            float Map(float2 destRange, float2 srcRange, float srcValue)
            {
                return (((destRange.y - destRange.x) / (srcRange.y - srcRange.x)) * (srcValue - srcRange.x)) + destRange.x;
            }

            half4 frag (float4 pos : SV_POSITION) : SV_Target
            {
                float2 _texDrawPoint = float2(_texDrawPointX, _texDrawPointY);
                pos.y = _ScreenParams.y - pos.y; //cmd.SetupCameraProperties(camera)で設定される
                half4 col = half4(0,0,0,0);
                float2 texDrawEndpoint = _SourceTex_TexelSize.zw * float2(_texDrawScaleX, _texDrawScaleY) + _texDrawPoint;
                if((_texDrawPoint.x <= pos.x && pos.x < texDrawEndpoint.x) && (_texDrawPoint.y <= pos.y && pos.y < texDrawEndpoint.y))
                {
                    float2 sampleUV = float2(Map(float2(0.0, 1.0), float2(_texDrawPoint.x, texDrawEndpoint.x), pos.x),
                                        Map(float2(0.0, 1.0), float2(_texDrawPoint.y, texDrawEndpoint.y), pos.y));
                    uint2 loadUV = uint2((uint)(sampleUV.x * _SourceTex_TexelSize.z),
                                        (uint)(sampleUV.y * _SourceTex_TexelSize.w));

                    #if defined(_USE_ANISOTROPIC)
                        col = AnisotropicSample(_SourceTex, sampleUV, _AnisoLevel, sampler_SourceTex);
                    #elif defined(_USE_SAMPLE)
                        col = _SourceTex.Sample(sampler_SourceTex, sampleUV);
                    #elif defined(_USE_LOAD)
                        col = _SourceTex.Load(int3(loadUV, 0));
                    #endif
                }
                else
                {
                    discard;
                }
                return col;
            }
            ENDHLSL
        }
    }

}