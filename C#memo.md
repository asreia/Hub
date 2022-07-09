# C#まとめ

## 言語表現によるC#の構文構造  
- 定義  
    - 修飾子  
        - アクセス指定子  
            `＄アクセス＝＠⟪public¦protected internal¦internal¦protected¦private protected¦％❰private❱⟫`  
        - 修飾子  
            `＄修飾子＝⟪∫アクセス∫ ＄仮/イ/静/s＝⟪｡｡＄仮/イ/静＝＠⟪｡abstract¦virtual¦override¦＄イ/静＝＠❰static❱｡⟫¦sealed｡｡⟫ ＠❰new❱ ＠❰readonly❱ ＠❰ref❱ ＠❰readonly❱ ＠❰partial❱ ＠❰const❱⟫`  
    - ジェネリック  
        - ジェネリック  
            `＄変性ジェネ＝❰<⟦,┃1～⟧❰｡＠⟪in ¦out ⟫∫Type∫⊃∫Gene∫｡❱>❱`  
            `＄ジェネ＝【∫変性ジェネ∫∠✖⸨＠⟪in ¦out ⟫⸩】`
        - 型制約  
            `＄制約子＝⟪⟪｡unmanaged¦class¦struct¦∫Type∫⊃∫class∫｡⟫¦Delegate¦Enum¦∫Type∫⊃∫Gene∫¦∫Type∫⊃∫interface∫¦new()⟫` //∫Type∫⊃⟪..¦..¦..⟫でまとめる?  
            `＄型制約＝❰⟦⟪ ¦∫LRetInd∫⟫┃～⟧❰｡where ∫Type∫⊃∫Gene∫: ⟦, ┃～⟧∫制約子∫｡❱`  
    - 型とリテラル  
        - 型  
            `＄Type＝⟪∫ReferenceType∫¦∫ValueType∫¦∫Generic∫⟫`  
        - リテラル  
            `＄Lit＝⟪∫LitReferenceType∫¦∫LitValueType∫¦∫LitGeneric∫⟫`  
        - 値型  //型int , リテラル整数  
            - 型  
                `＄ValueType＝⟪∫Num∫¦∫Bool∫¦∫Char∫¦∫Struct∫¦∫Tuple∫¦∫UMPointer∫⟫`  //∫UMPointer∫はアンマネージドポインタ型
            - リテラル   
                `＄LitValueType＝⟪∫LitNum∫¦∫LitBool∫¦∫LitChar∫¦∫LitStruct∫¦∫LitTuple∫¦∫LitUMPointer∫⟫`   
            - 数値型  
                - 型  
                    `＄Num＝⟪∫Integer∫¦∫Float∫¦∫Decimal∫⟫`  
                - リテラル  
                    `＄LitNum＝⟪∫LitInteger∫¦∫LitFloat∫¦∫LitDecimal∫⟫`  
                - 整数型  
                    - 型  
                        `＄Integer＝⟪∫Int∫¦∫UInt∫⟫` 
                    - リテラル  
                        `＄LitInteger＝⟪∫LitInt∫¦∫LitUInt∫⟫` 
                    - 符号付き  
                        - 型  
                            `＄Int＝⟪sbyte¦short¦int¦long¦System.⟪SByte¦Int16¦Int32¦Int64⟫⟫`  
                        - リテラル  
                            `＄LitInt＝❰＠❰-❱⟪～⟫❱`
                    - 符号なし  
                        - 型  
                            `＄UInt＝⟪byte¦ushort¦uint¦ulong¦System.⟪Byte¦UInt16¦UInt32¦UInt64⟫⟫`  
                        - リテラル  
                            `＄LitUInt＝⟪～⟫`  
                - 浮動少数点型  
                    - 型  
                        `＄Float＝⟪float¦double¦System.⟪Single¦Double⟫⟫`  
                    - リテラル  
                        `＄LitFloat＝❰＠❰-❱⟪～⟫＠❰.⟪～⟫❱❱`  
                - 十進数型  
                    - 型  
                        `＄Decimal＝⟪decimal¦System.Decimal⟫`  
                    - リテラル  
                        `＄LitDecimal＝⟪＃十進数の型の数＃⟫`
            - 真理値型  
                - 型  
                    `＄Bool＝⟪bool¦System.Boolean⟫`  
                - リテラル  
                    `＄LitBool＝⟪true¦false⟫`  
            - 文字型  
                - 型  
                    `＄Char＝⟪char¦System.Char⟫`  
                - リテラル  
                    `＄LitChar＝❰'⟪U+┃～0xFFFF⟫'❱`  
            - 列挙型  
                - 型  
                    `∫Enum∫`  
                - 定義  
                    `＄Enum_define＝❰＠∫アクセス∫ enum ＄Enum＝｢name｣｡＠❰ : ∫Type∫⊃∫Integer∫❱{⟦, ┃～⟧❰｡＄e＝｢val_name｣＠❰= ∫LitInteger∫❱｡❱}❱`  
                - リテラル  
                    `＄LitEnum＝❰○∫Enum∫.○∫e∫❱ 『Enumは∫Integer∫と識別子のタプルのようなもの`  
        - 参照型  
            - 型  
                `＄ReferenceType＝⟪∫Object∫¦∫String∫¦∫Array∫¦∫Delegate∫¦∫Class∫¦∫Interface∫¦∫Anonymous∫¦∫MPointer∫⟫`//∫MPointer∫はマネージドポインタ型  
            - リテラル   
                `＄LitReferenceType＝⟪∫LitObject∫¦∫LitString∫¦∫LitArray∫¦∫LitDelegate∫¦∫LitClass∫¦∫LitInterface∫¦∫LitAnonymous∫¦∫LitMPointer∫⟫`  
            - オブジェクト型  
                - 型  
                    `＄Object＝⟪object¦System.Object⟫`  
                - リテラル  
                    `＄LitObject＝∫Lit∫`  //object o = t;できるのでok
            - 文字列型  
                - 型  
                    `＄String＝⟪string¦System.String⟫`  
                - リテラル  
                    `＄LitString＝❰"⟪～⟫⟪U+┃～0x10FFFF⟫"❱`  
            - 配列(`＄Array`)  
                - 型  
                    `＄Array＝⟪∫Array[,]∫¦∫Array[][]∫¦System.Array⟫`  
                - リテラル  
                    `＄LitArray＝⟪∫LitArray[,]∫¦∫LitArray[][]∫⟫`  
                - 配列(`＄Array[,]`)  
                    - 型  
                        `＄Array[,]＝❰∫Type∫[⟦～⟧❰,❱]❱`  
                    - リテラル  
                        `＄LitArray[,]＝⟪new ∫Type∫[＄n＝⟦,┃1～⟧⟪～⟫]｡¦｡new ∫Type∫[○∫n∫]＄recur＝❰｡｡{⟦, ┃～⟧⟪｡∫『∫Type∫の』Lit∫¦∫recur∫｡⟫}｡｡❱⟫`   
                - 配列(`＄Array[][]`)  
                    - 型  
                        `＄Array[][]＝❰∫Type∫⟦1～⟧❰[]❱❱`  
                    - リテラル  
                        `＄LitArray[][]＝❰new ∫Type∫[⟪～⟫]⟦～⟧❰[]❱❱`  
            - デリゲート  
                - 型、定義  
                    `＄Delegate_define＝❰delegate ⟪∫Type∫¦void⟫ ｢name｣｡＠∫変性ジェネ∫(∫仮引数∫)❱`
                    `【∫Delegate_define∫∠＄Delegate＝⸨｢name｣⸩】`  
                - リテラル  
                    `＄LitDelegate＝⟪∫ラムダ式∫¦❰｡∫Method∫∸⟪∫Tructor∫¦∫Prodexer∫⟫｡❱⟫` 
                    - ラムダ式  
                        `＄ラムダ式＝❰⟪｡｢vari｣¦(⟦, ┃～⟧｢vari｣)｡⟫ => ⟪｡∫式∫¦{∫文∫＠❰∫return文∫❱}｡⟫❱`  
        - 複合型  
            `＄Complex＝⟪∫Struct∫¦∫Class∫¦∫Interface∫⟫`  
            `＄Value_type＝❰∫Struct∫❱『IL：値型`
            `＄Reference_type＝⟪∫Class∫¦∫Interface∫⟫『IL：O型`
            `＄LitComplex⟪∫LitStruct∫¦∫LitClass∫¦∫LitInterface∫⟫`
            `＄Complex_define＝⟪∫Struct_define∫¦∫Class_define∫¦∫Interface_define∫⟫`  
            - [値型]構造体 //thisはref Str this?  
                - 型、定義  
                    `＄Struct＝⟪○∫Normal_Struct_name∫｡¦｡○∫Readonly_Struct_name∫｡¦｡○∫Ref_Struct_name∫｡¦｡○∫Readonly_ref_Struct_name∫⟫` 
                    `＄Struct_define＝⟪∫Normal_Struct∫｡¦｡∫Readonly_Struct∫｡¦｡∫Ref_Struct∫｡¦｡∫Readonly_ref_Struct∫⟫`  
                    - 修飾子  
                        `＄Str修飾子＝❰∫修飾子∫⊃❰｡＠∫アクセス∫ ＠❰readonly❱ ＠❰ref❱ ＠❰partial❱｡❱❱`  
                    - メソッド  
                        `＄Struct_method＝【｡｡｡∫Method∫⊃⟪｡｡∫Normal_method∫｡¦｡∫expr_method∫｡¦｡∫Tructor∫⊃⟪cctor¦ctor⟫｡¦｡∫Prodexer∫｡¦｡∫Iterator∫｡¦｡∫Operator∫｡｡⟫⏎`  
                            `∠❰⸨∫仮/イ/静∫⸩∸⟪abstract¦virtual¦override⟫❱｡｡｡】`  
                    - 構造体 //構造体はILでsealed。//structはゴミが無く、レイアウト可能(unionとか)、コピーとbox化に注意する  
                        `＄Normal_Struct＝❰∫Str修飾子∫⊃❰｡＠∫アクセス∫ ＠❰partial❱｡❱ struct ｢name｣＠∫ジェネ∫{`  
                            `＄構造体メンバ＝⟦∫LRetInd∫┃～⟧⟪⏎`  『 構造体、クラスのアクセスレベルは｢name｣へのアクセスレベル(｢name｣は型の名前なので静的)  
                                `∫Field_member∫¦⏎`       『 コンストラクタは静的(インスタンスだけど?)メソッドでCls.Cls();が本来でアクセスがクラスのとメンバの両方を満たすこと(普通の静的メソッドも同じ)  
                                `∫Struct_method∫¦⏎`  //ctorは引数ありでフィールドメンバ全初期化  
                                `∫Complex_define∫⏎`  
                            `⟫⏎`  
                        `}❱`  
                        `【∫Normal_Struct∫∠＄Normal_Struct_name＝⸨｢name｣⸩】`  
                    - readonly構造体 //防衛的コピーの回避((temp=str).func();)  
                        `＄Readonly_Struct＝【∫Normal_Struct∫∠readonly ⸨struct⸩┃⇒∠∫Field_member∫∠✖⸨∫イ/静∫⸩∥∠∫Field_member∫∠readonly ⸨∫Type∫⸩⏎『✖⸨∫イ/静∫⸩はインスタンス`
                            `∥∠∫Struct_method∫∠∫property∫∠✖⸨＠❰＠∫アクセス∫ set;❱⸩`
                            `∥∠∫Struct_method∫∠∫property∫∠✖⸨＠❰｡＠∫アクセス∫ set{⟪＃value(＃1の型)が使える＃⟫}｡❱⸩】『get-onlyだけ許可`  
                        `【∫Readonly_Struct∫∠＄Readonly_Struct_name＝⸨｢name｣⸩】`
                    - ref構造体  
                        `＄Ref_Struct＝【∫Normal_Struct∫∠ref ⸨struct⸩】`『ref構造体はref構造体のメンバかローカルしか置けない(スタックのみ)  
                        `【∫Ref_Struct∫∠＄Ref_Struct_name＝⸨｢name｣⸩】`  
                    - readonly ref構造体  
                        `＄Readonly_ref_Struct＝【∫Readonly_Struct∫∠readonly ref ⸨struct⸩】`  
                        `【∫Readonly_ref_Struct∫∠＄Readonly_ref_Struct_name＝⸨｢name｣⸩】`  
                - リテラル  
                    `＄LitStruct＝⟪new ∫Struct∫(実引数¦default⟫`  //実引数追加
                - タプル  
                    - 型  
                        `＄Tuple＝❰＄recur＝❰(｡｡⟦, ┃2～⟧⟪｡❰∫Type∫ ＠｢『1』vari｣❱¦∫recur∫｡⟫｡｡)❱❱`  
                        `『『1』タプル型は型の中に識別子を持てるが、コンパイル時に消失するため属性で保持している`  
                        `『『1』を省略した場合、タプルの中の値を取得するには｢vari｣.Item⟪1～⟫となる`  
                        `『タプルはSystem.ValueTuple＠❰<⟦, ┃1～8⟧∫Type∫>❱最後は↑の記述で8個以上の時の入れ子のための型引数(1,2,..,7,8,..) == (1,2,..,7,(8,..))`  
                    - リテラル  
                        `＄LitTuple＝❰＄recur＝⟪(｡⟦, ┃2～⟧❰＠❰｢name｣: ❱∫式∫❱∪∫recur∫｡)❱❱`  
            - [参照型]クラス  
                - 型
                    `＄Class＝⟪○∫Normal_Class_name∫¦○∫Abstract_Class_name∫¦○∫Static_Class_name∫¦○∫Sealed_Class_name∫⟫`  
                    `＄Class_define＝⟪∫Normal_Class∫¦∫Abstract_Class∫¦∫Static_Class∫¦∫Sealed_Class∫⟫`  
                    - 修飾子  
                        `＄Cls修飾子＝❰∫修飾子∫⊃❰｡＠∫アクセス∫ ＠⟪abstract¦sealed¦static⟫ ＠❰partial❱｡❱❱`   
                    - クラス  
                        `＄Normal_Class＝❰∫Cls修飾子∫⊃❰｡＠∫アクセス∫ ＠❰partial❱｡❱ class ＄Normal_Class_name＝｢name｣｡＠∫ジェネ∫ ＄継承＝＠❰｡: ＠❰∫Class∫❱＠❰,❱＄実装＝⟦, ┃～⟧❰∫Interface∫❱｡❱{`  
                            `＄クラスメンバ＝❰⟦∫LRetInd∫┃～⟧⟪⏎`  
                                `∫Field_member∫❰｡｡ = ⟪｡∫Lit∫¦⟪＃静的な｢vari｣＃⟫｡⟫｡｡❱¦⏎`  
                                `【∫Method∫∠∫仮/イ/静∫∠✖⸨abstract❱⸩】⏎`
                                `∫Complex_define∫⏎`  
                            `❱❱⏎`  
                        `}❱`  
                    - 抽象クラス  
                        `＄Abstract_Class＝【∫Normal_Class∫∠abstract ⸨class⸩┃∠∫Method∫⊃⸨✖❰abstract❱⸩abstract】`  
                            『インスタンス生成できないのでctorやFinalizeが定義できないと思ったが定義できる謎。  
                        `【∫Abstract_Class∫∠＄Abstract_Class_name＝⸨｢name｣⸩】`  
                    - staticクラス  
                        `＄Static_Class＝【∫Normal_Class∫∠static ⸨class⸩┃∠∫クラスメンバ∫∠⸨∫仮/イ/静∫⸩⊃❰static❱⏎`  
                            `┃∠✖⸨∫継承∫⸩┃∠∫Method∫∠⸨∫Tructor∫⸩∸⟪ctor¦Finalize⟫】`

                        `【∫Static_Class∫∠＄Static_Class_name＝⸨｢name｣⸩】`  
                    - シールドクラス  
                        `＄Sealed_Class＝【∫Normal_Class∫∠sealed ⸨class⸩┃∠✖⸨∫継承∫⸩】`  
                        `【∫Sealed_Class∫∠＄Sealed_Class_name＝⸨｢name｣⸩】`
                - リテラル  
                    `＄LitClass＝⟪new ∫Class∫(∫実引数∫)¦default⟫`  
                - 匿名型  
                    - 型  
                        `＄Anonymous＝❰__Anonymous~❱`  
                    - リテラル  
                        `＄LitAnonymous＝❰new{⟦, ┃1～⟧⟪｢vari｣¦｢vari｣ = ∫Lit∫⟫}❱ 『｢vari｣でアクセスする。メソッド内で完結。new[]{~}と組合せるといい`  
                        `『var ano = ∫LitAnoymous∫というふうにvarしかないのでメッソドの戻り値にできない。あとILコードがToStringとかのメソッド定義ですごい長い`  
                        `『ILで、[1] class '<>f__AnonymousType~のnewobjなので普通に値型も参照型もヒープにコピーされる`
            - [参照型]インターフェース
                - 型
                    `＄Interface＝❰○∫Interface_name∫❱`  
                    `＄Interface_define＝❰∫Normal_Interface∫❱`   
                    - インターフェース 
                        `＄Normal_Interface＝❰∫Cls修飾子∫⊃❰｡＠∫アクセス∫ ＠❰partial❱｡❱ interface ｢name｣｡＠∫変性ジェネ∫ ＠❰:❱ ∫実装∫{`  
                            `＄インターフェースメンバ＝❰⟦∫LRetInd∫┃～⟧⟪⏎`  
                                `∫Field_member∫⊃❰＠∫アクセス∫ ＠⟪static¦const⟫ ∫Type∫ ｢name｣❱❰｡｡ = ⟪｡∫Lit∫¦⟪＃静的な｢vari｣＃⟫｡⟫｡｡❱¦⏎`  
                                `∫Method∫⊃∫Interface_method∫¦『フィールドを持てない代わりに多重継承できる⏎`  
                                `∫Complex_define∫⏎`  
                            `⟫`  
                        `}❱` 
                        `【∫Normal_Interface∫∠＄Interface_name＝⸨｢name｣⸩】`  
                - リテラル  
                    `＄LitInterface＝❰new ❰∫Complex∫∸∫Interface∫❱(∫実引数∫)❱`  
        - ポインタ型  
            - 型  
                `＄Pointer＝⟪∫UMPointer∫¦∫MPointer∫⟫`  
            - リテラル  
                `＄LitPointer＝⟪∫LitUMPointer∫¦∫LitMPointer∫⟫`  
            - [値型]アンマネージポインタ(IL:\*,C#:*)//✖値型?
                - 型  
                    `＄UMPointer＝❰∫Type∫*❱`  
                - リテラル  
                    `＄LitUMPointer＝❰&｢vari｣❱『｢vari｣は∫Type∫型`  
            - [参照型]マネージポインタ(IL:&,C#:ref)//✖参照型?
                - 型  
                    `＄MPointer＝❰ref ∫Type∫❱`  
                - リテラル  
                    `＄LitMPointer＝❰ref ｢vari｣❱『｢vari｣は∫Type∫型`  
        - ジェネリック  
            - 型  
                `＄Generic＝⟪∫GeneType∫¦∫Gene∫⟫`   
            - リテラル  
                `＄LitGeneric＝⟪∫LitGeneType∫¦∫LitGene∫⟫`  
                - ジェネリック付きの型  
                    - 型  
                        `＄GeneType＝❰∫Type∫<∫Type∫>❱`  
                    - リテラル  
                        `＄LitGeneType＝⟪new ∫GeneType∫(∫実引数∫)¦default⟫`
                - ジェネリック型
                    - 型  
                        `＄Gene＝❰｢name｣❱`  
                    - リテラル  
                        `＄LitGene＝∫Lit∫`  
    - メンバ  
        `＄Member＝❰＠❰unsafe❱⟪∫Field_member∫¦∫Method∫⟫❱`  
        - 変数  
            - メンバ変数//structもclassも同じ  
                `＄Field_member＝❰∫修飾子∫⊃❰｡＠∫アクセス∫ ＠∫イ/静∫ ＠❰new❱ ＠⟪∫readonly∫¦∫const∫⟫｡❱ ∫Type∫ ⟦, ┃1～⟧⟪｢vari｣¦｢vari｣ = ∫式∫⟫;❱`
        - メソッド  
            - シグネチャ  
                `＄Signature＝❰｢name｣(⟪＃∫仮引数∫の∫Type∫の列＃⟫)❱`
            - メンバメソッド  
                `＄Method＝⟪∫Normal_method∫｡¦｡∫Instance∫｡¦｡∫Static∫｡¦｡∫part_method∫｡¦｡∫Interface_method∫｡¦｡∫expr_method∫｡¦｡∫Tructor∫⊃⟪∫cctor∫¦∫ctor∫¦∫Finalize∫⟫｡¦⏎`  
                            `｡∫Prodexer∫⊃⟪∫Property∫¦∫Indexer∫⟫｡¦｡∫Iterator∫｡¦｡∫Extension∫｡¦｡∫Operator∫⟫`  
                - メソッド修飾子  
                    `＄Method修飾子＝❰＠∫アクセス∫ ∫仮/イ/静∫⊃＠⟪｡abstract¦virtual¦override¦∫イ/静∫⊃＠❰static❱｡⟫ ＠❰new❱ ＠❰readonly❱ ＠❰partial❱❱`
                - 仮引数 と 戻り値  
                    - 仮引数  
                            `＄通常仮引数＝❰∫Type∫ ｢arg｣❱`  
                            `＄オプション仮引数＝❰∫Type∫ ｢arg｣ = ∫Lit∫❱`  
                            `＄参照渡し＝❰⟪ref¦in¦out⟫ ∫Type∫ ｢arg｣❱`  
                            `＄可変長仮引数＝❰params ∫Type∫⊃∫Array[][]∫ ｢arg｣❱`  
                                `＄仮引数＝⟦, ┃～⟧⟪∫通常仮引数∫¦∫オプション仮引数∫¦参照渡し¦∫可変長仮引数∫⟫`  
                    - 戻り値  
                        `＄戻り値＝⟪∫Type∫¦void⟫`  
                - メンバメソッド実装  
                    - 普通のメソッド  
                        `＄Normal_method＝❰＠∫アクセス∫ ∫仮/イ/静∫ ＠❰readonly❱ ∫戻り値∫ ｢name｣｡＠∫ジェネ∫(∫仮引数∫)＠∫型制約∫⟪｡;¦{⟪＃静的 または インスタンスメソッドの処理＃⟫}｡⟫❱`//文?＠∫型制約∫  
                    - 戻り値があるメソッド  
                        `＄expr_method＝【∫Method∫∠⸨∫戻り値∫⸩⊃∫Type∫┃∠✖⸨{⟪＃静的 または インスタンスメソッドの処理＃⟫}⸩ => ∫式∫;】`//＃静的 または インスタンスメソッドの処理＃文?  
                    - 部分メソッド 
                        `＄part_method＝【∫Method∫∠⸨∫戻り値∫⸩⊃❰void❱┃∠partial ⸨void⸩】`  
                    - インスタンスメソッド  
                        `＄Instance＝【∫Method∫∠✖⸨∫仮/イ/静∫⸩】`  
                        - 仮想メソッド  
                            `＄Virtual_method＝⟪∫Abstract∫¦∫Virtual∫¦∫Override∫⟫`  
                            - 抽象メソッド  
                                `＄Abstract＝【∫Method∫∠⸨＠∫アクセス∫⸩∸❰private❱┃∠⸨∫仮/イ/静∫⸩⊃❰abstract❱】`  
                            - 仮想メソッド  
                                `＄Virtual＝【∫Method∫∠⸨＠∫アクセス∫⸩∸❰private❱┃∠⸨∫仮/イ/静∫⸩⊃❰virtual❱】`   
                            - オーバーライドメソッド  
                                `＄Override＝【∫Method∫∠⸨＠∫アクセス∫⸩∸❰private❱┃∠⸨∫仮/イ/静∫⸩⊃❰override❱】`   
                    - 静的メソッド  
                        `＄Static＝【∫Method∫∠⸨∫仮/イ/静∫⸩⊃❰static❱】`
                    - 静的コンストラクタ と コンストラクタ と デストラクタ 
                        `＄Tructor＝⟪∫cctor∫¦∫ctor∫¦∫Finalize∫⟫`  
                        - 静的コンストラクタ  
                            `＄cctor＝❰static ∫Complex∫⊃⟪∫Struct∫¦∫Class∫¦∫Interface∫⟫(){⟪＃静的メソッドの処理＃⟫}❱`//文?  
                        - コンストラクタ//インスタンスが無くても呼べるインスタンスメソッド?←違うインスタンスを生成して呼んでる,classのアクセスかつメソッドのアクセス//ジェネはクラスに付いてる  
                            `＄ctor＝❰＠∫アクセス∫ ∫Complex∫⊃＄cname＝⟪∫Struct∫¦∫Class∫∸❰∫Abstract_Class_name∫❱⟫✖∫ジェネ∫(∫仮引数∫)＠❰:⟪base¦this⟫(∫実引数∫)❱{⟪＃インスタンスメソッドの処理＃⟫}❱`
                        - デストラクタ　
                            `＄Finalize＝❰~∫Complex∫⊃❰∫Class∫❱(){⟪＃インスタンスメソッドの処理＃⟫}❱`『クラスのみがFinalizeを持てる。  
                    - プロパティ と インデクサ  
                        `＄Prodexer＝⟪Property¦Indexer⟫`  
                        - プロパティ    
                            `＄Property＝❰＠∫アクセス∫ ＠∫仮/イ/静∫ ＠❰readonly❱ ∫『1』Type∫ ｢name｣⟪⏎` //＠❰readonly❱は多分structの防衛的コピーの回避  
                                `{❰＠∫アクセス∫ get;＠❰＠∫アクセス∫ set;❱❱}＠❰= ∫Lit∫;❱¦⏎`  
                                `{＄getSet＝❰｡｡＠❰｡＠∫アクセス∫ get{return ⟪＃『1』の型の値を返す＃⟫}｡❱＠❰｡＠∫アクセス∫ set{⟪＃value(『1』の型)が使える＃⟫}｡❱｡｡❱}⏎`  
                            `⟫❱`  
                        - インデクサー  
                            `＄Indexer＝❰＠∫アクセス∫ ∫仮/イ/静∫∸❰static❱ ＠❰readonly❱ ∫Type∫ this[∫Type∫ ｢index｣] ⟪{∫getSet∫}¦get;set;⟫❱`
                    - イテレーター  
                        - Iter戻り値  
                            `＄Iter戻り値⊃⟪IEnumerable<∫Type∫>¦IEnumerator<∫Type∫>⟫`  
                            `＄yield＝❰yield ⟪break¦return ∫Lit∫⟫❱  『using System.Collections.Generic;`
                                `＄Iterator＝❰＠∫アクセス∫ ∫仮/イ/静∫ ＠❰readonly❱ ∫Iter戻り値∫ ｢name｣｡＠∫ジェネ∫(∫仮引数∫)∫型制約∫⟪｡;¦{⟪＃∫yield∫を含む処理＃⟫}｡⟫❱`  
                    - 拡張メソッド `『static classで、非ジェネリック`  
                        `＄Extension＝❰＠∫アクセス∫ static ∫戻り値∫ ｢name｣｡＠∫ジェネ∫(this ∫Type∫ ｢arg｣, ∫仮引数∫)∫型制約∫{⟪＃静的メソッドの処理＃⟫}❱`  
                            `『＄Extension＝❰＠∫アクセス∫ static ∫戻り値∫ ｢name｣<T>(this ref T ｢arg｣, ∫仮引数∫)where T: struct{⟪＃静的メソッドの処理＃⟫}❱ refはstructのみ』`  
                    - 演算子のオーバーロード//❰readonly❱だめ//後で======================================================================  
                        `＄Operator＝❰❱`  
                        - ユーザー定義型変換  
                    - インターフェースメソッド  
                        `＄Interface_method＝❰⟪｡｡∫Interface_normal_method∫｡¦｡∫Interface_explicit_method∫｡｡⟫｡∸｡∫Interface_exclusion_method∫❱`  
                        - 除外メソッド  
                            `＄Interface_exclusion_method＝⟪∫Extension∫¦∫Tructor∫⊃⟪∫ctor∫¦∫Finalize∫⟫⟫`  
                        - interfaceアクセス  
                            `＄interfaceアクセス＝【∫アクセス∫∠％⸨public⸩┃∠✖⸨％❰private❱⸩private】`  
                            - インターフェース普通のメソッド  
                                `＄Interface_normal_method＝【∫Method∫∠✖⸨∫アクセス∫⸩∫interfaceアクセス∫┃∠✖⸨∫仮/イ/静∫⸩∫仮/イ/静/s∫∸❰override❱｡┃∠✖⸨＠❰readonly❱⸩｡｡❱】`
                            - インターフェース明示的実装  
                                `＄Interface_explicit_method＝【∫method∫∠✖⸨∫アクセス∫⸩┃∠⸨∫仮/イ/静∫⸩⊃＠❰abstract❱┃∠✖⸨＠❰readonly❱⸩┃∠⟪＃定義元のInterface名＃⟫.⸨｢name｣⸩】`
                                `＄Interface_explicit_method＝【∫method∫∠✖⸨∫アクセス∫⸩┃∠⸨∫仮/イ/静∫⸩⊃＠❰abstract❱┃∠✖⸨＠❰readonly❱⸩┃∠⟪＃定義元のInterface名＃⟫.⸨｢name｣⸩】`
    - ローカル  
        - ローカル変数
            `∫Local_variable∫`  
        - ローカルメソッド  
            `＄Local_Method＝❰∫仮/イ/静∫⊃＠❰『1』static❱ ∫戻り値∫ ｢name｣｡＠∫ジェネ∫(∫仮引数∫)∫型制約∫⟪｡;¦{⟪＃静的 または インスタンスメソッドの処理＃⟫}｡⟫❱`  
                `『『1』staticだと静的ローカルメソッドになって静的メンバ変数以外キャプチャしないのでオブジェクトの寿命を伸ばさない`  
    - 名前空間  
        `＄path0＝⟪＃名前空間~∫Complex∫までのパス＃⟫`  
        `＄path1＝⟪＃名前空間のパス＃⟫`  
        `＄Namespace＝❰＄recur＝⟦～⟧❰｡｡⏎`  
        `⟦;∫LReturn∫┃～⟧⟪｡❰using ∫path1∫❱¦❰using static ∫path0∫❱¦❰using ｢name｣ = ∫path0∫❱¦❰extern alias ｢name｣❱｡⟫⏎`  
        `namespace ｢name｣{⏎`  
            `∫recur∫⏎`  
        `}⏎`  
        `｡｡❱❱`  
- 使用(演算、宣言、生成、代入、分岐)  
    - 型  
        - 宣言  
            `＄宣言＝❰∫Type∫ ｢vari｣❱`  
        - キャスト (IL:castclass ∫Type∫)  
            `＄キャスト＝❰(∫Type∫)⟪｢vari｣¦∫Lit∫⟫❱`  
        - is  (IL:isinst ∫Reference_type∫=>obj != null(正確にはcgt.un(`>`)している))
            `＄is＝❰∫Instance∫ is ∫Reference_type∫❱`  
        - as  (IL:isinst ∫Reference_type∫)
            `＄as＝❰∫Instance∫ as ∫Reference_type∫❱`  
        - default()  
            `＄default()＝❰default(∫Type∫)❱`  
        - typeof()  
            `＄typeof()＝❰typeof(∫Type∫)❱`
        - sizeof()  
            `＄sizeof()＝❰sizeof(∫Type∫)❱`
        - nameof()  
            `＄nameof()＝❰nameof(∫Type∫)❱`  
    - 生成  
        - 複合型  
            `＄複合型生成＝❰new ∫Complex∫∸⟪∫Interface∫¦∫Abstract_Class_name∫⟫(∫実引数∫)❱` 
            `＄デリゲート生成＝❰∫Method∫∸⟪∫Tructor∫¦∫Prodexer∫¦∫Operator∫⟫❱`
        - 配列  
            `＄配列生成＝⟪❰｡｡｡new ⟪｡｡∫Type∫[｢index｣]¦＠❰∫Type∫❱[]{⟦, ┃1～⟧∫Lit∫}｡｡⟫｡｡｡❱｡¦｡{⟦, ┃1～⟧∫Lit∫}⟫`  
    - メンバアクセス  
        `＄Instance＝❰｢vari｣❱ = ∫Use_ctor∫;` 
        `＄メンバアクセス＝❰⟪○∫Instance∫¦∫Complex∫¦∫Lit∫⟫.【❰∫Member∫❱∠〖｢name｣〗】`  
    - メソッド  
        - ラムダ式  
            `＄Use_ラムダ式＝⟪○∫ラムダ式変数∫(∫実引数∫)｡¦｡(｡｡(∫Type∫⊃∫Delegate∫)｡(∫ラムダ式∫)｡｡)｡(∫実引数∫)⟫`『キャストして戻り値と引数の型が分かれば名前が無くても呼べる❰((Action)(()=>{}))();❱
            `＄代入ラムダ式＝❰∫Type∫⊃∫Delegate∫ ＄ラムダ式変数＝｢vari｣ = ∫ラムダ式∫❱`『ラムダ式には型情報がないのでターゲットの型を見て型推論する
        - メソッド  
            `＄Use_Normal_method＝❰【∫Normal_method∫∠〖｢name｣〗】(∫実引数∫)❱`  
        - コンストラクター  
            `＄Use_ctor＝❰new 【∫ctor∫∠∫Complex∫⊃〖∫cname∫〗⸩】(∫実引数∫)❱`  
        - 静的コンストラクター  
            `＄Use_cctor＝⟪＃最初の∫メンバアクセス∫時＃⟫`  
        - デストラクター  
            `『＄Use_Finalize＝⟪＃GCのコレクト時＃⟫』『これはマシン依存..`  
        - 拡張メソッド  
            `＄Use_Extension＝❰⟪∫Lit∫¦∫Instance∫⟫.【∫Extension∫∠〖｢name｣〗】(∫実引数∫)❱`  
        - プロパティ  
            `＄Use_Property_get＝❰∫式∫⊃【∫Property∫∠〖｢name｣〗】❱`  
            `＄Use_Property_set＝❰【∫Property∫∠〖｢name｣〗】= ∫式∫❱`  
        - インデクサー  
            `＄Use_Indexer_get＝❰∫式∫⊃∫indexName∫❱`  
            `＄Use_Indexer_set＝❰∫indexName∫ = ∫式∫❱`  
                `＄indexName＝❰｡｡⟪＃∫Indexer∫を定義している【∫Complex∫∠〖｢name｣〗】＃⟫[｢index｣]｡｡❱`
        - イテレーター  
            `＄Use_Iterator＝❰∫iter∫⟪.MoveNext()¦.Current¦.Reset()⟫❱`
                `＄iter＝❰【∫Complex∫∸∫Interface∫∠∫実装∫∠⸨∫Interface∫⸩⊃∫iterType∫┃∠〖new ⸨｢name｣⸩(∫実引数∫)〗】❱`
                `＄iterType＝❰｡IEnumera⟪ble¦tor⟫＠❰<∫Type∫>❱｡❱`
        - 演算子のオーバーロード//後で==================================================================================================================  
            - ユーザー定義型変換   
- 構造化  
    - 式  
        `＄値＝⟪＃戻り値がある0引数のメソッドや∫Lit∫、｢vari｣など＃⟫`  
        `＄関数＝⟪＃戻り値がある1引数以上のメソッドなど＃⟫` 
        `＄式＝⟪＃＄式＝⟪∫値∫¦∫関数∫(⟦, ┃1～⟧∫式∫)⟫のようなもの＃⟫`//∫関数∫∫値∫で関数適用を表現している  
    - 文  
        `＄構文＝⟪＃コンパイラへ指示。修飾子や制御構文、セミコロン、ブロックなど＃⟫`  
        `＄文＝⟪＃＄文＝⟪∫構文∫¦❰｡∫構文∫ ⟦ ┃1～⟧⟪∫式∫¦∫文∫⟫｡❱⟫のようなもの＃⟫`//構文を並べる、構文の入れ子構造、式を構文に与える、を表現している  
        - 埋め込みステートメント  
    - 変数定義  
        `＄変数定義＝⟪∫Field_member∫¦∫Local_variable∫⟫`  
        `＄Local_variable＝❰＠⟪const¦out⟫ ⟪∫Type∫¦var⟫ ⟦, ┃1～⟧❰｡＠❰@❱｢vari｣ ＠❰= ∫式∫❱❱｡❱`//out??  
        `『２つの同名の識別子の片方に"@"を付けてもIL上では同じ識別子として扱われて、キーワードにも付けれてキーワードを識別子にできる。`  
            `『それによって他の言語で書かれた識別子がキーワードと被っても"@"を付ければ使えるようになる("@"は識別子である事をコンパイラに教えている?)`  
        - 代入  
            - 代入  
                `＄Assign＝❰｢vari｣ = ∫式∫❱ 『代入という∫文∫と、代入する∫値∫を返すという∫式∫、という両方の側面がある`  
                - タプル分解代入  
                    `＄TupleAssign＝❰＠❰var❱ ＄recur＝❰(｡｡｡⟦, ┃2～⟧⟪｡｡❰｡＠⟪var¦∫Type∫⟫ ｢vari｣｡❱¦∫recur∫｡｡⟫｡｡｡)❱ = ∫LitTuple∫❱`  
                    `//∫Complex∫のユーザー定義タプル分解代入拡張メソッド`  
                        `//定義//public static void Deconstruct<T1, T2>(this Tuple<T1, T2> x, out T1 item1, out T2 item2){item1 = x.Item1; item2 = x.Item2;}`  
                        `//使用//var tuple = Tuple.Create("abc", 100); var (x, y) = tuple;//https://ufcpp.net/study/csharp/datatype/deconstruction/#arbitrary-types`  
            - 引数渡し  
                `＄実引数＝⟦, ┃～⟧⟪∫式∫¦❰｢vari｣: ∫式∫❱¦❰in ∫式∫❱¦❰out ＠∫Type∫ ｢vari｣❱¦❰ref ｢vari｣❱⟫`  
    - ステートメント  
        - 制御構文  
            - goto  
                `＄Label＝❰｢name｣❱:`  
                `＄goto文＝❰goto ○∫Label∫;❱`  
            - if  
                `＄if文＝❰＄Recur＝❰｡｡if(⟪＃∫LitBool∫を返す∫式∫＃⟫)∫body∫｡＠❰else ∫body∫❱｡｡❱❱`  
                    `＄body＝⟪∫『1』文∫¦{⟪1～⟫∫文∫}⟫｡¦｡∫Recur∫⟫`
                    `『『1』∫変数定義∫は含めいない`  
            - ?:  
                `＄3項演算子＝❰∫式∫⊃❰｡⟪＃∫LitBool∫を返す∫式∫＃⟫?∫式∫:∫式∫｡❱❱`  
            - switch//また後で==================case int n when n < 100: return 2;=========================================== 
                `＄switch文＝❰switch(∫式∫){`  
                    `⟦⟪ ¦∫LRetInd∫⟫┃～⟧❰｡｡case ∫式∫:`  
                        `⟦⟪ ¦∫LRetInd∫⟫┃～⟧∫文∫`  
                        `⟪break;¦∫return文∫¦∫goto文∫⟫`  
                `｡｡❱}❱`  
            - while  
                `＄while文＝❰while(⟪＃∫LitBool∫を返す∫式∫＃⟫)⟪『1』∫文∫¦{⟪1～⟫∫文∫}⟫❱`  
                    `『『1』∫変数定義∫は含めいない`  
            - for  
                `＄for変数＝【❰∫Local_variable∫⊃❰⟪∫Type∫¦var⟫ ⟦, ┃～⟧❰｢vari｣ = ∫式∫❱❱∠＠⸨∫Type∫¦var⸩】『外で定義してあればできる`  
                `＄for文＝❰for(｡｡∫for変数∫;＠⟪＃∫LitBool∫を返す∫式∫＃⟫;⟪＃∫for変数∫をカウントする∫式∫＃⟫｡｡)❱{⟪1～⟫∫文∫}`  
            - foreach  
                `＄foreach変数＝❰∫Local_variable∫⊃❰『1』⟪∫Type∫¦var⟫ ｢vari｣❱❱ 『『1』が必ず必要`  
                `＄foreach文＝❰(『1』∫foreach変数∫ in ⟪＃Enumera⟪ble¦tor⟫のインスタンス＃⟫)⟪『2』∫文∫¦{⟪1～⟫∫文∫}⟫❱『『1』はreadonly的`  
                    `『『2』∫変数定義∫は含めいない`  
        - 例外処理  
            `＄Exception_handling＝❰＄Recur＝❰｡｡｡｡⏎`  
            `try{`  
                `∫Recur∫`  
            `}`  
            `⟦～⟧❰｡｡｡catch＠❰｡｡(∫Type∫⊃⟪＃Exceptionを継承したクラス＃⟫ ｢vari｣) ＠❰｡when(⟪＃∫LitBool∫を返す∫式∫＃⟫)｡❱｡｡❱{`  
                `⟪＃例外処理＃⟫`  
            `}`  
            `｡｡｡❱＠❰｡finally{`  
                `⟪＃リソース廃棄＃⟫`  
            `}｡❱｡｡｡｡❱❱`  
        - usingステートメント  
            `＄using_statement＝❰using(∫Type∫⊃⟪＃IDisposableを継承した∫Type∫＃⟫ ＄Resource＝｢vari｣ = ⟪＃nullかIDisposableのインスタンスを返す∫式∫＃⟫){`  
                `⟪＃○∫Resource∫を使う処理＃⟫`  
            `}❱`  
                `『ブロック{}を抜けるとDisposeが呼ばれる`  
            - using変数宣言  
                `＄using変数宣言＝❰using ∫Type∫⊃⟪＃IDisposableを継承した∫Type∫＃⟫ ｢vari｣;❱`  
                    `『｢vari｣のブロック{}の範囲はスコープ全体になるので注意`  
        - 戻り値  
            `＄return文＝❰return ∫式∫;❱`  

## SharpLab
  - クラス  
      - 初期化の実行順  
          [Init.cs](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUFAQwFsBTAZwAcCBjEgAgEkoBLYAbzs7oHpvAQf4BKJahDABeQA7kAZQBiAWgB8UgMKKG8hYGoiISLCa1ywI7keLnVQBmM+jpSIFEmDYnTXVJgBsdZlGB0yUCCI6MTp0AG5eAFYQW3swQE10wBC3QFWGQGKGQB+GQGuGQEmGAApZAEpAXCVAfHNANGVnFy8fOgCgkPMI7gB2GLsHNKy89WLyypc3T1iHXIKnXCqqtwBOXIASACJo4YSUjJzSsoWCsP7Js0xZxfc2uKTAZoZAZ4ZAToZACYZAS4ZAeoZAfoZt3YnJgF8910sV0fG+1MM3mC1aK06G3Kbx+U0OoIAHCA3AA6ADyACMAFbCYBXO5PZ6AHxVAM4MgC/FQDqDPkNCo1BoGMoCjCPkCDkcFtNTg58Q8Xsz9t8WXRBcDLBgbBAMVzHLCzGKPNVfP5AsE6AAGJqYNoYpKQvKFTay7y+OqqzBNczavW5HqGoXAhW2DEA2UO9lap269bZTb81ls0HobUXG68147PCuri8J08wmAawZAB9mgFkGQD6DIARBkAMgybQC0cvGiYBPBkAVgzpwB2DIBzBkAygyVpOAeQYo5wQYtLU69b6I/bJk2FgAWaWlwBeboAQHT9VRFXb+kpduAbAcWmDV2rjfI7Av648bYusABkCNBqAALQFd9DoY+s3gAfVVTpRyqIYR43FkihUgHsGR73a7xoqAQIZAGYMgBFDI8gDdDNcgCaDIA0QyABYMyaAPnagAyEYAqgyADEMkGzt2i5OoOI6rv6UAkAA7hKzp4WO654J8QA==)
      - アクセスの仕方とnew
          [アクセスの仕方とnew](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyDMABGgQEIDe+BAbmAE7ACuAhhEQCwED2JAYgBQBKMgF8ihZB16CRYougBs7AgGcpQ4cJyViAEQIhSFQpyoBTWrTAxTS7v3WyAdqYDuStTMrO3yBUtX2Ipq44qgEOnoGOkZcZhZWNhJcHqJeru6BqYTecopJAdIaWqEEAMIEZARVBAD0NYCrSoAhboBWDIDR6gZkJuaW1gSASYQETABGysC0TADGwFA09MwQooCa6S2tUMuARamADqYdOQPNgOoMwdWySQCyghU4x8d1gCUMgM8MgJ0MgP0MT4B2DMttgNYMgF/qgKYMgHkGCYsVhgRyjJiOCamK7XKo5HSCAB0agA3LC4Xw+CQBAjBAIUfZ0dg4VUsTiEYiBAS0RjrnSbjUHi93k1vv8gSDZsACGCIVCYSTSXiCXZBMTSQRybj0lTRbShZjsTK3BF8UixQIJaSGdU7k9Xo9ALBygEDIwBADBtNgRgRAILqqnVAGjkgAdyEVIgpawCO5Pbwu6FZKIn6iT6SEHxXTgsIgA)
      - 動的な型、静的な型、アクセス元、アクセス先  
          [ILとメソッドの種類](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABALLHAAWA9jANJi4wDeOtQbWQBmWvgBGpYACdqwYaloBlAK7liM2v2xC9wscnQA2YQBYVACgCUvAL76A9I8Ca6YBC3QIcMgXoZAwwyBJhn1A/RBhEwEg0XNaAEkbeyCnR0AShkBnhkBOhkB+hnTvfwTaEO5pfFwacMDIyWk5KgVkCwBBGwBuPWdAXqNAQxjLQDu5QGeDaxyAvJDcYgB3UghWBUrZeVoANzAZYFV8CFpC4GLS3QixJZW1jbraADU4hyFnPsG8oJHxyenF5dX1zdwikuIy2js/s5RhMpsBAPYMgCxNQBgSoBABkAICqAfFdAAhGgHvlQC/AYArBkA9QyAS4Y0oBrBkAAHKAEwZAFoMxMAQgyAaIYztsJBBiPjAA2mgHUGQAiDLdfH5AB4MgDsGQC7DKkMoBf+MABUqATQYqYBouUABgyACwZALgGgDEGAB0Kr+kTQKlUEhAag0Mh0CUcACpaZIGSbaAB9WkABVkTRNjnutBuvUGfMFKQyvNxaV5gAiGJUYwBBDLzLO58YAs7UAfgyAYwZAGYMbKp1j+ekirAWmhkYBgdFOjVsVzdjkAVQyAH4ZAAsMFZxST8PX6dxCh3eGy2O1+e0EAO7pcAIP+DXmAOi9AHoZAF5AFfkgAJ/wAO5MDnsBAI7ktDns2qy9Xs9bxxXHZ+S8Ahv8AMsPRmMgGvyQGOfvuQbj2eAaiIh2PH4Bgoln56XS+v/cANoqAACKvKpJk2Rcg+z5ciOo7vrOh5zguoIrmuUhzDUyHbm8u6fN8NBHj+fbOP2IFZGk95PoBwHpKRgywfueEEa0N6UZy/gQZRJFgf4sEIU8SFbuu8grjuHz0cQ36/p05GPixHpcrBiEvIJNSHq8RyiV82wHox1w3qxfjsUB+kKXx0yqSJ7aaZ2EmETegBQcoAH2bSbJtEfoeFk4VpDFCIAcAxZjmeZdkxkH+NBsGAHPE156I+d7ycekW2dFHHUVxfgRVFQSPpxZFxQlTEJDJRlydxH55bprq0I+Um5Rl9yPsZ8W1Xkj6ObRjWJUEOmCM4ehdaWBF2EAA=)
          [仮想メソッド](https://drive.google.com/file/d/1OIWXOpBnGQMefxL4qLLCwGlYo_isn-Xz/view?usp=sharing)
          [仮想関数テーブル(Vtable)](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUKgBgAIBhIgXiNUwDoSB7KAZ3oBsBTAbjzw1IIDeeIiKoBmIgDcAlmGAQAhqyoAWIgDUAYgQAUASgEkaAdTDTg7ADLSo7HQCISBECC0F7ezgF9ho1BJk5RWVUNS1MfUMTMwtrWwcnF3CPbzwfXF50UkwiEH4hXFFxInpJdjAzOHZVDW1Io1NzKxs7R0wk7RT0ov8iWwB3KVl5JRrw+uimuNaSdtdNTC60zNIsvNmCnokBoaDR0NrdAwaY5vjHdA73T26/bfZBwJGQsIWJxtiWhMv5xZvl/BZEgSdboTZ3EplCrSKpjOrHSafc7Aq5LQoQ0rlSrVA7jBEfM4zMQdP6pXDpFYAWXBIgwYN8RREwKIAGMJJQdsD9Nx0YyiAzGWyaG5uXyiAB6cXkAB8KPmBElJHQOiewT0gGsGQC1DIBjhkAnQyACYZFT83LlSMS8m4BUUdDonHo2XphfDOBKpbK5iaxV6+YrnPLTbMXIcraIbbN7WJHSLPK6ZYH5b6VcM1Vq9YbxYl/es5sHeYyw+gI1HnbHZebPd7K66lVcA+bc16w5GHU6ji7JXHy9ojUm9qwNTqDUba+t65a80UQ0zEYTuVP+RPREK8S6fW65eEe6qlAO08PfnWSfOwwQi06IjGO7K/eEq1XfSSAzeFsfbZgzyvS/Hb3fvYqPQsAYATgi4iAWH5vJe65djkv5evut6jkeoFEE2EEXu20EkluyY7qmQ4ZsagFIb887zpKgDpZoAm3mACFu6oDEwrD0MA+H6oAqgyADEMgCADOowAKAARhw6qABEMgBRDIAPiqAA4MHGANEMgCHDIAvQyAMMMgCTDIAMgyAF42gB+DDJgBADOR4qAKtKNGAFYMgDR6oAp3KAFBygDmDAxTHAIAdgyAA6mgAGDIAigxcYAugyOYpqnqoAJgyACIMgBmDI5vECUJgDeboATMmcTJuqALcMSn6mpgDpGoAEgx6QZ9nMWpgC7DIAVwyAEcMgA/DIAqwyAMUMgBdDIABwyOYAXMqAOBKonqiZgDGDCFQXZShBkRYJ7A5Q8ZpEAAtNKRA0oykpwW4IAANoGXBRSGM+xDZlcXgwEty0iIYRHrWam07XeAC6MAiCdVbhAtV1watj7rM+mBbXdv6GMBQEkq9KEzbtk01geJHhLca5wadb1FKDRDpF4QA==)
          [C++Vtable解説](https://www.youtube.com/watch?v=2MtC4tBLlow&list=LL&index=1)
      - CS0122とCS1540
          [CS1540とアクセス](https://sharplab.io/#v2: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)
          //↑(下を呼び出してるように見えるがcallvirtにより自分を呼び出していて違反にならない)は、審議 //インスタンスメソッドでCS1540も謎
      - 一貫性のないアクセシビリティ
          [CS0052：一貫性のないアクセシビリティ](https://sharplab.io/#v2:CYLg1APg9FDCDKAGRBWATILH/AAcoapjDkBoHYMgVgyAiDIEUMg9QyDdDIO0MgywyBXDIGMMgxQwaCrDC4D8Mg1wyCTDIH6GQCUMgdYYA5ADEAlgFMANsDGB7Bk68+Y2HICGAZx0A6AGayFYwBEMgKIZAQQyUqgToZA6gyBrBkDyDCQCwAKBibdOgsbywIAyDNJBljbUjoDIZiSA+gyAgQyAgAxhCoBJDID+8oDxDEQugGYMVr56gDK+gIyugOYMqcCWgJe+5FHB5YCyDHkOgH4MgGIMXlIAdgAuMgBOPVpyAAQAAmhjVQDeAL5eEwDMk9NFOrNeYzuTq1VjgQoA3F7zQA==)
      - thisの型  
          クラスのthisは`readonly Class this`  
  - インターフェース  
      - インターフェースはフィールドがない抽象クラス  
          [interface](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOwDtgBTAJwDMBDAYyIAIBJQ0gBgG8B6dg486owewZAqwyBihkA/DIGuGQJMMgGQZAgMaAzBkBWDIBEGQKdygNE1+FAEYBnYCWrBAFgyAYFUAaDDlrXanbrXwBuToEOGQM8MgBYZAkJqAHU36A7uUBng0BrBkVASIYVQDsGQHMGQBKGN0BOhkB+hgTfQCSGQGLtQAAowE0GQGiGY0AKhkBLhlErG2R0ADZae3wAVwBbWgBeWgAWR3LrZABmWkqa5HbaXTJ6/CoACgBKVgBfbtrCBkIAJSJgWa7sG2XgVeAN4AAFeogKXXRZ1mQAdkPj2dp4WnRHRd2bdgAqQFB/wAO5H1AI7kP3Ytn+AJ0+kMVGAoPBwwYAEFkABWbauQC9DIBhhkkgCztQCV/vxANRE/nYsXYgE10klpQCA7oBTV3yxiCLlxkmCS1+gJBYIhgIAbmASMB6hQIAiBiN6AA1dE3eacRR5GLxZIJNl44L8cIqQAblqFAMYM8mUeSWuiI4qIMClDAAyvK5or2BarTA0oANuUAigwFNUpTUc5VLAAOJDAAooxAhgDRyAFCkViiV8pH0E5hx0LTih8ORoiKVns/iAQkdlKrEv72aEzV9rKGAPbEOHW22pkh1jPO+uN4gwfgAYTt6DR7WYxkAdmaALATIoB87UAoxH8PrGZSAMQZANFygGkGMLcUiUGiAUMVIoB/BkA/vKACldAPfKgF+AnCfPBMXg0Q6kdC0EBPkhsTiAZMJtw+iHvwkAWoZAGOGBJAAmGItAHUGaJAF0GSJAD8GeDAB8VQBPBmrPYU3vZgADp6FRDE5llR1HFbdsCI+Ox713IhIkAKoZRA8URiliSR+EAODNAC5PQAQtznI8lmhAwjBbLDcLlcjOEAWMVAF6jQBDGMANGUbxwHA0FoNM6wAc1fd90FYJY+n2R5Nhue5aFQD49P6FMxIVThJxnec+jSQBR/UALQZAA+zQB5Bk4RgeGYYIQPA6JAFkGPINH4IkJ0AKIZIkAfStLyrPTpREoiCMzdhOK48cJ2kQBCOUATtNACEGPJABYNQAIFUAQAYMukAstSDGtaAs2163Um4lgw9AAE4pm6nyWBmYAAAswF0GZcPWTYzguK5ZhmHY9j2FLtnm5a9lsqc8ukJDAAcGQAYhnQlaeqw/qhpG0TiM4DLIg27a9ramxDt847htGvDzq4Ki+EiQBxhkEQBKhnEMC53gwB9BnY7i534X9qP80CwPyJzAEDI6Qiru6wHr6wbntwh1yNR2h0ZIdAntO+gccxdgArAwAJRWkKG+GiAqdq23qP2iQAghnQdnlGUQ9geUPHOAJ5hiZetS3oHIcRzxlmcLGCZplmpZPgw1BUFYKZ8CIAB3VS2xamZRua7ZPk+ThAF/4wAMKJABHABBfcAPpoIQxCkOQlGUQBjuUAeIZIuCQAsTUAWcTAEzfQB/I2C6tOBW6w6ZoaIAzSTzjB+/6wKmP4ZmiAZ+mMWgaskWgEuwCPI8GXxaGzwYA1ofhAHqGYoEmMQBpy0AAgTAGV5YwvE9iJ+cL8Fi96RQA0AAwYvWjogNGiejGOYyQw5wIuVtHtJAGoVQBAhmBwALRXkSIMrnYxytH8fJ6YlipjsucZjSQBVBh2k0VDn3vI/2HdPtzvfR8iP1EgDcq0gP9RoiQoANwYyr8EptIRG99H7WEACYM8FIr7wdjROOQDABCZggng1Fx4TkABx69k0iQRNHkAMYUwEQJ7lA2gpCEaKEiPwAOIdojlSQoAZwZABgSl6cB1DgYrwAPw4CAA)
      - thisの型  
          インターフェースのthisは`readonly Interface this`  
  - 構造体  
      ref構造体はスタック直下であるがマネージ型も持てるためアンマネージ型でない場合もある
      [構造体](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABAMrABOArlcAPqH7AD2JAbxy0RtZAGZaYXMGJMAZtToBJAAwCAvhKky5imrWXpNw0dtLM2wBuo2mR2tLQDCqgPQAqEPVUe3mtzdAactAAgTAZXlAewZSYnwIYhhAVQZAGIZAMwZAEQZAaIZ7MUkLVnYGEE9XKD81KCMBQMB6hkBLhkBOhkAghkBM30B/I0BYOUB75UAgBmlZBSUIwCxNQFnEts7AX4CACkBahkBjhnrACYZAITMIwB2GQGGGavrANGVAGQZAd1jAWQYMgEps0VpAvtpcVVoAXloATgBuQKHAMCVAQAZAVH1AB1NAFYMgHUGQDNDIBnhiWtWqgH6GCKAVYZAMUMgB+GQDXDIBJhkASQyAHoZALcMgFwlQD45jtAJoMGUAdgyAcwZAIcM4MACwyASE1/oBrBiJO0AJAqYwDHcoBTRUpR0BaQulxFosugUAiYTOADEtHQqgAdPdABEMgCiGGDERQsCDAQAWDLhiAB3KanTGALmVAKRKZMA0XLMtUAkHsXhMTGAHxVAJ4MGUAG5bMwGq77ChySG64dCvcVuQC6DOTAGtyzMatMZ/15bMBgEsGQBOQYBQBkBgDYlQAgvntAMeRgBM05IZbqBnI6azkdQAc2IwFeGkCoUi9RhW2ZgFkvQB+DEkKZTAOMMyJhgFaGQAlDIAuhgi6AAbHTJ4tAKdygGg5PaAAHNAJYJVe0TBiMF4uAgAE8a7QmKpXlXAtp6CaAsFwuSHWDIYtoXCZXbAMYMKSRNEsTxNkyV1NJADEGQB9KwmSlAC+1QAs7WSdIsmwMVqwfG5CFOIQ0PQ0Q7wAVhNd43GcHx0AADkokBAAtFFJyUAjFEz2QAnJVVQBlfRSQBIhkHPtKWAAALMBSCmalAF6GDZ0QAflNQB/eUACQZBSrS5Q0eWhCFIwBkwigLStIBQAhBlVCEoVhCJ6MYlFmPpck2UxQBK/0ATaVAA25QBFBl41D8NEOt1M0tsX2kkzP1hclG2AABaY8z0AdYZcUARYZADGGRFAA8GcleTEyT0VOCI6Q2GZwUABoZ4UAcoYgOZQAXt0AUuNeTJFTRCvXzSIPfAjxPU8IiCr9yUAfwY2UmQEqUAKQZXLqhxiNOUj6PYrikkABwYPU8sU7Dw0VHAAFjEcaTBW2hluyRxUBcWhcPQtBDpOryGFoUh7ieA1DQYEjAiY9EQOJUartIdB1Pux7CImwJDNVcyUmmlJkMyD7kHQZ4pgAEgAIhukBjpuhVQw0KBUcVOtMexhUrw0BGJqhmH4aR1QUYEL70fQPHqfQBVcaxhmCdUImSZ2y5lu5nANCAA==)  
      [構造体はメンバのrefを返せない](https://sharplab.io/#v2:C4LglgNgNAJiDUAfABK5ABATARgLACgB6QgA3QGYNsA2ZAJwFMAzZMAO2GQGVg6BXAMbAA+oybCAhnQDmACjHdkAZwCUAb3QB2es2UA6NnwC2AbgC+xJb0HBAdgxjApuaB9BkBBDLduBaOUDVcYCSGMYHMGQBX4wG0GQGiGEgJiAAtgYAAHJRBiPiYBOLiDBmBCKz4YAE9CASUoqTicuNFmPRijCAB+OIBeTGRiMR9AwE0GAl78AjYJIwYlOIkBBm5rISqmNQI0ZAW0CipaLGQAUQAPYA1KdBoMABYt3YAlZkkZAB4AFQA+WWAosCUdFjvkAHsVZAB3KIMRjIL4gZTTThqMxmZaLNqEQDKDIAYhkA9gyAUiVbIB5VUAMhn1D7+LA+QCEdl1ABYMgF2lQDSRoBrBkAqgmAMQZAFUMgDWGQAdDIByhkA9QyACYYaV1AFEMgH+zQASDIBIhmRoVsPkATGmABQZ6rIsLYXm9UYwJDBvmwIPkVH1FqsrPwhNx9h9FJcRKqlPJdFwMOgbX8mg8Legna8lCZDZR2JwlNtzHD4aGw+GI5Go4tiABhLgADmwmgADCAPTbUYAsBKlNMzgAMGQCBDIAgBhDqw2seQ83w4dW/uQAiDId9J2QAFlZH9q9GHe8msg2Ax/txOyZm2GFA66H2LUo9FbhDa7SxVD60HPA8h+8cfZWBFuB0PkLHRw29I2Dzvx6HiIBkwlvtkAbU6AQZsfIBGKK6TkLxpsgBe3QDQcrYgCHDIAzwyAAsMgB+DE4qKAFeBXjPF6siACQKgDR6ioKiooAQ8qAOaOy4qJ0wSAFYMgAiDIA0XI0oA6gxOKWNbRgigCo+oADqaADK+gCMrvmZKobYOHEhRgDXDIAPwx8f4gCR2oAFoqEThgDcRgKSGANIMgDNDCBgCLDIAJQyAJcMgAyDIAgwwcoAwwxcv4gCyDKENF0WgxCALoMLjWYAgAw/kIKpeppYiEbYqLIDaB6DsOXCjl0oRkgpgArDAJmmAO6xiKhHJYqyIA+P/6rRoZmBGxCAImEKRpHEICANOWgAECYAyvKAKGK/iorZgCrDIAxQxCYAkwy2IAtVGAP4MgAfZkZthfjStmAPnaqI2ppOGAI5GxE0oRgDGDIAZgzERBXRmWgKXmQiagbtsl7mKiACSAAypW2WoADaKYALrIAAbhIEB8Fk+RxJMPAmiIYh6L2KWulWEAwBA3wCBIejvCmgBwDIARQw1YANwyAJ0M72APAMsJJQtFmEFubpqO9Ah6GAxx6McL1I1YTDvawHDkK0d02LMT0gCAhhGClZJjZNoTXuZxBcupYO2IALBqABAqqLIx9X16Jg/1Ie9siAEsMAmAOsMgAeDNx2EqNDtMTcRDOw+WrQACLmvWbDBqrBytOc5oKPWC7q52GjaAoPnIGbKgGOYMNw/CxA4ShYuSyowHgbLBGhO7EuEWKYMKZLPiADlGgCKDONLjixLMWAFJKgAmaYAqgzIiRiVOzGhCu7YgCdpppgD/2oAviqaYA7qnEV+tmoVh2EoTSnOAPEMYQdQBPshCRiJkpRThxQQMOWJCZqrBqWo6vkFr1gAmojFrCGdF0MAdh261nsixgAxMgCZ6CmKgj9quofKiTj5vvY8QvdPhAYAvQx6TVgAayoAsvL+GfuqUeNMqANoRgAhbiFAmysRfwyt8wR3zKNJwIYB73TNIAaiJAB38oAP4zAAK2jAwAwUSAAdyYeDBNQH3yIAR3JkAwJkkhNB6DjYcAIdPV6Cg57nUukvXWQA==)  
      - thisの型  
          構造体`ref Struct this`  
          readonly構造体`ref readonly Struct this`  
  - 初期化子  
      [初期化子](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHqAQwFsBTAZwAdCBjUgAkFwlQfHNA0ZUAVtAbz3t/tQAM/TABYA3Dz6DhAOgDCAewA2S0jWABLBVHITcffkNSYAbLMU7lpPQdQBmfunpyQxuwB4NUYAD5u+gykHAEkAUSgIMjBCYAUwT28fegBxUmBwyNJo2LAACgBKTlQAdno4UgAzQgglYDEAXwB6RsAEwkBkwkB7BkBABkB/eUB4hkAYhkAzBkARBkArBhHJQPowiKiYuOE7GVT0uayFvMKSlLSM+ZyChqnA+34RegBBODhcr2B6KEKAdTANYFIAGS9SXIAia4IP70RCPfLHALTM5yAqcV7vL4/f6AVH1AA6mY0A6gx/cH1E62Bww+70YiFKD0AC89EoAH0AOZpClUulpcgMynEMT0eEfb5QX5/NGAIQZAFEMIGBoJJEOmvEobwAbjEGPcANoAXXohDAYEZfIA7vQVehVTZpWcicAABYacjKokaVWcenAIqlTVgW3G+qs51uj2MhVKCDWeq4yGnBxEqAmqER7xMp2OtJS02xh40p3exN1b3J3ih2zoBz+aWOJycZosDgdQDNDIBnhkAnQyACYZAJcMgHqGQD9DIA7BkAMfqAawZAEUMrcA3Qz1wD3yoBfgLGgEOGWuABYZe4A/7QxQ0rgE0GQDRDHjpc1ybu9/uD4ej8eT6ez+et9M5PQaEJKXrnJwyZSRDB4yy2fRX/RlQJ1S+32VTB/y/eoxGaQAqhkANYZAA6GQByhlbRtAAsGQAShlrQBxhkAYYZW3rSt2EvQJmgfGEAD9ChI+pOz+Ao/kAJIZAEB3QBTVw3JC0UxOsmzbdtABkGQAfFUABwYBk3MMS25RE+VyAASP5bxkKAQE4WSoHqN9ZPTNIFLU5lgBUm8BBkdTgG9TT9MM71dNvX9VRMqyLIEIDrMU+zgPqbFoxLehmkAVMIfN8vz/ICwKguCkLQrCgiDGvGhMB1Uh9RhcEIr4aK5MZcQkt4FKzM/dKROlaKrLS9ySwK4Cioyrk3h5JFpJS+TFMwOSLMawzNJa7TmoM7TjIarqMzSZqrLa2zVMwBzhpctyKp3c9Zrm+aFqPCrIpvJx7zi5xchEQpMDfdA3zsMDmmrQAbhlbQB2hkAc4Zazw5pAETCaD4MQ1CMOw3C2HYJDAHfbQAdBgGCZpsaRbgZB0H9wq75yGABJfHoJQ70eDbIeh+4/F2+h9voQ7wMaU6LuuvDAbcWjAHztQBRiOXLpASnQBehkwwBJhlowBAY0AEwZW2betAHMGTnAFkGYSPL4bywuFkXRbFwKIetFHEjhmL1v1ZGYZ8I4KqURrAVyTBEry6Y1ZkDX0G1gXeD1jW7CN42ZrB62bdPZa+AVbViEIShYv1AARDR1C0IgwAAT3cYwBDfVHOE4eg/gAeT5P43xilTw7+AAVXUFFjjH6ATiOk4tMBSFIdOHBDYrLcaam6fp7j0DRTtACEzO7O0ACIZADEGXtAA+zQBFBnXXIoBr8uGcAIIZAB15IT8gI/M+FDeogA)  
  - ジェネリック  
      - 型制約、変性  
          [ジェネリック](https://sharplab.io/#v2:CYLg1APgAgTAjAWAFADsCGBbApgZwA5oDGWABAOJYpYBOAloQPoZoAuA9tgN7Im8lQBmErRQsaAMyKkAkqJoAeESWkAaEmwCuLEgHkAfJwD0hwJCagcgNA9gwix1ScUAWDMCwAbLAHNWWQHYMgfwZA98qAvwE8fCFQcABsuro4ANxhkdLKscaAmumAIW4mgA6mgFYMgJEM5haAhI6AnUoA/MEhvIJROhicblgsMQC+JMaAjoomABSaLACUFtRYaMBsKM4AnmmA1gyA/vKAYgwVldWJ0nU4jS1thoCzyt0iAwDudGJjkzMLS81L1bAkAMIk8gAqaqq6eiCyNvKq+txISp8ao6XR1BpNa6AoH8ISJBhrGJLUJw5TrRqcBEYEgAXhIADc0M4NFgWlCYbCSE5XB4xLoYCQACJQACsihQyjgal69L0XWkcGEfRixhEID2gEAGXogTrI4FCMIABhIAFlGgALNjAeQAVRUAHU+TqSBo1PqSIc+sZACD/gGj1QB3coBUfUyOMADuTyO56glEkksCZ4UgAPxIhsAjuRdACEEeNpqj5stcqBh3VNFIOpA/BgicqydTIYzOBY1A0hBYOL0JoAdM82ABlIsiNxdYXZ24AFlVzYBFJC3wUSpUbAARgArLCliuEZV4qiHB7sliD0fjxdKvnC4zZkIdbq9AaATN9AP5GXkAiCqAWQZh2PS4AEwiV9kAx3KAU0VTxfl6WcUrADIMgAA5QAvgYB1BkAMwZABEGQBohkAIAY9i6A4LD/f9smArdgTgZUcFxVUNS1LoACI0CHQhsLUOAYAEDdDBAEA1RYTVgDtQB87WdN0PRQtRrAEGA9HDKNFTUKM4D6JDeGMZ0vEAaiIABlaBYUTAGkGXJbU/QAK40ANajAFUGQAYhhAiDnTkwALRUArwHWdQAkhkAcGNACztQAFbQQ+wVI0hDBP4OAAE4uhwFtoSBck+G8qoFQZAAFag2DcbsYVgGAwp7b1qBIQhcVnedrCXK9VxQ9ckU8ntCErFVm0y6KhMMQBZRMAdCULFgQADBkARQZAEsGQBPJ0AUiVpllLKKSVABtABdEhC1ivFOBIbCAEFCKGgAhMbsPubCSGaArCsvFduvUUd0L6havL4K5HOMABaA7DqO46TtOs7zouy6rr2xzt0MDrmx6+Q8Q69IerxLpAFWlNI+nAp8LHSbJbS6QBS40AToc+mmb6gZ6Uc+mMwBQxUAUGVAE0GUDAFmTQAdeUAcwZACCGQAhBgg26+GMApzC8QANuWAwB9BkAfoZAB+GQAmhkAYYZAAmGLJpifDTQMAITMJXJ4pABzzT9CfA4misAIoZmcAdYZAFqGQBjhkAToZWZmJ80a8CxAGNrQAOPXF7ZABIFW0LHMbGzwQ8Ce12wxrpt227fto7kGaIA=)  
          [ジェネリック変性](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHgegMEhNQcgNB7BkA6GQcoZBZhkCuGQYYZB6hkDsGQIoZHB1hkFqGQY4ZAnQyAJhkBJDIHvlQL8BgawZAmgyBohkBBDK0IlSrAJZRygTXTAIW6ArBkDR6oGkGQJEMgU0VAxgyAzBkCyDIH0GQyJmAFBkCrSgcNTOvQUPkAFHoAtJrBAHweAJQqAPZQwOQeRmaAX4rWgCIMgPIMgEJmUnpG3tz8woF6ADzBsRDAwdEqZOTmqYDZSnk+JUKAMgzmcoD6VhIA/A2kgKoMgDEMjlKAx3Lm8jnkgIAMzW1FvsJS+nKAEQyAYgz6yU7m4+PWgGxKgCC+XYCXvllbgJ2mgIgqm3IjE44igP7yuwuLHX49OSsQDmDJdAU8XoMgrpACuEHkAV4Fuf7CAaDGK4Ihkd6aYAAUzAADMAIYAY1xrC8gEdfQAAcoB1BgAMuoAM7AcoASQS+PZsAA8jVwuEAAQAG2ZwEFgGjI+wZVhdQA68uM5MMMQQ4LjhbiAOZEvEKQAwKoANBkABgyACwZALgGuzweFQAGZBRhBQAhIlM3EAbzwgq99rtADd1GBgBAicL7QAWQUANVQAFYAlE3QBheJM2IagB0AHUwOo8QEAETRmP5qIAbgAvnhK7hbfb0IKAMoQABGAEFBSAnS73Z7vbXYr78Tm1eGo7H40mU2ncVmc3n8zzYyWK1XrXacfjiWTBRy8WBypodzBBdVxTzwh7cN6fYKeSe3ZrccAV1e+3a72zx2zBeoy72vUQgAlDIAzwwCIA/QygYAhwyAL0MjCAJMMSSeF0zqugEJLCkyjoNlEwIFIYXRNm2AQbmADaOlEJoZLsgBtToA0oZdIA7rGAMoMgBRDLsLGLP+gpEB4Jp6F0gBgCZsNgZNYgA+KoAngz6BktIgWBgA/DIAqwzUPJAimoAWdqAJX+wK2NJgBADCCjRyUpKkCMq15EPoII8oOYDDri7wwfBIhiYAbgy0sY+mALoMrArO0xQAr0emrjWdoOomGHsse54djunL7my0VCgA7gAFviuI7p2KGZWlGW3tl3aXtetZ3rED5Pi+JV2hFTIBGVCbAKlzLprEgoALwnlVb63juX4/gmRCEa2Zg5WsnRdIAXm6APiutKZEqXHXuo6ZFvGpaLd6yZQKmGbZrmuIFj+ID5ogy0AOJPgAKgAngADgdUR/q+15eltO0znt85ZfmgqIIKwB3bisQEgEbJRL9go/cuG1erEK3jk9L2bVOu1zgdP2xMdiBwxdwA3fd8aI0j9qYAAnIdPLHRDAP3cD9WPetz0vagADsXVcdW1a1uFGGxbuXI5cew1Cm6RC1YAeuk0LQgASTlxtbDfej7PtW1WCrVAQK7EjXNUyrUdV1Ks9Qrn5xjlA2iwQw2jd240AjNc0ZAtTNLfDcZE0jb3TrO+2HeoWPnVdgOE4zxNqyjH1o4dbJU39NNAyDOXg39UPuy9cOran16e6jPsY1jOOBwTDMwyT5M/ZTP2x4DdPDcXzt9mzsQh96nNrnWauCsVfboPWXdI0QvpEmAgroZhDb61AuLJWrkWC42LatuEAST9Pw3ByXg/D6PWET1PM91Sv89EQzYsYZLdCy/XXrbw26Ym8ve85evV8jxhWF3+Oh9r3Xocl/z+7DWPDlIUJEyL6xvs3fuBBQGOg/nGQ+T8T4EEADAMOVACwDGYZBw00FSGIIAbeMdKAD8GQA2gyAGSGEuZtmz6xgXAh+q8F7P1DkNBegomTUPimRWhX8GFINQd2NBIgsELxwYAdLNACbeboXBeD5BMRNOQIhZDgovxLhzKsQA==)
          [ジェネリック展開](https://drive.google.com/file/d/194I5BFV4gN6ubJ8FpbMIPu-Qi3vnADg-/view?usp=sharing)  
  - スコープと寿命  
      [スコープと寿命](https://sharplab.io/#v2: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)  
  - オーバーロード解決  
      [オーバーロード解決](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHgegMF/4wDCiQACQKoZAfhkAWGGwW4YbBJhkGPIwLxtB7BkFgVQWSVAXl6Bk1MB2DICpFQBKKgMr0RnEYBj9QNYMgIQZA0QyAgBkIFaDZuw5EAFsGAAHAM4giEAGYBjU6YB0UAKbAC54BDgBPAnbmhgCGYKaewJB23mCucAQAtgCW5nYA9gBurmAANmnBcLHmaTkQwElpUAR4FLV1FESAyYSKgB9mgCIMgGYMgFYMbYDKDIAWDFyAlwyABwyAAwyA8PqAgAyAJQyAzwyAnQyA/QyL/YCa6YAhboCHDIC9DIDDDCyAPiqAzgyAYEqAigxTgNHqgGxKgCC+gKoJbSInF3hagPUMQwuAoYo1dUQroAY7UAFoqAADlAC+BFEASYQUNKmcqVGFwhEVKCIUyhYIJczIzFgbHmf61JrEhoEQAdDIByhkAswyAK4Y9h9kfDEVBmajKhisTi8dyibh6qSBQCCIBM30A/kaAItTAEPy7NZcrRXIJPNh+MJWkAaJqAC4SyUR5stFmTBQRgeCIQA/GHU+mMy2wiUy826giNZ2AQ6NAKz6uwORpFpshdutDI+dooDulTvwuCIX1+eCg2Nc5kxdlcFCuAG9nRQAILIgBCyIAwhQRFdAHFpgBMdbMASQAIilTGlzMEAEY5NPzJY0QCrDFSZgtAEkMgHztQCjEdmS4ouBQklBgBRAB4MIkA6WaATbyNldAJCagG3jZSAKIYyRhcxRKPXG822x2KBmKKgAMy3gAsFAbyebrgAFABKa8UAC+/6HugFCFpQOYwBQZ5vi27Zpjef5ARQJaUPmEFQU2MEdlmwr1LU963pgABsM4JKYORJHYSTzvC2TBMAaRgDOc4fiWAAeP4ALwAHwUAADAA3GSCE4bewGoJgADs2G4SKFCAFBygDSDIAkQyAGAZiiAJgKgA7EYoXQaL6JIEPJIiAJ2mg6ABG2gCqDD0/RTIA0HImYAMgyHIADgyADEMqgDFwPSaFGMkGRQgAwDIAugwiIAQQwiIAsAwOYAyvqAGyOmCAFaukJSIAG1ltPpBHEagz4ALIsRQ7EUNxBEAJwfgAREW5VfoJvl+UQFCAITWDlBoyXSACEMgAmDOoW7bg5gCBkUpPQiIA5gzJQ5aX9G0gBiDIAjkZtD5fmZU+FA5QAPAAKlxH4bQVnE8eJZXlQA5q4bhgBR1W1RlDWAF/qgCperGXBTFWDmAIvxbTGQ5kjqL0IiACwagAQKiBFAORII1THmDnyBl4lZblH6FoVxWHRV+ZXbDRErXlebIwdmBHTmGN1TJDWNYosxzN2fYDlMa4bjuDmADAqgDhzgA/Gzi31YZH6ANOWgAECYAyvIiIAQmYiBQABqwSlK4G2+KYab9A8gDGDB0UxPYAcGaAF0eigPbGDlyeoX43YZf3oJ1zMixp2ldKrqj9BQ2Ktkkx0QGkEC4oAFQxDDQmPw6tH6zvOeOlRVQfE0tRBRwQeVB3tH69V+XDhmW/SAG9egOAOoMHTKG0gB+DG03u+yJ9Rw9jH7oResF7UV+NHZXmGuBHfkmxQgAA5vIDlwq2ABWrjRFzMll9lAdpL3/fB/toflWPffRM3Q8PsPuXuIYaRwN+0lLbJOWljoTCsA5bSAPIMnUKWogC0clMJaDoAqPqAA6mijZycgBfioAmgwZfUeVuAA7kh341QysJeowlhIJgSEmFMaZADQCvfLeeExKSXgbhBqvRXIOTbpIP25cp6o3KtlBeKDDLNTQeoQAR6aACztQAJAqDkAHBygAJBkAPoM4VsEj1jnOAqRU+K4IJmHDhrEuG8UIcadundSFqhxKwhGEjcRBwANoAF0a4o14eVGRTFgCKOEXUZeq1V7r03p/OoeVAElzqMA8xeAwGJmTMEVM6Z+iwOQfhI8dZkEWLwkvYCRYQB1mvCbFxz4ABi+U7BfgzHgkslQm41T/ICZmgANBi6KNFkaJ+iAF0IwAgV5v1ULDJewTQlcKgBAHIORwl4NSZUK6cSTQiEhN9CQeSVohJLHYIpJScgQXUUWRRFBAjlNUZUtkiAKAyOqU0keITum9P6RE1RYzYkBPyRQEJfi4ADKOn46J1TARPAmQUvxbSOIUGKaUjZFUhnjLMZ45pH5DntNKV0vkSEZnmHOTPDkwzRl8iuUtQJKyPzqLrK895Cz+IeNwrDdAwFkFLVaUU1wf8izflqtvOodgnAhLCaitFFAjG1CuE4cSEknA5X0RvUxS0IWgM1DqXA4DIF2M7IsFYCwFKen6FcRxcDEI5lhTo5ZeV1Gzwnr05sPCjqzi8MEKAqZCEQuHsBAAoqxYAZ1zBonMPym5ui8rAEMCkY8wQYB5mCJgCVFVXCqvVWieViFiXauWmw8lhjrm4V/rmb8pKPwepzAA/iRApXABlamJyblcluupVYoAA===)  
  - 型の決定と型推論  
      代入:ターゲット = ソース, メソッド:void Func(∫Type∫ ターゲット){} Func(ソース)
      型の決定は、`ソースまたはターゲットの型情報`によって決まる。(`代入のソースの型が分かればvarによる型推論`ができる)
  - デリゲート、ラムダ式  
      [デリゲート代入テスト](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUKgBgAJVMAWAbjzwHobUBWQCwZT1A7BkHztQUYi8oBDALYBTAM4AHPgGMhJTAG88RJUThCANkIDmfYDIAqAJVEQ1wEgwD6AHgCWUInphEA9hDOHjpgHwAKPUT4wTQBKKlxlFXUtHX0jERMzRms7B0wnFL10J1d3OITfPUwAoLSHdGLNdFDFZVUNbV0SMll0ZPs9Aorq8NqohplUZtY21PT29AKiwM1SzIqqsIBfalx+YXEpGUBxhkArhkAmhkAfhkAJhkBjuUBTRUAxhkBOhiOFHqVUAGZZADYScoBRAA9gO4iIp6vJpEb7ARg+YAACxsIiIAEkiAJgnJSABOHwAIlBjAxoWW9yI+IiNQezzsujAADNNvC/v9ScC4eDkWjMXCoBTqdIfIA7uUAzwaAQ4ZAL0MgGGGQCTDMFcRQifSSM8REI+Bo4IyAMqK5XMlGYdEY9mczY+dVKoRwHwAXnNdhEwD4UGkwUleJJ/0BpDeg3hqq1rL1HKEVMN7qd0pdMqULsBEjAwBsSvesgA7HT6e6iNHY/HPQAFQI+nWY3NgHHOgnhuUJgBCfAVKddzwAbjYYxAs80AGr53XVhUgduU6CSKXl8tRwKZtQJ0iJkA9oQwBF1gHPZwNgNgGyqYGdhg+FkFjHTvsD+3Dl0AzBvDNxyc5vO77qyiukYjfMRgPfmrxYr7vjFhJ83UvJMXDEAMdGcMBEB8adEQIJxYIETBkXPWVfWcMCwAgqCpVQ1NE0iakEgA2Vy2XIhySIbM5E0IRgFDAkIjoQE6miRpPQAMT3EjU2eOEPigCBhCw4BINsDkvHhCk921XU4QpKVSCKAAjMBFQAawYp88OfZoAFk6MhZwzRQxjZToQA50n5YVxUAKQZAEUGOhAB8VQBPBkAaIZAAiGQAxBkAawZACsGQBjBkAfQZACTCERIUCMQ1D4ZTAAMGJhrNFMVAHsGQBYOUAe+UfAAVSgVSNJijQADoAEERAVARlLUABPABxVswDgb5pDEWNnCgEBABxLQAeKMAcwZAFkGPzABEGQBdhkAEoZAGeGQB+hgOQBZk0AHXlACSGDhABkIwB1BkAPwZAE0GQAgBmCHSL3KAARdsOKIc0iChGFCp3HinyIY6uMfO6lDoQA0ckAdYZAFuGQBFhguQBihgynxACLUwAHUyYVbADMGQALCMAP+d+qGvazPpZiGHE4BJMO7Mzqo26n0s8kAy5IQ2ESuz7P25RWHuuFTvOnwfDhYJLpEYJCqZBhcdlQ6ae4imXpoCyCcDaQ2AVE0zQmq5JvGq4bIlBy+ZaanjWVWmiHpxnmdZuEVdNRhOfpbndbgR6Df+fH/WF4nAE10wAQtzl5KFaR11MCOnW1bhNnvQ5xWjdNxW3sAZoYZaOQBLhkAeoZJuSwBo9SYQBYqMADWjAFUGQAYhgygP6FdgJynO6czaYgXRouQAbhgOaaneeh5Uf4wThOwtGvEkjiBKEiwbDO+SA3z5Rm9rtu90KvTVyEAA5IQfl552C4swBDo0AVn05Yryuqf4n5sY1pnoRZwrsR9qflFX4BJ8rohLMAFHtAHTvQAFbTYQAqhgOQAFhgOT6DhS0PAAOGQABhkAeH0FsAbTV1qAG0GFOgAHBkGvZNydBAC6DCnZKgBABg/j/PybAerJUKoVRWjArDThgDg6cklsxqAgLCOmYh4JiGQmdSSZCiCIHTJgbuShCHEJ8FAIQAB3JMe4YBsM4dOPcT1nqWXHu+UQIgbBtQALTKWMjYU0S9npYJfJJb42M3xgEYbIdE3wBGaMsteJUPhAAXCYAMCU5bBAUXdKmRZsZFn1orIsx9SI6QwOUJcspeFcNZgZKExknH0hHHgRYQA==)  
  - ローカルメソッド
      [ローカルメソッド](https://sharplab.io/#v2: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=)  
  - refのルール  
      [refのルール](https://sharplab.io/#v2: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=)  
  - readonly struct  
      [readonly構造体](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHqAQwFsBTAZwAdCBjUgAjFMLgHsoAbAT0GnLQAgTAyvIBvPPTH1UAZnrlgkGsHoBlEbnHqJ0gJZRFAQQDcojWKn0digEJG1JzRIAs9ALIAKAJQBeAHz099T3pLAPoHGzt7VEwANnNdCVQo6PC7MyYWdm5HegAlN3cA30SkkLD6AHpytOY2Ti5svMwPHz8Q4MCw4xNqjLqG53QPIWKY0oMXDwMAXy6NSoAOADoABkr02u5AQ4ZAXoZAYYZASYZAewYdg8BQxUA7BkBVhkBihkAfhkBrhn3ADwZzwCKGQHqGQG6GQE6GQCSGdcyXEAFgyAdQZAGYMgEcjQAiDIAmNMA2hGAELdAM0MgBWGW6/E77QAayoBZeUA5gwwwAKDLMIpUQYBpBhBgEAGTHnQC0cniAXVMXjDoBCRyhmN+gB8VQBuDED4ci0YAZBkAXm6AfFdAJoMgGiGJncfgCWYzWymaRy+qyeSKHIqEniHobeoWPzjcRrGqAmRyCAKQ6AC0UwVc7o9NoBnhkACwyASE1AA6m/wtdUA1gwywDKDEDfoBK/0Am0qADblAIoMetVjH9WWN1nUlWAAAstORAEEMTAAZknelkdfQszn6AHAFYM0oTFSq0lQTnyLX8gXaoRS3SbLaa7iEyvUQ/16GkqgiJXVyhkISgpAA7spJvWp8n6mWwOQ54vckoVyqNBh0PRVPX0+V1dW3e7zoASBUA0equBXuAOAXQZzqdABcJv09gBezEFADEGaUuW5QAfBkAoEZSBc8zXKQVUVuVxXGATxyHcRZ8ncYVAHdYwBZBjrQ8InITDJgkTAAE5XAAEgAIhIvQQCEBiphgGRFksJiSMsKZaPcbsInoWCxC3TD+wMSodUOadNRtYBzkAfQZAECGBC0UOCUQWrKE8GEhsZOtBR6EAEIZi0NTFDlU9EiUAAwZYzxQB7JUAHXlAApXdTxU07SiLsSpEKRb5AAmGX42UAeQZJWUgM40AGIZpWrQBIhjjBTAG8GQDhShKEgUAXANAN080Sy4H16HJWL6EAK/IADlWByddnFILNWDgVwlmWdwADKKqqvKlAM4AarquBAGvyegeV5QAhM0ssUpUIiIRzEZUpiAA)  
  - イテレータ  
      [イテレータ](https://sharplab.io/#v2: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=)  
      - Linq  
          [Linqの仕組み](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHqAQwFsBTAZwAdCBjUgAgBkBLKAR0DsGQVblARX0H8GAN556I+qgAMYzABYA3MNEB6RRKkA6Fu3n5cosQGYpANjHp6AUQAewActSZ9gCwZAa8qBjBkBmDPf2AkhkAr8YE0GBT0RVEN7Ey8AHgAVAD56AGVSABtSGmAAfRjYgApgAAtmcil9bPpyAHsIMDoYMQBWGLq4+gAzZmTgUjAASiFdYMHWirBSWnz6HIA3QjB6S3pWcqqa0j7lQGUGDcBzBkBUfUAHUw4vDkBAyMACX0AZBmdAKIZAL8VAVQZAGIZAaIYgwff7TDEAdjaOrrAOUsPVkykAogmAN71ACFugGsGW6AbKVtoBZBleA3eogAvm89BjlElUukMoB7BnanW63i8MMAgMaAQwZAFBygCUGQD6DIAi1MA0kY5QCrDIBChkAzwyAeoZAP0MgB+GHpvLE6PQYQz9dEYMyy9GiFjkYCRVjAeLJIrAegAXnoUFIAHcmDr1VBNQJMHV0HV9HVpBjtNiPpgAJw5ABEgHGGTmASoZANcMgAmGL0g11DEZjGj5HIaw2LKD0bWqvoRpX2T1QcNopUS9NSj3egA6UEAV4GAHXlNJxAIdGgFZ9fFpTKAQ4ZAL0MgGGGQCTDGHtErRMNRuM45aE0sU8A1I3CTkk3r4kmAFT0TDicQ9NO5vshIvZ2T0ZQFIolC2a5bVOgwwARDB4p5kT7EYYBo9UAFcaANainoBAd0Apq6Ae+VAL8BBaYumEo4ngGJAA===)  
  - usingステートメント  
      [usingステートメント](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUFAQwFsBTAZwAcCBjEgAlQAZBOhkDGGQH4ZAJhkEOGQZ4ZPAfgyALBkCBDAG88dKXQD0MgLyKlylarXqNmrds2TpGOgH0JuaWYYAWI4YAUASjFymgewZA0fKA0TUBnioDAXQHYMgNZ9AYwjAactAcNM9c3M5HRjYuLiIyP1GGwAlcgB7CDBaOjBGOnk6KBIAdzp0siyckhsAIjq7BycCm0Ar8kBqIkB4fQ6AMoyAIwArOgIyOgBJABEAS0oMsgIBgBsSQGvyBwBfQHMGQCCGQBEGfcSkqIAqQCuGQF6GdmZAawZAP+1ATQZAaIZAWZNAHXlTmWOkzZ/IuSAVMJgSDQWDwRDIVDoTDIf9zJVqrkwJhCrJTl0vsUyhVMtlaPVGgBueFmYBgACeJhOJxkF2udyeb0+31MNKkfzZ7IAZjNCMtllTSUkZtybCi6ABCIpQCACuw2GzTOYUBZLVZ2FF2AB0s3mZFqdhJXJpnPZUmi8St1utwqkdoYKRK5VsdlG42V+vVJC2LRsgAdyQDA/7KBYBpBk9qsWKxIvkAJQy8ZiAfoYE4AkhkAK/GPbodQOAR3Itg65GASNzgoACBMAyvKAAwZAIoMgE8nQCkSrcI2roynAJm+gH8jQDqDIArBkA8gyAMwZdoBFhkT7F4gE2GG42QADDIBhhkA9QyJ2OAFYZeIAGhgA/HZtoB/eUAEgzPeFm6Qn+0my02q/Xm+JRKoADMDHQuKq+JIIGbUdW1KSFDAMwAG4EMA9ADBkGTLKMyxASQYgAOYkMARIGshmxouSEAkMaJyPgwmAFIQpAIUhRJnuYeGIu+NioARxTED6YjAAAFnM2pEfQMoMWR8J4agVgAIIwYBhpiKKNgEMJPq0QAnDYHF0IgdB1IAgAyAPiugAa2j2gBiDMSDrmuYJDLAa+FyQpSl1IAQIGAFUBxLkQZDmOQ5ciAImEgDe1oAyQyVoAMfrOMwgDNDOwgDrDKGgCRDO4gAxDAOzxhYAAFH7j2RwmmY/GTHqkberqKoLKJsnyQxinKSpgDgZoAlJo6cSkmwWi3KSQaPEmpy97oE+v6RBgL5tTS5JCsl5p5QAJHUTB0GIHEZGKVE1HY6FFm+NQFDKOJjQxE1pHi02bOKG20IwRp6X17IjVNyI7SQi3YuUJ21ONYpzUi51NDhTn6dIciAJCagAOprcgDHcoApoq9oOuxfiQziAD4qgAODBFzyAEAMEwADLbM4Kn/b2+yACwagAQKnsgCWCYAyvo9l9gAyDO8oXQ69Uh5cNrTOq+D02AAOnUX2/QDiVM00Yh5UzPgc2RB1OY68k4td9Qs6jiWNA4VM+HZFN0PLcghsslaAMYMgD6DFjKm8vygrbMrgAkSoAWdqAKoMEUg+DUOJTD8tMOtwAAHJyss6X2Fs+EAGxpdl370OkjvO670sAOzFM7DWC4rMhMF9gDF2rFciAGsMgC3DIuYibETAXBYAForlrcPbPL42yABc2gDwhvL93vow2pCbB9jPY5g3UzYK2kGt10zXkZ2okt5StyQ7dnTN23zbQmCc2IAtOXb11d6PJA95ddPUbdI8PePnPy2YlcLTXUn11v0g72Pe910a8v2f1mByXUmxT45x/nafIkH4dNJyCxYAZOUtMAKIAB60AoMAGYGQoD1zkLXESux0o5V2JDKK+xnjOEAFoMNZACWDIACwjABcnmbRKDpL6RGoCBagzEuoGSph4ImgAvNzUl2AQXZ1b31NMeRI5FLw3k4VwnQeBNhAA=)  
  - dafault(T)  
      [default](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOB2AhgLYCmAzgA4EDGJABDCQGYECuEwgfgyAWDIIEMA3jjrC6yAMx0ywAE6tqwOgGUZc4PzB4FeANwBfISLSj0AdkHYRl4wDY6GhQDEAFPbp4AlHQC8APgbM2Dm06AHoQwG4jQCiGQBIFQCkGQEUGAysbUQAWOgBZAg0nZHQABgBtAF06AmkAczJ3c2TksMYWdmBAewZAI9NALO1op3zAOwZACIZAMQZAKwZAIIZe90BzBkBcJUB8c0A0ZUBNBkBohiS64QB9b38mjhdNd2019dc8bcbA4GDQkMB+hkAfhkAmhkBhhkAJhl7AaPVAJIZAaMjAMwYPoAK40Aa1ErY51JgQAD2BAUTCOFnWIiY5wCzWuljCgGLtQAAUYAhM0mgEJHQDyDH9RoARBnJYUAlgyAZIZhh9eoBITUADqaTUYXZpfQCBXoBjBkAPfG8ZYAfnByS2XjcdAAhJLOUEbrKoF4vIAUe0A6d6ABW1eoBxJ0Anaa9QBVDIAthkAlwyAZ4ZAJMMn1+AJBYMRSLoEqlKp2l2CYSVKsAp3KANE1ehrNS1AFiagDAlQD+8oAKV0AsHKAe+UxVYXfKFG6dInLMm0RwZZL046kQA3cp0QvbZwpw5hQCo+iyEhmRIoqHgADz2PyUAhnOXZq43JtdlsAFR8vUAtHK9aTMJwAMncLTw7AgooL62UsnkkhkqN2fbC3jzJAA7koVPInJ4nIBpy0ABAmAZXleoAWDUAECqAQAZADAqgA0GACSABkvoA+K6AAhGKzuA2whhMW0ilugO4emEHyAKMGgCMGoAMgyAIRyerDOSvREoAYC7gTcdBzPMgDaDIAWgyANYMgCrDIAxQz3IA1wyWl8gC1UbSkyAP4MyxOJ00SoYApoq8n85LLGBq51FBZTbHgx50AAIr2ABqBAQKw5AXgiTrCHkACcTgEAAdGAhyEbp+kGQAHiZ4nJGZ+zAO4hnUIcREAOQADr5K5dBOIAMwwMYA4aaAOra86ABYRgBcnpMgCyDNhuEstE8yAOoMfwRaZ6B6YZABGLn7g4KlkCQaUZQZZAHm4S45SE5UQBAdAtBF0WxZMgAr1oAwPGTIAQAxDrI9CUcMqw2XQ+gFsNaxGApu7Kap5BrLUdTiHYmh2Jp80SDAUKsJlED0BZK22RI1AABYltQe1WAtmVQlCtWZWdlgLXk+SSGdw26EAA=)  
  - ref構造体  
      [ref構造体](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOB2AhgLYCmAzgA4EDGJABAE4kBmg05aAECYMrygfgyAWDIIEMAbxx1RdAPTjkAZjplgDAK7VgdAMoLlwQU2YB9DUpV70chegDcAXwnjAdmaAsBMB2DB06B7Bl2vASQyBAY0BmDIBWDIAiDCJiMnRodADCEARkZILhYHiqeBYRssjoAAxy6eIAVLoGmsaoZgyoFgXiVqGi4UwEMAD2eBAAnowsFVrd+oZaeqiCYmPjExOSgImEgPUMgJcMgJ0MgEEMgAYM/PXjTa3tXWpUeAA8ycAAfHIHFptj4cgALHQAgvFgAOZ4esAAFmBkABTFQZlRhkVAASkEVkkAF5zt9fnRoSCqps6tgxpJAMmEbmeZDeH3hZEA5gyAWjknLo/gAyMFeQAfZoBZBkA28aACQZANEMgEdlQBeaYByA0AMgyAIQZWbzpuSSM02p1+m5AISOQScRMAhwyAXoZAMMMgEmGQA+KoBnBkAX4qAdQZeYBI7UAFopEwDNDIBnhkAiwyAEoZANcMvIcrM2jR68iMqkBpWAJmE6MmtweuPxnx+/y9HpMIPQYNhdEJiOjV39EwjQ3K7pRKfGMTiZDo1AgZGTkwiOTypckni4bkAQ8qAc0dANHqvMAdv6ARQZgoBmKKJbmCqJdsl0vRU/RKkeyftLaeMpndlgx4mr7iXTgVFsACwyAawZ26zAEAMgF0GPV+QCxioAGPUAIW6b/eAMQYiYLAKP6gEDIgWswD6DBts2N9gQjidzpQf3SKZFxYVw3EADj1N0WQB+hhVWZN0ADW1AAp1QBNBj3b8jgAFVOVxliXIl91XDdN2uUsyMrEDmDoTC6AAJRYEgmDwWgLCcQAIhmvAI13XLxAE8nQBSJU3HxAGMGPwgj8QBABgpaknAbRs3FI8ilLESRIJguDEKQvUAjbQAYhkABwYAkASIZgl5KTQJrNTYPg5C0MU5SlMkXggmvblACuYzc2NEwAIFUAVZtNwCUSgj3UtpkAGAZ5BoABrABaCUukbQBYBnshyyKrCzlxYKkaREsTWXlXQ7UAH4Y7ScQA2JUAEF8X37XBs1ucoojoSdJkkOhAEMGQBghkAZIZeDChVACOGQArhkALo9AF35RKBWE19AGkGQBLBmvaavEAaMi/GdT8VPEOhABgVQANBg4gIzSKtUiWWQBlBl0tw6AAeQAaToP4YBYAhFAgVRVl4QBVeWWMbtKCMF7IoBgwAANwIYB6CyAA2dQDmOFJzjUZRaHidA/ghFKxAwuGzjoAAPRNHuYZ7XpLRzKLoE46DwAmnpe4ALHS5hsrcIl6T7daJmQAB2PH8nELHsIBFgwVWNtZsJ4ngFWaajMAFfjAG0GNbSzRUt7MDOgAFk0aa+z7MkQA4aMACoZ5iKugvmAYAKDIEBJGAAB3MALcYgA6agWiIcQCBIYAWjIaQAA5UHESLgEUMhxHQVBUC56P/ZjgBWGPUDuSHsnQVO9fEKI1H96R4+kQAsf+gm1AHWGAByDC6Gx04y6cSLqCiggIAgFpqEAeH0nEABV9ABzzNwJMAUGVAGoVZV1ScQA0TUAC4TN0AG0VAGjUwBVBl01lAHvlQBfgL5QV3wk06nEtQAxhkAWoZFkACYYe0Af3lAApXVnuHlrrM6xv8Lh/GmibpvmBdONwJbpwBTuTHuvgDRSbi3agARZpEkALhKgB8c0AGjKv1eDGyKgFJWkwH7wyftTJE9dG7N1bhTFIABtVAABdBmlFXBOBJJuNQBAKBYVOC+QALBreQkq7H2DBAChipA2B8CggdhCBzDBL9JZARauIaBMC/5Em/q9LwgBTRV0n4ZYiCAomSCH8fW/1BEAUwWYIBuDqD4OAAQu4pD7IqzEGiKwQA===)  
      - Sapn構造体  
          [SpanT](https://sharplab.io/#v2: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)  
  - ポインタ操作  
      [ポインタ](https://sharplab.io/#v2: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)  
  - 名前空間  
      [名前空間](https://sharplab.io/#v2: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)  
  - 例外処理  
      [例外処理まとめ](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHgegIF5SzyLKrqba76G9UBmAAg1YFEAPABzAAqACzAB7AO4BvIsBETA8PpFAyYSBlBkAxDIFO5QGiagOwZA4aaAIQMDmDLLHj5gJIZAH2aB5BhWALBkD2DEcCyDID8GQNoMgZIZAQAx5WAaxErPKARvrygIcMgM8MgAsMgD8MOoC0cgAUrMQAfKw6gC9ugDH6AJT+gaiYAGzsACysAILJ+WmZphKsUACm4qwAcqLAAJIAtrwANq39rVDArXA8AMatvMAAlqJQdQDceEUBJeUlAAysAELJogBGAFatM8CsZ+f5kluBAcFQEENDrIAQKoDK8oAo9oB070ACtrJAD8oPyOlygFiGJ7PABuAEMwKwAM5pW4XViI9H7Vjg1hNDptDrVMAAcwgYwms3mSxWySgiLGogAZicLvl8ht8Lhns9goBDc0Ab3KAkGgkCQkXaHSAC4TAGBKcOKAHY0QA6AAy43JsjSxFYB1BrAARKMFgBPY2sEBK/nqrVQHVCVgAHlYAFZ8SbUUJRGBgFabXy7fyiS12qxelAkUNFnAAPK8VpgRH0qC0hbLVbG4CiUSsIYrcnG7lPAC+gTwZcIJAYdfrDcb9aY6DYj2D2zYO1YiwmrAAGnVpAAqRbs1mIoao1r5YcEQlyYkRjNp5LGmyASIZABIMJbW1Y7/KIgGj5LQWImATQZANEMDkAMgyALo9APnagCsGQAsGl9HIBuI0AUQyAEgUb4BvuUAeEMnhYdgykqVgACVWiJABpXs4CHW1CTAc12xDflB1LA9nn3DCZlTGZnWSFcs1YGdpAIfsIkAXoZAGGGQBJhiMQB3WJcQB9BhPCxAFjFQBOhkAW4Y4ivZDniJNYggIAjgCIwB1Bg43RABKGKIeMAfoYlOSGcLEAM8iN0ALQZAGsGQBdBh0Nw3F4gTkhUujAHqGQAJhkAG4Y4h4m8rBcQBt40AeIYNyfQARBkKHCMJDIheg1EBWDAGDF2EwIZEXcMOlIhljU4k8jEAUGVAACGY0YHI7lxMkmS5PU/ILEAfFdAAQjWTAAbTNinxPPTUq8IxABX488ooC9qOs6oKQoXMwiFaRwozaMAEqgQB1hj4wBFhkAMYZAGKGIxADXlL9AC/FDxLza8Sw1aMTutCokiEs2yHKcwAoORcwBVBjUQAzBkAGnNAFNzHQTx0QAvNxKwAEuxvQAI2y3K7pLcQAxBmQvDAmBzswN2KpZgABTEGY6lYSiT0AM8VADAXIxaMYnRAFNFNQgQsQBKXXPexVC8J95IiOJAA6GZDgFQ9COtptCNueaC4IQ9Zmcrfy7VBgL8qEenOoCUSNt53DkP5kjuDmTMVhywAABnEIRxnUtUAFlWlRVFEXJVo9RNTcdweIglfGQBT5SMc6Nz0kUjE4wAPBMAPKMr1vVx1u5/kSgATjXQAFBkcQBBBl3DaRY9sXAkl0aFdN1ZWnVzXtd1/WktPFKMpLSiY4tq2beFO3Tydl37BvN2Nu9tclB0E1WEQcj461nWZx5LqCHLuOhuTUb68TpuNqCqBhtGnRACHlQBzRxvH1kWGRFTkAAwZ7H+twbEAVaVAGkGHyQ8XZuQ3DgJWV7SchiZj3nnL40VGMtxOMALATz3cwBFBj0xw6MAQH/kn3pkPnNfJg7D8sQPQOgSQ0NYbrH3PuIgTYoHQJgQwIAA=)  
  - インライン化  
      JITコンパイル時で`静的かインスタンスメソッド?でIL命令32バイト以下、反復、例外を含んでいない時`インライン展開されるかも知れない  
  - ポインタとref  
      //↓なぜかURLが1ﾐﾘでも変わると"using System.Runtime.CompilerServices;"が効かなくなる
      [Pointer_test](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugHQBKArgHbBgC2ApiQMID2dADpAwE4DKvAG5gAxgwDOAbhxUAho3EdZYggAVWYGrwD6wCcADeOAiYIB6M4HqGQJcMgToZAdgyAWDUAQKiJIw8gewYAFACoRAJTueIDmDCIAtAB8HoAGDIDGDID6DMamFja2nskmFgDagCD/IgC8gA7k2bIwMDwAuoCO5JUEUQR5/sV5foHBLR41ULltQTDonejdJCQ1dd5uAH6AFgyAmgyA0QwzfSRTgNYMgFYMC4ACRp48DABmgP4MCYAiDCGAgAwinv6bgJEMgFoMIvaAfgwvAZnmZoDTloAECYDK8hlsKYvnlxC1cuIOiUutUelCBkMRmNap8LIBpBg2AH4cGizHwCOICAUCFQGAB3Ah8bwBSQEGZU3wEDhEkkAMik9IIzAIImJpIp3JpdJCeKK4gAZSJqu4oAQ/Cz+nKWVEYARAEkMgDMowDqDAlADIMeMAFoqAMwYCBAYAdzfYjgR4gl5YBjuUApopOZyeQnAS0wa0fYEpMxFc0S8TAapegju1gAIwAVlzfNG4+rAFzKgFIlQDaDA9AOvKgEUGQBryoAohgugFo5EIuQBgSjn1drjacFjjsHj9gcCIBqIgAKgBPDgMVsEHj85sEWQEQArhG2uz3W0yB+yR94h7JvLJPIA9dObATHBDZsl9IIsA/Hvh4eI22q3J79WTMZ63bMv+7MgETCJ8zJ+eABEPA/esAX4qAP+dFhmD9jw/NZAGPIwAJxMAVQZABiGBZ7BCQALCP/EJAFkGDZTkAIAZABMGF4C2bTxACvA8siMAarj7EAIoZAEmGQAbhlsGZAHGGQAfhkAfoZvEAIeVAHNHQAJRQCdUUPQzDPkfQBdBkAIIYJIASQAGUAELc1gAeUAaPVPEAWDlAHgEwA4MxCQBdhkAEoZAGeGVjAFmTQAdeT1ITTjxQAIhjiY0v0OfsP3VAzjOQuYZguIdD37Jy/B4TdxzZAKgv7EFvFkkAzRgCBWD5QK8UclslxXddDgCD8Yi8xc7xHTYFkxLEQRK0rSsimTovNOKRBHXcm0OYjSLIgTULQh4LmS5zPEAXCVAHxzQA0ZUAITNrUAf3lAApXdCFgSQBAhgub5AHDTBTm3sbiePVQBHRUASHNFkAZQYZk8dDjXEwjm3VQBwY3TYTGyvL5m0AWMVnXdHh0EHJyeEIEINkAeQY/JbF63v+z7ACEGBZAE8GNCsW8L90A/T9j1hzZ7O1F5ADEGPcyosEJxM6nzYc8EKEYCEJAABzQAY/T1D82Q/EI2VbQRZB4MBW08QAs7UAB1MNkQggig56p+SmAgkeNFHUdmkT/VY5jACaGQBhhkACYZABcFNZmz1UGXD2dBNdeF4QlBmZAAAo1GoLWXUGzxAALYBgBZEALHJB2SE7VgKGACgoyYER2DMclZGAEQLaxQQCgAZgAOQAWQAVQAKRgAAtKSYAARQjtkIDAEMClUGTgBjABBKSyCoKhOxDi2AHYAGkABEa44ABrHhxAAURbqhZOAFvk5k8lFIACTAZg2U0GAGAADwKV6CE+NACBrgwAF9Z9QbkjFukFkBDghLVYP2CA8fl0FQEOSAAFgAVgANgOaRbuX2654j2RNG0deyoIahxFkA4GCIVBUDvw/qYHkfISRkkpMwYUEtSpDh5DwMBf1eR32AaYBBgNkEwJKhYBmA5GaIPvCIOkFhvDMD4HgVAeAAAcARAB7DIAWYZmKAA6GZS8oADkRROFFGYNUXh1Q2GbionRBweZADp+hcQArQz6UAF0M9gxEXBeAkTwol7AMOYcpQAHgz2DclLewgBi7QNnqQA+dqABkI9CLx0yAGSGRozAQAAB0OCdmABbVgVBABm2oAJf9ACHDIAI4Y7EFAKMwAAxJ4rxWDN5n18EyEQHB+TeGQJE3wAQo5UG/r/Eg+dxDqE0HoHgC53qBBQagnecU/ZMgOLEkk3hd5lICN4bwUYnEMCSf4WJ8ACA0LpKgiwbhD7jhWH03kqpCCTGJCSBBcpD4BJ3hwDGxSzBMmqvFEgRIYkghADE8JZVzSjxIGAMqIBWlbNKuaNwYAz4kCoaVQ5ayTBUOOSVMoapUE3LaR0h5IIQw1RWTM9ZFT+StIIO0+5G95lmEALUMgBjhlsPLPUgBCR2+hseydZfBmAecQAAnN4AAJCBCp0UDC+AqYvOUvS8AErJYvD8tIHlEsqQQdApAAG33RegLFuK6UErpSS3kwQKXBCpTS0FpUyX8gASQEOIcWXCpKpinFeKOBcuJaSvlBADCUupXfB5FhACrSnJDYykobiAtozDgEBZBRnsMAbsf8TVUFigwfigBITUABIMNlUYzF1HqVGLxvqnExOiRIrL2UfjcAAcQYMAScDAaQkHbKwPgwAmZUAAOY0n5RGqNNrY3xsTcmtNAQeXhsjdG9Nari1Zp7DSItFLBVFNQTXA+/IIHz2gTKkEFAclMmAGqKpnaaBJL8A0ppUTkmpJ/kwTJ2StB5KHDATc7Sz5CuKZ/LtBB/ZxP7cAQdw69BJP+eEAgS763ALlbi/2XL/Y8p7ZemAPLxBgAAF4MFYAcbwW6AgEofc+1976cmFs1bSjdJJfA9pPR/M9oaSCZtLUEXNSbNAFozSW7NcGE0IdTdWlVMHUPIcrTGwtpLa2Afbf6HJ8pfBKACNkSo4HMZmDXbIDgNHm2Ci7dkM+tGsEPxBA/B+CzjWmvNVGHAaAsC4EIMQcg1BaCMBYOwLgEBeACB4MIMQUhRPbzngvLeBByO9oZSfM+kgH66ZDDwCgIhgBUgMLp/TJmcBfwnUQLTq8eRAJMLphJBAI40jVVg8jVB+R4DoyYNd9K2RUFC0g8jH0MFRawUOWLr1xnvRCwFgdzIUvMmi1gxtBmW01zbRBxJvJ6XxMSWOtJk6skaBnfkls87os1O3b8qpLWd2NL3QEQFwLl1lSwQSVkApKTUn66VPgTIWT8g5NF8jdBCAkgRHNmgBA6DZZVNEFb1m6CryqZR8Q/Rttre3iSGA7hovRYRPyBb0XNsGdu1ghUh33A3fSzK87D33u8ZwA/VFQA)
      [Pointer_test補足](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugHQBKArgHbBgC2ApiQMID2dADpAwE4DKvAG5gAxgwDOAbgIB6GYE6lQNIMgKwZAIgw4caAswgBDceIDeyAMwEwNAgA9ppogBYCAMQAUASkMBfTznHAeFCLABHz+gcDGZhbBNkRmyI6uHt4auKgEALKGOAS5ROgAbA6Zuhbu2dh5Vdp6BgQiBAC8BFQMAO41+uLukjnVuaEBQUTIfkPBza0dg+E9ff3RBLq9lf1LTfUkNvPVuhvIo2FBW9JygIcMgL0MgMMMgJMMgD4qgM4MgF+KgOoMLuIkSW6A9gwQMBCsES6EjiAjoZSAOwZANYMfwAZn9fv9ASR0IBzBkARQw3QA3DIBOhkASQx/F6AMwZVGodlURJ85qt+gcxuEqW4Vqk1gRqOJdLCGIYZAAqWQyfD5cjUWiMFjsLgQXgCHjCMRSQAyDEoyTTWeZLFYNugVuq8gl+RxWNFeBsAKpUDlckgAQXEAAVjTReC4eAxYdYmeS1ryXNFeW4jSaeBtUMy1aziABOFxWJkCgjpKHKQDGDETANEMLL1BDdHsWIeauYIFqtTDtZHdAB5ogA+FxB508L0RtYFggmcPZ/Ix33+wNO4C8eNydsEZNpzMt3K8mQ+NVzudAA=)
  - StructLayout(マーシャリングで使う?)  
      [StructLayout](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYAMBYAUAenwAthgAHAZxEIE8B7CYCAOgCMBTfAZgDkBZAKoApOAC0AknACKfAPwAbAJYVgAXgAKAGWAArAILiASlCg0uRAOwBpACI2yAazAUAoi6jjtLqZoDuAeQAJRQBhADI1AEZIgA4AVjxCEnIqQggAMwBjMjJmKHZgfBUIOBp8TIoiAEMwMnxFKGB2MDo6gFt2NrowGnkq+kZ8AGJ2AA8yJUzFYABaPoHgRPxAeoZAS4ZAToZAOwZAVYZAYoZAH4ZAa4ZASYZAcYZ9wH6GQAuEwDAlAAoidiq4ZsALBmAaMnYAAmqoOHk7EAp3KANE0AJSvJaAactAAQJgGV5HYHE7na73QC6DIAYhlBgCsGQAiDGDNoBVBNxgHsGaHwwAyDJp+gxgFYGnBmABldgARwg7Eaiiq8kA5gyAQAY1usqTTGPT/sw9Iw6ETcYADBkAigxLQAmDIA/BkAUQxCkmwuGAJIZqQtxYypcA6IBrBkA6gyAfQYdbrANQqgGSGXm4wDRDHcdXcTXQwYAgoiFYLwUCqHQoZCqmR+TOAkEywANtIA3nhvinvhhIsnU6hMGnIsxDNBgIoOsxxI1mq0WWAAG6KCMUADceEzKcIOpb3wA2lGY3HRcA7vGxQzmPgAFSAUH/AA7kY/wLPZnKLPPHgEdyWdggC6HZUve+84gPYTDWA30UDe+8joUAA5hfz6waE1vqwGwBfQiAADlAFaugHJNQCyDLigDR6psgCtDIAJQyAF0MvKAMXagAAUYAYBlmoAsomAHb+gCqDBiLrNrgqZdj2ECxkOA5EUakrSpu27RgRJ5eoex6nuel43nez6Pj8L7vvg37/kBoGQYA0gyAJEMgAOUYAgQyAGYMqEYVhuAdt2VGEf2g79qRLjjJM0ybt8hCABaK4mANJGgCrSncABiijsPIcD+Ok6QUAUdz0XQNl2cAYJgryqGAJoMbqALOJgC2DOJ0CKFegDRkVauLYoA/vK4oAnaaAEEMvJ/i6AY4amO7Ud8akTIenZmRZVnOfZmCbvRZ5dnllnWbZRWbkxt7yOeuXmZVhUDsVG4Pk+HEdrhhCAI6KgAUrgigAblma2IuiSgBaDIqgCWDGWobsLGzLAOGDh3OwFBgg2myABYRgBcnhSgBSDPxFq4h2cn4YpCzKYaI77ou3LyDA3zqKt3yqN8AAs46AFfkmCAHr/FiAMCk6AAHqKIA1+TrluqUpulsZ7myECvZkDh0Y0DEXle9X3mxz5voQKMOCSgBFDKsYGAIcMgDPDIAEwy6oA4MaAFna3k9amhBk5TtOvCS/IopsgASioAtBmALLyKF3CTxyADcM6xggzjPWoAsAqAA6mmyAMmpgApaZsotmoAw8aABIMkkoRaITqAIFKALVRgD+DIAIQyeYA8gws7JsPaSsGwXQpfbXSRd1Iw9PIUc7GCI+yITyBQR4Y2VdUsZ17FvthuHyb2RE3bSpHjtOs5equ0MdkHXqh+HpWMdjMd491TuJ5dnu0qnw4SllGmuTDuFB43hcJk1+VVS5dztcX5XNQV1UDp9tWlw1g/d61dyROgm6x/jr7nc7SfUSn3sSl6z1E+93wxAHrfoMHyOrR3A/R5Pi8V3n6BcN8SbO0fx+P7hb9s/g81fLGJI+WdT/vxdu2AB799w9m+BQCge98i+BPj2O4W1Wbvy/otYAy1Vp3EgYgkBb9aLRjTKgKoUCPowO+HgsACCmw4NwigpaUYMGoEIZtKhgC36N3ARtaB7BYHsOjJQpBb9aFoPoajdazCBG4X3ETcBoZiHfFIVI1a8DsGsJoVABadCVqiNkSo1RKYJEfyFAYlM+5C4QMyFw2Bpiw78OoamIRzBAhPDIJgzIui9EFzDt8cMliyHSkLrYvRKYHFOKqC48M7i37L2dtE18QA=)
  - ポインタクラス
      [Pointer<T>](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQB6XYAdsAFQkAEADsAE7mArhOQAoD2RwApjQDyvtfd2APiFVaOfGWp1GJABQlpAOkoBKFTgCuhAM4BDAGYdyO2poDGwZm2ICAKqIDuACy7G75EOW0BbPYT0Acw4YAG9kAGZyOwpKAG4AXxxI8jRyAGFyUJxyXO9dQ2MU5AAWcgBZOVUsnLy6vlseBs4eYVFpcgBeckIOR2t+JpsWwWIRKrjautyAfS7yeUVaFXV48nxKQEAGQFwlQHxzQDRlQHUGQDMGQCsGQBEGOUJNCAhAaQZTgH5VKfIk7ASgA=)
  - UTF-16とUTF-8まとめ
      [UTF-16とUTF-8まとめ](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUKgZgAIMiBhIgbzyNqOgGcBDAMwFMT10qa6+BjABZMwRfkQC8RAOSB//+kBuIgHplQkYGj1ABToARgE9gbAJSB7BgCqUAJb8A9nDaALBkAAcoBfAnQD1MANi0AGAD9vAFZggmDjQD8GQHMGLUBWhkBbhkAmhkAfhkAJhkA7BkAMDMB99UAghkBw00B1bUdAdQZAMwZAEQZAaIYAfmNjXj5aVEwAOlaATi0tKyhgY35jBSbm1r8iBkkiACJAXg3AYD2ZhVVxjQsAFQAxAFofFWVLG3s2QGsGU0TUtMdKwDEGABZABYZAEoY06MALCMBNBkAuhwzAFy9ADIMgC8vSpVaqjZqQqG0VSAaDlALLyl3SICIABEABx+CY7NEAIS2WyIWkwfnQ92MRBJZKIACpKaT7tN0B50H4IdCOTDlIAoOURyWRaLIWKIONRBMJxIZFOZrKIiCIzP2OMw0z8AA8SeKtv41X4durNeLGrhoSwrGq2HAtOowHT+AAHaYMYyUVTzJaSKTqjEEVFEb0AUT8xAkUlC4QA7ERgt50eF2ZDWh1MN0en0Bg6ANp+AC6xjaG1sAGVgGA+gBzLQU+UzWZyom9fqDe2ZzB5gvF0sVqvDBPNPt8VTmbZ7bwZJ6vYrumaAJIYbRpToBTRUAMQzld6ALk8KoB/BnBJs5YmEYEzOaITHEUigbAA7geRJn0DmRnvOWfsyepFnc0pX23plm29+/BvtM1qHsY3pxqigGtu+RI2uBaqokGBBPvuJDtF0Whnr2z7NAAvn2Q4juipiAIAMvKAMsMgDDDGkpyAGTegDvyjOgCAxoAhgyALoMy6nIAEQyAMYM5SAKryLxpIAo/qAIGRjifNUxSANIMgBeNpEu7QqomAGEYWgRroVgDAA+jp5iIFiEaErQfgABoWTpFkWYRyh6IYbDEqp2nGPphnGSZlIkmZVnWfSlnWWZtkEGpjk+FpukGVqdCYLFfi+RZ/k+YFSUJUFuGDso9yhTozmRYghoEl5sU6eZiXeclfkVWlqWBaMBG4HhQA)
  - 型付き参照(TypedReference)まとめ
      [型付き参照(TypedReference)まとめ](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQO0AJl0B2HAbx3ysPQDZCAWfAWQEMwA7ACiIAYBtALr5WAJwDmAZwCUlahWzUl+APQrA0eqANuUCyDICHlQOaOc5fk7B8AD3wBefOl4BuI8oAqATwAOAUxgAlTwDNPUU8OAGNPfFFrfAB9GIBbVgBrIICuc2l7VRULfEA7BgNAJIZAHXlAaIYcJyU44P8AN1YIAFdPLlEoEw5gaWiATh6stXwDQGFFPNzAIIZAD7NtQG3jQHiGctxFYxoe9Mzs/D7K6h2qIgA6Ny98MxtqgOAPFtFMvdWuLsdl5Xu1GsAJJ3vTC3Rou2eK0iAQ6ZlEfxsNV+AxylgK+hKi3u4N6/U2IzGlimswWFReyiIa3M6A2g22+KU90OxwiwAhkXQBwA4p5gDSuHcKdRCY8SYCVm8VHExOIIGBJMBAPYMgEAGQCyiYB0JUAqPqAB1MuO4xKx4nlABuWgCsGcZ5WRcqgisUS5AAVi4wokZuAXHQ7QARKxXU72gBycwe9qMi3SUk5QA+KoBnBkAX4qAdQZAC4KgGsGcaxGKm8VmQqTQDyDE4AL5OJxEejIJhJ81Wm2i5PdQaAO7kVXlABYMgGUGQCxDDGE0WU+mnAogYNALRygGq4uuAKIZAAYMEZjkvwAEEJABJYBBVjAAD2UUA05aAAgTAMryhUA/vKAMQZ7tPxHOF8uoiLGvEQsAT6JFyvohxPAB3Kez+d3s/WxO28v84zPgAFpAERcBeV6dLe96iMyrJ+IknCcOIADCS6NJ0HL4AAfPgvBGkCXZAtQS4AEYAFaeKEZj1E0EQ2BwjQQBAzz3EoNK+AEqRhBEUQ2OB15QWesHAAAcp45jAEeHIwvggD3yoAkJqANAKKqANIMgCRDIAgZGAAS+gBryoOkasdyjI0qc0QXP4VxeK0gb4OxfiBME3H4Op0qjOohQ6YA6fp4kR1CDOojzSIA5gyAGBKgAJOhGgBmDNK5k0c0rRQOo3RBYAJAp5HpBnGsoYD+PgjzWDYlmeEu/hcKYAb4HFdEJjUVUJaCnK+VQngQJIEQ5XlZyFdcJVcKEgFiBVVVmTEtUNPFbT4P1g3/kRLVtSYuX5VY3VeL1MBoSRECeEN43VbFe31RtjRbTts1AvNETDeco0BHVk18Jyhn7OgazAAcImahE8D4E6IC/fgP1VY1KzZviYOZkAA===)

{//メソッドブロックの中は型宣言か**代入と式**か**システムコール**の文の集まり(ILに型宣言文は無い、ネイティブには型が無い)
    int a = 2, b = 6, c;  // 宣言文: 変数の定義
    c = (a + b) / 2;      // 代入文と式: c に a と b の平均値を代入
    systemCall();     //OS->ハードウェアへの命令を含むメソッドコール
    ;;                //;から;までに何もない場合ILでnop命令が入る
}
