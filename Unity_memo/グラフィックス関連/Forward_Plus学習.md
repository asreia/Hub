# Forward_Plus学習

- Editor
  - Camera
  - Light
  - MeshFilter
  - MeshRenderer
    - Material (バリアント)
- C#
  - UniversalRenderPipeline : RenderPipeline
    - UniversalRenderPipelineAsset : RenderPipelineAsset
    - UniversalRenderer : ScriptableRenderer
      - UniversalRendererData : ScriptableRendererData
      - CustomPass : ScriptableRenderPass
        - CustomFeature : ScriptableRenderFeature
- ShaderLab
  - Shader
    - Properties
    - SubShader
      - Tags
        - RenderPipeline
      - Pass
        - Name
        - Tags
          - LightMode
        - RenderingState『GPUへの固定機能の設定(カリング、RGBADSマスク、ADSテスト、ブレンド)
        - HLSL
          - ShaderKeyword
- HLSL
  - SimpleLitForwardPass.hlsl
  - SpaceTransforms.hlsl //座標変換系
  - Color.hlsl
  - Lighting.hlsl
  - Shadows.hlsl
  - ShaderVariablesFunctions.hlsl //便利メソッド
  - UnityShaderUtilityLibrary? //UnityCG.incのURP版
- ライティング
  - 直接光
    - 反射光
    - 拡散光
  - シャドウ
  - 環境光
    - ライトマップ
    - ライトプローブ
    - リフレクションプローブ

