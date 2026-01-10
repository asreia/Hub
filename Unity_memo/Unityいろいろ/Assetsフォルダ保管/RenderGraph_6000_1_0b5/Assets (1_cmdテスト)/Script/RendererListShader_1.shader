Shader "Custom/RendererListShader_1"
{
    SubShader
    {
        Tags{"SubShaderTagName" = "TagValue_1"}
        //＠❰"Queue" = "⟪Background『1000』¦Geometry『2000(デフォルト)』¦AlphaTest『2450』¦Transparent『3000』¦Overlay『4000』⟫＠❰+⟪～⟫❱"❱
        Tags{"Queue" = "AlphaTest+100"}
        Pass
        {
            Tags{"LightMode" = "RendererListShaderTag_1"}

            ZTest Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _ _OptimizeStateChanges_Key

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attribute
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NNNNNNNNN; //INPUT_LAYOUT じゃないから何でもいいのか？
                float3 positionWS : TEXCOORD1; // 追加
            };

            Varyings vert(Attribute input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz); // 追加
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 color;
                // 並行光源の方向ベクトル（例: 上から右斜め前方向）
                float3 lightDir = normalize(float3(0.3, 1.0, 0.5));
                // カメラ位置（ワールド座標）は組み込み定数で取得
                float3 viewPosWS = GetCameraPositionWS();
                float3 viewDir = normalize(viewPosWS - input.positionWS); // ピクセルからカメラへのベクトル
                float3 halfVec = normalize(viewDir + lightDir);
                float diffuse = 0.5 + 0.5 * dot(normalize(input.normalWS), lightDir);
                float specular = pow(/*saturate(*/dot(halfVec, input.normalWS)/*)*/, 4.0);
                #if !defined(_OptimizeStateChanges_Key)
                    float3 ambient = float3(0.25, 0.15, 0.05); // 柔らかいオレンジ色
                #else
                    float3 ambient = float3(0.6, 0.15, 0.05); // 強い朱色
                #endif
                float3 finalColor = diffuse + specular + ambient;
                return float4(finalColor, 0.5); // 半透明
            }
            ENDHLSL
        }
    }
}
