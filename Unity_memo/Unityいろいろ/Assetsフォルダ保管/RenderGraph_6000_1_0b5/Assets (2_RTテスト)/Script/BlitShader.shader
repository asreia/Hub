Shader "Custom/BlitShader"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _CameraTarget
            #pragma multi_compile _ _RTHandle

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            struct FragOut
            {
                half4 col0 : SV_TARGET/*0*/;
                #if !defined(_RTHandle)
                    half4 col1 : SV_TARGET1;
                    half4 col2 : SV_TARGET2;
                    float depth : SV_DEPTH;
                #endif
            };

            Texture2D _SourceTex;
            SamplerState sampler_SourceTex;
            float4 _SourceTex_TexelSize; // x=1/width, y=1/height, z=width, w=height

            Varyings vert (uint vertexID : SV_VERTEXID)
            {
                Varyings output;
                //vertexID == 0
                    //uv = (0, 0)
                    //pos = (-1, -1, 0, 1)
                //vertexID == 1
                    //uv = (0, 2)
                    //pos = (-1,  3, 0, 1)
                //vertexID == 2
                    //uv = (2, 0)
                    //pos = ( 3, -1, 0, 1)
                // フルスクリーントライアングル
                output.uv = float2(vertexID & 2, (vertexID << 1) & 2);
                output.pos = float4(output.uv * 2.0 - 1.0, 0, 1);
                output.pos.y *= _ProjectionParams.x; //RenderTexture描画時、`-1`となり`Y`が反転する (`Front CCW:true`で描画される)
                return output;
            }

            half4 ParamTest(half4 col, Varyings input)
            {
                #if !defined(_CameraTarget)
                    if (input.uv.x < 0.5)
                #else
                    if (input.uv.x >= 0.5)
                #endif
                    {
                        // if (_ProjectionParams.x == -1.0) //恐らく、`_ProjectionParams`を試しただけで、`～_TexelSize.y`のマイナスは確認してないと思われる
                        if (_SourceTex_TexelSize.y < 0.002)
                        {
                            col = half4(0, 0, 0.5, 0);
                        }
                        else// if (_ProjectionParams.x == 1.0)
                        {
                            col = half4(0.5, 0, 0, 0);
                        }
                    }
                return col;
            }

            FragOut frag (Varyings input)
            {
                FragOut output = (FragOut)0;
                #if !defined(_RTHandle)
                    // output.col = half4(0,0,0,0);
                    // output.col += ParamTest(output.col, input);
                    half4 col = _SourceTex.Sample(sampler_SourceTex, input.uv);
                    output.depth = (col.r + col.g + col.b) / 3.0;
                    output.col0 = output.col1 = output.col2 = col;
                    output.col0.r += 0.25;  
                    output.col1.g += 0.25;  
                    output.col2.b += 0.25;  
                    // if(all(output.col.rgb == float3(0.0, 0.0, 0.0)))
                    // {
                    //     discard;
                    // }
                #else
                    output.col0 = _SourceTex.Sample(sampler_SourceTex, input.uv);
                    // output.col0.rg = input.uv; //UVテスト
                    // output.col0 = input.pos; //SV_POSITIONテスト //R16G16B16A16_SFloatで(～.5, ～.5, 0, 1)になることを確認。
                #endif
                return output;
            }
            ENDHLSL
        }
    }

}