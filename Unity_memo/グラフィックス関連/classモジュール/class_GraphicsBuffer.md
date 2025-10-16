# GraphicsBuffer (非UnityObject)

流れは`.ctor(～)` => `SetData(～)` => `cmd.Set～(～)`, => `Release()`

- **GraphicsBuffer.Target～**: (組み合わせ可能(`|`))
  - **.Structured**: SRV,UAV
    - C#側:
      - **.ctor**`(Target.Structured, int count, int stride)`:
        - `int stride`: `16Byte単位(SIMD)`で設定する(**16Byteアライメント**)
          `stride`は**DirectX12API**の`StructureByteStride`を設定するが、**アクセス単位**は`StructuredBuffer<｢struct｣>`の**_｢struct｣単位**(`stride != sizeof(｢struct｣)`でもいいみたい)
      - `SetData(～)`
      - `cmd.`**SetComputeBufferParam**`(ComputeShader computeShader, int kernelIndex, string name, GraphicsBuffer buffer)`:
        - アドレス: `ComputeShader computeShader, int kernelIndex, string name`: `kernelIndex = computeShader.FindKernel("｢CSMain｣")`
        - データ: `GraphicsBuffer buffer`
      - `Release()`
    - シェーダ側
      - 宣言: `＠❰RW❱StructuredBuffer<｢struct｣> buffer` (`✖❰RW❱`:`SRV`, `❰RW❱`:`UAV`)
      - 使用: `buffer[｢index｣] ＠❰= val❱` (`❰RW❱`だと書き込み可能)
  - **.Append**: UAV
    - C#側:
      - **.ctor**`(Target.Append, int count, int stride)`: `Target.Append`以外`.Structured`と**同じ**
      - `SetData(～)`
      - **SetCounterValue**`(uint counterValue)`: カウンター機能はピクセルとコンピュートで使用可能
        - `uint counterValue`: バッファの**アクセス位置**を指定する
          `R_Device->CreateUnorderedAccessView(., R_Resource pCounterResource, ..)`の`pCounterResource`を設定 (DirectX12メモ.md/G:202 参照)
      - `cmd.`**SetComputeBufferParam**`(ComputeShader computeShader, int kernelIndex, string name, GraphicsBuffer buffer)`: `.Structured`と**同じ**
      - `Release()`
    - シェーダ側: `appendBuffer[appendBuffer.IncrementCounter()] = consumeBuffer[consumeBuffer.DecrementCounter()] ⇔ appendBuffer.Append(consumeBuffer.Consume())`
      - 宣言: `RWStructuredBuffer<｢struct｣> buffer`
        - 使用: `buffer[buffer.IncrementCounter()]`: 後置インクリメント(return counterValue++)
        - 使用: `buffer[buffer.DecrementCounter()]`: 前置デクリメント(return --counterValue)
      - 宣言: `AppendStructuredBuffer<｢struct｣> buffer`
        - 使用: `buffer.Append(｢struct｣ val)`
      - 宣言: `ConsumeStructuredBuffer<｢struct｣> buffer`
        - 使用: `｢struct｣ val = buffer.Consume()`
  - **.Raw**: SRV,UAV
    - C#側:
      - **.ctor**`(Target.Raw, int count, int stride)`:
        - `int stride`: `4Byte単位`で設定する(**4Byteアライメント**)
          **DirectX12API**のビューは`DXGI_FORMAT_R32_TYPELESS`に設定される
      - `SetData(～)`
      - `cmd.`**SetComputeBufferParam**`(ComputeShader computeShader, int kernelIndex, string name, GraphicsBuffer buffer)`: `.Structured`と**同じ**
      - `Release()`
    - シェーダ側
      - 宣言: `＠❰RW❱ByteAddressBuffer buffer` (`✖❰RW❱`:`SRV`, `❰RW❱`:`UAV`)
      - 使用: `uint val = buffer.Load(byteAddress)`: `byteAddress`は`4Byte単位`。`uint`で返るので、`as｢Type｣(val)`で変換して使う
      - 使用(`❰RW❱`のみ): `buffer.Store(byteAddress, uint val)`: `uint val`で受けるので、`asuint(val)`で変換して入れる
  - **.Constant**: CBV
    - C#側:
      - **.ctor**`(Target.Constant, int count, int stride)`:
        - `int count`: **CBuffer数**を指定する    (通常は`Heap ～`の`256BM内`に配置され、`＄BufferSize＝❰count * stride❱`が128MB以上の場合は`CreateCommittedResource(..)`となる)
        - `int stride`: CBufferの**アライメント**が`256Byte`なので`256Byte単位`で設定したほうが良いと思われる
      - `SetData(～)`
      - `cmd.`**SetGlobalConstantBuffer**`(GraphicsBuffer buffer, ⟪string name¦int nameID⟫, int offset, int size)`:
        - `⟪string name¦int nameID⟫`: `cbuffer name{..}`の`name`を指定
        - `int offset`: CBufferの**アライメント**が`256Byte`なので`256Byte単位`で設定しないと**警告**がでる (`SystemInfo.constantBufferOffsetAlignment//=>256`)
        - `int size`: **DirectX12側**が`SetGraphicsRootConstantBufferView(..)`でSetするため、`offset`から`バッファ全体`が設定され、
          `size`の意味が無くなる気がするが、一応`256Byte単位`で設定するのが無難と思われる
        - その他メモ: `違うシェーダーステージ`、同じ`offset`でも問題ない。`offset`を`∫BufferSize∫以上`にするとRenderDocが`No Resource`となる
      - `Release()`
    - シェーダ側
      - 宣言: `cbuffer name{..float4 val;..}`
      - 使用: `val`
  - `.Vertex`:VBV,`.Index`:IBV
    - これを.ctorで手動で生成しても**直接VBV,IBVとして使えるメソッドが無い**。
      (グラフィックスパイプラインで直接メッシュとして使えない。(`cmd.Procedural`で`VBV[IBV[SV_VertexID]]`することはできるが`|= Target.Structured`を付ける必要がある))
    - 恐らく、`class Mesh`で`mesh.⟪vertex¦index⟫BufferTarget |= GraphicsBuffer.Target.Raw`し、
      `GraphicsBuffer Get⟪Vertex¦Index⟫Buffer(＠❰int stream❱)`から取得して、
      **コンピュートで編集する用**だと思われる。(>DirectX11APIでは、`⟪Index¦Vertex⟫バッファ`を`Target.Structured`にできない。DirectX11APIと互換性を保つなら`Target.Raw`を使用)
  - `.Counter`: SRV,UAV
    - `Target.Raw`で**代用**できるし存在している理由がよく分からない
  - `.IndirectArgument`: SRV,UAV
    - めんどくさそうなので触れないでおく。**Draw,Dispatch系の引数しか積めない**
  - `.Copy⟪Source¦Destination⟫`: `cmd.CopyBuffer(GraphicsBuffer source, GraphicsBuffer dest)`で使用 (>GPUによって効率的にコピーされます。)

