# ScriptableRendererFeature

- `public override void`**Create**`()`
  - sRPass.ctor(..)

- `public override void`**AddRenderPasses**`(ScriptableRenderer renderer, ref RenderingData renderingData)` //今はframDataかも
  - sRPass.Setup(..)
  - renderer.EnqueuePass(sRPass)
