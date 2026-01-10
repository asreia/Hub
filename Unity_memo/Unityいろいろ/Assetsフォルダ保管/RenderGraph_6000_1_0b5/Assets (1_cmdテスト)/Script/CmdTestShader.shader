Shader "Custom/CmdTestShader"
{
    Properties
    {
        /*[MainTexture]*/ _MainTex ("ねこ化ベルーガ", 2D) = "white" {}
        //Properties{..}に含めなければGlobalPropertyとなる(cmd.SetGlobal～(..))
        //Properties{..}に含めるとMaterialPropertyとなる(mat.Set～(..),MaterialPropertyBlock)
    }
    SubShader
    {
        Pass
        {
            Name "CmdTest_CmdTest_CmdTest_CmdTest"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            // Tags{"LightMode" = "ForwardBase"}
            // Tags{"LightMode" = "ForwardAdd"}
            // Tags{"LightMode" = "Test"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #pragma multi_compile _ _VIEWPORT _SCISSOR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

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
                #if !defined(_VIEWPORT) && !defined(_SCISSOR)
                    position = ApplyAspect(position, _aspect);
                #endif
                position = TransformObjectToWorld(position);
                output.positionCS = float4(position, 1.0);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                #if defined(_VIEWPORT)
                    float4 col = float4(0.0, 1.0, 0.0, 1.0); // 緑色
                #elif defined(_SCISSOR)
                    float4 col = float4(1.0, 0.0, 0.0, 1.0); // 赤色
                #else
                    float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                #endif
                return col;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
