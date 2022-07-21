-- {-# OPTIONS_GHC -Wno-incomplete-patterns #-}
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
maximum' [] = errorWithoutStackTrace "error"
maximum' [x] = x
maximum' (x:y:xs) 
    | x < y =  maximum' (y:xs)
    | otherwise = maximum' (x:xs)
b = maximum[1,2,3,4]
a = and [True,True,True]
