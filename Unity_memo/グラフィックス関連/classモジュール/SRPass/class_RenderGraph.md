# RenderGraph

- RenderGraphは、RenderGraph内で扱う`～Handle`を準備し、
  `.Add～Pass(..)`で**RenderPass**を生成し、その`builder`で`～Handle`入出力と`SetRenderFunc(..)`で実行する`cmd`を記述し、
  `BeginRecording(.)`から`EndRecordingAndExecute()`で生成された**RenderPassのグラフ**を**コンパイル**することでパスカリングなどを行い、
  上記を`camera`の`renderer`毎に実行し、最後に`EndFrame()`を呼び、
  最終的に最初に`RenderGraph`にセットした`cmd`に必要な全ての**RenderPass**のみを記述され、最後にその`cmd`を`ctx.Submit(cmd)`する。
  (drawioで表現する)
- リソースは、最初Writeで確保、最後Readで解放
- NRPは、パスカル後にNRP条件から外れるまで複数のSubPassを1つのNRPに含め続ける
- Computeととの同期ポイントも作られる
- RasterPassのみパスマージされる

- まずは極力中身を見ない

- RenderGraph
  - **リソース準備**
    - Desc系
      - `TextureDesc GetTextureDesc(TextureHandle texture)`
      - `BufferDesc GetBufferDesc(BufferHandle buffer)`
    - Create系
      - `TextureHandle CreateTexture(⟪TextureDesc desc¦TextureHandle texture ＠❰, string name, bool clear = false❱⟫)`
    - Import系
      - `TextureHandle ImportBackbuffer(RenderTargetIdentifier rt, RenderTargetInfo info, ImportResourceParams importParams = default)`
        >`RenderTargetIdentifier`は不透明なハンドルであるため、レンダリンググラフはプロパティを導出できません。そのため、ユーザーは`info`引数を介してプロパティを渡す必要があります。
      - `BufferHandle ImportBuffer(GraphicsBuffer graphicsBuffer)`
      - `TextureHandle ImportTexture(RTHandle rt ＠❰｡＠❰, RenderTargetInfo info❱, ImportResourceParams importParams = default｡❱)`
        - `RenderTargetInfo info`: `RTHandle rt`の中身が`RTI`の時に必要
  - RenderPass構築
    - RenderPass生成
      - AddRasterRenderPass()
      - AddRenderPass()
      - AddComputePass()
      - AddUnsafePass()
    - **Builder**
      - Graph入出力
        - Use系
        - Attachment系
      - SetRenderFunc(..)
        - CommandBuffer
