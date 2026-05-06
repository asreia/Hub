# RenderGraphUtils(Blit)

- (`static`と`this RenderGraph graph`を省略)
- `using UnityEngine.Rendering.RenderGraphModule.Util`が必要
- `returnBuilder = true`にすることで`builder`に**追加設定可能**(`Use系`など)

## AddCopyPass

- `IBaseRenderGraphBuilder AddCopyPass(TextureHandle src, TextureHandle dest, string passName = "Copy Pass Utility", bool returnBuilder = false)`
  - **FBF**で`slice:0`,`mip:0`の**一枚のカラー**を`src`(カラー)から`dest`(カラー)へ**正確にコピー**する(`MSAA`も**サンプル毎にコピー**)
    [CanAddCopyPassMSAAとシェーダーコード](images\CanAddCopyPassMSAA.png)

## AddBlitPass (基本的にこっちを使う)

- `scale＆offset`なし, `num⟪Slice¦Mip⟫`が`⟪1¦-1⟫`, **NRP条件** に**合致**するとき、最適化のために`AddCopyPass(..)`に自動的に**切り替える** (しかし、`num`が`⟪1¦-1⟫`で`⟪src¦dest⟫⟪Slice¦Mip⟫`が`非0`でも**切り替えられる**という**バグ**がある[](images\BlitBug0.png))
- `src`が`デプス`かつ`MSAA`であるなら`.bindTextureMS`が`true`であること。(多分`デプス`は**リゾルブできない**から)
- `src`(カラー)から`dest`(デプス)への`Blit`は**禁止**されている。(多分カラーのほうが次元が多いから)

- `IBaseRenderGraphBuilder AddBlitPass(○⟦, ┃TextureHandle ⟪src¦dest⟫⟧, ○⟦, ┃Vector2 ⟪scale¦offset⟫⟧, ○⟦, ┃int ⟪src¦dest¦num⟫⟪Slice¦Mip⟫⟧,`
  `BlitFilterMode filterMode = .ClampBilinear, string passName = "Blit Pass Utility", bool returnBuilder = false)`
  - **説明**
    - `dest`カラーTarget: `src`(⟪カラー¦デプス⟫)の`Slice`と`Mip`の**任意の範囲**を`dest`(カラー)の**任意の位置**に`scale＆offset`と`filterMode`で**Blit**する (しかし、`src`は`Tex2D`しか**扱えない**という**バグ**がある[](images\BlitBug1.png))
    - `dest`デプスTarget: `slice:0`,`mip:0`の**一枚のデプス**を`src`(デプス)から`dest`(デプス)へ`scale＆offset`と**LOAD～(..)**で**Blit**する (`src`が`MSAA`の場合は手動リゾルブされ全サンプルの**一番奥を選択**する[](images\Blitデプス.png))
  - `num⟪Slice¦Mip⟫`は`numSlice:％-1`,`numMip:％1`であり、`-1`は**全範囲**となる。

## AddBlitPass(Material版)

- `IBaseRenderGraphBuilder AddBlitPass(BlitMaterialParameters blitParameters, string passName = "Blit Pass Utility w. Material", bool returnBuilder = false)`
  - `BlitMaterialParameters(○⟦, ┃TextureHandle ⟪src¦dest⟫⟧, ＠❰｡○⟦, ┃Vector2 ⟪scale¦offset⟫⟧ ＠❰, int scaleBiasPropertyID = -1❱｡❱, Material material, int shaderPass,`
    `＠❰, MaterialPropertyBlock mpb, ＠○⟦, ┃int ⟪src¦dest¦num⟫⟪Slice¦Mip⟫⟧, FullScreenGeometryType geometry = ⟪.ProceduralTriangle¦.Mesh⟫, int source⟪Texture¦Slice¦Mip⟫PropertyID = -1❱)`
    - **説明**: **独自**の`material`で**Blit**できるPassであり、基本的には❰↑↑の`AddBlitPass`の`dest`カラーTarget:～❱と同じようにループでサブリソースを指定しているが、こちらは独自の`material`を使うので⟪カラー¦デプス⟫の制限はなくバグも無い。
    - `MaterialPropertyBlock mpb`は`％null`でありその場合、内部の`s_PropertyBlock`がセットされ`～PropertyID系`が設定される。
    - `TextureHandle src`は`.nullHandle`にすることで`dest`**のみ**に**シェーダー処理**することもできる。
    - `src⟪Slice¦Mip⟫`は`％-1`でありその場合、`source⟪Slice¦Mip⟫PropertyID`は設定されない。
    - `num⟪Slice¦Mip⟫`は`numSlice:％-1`,`numMip:％1`であり、`-1`は**全範囲**となる。
    - `⟪｡scaleBiasPropertyID｡¦｡source⟪Texture¦Slice¦Mip⟫PropertyID｡⟫:％-1`の時の`PropertyID名`は、`"_Blit⟪ScaleBias¦Texture¦TexArraySlice¦MipLevel⟫"`
    - シェーダーコード(`Project/Create/Shader/SRP Blit Shader`)
      ```c
      Shader "Custom/NewBlitScriptableRenderPipelineShader"
      {   
          SubShader
          {
              HLSLINCLUDE
              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
              #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
              ENDHLSL

              Tags { "RenderType"="Opaque" }
              LOD 100
              ZWrite Off Cull Off
              Pass
              {
                  Name "NewBlitScriptableRenderPipelineShader"

                  HLSLPROGRAM
                  
                  #pragma vertex Vert
                  #pragma fragment Frag

                  float4 Frag (Varyings input) : SV_Target
                  {
                      float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgba;
                      return color;
                  }
                  
                  ENDHLSL
              }
          }
      }
      ```
