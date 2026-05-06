Shader "BlitWithMaterial"
{
   Properties
   {
       _BlitTint("Blit の色", Color) = (0, 1, 0, 1)
       _BlitStrength("Blit の強さ", Range(0, 1)) = 1
   }
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "BlitWithMaterialPass"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           float4 _BlitTint;
           float _BlitStrength;

           // Frag 関数は、テクスチャのサンプリングに使うスクリーン空間座標を含む構造体を入力として受け取ります。
           // また SV_Target0 へ書き込みます。これはレンダーパススクリプト側で定義したアタッチメントのインデックス 0 と対応する必要があります。
           float4 Frag(Varyings input) : SV_Target0
           {
               // XR プラットフォームごとのテクスチャ配列の扱いの違いを考慮するために必要です。
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

               // SAMPLE_TEXTURE2D_X_LOD を使ってテクスチャをサンプリングします。
               float2 uv = input.texcoord.xy;
               half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);
               
               // サンプリングした色を変更します。
               half4 tintedColor = color * _BlitTint;
               return lerp(color, tintedColor, saturate(_BlitStrength));
           }

           ENDHLSL
       }
   }
}
