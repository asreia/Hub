Shader "Custom/ShaderKeywordTestShader"
{
    SubShader
    {
        Pass
        {
            Name "Name_ShaderKeywordTestShader"

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            
            #pragma multi_compile _ _GLOBAL
            #pragma multi_compile_fog //FOG_EXP2 //Lighting/Environment/Fog と Project Settings/Graphics/Fog Modes
            #pragma shader_feature _SVC //Assets\Resources\_SVC.shadervariants を削除するとマッチ失敗する //_feature 1Keyword無し版(`_`) 省略
            #pragma multi_compile_local_vertex _ _VERTEX //_fragmentにすると効くようになる

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            int _spritCount;
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
            }

            float4 frag(Varyings input) : SV_TARGET
            {
                float2 uv;
                uv = input.uv;
                float spritRange = 1.0 / _spritCount;
                for(int i = 0; i < _spritCount; i++)
                {
                    float2 range = float2(i * spritRange, (i + 1) * spritRange);
                    //Generate Mipmap を Off にしたら線が消えた。
                        //(恐らく、Atlasの境界でUV座標が大幅に変わってddx(uv.x)が大きくなりMaxMipLvが参照された(多分、1SubMeshに1Atlasしか貼らない))
                    if (/*input.uv.x >= range.x &&*/ input.uv.x < range.y)
                    {
                        if(i == 0)
                        {
                            #if defined(_GLOBAL)
                                uv = CalculateAtlasUV(_atlasRanges[1], range, input.uv);
                            #else
                                uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                            #endif
                            break;
                        }
                        else if(i == 1)
                        {
                            #if defined(FOG_EXP2)
                                uv = CalculateAtlasUV(_atlasRanges[2], range, input.uv);
                            #else
                                uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                            #endif
                            break;
                        }
                        else if(i == 2)
                        {
                            #if defined(_SVC)
                                uv = CalculateAtlasUV(_atlasRanges[3], range, input.uv);
                            #else
                                uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                            #endif
                            break;
                        }
                        else if(i == 3)
                        {
                            #if defined(_VERTEX) //_vertexサフィックスなのでこれは常にfalse
                                uv = CalculateAtlasUV(_atlasRanges[1], range, input.uv);
                            #else
                                uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                            #endif
                            break;
                        }
                        else
                        {
                            uv = CalculateAtlasUV(_atlasRanges[0], range, input.uv);
                            break;
                        }
                    }
                }

                float4 color = SAMPLE_TEXTURE2D(_face, sampler_face, uv);
                return color; 
            }
            ENDHLSL
        }
    }
}
