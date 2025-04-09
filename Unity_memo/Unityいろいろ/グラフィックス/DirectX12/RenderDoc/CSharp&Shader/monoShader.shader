Shader "Test/MonoShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="RenderGraphTestPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            float4 vert (float3 pos : POSITION) : SV_POSITION
            {
                return float4(pos, 1.0);
            }

            half4 frag () : SV_Target
            {
                return half4(0.5, 0.5, 0.5, 1.0);
            }
            ENDHLSL
        }
    }
}
