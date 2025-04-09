Shader "Test/BlitShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="RenderGraphTestPipeline" }

        Pass
        {
            HLSLPROGRAM

            // #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            
            Texture2D p_intermediateRT;
            SamplerState sampler_p_intermediateRT;

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(float3 pos : POSITION, float2 uv : TEXCOORD0)
            {
                Varyings output = (Varyings)0;
                output.pos = float4(pos, 1.0);
                output.uv = uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = p_intermediateRT.Sample(sampler_p_intermediateRT, input.uv);
                return color;
            }
            ENDHLSL
        }
    }
}
