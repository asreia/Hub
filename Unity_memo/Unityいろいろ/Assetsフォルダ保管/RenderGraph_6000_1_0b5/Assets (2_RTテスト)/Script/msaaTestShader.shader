Shader "Coustom/msaaTestShader"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _msaaColor;

            float4 vert (float3 positionOS : POSITION) : SV_POSITION
            {
                return float4(TransformObjectToWorld(positionOS), 1.0);
            }
            half4 frag () : SV_Target
            {
                return _msaaColor;
            }
            ENDHLSL
        }
    }
}