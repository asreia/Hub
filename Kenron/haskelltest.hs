import qualified Data.Map as M --Data.Map内の定義物をM.∫LAny∫で参照

main :: IO()
main = do
    print ""
    -- print $ map' (*2) [1, 2, 3, 4]
    -- print $ and' [True,True,True]
    print $ maximum' [2]

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

data Val = Abc | Def | Val deriving(Show) -- 型コンストラクタと値コンストラクタ つまり、型と値は、大文字から始まる(関数は小文字から)
                                            -- コンストラクタというのは、引数を受け取って型または値を作るから
type Waybe = Maybe -- たしか、型シノニム

infixr 5 :-
data List a = Empty | a :- (List a) deriving(Eq, Ord, Show, Read)
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