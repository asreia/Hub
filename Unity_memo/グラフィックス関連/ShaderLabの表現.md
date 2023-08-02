# ShaderLabの表現

```shaderlab
『整列のための⏎とか｡｡｡｡とかを無くした(∫LRetInd∫┃は無くせなかった) ==========書き方のスタイルによってその改行やインデントに意味があるか無いかのルールを作る？================
『[スタイルルール] => [マクロ] => ..
『
    [スタイルルール]================
        {⟪¥＄¦¥＃⟫∮SEC∮∮Space∮＝∮LAny∮} => {⟪¥＄¦¥＃⟫∮SEC∮＝∮LAny∮}『∮Space∮∮SEC∮∮Space∮にしようと思ったけど⟪¥＄¦¥＃⟫∮SEC∮で検索したいのでやめた
            ＃Space＝≪⟦～⟧❰ ❱≫
            ＃NoSpace＝≪⟦～⟧❰∫LChar∫∸❰ ❱❱≫
            ＃SEC＝≪∮NoSpace∮∫LToken∫∮NoSpace∮≫『最初と最後の文字はスペース(❰ ❱)以外の文字になる(中間(∫LToken∫)にスペースが含まれることはある)

        ↓⟪¦⟫と【⇒∥┃∠〖⸨⸩〗】
        {『本当に改行やインデントが必要な場合は∫LReturn∫や∫LIndent∫や❰ ❱を使う
            ∮LAny∮○¦Blk⟪¥⟪¦¥【⟫
                ｡∮LAny∮｡
                ｡⟦∫LRetInd∫┃～⟧❰○¦Blk⟪¥¦¦⟪¥┃¦¥∥⟫¦Ø⟫∮LAny∮❱｡
            ○¦Blk⟪¥⟫¦¥】⟫
        }
        ↓
        {
            ∮LAny∮○¦Blk⟪¥⟪¦¥【⟫｡∮LAny∮｡｡⟦～⟧❰○¦Blk⟪¥¦¦⟪¥┃¦¥∥⟫¦Ø⟫∮LAny∮❱｡○¦Blk⟪¥⟫¦¥】¦¥❱¦¥≫⟫
        }
        ↓❰❱と≪≫
        {
            ∮LAny∮⟪¥❰¦¥≪⟫
                ∮LAny∮
                ⟦∫LRetInd∫┃～⟧❰∮LAny∮❱
            ⟪¥❱¦¥≫⟫
        }
        ↓
        {
            ∮LAny∮⟪¦¥❰¦¥≪⟫∮LAny∮
            ⟦∫LRetInd∫┃～⟧❰∮LAny∮❱⟪¦¥❱¦¥≫⟫
        }
    ================

    要るパターン================
        {≪○¦PT⟪Int｡｡｡｡¦Float｡｡｡｡¦Range∮VECTOR2∮¦C}
        とか
        {
            ＃Int_Float_ATT＝≪＠❰[｡Toggle＠❰(｢define ShaderKeyword｣)❱｡]❱⏎
                    ＠❰[Enum＠❰(｡｡⟪｢C#列挙型｣¦｡⟦,┃1～7⟧❰｢ShowEnum｣,⟪～⟫❱｡⟫｡｡)❱]❱⏎
                    ＠❰[KeywordEnum(⟦, ┃1～⟧❰｢Keyword｣❱)]『｢PropertyName｣_｢Keyword｣と
        }
        とかは`｡`や`⏎`が要る。
        ↓ボツ
        見やすさのために行を開けたい場合は、
        {
            aaa
        『
            bbb
        }
        とするかいやどうしようやめた(マクロ定義を⟪❰ ❱¦\n⟫にした)
    ================

    対象言語特化パターン================
        普通のコピペと同じで、
        1.{
            ＃複数行＝≪
                あああ
                いいい
                ううう
            ≫
            {
                ∮複数行∮
            }
        }
        ↓
        {
            {
                あああ
            いいい
            ううう
            }
        }
        となってしまう問題は残る。変数定義位置と変数出現位置のインデント差を2行目移行に足すかどうかの制御が必要？
            足すかどうかのメタ文字を作るとまたややこしくなるので対象言語によって決定される事にしよう
        2.{∮LAny∮＠❰\n❱⟦～⟧❰ ❱『コメント   \n} => {∮LAny∮\n}
        3.{\n⟦～⟧⟪❰ ❱¦\n⟫⟪＃マクロ定義＃⟫⟦～⟧⟪❰ ❱¦\n⟫\n} => {\n}
        他にも↑のように文章やコード系に特化した[スタイルルール]が必要
    ================
    ❰＠❰A❱ ＠❰B❱ ＠❰C❱❱
    ↓
    ⟪⟦＠❰A❱ ⟧⟦＠❰B❱ ⟧⟦＠❰C❱ ⟧⟫
    ↓
    ⟪
        ⟦＠❰A❱ ⟧
        ⟦＠❰B❱ ⟧
        ⟦＠❰C❱ ⟧
    ⟫
    Permutation(置換(`⟦：⟧`))は`⟪⟫`を`❰❱`のように使ったりXorのように使ったりするからやめとくか
』

『==============================================================================================================================================================================
Shader ｢ShaderName｣
{
    Properties
    {
        ⟦∫LRetInd∫┃～⟧❰｡｡∮PROPERTY_ATTRIBUTE∮ ｢PropertyName｣ (｡"｢PropertyShowName｣" ∮PROPERTY_TYPE∮｡) = ∮PROPERTY_LITERAL∮｡｡❱
            ＃PROPERTY_TYPE   ＝≪○¦PT⟪Int｡｡｡｡¦Float｡｡｡｡¦Range∮VECTOR2∮¦Color｡｡｡｡｡｡¦Vector｡｡｡｡｡¦2D｡｡｡｡｡｡｡｡｡｡¦Cube｡｡｡｡｡｡｡¦3D｡｡｡｡｡｡｡｡⟫≫
            ＃PROPERTY_LITERAL＝≪○¦PT⟪∮INT∮¦∮FLOAT∮¦∮FLOAT∮｡｡｡｡｡｡｡｡¦∮VECTOR4∮¦∮VECTOR4∮¦∮TEXTURE∮¦∮TEXTURE∮¦∮TEXTURE∮≫
                ＃INT＝≪⟪int┃～⟫≫
                ＃FLOAT＝≪⟪float┃～⟫≫
                ＃VECTOR2＝≪(∮FLOAT∮,∮FLOAT∮)≫
                ＃VECTOR4＝≪(∮FLOAT∮,∮FLOAT∮,∮FLOAT∮,∮FLOAT∮)≫
                ＃TEXTURE＝≪"⟪white¦black¦gray¦bump⟫" {}≫
            ＃PROPERTY_ATTRIBUTE＝≪○¦PT⟪∮Int_ATT∮¦∮Float_ATT∮¦∮Range_ATT∮¦∮Color_ATT∮¦∮Vector_ATT∮¦∮2D_ATT∮¦∮Cube_ATT∮¦∮3D_ATT∮⟫≫
                ＃Int_ATT＝≪∮ALL_ATT∮∮Int_Float_ATT∮≫
                ＃Float_ATT＝≪∮ALL_ATT∮∮Int_Float_ATT∮＠❰[Gamma]❱≫
                    ＃Int_Float_ATT＝≪＠❰[｡Toggle＠❰(｢define ShaderKeyword｣)❱｡]❱⏎
                                    ＠❰[Enum＠❰(｡｡⟪｢C#列挙型｣¦｡⟦,┃1～7⟧❰｢ShowEnum｣,⟪～⟫❱｡⟫｡｡)❱]❱⏎
                                    ＠❰[KeywordEnum(⟦, ┃1～⟧❰｢Keyword｣❱)]『｢PropertyName｣_｢Keyword｣というShaderKeywordがdefineされる』❱≫
                ＃Range_ATT＝≪∮ALL_ATT∮＠❰[PowerSlider(⟪unsigned float┃～⟫)]『Range^PowerSlider ?』❱≫
                ＃Color_ATT＝≪∮ALL_ATT∮≫
                ＃Vector_ATT＝≪∮ALL_ATT∮＠❰[Gamma]❱≫
                ＃2D_ATT＝≪∮ALL_ATT∮∮2D_Cube_3D_ATT∮≫
                ＃Cube_ATT＝≪∮ALL_ATT∮∮2D_Cube_3D_ATT∮≫
                ＃3D_ATT＝≪∮ALL_ATT∮∮2D_Cube_3D_ATT∮≫
                    ＃2D_Cube_3D_ATT＝≪＠❰[NoScaleOffset]❱＠❰[Normal]❱＠❰[HDR]❱＠❰[PreRenderData]❱≫
                    ＃ALL_ATT＝≪＠❰[HideInspector]❱＠❰[Header(｢ShowString｣)]❱＠❰[｡Space＠❰(⟪～⟫)❱｡]❱≫
    }

    ⟦∫LRetInd∫┃1～⟧❰SubShader
    {
        『SubShaderのTags
        ＠❰Tags
        {
            『↓なくなったっけ？
            ＠❰"Queue" = "⟪Background『1000』¦Geometry『2000(デフォルト)』¦AlphaTest『2450』¦Transparent『3000』¦Overlay『4000』⟫＠❰+⟪～⟫❱"❱
            ＠❰"RenderType" = "⟪Opaque¦Transparent⟫"❱
            『↓なさそう
            ＠❰"DisableBatching" = "⟪True¦False¦LODFading⟫"❱＠❰"ForceNoShadowCasting" = "True"❱＠❰"IgnoreProjector" = "True"❱＠❰"CanUseSpriteAtlas" = "False"❱
            『↓これはある？
            ＠❰"PreviewType" = "⟪Sphere¦Plane¦Skybox⟫"❱
        }❱

        ＠❰LOD ⟪～⟫❱ 

        ∮RENDERING_STATE∮『ここでのRENDERING_STATE定義はこのSubShader内の全てのPassに適用される(Passと設定項目が被ったらそのPassの設定で上書きされる?)

        ⟦∫LRetInd∫┃1～⟧『Passの定義
        ❰
            ⟪『Passの種類
                Pass
                {
                    ∮DEFINE_PASS∮
                }
                ¦✖❰GrabPass{＠❰"｢UniformTextureName｣"❱}❱
                ¦UsePass "｢ShaderPassName｣"

                ＃DEFINE_PASS＝≪『Passの実装
                    ❰『Passの名前とタグ付け
                        『
                        『Passの名前
                        ＠❰Name "｢ShaderPassName｣"❱
                        『
                        『PassのTags
                        ＠❰Tags
                        {
                            『LightModeはPass(｢ShaderPassName｣)のグループみたいなもの?
                            ＠❰"LightMode" = "⟪｢LightModeName｣¦Always『ある?』¦✖❰ForwardBase❱¦✖❰ForwardAdd❱¦ShadowCaster『ある？』¦MotionVectors『ある？』⟫"❱
                        }❱
                    ❱
                    『
                    ❰『GPUへの命令
                        『
                        ∮RENDERING_STATE∮
                        『
                        『シェーダーコード
                        ∮START_SHADER_CODE∮
                            『
                            ∮SHADER_CODE∮
                            『
                        ∮END_SHADER_CODE∮
                            ＃START_SHADER_CODE＝≪｡｡｡○¦SC⟪CG¦HLSL⟫PROGRAM≫
                            ＃END_SHADER_CODE  ＝≪END○¦SC⟪CG¦HLSL⟫≫
                    ❱

                    ＃RENDERING_STATE＝≪『GPUへの固定機能の設定(カリング、AGBAマスク、ADSテスト、ブレンド)
                        『
                        『カリング
                        ＠❰Cull ⟪Back¦Front¦Off⟫❱
                        『
                        『カラーマスク (ZWriteもDのマスクと思うことも出来る)
                        ＠❰ColorMask ⟪RGB¦A¦0¦｢R,G,B,Aの任意の組み合わせ｣⟫❱『教科書に書いてあるやつそのまま写した。良く分からないが、コストが高いらしい
                        『
                        『深度系
                        ＠❰ZWrite ⟪On¦Off⟫❱
                        ＠❰ZTest ⟪Less¦Greater¦⟪L¦G¦Ø¦Not⟫Equal¦Always⟫❱『⟪>¦<¦>=¦<=¦==¦!=⟫∪❰True❱
                        『
                        『Zファイティング回避 (glPolygonOffset関数)
                        ＠❰Offset ❰⟪float┃～⟫『％0Factor』,⟪float┃～⟫『％0Units』❱『Zファイティングが発生した場合、Unitsを⟪1¦-1⟫にしダメだったらFactorも⟪1¦-1⟫にしてみる
                        『
                        『アルファブレンディング Color(RGB(∮BlendOp∮(Src * ∮SrcFactor∮, Dst * ∮DstFactor∮)),A(∮AlphaOp∮(SrcA * ∮SrcFactorA∮, DstA * ∮DstFactorA∮)))
                        ＠❰Blend Off❱『Offしかない?
                        ＠❰Blend ∮SrcFactor∮ ∮DstFactor∮ ＠❰∮SrcFactorA∮ ∮DstFactorA∮❱❱『○¦SD⟪Src¦Dst⟫○¦A＠❰A❱ * ○¦SD⟪Src¦Dst⟫Factor○¦A＠❰A❱ (ブレンド前処理)
                        ＠❰Blend ∮BlendOp∮ ＠❰∮AlphaOp∮❱❱                         『∮○¦A⟪Blend¦Alpha⟫Op∮(｡Src○¦A＠❰A❱ * ∮SrcFactor○¦A＠❰A❱∮, Dst○¦A＠❰A❱ * ∮DstFactor○¦A＠❰A❱∮｡)
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
                    ＃SHADER_CODE＝≪

                    ≫SHADER_CODE_終わり
                ≫『Passの実装_終わり(＃DEFINE_PASS)
            ⟫『Passの種類_終わり(⟪Pass¦✖❰GrabPass❱¦UsePass⟫)
        ❱『Passの定義_終わり(⟦∫LRetInd∫┃1～⟧)
    }❱『SubShader_終わり

    FallBack "｢ShaderPassName｣"

    CustomEditor "｢EditorName｣"
}『Shader_終わり
```
