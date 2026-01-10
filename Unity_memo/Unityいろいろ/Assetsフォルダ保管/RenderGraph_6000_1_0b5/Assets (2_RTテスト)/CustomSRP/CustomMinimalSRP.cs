using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using UnityEditor;
#if USING_CORE_RP // ← CoreRP がある時だけ定義して使う（Player Settings で Scripting Define Symbols に追加）
using UnityEngine.Experimental.Rendering;
#endif
//SCMテスト
public sealed class CustomMinimalSRP : RenderPipeline
{
    static class ShaderIDs
    {
        public static readonly int _CameraColorTex = Shader.PropertyToID("_Custom_CameraColor");
        public static readonly int _CameraDepthTex = Shader.PropertyToID("_Custom_CameraDepth");
        public static readonly int _ResolvedTex    = Shader.PropertyToID("_Custom_Resolved");
        public static readonly int _SourceTex      = Shader.PropertyToID("_SourceTex");
    }

    readonly CustomMinimalSRPAsset _asset;
    readonly Material _fullscreenMat;

#if USING_CORE_RP
    // （任意）RTHandle の例。CoreRP がある時だけ使える。
    RTHandle _colorRTHandle, _depthRTHandle, _resolveRTHandle;
#endif

    public CustomMinimalSRP(CustomMinimalSRPAsset asset)
    {
        _asset = asset;
        _fullscreenMat = new Material(asset.fullscreenShader); //{ hideFlags = HideFlags.HideAndDontSave };
        #if UNITY_EDITOR
            AssetDatabase.CreateAsset(_fullscreenMat, "Assets/CustomSRP/FullscreenSample.mat");
        #endif
        _asset.fullscreenMaterial = _fullscreenMat;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            //if (_fullscreenMat) UnityEngine.Object.DestroyImmediate(_fullscreenMat); 
#if USING_CORE_RP
            _colorRTHandle?.Release();
            _depthRTHandle?.Release();
            _resolveRTHandle?.Release();
#endif
        }
    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        // Array.Sort(cameras.ToArray(), (a,b) => a.depth.CompareTo(b.depth));

        foreach (var cam in cameras)
        {
            if (cam == null || !cam.isActiveAndEnabled) continue;

            // --- Culling ---
            if (!cam.TryGetCullingParameters(out var cullingParams)) continue;
            var cull = context.Cull(ref cullingParams);

            // --- Camera setup ---
            var cmd = CommandBufferPool.Get("Custom SRP");
            context.SetupCameraProperties(cam);

            // --- RTDesc（MSAA / MipMap 手動生成 可）---
            var desc = new RenderTextureDescriptor(
                cam.pixelWidth, cam.pixelHeight,
                _asset.colorFormat, _asset.depthBits)
            {
                msaaSamples = Mathf.Max(1, _asset.msaaSamples),
                sRGB = _asset.useSRGB,
                autoGenerateMips = false,   // ← GenerateMips を使うので false
                useMipMap = true,
                depthBufferBits = 0,        // depth は別で確保
            };
            var depthDesc = new RenderTextureDescriptor(cam.pixelWidth, cam.pixelHeight,
                RenderTextureFormat.Depth, _asset.depthBits)
            {
                msaaSamples = desc.msaaSamples,
            };

            // --- Temporary RT を ID で（RenderTargetIdentifier を使う）---
            cmd.GetTemporaryRT(ShaderIDs._CameraColorTex, desc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderIDs._CameraDepthTex, depthDesc, FilterMode.Point);

            // --- SetRenderTarget + ClearRenderTarget（クリア系）---
            cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderIDs._CameraColorTex),
                                new RenderTargetIdentifier(ShaderIDs._CameraDepthTex));
            cmd.ClearRenderTarget(true, true, cam.backgroundColor); // 深度/カラーをクリア
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // --- Draw Opaque ---
            var sorting = new SortingSettings(cam) { criteria = SortingCriteria.CommonOpaque };
            var drawing = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sorting)
            {
                perObjectData = PerObjectData.None
            };
            var filtering = new FilteringSettings(RenderQueueRange.opaque);

            RendererListParams rendererListParams = new(cull, drawing, filtering);
            RendererList drawRendererList = context.CreateRendererList(ref rendererListParams);
            cmd.DrawRendererList(drawRendererList);

            // --- Skybox ---
            RendererList skyRendererList = context.CreateSkyboxRendererList(cam);
            cmd.DrawRendererList(skyRendererList);

            // --- Draw Transparent ---
            sorting.criteria = SortingCriteria.CommonTransparent;
            drawing.sortingSettings = sorting;
            filtering = new FilteringSettings(RenderQueueRange.transparent);
            RendererListParams transparentRendererListParams = new(cull, drawing, filtering);
            RendererList drawTransparentRendererList = context.CreateRendererList(ref transparentRendererListParams);
            cmd.DrawRendererList(drawTransparentRendererList);

            // --- SetGlobalTexture & GenerateMips（MipMap 生成）---
            cmd.SetGlobalTexture(ShaderIDs._SourceTex, ShaderIDs._CameraColorTex);
            cmd.GenerateMips(new RenderTargetIdentifier(ShaderIDs._CameraColorTex)); //rtがMSAAの場合はMipMapは生成しない?

            // --- MSAA Resolve（MSAA → 非 MSAA へ Blit）---
            if (desc.msaaSamples > 1)
            {
                var resolveDesc = desc;
                resolveDesc.msaaSamples = 1;
                resolveDesc.useMipMap = false;
                cmd.GetTemporaryRT(ShaderIDs._ResolvedTex, resolveDesc, FilterMode.Bilinear);

                // Blit は暗黙に MSAA を解決してコピーする
                cmd.Blit(new RenderTargetIdentifier(ShaderIDs._CameraColorTex),
                         new RenderTargetIdentifier(ShaderIDs._ResolvedTex));

                cmd.SetGlobalTexture(ShaderIDs._SourceTex, ShaderIDs._ResolvedTex);
            }

            // --- 最終フレーム（CameraTarget）へフルスクリーン描画 ---
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            // フルスクリーントライアングル（頂点なし：SV_VertexID）
            cmd.DrawProcedural(Matrix4x4.identity, _fullscreenMat, 0,
                               MeshTopology.Triangles, 3, 1);

            // --- 後片付け ---
            cmd.ReleaseTemporaryRT(ShaderIDs._CameraColorTex);
            cmd.ReleaseTemporaryRT(ShaderIDs._CameraDepthTex);
            if (desc.msaaSamples > 1)
                cmd.ReleaseTemporaryRT(ShaderIDs._ResolvedTex);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }
    }
}
