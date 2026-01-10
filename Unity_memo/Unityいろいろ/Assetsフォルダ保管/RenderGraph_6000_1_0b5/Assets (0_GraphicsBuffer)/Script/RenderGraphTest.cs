using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System.Text;

[CreateAssetMenu(fileName = "RenderGraphTest_PipelineAsset", menuName = "Rendering/RenderGraph Test PipelineAsset")]
public class RenderGraphTestAsset : RenderPipelineAsset<RenderGraphTestAsset.RenderGraphTestPipeline>
{
    public ComputeShader structuredBufferTest;
    protected override RenderPipeline CreatePipeline() => new RenderGraphTestPipeline(){structuredBufferTest = structuredBufferTest};

    public class RenderGraphTestPipeline : RenderPipeline
    {
        public ComputeShader structuredBufferTest; int kernelCSMain;
        GraphicsBuffer constantBuffer;
        GraphicsBuffer structuredBuffer;
        GraphicsBuffer appendStructuredBuffer;
        GraphicsBuffer consumeStructuredBuffer;
        GraphicsBuffer rawBuffer;
        GraphicsBuffer rWRawBuffer;
        unsafe struct CSharpData
        {
            public float data0; //4 (4)
            public Vector2 data1; //12 (8)
            public int data2; //16 (4)
            public Matrix4x4 data3; //80 (64)
            public fixed double data4[2]; //96 (16)
            public double4 data5; //128 (32)
            public int4x4 data6; //192 (64)
        }
        static readonly int allDataCount = 100;
        CSharpData[] cSharpData = new CSharpData[allDataCount];
        CSharpData[] tempCSharpData = new CSharpData[allDataCount];
        private int loopCounter = 0;
        protected override void Render(ScriptableRenderContext ctx, List<Camera> cameras)
        {
            void PrintBuffer(GraphicsBuffer buffer, string name, int dataCount, int startIndex = 0)
            {
                Span<CSharpData> tempSpan;
                buffer.GetData(tempCSharpData, 0, startIndex, dataCount);
                tempSpan = tempCSharpData.AsSpan(0, dataCount);
                PrintStructuredData(tempSpan, name, startIndex);

            }
            int dataCount = 20;
            uint counterValue = 10;
            if(loopCounter == 0)
            {
                Debug.Log("初期化 --------------------------------------------");
                kernelCSMain = structuredBufferTest.FindKernel("CSMain");
                appendStructuredBuffer =    new GraphicsBuffer(GraphicsBuffer.Target.Append, allDataCount , 192){name = "n_AppendStructured"}; //name は set{..} しか無かった
                consumeStructuredBuffer =   new GraphicsBuffer(GraphicsBuffer.Target.Append, allDataCount , 192){name = "n_ConsumeStructured"}; //GraphicsBuffer は UnityObject では無い
                structuredBuffer =          new GraphicsBuffer(GraphicsBuffer.Target.Structured, allDataCount , 192){name = "n_Structured"};
                constantBuffer =            new GraphicsBuffer(GraphicsBuffer.Target.Constant, allDataCount, 192){name = "n_Constant"};//Constant は stride:256 推奨
                rawBuffer =                 new GraphicsBuffer(GraphicsBuffer.Target.Raw, allDataCount , 192){name = "n_Raw"};
                rWRawBuffer =               new GraphicsBuffer(GraphicsBuffer.Target.Raw, allDataCount , 192){name = "n_RWRaw"};
                // tempCSharpData = new CSharpData[allDataCount];
                // cSharpData = new CSharpData[allDataCount];
                for(int i = 0; i < cSharpData.Length; i++)
                {
                    FillStructuredData(ref cSharpData[i]);
                }
                Debug.Log($"Shader.globalRenderPipeline: {Shader.globalRenderPipeline}");//=>RenderGraphTestPipeline
                // struct NumthreadsValues{uint x, uint y, uint z}; NumthreadsValues numthreadsValues;
                structuredBufferTest.GetKernelThreadGroupSizes(structuredBufferTest.FindKernel("CSMain"), out uint x, out uint y, out uint z);
                Debug.Log($"[numthreads({x}, {y}, {z})]");
                //Consume
                consumeStructuredBuffer.SetData(cSharpData, 0, 0, dataCount);
                PrintBuffer(consumeStructuredBuffer, "ConsumeStructured", dataCount);
                //Structured
                structuredBuffer.SetData(cSharpData, 0, 0, dataCount);
                PrintBuffer(structuredBuffer, "Structured", dataCount);
                //CBuffer
                constantBuffer.SetData(cSharpData, 0, 0, dataCount);
                PrintBuffer(constantBuffer, "Constant", dataCount);
                //Raw
                rawBuffer.SetData(cSharpData, 0, 0, dataCount);
                PrintBuffer(rawBuffer, "Raw", dataCount);
            }
            else
            {
                Debug.Log($"loop: {loopCounter} --------------------------------------------");
                //Append
                uint count = appendStructuredBuffer.GetCounterValue();
                int appendCount = (int)(count - counterValue);
                if(appendCount > 0)
                {
                    PrintBuffer(appendStructuredBuffer, "AppendStructured", appendCount, (int)counterValue);
                }
                //Raw
                PrintBuffer(rWRawBuffer, "RWRaw", dataCount);
            }
            CommandBuffer cmd0 = CommandBufferPool.Get("cmd0"); //.name未設定時Unnamed command buffer
            // cmd0.Clear(); //new ObjectPool<CommandBuffer>(null, x => x.Clear())のx => x.Clear()でClear()しているので呼ぶ必要は無い
            //Append
            appendStructuredBuffer.SetCounterValue(counterValue);
            consumeStructuredBuffer.SetCounterValue(counterValue);
            cmd0.SetComputeBufferParam(structuredBufferTest, kernelCSMain, "p_append_structuredBuffer", appendStructuredBuffer);
            cmd0.SetComputeBufferParam(structuredBufferTest, kernelCSMain, "p_consume_structuredBuffer", consumeStructuredBuffer);
            //Structured
            cmd0.SetComputeBufferParam(structuredBufferTest, kernelCSMain, "p_structuredBuffer", structuredBuffer);
            //CBuffer
            cmd0.SetGlobalConstantBuffer(constantBuffer, "p_constantBuffer", 0, 256); 
            //Raw
            cmd0.SetComputeBufferParam(structuredBufferTest, kernelCSMain, "p_rawBuffer", rawBuffer);
            cmd0.SetComputeBufferParam(structuredBufferTest, kernelCSMain, "p_rWRawBuffer", rWRawBuffer);
            //コンピュートシェーダ実行====
            cmd0.DispatchCompute(structuredBufferTest, kernelCSMain,3,1,1);
            //cmd0の実行================
            ctx.ExecuteCommandBuffer(cmd0);
            CommandBufferPool.Release(cmd0);
            ctx.Submit();

            loopCounter++;
        }

