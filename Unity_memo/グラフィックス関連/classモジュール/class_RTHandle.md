# RTHandle ＆ RTHandleSystem

## RTHandleSystem

- ☆`.ctor()` => `Initialize(int width, int height)` => `Alloc(..)` => `＠❰Re❱SetReferenceSize(int width, int height)` => `SetRT(..)`＆`cmd.Draw～(..)`
  `＠❰Re❱SetReferenceSize(..)`を**一切呼ばない**と、`rTHSys.rtHP.currentViewportSize`が更新されず`0`のままで描画できない
- 基本的に、`rth.referenceSize` = `rthSys.currentRenderTargetSize` = `(m_MaxWidths, m_MaxHeights)`
- `public static class RTHandles`: ただの**RTHandleSystemの静的ラッパー** (Unityのレンダリングシステムと合わせるならこれを使用)
    `static RTHandleSystem s_DefaultInstance = new RTHandleSystem()`;
    `public static void SetReferenceSize(int width, int height)`
      `s_DefaultInstance.SetReferenceSize(width, height)`;

- `.ctor()`
  `m_AutoSizedRTs = new HashSet<RTHandle>()`;`m_ResizeOnDemandRTs = new HashSet<RTHandle>()`;
  `m_MaxWidths = m_MaxHeights = 1`;

- `Initialize(int width, int height)`
  `m_MaxWidths = width`;`m_MaxHeights = height`;
- `ResetReferenceSize(int width, int height)`
  `m_MaxWidths = width`;`m_MaxHeights = height`; `//なぜか.Max(..)されているので必要`
  `SetReferenceSize(width, height, reset: true)`;
- `SetReferenceSize(int width, int height)`
  `SetReferenceSize(width, height, reset: false)`;

- `RTHandle Alloc(⟪Vector2 scaleFactor¦ScaleFunc scaleFunc⟫, RTHandleAllocInfo info)`: .useScaling=true
- `RTHandle Alloc(int width, int height, RTHandleAllocInfo info)`: .useScaling=false

- `Dispose()`
  `foreach(var rth in m_AutoSizedRTs)`{`Release(rth);`}``m_AutoSizedRTs.Clear()`;
  `foreach(var rth in m_ResizeOnDemandRTs)`{`Release(rth);`}`m_ResizeOnDemandRTs.Clear()`;
- `Release(RTHandle rth) => rth.Release()`;

```csharp
public void SetReferenceSize(int width, int height, bool reset)
{
    //previous = current
    m_RTHandleProperties.previousViewportSize = m_RTHandleProperties.currentViewportSize;
    m_RTHandleProperties.previousRenderTargetSize = m_RTHandleProperties.currentRenderTargetSize;
    //⟪｡設定される`ViewportSize`(width,height)が、`RenderTargetSize`(m_MaxWidths,m_MaxHeights)を超えている｡¦｡`reset`｡⟫なら`Resize(width, height)`
    if (width > m_MaxWidths || height > m_MaxHeights || reset) Resize(width, height); //`UNITY_EDITOR`だと、極端に大きい`RenderTargetSize`の場合は`reset`される
    //`current`設定
    m_RTHandleProperties.currentViewportSize = new Vector2Int(width, height);
    m_RTHandleProperties.currentRenderTargetSize = new Vector2Int(m_MaxWidths, m_MaxHeights);
    //`scale`設定 (ViewportSize / RenderTargetSize)
    Vector2 scales        = (Vector2)m_RTHandleProperties.currentViewportSize  / (Vector2)m_RTHandleProperties.currentRenderTargetSize;
    Vector2 scalePrevious = (Vector2)m_RTHandleProperties.previousViewportSize / (Vector2)m_RTHandleProperties.previousRenderTargetSize;
    m_RTHandleProperties.rtHandleScale = new Vector4(scales.x, scales.y, scalePrevious.x, scalePrevious.y);
}