- Forward+進め方
  - C#クラス
    - Component
      - Camera
      - Light
      - MeshFilter
      - MeshRenderer
    - Asset
      - Mesh
      - Texture
      - Shader
        - ShaderLab //言語表現
      - Material //バリアント
    - SRP
      - ●URP //URPPackageにDoxygen
        - ●frameData (旧RenderingData)
        - ●Forward+Renderer
    - 低レベル操作
      - データ
        - メッシュ
          - Mesh
        - テクスチャ
          - Texture
          - RenderTexture
        - スカラーとベクトルと行列 (テンソル)
          - float
          - Vector⟪2～4⟫
          - Matrix
      - 描画API
        - CommandBuffer
        - LowLevelAPI
          - Graphics
          - GL
  - ●HLSL //言語表現
    - [Direct3D でのグラフィックスの概念](https://learn.microsoft.com/ja-jp/windows/uwp/graphics-concepts/)
    - [Unity でレンダリングパイプラインとライティングを設定する](https://docs.unity3d.com/ja/2022.2/Manual/BestPracticeLightingPipelines.html)
    - [空間とプラットフォームの狭間で](https://tech.drecom.co.jp/knowhow-about-unity-coordinate-system/)
    - [【Unity】接空間について+link](https://coposuke.hateblo.jp/entry/2020/12/21/144327#itangentw-%E3%81%A8-unity_WorldTransformParamsw)
      - t,bはU,Vの座標系に一致している。接空間は右手系?  tangentOS.w が通常は-1なのは左手→右手系の変換? 基本的にはUV(tb)平面にNormalMapを張っている
      - tは必ずUの向きに一致し、bは必ずtに直行しVに一致するが、Vがどっちに向いているか分からない為、tangentOS.wにV(b)の向きを保存している
      - transpose(float4x4(row0, row1, row2, row3))で最適化してくれるかな?
    - [その70 完全ホワイトボックスなパースペクティブ射影変換行列](http://marupeke296.com/DXG_No70_perspective.html)
      - デプスバッファは非線形: >LinearEyeDepth を使うと非線形のデータをビュー座標系におけるカメラからの距離に変換できます。一方、Linear01Depth を使うとニアクリップ面で 0、ファークリップ面で 1 になる値に変換できます。[Unity のシェーダ内でスクリーンの色と深度を取得するときの注意点](https://blog.oimo.io/2022/07/19/unity-ss/)
    - [positionNDC](https://ny-program.hatenablog.com/entry/2021/10/20/202020)
    - [よく使うシェーダーのテクニック](https://zenn.dev/r_ngtm/articles/unity-urp10-shader-memo)
    - [シャドウ深度マップを向上させるための一般的な方法](https://learn.microsoft.com/ja-jp/windows/win32/dxtecharts/common-techniques-to-improve-shadow-depth-maps)
    - [カスケードされたシャドウ マップ](https://learn.microsoft.com/ja-jp/windows/win32/dxtecharts/cascaded-shadow-maps)
    - [LightModeの用途を整理する](https://light11.hatenadiary.com/entry/2022/03/15/195620)
    - 構成
      ジオメトリ ⊃ プリミティブ ⊃ サーフェス
      ピクセル、テクセル
    - データ
      - Pos(V3),Normal,Tangent,UV(V2),Color(V4),Motion(V3) と 頂点インデックス
      - RGBADS //D:Depth, S:Stencil
    - シェーダーステージ VTGC_R_DFDSAA
      - Vertex(3D空間) -> Tessellation\[Hull,Tessellator,Domain] -> Geometry -> Culling -> //Vector処理 VTGC
      Rasterize -> //パスタライス(あんこ入り) R
      PreZTest -> Fragment(スクリーン空間) -> ZTest -> StencilTest -> AlphaTest -> AlphaBlending //Raster処理 DFDSAA
    - 座標系と座標変換 M (-M->) W (-V->) V (-P->) P (-/Z->) C(ここでクリッピングされる?) (-Vp->) (\[0,解,0]～\[解,0,1]?) (-Rasterize?->)
      - MVP変換
        - UnityではM(Transform)とVP(Camera)変換が良く使われる?                            ↓光が手前が明るくて奥が暗いと考える
        - ⟪モデル¦ワールド¦ビュー¦プロジェクション,クリッピング(\[-1,-1,1]～\[1,1,0](Unityは1がNear))¦ビューポート(\[-1,-1]～\[1,1](違うかも))¦スクリーン(\[0,0]～\[RT解像度])⟫空間
    - プローブ
      - ⟪ライト¦アンビエント(IBL?)¦リフレクション⟫プローブ
    - シャドウ
      - カスケードシャドウ
    - ShaderLabの書き方(言語表現)
      - 教科書1に書いてあるので参考にする
      - レンダリングステート
        - ZClip, ZTest, ZWrite, Cull, Conservative, Offset(Zファイティング回避), ColorMask, Blend, AlphaToMask, centroid(ある?)
          - ZClip:
          >深度クリップモードを固定に設定します。ニアクリップ面よりも近いフラグメントは、正確にアクリップ面に配置され、ファークリップ面よりも遠いフラグメントは正確にファークリップ面に配置されます。
          - Conservative:
          >慎重なラスタライズ(Conservative)とは、覆われる度合い(ピクセル中心点?)にに関係なく、**三角形で部分的に覆われているピクセルをラスタライズ**することです。
          ![Conservative](\..\画像\D3D12_CONSERVATIVE.png)
          >これは、オクルージョンカリング、GPU の衝突検出、可視性検出を行う場合など、確実性が求められる場合に役立ちます。
          - AlphaToMask
          >アルファテストで破棄された箇所の境界線を滑らかにします。(MSAAのアルファ版?)(教科書1_P43)
          - [centroid](https://wgld.org/d/webgl2/w013.html)
          ![centroid](\..\画像\centroid.png)
        - ステンシル (あるモノでマスクを作り、そのマスクで別のモノを描画(基本的にWriteしてRead))
          - Comp(ref & ReadMask, Ref & ReadMask)//特定のビットを比較, ⟪Pass¦Fail¦ZFail⟫(StencilBuffer & WriteMask)//Writeするビットを指定 かな?
          - 初期値: ReadMask:11111111, WriteMask:11111111, Comp:Always, ⟪Pass¦Fail¦ZFail⟫:Keep
    - [ラスター化ルール](https://learn.microsoft.com/ja-jp/windows/uwp/graphics-concepts/rasterization-rules)
      - ![ラスター化ルール](\..\画像\ラスター化ルール.png)
    - ライブラリ
      - SimpleLitForwardPass.hlsl
      - SpaceTransforms.hlsl //座標変換系
      - Color.hlsl
      - Lighting.hlsl
      - Shadows.hlsl
      - ShaderVariablesFunctions.hlsl //便利メソッド
      - UnityShaderUtilityLibrary? //UnityCG.incのURP版
  - デバッガー
    - FrameDebugger
    - RenderingDebugger
    - RenderDoc,PIX //ここまでは使わないかも

- その他メモ
  - ●SSGI:まずpositionWSを描画して、それをpositionWSの.x,.y,.zの順で昇順ソート(positionWSのAS構造)してバッファに書き出す。(positionWSに対するpositionSSもペアで書く)
      (SpaceFillingCurves.hlsl/uint EncodeMorton3D(uint3 coord) 使う? これを使って、ピクセルシェーダ時にUAVバッファに書いて挿入ソート?する)
      (静的メッシュを全てEncodeMorton3Dでソートしてバッファにベイクしとく?)
      (CubeMapのようにすれば全方位いける?(CubeMapのpositionWSポイントクラウド上でレイマーチ))
      ソートコストが大きすぎる。普通のレイトレーシングシェーダーのほうがいいかも。レイをSS空間に変換してレイマーチでもそれはSSGIか
    コンピュートシェーダで、positionWSバッファをLoadしてライティングしてpositionSSを参照しライティング結果用バッファに書く。
    続けて反射したレイでpositionWSバッファを参照し.x,.y,.zの順で２分探索してレイマーチして、(レイマーチ中はマンハッタン距離使うとか)
    しきい値内かつ最も近いpositionWSを探してライティングしてInterlockedAdd(..)(←要らないかも)でライティング結果用バッファに書く。
      (拡散反射を表現するために、しきい値を大きめにして、しきい値内に入ったpositionWS を 最も近い位置を探して、しきい値内に入った複数のpositionWSをライティングする?)
    そして、ライティング結果用バッファが描画結果となる
  - ●複雑な半透明の描画: PSO:Zを奥から手前に描画,フロントフェイスカリング で 一番奥のバックフェイスの深度のみを描画。その後、深度と一致したピクセルのみ描画。
    - PSO:バックフェイスカリング と 新しいDSのRT にして Pixelシェーダで前の深度値を参照しそれより奥だったらDiscardし手前ならその深度のみを描画。その後、深度と一致したピクセルのみ描画。
      - (PSO:Zを奥から手前に描画 と Discard により、PreZカルされるかDiscardされ無ければ、深度を描画できる)   ↓>SV_Depth は書き込み専用の出力変数；；UAVでいける?(それ自体が重い..)
      - (しかし、Discardを使うとPreZTest無効；；(Pixelシェーダの最初でPreZTest相当(CS_ZとSV_Depthの比較)を書けばコスト安いか?)) SV_DepthGreaterEqual: 初期 Z を無効にせず
        - ↑恐らく通常のPreZTestはラスタライザの時点で深度を決定してDSVに書いてしまうから?(Discardを使うとピクセルシェーダー起動しないと分からない)
          でもLODCrossFadeとか普通にDiscard(clip(.))使ってるが?
    - 追記:深度描画時、ピクセルシェーダーでDiscardしなかったらUAVに論理和(|=)で1を書いて、"深度と一致したピクセルのみ描画"をするかのフラグにする
  - ●アルファブレンディングを設定するより、RTのカラーをSRVとして読み込みfragでブレンド処理して、RTのデプスと新しいカラーバッファをRTVにセットすれば良いのでは？
    - (ジオメトリレンダー＆ポストプロセス)
  - 白飛び回避にSoftmax使うとか
  - デファードMSAA: ジオメトリ毎にindexを振りTextureにindexを書き込む
    そして、ライティング後ポスプロでddx,ddy系(フィルタ系処理でもいい)で隣のピクセルのindexを読みindexが同じで無かったらぼかすとか
    (プリミティブにindexを振ればwireFrame?,角度を書き込めば角度の度合いでアウトライン描画?)
  - SSAOは単純に周囲のピクセルのwSpと自信のピクセルのwSpの距離を取り周囲のサーフェスの近さでoS_attを決めてるだけ?
  - TextureArrayで影や輪郭のアニメーションとか(まぁビデオと同じでGPU送るTexture切り替えるかｗ)
  - LightingDataアセットはランタイムで参照される。(uniform変数かcpu側か)
  - ライトマップ専用のuvが作られライトマップはアトラス化されている
  - RealtimeGIはデフォルトで有効(BuildInRP(教科書3))
  - ライトマップBake時にアルベドとエミッションが必要なため"LightMode" = "META"を指定したメタPassが走る?(アルベドとエミッション(とスペキュラ)のマップを焼くだけ?)(教科書3P23,教科書4P61)
  - ●Mixedライトは、直接光はリアルタイム描画し、間接光はライト⟪マップ¦プローブ⟫ (Subtractiveは静的オブジェクトの直接光にもライトマップ使う(スペキュラがしぬ))
  - ポイントライト6面,並行光源カスケードと同様にスポットライトも分割キャストシャドウできるかな
  - GIにはライトマップには＠❰直接光❱,間接光があり、シャドウは⟪ライトマップ¦シャドウマスク¦ライトプローブ¦Ø⟫にベイク?
    - GIによる間接光計算には、くっきりとした影を描画するほど精度がないので、自前でシャドウを考慮した描画を行う必要がある(教科書3P31)
    - シャドウマスクのRGBAを超えたライトのシャドウはライトマップにベイクされる
    - アンビエントオクルージョンもライトマップにベイクできるがデフォルトoff
    - ●事前計算の直接光,間接光,シャドウ,AOはバラバラに使うことができる?
  - 教科書3、ライト⟪マップ¦プローブ⟫の理解度がうんこ
  - Scは可視光のRGB毎の反射量だから、Sc * Lcとなる
  - ジオメトリのLODで、頂点をカメラからの距離に対して徐々に移動し他の頂点に重ね、そこから先は重なった頂点の片方を飛ばすように描画できないか?(頂点削除とインデックス変更)
    - 隣接する頂点の位置が分からないか、サンプリングするか、頂点に格納する必要がある?MeshShader書け(Unityまだ実装してないかも(cmdに無かった))
  - 大量描画システムに縮小バッファオクルージョンカリングで更に高速化?(一番荒いLODLvでFarクリップを同じにした少し狭いFOVで描画、最終的に残ったジオメトリIDのみをDrawIndirect)
    - フラスタ無化リングはAABB?
  - Kyoukasyo4
    - Per Object Limit 追加ライト8個まで
    - 光や減衰などの呼び方と記号をきめる?(鏡面光など)
    - フレネル式が拡散反射量と鏡面反射量の割合を決めている。F0は鏡面反射の色
    - ●shadowDistance:ディレクショナルはカスケードが範囲によってストレッチする。スポットは範囲内外によってスイッチする
    - 初期化方法: Varings output = (Varings)0;
    - TransformObjectToWorld⟪Normal¦Dir⟫()の違い(教科書4P72)
    - ●SampleSH⟪Vertex¦Fragment⟫でライトプローブから取得してL0/L1/L2の計算をしている?(多分ライトマップは取得して貼ってるだけ)
    - SAMPLE_TEXTURE2D(map, sampler, uv) (教科書4P84)
      - GLES2: tex2D(map, uv)
      - 非GLES2: map.Sample(sampler, uv)
    - SAMPLE_TEXTURE2D_SHADOW(shadowMap, sampler, shadowCoord.xy, shadowCoord.z) (教科書4P105)
      - DX11: shadowMap.SampleCmpLevelZero(sampler, shadowCoord.xy, shadowCoord.z)
        - (int)(サンプリング値 >= shadowCoord.z) を返す
    - SAMPLE_TEXTURECUBE_LOD(cubeMap, sampler, reflect, mip)『mipは実数?(教科書4P110)
      - DX11: cubeMap.SampleLevel(sampler, reflect, mip)
    - real型(マクロ)練乳1.5
    - SafeNormalize(ベクトル)という長さがゼロでも不定にならない謎関数(教科書4P93)
    - Mixed Light対応 教科書4P108
    - UnpackNormal＠❰Scale❱(n, ＠❰scale❱); (教科書4P88)
  - ●Textureはサーフェス単位の頂点属性のようなもの?(baseColMapをvertexでuv参照したらそれは頂点カラーと変わらない)
  - Kyoukasyo5
    - _
