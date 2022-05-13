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

- CLI: .netを中心に**多言語を多プラットフォーム**で動くようにした仕様。ECMA-335で標準化されている 
  - CLR: CLRはCLIをWindows上で動くように**マイクロソフトが実装**したもの
  - CTS: プログラミング言語間で共通して用いられる**型の集合**  
    - CIL: IL, MSIL とも呼ばれ、多言語をこのILにコンパイルして**多言語間で共通のライブラリ**として使える
      - ↓はCLIの実装部分、各プラットフォームにインストールされるもの  
      - CBL: 開発者が書いたコードを各プラットフォームで動くようにした**ライブラリ**(ILとネイティブ？)
      - VES: 開発者のILとCBLを使ってこのプラットフォーム用の**ネイティブコードを生成**する
        - これを実行時、関数ごとにやるのが**JITコンパイラ**、プログラム起動前にやるのが**AOTコンパイラ**

## [ILコードの例](http://www5b.biglobe.ne.jp/~yone-ken/VBNET/IL/il08_NewObj.html)  
```
.assembly NewObjAndCallInstanceMethod {}
.method public static void main()
{
    .entrypoint //このプログラムのエントリーポイント。"main"という名前はエントリーポイントにならない。
    .maxstack 3 //この関数で使うスタックの大きさ。無駄に大きいとメモリを無駄に食う。足りないとエラー

    ldstr "16進数へ："

    //スタックにプッシュされたstring("16進数へ:")を引数にとり、参照型のオブジェクトを生成(newobj)し、そのインスタンス(instance)のコンストラクタ(.ctor)を呼び出し初期化して返す。
    newobj instance void [mscorlib]System.Text.StringBuilder::.ctor(string)
    
    ldstr "255={0:X}"
    ldc.i4 255
    //スタックにプッシュされたInt32(255)を引数にとり、ボックス化(box)された参照型を返す。
    box valuetype [mscorlib]System.Int32  //値型(struct)はvaluetype //参照型(O型,class)はclass

    //call instanceは、インスタンスメソッドであり、第零引数にそのインスタンス(StringBuilder)、第一引数にstring("255={0:X}")、第ニ引数にobject(.i4 255のボックス化)をとり、
      //新しいオブジェクト(StringBuilder)を生成して返す。  
    call instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::AppendFormat(string, object)

    //第零引数にそのインスタンス(StringBuilder)をとり、stringを返す。  
    call instance string [mscorlib]System.Text.StringBuilder::ToString()

    //callのみは静的関数。stringを引数にとり、Consoleに出力する。戻り値は無し。
    call void [mscorlib]System.Console::WriteLine(string)

    // 戻り値がvoidなため、ILスタックが空でなければいけない。戻り値がある場合はその型がちょうど一つある事。
    ret
}
```
|                    |                          |                      |                         |                         |                         |                             |                         |                    |
| :----------------- | :----------------------- | :------------------- | :---------------------- | :---------------------- | :---------------------- | :-------------------------- | :---------------------- | :----------------- |
|                    |                          |                      |                         | int32<br>(255)          | box_int32<br>(255)      |                             |                         |                    |
| スタック状態の関係 |                          |                      | string<br>("255={0:X}") | string<br>("255={0:X}") | string<br>("255={0:X}") |                             |                         |                    |
|                    | string<br>("16進数へ：") | StringBuilder        | StringBuilder           | StringBuilder           | StringBuilder           | StringBuilder               | string                  |                    |
| ============       | ==========               | ============         | =========               | =========               | =========               | ===================         | ==============          | ============       |
| 実行命令           | ldstr                    | newobj               | ldstr                   | ldc.i4                  | box                     | call                        | call                    | call               |
| 実行メソッド       |                          | StringBuilder::.ctor |                         |                         |                         | StringBuilder::AppendFormat | StringBuilder::ToString | Console::WriteLine |

ILはスタックマシン、逆ポーランド記法 {3 4 + 1 2 - *} <=> {(3 + 4) * (1 - 2)}(数字(データ)をプッシュして演算子(関数)でポップして処理して結果をプッシュする)  

## コンパイルから.netの実行まで  

