# CommandBuffer全extern

- nameIDはグローバルスコープでuniformデータSet系は全てグローバル? uniform変数名,nameIDはShaderファイルを跨いで衝突する?
- Internal_**Draw**『←は全て、BatchRendererGroup(BRG)(UnityのMDI機構?)に置き換えれるらしい。
  - (rRG->drawCommandsでUnity描画パイプラインに差し込まれる?)
  - (｡GPU Instancing(SSBO(unity_DOTSInstanceData <= GraphicsBuffer?))、
    - Job/NativeArray、カメラカリング、SRPBatcherフレンドリー(Meshを固定しInstanceコールする?(SRPBatcherはCbufferとMeshを変える))、
    - Hybrid Renderer V2の基盤｡)
  - ECS\[Hybrid Renderer V2] -> SRP\[｡GPU Resident Drawer -> Batch Render Group -> DOTS Instancing -> \[SRPBatcher, GPU Instancing]｡] かな?
  - (シェーダーはDOTS Instancingをサポートしている必要がある, BatchID毎にInstanceコールしている)[](https://blog.virtualcast.jp/blog/2019/10/batchrenderergroup/)
  - [BRG->DOTS Instancingという機能はSRP BatcherとGPU Instancingの併用を可能にしました。](https://logicalbeat.jp/blog/15417/)
  - [DOTS Hybrid Renderer V2](https://docs.unity3d.com/Packages/com.unity.rendering.hybrid@0.4/manual/index.html)
  - [Entities Graphics](https://docs.unity3d.com/Packages/com.unity.entities.graphics@1.2/manual/overview.html)
    - >実行時に、`Entities Graphics` は `LocalToWorld`、`RenderMesh`、`RenderBounds` コンポーネントを持つすべてのエンティティを処理します。

- よく使うor忘れそうなcmd==後ろにCmdクラスメソッド書く?==
  - Resource操作系
    - Convert**Texture**_Internal
      - Copy**Texture**_Internal
  - ShaderKeyword Set系
    - Set⟪｡Global『GlobalKeyword』｡¦｡⟪Material¦Compute⟫『LocalKeyword』｡⟫**Keyword**
  - uniformデータSet系
    - Set**Global**⟪Float¦Int¦Integer¦Vector¦Color¦Matrix⟫
      - Set**Global**⟪Float¦Vector¦Matrix⟫**Array**＠❰ListImpl❱
      - Set**Global**Texture_Impl
      - Set⟪｡⟪View¦Projection⟫**Matrix**｡¦｡ViewProjection**Matrices**｡⟫
    - Setup**CameraProperties**_Internal
  - レンダリングステートSet系
    - Set**InvertCulling**
    - Set**Viewport**
    - ⟪Enable¦Disable⟫**ScissorRect**
    - SetGlobal**DepthBias**
    - Set**ShadowSamplingMode**
  - レンダーターゲットSet系
    - Set**RenderTarget**
      - Clear**RenderTarget**
  - DrawCall,Blit,Dispatch系 『BRGもそうだが、Graphics.～系関数も独自のDrawCallコマンドを発行している可能性がある
    - Internal_Draw**Renderer**＠❰List❱
      - Internal_Draw**Mesh**
      - Internal_Draw**MeshInstanced**IndirectGraphicsBuffer
    - **Blit**_⟪Texture¦Identifier⟫
  - GPU機能系
    - ⟪Create¦WaitOn⟫**GPUFence**_Internal
  - Unity Utility系
    - Set**ExecutionFlags**
    - Internal_**RequestAsyncReadback**_⟪1～9⟫
      - WaitAll**AsyncReadbackRequests**
    - ⟪Begin¦End⟫**Sample**＠⟪_CustomSampler¦_ProfilerMarker⟫
  - CommandBuffer管理系
    - **name**
    - **Clear**

- Resource操作系
  - Get**TemporaryRT**＠⟪Array¦WithDescriptor⟫  『nameID,descを引数に取りnameIDにRTを割り当てる。(>指定したパラメーターでテンポラリーレンダーテクスチャを作成し、 nameID でのグローバルシェーダープロパティーとして設定します。)
    - Release**TemporaryRT**  『nameIDを引数に取り、そのRTをプール?にReleaseする。明示的Releaseしなくてもフレームの最後には自動的にReleaseされるらしい? NativeArrayのTempのように数フレーム使って無いとGPUのメモリから開放される(プールからもRT削除?)ような気がした(>指定した名前のテンポラリーテクスチャをリリースします。明示的に解放されなかった一時的なテクスチャは、カメラのレンダリングが終了した後、またはGraphics.ExecuteCommandBufferが終了した後に削除されます。)
  - Copy**Buffer**Impl  『CopyBuffer(GraphicsBuffer src, GraphicsBuffer dst) srcとdstのサイズは同じである必要がある (>Unity の CommandBuffer.CopyBuffer は、1 つの GraphicsBuffer の内容を別の GraphicsBuffer にコピーするコマンドです。これは Graphics.CopyBuffer を呼び出すことに相当します)
    - Copy**CounterValue**⟪CC¦GC¦CG¦GG⟫  『srcの追加/消費バッファのカウンタ値(バッファの要素数?)をdst+Offsetにコピーする?(C:ComputeBuffer,G:GraphicsBuffer)(>カウンター値をコピーするコマンドを追加します。)
  - ●Convert**Texture**_Internal  『>異なる形式や寸法のテクスチャ間で変換する効率的な方法を提供します。宛先テクスチャ形式は非圧縮であり、サポートされているRenderTextureFormatに対応している必要があります。
    - ●Copy**Texture**_Internal
  - ●Internal_**GenerateMips**  『確か、MipMapを手動で生成する。(>レンダー テクスチャのミップマップ レベルを手動で再生成します。)
  - ●Internal_**ResolveAntiAliasedSurface**  『引数のMSAAテクスチャをリゾルブする(>アンチエイリアス処理されたレンダー テクスチャを強制的に解決します。)
  - ●**IncrementUpdateCount**  『❰Texture.IncrementUpdateCount()❱(>更新カウンタをインクリメントします。GPU 側からテクスチャを更新する場合、またはカウンタを明示的にインクリメントする場合は、このメソッドを呼び出します。)
  - Internal_⟪Build¦Set＠❰Compute❱⟫**RayTracingAccelerationStructure**  『>RayTracingAccelerationStructure は、GPU RayTracing用のシーン内のジオメトリを表すデータ構造です。
- ShaderKeyword Set系
  - ●Set⟪｡Global『GlobalKeyword』｡¦｡⟪Material¦Compute⟫『LocalKeyword』｡⟫**Keyword**『SetKeyword』『void SetKeyword(⟪｡GlobalKeyword｡¦｡⟪Material¦ComputeShader⟫, LocalKeyword｡⟫, bool)
    - ⟪Enable¦Disable⟫｡⟪Shader¦『EnableKeyword』⟪Global¦Material¦Compute⟫⟫｡**Keyword**  『こっちは使わないかも(>EnableShaderKeyword: ❰グローバル❱シェーダキーワードを有効にするコマンドを追加します。)
- uniformデータSet系
  - ●Set**Global**⟪Float¦Int¦Integer¦Vector¦Color¦Matrix⟫  『ローカルなプロパティは設定されない?ローカルって何?(Propertiesに含めるとローカル?)(>すべてのシェーダーに適用されるグローバルな Float プロパティーを設定します。グローバルプロパティは、シェーダがそれを必要とするが、マテリアルがそれらを定義していない場合に使用されます（例えば、シェーダがPropertiesブロックでそれらを公開していない場合）。)
    - ●Set**Global**⟪Float¦Vector¦Matrix⟫**Array**＠❰ListImpl❱  『❰SetGlobalFloatArray❱ nameIDについてfloat[]を設定する(>すべてのシェーダーに適用されるグローバルな Float 配列プロパティーを設定します)
    - ●Set**Global**⏎  『❰SetGlobalTexture❱ (>すべてのシェーダーに適用されるグローバルな Texture プロパティーを設定します)
        ⟪
            Texture_Impl
            ¦❰Buffer❱Internal
            ¦❰GraphicsBuffer❱Internal
            ¦RayTracingAccelerationStructureInternal
        ⟫
    - ●Set⟪｡⟪View¦Projection⟫**Matrix**｡¦｡ViewProjection**Matrices**｡⟫  『nameIDを受け取る引数はないが、Shader側でUnity定義のuniform変数に入ると思う(>行列を設定するコマンドを追加します。)
  - ●Setup**CameraProperties**_Internal  『ctxにもある。これはGlobalなuniform変数(Unity_Par_Drawとか?)を設定する? 引数:Camera camera, bool stereoSetup,int eye(>カメラ固有のグローバル シェーダー変数のセットアップをスケジュールします。この関数は、ビュー、投影プレーン、クリッピング プレーンのグローバル シェーダ変数を設定します。)
  - Set**Compute**⟪Float¦Int¦⟪Vector¦Matrix⟫＠❰Array❱⟫Param  『>float＠❰ベクトル＠❰配列❱❱の値を設定するために使用できます。例:float4 myArray\[4]
    - Internal_Set**Compute**⏎  『多分、ComputeShaderの色々なSet
        ⟪
            ⟪Floats¦Ints⟫
            ¦⟪
                ⟪Texture¦Buffer¦GraphicsBuffer＠❰Handle❱⟫
                ¦Constant⟪Compute¦Graphics⟫Buffer
            ⟫❰Param❱
            ¦ParamsFromMaterial  『MaterialからComputeShaderカーネルのパラメータを設定します。
        ⟫
  - Internal_Set**RayTracing**⏎  『RayTracingShaderとnameIDを指定し、数値データやバッファを送る
      ⟪
          ⟪
              ⟪＠❰Constant❱⟪Compute¦Graphics⟫Buffer⟫
              ¦GraphicsBuffer❰Handle❱
              ¦⟪Texture¦Float¦Int¦⟪Vector¦Matrix⟫＠❰Array❱⟫
          ⟫❰Param❱
          ¦⟪Floats¦Ints⟫
      ⟫
  - Internal**Set⟪ComputeBuffer¦GraphicsBuffer⟫**⟪＠❰Native❱Data¦CounterValue⟫  『違うかも✖uniformのStructuredbufferにSet?(>名前付き(nameID)の GraphicsBuffer プロパティの値を設定します。)
    - Set**GlobalConstant**⟪GraphicsBuffer¦Buffer⟫Internal  『✖❰RW❱Structuredbuffer(読み取り専用)なuniformに送信? Set⟪ComputeBuffer¦GraphicsBuffer⟫,SetRandomWriteTargetの読み取り専用版?
    - Set**RandomWriteTarget**_⟪Texture¦Buffer¦GraphicsBuffer⟫  『(UAV) FragmentShaderでRenderTextureや～BufferをuniformのようにRWStructuredBufferで取得しBufferに書き込む事ができる?(>Shader Model 4.5レベルのピクセル シェーダのランダム書き込みターゲットを設定します。)
      - Clear**RandomWriteTargets**  『↑のSetによる設定の解除?(追記:単なる初期化?)(>unbind しただけかな)(>Shader Model 4.5レベルのピクセル シェーダのランダム書き込みターゲットをクリアします。)
      - (>DirectX の Unordered Access View（UAV）は、順序指定されていないアクセス リソースのビューです。UAV により、複数のスレッドからの順序指定されていない読み取り/書き込みアクセスが一時的に許可されます。)
- レンダリングステートSet系
  - ●Set**InvertCulling**  『RenderDoc:Cull Modeを反転(Back=>Front)
  - ●Set**Viewport**  『Rectを引数に❰スクリーン空間(\[0,0]～\[RT解像度])❱の範囲へとストレッチするように描画される(DirectXでは深度(Z(0.0～1.0))方向の制限もあるみたいだが)[](https://qiita.com/akurobit/items/1619bc26010441b8008c)[Unityでは常に左下が原点?](https://nekojara.city/unity-screen-viewport)(>レンダーターゲットの変更後のデフォルトでは、ビューポートはレンダーターゲット全体を包含するように設定される。このコマンドを使用すると、レンダリングを指定したピクセル矩形に制限することができます。)
  - ●⟪Enable¦Disable⟫**ScissorRect**  『Rect❰スクリーン空間❱を引数に取り、Rectの外をクリップする?(>画面の領域をレンダリングからクリップすることができます。)
  - ●SetGlobal**DepthBias**  『Zファイティング回避などのためのbias, slopeBias(レンダリングステート?)設定(slopeBiasはクリップ面に対して並行でない勾配の付いたポリゴンに対しての設定) (>グローバル深度バイアスを設定するコマンドを追加します。)
  - ●Set**ShadowSamplingMode**_Impl  『DX11: shadowMap.SampleCmpLevelZero(sampler, shadowCoord.xy, shadowCoord.z)のsamplerの設定?(>「シャドウサンプリングモード設定」コマンドを追加します。シャドウマップ レンダー テクスチャは通常、比較フィルタを使用してサンプリングされるように設定されます。)
  - ●Set**Wireframe**  『(>有効にすると、シーンはワイヤフレームモードでレンダリングされます（GL.wireframeを参照）。これは一部のプラットフォーム（OpenGL ESなど）ではサポートされていません。)
- レンダーターゲットSet系
  - ●Set**RenderTarget**⏎  『多分、高レベルAPIから分岐し↓を呼び出すと思う(>レンダーターゲットを設定します。)
      ⟪
          Single_Internal  『非MRT
          ¦⟪ColorDepth¦Multi⟫⟪_Internal¦Subtarget⟫  『MultiはMRT,SubtargetはdepthSlice
      ⟫
    - ●Clear**RenderTarget**⟪Single¦Multi⟫_Internal  『SetされているRTのRGBADSをClearする
- DrawCall,Blit,Dispatch系
  - ●Internal_**Draw**⏎  『正直、ごしゃごしゃしていてよく分からない。BRGはDraw命令を自作?するし..
      ⟪
        ●**Renderer**＠❰List❱  『Renderer(継承:Component)を描画する。＠❰List❱はctx.CreateRendererListを描画する(RendererListの内部はRenderer(継承:Component)のリスト?)
        ¦○¦M＠❰Multiple❱**Mesh**｡○¦M＠❰es❱  『(Graphics.RenderMesh) DrawMesh は 1 フレーム分のメッシュを描画します。メッシュは、ゲーム オブジェクトの一部であるかのように、ライトの影響を受け、影を投げたり受けたり、プロジェクターの影響を受けることができます。すべてのカメラに対して描画することも、特定のカメラに対してのみ描画することもできます。DrawMesh は、大量のメッシュを描画したいが、ゲーム オブジェクトの作成と管理のオーバーヘッドを望まない状況で使用します。
        ¦❰**MeshInstanced**❱⏎  『DrawMeshのGPUインスタン寝具版。引数:メッシュ,マテリアル,寝具カウント(Material.enableInstancingがtrueであること。ライトとシャドウを受けないらしい)
          ⟪
            Procedural  『引数にmatrix(オブジェクトの変換行列)が無い(>GPUインスタンシングを使用して同じメッシュを複数回描画します。これはGraphics.DrawMeshInstancedIndirect(↓)に似ていますが、インスタンス数がスクリプトからわかっている場合、ComputeBuffer 経由ではなく、このメソッドを使用して直接提供できる点が異なります。)
            ¦Indirect＠❰GraphicsBuffer❱ 『●GPU駆動レンダリング、**Multi Draw Indirect**
          ⟫
        ¦❰**Procedural**❱⏎  『引数:トポロジ,頂点数,寝具数[GPUで頂点番号のみでProceduralにトポロジーを形成し描画する。SV_VertexID,SV_InstanceIDを利用(GraphicsBuffer&ComputeShaderの方がいいらしい?)](https://youtu.be/7tjycAEMJNg?list=PLtjAIRnny3h50fYbMrHgOEtaYtnfUy6qi&t=4164)(>頂点バッファーやインデックス バッファーを使用せずに、GPU 上で描画呼び出しを実行します。)
          ⟪
            Indexed
            ¦＠❰Indexed❱Indirect＠❰GraphicsBuffer❱『>引数を持つバッファ、bufferWithArgsは、与えられたargsOffsetオフセットに4つの整数値（インスタンスごとの頂点数、インスタンス数、開始頂点位置、開始インスタンス位置）を持たなければなりません。
          ⟫
        ¦**OcclusionMesh**  『VRデバイスから提供されるOcclusionMeshを描画しPreZTestで他の描画を阻止する?(>アクティブな VR デバイスによって提供されるオクルージョン メッシュをレンダリングするために使用されます。VR デバイスの可視領域の外側にあるオブジェクトのレンダリングを防ぐには、他のレンダリング メソッドの前にこのメソッドを呼び出します。)
      ⟫
  - ●**Blit**_⟪Texture¦Identifier⟫  『mat,src,dst,srcOffset,srcScale,srcSlice,destSliceを引数に取り、dstにBlitする(>元のテクスチャをシェーダーでレンダリングするテクスチャへコピーします。)
  - Internal_**DispatchCompute**＠❰Indirect＠❰GraphicsBuffer❱❱  『computeShader, kernelIndex, threadGroupsXYZ, ＠❰buffer❱ を引数にComputeShaderをDispatchする?
  - Internal_**DispatchRays**  『DispatchCompute＠❰Camera❱のように引数を渡しDispatchしRayTracingShaderを起動する
    - Set**RayTracingShaderPass**  『多分、RayTracingのShaderPassをSetする?
- GPU機能系
  - ⟪Create¦WaitOn⟫**GPUFence**_Internal  『多分、❰ComputeShaderのDispatchまたは通常の描画❱の完了を同期ポイント(Fence)とし、JobSystemのように繋げられる?(>AsyncComputeキュー上のタスクとGraphicsキュー間の同期を管理するために使用されます。(GraphicsFence.passedでこのコマンドを追加した直後のコマンドの完了を検出?))(cmd.WaitOnAsyncGraphicsFence(GraphicsFence,..)で待つ)
  - Internal_Set❰**SinglePassStereo**❱  『1Passで両目分を1枚に描画し、GPU固定機能で左右の目用に分割し画像を歪める?(つまり偽VR)(>SinglePassStereoは、 NVIDIA Pascal ベースの GPU の新しい同時マルチ投影アーキテクチャを使用して、ジオメトリを1 回だけ描画し、ジオメトリの右目と左目の両方のビューを同時に投影します。[](https://developer.nvidia.com/vrworks/graphics/singlepassstereo))
    - Set**InstanceMultiplier**  『シングルパスステレオと関係あるみたい?(>すべての描画呼び出しのインスタンス数に特定の乗数を乗算するコマンドを追加します。インスタンス乗数の変更は、シングル パス インスタンス レンダリングなどのステレオ レンダリングの最適化に役立ちます。たとえば、乗数を 2 に設定すると、1 つのインスタンスを描画するコマンドは 2 つのインスタンスを描画し、2 つのインスタンスを描画するコマンドは 4 つのインスタンスを描画します。詳細については、「シングル パス インスタンス レンダリング」を参照してください。)
  - Set**LateLatch**Projection｡Matrices  『(>レイトラッチングは、レンダリング開始直前にGPUにカメラとコントローラーのポーズを提供することで、Meta QuestとQuest 2アプリで、カメラとコントローラーの変換のMotion-to-Photon遅延を抑えます。[](https://developer.oculus.com/documentation/unreal/unreal-late-latching/?locale=ja_JP))
    - Mark**LateLatch**Matrix｡ShaderPropertyID  『HMDなどのデバイスから送られるMatrixとnameIDの関連付け?(勘)
    - Unmark**LateLatch**Matrix  『↑の解除?
  - Set**FoveatedRendering**Mode  『(>FoveatedRenderingに使用するモードを設定するコマンドを追加します。このコマンドを使用する前に、SRPは↓を呼び出す必要があります。)
    - Configure**FoveatedRendering**  『IntPtr XRDisplaySubsystem.XRRenderPass.foveatedRenderingInfoを引数に渡している(>FoveatedRenderingを構成するコマンドを追加します。XR レンダリング パスごとにこのメソッドを呼び出す必要があります。)
  - ⟪｡⟪Begin¦End⟫Render｡¦｡NextSub｡⟫**Pass**_Internal  『NativeRenderPass関係。NextSubPassで1つのタイルを複数のPassで描画していく?
- Unity Utility系
  - Set**ExecutionFlags**  『cmdを実行する方法を選択する。None以外にAsyncComputeを選択するとGPU非同期コンピューティングで使用できないcmdが積まれると例外を出すらしい(>コマンドバッファの実行方法の意図を説明するフラグを設定します。実行フラグを none 以外の値に設定すると、意図した実行方法と互換性のないコマンドを発行したときに例外がスローされるようになります。たとえば、非同期コンピューティングでの使用を目的としたコマンド バッファーには、純粋にレンダリングに使用されるコマンドを含めることはできません。このメソッドは空のコマンド バッファに対してのみ呼び出すことができるため、構築直後、またはCommandBuffer.Clearを呼び出した後に呼び出してください。)
    - **ValidateAgainstExecutionFlags**  『ググっても出てこなかったが↑について積まれたcmdを検証する?
  - Internal_**RequestAsyncReadback**_⟪1～9⟫  『GPUからコピーしてきたNativeArrayに対してCallbackを呼ぶ(確か)(>AsyncGPU ReadbackRequestコマンドをCommandBufferに追加します。)
    - WaitAll**AsyncReadbackRequests**    『↑で発行されたRequestを全て終わるまで待つ(確か)
  - Internal_Process**VTFeedback**  『ストリーミング仮想テクスチャリングに関係がある?(Virtual Texturing == VT?)[](https://forum.unity.com/threads/feedback-wanted-streaming-virtual-texturing.849133/page-3)
  - **IssuePlugin**⏎  『全てユーザー定義のネイティブcallbackを呼んでいる(他プログラムとの連携など)
      ⟪
          ❰Event❱Internal
          ¦❰Event❱AndDataInternal＠❰WithFlags❱  『IntPtr callback,int eventID,IntPtr dataを引数に取り、レンダリングスレッドからcallbackを呼んでくれる?(>ユーザー定義のイベントを、カスタム・データを持つネイティブ・コード・プラグインに送信します。)
          ¦Custom❰Blit❱Internal                 『callback,src,dst,..を引数に取り、レンダリングスレッドからcallbackを呼んでBlitする?(>ユーザー定義のブリット・イベントをネイティブ・コード・プラグインに送信します。)
          ¦Custom❰TextureUpdate❱Internal        『callback,targetTexture,userDataを引数に取り、レンダリングスレッドからcallbackを呼んでtargetTextureを描画する。❰IssuePluginCustomTextureUpdateV2❱(>テクスチャ更新イベントをネイティブ コード プラグインに送信します。)
      ⟫
  - Invoke**OnRenderObjectCallbacks**_Internal  『URPがExecuteで呼んでいる?(>MonoBehaviour スクリプトの OnRenderObject コールバックの呼び出しをスケジュールします。カメラがシーンをレンダリングした後、ポストプロセスを追加する前に呼び出す)
  - ●⟪Begin¦End⟫**Sample**＠⟪_CustomSampler¦_ProfilerMarker⟫  『using _ = new ProfilingScope(cmd, sampler)から呼ばれる?
- CommandBuffer管理系
  - ●Init**Buffer**  『.ctor時、nativeリソースを確保し、そのポインタをCommandBuffer.m_Ptrに保持する
    - Release**Buffer**  『Dispose()時、m_Ptrの参照先のnativeリソースを開放し、IntPtr.Zeroを代入
  - ●**name**  『.ctorではなくメンバアクセスで設定する(>このコマンドバッファの名前。これはデバッグに使用できるため、プロファイラーまたはフレーム デバッガーでのコマンド バッファーのアクティビティを簡単に確認できます。名前はレンダリングにはまったく影響しません。)
  - ●**Clear**  『m_Ptrの参照先をClear?(>バッファ内のすべてのコマンドをクリアします。以前に追加されたすべてのコマンドをバッファから削除し、バッファを空にします。)
  - **sizeInBytes**  『m_Ptrの参照先のBuffer(nativeリソース)サイズ? NativeMethod("GetBufferSize")(>このコマンドバッファーのバイトでのサイズ(読み出し専用))

- 生extern
  WaitAllAsyncReadbackRequests
  Internal_RequestAsyncReadback_1
  Internal_RequestAsyncReadback_2
  Internal_RequestAsyncReadback_3
  Internal_RequestAsyncReadback_4
  Internal_RequestAsyncReadback_5
  Internal_RequestAsyncReadback_6
  Internal_RequestAsyncReadback_7
  Internal_RequestAsyncReadback_8
  Internal_RequestAsyncReadback_9
  SetInvertCulling
  ConvertTexture_Internal
  Internal_SetSinglePassStereo
  InitBuffer
  CreateGPUFence_Internal
  WaitOnGPUFence_Internal
  ReleaseBuffer
  SetComputeFloatParam
  SetComputeIntParam
  SetComputeVectorParam
  SetComputeVectorArrayParam
  SetComputeMatrixParam
  SetComputeMatrixArrayParam
  Internal_SetComputeFloats
  Internal_SetComputeInts
  Internal_SetComputeTextureParam
  Internal_SetComputeBufferParam
  Internal_SetComputeGraphicsBufferHandleParam
  Internal_SetComputeGraphicsBufferParam
  Internal_SetComputeConstantComputeBufferParam
  Internal_SetComputeConstantGraphicsBufferParam
  Internal_SetComputeParamsFromMaterial
  Internal_DispatchCompute
  Internal_DispatchComputeIndirect
  Internal_DispatchComputeIndirectGraphicsBuffer
  Internal_SetRayTracingComputeBufferParam
  Internal_SetRayTracingGraphicsBufferParam
  Internal_SetRayTracingGraphicsBufferHandleParam
  Internal_SetRayTracingConstantComputeBufferParam
  Internal_SetRayTracingConstantGraphicsBufferParam
  Internal_SetRayTracingTextureParam
  Internal_SetRayTracingFloatParam
  Internal_SetRayTracingIntParam
  Internal_SetRayTracingVectorParam
  Internal_SetRayTracingVectorArrayParam
  Internal_SetRayTracingMatrixParam
  Internal_SetRayTracingMatrixArrayParam
  Internal_SetRayTracingFloats
  Internal_SetRayTracingInts
  Internal_BuildRayTracingAccelerationStructure
  Internal_SetRayTracingAccelerationStructure
  Internal_SetComputeRayTracingAccelerationStructure
  SetRayTracingShaderPass
  Internal_DispatchRays
  Internal_GenerateMips
  Internal_ResolveAntiAliasedSurface
  CopyCounterValueCC
  CopyCounterValueGC
  CopyCounterValueCG
  CopyCounterValueGG
  name
  sizeInBytes
  Clear
  Internal_DrawMesh
  Internal_DrawMultipleMeshes
  Internal_DrawRenderer
  Internal_DrawRendererList
  Internal_DrawProcedural
  Internal_DrawProceduralIndexed
  Internal_DrawProceduralIndirect
  Internal_DrawProceduralIndexedIndirect
  Internal_DrawProceduralIndirectGraphicsBuffer
  Internal_DrawProceduralIndexedIndirectGraphicsBuffer
  Internal_DrawMeshInstanced
  Internal_DrawMeshInstancedProcedural
  Internal_DrawMeshInstancedIndirect
  Internal_DrawMeshInstancedIndirectGraphicsBuffer
  Internal_DrawOcclusionMesh
  SetRandomWriteTarget_Texture
  SetRandomWriteTarget_Buffer
  SetRandomWriteTarget_GraphicsBuffer
  ClearRandomWriteTargets
  SetViewport
  EnableScissorRect
  DisableScissorRect
  CopyTexture_Internal
  Blit_Texture
  Blit_Identifier
  GetTemporaryRT
  GetTemporaryRTWithDescriptor
  GetTemporaryRTArray
  ReleaseTemporaryRT
  SetGlobalFloat
  SetGlobalInt
  SetGlobalInteger
  SetGlobalVector
  SetGlobalColor
  SetGlobalMatrix
  EnableShaderKeyword
  EnableGlobalKeyword
  EnableMaterialKeyword
  EnableComputeKeyword
  DisableShaderKeyword
  DisableGlobalKeyword
  DisableMaterialKeyword
  DisableComputeKeyword
  SetGlobalKeyword
  SetMaterialKeyword
  SetComputeKeyword
  SetViewMatrix
  SetProjectionMatrix
  SetViewProjectionMatrices
  SetGlobalDepthBias
  SetExecutionFlags
  ValidateAgainstExecutionFlags
  SetGlobalFloatArrayListImpl
  SetGlobalVectorArrayListImpl
  SetGlobalMatrixArrayListImpl
  SetGlobalFloatArray
  SetGlobalVectorArray
  SetGlobalMatrixArray
  SetLateLatchProjectionMatrices
  MarkLateLatchMatrixShaderPropertyID
  UnmarkLateLatchMatrix
  SetGlobalTexture_Impl
  SetGlobalBufferInternal
  SetGlobalGraphicsBufferInternal
  SetGlobalRayTracingAccelerationStructureInternal
  SetShadowSamplingMode_Impl
  IssuePluginEventInternal
  BeginSample
  EndSample
  BeginSample_CustomSampler
  EndSample_CustomSampler
  BeginSample_ProfilerMarker
  EndSample_ProfilerMarker
  IssuePluginEventAndDataInternal
  IssuePluginEventAndDataInternalWithFlags
  IssuePluginCustomBlitInternal
  IssuePluginCustomTextureUpdateInternal
  SetGlobalConstantBufferInternal
  SetGlobalConstantGraphicsBufferInternal
  IncrementUpdateCount
  SetInstanceMultiplier
  SetFoveatedRenderingMode
  SetWireframe
  ConfigureFoveatedRendering
  ClearRenderTargetSingle_Internal
  ClearRenderTargetMulti_Internal
  SetRenderTargetSingle_Internal
  SetRenderTargetColorDepth_Internal
  SetRenderTargetMulti_Internal
  SetRenderTargetColorDepthSubtarget
  SetRenderTargetMultiSubtarget
  Internal_ProcessVTFeedback
  InternalSetComputeBufferNativeData
  InternalSetComputeBufferData
  InternalSetComputeBufferCounterValue
  InternalSetGraphicsBufferNativeData
  InternalSetGraphicsBufferData
  InternalSetGraphicsBufferCounterValue
  CopyBufferImpl
  BeginRenderPass_Internal
  NextSubPass_Internal
  EndRenderPass_Internal
  SetupCameraProperties_Internal
  InvokeOnRenderObjectCallbacks_Internal
