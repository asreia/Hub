{-# OPTIONS_GHC -Wno-unrecognised-pragmas #-}
{-# HLINT ignore "Redundant $" #-}
module Haskelltest where
import qualified Data.Map as M --Data.Map内の定義物をM.∫LAny∫で参照
import Control.Monad -- ()はインポートしたい物がない?H本P91
import System.IO.Unsafe ( unsafePerformIO )

unsafeExe :: String
unsafeExe = unsafePerformIO getLine ++ "!!"

-- print = putStrLn $ show
-- main :: IO ()
-- main = return () -- putStrLn "abc"
-- main :: IO ()
-- main = putStrLn "Hello_World"
main :: IO()
main = do   --IOdoの中では戻り値がIOなら良い? IOはdo構文の中でしか実行できない?
    print "Hello"
    input <- getLine
    print input
    -- print $ map' (*2) [1, 2, 3, 4]
    -- print $ and' [True,True,True]
    print $ maximum' [2]
    main --ループした(再帰)

-- ghci は戻り値がIOで無い関数の場合は、自動的にその関数にprint(putStrLn $ show)を合成している?
    -- 戻り値がIOの場合はそのIOアクションを実行するだけ?
-- IO a -> 非IO a は存在しない?
printSequ :: IO [()]
printSequ = sequence . map  print $ [1,2,3,4]
printSequ1 :: IO [()]
printSequ1 = mapM print $ [1,2,3,4]
-- mapM' = sequence . map -- がなぜか定義できない

gl :: IO String
gl = getLine

map' :: (a -> b) -> [a] -> [b]
map' _ [] = []
map' f (x:xs) = f x : map' f xs

and' :: [Bool] -> Bool
and' [] = False
and' [True] = True
and' (False:xs) = False
and' (True:xs) = and' xs

maximum' :: Ord a => [a] -> a
maximum' [] = error "error"
maximum' [x] = x
maximum' (x:y:xs)
    | x < y =  maximum' (y:xs)
    | otherwise = maximum' (x:xs)

(!!^) :: [a] -> Int -> a
[] !!^ _ = error "error"
(x:xs) !!^ n
    | n == 0 = x
    | otherwise = xs !!^ (n-1)

asPattern :: [a] -> ([a],a)
asPattern [] = error "error"
asPattern all@(x:xs) = (all,x)

whereScope :: Int -> String
whereScope 2 = a
    where a = "abc"
whereScope _ = b
    where b = "def"

caseSiki :: (Eq a1, Num a2, Num a1) => a1 -> a2
caseSiki x = 123 + case x == 4 of -- 演算子が使える
    True -> 100
    False -> 1

-- f :: (Eq a) => a -> ()
t :: Maybe [a] -> ()
t (Just [a,b]) = ()
t Nothing = ()
t (Just a) = ()
-- f (Just 2) = ()
f :: Bool -> ()
f True = ()
f False = ()

f1 :: Maybe a -> ()
f1 (Just a) = ()
f1 Nothing = ()

f2 :: Maybe (Maybe a) -> ()
f2 (Just (Just a)) = ()
f2 (Just Nothing) = ()
f2 Nothing = ()

g1 a 
    | a == (1,2) = ()
    | (6, 2) == (4,2) = ()
    | True = ()
g2 |False = ()
g3 |True = ()

f3 :: Show a => a -> String
f3 a = show a
-- f3 1 = show 1
-- f3 True = show True

f4 :: (Show a) => a -> String
f4 a = show a
-- f4 1 = show 1
-- f4 True = show True

-- data Type1 = A
data Type2 = Type2 deriving(Show)
-- data Type3 = A | B | C
-- data Type4 a b = A a b
-- data Type5 = A Type5 | Nil deriving(Show)
data Type4 = A Int deriving(Show)

class Show i => Interface i where
    value :: i
    -- strval :: String
    odoroki :: i -> String
    odoroki a = show a ++ "!!"
    ifunc :: i -> i -> String

class Wrapper w where
    wrapper :: a -> w a

instance Interface Int where
    -- value :: Int
    value = 496
    ifunc a b = show a ++ "," ++ show b

va = value :: Int
od = odoroki (124 :: Int)
ic = ifunc (123 :: Int) (456 :: Int)

uncurryVal :: Int
uncurryVal = uncurrying (1,2,3) -- => 5
curryVal :: Int
curryVal = currying 1 2 3 -- => 5

func :: a -> b -> c -> ()
func a b c = () -- "()"は非関数型のnullのようなもの

f11 :: Int -> Int -> Int
f11 x y = x + y

uncurrying :: (Int, Int, Int) -> Int
uncurrying (a, b, c) = a * b + c

currying :: Int -> Int -> Int -> Int
currying a b c = a * b + c

-- 関数適用
infixr 0 #$
(#$) :: (a -> b) -> a -> b
f #$ a = f a
-- 関数合成(射の合成)
(#.) :: (b -> c) -> (a -> b) -> (a -> c)
f #. g = \x -> f (g x)
-- 射の関手 (Maybeの実装の場合)
fmap_ :: (a -> b) -> Maybe a -> Maybe b
fmap_ f Nothing = Nothing
fmap_ f (Just a) = Just (f a)
-- (リストの実装の場合)
-- fmap = map -- (map f [] = []; map f (x:xs) = f x : map f xs)
-- Applicative (Maybeの実装の場合)
pure_ :: a -> Maybe a
pure_ a = Just a
(<#*>) :: Maybe (a -> b) -> Maybe a -> Maybe b
Nothing <#*> _ = Nothing
Just f <#*> a = fmap f a
-- (リストの実装の場合)
pure__ :: a -> [a]
pure__ a = [a]
(<##*>) :: [a1 -> a2] -> [a1] -> [a2]
[] <##*> _ = []       -- fs,xsなど二重ループしたかったら二重再帰ループか?(fs<#*>とfmap xs)
(f:fs) <##*> xs = fmap f xs ++ (fs <##*> xs)
-- Monad (Maybeの実装の場合)
return_ :: a -> Maybe a
(>>#=) :: Maybe a -> (a -> Maybe b) -> Maybe b
return_ a = Just a
Nothing >>#= _ = Nothing
Just a >>#= f = f a

-- return x = [x]
-- xs >>= f = concat (map f xs)

newtype State_ s a = State_ {runState_ :: s -> (a, s)}
return__ :: a -> State_ s a
(>>##=) :: State_ s a -> (a -> State_ s a) -> State_ s a
return__ x = State_ $ \s -> (x, s)
State_ h >>##= f = State_ $ \s -> let (a, newState) = h s in runState_ (f a) newState

fff x y = x + y
ggg x y = x * y

infixl 4 #<=#
a #<=# b = a + b
infixr 4 #=>#
a #=># b = a * b

-- infixl 4 infixfunc
-- infixfunc a b = a + b

foldMap' :: Monoid m => (a -> m) -> [a] -> m
foldMap' f xs = mconcat $ map f xs

maybeId :: Maybe a -> Maybe a
maybeId a = a

fd Nothing = Nothing
fd (Just (a, b)) = Just a

applicative_style :: Maybe Integer
applicative_style = pure (+) <*> Just 3 <*> Just 5 -- 文脈付きのまま計算することができる(C#のNullable型)


-- 青線の"Eta reduce"は役に立つ
-- "func1"は、0引数を取り、値(Value)を返す関数
func1 :: Type
func1 = Value
data Type = Value deriving(Show)
-- func10 1 = show 1
ret1 = func1 -- => Value
-- "func2"は、1引数を取り、その引数を返す関数
func2 :: a -> a -- func2 :: a -> b はエラー。"a = xの型"とされるため?(a -> bは証明できないので書けない)
func2 x = x
ret2 = Value
-- "func3"は、1引数を取り値(Value)を返す
func3 :: a -> Type
func3 x = Value
ret3 = func3 () -- => Value
-- "f"が値を取り、値を返す関数
func4 :: (a -> b) -> a -> b
func4 f x = f x
ret4 = func4 show 4 -- => "4"
-- "f"が値を取り、関数を返す関数
func5 :: (a -> (b -> c)) -> a -> (b -> c)
func5 f x = f x
ret5 = func5 (*) 5
-- "f"が関数を取り、値を返す関数
func6 :: ((a -> b) -> c) -> (a -> b) -> c
func6 f g = f g
-- func6 f = f でもok。"func6 f"は"(a -> b) -> c"型を返すから(ポイントフリースタイル)
ret6 = func6 (\g -> show.g $ 2) (8/) -- => "4"
-- "f"が関数を取り、関数を返す関数
func7 :: ((a -> b) -> (c -> d)) -> (a -> b) -> (c -> d)
func7 f g = f g
ret7 = func7 (\g -> (++).show.g $ 2) (8/) $ "st" -- => "4"
-- "func8"は再帰関数
func8 :: (Eq a, Num a) => a -> a -- パターンマッチで"=="比較されるのでEq型クラス制約が必要?
func8 0 = 0 -- "0"は
func8 n = func8 (n - 1)
ret8 = func8 8 -- => 0
-- 
func9 :: (Eq a, Num a) => a -> a
func9 0 = 0 -- "0"は"Int"?型で具体的な型の値なのに多相変数をNumの型クラス制約で"a = "0"の型"ができてしまっている
func9 n = n + func9 (n - 1)
-- whereネスト
where' = where'1
    where
        where'1 = where'2 -- where'1 = where'4 はエラー
            where
                where'2 = where'3
                    where
                        where'3 = ()
                        where'4 = ()

-- 引数再帰(万能?(自身の結果が出力の型と合わないとだめ))、外側再帰(fold出来るようなモノを外側にし、それと自身の結果を合わせて出力の型と合わないとだめ)、単位元、初期値、foldl、foldr 15
-- 多相変数に何か演算しようと思うなら型制約をする
sumList :: (Eq a, Num a) => a -> (a, [a])
sumList n = sumList' (0, []) n
    where
        sumList' (e, xs) 0 = (e, xs)
        sumList' (e, xs) n = sumList' (e + n, n:xs) (n - 1)

listDo = do         -- 食わせる所までが一行。上から実行される。
    op <- [(+),(*)] -- [(+),(*)] >>= (\op ->
    x <- [2,3]      -- [2,3] >>= (\x ->
    y <- [4,5]      -- [4,5] >>= (\y ->
    [op x y]        -- [op x y])))
listDo1 = do         
    op <- [(+),(*)] 
    x <- [op 2, op 3]
    y <- [x 4,x 5]
    [y]
-- listDo1 = do     -- join . fmap f $ m で、各行から終わりがjoin . fmap f <- m の様なもの
--     op) <- [(+),(*)] 
--     x) <- [2,3]     
--     y) <- [4,5]     
--     [op x y])))  

updateAbc :: IO ()
updateAbc = do
    a <- getLine
    b <- getLine
    c <- getLine
    putStrLn $ a ++ b ++ c
    if a == "quit"
        then
        putStrLn "End"
        else
        updateAbc

x :: (Eq t, Num t) => t -> Int  --モナドの文脈があればどこでも"do"することができる
x 0 = 0
x n = x (n-1) + length
    (do
    a <- [1,2,3]
    b <- [Just 4,Just 5,
        do
        a <- Just 4
        b <- Just 2
        Nothing
        return $ a + b
        ]
    ["a"]
    )

y = do
     a <- \x -> x * 2
     b <- \x -> a + x
     \x -> a * b + x
-- ↕↕↕↕↕↕↕↕↕↕Readerモナド(関数モナド((->) r))↕↕↕↕↕↕↕↕↕↕ -- \xのxには同じ値が入るmapしてる感じ
y' = (\x -> x * 2) >>= \a -> (\x -> a + x) >>= \b -> (\x -> a * b + x)

j' = Just 1 >>= (\a -> Just 2 >>= (\b -> Just (a+b)))

newtype Arrow' r a = Arrow' {arrow' :: r -> a}

instance Functor (Arrow' r) where
    fmap f g = Arrow' $ (\x -> f (arrow' g x))

instance Applicative (Arrow' r) where
    pure a = Arrow' $ const a
    f <*> g = Arrow' $ \x -> (arrow' f x) (arrow' g x)


instance Monad (Arrow' r) where
    return a = Arrow' $ const a
    (Arrow' h) >>= f = Arrow' $ \w -> arrow' (f (h w)) w
    -- h >>= f = \w -> f (h w) w

arr = (Arrow' $ \x -> x * 2) >>= (\a -> Arrow' $ \y -> y + 1)

listMonad = [(+),(*)] >>= (\op -> [2,3] >>= (\x -> [4,5] >>= (\y -> [op x y])))
pure' = pure 1 :: Num a => IO a -- "Num a =>"が無いとエラー
-- ❰(){}[]"'_`;,❱中置記法に使えない記号
infixl 0 <=#
(<=#) a b = ()
infixr 2 **=>
(**=>) a b = ()

iOFunc1 :: Maybe (IO String)
iOFunc1 = Just (getLine >>= \x -> return x)
iOFunc :: IO [String]
iOFunc = sequence [getLine >>= \x -> return x] -- sequenceの中で実行される?

-- https://yomi322.hateblo.jp/entry/2012/10/18/204707
quotYRem :: Integral a => a -> a -> Bool
quotYRem x y = (x `quot` y) * y + (x `rem` y) == x -- => True
divYMod :: Integral a => a -> a -> Bool
divYMod x y = (x `div`  y) * y + (x `mod` y) == x -- => True
-- divの 5 `div` (-2) == -3 になるのは
    -- 5 / (-2) == -2.5 をマイナス方向に丸めると-3でdivと同じになる(modはその時の余り)
    -- 5 / (-2) == -2.5 をプラス  方向に丸めると-2でquotと同じになる(remはその時の余り)
    -- しかし、5 / 2 == (-5) / (-2) == 2.5 で商がプラスになるとdivもquotもマイナス方向に丸める
    -- 見方を変えると、divはマイナス無限方向に丸め、quotはゼロ方向に丸める(少数切り捨て)と見ることもできる

data D_ a b = A_ a b | B1 (a,b) | C_ Int (D_ a b) deriving(Show)
data Tree a = EmptyTree | Node a (Tree a) (Tree a) (Tree a) (Tree a)

-- l :: (t1 -> t2) -> t2
-- l h = h $ h

data Val = Abc | Def | Val deriving(Show) -- 型コンストラクタと値コンストラクタ つまり、型と値は、大文字から始まる(関数と型引数は小文字から)
                                            -- コンストラクタというのは、引数を受け取って型または値を作るから
type Waybe = Maybe -- たしか、型シノニム

infixr 5 :-
data List a = Empty | a :- (List a) deriving(Eq, Ord, Show, Read) -- 型引数は小文字から(大抵一文字)、List Int,List m の様に型なのか型引数か多相型なのか区別している
(+-+) :: List a -> List a -> List a
Empty +-+ ys = ys
(x :- xs) +-+ ys = x :- xs +-+ ys -- 1 :- (2 :- (..))という形になるので 1 :- (..) => (1 :- xs) という風にガッチする? 
list' :: List Integer
list' = (1 :- 2 :- 3 :- Empty) +-+ (4 :- 5 :- 6 :- 7 :- Empty) -- =>1 :- (2 :- (3 :- (4 :- (5 :- (6 :- (7 :- Empty))))))

-- レコード構文
data Animal = Ningen {namae :: String, takasa :: Float} | Cat {name :: String, atk :: Int} deriving(Show)
coco :: Animal
coco = Ningen {namae = "kaduki_coco", takasa = 150}
doruti :: Animal
doruti = Cat{name = "ogiya_nyanco", atk = 9999}
cocoTuple :: (String, Float)
cocoTuple = (namae coco, takasa coco)
dorutiTuple :: (String, Int)
dorutiTuple = (name doruti, atk doruti)

-- 型クラス============================================================================================================================================================
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
-- ====================================================================================================================================================================
