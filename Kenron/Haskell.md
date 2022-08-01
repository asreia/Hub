# Haskellまとめ

## サイト

## 構文

### 関数

- 関数とは、式に現れる変数に値を与える事によって式が具体的な値を表す仕組みです
**数学の関数**は`f(x) = x + 2`とか、`f(x,y) = x + y`とかで、
部分的にはHaskellも同じ仕組みをしています。
数学の分野の**ラムダ計算**は、カリー化もありこれも部分的にはHaskellも多分同じ仕組みをしています(計算行為上例外的な状況は多少ある..(ボトム(⊥)とか?))
詳しくは知らないが、Haskellは関数名もパターンマッチもカリー化もできるので、数学の関数とラムダ計算を足した感じ?

- 数学の関数とHaskellの関数とラムダ計算の比較
  - 数学の関数
  `f(x,y) = x + y`
  - Haskellの関数
  `f x y = x + y`
  - ラムダ計算
  `λxy.x + y`
  大体同じ

- Haskellの関数
```haskell
f :: Int -> Int -> Int -- 関数の型
f x y = x + y          -- 関数の定義
```
↓大体こんな感じです↓
｢関数名｣ :: ｢引数1の型｣ -> ｢引数2の型｣ -> ｢式の型｣
｢関数名｣ ｢引数1｣ ｢引数2｣ = ｢式｣

#### 式

- 式は関数の本体であり関数の全ての処理がこの**一つの式に記述**されます。
`｢関数名｣ ｢引数..｣ = ｢式｣`
- C#や数学の数式と同じく式は、**必ず一つの値または関数を返します**。
そこにいくつかの変数を引数として与えると、その与えられた引数に対応する値または関数を返します。
  - なので、式は、**同じ引数に対しては必ず同じ値または関数を返します**。これが**参照透過性**といわれています。

##### カリー化

- カリー化された関数とは、関数が必要とする**引数の数より少ない数の引数を渡す**と、残りの引数を必要とする**関数を返す関数**のことである(関数型ではこれが一般的)
  - 反対に関数が必要とする引数を同時に全て渡すのがアンカリー化された関数である(非関数型ではこれが一般的)
  - 圏論では`Uncurry : X × A -> B, Curry : X -> B^A (Xを与えるとHom(A,B)から射を一つ取り出す)`である

- C#版とHaskell版のカリー化を比べてみる
  - C#版
    ```csharp
    using System;
    public class M
    {
        public static void Main()
        {
            Console.WriteLine(Uncurrying(1,2,3));//=>5
            Console.WriteLine(Currying(1)(2)(3));//=>5
        }
        //アンカリー化        (Int,   Int,   Int)  ->  Int
        static int Uncurrying(int a, int b, int c) => a * b + c;
        //カリー化   Int -> (Int -> Int)           Int  ->   Int-> Int-> Int
        static Func<int, Func<int, int>> Currying(int a) => (b => (c => a * b + c));
    }
    ```
  - Haskell版
    ```haskell
    uncurryVal :: Int
    uncurryVal = uncurrying (1,2,3) -- => 5
    curryVal :: Int
    curryVal = currying 1 2 3 -- => 5

    uncurrying :: (Int, Int, Int) -> Int
    uncurrying (a, b, c) = a * b + c

    currying :: Int -> Int -> Int -> Int
    currying a b c = a * b + c
    ```
- ここで、引数を0個取る関数を作ってみます
  - C#版`int Five() => 5;`
  - Haskell版`five = 5`
  - Haskellでは、これ以外に`five`に`5`を関連付ける方法は無く、`=`は代入記号(コピー)ではなく
  数学の`=`と同じ意味の`five`は`5`であるという意味になります
  なので、`five`は引数を0個取る関数とも見えるし、`five`は`5`という値とも見え、
  Haskellでは**関数と値を区別しません**。なのでこれから**関数と言うとき引数が0個の関数も含める**場合があります

