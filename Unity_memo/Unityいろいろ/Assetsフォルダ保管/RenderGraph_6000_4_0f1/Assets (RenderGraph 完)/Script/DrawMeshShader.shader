Shader "Custom/DrawMeshShader"
{
    Properties
    {
        //`builder.AllowGlobalStateModification(true)`をやめて`LocalProperty`にするために`Properties{..}`に含める必要は無い！(含めない場合は⟪Local¦Global⟫両方可能)
    }
    SubShader
    {
        Pass
        {
            //`builder.SetRenderAttachmentDepth`していなくても`cmd.BeginRenderPass(..)`の定義上どうしても
            //デプスバッファがSetRT(..)されてしまうので、明示的に`Off`にする必要があると思われる。
            ZTest Off   //C#側は`ReadOnly＠❰Depth❱＠❰Stencil❱`にすることしかできないと思われる。

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _FB_BLIT _FB_2_MERGE _FB_2_PLUS_1_MERGE _FB_3_MERGE
            #pragma multi_compile _ _FB_MRT
            #pragma multi_compile _ _ADD_TEX

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #if defined(_FB_BLIT)
                FRAMEBUFFER_INPUT_X_FLOAT(0);
                StructuredBuffer<float4> buffer;
            #elif defined(_FB_2_MERGE) || defined(_FB_2_PLUS_1_MERGE)
                FRAMEBUFFER_INPUT_X_FLOAT(0);
                FRAMEBUFFER_INPUT_X_FLOAT(1);
            #elif defined(_FB_3_MERGE)
                FRAMEBUFFER_INPUT_X_FLOAT(0);
                TEXTURE2D_X(_frierenTex); SAMPLER(sampler_frierenTex);
                TEXTURE2D_X(_fernTex); SAMPLER(sampler_fernTex);
                // FRAMEBUFFER_INPUT_X_FLOAT(1);
                // FRAMEBUFFER_INPUT_X_FLOAT(2);
            #else
                TEXTURE2D_X(_texture); SAMPLER(sampler_texture);
            #endif

            #if defined(_ADD_TEX)
                TEXTURE2D_X(_addTexture); SAMPLER(sampler_addTexture);
            #endif

            #if defined(_FB_MRT)
                FRAMEBUFFER_INPUT_X_FLOAT(2);
            #endif
            //↑をコメントアウトして↓だけでも動く(使われない`index`があっても良いみたい)
            // FRAMEBUFFER_INPUT_X_FLOAT(0);
            // FRAMEBUFFER_INPUT_X_FLOAT(1);
            // FRAMEBUFFER_INPUT_X_FLOAT(2);
            // TEXTURE2D_X(_frierenTex); SAMPLER(sampler_frierenTex);
            // TEXTURE2D_X(_texture); SAMPLER(sampler_texture);
            // TEXTURE2D_X(_addTexture); SAMPLER(sampler_addTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = float4(TransformObjectToWorld(input.positionOS.xyz), 1.0);
                output.positionCS.y *= _ProjectionParams.x;
                output.uv = input.uv;
                return output;
            }

            struct FragOut
            {
                float4 color0 : SV_Target0;
                float4 color1 : SV_Target1; //二個目のRTを`SetRT(..)`しなければ書いてあっても影響無いみたい。
                // float4 color2 : SV_Target2; //これを入れても動く(使われない`index`があっても良いみたい)
            };

            FragOut frag(Varyings input)
            {
                half4 color;
                #if defined(_FB_BLIT)
                    float2 uv = input.uv;
                    if(_ProjectionParams.x > 0) {uv.y = 1.0 - input.uv.y;}
                    float2 charSize = float2(_ScreenParams.y / 4.0, _ScreenParams.y / 2.0) * 1.1;
                    color = LOAD_FRAMEBUFFER_INPUT_X(0, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0));
                    color += (buffer[0] + buffer[1]) * 0.2;
                #elif defined(_FB_2_MERGE)
                    float2 uv = input.uv;
                    if(_ProjectionParams.x > 0) {uv.y = 1.0 - input.uv.y;}
                    float2 charSize = float2((_ScreenParams.y / 4.0) * 2.0, _ScreenParams.y / 2.0);
                    if(uv.x < 1.0 / 2.0)
                    {
                        color = LOAD_FRAMEBUFFER_INPUT_X(0, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(+charSize.x / 4.0, 0.0));
                    }
                    else
                    {
                        color = LOAD_FRAMEBUFFER_INPUT_X(1, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(-charSize.x / 4.0, 0.0));
                    }
                #elif defined(_FB_2_PLUS_1_MERGE)
                    float2 uv = input.uv;
                    // if(_ProjectionParams.x > 0) {uv.y = 1.0 - input.uv.y;} //バックバッファだから要らない
                    float2 charSize = float2((_ScreenParams.y / 4.0) * 3.0, _ScreenParams.y / 2.0);
                    if(uv.x < 2.0 / 3.0)
                    {
                        color = LOAD_FRAMEBUFFER_INPUT_X(0, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(+charSize.x / 6.0, 0.0));
                    }
                    else
                    {
                        color = LOAD_FRAMEBUFFER_INPUT_X(1, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(-charSize.x / 3.0, 0.0));
                    }
                #elif defined(_FB_3_MERGE)
                    float2 uv = input.uv;
                    // if(_ProjectionParams.x > 0) {uv.y = 1.0 - input.uv.y;} //バックバッファだから要らない
                    float2 charSize = float2((_ScreenParams.y / 4.0) * 3.0, _ScreenParams.y / 2.0);
                    if(uv.x < 1.0 / 3.0)
                    {
                        color = LOAD_FRAMEBUFFER_INPUT_X(0, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(+charSize.x / 3.0, 0.0));
                    }
                    else if(uv.x < 2.0 / 3.0)
                    {
                        // color = LOAD_FRAMEBUFFER_INPUT_X(1, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0));
                        color = SAMPLE_TEXTURE2D_X(_frierenTex, sampler_frierenTex, ((uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0)) / _ScreenParams.xy);
                    }
                    else
                    {
                        // color = LOAD_FRAMEBUFFER_INPUT_X(1, (uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(-charSize.x / 3.0, 0.0));
                        color = SAMPLE_TEXTURE2D_X(_fernTex, sampler_fernTex, ((uv * charSize) + ((_ScreenParams.xy - charSize) / 2.0) + float2(-charSize.x / 3.0, 0.0)) / _ScreenParams.xy);
                    }
                #else
                    float2 uv = input.uv;
                    if(_ProjectionParams.x > 0) {uv.y = 1.0 - input.uv.y;}
                    color = SAMPLE_TEXTURE2D(_texture, sampler_texture, uv);
                #endif

                #if defined(_ADD_TEX)
                    float2 uv1 = input.uv;
                    if(_ProjectionParams.x > 0) {uv1.y = 1.0 - input.uv.y;}
                    float4 addTexCol = SAMPLE_TEXTURE2D(_addTexture, sampler_addTexture, uv1);
                    color = (addTexCol * addTexCol.a) + (color * (1.0 - addTexCol.a));
                #endif

                FragOut output;

                #if defined(_FB_MRT)
                    half4 color1;
                    float2 uv1 = input.uv;
                    if(_ProjectionParams.x > 0) {uv1.y = 1.0 - input.uv.y;}
                    float2 charSize1 = float2((_ScreenParams.y / 4.0) * 2.0, _ScreenParams.y / 2.0);
                    color1 = LOAD_FRAMEBUFFER_INPUT_X(2, (uv1 * charSize1) + ((_ScreenParams.xy - charSize1) / 2.0));
                    color1 += float4(0.5, 0.3, 0.1, 1.0);
                    output.color1 = color1;
                #endif

                output.color0 = color;

                return output;
            }
            ENDHLSL
        }
    }
}
