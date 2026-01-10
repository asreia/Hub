# RenderTexTure操作関連

## UNITY_UV_STARTS_AT_TOP <https://chatgpt.com/c/68f219e2-1070-8321-a144-66b1528b2686>

- **画像ファイル**は左**下**原点: **左下**の**ピクセルデータから順にデータが配列**される(左下ピクセルデータを`0x0000`に配置)。**数学的な画像処理**で扱いやすい。
- **ディスプレイ**は左**上**原点: **左上**から**スキャンライン(CRT走査線)が走る**ので、画像ファイルをそのままスキャンすると上下反転する。
  (`画像プレビュー`と`OpenGL`は**自動的に反転処理**が入る)
- **DirectX**は左**上**原点: `DirectX`は**ディスプレイの座標系に従い**、自動的な反転処理は行わず、**最後に反転処理**を行うかはユーザーに委ねられる。
- [上下反転](images\上下反転.png)

## new RenderTexture(RTDesc)

- 設定の適用のされかた
  - `RTDesc`の`msaaSamples`,`width`＆`height`, `volumeDepth`は、**カラーとデプス**で**同じ設定が適用**される。
  - `RTDesc.volumeDepth`関係なしに`RTDesc.dimension`の`Tex2D`は**1つ**,`Cube`は**6つ**、**バッファを作る**。(`CubeArray`は`.volumeDepth`が**6の倍数**であることを要請する)
  - **デプスバッファでない**かつ**MSAAでない**ならば**MipMapを作れる**。(`useMipMap=ture`,`mipCount>1`)
  - `Tex3D`は`depthStencilFormat.None`(デプスは作れない),`colorFromat.～_SRGB`不可であり、`Slice 0`は設定した全ての`MipLv数`を持っているが、`Slice`が進む毎に`MipLv数`が減っていく
- フォーマット選定 (`R16G16B16A16_SFloat`, `⟪D24_UNorm¦D32_SFloat⟫_S8_UInt`, `R8_UInt` 基本的にパフォーマンスに問題なければこれでいい)
  - `RTDesc.graphicsFormat`: `⟪｡⟪⟦R⟧8G8⟦B⟧8A8⟫_SRGB｡¦｡＠❰R⟪8¦16¦32⟫＠❰G⟪8¦16¦32⟫＠❰B⟪8¦16¦32⟫A⟪8¦16¦32⟫❱❱❱_⟪U¦S⟫⟪Norm¦Int¦Float⟫｡⟫`『基本的に`Float`は`S`のみ
  - `RTDesc.depthStencilFormat`: `D⟪16¦24¦32⟫_⟪U¦S⟫⟪Norm¦Float⟫＠❰_S8_UInt❱`
  - `RTDesc.stencilFormat`: `R8_UInt` (`RenderTextureSubElement.Stencil`(class_CommandBuffer.md/SetGlobalTexture 参照))
  - `RTDesc.⟪graphics¦depthStencil¦stencil⟫Format`で`.None`を指定すると、その`rt`はその`～Format`の**バッファを持たない**
    そして、`SetRenderTarget(..)`で、そのバッファの**出力をキャンセル**することができ、設定次第で[*PixelShader*を起動させない事もできるらしい。](images\PS不発.png)
      (MRTの時、`SetRenderTarget(new RTI[]{..,rt{｢GraGraphicsFormat.None｣},..})`と入れると、`.None`以降の`rt`がSetされなくなるので注意)
  - [⟪｡⟪カラー¦デプス⟫バッファ｡¦｡プラットフォーム｡⟫でフォーマットがサポートされない場合はエラー](images\Format選定.png)
- `rt`の生成
  - 基本的には`rt.Create()`は実際は何も**作っていない**！。`rt.Create()`関係なく`SetRenderTarget(..)`など必要時に**必要な⟪デプス¦カラー⟫バッファのみ生成**される。

## ClearRenderTarget

