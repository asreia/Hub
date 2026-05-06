# class_SRF＆SRPass＆ContexItem

## 構成

- **構成コード**
  ```csharp
  using UnityEngine.Rendering;
  using UnityEngine.Rendering.Universal;
  using UnityEngine.Rendering.RenderGraphModule;

  public class Feature : ScriptableRendererFeature //`RenderPipelineAsset<T>`相当
  {
      class Item : ContextItem
      {
          public TextureHandle textureHandle;
          public override void Reset() //プールされているので、毎フレーム`Reset()`する
          {
              textureHandle = TextureHandle.nullHandle;
          }
      } 
      Pass pass;
      class Pass : ScriptableRenderPass //`RenderPipeline`相当
      {
          public Pass() //.ctor()
          {
              requiresIntermediateTexture = true; //サンプル集ではここで設定していた
          }
          public void Setup() //ユーザー定義
          {
              ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
          }
          class PassData{}
          public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
          {
              Item item = frameData.GetOrCreate<Item>(); //`ContextItem`を取得

              using(var builder = renderGraph.AddRasterRenderPass<PassData>("PassName", out var passData))
              {
                  builder.SetRenderFunc((PassData data, RasterGraphContext context) =>{});    
              }
          }
      }

      public override void Create() //`renderer`に`sRF`をセット時に呼ぶ
      {
          pass = new Pass(){renderPassEvent = RenderPassEvent.AfterRenderingOpaques + 1}; //.ctor()
      }
      public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) //毎フレーム呼ぶ
      {
          pass.Setup();
          renderer.EnqueuePass(pass); //`renderer`に`pass`を追加
      }
      public bool active;
      void OnValidate()
      {
          SetActive(active); //`SRF`有効化設定
      }
  }
  ```
- **処理サイクル**
  - `sRF`を`renderer`にセット時: `sRF.Create()`{`sRPass = SRPass.ctor()`}
  - **毎フレーム**:              `sRF.AddRenderPasses(renderer,.)`{`sRPass.Setup()`=>`renderer.EnqueuePass(sRPass)`}

## ScriptableRendererFeature

- `abstract void` **Create**`()`: `renderer`に`sRF`をセット時とデシリアライズ時に呼ぶ
- `abstract void` **AddRenderPasses**`(ScriptableRenderer renderer, ref RenderingData renderingData)`: 毎フレーム`urp.RecordAndExecuteRenderGraph(..)`前に呼ぶ
- `virtual void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)`: >レンダラーでカリングが行われる前のコールバックです。(`renderer.SetupCullingParameters(ref cullingParameters, ref cameraData)`前)
- `void SetActive(bool active)`,`bool isActive`: `SRF`有効化設定[SetActive](images\SetActive.png)
- `virtual void Dispose(bool disposing)`: `renderer.Dispose(){rendererFeatures[i].Dispose();}`で呼ばれる

## ScriptableRenderPass

- **.ctor()**`{renderPassEvent = RenderPassEvent.AfterRenderingOpaques; profilingSampler = new ProfilingSampler(this.GetType().Name);}`
- `override void`**RecordRenderGraph**`(RenderGraph renderGraph, ContextContainer frameData)`: `renderGraph`パスを記録
  - `virtual void OnCameraCleanup(CommandBuffer cmd)`: カメラスタック毎の`renderer`の最後にリソースを解放するためのコールバック。(`OnFinishCameraStackRendering(cmd)`もあった気がするけど消えてる?..)
- `RenderPassEvent` **renderPassEvent** `{ get; set; }`:`SRPass`を差し込むタイミングを指定(`+ 1`とかで細かく調整可能)。(`.ctor()`時に設定)
  - `static bool operator ⟪<¦>⟫(ScriptableRenderPass lhs, ScriptableRenderPass rhs) => lhs.renderPassEvent ⟪<¦>⟫ rhs.renderPassEvent`
  - `enum RenderPassEvent`{⟪`Before`『0～』¦`After`『1000～1050』⟫Rendering⏎
    ＠⟪`Shadows`『50～,100～』¦`PrePasses`『150～,200～』¦`Gbuffer`『210～,220～』¦`DeferredLights`『230～,240～』¦`Opaques`『250～,300～』¦`Skybox`『350～,400～』¦`Transparents`『450～,500～』¦`PostProcessing`『550～,600～』⟫}
- `void` **ConfigureInput**`(ScriptableRenderPassInput passInput)`, `ScriptableRenderPassInput input {get}`: `UniversalRenderer`に**指定されたテクスチャを要求**する。(`.Setup()`時に設定)
  - `enum ScriptableRenderPassInput{None, Depth, Normal, Color, Motion}`:組合せ可能
- `bool requiresIntermediateTexture { get; set; }`: >このプロパティを true に設定すると、URP フレーム内のすべてのパスが中間テクスチャを介してレンダリングされるようになります。
  このオプションは、バックバッファへの直接レンダリングに対応していないパスや、アクティブなカラーターゲットのサンプリングが必要なパスに使用してください。このオプションを使用すると、有線接続されていない VR プラットフォームにおいて、パフォーマンスに大きな影響が出る可能性があります。
- `DrawingSettings CreateDrawingSettings(＠❰List<❱ShaderTagId＠❰>❱ shaderTagId, ⟪｡ref RenderingData renderingData｡¦｡UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData｡⟫, SortingCriteria sortingCriteria)`
  最終的に`static DrawingSettings` **RenderingUtils**`.CreateDrawingSettings(ShaderTagId shaderTagId, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, SortingCriteria sortingCriteria)`を呼んでいる

## ContextContainer

- `using ContextContainer frameData = renderer.frameData`は、`URP.RenderSingleCamera(..)`で**設定**され、メソッドの終わりで`frameData.Dispose()`=>`contextItem.Reset()`される。

- public class MyContexItem : **ContexItem**: 基本的に`sRF`で定義(複数の`sRPass`で共有するため)
  - `public ｢Type｣ ｢name｣`: ユーザー定義メンバ (メソッドも書ける)
  - `public override void Reset(){..}`: `ContextItem`は`ContextContainer`で**プール**される(多分)ため**各メンバを再利用**できるよう**リセット**(初期化,参照破棄など)する

- **ContextContainer** frameData: `sRPass.RecordRenderGraph(..)`で使う
  - `bool Contains<MyContexItem>()`
  - `ContextItem Create<MyContexItem>()`
  - `ContextItem Get<MyContexItem>()`
  - `ContextItem GetOrCreate<MyContexItem>()`
