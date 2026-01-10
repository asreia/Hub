Shader "Custom/ShaderKeywordTestShader_Asset"
{
    Properties
    {
        // [Toggle(_SWITCH_0)] _switch_0 ("Switch 0", Float) = 0
        _face ("Face Atlas", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Name "Name_ShaderKeywordTestShader_Asset"

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            
            #pragma multi_compile _ _GLOBAL
            #pragma shader_feature _DEFAULT _ASSET_0 _ASSET_1 //2 * 2 * ⟪1¦2¦3⟫ バリアント を生成
            #pragma multi_compile _ _STRIP //Assets\Editor\ShaderKeywordTestShaderKeywordStripper.cs からストリップされる Keyword

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            int _spritCount = 4;
            float4 _atlasRanges[4];
            TEXTURE2D(_face);
            SAMPLER(sampler_face);

            struct Rect
            {
                float2 min;
                float2 max;
            };

            Rect VectorToRect(float4 vec)
            {
                Rect rect;
                rect.min = float2(vec.x, vec.y);
                rect.max = float2(vec.z, vec.w);
                return rect;
            }

            float Map(float2 destRange, float2 srcRange, float srcValue)
            {
                return (((destRange.y - destRange.x) / (srcRange.y - srcRange.x)) * (srcValue - srcRange.x)) + destRange.x;
            }

            float2 MapToRectUV(Rect rect, float2 uvRange, float2 uv)
            {
                return float2(Map(float2(rect.min.x, rect.max.x), uvRange, uv.x), Map(float2(rect.min.y, rect.max.y), float2(0.0, 1.0), uv.y));
            }

            float2 CalculateAtlasUV(float4 atlasRange, float2 uvRange, float2 uv)
            {
                return MapToRectUV(VectorToRect(atlasRange), uvRange, uv);
            }

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = float4(TransformObjectToWorld(input.positionOS), 1.0);
                output.uv = input.uv;
                return output;
                #define VERTEX
            }

            float4 frag(Varyings input) : SV_TARGET
            {
                float2 uv;
                uv = input.uv;
                //これでもGenerate Mipmap を ON にすると線が出る。//spritIndex と range 計算
                int spritIndex = clamp(int(input.uv.x * _spritCount), 0, _spritCount - 1);
                float spritRange = 1.0 / _spritCount;
                float2 range = float2(spritIndex * spritRange, (spritIndex + 1) * spritRange);
                if(spritIndex == 0)
                {
                    #if defined(_GLOBAL)
                        uv = CalculateAtlasUV(_atlasRanges[1], range, input.uv);
                    #else
                        uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                    #endif
                }
                else if(spritIndex == 1)
                {
                    #if defined(_DEFAULT)
                        uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                    #elif defined(_ASSET_0)
                        uv = CalculateAtlasUV(_atlasRanges[1], range, input.uv);
                    #elif defined(_ASSET_1)
                        uv = CalculateAtlasUV(_atlasRanges[2], range, input.uv);
                    #endif
                }
                else if(spritIndex == 2)
                {
                    #if defined(VERTEX) //これは必ずtrue
                        uv = CalculateAtlasUV(_atlasRanges[3], range, input.uv);
                    #else
                        uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                    #endif
                }
                else if(spritIndex == 3)
                {
                    #if defined(_STRIP)
                        uv = CalculateAtlasUV(_atlasRanges[1], range, input.uv);
                    #else
                        uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                    #endif
                }
                else
                {
                    uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                }
                float4 color = SAMPLE_TEXTURE2D(_face, sampler_face, uv);
                return color;
            }
            ENDHLSL
        }
    }
}