- `ClearRenderTarget(RTClearFlags clearFlags, Color＠❰[]❱ backgroundColor＠❰s❱, float depth = 1.0, uint stencil = 0)`: (class_CommandBuffer.md/ClearRenderTarget 参照)

## SetRenderTarget(..)

- **メモ**
  - 引数の全ての`rt`(`color＠❰s❱`,`depth`)の間で、`RTDesc`のビュー項目(`.dimension`,`.⟪graphics¦depthStencil⟫Format`,`.mipCount`,`.volumeDepth`)
      がバラバラに**違っていても良い(普通に描画可能)**。
    `width`＆`height`は**エラーは出るが描画はできる**。が、やらない方がいいだろう。(デプス解像度サイズが足りない部分はデプステスト失敗となる)
    **全MSAAのMRTはok**だが、`rt`間の`msaaSamples`の違いはエラーで**Unity落ちる**。
  - **失敗**すると`BRTT.CameraTarget`に**フォールバック**することがある。
  - `RTDesc.volumeDepth`より大きい`RTI.depthSlice`を指定して描画するとエラーで**Unity落ちる**。(`Tex2D`は例外的にエラーにならず**描画できる**)
  - ☆[Mipが足りない場合は左上で描画](images/Mipが足りない場合は左上で描画.png) (`depth`は**Mipを作らない**)
  - MRTも非MRTも引数の**rt毎に∫サブリソース指定設定∫**することは**できない**。(つまり`Tex2DArray`の**要素を並べてMRT描画**することは**できない**)
  - 引数の`color＠❰s❱`に`rt.depthBuffer`、`depth`に`rt.colorBuffer`を渡しても、そのまま`rt`を渡したのと変わらず、適切に設定される。(`RenderBuffer`の意味が薄い気がする)
  - `CubeArray`のみ何故か`depth`が常に`Silce 0`をSetRTとなる。

- `＄サブリソース指定設定＝❰, int mipLevel, CubemapFace cubemapFace, int depthSlice❱`
  - `Tex2DArray`,`CubeArray`,`Tex3D`: `depthSlice`で指定
  - `Cube`: `cubemapFace`で指定 (`.Unknown`で`depthSlice`も可)
  - (大体、`CubemapFace`が`.Unknown`以外ならば`cubemapFace`、`.Unknown`ならば`depthSlice`が指定される(`Tex3D`は例外で`depthSlice`のみ))
  - **RenderTargetIdentifier(RTI)**: (Unity.drawio/ページ32 参照)

- **SetRenderTarget(..)**
  - `SetRenderTarget(RenderTargetIdentifier rt ＠∫サブリソース指定設定∫)`: **Single** : `rt`で**カラーとデプス**または**どちらか**を設定。
    `rt`が持つ`RTI`の`∫サブリソース指定設定∫`または`＠∫サブリソース指定設定∫`によって**サブリソースが指定される**。
      (`＠∫サブリソース指定設定∫`は、内部で`new RTI(rt, ∫サブリソース指定設定∫)`されているだけ(↓は`new RTI(color, ..)`))
  - `SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth ＠∫サブリソース指定設定∫)`: **ColorDepth** : `color`を**カラー**、`depht`を**デプス**に設定
    `color`が持つ`RTI`の`∫サブリソース指定設定∫`または`＠∫サブリソース指定設定∫`によって**サブリソースが指定される**。(`depth`に`∫サブリソース指定設定∫`があっても効果なし)
  - `SetRenderTarget(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth ＠∫サブリソース指定設定∫)`: **Multi＠❰Subtarget❱** : **MRT**を設定
    `＠∫サブリソース指定設定∫`のみによって**サブリソースが指定される**。(`colors`,`depth`に`∫サブリソース指定設定∫`があっても効果なし)