- プロパティ
  - **.ctor(target, ＠❰usageFlags❱, count, stride)**:
    - `GraphicsBuffer.Target target`(ReadOnly): `.ctor(～)`時に組み合わせ可能。↑の`GraphicsBuffer.Target～`を参照
    - `GraphicsBuffer.UsageFlags usageFlags`(ReadOnly):
      - `enum UsageFlags`: {`None`, `LockBufferForWrite`}: ↓↓の`UsageFlags.LockBufferForWrite`を参照 (**直接バッファに書き込む**)
    - **バッファサイズ** (>バッファのサイズが`SystemInfo.maxGraphicsBufferSize`の値を超えると、`.ctor(～)`は**例外をスロー**します)
      - `int count`(ReadOnly): バッファの要素数
      - `int stride`(ReadOnly): バッファの要素サイズ
  - `string name`(WriteOnly): バッファに名前を付ける。**DirectX12API**では`R_Resource->SetName(String name)`となり**RenderDoc**で確認できる
  - ハンドル
    - `GraphicsBufferHandle bufferHandle`(ReadOnly): **DOTS(Burst(NativeArray))用**の`Blittable型`な**ハンドル**。
      `GraphicsBufferHandle`から`GraphicsBuffer`を**復元するAPI**は意図的に**用意されていない**。(復元したい場合は独自にDictを作る)
      注意:**元のGraphicsBufferが有効**(`GraphicsBuffer.IsValid() == true`)である場合のみ有効なハンドル
    - (メソッド)`IntPtr GetNativeBufferPtr()`: >ターゲットの**グラフィックスAPIのIntPtr**
  - (メソッド)`bool IsValid()`: バッファが有効ならば`true`

