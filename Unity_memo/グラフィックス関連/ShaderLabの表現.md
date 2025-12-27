# ShaderLabの表現

//☆←検索 //網羅した雛形.shader作る
<!-- ```shaderlab -->

Shader "｢ShaderName｣"『"Legacy Shaders/VertexLit"のように"/"で階層を作れる『`Shader Shader.Find(string ｢ShaderName｣)`で検索
{
    Properties //☆
    {
        ⟦∫LRetInd∫┃～⟧❰｡｡∮PROPERTY_ATTRIBUTE∮ ｢PropertyName｣ (｡"｢PropertyShowName｣" ∮PROPERTY_TYPE∮｡) = ∮PROPERTY_LITERAL∮｡｡❱
            ＃PROPERTY_TYPE   ＝≪○¦PT⟪Int｡｡｡｡¦Float｡｡｡｡¦Range∮VECTOR2∮¦Color｡｡｡｡｡｡¦Vector｡｡｡｡｡¦2D｡｡｡｡｡｡｡｡｡｡¦Cube｡｡｡｡｡｡｡¦3D｡｡｡｡｡｡｡｡⟫≫『スカラー、ベクトル、テクスチャ。(行列が無い)
            ＃PROPERTY_LITERAL＝≪○¦PT⟪∮INT∮¦∮FLOAT∮¦∮FLOAT∮｡｡｡｡｡｡｡｡¦∮VECTOR4∮¦∮VECTOR4∮¦∮TEXTURE∮¦∮TEXTURE∮¦∮TEXTURE∮≫
                ＃INT＝≪⟪int┃～⟫≫
                ＃FLOAT＝≪⟪float┃～⟫≫
                ＃VECTOR2＝≪(∮FLOAT∮,∮FLOAT∮)≫
                ＃VECTOR4＝≪(∮FLOAT∮,∮FLOAT∮,∮FLOAT∮,∮FLOAT∮)≫
                ＃TEXTURE＝≪"⟪white¦black¦gray¦bump⟫" {}≫
            ＃PROPERTY_ATTRIBUTE＝≪○¦PT⟪∮Int_ATT∮¦∮Float_ATT∮¦∮Range_ATT∮¦∮Color_ATT∮¦∮Vector_ATT∮¦∮2D_ATT∮¦∮Cube_ATT∮¦∮3D_ATT∮⟫≫
                ＃Int_ATT＝≪∮ALL_ATT∮∮Int_Float_ATT∮≫
                ＃Float_ATT＝≪∮ALL_ATT∮∮Int_Float_ATT∮＠❰\[Gamma]『`Color Space:Linear`のときリニア変換される』❱≫
                    ＃Int_Float_ATT＝≪＠❰[｡Toggle＠❰(｢define ShaderKeyword｣)❱｡]❱⏎
                                    ＠❰\[Enum＠❰(｡｡⟪｢C#列挙型｣¦｡⟦,┃1～7⟧❰｢ShowEnum｣,⟪～⟫❱｡⟫｡｡)❱]❱⏎
                                    ＠❰[KeywordEnum(⟦, ┃1～⟧❰｢Keyword｣❱)]『｢PropertyName｣_｢Keyword｣(を大文字化)というShaderKeywordがdefineされる』❱≫
                ＃Range_ATT＝≪∮ALL_ATT∮＠❰[PowerSlider(⟪unsigned float┃～⟫)]『Range^PowerSlider ?』❱≫
                ＃Color_ATT＝≪∮ALL_ATT∮＠❰\[HDR]❱≫『✖❰\[HDR]❱の時、常にリニア変換される
                ＃Vector_ATT＝≪∮ALL_ATT∮＠❰\[Gamma]❱≫
                ＃2D_ATT＝＃3D_ATT＝＃Cube_ATT＝≪∮ALL_ATT∮∮2D_3D_Cube_ATT∮≫
                    ＃2D_3D_Cube_ATT＝≪＠❰\[NoScaleOffset]❱＠❰\[Normal]❱＠❰\[PerRendererData]『MaterialPropertyBlock』❱≫
                    ＃ALL_ATT＝≪＠❰\[HideInInspector]❱＠❰\[Header(｢ShowString｣)]❱＠❰\[｡Space＠❰(⟪～⟫)❱｡]❱≫
    }

    『`int shader.subshaderCount`: >このシェーダーの**SubShader数**を返します。
    『`int GetPassCountInSubshader(int subshaderIndex)`: >指定されたSubShaderの**Pass数**を返します。
    ⟦∫LRetInd∫┃1～⟧❰SubShader
    {
        『SubShaderのTags (https://docs.unity3d.com/ja/2023.2/Manual/SL-SubShaderTags.html)
        『`ShaderTagId FindSubshaderTagValue(int subshaderIndex, ShaderTagId tagName)`:
            『`subshaderIndex`の`tagName`を**キー**に、その**バリュー**の**ShaderTagIdを返す**?
        ＠❰Tags //☆
        {
            『ある
            ＠❰"Queue" = "⟪Background『1000』¦Geometry『2000(デフォルト)』¦AlphaTest『2450』¦Transparent『3000』¦Overlay『4000』⟫＠❰+⟪～⟫❱"❱
                『`int ⟪material¦shader⟫.renderQueue`: >シェーダーのレンダーキュー(ReadOnly)
            ＠❰"RenderType" = "⟪Opaque¦Transparent⟫"❱
            『↓なさそう
            ＠❰"DisableBatching" = "⟪True¦False¦LODFading⟫"❱＠❰"ForceNoShadowCasting" = "True"❱＠❰"IgnoreProjector" = "True"❱＠❰"CanUseSpriteAtlas" = "False"❱
            『↓これはある？
            ＠❰"PreviewType" = "⟪Sphere¦Plane¦Skybox⟫"❱
            ＠❰"RenderPipeline" = ｢Name｣❱『`string Shader.globalRenderPipeline`と比較
        }❱

        ＠❰LOD ⟪～⟫❱『`int Shader.globalMaximumLOD`と比較。`int shader.maximumLOD`: `globalMaximumLOD`のLocal版

        ∮RENDERING_STATE∮『ここでのRENDERING_STATE定義はこのSubShader内の全てのPassに適用される(Passと設定項目が被ったらそのPassの設定で上書きされる?)

        CGINCLUDE『SubShader内に記述できる特殊なスペニット([Shader{}にも書けるみたい](https://light11.hatenadiary.com/entry/2018/02/03/141958))
        『ここに記述された文字列は全てのPassの∮SHADER_CODE∮に同じ文字列が書かれたように振る舞うらしいがあまり推奨されて無さそう
        ENDCG

        ⟦∫LRetInd∫┃1～⟧『Passの定義
        ❰
            『`int ⟪material¦shader⟫.passCount`: >アクティブなSubShaderの**Pass数**を返します。
            『`bool SetPass(int passIndex)`: `passIndex`の`Pass`の`Vertex,Fragment Shader`を**GPUにセット**する?
            『`int FindPass(string ｢ShaderPassName｣)`: `｢ShaderPassName｣`から`ShaderPassIndex`を返します。(存在しない場合は、-1)
            『`string GetPassName(int ShaderPassIndex)`: ↑の逆射。`ShaderPassIndex`から`｢ShaderPassName｣`を返します。(存在しない場合は、空文字列)
            ⟪『Passの種類
                Pass //☆
                {
                    ∮DEFINE_PASS∮
                }
                ¦✖❰GrabPass{＠❰"｢UniformTextureName｣"❱}❱『{}が空だとsampler2D _GrabTextureにフレームバッファがコピーされる。今(SRP)では使われない
                ¦UsePass "｢ShaderName｣/｢｢ShaderPassName｣の大文字化｣"『多分、指定したPassがそのまま展開される
            ⟫

                ＃DEFINE_PASS＝≪『Passの実装
                    『`ShaderTagId FindPassTagValue(＠❰int subshaderIndex,❱ int passIndex, ShaderTagId tagName)`:
                        『⟪アクティブなSubShader¦`subshaderIndex`⟫の`passIndex`の`tagName`を**キー**に、その**バリュー**の**ShaderTagIdを返す**?(PassのTags{..})
                    ❰『Passの名前とタグ付け //☆
                        『
                        『Passの名前
                        ＠❰Name "｢ShaderPassName｣"❱
                        『
                        『PassのTags
                        ＠❰Tags
                        {
                            『LightModeはPass(｢ShaderPassName｣)のグループみたいなもの? //追記: Passの種類?
                            ＠❰"LightMode" = "⟪｢LightModeName｣¦Always『ある?』¦✖❰ForwardBase❱¦✖❰ForwardAdd❱¦ShadowCaster『ある？』¦MotionVectors『ある？』⟫⏎
                                                ¦UniversalForward『ForwardLit』¦ShadowCaster『ShadowCaster』¦DepthOnly『DepthOnly』¦Meta『Meta』¦Universal2D『Universal2D』"❱
                                                    『↑教科書4P61(『ForwardLit』は｢ShaderPassName｣)』
                        }❱
                    ❱
                    『
                    ❰『GPUへの命令
                        『
                        ∮RENDERING_STATE∮ //☆
                        『
                        『シェーダーコード //☆
                        ∮START_SHADER_CODE∮
                            『
                            ∮SHADER_CODE∮
                                ＃SHADER_CODE＝≪○¦SC⟪∮CG_SHADER_CODE∮¦∮HLSL_SHADER_CODE∮⟫≫『CG_SHADER_CODEは定義した
                            『
                        ∮END_SHADER_CODE∮
                            ＃START_SHADER_CODE＝≪｡｡｡○¦SC⟪CG¦HLSL⟫PROGRAM≫
                            ＃END_SHADER_CODE  ＝≪END○¦SC⟪CG¦HLSL⟫≫
                                『>HLSLとCGの差は型名などあるのですが、Unityではマクロ等でそれらの差を多く吸収しているため、ユーザーは意識せずに済むようになっています。(教科書4P62)
                    ❱

                    ＃RENDERING_STATE＝≪『GPUへの固定機能の設定(カリング、RGBADSマスク、ADSテスト、ブレンド)
                        『
                        『カリング
                        ＠❰Cull ⟪Back¦Front¦Off⟫❱
                        『
                        『カラーマスク (ZWriteもDのマスクと思うことも出来る)
                        ＠❰ColorMask ⟪RGB¦A¦0¦⟪＃R,G,B,Aの任意の組み合わせ＃⟫⟫❱『教科書に書いてあるやつそのまま写した。良く分からないが、コストが高いらしい
                        『
                        『深度系
                        ＠❰ZWrite ⟪On¦Off⟫❱『これをOnにしないとSV_DEPTHへの書き込みができない?『追記:SV_DEPTHに書くとPreZTestが無効?
                        ＠❰ZTest ⟪Less¦Greater¦⟪L¦G¦Ø¦Not⟫Equal¦Always⟫❱『⟪>¦<¦>=¦<=¦==¦!=⟫∪❰True❱ (Src OP Dst)(デフォルトはLEqual)
                        『
                        『Zファイティング回避 (glPolygonOffset関数) (URPAssetのDepthBias,NormalBiasはシェーダー制御っぽい(教科書4P40))
                        ＠❰Offset ❰⟪float┃～⟫『％0Factor』,⟪float┃～⟫『％0Units』❱『Zファイティングが発生した場合、Unitsを⟪1¦-1⟫にしダメだったらFactorも⟪1¦-1⟫にしてみる
                        『
                        『アルファブレンディング
                        Color(RGBA(∮BlendOp∮(Src * ∮SrcFactor∮, Dst * ∮DstFactor∮)))『この場合⟪Src¦Dst⟫はRGBA (つまり∮⟪Src¦Dst⟫Factor∮はAlpha(A)にも掛かる)
                        Color(RGB(∮BlendOp∮(Src * ∮SrcFactor∮, Dst * ∮DstFactor∮)),A(∮AlphaOp∮(SrcA * ∮SrcFactorA∮, DstA * ∮DstFactorA∮)))『この場合⟪Src¦Dst⟫はRGB
                        ＠❰Blend Off❱『Offしかない?
                        ＠❰Blend ∮SrcFactor∮ ∮DstFactor∮ ＠❰∮SrcFactorA∮ ∮DstFactorA∮❱❱ (ブレンド係数)
                        ＠❰Blend ∮BlendOp∮ ＠❰∮AlphaOp∮❱❱                                  (ブレンド演算)
                            ＃SrcFactor＝＃DstFactor＝＃SrcFactorA＝＃DstFactorA＝≪
                                ⟪
                                    ⟪One¦Zero⟫『⟪1¦0⟫
                                    ¦＠❰OneMinus❱⟪Src¦Dst⟫⟪Color¦Alpha⟫『＠❰1-❱⟪Src¦Dst⟫⟪RGB¦A⟫
                                ⟫
                            ≫
                            ＃BlendOp＝＃AlphaOp＝≪
                                ⟪
                                    Add          『Src＠❰A❱ + Dst＠❰A❱
                                    ¦○¦R＠❰Rev❱Sub『○¦R⟪Src¦Dst⟫＠❰A❱ - ○¦R⟪Dst¦Src⟫＠❰A❱
                                    ¦⟪Min¦Max⟫   『⟪Min¦Max⟫(Src＠❰A❱, Dst＠❰A❱) (要素毎?ノルム?)
                                ⟫
                            ≫
                        『
                        『ステンシル
                        ＠❰Stencil
                        {
                            Ref ⟪0～225⟫『StencilもRefもunsigned byteと思われる
                            ＠❰ReadMask ⟪％255¦⟪0～255⟫⟫❱
                            ＠❰WriteMask ⟪％255¦⟪0～255⟫⟫❱
                            Comp ⟪Greater¦Less¦｡⟪G¦L¦Ø¦Not⟫Equal｡¦Always¦Never⟫ 『❰｡Stencil & ReadMask ⟪>¦<¦>=¦<=¦==¦!=⟫ Ref & ReadMask｡❱∪⟪True¦False⟫
                            ＠❰Pass ∮OP∮❱
                            ＠❰Fail ∮OP∮❱      『if(⟪Pass¦Fail¦ZFail⟫) ∮OP∮
                            ＠❰ZFail ∮OP∮❱『StencilはPassしたがZTestがFailした場合(PreZTestがFailしてもStencilTestは実行される?)
                                ＃OP＝≪⟪    『多分、マスクをIncrで重ねたり、(WriteMask)でマスクを別々に保存したりできる
                                            Replace『(WriteMask)Stencil = (WriteMask)Ref『Refを使ってるのはこれだけ
                                            ¦Keep  『(WriteMask)Stencil = (WriteMask)Stencil
                                            ¦Zero  『(WriteMask)Stencil = 0
                                            ¦Invert『(WriteMask)Stencil = ~(WriteMask)Stencil『↓(WriteMask)するとその範囲になる?(Stencil,255,0の全てに(WriteMask)が付く)
                                            ¦○¦ID⟪Incr¦Decr⟫○¦WS⟪Wrap¦Sat⟫『○¦WS＠❰｡Stencil == ○¦ID⟪255¦0⟫? ○¦ID⟪255¦0⟫ : ｡❱Stencil○¦ID⟪++¦--⟫ 
                                        ⟫≫
                        }❱
                        『
                        『AlphaToMask (GPUの機能である"Alpha-to-coverage"を有効にするらしい)
                        ＠❰AlphaToMask On❱『>アルファテストで破棄された箇所の境界線を滑らかにします。(MSAAのアルファ版?)(教科書1_P43)
                        『
                        『ZClip (>深度クリップモードを固定に設定します。ニアクリップ面よりも近いフラグメントは、正確にアクリップ面に配置され、ファークリップ面よりも遠いフラグメントは正確にファークリップ面に配置されます。)
                        ＠❰ZClip ⟪¦⟫❱
                        『
                        『コンサバティブ (>慎重なラスタライズ(Conservative)とは、覆われる度合い(ピクセル中心点?)にに関係なく、**三角形で部分的に覆われているピクセルをラスタライズ**することです。
                            『>これは、オクルージョンカリング、GPU の衝突検出、可視性検出を行う場合など、確実性が求められる場合に役立ちます。)
                        ＠❰Conservative ⟪¦⟫❱
                        『
                        『セントロイド(これはある？)
                        ＠❰centroid ⟪¦⟫❱
                    ≫『RENDERING_STATE_終わり
                    『
                    ＃CG_SHADER_CODE＝≪
                        ❰『定義部 //☆
                            ∮PreProcess∮
                            ⟦∫LRetInd∫┃～⟧⟪『シェーダーバリアントの条件分岐系(#if)が噛む
                                ∮VariableDefine∮✖❰∮:Semantics∮『多分使えない』❱
                                ¦∮StructDefine∮
                                ¦∮FunctionDefine∮
                                ¦❰#include ｢FilePath｣❱
                            ⟫
                        ❱
                        ❰『マクロ定義部
                            ＃PreProcess＝≪
                                ⟦∫LRetInd∫┃～⟧❰⟪
                                    ❰『プラグマ系 //☆
                                        ❰#pragma ❱⏎『(https://docs.unity3d.com/ja/2023.2/Manual/SL-PragmaDirectives.html)
                                        ⟪
                                            ❰『各シェーダーステージの使用の宣言とその関数名を指定
                                                ⟪vertex『vert』¦⟪hull¦domain⟫¦geometry『target 4.0以降』¦fragment『frag』⟫ ｢ShaderStageFunctionName｣
                                            ❱
                                            ¦⟪『[シェーダーバリアント](https://light11.hatenadiary.com/entry/2019/01/12/232533)(https://www.youtube.com/watch?v=GbqeTM8fxtk)
                                                『｢ShaderKeyword｣は｢DEFINE_WORD｣。❰_❱は｢ShaderKeyword｣なし版。 (∮RENDERING_STATE∮の切り替えは、シェーダーバリアント要らない)
                                                ❰multi_compile ⟦ ┃2～⟧⟪_¦｢ShaderKeyword｣⟫❱
                                                    『(取得: ❰GlobalKeyword[] Shader.＠❰enabled❱GlobalKeywords❱, ❰LocalKeywordSpace ⟪shader.keywordSpace¦material.enableKeywords⟫❱)
                                                    『(設定: `⟪material¦Shader⟫.SetKeyword(ref ⟪Local¦Global⟫Keyword keyword, bool value)`)
                                                    『≪⟦～⟧❰｡｡multi_compile⟪｡⟦¦┃2～⟧⟪_¦｢ShaderKeyword｣⟫｡⟫｡｡❱≫を全て網羅しコンパイルする([MC N個] * [MC N個] *..* [MC N個])
                                                    『multi_compile_fogは、multi_compile _ FOG_EXP FOG_EXP2 FOG_LINEAR と同じ(教科書4P63)他にmulti_compile_instancing(GPU Instancing)
                                                ¦❰shader_feature ⟦ ┃1～⟧⟪_¦｢ShaderKeyword｣⟫❱
                                                    『コンパイル時に∮PROPERTY_ATTRIBUTE∮の❰Toggle❱と❰Enum❱と❰KeywordEnum❱に設定された｢ShaderKeyword｣のみコンパイルする
                                                        『❰Toggle❱と❰Enum❱と❰KeywordEnum❱のPropertyの定義からshader_featureの定義が一意に決まりそうだが、
                                                        『∮SHADER_CODE∮内に記述し、ShaderCompilerに伝える必要があるためにshader_featureが必要と思われる
                                                            『追記:❰Toggle❱と❰Enum❱と❰KeywordEnum❱に関係なくコンパイル時に定義されているかいないかかも(教科書4P63)
                                                    『なぜか、shader_featureだけ⟦ ┃1┃⟧の時❰shader_feature _ ｢ShaderKeyword｣❱になる❰_❱の省略定義がある
                                            ⟫
                                            ¦⟪『シェーダーターゲット (GPUのシェーダー機能のレベル。Unityが独自に定義した値)(この条件を満たすSubShaderが選択される?)
                                                ％❰target 2.0❱『Unityの全プラットフォームでサポート (URPのデフォルト(教科書4P62))
                                                ¦❰target 2.5❱『3.0のサブセット(DX9SM3.0からInterpolator8個まで、LODテクスチャサンプリングを含まない)
                                                ¦❰target 3.0❱『DX9シェーダーモデル3.0相当
                                                ¦❰target 3.5❱『OpenGLES3.0相当(DX10SM4.0からジオメトリシェーダを含まない)。Metalのサポート範囲 (近年(2017/10)のモバイル)
                                                ¦❰target 4.0❱『DX11シェーダーモデル4.0相当
                                                ¦❰target 4.5❱『OpenGLES3.1相当(DX11SM5.0からテセレーションシェーダーを含まない)
                                                ¦❰target 4.6❱『OpenGL4.1相当(DX11SM5.0からコンピュートシェーダーを除く)
                                                ¦❰target 5.0❱『DX11シェーダーモデル5.0相当 (最近ではジオメトリシェーダーはダメでコンピュートシェーダーは動くらしい?)
                                            ⟫
                                            ¦❰『どのレンダラー向けにシェーダーをコンパイルするかを指定(多分スイッチプラットフォームでシェーダーファイルが切り替わる)
                                                ⟪only_renderers¦exclude_renderers⟫❰ ❱『[onlyは指定したレンダラーのみ、excludeは指定したレンダラーを除外]してコンパイル
                                                ⟦ ┃1～⟧⟪『大体 DirectX、OpenGL＠❰ES❱、各プラットフォーム
                                                    ❰d3d9❱『Direct3D 9.
                                                    ¦❰d3d11❱『Direct3D 11/12 (d3d11なのに12もこれ?)
                                                    ¦❰d3d11_9x❱『WSA(Windows ストアアプリ)プラットフォームなどの環境
                                                    ¦❰glcore❱『OpenGL 3.x/4.x.
                                                    ¦❰gles❱『OpenGL ES 2.0.
                                                    ¦❰gles3❱『OpenGL ES 3.x.
                                                    ¦❰metal❱『iOS/Mac Metal.
                                                    ¦❰xbox360❱『Xbox 360.
                                                    ¦❰xboxone❱『Xbox One.
                                                    ¦❰ps4❱『PlayStation4.
                                                    ¦❰psp2❱『PlayStation Vita.
                                                    ¦❰n3ds❱『Nintendo 3DS.
                                                    ¦❰wiiu❱『Nintendo WiiU.
                                                    ¦❰switch❱『Nintendo Switch.
                                                ⟫『シェーダーモデルもあるからtargetレベルと紐付けできない
                                            ❱
                                            ¦❰enable_d3d11_debug_symbols❱『DirectX11をターゲットにした出力のバイナリにデバッグ情報を埋め込みます(教科書P68)
                                            ¦❰hardware_tier_variants❱『DirectX12環境のTierによりShaderKeyword(UNITY_HARDWARE_TIER⟪1～3⟫)が有効になりバリアントコードが生成する(教科書P68)
                                            ¦❰prefer_hlslcc gles❱『Open GL ES(gles)環境でもHLSLccトランスパイラの使用を強制する(教科書4P62)
                                        ⟫
                                    ❱
                                    ¦⟪『マクロ定義系 (マクロ変数は⟪定義¦未定義⟫という状態と❰置換された値❱という状態を持っている)
                                        『[C言語のマクロについてまとめる](https://qiita.com/Yuzu2yan/items/0e7bcf2e8bc1aa1c030b)
                                        『マクロ定数』❰#define ｢DEFINE_WORD｣ ⟪｢置換する文字列｣¦Ø⟫❱
                                        ¦『マクロ関数』❰#define ｢DEFINE_WORD｣(｡⟦, ┃1～⟧｢arg｣｡)  ⟪＃argを使った置換＃⟫❱
                                            『定義: #define DEFINE_WORD(arg) "arg" + "ｧ"『追記: #arg ?
                                            『使用: DEFINE_WORD(KND)
                                            『評価: "KND" + "ｧ" //=>"KNDｧ" となる
                                        ¦『マクロ定義解除』❰#undef ｢DEFINE_WORD｣❱『定義された｢DEFINE_WORD｣を未定義にする(マクロ変数を再定義する時に使われる)
                                        『[❰#❱と❰##❱](http://wisdom.sakura.ne.jp/programming/c/c42.html)
                                            『トークン連結演算子❰##❱ ({∫String&_∫##∫String&_∫} => {∫String&_∫∫String&_∫} と評価されるっぽい)
                                                『❰∫String&_∫❱が隣接する場合は❰##❱がいるが、メンバ参照(❰.｢メンバ｣❱)は要らない
                                                『#define DEFINE_WORD(arg) ＠❰∫String&_∫##❱arg＠❰##∫String&_∫❱＠❰.｢メンバ｣❱ 『(＠❰∫String&_∫##❱argは確認してない)
                                                    『＄String&_＝⟦1～⟧⟪⟪｡⟪char┃a～z⟫¦⟪char┃A～Z⟫｡⟫｡¦｡⟪0～9⟫｡¦｡_⟫ (マクロ引数に使える文字列?)
                                            『文字列化演算子❰#❱ (多分、❰"arg"❱と書くと引数ではなく❰"arg"❱という文字列として解釈されるため)
                                                『{#define DEFINE_WORD(arg) #arg} => {DEFINE_WORD(SKD)} => {"SKD"}
                                    ⟫
                                    ¦⟪『条件分岐系
                                        ⟪#if¦#elif¦#else¦#endif⟫『定義チェックif:#if ＠❰!❱defined(KND), 数値比較if:#if KND > SKD
                                        ¦⟪#ifdef¦#ifndef⟫『後方互換用 (#ifdef KND <=> #if defined(KND), ifndef SKD <=> #if !defined(SKD))
                                    ⟫
                                    ¦❰#include ｢FilePath｣❱『❰#include ｢FilePath｣❱が定義された行に｢FilePath｣の中身を展開する
                                    ¦❰#error ｢ErrorMessage｣❱『これが評価されるとプリプロセスを止め｢ErrorMessage｣を表示させる(条件分岐系の中で使われる)
                                ⟫❱
                            ≫
                            ＃:Semantics＝≪『データをレンダリングパイプラインに流し込む時にGPUに役割(意味)を伝える(https://docs.unity3d.com/ja/2023.2/Manual/SL-ShaderSemantics.html)
                                ❰ : ❱
                                『Interpolator(補間器)(Vertex->Fragmentのラスタライズする時に補間するための器)(使用数を節約するためにパッキングすることもある)
                                    『8      Direct3D9 シェーダーモデル2.0
                                    『10     Direct3D9 シェーダーモデル3.0 (#pragma target 3.0)
                                    『16     OpenGL ES 3.0
                                    『32     Direct3D10 シェーダーモデル4.0 (#pragma target 4.0)
                                ⟪ //☆
                                    ⟪Vertex『主に、TBNP、UV、Colorなど
                                        ⟪『入力(in) (モデルデータ(ジオメトリ)のデータの並びをDirectX12のD_INPUT_LAYOUT_DESCのSemanticNameに対応させる)
                                            『頂点インデックス
                                            SV_VertexID(unit)『VertexTextureFetch(VTF)で使うやつ(テクスチャによる頂点アニメーション)『> uint の必要があります
                                                             『単純に頂点処理の連番でありIndexバッファの内容と同じではない
                                            『位置(P)
                                            ¦POSITION(float4)『位置ベクトル
                                            『接空間(TBN)『位置と合わせるとTBNP(アフィン) ?
                                            ¦NORMAL(float3)『法線ベクトル(Z)
                                            ¦TANGENT(float4)『接線ベクトル(X)(最後の1要素は正しい向きへの係数?(⟪1¦-1⟫)
                                                『座標系の⟪1¦-1⟫はGetOddNegativeScale()(WorldTransformParams.w)みたい教科書4P72)
                                            ¦BINORMAL『従法線ベクトル(Y)(これが無くてもNORMALとTANGENTの外積で作れる)
                                            『頂点カラー
                                            ¦COLOR(float4)
                                            『ブレンドシェイプ用
                                            ¦BLENDINDICES『頂点インデックス
                                            ¦BLENDWEIGHT 『頂点の重み
                                        ⟫
                                        ¦⟪『入出力(inout)
                                            TEXCOORD⟪0～3⟫(float4)『テクスチャUV座標。
                                                『>テクスチャ座標や位置などの任意の高精度のデータを示すために使用されます(任意のデータもこれが使われうる)
                                            ¦PSIZE『ポイントサイズ
                                        ⟫
                                        ¦⟪『出力(out)
                                            SV_POSITION(float4)『VertexでMVP変換された位置ベクトルを、Z除算しクリッピングしてカリングしラスタライズするためにGPUが使う
                                            ¦TEXCOORD⟪4～7⟫(float4)『テクスチャUV座標
                                            ¦COLOR⟪0～1⟫(float4)『頂点カラー
                                            ¦FOG『固定機能で使用する頂点フォグ
                                            ¦TESSFACTOR『テセレーション係数
                                        ⟫
                                    ⟫
                                    ¦⟪Fragment『主に、SV_TARGET、SV_DEPTH
                                        ⟪『入力(in)
                                            TEXCOORD⟪0～7⟫(float4)『テクスチャUV座標
                                            ¦COLOR⟪0～1⟫(float4)『頂点カラー
                                            ¦VFACE(fixed(floatでもいい?))『面がカメラに向いている度合い(float)(視線ベクトルとプリミティブのP->PとP->Pの外積による法線の内積?)
                                            ¦VPOS(UNITY_VPOS_TYPE)『スクリーン座標([0,0]～[RT解像度])。(UNITY_VPOS_TYPEマクロを使って型を合わせるらしい)
                                                『[VPOSと構造体について](https://light11.hatenadiary.com/entry/2018/07/22/205542)
                                        ⟫
                                        ¦⟪『出力(out)
                                            SV_TARGET⟪＠❰0❱¦1¦2¦3¦4¦..⟫(float4)『画面に出力するカラー値(MRT: Position, Normal, Velocity, Specula, Roughness)
                                            ¦SV_DEPTH(float)『深度(Z)。多分設定するとPreZTestが死ぬ(死ぬ必要はあるのか?) (VPOS?+Depthでスクリーンスペース)
                                                『[Depthテスト](https://youtu.be/iqYQvpTndTw?t=611)
                                                『>多くの GPU では、これは深度バッファの最適化をオフ(多分PreZ)にするので、正当な理由なしに Z バッファ値を上書きしないように注意してください。SV_Depth で発生するコストは GPU アーキテクチャによって異なり ますが、全体的にはアルファテストのコスト（HLSL の組み込み void clip(float4 x) { if (any(x < 0)) discard; } 関数を使用）とほぼ同じです(**discardがSV_DEPTHを元のDepth値に戻すよう上書きしPreZを殺す?**)。深度を変更するシェーダは、すべての通常の不透明シェーダの後にレンダ リングします（たとえば、AlphaTest レンダリングキューを使用します）。
                                                『追記: GPT-4o: >discardを使用すると、Early-Z(PreZTest) が無効。SV_DepthGreaterEqual: 初期 Z を無効にせず
                                            『Stencil(byte)『描画マスク(マスクを作ってそこに描画する)
                                        ⟫
                                    ⟫
                                    ¦⟪『良く分からんシステムバリュー
                                        ⟪
                                            SV_PrimitiveID『プリミティブごとの識別子 (fragment ?)
                                            ¦SV_InstanceID『インスタンスごとの識別子 (vertex, fragment ?)
                                            ¦SV_IsFrontFace『プリミティブがカメラに向いていればtrue (fragment ?)
                                            ¦SV_ClipDistance『クリップ距離デーダ (fragment ?)
                                            ¦SV_CullDistance『カリング距離データ (fragment ?)
                                            ¦SV_Coverage『出力カバレッジマスク (fragment ?)
                                            ¦SV_RenderTargetArrayIndex『レンダーターゲットの配列インデックス (fragment ?)
                                            ¦SV_ViewportArrayIndex『ビューポート配列インデックス
                                            ¦SV_SampleIndex『サンプル頻度インデックスデータ
                                        ⟫
                                    ⟫
                                ⟫
                            ≫
                            ＃DataType＝≪
                                ⟪ //☆
                                    ｢StructType｣『構造体
                                    『
                                    ¦＃∮Scalar＝≪⟪『スカラー
                                        float           『32bit浮動少数点
                                        ¦half           『16bit浮動少数点
                                        ¦✖❰fixed❱      『12bit固定少数点(今はほぼ使われないみたい)
                                    ⟫≫
                                    『
                                    ¦＃∮Vector＝≪『ベクトル ↓uniform var <= ∮PROPERTY_TYPE∮ (｢VariableName｣ == ｢PropertyName｣)
                                        ∮Scalar∮⟪1～4⟫『∮Scalar∮4 <= ⟪Vector¦Color⟫ (⟪1～4⟫でも入る?)
                                                        『∮Scalar∮ <= ⟪Float¦Range⟫
                                    ≫
                                    『
                                    ¦＃∮Matrix＝≪『行列
                                        ∮Scalar∮⟪1～4⟫x⟪1～4⟫
                                    ≫
                                    『
                                    ¦⟪『テクスチャー     ↓uniform var <= ∮PROPERTY_TYPE∮ (｢VariableName｣ == ｢PropertyName｣)
                                        Sampler⟪1～3⟫D『Sampler2D <= 2D     (##_ST:xy=Tiling,zw=Offset, ##_TexelSize:xy=1/⟪width¦heigh⟫,zw=⟪width¦heigh⟫)
                                                      『Sampler3D <= 3D
                                        ¦SamplerCUBE  『SamplerCUBE <= Cube
                                    ⟫
                                    『
                                    ¦⟪『真偽値、整数
                                        bool『真偽値
                                        ¦int『32bit整数
                                    ⟫
                                ⟫
                            ≫
                            ❰バリュー系
                                ＃Value＝≪⟪∮Literal∮¦｢VariableName｣¦∮StructRef∮¦SwizzleOp⟫≫
                                ＃SwizzleOp＝≪
                                    ⟪   『⟦1～4⟧の数だけその数の∮Scalar∮⟪1～4⟫の型になる
                                        ｢∮Vector∮の｢VariableName｣｣.⟪｡⟦1～4⟧⟪x¦y¦z¦w⟫｡¦｡⟦1～4⟧⟪r¦g¦b¦a⟫｡⟫
                                        ¦｢∮Matrix∮の｢VariableName｣｣.⟦1～16⟧❰_m⟪0～3⟫⟪0～3⟫❱『16までいける?
                                    ⟫
                                    『↓あってるか分からん
                                    『∮VMSVarS∮ = ⟪∮VMSVarS∮¦∮VectorLit∮¦∮MatrixLit∮⟫ 『"="の右項は左項と要素数が同じか、1つ(1つの場合、全要素にそれを代入)
                                        『＃VMSVarS＝≪⟪∮VectorVar∮¦∮MatrixVar∮¦∮ScalarVar∮¦∮SwizzleOp∮⟫≫
                                        『＃●●＄Type＝●●⟪Vector¦Matrix¦Scalar⟫Var＝｢∮●●○∫Type∫∮の｢VariableName｣｣
                                            『言語表現の違法な使用(面倒くさかった(●●(即時実行)を認めるか審議(●を即時実行にするか?)))
                                ≫
                                ＃Literal＝≪
                                    ⟪
                                        VectorLit＝≪∮Vector∮(｡⟦, ┃1～4⟧⟪float┃～⟫｡)≫『∮Vector∮の⟪1～4⟫と⟦, ┃1～4⟧の数は同じ
                                        MatrixLit＝≪∮Matrix∮(｡⟦, ┃1～16⟧⟪float┃～⟫｡)≫『∮Matrix∮もあるか？
                                        ¦⟪float┃～⟫
                                        ¦⟪int┃～⟫
                                        ¦⟪bool┃～⟫
                                    ⟫
                                ≫
                                ＃StructRef＝≪｢StructVariable｣.｢StructDefineVariable｣≫
                            ❱
                            ❰定義系
                                ＃StructDefine＝≪
                                    struct ｢StructType｣
                                    {
                                        ⟦∫LRetInd∫┃～⟧❰∮VariableDefine∮❱∮:Semantics∮
                                    };
                                ≫
                                ＃FunctionDefine＝≪
                                    ∮DataType∮ ｢FunctionName｣(｡｡⟦, ┃～⟧❰｡＠∮InOut∮ ∮DataType∮ ｢VariableName｣∮:Semantics∮｡❱｡｡)∮:Semantics∮
                                    {
                                        ∮Implement∮
                                    }
                                        ＃InOut＝≪『outが教科書4P83のLitPassFragment関数で使われていたが、構造体で返せば良くない？
                                            ⟪ //☆
                                                in      『入力セマンティクス(引数のデフォルト)
                                                ¦out    『出力セマンティクス(戻り値のデフォルト?)(C#と同じ挙動で参照渡しで変更が呼び出し元に反映されるみたい)
                                                ¦inout  『入出力セマンティクス
                                            ⟫
                                        ≫
                                        ＃Implement＝≪⟪＃ここをド真面目に書くと頭が禿げ上がるのでやめとく＃⟫≫
                                ≫
                                ＃VariableDefine＝≪
                                    ＠❰uniform❱ ∮DataType∮ ｢VariableName｣＠❰ = ∫Express∫❱;『uniformは付けなくてもuniform変数になる(省略できる)
                                        『`int Shader.PropertyToID(string ｢VariableName｣)`でID化
                                        『Has,Get,Set
                                            『Has: bool material.Has～(⟪int nameID¦string name⟫)
                                            『Set: void ⟪material¦Shader⟫.Set＠❰Global❱～(⟪int nameID¦string name⟫, ｢Type｣ value, ..)
                                            『Get: ｢Type｣ ⟪material¦Shader⟫.Get＠❰Global❱～(⟪int nameID¦string name⟫)
                                ≫
                            ❱
                            ✖＄Express＝❰『✖＄..＝❰..❱を作るか?..いや✖＄がそもそも✖❰＄だっけ..
                                ⟪
                                    ∮Value∮
                                    ¦❰｡｡○¦P＠❰(❱｡｢FunctionName｣(⟦, ┃～⟧∫Express∫)｡○¦P＠❰)❱｡｡❱
                                    ¦❰｡｡○¦P＠❰(❱｡∫Express∫ ∮Operator∮ ∫Express∫｡○¦P＠❰)❱｡｡❱
                                        ＃Operator＝≪⟪+¦-¦*¦/⟫≫
                                ⟫
                            ❱
                        ❱
                    ≫CG_SHADER_CODE_終わり
                ≫『Passの実装_終わり(＃DEFINE_PASS)
        ❱『Passの定義_終わり(⟦∫LRetInd∫┃1～⟧)
    }❱『SubShader_終わり

    『多分、全てのSubShaderの実行が失敗した場合、指定した｢ShaderName｣のSubShaderを実行する。Offにすると全てのSubShaderが失敗しても警告も出さずFallBackもしない(描画されない?)
    FallBack ⟪"｢ShaderName｣"¦Off⟫ 『(教科書4P64)

    CustomEditor "｢EditorName｣"『｢EditorName｣はShaderGUIの派生クラス。今はUIElementでエディタ拡張できるはず。(UIElementからMaterial(.mat)のシリアライズ経由でPropertiesの操作かな?)
}『Shader_終わり
<!-- ``` -->

- Category
<!-- ```shaderlab -->
    ⟦∫LRetInd∫┃1～⟧❰Category『ShaderCode内にスコープを設定するために使われる。主に複数のSubShaderに同一の∮RENDERING_STATE∮を適用するため。らしいが、汎用性が低く公式も推奨していないみたい
    {
        ∮RENDERING_STATE∮
        『
        ⟦∫LRetInd∫┃1～⟧❰SubShader
        {
            ∫LAny∫
        }
    }❱
<!-- ``` -->