        double fillValue = 0.0;
        unsafe void FillStructuredData(ref CSharpData data, double increment = 0.01)
        {
            data.data0 = (float)(fillValue/0.42); fillValue += increment;
            
            // Vector2 data1 の各成分
            data.data1.x = (float)fillValue; fillValue += increment;
            data.data1.y = (float)fillValue; fillValue += increment;
            
            // float data2
            data.data2 = (int)(fillValue * 100); fillValue += increment;
            
            // Matrix4x4 data3 の各要素（行優先で代入）
            data.data3.m00 = (float)fillValue; fillValue += increment;
            data.data3.m01 = (float)fillValue; fillValue += increment;
            data.data3.m02 = (float)fillValue; fillValue += increment;
            data.data3.m03 = (float)fillValue; fillValue += increment;
            
            data.data3.m10 = (float)fillValue; fillValue += increment;
            data.data3.m11 = (float)fillValue; fillValue += increment;
            data.data3.m12 = (float)fillValue; fillValue += increment;
            data.data3.m13 = (float)fillValue; fillValue += increment;
            
            data.data3.m20 = (float)fillValue; fillValue += increment;
            data.data3.m21 = (float)fillValue; fillValue += increment;
            data.data3.m22 = (float)fillValue; fillValue += increment;
            data.data3.m23 = (float)fillValue; fillValue += increment;
            
            data.data3.m30 = (float)fillValue; fillValue += increment;
            data.data3.m31 = (float)fillValue; fillValue += increment;
            data.data3.m32 = (float)fillValue; fillValue += increment;
            data.data3.m33 = (float)fillValue; fillValue += increment;
            
            // fixed double 配列 data4[2]
            // fixed (double* p = data.data3)
            // {
                for (int i = 0; i < 2; i++)
                {
                    data.data4[i] = fillValue;
                    fillValue += increment;
                }
            // }
            
            // double4 data5（4要素をまとめて代入）
            data.data5 = new double4(fillValue, fillValue + increment, fillValue + 2 * increment, fillValue + 3 * increment);
            fillValue += 4 * increment;
            
            // float3x3 data6
            data.data6 = new int4x4(
                new int4((int)(fillValue * 100), (int)((fillValue + 1 * increment) * 100), (int)((fillValue + 2 * increment) * 100), (int)((fillValue + 3 * increment) * 100)),
                new int4((int)((fillValue + 4 * increment) * 100), (int)((fillValue + 5 * increment) * 100), (int)((fillValue + 6 * increment) * 100), (int)((fillValue + 7 * increment) * 100)),
                new int4((int)((fillValue + 8 * increment) * 100), (int)((fillValue + 9 * increment) * 100), (int)((fillValue + 10 * increment) * 100), (int)((fillValue + 11 * increment) * 100)),
                new int4((int)((fillValue + 12 * increment) * 100), (int)((fillValue + 13 * increment) * 100), (int)((fillValue + 14 * increment) * 100), (int)((fillValue + 15 * increment) * 100))
            );
            fillValue += 16 * increment;
        }

