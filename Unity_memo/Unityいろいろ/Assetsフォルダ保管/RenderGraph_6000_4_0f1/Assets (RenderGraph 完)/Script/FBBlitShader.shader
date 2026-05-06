Shader "Custom/FBBlitShader"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _UAV

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #if !defined(_UAV) //めんどくさかったから論理反転した
                RWTexture2D<float4> _uav : register(u1);
            #else
                FRAMEBUFFER_INPUT_X_FLOAT(0);
            #endif

            float4 vert (uint vertexID : SV_VERTEXID) : SV_POSITION
            {
                float4 pos;
                float2 uv = float2(vertexID & 2, (vertexID << 1) & 2);
                pos = float4(uv * 2.0 - 1.0, 0, 1);
                pos.y *= _ProjectionParams.x;
                return pos;
            }

            half4 frag (float4 positionCS : SV_POSITION) : SV_TARGET
            {
                #if !defined(_UAV)
                    float4 color = _uav[uint2(positionCS.xy)];
                    if(positionCS.x % 20 < 1 || positionCS.y % 20 < 1)
                    {
                        _uav[uint2(positionCS.xy)] = float4(1, 0.5, 0.25, 1);
                    }
                    return color;
                #else
                    return LOAD_FRAMEBUFFER_INPUT_X(0, positionCS.xy);
                #endif
            }

            ENDHLSL
        }
    }
}