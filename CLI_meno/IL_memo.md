# ILのまとめ  

## リンク  

[IL入門_KEN's](http://www5b.biglobe.ne.jp/~yone-ken/VBNET/index.html)  
[ECMA-335_CIL命令セット](http://www.atelier-blue.com/program/il/ecma-335/p-3/index.htm#pagetop)  
[フレームワーク / 実行環境: 実行基盤](https://ufcpp.net/study/csharp/FwInfrastructure.html)  
[Unity開発するにあたって知っておきたいコンパイラのすゝめ](https://qiita.com/4_mio_11/items/89fd91ef8ede02bfdd46)  
[.NETの動作原理を基礎から理解する！](https://www.atmarkit.co.jp/fdotnet/dotnetwork/index/index.html)  
[.Netとは ~ ファイル作成 まで](http://home.a00.itscom.net/hatada/dotnet/dotnet01.html)  
[OpCodes](https://docs.microsoft.com/ja-jp/dotnet/api/system.reflection.emit.opcodes.add?view=netframework-4.8)  
[sharplab](https://sharplab.io/)  
[C#とILとネイティブと](https://www.slideshare.net/ufcpp/compilation-29412750)  
[ECMA-335](https://www.ecma-international.org/publications-and-standards/standards/ecma-335/)

## C#とIL  

---

## CLIの用語  

- CLI(Common Language Infrastructure): .netを中心に**多言語を多プラットフォーム**で動くようにした**仕様**。ECMA-335で標準化されている
  - CLR(Common Language Runtime): CLRはCLIをWindows上で動くように**マイクロソフトが実装**したもの
  - CTS(Common Type System): プログラミング言語間で共通して用いられる**型の集合**  
  - CIL(Common Intermediate Language): IL, MSIL とも呼ばれ、多言語をこのILにコンパイルして**多言語間で共通のライブラリ**として使える(その他、**中間言語**として色々なメリットがる)
  - 以下はCLIの実装部分、各プラットフォームにインストールされるもの  
    - CBL(Base Class Library): 開発者が書いたコードを各プラットフォームで動くようにした**ライブラリ**(ILとネイティブ？)
    - VES(Virtual Execution System): 開発者のILとCBLを使って実際のプラットフォーム用の**ネイティブコードを生成**する
      - これを実行時、メソッドごとにやるのが**JITコンパイラ**、プログラム起動前にやるのが**AOTコンパイラ**

## [ILコードの例](http://www5b.biglobe.ne.jp/~yone-ken/VBNET/IL/il08_NewObj.html)  

```plaintext
.assembly NewObjAndCallInstanceMethod {}
.method public static void main()
{
    .entrypoint //このプログラムのエントリーポイント。"main"という名前はエントリーポイントにならない。
    .maxstack 3 //このメソッドで使うILスタックの大きさ。無駄に大きいとメモリを無駄に食う。足りないとエラー

    ldstr "16進数へ：" //文字列をILスタックにプッシュ

    //ILスタックにプッシュされたstring("16進数へ:")を引数にとり、参照型のオブジェクトを生成(newobj)し、
      //そのインスタンス(instance)のコンストラクタ(.ctor)を呼び出し初期化してStringBuilderのインスタンスをプッシュする。
    newobj instance void [mscorlib]System.Text.StringBuilder::.ctor(string)
    
    ldstr "255={0:X}" //文字列をILスタックにプッシュ
    ldc.i4 255        //int32型をILスタックにプッシュ
    //ILスタックにプッシュされたint32(255)を引数にとり、ボックス化(box)された参照型を返す。
    box valuetype [mscorlib]System.Int32  //値型(struct)はvaluetype //参照型(O型,class)はclass

    //instanceは、インスタンスメソッドであり、第零引数にそのインスタンス(StringBuilder)、第一引数にstring("255={0:X}")、第ニ引数にobject(.i4 255のボックス化)をとり、
      //新しいオブジェクト(StringBuilder)を生成して返す。  
    call instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::AppendFormat(string, object)
    //命令:❰call❱ メソッド:❰｡｡instance 戻り値:❰class [mscorlib]System.Text.StringBuilder❱ メソッド名:❰[mscorlib]System.Text.StringBuilder::AppendFormat❱引数:❰(string, object)❱｡｡❱

    //第零引数にそのインスタンス(StringBuilder)をとり、stringを返す。  
    call instance string [mscorlib]System.Text.StringBuilder::ToString()

    //callのみは静的メソッド。stringを引数にとり、Consoleに出力する。戻り値は無し。
    call void [mscorlib]System.Console::WriteLine(string) //=>16進数へ：255=FF

    // 戻り値がvoidなため、ILスタックが空でなければいけない。戻り値がある場合はその型がちょうど一つある事。
    ret
}
```

|                      |                          |                      |                         |                         |                         |                             |                         |                    |
| :------------------- | :----------------------- | :------------------- | :---------------------- | :---------------------- | :---------------------- | :-------------------------- | :---------------------- | :----------------- |
|                      |                          |                      |                         | int32<br>(255)          | box_int32<br>(255)      |                             |                         |                    |
| ILスタック状態の関係 |                          |                      | string<br>("255={0:X}") | string<br>("255={0:X}") | string<br>("255={0:X}") |                             |                         |                    |
|                      | string<br>("16進数へ：") | StringBuilder        | StringBuilder           | StringBuilder           | StringBuilder           | StringBuilder               | string                  |                    |
| ============         | ==========               | ============         | =========               | =========               | =========               | ===================         | ==============          | ============       |
| 実行命令             | ldstr                    | newobj               | ldstr                   | ldc.i4                  | box                     | call                        | call                    | call               |
| 実行メソッド         |                          | StringBuilder::.ctor |                         |                         |                         | StringBuilder::AppendFormat | StringBuilder::ToString | Console::WriteLine |

- ILコードの
  - ILはスタックマシン、逆ポーランド記法 {3 4 + 1 2 - \*} <=> {(3 + 4) * (1 - 2)}(数字(データ)をプッシュして演算子(メソッド)でポップして処理して結果をプッシュする)  
  - 1行の形式は、**ラベル: 命令 オペランド**  
  - 全ての命令は**ILスタック、定数、引数、ローカル変数、ヒープ、外部メモリ、演算器、プログラムカウンタ**への命令(命令は殆ど1バイト)
    - メソッドは、IL命令が32バイト以下かつ反復と例外を含んでいないそれと、仮想呼び出し、デリゲートでない時、JITコンパイラはそのメソッドは**インライン化**する(メソッドを展開して埋め込まれる)
  - "."から始まる文字列は**ディレクティブ**と言い、アセンブラが**プログラムの構造(式木?)を認識**するために用いられるもの  

## アセンブリの構造  

.netは**アセンブリと言う単位**でプログラムが読まれる  
アセンブリは複数ファイルにまたがる事がある。  

CLI(BCLとVES)は実行環境によって色々ある

- CLRヘッダ
  - マニフェスト: このアセンブリの基本情報  
- CLRデータ  
  - 型メタデータ: 型情報  
  - ILコード: 複数のメソッドが定義されてる  
  - マネージ・リソース: 文字列や画像などのデータ  

## C#のコンパイルから.netの実行まで  

```plaintext
  |IL| ---ilasm.exe--> |   .exe    |  
  |__| <--ildasm.exe-- |   .dll    | ---CLR(AOT or JIT)--> ネイティブ  
  |C#| --csc.exe---->  |(ILバイナリ)|  
```

- C#コンパイラ  
  - cmd.exeのコンパイルでは、csc.exe source.cs System_0.dll System_1.dll.. (**❰C#コンパイラ❱ ❰ソースコード❱ ❰参照するアセンブリ(dll)..❱**)とすると、  
    **dllかexeファイルが生成**される(C#では`Main関数`、ILでは`.entrypoint`があれば**exeが生成される?**)。のでdllの循環参照は起こらない?[(C#コードのコンパイル)](https://qiita.com/asterisk9101/items/4f0e8da6d1e2e9df8e14)  
  - コンパイラの場所: C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe (そこのdll群はランタイム用?)[(コンパイラのオプション)](https://qiita.com/toshirot/items/dcf7809007730d835cfc)
- .NETライブラリ  
  - **mscorlib.dll**(System.Objectとかの基本型)は指定しなくても参照される[(mscorlib.dllをインポートさせない)](https://qiita.com/gdrom1gb/items/69ed26a72c6c2b9445e3)  
  - 参照するアセンブリ(dll): C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1 (参照アセンブリは定義のみで中身空っぽ?)  
  - GAC(グローバル・アセンブリ・キャッシュ): C:\Windows\assembly (↑↑をキャッシュしたもの??Windows自身がOSで使っている?)  
- .netの実行: {windowsローダー} => {win32(PEヘッダ)} => {.net(IL(バイナリ))(CLR)}  

### JITコンパイラ

あるメソッドを初めて実行する時、そのメソッドを実行するための型を**型メタデータ**から読まれて検証され  
JITコンパイラによってそのメソッドがネイティブコードにコンパイルされる  
その後、再びそのメソッドを呼ぶ時は**既にコンパイルされたネイティブコードを実行**する。
そのため、起動時は重い  
ネイティブ実行中は、**GC, 例外, 実行権限**の恩恵を受ける。  

## ILの型  

- ILスタックには**int32, int64, native int, F型, O型, ポインタ型(native unsigned int(*), &)**しか区別できない(ILスタックは.maxstack nの事)  
  - ILスタックとILスタック以外に入出力する場合は暗黙的な型変換が入る(0拡張、符号拡張、切り捨て、0方向への切り捨て)  
- マネージドポインタ(IL:&)
  - **C#のref**である
  - **メソッドスタックのみ存在**できます (メソッドスタックは関数スタックです)
  - **マネージドポインタを指せない**それ以外は全て指せる
  - **nullにならない**
  - アンマネージドポインタを制約した**安全なポインタ**とも言える
- アンマネージドポインタ(IL:*)
  - **C#のポインタ(*)**である
  - **全ての領域に存在**できる
  - **アンマネージ型**のみ指せます
  - O型の参照先(ヒープ)のフィールドか静的領域(staticフィールド)を参照する場合は**fixedステートメント**が必要
  - **nullになります**
  - **unsafeキーワード**をブロックに付けなければ使えません
  - C++のような**ポインタ操作**ができます(安全ではない)
- 値型(IL:valuetype, int32, bool,..)
  - **C#の値型**である(C#の構造体(struct))
  - **全ての領域に存在**できます
  - C#のキーワード`int`も定義は`System.Int32`という**構造体(struct)で定義**されている。
  - 構造体の中でも、構造体のフィールドが**再帰的に値型**の場合、**アンマネージ構造体**と呼ぶことにする
  - **ユーザー定義構造体**では、**初期化時**にILでは、`intiobj ｢定義型名｣` または `call instance void ｢定義型名｣::.ctor(｢引数の型｣)` が呼ばれます
- O型(IL:class, string, int32[],..)
  - **C#の参照型**である。(classまたはvalueType(値型)の**ボックス化表現**(つまりclass))
  - **全ての領域に存在**できます
  - **O型は、ヒープへの参照**しか持ちません
  - マネージドポインタと同じく参照として**安全ではない事はできません**
  - **nullになります**
  - どこからも参照されなくなると**GCの対象**になります
  - ILの `isinst`命令で動的な型を調べれるので**動的な型の情報を持っています**  
  - ILの `newobj`と`newarr` 命令によって**生成**されます
- マネージ型
  - マネージ型は、**O型**と**マネージドポインタ**の事です
  - **CLRのGC**によって**管理(マネージ)**されます
- アンマネージ型
  - アンマネージ型は、**アンマネージドポインタ**と**アンマネージ構造体**の事です
- 説明した全ての型で**生成される領域**は、メソッドの中で定義すると**メソッドスタックに生成**し、参照型(class)のフィールドに定義すると**ヒープに生成**される

## ILのメモリマップ

```plaintext

　~底位アドレス~    (この図は感で書いている)
 | プログラム  |                                     ＄Stack＝❰メソッドスタック❱ |`````````|        //refとO型の参照の使われ方を表した図 (refとO型はマネージド(安全))
 |    定数    |                                        ref┌--[↑∫Stack∫]--ref--|->{field}|        //∫Stack∫はMainからメソッドスタックが積まれる
 |  静的変数  |                                           └->[↑∫Stack∫]--O型->|  Heep   |--┐O型  //Heepは[∫Stack∫]と[Static]から大体、O型によって参照される。
↓|   ヒープ   |                                        ref┌--[↑∫Stack∫(Main)] |         |<-┘        //参照されなくなるとGCの対象になる
↓|   ヒープ   | {マネージドヒープ(GCの管理下にある)}         └->[Static]----O型->|_________|        //refは自分より上のプッシュされた[∫Stack∫]を参照できない
↓|   ヒープ   |                                                                                  //refは何らかのfieldも参照する事ができる
 |    ...     |                                     
 |    ...     |メ {"--繰り返し--"で繰り返されるスタックをメソッドスタックと呼ぶことにする。ILコードの .maxstack n が使うスタックをILスタックと呼ぶことにする}
↑|--繰り返し---|ソ {stackallocは明示的開放不可。メソッドから戻るときに自動的に破棄。}{stackallocを使うとILでlocallocが呼ばれるのでlocallocとstackallocは同じ}
↑|⟦～⟧localloc?|ッ{(stackalloc int[DateTime.Now.Second])[30]ができてメソッド実行中動的生成(確保)できているので可変量だと思う}
↑| ILスタック  |ド {ILスタックは.maxstack n で容量が決められているので固定量}
↑|ローカル変数 |ス {このメソッドスタックはスレッド毎にあると思う}
↑|    引数    |タ      |         |
↑|--繰り返し--|ッ      |外部メモリ|
 ~高位アドレス~ク      |         |

```

## ILの命令

```plaintext
|      | <----(ldはプッシュ) (stはポップ)---> |静的変数,ヒープ,localloc,ローカル変数,引数,外部メモリ |
|      | <----ld⟪c¦str¦null⟫--------------- |定数                                               |
|      | <----ldloc＠❰a❱, stloc------------> |ローカル変数                                        |
|      | <----ldarg＠❰a❱, starg------------> |引数                                               |
|      | <----ld⟪obj¦ind⟫, st⟪obj¦ind⟫----> |ポインタ(スタックか外部メモリ?)                      |
|      | <----ld⟪len¦elem＠❰a❱⟫, stelem----> |配列                                               |
|  I   | -----newobj, newarr--------------- |O型(ヒープ)                                         |
|  L   | <----ldfld＠❰a❱, stfld------------> |フィールド(O型(ヒープ))                              |
|  ス  | <----ldsfld＠❰a❱, stsfld----------> |フィールド(静的変数)                                 |
|  タ  | =====castclass, isinst============ |キャスト(処理)                                       |
|  ッ  | <----ld＠❰virt❱ftn----------------- |メソッド(アドレス)                                   |
|  ク  | -----b⟪r＠❰true❱¦eq⟫, switch------> |分岐(｢プログラムカウンタ)                             |
|      | -----call＠⟪i¦virt⟫, jmp---------> |コール(｢プログラムカウンタ)                           |
|      | =====add,sub,mul,div,rem,neg====== |演算(処理(演算器))                                   |
|      | =====and,or,xor,not=============== |論理演算(処理(演算器))                               |
|      | =====shl,shr,ceq,conv============= |ビットシフト、比較、型変換(処理(演算器))               |
|      | -----box,unbox＠❰.any❱------------- |ボックス化{値型(メソッドスタック)}<->{O型(ヒープ)}     |
|      | -----cp⟪blk¦obj⟫------------------ |コピー(スタックか外部メモリ?)                         |
|      | -----init⟪obj¦blk⟫---------------- |領域初期化                                           |
|      | =====dup, pop===================== |ILスタック操作                                       |
"==..="は、ILスタックからILスタック。 "--..-"は、領域の初期化やコピーとポックス化
ILスタックは何かを計算したりコピーしたり制御信号を送信したりするための一時的なデータの保管場所(アキュームレータの様な)
    ⟦=>┃～⟧❰｡⟦=>┃1～⟧❰プッシュ❱ => 処理(ポップ、計算、コピー、制御信号)｡❱ 

```

## **C#の型**と**ILが認識する型**の対応表  

  | C#キーワード           | C#型定義                 | ILの型 (ILの命令)        | ILの型の種類                  | 詳細                                                                               |
  | :--------------------- | :----------------------- | :----------------------- | :---------------------------- | :--------------------------------------------------------------------------------- |
  | object                 | System.Object (class)    | object                   | O型 (class)                   | 値型をこれ(`object`)にキャスト変換すると**ボクシングが発生**する                   |
  | bool                   | System.Boolean (struct)  | bool                     | 値型 (valuetype)              | `bool型`は1バイト                                                                  |
  | byte                   | System.Byte (struct)     | uint8 (.i1)              | 値型 (valuetype)              |                                                                                    |
  | char                   | System.Char (struct)     | char                     | 値型 (valuetype)              | `char型`は**2バイト**でUnicodeの`U+0x0~U+0xFFFF`を表現する。あと、無理やり、       |
  | short                  | System.Int16 (struct)    | int16 (.i2)              | 値型 (valuetype)              | `ac[0]=(char)0xD83D; ac[1]=(char)0xDE03;`と入れると**4バイトも表現**できなくもない |
  | int                    | System.Int32 (struct)    | int32 (.i4)              | 値型 (valuetype)              |                                                                                    |
  | long                   | System.Int64 (struct)    | int64 (.i8)              | 値型 (valuetype)              | 全ての値型(struct)は、`System.ValueType`を継承し、                                 |
  | ushort                 | System.UInt16 (struct)   | uint16 (.u2)             | 値型 (valuetype)              | `System.ValueType`は`System.Object`を継承                                          |
  | uint                   | System.UInt32 (struct)   | uint32 (.u4)             | 値型 (valuetype)              |                                                                                    |
  | ulong                  | System.UInt64 (struct)   | uint64 (.u8)             | 値型 (valuetype)              |                                                                                    |
  | float                  | System.Single (struct)   | float32 (.r4)            | 値型 (valuetype)              |                                                                                    |
  | double                 | System.Double (struct)   | float64 (.r8)            | 値型 (valuetype)              |                                                                                    |
  | string                 | System.String (class)    | string                   | O型 (class)                   | C#の文字コードは**UTF-16**でchar型で表現できない文字はstringで表現する             |
  | int[]                  | System.Int32[] (class?)  | int32[]                  | O型 (class)                   | 全ての配列は`System.Array`を継承。`ldelem`での配列命令の為に配列長と要素型がある?  |
  | int*                   | System.Int32* (struct*?) | int32*                   | アンマネージドポインタ型 (*)  | `typeof`で調べたら`System.Int32*`と出た                                            |
  | ref int                | 調べられない             | int32&                   | マネージドポインタ型 (&)      |                                                                                    |
  | ↓ユーザー定義型の定義↓ |                          |                          |                               |                                                                                    |
  | struct S{}             | S (struct)               | valuetype S              | 値型 (valuetype)              |                                                                                    |
  | class C{}              | C (class)                | class C                  | O型 (class)                   |                                                                                    |
  | class C<T,U>{}         | C<int,long> (class)      | class S\`2<int32, int64> | O型 (class)                   | 変数を、`C<int,long> c;`と定義                                                     |
  | fixed(int* p = arr){}  | System.Int32\*とInt32[]  | int32* とint32[] pinned  | アンマネージドポインタ型とO型 | `pinned`にアドレスが入ると**GCはそのデータを移動**しない、nullを入れると解除       |
  | delegate void F()      | F (class)                | class F                  | O型 (class)                   | `[System.Runtime]System.MulticastDelegate`を継承                                   |
  | enum E{}               | E (struct)               | valuetype E              | 値型 (valuetype)              | `[System.Runtime]System.Enum`を継承 ([..]はアセンブリ名(`System.Runtime.dll`))     |
  |                        |                          |                          |                               |                                                                                    |

## スタック遷移図

命令:add, スタック遷移図:…, value1, value2 => …, result  (ILの一つの命令でプッシュされるのは0個または1個)
スタック遷移図は、IL命令を実行する**前後のILスタックの状態**を表し"=>"の左項が命令実行**前**の状態で、右項が命令実行**後**状態である。
そして、"=>"の両項のILスタックの状態は左に行くほどスタックの底を表しで右に行くほどスタックの先頭を表している。
"…,"の部分は任意の値の列が入っている。

```plaintext
    |``4```|
    |value2|  ---\
    |``2```|  ---/  |``6```|
    |value1|   add  |result|
    |``````|        |``````|
    ～～～～         ～～～～
```

## IL命令表

プッシュとは、ILスタックにプッシュすること。ポップとは、ILスタックからポップすること。(`stloc = ldloc` (ldをstに代入(代入の間には**ILスタックを経由**している)))
| アセンブリ                             | ILスタック遷移図                  | 説明 (C#コードとの対応([SharpLab](https://sharplab.io/)でコード書いてILコードを見れば多分何となく分かる))                                                                                                            |
| :------------------------------------- | :-------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **ロード、ストア系 <=ld st=>**         |                                   |                                                                                                                                                                                                                      |
| ILスタック <= 定数                     |                                   |                                                                                                                                                                                                                      |
| `ldnull`                               | `… => …, null`                    | `null`をプッシュ                                                                                                                                                                                                     |
| `ldstr string`                         | `… => …, string`                  | `string`をプッシュ                                                                                                                                                                                                   |
| `ldc.i4 num`                           | `… => …, num`                     | `.i4(int32型)`の`num`をプッシュ                                                                                                                                                                                      |
| ILスタック <=> 引数                    |                                   |                                                                                                                                                                                                                      |
| `ldarg.argNum`                         | `… => …, val`                     | `argNum`をプッシュ                                                                                                                                                                                                   |
| `ldarga.argNum`                        | `… => …, addr`                    | `&argNum`をプッシュ                                                                                                                                                                                                  |
| `starg.argNum`                         | `…, val => …`                     | `val`をポップし、`argNum = val`をする                                                                                                                                                                                |
| ILスタック <=> ローカル変数            |                                   |                                                                                                                                                                                                                      |
| `ldloc.locNum`                         | `… => …, val`                     | `locNum`をプッシュ                                                                                                                                                                                                   |
| `ldloca.locNum`                        | `… => …, addr`                    | `&locNum`をプッシュ                                                                                                                                                                                                  |
| `stloc.locNum`                         | `…, val => …`                     | `val`をポップし、`locNum = val`をする                                                                                                                                                                                |
| ILスタック <=> 参照先                  |                                   |                                                                                                                                                                                                                      |
| `ldobj type`                           | `…, addr => …, val`               | `addr`をポップし、`(type)(*addr)`をプッシュ                                                                                                                                                                          |
| `stobj type`                           | `…, addr, val => …`               | `addr,val`をポップし、`*addr = (type)val`をする                                                                                                                                                                      |
| `ldind.i4`                             | `…, addr => …, val`               | `addr`をポップし、`(int)(*addr)`をプッシュ                                                                                                                                                                           |
| `stind.i4`                             | `…, addr, val => …`               | `addr,val`をポップし、`*addr = (int)val`をする                                                                                                                                                                       |
| ILスタック <=> 配列                    |                                   |                                                                                                                                                                                                                      |
| `ldlen`                                | `…, array => length`              | `array`をポップし、`array.Length` をプッシュ                                                                                                                                                                         |
| `ldelem`                               | `…, array, index => …, val`       | `array,index`をポップし、`array[index]` をプッシュ                                                                                                                                                                   |
| `ldelema`                              | `…, array, index => …, addr`      | `array,index`をポップし、`&(array[index]`) をプッシュ                                                                                                                                                                |
| `stelem`                               | `…, array, index, val => …`       | `array,index,val`をポップし、`array[index] = val` をする                                                                                                                                                             |
| ILスタック <=> フィールド              |                                   | `type`は無かったがSharpLabに出てた。`field`は、`型名::フィールド名`の形式                                                                                                                                            |
| `ldfld type field`                     | `…, obj => …, val`                | `obj`をポップし、`(type)obj.field`をプッシュ (`obj:=⟪O型¦値型⟫`)(`*(&obj+field)`している?)                                                                                                                           |
| `ldflda type field`                    | `…, obj => …, addr`               | `obj`をポップし、`&obj.field`をプッシュ                                                                                                                                                                              |
| `stfld type field`                     | `…, obj, val => …`                | `obj,val`をポップし、`obj.field = (type)val`をする                                                                                                                                                                   |
| ILスタック <=> 静的フィールド          |                                   |                                                                                                                                                                                                                      |
| `ldsfld type field`                    | `…, => …, val`                    | `(type)型.field`をプッシュ                                                                                                                                                                                           |
| `ldsflda type field`                   | `…, => …, addr`                   | `&型.field`をプッシュ                                                                                                                                                                                                |
| `stsfld type field`                    | `…, val => …`                     | `val`をポップし、`型.field = (type)val`をする                                                                                                                                                                        |
| ILスタック <= メソッド(アドレス)       |                                   | `ftn`が何の略か分からないがメソッドのアドレス(エントリーポイント)だと思われる                                                                                                                                        |
| `ldftn method`                         | `… => …, ftn`                     | `method`をプッシュする (`ftn`は`calli`命令で呼び出せる) 例: `ldftn int32 Func(int32)`                                                                                                                                |
| `ldvirtftn method`                     | `…, obj => …, ftn`                | `obj.method`をプッシュする (`ftn`は`calli`命令で呼び出せる) 例: `ldvirtftn instance void Cls::Func(int32)`                                                                                                           |
| ILスタック <= トークン(ランタイム表現) |                                   |                                                                                                                                                                                                                      |
| `ldtoken token`                        | `… => …, RuntimeHandle`           | メタデータトークン(`∫仮/イ/静∫`を判断する情報を持つ。らしい)をランタイム表現に変換してプッシュ(`calli`で呼びる?リフレクションで使う?)                                                                                |
| **分岐系**                             |                                   |                                                                                                                                                                                                                      |
| `br LABEL`                             | `… => …`                          | `LABEL`へ無条件分岐                                                                                                                                                                                                  |
| `beq LABEL`                            | `…, val1, val2 => …`              | `val1,val2`をポップし、`val1 == val2`なら`LABEL`へ分岐                                                                                                                                                               |
| `brtrue LABEL`                         | `…, val => …`                     | `val`をポップし、`val != 0`なら`LABEL`へ分岐                                                                                                                                                                         |
| `switch (LABEL_0, LABEL_1,..)`         | `…, val => …`                     | `val`をポップし、`val == 0`なら`LABEL_0`、`val == 1`なら`LABEL_1`、..へ分岐                                                                                                                                          |
| **コール系**                           |                                   |                                                                                                                                                                                                                      |
| `call method`                          | `…, arg0…argN => …, retVal`       | `arg0…argN`をポップし、`method(arg0…argN)`する。戻り値がある場合は戻り値(`retVal`)をプッシュ (多分静的メソッド専用)                                                                                                  |
| `callvirt method`                      | `…, obj, arg0…argN => …, retVal`  | `obj,arg0…argN`をポップし、`obj.method(arg0…argN)`する。戻り値がある場合は戻り値(`retVal`)をプッシュ (多分仮想とインスタンスメソッド専用)                                                                            |
| `calli callSiteDescr`                  | `…, arg0…argN, ftn => …, retVal`  | `arg0…argN,ftn`をポップし、`ftn(arg0…argN)`する。戻り値がある場合は戻り値(`retVal`)をプッシュ                                                                                                                        |
|                                        |                                   | ldftnとldvirtftnでプッシュされたftnを呼べる。`callSiteDescr`はメソッドの説明(例:{`calli uint32(int32)` 定義: `.method public static uint32 Func(int32)`})                                                            |
| `jmp method`                           | `… => …`                          | 現在のメソッドを終了し、指定したメソッドにジャンプします(現在の引数(`arg`)は指定したメソッドへ転送される)。(多分戻ってこない(メソッドスタックはリセット?))                                                           |
| `ret`                                  | `⟪retVal => retVal¦ => ⟫`         | 現在のメソッドを終了し、呼び出し元のメソッドに戻る。戻り値がある場合は、**現在のILスタックの先頭**を**呼び出し元のILスタックの先頭**に**コピー**する                                                                 |
| **演算子系**                           |                                   |                                                                                                                                                                                                                      |
| 四則演算                               |                                   |                                                                                                                                                                                                                      |
| `add`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 + val2`をプッシュ                                                                                                                                                                       |
| `sub`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 - val2`をプッシュ                                                                                                                                                                       |
| `mul`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 * val2`をプッシュ                                                                                                                                                                       |
| `div`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 / val2`をプッシュ                                                                                                                                                                       |
| `rem`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 % val2`をプッシュ                                                                                                                                                                       |
| `neg`                                  | `…, val => …, result`             | valポップし、-val をプッシュ                                                                                                                                                                                         |
| ビット演算                             |                                   |                                                                                                                                                                                                                      |
| `and`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 & val2`をプッシュ                                                                                                                                                                       |
| `or`                                   | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 \| val2`をプッシュ                                                                                                                                                                      |
| `xor`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 ^ val2`をプッシュ                                                                                                                                                                       |
| `not`                                  | `…, val => …, result`             | valポップし、~val をプッシュ                                                                                                                                                                                         |
| シフト演算                             |                                   |                                                                                                                                                                                                                      |
| `shl`                                  | `…, val, sh_Amount => …, result`  | `val,shAmount`をポップし、`val`を`sh_Amount`分、左にゼロシフトしてプッシュ                                                                                                                                           |
| `shr`                                  | `…, val, sh_Amount => …, result`  | `val,shAmount`をポップし、`val`を`sh_Amount`分、右に符号シフトしてプッシュ                                                                                                                                           |
| `shr.un`                               | `…, val, sh_Amount => …, result`  | `val,shAmount`をポップし、`val`を`sh_Amount`分、右にゼロシフトしてプッシュ                                                                                                                                           |
| 比較演算                               |                                   |                                                                                                                                                                                                                      |
| `ceq`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 == val2` をプッシュ                                                                                                                                                                     |
| `clt`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 <  val2` をプッシュ                                                                                                                                                                     |
| `cgt`                                  | `…, val1 val2 => …, result`       | `val1,val2`をポップし、`val1 >  val2` をプッシュ                                                                                                                                                                     |
| 型変換                                 |                                   |                                                                                                                                                                                                                      |
| `conv.i`                               | `…, val => …, result`             | `val`をポップし、`val`を`native int型`に変換して`native int型`としてプッシュ                                                                                                                                         |
| `conv.u`                               | `…, val => …, result`             | `val`をポップし、`val`を`native unsigned int型`に変換して`native int型`としてプッシュ                                                                                                                                |
| `conv.i1`                              | `…, val => …, result`             | `val`をポップし、`val`を`int8型`に変換して`int32型`としてプッシュ                      (ILスタック内では表現できる型が限られている)                                                                                  |
| `conv.i4`                              | `…, val => …, result`             | `val`をポップし、`val`を`int32型`に変換して`int32型`としてプッシュ                                                                                                                                                   |
| `conv.i8`                              | `…, val => …, result`             | `val`をポップし、`val`を`int64型`に変換して`int64型`としてプッシュ                                                                                                                                                   |
| `conv.u2`                              | `…, val => …, result`             | `val`をポップし、`val`を`unsigned int16型`に変換して`int32型`としてプッシュ                                                                                                                                          |
| `conv.r8`                              | `…, val => …, result`             | `val`をポップし、`val`を`float64型`に変換して`F型`としてプッシュ                                                                                                                                                     |
| **ボックス系**                         |                                   | C#コードで**ボックス化**は、`⟪object¦interface⟫ o = ｢値型｣;` で起こる。**アンボックス**は、さっき代入した `o` を使い `｢値型｣ = o;` で起こる                                                                          |
| `box valuetype`                        | `…, valuetype => …, obj`          | `valuetype`をポップし、`valuetype`を`O型`の参照先(ヒープ)にコピー(ボクシング)してプッシュ (**box化**: {ILスタック[val]} => {{ILスタック[O型]}-->{ヒープ[val]}})                                                      |
| `unbox valuetype`                      | `…, obj => …, valueTypePtr`       | `obj`をポップし、**ボックス化されている**`obj`の`O型`の**参照**を**マネージドポインタ**(`ref`)に**変えて**プッシュ(`box`と違って**コピーが発生しない**)                                                              |
| `unbox.any type`                       | `…, obj => …, ⟪valuetype¦obj⟫`    | `obj`をポップし、`obj`がボックス化されているなら`unbox`の後`ldobj`をしてプッシュ(**コピーが発生**)(`box`の逆射)、そうでないなら`castclass`をしてプッシュ([unbox.any](https://www.asukaze.net/etc/cil/unboxany.html)) |
| **ダウンキャスト系**                   |                                   |                                                                                                                                                                                                                      |
| `castclass type`                       | `…, obj => …, obj`                | `obj`をポップし、`(type)obj`をプッシュ (`O型`は、ダウンキャスト可能かどうか`O型`の参照先(ヒープ)に**動的な型**(SharpLabの`Inspect.Heap`の`type handle`)を持っている)                                                 |
| `isinst type`                          | `…, obj => …, ⟪obj¦null⟫`         | `obj`をポップし、`obj as type`をプッシュ (キャストに成功すれば`obj`をそのまま返し、失敗なら`null`を返す)                                                                                                             |
|                                        |                                   | キャストに成功しても`O型`も`O型の参照先`も(`type handle`(動的な型)を含め)何か**変更される事は無い**。静的な型はプログラム(変数の宣言)が保証する(かO型の参照元を追跡)?                                                |
|                                        |                                   | キャストに失敗した時、`castclass`は**例外**を返し`isinst`は`null`を返す。`castclass`も`isinst`もアップキャストの場合、常に安全なのでコピーしかしない                                                                 |
| **生成系**                             |                                   |                                                                                                                                                                                                                      |
| `newarr type`                          | `…, num => …, array`              | `num`をポップし、`new type[num]`をプッシュ (作れるのは**一次元配列**であり配列は`System.Array`を継承する`O型`)                                                                                                       |
| `newobj .ctor`                         | `…, arg0,…argN => …, obj`         | `arg0,…argN`をポップし、`.ctor(arg0,…argN)`をプッシュ (`O型`が生成される)(フィールドを**ゼロ**(`default`)**初期化**した後、`.ctor`が呼ばれる)(値型は通常`newobj`は呼ばれず、`initobj`で初期化される)                 |
| `localloc`                             | `size => addr`                    | `size`をポップし、`stackalloc type[size]`をプッシュ (`localloc`に`type`の指定が無いが多分、`size * type`のバイト数 で`localloc`のサイズを決める為に必要?)                                                            |
|                                        |                                   | >現在のメソッドが戻ったとき，ローカルメモリプールは**再利用可能**である。らしい。そうすると自分のメモリマップの考えとは違うかもしれない。。                                                                          |
| **初期化系**                           |                                   |                                                                                                                                                                                                                      |
| `initobj type`                         | `…, dest => …`                    | `dest`をポップし、`dest`が**値型へのアドレス**の場合その`dest`の参照先を`new type()`で初期化する (フィールドを**ゼロ初期化**(`default`?)する)(`dest`が`O型`の場合、`null`が代入される)                               |
|                                        |                                   | (`S`は`struct`で、`S s = new S();`とすると`initobj`が実行され、`S s = new S(arg);`とすると`call.ctor`が実行され、`S s;`とすると何も**初期化の処理が実行されない**。)                                                 |
|                                        |                                   | (`C#`の値型は**初期化されていないと使うことが出来ない**ため、`call.ctor`か`initobj`される必要がある)                                                                                                                 |
| `initblk`                              | `…, addr, val, size => …`         | `addr,val,size`をポップし、`val`は`unsigned int8`で、`addr`から`val`を`size`(バイト数)分**レプリケート(複製)**する?                                                                                                  |
| **コピー系**                           |                                   |                                                                                                                                                                                                                      |
| `cpblk`                                | `…, destAddr, srcAddr, size => …` | `destAddr,srcAddr,size`をポップし、`for(int i = 0; i < size; i++){((byte*)destAddr)[i] = ((byte*)srcAddr)[i]}`をする (`srcAddr`から`size`バイト分を`destAddr`にコピーする)                                           |
| `cpobj type`                           | `…, destAddr, srcAddr => …,`      | `destAddr,srcAddr`をポップし、`type`は**値型**であり、`*(type*)destAddr = *(type*)srcAddr`をする                                                                                                                     |
| **例外系**                             |                                   |                                                                                                                                                                                                                      |
| `throw`                                | `…, e => …,`                      | `e`をポップし、`e`は`System.Exception`のインスタンスであり、`throw e`をする (ILの例外の構文: `.try{.try{..}catch{..}}finally{..}`)                                                                                   |
| `rethrow`                              | `… => …`                          | ポップもプッシュもしない? ただ**構文の構成**によって**処理が遷移**している様に見える。(`catch句`に入ると`例外(e)`が**プッシュされている状態**になっている。そして、                                                  |
|                                        |                                   | `catch(Exception e){}`の構文なら`stloc->e`される。`catch{}`の構文なら`pop`される。そして、`catch{throw}`という`catch句`の中に`throw`が入っていると、                                                                 |
|                                        |                                   | それはILでは`rethrow`になっている。そしてまた`try`からの`catch句`でキャッチされる。あと、`try句`の**直前**では**ILスタックが空**でないと**実行時エラー**となる。                                                     |
| `endfilter`                            | `…, bool => …`                    | `bool`をポップし、`filter句`を抜け、`bool`が真なら**対の**`catch句`に移り、偽なら移らない。(`filter句`は`catch(Exception e)when(e.Message=="errer"){}`の`when句`に当たる)                                            |
| `end⟪finally¦fault⟫`                   | `… => …`                          | ポップもプッシュもなし。`⟪finally¦fault⟫句`を終了する (`fault句`はC#には無い)                                                                                                                                        |
| `leave LABEL`                          | `…, =>`                           | 全てポップし**ILスタックを空**にする。プッシュは無し (`leave`は`br`に似ていて`leave`は`try句,catch句,filter句`から**抜ける**のに使います)                                                                            |
| `ckfinite`                             | `…, val => …, val`                | `val`（浮動小数点数）が "数字ではない"値(NaN)または+/-無限大の値である場合に`ArithmeticException`をスローします。                                                                                                    |
| **ILスタック操作系**                   |                                   |                                                                                                                                                                                                                      |
| `dup`                                  | `…, val => …, val, val`           | ILスタックの先頭(`val`)を**複製**します                                                                                                                                                                              |
| `pop`                                  | `…, val => …`                     | `val`をポップする (ILスタックの先頭を**ポップするだけ**)                                                                                                                                                             |
| **その他**                             |                                   |                                                                                                                                                                                                                      |
| `sizeof type`                          | `… => …, size`                    | ポップなし、`sizeof(type)`をプッシュ (**値型のサイズを取得**できる)(C#では参照型のサイズは取得不可)                                                                                                                  |
| `nop`                                  | `… => …`                          | 何もしない。 (バイトコードがパッチされている場合、スペースを埋めることを目的?)(一般的には他のシステムと**同期**をとるためとか、とりあえず処理の代わりとか)                                                           |
| `break`                                | `… => …`                          | **ブレークポイント**に到達したことをデバッガーに通知します。                                                                                                                                                         |
| **よく分からない**                     |                                   |                                                                                                                                                                                                                      |
| ?`unaligned. alignment`                | `…, addr => …, addr`              | `ldind`などでうまく値をILスタックに持ってこれない(**自然サイズに配置されていない**)事を示す？(分からない)                                                                                                            |
| ?`volatile.`                           | `…, addr => …, addr`              | ILスタックにあるアドレスが**複数のスレッドから参照**されている事を示す？(現在実行されているスレッドの外部で参照できる)                                                                                               |
| ?`tail.`                               | 書いてない                        | この次の`call＠⟪i¦virt⟫`命令で現在のメソッドに**戻ってこない**？(現在のメソッドの**ILスタックフレームを削除**する必要があることを示します。)                                                                         |
| **型付き参照系**                       |                                   | [↓相互運用/型付き参照(refとparamで代記できる)](https://ufcpp.net/study/csharp/sp_makeref.html)                                                                                                                       |
| `mkrefany class`                       | `…, addr => …, TypedReference`    | `addr`をポップし、`&val==addr`とし`__makeref(val)`をプッシュする (`ref type r = ref x;`の`ref x`に当たる機能)(`var r = __makeref(val)`は`ldloca; mkrefany class; stloc;`になる)                                      |
| `refanyval type`                       | `…, TypedReference => …, addr`    | `TypedReference`をポップし、`__refvalue(TypedReference, type)`をプッシュする(これは↑の"`r`"**のポインタ**と同じ)                                                                                                     |
| `refanytype`                           | `…, TypedReference => …, type`    | `TypedReference`をポップし、`__reftype(TypedReference)`をプッシュする (`r`は`ref`で`r.GetType()`に当たる機能)                                                                                                        |
| `arglist`                              | `… => …, argListHandle`           | `ArgIterator argumentIterator = new ArgIterator(__arglist);`をするとIL内部で`arglist`が実行される (可変個引数(`__arglist`)**のハンドル**をプッシュする)                                                              |

## ILのメタデータ

| ディレクティブ  | 説明                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| :-------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| auto            | 一番大きい型のサイズから詰めて配置される(classでデフォルト)   //[C# Struct All Things](https://youtu.be/3NMUJdZIdQM?list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&t=1185)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| sequential      | 一番大きい型のサイズで宣言順に配置される(structでデフォルト)  //[StructLayout](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYAMBYAUAenwAthgAHAZxEIE8B7CYCAOgCMBTfAZgDkBZAKoApOAC0AknACKfAPwAbAJYVgAXgAKAGWAArAILiASlCg0uRAOwBpACI2yAazAUAoi6jjtLqZoDuAeQAJRQBhADI1AEZIgA4AVjxCEnIqQggAMwBjMjJmKHZgfBUIOBp8TIoiAEMwMnxFKGB2MDo6gFt2NrowGnkq+kZ8AGJ2AA8yJUzFYABaPoHgRPxAeoZAS4ZAToZAOwZAVYZAYoZAH4ZAa4ZASYZAcYZ9wH6GQAuEwDAlAAoidiq4ZsALBmAaMnYAAmqoOHk7EAp3KANE0AJSvJaAactAAQJgGV5HYHE7na73QC6DIAYhlBgCsGQAiDGDNoBVBNxgHsGaHwwAyDJp+gxgFYGnBmABldgARwg7Eaiiq8kA5gyAQAY1usqTTGPT/sw9Iw6ETcYADBkAigxLQAmDIA/BkAUQxCkmwuGAJIZqQtxYypcA6IBrBkA6gyAfQYdbrANQqgGSGXm4wDRDHcdXcTXQwYAgoiFYLwUCqHQoZCqmR+TOAkEywANtIA3nhvinvhhIsnU6hMGnIsxDNBgIoOsxxI1mq0WWAAG6KCMUADceEzKcIOpb3wA2lGY3HRcA7vGxQzmPgAFSAB3JAKD/Y/wLPZnKLPPHgEdyWdggC6HZUve+84gPYTDWA30UDe+8joUAA5hfz6waE1vqwGwBfQiAADlAFaugHJNQCyDLigDR6psgCtDIAJQyAF0MvKAMXagAAUYAYBlmoAsomAHb+gCqDBiLrNrgqZdj2ECxkOA5EUakrSpu27RgRJ5eoex6nuel43nez6Pj8L7vvg37/kBoGQYA0gyAJEMgAOUYAgQyAGYMqEYVhuAdoQ3ZUYR/aDv2pEuOMkzTJu3yEIAForiYA0kaAKtKdwAGKKOw8hwP46TpBQBR3PRdC2fZwBgmCvKoYAmgxuoAs4mALYM4nQIoV6ANGRVq4tigD+8rigCdpoAQQy8n+LoBjhqaEDu1HfOpEyHp25mWdZLkOZgm70WeXYFVZNl2Q5AAsm5Mbe8jnvlFnVcVA6ROgm4Pk+HEdrhhCAIxRgD3yhh4mRbigBwZoAXJ6ACFugA3DGBgBFDIAZQyABMMrywYA8QwYuJgAVDKs+yAEAMZCZHKryAFgJMlySsGwdgpvZESphojvui7cvIFFpSmGB7myEAhPIFBHo0DEXlezX3mxz5vthuFPdRL0kSO47TrOXqruuW6/Wm6DfF6wOg+VjFQyxfXsfDsl4/J+FKQsr20mpGl1lpuNDfg/05fIxMJm1hU1a5dylRupOVe1RW1QODUbk1LECx10t3N1vWwwNNO4RgXDfEmeNa+gBN67hJtZpEACcdwAESADwbgAg+1bYJNvrps6fg7Yu6b+49t8FAUN8qjfPkvgA+yPZ3E7g2u2WobsLGzLAOGDh3H7keeybtHRmmqBVP7gfB4T0rh2nrsmzHXzx1GSd3KgOcUCXpfpfd6xR17gPE77mQB0H7Ah/uxMR87jepuXcfAMwgRPGQKeZA3w9EyD3zht3BcLxQg+t2XUCx/Hk9VNP4Zzybr4difuCvkAA==) |
| beforefieldinit | フィールドメンバアクセス時よりも前に、静的メンバ変数を0初期化する?(default(T)の状態になる?)(静的コンストラクタを明示的に書くと消える)[静的コンストラクタ](http://csharper.blog57.fc2.com/blog-category-3.html)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| sealed          | 継承不可                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| ansi            | 文字列をASCIIとしてプラットフォームにマーシャリングします。(ASCIIはANSIが定める文字コード。[ANSI](http://www.daido-it.ac.jp/~oishi/HT191/ht191.html)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| hidebysig       | _名前と署名(シグネチャ?)で非表示にします。 ランタイムでは無視されます。(基底クラスのメソッドと同じシグネチャを持つ時、非表示になる?(newがいる))                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| specialname     | 主に.ctor()に付いていてCLR以外に特別な名前であることを示す                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| rtspecialname   | 主に.ctor()に付いていてCLRに特別な名前であることを示す                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| cil managed     | このメソッドの本体がCILのコードでマネージドメソッドであることを示します。managed以外を記述するのは稀です。                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |

| 具体的なIL命令書式(引数も含める)                                   |
| :----------------------------------------------------------------- |
| `ldc⟪❰｡｡❰.i4❱＠⟪｡❰.⟪0～8⟫❱¦.m1¦.s num｡⟫｡｡❱｡¦｡❰⟪.i8¦.r4¦.r8⟫ num❱⟫` |
