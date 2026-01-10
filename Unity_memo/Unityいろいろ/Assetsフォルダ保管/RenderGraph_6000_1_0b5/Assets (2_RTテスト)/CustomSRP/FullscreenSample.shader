Shader "Hidden/CustomSRP/FullscreenSample"
{
    Properties
    {
        _mipLv("Mip Level", Range(0.0, 9.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            Texture2D _SourceTex;
            SamplerState sampler_SourceTex;

            float _mipLv;

            struct VSOut {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            VSOut Vert(uint vid : SV_VertexID)
            {
                float2 uv = float2((vid << 1) & 2, vid & 2);
                VSOut o;
                o.pos = float4(uv * 2.0 - 1.0, 0, 1);
                // DX の Y 反転は不要（CommandBuffer.Blit ではなく DrawProcedural）
                o.uv = uv;
                return o;
            }

            float4 Frag(VSOut i) : SV_Target
            {
                // 例: LOD 0 を普通にサンプリング（.Sample(..)）
                // float4 col0 = _SourceTex.Sample(sampler_SourceTex, i.uv);

                // 例: 明示的 LOD を使いたければ .SampleLevel(..)
                float4 col1 = _SourceTex.SampleLevel(sampler_SourceTex, i.uv, _mipLv);

                return col1;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