```
  |IL| ---ilasm.exe--> |   .exe    |  
  |__| <--ildasm.exe-- |   .dll    | ---CLR(AOT or JIT)--> ネイティブ  
  |C#| --csc.exe---->  |IL(バイナリ)|  
```
- コンパイラ  
  - cmd.exeでのコンパイルは、csc.exe source.cs System_0.dll System_1.dll.. (**C#コンパイラ ソースコード 参照するアセンブリ(dll)..**)  
    とすると、**dllかexeファイルが生成**される。のでdllの循環参照は起こらない?(https://qiita.com/asterisk9101/items/4f0e8da6d1e2e9df8e14)  
  - コンパイラ: C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe (そこのdll群はランタイム用??)  
  - コンパイラのオプション:https://qiita.com/toshirot/items/dcf7809007730d835cfc
- .NETライブラリ  
  - **mscorlib.dll**(System.Objectとかの基本型)は指定しなくても参照される(https://qiita.com/gdrom1gb/items/69ed26a72c6c2b9445e3)  
  - 参照するアセンブリ(dll): C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1 (参照アセンブリは定義のみで中身空っぽ??)  
  - GAC(グローバル・アセンブリ・キャッシュ): C:\Windows\assembly (↑↑をキャッシュしたもの??Windows自身がOSで使っている??)  
- .netの実行: {windowsローダー} => {win32(PEヘッダ)} => {.net(IL(バイナリ))(CLR)}  

## アセンブリの構造  

.netは**アセンブリと言う単位**でプログラムが読まれる  
アセンブリは複数ファイルにまたがる事がある。  

CLI(BCLとVES)は実行環境によって色々ある   

- CLRヘッダ  
  - マニフェスト: このアセンブリの基本情報  
- CLRデータ  
  - 型メタデータ: 型情報  
  - ILコード: 複数の関数が定義されてる  
  - マネージ・リソース: 文字列や画像などのデータ  

## JITコンパイラ

ある関数を初めて実行する時、その関数を実行するための型を**型メタデータ**から読まれて検証され  
JITコンパイラによってその関数がネイティブコードにコンパイルされる  
その後、再びその関数を呼ぶ時は**既にコンパイルされたネイティブコードを実行**する。  
そのため、起動時は重い  
ネイティブ実行中は、**GC, 例外, 実行権限**の恩恵を受ける。  

## ILの構造  

"."から始まる文字列はディレクティブと言い、アセンブラがプログラムの構造を認識するために用いられるもの  
- スタックには**int⟪32¦64⟫, native int, F型, O型, ポインタ型(native unsigned int, &)**しか区別できない  
  - スタックとスタック以外に入出力する場合は暗黙的な型変換が入る(0拡張、符号拡張、切り捨て、0方向への切り捨て)  
- マネージポインタ(C#のref?)は、  
  - 何らかの構造のフィールドに存在しない  
  - 静的変数(静的領域?)に存在しない  
  - スタック、引数、ローカル変数に存在できる(**メソッドの中でしか存在できない**)  
  - マネージポインタはマネージポインタを指さない(**それ以外は何でも指せる**)  
  - **nullにならない**  
- O型(C#の参照型?)は、  
  - ⟪class¦valueTypeのボックス化表現(つまりclass)⟫  
  - isinst命令で動的な型(System.TypedReferenceではない)をobjから調べれるので動的な型の情報をもっている？  
  - あるCILオブジェクト命令（特にnewobjおよびnewarr）によって作成されます。  
  - 引数(call)、返り値(ret)、ローカル変数(stloc)、配列の要素(stelem)、フィールド(stfld)、に格納出来ます(**全ての場所に格納**できる)。  
- ILコードの１行の形式は、  
  - **ラベル: 命令 オペランド**  
  - 全ての命令は**スタック、定数、引数、ローカル変数、ヒープ、外部メモリ、演算器、プログラムカウンタ**への命令  
  - 命令は殆ど1バイト + 定数のバイト数 = １行のバイト数  
    - IL命令が32バイト以下かつ反復と例外を含んでいないあと仮想呼び出し、デリゲートでない時、JITコンパイラはその関数は**インライン化**する(関数を展開して埋め込まれる)  
- C#のプリミティブ型とILが認識する型の対応表  
  | C#キーワード           | ILの型 (ILの命令)        | C#型定義                | 型の種類      | 詳細                                                                          |
  | :--------------------- | :----------------------- | :---------------------- | :------------ | :---------------------------------------------------------------------------- |
  | object                 | object                   | System.Object           | 参照型(class) | 値型をこれ(object)にキャスト変換するとボクシングが発生する                    |
  | bool                   | bool                     | System.Boolean          | 値型(struct)  | bool型は1バイト                                                               |
  | byte                   | uint8 (.i1)              | System.Byte             | 値型(struct)  |                                                                               |
  | char                   | char                     | System.Char             | 値型(struct)  | char型は2バイトでUnicodeのU+0x0~U+0xFFFFを表現する。あと、無理やり、          |
  | short                  | int16 (.i2)              | System.Int16            | 値型(struct)  | ac[0]=(char)0xD83D; ac[1]=(char)0xDE03;と入れると4バイトも表現できなくもない  |
  | int                    | int32 (.i4)              | System.Int32            | 値型(struct)  |                                                                               |
  | long                   | int64 (.i8)              | System.Int64            | 値型(struct)  | 全ての値型(struct)は、System.ValueTypeを継承し、                              |
  | ushort                 | uint16 (.u2)             | System.UInt16           | 値型(struct)  | System.ValueTypeはSystem.Objectを継承                                         |
  | uint                   | uint32 (.u4)             | System.UInt32           | 値型(struct)  |                                                                               |
  | ulong                  | uint64 (.u8)             | System.UInt64           | 値型(struct)  |                                                                               |
  | float                  | float32 (.r4)            | System.Single           | 値型(struct)  |                                                                               |
  | double                 | float64 (.r8)            | System.Double           | 値型(struct)  |                                                                               |
  | string                 | string                   | System.String           | 参照型(class) | C#の文字コードはUTF-16でchar型で表現できない文字はstringで表現する            |
  | int[]                  | int32[]                  | System.Int32[]          | 参照型(class) | 全ての配列はSystem.Arrayを継承。ldelemでの配列命令の為に配列長と要素型がある? |
  | int*                   | int32*                   | System.Int32*           | ポインタ型    | typeofで調べたらSystem.Int32*と出た                                           |
  | ref int                | int32&                   | 調べられない            | ポインタ型    |                                                                               |
  | ↓ユーザー定義型の定義↓ |                          |                         |               |                                                                               |
  | struct S{}             | valuetype S              | S                       | 値型(struct)  |                                                                               |
  | class C{}              | class C                  | C                       | 参照型(class) |                                                                               |
  | class C<T,U>{}         | class S\`2<int32, int64> | C<int,long>             | 参照型(class) | 変数を、`C<int,long> c;`と定義                                                |
  | fixed(int* p = arr){}  | int32* とint32[] pinned  | System.Int32*とInt32[]? | 値型と参照型? | pinnedにアドレスが入るとGCはそのデータを移動しない、nullを入れると解除        |
  | delegate void F()      | class F                  | F                       | 参照型(class) | [System.Runtime]System.MulticastDelegateを継承                                |
  | enum E{}               | valuetype E              | E                       | 値型(struct)  | [System.Runtime]System.Enumを継承 ([..]はアセンブリ名(System.Runtime.dll))    |
  |                        |                          |                         |               |                                                                               |
## ILの命令

v := value, a:= arg, TypeTokenは型

```
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
|      | -----box,unbox＠❰.any❱------------- |ボックス化{値型(スタック)}<->{O型(ヒープ)}            |
|      | -----cp⟪blk¦obj⟫------------------ |コピー(スタックか外部メモリ?)                         |
|      | -----init⟪obj¦blk⟫---------------- |領域初期化                                          |
|      | =====dup, pop===================== |スタック操作                                         |
"==..="は、ILスタックからILスタック。 "--..-"は、領域の初期化やコピーとポックス化
ILスタックは何かを計算したりコピーしたり制御信号を送信したりするための一時的なデータの保管場所(アキュームレータの様な)
    ⟦=>┃～⟧❰｡⟦=>┃1～⟧❰プッシュ❱ => 処理(ポップ、計算、コピー、制御信号)｡❱ 

　~底位アドレス~    (この図は感で書いている)
 | プログラム  |  
 |    定数    |   
 |  静的変数  |   
↓|   ヒープ   |
↓|   ヒープ   | (マネージドヒープ(GCの管理下にある))
↓|   ヒープ   |
 |    ...     |
 |    ...     |
↑|--繰り返し--|    (stackallocは明示的開放不可。関数から戻るときに自動的に破棄。)(stackallocを使うとILでlocallocが呼ばれるのでlocallocとstackallocは同じ))
↑| localloc?　|ス ((stackalloc int[DateTime.Now.Second])[30]ができて関数実行中動的生成(確保)できているので可変量だと思う)
↑| ILスタック?|タ (ILスタックは.maxstack n で容量が決められているので固定量)
↑|ローカル変数 |ッ (この✖❰IL❱スタックはスレッド毎にあると思う)
↑|    引数    |ク    |         |
↑|--繰り返し--|      |外部メモリ|
 ~高位アドレス~      |         |

```

プッシュとは、ILスタックにプッシュすること。ポップとは、ILスタックからポップすること。(`stloc = ldloc` (ldをstに代入(代入の間には**スタックを経由**している)))
| アセンブリ                          | スタック遷移                    | 説明                                                                                                                                                        |
| :---------------------------------- | :------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **ロード、ストア系 ←ld st→**        |                                 |                                                                                                                                                             |
| スタック ← 定数                     |                                 |                                                                                                                                                             |
| @`ldnull`                           | … => …, null                    | nullをプッシュ                                                                                                                                              |
| @`ldstr string`                     | … => …, string                  | stringをプッシュ                                                                                                                                            |
| @`ldc.i4 num`                       | … => …, num                     | .i4型のnumをプッシュ                                                                                                                                        |
| スタック ⇔ 引数                     |                                 |                                                                                                                                                             |
| @`ldarg.argNum`                     | … => …, val                     | argNumをプッシュ                                                                                                                                            |
| `ldarga.argNum`                     | … => …, addr                    | &argNumをプッシュ                                                                                                                                           |
| @`starg.argNum`                     | …, val => …                     | valをポップし、argNum = valをする                                                                                                                           |
| スタック ⇔ ローカル変数             |                                 |                                                                                                                                                             |
| @`ldloc.locNum`                     | … => …, val                     | locNumをプッシュ                                                                                                                                            |
| `ldloca.locNum`                     | … => …, addr                    | &locNumのアドレスをプッシュ                                                                                                                                 |
| @`stloc.locNum`                     | …, val => …                     | valをポップし、locNum = valをする                                                                                                                           |
| スタック ⇔ 参照先                   |                                 |                                                                                                                                                             |
| `ldobj type`                        | …, addr => …, val               | addrをポップし、(type)(*addr)をプッシュ                                                                                                                     |
| `stobj type`                        | …, addr, val => …               | addr,valをポップし、*addr = (type)valをする                                                                                                                 |
| `ldind.i4`                          | …, addr => …, val               | addrをポップし、(int)(*addr)をプッシュ                                                                                                                      |
| `stind.i4`                          | …, addr, val => …               | addr,valをポップし、*addr = (int)valをする                                                                                                                  |
| スタック ⇔ 配列                     |                                 |                                                                                                                                                             |
| @`ldlen`                            | …, array => length              | arrayをポップし、array.Length をプッシュ                                                                                                                    |
| @`ldelem`                           | …, array, index => …, val       | array,indexをポップし、array[index] をプッシュ                                                                                                              |
| `ldelema`                           | …, array, index => …, addr      | array,indexをポップし、&(array[index]) をプッシュ                                                                                                           |
| @`stelem`                           | …, array, index, val => …       | array,index,valをポップし、array[index] = val をする                                                                                                        |
| スタック ⇔ フィールド               |                                 | typeは無かったがSharpLabに出てた。fieldは、型名::フィールド名の形式                                                                                         |
| `ldfld type field`                  | …, obj => …, val                | objをポップし、(type)obj.fieldをプッシュ (obj:=⟪O型¦値型⟫)(*(&obj+field)している?)                                                                          |
| `ldflda type field`                 | …, obj => …, addr               | objをポップし、&((type)obj.field)をプッシュ                                                                                                                 |
| `stfld type field`                  | …, obj, val => …                | obj,valをポップし、obj.field = (type)valをする                                                                                                              |
| スタック ⇔ フィールド(静的)         |                                 |                                                                                                                                                             |
| `ldsfld type field`                 | …, => …, val                    | field(型.field)をプッシュ                                                                                                                                   |
| `ldsflda type field`                | …, => …, addr                   | &field(&型.field)をプッシュ                                                                                                                                 |
| `stsfld type field`                 | …, val => …                     | valをポップし、field(型.field) = valをする                                                                                                                  |
| スタック ← メソッド(アドレス)       |                                 |                                                                                                                                                             |
| `ldftn method`                      | … => …, ftn                     | methodからそのメソッドへのポインタ(ftn(アンマネージポインタ))をスタックに積む。ftnはcalli命令で呼び出せる。                                                 |
| `ldvirtftn method`                  | …, obj => …, ftn                | スタックにインスタンスオブジェクト(obj)があり、そのobjのmethod(仮想メソッド)へのポインタ(ftn)をスタックに積む。ftnはcalli命令で呼び出せる。                 |
| スタック ← トークン(ランタイム表現) |                                 |                                                                                                                                                             |
| `ldtoken token`                     | … => …, RuntimeHandle           | メタデータトークン(識別子とかキーワードとか？)をランタイム表現(RuntimeHandle)に変換してスタックに積む。(良くわからない)                                     |
| **分岐系**                          |                                 |                                                                                                                                                             |
| @`br LABEL`                         | … => …                          | LABELへ無条件分岐                                                                                                                                           |
| @`beq LABEL`                        | …, val1, val2 => …              | val1,val2をポップし、val1 == val2ならLABELへ分岐                                                                                                            |
| @`brtrue LABEL`                     | …, val => …                     | valをポップし、val != 0ならLABELへ分岐                                                                                                                      |
| @`switch (LABEL_0, LABEL_1,..)	`    | …, val => …                     | valをポップし、val == 0ならLABEL_0、val == 1ならLABEL_1、..へ分岐                                                                                           |
| **コール系**                        |                                 |                                                                                                                                                             |
| @`call`                             | …, arg0…argN => …, (rV)         | メソッドを呼び出す                                                                                                                                          |
| `callvirt method`                   | …, obj, arg0…argN => …, (rV)    | class A{virtual T f(){}} class B{override T f(){}}。ポリモーフィズム。vtableでobjに関連付けられたメソッドを呼び出す。                                       |
| `calli callSiteDescr`               | …, arg0…argN, ftn => …, (rV)    | リフレクション。スタックに引数とメソッドへのポインタ(ftn)があり、そのメソッドにその引数を使って呼び出す。callSiteDescrは引数の説明?(多分、長さとか型とか)。 |
| `jmp method`                        | … => …                          | 現在のメソッドを終了し、指定したメソッドにジャンプします。現在の引数を宛先の引数に転送(arg?)                                                                |
| @`ret`                              | (rV(先))=> …, (rV(元))          | 呼び出し先の評価スタックの戻り値を呼び出し先の評価スタックの戻り値に積む。型も呼び出し元の型として管理される？                                              |
| **演算子系**                        |                                 |                                                                                                                                                             |
| 四則演算                            |                                 |                                                                                                                                                             |
| @`add`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 + val2をプッシュ                                                                                                                  |
| @`sub`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 - val2をプッシュ                                                                                                                  |
| @`mul`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 * val2をプッシュ                                                                                                                  |
| @`div`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 / val2をプッシュ                                                                                                                  |
| @`rem`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 % val2をプッシュ                                                                                                                  |
| @`neg`                              | …, val => …, result             | -val をプッシュ                                                                                                                                             |
| ビット演算                          |                                 |                                                                                                                                                             |
| @`and`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 & val2をプッシュ                                                                                                                  |
| @`or`                               | …, val1 val2 => …, result       | val1,val2をポップし、val1 \| val2をプッシュ                                                                                                                 |
| @`xor`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 ^ val2をプッシュ                                                                                                                  |
| @`not`                              | …, val => …, result             | valポップし、~val をプッシュ                                                                                                                                |
| シフト演算                          |                                 |                                                                                                                                                             |
| @`shl`                              | …, val, sh_Amount => …, result  | val,shAmountをポップし、valをsh_Amount分、左にゼロシフトしてプッシュ                                                                                        |
| @`shr`                              | …, val, sh_Amount => …, result  | val,shAmountをポップし、valをsh_Amount分、右に符号シフトしてプッシュ                                                                                        |
| @`shr.un`                           | …, val, sh_Amount => …, result  | val,shAmountをポップし、valをsh_Amount分、右にゼロシフトしてプッシュ                                                                                        |
| 比較演算                            |                                 |                                                                                                                                                             |
| @`ceq`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 == val2 をプッシュ                                                                                                                |
| @`clt`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 <  val2 をプッシュ                                                                                                                |
| @`cgt`                              | …, val1 val2 => …, result       | val1,val2をポップし、val1 >  val2 をプッシュ                                                                                                                |
| 型変換                              |                                 |                                                                                                                                                             |
| @`conv.i`                           | …, val => …, result             | valをポップし、valをnative int型に変換してnative int型としてプッシュ                                                                                        |
| @`conv.u`                           | …, val => …, result             | valをポップし、valをnative unsigned int型に変換してnative int型としてプッシュ                                                                               |
| @`conv.i1`                          | …, val => …, result             | valをポップし、valをint8型に変換してint32型としてプッシュ     (ILスタック内では表現できる型が限られている)                                                  |
| @`conv.i4`                          | …, val => …, result             | valをポップし、valをint32型に変換してint32型としてプッシュ                                                                                                  |
| @`conv.i8`                          | …, val => …, result             | valをポップし、valをint64型に変換してint64型としてプッシュ                                                                                                  |
| @`conv.u2`                          | …, val => …, result             | valをポップし、valをunsigned int16型に変換してint32型としてプッシュ                                                                                         |
| @`conv.r8`                          | …, val => …, result             | valをポップし、valをfloat64型に変換してF型としてプッシュ                                                                                                    |
| **ボックス系**                      |                                 |                                                                                                                                                             |
| @`box ＄vTT=❰valuetype❱`            | …, val(∫vTT∫) => …, obj         | 値型(val)をpopし、それをbox化したO型をpushする。 box化: {スタック[val]} => {{スタック[O型]} -> {ヒープ[val]}}                                               |
| @`unbox valuetype`                  | …, obj => …, val(∫vTT∫)         | ↑の逆操作。O型(obj)をpopし、それをunbox化した値型をpushする。(本当の内部の挙動は違うかもしれない?)                                                          |
| `unbox.any ＄TT=❰type❱`             | …, obj => …, ❰val¦obj❱(∫TT∫)    | 多分、objが、box化された値型なら`unbox`と同じ。そうでないなら`castclass`と同じ。                                                                            |
| **オブジェクト操作系**              |                                 |                                                                                                                                                             |
| `castclass type`                    | …, obj => …, obj2(∫TT∫)         | (T)obj。 スタックにobj(O型)があり、❰type型❱にダウンキャスト(アップキャストは使われない)してまた積む(obj2)。O型はアドレスと動的な?型も持っている？           |
| `isinst type`                       | …, obj => …, ❰res(∫TT∫)¦null❱   | type res = obj as type。 は、❰obj❱が❰type❱のインスタンスかテストし、nullかそのインスタンスを返す。                                                          |
| **生成系**                          |                                 |                                                                                                                                                             |
| @`newarr`                           |                                 | etype型の要素を持つ新しい配列を作成します                                                                                                                   |
| @`newobj .ctor`                     | …, arg0,..argN => …, obj        | .ctor(arg0,..argN) をプッシュ (ヒープ領域にそのインスタンス用のメモリ領域を確保しO型を生成)                                                                 |
| `localloc`                          | size => addr                    | スタックにsizeがあり、sizeバイト分の領域をローカルメモリプール?(ローカルヒープ?)から確保しその先頭アドレスをスタックに積む。                                |
|                                     |                                 | (stackalloc int[DateTime.Now.Second])[30] できるので動的生成できている                                                                                      |
| **初期化系**                        |                                 |                                                                                                                                                             |
| `initobj type`                      | …, addr => …                    | スタックにアドレスがあり、typeが値型の場合そのアドレスをその値型で初期化する。typeが参照型の場合はnullが入る。                                              |
| `initblk`                           | …, addr, val, size => …         | スタックにアドレス、値(unsigned int8)、バイト数があり、そのアドレスにその値でバイト数分初期化する。(値をレプリケートする？)                                 |
| **コピー系**                        |                                 |                                                                                                                                                             |
| `cpblk`                             | …, destaddr, srcaddr, size => … | スタックに送信元、送信先、バイト数(blk)があり、送信元から送信先へバイト数分データをコピーする。                                                             |
| `cpobj type`                        | …, dest, src => …,              | dest = (type)src。srcValObjからdestValObjに値をコピーします。                                                                                               |
| **例外系**                          |                                 |                                                                                                                                                             |
| @`throw`                            |                                 | 例外をスローします           .try{.try{..}catch{..}}finally{..}                                                                                             |
| `rethrow`                           | … => …                          | catch句でキャッチした例外を再スローする。(catch句内のみ使用可能)                                                                                            |
| `endfilter`                         | …, val => …                     | 例外処理のfilterが何か知らないけどendfinallyと同じ様にfilter句の最後で値(val)を取って使ってfilter句の処理を終わる。                                         |
| `end❰finally¦fault❱`                | … => …                          | 例外ブロックの❰finally¦fault❱節を終了                                                                                                                       |
| @`leave`                            | …, =>                           | コードの保護された領域を終了します。                                                                                                                        |
| `ckfinite`                          | …, val => …, val                | if(val == NaN){throw("ArithmeticException");}。スタックにF型?があり、値が有限でない場合(無限?)例外を投げる。値がF型でない場合何もしない?                    |
| **スタック操作系**                  |                                 |                                                                                                                                                             |
| `dup`                               | …, val => …, val, val           | スタックの一番上の値を複製します。                                                                                                                          |
| `pop`                               | …, val => …                     | pop命令は、スタックから最上位の要素を削除します。                                                                                                           |
| **その他**                          |                                 |                                                                                                                                                             |
| `sizeof type`                       | … => …, size                    | typeが値型の場合スタックにその値型のサイズ(バイト数)を積む。参照型の場合は参照先のサイズではなく参照のアドレスサイズになる？                                |
| @`nop`                              | … => …                          | 何もしない。 バイトコードがパッチされている場合、スペースを埋めることを目的。                                                                               |
| `break`                             | … => …                          | ブレークポイントに到達したことをデバッガーに通知します。                                                                                                    |
| **よく分からない**                  |                                 |                                                                                                                                                             |
| ?`unaligned. alignment`             | …, addr => …, addr              | ldindなどでうまく値をスタックに持ってこれない(自然サイズに配置されていない)事を示す？(分からない)                                                           |
| ?`volatile.`                        | …, addr => …, addr              | スタックにあるアドレスが複数のスレッドから参照されている事を示す？(現在実行されているスレッドの外部で参照できる)                                            |
| ?`tail.`                            | 書いてない                      | この次のcall@❰i¦virt❱命令で現在のメソッドに戻ってこない？(現在のメソッドのスタックフレームを削除する必要があることを示します。)                             |
| **型付き参照系**                    |                                 | [↓相互運用/型付き参照](https://ufcpp.net/study/csharp/sp_makeref.html)                                                                                      |
| `mkrefany class`                    | …, ptr => …, typedRef           | (class❰*¦&❱)ptr。スタックに❰&型¦native int❱があり、classへのポインタにする？                                                                                |
| `refanyval type`                    | …, TypedRef => …, addr          | type &a = b;のaのアドレス？。 スタックに値型参照(&)？があり、そのアドレスをスタックに積む。                                                                 |
| `refanytype`                        | …, TypedRef => …, type          | 型指定された参照(TypeRef)には、型トークンとオブジェクトインスタンスへのアドレスが含まれています。その型トークンをスタックに積む。                           |
| `arglist`                           | … => …, argListHandle           | 現在のメソッドの引数リストハンドル(System.RuntimeArgumentHandle型?(native intのアンマネージポインター))を返すらしい。                                       |
<!-- markdownlint-enable MD013 -->

## ILのメタデータ

| ディレクティブ  | 説明                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| :-------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| auto            | この型はヒープに生成される?(class)     //←↓違うhttps://youtu.be/3NMUJdZIdQM?list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&t=1185                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| sequential      | この型はスタックに生成される?(struct)  //    [StructLayout](https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYAMBYAUAenwAthgAHAZxEIE8B7CYCAOgCMBTfAZgDkBZAKoApOAC0AknACKfAPwAbAJYVgAXgAKAGWAArAILiASlCg0uRAOwBpACI2yAazAUAoi6jjtLqZoDuAeQAJRQBhADI1AEZIgA4AVjxCEnIqQggAMwBjMjJmKHZgfBUIOBp8TIoiAEMwMnxFKGB2MDo6gFt2NrowGnkq+kZ8AGJ2AA8yJUzFYABaPoHgRPxAeoZAS4ZAToZAOwZAVYZAYoZAH4ZAa4ZASYZAcYZ9wH6GQAuEwDAlAAoidiq4ZsALBmAaMnYAAmqoOHk7EAp3KANE0AJSvJaAactAAQJgGV5HYHE7na73QC6DIAYhlBgCsGQAiDGDNoBVBNxgHsGaHwwAyDJp+gxgFYGnBmABldgARwg7Eaiiq8kA5gyAQAY1usqTTGPT/sw9Iw6ETcYADBkAigxLQAmDIA/BkAUQxCkmwuGAJIZqQtxYypcA6IBrBkA6gyAfQYdbrANQqgGSGXm4wDRDHcdXcTXQwYAgoiFYLwUCqHQoZCqmR+TOAkEywANtIA3nhvinvhhIsnU6hMGnIsxDNBgIoOsxxI1mq0WWAAG6KCMUADceEzKcIOpb3wA2lGY3HRcA7vGxQzmPgAFSAB3JAKD/Y/wLPZnKLPPHgEdyWdggC6HZUve+84gPYTDWA30UDe+8joUAA5hfz6waE1vqwGwBfQiAADlAFaugHJNQCyDLigDR6psgCtDIAJQyAF0MvKAMXagAAUYAYBlmoAsomAHb+gCqDBiLrNrgqZdj2ECxkOA5EUakrSpu27RgRJ5eoex6nuel43nez6Pj8L7vvg37/kBoGQYA0gyAJEMgAOUYAgQyAGYMqEYVhuAdoQ3ZUYR/aDv2pEuOMkzTJu3yEIAForiYA0kaAKtKdwAGKKOw8hwP46TpBQBR3PRdC2fZwBgmCvKoYAmgxuoAs4mALYM4nQIoV6ANGRVq4tigD+8rigCdpoAQQy8n+LoBjhqaEDu1HfOpEyHp25mWdZLkOZgm70WeXYFVZNl2Q5AAsm5Mbe8jnvlFnVcVA6ROgm4Pk+HEdrhhCAIxRgD3yhh4mRbigBwZoAXJ6ACFugA3DGBgBFDIAZQyABMMrywYA8QwYuJgAVDKs+yAEAMZCZHKryAFgJMlySsGwdgpvZESphojvui7cvIFFpSmGB7myEAhPIFBHo0DEXlezX3mxz5vthuFPdRL0kSO47TrOXqruuW6/Wm6DfF6wOg+VjFQyxfXsfDsl4/J+FKQsr20mpGl1lpuNDfg/05fIxMJm1hU1a5dylRupOVe1RW1QODUbk1LECx10t3N1vWwwNNO4RgXDfEmeNa+gBN67hJtZpEACcdwAESADwbgAg+1bYJNvrps6fg7Yu6b+49t8FAUN8qjfPkvgA+yPZ3E7g2u2WobsLGzLAOGDh3H7keeybtHRmmqBVP7gfB4T0rh2nrsmzHXzx1GSd3KgOcUCXpfpfd6xR17gPE77mQB0H7Ah/uxMR87jepuXcfAMwgRPGQKeZA3w9EyD3zht3BcLxQg+t2XUCx/Hk9VNP4Zzybr4difuCvkAA==) |
| beforefieldinit | フィールドメンバアクセス時よりも前に、静的メンバ変数を0初期化する?(default(T)の状態になる?)(静的コンストラクタを明示的に書くと消える)http://csharper.blog57.fc2.com/blog-category-3.html                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| sealed          | 継承不可                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ansi            | 文字列をASCIIとしてプラットフォームにマーシャリングします。(ASCIIはANSIが定める文字コード。http://www.daido-it.ac.jp/~oishi/HT191/ht191.html)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| hidebysig       | _名前と署名(シグネチャ?)で非表示にします。 ランタイムでは無視されます。(基底クラスのメソッドと同じシグネチャを持つ時、非表示になる?(newがいる))                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| specialname     | 主に.ctor()に付いていてCLR以外に特別な名前であることを示す                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| rtspecialname   | 主に.ctor()に付いていてCLRに特別な名前であることを示す                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| cil managed     | このメソッドの本体がCILのコードでマネージドメソッドであることを示します。managed以外を記述するのは稀です。                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
|                 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
|                 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |

| 具体的なIL命令書式(引数も含める)                                   |
| :----------------------------------------------------------------- |
| `ldc⟪❰｡｡❰.i4❱＠⟪｡❰.⟪0～8⟫❱¦.m1¦.s num｡⟫｡｡❱｡¦｡❰⟪.i8¦.r4¦.r8⟫ num❱⟫` |



Nyasakii