- Haskellの関数の式以外の記法
  - とりあえず、適当な関数を定義します
    - ```haskell
        func :: a -> b -> c -> ()
        func a b c  = () -- "()"は非関数型のnullのようなもの
        ```
    - そして`func`に一つづつ引数を与えて型を調べて行きます
    1. `:t func` => `func :: a -> b -> c -> ()`
    2. `:t func 1` => `func 1 :: b -> c -> ()`
    3. `:t func 1 2` => `func 1 2 :: c -> ()`
    4. `:t func 1 2 3` => `func 1 2 3 :: ()`
    つまり、関数に引数を与えるごとに以下のような関係があり、
    `((((func) 1) 2) 3)` <=> `(a -> (b -> (c -> (()))))`
    `a`を渡すと`b -> c -> ()`の関数を返す。`a b`を渡すと`c -> ()`の関数を返す
    よって、**どのくらい引数を与えるか**で**返される関数が決まります**
    なので非関数型と違って、引数を定義する順序は重要です
    それによって、他の関数の引数にできるかできないかが決まります(**合成可能性**)(まぁflip使えばできるけど)

##### 式の実態(ここでは引数0個の関数を値と呼んでいる)

- 式がどのように構成されて行くかを分解して観察してみる。以下の1.~10.は関数の`=`記号の右側 **(式)を表している**
小文字一文字のアルファベットまたは`(*)`は、0個以上の引数を取る**値または関数**で`❰f:2❱`という表記は`f`が2個の引数を取る関数とする。(`f :: a -> b -> c`)

1. 式は`a`という値を返す
    `a ❰a:0❱`
2. `f`が引数に`a`を取り値を返す
    `f a ❰f:1,a:0❱`
3. `f`が引数に`a`と`b`を取り値を返す
    `f a b ❰f:2,a:0,b:0❱`
4. `f`が引数に`a`を取り、もう一つ引数を取る関数を返す(カリー化の部分適用により関数が関数を返すこともある)
    `f a ❰f:2,a:0❱`
5. `(*)`が引数に`a`と`b`を取り値を返す
    `a * b ❰(*):2,a:0,b:0❱` => `(*) a b`
6. `(*)`が引数に`a`と`b`と`c`を取り値を返す(中置記法が2引数しか取らないとは限らない)
    `(a * b) c ❰(*):3,a:0,b:0,c:0❱` => `(*) a b c`
7. `g`が引数に`b`を取り値を返し、`f`が引数に`a`と`(g b)の返された値`を引数に取り値を返す
    `f a (g b) ❰f:2,g:1,a:0,b:0❱`
8. `g`が引数に`a`を取り、もう一つ引数を取る関数を返し、`f`が引数に`(g b)の返された関数`を引数に取り値を返す
    `f (g a) ❰f:1,g:2,a:0❱`
9. 内部の`(an1 an2..ann)`が値または関数を返し外部の`(b11..b1n)`が内部の`(an1 an2..ann)`を引数に取り値または関数をまた外部に返す
    `(..(b11..(a11 a12..a1n)..(a21 a22..a2n)..b1n..)..) ❰ann:0個から任意個の引数を取る❱`
10. `9.`をランダムに再帰的に拡張すると以下の様になり式というのは本当に**数学の数式と構造が同じ**である(ラムダ式、if,case式は関数の`=`の左側に追いやることができる。where,letは展開できる)
    `..(..(..)..(..(..)..(..)..)..(..(..)..(..)..(..(..)..(..)..(..)..)..(..)(..)..)..).. ❰..:関数または値、の列(0個から任意個の引数を取る関数の列)❱`
    つまり、式は、**関数と括弧**で構成するように書き直すことができる。
    当然そこには、他の非関数型言語で行う状態を保存したり取り出したりする**代入記号`=`は現れない(純粋関数型は状態を一切持たない(デフォルトスレッドセーフ?))**
    したがって、関数型の`=`は**何であるか**であり、非関数型の`=`は**何をするか**、である

- 合成a->b、適用a、

#### ラムダ式

#### マッチ

- マッチは、値を変数に束縛する時、`=`の左側の構文によって値をどの様に束縛するかを決めます

1. **値**を束縛
`a = Just 1` => `a = Just 1`
2. **値引数**を束縛
`Just a = Just 1` => `a = 1`
3. **値引数の値引数**を束縛
`Just (Just a) = Just (Just 1)` => `a = 1`
`[a,b,c] = [1,2,3]` => `a = 1, b = 2, c = 3`
4. **複数の値引数**を束縛
`(a,b,c) = (1,2,3)` => `a = 1, b = 2, c = 3`
5. **ワイルドカード`_`**への束縛
`_ = 1` => なし
`(a,_,c) = (1,2,3)` => `a = 1, c = 3`
6. asパターン`@`への束縛
`all@(a,b,c) = (1,2,3)` => `all = (1,2,3)`

