using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "RenderGraphTest_PipelineAsset", menuName = "Rendering/RenderGraph Test PipelineAsset")]
public class RenderGraphTestAsset : RenderPipelineAsset<RenderGraphTestAsset.RenderGraphTestPipeline>
{
    protected override RenderPipeline CreatePipeline() => new RenderGraphTestPipeline();

    public class RenderGraphTestPipeline : RenderPipeline
    {
        Mesh quadMesh;
        Material monoMaterial;
        Material blitMaterial;
        RenderTexture intermediateRT;
        struct VertexData
        {
            public Vector3 pos;
            public Vector2 uv;
        }
        protected override void Render(ScriptableRenderContext ctx, List<Camera> cameras)
        {
            if(quadMesh == null || monoMaterial == null || intermediateRT == null)
            {
                if (quadMesh == null)
                {
                    quadMesh = new Mesh();
                    quadMesh.name = "quad";
                    quadMesh.Clear();
                    var verDesc = new VertexAttributeDescriptor[]
                    {
                        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0)
                    };
                    quadMesh.SetVertexBufferParams(4, verDesc);
                    // Vector3[] vertexData = new Vector3[]{new Vector3(-1.0f,-1.0f,0.0f), new Vector3(1.0f,-1.0f,0.0f), new Vector3(-1.0f,1.0f,0.0f), new Vector3(1.0f,1.0f,0.0f)};
                    // vertexData = new Vector3[]{new Vector3(-0.5f,-0.5f,0.0f), new Vector3(0.5f,-0.5f,0.0f), new Vector3(-0.5f,0.5f,0.0f), new Vector3(0.5f,0.5f,0.0f)};
                    var vertexData = new VertexData[]
                    {
                        new VertexData(){pos = new Vector3(-0.5f,-0.5f,0.0f), uv = new Vector2(0.0f, 0.0f)},
                        new VertexData(){pos = new Vector3(0.5f,-0.5f,0.0f), uv = new Vector2(1.0f, 0.0f)},
                        new VertexData(){pos = new Vector3(-0.5f,0.5f,0.0f), uv = new Vector2(0.0f, 1.0f)},
                        new VertexData(){pos = new Vector3(0.5f,0.5f,0.0f), uv = new Vector2(1.0f, 1.0f)},
                    };
                    quadMesh.SetVertexBufferData<VertexData>(vertexData, 0, 0, 4, 0, MeshUpdateFlags.Default);
                    quadMesh.SetIndexBufferParams(6, IndexFormat.UInt16);
                    // quadMesh.SetIndexBufferData<short>(new short[]{0,2,3,0,3,1}, 0, 0, 6, MeshUpdateFlags.Default);
                    quadMesh.SetIndexBufferData<short>(new short[]{0,2,3,0,1,3}, 0, 0, 6, MeshUpdateFlags.Default); //☆
                    quadMesh.SetSubMesh(0, new SubMeshDescriptor(0,6,MeshTopology.Triangles));
                    Debug.Log($"Create Mesh: {quadMesh}");
                }
                if (monoMaterial == null)
                {
                    monoMaterial = new Material(Shader.Find("Test/MonoShader"));
                    blitMaterial = new Material(Shader.Find("Test/BlitShader"));
                    Debug.Log($"Create Material: {monoMaterial}");
                    Debug.Log($"Create Material: {blitMaterial}");
                }
                if(intermediateRT == null)
                {
                    // intermediateRT = new RenderTexture(new RenderTextureDescriptor(150, 100, GraphicsFormat.R8G8B8A8_UNorm, 0, 0));
                    intermediateRT = new RenderTexture(new RenderTextureDescriptor(150, 100, GraphicsFormat.R8G8B8A8_SRGB, 0, 0)); //☆
                    intermediateRT.name = "n_intermediateRT";
                    Debug.Log($"Create RenderTexture: {intermediateRT}");
                }
            }
            CommandBuffer cmd0 = CommandBufferPool.Get("cmd0"); //.name未設定時Unnamed command buffer
            // cmd0.SetRenderTarget(BuiltinRenderTextureType.CameraTarget); //823 OMSetRenderTargets({ GameView RT })
            cmd0.SetRenderTarget(intermediateRT); //823 OMSetRenderTargets({ GameView RT })
            cmd0.ClearRenderTarget(RTClearFlags.Color, new Color(0.059f, 0.137f, 0.314f).linear); //824 ClearRenderTargetView({ 0.00, 1.00, 0.00, 1.00 })
            cmd0.DrawMesh(quadMesh, Matrix4x4.identity, monoMaterial);
            cmd0.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd0.ClearRenderTarget(RTClearFlags.Color, new Color(0.2f, 0.25f, 0.25f).linear);
            // cmd0.SetGlobalTexture("p_intermediateRT", new RenderTexture(1,1,0));
            cmd0.SetGlobalTexture("p_intermediateRT", intermediateRT); //コメントアウトしても前回の設定をキャッシュしてるみたい?
            cmd0.DrawMesh(quadMesh, Matrix4x4.identity, blitMaterial);
            ctx.ExecuteCommandBuffer(cmd0);
            CommandBufferPool.Release(cmd0);
            ctx.Submit();
            // Debug.Log($"Shader.globalRenderPipeline: {Shader.globalRenderPipeline}");
        }
    }
}