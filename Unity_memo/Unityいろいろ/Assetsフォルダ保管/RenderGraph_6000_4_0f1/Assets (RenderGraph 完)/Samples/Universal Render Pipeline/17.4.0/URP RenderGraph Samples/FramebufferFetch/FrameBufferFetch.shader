Shader "FrameBufferFetch"
{
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "FrameBufferFetch"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           // half 型を保持する 2D テクスチャとして、framebuffer input を宣言します。
           FRAMEBUFFER_INPUT_HALF(0);

           // Frag 関数は、テクスチャのサンプリングに使うスクリーン空間座標を含む構造体を入力として受け取ります。
           // また SV_Target0 へ書き込みます。これはレンダーパススクリプト側で定義したアタッチメントのインデックス 0 と対応する必要があります。
           float4 Frag(Varyings input) : SV_Target0
           {
               // XR プラットフォームごとのテクスチャ配列の扱いの違いを考慮するために必要です。
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

               // フレームバッファから現在のピクセルを読み取ります。
               float2 uv = input.texcoord.xy;
               // 前のサブパスをフレームバッファから直接読み取ります。
               half4 color = LOAD_FRAMEBUFFER_INPUT(0, input.positionCS.xy);
               
               // サンプリングした色を変更します。
               return half4(0,0,1,1) * color;
           }

           ENDHLSL
       }

       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "FrameBufferFetchMS"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag
           #pragma target 4.5
           #pragma require msaatex

           // half 型を保持する 2D テクスチャとして、framebuffer input を宣言します。
           FRAMEBUFFER_INPUT_HALF_MS(0);

           // Frag 関数は、テクスチャのサンプリングに使うスクリーン空間座標を含む構造体を入力として受け取ります。
           // また SV_Target0 へ書き込みます。これはレンダーパススクリプト側で定義したアタッチメントのインデックス 0 と対応する必要があります。
           float4 Frag(Varyings input, uint sampleID : SV_SampleIndex) : SV_Target0
           {
               // XR プラットフォームごとのテクスチャ配列の扱いの違いを考慮するために必要です。
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

               // フレームバッファから現在のピクセルを読み取ります。
               float2 uv = input.texcoord.xy;
               // 前のサブパスをフレームバッファから直接読み取ります。
               half4 color = LOAD_FRAMEBUFFER_INPUT_MS(0, sampleID, input.positionCS.xy);
               
               // サンプリングした色を変更します。
               return half4(0,0,1,1) * color;
           }

           ENDHLSL
       }
   }
}
