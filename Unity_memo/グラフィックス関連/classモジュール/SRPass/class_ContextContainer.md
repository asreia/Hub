# ContextContainer

- `using ContextContainer frameData = renderer.frameData`は、
  `URP.RenderSingleCamera(..)`で**設定**され`Dispose()`=>`Reset()`される。

- ContexItem
  - `public class MyContexItem : ContexItem`
    - `public ｢Type｣ ｢name｣` (ユーザー定義メンバ) (メソッドも書けるだろう)
    - `public override void Reset(){..}`
      `ContextItem`は`ContextContainer`で**プール**される(多分)ため**各メンバを再利用**できるよう**リセット**(初期化,参照破棄など)する

- ContextContainer
  - bool .Contains<MyContexItem>()
  - ContextItem .Create<MyContexItem>(): ⟪`sRF`¦`sRPass`⟫で`.Create`しとくべきかな?
  - ContextItem .Get<MyContexItem>(): `sRPass.RecordRenderGraph(., ContextContainer frameData)`ではこっちを使う
  - ContextItem .GetOrCreate<MyContexItem>()
