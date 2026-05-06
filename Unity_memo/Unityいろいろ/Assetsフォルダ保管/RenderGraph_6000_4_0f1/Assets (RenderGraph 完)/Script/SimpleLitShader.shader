Shader "Custom/SimpleLit"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _SunColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };    

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD2;
                float2 uv : TEXCOORD0;
            };    

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalInputs.normalWS;
                output.uv = input.uv;
                
                return output;
            }
            
            // 暖かい太陽光の定義（正規化済み）
            #define SUN_DIRECTION half3(0.4472, 0.8944, -0.2683)

            half4 frag(Varyings input) : SV_Target
            {
                // テクスチャのサンプリング
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // 法線の正規化
                half3 normalWS = normalize(input.normalWS);
                
                // 暖かい太陽光のライティング計算
                half NdotL = saturate(dot(normalWS, SUN_DIRECTION));
                half3 diffuse = _SunColor.rgb * NdotL;
                
                // 暖かい環境光
                half3 ambient = half3(0.4, 0.5, 0.7) * 0.5;
                
                // シンプルなライティング合成
                half3 finalColor = (diffuse + ambient) * albedo.rgb;
                
                return half4(finalColor, albedo.a);
            }

            ENDHLSL
        }
    }
}