void Resize(int width, int height)
{
    m_MaxWidths = Math.Max(width, m_MaxWidths); m_MaxHeights = Math.Max(height, m_MaxHeights); //m_MaxWidths = width; m_MaxHeights = height; で良くない?..
    var maxSize = new Vector2Int(m_MaxWidths, m_MaxHeights);
    foreach(RTHandle rth in m_AutoSizedRTs)
    {
        rth.referenceSize = maxSize;
        RenderTexture rt = rth.m_RT;
        rt.Release();
        rt.width = rth.GetScaledSize(maxSize).x; rt.height = rth.GetScaledSize(maxSize).y;
        rt.name = "～name～";
        rt.Create();
    }
}
```

### .Allocテクスチャ設定

#### RT生成 useScaling=⟪true¦false⟫

- ☆**RT生成.Alloc**は、内部に**バッファ**を基本的に**1つのみ持つ**(*デプスバッファ*のときは*ステンシルバッファ*も作られる)。
- デフォルトが`.Point`,`.Repeat`。[RTHandleAllocInfo例](images\RTHandleAllocInfo.png)

- `RTHandle Alloc(⟪｡⟪Vector2 scaleFactor¦ScaleFunc scaleFunc⟫｡¦｡int width, int height｡⟫, RTHandleAllocInfo info)`
  - **RenderTexture**
    - `rt.dimension`                = **info.dimension** `= TextureDimension.Tex2D;`
    - **○⟦, ┃rt.⟪width¦height⟫⟧**:
      - *useScaling=false*: <= `.Alloc(width, height,..)`
      - *useScaling=true* : <= `scale⟪Factor¦Func⟫`(`m_MaxWidths, m_MaxHeights`) <= `.Alloc(scale⟪Factor¦Func⟫,..)`
        (`info.enableShadingRate=true`のとき`ShadingRateImage.GetAllocTileSize(..)=>extern`を加味される)
    - **⟪color¦depthStencil⟫Format** <= **info.format** `= GraphicsFormat.R8G8B8A8_SRGB;`『`info.isShadowMap = true`ならば`info.format`を`デプス16bit`以上にして`depthStencilFormat`に設定
      - `rt.stencilFormat` = `GetStencilFormat(info.format)`『`info.format`が**デプスのとき自動的に設定**される
      - `rt.shadowSamplingMode`      <= **info.isShadowMap** `= false;`『`true`ならば`ShadowSamplingMode.CompareDepths`を設定。それ以外は`.None`
      - [バッファ選択](\images\バッファ選択.png)
    - `rt.msaaSamples`              = **info.msaaSamples** `= MSAASamples.None;`
      - `rt.bindMS`                   = **info.bindTextureMS** `= false;`
    - `rt.useMipMap`                = **info.useMipMap** `= false;`
      - `rt.autoGenerateMips`         = **info.autoGenerateMips** `= true;`
    - `rt.volumeDepth`              = **info.slices** `= 1;`
    - `rt.enableShadingRate`        = **info.enableShadingRate** `= false;`『>`true` に設定されている場合、`width` と `height` はタイル単位になる。
    - 以下雑多
      - `rt.useDynamicScale`          = **info.useDynamicScale** `= false;` && `m_HardwareDynamicResRequested` (*DynamicScale*するかは`DynamicResolutionHandler`のみに依存するっぽい?)
        - `rt.useDynamicScaleExplicit`  = **info.useDynamicScaleExplicit** `= false;` && `m_HardwareDynamicResRequested`
      - `rt.enableRandomWrite`        = **info.enableRandomWrite** `= false;`
      - `rt.vrUsage`                  = **info.vrUsage** `= VRTextureUsage.None;`
      - `rt.menoryless`               = **info.memoryless** `= RenderTextureMemoryless.None;`
  - **Texture** (デフォルトが`.Point`,`.Repeat`)
    - `tex.filterMode`              = **info.filterMode** `= FilterMode.Point;`『`.Bilinear`で`黒`を拾いたくない場合は`.Point`もあり (しかし、MipMapで拾うこともある)
    - `tex.anisoLevel`              = **info.anisoLevel** `= 1;`『`Forced On`で`1`が`9`になる
    - `tex.mipMapBias`              = **info.mipMapBias** `= 0f;`
    - `tex.wrapMode`⟪U¦V¦W⟫         = **info.wrapMode**⟪U¦V¦W⟫ `= TextureWrapMode.Repeat;`
      - `.Clamp`にしないと反対の`黒`を`.Bilinear`で拾ってしまう (しかし、開いている方は`黒`を拾う(`ClampAndScaleUV(..)`で*Clamp*する))
  - **RTHandle** (`m_～`は`internal`, それ以外は`{get; internal set;}`)
    - **m_Owner** = `this『rTHandleSystem』`『.ctor
    - **rth.referenceSize**:
      - *useScaling=false*: <= `.Alloc(width, height,..)` (`○⟦, ┃rt.⟪width¦height⟫⟧`と同じ)
      - *useScaling=true* : 本来は、`currentRenderTargetSize`と同じになるはずだが、`.Alloc`時は、`○⟦, ┃rt.⟪width¦height⟫⟧`と同じになっている..(`rth.GetScaledSize()`使用時注意)
      - 非RT生成.Alloc    : 設定されない。`0`のまま
    - **rth.useScaling** <=  ⟪`.Alloc(scale⟪Factor¦Func⟫,..)`:`true`¦`.Alloc(width, height,..)`:`false`⟫
      - ☆**rth.scale⟪Factor¦Func⟫** <= `.Alloc(scale⟪Factor¦Func⟫,..)`
      (`rth.scaleFunc`は`info.enableShadingRate=true`のとき`useScaling=false`の場合でも`ShadingRateImage.GetAllocTileSize(..)=>extern`を加味される)
    - `rth.SetRenderTexture(rt)` (`rt = CreateRenderTexture(..)`)
      - `m_RT`=`rt`, `m_NameID`=`new RenderTargetIdentifier(rt)`
      - `m_RTHasOwnership`=`true`『`true`のとき`rTHandle.Release()`時に`CoreUtils.Destroy(m_RT)`される
      - `m_ExternalTexture`=`null`
    - 以下`internal`
      - `rth.m_EnableHWDynamicScale`  = **info.useDynamicScale** `= false;`
      - `rth.m_EnableMSAA`           <= **info.msaaSamples** != `MSAASamples.None`
      - `rth.m_EnableRandomWrite`     = **info.enableRandomWrite** `= false;`
      - `rth.m_Nane`                 <= **info.name** `= name;`
  - **RTHandleSystem**
    - **m_AutoSizedRTs** <= `rth.useScaling=true`のとき`rth`を追加
  - **UnityObject**
    - `object.hideFlags` = `HindeFlags.HideAndDontSave`
    - `object.name` = `CoreUtils.GetRenderTargetAutoName(width, height, info.slices, info.format, info.dimension,` **info.name**`, info.useMipMap,..)`

#### 外部からテクスチャ持ち込み ⟪RT¦Tex¦RTI⟫

- `RTHandle Alloc(⟪RT¦Tex¦RTI⟫ texture ＠❰, bool transferOwnership = false『RT』❱ ＠❰, string name『RTI』❱)`
  - `rth.SetRender＠❰Texture❱(texture ＠❰, transferOwnership❱)`
    - `m_RT = ＠❰texture❱『RT』`, `m_RTHasOwnership = ＠❰transferOwnership❱『RT』`
    - `m_NameID = ⟪new RTI(texture)¦texture⟫『⟪RT,Tex¦RTI⟫』`
    - `m_ExternalTexture = ＠❰texture❱『Tex』`
  - `rth.useScaling = rth.m_EnableMSAA = rth.m_EnableRandomWrite = rth.m_EnableHWDynamicScale = false`
  - `rth.m_Name = ⟪texture.name¦name⟫『⟪RT,Tex¦RTI⟫』`

## RTHandle

- ＄HP＝❰HandleProperties❱

- RTHP
  - `struct RTHP`
    `Vector2Int ⟪previous¦current⟫｡⟪Viewport¦RenderTarget⟫Size;`
    `Vector4 rtHandleScale;`『.xy:current, .zw:previous
  - `RTHP`**rtHP**`{ get { return m_UseCustomHandleScales ? m_CustomHP : m_Owner.rtHP; } }`
    - `public void`**SetCustomHandleProperties**`(in RTHandleProperties properties)`
      `m_UseCustomHandleScales = true`;`m_CustomHP = properties`;
    - `public void`**ClearCustomHandleProperties**`()`
      `m_UseCustomHandleScales = false`;
- *referenceSize*とその*Scaling*
  - `public Vector2Int`**referenceSize**`{ get; internal set; }`: *useScaling=true*のとき注意↑↑
  - `public bool`**useScaling**`{ get; internal set; }`
    - `public Vector2` **scaleFactor**`{ get; internal set; }`
    - `internal ScaleFunc`**scaleFunc**
      `delegate Vector2Int ScaleFunc(Vector2Int size)`
    - `public Vector2Int`**GetScaledSize**`(＠❰Vector2Int refSize❱)`
      ⟪`refSize`¦`referenceSize`⟫を`useScaling`により⟪そのまま¦`scale⟪Factor¦Func⟫`⟫して返す。(`referenceSize`が信用ならないので注意)
- テクスチャ取得
  - `public ⟪RT¦Tex¦RTI⟫ ⟪rt¦externalTexture¦nameID⟫ { get { return ⟪m_RT¦m_ExternalTexture¦m_NameID⟫; } }`
  - `static implicit operator ⟪RT¦Tex¦RTI⟫(RTHandle handle)`: `Tex`は`⟪RT¦Tex⟫`
- その他情報取得
  - `public string name { get { return m_Name; } }`
  - `public bool isMSAAEnabled { get { return m_EnableMSAA; } }`
- Release()
  `m_Owner.m_AutoSizedRTs.Remove(this)`;
  `if(m_RTHasOwnership) CoreUtils.Destroy(m_RT)`;`m_RTHasOwnership = true`;
  `m_RT = null`;`m_ExternalTexture = null`;`m_NameID = BuiltinRenderTextureType.None`;

## RTHandleの使用

### CoreUtils.SetRenderTarget(.,RTHandle,..)

- ☆基本的には、`cmd.SetRenderTarget(..)` => `if(.useScaling) cmd.SetViewport(..)` => `cmd.ClearRenderTarget(..)`

```c
void SetRenderTarget(CommandBuffer cmd, RTHandle colorBuffer, RTHandle depthBuffer, ClearFlag clearFlag, Color clearColor,
                      int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown, int depthSlice = -1)//`∫サブリソース指定設定∫`
{
    cmd.SetRenderTarget(colorBuffer, depthBuffer, miplevel, cubemapFace, depthSlice); //`colorBuffer`の`RTI`からではなく、引数からの`∫サブリソース指定設定∫`で指定される
    SetViewportAndClear(cmd, colorBuffer, clearFlag, clearColor); //MRTの場合は`depthBuffer`が渡される
}
void SetViewportAndClear(CommandBuffer cmd, RTHandle colorBuffer, ClearFlag clearFlag, Color clearColor)
{
    // 部分的なビューポートをクリアする場合、
    // Unity はハードウェアクリアを使わず、`専用シェーダー`で描画したクアッドによってクリアを行う。
    //
    // しかし Scene View で `Wireframeモード`を有効にすると、 (`D3D12_FILL_MODE_WIREFRAME`ではないみたい)
    // `専用シェーダー`が上書きされてしまい、クリア処理が正しく動作しなくなる。
    //
    // そのため`エディター`環境では、
    //   - クリア前はビューポートを設定せず（フルスクリーンのまま）
    //   - ハードウェアクリアを確実に行い
    //   - クリア後にビューポートを設定する
    // という回避策を取っている。
    //
    //`エディター`でのわずかなパフォーマンス低下は許容すると判断している。
    //
    // 根本的な解決には`Wireframeモード`の仕組み自体のリファクタリングが必要。
    // （理想的には、ここで特別な処理を一切しなくて済む状態にしたい）
    #if !UNITY_EDITOR
        SetViewport(cmd, colorBuffer);//非`エディター`では`.currentViewportSize`でクリア (本来はこっち)
    #endif
        if(clearFlag != ClearFlag.None) cmd.ClearRenderTarget((RTClearFlags)clearFlag, clearColor, 1.0f/*デプス*/, 0x00/*ステンシル*/);
    #if UNITY_EDITOR
        SetViewport(cmd, colorBuffer);//`エディター`では`全面`クリア (`Wireframeモード`のために仕方なく)
    #endif
}
void SetViewport(CommandBuffer cmd, RTHandle colorBuffer) //基本的に`colorBuffer`の`.currentViewportSize`を使う
{
    if (colorBuffer.useScaling) //`useScaling=true`の時のみビューポートを設定
    {
        Vector2Int scaledViewportSize = colorBuffer.GetScaledSize(colorBuffer.rtHandleProperties.currentViewportSize);
        cmd.SetViewport(new Rect(0.0f, 0.0f, scaledViewportSize.x, scaledViewportSize.y));
    }
}
Vector2Int GetScaledSize(Vector2Int refSize)
{
    if (!useScaling)
        return refSize;

    if (scaleFunc != null)
    {
        return scaleFunc(refSize);
    }
    else
    {
        return new Vector2Int(
            x: Mathf.RoundToInt(scaleFactor.x * refSize.x),
            y: Mathf.RoundToInt(scaleFactor.y * refSize.y)
        );
    }
}
```

### ClampAndScaleUV(UV,..,scale) `useScaling=false`では使わない

```c
//DynamicScalingClamping.hlsl ==================================================================================================================
float2 ClampAndScaleUV(float2 UV, float2 texelSize, float numberOfTexels, float2 scale) //☆ //`scale`は`rtHandle.rtHP.rtHandleScale`を入れる
{
    float2 maxCoord = 1.0f - numberOfTexels * texelSize; //`texelSize`が`1/_viewportSize`なら正しいが..
    return min(UV, maxCoord) * scale; // 1/RT❰`texelSize`❱ * V/RT❰`scale`❱ = V/RT^2 を引くのは間違っていないか?..
    // return min(UV * scale, scale - (numberOfTexels * texelSize)); //●`～_TexelSize`ならこっちで正しいはず..
}
float2 ClampAndScaleUV(float2 UV, float2 texelSize, float numberOfTexels)
{
    return ClampAndScaleUV(UV, texelSize, numberOfTexels, _RTHandleScale.xy); //`_RTHandleScale`(恐らく、`RTHandles`)は`Core.hlsl`にある
}
float2 ClampAndScaleUVForBilinear(float2 UV, float2 texelSize)
{
    return ClampAndScaleUV(UV, texelSize, 0.5f); //`.Bilinear`は`0.5`
}
// >これはフルスクリーンバッファと、クリッピング用のテクセルの半分のオフセットを想定しています。
float2 ClampAndScaleUVForBilinear(float2 UV)
{
    return ClampAndScaleUV(UV, _ScreenSize.zw, 0.5f); //float4 _ScreenSize;//{w, h, 1/w, 1/h} (`Core.hlsl`にある) (これも`～_TexelSize`系..)
}
float2 ClampUV(float2 UV, float2 texelSize, float numberOfTexels, float2 scale)
{
    float2 maxCoord = scale - numberOfTexels * texelSize; //こちらの`texelSize`は`～_TexelSize`で正しい..
    return min(UV, maxCoord);
}