- メソッド
  - **Release**`()`: バッファーを解放する
  - バッファへのアクセス
    - **SetData**`(⟪NativeArray<T>¦List<T>¦Array⟫ data ＠❰, int managedBufferStartIndex, int graphicsBufferStartIndex, int count❱)`
      - `graphicsBuffer{(graphics～graphics + count) * sizeof(data[0])} = data[managed..managed + count-1]`: `dataの範囲`をそのまま`オフセット(graphics)`して`graphicsBuffer`に入れる
      - `❰int managedBufferStartIndex, int graphicsBufferStartIndex, int count❱`を省略した場合は、`(0, 0, data.Length)`になると思われる
      - `⟪managed¦graphics⟫BufferStartIndex`と`count`は`sizeof(data[0])`の**単位**
      - `graphicsBuffer`へのアクセスは`graphicsBuffer.stride`の**単位**。`stride`と`sizeof(data[0])`が**整除関係**。`data`と`graphicsBuffer`が`各データ領域`を**超えない**。
        - `graphicsBufferStartIndex * sizeof(data[0])`と`count * sizeof(data[0])`が`.ctor(.., stride)`の`stride`の**倍数**であること
        - `.ctor(.., stride)`の`stride`と`sizeof(data[0])`が同じか**一方が他方の倍数**であること
        - `managedBufferStartIndex + count`が`data.Length`を**超えない**こと
        - `(graphicsBufferStartIndex + count) * sizeof(data[0])`が`∫bufferSize∫`を**超えない**こと
    - **GetData**`(Array data, ＠❰int managedBufferStartIndex, int graphicsBufferStartIndex, int count❱)`
      - `data[managed..managed + count-1] = graphicsBuffer{(graphics～graphics + count) * sizeof(data[0])}`
      - `Array`版しかない。後は`SetData(..)`と同じと思われる
    - **UsageFlags.LockBufferForWrite**: (↑`⟪Set¦Get⟫Data(～)`も併用して使える。(フラグがあるのは後方互換らしい))
      `SetData(～)`の代わりに`LockBufferForWrite<｢struct｣>(～)`～`UnlockBufferAfterWrite(～)`を使い、C#データからバッファに**コピーするのではなく**、**直接バッファに書き込む**。
      内部の**DirectX12API**では、`R_Resource->⟪Map¦Unmap⟫(～)`を使っていると考えられるが、
      直接バッファに書き込めない(`Map(～)`できない?)場合は、Unityが一時的な**C#キャッシュに書き込む場合がある**
      返される`NativeArray<｢struct｣>`の読み取りは、GPUバッファの読み取りを保証していない(**書き込み専用**として使うことが推奨)
      `Map(～)`されたポインタのため**シーケンシャルな書き込み**が推奨される
      - `NativeArray<｢struct｣>` **LockBufferForWrite<｢struct｣>**`(int bufferStartIndex, int count)`:
        返される`NativeArray<｢struct｣>`は`R_Resource->Map(.., out void** ppData)`の`*ppData`だと思われる
        - `int bufferStartIndex, int count`: 多分、`sizeof(｢struct｣)`の単位
      - **UnlockBufferAfterWrite**`(int countWritten)`:
        `R_Resource->Unmap(～)`に対応すると思われる
        `LockBufferForWrite<｢struct｣>(～)`で取得した`NativeArray<｢struct｣>`が**Dispose()される**
        - `int countWritten`: バッファに書き込まれた要素の数
    - **Target.Append**:
      - **SetCounterValue**`(uint counterValue)`: **カウンターを設定**する
      - (static)**CopyCount**`(GraphicsBuffer src, GraphicsBuffer dst, int dstOffsetBytes)`
        :`src`の**カウンター**を`dst`の`dstOffsetBytes`された位置に**コピー**する。(`dst`=`new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, sizeof(uint))`)

