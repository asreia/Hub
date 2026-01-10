Shader "Custom/RendererListOverrideShader"
{
    Properties
    {
        _floatValue("Float Value", Float) = 0.3
    }
    SubShader
    {
        //＠❰"Queue" = "⟪Background『1000』¦Geometry『2000(デフォルト)』¦AlphaTest『2450』¦Transparent『3000』¦Overlay『4000』⟫＠❰+⟪～⟫❱"❱
        Tags{"Queue" = "AlphaTest+100"}
        Pass
        {
            // Tags{"LightMode" = "RendererListOverrideShaderTag"}

            ZTest Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _ _OverrideShaderKeyword

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _floatValue;

            struct Attribute
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings_vert //Vertexシェーダーの出力セマンティクスとして解釈される
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NNNNNNNNN; //INPUT_LAYOUT じゃないから何でもいい。
                float3 positionWS : TEXCOORD1;
            };
            struct Varyings_frag //fragmentシェーダーの入力セマンティクスとして解釈される
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NNNNNNNNN/*a*/; //入力セマンティクスと出力セマンティクスが違うと無警告で飛ぶ
                float3 positionWS : TEXCOORD1;
            };

            Varyings_vert vert(Attribute input)
            {
                Varyings_vert output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings_frag input) : SV_Target
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
                #if !defined(_OverrideShaderKeyword)
                    float3 ambient = float3(0.2, 0.6, 0.3); // 柔らかい緑色
                #else
                    float3 ambient = float3(1.0, 0.2, _floatValue); // 強い赤色
                #endif
                float3 finalColor = diffuse + specular + ambient;
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
