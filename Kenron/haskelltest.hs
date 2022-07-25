import qualified Data.Map as M --Data.Map内の定義物をM.∫LAny∫で参照

-- print = putStrLn $ show
-- main :: IO ()
-- main = return () -- putStrLn "abc"
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

applicative_style :: Maybe Integer
applicative_style = pure (+) <*> Just 3 <*> Just 5 -- 文脈付きのまま計算することができる(C#のNullable型)

data D a b = A a b | B1 (a,b) | C Int (D a b) deriving(Show)

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
data Color = R | G | B

-- class    Eq' a     where
--              ↓  C#では、class Color : Eq'<Color> のようなinterface実装
-- instance Eq' Color where

class Eq' a where
    -- fail :: Int -> Int -- 引数に"a"を含めなければ実際に使われる時、型推論できない
    (===) :: a -> a -> Bool
    (=/=) :: a -> a -> Bool
    x === y = not $ x =/= y -- C#interfaceのデフォルト実装のような(Haskellでもデフォルト実装とよぶ)
    x =/= y = not $ x === y

-- fmapは、(1 a -> 1 b) -> (f a -> f b)と考えると自然変換でもある?(型と関数からなる圏の射の関手でもある) 
-- fmapはC#でList<int>を引数にList<string>を返すような => interface Functor<f> {f<b> fmap<a,b>(Func<a,b> Arrow_ab, f<a> f_a);}
                                                                    -- //CS0307: 型パラメーター「F」は型引数と一緒に使用できません //文脈には関数を定義できない
-- instance Functor Maybe where -- :k Maybe => Maybe :: * -> *, :k Functor => (* -> *) -> Constraint
    -- fmap :: (a -> b) -> f a -> f b -- f a, f b のように、aを型引数に取っていてMaybeは型引数を一つ取るのでカインド(種類)が合う
-- instance (Eq m) => Eq (Maybe m) where --Maybe Int とか具体型を書かなくても、型引数?多相型?を使える。(Eq m) => はC#では、where m : Eqでジェネリックの型制約の様な
instance Eq' Color where
    R === R = True      -- "==="は上のclassの"==="をオーバーライドして実装している
    G === G = True
    B === B = True
    _ === _ = False

c0 :: Bool
c0 = R === R -- =>True
c1 :: Bool
c1 = R === G -- =>False
c2 :: Bool
c2 = G === B -- =>False

c3 :: Bool
c3 = R =/= R -- =>False
c4 :: Bool
c4 = R =/= G -- =>True
c5 :: Bool
c5 = G =/= B -- =>True
-- ====================================================================================================================================================================