- アライメント
  - **アライメントルール**
    - cbufferアライメントルール: (`#pack(cbuffer)`)
      - memberが＄16境界＝❰16Byteアライメント境界❱を**跨ぐ場合**は自動的にパディングが詰められmemberが**_∫16境界∫にアライメント**される。
      - **各データ型**(struct以外)のアライメントは**その型のサイズ**でアライメントされる。
      - CBuffer内の**struct**は、**無条件で∫16境界∫**にアライメントされる。
      - **行列**(float3x4など)は、**行列と各列ベクトル要素**も**無条件で∫16境界∫**にアライメントされる。
      - **配列**は行列と似ていて、**配列と各配列要素**も**無条件で∫16境界∫**にアライメントされる。
      - (全体的に∫16境界∫にアライメントされるのみであって、memberやstructや行列や配列 の**後方**は普通に詰められる(structとかのサイズは特に無い感じ))
    - Structuredアライメントルール: (`#pack(structured)`)
      - **各データ型**(struct以外)のアライメントは**その型のサイズ**でアライメントされる。(**∫16境界∫**のアライメントは**無い**)
    - C#アライメントルール (ShaderLab: `Inspect.Stack(data); Console.WriteLine(sizeof(Data));`)
      - **各データ型**(struct以外)のアライメントは**その型のサイズ**でアライメントされる。
      - `入れ子のstruct`の**アライメント**は、その`入れ子のstruct`の全てのメンバの型の**最大サイズの単位**
      - `struct`の**サイズ**は、`入れ子のstruct`のメンバも含めた全てのメンバの型の**最大サイズの単位**
      - [C#アライメント規則](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUKgZgAIMiBhIgbzyNrtoGdhIBjYIgSSigFMwARAIbBBNenWq5x0ksQBmAGwD2wonGGCCAbiIB6XZgBsYmXUJqlEAEYKeajQBYd+9A5PiAvu/pNW7AMrMEGwQYDxwQiLeEtHS5gwAFkpg7OoiAAzOuumx4uaKKqkamFluUqZmxInJRSLoWQAcufTmXLwCGkQAltx8kYJZRs2VRNUp9iIArFkE6MO0+g455dJeK7JE0AyCcnYkRiQORACygj0AFACU7pKmgX6h4f0TgkQAvES8AO5XWrlcDAADjw2AA6QKCFgAa3OaUElz+6xamAAnOcGF0AF48JRyc73YLAR4RDSXBHuNYeIA)
  - **C#,HLSL(Cbuffer, Structured)のstructのアライメント**: (要するに全て**SIMD単位(16Byte)区切りで作る**)
    - `VectorN`や`｢Type｣N`は、`16Byte境界`を跨がない
    - `Matrix4x4`や`｢Type｣NxN`は、`16Byte境界`で作る (`float4x4`推奨)
    - `structのサイズ`が`16Byte単位`になるようにする
    - 固定長配列(`fixed float val[～]`)は極力使わない。`unsafe`が必要。`1要素16Byte`推奨。
      Structuredなら`型,型,..`と`型[n]`は同じ。(`cbuffer`は`Matrix風`)
    - `入れ子のstruct`は使わない。**2Byte型は使わない**(`half`など)
  - アライメント確認方法:
    - C#: ShaderLab: `Inspect.Stack(data); Console.WriteLine(sizeof(Data));`
    - HLSL: RenderDoc: Bufferのビューア (`#pack(structured)`, `#pack(cbuffer)`)

  ```CSharp
  using System.Runtime.InteropServices;

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  struct Data{..}
  //または
  [StructLayout(LayoutKind.Explicit)]
  struct Data
  {
    [FieldOffset(1)]
    public byte A;
    [FieldOffset(4)]
    public long B;
    [FieldOffset(15)]
    public byte C;
  }
  ```
![GraphicsBufferなど](\..\..\Unityいろいろ\グラフィックス\DirectX12\GraphicsBufferなど.png)

## コード

C#側
```CSharp
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

                for(int i = 0; i < cSharpData.Length; i++)
                {
                    FillStructuredData(ref cSharpData[i]);
                }
                Debug.Log($"Shader.globalRenderPipeline: {Shader.globalRenderPipeline}");//=>RenderGraphTestPipeline
                
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
```
シェーダ側
```C
// #pragma target 5.0
#pragma kernel CSMain

struct StructuredData //42変数
{
    float data0; //4 (4)
    float2 data1; //12 (8)
    int data2; //16 (4)
    float4x4 data3; //80 (64)
    double data4[2]; //96 (16)
    // double data4_0; //88 (8)
    // double data4_1; //96 (8)
    double4 data5; //128 (32)
    int4x4 data6; //192 (64)
};

// ConstantBuffer<StructuredData> p_constantBuffer //まだ ConstantBuffer<～> は使えない
cbuffer _constantBuffer
{
    // StructuredData p_constantBuffer; //コンピュートシェーダではstructを含められない

    float data0; //4 (4)
    float2 data1; //12 (8)
    int data2; //16 (4)
    float4x4 data3; //80 (64)
    double data4[2]; //96 (16)
    // double data4_0; //88 (8)
    // double data4_1; //96 (8)
    double4 data5; //128 (32)
    int4x4 data6; //192 (64)
};

// RWStructuredBuffer<StructuredData> p_structuredBuffer;
StructuredBuffer<StructuredData> p_structuredBuffer; //(SRV) //✖❰RW❱で書き込むとエラー

#define RW
#if(defined(RW))//全て、UAV + Counter(.Append)。 Counter が無い場合は SetCounterValue(0) と同じ挙動
    RWStructuredBuffer<StructuredData> p_append_structuredBuffer;
    RWStructuredBuffer<StructuredData> p_consume_structuredBuffer;
#else
    AppendStructuredBuffer<StructuredData> p_append_structuredBuffer;
    ConsumeStructuredBuffer<StructuredData> p_consume_structuredBuffer;
#endif

ByteAddressBuffer p_rawBuffer;
RWByteAddressBuffer p_rWRawBuffer;

[numthreads(2,1,1)]
void CSMain (uint3 id : SV_DispatchThreadID)
{
    #if(defined(RW))
        int appendIndex = p_append_structuredBuffer.IncrementCounter(); //後置インクリメント(return counterValue++) (Pixel, Compute)
        int consumeIndex = p_consume_structuredBuffer.DecrementCounter(); //前置デクリメント(return --counterValue) (Pixel, Compute)
        p_append_structuredBuffer[appendIndex] = p_consume_structuredBuffer[consumeIndex];
        // StructuredData sd = (StructuredData)0; p_append_structuredBuffer.Append(sd); //.Append(..)は不可
        p_append_structuredBuffer[appendIndex].data1.x = p_structuredBuffer[1].data1.x;
        p_append_structuredBuffer[appendIndex].data1.y = data1.y;
    #else
        p_append_structuredBuffer.Append(p_consume_structuredBuffer.Consume());
        // p_append_structuredBuffer[id.x].data2 = 4; //インデックスアクセスは不可
    #endif

    for(int i = 0; i < 48; i++) p_rWRawBuffer.Store(id.x * 192 + 4 * i, /*asuint*/(p_rawBuffer.Load(id.x * 192 + 4 * i)));
    // uint Load(in int address); void Store(in uint address, in uint value); //uint として Load し、 uint として Store する
        //なので、 Store 時は、 asuint(..) し、 Load 時は、目的の｢Type｣に as｢Type｣(..) する?

    //GetDimensions(..), InterlockedAdd(..)
}
```
