Shader "Custom/InstancingShader"
{
    Properties
    {
        [MainTexture] _Smile ("エガオ", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Smile);
            SAMPLER(sampler_Smile);
            float4x4 _move;

            float _aspect;
            float3 ApplyAspect(float3 pos, float aspect)
            {
                if (aspect > 1.0)
                {
                    pos.x /= aspect;
                }
                else
                {
                    pos.y *= aspect;
                }
                return pos;
            }

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                uint instanceID    : INSTANCEID_SEMANTIC; // SV_INSTANCEID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                float3 position = input.positionOS;
                position *= 0.25;
                position = ApplyAspect(position, _aspect);
                position = mul(_move, float4(position, 1.0)).xyz;
                position.x += 0.125 * input.instanceID;
                output.positionCS = float4(position, 1.0);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_Smile, sampler_Smile, input.uv);
                return col;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
