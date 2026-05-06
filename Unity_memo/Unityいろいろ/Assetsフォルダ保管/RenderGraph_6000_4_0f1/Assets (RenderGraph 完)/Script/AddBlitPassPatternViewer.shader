//AIに書かせたが一発で完璧(AddBlitPassPatternRendererFeature.cs, AddBlitPassPatternViewer.shader, codex://threads/019d917f-e660-74a3-80d4-dca1de923d6f)

Shader "Hidden/RenderGraph/AddBlitPassPatternViewer"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_GoodMipTarget);
            TEXTURE2D_ARRAY(_GoodArrayTarget);
            TEXTURE2D(_BadSelectedSliceTarget);
            TEXTURE2D_ARRAY(_BadArrayCopyTarget);

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(uint vertexID : SV_VertexID)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(vertexID);
                output.uv = GetFullScreenTriangleTexCoord(vertexID);
                return output;
            }

            float4 WithExpectedStrip(float4 actualColor, float4 expectedColor, float2 localUV)
            {
                if (localUV.x < 0.018 || localUV.x > 0.982 || localUV.y < 0.018 || localUV.y > 0.982)
                    return float4(1, 1, 1, 1);

                if (localUV.y < 0.18)
                    return expectedColor;

                return actualColor;
            }

            float4 ReferencePanel(float2 localUV)
            {
                if (localUV.x < 0.018 || localUV.x > 0.982 || localUV.y < 0.018 || localUV.y > 0.982)
                    return float4(1, 1, 1, 1);

                if (localUV.x < 0.333)
                    return float4(1, 0, 0, 1);
                if (localUV.x < 0.666)
                    return float4(0, 1, 0, 1);
                return float4(0, 1, 1, 1);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 gridUV = float2(saturate(input.uv.x), saturate(1.0 - input.uv.y));
                int column = min((int)(gridUV.x * 3.0), 2);
                int row = min((int)(gridUV.y * 2.0), 1);
                int panel = row * 3 + column;
                float2 localUV = frac(float2(gridUV.x * 3.0, gridUV.y * 2.0));

                if (panel == 0)
                {
                    float4 actualColor = SAMPLE_TEXTURE2D_LOD(_GoodMipTarget, sampler_PointClamp, localUV, 1.0/*`0.0`は赤*/);
                    return WithExpectedStrip(actualColor, float4(0, 1, 0, 1), localUV);
                }

                if (panel == 1)
                {
                    float4 actualColor = SAMPLE_TEXTURE2D_ARRAY_LOD(_GoodArrayTarget, sampler_PointClamp, localUV, 1, 0.0);
                    return WithExpectedStrip(actualColor, float4(0, 1, 1, 1), localUV);
                }

                if (panel == 2)
                {
                    float4 actualColor = SAMPLE_TEXTURE2D_LOD(_BadSelectedSliceTarget, sampler_PointClamp, localUV, 0.0);
                    return WithExpectedStrip(actualColor, float4(0, 1, 0, 1), localUV);
                }

                if (panel == 3)
                {
                    float4 actualColor = SAMPLE_TEXTURE2D_ARRAY_LOD(_BadArrayCopyTarget, sampler_PointClamp, localUV, 0, 0.0);
                    return WithExpectedStrip(actualColor, float4(1, 0, 0, 1), localUV);
                }

                if (panel == 4)
                {
                    float4 actualColor = SAMPLE_TEXTURE2D_ARRAY_LOD(_BadArrayCopyTarget, sampler_PointClamp, localUV, 1, 0.0);
                    return WithExpectedStrip(actualColor, float4(0, 1, 0, 1), localUV);
                }

                return ReferencePanel(localUV);
            }
            ENDHLSL
        }
    }
}
