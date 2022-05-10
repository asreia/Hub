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
    newobj instance void [mscorlib]System.Text.StringBuilder::.ctor(string)
    ldstr "255={0:X}"
    ldc.i4 255
    box valuetype [mscorlib]System.Int32
    call instance class [mscorlib]System.Text.StringBuilder 
        [mscorlib]System.Text.StringBuilder::AppendFormat(string, object)
    call instance string [mscorlib]System.Text.StringBuilder::ToString()
    call void [mscorlib]System.Console::WriteLine(string)
    ret
}
```
ILはスタックマシン、逆ポーランド記法 {3 4 + 1 2 - *} <=> {(3 + 4) * (1 - 2)}(数字(データ)をプッシュして演算子(関数)でポップして処理する)  

## .netの実行まで  

```
  |IL| ---ilasm.exe--> |   .exe    |  
  |  | <--ildasm.exe-- |   .dll    | ---CLR(AOT or JIT)--> ネイティブ  
  C#  ---csc.exe---->  |IL(バイナリ)|  
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
- {windowsローダー} => {win32(PEヘッダ)} => {.net(IL(バイナリ))(CLR)}  

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
  - 全ての命令は**スタック、ヒープ、演算器、プログラムカウンタ**への命令  
  - 命令は殆ど1バイト + 定数のバイト数 = １行のバイト数  
    - IL命令が32バイト以下かつ反復と例外を含んでいないあと仮想呼び出し、デリゲートでない時、JITコンパイラはその関数は**インライン化**する(関数を展開して埋め込まれる)  

## ILの命令