        StringBuilder sb = new StringBuilder();
        unsafe void PrintStructuredData(Span<CSharpData> temp1, string name, int startIndex = 0)
        {
            sb.Clear();

            int index = startIndex;
            bool once = true;
            foreach (var e in temp1)
            {
                if(once)
                {
                    once = false;
                    sb.AppendFormat("Index:{0}=={1}==\n", index, name);
                }
                else
                    sb.AppendFormat("Index:{0}===========================================\n", index);
                sb.AppendFormat("float     data0: {0,-5:F3}\n", e.data0);
                sb.AppendFormat("Vector2   data1: ({0,-5:F3}, {1,-5:F3})\n", e.data1.x, e.data1.y);
                sb.AppendFormat("int       data2: {0,-5}\n", e.data2);
                
                sb.AppendLine();
                sb.AppendLine("Matrix4x4 data3:");
                for (int row = 0; row < 4; row++)
                {
                    sb.Append("    ");
                    for (int col = 0; col < 4; col++)
                    {
                        sb.AppendFormat("{0,-5:F3} ", e.data3[row, col]);
                    }
                    sb.AppendLine();
                }
                
                sb.AppendLine();
                sb.AppendFormat("double2   data4: ({0,-5:F3}, {1,-5:F3})\n", e.data4[0], e.data4[1]);
                sb.AppendFormat("double4   data5: ({0,-5:F3}, {1,-5:F3}, {2,-5:F3}, {3,-5:F3})\n",
                    e.data5.x, e.data5.y, e.data5.z, e.data5.w);
                
                sb.AppendLine();
                sb.AppendLine("int4x4    data6:");
                for (int row = 0; row < 4; row++)
                {
                    sb.Append("    ");
                    for (int col = 0; col < 4; col++)
                    {
                        sb.AppendFormat("{0,5} ", e.data6[row][col]);
                    }
                    sb.AppendLine();
                }
                index++;
            }

            Debug.Log(sb.ToString());
        }
    }
}

public static class GraphicsBufferExt
{
    static uint[] copyCounter = new uint[1];
    static GraphicsBuffer copyCounterBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, sizeof(uint));

    public static uint GetCounterValue(this GraphicsBuffer buffer)
    {
        if(buffer.target != GraphicsBuffer.Target.Append) throw new ArgumentException($"引数 {nameof(buffer)} が {GraphicsBuffer.Target.Append} ではありません");
        GraphicsBuffer.CopyCount(buffer, copyCounterBuffer, 0);
        copyCounterBuffer.GetData(copyCounter);
        return copyCounter[0];
    }
}