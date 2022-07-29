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

listDo = do         -- 食わせる所までが一行。上から実行される。
    op <- [(+),(*)] -- [(+),(*)] >>= (\op ->
    x <- [2,3]      -- [2,3] >>= (\x ->
    y <- [4,5]      -- [4,5] >>= (\y ->
    [op x y]        -- [op x y])))
listDo1 = do         -- 食わせる所までが一行。上から実行される。
    op <- [(+),(*)] -- [(+),(*)] >>= (\op ->
    x <- [op 2, op 3]      -- [2,3] >>= (\x ->
    y <- [x 4,x 5]      -- [4,5] >>= (\y ->
    [y]        -- [op x y])))
-- listDo1 = do     -- join . fmap f $ m で、各行から終わりがjoin . fmap f <- m の様なもの              -- (((aaa <- m \a) <- m \a) <- m \a) <- m
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
infixl 1 <=*
(<=*) a b = ()
infixl 2 <=**
(<=**) a b = ()
infixl 3 <=***
(<=***) a b = ()
infixl 4 <=****
(<=****) a b = ()
infixl 5 <=*****
(<=*****) a b = ()
infixl 6 <=******
(<=******) a b = ()
infixl 7 <=*******
(<=*******) a b = ()
infixl 8 <=********
(<=********) a b = ()
infixl 9 <=*********
(<=*********) a b = ()

infixr 0 #=>
(#=>) a b = ()
infixr 1 *=>
(*=>) a b = ()
infixr 2 **=>
(**=>) a b = ()
infixr 3 ***=>
(***=>) a b = ()
infixr 4 ****=>
(****=>) a b = ()
infixr 5 *****=>
(*****=>) a b = ()
infixr 6 ******=>
(******=>) a b = ()
infixr 7 *******=>
(*******=>) a b = ()
infixr 8 ********=>
(********=>) a b = ()
infixr 9 *********=>
(*********=>) a b = ()
data D a b = A a b | B1 (a,b) | C Int (D a b) deriving(Show)
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
                                                                    -- //CS0307: 型パラメーター「f」は型引数と一緒に使用できません //文脈には関数を定義できない
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
