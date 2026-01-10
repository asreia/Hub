Shader "Custom/LayoutBlitShader"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MSAA _MSAA_SAMPLE _RTHandle
            #pragma multi_compile _SAMPLE_COL _SAMPLE_INDEX_COL _SAMPLE_AVERAGE_COL
            #pragma multi_compile _ _RTHandle_ClampAndScaleUV

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #if defined(_RTHandle)
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DynamicScalingClamping.hlsl"
            #endif

            #if defined(_MSAA_SAMPLE)
                Texture2DMS<float4> _SourceTex; //なぜか型引数は必須
            #elif defined(_MSAA) || defined(_RTHandle)
                Texture2D _SourceTex;
            #else
            // Texture2D<uint> _SourceTex; //depthDesc.stencilFormat用
            // Texture2D<float4> _SourceTex;
            Texture2DArray<float4> _SourceTex;
            // Texture2DArray<uint> _SourceTex;
            #endif
            SamplerState sampler_SourceTex;

            float2 _texDrawPoint;

            float4 _SourceTex_TexelSize; // x=1/width, y=1/height, z=width, w=height

            float4 _mipLevelAndDepthSlice; //x=mip, y=depthSlice

            #if defined(_RTHandle)
                // RTHandleSystemの最大サイズから要求された参照サイズへのスケール係数 (referenceSize/maxSize)
                // (x,y) 現在のフレーム (z,w) 前のフレーム (これはバッファ化されたRTHandleシステムでのみ使用される)
                float4 _MyRTHandleScale;
                float4 _viewportSize;
            #endif

            #if defined(_MSAA) || defined(_MSAA_SAMPLE)
                float4 _cellSize; //= float4(100.0, 100.0, 0.0, 0.0); //デフォルト値入らない..
            #endif

            float4 vert (uint vertexID : SV_VERTEXID) : SV_POSITION
            {
                float4 pos;
                float2 uv = float2(vertexID & 2, (vertexID << 1) & 2);
                pos = float4(uv * 2.0 - 1.0, 0, 1);
                pos.y *= _ProjectionParams.x;
                return pos;
            }

            float Map(float2 destRange, float2 srcRange, float srcValue)
            {
                return (((destRange.y - destRange.x) / (srcRange.y - srcRange.x)) * (srcValue - srcRange.x)) + destRange.x;
            }

            half4 frag (float4 pos : SV_POSITION) : SV_Target
            {
                pos.y = _ScreenParams.y - pos.y; //cmd.SetupCameraProperties(camera)で設定される
                half4 col = half4(0,0,0,0);
                #if defined(_MSAA) || defined(_MSAA_SAMPLE)
                    float2 texDrawEndpoint = _SourceTex_TexelSize.zw * _cellSize.xy + _texDrawPoint;
                #else
                    float2 texDrawEndpoint = _SourceTex_TexelSize.zw + _texDrawPoint;
                #endif
                if((_texDrawPoint.x <= pos.x && pos.x < texDrawEndpoint.x) && (_texDrawPoint.y <= pos.y && pos.y < texDrawEndpoint.y))
                {
                    float2 sampleUV = float2(Map(float2(0.0, 1.0), float2(_texDrawPoint.x, texDrawEndpoint.x), pos.x),
                                        Map(float2(0.0, 1.0), float2(_texDrawPoint.y, texDrawEndpoint.y), pos.y));
                    uint2 loadUV = uint2((uint)(sampleUV.x * _SourceTex_TexelSize.z),
                                        (uint)(sampleUV.y * _SourceTex_TexelSize.w));

                    #if defined(_MSAA) || defined(_MSAA_SAMPLE)
                        sampleUV.y = 1.0 - sampleUV.y;
                        loadUV.y = (_SourceTex_TexelSize.w - 1) - loadUV.y;
                        
                        // セルの境界で線を描画
                        float2 cellPos = fmod(pos.xy - _texDrawPoint, _cellSize.xy);
                        if (cellPos.x < _cellSize.z || cellPos.y < _cellSize.z || 
                            cellPos.x > _cellSize.x - _cellSize.z || cellPos.y > _cellSize.y - _cellSize.z)
                        {
                            col = half4(1.0, 1.0, 1.0, 1.0); // 白い線
                        }
                        else
                        {
                            #if defined(_MSAA_SAMPLE)
                                #if defined(_SAMPLE_COL) || defined(_SAMPLE_INDEX_COL)
                                    float4 colors[4] = {
                                        float4(0.5, 0.0, 1.0, 1.0), // 紫 (0°)
                                        float4(1.0, 0.5, 0.0, 1.0), // オレンジ (90°)
                                        float4(0.0, 1.0, 0.0, 1.0), // 緑 (180°)
                                        float4(0.0, 0.0, 1.0, 1.0)  // 青 (270°)
                                    };
                                    int2 cellPart = int2(floor(cellPos.x / (_cellSize.x / 2.0)),
                                                    floor(cellPos.y / (_cellSize.y / 2.0)));
                                    // このセル部分に対応する単一のMSAAサンプルインデックスを計算する。
                                    int sampleIndex = (1 - cellPart.y) * 2 + cellPart.x;
                                    float4 sampleCol = _SourceTex.Load(int2(loadUV), sampleIndex);
                                    if (any(sampleCol.rgb != 0.0))
                                    {
                                        #if defined(_SAMPLE_COL)
                                            col = sampleCol;
                                        #elif defined(_SAMPLE_INDEX_COL)
                                            int colorIndex = 0;
                                            if (cellPart.x == 0 && cellPart.y == 0)
                                            {
                                                colorIndex = 0;
                                            }
                                            else if (cellPart.x == 1 && cellPart.y == 0)
                                            {
                                                colorIndex = 3;
                                            }
                                            else if (cellPart.x == 0 && cellPart.y == 1)
                                            {
                                                colorIndex = 1;
                                            }
                                            else // cellPart.x == 1 && cellPart.y == 1
                                            {
                                                colorIndex = 2;
                                            }
                                            col = colors[colorIndex];
                                        #endif
                                    }
                                #elif defined(_SAMPLE_AVERAGE_COL)
                                    for(int sampleIndex = 0; sampleIndex < 4; sampleIndex++)
                                    {
                                        col += _SourceTex.Load(int2(loadUV), sampleIndex);
                                    }
                                    col /= 4.0;
                                #endif
                            #elif defined(_MSAA)
                                col = _SourceTex.Sample(sampler_SourceTex, sampleUV);
                            #endif
                        }
                    #elif defined(_RTHandle)
                        #if defined(_RTHandle_ClampAndScaleUV)
                            float2 texelSize = 1 / float2(_viewportSize.x, _viewportSize.y); // 1/V  * V/RT = 1/RT (こっちと思ったけど、公式は`～_TexelSize`を使う..)
                            // texelSize = _SourceTex_TexelSize.xy;                          // 1/RT * V/RT = V/RT^2
                            sampleUV = ClampAndScaleUV(sampleUV, texelSize, 0.5, _MyRTHandleScale.xy); //`0.5`でちょうど`.Bilinear`はみ出しサンプの`黒`が消える
                                //useScaling=falseでは使用しない
                        #endif
                        col = _SourceTex.Sample(sampler_SourceTex, sampleUV);
                    #else
                        // col = _SourceTex.Load(int3(loadUV, (int)_mipLevelAndDepthSlice.x)); //恐らく、`pos`は`0.5`とか`2.5`とか`～.5`とか入るが、整数キャストにより`0`とか`2`とか`～`になる
                        // col = _SourceTex.Load(int4(loadUV, (int)_mipLevelAndDepthSlice.y, (int)_mipLevelAndDepthSlice.x)); //Texture2DArray //.Loadは範囲外のテクセルは黒になる
                        // col /= 255.0; //depthDesc.stencilFormat用
                        col = _SourceTex.SampleLevel(sampler_SourceTex, float3(sampleUV, _mipLevelAndDepthSlice.y), _mipLevelAndDepthSlice.x);
                        // col = _SourceTex.SampleBias(sampler_SourceTex, float3(sampleUV, _mipLevelAndDepthSlice.y), _mipLevelAndDepthSlice.x);
                    #endif
                }
                else
                {
                    // col = half4(pos.x / _ScreenParams.x , pos.y / _ScreenParams.y, 0.0, 0.0);
                    discard;
                }
                return col;
            }
            ENDHLSL
        }
    }

}