#### パターンマッチ

- パターンマッチは、関数に渡さっる値により評価する式を変えることによって**処理を分岐**します。switch文に近い構文です
  - パターンマッチは**上からマッチ**するか判定し、`マッチされる値(変数付き) == 渡される値`が`True`になればそれに**対応する式が評価**されます。
- この**パターンマッチとガード**でHaskellは全ての分岐を表現できます

1. **具体型**によるパターンマッチ
    ```haskell
    f1 :: Bool -> ()
    f1 True = ()
    f1 False = ()
    ```
2. **文脈**によるパターンマッチ
    ```haskell
    f2 :: Maybe a -> ()
    f2 (Just a) = ()
    f2 Nothing = ()
    ```
3. **文脈の文脈**によるパターンマッチ
    ```haskell
    f3 :: Maybe (Maybe a) -> ()
    f3 (Just (Just a)) = ()
    f3 (Just Nothing) = ()
    f3 Nothing = ()
    ```
4. **型クラス**によるパターンマッチはできない
    ```haskell
    f4 :: Show a => a -> String
    f4 a = show a
    -- f4 1 = show 1    -- この2つを同時に定義することはできない
    -- f4 True = show True  -- 同じShowクラスのインスタンスであっても、型が違うので"a"がどっちの型で定義すればいいか分からない?
                            -- これをしたい場合、"f4"を"Show"型クラスのメソッドとして追加し、をそれぞれの型(IntとBool)でオーバーライドする必要がある
    ```

#### [ガード](http://walk.northcol.org/haskell/expressions/#_%E3%82%AC%E3%83%BC%E3%83%89case_%E5%BC%8F)

- ガードはパターンマッチに成功した時に**束縛された変数**をBoolを返す関数に渡し、**上からTrue**になった行に**対応する式を評価**します
  - パターンマッチと比較すると、変数に**束縛しない**、`==`だけでなく**任意のBoolを返す関数**を使える
  - `g |True = ()` <=> `g | otherwise = ()` => `otherwise == True -- => True`

#### 主要な関数

```haskell
-- 関数適用
infixr 0 $
($) :: (a -> b) -> a -> b
f $ a = f a
-- 関数合成(射の合成)
(.) :: (b -> c) -> (a -> b) -> (a -> c)
f . g = \x -> f (g x)
-- 射の関手
fmap :: (a -> b) -> f a -> f b
-- (Maybeの実装の場合)
fmap f Nothing = Nothing
fmap f (Just) a = Just (f a)
-- (リストの実装の場合)
fmap = map -- (map f [] = []; map f (x:xs) = f x : map f xs)
-- Applicative
pure :: a -> f a
(<*>) :: f (a -> b) -> f a -> f b
-- (Maybeの実装の場合)
pure a = Just a
Nothing <*> _ = Nothing
Just f <*> a = fmap f a
-- (リストの実装の場合)
pure a = [a]
[] <*> _ = []       -- fs,xsなど二重ループしたかったら二重再帰ループか?(fs<*>とfmap xs)
(f:fs) <*> xs = fmap f xs ++ (fs <*> xs)
-- Monad
return :: a -> m a
(>>=) :: m a -> (a -> m b) -> m b
-- (Maybeの実装の場合)
return a = Just a
Nothing >>= _ = Nothing
Just a >>= f = f a
-- (Stateの実装の場合)
newtype State s a = State {runState :: s -> (a, s)}
return a = State $ \s -> (a, s)
State s >>= f = State $ \a -> let (v, ns) = s a in runState (f v) ns

```

### 代数的データ型

- データ型は、Haskellの全ての型とその値を定義します。(値そのものを定義できます)
- 型ですがC#のような関数もフィールドも持ちません。**定義しているのは値**です。int型やEnumを定義しているような感じです
  - C#で直和型は作れませんが共用体(Union型)で作れそう(先頭アドレスにEnumを仕込み今なんの値か判定する)
