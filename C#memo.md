    Console.WriteLine((void*)0 == null);
    Console.WriteLine((int*)0 == null);
    Console.WriteLine((float**)0 == null);
    Console.WriteLine((Str*)0 == null);
https://bit.ly/3bQe6xf
# C#まとめ

## 低レベル(IL)、型に関すること  
//\(call)[引数][ローカル変数][オペランド]\(call)[引数][ローカル変数][オペランド]\(call)[引数]..の様に積まれる?(オペランドはld)  
//オペランドは引数かローカル変数をロード(ld)したものか、それを消費して返ってきた値。
//call時は引数分のオペランドを消費して引数に代入される。ローカル変数はcall時に領域確保される?(しかしブロックのスコープが寿命になっている)  
//そして、ret時に返り値無しか１つのオペランドが返り値としてスタックにロード(ld)される。 
//ILはスタック、ヒープ、静的領域、引数、ローカル変数、を関数単位(スタックマシン)で制御する。  

- C#の型とILの型  
  - C#の型  
    - 参照型(O型(クラス, インターフェース, デリゲート, 配列, string, object))  
        - 参照型はILでは`O型(IL:＃1❰class ∫name∫❱, ∫Type∫[], string, object)`になり、スタック(値型が動く範囲(スタックマシン(.maxstack ❰~❱)、引数、ローカル変数))に  
             O型という参照?を置き、参照は`ヒープにある参照型のインスタンスフィールドを指している`。 ❰/;＃1: クラス、インターフェース、デリゲートはILではclassと表現される  
        - O型(オブジェクト参照型)は不透明でポインタ操作はできないが、同一か調べる(`ceqなど`)、キャスト(`castclass, isinst(is, as)`)、を使えます。  
        - isinstでアップキャスト可能かどうか分かるので多分O型は`動的な型を持っている`と思われます。  
        - new Cls();, new int[4]; とやるとILで`newobj、newarr`が実行されO型(class ∫name∫, ∫Type∫[])が生成されます。  
    - 値型(ユーザー定義構造体(struct)、数値型(int,floatなど)、bool)  
        - 値型はスタックか、O型のメンバ(ヒープ)に存在するような型です。(IL:valuetype ∫name∫, int32, float64など)  
        - 値型が動く範囲(スタックマシン(.maxstack ❰~❱) + 引数(arg) + ローカル変数(.locals init))は`固定サイズ`だと思う。  
            (stacalloc(:=localloc)はスタックに動的に足される?)  
        - boolはILでは1byteを占め0はfalse、それ以外はtrueになる。  
        - 数値型(とboolも?)はILのスタックマシン上では＄stVal=❰`int32、int64、native int、F`❱になって`それの単位でcpu演算`される。  
            しかし、ILはそれよりも`細かい型`(int8,float32など(大体`C#の数値型と一対一対応`))をもっており、  
            スタックマシンとそれ以外(＄alh=❰引数(❰ld¦st❱arg)、ローカル変数(❰ld¦st❱loc)、ヒープ(❰ld¦st❱❰obj¦ind❱))❱で`暗黙的または明示的(conv)`な型変換が入る。  
        - ユーザー定義構造体(struct)はILの❰∫alh∫❱では`valuetype ∫name∫`になる。  
            - valuetype ∫name∫を操作するためにはスタックマシンにロード、ストア(ld,st)しないといけないため、スタックマシン(❰∫stVal∫❱)とvaluetype ∫name∫の変換は  
                valuetype ∫name∫の中身を❰∫stVal∫❱の型に展開(ld)、またはその逆で収束(st)するのかな？？  
            - ユーザー定義構造体(struct)はStr str = `new Str();`のとき`initobj ∫Type∫`が実行され、
                `Str str;`という宣言だけの場合、スタックに`valuetype Strが確保されるだけ`で何もされない。  
  - ILの型  
    - マネージ型  
        - マネージ型は`O型(参照型)と、マネージポインタ(IL:&, C#:ref)`です (CLR(.net)の管理によって参照先が勝手に変更されうる)  
        - ＃1❰マネージ型はCLR(.net)の管理によって`参照先のオブジェクトが移動`(コンパクション)されうります。  
            その結果、通常は見ることはできないが`マネージ型の値(参照先)が変わっています`。❱  
        - `O型`のオブジェクトは`参照が外れるとGC`されます。  
    - アンマネージ型  
        - アンマネージ型は`マネージ型ではない型と、マネージ型を含んでいない型`。(スタック内で完結しているか、O型(参照型(ヒープ))のメンバ内で完結しているデータ)  
            つまり、値型(ただし、構造体は＃1❰再帰的に`マネージ型を含んでいない`構造体❱)と、アンマネージポインタ(*) です。  
            ❰/;＃1: つまり、構造体(値型)であってもマネージ型になりうる?  
    - マネージポインタ型(IL:&, C#:ref(参照渡し))  
        - マネージポインタは`スタック(メソッド内)にしか存在できないが、マネージポインタ以外なら何でも指せます`。  
            IL:❰❰❰＃O型＃❱¦&a❰＃アンマネージ型＃❱❱&[a]=＠❰{~}❰*❱❱❱& (~&&はだめ(マネージポインタはマネージポインタを指せない))  
        - 参照先が`マネージ型の場合`＃1の説明と同じで`参照先が変更`されうります。  
        - `C#の場合` ref ∫Type∫ = ref ∫vari∫; や func(ref ∫Type∫){}, func(ref ∫vari∫); という形式で`宣言と同時に初期化が要求`される。  
    - アンマネージポインタ型(*)  
        - アンマネージポインタは`何処にでも存在できるが、アンマネージ型しか指せません`。しかもunsafeのみです。  
            IL:❰＃アンマネージ型＃❱{~}❰*❱ (~&*はだめ(アンマネージポインタはマネージポインタを指せない))  
        - アンマネージポインタで`マネージ内`(クラスメンバ(ヒープ))の`アンマネージ型`を参照する場合`fixed()`が必要です。  

Java 開発者向けチート シート
## 言語表現によるC#の構文構造  
`❰/:＄arg=＄vari=＄name=＄index=＃id❰＃識別子＃❱`  
- 定義  
    - 修飾子  
        - アクセス指定子  
            `＄アクセス=＠❰public¦protected internal¦internal¦protected¦private protected¦％private❱`  
        - 修飾子  
            `＄修飾子=❰∫アクセス∫ ＄仮/イ/静/s=❰＄仮/イ/静=＠❰｡abstract¦virtual¦override¦＄イ/静=＠❰static❱｡❱¦sealed❱ ＠❰new❱ ＠❰readonly❱ ＠❰ref❱ ＠❰readonly❱ ＠❰partial❱ ＠❰const❱❱`  
    - ジェネリック  
        - ジェネリック  
            `＄変性ジェネ=❰<⟪❰,❱,1~⟫❰｡＠❰in ¦out ❱∫Type∫⇒∫Gene∫｡❱>❱`  
            `＄変性ジェネ=❰<⟦,|1~⟧❰｡＠⟪in ¦out ⟫∫Type∫⇒∫Gene∫｡❱>❱`  
            `＄ジェネ=∫変性ジェネ∫∠❰⸨＠❰in ¦out ❱⸩❱⇒❰⟪⟫❱`  
            `＄ジェネ＝【∫変性ジェネ∫∠✖⸨＠⟪in ¦out ⟫⸩】` //破壊(と言うより置換?)の記号は`⇒`っぽい記号を探す?トークン単位でない事もある  
            `＄ジェネ＝【∫変数∫⇒∫変性ジェネ∫∠❰in ❱∩⸨＠❰in ¦out ❱⸩¦¦❰abc❱】¦¦❰A❱`≪⟪⟫≫ 
        - 型制約  
            `＄制約子=❰❰unmanaged¦class¦struct¦∫Type∫⇒∫class∫❱¦Delegate¦Enum¦∫Type∫⇒∫Gene∫¦∫Type∫⇒∫interface∫¦new()❱`  
            `＄型制約=❰⟪❰ ¦⏎[Tab]❱,~⟫❰｡where ∫Type∫⇒∫Gene∫: ⟪❰, ❱,~⟫❰∫制約子∫❱❱｡❱`  
    - 型とリテラル  
        - 型  
            `＄Type=❰∫ReferenceType∫¦∫ValueType∫¦∫Generic∫❱`  
        - リテラル  
            `＄Lit=❰∫LitReferenceType∫¦∫LitValueType∫¦∫LitGeneric∫❱`  
        - 値型  //型int , リテラル整数  
            - 型  
                `＄ValueType=❰∫Num∫¦∫Bool∫¦∫Char∫¦∫Struct∫¦∫Tuple∫¦∫UMPointer∫❱`  
            - リテラル   
                `＄LitValueType=❰∫LitNum∫¦∫LitBool∫¦∫LitChar∫¦∫LitStruct∫¦∫LitTuple∫¦∫LitUMPointer∫❱`   
            - 数値型  
                - 型  
                    `＄Num=❰∫Integer∫¦∫Float∫¦∫Decimal∫❱`  
                - リテラル  
                    `＄LitNum=❰∫LitInteger∫¦∫LitFloat∫¦∫LitDecimal∫❱`  
                - 整数型  
                    - 型  
                        `＄Integer=❰∫Int∫¦∫UInt∫❱` 
                    - リテラル  
                        `＄LitInteger=❰∫LitInt∫¦∫LitUInt∫❱` 
                    - 符号付き  
                        - 型  
                            `＄Int=❰sbyte¦short¦int¦long¦System.❰SByte¦Int16¦Int32¦Int64❱❱`  
                        - リテラル  
                            `＄LitInt=❰＠❰-❱❰~❱❱`  
                    - 符号なし  
                        - 型  
                            `＄UInt=❰byte¦ushort¦uint¦ulong¦System.❰Byte¦UInt16¦UInt32¦UInt64❱❱`  
                        - リテラル  
                            `＄LitUInt=❰~❱`  
                - 浮動少数点型  
                    - 型  
                        `＄Float=❰float¦double¦System.❰Single¦Double❱❱`  
                    - リテラル  
                        `＄LitFloat=❰＠❰-❱❰~❱＠❰.❰~❱❱❱`  
                - 十進数型  
                    - 型  
                        `＄Decimal=❰decimal¦System.Decimal❱`  
                    - リテラル  
                        `＄LitDecimal=❰＃十進数の型ぁの数ぅ＃❱`//後でで  
            - 真理値型  
                - 型  
                    `＄Bool=❰bool¦System.Boolean❱`  
                - リテラル  
                    `＄LitBool=❰ture¦false❱`  
            - 文字型  
                - 型  
                    `＄Char=❰char¦System.Char❱`  
                - リテラル  
                    `＄LitChar=❰'❰#UTF-16の一文字#❱'❱`  
            - 列挙型  
                - 型  
                    `∫Enum∫`  
                - 定義  
                    `＄Enum_define=❰＠∫アクセス∫ Enum ＄Enum=∫name∫｡＠❰ : ∫Type∫⇒∫Integer∫❱{⟪❰, ❱,~⟫❰｡＄e=∫name∫＠❰= ∫LitInteger∫❱｡❱}❱`  
                - リテラル  
                    `＄LitEnum=❰∫∫Enum∫.∫∫e∫❱ ❰/;Enumは∫Integer∫と識別子のタプルのようなもの`  
        - 参照型  
            - 型  
                `＄ReferenceType=❰∫Object∫¦∫String∫¦∫Array∫¦∫Delegate∫¦∫Class∫¦∫Interface∫¦∫Anonymous∫¦∫MPointer∫❱`  
            - リテラル   
                `＄LitReferenceType=❰∫LitObject∫¦∫LitString∫¦∫LitArray∫¦∫LitDelegate∫¦∫LitClass∫¦∫LitInterface∫¦∫LitAnonymous∫¦∫LitMPointer∫❱`  
            - オブジェクト型  
                - 型  
                    `＄Object=❰object¦System.Object❱`  
                - リテラル  
                    `＄LitObject=∫Lit∫`  
            - 文字列型  
                - 型  
                    `＄String=❰string¦System.String❱`  
                - リテラル  
                    `＄LitString=❰"⟪~⟫❰#UTF-16の一文字#❱"❱`  
            - 配列(`＄Array`)  
                - 型  
                    `＄Array=❰∫Array[,]∫¦∫Array[][]∫¦Array¦System.Array❱`  
                - リテラル  
                    `＄LitArray=❰∫LitArray[,]∫¦∫LitArray[][]∫❱`  
                - 配列(`＄Array[,]`)  
                    - 型  
                        `＄Array[,]=❰∫Type∫[⟪~⟫❰,❱]❱`  
                    - リテラル  
                        `＄LitArray[,]=❰new ∫Type∫[＄n=⟪❰,❱1~⟫❰~❱]¦new ∫Type∫[∫∫n∫]＄recur=❰｡｡{⟪❰, ❱~⟫❰｡❰＃←のTypeのLitˆ∫Lit∫❱¦∫recur∫｡❱}｡｡❱❱`   
                - 配列(`＄Array[][]`)  
                    - 型  
                        `＄Array[][]=❰∫Type∫⟪1~⟫❰[]❱❱`  
                    - リテラル  
                        `＄LitArray[][]=❰new ∫Type∫[❰~❱]⟪~⟫❰[]❱❱`  
            - デリゲート  
                - 型、定義  
                    `＄Delegate=∫Delegate_define∫∠❰●⸨∫name∫⸩●❱`  
                    `＄Delegate_define=❰delegate ❰∫Type∫¦void❱ ∫name∫｡＠∫変性ジェネ∫(∫仮引数∫)❱`  
                - リテラル  
                    `＄LitDelegate=❰∫ラムダ式∫¦¦❰｡∫Method∫∸❰∫Virtual_method∫｡¦｡∫Tructor∫｡¦｡∫Prodexer∫❱｡❱❱` //∫Virtual_method∫取り込めたが??  
                    - ラムダ式  
                        `＄ラムダ式=❰❰∫vari∫¦(⟪❰, ❱~⟫∫vari∫)❱ => ❰∫式∫¦{∫文∫＠❰∫return文∫❱}❱❱`  
        - 複合型  
            `＄Complex=❰∫Struct∫¦∫Class∫¦∫Interface∫❱`  
            `＄LitComplex❰∫LitStruct∫¦∫LitClass∫¦∫LitInterface∫❱`
            `＄Complex_define=❰∫Struct_define∫¦∫Class_define∫¦∫Interface_define∫❱`  
            - [値型]構造体 //thisはref Str this?  
                - 型、定義  
                    `＄Struct=❰∫∫Normal_Struct_name∫｡¦｡∫∫Readonly_Struct_name∫｡¦｡∫∫Ref_Struct_name∫｡¦｡∫∫Readonly_ref_Struct_name∫❱` 
                    `＄Struct_define=❰∫Normal_Struct∫｡¦｡∫Readonly_Struct∫｡¦｡∫Ref_Struct∫｡¦｡∫Readonly_ref_Struct∫❱`  
                    - 修飾子  
                        `＄Str修飾子=❰∫修飾子∫⇒❰｡＠∫アクセス∫ ＠❰readonly❱ ＠❰ref❱ ＠❰partial❱｡❱❱`  
                    - メソッド  
                        `＄Struct_method=❰｡｡｡∫Method∫⇒❰｡｡∫Normal_method∫｡¦｡∫expr_method∫｡¦｡∫Tructor∫⇒❰cctor¦ctor❱｡¦｡∫Prodexer∫｡¦｡∫Iterator∫｡¦｡∫Operator∫｡｡❱✖⏎`  
                            `∠❰⸨∫仮/イ/静∫⸩❱⇒❰⟪∫仮/イ/静∫∸❰abstract¦virtual¦override❱⟫❱｡｡｡❱`  
                    - 構造体 //構造体はILでsealed。//structはゴミが無く、レイアウト可能(unionとか)、コピーとbox化に注意する  
                        `＄Normal_Struct=❰∫Str修飾子∫⇒❰｡＠∫アクセス∫ ＠❰partial❱｡❱ struct ∫name∫｡＠∫ジェネ∫{`  
                            `＄構造体メンバ=❰⟪❰⏎[Tab]❱,~⟫❰✖⏎`  ❰/; 構造体、クラスのアクセスレベルは∫name∫へのアクセスレベル(∫name∫は型の名前なので静的)  
                                `∫Field_member∫¦✖⏎`       ❰/; コンストラクタは静的メソッドでCls.Cls();が本来でアクセスがクラスのとメンバの両方を満たすこと(普通の静的メソッドも同じ)  
                                `∫Struct_method∫✖⏎`  //ctorは引数ありでフィールドメンバ全初期化  
                                `∫Complex_define∫✖⏎`  //✖⏎を⏎に変える?それとも別のにする?改行の意味より広い意味で
                            `❱❱✖⏎`  
                        `}❱`  
                        `＄Normal_Struct_name=∫Normal_Struct∫∠●∫name∫●`  
                    - readonly構造体 //防衛的コピーの回避((temp=str).func();)  
                        `＄Readonly_Struct=∫Normal_Struct∫∠❰⸨struct⸩❱≪❰⟪≪readonly ≫struct⟫❱✖⏎`  
                            `⟦if⟧⟦this⟧∠❰⸨∫Field_member∫∠❰∫イ/静∫❱⸩❱⇒❰⟪❰＆Null❱⟫❱⟦then⟧⟦this⟧∠❰⸨∫Field_member∫∠∫Type∫⸩❱≪❰⟪≪readonly≫ ∫Type∫⟫❱⟦end⟧✖⏎`  
                            `∠❰⸨∫Struct_method∫∠∫property∫∠＠❰∫アクセス∫ set;❱⸩❱⇒❰⟪❰＆Null❱⟫❱ ❰/;get-onlyだけ許可`  
                        `＄Readonly_Struct_name=∫Readonly_Struct∫∠●∫name∫●`  //●∫ ∫●やめる?nameはnameだし..
                    - ref構造体  
                        `＄Ref_Struct=∫Normal_Struct∫∠❰⸨struct⸩❱≪＃1❰⟪≪ref ≫struct⟫❱`❰/;＃1:ref構造体はref構造体のメンバかローカルしか置けない(スタックのみ)  
                        `＄Ref_Struct_name=∫Ref_Struct∫∠●∫name∫●`  
                    - readonly ref構造体  
                        `＄Readonly_ref_Struct=∫Readonly_Struct∫∠❰⸨struct⸩❱≪＃1❰⟪≪readonly ref ≫struct⟫❱`  
                        `＄Readonly_ref_Struct_name=∫Readonly_ref_Struct∫∠●∫name∫●`  
                - リテラル  
                    `＄LitStruct=❰new ∫Struct∫()❱`  
                - タプル  
                    - 型  
                        `＄Tuple=❰＄recur=❰(｡⟪❰, ❱,2~⟫❰｡❰∫Type∫ ＃1＠∫vari∫❱¦∫recur∫｡❱｡)❱❱`  
                        `❰/;＃1タプル型は型の中に識別子を持てるが、コンパイル時に消失するため属性で保持している`  
                        `❰/;＃1を省略した場合、アクセスは∫vari∫.Item❰1~❱となる`  
                        `❰/;タプルはSystem.ValueTuple＠❰<⟪❰, ❱,1~8⟫∫Type∫>❱最後は↑の記述で8個以上の時の入れ子のための型引数(1,2,..,7,8,..) == (1,2,..,7,(8,..))`  
                    - リテラル  
                        `＄LitTuple=❰＄recur=❰(｡⟪❰, ❱,2~⟫❰＠❰∫name∫: ❱∫式∫❱¦¦∫recur∫｡)❱❱`  
            - [参照型]クラス  
                - 型
                    `＄Class=❰∫∫Normal_Class_name∫¦∫∫Abstract_Class_name∫¦∫∫Static_Class_name∫¦∫∫Sealed_Class_name∫❱`  
                    `＄Class_define=❰∫Normal_Class∫¦∫Abstract_Class∫¦∫Static_Class∫¦∫Sealed_Class∫❱`  
                    - 修飾子  
                        `＄Cls修飾子=❰∫修飾子∫⇒❰｡＠∫アクセス∫ ＠❰abstract¦sealed¦static❱ ＠❰partial❱｡❱❱`   
                    - クラス  
                        `＄Normal_Class=❰∫Cls修飾子∫⇒❰｡＠∫アクセス∫ ＠❰partial❱｡❱ class ＄Normal_Class_name=∫name∫｡＠∫ジェネ∫ ＄継承=＠❰｡: ＠❰∫Class∫❱ ⟪~⟫❰, ∫Interface∫❱｡❱{`  
                            `＄クラスメンバ=❰⟪❰⏎[Tab]❱,~⟫❰✖⏎`  
                                `∫Field_member∫❰;¦❰｡｡ = ❰｡∫Lit∫¦❰＃静的な∫vari∫＃❱｡❱｡｡❱❱¦✖⏎`  
                                `∫Method∫∠❰⸨∫アクセス∫∠❰abstract❱⸩❱≪❰⟪≪not❰abstract❱≫⟫❱✖⏎` //∫アクセス∫∠❰abstract❱??  
                                `∫Complex_define∫✖⏎`  
                            `❱❱✖⏎`  
                        `}❱`  
                    - 抽象クラス  
                        `＄Abstract_Class=∫Normal_Class∫∠❰⸨class⸩❱≪❰⟪≪abstract ≫class⟫❱∠❰⸨∫アクセス∫∠❰abstract❱⸩❱⇒❰⟪＠❰abstract❱⟫❱✖⏎`  
                            ❰/;インスタンス生成できないのでctorやFinalizeが定義できないと思ったが定義できる謎。  
                        `＄Abstract_Class_name=∫Abstract_Class∫∠●∫name∫●`  
                    - staticクラス  
                        `＄Static_Class=∫Normal_Class∫∠❰⸨class⸩❱≪❰⟪≪static ≫class⟫❱∠❰●⸨∫クラスメンバ∫∠∫仮/イ/静∫⸩❱⇒❰⟪static⟫❱✖⏎`  
                            `∠❰⸨∫継承∫⸩❱⇒❰⟪❰＆Null❱⟫❱∠❰⸨∫Method∫∠∫Tructor∫⸩❱⇒❰⟪∫Tructor∫∸❰ctor¦Finalize❱⟫❱`  
                        `＄Static_Class_name=∫Static_Class∫∠●∫name∫●`  
                    - シールドクラス  
                        `＄Sealed_Class=∫Normal_Class∫∠❰⸨class⸩❱≪❰⟪≪sealed ≫class⟫❱∠❰⸨∫継承∫⸩❱⇒❰⟪❰＆Null❱⟫❱`  
                        `＄Sealed_Class_name=∫Sealed_Class∫∠●∫name∫●`  
                - リテラル  
                    `＄LitClass=❰new ∫Class∫()❱`  
                - 匿名型  
                    - 型  
                        `＄Anonymous=❰__Anonymous~❱`  
                    - リテラル  
                        `＄LitAnonymous=❰new{⟪❰, ❱,1~⟫❰∫vari∫¦∫vari∫ = ∫Lit∫❱}❱ ❰/;∫vari∫でアクセスする。メソッド内で完結。new[]{~}と組合せるといい`  
                        `❰/;var ano = ∫LitAnoymous∫というふうにvarしかない`  
            - [参照型]インターフェース
                - 型
                    `＄Interface=❰∫∫Interface_name∫❱`  
                    `＄Interface_define=❰∫Normal_Interface∫❱`   
                    - インターフェース //「フィールドを持てない代わりに多重継承できる」  
                        `＄Normal_Interface=❰∫Cls修飾子∫⇒❰｡＠∫アクセス∫ ＠❰partial❱｡❱ interface ∫name∫｡＠∫変性ジェネ∫ ＠❰｡: ⟪❰, ❱,~⟫∫Interface∫｡❱{`  
                            `＄インターフェースメンバ=❰⟪❰⏎[Tab]❱,~⟫❰✖⏎`  
                                `∫Field_member∫⇒❰＠∫アクセス∫ ＠❰static¦const❱ ∫Type∫ ∫name∫❱❰;¦❰｡｡ = ❰｡∫Lit∫¦❰＃静的な∫vari∫＃❱｡❱｡｡❱❱¦✖⏎`  
                                `∫Method∫⇒∫Interface_method∫`  
                                `∫Complex_define∫✖⏎`  
                            `❱`  
                        `}❱`  
                        `＄Interface_name=∫Interface∫∠●∫name∫●`  
                - リテラル  
                    `＄LitInterface=❰new ❰∫Complex∫∸∫Interface∫❱()❱`  
        - ポインタ型  
            - 型  
                `＄Pointer=❰∫UMPointer∫¦∫MPointer∫❱`  
            - リテラル  
                `＄LitPointer=❰∫LitUMPointer∫¦∫LitMPointer∫❱`  
            - [値型]アンマネージポインタ(*)  
                - 型  
                    `＄UMPointer=❰∫Type∫*❱`  
                - リテラル  
                    `＄LitUMPointer=❰＃∫vari∫は∫Type∫型❰&∫vari∫❱❱`  
            - [参照型]マネージポインタ(&,ref)  
                - 型  
                    `＄MPointer=❰ref ∫Type∫❱`  
                - リテラル  
                    `＄LitMPointer=❰＃∫vari∫は∫Type∫型❰ref ∫vari∫❱❱`  
        - ジェネリック  
            - 型  
                `＄Generic=❰∫GeneType∫¦∫Gene∫❱`   
            - リテラル  
                `＄LitGeneric=❰∫LitGeneType∫¦∫LitGene∫❱`  
                - 抽象型  
                    - 型  
                        `＄GeneType=❰∫Type∫<∫Type∫>❱`  
                    - リテラル  
                        `＄LitGeneType=∫Lit∫`  
                - 多層型  
                    - 型  
                        `＄Gene=❰∫name∫❱`  
                    - リテラル  
                        `＄LitGene=∫Lit∫`  
    - メンバ  
        `＄Member=❰＠❰unsafe❱❰∫Field_member∫¦∫Method∫❱❱`  
        - 変数  
            - メンバ変数//structもclassも同じ  
                `＄Field_member=❰∫修飾子∫⇒❰｡＠∫アクセス∫ ＠∫イ/静∫ ＠❰new❱ ＠❰∫readonly∫¦∫const∫❱｡❱ ∫Type∫ ⟪❰, ❱,1~⟫❰∫vari∫ ＠❰= ∫式∫❱❱＠❰;❱❱`//;?  
        - メソッド  
            - シグネチャ  
                `＄Signature=❰∫name∫(∫仮引数∫∠❰●⸨∫Type∫⸩●❱)❱`  
            - メンバメソッド  
                `＄Method=❰∫Normal_method∫｡¦｡∫Instance∫｡¦｡∫Static∫｡¦｡∫part_method∫｡¦｡∫Interface_method∫｡¦｡∫expr_method∫｡¦｡∫Tructor∫⇒❰∫cctor∫¦∫ctor∫¦∫Finalize∫❱｡¦✖⏎`  
                            `｡∫Prodexer∫⇒❰∫Property∫¦∫Indexer∫❱｡¦｡∫Iterator∫｡¦｡∫Extension∫｡¦｡∫Operator∫❱`  
                - メソッド修飾子  
                    `＄Method修飾子=❰＠∫アクセス∫ ∫仮/イ/静∫⇒＠❰｡abstract¦virtual¦override¦∫イ/静∫⇒＠❰static❱｡❱ ＠❰new❱ ＠❰readonly❱ ＠❰partial❱❱` 
                - 仮引数 と 戻り値  
                    - 仮引数  
                            `＄通常仮引数=❰∫Type∫ ∫arg∫❱`  
                            `＄オプション仮引数=❰∫Type∫ ∫arg∫ = ∫Lit∫❱`  
                            `＄参照渡し=❰❰ref¦in¦out❱ ∫Type∫ ∫arg∫❱`  
                            `＄可変長仮引数=❰params ∫Type∫⇒∫Array[][]∫ ∫arg∫❱`  
                                `＄仮引数=⟪❰, ❱,~⟫❰∫通常仮引数∫¦∫オプション仮引数∫¦参照渡し¦∫可変長仮引数∫❱`  
                    - 戻り値  
                        `＄戻り値=❰∫Type∫¦void❱`  
                - メンバメソッド  
                    - 普通のメソッド  
                        `＄Normal_method=❰＠∫アクセス∫ ∫仮/イ/静∫ ＠❰readonly❱ ∫戻り値∫ ∫mane∫｡＠∫ジェネ∫(∫仮引数∫)∫型制約∫❰｡;¦{❰＃静的 または インスタンスメソッドの処理＃❱}｡❱❱`  
                    - 戻り値があるメソッド  
                        `＄expr_method=❰∫Method∫∠❰⸨∫戻り値∫⸩❱⇒❰⟪∫Type∫⟫❱∠❰●⸨{∫❰＃各メソッドの本体の中身＃❱∫}⸩❱≪❰⟪≪ => ∫式∫;≫⟫❱❱`  
                    - 部分メソッド 
                        `＄part_method=❰∫Method∫∠❰⸨∫戻り値∫⸩❱⇒❰⟪void⟫❱∠❰⸨void⸩❱≪❰⟪≪partial≫ void⟫❱❱`  
                    - インスタンスメソッド  
                        `＄Instance=❰｡∫Method∫∠❰⸨∫仮/イ/静∫⸩❱⇒❰⟪❰＆Null❱⟫❱｡❱`  
                        - 仮想メソッド  
                            `＄Virtual_method=❰∫Abstract∫¦∫Vritual∫¦∫Override∫❱`  
                            - 抽象メソッド  
                                `＄Abstract=❰∫Method∫∠❰｡｡＠⸨＠∫アクセス∫⸩∸⸨∫仮/イ/静∫⸩｡｡❱⇒❰｡｡⟪❰＠∫アクセス∫∸❰private❱❱⟫｡⟪❰abstract❱⟫｡｡❱❱`  
                            - 仮想メソッド  
                                `＄Vritual=❰∫Method∫∠❰｡｡＠⸨＠∫アクセス∫⸩∸⸨∫仮/イ/静∫⸩｡｡❱⇒❰｡｡⟪❰＠∫アクセス∫∸❰private❱❱⟫｡⟪❰vritual❱⟫｡｡❱❱`   
                            - オーバーライドメソッド  
                                `＄Override=❰∫Method∫∠❰｡｡＠⸨＠∫アクセス∫⸩∸⸨∫仮/イ/静∫⸩｡｡❱⇒❰｡｡⟪❰＠∫アクセス∫∸❰private❱❱⟫｡⟪❰override❱⟫｡｡❱❱`   
                    - 静的メソッド  
                        `＄Static=❰｡∫Method∫∠❰●⸨∫仮/イ/静∫⸩❱⇒❰⟪❰static❱⟫❱｡❱` 
                    - 静的コンストラクタ と コンストラクタ と デストラクタ 
                        `＄Tructor=❰∫cctor∫¦∫ctor∫¦∫Finalize∫❱`  
                        - 静的コンストラクタ  
                            `＄cctor=❰static ∫Complex∫⇒❰∫Struct∫¦∫Class∫¦∫Interface∫❱(){❰＃静的メソッドの処理＃❱}❱`  
                        - コンストラクタ//インスタンスが無くても呼べるインスタンスメソッド?,classのアクセスかつメソッドのアクセス//ジェネはクラスに付いてる  
                            `＄ctor=❰＠∫アクセス∫ ∫Complex∫⇒＄cname=❰∫Struct∫¦∫Class∫∸❰∫Abstract_Class_name∫❱❱not∫ジェネ∫(∫仮引数∫)＠❰:❰base¦this❱(∫実引数∫)❱{❰＃インスタンスメソッドの処理＃❱}❱`  
                                //∫Class∫∸❰∫Abstract_Class_name∫❱の他に∫Interface_name∫も??
                        - デストラクタ　
                            `＄Finalize=❰~∫Complex∫⇒❰∫Class∫❱(){❰＃インスタンスメソッドの処理＃❱}❱`❰/;クラスのみがFinalizeを持てる。  
                    - プロパティ と インデクサ  
                        `＄Prodexer=❰Property¦Indexer❱`  
                        - プロパティ //＠❰{get;set;}❱  `/*アクセス: n > n.get、get,setのどっちかのみ*/`  
                            `＄Property=❰＠∫アクセス∫ ＠∫仮/イ/静∫ ＠❰readonly❱ ＃1∫Type∫ ∫name∫❰✖⏎` //＠❰readonly❱は多分structの防衛的コピーの回避  
                                `{❰＠∫アクセス∫ get;＠❰＠∫アクセス∫ set;❱❱}＠❰= ∫Lit∫;❱¦✖⏎`  
                                `{＄getset=❰｡｡＠❰｡＠∫アクセス∫ get{return ❰＃＃1の型の値を返す＃❱}｡❱＠❰｡＠∫アクセス∫ set{❰＃value(＃1の型)が使える＃❱}｡❱｡｡❱}✖⏎`  
                            `❱❱`  
                        - インデクサー  
                            `＄Indexer=❰＠∫アクセス∫ ∫仮/イ/静∫⇒＠❰virtual¦override¦static❱ ＠❰readonly❱ ∫Type∫ this[∫Type∫ ∫index∫] {∫getset∫}❱`  
                    - イテレーター  
                        - Iter戻り値  
                            `＄Iter戻り値⇒❰IEnumerable<∫Type∫>¦IEnumerator<∫Type∫>❱`  
                            `＄yield=❰yield ❰break¦return ∫Lit∫❱❱`  
                                `＄Iterator=❰＠∫アクセス∫ ∫仮/イ/静∫ ＠❰readonly❱ ∫Iter戻り値∫ ∫name∫｡＠∫ジェネ∫(∫仮引数∫)∫型制約∫❰｡;¦{❰＃∫yield∫を含む処理＃❱}｡❱❱`  
                    - 拡張メソッド//classもstaticにする  
                        `＄Extension=❰＠∫アクセス∫ static ∫戻り値∫ ∫name∫｡＠∫ジェネ∫(this ∫Type∫ ∫arg∫, ∫仮引数∫)∫型制約∫{❰＃静的メソッドの処理＃❱}❱`  
                        //`＄Extension=❰＠∫アクセス∫ static ∫戻り値∫ ∫name∫<T>(this ref T ∫arg∫, ∫仮引数∫)where T: struct{❰＃静的メソッドの処理＃❱}❱`//refはstructのみ  
                    - 演算子のオーバーロード//❰readonly❱だめ//後で======================================================================  
                        `＄Operator=❰❱`  
                        - ユーザー定義型変換  
                    - インターフェースメソッド  
                        `＄Interface_method=❰❰∫Interface_normal_method∫｡¦｡∫Interface_explicit_method∫❱∸｡｡∫Interface_exclusion_method∫｡｡❱`  
                        - 除外メソッド  
                            `＄Interface_exclusion_method=❰∫Extension∫¦∫Tructor∫⇒❰∫ctor∫¦∫Finalize∫❱❱`  
                        - interfaceアクセス  
                            `＄interfaceアクセス=❰｡∫アクセス∫∠❰⸨public⸩❱⇒❰⟪％public⟫❱｡❱`  
                            - インターフェース普通のメソッド  
                                `＄Interface_normal_method=❰✖⏎`  
                                    `∫method∫∠❰｡｡＠⸨∫アクセス∫⸩｡＠⸨∫仮/イ/静∫⸩｡＠⸨＠❰readonly❱⸩｡｡❱⇒✖⏎`  
                                    `❰｡｡｡⟪＠∫interfaceアクセス∫⟫｡⟪＠❰｡∫仮/イ/静/s∫∸❰override❱｡❱⟫｡⟪⟫｡｡｡❱✖⏎`  
                                    `❰/;実装側のアクセスはpublicのみ可能`  
                                `❱`  
                            - インターフェース明示的実装  
                                `＄Interface_explicit_method=❰∫method∫∠❰｡｡＠⸨∫アクセス∫⸩ ＠⸨∫仮/イ/静∫⸩ ＠⸨＠❰readonly❱⸩｡｡❱⇒✖⏎`  
                                    `❰⟪⟫⟪＠❰abstract❱⟫⟪⟫❱❱∠❰●⸨∫name∫⸩❱≪❰⟪≪❰＃定義元のinterface名＃❱.≫∫name∫⟫❱`  
    - ローカル  
        - ローカル変数
            `∫Local_variable∫`  
        - ローカルメソッド  
            `＄Local_Method=❰∫仮/イ/静∫⇒＃1＠❰static❱ ∫戻り値∫ ∫mane∫｡＠∫ジェネ∫(∫仮引数∫)∫型制約∫❰｡;¦{❰＃静的 または インスタンスメソッドの処理＃❱}｡❱❱`  
                `❰/;＃1:staticだと静的ローカルメソッドになって静的メンバ変数以外キャプチャしないのでオブジェクトの寿命を伸ばさない`  
    - 名前空間  
        `＄path0=❰＃名前空間~∫Complex∫までのパス＃❱`  
        `＄path1=❰＃名前空間のパス＃❱`  
        `＄=❰＄recur=⟪~⟫❰｡｡✖⏎`  
        `⟪❰;⏎❱,~⟫❰｡❰using ∫path1∫❱¦❰using static ∫path0∫❱¦❰using ∫name∫ = ∫path0∫❱¦❰extern alias ∫name∫❱｡❱`  
        `namespace ∫name∫{`  
            `∫recur∫`  
        `}`  
        `｡｡❱❱`  
- 使用(演算、宣言、生成、代入、分岐)  
    - 型  
        - 宣言  
            `＄宣言=❰∫Type∫ ∫vari∫❱`  
        - キャスト  
            `＄キャスト=❰(∫Type∫)❰∫vari∫¦∫Lit∫❱❱`  
        - is  
            `＄is=❰∫Instance∫ is ∫Complex∫❱`  
        - as  
            `＄as=❰∫Instance∫ as ∫Complex∫❱`  
        - default()  
            `＄default()=❰default(∫Type∫)❱`  
        - typeof()  
            `＄typeof()=❰typeof(∫Type∫)❱`  
        - sizeof()  
            `＄sizeof()=❰sizeof(∫Type∫)❱`  
        - nameof()  
            `＄nameof()=❰nameof(∫Type∫)❱`  
    - 生成  
        - 複合型  
            `＄複合型生成=❰new ∫Complex∫∸∫Interface∫(∫実引数∫)❱` //∫Abstract_Class_name∫も??  
        - デリゲート  
            `＄デリゲート生成=❰∫Method∫∸❰∫Tructor∫¦∫Prodexer∫¦∫Operator∫❱❱`  
        - 配列  
            `＄配列生成=❰❰｡｡new＠❰∫Type∫❱❰｡❰[∫index∫]❱¦❰[]{⟪❰, ❱,1~⟫∫Lit∫}❱｡❱｡｡❱¦¦❰｡{⟪❰, ❱,1~⟫∫Lit∫}｡❱❱`  
    - メンバアクセス  
        `＄Instance=❰∫vari∫❱ = ∫Use_ctor∫;` 
        `＄メンバアクセス=❰❰∫∫Instance∫¦∫Complex∫¦∫Lit∫❱.❰∫Member∫❱∠❰●⸨∫name∫⸩●❱❱`  
    - メソッド  
        - ラムダ式  
            `＄Use_ラムダ式=❰∫ラムダ式変数∫(∫実引数∫)¦((∫Type∫⇒∫Delegate∫)(∫ラムダ式∫))(∫実引数∫)❱`❰/;キャストして戻り値と引数の型が分かれば名前が無くても呼べる  
            `＄代入ラムダ式=❰∫Type∫⇒∫Delegate∫ ＄ラムダ式変数=∫vari∫ = ∫ラムダ式∫❱`❰/;ラムダ式には型情報がないのでターゲットの型を見て型推論する  
        - メソッド  
            `＄Use_Normal_method=❰∫Normal_method∫∠❰●⸨∫name∫⸩●❱(実引数)❱`  
        - コンストラクター  
            `＄Use_ctor=❰new ∫ctor∫∠❰●⸨∫Complex∫⇒∫cname∫⸩●❱(実引数)❱`  
        - 静的コンストラクター  
            `＄Use_cctor=❰＃最初の∫メンバアクセス∫時＃❱`  
        - デストラクター  
            `＄Use_Finalize=❰＃GCのコレクト時＃❱`  
        - 拡張メソッド  
            `＄Use_Extension=❰❰∫Lit∫¦∫Instance∫❱.∫Extension∫∠❰●⸨∫name∫⸩●❱(実引数)❱`  
        - プロパティ  
            `＄Use_Property_get=❰∫式∫∠❰●⸨∫Property∫∠❰∫name∫❱⸩●❱❱`  
            `＄Use_Property_set=❰∫Property∫∠❰●⸨∫name∫⸩●❱ = ∫式∫❱`  
        - インデクサー  
            `＄Use_Indexer_get=❰∫式∫∠❰●⸨∫Indexer∫∠❰∫name∫❱⸩●❱[∫index∫]❱`  
            `＄Use_Indexer_set=❰∫Indexer∫∠❰●⸨∫name∫⸩●❱ = ∫式∫❱`  
        - イテレーター  
            `＄Use_Iterator=❰❰＃ループ構文の更新の所＃❱⇒❰●⸨＄iter=❰｡∫Lit∫⇒❰＃Enumera❰ble¦tor❱の❰∫vari∫¦∫Lit∫❱＃❱｡❱⸩●❱¦¦❰∫iter∫❱.❰＃イテレータのメソッド＃❱❱`  
        - 演算子のオーバーロード//後で==================================================================================================================  
            - ユーザー定義型変換   
- 構造化  
    - 式  
        `＄値=❰＃戻り値がある0引数のメソッドや∫Lit∫、∫vari∫など＃❱`  
        `＄関数=❰＃戻り値がある1引数以上のメソッドなど＃❱`  
        `＄式=❰＃＄式=❰∫値∫¦∫関数∫(⟪❰, ❱1~⟫∫式∫)❱のようなもの＃❱`  
    - 文  
        `＄構文=❰＃コンパイラへ指示。修飾子や制御構文、セミコロン、ブロックなど＃❱`  
        `＄文=❰＃＄文=❰∫構文∫¦❰｡∫構文∫ ⟪❰ ❱1~⟫❰∫式∫¦∫文∫❱｡❱❱のようなもの＃❱`  
        - 埋め込みステートメント  
    - 変数定義  
        `＄変数定義=❰∫Field_member∫¦Local_variable❱`  
        `＄Local_variable=❰＠❰const¦out❱ ❰∫Type∫¦var❱ ⟪❰, ❱,1~⟫❰＠❰@❱∫vari∫ ＠❰= ∫式∫❱❱❱`  
        `❰/;２つの同名の識別子の片方に"@"を付けてもIL上では同じ識別子として扱われて、キーワードにも付けれてキーワードを識別子にできる。`  
            `❰/;それによって他の言語で書かれた識別子がキーワードと被っても"@"を付ければ使えるようになる("@"は識別子である事をコンパイラに教えている?)`  
        - 代入  
            - 代入  
                `＄Assign=❰∫vari∫ = ∫式∫❱ ❰/;代入という∫文∫と、代入する∫値∫を返すという∫式∫、という両方の側面がある`  
                - タプル分解代入  
                    `＄TupleAssign=❰＠❰var❱ ＄recur=❰(｡⟪❰, ❱2~⟫❰＠❰var¦∫Type∫❱ ∫vari∫❱¦¦∫recur∫｡)❱ = ∫LitTuple∫❱`  
                    `//∫Complex∫のユーザー定義タプル分解代入拡張メソッド`  
                        `//定義//public static void Deconstruct<T1, T2>(this Tuple<T1, T2> x, out T1 item1, out T2 item2){item1 = x.Item1; item2 = x.Item2;}`  
                        `//使用//var tuple = Tuple.Create("abc", 100); var (x, y) = tuple;`  
            - 引数渡し  
                `＄実引数=⟪❰, ❱,~⟫❰∫式∫¦❰∫vari∫: ∫式∫❱¦❰in ∫式∫❱¦❰out ＠∫Type∫ ∫vari∫❱¦❰ref ∫vari∫❱❱`  
    - ステートメント//困りはしない?  
        - 制御構文  
            - goto  
                `＄Label=❰∫name∫❱:`  
                `＄goto文=❰goto ∫∫Label∫;❱`  
            - if  
                `＄if文=❰＄RecurIf=❰｡｡｡if(❰＃∫LitBool∫を返す∫式∫＃❱)❰＃1∫文∫¦{⟪1~⟫∫文∫}❱｡＠❰｡｡❰else❰ ＃1∫文∫¦{⟪1~⟫∫文∫}❱❱｡¦｡❰else ∫RecurIf∫❱｡｡❱｡｡｡❱❱`  
                    `❰/;＃1∫変数定義∫は含めいない`  
            - ?:  
                `＄3項演算子=❰∫式∫⇒❰｡❰＃∫LitBool∫を返す∫式∫＃❱?∫式∫:∫式∫｡❱`  
            - switch//また後で==================case int n when n < 100: return 2;=========================================== 
                `＄switch文=❰switch(∫式∫){`  
                    `⟪❰ ¦⏎[Tab]❱,~⟫❰｡｡case ∫式∫:`  
                        `⟪❰ ¦⏎[Tab]❱,~⟫∫文∫`  
                        `❰break;¦∫return文∫¦∫goto文∫❱`  
                `｡｡❱}❱`  
            - while  
                `＄while文=❰while(❰＃∫LitBool∫を返す∫式∫＃❱)❰＃1∫文∫¦{⟪1~⟫∫文∫}❱❱`  
                    `❰/;＃1∫変数定義∫は含めいない`  
            - for  
                `＄for変数=❰∫Local_variable∫⇒❰❰∫Type∫¦var❱ ⟪❰, ❱~⟫❰∫vari∫ = ∫式∫❱❱∠❰⸨❰∫Type∫¦var❱⸩❱≪❰⟪＠❰∫Type∫¦var❱⟫❱❱`  
                `＄for文=❰for(∫for変数∫;＠❰＃∫LitBool∫を返す∫式∫＃❱;∫for変数∫)❱{⟪1~⟫∫文∫}`  
            - foreach  
                `＄foreach変数=❰∫Local_variable∫⇒❰＃1❰∫Type∫¦var❱ ∫vari∫❱❱ ❰/;＃1が必ず必要`  
                `＄foreach文=❰(＃1∫foreach変数∫ in ❰＃Enumera❰ble¦tor❱のインスタンス＃❱)❰＃2∫文∫¦{⟪1~⟫∫文∫}❱❱❰/;＃1はreadonly的`  
                    `❰/;＃2∫変数定義∫は含めいない`  
        - 例外処理  
            `＄Exception_handling=❰＄Recur=❰｡｡✖⏎`  
            `try{`  
                `∫Recur∫`  
            `}`  
            `⟪~⟫❰｡｡｡catch＠❰｡｡(∫Type∫⇒❰＃Exceptionを継承したクラス＃❱ ∫vari∫) ＠❰｡when(❰＃∫LitBool∫を返す∫式∫＃❱)｡❱｡｡❱{`  
                `❰＃例外処理＃❱`  
            `}`  
            `｡｡｡❱＠❰｡finally{`  
                `❰＃リソース廃棄＃❱`  
            `}｡❱｡｡❱❱`  
        - usingステートメント  
            `＄using_statement=❰using(∫Type∫⇒❰＃IDisposableを継承した∫Type∫＃❱ ＄Resource=∫vari∫ = ❰＃nullかIDisposableのインスタンスを返す∫式∫＃❱){`  
                `❰＃∫∫Resource∫を使う処理＃❱`  
            `}❱`  
                `❰/;ブロック{}を抜けるとDisposeが呼ばれる`  
            - using変数宣言  
                `＄using変数宣言=❰using ∫Type∫⇒❰＃IDisposableを継承した∫Type∫＃❱ ∫vari∫❱`  
                    `❰/;∫vari∫のブロック{}の範囲はスコープ全体になるので注意`  
        - 戻り値  
            `＄return文=❰return ∫式∫❱`  
- クラス  
    - 初期化の実行順  
        ボツ[Init.cs](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABAJK5jADeOtHtyAzF6rQGUAruWIAnNtk7Su6AGy0wuYLVK4hhWgF5a6ANwB6A9xDDRYwJrpgELdAqwyBihkA/DIGuGQJMMgXCVA+OaA0ZXYyOSlXVNHW5DAwB2UxFxe2d3b18/ZHlBaLEACgBKST8k9ABOdIASACITM3FrWNdPLxLMvUTcjmTC0oAWKPNrQGaGQGeGQE6GQAmGQEuGQHqGQH6GesbsJuajYQAjfuHxicBrBkAPs0BZBkB9BkARBkAZBlrAWjl1wB8VQE8GQCsGfcA7BkBzBkBlBietwHkGOdzW4pKAVijFtV4nUGp88m0SnIuuI7oAvN0AIDrTMG0AC+yJ4qXMWRyzU431KkQqYiBtSRUlxLQKPwAHNCxCtRpMyc00eSOKzpBi0KlFnScXkFAFVEFtLQAAxhdAAqqOGoJNkyIUinSoML/JYk+XNZIKJbY5GcqmlKVLGVxUmghXao0lVDSqwMtbM3Icr68PXZHAGvE2/IAx1My0spqulq8bkAGXwQlwVAAFvyZGhUInrRCxQC4YigxTaLhiAB3HlZGbBhWslFAA) 
        修正[Init.cs](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUFAQwFsBTAZwAcCBjEgAgEkoBLYAbzs7oHpvAQf4BKJahDABeQA7kAZQBiAWgB8UgMKKG8hYGoiISLCa1ywI7keLnVQBmM+jpSIFEmDYnTXVJgBsdZlGB0yUCCI6MTp0AG5eAFYQW3swQE10wBC3QFWGQGKGQB+GQGuGQEmGAApZAEpAXCVAfHNANGVnFy8fOgCgkPMI7gB2GLsHNKy89WLyypc3T1iHXIKnXCqqtwBOXIASACJo4YSUjJzSsoWCsP7Js0xZxfc2uKTAZoZAZ4ZAToZACYZAS4ZAeoZAfoZt3YnJgF8910sV0fG+1MM3mC1aK06G3Kbx+U0OoIAHCA3AA6ADyACMAFbCYBXO5PZ6AHxVAM4MgC/FQDqDPkNCo1BoGMoCjCPkCDkcFtNTg58Q8Xsz9t8WXRBcDLBgbBAMVzHLCzGKPNVfP5AsE6AAGJqYNoYpKQvKFTay7y+OqqzBNczavW5HqGoXAhW2DEA2UO9lap269bZTb81ls0HobUXG68147PCuri8J08wmAawZAB9mgFkGQD6DIARBkAMgybQC0cvGiYBPBkAVgzpwB2DIBzBkAygyVpOAeQYo5wQYtLU69b6I/bJk2FgAWaWlwBeboAQHT9VRFXb+kpduAbAcWmDV2rjfI7Av648bYusABkCNBqAALQFd9DoY+s3gAfVVTpRyqIYR43FkihUgHsGR73a7xoqAQIZAGYMgBFDI8gDdDNcgCaDIA0QyABYMyaAPnagAyEYAqgyADEMkGzt2i5OoOI6rv6UAkAA7hKzp4WO654J8QA==)
    - 動的な型、静的な型、アクセス元、アクセス先  
        [ILとメソッドの種類](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABALLHAAWA9jANJi4wDeOtQbWQBmWvgBGpYACdqwYaloBlAK7liM2v2xC9wscnQA2YQBYVACgCUvAL76A9I8Ca6YBC3QIcMgXoZAwwyBJhn1A/RBhEwEg0XNaAEkbeyCnR0AShkBnhkBOhkB+hnTvfwTaEO5pfFwacMDIyWk5KgVkCwBBGwBuPWdAXqNAQxjLQDu5QGeDaxyAvJDcYgB3UghWBUrZeVoANzAZYFV8CFpC4GLS3QixJZW1jbraADU4hyFnPsG8oJHxyenF5dX1zdwikuIy2js/s5RhMpsBAPYMgCxNQBgSoBABkAICqAfFdAAhGgHvlQC/AYArBkA9QyAS4Y0oBrBkAAHKAEwZAFoMxMAQgyAaIYztsJBBiPjAA2mgHUGQAiDLdfH5AB4MgDsGQC7DKkMoBf+MABUqATQYqYBouUABgyACwZALgGgDEGAB0Kr+kTQKlUEhAag0Mh0CUcACpaZIGSbaAB9WkABVkTRNjnutBuvUGfMFKQyvNxaV5gAiGJUYwBBDLzLO58YAs7UAfgyAYwZAGYMbKp1j+ekirAWmhkYBgdFOjVsVzdjkAVQyAH4ZAAsMFZxST8PX6dxCh3eGy2O1+e0EAO7pcAIP+DXmAOi9AHoZAF5AA7kk8ABP+T4HPYCAR3JaHPZtVl6vJ63jiuOz8lwAyozGJeAxz99yDKeAaiIh2Ob4AlwknJ6XZ77zn7gBtFQAAirzUpk2Rcre96jk+s7zk8oIrmuUhzDUMHbm8u6fN8NBvue/YAVkaTXpON6/v+6Q4YM4H7uh76tBehGcv4t6EdhQH+DegAERJOgAUdHOO7rEugCUdEuz6AFfkSFHDxgDX5JR1wXp0eEEX+tF+KxHFzgu0Fbuu8h8QJk7CapLwaQhEmSYIn4KfR8kelySmcZBILTIhBnLvxQm2YuS5GZhgBQcoAH2ayTRlnMWxNl6ZusFVJpzk6R5H6OHeXIjmBz6AGjkgBzxOeeg3leVmHql6VCHJRGAbhVnJWlMUJDejHFcxOVlVRFX+aRpV5fcN4ydluXla6N4KTetUtXkN4+aR/VdYExmlnoE3OO+dhAA==)  
        [仮想メソッド](https://drive.google.com/file/d/1OIWXOpBnGQMefxL4qLLCwGlYo_isn-Xz/view?usp=sharing)
    - CS1540  
        [CS0122とCS1540](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABAMIDK6ArACwAMA3jrX7cgDMtfACNSwAE7VgA1LQAiPbP1W1ykgPbBiVHTBHipMgWxHIWACgCUAbl5q+G7bv2naANws3ljx8nQATksAIgUQuwdHAF8otWcdPWIDAIA2d1Jva18/VQDgsIj7FT9Ykr4yvOE0BnRaEEVaHMcE12TaTQ9iSUkwGDpkM3ws5tyBIND6dCK41Ur+eb4hOQYOesVR1Vakg07u3v73Yats2b98yY4Z8rVFtTRUTb8AemfLSwVrXGIAd1WbawAOmONlsryYrE4Z0cr0APiqAZwZAF+KgHUGQDCioAIhkAUQyAKDlAHYMgGj1QDmDIBahkAxwyAToZABMMgE0GQDRDIALBkA+dqAGQjAEJmgGsGQCrSoAQt0AVgz4wAyDIBVeUA0gyAMCURYBIhkAWgxUfAQCAeMCSYCEwCqCYBZ5Tp4qlvMAIgzQtSvQDNDIBnhkAiwyAEoZAJcMgHsGQDRcoBZJS5fPxgCSGQARcoAaO0JgFkGfWAIAZDao4Ui0YBTuUA0HK8yWABwY5QqlSr2ejaZHMbixYAEnVtCJRqKjvNx6o1yMDwf4ofz9O2+lxgGTU9lkqkCwCmigzALoMgGUGQnZ5GAMwZANxGgGMGftd/WWwCHDIAfhkAHQwVvivSzfP5TAGArwnYpjSvPQB3coBng0ngF6GQDDDIBJhnDgDRNQmAIIZABUM1tpuNtgDhooM3XJTQGZLcL2ivIAmuncieF62oA96SGncZRlOCjAcOgqDyMBoHnjgyw1PQHCMFwtbtGk7gAAocD4zwAFT0KgjCAgAsqCUzUYR6CgmRzxYYxJF2NEZQYfIDEgOxuFaIkbgEYMtBMaRFFUbRLFsegjHMXY7GAsRoLceh1R8VRAkKUJLg7OM6TiXR2SUdRpm2AxqlKVZ2GqZxtgadgOBwQhSH0hC7BrJaprkoA/Qx+ahmkrFhACS+kifhZhqdk5ErgwqBhTYsl2AlUzJUCkl2Kx6UcJlDnqTxWm1GFAn5ZFbQpDFSlcPFvyJQVpmsbY6XoAV2VgmReUdY5rHObxjW6RFeHVbQplcOlSUpZZ9Wru1KWdT1i19c8zmuWxzDebQh6obQliRm6eYDrS6KAGIM7KANGRgASDLSAoJoqyrACmmKAFeBYpHWG/Z6v2mrsjG+rWCFmEcAAapVhlJsAACu8ruKDPhTQjQLI61DVTMjgKo3lmOowNJUY+V4OjR0XQ9H0AxmMjk3o6guOgm19N2HNqxM7Y/XFaFdO6cTwlVaT+wU/DiO02zLMYyl2Po2DktydxQA)  
        追記[CS0122とCS1540](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABAMIDK6ArACwAMA3jrX7cgDMtfACNSwAE7VgA1LQAiPbP1W1ykgPbBiVHTBHipMgWxHIWACgCUAbl5q+G7bv2naANws3ljx8nQATksAIgUQuwdHAF8otWcdPWIDAIA2d1Jva18/VQDgsIj7FT9Ykr4yvOE0BnRaEEVaHMcE12TaTQ9iSUkwGDpkM3ws5tyBIND6dCK43MtLelRrXGIAd1qbawA6YatI8rVK/iO+ITkGeQaprhOBavl6DnrFUdVWpINO7t7+912fWZ+fKTDgzA6qW55VCoV5+AD0cPmCmWawYHE2OyytgRTFYnFoCMAzQyAZ4ZAIsMgBKGQCXDIAZBkA0ZGACQZAGYMgBEGQDRDIA7BkA9gyKTF7QA+KoBnBkAX4qAdQZAMKKIEegLGMtlsoRCl5Nk5VHwEAgHjAkmAgHMGR5K6yAJIY+YA3BkA0gyAMCUzYBIhkAVgzM6VqBGC0VihbogCy+DAuGs1kAEQyAKIZAFBy7MA0eo6wC1DIBjhkAnQyACYZAJoMrMAFgyO1QIwD52oAZCMAQmaAawZAKtKgBC3W1h6mAVXlLTbAFoMqvVmu1OsAqgmAWeVk9W7Q7wfC4SSKZTOYBouUAskpF0thw2ACLlADR2OsAsgz2wBADOn+M7heLAKdygGg5W3WwAODPWNVrgPn/azt4H2RbAAk6nJdW937JbrZFy9XfHXrpT7307vZAD81jsoAyan5jGCbUoApoqpoAugyAMoMOq3iKjKANxGgDGDIy8H2uSgCHDIAPwyAB0MH4EoiKzrFMGJeHsxRygigB3coAzwa4YAvQyAMMMgCTDJugBomjqgBBDIAFQyUhynKAHDRK49o4CKAL/xgAYUSAhKEpyjzsoA9QyUrGgAGDDaAEkRmcKAIGRyb5r+yScoAmukTjqsHsqZMDUrJgAP8YAqgyADEMjJhkG242ny+72imzKAIoM9q6ZJYwIgA2g+gASiuGAC6AA8AC0UUbmK8UhAlAC8IQRZZZaJVlEUlgVtCWBFpAAK7kN0iUpVVohxdYelrnC+a2qy/qAGIM+bJgBliAMmE7Ldfm+WToA03IochjIioAfgxdeyhqAExpgAKDM1YW5JFY1xbQyW0KloqxWGhXFROcUtZ+cJ5WdmVJblD7pcdF2keyKbznZ+Ztn1JFTFsmQ0SRCKWaxnGcoA96SOrcZRlDijAcOg0K0MD7EcTgZw1I8jBcHZ4zpIMtAAArotkcIAFSLIwWwejYthTJTBPoDTpNwpjWxEzT0RlOjDzoIwkocFjONpO4DM+GTFNU0zLO82zjN2Kz7N2JzaP3BcfN09jWiJG4wv49T2QS/rtMy6L8sC2zxO2Mr2A4LD8PQimuLsE85LErGgD9DG7IOo9g3NogAkprLgfCLxNcGT5EXP7NiS3YkdTNH2ym7YzPxxwicWxzXOqwn/OB0LZim+HpPx6gGf68ztjx+gGfJxHqKPLXlvM9bfuLP7ko10H2vtHrPil+XNP1xRNcx8nadN1L1u2yzzDO7QTHe2V27GhujKdT1DKstSR6NqeQaAFeBFqr6KLKMm27VdutbccAAat3bQGHvlVqu4t/9w3qDv9s39Vw36Dfy2L/NOgDf6txzgA/m98cZfB6H0AYZhv5cFLqAmm1dUF2GHmiDBKc4TgPOIsW+ndoFa0fh0LocDfj4yQSgmOv8sFTBwSAuhU9ohAA==)
        //↑(下を呼び出してるように見えるがcallvirtにより自分を呼び出していて違反にならない)は、審議 //インスタンスメソッドでCS1540も謎
    - CS0052  
        ```C#
        public class C {
            private class Pri{}
            public Pri pri; //CS0052: 型(private) < インスタス(public)、なアクセシビリティはだめ
        }
        ```
    - thisの型  
        クラスのthisは`readonly Class this`  
- インターフェース  
    - インターフェースはフィールドがない抽象クラス  
        [interfaceまとめ](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQYB2wApgE4BmAhgMaEAEAkgSQAwDeA9G/kWVYYPYMgVYZAxQyAfhkDXDIEmGQDIMgQGNAZgyArBkAiDIFO5QGiafcgCMAzsGJVggCwZAMCqANBhw0rNDlxp4A3B0CHDIGeGQAsMgSE1ADqZ9Ad3KAzwaA1gwKgJEMyoB2DIDmDIAlDK6AnQyA/QzxPoBJDIDF2oAAUYCaDIDRDEaAFQyAlwwiltZsAFSAoP+ADuTIAMyAjuQVbDbVNdp6BpTAza3IACz0AILIAKwAFACUTmzOgL0MgMMMEoBZ2oCV/nyA1ER+bDFsgJrpm6mAgO6Apq55RoELy0FlVpW1DX1ttQBuYMTAAK7kEM+D9AAauNpiwAL4cBS5aJxJLxa4SIJ8MLKQAbliFAMYMciUuTuNB0hF+hBgNABdAAyiCpuCOASiTBUoANuUAigz5WHJBEhXHYaw0AAOxDAr3IRDaeAA9sAau9Pj8/i1SUM6AAFQVUmlsAVCkWEBRXJYSPiAQkclDCEhyDVy8QLJYQesTFfRVeL1RDNcRbfaYHwAMLk9BjAZMIyAOzNAFgJEUA+dqAUYi+A0jEpAGIMgGi5QDSDKEuCQKNRAKGKEUA/gyAf3lABSugHvlQC/ATgwThMzxqPRGMR0DQQA3uKwOIBkwlr2cIObCgFqGQDHDPFABMMRsA6gxRQC6DBFAH4Mc8APiqATwZubyyY2mAA6OijSbUujA/cOFUeqkON093gRQBVDCJ3CIijFDYA4M0AXJ6AELcYwW8Z19IZHQYdsdyPaZZkAWMVAF6jQBDGMANGUqxwP9umAUlUBoZ0AHMWzbEh0BYPEGkA0DqTdcMo1jBpUkAUf1AC0GQAPs0AeQYOCA5ggmHMcokAWQZcnUPh1jDQAohgiQB9K3LK0eWsDdgMPV0OHfD9QzDKRAEI5QBO00AIQZckAFg1AAgVQBABnkqR9RuKE8QI+pHRtDDQTxXkZOPXlHKcjgyNUqRF0ABwZABiGNcnJoCYJhY4gmCmYAAAswB0KYQPPOTPwiNzPJ82zrACoKQvCyLot3WLOEbXsIkAcYYBEASoYxFHGM50AfQY+HkmM+Cvag2JHUc8iowBAyKkTSUqsNLN1CiKop3Sljx6/zAsbdABqy4bcvY0dAAlFKRGsIKJ1K8jz0qiQAghnQHalCUfMqqUMaOD69tpqG08XWPDg/QDIM8WrbBnpwIA==)  
        [interfaceまとめ追記](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQYB2wApgE4BmAhgMaEAEAkgSQAwDeA9G/kWVYYPYMgVYZAxQyAfhkDXDIEmGQDIMgQGNAZgyArBkAiDIFO5QGiafcgCMAzsGJVggCwZAMCqANBhw0rNDlxp4A3B0CHDIGeGQAsMgSE1ADqZ9Ad3KAzwaA1gwKgJEMyoB2DIDmDIAlDK6AnQyA/QzxPoBJDIDF2oAAUYCaDIDRDEaAFQyAlwwiltbI6ABsNHZ4AK4AtjQAvDQALA5lVsgAzDQV1chtNDqkdXiUABQAlCwAvl01BPQEAEqEwDOd2NZLwCvA68AACnUQ5DroMyzIAOwHRzM08DToDgs71mwAVICg/4AO5L1AI7k3zYNj+/20egMlGAILBQ3oAEFkABWLYuQC9DIBhhgkgCztQCV/nxANREfjYMTYgE104mpQCA7oBTVzyRkCzhxEiCix+AOBoPBAIAbmBiMA6uQIPD+sM6AA1NHXOYcBS5aJxJLxVm4oJ8MLKQAbliFAMYMciUuUWOkIYsIMEl9AAynLZgq2ObLTBUoANuUAigz5VXJDXspWLAAOxDA/PIRHBeAA9sB/oLhaLxbzEXRjqGHfMOCGwxHCAoWWy+IBCRyUKoSfrZIVNnysIdjhFhVptaeI0czTvrRCbMD4AGFbehUW0mEZAHZmgCwEiKAfO1AKMRfF6RiUgDEGQDRcoBpBlCXBIFGogFDFCKAfwZAP7ygApXQD3yoBfgJwH23PGoBxI6BoIEfxFYHEAyYR33eEPdhQBahkAY4Z4kACYZi0AdQYokAXQYIkAPwY4MAHxVAE8GGtdlTRh3wAOjoFF0VmGUHQcVt23w95bEw38IkAKoYRHcEQihiCQ+EAODNAC5PQAQtznI9FihfRDBbTCmBw2UyI4QBYxUAXqNAEMYwA0ZRvHA0BodNowAcxfN90BYRZej2B4NmuO4aFQd4dL6VNRPlDhJxnedelSQBR/UALQZAA+zQB5Bg4BhuCYIJgLAqJAFkGXJ1D4QkJ0AKIYIkAfStL2rHSpSEkSOw4diOPHCcpEAQjlAE7TQAhBlyQAWDUACBVAEAGVKpELTVA1rGgzJtetVOuRZ0PQABOSYOq85hpmAAALMAdGmHC1g2U5zkuGZpm2XZdkIsiZoW3ZrKnbKpEQwAHBkAGIY0MWzqhJ6/rBqSsS2FSiJVo27bmusPbvIOgahtwoiKO4KjAHGGARAEqGMRQLnODAH0GVjOLnPgf14XyQNAvIHMAQMipHy66rFu7q+oenD7TIxGaGR4h0Huo66AxjE2D80DAAlFKQweoKJcs29auvfKJACCGdBmaUJRD3+pQsY4HGmHxx6VOetgByHEcsYZ4TRnGKYpsWD50NQVAWEmPBCAAd2UttGumIaGq2D4Pg4QBf+MADCiQBhwAQX3ASjeEEURJFkRQlEAY7lAHiGcKgkALE1AFnEwBM30AfyNAprDhFu6HoFH9QADBk9KnCHUKJaPoxiJGDnBQ7DuOE/+QALRTkCJUrnIEjH+JOGKYwBVBk241lHTsEw92OOIl9BJ10qiQgVSRDADcGYq+FJqRYZwIA)
    - thisの型  
        インターフェースのthisは`readonly Interface this`  
- 構造体  
    ref構造体はスタック直下であるがマネージ型も持てるためアンマネージ型でない場合もある
    [構造体まとめ](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABAMrABOArlcAPqH7AD2JAbxy0RtZAGZaYXMGJMAZtToBJAAwCAvhKky5imrWXpNw0dtLM2wBuo2mR2tLQDCqgPQAqEPVUe3mtzdAactAAgTAZXlAewZSYnwIYhhAVQZAGIZAMwZAEQZAaIZ7MUkLVnYGEE9XKD81KCMBQMB6hkBLhkBOhkAghkBM30B/I0BYOUB75UAgBmlZBSUIwCxNQFnEts7AX4CACkBahkBjhnrACYZAITMIwB2GQGGGavrANGVAGQZAd1jAWQYMgEps0VpAvtpcVVoAXloATgBuQKHAMCVAQAZAVH1AB1NAFYMgHUGQDNDIBnhiWtWqgH6GCKAVYZAMUMgB+GQDXDIBJhkASQyAHoZALcMgFwlQD45jtAJoMGUAdgyAcwZAIcM4MACwyASE1/oBrBiJO0AJAqYwDHcoBTRUpR0BaQuZkkN1w6FewocopktHI6gA5sRgK8NIFQpF6jCtszALJegD8GJIUymAcYZkTDAK0MgBKGQBdDBF0AA2OlWxaAU7lANBye0AAOaASwSpTlaEwYjBeLgIABPHTWJiqSXYS6XQLaehTU4BYLhckAkEQqGwiLOADEzMAEQyAYwYUki0Vi8WyyYALBjSgDEGQD6VhNKYAvtUAWdrJdJZeMJ6UMKY3Qhp/2JtzIACsqfebmcPnQAA5lyBABaKKXJVYxtLpe0ATkqAKIZAMr6KUAkQxG/WU4AACzApCm1MAvQwbdEAflOmMA/vKACQZBROojio8tCEPOgDJhFA4HgQCgBCDEeuaLNCcKbtuKK7vS5JspigCV/oAm0qABtygCKDJe/aDoO8ogWB6qZu+iHIeSSrAAAtKGEaAOsMuKAIsMgBjDIigAeDOSvJPq+6KnBEdIbDM4KAA0M8KAOUM1bMoAL26AKXGvJkoBIgxlR85BvgIZhuGET0bC5KAP4MbKTICVKAFIMRFaWIs6nPOm7HmeSSAA4Mf5keRdgDoOjgACxOamJgBbQ/nZI4qAuLQQgRZcaCxQl5EJvQtCkPcTy4MQADuw4uYEO7orWxKORlpDoCBuUFSm05FW4cFHhEbmnikvaZI5yDoM8UwACQAERZSA8VZQAdOKGhQGNqjjfK02zeNMYaINLndb1A3Daoo0CFVk3oIte3oPNqhHftK1rXGaX+Zc/kaEAA==)  
    [構造体はメンバのrefを返せない](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQB6XABsgMwAEy6AbKQE4CmAZqWAHbCkDK9DAhjQOYAKbp2A1SAZzEBKAN7IA7LUaSxAOhYBXALYBuAL74pNTQGNggOwZugU3NA+gyAghgsXAtHKBquMBJDN0DmDIBX4wNoMgNEMBDj4ABbAwAAOEiD4mgymkZEadMC4UpowAJ64phKhfJHpkQD63Grh2hAA/JEAvKik+NzuPoCaDDgsPNp0EpE8pnSiJuZljLI4OKTT5GTGZuwcYvJkIkviAEqpJcChYBLCKuvkyLv70qR1AHzKTMinexIGUzOvb+8fn1/f3/j3ZxJAPYMgCwEgIWQDWDADAYADBkAgQyAIAZAFYMgB0GaEvaYkciNADCpAm2E+mOQABZSABZQQXfE/YaqcR1UgsOgAd2GlN06K+azEtBKxkutzpai2wB2j0OTGM0g5BJpH3wgGTCCyANqdAIM27kAjFFtWyw+bmQAvboBoOQsgEOGQDPDIAFhkAfgy2QGAK8DXIIAYJACQKgGj1aTSQGAIeVAOaOrT8iMAIgyAaLlwYB1Bls8M5cvluEAqPqAB1NADK+gEZXaGACwZ3RY/YBCO3DgGuGQA/DAWvIBI7UAFoqIv2AbiNAFEMLsA0gyAZoYzYBFhkAJQyAS4ZADIMgEGGQDlDIBhhkA9QxeQCyDAFo7LY3L8IBdBnsi8AgAx6ywAnvcREWQGkAECpms9bstoBDPNwArDEWe4B3WMAygwBRuASIYYzN9Jz39hOUTGgARFbMGwjLPDOsxYqQGwASIrDsCKJS/pS8hKCIgiHqQCGehoBifrO7z4H6bqCIASwxFoA6wzSKalo5v6/gBMRZGIk+gCdDM2ZHuIAOUaAIoMgBmDPYJGkc+gBSSoAJmmAKoMgAxDMG0ivrO+G+m6FiAJ2mPaAP/agC+Kj2gDuqUGOrLu6PryeCgAQKoA8QyBDq4JGtRAbBneGYRrYgD4/zgn5GGICycFBdA8DAAD2LAQFkgowaQACalw3CIJQAG48BAmh0AA2gADAAuiB+CCNiADEpAABxqMl0j0D5/mBbcgK2NCJV+QFQVru4JqAL0MQ6AJMMgAayoAsvJeDVZVZBG3GAExpgDaEYAIW6XkW7iDUGXhBgE0KcdCgDGDLYITpO55ieYA1ESAHfygB/GYACtpbYAS4SAA7kmK9XVgCO5KQW31i6J2ndBbA3eF1yCjFcUJSl6WfkAA===)  
    - thisの型  
        構造体`ref Struct this`  
        readonly構造体`ref readonly Struct this`  
- 初期化子  
    [初期化子まとめ](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHqAQwFsBTAZwAdCBjUgAkFwlQfHNA0ZUAVtAbz3t/tQAM/TABYA3Dz6DhAOgDCAewA2S0jWABLBVHITcffkNSYAbLMU7lpPQdQBmfunpyQxuwB4NUYAD5u+gykHAEkAUSgIMjBCYAUwT28fegBxUmBwyNJo2LAACgBKTlQAdno4UgAzQgglYDEAX0lA+jCIqJi44TsZVPS2rI68wpKUtIz2nIKGpsD7fhF6AEE4OFyvYHooQoB1MA1gUgAZL1JcgCJlhDP6RE386YDmubkCzl39o5PzwFR9QAdTQCsGQDqDGd7o1HrMHC91vRiIUoPQALz0SgAfQA5mlEcj0WlyJikcQxPR3gdjlBTmc/oAhBkAUQwga63WEPZq8Sh7ABuMQY6wA2gBdeiEMBgLHkgDu9F56D5NhZc2hwAAFhpyDzoRo+ZwMcAiqUhWA1TL6nidfrDVjOUoINZ6mCWfwHNCoLKno7vNjtVq0sy5W6NqjtSavXUTT7eHapOgHP57RgnJwAPQJlgcQD2DIBmhkAzwyAToZABMMgEuGQD1DIB+hkAdgyAGP1ANYMgCKGQuAboZs4B75UAvwH/QCHDJnAAsMlcAf9qAwBmDCnAJoMgGiGGb2pMIqfTmezufzheLpfLlfjllyeg0IRI8XOTjwpEiGAe3H4+hH+g8gQCw/HnmYG/n+piJOAKoZAGsMgA6GQDlDIXc4ALBkAEoZM0AcYZAGGGQtsxTdg12aJNdxeAA/QpEPqUszgKM5ACSGQBAd0AU1dR3/P4gSzPMi2LQAZBkAHxVAAcGQAYhjHcF7RJT5yVyAASM4txkKAQE4bioHqY9uIDNI+JEnFgCEzcBBkUTgBNcTZPkk1pK3K8+SUjS1IEe9NP43SH3qEEXXtegk0AVMIrOsmzbLs+yHMcpznJc2DAg3GhMFFUgJRee43IMTyeKxcQAr4IKVLPUKmJZTyNJC0z7Tih8ErC3gWLJU5OKC3j+MwHi1Py+TxKKyTCrkyTFLyirAzSQqNJK7ThMwPTGqMky0vMhMVx63q+v6hdOoMDynB3HznFyERCkwY90GPOxnyTdNABuGQtAHaGQBzhkzaCk0ARMIPx/P8gNAiCoLYdh/0Ad9tAB0GOj/kAEQZOsnAaXtet7p0645yGABJfHoJRt02cavp+9Y/Bm+g5voBaXwTFb1q26CnoTNwsMAfO1AFGIgdAEAGS420AXoYwMASYYsMAQGNABMGQt82zQBzBhpwBZBkYsy+Esly2fZjnOfsz6VVBxJ/q8saJRB36fCmTqlHyy5ckwfyYuaSWZGl9A5eZ3hFeluxVbV573r1/WlyGvhORFYhCEobyJQAEQ0dQtCIMAAE93GMARjzBzhOHoM4AHlyTOY8vKEr2zgAFTFBQA8h+hg+90PFTAUhSCjhxbUSnWEzxwmiYo9A/lLQAhM120tAAiGQAxBkrQAPs0ARQYmZZCNwzweogA)  
- ジェネリック  
    - 型制約、変性  
        [ジェネリックまとめ](https://sharplab.io/#v2:CYLg1APgAgTAjAWAFADsCGBbApgZwA5oDGWABAOJYpYBOAloQPoZoAuA9tgN7Im8lQBmErRQsaAMyKkAkqJoAeESWkAaEmwCuLEgHkAfJwD0hwJCagcgNA9gwix1ScUAWDMCwAbLAHNWWQHYMgfwZA98qAvwE8fCFQcABsuro4ANxhkdLKscaAmumAIW4mgA6mgFYMgJEM5haAhI6AnUoA/MEhvIJROhicblgsMQC+JMaAjoomABSaLACUFtRYaMBsKM4AnmmA1gyA/vKAYgwVldWJ0nU4jS1thoCzyt0iAwDudGJjkzMLS81L1bAkAMIk8gAqaqq6eiCyNvKq+txISp8ao6XR1BpNa6AoH8ISJBhrGJLUJw5TrRqcBEYEgAXhIADc0M4NFgWlCYbCSE5XB4xLoYCQACJQACsihQyjgal69L0XWkcGEfRixhEID2gEAGXogTrI4FCMIABhIAFlGgALNjAeQAVRUAHU+TqSBo1PqSIc+sZACD/gGj1QB3coBUfUyOMADuTyO56glEkksCZ4UgAPxIhsAjuRdACEEeNpqj5stcqBh3VNFIOpA/BgicqydTIYzOBY1A0hBYOL0JoAdM82ABlIsiNxdYXZ24AFlVzYBFJC3wUSpUbAARgArLCliuEZV4qiHB7sliD0fjxdKvnC4zZkIdbq9AaATN9AP5GXkAiCqAWQZh2PS4AEwiV9kAx3KAU0VTxfl6WcUrADIMgAA5QAvgYB1BkAMwZABEGQBohkAIAY9i6A4LD/f9smArdgTgZUcFxVUNS1LoACI0CHQhsLUOAYAEDdDBAEA1RYTVgDtQB87WdN0PRQtRrAEGA9HDKNFTUKM4D6JDeGMZ0vEAaiIABlaBYUTAGkGXJbU/QAK40ANajAFUGQAYhhAiDnTkwALRUArwHWdQAkhkAcGNACztQAFbQQ+wVI0hDBP4OAAE4uhwFtoSBck+G8qoFQZAAFag2DcbsYVgGAwp7b1qBIQhcVnedrCXK9VxQ9ckU8ntCErFVm0y6KhMMQBZRMAdCULFgQADBkARQZAEsGQBPJ0AUiVpllLKKSVABtABdEhC1ivFOBIbCAEFCKGgAhMbsPubCSGaArCsvFduvUUd0L6havL4K5HOMABaA7DqO46TtOs7zouy6rr2xzt0MDrmx6+Q8Q69IerxLpAFWlNI+nAp8LHSbJbS6QBS40AToc+mmb6gZ6Uc+mMwBQxUAUGVAE0GUDAFmTQAdeUAcwZACCGQAhBgg26+GMApzC8QANuWAwB9BkAfoZAB+GQAmhkAYYZAAmGLJpifDTQMAITMJXJ4pABzzT9CfA4misAIoZmcAdYZAFqGQBjhkAToZWZmJ80a8CxAGNrQAOPXF7ZABIFW0LHMbGzwQ8Ce12wxrpt227fto7kGaIA=)  
        [ジェネリック変性](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHgegMEhNQcgNB7BkA6GQcoZBZhkCuGQYYZB6hkDsGQIoZHB1hkFqGQY4ZAnQyAJhkBJDIHvlQL8BgawZAmgyBohkBBDK0IlSrAJZRygTXTAIW6ArBkDR6oGkGQJEMgU0VAxgyAzBkCyDIH0GQyJmAFBkCrSgcNTOvQUPkAFHoAtJrBAHweAJQqAPZQwOQeRmaAX4rWgCIMgPIMgEJmUnpG3tz8woF6ADzBsRDAwdEqZOTmqYDZSnk+JUKAMgzmcoD6VhIA/A2kgKoMgDEMjlKAx3Lm8jnkgIAMzW1FvsJS+nKAEQyAYgz6yU7m4+PWgGxKgCC+XYCXvllbgJ2mgIgqm3IjE44igP7yuwuLHX49OSsQDmDJdAU8XoMgrpACuEHkAV4Fuf7CAaDGK4Ihkd6aYAAUzAADMAIYAY1xrC8gEdfQAAcoB1BgAMuoAM7AcoASQS+PZsAA8jVwuEAAQAG2ZwEFgGjI+wZVhdQA68uM5MMMQQ4LjhbiAOZEvEKQAwKoANBkABgyACwZALgGuzweFQAGZBRhBQAhIlM3EAbzwgq99rtADd1GBgBAicL7QAWQUANVQAFYAlE3QBheJM2IagB0AHUwOo8QEAETRmP5qIAbgAvnhK7hbfb0IKAMoQABGAEFBSAnS73Z7vbXYr78Tm1eGo7H40mU2ncVmc3n8zzYyWK1XrXacfjiWTBRy8WBypodzBBdVxTzwh7cN6fYKeSe3ZrccAV1e+3a72zx2zBeoy72vUQgAlDIAzwwCIA/QygYAhwyAL0MjCAJMMSSeF0zqugEJLCkyjoNlEwIFIYXRNm2AQbmADaOlEJoZLsgBtToA0oZdIA7rGAMoMgBRDLsLGLP+gpEB4Jp6F0gBgCZsNgZNYgA+KoAngz6BktIgWBgA/DIAqwzUPJAimoAWdqAJX+wK2NJgBADCCjRyUpKkCMq15EPoII8oOYDDri7wwfBIhiYAbgy0sY+mALoMrArO0xQAr0emrjWdoOomGHsse54djunL7my0VCgA7gAFviuI7p2KGZWlGW3tl3aXtetZ3rED5Pi+JV2hFTIBGVCbAKlzLprEgoALwnlVb63juX4/gmRCEa2Zg5WsnRdIAXm6APiutKZEqXHXuo6ZFvGpaLd6yZQKmGbZrmuIFj+ID5ogy0AOJPgAKgAngADgdUR/q+15eltO0znt85ZfmgqIIKwB3bisQEgEbJRL9go/cuG1erEK3jk9L2bVOu1zgdP2xMdiBwxdwA3fd8aI0j9qYAAnIdPLHRDAP3cD9WPetz0vagADsXVcdW1a1uFGGxbuXI5cew1Cm6RC1YAeuk0LQgASTlxtbDfej7PtW1WCrVAQK7EjXNUyrUdV1Ks9Qrn5xjlA2iwQw2jd240AjNc0ZAtTNLfDcZE0jb3TrO+2HeoWPnVdgOE4zxNqyjH1o4dbJU39NNAyDw3g39UPuy9cOran16e6jPsY1jOOBwTDMwyT5M/ZTP2x4DdM5cXzt9mzsQh96nNrnWauCsVfboPWXdI0QvpEmAgroZhDb61AuLJWrkWC42LatuEAST9Pw3ByXg/D6PWET1PM91Sv89EQzYsYZLdCy/XXrbw26Ym8ve85evV8jxhWF3+Oh9r3Xocl/z+7DWPDlIUJEyL6xvs3fuBBQGOg/nGQ+T8T4EEADAMOVACwDGYZBw00FSGIIAbeMdKAD8GQA2gyAGSGEuZtmz6xgXAh+q8F7P1DkNBegomTUPimRWhX8GFINQd2NBIgsELxwYAdLNACbeboXBeD5BMRNOQIhZDgovxLhzKsQA==)
        [ジェネリック展開](https://drive.google.com/file/d/194I5BFV4gN6ubJ8FpbMIPu-Qi3vnADg-/view?usp=sharing)  
- スコープと寿命  
    [スコープと寿命](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQA7AhgLYCmAzgA74DGxABIJ0MgzQyA/DIOsMgFgyD/2oL4qA3jrUG0CJCtToBJACL9sQ+bWQBmRalrTeAek2B6hkCXDPUCwKu0C6DIDsGQIcMgZ4ZACwyHA9gyHAskqAZBkC6EYECvQJoMgaIYAFIAwKoAaDM72gNREACoATgCuVMAA9lFh7IDqDIBmDID+8oDxDIAxDD6A+gyAgQwAlAIKFcqq6lIADLIVjUJVyOgAbLRguMA1tQD6tbQAvLS1ANzlTZUqrR1dPdL96H4lQwB8tOgTclOCAL6TUy3tnd016MO0Stu7gsdzZ9KoK+u0ACw3jQc7+4ctagAFKIJADmtAajXuijeigArH4AEYJBIQYRQBIxHrzWiEEq8QiXcbfJpoNQQ260bQgiAJBH4CAgEDOQBeXoBk1IAdPp6OzrDY/FySjzbIcmn1LtTafTGUw2FxuOzpAqpEraiqBp9dmKRhK6QyQDKODwlcbFmrliUNbsqTTdYyWRyuUKbCLGlraDqpfqWIb5Yq/VIti6Km6PXqDXKTcqnitLVbNGEAMIJQjkCDEAAeKXMvNZgHMGZzubw+QCncoA0TXsgBAVQAmOn5/SVc55UoArBkAIgzsQC4BoAxBhwQYUmgAVG7FmN+5pe/JsYtCbHGuSKacFgHp+PXZcpyNpIGfvO5/OhJOpGoRuMV5q13VzxcN4eZ7tiXvBEOL9eLgPr6hR7eFPfbk/Bm/znPD9NDGSlNAPWooE3KCb1PMDAC/1QAac0AELdAGsGQBITUAB1MzANexADWGQBbhkAYYYdAAIl4PZSMAUMUzEAfwY4O0LCzB4exAGLtQAAKMACIZACiGQBoOUAaQZAEiGQiSLMQAhX2ybjSMowA/BlzPwomIfAYF8JxUIAdyiMBgGIQBVBlydIWx8QBouT8QAuj0AfO0m0AXCVAHxzQA0ZRKMpsDgg8ADNBmPL95DcpI/Hcq9RjGaQ3IuAAeTYxifDzOlIBcajc1A8W0QAi1Mw9gwDcwBw0zLMwwFIMwsMALO1AEr/HDvXscxSJWai4InR4pDclQvO0QBrhjYGjipK+xMhbQA9tVSXN7FbOqhGizyEtfftr0S0dzyasZtBC5ZfKiMx2tYLCSh6/rUkAIAZVqUqgAAsdoGoaRu3CllsuLcKW+BjNAI5hAGqGVq0tcs4qEuD5tEAHxVAGcGQAvxVSNTAFqGQBjhlYQBBhgh/SfCwwAkhjs+yMhbXJAAcGFsW0e2hnretK2K47inFzTJAAkGHw+00QATBhk7jAEAGAEAE9gGOhJcEAeH04IAN3wKJaGZy4PL8JLvP3M4RexdMSmGDZ01oftaCobZHr0QADhkAAYZuauyFYTC1pIIXDZQsuOLXnmdlomZgF+dIYg/FIKBaAxHo+YFmXaAAfloBWQFoABaO7bm0c2Nl4S3rfZdk9jMTjO1QjXtbMRDAGV5exeBEnQ9lzQAntRk/JigNQAghizqi4NCvxSPQVAlFIi15G0QBEwgAGS6ABHXNOMAeQYcnyR7WgATj8GXQO0WhAGUGCfc192gevJy7HrSjLss+xcADEJpPXXKnhaRN9i+LcGd134sUtzcAtTLaykA/8vi3EKLgt1cEuc/X5GQgvx/Whvj2IA==)  
- オーバーロード解決  
    //追記: オーバーロード解決は名前空間の優先度の解決の後にある。
    [オーバーロード解決](https://sharplab.io/#v2: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)  
- 型の決定と型推論  
    代入:ターゲット = ソース, メソッド:void Func(∫Type∫ ターゲット){} Func(ソース)
    型の決定は、`ソースまたはターゲットの型情報`によって決まる。(`代入のソースの型が分かればvarによる型推論`ができる)
- デリゲート、ラムダ式  
    [デリゲート代入テスト](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUFAQwFsBTAZwAcCBjEgAkHGGQK4ZAmhkB+GQCYZBjuUFNFQMYZAnQwcA3njri6qAAyTMAFgDcYiagDMsgGyT0dAKIAPYKNwTTk9akxbUcvYdQBWABTAAFgEsydAJJ0iASmFLAE4nACIDYEcw/wUAX2VxBJMJRPM6dyhgEjAAMxp6b2MzFQtbb0cnQJDw7yyc/NonQDu5QGeDQEOGQF6GQGGGQEmGfxj4tNM1OjISAgAbEjhJcoBlCem4SurMULC67LyCp0WpmacAXiPMsmACKFp/AdjkkvFRy2sF1aD12vqdpufboZSkmk0qMqGBgO4ptpZAB2YoPZ50UHgyE2OgABQIYDeNTCGKxDkG9wkRMeFh0ACECOM4SVRgA3dxgiAo2wANWxHzClPGIFZuWg1EJaRJ6SREMmUMs0JA3JIMF8NLMowA9nScmB3HB6Kj2c41hspbz+VchQClVZEZjkRLUXjVkozSMLJgZAYKGAqkcAHwRfTusIOh7pBFSujKig5AjAZVgRBOUNEKQwGF+TCBYbwznhyPR2ODDO06F0LX5CCTYCBh4ip0ZLLo4QAcxIFerEgA9G3Rlrpg2o9rbAAxKqV2nqby6KAQUhgKMxgA8mWAXp82yq7w23m2g0smDoACMwBMANb/IMF4O2ACyzdcyrga/Ppg7lh0ABFWQO6Ec6G5PAA6XURyDOgO3fIdYkfds20AOdI2i6PpACkGQBFBg7MhXExChJgIPdAAMGQALBjgnpekAewZAFg5QB75ScABVKAD2PbDpj/ABBMhxiIPdJgATwAcWZMA4AMWgKHBZUoBAQAcS0AHijAHMGQBZBkAKwZABEGQBdhkAEoZAGeGQB+hjYQBZk0AHXlACSGQB87UAGQjAHUGQA/BkATQZACAGfxIPEZ8HAXLJl1fNEv3RICgw7QA0ckAdYZAFuGQBFhj4QBihgopxACLUwAHUzwszADMGQALCMAP+c5KU+zHQeZ9MDfbxP2/JwnG8fxfzIfw/wqBxvOyttX3y4cHJAmDFwaApADsGQjEJQts0IwrDcLw1rvhITqOiIwAghgopqXzoer9mWAq6CKkqyoq7wFpmRwapKealhmcCdrMDtoOGxpRvGA57w0gRNPUgR4P6YjkJm3K5o2pbvEq+Ztqa+bDqak6zo6wBNdMAELdHuepDAc7N6CB0b8pSOp8218wBmhnujhAEuGQB6hk04jAGj1PDAFiowANaMAVQZABiGaastpZzx0nadZzAFyl2XAcJynAB9dwv03HJkYkDnGZ5qo/0vVUSAAORIQxGtp46YNUvhABuGNhtJehWRje8dDE8lbSo8cq/0iX6tYkXXgHl4CoOgwBDo0AVn1Hs1m3HkwIsiH1qASAAdxhKp/DoRBlu9v2pQDwXHmcqUYBjqVlzRSYIC8QqKCTCg0y/Zc06DxFMEj9Ek7IIrQ/9m4YCcUvw5uCDzccmDABR7QB070ABW12sAKoY2EABYY2ECtgSKxwADhkAAYZAHh9AzAG01CzAG0GSnAAcGRSkMAaIYZujl1lwMTy3TAAuagMCOYeg2X3XIMh3FEgBaPc73cGYXdd2a8U8u1qqavFrddk6xSmJxAAuEwAwJUev4e+JRWyPHQDoRUQYq7uzFteNwd4P6gOFHgOIQA=)  
- ローカルメソッド
    [ローカルメソッドまとめ](https://sharplab.io/#v2:CYLg1APgAgTAjAWAFADsCGBbApgZwA5oDGWABILcMgPwyDVDINcMghwyC9DIMMMgkwwDeyJXJn3A9H0DjDICuGQE0MFQBMMgaPVA5gyBITUAOpoGsGRq0BJDIH1zQNh+gTQZA0QyALBlUtAoYozA/vKB4hkAxDHsUAKQFUMgNYZAHQyByhkD1DBICUgewZjTS1AdQZAfQZFQDsGXi4BQH/tQF8VQBkGQCFfSz1APwYZYzMrW0iExMAOeUBnBkAzBmD0wDEGQCAGe0paRT9PQEuGQE6GCMBVhkBihgoaFiVAcDNAHgt9H2ieJG4SAXr6ZhY/QGoiQBKGPkBNdMX7QA+zQGkGQCsGQHkGUsAghkArwMBVpUAQtyVALxtALO1AVQZbH0BABkAihk9AboZWveDxgUAEQyAYwZSs1AAcMgAGGQDw+jJjrNjAZAOSagGV5QBSDHtABoMgHpjPSAPCDACIM43GUAAzCRYCQAMIAGzQOBwHEmU34fAAvOyOZyudyeby+fyBYLueMphTALUMgGOGQDrDIBBhglv0ZzKV5LJUAALCQALJYAAuAAsAPbAAAM9h8iuVloAcgbKWg8GaANwiy3cACWKB1JBQAFcMCRWSQ4DASc6ma7mXwAFSAUH/AA7kUDgADZAI7kUb45I1NrtDvNLojUw9XpQAZIarDBddRe9ftLwdD0z4gHaGCUsCjLQDPDIAGhnsAA4AHTGnyANE0IoANaMApEqABW0DIAYFUAsCrj6dqO6ASv99AYJ4BQBiXU6Uez0+ddAF8j1NT+HXQJAKmEt7v94fgA1lJRrS6BQC0ckpAKaK1lCgAMGQBFBgfYCHzPbhSUzLVdUNE0AH0zQtSsuGze14J8CsCwvSsiywAAndBqXJZNIJQvA0MQpDqxQWDS3LMCuCw5VGKVAQhTY9iOM4vkjwpbpehYOgO0ABYYIklWUJQoy0IKo0sYAwiMIPVKD9SNOAEPo5kADE3SwalgCdDSpmjFADR1BNkzTDMlO03T9PNAREyTNRADgzQAuT2uQANuQAwxX3heYDEAACiQReQzlRLQNyyQyN1jfYTFEAU7lRzEuUXPcpRAEDPQAja30QBlBgMRIZEAWQZD0vStjyipCBCcNwvAkCIEjUQpAAcGB49nxMZSqYjSbxAkDn07VpAH6GDtWg/b9f0A3reo06TPW9GjAzkmbVQ1bUVOAOByNCrgbL0tD5JPDS8Fwt0ADc0B1UhrJ0vb1M6iNqNog7LWY5lXqMtkuK+76fq5HiYHIagaEiZKJOWoiAbWmCYDuiqSBkwNQ22kgABkDUIAA1NBqQM+7XSgAB2VH0ax6lnoLYzTPM1N00gtHMex2G4e4cKy3JiN3q6vGPt62ZIkANqdAGlDNRnDIJhPB2QBIhkAHKMAJOQBd+UAQGN9FCQBAhhePmFAMPz1G6vgWlaNRAB15EFAF0GCIBuGw3AHxXQAEI2CUpLZGwAZX0ARlcZE1tRAEcjSopum7nuGOs6LtIHAsGxrBgHJAGABE3XwWkAE82A+wAY/RePi+kEoTjnMSoDANvx9Xj45FcAEwZkdm4t2arT08IIumSexraA4ep7kc5pVO+ZRTg2U6GW7huOE7QROSGAeO8FLFAsAAdxIYe8CT3G4Yn/B+1ZpHW+ZNe8H7enSf2yuiZn+fExh3f96b6lYPQjTu9Y36n+ftij1ei9XqAA)  
- refのルール  
    [refのルール](https://sharplab.io/#v2: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=)  
- readonly struct  
    [readonly構造体](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHqAQwFsBTAZwAdCBjUgAjFMLgHsoAbAT0GnLQAgTAyvIBvPPTH1UAZnrlgkGsHoBlEbnHqJ0gJZRFAQQDcojWKn0digEJG1JzRIAs9ALIAKAJQBeAHz099T3pLAPoHGzt6AHpIsyYWdm5HegAlNy9ff0DgwLDjE2iADgA6AAZouLZOLkBDhkBehkBhhkBJhkB7BjqmwFDFQDsGQFWGQGKGQB+GQGuGRsAPBk7AIoZAeoZAboZAToZAJIYKhK5ACwZAdQZAMwZARyNAEQZAJjTAbQjAELdAZoZAFYZ++bbGwA1lQFl5QHMGPcAFBjz6AF832OZKxNl5RTJFRvdTfeJVcy6PwGdTlH7LGRyCAKZqAC0UNj0BsNqoBnhkACwyASE1AA6mi3hVUA1gyATQZANEMgGUGFbzQCV/oBNpUAG3KARQYQeIwb8uJCrDDxNFgAALLTkQBBDEwAGaMMmJIH0UXi+jkwBWDNSuWJomZUE40j4/CFsqFwnZdfrMB4hJ9bGJbaD0NJVBFUJgAGxy8GJJQyEJQUgAd2UHjNJjdnqWEMVYHI/qDKSUoa1EnQ6HoLoi4nIRTSYazboAnK4ACQAIhzehAQkr7xgMiKlmrOcs7zL7nzETh3q4atxeM6gBIFQDR6q5+AJ3OTALoMnXagAuE+YEwAvZmtAGIM1Ou80APiqAHwZVysaSsU7DIodThdXK5gJ5yO5cx53IAZBkA7rGAWQZNXas+pfeRMPHg0mHZiL6wCCleN6YHeeamJgxblr+RRVjWmCIXWMgoU2yGNm2HbHuIsa5laHbRECzRRn8SIKJ0gD6DIAgQxnuc/TNIAXm6APiuaxqjseB4dqkTkXy/zIoogAhDF6vLXM0DEXPMLyAAYM7IPIA9kqADrygAUrix7GcTxUSRIxJyzIAEwzzIAhI6APIMlJ0eSHKADEM1JqoAkQwctRgDeDKuj47DsKyALgGq7adEgAlDNU/SAB0MRzkoAcWmACY6YVicsRT8fF8p8oAfgzUc09AAHKsAA+glWW5UlRT5fxR6fl2kQag5UlMWxHE7I+fl8YVeU5SVlX2dVGmHj5KYOuItrvEAA===)  
- イテレータ  
    [イテレータまとめ](https://sharplab.io/#v2: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=)  
    - Linq  
        [Linqの仕組み](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHqAQwFsBTAZwAdCBjUgAgBkBLKAR0DsGQVblARX0H8GAN556I+qgAMYzABYA3MNEB6RRKkA6Fu3n5cosQGYpANjHp6AUQAewActSZ9gCwZAa8qBjBkBmDPf2AkhkAr8YE0GZUAjhkAphkBJhkBrBkAeeXdAEQZAaIYFPRFUQ3sTLwAeABUAPnoAZVIAG1IaYAB9HNyACmAAC2ZyKX1q+nIAewgwOhgxAFYcvrz6ADNmYuBSMABKIV1kxdGOsFJaevoagDdCMHpLelZ2rp7SOeVAZQYLwHMGQFR9QAdTDi8OQEDIwAJfQBkGZ0AohkAvxUBVBkAMQyJBaLMFSTBiADsYwmUzANUsM1kykAogmAN71ACFuEV+gGyla6AWQYQeC9ABfJLJUl6CnknR6DCGebgjBmJkkkQscjATKsYD5YpNYD0AC89CgpAA7kxBTyoHyBJg+ug+vo+tJSdoKczMABOGoAIkA4wyAVYZAJUMgGuGQATDPrkVqwctVusaryxYcoPQBVy5nb2fY9VBbaCSbSfck/QaADpQQBXgYAdeU0nEAh0aAVn0iqVyhVAIcMgF6GQDDDGEbdp2SIHWsaPVnXLXUdPcA1GmypUau7hfl3QAqeiYcTiGbeoPFqT+5H0ZQNJotWV847dOgRQARDB4Gxmp7kIoBo9UAFcaANaigYBAd0Apq6Ae+VAL8BobJodpF9wpKAA=)  
- usingステートメント  
    [usingステートメントまとめ](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUFAQwFsBTAZwAcCBjEgAlQAZBOhkDGGQH4ZAJhkEOGQZ4ZPAfgyALBkCBDAG88dKXQD0MgLyKlylarXqNmrds2TpGOgH0JuaWYYAWI4YAUASjFymgewZA0fKA0TUBnioDAXQHYMgNZ9AYwjAactAcNM9c3M5HRjYuLiIyP1GGwAlcgB7CDBaOjBGOnk6KBIAdzp0siyckhsAIjq7BycCm0AHckBqIkB4fQ6AMoyAIwArOgIyOgBJABEAS0oMsgIBgBsSQEdyBwBfQHMGQCCGQBEGfcSkqIAqQCuGQF6GdmZAawZAP+1ATQZAaIZAWZNAHXlTmWOkzZ/IuSAVMJgSDQWDwRDIVDoTDIf9zJVqrkwJhCrJTl0vsUyhVMtlaPVGgBueFmYBgACeJhOJxkF2udyeb0+31MNKkfzZ7IAZjNCMtllTSUkZtybCi6ABCIpQCACuw2GzTOYUBZLVZ2FF2AB0s3mZFqdhJXJpnPZUmi8St1utwqkdoYKRK5VsdlG42V+vVJC2LXagGB/2UCwDSDJ7VYsViRfIAShl4zEA/QxxwBJDIAV+Me3Q6/o2YjN5rz+fNciDy0ABgyAYwZAPoMgBYNQAQKs49eGSIAZBkAPiqABwZADEMgCsGfYlwCKDMGuw65GASNzgoACBMAyvL9wCeToBSJVuYbVkaTgEzfQD+RoB1Bi7gHkGQBmDLtAIsM8fYvEAmww3GyAAYZAMMMgHqGePRwArDLxAA0MAH47NtAP7ygAkGZ54VzDlEkSS0bQgyCoNAk1UAAZgYdBcSqfESBAZcI1WakkgoMAZgANwIYB6AGDIMmWUZlgIkgxAAcxIYAiQNRjNjRckIBIY0TnghhMAKQhSDohiiWA8weMRVCbFQPjimIH0xGAAALOZtQE+gZTkkT4R41ArAAQSo/DDTEUUbAIQyfWkgBOGw1LoRA6DqQBABkAfFdAA1tLtADEGYkHQLKQSGWA1eJsuyHOcrcnMAb2tAGSGbZnkAQH/AHx/4lRL8tL0rMORAETCGKS0AGP1nGYQBmhnYQB1hmDQBIhncDt92eSrAAAov8e20qwMO9XUVQWYzrNsuT7McpzAHAzQBKTS84lzOotFuXMg0tJNTlEgwBDsMiDAkJWmlySFE08x6upAB4NwAQfZ8nbzR6gASOomDoMQ1IyMUJJqOxWNHFCagKGUcVuuT7rSPEns2cV/toRgjROtLrse5FgZID7sXKKHajusVXqRWGmi4vyWkASE1AAdTQBi7XquRADWGQBbhgfHMm2KsrAAtFKdbi7Z5fG2QALm0AeENfOkVHUMYbUDOo+xMYLC6rpSb7SF+xHnryGHUU+8pxZISWYeeoG3toTAmjEMRwb8pg/vV+huZqOX4eQtHeollHZaaBxOciY2Qb5izBft8xHZITBnYFo03dSs7MBsupNl1gsPd5/mjNd072TkJSwAycpnToABRAAPWgKGAGYMigQW5EjkhdgbLrdnbGr9meZxAC0GftAEsGQALCMALk9AFUGbsjhj8x/ciagiOoRSNt2wP6g8JtAC83FytwELcK1D00gMSUTwKglfV50PBNiAA===)  
- dafault(T)  
    [defaultまとめ](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOB2AhgLYCmAzgA4EDGJABDCQGYECuEwgfgyAWDIIEMA3jjrC6yAMx0ywAE6tqwOgGUZc4PzB4FeANwBfISLSj0AdkHYRl4wDY6GhQDEAFPbp4AlHQC8APgbM2Dm06AHoQwG4jQCiGQBIFQCkGQEUGAysbUQAWOgBZAg0nZHQABgBtAF06AmkAczJ3c2TksMYWdmBAewZAI9NALO1op3zAOwZACIZAMQZAKwZAIIZe90BzBkBcJUB8c0A0ZUBNBkBohiS64QB9b38mjhdNd2019dc8bcbA4GDQkMB+hkAfhkAmhkBhhkAJhl7AaPVAJIZAaMjAMwYPoAK40Aa1ErY51JgQAD2BAUTCOFnWIiY5wCzWuljCgGLtQAAUYAhM0mgEJHQDyDH9RoARBnJYUAlgyAZIZhh9eoBITUADqaTUYXZpfQCBXoBjBkAPfG8ZYAfnByS2XjcdAAhJLOUEbrKoF4vIAUe0A6d6ABW1eoBxJ0Anaa9QBVDIAthkAlwyAZ4ZAJMMn1+AJBYMRSLoEqlKp2l2CYSVKsAp3KANE1ehrNS1AFiagDAlQD+8oAKV0AsHKAe+UxVYXfKFG6dInLMm0RwZZL046kQA3cp0QvbZwpw5hQCo+iyEhmRIoqHgADz2PyUAhnOXZq43JtdlsAFR8vUAtHK9aTMJwAMncLTw7AgooL62UsnkkhkqN2fbC3jzJAA7koVPInJ4nIBpy0ABAmAZXleoAWDUAECqAQAZADAqgA0GACSABkvoA+K6AAhGKzuA2whhMW0ilugO4emEHyAKMGgCMGoAMgyAIRyerDOSvREoAYC7gTcdBzPMgDaDIAWgyANYMgCrDIAxQz3IA1wyWl8gC1UbSkyAP4MyxOJ00SoYApoq8n85LLGBq51FBZTbHgx50AAIr2ABqBAQKw5AXgiTrCHkACcTgEAAdGAhyEbp+kGQAHiZ4nJGZ+zAO4hnUIcREAOQADr5K5dBOIAMwwMYA4aaAOra86ABYRgBcnpMgCyDNhuEstE8yAOoMfwRaZ6B6YZABGLn7g4KlkCQaUZQZZAHm4S45SE5UQBAdAtBF0WxZMgAr1oAwPGTIAQAxDrI9CUcMqw2XQ+gFsNaxGApu7Kap5BrLUdTiHYmh2Jp80SDAUKsJlED0BZK22RI1AABYltQe1WAtmVQlCtWZWdlgLXk+SSGdw26EAA=)  
- ref構造体  
    [ref構造体まとめ](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOB2AhgLYCmAzgA4EDGJABAE4kBmg05aAECYMrygfgyAWDIIEMAbxx1RdAPTjkAZjplgDAK7VgdAMoLlwQU2YB9DUpV70chegDcAXwnjAdmaAsBMB2DB06B7Bl2vASQyBAY0BmDIBWDIAiDCJiMnRodADCEARkZILhYHiqeBYRssjoAAxy6eIAVLoGmsaoZgyoFgXiVqGi4UwEMAD2eBAAnowsFVrd+oZaeqiCYmPjExOSgImEgPUMgJcMgJ0MgEEMgAYM/PXjTa3tXWpUeAA8ycAAfHIHFptj4cgALHQAgvFgAOZ4esAAFmBkABTFQZlRhkVAASkEVkkAF5zt9fnRoSCqps6tgxpJAMmEbmeZDeH3hZEA5gyAWjknLo/gAyMFeQAfZoBZBkA28aACQZANEMgEdlQBeaYByA0AMgyAIQZWbzpuSSM02p1+m5AISOQScRMAhwyAXoZAMMMgEmGQA+KoBnBkAX4qAdQZeYBI7UAFopEwDNDIBnhkAiwyAEoZANcMvIcrM2jR68iMqkBpWAJmE6MmtweuPxnx+/y9HpMIPQYNhdEJiOjV39EwjQ3K7pRKfGMTiZDo1AgZGTkwiOTypckni4bkAQ8qAc0dANHqvMAdv6ARQZgoBmKKJbmCqJdsl0vRU/RKkeyftLaeMpndlgx4mr7iXTgVFsACwyAawZ26zAEAMgF0GPV+QCxioAGPUAIW6b/eAMQYiYLAKP6gEDIgWswD6DBts2N9gQjidzpQf3SKZFxYVw3EADj1N0WQB+hhVWZN0ADW1AAp1QBNBj3b8jgAFVOVxliXIl91XDdN2uUsyMrEDmDoTC6AAJRYEgmDwWgLCcQAIhmvAI13XLxAE8nQBSJU3HxAGMGPwgj8QBABgpaknAbRs3FI8ilLESRIJguDEKQvUAjbQAYhkABwYAkASIZgl5KTQJrNTYPg5C0MU5SlMkXggmvblACuYzc2NEwAIFUAVZtNwCUSgj3UtpkAGAZ5BoABrABaCUukbQBYBnshyyKrCzlxYKkaREsTWXlXQ7UAH4Y7ScQA2JUAEF8X37XBs1ucoojoSdJkkOhAEMGQBghkAZIZeDChVACOGQArhkALo9AF35RKBWE19AGkGQBLBmvaavEAaMi/GdT8VPEOhABgVQANBg4gIzSKtUiWWQBlBl0tw6AAeQAaToP4YBYAhFAgVRVl4QBVeWWMbtKCMF7IoBgwAANwIYB6CyAA2dQDmOFJzjUZRaHidA/ghFKxAwuGzjoAAPRNHuYZ7XpLRzKLoE46DwAmnpe4ALHS5hsrcIl6T7daJmQAB2PHSYmNFS0DOgAFk0aanB7IFsQ0SsIA)  
    - Sapn構造体  
        [SpanTまとめ](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQOB2AhgLYCmAzgA4EDGJABAMpV4AqgfgyAWDIIEMA3jnQLrIAzHTLAATgFdqwRpJnAeIumDxy8AbgC+/QWlF9sgk0NSo6R09boB6W4C/1QDTmgELdAVgyB/eUAiDIE7TPTcE1YABtAF06AgkJOgBeOjwSAHdVdWD0AAZQnnSoOnRc1FzhXIAWXIBWXIA2XIB2XIAOXIBObU1/AMZmAB4ggD4xZliIqIA6AEEyJgI8AAoASk07W0npvEB7BkBDo0BWfUBDhkBehkBhhkBJhlnkdCqume6WAdWe29ngAAswMjoWMIB+EYkCAE9FvMOgFKDNhmC8KMGBAwLRZoU6FVFjgQTYAMIAezwZExEBIowA6hIwMASLMAEQAM0xEhAdApKOMnQENIksyCqmG6SWYDo3UGM1GABkSHgAOYvXnweDzOhYnF4gnE0nkyHBMDheAMqCMpb2GIDNn0wrFMqVGr1NHWBW4/FEklkykAHTwNJINGe9L11tMbI91GeHPU8RSgrwcttSodqtmeDo2opusWLNTBqNmIDXropqg5qglqgvpM52aC3azJZqMrnXsACUPTAAPJ4CD/NY3PocDu3YuCBsEZut9s9fp0CSQ/W2CQkKnjxvYtsfOgvN4akNqGAkAAeoT7Ag7Y8h6CWAXsM7nLBXrzI67km53e5rAWrqeWgCcgwCgDG4e31AHYMgA68oAUQx+M+NgAPrDAkySHuofRxkkKQhBkoQFNUTL7khYR0DAUGIUEwRVFkOQ5kUpQVNUbSYbBwADGQuFxDAExTMw5bWPYgAO5AAbpEABlNF9IAjuRiAxOGAAT/HHMWsCyCRWb48dE9HoMMTH3DMbH2IA0eqABXGgBrUZhA5Dm2AkiRYjFToA6WaAJt5LiaYAkJqANvGgAo9oA6d6AAragDWDIAEQxAY5gCqDIAMQyANEMHJEBQsLUKSdCYhQJB/MAtJ0BJhktm2gm/sCYHsbYX6AKaKQUGY2qUjtcgaRHRzDgeI0RxBSBAAEbUFuVLiq84oANYQEQeAUlJrGIsiU6AOGmgDq2oA6ErrClw4dmVEh9NRo5weGVXADQ7UEBAECYtQEKrdQ62bdtWEoVOUg4gQVIkIAaJqAOYMQQAFSaYABgyAIoMgCWDPlC3XAwCiyAMazgT90iyHhMG/chmR9epA0pvYgDURFiYX4tucOAFIML2vqmmFnWQF0kDwAj2L+f6ACUMuyAD8MgAdDLM1WKFceCdjwF7Lhe4G8iG+ISlK2iZW+JgPb8AK7WtG1bTtBEodkuT5KRdDFHQZR0JUSJ1I0LRUbgWW1rYTiAMr6Hm7IARwyAFcMf6AEUMxyADcMgCdDBwgDuqX5f6AFsMZuYSYUhbRK85UpBcSzB72LivMABkkR/P8wSZAmYhgAAXiQmJUsGwByvdOZyXzAicpzkrPBCccJ0nQSp0iFZu4IUb2iqToUqz/Le+B8A5y83pMpnrK0v7nvilycSs7y9d19qXeB/MzfPAP8BxGQBeJ8n8yRtidrKo65L3cn93zHyibJhnmf2MI6RH4Ap3KAFByf52W4gACRh5ZuAPUMgDdDNbgCaDEFHCABUMgCXDEFLLpg3g+zkbuPE0UAzRQAtFAK0Ws3z2HRMKOsdghDoAmNQWgZAyAADUwB4gIMAbBeAACi25aAUDwdiekgBd+UAO7RgVAD6DEbU2gBaqMAP4MgA15SAmwWh6xAAfZoAWQZAA98cwwASQyADMo5hbBADqDLQwAQAx0HLtlQAygwBXWIALE1ADyDH+QALBqAAgVQAgAyADW5P8DDAAyDIAFg9ACwJhIwAZgxeCCgojgQjABcntYrwbBn7SMxpnSuK9YwUldJCcCrc958xMgE0G9NehwVmGcEom9Wa5HHogme8c57Fzbu3f0npk6hjUMtReioq6rzjNHJMeo/4BJAWAiBUD5FILLOklkugtZNJME07QQA)  
- ポインタ操作  
    [ポインタまとめ](https://sharplab.io/#v2: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)  
- 名前空間  
    [名前空間まとめ](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUAenwAthgAHAZxEIgDMBjMsgOigFNh8LgI4BPfPQpEAhmDJcyAfSgiAtmwpkR9NsxJyANnkIBzTQHsARiM0gQgWBVAskqAvL0DJqZmbX76Zm+d2ozQNREAYQNyZJpsAB7ejn4BQaHerm6RgcFhXoCHDIDPDIALDI7pGXHMOVA6+B6AEP+yCgAESipsgJD/gBYMgPYMgIAMCdFhgEkMgHRegHoZgCoM9YDWDIBgSoCKDIBmDICF3oBgLoCaDIDRDID6DICBDIB2DID6VoDTloDmDLOAQAxFALzHJ6dn5xeXV9c3t3eXeOWKyqoVgNGRgBIM4/OAI96AT2oAbzwFRBFUIgCcgwCgDIAPs0AIgyLQAyDM1AKGKgBcFQDSDIBIhkAaJqoxqfcaw8bNVAABgqAEFmAAhADcFUAIQyUmnMAB+FUagF0GQDKDKtAHFpgBMdbaALO1AJX+gHUGRaAIIZVgiCfM9ujAFYMbmBoKe1VeFKkcgAlmxIADCMjABcJWOxJvxX1h8wAFGSpQBKPamjpvRawwAsGoAIFUaHg6gGk/QBKDPM1aDQRgKr4KUDcOH4xGACwVABitsdAPYAHcoxT07TsxVqcxfNT84Xi74S758wBfMPx+txhMVDUvNhF2Mt+OR0td7st1DJtMZwvR8tsHOlidT6v5hsDxcg/AAKjHJepUnzuKxaNWZMpUjJNKktKthKVsJX+CbS4qt6XbZqUf7d5BB+1x83tIXS97vlfN9wyHVN00zSdcxnKMy0dAsIN8GtYN/ICBzHTd52bFC71XMdfC3WCLX3ck8LPAlYUva9kKwhMcPg7VtxNVY0XPcjYVtQABHUAASNABC3R1rwfICBIHITwyEoSn1eABxAFe2k2txPkZ5n0/UkjXwMlGgtXdGMAIQZ5kFDouO4wY3nmbl6gRbZAFkGO0NIJS9nSog9vyc8lNxLU9CG1PUDQgVZAA3LQZACm5fkBVWQBZeUAO38sX9YNQ0wnt0FzKRAO7ECR3Aqd6NgtD8Lgqc8PzWip0k/NKIS6jiqjKRqzywhnNPDpAEolQAKV3FcYxxqhC8vqBZzMaazLz2fiqJE9VFM1DtN1SwdiNPKjgPJSSfwqwcktLFKFsHYcwLHbL8ugvLcKO+DStgsaEwu+MJI7PCZsXf9NtWmj1NJRpAEhNQAHU1WQBOhkAZoZAB+GQB1hgCwZOVWDxADWGQBbhkAYYZAHqGZFVkAfwZAEhzQBOpRkpb6W5blGgxgFcalDH5K2h6dtHOiTqndCcvgwq6ZKusycbFmrrEvAm0Ie4ed5vn+YFx4JvbCpAC/1QAac24pVAH95WFAE7TQDCFCYADSgCpTF1EQKAqAANelCAtQAKhkAEoZACuGQAihh+wgACIhHoMEwBAHXDgAGV1IxmDgTRNAqAAVRRgGYIRrYqeo/rSQBFhmNwBrhgWeo3Y9r3NFWfRjFMDoWe7Qgdfqdrmh18xAAdyQBgf4Tz3vaRwBAyMAAl9i8AR3J6kAcGNAG0GBUdPqG7VkACUVAFoM7Y9MAUf1K4RPSGw/FbFuZOkx/JClNE17XDinjdaUIE2Lat4pbDsVk2iSbxBkAUiULA6QA4OQ+eLJ7nhfMAqJeqWnhKD1QTAADZl9LfX8D3mJAD8GbZACA7oAU1dxSAE107iOQOiABe3QApcbQlGJfaiLZ6qknqIALmVD4LGmIARldGg3XqIAGBULCrB/p0MU7VAAsHoAKY1xSXkABEMgAxBkGAsFYDYboVDJKsaWLV7rviSpw7halBSAFUE2E2wiGrEABrRh9AAK2h0bhgB75UAL8Bmd0pgUzuGKQd9Wx0XzGCV6lJaSDDoYAKIYqTRlWBSDowDrKIJQlopeaE9EoKnkY0xD8NyWJpNYoBtiNGggcTonM18tbOIMSExeH86TGLMSyUsqxj4RPkYAYwZFj+KQfoiJCJ4aAEuGH6gADBnqIAL8VxiAHkGeYqxti5J+gkiwhTADoNoAUwYrKDFKbCdJFRAmFgiY4acsFOnaO6fPLWmBzB9K/tbcw1tthQKsAidelt+5mXqB0QAcGaAC5PPxz1FyBPieAnIX9n5vyiW4kxpDvD/2AWAiBmRoFwIQR0rpEFU4mDMCACWUtZZy2YB48Z+jAAx2oAC0VAB38oAP4yLB6EMK8qUCyfqZyuveUEDYFIKEmpSXhLjfCr3wL4AAyqSdAiZX6EEAMmEZLBhSjhOMPSCJ2ExlrNbKBgBoOWtoMdhAFawIkAISOsJKmGxyQDQYSo7GJVzACK67DqS8OAutKVqAADMHDjm6igMAaCqx9mZG0aSWkCKIxJR1uKlm7LpUtjZqNZFbDhbKRpGpQAsww/UABMMQrkmEnmDdDoD96iwgYYAVwYmHQlKYAaLlLI2SovVA14r9F4tJJgWNIAKgeEADAMHyZbyx+TSQAsAyDBaLMbYgwk060zasMUCJADUKr/QAMQyEl/rMFmhA6WZmta8KV1s/p/WtvJUOrZm0dgfgCNtHbawVFWDdRoBDAAaDDdXYlqEqSpkpG+SDbe1T3FR3FdUrayNEPqO3tSpKmRkNbWDoYotlc3wALS9V7r18yFqikWZJ133pqIBdhnDO6AA1tQAFOpqRusibY37aGwhuhYBk5yOhkj6k0Qa7TtkZIHIQbklbjGuowJgeo6J4HzHqIAACiGH9WaODG6CIxQdIyYQQAqgyVsWObQA3QxpEhqbBE4co7R0AEJmgxAC1UVWt1QaOiAHhDQAzirtRtPUQAuAYMKouGgxgHLwgejfiwlr8QCAH6GY2QNf6LD9NvCwFQADksngO9osHpuUgATBnRCY3+zdADJDNsSUs7wzsMAGAZgB4vV/b2/9x5mAwB7c+1QbhVhue2Jeeo+Tli2mVqrWFgBi7Rwx0QAJmlEkdFRdhgApBlGKa985J5pwfGv5jsKVh0Wu2ew8LWW/NKVeEV/RJLABYmsMZonde6RZCCrMAasNZa1WIAQmtHQhdhGF5Y9aDGngU5gAArOgAAnAm6dYoqNusAGtyqxZiAE8GcYqwa6rCsIMSKpSh4jys3W7ZHNcDnpvZdq713Dh3qqx2TugAqRS7oAMr1AJHPfi/EsBgoAUAMMECeEZZ6A+yxUBO2iqwJxBxwpK4OZKKpfp9imd8AB8FQADqYBdQqzduwW01sXmmGtudK1BX0WRgTvDpVSPQKOlRxjrHOOVVsHxxSYnuqUX3c7BT92VPEccOR4cNHmPsdsFxyz621J2ekznSugCPOjB8+OWounQuGei/F/j3w0vOdooALJ8ApEwXhzl6ROVh+7CoSuacjnpyLpnePrYG6N2QaX5ulUAHYKv85AnrkQKr1F5Zek2grqwYYI2RLWBNsCKiABTCCosKKiACTCDhpJ0mEHDCS9P+AQ/3bD3DeG2Io8VBj/HxPKeyTpLLiOaH2F8AVEAIYMgBghhs/UQAPiqAGcGMYixRiVsAA4MgBVhmjnagGgADhm2IG9JVJq8YSwsWGfSEg/xkh+7ZgNf0nO6YMwBftelyE7eU9172/V/r6X+zWdF2btX+v/cIAA==)  
- 例外処理  
    [例外処理まとめ](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHgegIF5SzyLKrqba76G9UBmAAg1YFEAPABzAAqACzAB7AO4BvIsBETA8PpFAyYSBlBkAxDIFO5QGiagOwZA4aaAIQMDmDLLHj5gJIZAH2aB5BhWALBkD2DEcCyDID8GQNoMgZIZAQAx5WAaxErPKARvrygIcMgM8MgAsMgD8MOoC0cgAUrMQAfKw6gC9ugDH6AJT+gaiYAGzsACysAILJ+WmZphKsUACm4qwAcqLAAJIAtrwANq39rVDArXA8AMatvMAAlqJQdQDceEUBJeUlAAysAELJogBGAFatM8CsZ+f5kluBAcFQEENDrIAQKoDK8oAo9oB070ACtrJAD8oPyOlygFiGJ7PABuAEMwKwAM5pW4XViI9H7Vjg1hNDptDrVMAAcwgYwms3mSxWySgiLGogAZicLvl8ht8Lhns9goBDc0Ab3KAkGgkCQkXaHSAC4TAGBKcOKAHY0QA6AAy43JsjSxFYB1BrAARKMFgBPY2sEBK/nqrVQHVCVgAHlYAFZ8SbUUJRGBgFabXy7fyiS12qxelAkUNFnAAPK8VpgRH0qC0hbLVbG4CiUSsIYrcnG7lPAC+gTwZcIJAYdfrDcb9aY6DYj2D2zYO1YiwmrAAGnVpAAqRbs1mIoao1r5YcEQlyYkRjNp5LGmyASIZABIMJbW1Y7/KIgGj5LQWImATQZANEMDkAMgyALo9APnagCsGQAsGl9HIBuI0AUQyAEgUb4BvuUAeEMnhYdgykqVgACVWiJABpXs4CHW1CTAc12xDflB1LA9nn3DCZlTGZnWSFcs1YGdpAIfsIkAXoZAGGGQBJhiMQB3WJcQB9BhPCxAFjFQBOhkAW4Y4ivZDniJNYggIAjgCIwB1Bg43RABKGKIeMAfoYlOSGcLEAM8iN0ALQZAGsGQBdBh0Nw3F4gTkhUujAHqGQAJhkAG4Y4h4m8rBcQBt40AeIYNyfQARBkKHCMJDIheg1EBWDAGDF2EwIZEXcMOlIhljU4k8jEAUGVAACGY0YHI7lxMkmS5PU/ILEAfFdAAQjWTAAbTNinxPPTUq8IxABX488ooC9qOs6oKQoXMwiFaRwozaMAEqgQB1hj4wBFhkAMYZAGKGIxADXlL9AC/FDxLza8Sw1aMTutCokiEs2yHKcwAoORcwBVBjUQAzBkAGnNAFNzHQTx0QAvNxKwAEuxvQAI2y3K7pLcQAxBmQvDAmBzswN2KpZgABTEGY6lYSiT0AM8VADAXIxaMYnRAFNFNQgQsQBKXXPexVC8J95IiOJAA6GZDgFQ9COtptCNueaC4IQ9Zmcrfy7VBgL8qEenOoCUSNt53DkP5kjuDmTMVhywAABnEIRxnUtUAFlWlRVFEXJVo9RNTcdweIglfGQBT5SMc6Nz0kUjE4wAPBMAPKMr1vVx1u5/kSgATjXQAFBkcQBBBl3DaRY9sXAkl0aFdN1ZWnVzXtd1/WktPFKMpLSiY4tq2beFO3Tydl37BvN2Nu9tclB0E1WEQcj461nWZx5LqCHLuOhuTUb68TpuNqCqBhtGnRACHlQBzRxvH1kWGRFTkAAwZ7H+twbEAVaVAGkGHyQ8XZuQ3DgJWV7SchiZj3nnL40VGMtxOMALATz3cwBFBj0xw6MAQH/kn3pkPnNfJg7D8sQPQOgSQ0NYbrH3PuIgTYoHQJgQwIAA=)  
- インライン化  
    JITコンパイル時で`静的かインスタンスメソッド?でIL命令32バイト以下、反復、例外を含んでいない時`インライン展開されるかも知れない  
- ポインタとref  
    [Pointer_test](https://sharplab.io/#v2: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)
- StructLayout(マーシャリングで使う?)  
    [StructLayout](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYAMBYAUAenwAthgAHAZxEIE8B7CYCAOgCMBTfAZgDkBZAKoApOAC0AknACKfAPwAbAJYVgAXgAKAGWAArAILiASlCg0uRAOwBpACI2yAazAUAoi6jjtLqZoDuAeQAJRQBhADI1AEZIgA4AVjxCEnIqQggAMwBjMjJmKHZgfBUIOBp8TIoiAEMwMnxFKGB2MDo6gFt2NrowGnkq+kZ8AGJ2AA8yJUzFYABaPoHgRPxAeoZAS4ZAToZAOwZAVYZAYoZAH4ZAa4ZASYZAcYZ9wH6GQAuEwDAlAAoidiq4ZsALBmAaMnYAAmqoOHk7EAp3KANE0AJSvJaAactAAQJgGV5HYHE7na73QC6DIAYhlBgCsGQAiDGDNoBVBNxgHsGaHwwAyDJp+gxgFYGnBmABldgARwg7Eaiiq8kA5gyAQAY1usqTTGPT/sw9Iw6ETcYADBkAigxLQAmDIA/BkAUQxCkmwuGAJIZqQtxYypcA6IBrBkA6gyAfQYdbrANQqgGSGXm4wDRDHcdXcTXQwYAgoiFYLwUCqHQoZCqmR+TOAkEywANtIA3nhvinvhhIsnU6hMGnIsxDNBgIoOsxxI1mq0WWAAG6KCMUADceEzKcIOpb3wA2lGY3HRcA7vGxQzmPgAFSAB3JAKD/Y/wLPZnKLPPHgEdyWdggC6HZUve+84gPYTDWA30UDe+8joUAA5hfz6waE1vqwGwBfQiAADlAFaugHJNQCyDLigDR6psgCtDIAJQyAF0MvKAMXagAAUYAYBlmoAsomAHb+gCqDBiLrNrgqZdj2ECxkOA5EUakrSpu27RgRJ5eoex6nuel43nez6Pj8L7vvg37/kBoGQYA0gyAJEMgAOUYAgQyAGYMqEYVhuAdoQ3ZUYR/aDv2pEuOMkzTJu3yEIAForiYA0kaAKtKdwAGKKOw8hwP46TpBQBR3PRdC2fZwBgmCvKoYAmgxuoAs4mALYM4nQIoV6ANGRVq4tigD+8rigCdpoAQQy8n+LoBjhqaEDu1HfOpEyHp25mWdZLkOZgm70WeXYFVZNl2Q5AAsm5Mbe8jnvlFnVcVA6ROgm4Pk+HEdrhhCAIxRgD3yhh4mRbigBwZoAXJ6ACFugA3DGBgBFDIAZQyABMMrywYA8QwYuJgAVDKs+yAEAMZCZHKryAFgJMlySsGwdgpvZESphojvui7cvIFFpSmGB7myEAhPIFBHo0DEXlezX3mxz5vthuFPdRL0kSO47TrOXqruuW6/Wm6DfF6wOg+VjFQyxfXsfDsl4/J+FKQsr20mpGl1lpuNDfg/05fIxMJm1hU1a5dylRupOVe1RW1QODUbk1LECx10t3N1vWwwNNO4RgXDfEmeNa+gBN67hJtZpEACcdwAESADwbgAg+1bYJNvrps6fg7Yu6b+49t8FAUN8qjfPkvgA+yPZ3E7g2u2WobsLGzLAOGDh3H7keeybtHRmmqBVP7gfB4T0rh2nrsmzHXzx1GSd3KgOcUCXpfpfd6xR17gPE77mQB0H7Ah/uxMR87jepuXcfAMwgRPGQKeZA3w9EyD3zht3BcLxQg+t2XUCx/Hk9VNP4Zzybr4difuCvkAA==)
- ポインタクラス
    [Pointer](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQB6XYAdsAFQkAEADsAE7mArhOQAoD2RwApjQDyvtfd2APiFVaOfGWp1GJABQlpAOkoBKFTgCuhAM4BDAGYdyO2poDGwZm2ICAKqIDuACy7G75EOW0BbPYT0Acw4YAG9kAGZyOwpKAG4AXxxI8jRyAGFyUJxyXO9dQ2MU5AAWcgBZOVUsnLy6vlseBs4eYVFpcgBeckIOR2t+JpsWwWIRKrjautyAfS7yeUVaFXV48nxKQEAGQFwlQHxzQDRlQHUGQDMGQCsGQBEGOUJNCAhAaQZTgH5VKfIk7ASgA=)

https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHgegIF5SzyLKrqba76G9UBmAAg1YFEAPABzAAqACzAB7AO4BvIsBETA8PpFAyYSBlBkAxDIFO5QGiagOwZA4aaAIQMDmDLLHj5gJIZAH2aB5BhWALBkD2DEcCyDID8GQNoMgZIZAQAx5WAaxErPKARvrygIcMgM8MgAsMgD8MOoC0cgAUrMQAfKw6gC9ugDH6AJT+gaiYAGzsACysAILJ+WmZphKsUACm4qwAcqLAAJIAtrwANq39rVDArXA8AMatvMAAlqJQdQDceEUBJeUlAAysAELJogBGAFatM8CsZ+f5kluBAcFQEENDrIAQKoDK8oAo9oB070ACtrJAD8oPyOlygFiGJ7PABuAEMwKwAM5pW4XViI9H7Vjg1hNDptDrVMAAcwgYwms3mSxWySgiLGogAZicLvl8ht8Lhns9goBDc0Ab3KAkGgkCQkXaHSAC4TAGBKcOKAHY0QA6AAy43JsjSxFYB1BrAARKMFgBPY2sEBK/nqrVQHVCVgAHlYAFZ8SbUUJRGBgFabXy7fyiS12qxelAkUNFnAAPK8VpgRH0qC0hbLVbG4CiUSsIYrcnG7lPAC+gTwZcIJAYdfrDcb9aY6DYj2D2zYO1YiwmrAAGnVpAAqRbs1mIoao1r5YcEQlyYkRjNp5LGmyASIZABIMJbW1Y7/KIgGj5LQWImATQZANEMDkAMgyALo9APnagCsGQAsGl9HIBuI0AUQyAEgUb4BvuUAeEMnhYdgykqVgACVWiJABpXs4CHW1CTAc12xDflB1LA9nn3DCZlTGZnWSFcs1YGdpAIfsIkAXoZAGGGQBJhiMQB3WJcQB9BhPCxAFjFQBOhkAW4Y4ivZDniJNYggIAjgCIwB1Bg43RABKGKIeMAfoYlOSGcLEAM8iN0ALQZAGsGQBdBh0Nw3F4gTkhUujAHqGQAJhkAG4Y4h4m8rBcQBt40AeIYNyfQARBkKHCMJDIheg1EBWDAGDF2EwIwxJLhuDmTMGWNTiTyMQBQZUAAIZjRgcjuXEySZLk9T8gsQB8V0ABCNZMABtM2KfE89LSrwjEAFfjzyigKOs6rqgpChczCIVpHCjNowFIlZAHWGPjAEWGQAxhkAYoYjEANeUv0AL8UPEvdq+uaVoxJ60KiSISzbIcpzACg5FzAFUGNRADMGQAac0AU3MdBPHRAC83UrAAS7G9AAjbLdruktxADEGZC8MCEHOzA3YqlmAAFMQZjqVhKJPQAzxUAMBcjFoxidEAU0U1CBCxAEpdc97FULwn3kiI4kADoZkOAVD0M6um0M255oLghD1hZyt/LtMGAoKoQGa6gJRM2vmMNZXtJyGNCSgATjXKMYzjEplTGqBQsAR0VACwEwAAKMAPKjAEuGKJlPkwADhhvQBCR0ATqUn0mwAGhkNs2LEAHEsVv+2SfF3cXQeQgWSPiukyJnQAABnEIRxnUtUAFlWlRVFEXJVo9RNTcdweIhw/GQBT5SMC6Nz0kUjE4wAPBMAPKMr1vVwNp5/l5bXQAFBkcQBBBl3TbRdrn2An99XcrDiPVlaGO44TpOU+S09UsyktKKzqBc/zwvhWL09y8r+wb2rzb66H2P48TmceW6ggd7VYbk3V4f96T7Dj4AVSgU5wsRABrRFThGNVqnj0YP/NABxCAyJpiB0SlAQAWP+x36KcZMYE1SXyTsAAA+ufUaoC0yOEAIxRgB75SutdbynhfDt0XEfEMXdWCSyZB8Zmtdnj12NCoYybhOLa3PO5QAigx6UcHRQAgP+UOlpaW+ZDywgXQOgSQMM4brH3PuIgTZ5EKMUQwIAA==
https://sharplab.io/#v2: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=  
https://sharplab.io/#v2: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=
## 構造化に関すること  
❰/:＄Type=❰＃C#の全ての型＃❱  
❰/:＄Lit=❰＃リテラル＃❱  
❰/:＄arg=＄Targ=＄vari=＄Method=＄name=＃id❰＃識別子＃❱  
❰/:＄アクセス指定子=＠❰public¦protected¦private❱  
❰/:＄修飾子=❰＠∫アクセス指定子∫ ＠❰ref❱ ＠❰readonly❱ ＠❰static❱❱
- メソッドの種類  
    - メソッド  
    //拡張メソッド  
    //コンストラクター  
    //静的コンストラクター  
    //プロパティ  
    //インデクサー  
    //イベント  
    //演算子  
    //ユーザー定義の型変換  
    //デストラクター  

//引数について  
    //オプション引数  
        //オプション引数  
        //引数指定  
    //params  
    //参照渡し(渡し方だけ)  

//オーバーロード解決  

//インライン化  

- ブロックの中でありえる事  
    - 型宣言か式か代入かステートメントかシステムコール  
        - 定義  
            - 変数  
                ```C#    
                ＄変数定義=❰∫修飾子∫⇒❰＆0＠❰ref❱ ＠❰readonly❱ ＆⟪0⟫！＠❰static❱❱ ❰∫Type∫¦var❱ ⟪❰, ❱,~⟫❰∫vari∫＆⟪0⟫＠❰ = ❰∫expr∫¦＆⟪0⟫❰ref ∫vari∫❱❱❱❱❱
                    //型推論(var)の場合、複数同時に定義できません。                     ＆⟪0⟫
                ∫メソッド呼び出し∫⇒❰∫Method∫(⟪❰, ❱,1~⟫❰out ∫Type∫ ∫vari∫❱)❱
                ```  
            - メソッド 
                ```C# 
                ❰/:＄アクセス指定子=＠❰public¦protected¦private❱  
                ❰/:＄修飾子=❰＠∫アクセス指定子∫ ＠❰ref❱ ＠❰readonly❱ ＠❰static❱❱
                ＄メソッド修飾子=❰∫修飾子∫⇒❰＠∫アクセス指定子∫ ＠❰ref❱ ＠❰readonly❱ not❰static❱❱❱

                ＄戻り値=❰∫Type∫¦void❱

                ＄通常引数=❰∫変数定義∫⇒❰∫Type∫ ∫arg∫❱❱
                ＄オプション引数=❰∫変数定義∫⇒❰∫Type∫ ∫arg∫ = ∫Lit∫❱❱
                ＄参照渡し=❰❰ref¦in¦out❱ ∫Type∫ ∫arg∫❱
                ＄可変長引数=❰params ∫Type∫[] ∫arg∫❱

                ＄引数=⟪❰, ❱,~⟫❰∫通常引数∫¦∫オプション引数∫¦参照渡し¦∫可変長引数∫❱

                ＄メソッド宣言=❰∫メソッド修飾子∫ ∫戻り値∫ ∫Method∫(∫引数∫)
                ```
        - メソッド呼び出し  
            ```C#  
            ＄メソッド呼び出し=❰∫Method∫(⟪❰, ❱,~⟫❰＠❰ref¦in¦out❱ ∫Type∫ ∫arg∫❱)❱//オプションまだ======================  
            ```
        - 代入  
            ```C#  
            ＄代入=❰∫vari∫ = ∫expr∫❱;
            ＄変数定義と初期化=❰∫変数定義∫⇒❰❰∫Type∫¦var❱ ⟪❰, ❱,~⟫❰∫代入∫❱❱❱;  
            //タプルまだ==============================
            ```  
        - 式  
            ```C#  
            ＄式=❰＃式＃❱
            ```  
        - ステートメント  
            ```C#  
            
            ```  
    - 組み合わせ  
        - 複文  
            ```C#   

            ```  

- スコープと寿命  
    - スコープ  
     
    - 寿命  

- 制御構文  
    - 分岐  
        - if  
        - ?:  
        - switch  
    - ループ  
        - for  
        ```C#  
        ＄for文=❰for(∫変数定義と初期化∫;∫式∫⇒❰＃boolを返す式＃❱;＃カウンタの更新∫式∫)❱{}
        ```
        - while  

\================  
継承関係
    object, ValueType, Enum, Delegate, Array
ジェネリックに関すること

オブジェクト指向に関すること
    クラスと構造体

    クラス
        ポリモーフィズム

    構造体

ポリモーフィズム(動的な型?)とジェネリック(静的な型?(コードの再利用性を上げる))の違い

関数型に関すること

組み込み型
    組み込み型
        リテラル
    組み込み演算子
    組み込み型の型変換

型推論
    型の決定

配列
    可変長引数

参照渡し

ポインタ、unsafe

イテレータ
    Linq

dafault(T)

匿名型

タプル

列挙型

名前空間

例外

その他

ref readonly static C<\int>
---参照
---値型、参照型
--動的な型、静的な型 as、is
--ボックス化
---識別子のスコープとオブジェクトの寿命
---デフォルト引数、引数指
---関数型
    ラムダ式
        デリゲート
---ジェネリック 型制約where 共変out、反変in
---構造体
---タプル
---列挙型
---配列
---クラス
    静的コンストラクタ
    拡張メソッド
-アクセスレベル
---ポリモーフィズム
---インターフェース
--readonly
--プロパティ
--インデクサ
-抽象定義、具体定義、自動実装
--dafault(T)
--オブジェクト初期化子
---匿名型
リスト
---イテレータ
    LINQ
Sapn構造体
null許容型
---例外処理
---名前空間
辞書、ハッシュセット

S0{S00{S000, C001}, S01}  //スタック直下(構造体は全てスタック上にある)              //メンバ{メンバのフィールド}, S:構造体 C:クラス
S0{S00{S000}, S01}  //全てStruct(unmanaged(ヒープにデータを持っていない))
C0{S00{S000{S0001}, S001}} //S00から中はunmanagedだが、C0のメンバなのでデータは全てヒープ上にある(ref structにはできない(refはスタック上にある必要がある))
Static{S0{S00{S000, C001}, S01}}
Stack {S0{S00{S000, C001}, S01}} //スタックはrefが唯一持てる場所
Heep  {S0{S00{S000, C001}, S01}}
extern{S0{S00{S000}, S01}} //外部メモリ?は構造体にキャストして使う?
C#-----------------
[Stack] -> |      |     //StackはMainから関数スタックが積まれる
[Stack] -> | Heep |     //HeepはStackとStaticから参照される。参照されなくなるとGCの対象になる
[Stack] -> |      |
[Static]-> |      |
C#外----------------
[extern]    //externはC#の管理外のメモリ領域だと思う

isは真理値を返し、asはオブジェクトを返す(asは`isinst_typeTok`をそのまま使ってる)
is:`static bool @is<T, U>(T obj)where U: class=> obj as U != null;`

```C#
public interface I0{
    public void iFunc(); //IL的には∫Abstract∫(newslot abstract virtual instance)
    public void iImpFunc(){} //IL的には∫Virtual∫(newslot virtual instance)
}
public class C3 : I0{    //アクセスが無いのはprivate固定でそもそも"I0.iFunc"と言う名前を呼べない。
    void I0.iImpFunc(){}//明示的実装はそれ自体は呼び出せなく(↑)てinterfaceにキャストしてcallvrit経由で呼ぶ必要がある
    void I0.iFunc(){}//IL的には∫Virtual∫(にfinalが付いてる)でオーバーライド可能。あと、常にinterfaceを(明示的に?)継承しないと定義できない
    public virtual void iFunc(){}//∫Vritual_method∫の実装はその通りに実装されinterfaceにキャストするとcallvrit経由で呼ばれる
    //明示的と∫Virtual_method∫を両方定義した場合、ILでは明示的が∫Virtual∫になりinterfaceにキャストすると明示的の方が呼ばれる。
    //∫Virtual_method∫の方は単なるinterfaceとは無関係な∫Virtual_method∫になる。
}
//現在の静的の型のクラスから基底へメンバを検索する。仮想メソッドを検索して見つけた場合、そこの仮想メソッド(slot)でcallvirtして動的な型に近い(インスタンスに登録してある?)仮想メソッドを呼ぶ

public new int iFunc()=>0; //∫Signatur∫が同じならばコンパイラはメソッドを絶対に区別できない?
```

'C2.iFunc()': 継承されたメンバー 'C3.iFunc()' は virtual、abstract または override に設定されていないためオーバーライドできません。 

非表示(new)と仮想メソッド(virtual)

```C#
//↓↓↓エラーCS0052：一貫性のないアクセシビリティ：フィールドタイプ「D」はフィールド「C.dc」よりもアクセスしにくい
internal class D{private static int s; public int DM(){return s;}}
public class C {
    public D dc = new D();　//インスタンス経由でclass Dの中身にアクセスできてclass Dのアクセス制限の意味が無くなる
    public void M() {       //アクセスレベル (狭)クラス < インスタンス(広)　が正しい
        int a = dc.DM();
    }
}
```

8.0前のinterface
    static メソッドを持つことが出来ない。
    実装されたインスタンスメソッドを持てない
    宣言したメソッド・プロパティはすべてpublic abstractになる。
8.0
    メソッド、プロパティ、インデクサー、イベントのアクセサーの実装を持てるようになった
    アクセシビリティを明示的に指定できるようになった
    静的メンバーを持てるようになった
`public static ref int Srefarg(ref Str str){return ref str.num;}//structのref来たものの中身をrefで返せる`  
https://ufcpp.net/study/csharp/sp_ref.html?p=2 //refを返す
```C#
public struct Str{public ref Str Ret_this(ref Str @this) => ref @this;}//@thisは通るのにthisはだめ。なぜだ       
public class C {
    public void M() {
        Str str = new Str();
        ref Str r_str = ref str.Ret_this(ref str);                  //↓の状況を許すためstruct直下のメンバまたは自身(this(値型))は参照を返せない事にした。
    }                                                                   //引数経由だと値型の参照渡しルールで安全な参照戻り値かコンパイラがチェックできる。
                                                                            //そもそも、structのthisがrefなのは this = new Str();するとコピーが起こるから
public class D{public int n;}
public class R{public ref int Ret_D(){return ref (new D()).n;}}//参照型(ヒープ)のメンバの参照を返せる(ヒープならスコープを抜けてもヒープから削除されない)

struct S{public readonly ref int Y => ref _value[0];}//(C# 8.0)readonly refはただreadonly structをメソッド単位でreadonlyにして防衛的コピーを防いでいるだけだった
struct S{∫修飾子∫⇒❰public readonly❱ ∫戻り値∫⇒❰ref int❱ Y => ref _value[0];}
```
\[属性]  
イベント  
checked❰(❰＃式＃❱)¦{❰＃文＃❱}❱  

拡張メソッドの優先度はインスタンスメソッド以下  

派生クラスのコンストラクタ(引数) : base(基底クラスに渡したい引数){//派生クラスのctorが呼び出される前に呼び出される基底のctorを指定できる  
}  
派生クラスのコンストラクタ(引数){   
}

using static System.Math; //using staticはクラスまで省略できる
        var pi = 2 * `Asin`(1);

this, base(T) アクセス  base == (Sper)this ?

short → int 精度が失われない物についてのみ、暗黙的な型変換する

{//メソッドブロックの中は型宣言か式かシステムコールの文の集まり
    int c, d;         // 宣言文: 変数の定義
    c = (a + b) / 2;  // 代入文: c に a と b の平均値を代入
    int a, b; 　　　　 // 複数宣言文: 変数の宣言と同時に初期化もできる。
    int a = 3 + 2;    // 宣言&代入文&式
    int a = 3 + func(out int n); // 式中宣言
    systemCall();     //ハードウェアへの命令を含むメソッドコール
    ;;                //;から;までに何もない場合ILでnop命令が入る
}

class Cls{}
nameof(Cls); => "Cls" //名前(識別子)を文字列リテラルとして取得できます。

`ローカル関数`は、通常のメソッドでできることであれば概ね何でもできます。例えば、以下のようなこともできます。
    再帰呼び出し
    イテレーター
    非同期メソッド
    また、メソッド内に限らず、関数メンバーならどれの中でも定義できます。
匿名関数(ラムダ式)はどこにでも(式として)書けるという利点がある一方で、以下のような制限があります。
    再帰呼び出しが素直にはできない
    イテレーターにできない
    ジェネリックにできない
    引数の規定値を持てない
staticを付けると非クロージャ(8.0)
参照なし => 静的メソッド自動生成
フィールドメンバ参照　=> インスタンスメソッド自動生成
ローカル関数直接以外「ローカル変数がフィールドに昇格(elevate)した」(ローカル変数外に格納されると何処で実行されるか分からない)

```C#
namespace System{
    delegate TResult Func<in T, out TResult>(T arg);
    delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    delegate void Action<in T>(T arg);
    delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
}
```

global::名前空間名.クラス名.メンバ名 の各階層で`名前(識別子)が衝突`してなければいい
引数を含めたメソッド内のスコープはブロック("{}")になるため、
    外側のブロックで定義された変数名は入れ子のブロックで同じ変数名を定義できない。(ローカル関数以外(シャドーイング(8.0)))
基本的にオブジェクトの`寿命`はスコープ内。スコープから実行が出るとそのスコープ内の
    値型はスタック、引数、ローカル変数から消え、参照型はポインタが消えるのでヒープにあるオブジェクトはGCの対象になるかも
`変数を使える範囲`:
    スコープ内で、かつ、変数宣言より下でだけ変数を使えます
    さらに、変数に格納した値を読み出すためには、確実に初期化してからでなければいけません
```C#
Action a = null;
for (int i = 0; i < 10; i++)
{
    var j = i;
    a += () => Console.WriteLine(j); // この j は1回1回別
} //foreahはデフォルトでこれとほぼ同じ
a();// 結果、0～9が1回ずつ表示される
```
```C#
static void Main()
{
    int c = 4;//呼び出し前にキャプチャされる変数を初期化していればいい
    // ローカル関数は宣言より前で使える     //つまり、Python式
    var y = f(2);

    int f(int x) => x * c;
}
```
＃1❰＃statement＃❱(!A(is,out)){!B0 {B0 !B1}} ❰/;＃1: if,Method以外
{if(!A(is,out)){A} A;} //scopeAは多分ifの外で宣言されているのと同じ
＃定義❰＃func＃❱(!A){A}
＃使用❰＃func＃❱(!A) A //ifと同じで外で宣言されているのと同じ
            ↑↑↑↑↑↑↑↑↑↑↑↑↑
\--------------------------------------------------------
//isとかoutとか式の中で変数を作って初期化しちゃう系でもスコープは❰while¦for¦foreach¦using¦case¦not❰if❱not❰Method❱❱(ココ){ブロック内}ともしくはメンバ内
    //しかし、if文はif文を覆うブロック内までスコープがある({if(Func(out var x)){ココでも} ココでもxが使える})
    // ~~それと、「式を囲うブロック、埋め込みステートメント、while、for、foreach、using、 case内」(for(~out var x~のxはこの"()"内のみのスコープ))~~
```C#
Func<string, int> f = s => int.TryParse(s, out var x) ? x : -1;
    //s => {int.Try..}のようにラムダ式の本体は{ブロック}で囲まれるためスコープもブロック内
f("123");
Console.WriteLine(x); // ここで x は使えない
```
----------------------------------------------------------

sharpLabはILSpyによるIL→C#で逆コンパイルしている?

https://ufcpp.net/study/csharp/start/misctyperesolution/
Haskellの型推論は関数からその関数の引数まで型を推論する(ターゲットの関数からソースの型を推論)
C#はソースの型を決めてターゲットの型を決めたりメソッドを決める
    か、ターゲットの型を見てソースの型を決める(キャストが入った様な暗黙的型変換)
        ソースと(か?)ターゲットで型が一意的に定まれば良い感じ？
            new[] { 1, 2 };, 4; //int[], int 扱い。new[]は{}に含まれてるリテラルの型を推論する?
                                //それと4はintだという。リテラルは既定の型が決まっている
            new int[4]; == new[]{0, 0, 0, 0};//同じILが生成される
            int[] a = new int[]{1, 2};
            var a = new[]{1, 2}; //ソースに型情報がある
            int[] a = {1, 2};    //ターゲットに型情報がある

            new ＠❰∫Type∫❱[＠❰~❱]＠❰{..}❱

            Array array = new[] { 1, 2, 3 };
int F(int x, int y){~}のシグネチャはF(int, int)です

```C#
class X<TItem, TList>
    where TItem : class, IEquatable<TItem>, new()
    where TList : struct, IList<TItem>{
}

イテレータ=========================
```
IEnumerable{
    IEnumerator{
        MoveNext()
        Current
        Reset()
    }
    IEnumerator GetEnumerator()
}
IEnumerator<T> GetEnumerator(){
    for(~){yield return val;}
}
❰IEnumerable<T>¦IEnumerator<T>❱ Name(~){
    yield return val;
    yield break;
}
```C#
public static IEnumerable<T> Where(this IEnumerable<T> x, Func<T, bool> f){
    foreach (T p in x)
      if (f(p))
        yield return p;
}
var list = enumer.ToList();//List<T>を返す?
```
=============================================~~
```C#
using System;
interface Inter{ //interfaceはフィールドが持てない以外はabstractと同じ?//アクセス省略はpublic
    abstract void M(); //newslot abstract virtual instance
    void M1(); //abstractがあってもなくてもIL的には↑と同じのようだ
    void M2(){} //newslot virtual instance(virtual)
    protected void M3();//M,M1と同じ(とprotected)
    private void M4(){}//instance(instance)//privateはinstanceになる
    sealed void M5(){}//instance(instance)//privateではないがオーバーライド不可
    int Num{get;set;}//自動実装ではなく抽象実装になる
}
interface Inter1 : Inter{ //↓interface内ならキャストなしで呼べる//interface内のオーバーライドは必ず明示的実装
    void Inter.M(){M();M1();M2();M3();/*M4();*/M5();} //virtual instance(override).override method(.override)
                                                //interfaceのoverride
    abstract void Inter.M2();//abstract virtual instance .override method (newslotがないabstractと.override)
                            //overrideなabstract(再抽象化)
    //void Inter.M5(){}//sealedなのでオーバーライド不可?
}
abstract class prog : Inter1{
    public void M1(){} //newslot virtual instance(virtual) //必ずpublic
                       //キャストするとcallvirtしないとそのまま
    void Inter.M2(){} //newslot virtual instance(virtual).override method(.override)
    void Inter.M3(){} //protectedなものも明示的実装ならオーバーライドできる
    public int Num{get;set;}
    int Inter.Num{get;set;}//←↑普通の実装と明示的実装できる
    public void Method(){  
        ((Inter)this).M();
        this.M1(); //普通の実装ならキャストなし可能
        ((Inter)this).M2();
        //((Inter)this).M3();//CS1540によりprotected呼び出し不可
        this.Num = 4;
        ((Inter)this).Num = 5;
    }
}
```        
```C#  
using System; 
interface Inter{ //interfaceはフィールドが持てない以外はabstractと同じ?//アクセス省略はpublic
    public abstract void M(); //newslot abstract virtual instance
    public abstract void M1();;;;void M1(); //abstractがあってもなくてもIL的には↑と同じのようだ
    public virtual void M2(){};;;;void M2(){} //newslot virtual instance(virtual)
    protected abstract void M3();;;;protected void M3();//M,M1と同じ(とprotected)
    private void M4(){};;;;private void M4(){}//instance(instance)//privateはinstanceになる
    /*protected¦*/public sealed/*はvirtualを防いでる?*/ void M5(){};;;;sealed void M5(){}//instance(instance)//privateではないがオーバーライド不可
    public abstract int Num{get;set;};;;;int Num{get;set;}//自動実装ではなく抽象実装になる
}
interface Inter1 : Inter{ //↓interface内ならキャストなしで呼べる//interface内のオーバーライドは必ず明示的実装
    ;;;;void Inter.M(){M();M1();M2();M3();/*M4();*/M5();} //virtual instance(override).override method(.override)
                                                //interfaceのoverride
    ;;;;abstract void Inter.M2();//abstract virtual instance .override method (newslotがないabstractと.override)
                            //overrideなabstract(再抽象化)
    //void Inter.M5(){}//sealedなのでオーバーライド不可?
}
abstract class prog : Inter1{
    ;;;;public void M1(){} //newslot virtual instance(virtual) //必ずpublic
                       //キャストするとcallvirtしないとそのまま
    ;;;;void Inter.M2(){} //newslot virtual instance(virtual).override method(.override)
    ;;;;void Inter.M3(){} //protectedなものも明示的実装ならオーバーライドできる
    ;;;;public int Num{get;set;}
    ;;;;int Inter.Num{get;set;}//←↑普通の実装と明示的実装できる
    public void Method(){  
        ((Inter)this).M();
        this.M1(); //普通の実装ならキャストなし可能
        ((Inter)this).M2();
        //((Inter)this).M3();//CS1540によりprotected呼び出し不可
        this.Num = 4;
        ((Inter)this).Num = 5;
    }
}
```
「C# としては区別しているように見えるけども、内部的には同じ扱いになっていて区別できないのでオーバーロードにも使えない」という型がいくつかあります。

dynamicは内部的にはobject扱い
in、ref、outは内部的には同じ扱い
```C#
unsafe
{
    int x = 1;
    void* pointer = Unsafe.AsPointer(ref x);
    *(int*)pointer = 2;

    Console.WriteLine(x); // 2 になってる

    ref int r = ref Unsafe.AsRef<int>(pointer);
    r = 3;

    Console.WriteLine(*(int*)pointer); // 3 になってる
}
```
演算子のオーバーロードはまだ https://ufcpp.net/study/csharp/oo_operator.html
https://sharplab.io/#v2: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
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugCwDcOAdgIYC2ApgM4AO1AxvQcE8AN44FBQwcgDMBepQCutAgFFeAWTABrapSgEAksDABzAPYEAvAQC06TQCV6tA5QC+A4ULETpsudQIgCUxgAWBgBOfMpqGtq6hta29k7YLkIw9BD0etTcBGCUwASKABQAlBSJSUTiOdzBAGbsnACC/GXlrugAbNm5BJQmBOTOrSIkBDVSlGzFpUOuIwBWYIyMBlLFvABu1ME9feQJM4JVBLS8evR8yADsBKJkDoznvLKmmxBS9Hf75V+Cg0SoBAAwgAeAAqAD4fAQmn9hMxgmBNllQQQ9NNyvDEZlOEdKOikgB6AluYL0GpdPLBPGw1yVbonM7AO67fEuNxHRhgAAeJkhAPg/VZwnZ3U5XIACm9GAVcUVeQROgLqS02XS8lIxhM1lcFXcaSJxMgRprJkVeL1THt9RUiB0KQRGCbtddUHqVcLDXajhAnWadVb3bSCABlYAIyh6LgBRYAbVD4cjjAAus0DqjHjrGG7Wj82agAamDiCqpC2H1KPQAO5A4ElqbW4Sk8lHYJl0xNggFCvV4u5cHFIoAOmVac220UfUd41NQvKihjSeOjHLVfyMdQSdnSQnzx6q8KU4mJSJh7YW5csngph90/PwkvpjYg41t4bQlo9cDSTkC4knFM3byDGoibm+gj0PQMboIuphyIOOj6AYd5CAARgYBgQAQKGYaYBRweE6gmLB8HRAYJRgdCBDUN4j7IYc3SVv+1GDrQdEEE+vpkByYB9E+YpsWOVF9AUXK+KgmgAJ6+CQ5Ffi4gkoX01CDlyAlbBIwmiQQlgEFJBAAJyDqgACsNRDhJanbMupgACQAES8OgqCiJo6B4CAeCDngeAONQKFsHZbE1CEMrdHMfToGQBDhcC2l4FFczwPAZrxjkkaRqYdl1IFuatFUv4eH0vDiSQDhsfli4eACAGrvleCgXJwiCThe7VgAMoswC1n2BCOZo4k3GV1q5gkDhAA===
https://sharplab.io/#v2:CYLg1APgAgTAjAWAFADsCGBbApgZwA5oDGWABFHAN6wmG1UDMZcAbCQJYoAuJGvJAvCQAsIgNwBfccmoBxADwAVAHwUpSKI2oBhEhQD0egG5sATpwCuaADYkA9oawmTbYKQDK5gEYAKDtxQAlCQgJABCaDhY3oGqoiRonjicJkTcsAA08YnJqWRCJABm3gGZ9o7OrnmFxarIJPUkBsZmllZ1DQB+WjVqDVUyBXIAqkre8iMA2gC6JADmAbVIfeSsfiQoKALCou31a3yCGztLDRrsXCQAYt625txrgQJK61tCx32uVlizaJyk5AAGK5sXwXQLvBoGT7fX6kBQAJVw5isaQArHIOCQFKU7ljEThkZxRgp4iZ5hD6lCsF8fn88UiUWR0ZiFHBMiyYDjuAiGUTvKzSbM2ViYIKYAEKY09NDaf98uQYBjNsp+YKJbspTLYVUFUqscKOcS4ILhQpRWgyeLJWcoPkALI1DV9AbRLAAdxI8j8SgmQim6pOfXqhgtc1mWxQ7s9Sr5AaDDRDJniwC2N08ACssIROAFZrM4gYAB52DNZzjTPoGQCyiYB0JUA9gyATydAFHWgBkGQBYmoARBkAQAwmLAFQBJDIAV+MAmgyAQAZAI6KgEhNUeAXCVAPjmgDRlFuAOGinQnQ3njYco16uCpxJK+n4K4mcFsKGzOfQDwYKB10veAHSP8S1wB2DIArhkAYwyAS4ZANcMgAWEYAf84APyRh6x5+nWo7gSQACCThoAAnjGoxCAEa7xlh2E4bh8YGLBx5TBe6RXuI0GwQhKQod6NSXuk14gSBmHnOWMynnAh7riYOBbus7rTCRZFcfUtiltmFa2BaJicSxBhSU4fHgYJ9EAEQJIQqnpAU1iRAeUpESQHGABYMgAwKoAGgyAEb6gDw+oAVgxvoA1gyABUMP4sWJmYSTMClJoIQnpOpniadpulYAeLEPBsWy0IQj68BgInAoUbBbBgTwkAFmkJVAzJcJkbjJBwszPAUbCioIqX8M8GWqVlcCKt68TJeVaUUGsCSvBItX1blDVoGwbBZTlnDpH4I17iQwAFAA1lYODABS4UXIWy1bPQWX5BwkYmDUiaFK8JBgCQy2FhILHZTG6SAsVqZ6AAVFAUCAA7kfiAI7kt16OsQSVSQ0QHcIASPgotj5c4KCzMUg2oYUhipl9zxvGddWFGgsMtWFgZBms7kdSxBS2EmoL3FsAJxMlcgkHAAIk+wB2CNjbBnmshaiiBR2iiEcBBAYJCAMoMb5HSQdZ89ZgChioA5gyAAYMgCKDIA/vKABIMgDRDCxFAsX0u2FowgiqYApEpvoAJAo1UWMCiHWEuAIEMUokAA8gA0l2gAvZoADaai/zxbGXW2uAJZOLG9Fhu1IVs1y3Nw6tBLdR0JbtABeWwnYt/gdaIogGDVqn9oASP+AM0MgCncoAUHKAKaKgAxDArxkoLYeAtvnSsY30KCPjIWCcAoSF4FEcZYQYnAt1gthFOCLFW+J3C2BGUbdO38aeLYtg2J4ZUkF3re994WhfXT9eN83rcQyxU8zyQniawv3fL34a/rBvTfdzvNcNHvs/5IIi890UIOFefz/L6vtVAme68N1fbeAMABymAsA/xILxLYn8iir0fKA7ACV74HwjOwM8b8wZIOnrPVEqCGasXWMbFiOgYZ03iGeLQCUZBaEfFoGeXxsw3z6GocQQA===
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQaAEAggN455l7LoBseEA9gHYDmeAzngLx6oAMf/AwfwDcAelEA6AMZTgdAE6BNdMAhboGaGQM8MgToZAEwyBLhkD1DIH6GUuTANgeAEac8AZmF5xEgGZgAphBh4ADvLAA3AENgNzwzYFtUKxMyZ3pgvGcbAAo4umCASgAWCVRhGIpbQmSMvCJHUQISwAMGQAsGXwDgt2TANE1AC4TAaQZASIYGNwB3YozAcwZAWQZAKwZAEQYMgvIw8zxAmyz87FnZoPk8KUWuXoGqjJW18nFwth2FvAAqNiPyAF8C5CLKGmQsvABlEqJRK4BJErCFSAQ4ZAL0MgGGGQCTDN1ACUMWkMWnB0MA9gyAHxVAJ4Mkyuokeq3IzwoH0BGSI30OeNmT2og3K4hUGh0BkMKMAqPqAB1MxoB1BkAgAyAIoZ9IBuhk0gBuGQCbDIBrhkAGFGAB/juTNyBs8F4uABWLJ5BVkdhcewFPF48Qg9SABYYBcK8IAkwjwyWksgUyQZWj0RlK1tBpsAuEqAfHNAGjKyVecwsDCWwjdeEAZ4qAMBdpthxFabXtAPiugAQjBPSOTyZKMl2GcOAPXSPSaff6zgBbUPh6MZIA
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQBnYATgK4DGwABAEIDeO595y6AbOWAHYW7kC85ALAG5yAehFNWHLoHMGQJrpgELdAzQyBnhkCdDIAmGQJcMgeoZA/QyAghkBWDIBEGQCwagCBUAdCTIB7AoBkGQDrygSIZAMQyAtBkCq8vsCqDG5MHQEAGOgYw8IiGMQAjAFMAM3sEsFiIGA4wYGlAXCVAfHNANGV/QGiGAH4FFQ0dXUAkhkAPs0B5BkALBjzCgOMS0PopchhhMUoACgA/AEpAVH1AB1NAIQZAKIZAewYgwFWGQGKGQB+GQGuGQEmGQDsGFurADCjAB/jATQYiwFH9QEDIhymizsYAZioB7oBDEfJqfuGRuYAHAjAADcXsBYpZAFiagDAlQBomoALhMA0gwudixADuTxG0kAsgwmO6RPH0L4jAaTQyAdQYfjAEi8iBBgAMACrvHh8ZFowYYwARcoAQX38gDMGIrbaSAf3lABIMOOweJevAEglxDBg0pesolDAAvndkI8JIx+OQAMoDEafABUAElDYIFIBDhkAvQyAYYZNojACUMal0altDrmgB8VQCeDCZjSJ1Sr8SGwmJAHrpChWG02QQ9scAfgyAfQY5oB1hkAtwyARYZAGMMy3IAHJKJZTYAIf8AkP/5wDWDHNAFUMgDWGQAdDIByhm06kAQ8qAc0cHJdpMcIhqtbrzUaDSNBEGwtr2bRg+E7kGgzhNYxUOQAMIzye6gCyhs3kUo5Gi0tZT3uY/nOEX2DEgGTCbaADctDIAi1ImgBCGWqYwCdpnNZiKBfCJQDJYyLAJWgAa2oAFOqkoAYgxFIBgBXDIsgA3DNogDtDIA5wzKI08IjDgYjIEOAAyJC2JSe74SIYSmkRAD6lAQLYJAANYzmIeJiEMNHbIoqybEMlFhEGYiqkAA===
https://sharplab.io/#v2:CYLg1APgAgTAjAWAFAHoUDoDGAXA9gJ0AiGQKIZcAjAKwFMdALBgDsBDbASwDcqACF+7QGQZAqPqAHU0BWDIDsGQOYMgcYZAVwyAmhkA/DIAmGMYCqGQGsMgDoZA5QyB6hiWB7BkCHDIF6GQMMMgSYZAHgxjAuwyAShkDPDIH6GQEkMgQGNAxgyAzBkAiDIGiGQGi5PkAwJUBpBn9kNEAhBlpAQAZAB3J6XGwEzAAbFkBHcgAy/ABXXhYAWyoszmLGJgBzKmBAawYRHwColCg4dABZfPTWTEYAZ2wAESp0qmrmKldATN9AfyNAdQY/SKQoAGZOYDGJqc4oABZOYYAKAEoAbjR1kQWDAAd8dinkZFhOAGEAb2ROX724ABse0OADNCpgAIJnT4AXx+fwOnDB9Ew0Phfz+w02nAAvEjwedOGh6FQAO7kCjcehDKqYLiI4YgEBYPD4Y4UmjYAA0nCYrA4VOwp3RGNForQTkAnQzORySjQ6fTHQCa6YAQt1MljEgBYNQAQKgZ6D10qdaIAi1KEDhc0nkylcgHxXQAIRksWkgxb9gJwwHjkZDCWhjsNTscEgASQBjQANgOVg2GI0z3rhimQeFRA6Hw1keVG06dTpwEigQKngDiEu10KNxpNsGURS6xWhAEj/gBMGMSyRRKVxxhNJ9UWCSAADlG/VHSta6Xyzsq5xw7jNuca39gGcfSh+ul0mwWPhsFSaSj6YdGSAAJL0Ni4ADWyeFzpdwHQJ7Pl6XIrhSBhQA=
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQB6Xy6AdAKIB2ArgLaBJDIJm+g/kaBADAIYXAD2g1gyBWDIMYMgMwZA0QwAKQIcMgXoZAwwyBJhkChioHMGQPYMPQCIMgOwYFgToZA/QxTA9QxdAdv6BFBjUB+AJQ58RAGZgAphBgACZAGZXAZwAOjgGMwFggyFipHVwAnYD9A4NDwyLAyYAA2ABZXADcQikcAfQKbXHsnF3cvQjTXCDBgRyiQnLzHYABPf1cSVwBlAAswKI5XAF5XFPSMkQBaNLTrbEdKKm7XEFqOMgBzAG8AYQ4ILcjxmfRUKD7B4bHXc8vXAAl6igArMAKWIJgAXxwcJ53KhXPtXLscK4oZV3FkALIiSzgyHQ1GTVyrcYZADcKNRUKijjsE1S0TAd0JxKouOw+OhPVO3SIh2OZEc2PwLgCRDAGSI3nuFyZAD9XEp9gBiZSEUgrQCADABJAAygBuGQCbDIBrhmUgA1lHhCQAOpoASBTx+PRZDuIkmlhIzKOJxpdKhPRYdzZAHduoiHdD/thfkA==
https://sharplab.io/#v2:CYLg1APgAgTAjAWAFBQAwAIpwHQGED2ANoQKYDGALgJb4B2AzgNzID0LgDuSwBl9FATgFdKgR3J0uQrwDe7PiQBmXLADZ0suaKq0K6WowC+rFmvSbtAXjMmtAZhgAydIFV5QJEMgaQZAdgyBpy0AECe8CrDIDFDIA/DIDXDICTDIDWDIAa2oAU6oDqDIBWDIAiDIaAmumAIW6AkJqADqYAFBmAGBmA++oA/ACU0fHJhryClOgAyvySpjqMqvJW2gBK8rS5ZZIsAFS0+BScAOwdcm3Cwyx6Bkjoq+hsPoDK8oD2DIGhYe6ARQxhgDcMgJ0MgEkMgCvxgNoM1SjWmDBi6JLIa5iPUAAs6ACyA1e7w+a1aAA90JZvswViDVsZZABDYB0QgATy6qlkVEhM3QYJhcPhnSRKNo6MxfGxz0sCOxjDYCJIyNRaPcgAlFI6nM7bJks8lswD+DIBjuUApoqAcwZALIMgGiGYFwvlkimtWi4hWs3pyXJUkhUMqEolsMYTbU4yzWRjCRkk5mKjGmbaAQAZTIAZBlJrNygFqowWANeVAFEMgCAdQAUrmU5SDWsA+LjbHB0GAsTqYAyjNTIZYwYAgBkARanZdyAbiNfYASBUAp3KANE13GqBe4cttADryzkAMQz3Inx014iNJtQXQCkSlzzuLAB9mksA28aACQZZbC4Vb+RTjK1ZHxW8YCVPbTNtoA/BkABgyAQkckiVQx8AJL9MF64+5TT4s/9b4jZW474La+5CHDdDfPUHtZsc/sMFcS9/2+YQQwnMMtHQAR9ThAB5AQKFyfB4MgvU2AoAALKh6FTdBCGARE+AAc2wVAv1WOCEKQ7RWjRT8wI+NgKNydgqMgrhWJokCyMxAAvXE0WgkFZwggA3VVOg1LVOh4vV1iMaScLsHjuIEXFJOMGSH3aNhhg1LT3EACIZADEGBJuOE7QAHdxNmETtJYPExIsdA7FyYYRLKXIpLkDkyl7M4yjUEAIGRCMIAAB3wUwSD4CB0AAWgAPi6IKQpi/CKERCBQ2WD4oEecz0HUzpwTKSEkqXOyCr02lOghJzTDfarnLBQyTO45sOs6jq2HzAty3kbZAFR9HM1A5PyLjFOJADMGW5pVyQAh5UAc0cLm9QBjBimpbrkATQZQK6vb9ubNhYH8YJwkAQ4ZAGeGQAFhj87ZZtyWohAobZN0AQIZdoOz6DrYQBdBnrUtbsAfQZADcGQBkhluZJckAW4YgkAaoYQj8wALBlcD7VjyvE3QFSkbXVeQLxVEqyrxAlkFDdGfnQc9L2VQY2E0XlrWnNljISQAghncbjDXGdgVShC1OZYCnxEBMwkqNHnhDAMA7KWwBCOzibZAHqGGHAA6GQBjhgiQB/eRHJsiXDLSBawGAAB5TCSgBBMhVSJ8XaEl6XuNaABbA36LWAq+BdmrZidwSGMF35xBgEWxe5z37bs5WgnVwArwM2V15AubXddWHK1nJ34mI4iDaEGbjeffOzAFH9QAtBjFLbxw+NO0bgR5DwoaLchGcW1AA2h2Pg+YWBpt43dWZ3XcOlgW+KiDPesuY2EAEoZADGGE4gkAfoYGbkJOdZSPvMDgGMACMkQAa0E5Y9CAA===
[ジェネ](https://sharplab.io/#v2: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)
https://sharplab.io/#v2:CYLg1APgAgTAjAWAFBQAwAIpwCwG5lQDMmM6AwugN7Lq3oD0AVDXbOgJIAuAptwE7oQmOIQA8AIwCePAHwAaYYQXsAogDsArgFt+AQ04B7PhOnd5HACIBLAM4AHAzd3iANt2Xrtew32pI6ARyeOnz6RiayihFmAHQA4tycwd5GABQAlPT0gMoMgHYMgPUMgJcMgJ0MuYAlDIDPDMWA/QxVgEkMgCvxgJoMASy0AL5t6Iz0XURRUpFcvHypVmqc6Jpa6X6BrHBw6OJ83LoA1vj+88JLUADs6HhdrdsdfcRQ2OgAshlUJ3RZWABs6OOTNlboALxHWzt3hMAPzoXS/dBjEHpNTcADu6AA8uIAFbcADGnAyAJ2HwA2gBdMF8AR/WEI/HYAk4+YAMyMa3RAAsoZM1EDiXxZp0zoFHrQADK2TiiMgydAuYUUMnw9BCmwisXY/noemjD7vCGoXCa0ToODa95gMCzFUBSUKsgxACCwGAqXJ5Ay6RpgR5gLVjJZFAxHItnDIpt5gJV7roPJ5WTKgDGGQA3DIAfhmq8fQgDWGQC3DIBhhnygHsGQCADIBuI0AUQyAEgVGoBtBkA0QyACIZAGIMgCsGQBFqYAHU0AhwwVQALDPHUi3AL0MGcAkwy5wAo9oB070ACtq5wDrDGnAIsM0cAxQy5ADmiVzlUA4wz5QCtDPHl4l0nXAJEMgC/1QA05oAQt0A1gzZwCq8pfAH/agHUGQBmDIBzBkAQQyAf3lABIMgD8GJqAEAMyBMMgyA9EAA
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyDMABAM7ABOArgMbAEDKZ6A3gL4D0rg05aAECYMryg9gxEApgEMIgmICAGQJm+gfyNAsHKB75UCRDIHUGQCIMOfATAA7YINIAzYZUEEAksEONkAFgvIArAAoAlC2Y4C3gu0AlDIDPDID9DIA/DICrDIDlDCGAnQyAdgyA4wxhgJUMgNcMgBMMgPnagKMRfLIqgPoMgaGRMYDWDIC1DIDHDNFpKoBWDGqAFgyA/vKAEgwNmoQkFNR0ZAQgFlY2hHZ0Tm6MrABUzs6Whq7AABZgRK4AdNPs0+YTrgDc28y+01pjey7uzNtePvcPjw/s5/aXk7o0OgQAvAS2B1o+wOnmwT3B4NB51QBAAwgRGHdvK8CABZNwIpEPZw6QQAdz6pDcmyBVwOWPu7Bx+MJxI270OFJ8cwWrmpBPoRNc3PpwKxoOYQA=
[new](https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyDMABGgQEIDe+BAbmAE7ACuAhhEQCwED2JAYgBQBKMgF8ihZB16CRYougBs7AgGcpQ4cJyViAEQIhSFQpyoBTWrTAxTS7v3WyAdqYDuStTMrO3yBUtX2Ipq44qgEOnoGOkZcZhZWNhJcHqJeru6BqYTecopJAdIaWqEEAMIEZARVBAD0NYCrSoAhboBWDIDR6gYm5pbWBIBJhARMAEbKwLRMAMbAUDT0zKyAmuktrVCLgEWpgA6mBjn9zYDqDDjVskkAsoIVB4fVdYAlDIDPDICdDID9DA+AdgyLbYDWDIBf6oCmDIDyDOMWKwwI4RkxHONTBdLgQ+DkdIIBAA6NQAbmhlz4fBIAjh6QRAkJKPs6OwMKqWJxeLcEURyLRGMOjKuNTuT1eTU+vwBQJmwAIILBEKhZPJ1PCiKRdkEpPJsOxuPhdKlDNFMMpivxBKJ0oEsvJzKqNwez3ugFg5QCBkYAgBjW6wIgIgEENtRqAA9YUqiQU9c6dEjvfqYRF/aq5SQQyTnc6qExaAMCABeAg5EHAADaAFYALqBy4xuODRPJ1xprNkAAMUAIlerVZr5eEucOAEELEwAJ4DNudpM5CCcRwAc1LFVrY/LBEbzrqrbGndjcd76VnHZldSGIzGkyIqCjasnOGEQA=)
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyDMABMugGxGoEDKwA3jgQ0YcWWAHbAEDOnAvAQBYA3DgC+OfOQIBhdDXG5CPAE4BXAMadKchQHpdgPYZAswyAfhkAdDIGj1EAQDyFwIAMx84F2GQCUMgZ4ZA/QwWCACmkAkgAyvgB0bACmwACUgHYMgIXegGAugNYMgBEMgMYMgGYMgEPKgOaOgBKKgDIMgLtKgNJGyYCQmoAvZoCqDIAxDIBiDIDRDNE4+mGRwICXDF6ugAcMCYmA0HK+efmxgOMMJp6F4VFpWYDfnoCrSr6AzQzugIsMgPUMgO0MgOcM7tH19mOFFYDxDIBRDM2AQAz0jI9Pzy/P+gDi0vaARQyAkwyANwyAToZAOYMsW+20A3QyAwCBnoAja2i0QYbV033cTks1nRFmBgCsGQAiDI5TJZAEkMgGoVQDJDMC8fiLLdfIDPIBhhm2gFDFYGAGO1AAq+gHUGTJ4pqAaQZ7HZfIAlhhMgHWGOKAQ4Z3IAFhjZXN5/IsiIID1eb10gBMGQB+DFd7IASBQcgFjFQAMeoAQt2SqKx3IFFKp+MA05aAAgTAMryPzRRLcXgsGs1j30Nu9Hk8wPRLLFkriwfMFmJeUAmgxNQAsGoAIFQAZmAAB4RGC+aKFQCj+oBAyIkhFUbC4AEMMxEpNI6NhHpJZAR1AB9dToISvfR2f2SbTtjtcHt93Sut2DwjsTidsC9l76Q3+/SSZQRDMEOcjsDKXv6dE+zz+mc7jgAKj3AAcl4GUV6XKHAMXagAAowDTmmvdK30NfO92d7fpIvibtuc7RP+Hb7kBzaMJIc6XlBYA3rBLaENoUFjneWqxmYJ7voAQr7ntIBZNq8u5gAQ/DCAuN7UQQABki4EEGT74aGX5wVq/oPrxjCITu9H8MxQh0cJTEoSI3FPMOY4MZEADuVDoAWYmjugEmMWOh6PseobETJCgKJIlY1nW3DAGomhUOR8GEG2XBduOy66AOMlDug3AafeTz6FO567k5LGuauMnroQYEXpwwUHqxhghl4Z4ebOV7eVJuHsQRn7AQ5f7eYB0kPiBUUQdesVofZ0VIelqFFVVmHedhvkMGx+leER57IAIBAALJkfxDCUQxwjBVpLFtYlnhcROg3xXNgkoQxoljeNlXPHJXn8EpKlqU52HLTp8V4SehmPMZ5YEGZtb1mgBAACJ2QwkjdX1A0yY8bYALbOS1Abxe5rzDj9R3/Q+AUfQJHAED9VE0X9Wpha8C3Q7D9bw2DD7tdNvjvoAG3KALIMrSQ88pWo9BygMVFaO9nN33OejBA7bIalzY8UX092lP8NTDP1aTW7RTDFOM7zYARPzmr6FFZOcLDyjcwQYubpLjyIboSGa7owsoYzbCqBAECqy8suXhrmtmzrN6K2LN4S3NwMafbJNPFFjtjjbgsg+gEtHlNhG42+hPE8jLuCbDOGY5Nz4dTlLv6LI14/YVc3tLLkE68o61PIhSfQXVDv5d7kdg9HHGx2dLwo5wcOCEIEfLSFrV6VNM2uanujV9eS0iYuEdrcbDCbQpETKdoanF4d45l6d/rGUAA
https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQB6XyAzAARrEDCAzgA4DeRxAytQDxgB2wAfMTQNwBfHAwBOAUwBmvYCICuAY2BNWAFS61i+cRMDTloAIEwMrygewZAHHqBrBkCdDIH6GQMMMgeoYzgO39A0gyAYhkDRDAApKwAIbyAawBaAHt2CABPAEocYjjSEnJieT54tPitSX1jQCHlQHNHQGj1QCSGQGoVQGSGQHMGQBEGd2dAIIZAU/dXQCsGKsB9BkBAhnyCwAsGQBgVQA0GQDYlQBBfIrrAEwZY+IZmX3YWNWIAfRp51Px8osB87UBstLNAelVAADly6vdsuYW1UsAAhk6zQCEGLzX2Xl8JMWAI4jkIMRjsGlZtRPJdFjwXlFaKtqPNiABeXiw9h8eQI4jsMQAdwoniighwQmwDDISVo0ziJIALMQALJ44jkwHpOIAN18IiR6MxOOYbE4XG8fkCvggEBCaI4wAA2lSALr4inpMFSnivRE0AB0MPmmsYEDA8jEnnQUFQiuZcSJAiAA==







