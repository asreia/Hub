# URPレンダリングフローまとめ2

## OnMainRendering

- `void OnMainRendering(renderGraph, context, requireResults.renderPassInputs, requireResults.requirePrepass, requireResults.requireDepthTexture)`: [](images\URPレンダリングフローまとめ\まとめ2\OnMainRendering.png)

- `GPUOcclusionCulling`: [](images\URPレンダリングフローまとめ\まとめ2\GPUOcclusionCulling\GPUOcclusionCulling0.png)
```csharp
OccluderPass occluderPass = OccluderPass.None;
if (cameraData.useGPUOcclusionCulling)
{
    if (requiresPrepass)
        occluderPass = OccluderPass.DepthPrepass;
    else
        occluderPass = OccluderPass.ForwardOpaque;
}
if (requiresPrepass)
{
    TextureHandle depthTarget = useDepthPriming ? resourceData.activeDepthTexture : resourceData.cameraDepthTexture;

    //『`requiresPrepass==true`の場合はココで`GPUOcclusionCulling`を行う。そうでない場合は`OccluderPass.ForwardOpaque`で行う
    bool needsOccluderUpdate = occluderPass == OccluderPass.DepthPrepass;
    var passCount = needsOccluderUpdate ? 2 : 1;
    for (int passIndex = 0; passIndex < passCount; ++passIndex)
    {
        uint batchLayerMask = uint.MaxValue; //『`FilteringSettings`の.ctorでも`m_BatchLayerMask = uint.MaxValue`と設定されている
        if (needsOccluderUpdate)
        {
            // 1 回目のパス: 前フレームの最終深度ピラミッドに対して全ての`Instance`をテストします。
            // 2 回目のパス: `1 回目のパス`でカリングされた`Instance`を今フレームの中間深度ピラミッドに対して再テストします。
            OcclusionTest occlusionTest = (passIndex == 0) ? OcclusionTest.TestAll/*`uint.MaxValue`*/ : OcclusionTest.TestCulled/*`BatchLayer.InstanceCullingIndirectMask`(1u<<28(Layer28))*/;
            InstanceOcclusionTest(renderGraph, cameraData, occlusionTest);
            batchLayerMask = occlusionTest.GetBatchLayerMask();
        }

        m_DepthPrepass.Render(renderGraph, frameData, in depthTarget, batchLayerMask, depthTarget == resourceData.cameraDepthTexture);

        if (needsOccluderUpdate)
        {
            // 1 回目のパス: 現在フレームの中間深度ピラミッドを作成します。
            // 2 回目のパス: 現在フレームの最終深度ピラミッドを作成し、後続パス用のオクルージョンテスト結果を設定します。
            UpdateInstanceOccluders(renderGraph, cameraData, depthTarget);
            if (passIndex != 0)
                InstanceOcclusionTest(renderGraph, cameraData, OcclusionTest.TestAll);
        }
    }
}
```