//LayoutBlitShader.shader(自作) ======================================================================================================================
#if defined(_RTHandle_ClampAndScaleUV)  //`_viewportSize`⇔`rtHand.rtHandleProperties.currentViewportSize`
    float2 texelSize = 1 / float2(_viewportSize.x, _viewportSize.y); // 1/V  * V/RT = 1/RT (こっちかなと思ったけど、Unity公式は`～_TexelSize`を使っている..)
    // texelSize = _SourceTex_TexelSize.xy;                          // 1/RT * V/RT = V/RT^2
    sampleUV = ClampAndScaleUV(sampleUV, texelSize, 0.5, _MyRTHandleScale.xy); //`0.5`でちょうど`.Bilinear`はみ出しサンプの`黒`が消える (うまく`0.5`で機能する)
        //useScaling=falseでは使用しない
#endif

//DeclareOpaqueTexture.hlsl ====================================================================================================================
TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);  //>注記: 実行時にコピーカラーで設定され、ポイント補間またはバイリニア補間を選択可能。
float4 _CameraOpaqueTexture_TexelSize;
float3 SampleSceneColor(float2 uv)
{
    uv = ClampAndScaleUVForBilinear(UnityStereoTransformScreenSpaceTex(uv), _CameraOpaqueTexture_TexelSize.xy); //`1/_viewportSize`ではなく`～_TexelSize`になっている..
    return SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv).rgb;
}
float3 LoadSceneColor(uint2 uv)
{
    return LOAD_TEXTURE2D_X(_CameraOpaqueTexture, uv).rgb;
}
```