- **MRT**
```c
struct FragOut
{
    half4 col0 : SV_TARGET/*0*/; //`0`有無関係なし
    half4 col1 : SV_TARGET1;
    half4 col2 : SV_TARGET2;
    float depth : SV_DEPTH; //おまけ
};

FragOut frag (Varyings input){FragOut output; /*～*/ return output;}
```

## SetGlobalTexture

- `SetGlobalTexture(int nameID, RTI rt ＠❰, RenderTextureSubElement element❱)`: (class_CommandBuffer.md/SetGlobalTexture 参照)

## MipMap生成 (rt.autoGenerateMips, cmd.GenerateMips(rt)) (`サンプリング関連/MipMap選択.png`も参照)

- `useMipMap=ture`,`mipCount>1`の時、生成可能。`MipLv0`を基準に`Dispatch(..)`で全て(`mipCount`分)の`MipLv`を生成する。(基本的には..)
  - `MipLv0`を基準に全ての`MipLv`を作るので**上書き**されることに注意。
- なぜか、`MipMap`がうまく**生成されない**場合がある。(`Dispatch(..)`はあるが生成されていない)
- `RT.active`中でも生成できる(`cmd.GenerateMips(rt)`)。(`rt.autoGenerateMips`は**外れた時に生成**)
- `rt.autoGenerateMips`かつ`cmd.GenerateMips(rt)`は不可。エラーが出る。
  - RenderTexture.GenerateMips が失敗しました: このレンダリングテクスチャのミップマップは自動的に生成されます（この動作を無効にするには autoGenerateMips を false に設定してください）。

## MSAA関連 (`Unity.drawio/ページ38`も参照)

- [MSAAテスト](images/MSAAテスト.png)
- `bindMS=false`は、**普通のテクスチャ**を`bind`する用であり、**いつでも**`普通のテクスチャ`として`bind`可能。(`Texture2DMS<float4>`に`bind`しようとするとエラー)
  - 基本的に`RT.active`から外れたときに、`SampleCount`を`1`にした`非MSAAのrt`が**内部で自動的に生成**され、それに**リゾルブ**(`ResolveSubresource(..)`)される。
  - `cmd.ResolveAntiAliasedSurface(msaaRT ＠❰, resolveRT『.Create()が必要』❱)`することで、**任意のタイミング**で⟪自分自身(`msaaRT`)¦リゾルブ先(`resolveRT`)⟫に**リゾルブ**することができる。
- `bindMS=ture`は、**MSAAのテクスチャ**を`bind`する用であり、**いつでも**`MSAAのテクスチャ`として`bind`可能。(`Texture2D`に`bind`しようとするとエラー)
  - **リゾルブは一切されず**、`cmd.ResolveAntiAliasedSurface(msaaRT ＠❰, resolveRT❱)`を実行しても**一切リゾルブしない**。
- つまり`SampleCount > 1`の`rt`は、`bindMS`が`ture`か`false`かによって**どちらのテクスチャ**として`bind`するか**決まる**。
    (リゾルブする前は`MSAAのテクスチャ`で、リゾルブしたら`普通のテクスチャ`として`bind`することは**できない**)
- `msaaSamples`の指定は`⟪1¦2¦4¦8⟫`で、`1`は`非MSAA`。`bindMS=ture`は必ず`msaaSamples > 1`。`tex.Load(..,sampleIndex)`で`msaaSamples`の範囲外アクセスをしてもエラーにならず黒を出力する。
  - `enum MSAASamples`で設定できる

## メモ

- `cmd.SetGlobalTexture(sourceTex_id, BuiltinRenderTextureType.CameraTarget);`: `UnityDefault2D`になった..**BRTT**は入力に使えない?
- `cmd.Blit(customRT.depthBuffer, BuiltinRenderTextureType.CameraTarget);`: `customRT.depthBuffer`を渡しても**カラーバッファのみBlit**される
- `half4 col = half4(0,0,0,0);`: **シェーダーは暗黙的に0初期化しない**。複数スレッド間で使いまわされて何処かで色が入るとそのままになる