v := value, a:= arg, TypeTokenは型
`stloc = ldloc` (ldをstに代入(代入の間には**スタックを経由**している))  
**ldがプッシュ、stがポップ**
|スタック|引数、変数、localloc|ヒープ|
| アセンブリ                          | スタック遷移                    | 説明                                                                                                                                                        |
| :---------------------------------- | :------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **ロード、ストア系 ←ld st→**        |                                 |                                                                                                                                                             |
| スタック ← 定数                     |                                 |                                                                                                                                                             |
| @`ldnull`                           |                                 | スタック上のNULL参照をプッシュ                                                                                                                              |
| @`ldstr 文字列`                     |                                 | は、リテラル文字列の文字列オブジェクトをプッシュします                                                                                                      |
| @`ldc.i4 int32の数`                 |                                 | int32型のnumをint32としてスタックにプッシュします。                                                                                                         |
| `ldc`                               |                                 |                                                                                                                                                             |
| `ldc.i4.08.m1.s.i8.r4.r8`           |                                 |                                                                                                                                                             |
| スタック ⇔ 引数                     |                                 |                                                                                                                                                             |
| @`ldarg.argNum`                     |                                 | num番の引数をスタックにロードします。                                                                                                                       |
| `ldarga.argNum`                     | … => …, addr                    | argNumは引数のインデックスでスタックにそのインデックスのアドレスを積む。                                                                                    |
| @`starg.argNum`                     |                                 | numという番号の引数に値を保存します                                                                                                                         |
| スタック ⇔ ローカル変数             |                                 |                                                                                                                                                             |
| @`ldloc.locNum`                     |                                 | locNumのローカル変数をスタックにロードします。                                                                                                              |
| `ldloca.locNum`                     | … => …, addr                    | スタックにそのlocNumのローカル変数のアドレスを積む。                                                                                                        |
| @`stloc.locNum`                     | …, val => …                     | スタックに値(val)があり、そのvalを❰番号❱のローカル変数を宣言した型に変換しそのローカル変数に代入する。                                                      |
| スタック ⇔ 参照先                   |                                 |                                                                                                                                                             |
| `ldobj TypeToken`                   | …, addr => …, val               | スタックにaddr:= ❰@❰アン❱マネージポインター(❰native int¦&❱)❱があり、そのアドレスを参照しそれを❰TypeToken型❱へ変換してスタックに積む。                       |
| `stobj TypeToken`                   | …, addr, val => …               | スタックにaddr:= ❰@❰アン❱マネージポインター(❰native int¦&❱)❱と値(val)があり、値(val)を❰TypeToken型❱に変換してそのアドレスに保存する。                       |
| `ldind.i4`                          | …, addr => …, val               | val = *ptr。ldobjの短縮形。スタックに❰@❰アン❱マネージポインター(❰native int¦&❱)❱があり、そのアドレスを参照しそれを❰指定の型❱へ変換してスタックに積む。      |
| `stind.i4`                          | …, addr, val => …               | *ptr = val。stobjの短縮形。スタックに❰@❰アン❱マネージポインター(❰native int¦&❱)❱と値があり、値を❰指定の型❱に変換してそのアドレスに保存する。                |
| スタック ⇔ 配列                     |                                 |                                                                                                                                                             |
| @`ldlen`                            |                                 | スタック上の配列の長さ（ネイティブのunsigned int型）をプッシュします                                                                                        |
| @`ldelem`                           |                                 | インデックスの要素をスタックの最上部にロードします                                                                                                          |
| `ldelema`                           | …, array, index => …, addr      | &(array[index]) スタックに配列とインデックスがあり、その配列のインデックス番目のアドレスを積む。                                                            |
| @`stelem`                           |                                 | インデックスの配列要素をスタックの値に置き換えます                                                                                                          |
| スタック ⇔ フィールド               |                                 |                                                                                                                                                             |
| `ldfld field`                       | …, obj => …, val                | val = obj.field (*(&(*obj) + field)?) obj:= ❰O型¦native int¦&❱。スタックにobjがあり、そのフィールドのメンバ変数をスタックに積む。                           |
| `ldflda field`                      | …, obj => …, addr               | val = &(obj.field) (&(*obj) + field?) obj:= ❰O型¦native int¦&❱。スタックにobjがあり、そのフィールドのメンバ変数のアドレスをスタックに積む。                 |
| `stfld field`                       | …, obj, val => …                | obj.field = val (*(&(*obj) + field) = val) スタックにobjと値(val)があり、objのフィールドのメンバ変数に値を代入する。                                        |
| スタック ⇔ フィールド(静的)         |                                 |                                                                                                                                                             |
| `ldsfld field`                      | …, => …, val                    | val = cls.field (*(cls.field(==field?))?) フィールドの静的メンバ変数をスタックに積む。                                                                      |
| `ldsflda field`                     | …, => …, addr                   | val = &(cls.field) (cls.field(==field?)?) フィールドの静的メンバ変数のアドレスをスタックに積む。                                                            |
| `stsfld field`                      | …, val => …                     | cls.field = val (cls.field(==field?) = val) スタックに値(val)があり、フィールドの静的メンバ変数に値を代入する。                                             |
| スタック ← メソッド(アドレス)       |                                 |                                                                                                                                                             |
| `ldftn method`                      | … => …, ftn                     | methodからそのメソッドへのポインタ(ftn(アンマネージポインタ))をスタックに積む。ftnはcalli命令で呼び出せる。                                                 |
| `ldvirtftn method`                  | …, obj => …, ftn                | スタックにインスタンスオブジェクト(obj)があり、そのobjのmethod(仮想メソッド)へのポインタ(ftn)をスタックに積む。ftnはcalli命令で呼び出せる。                 |
| スタック ← トークン(ランタイム表現) |                                 |                                                                                                                                                             |
| `ldtoken token`                     | … => …, RuntimeHandle           | メタデータトークン(識別子とかキーワードとか？)をランタイム表現(RuntimeHandle)に変換してスタックに積む。(良くわからない)                                     |
| **分岐系**                          |                                 |                                                                                                                                                             |
| @`br`                               |                                 | ターゲットへの分岐                                                                                                                                          |
| @`beq`                              |                                 | 等しい場合はターゲットに分岐します                                                                                                                          |
| @`brtrue`                           |                                 | 値がゼロ以外（true）の場合、ターゲットに分岐します                                                                                                          |
| @`switch`                           |                                 | n個の値のいずれかにジャンプします                                                                                                                           |
| **コール系**                        |                                 |                                                                                                                                                             |
| @`call`                             | …, arg0…argN => …, (rV)         | メソッドを呼び出す                                                                                                                                          |
| `callvirt method`                   | …, obj, arg0…argN => …, (rV)    | class A{virtual T f(){}} class B{override T f(){}}。ポリモーフィズム。vtableでobjに関連付けられたメソッドを呼び出す。                                       |
| `calli callSiteDescr`               | …, arg0…argN, ftn => …, (rV)    | リフレクション。スタックに引数とメソッドへのポインタ(ftn)があり、そのメソッドにその引数を使って呼び出す。callSiteDescrは引数の説明?(多分、長さとか型とか)。 |
| `jmp method`                        | … => …                          | 現在のメソッドを終了し、指定したメソッドにジャンプします。現在の引数を宛先の引数に転送(arg?)                                                                |
| @`ret`                              | (rV(先))=> …, (rV(元))          | 呼び出し先の評価スタックの戻り値を呼び出し先の評価スタックの戻り値に積む。型も呼び出し元の型として管理される？                                              |
| **演算子系**                        |                                 |                                                                                                                                                             |
| 四則演算                            |                                 |                                                                                                                                                             |
| @`add`                              |                                 | 2つの値を追加して、新しい値を返します                                                                                                                       |
| @`sub`                              |                                 | value1からvalue2を減算し、新しい値を返します                                                                                                                |
| @`mul`                              |                                 | 乗算値                                                                                                                                                      |
| @`div`                              |                                 | 2つの値を除算して、商または浮動小数点の結果を返します                                                                                                       |
| @`rem`                              |                                 | value1をvalue2で割った余り                                                                                                                                  |
| @`neg`                              |                                 | 否定値(多分、負の値)                                                                                                                                        |
| ビット演算                          |                                 |                                                                                                                                                             |
| @`and`                              |                                 | 2つの積分値のビット単位のAND。積分値を返します                                                                                                              |
| @`or`                               |                                 | 2つの整数値のビット単位のOR。整数を返します。                                                                                                               |
| @`xor`                              |                                 | 整数値のビット単位のXOR。整数を返します                                                                                                                     |
| @`not`                              |                                 | ビット単位の補数                                                                                                                                            |
| シフト演算                          |                                 |                                                                                                                                                             |
| @`shl`                              |                                 | 整数を左にシフト（ゼロにシフト）し、整数を返します                                                                                                          |
| @`shr`                              |                                 | 整数を右にシフト（符号をシフト）、整数を返します                                                                                                            |
| 比較演算                            |                                 |                                                                                                                                                             |
| @`ceq`                              |                                 | value1がvalue2に等しい場合は1（int32型）をプッシュし、そうでない場合は0                                                                                     |
| 型変換                              |                                 |                                                                                                                                                             |
| @`conv.i4`                          |                                 | int32に変換し、スタックにint32をプッシュします                                                                                                              |
| **ボックス系**                      |                                 |                                                                                                                                                             |
| @`box ＄vTT=❰valueTypeToken❱`       | …, val(∫vTT∫) => …, obj         | 値型(val)をpopし、それをbox化したO型をpushする。 box化: {スタック[val]} => {{スタック[O型]} -> {ヒープ[val]}}                                               |
| @`unbox valueTypeToken`             | …, obj => …, val(∫vTT∫)         | ↑の逆操作。O型(obj)をpopし、それをunbox化した値型をpushする。(本当の内部の挙動は違うかもしれない?)                                                          |
| `unbox.any ＄TT=❰TypeToken❱`        | …, obj => …, ❰val¦obj❱(∫TT∫)    | 多分、objが、box化された値型なら`unbox`と同じ。そうでないなら`castclass`と同じ。                                                                            |
| **オブジェクト操作系**              |                                 |                                                                                                                                                             |
| `castclass TypeToken`               | …, obj => …, obj2(∫TT∫)         | (T)obj。 スタックにobj(O型)があり、❰TypeToken型❱にダウンキャスト(アップキャストは使われない)してまた積む(obj2)。O型はアドレスと動的な?型も持っている？      |
| `isinst TypeToken`                  | …, obj => …, ❰res(∫TT∫)¦null❱   | TypeToken res = obj as TypeToken。 は、❰obj❱が❰TypeToken❱のインスタンスかテストし、nullかそのインスタンスを返す。                                           |
| **生成系**                          |                                 |                                                                                                                                                             |
| @`newarr`                           |                                 | etype型の要素を持つ新しい配列を作成します                                                                                                                   |
| @`newobj ctor`                      | …, arg0…argN => …, obj          | スタックに引数があり、その引数を使ってctor(コンストラクタ)を呼び出し初期化されてない生成されたオブジェクト(O型)を初期化します。                             |
| `localloc`                          | size => addr                    | スタックにsizeがあり、sizeバイト分の領域をローカルメモリプール?(ローカルヒープ?)から確保しその先頭アドレスをスタックに積む。                                |
| **初期化系**                        |                                 |                                                                                                                                                             |
| `initobj TypeToken`                 | …, addr => …                    | スタックにアドレスがあり、TypeTokenが値型の場合そのアドレスをその値型で初期化する。TypeTokenが参照型の場合はnullが入る。                                    |
| `initblk`                           | …, addr, val, size => …         | スタックにアドレス、値(unsigned int8)、バイト数があり、そのアドレスにその値でバイト数分初期化する。(値をレプリケートする？)                                 |
| **コピー系**                        |                                 |                                                                                                                                                             |
| `cpblk`                             | …, destaddr, srcaddr, size => … | スタックに送信元、送信先、バイト数(blk)があり、送信元から送信先へバイト数分データをコピーする。                                                             |
| `cpobj TypeToken`                   | …, dest, src => …,              | dest = (TypeToken)src。srcValObjからdestValObjに値をコピーします。                                                                                          |
| **例外系**                          |                                 |                                                                                                                                                             |
| @`throw`                            |                                 | 例外をスローします           .try{.try{..}catch{..}}finally{..}                                                                                             |
| `rethrow`                           | … => …                          | catch句でキャッチした例外を再スローする。(catch句内のみ使用可能)                                                                                            |
| `endfilter`                         | …, val => …                     | 例外処理のfilterが何か知らないけどendfinallyと同じ様にfilter句の最後で値(val)を取って使ってfilter句の処理を終わる。                                         |
| `end❰finally¦fault❱`                | … => …                          | 例外ブロックの❰finally¦fault❱節を終了                                                                                                                       |
| @`leave`                            |                                 | コードの保護された領域を終了します。                                                                                                                        |
| `ckfinite`                          | …, val => …, val                | if(val == NaN){throw("ArithmeticException");}。スタックにF型?があり、値が有限でない場合(無限?)例外を投げる。値がF型でない場合何もしない?                    |
| **スタック操作系**                  |                                 |                                                                                                                                                             |
| `dup`                               | …, val => …, val, val           | スタックの一番上の値を複製します。                                                                                                                          |
| `pop`                               | …, val => …                     | pop命令は、スタックから最上位の要素を削除します。                                                                                                           |
| **その他**                          |                                 |                                                                                                                                                             |
| `sizeof TypeToken`                  | … => …, size                    | TypeTokenが値型の場合スタックにその値型のサイズ(バイト数)を積む。参照型の場合は参照先のサイズではなく参照のアドレスサイズになる？                           |
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
| hidebysig       | 名前と署名(シグネチャ?)で非表示にします。 ランタイムでは無視されます。(基底クラスのメソッドと同じシグネチャを持つ時、非表示になる?(newがいる))                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| specialname     | 主に.ctor()に付いていてCLR以外に特別な名前であることを示す                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| rtspecialname   | 主に.ctor()に付いていてCLRに特別な名前であることを示す                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| cil managed     | このメソッドの本体がCILのコードでマネージドメソッドであることを示します。managed以外を記述するのは稀です。                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
|                 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
|                 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |

| 具体的なIL命令書式(引数も含める)                                   |
| :----------------------------------------------------------------- |
| `ldc⟪❰｡｡❰.i4❱＠⟪｡❰.⟪0～8⟫❱¦.m1¦.s num｡⟫｡｡❱｡¦｡❰⟪.i8¦.r4¦.r8⟫ num❱⟫` |



Nyasakii
