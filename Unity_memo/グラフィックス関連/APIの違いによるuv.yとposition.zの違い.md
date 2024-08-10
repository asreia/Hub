# APIの違いによるuv.yとposition.yとZ バッファの違い

- [様々なグラフィックス API のシェーダーの作成](https://docs.unity3d.com/ja/2023.2/Manual/SL-PlatformDifferences.htmls)
- [よく使うシェーダーのテクニック](https://zenn.dev/r_ngtm/articles/unity-urp10-shader-memo)
- [【Unity】接空間について+link](https://coposuke.hateblo.jp/entry/2020/12/21/144327#itangentw-%E3%81%A8-unity_WorldTransformParamsw)
- あとは、`Forward_Plus学習.md`のリンク見る
- #elif defined(SHADER_API_D3D11)
    #include "core/ShaderLibrary/API/D3D11.hlsl"
      #define UNITY_UV_STARTS_AT_TOP 1
      #define UNITY_REVERSED_Z 1
  - と、なっているがSHADER_API_D3D11は、
    UniversalRenderPipelineCore.cs/class ShaderGlobalKeywordsに
    無く、C#内を検索したが定義が見つからないので、Unity側で設定されている?
- 特殊な関数や操作: clip(..),discard, ddx(..),ddy(..), SV_Depthへの代入

## uv.y

- #if UNITY_UV_STARTS_AT_TOP
  - 現在のAPIのuv.yが左上から始まるか?
  - >常に 1 または 0 で定義されます。1 の値は、「テクスチャの一番上」で、テクスチャの V 座標が 0 であるプラットフォームで使用します。Direct3D のようなプラットフォームでは 1 を使用し、Open-GL のようなプラットフォームでは 0 を使用します。
- if (_MainTex_TexelSize.y < 0)
  - 値が負の場合そのテクスチャーは反転して描画されている?
  - UNITY_UV_STARTS_AT_TOPが0で、～_TexelSize.yが負である。ということは無い?
  - `ProjectionParams.x`が`-1`で描画されたか?

## position.y

- if (_ProjectionParams.x < 0)
  - _ProjectionParams.xは**UNITY_MATRIX_PのY軸**が反転しているかどうか?
    反転しているから、↑↑のuv.yが反転する?

## Z バッファ

near,farは、VS空間:0.1～1000, 正規化デバイス座標:-1～+1, Zバッファ:0～1 がある
    他に、positionCSの w除算前のz(raw_z?,0～fZ), w除算後のz(非線形(0～fZ/nZ～fZ),正規化デバイス座標), w(変換前のz(VS空間:nZ～fZ))

- Linear01Depth(float z) と LinearEyeDepth(float z) //Zバッファ用
  - >LinearEyeDepth を使うと非線形のデータをビュー座標系におけるカメラからの距離に変換できます。
    一方、Linear01Depth を使うとニアクリップ面で 0、ファークリップ面で 1 になる値に変換できます。
    - LinearEyeDepth(z) == (Linear01Depth(z) * (far - near)) + near ?

- #if defined(UNITY_REVERSED_Z) //Zバッファ用
  - 現在のAPIが**UNITY_MATRIX_PのZ軸**を反転しているかどうか? (Z バッファ空間が反転しているならば、行列のZ軸も反転しているはず?)
  - >ノート: DX11/12、PS4、XboxOne、Metal では、**Z バッファの範囲**は 1 から 0、UNITY_REVERSED_Z は定義されます。他のプラットフォームでは、範囲は 0 から 1 です。
    - CS空間ではなく、**API毎**の**Z バッファ空間の反転**を表している?
  - **深度バッファー**のフェッチ
    深度 (Z) バッファーの値を手動でフェッチしている場合は、バッファーの方向を確認してください。以下は、その例です。
    float z = tex2D(_CameraDepthTexture, uv);
    #if defined(UNITY_REVERSED_Z)
        z = 1.0f - z; //Z バッファ空間を0～1に合わせる
    #endif

- UNITY_Z_0_FAR_FROM_CLIPSPACE(rawClipSpace)
  - _ProjectionParams と UNITY_Z_0_FAR_FROM_CLIPSPACE(rawClipSpace) //rawClipSpaceは、多分w除算する前のposition.z
    ```c++
    // x = 1 or -1 (-1 if projection is flipped 投影が反転している場合は-1) 
        //>_ProjectionParams.x にはプラットフォーム毎の y の向きの扱いを吸収するために +1.0 または -1.0 が入っています。これを y 座標に掛けると y の向きを統一することができます。
    // y = near plane   //✖?APIによって↓↓のUNITY_Z_0_FAR_FROM_CLIPSPACEの定義からnearもfarもマイナスが付いたり付かなかったり意味が違ったりするので当てならない?
    // z = far plane    //DirectXの場合、nearもfarも1? //●↑near:0.1～far:1000などビュー空間の範囲が設定されていると思われる(https://light11.hatenadiary.com/entry/2019/12/18/010038)
    // w = 1/far plane //UNITY_Z_0_FAR_FROM_CLIPSPACE 用? //0～1 == UNITY_Z_0_FAR_FROM_CLIPSPACE(positionCS.z) * _ProjectionParams.w
    //float4 _ProjectionParams;
    //map(x, a, b, c, d){return ((x - a) / (b - a)) * (d - c) + c;}     //-far___near |>[カメラ] かな
    #if UNITY_REVERSED_Z //基本的にDirectX系?
        // TODO: 回避策。SHADER_API_GL_COREがスイッチで誤って定義されるバグがあります。
        #if (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES3) //OpenGL系だけどZが反転している場合
            //Zを反転させたGL => Zのクリップ範囲は[near, -far] -> [0, far]に再マッピング
                //map(coord, near❰_P.y❱, -far❰-_P.z❱, 0, far❰_P.z❱)  //●UNITY_Z_0_FAR_FROM_CLIPSPACEは、✖?❰w除算後❱、positionCS.z を [0, far❰_P.z❱] にすると思われる
            #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord/*positionCS.z?*/) max((coord - _ProjectionParams.y)/(-_ProjectionParams.z-_ProjectionParams.y)*_ProjectionParams.z, 0)
        #else //DirectX系
            //maxは、斜めの行列の場合にニアプレーンが正しくない／意味をなさないことを防ぐために必要である。
            //Zを反転させたD3d => Zのクリップ範囲は[near, 0] -> [0, far]に再マッピング //near❰.y❱ == 1 ? //もしかして、far❰.z❱ == 1 ?
            #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((1.0-(coord) / /*0 -*/ _ProjectionParams.y)*_ProjectionParams.z),0) //DirectXはこれ?
            #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max((((coord-_ProjectionParams.y) / 0 - _ProjectionParams.y)*_ProjectionParams.z) ,0) //ではない?
        #endif
    #elif UNITY_UV_STARTS_AT_TOP //DirectX系だけどZが反転しなかった場合
        //D3dでzが反転していない場合 => zのクリップ範囲は[0, far] -> 何もしない //0～fZのことを言っている?(http://marupeke296.com/DXG_No70_perspective.html)
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
    #else //OpenGL系
        //Opengl => z クリップ範囲は [-near, far] -> [0, far] にリマッピング。
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((coord + _ProjectionParams.y)/(_ProjectionParams.z+_ProjectionParams.y))*_ProjectionParams.z, 0)
    #endif
    ```