- 値コンストラクタはC#と比較して`new Struct(1,2,3) == Struct 1 2 3`のようなものですが、`Struct`は型ではなく値を作る関数です
- 型は圏論でいう対象にあたります
  - 型コンストラクタは対象の関手にあたります(射の関手はfmap)

1. 最もシンプルなデータ型
    - 型名と値名は大文字から始まります
    `data Type1 = A`
    - 値コンストラクタ名が型名と同じでもいい
    `data Type2 = Type2`
2. 値コンストラクタが複数のデータ型
   - 直和型によって、C#のEnumのように値を判別することができます
   `data Type3 = A | B | C`
3. 値コンストラクタが値引数を取るデータ型
    - `data Type4 = A Int`
4. 型コンストラクタが型引数を取るデータ型
    - 型引数はC#のジェネリックの型引数のようなものです
    型引数は小文字から始まります
    `data Type5 a b = A a b`
    再帰的なデータ型
    `data Type6 = A Type5 | Nil`

#### 主要なデータ型

### [型クラス](http://walk.northcol.org/haskell/type-classes/)

- 型クラスとは、ある型の集合が利用する同じ型の構造を持つ関数(または定数)を定義します
- C#のインターフェースとその実装に似ています
```haskell
-- "Interface"がこの型クラスの名前になり、"i"にある型の集合が入ります
-- "Show i =>" は関数の型制約と同じく、"i"が"Show"型クラスのinstanceであることを要求します(C#の型制約(where)に似ています)
    -- これにより、"i"は"Show"型クラスが定義する関数を利用できます。
    -- この関係を継承関係と見ることもできるし、"Interface" ⊂ "Show"("Interface"は"Show"の部分集合である)と見ることもできます
class Show i => Interface i where
    value :: i
    -- strval :: String  -- "i"に関係のない関数は書けません(型に"i"が必ず現れる必要がある(多分))
    odoroki :: i -> String
    odoroki a = show a ++ "!!" --デフォルト定義。C#のデフォルト定義と同じ
    ifunc :: i -> i -> String

class Wrapper w where
    wrapper :: a -> w a  -- 型クラス変数"w"は、具体型でなく文脈(抽象型)としても定義できます。(w = Maybe a ではなく w = Maybe のような感じです)

-- Interface i => Interface Int に変え"i"に具体的な型を与えます(i = Int)
-- Int ∈ Interface と見ることもできます(値(1,2,3..) ∈ Int ∈ Interface ⊂ Show みたいな(Haskellの型のベン図が書けそう))
-- classで定義した関数をオーバーライドします
instance Interface Int where
    -- value :: Int     -- "i"に"Int"を適用しますので、classで定義した"i"が"Int"に置き換わった型になりますが、明示するとエラーになります
    value = 496
    -- ifunc :: Int -> Int -> String
    ifunc a b = show a ++ "," ++ show b

-- 使用
va = value :: Int -- => 496
od = odoroki (124 :: Int) -- => "124!!"
ic = ifunc (123 :: Int) (456 :: Int) -- => "123,456"

-- C#との比較: https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUHvDAAgCUiQiBhAewBtqwBvAXyOIHFyq6GW30iAIS416TVgHoJcAIbAZ3MUQC8pIsiKcNgvET36Dho8ZOnTUgBSUAxIGsGQPYMgF7dAMSqBo9UAyDIEJHQCIMgOwZA5gyAuwyAVwyARwyAPwyAqwyAxQyAXQyABwz+gBYRgH/OAJR4AJZQwACmYABmMgDGuUQAogCOAPoAPDIAfERSxFUA5EQKAO4AFvm5eIy6+gBG1HREuVMWCgAeMAoAnmkqTQCEubS5FvPLANxSsyrKqosqRFDUwEQAJERHyhKnw3pjE5vbcwtEy6tEG9NdmkDhIHk8fudLtc7g8ThCpIAAc0A8DqAGO1ABaKgCztQCV/nhmIQBKIGFwqnVCWAmlJsgBneRQUoVSodMlEXr9QYvNgAZiIb1ok2mzPmiiJv2UDQ5BlmADp2LlgAAVRYAB22KzhwGVuWoBQsJBWADJ9T8ZXLFSqLGrVBqVdrdWkAPxEYCQMogKRkE6qD1EeUuiX6aWyhWai3HJ2a23sA1GxYm4Pmy3hm06qOO50QV1STiezTnX0Z/16QOmkOJ61anWCaPGoNm1Vh8u2qtpl3kKTCHMdn1+3ASABUZhMgETCAyARYZAP0M4UAzwyAPYZAMMMgEGGQcj0x9iRFWhU3Ig6rHVS71QAMRkm4GuDx+FwxAAskNcAZUJgAGxsAAsRGvMmyFo5d5MADcZDAIhij0VQoFyLphTAC09kLIhAOA4DwMg0hYPgxCiAAc0hVD2HQ+9jEwkZcKgwQCJMeDHwATgsAAiXl+U+e5vmWOjgSozBaIsCwSVqMkGjSYo0ilKZtjAGAwDSYEpDFfMymaCRigABnOD04TIABaTTVnkzjuN4mp+J4ckhJEsSLAkrDpJBMVj1PRTikwNS91zbTVnsrd9J4wzSRMwThNE6YsJgEYbNkhpPLKKRigEVRszhYR3Lsk8vMIowaPoxiPhmFiljSdi4PSwxMp4viBLM0StksyTwqeSLUuipTuS9FRwS0nSUtPbyyqMirApyqy6rk1tHPfVrHniohkoaPTiofLifPK/zKpykKwpk+r5L0GKAFZzmzcEks62aewMC8LyAA===
data Color = R | G | B_

-- class    Eq' a     where
--              ↓  C#では、class Color : Eq'<Color> のようなinterface実装
-- instance Eq' Color where

class Eq' a where
    -- fail :: Int -> Int -- 引数に"a"を含めなければ実際に使われる時、型推論できない
    (===) :: a -> a -> Bool
    (=/=) :: a -> a -> Bool
    x === y = not $ x =/= y -- C#interfaceのデフォルト実装のような(Haskellでもデフォルト実装とよぶ)
    x =/= y = not $ x === y

-- fmapはC#でList<int>を引数にList<string>を返すような => interface Functor<f> {f<b> fmap<a,b>(Func<a,b> Arrow_ab, f<a> f_a);}
                                                                    -- //CS0307: 型パラメーター「f」は型引数と一緒に使用できません //文脈には関数を定義できない
-- instance Functor Maybe where -- :k Maybe => Maybe :: * -> *, :k Functor => (* -> *) -> Constraint
    -- fmap :: (a -> b) -> f a -> f b -- f a, f b のように、aを型引数に取っていてMaybeは型引数を一つ取るのでカインド(種類)が合う
-- instance (Eq m) => Eq (Maybe m) where --Maybe Int とか具体型を書かなくても、型引数?多相型?を使える。(Eq m) => はC#では、where m : Eqでジェネリックの型制約の様な
instance Eq' Color where
    R === R = True      -- "==="は上のclassの"==="をオーバーライドして実装している
    G === G = True
    B_ === B_ = True
    _ === _ = False

c0 :: Bool
c0 = R === R -- =>True
c1 :: Bool
c1 = R === G -- =>False
c2 :: Bool
c2 = G === B_ -- =>False

c3 :: Bool
c3 = R =/= R -- =>False
c4 :: Bool
c4 = R =/= G -- =>True
c5 :: Bool
c5 = G =/= B_ -- =>True
```

#### 主要な型クラス

## その他考察

## 圏論とHaskell

- デカルト閉圏、関数合成(.)が合成 関数が射、型が対象、対象の関手が型コンストラクタ、射の関手がfmap
- 射(関数)のドメインとコドメインを合成で操作して合成可能なように射を作るイメージ?

## コンパイラ

### GHC

### GHCi


- ====== =<<,fmap,<*>の型を並べる(.),タプル,リスト,$,foldmap,GHCi,GHC,ラムダ式,パターンマッチ,オーバーロード不可?,型引数,値引数,関数引数,型と値は大文字,
- 型コンストラクタはオブジェクトの関手,圏論でfmapやjoinなどで射を変換するプログラム運算,error関数
- カリー化による高階関数(何を与えると何を返すのか a -> (b -> (c -> d))),直和型(Union型,intもUnionと見ることもできる),非関数型は状態を持つ
- ノードベースプログラミングは関数型
