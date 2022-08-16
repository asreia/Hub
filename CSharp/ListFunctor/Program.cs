using System;
class M{
    record List<a>();
    record Cons<a>(a a_,List<a> l) : List<a>; //コンストラクターの引数を満たして呼ばないと行けないのでオブジェクト初期化("{}")を使っても意味ない
    record Empty<a>() : List<a>;

    static Func<List<a>,List<b>> List_Functor<a,b>(Func<a,b> f) => list =>
    list switch
    {
        Empty<a> => new Empty<b>(),
        Cons<a> and var (x, xs) => new Cons<b>(f(x), List_Functor(f)(xs))
    };
    //ジェネリック、delegate(Func)、ラムダ式、switch式、Deconstruct、再帰関数、expression-bodied、record
    static string ListShow<a>(List<a> list) /*where a : Object*/ =>
    list switch{
        Empty<a> => ".",
        Cons<a> and var (x, xs) => x?.ToString() + "_" + ListShow(xs)
    };
    static void Main()
    {
        List<int> list_int =
            new Cons<int>(1,
            new Cons<int>(2,
            new Cons<int>(3,
            new Cons<int>(4,
            new Cons<int>(5,
            new Empty<int>(
            ))))));
        Console.WriteLine(ListShow(List_Functor<int,int>(x => x * 2)(list_int)));
    }
}
            // infixr 5 :-      -- 型引数は小文字から(大抵一文字)、List Int,List m の様に型なのか型引数か多相型なのか
            // data List a = Empty | a :- (List a) deriving(Eq, Ord, Show, Read) 区別している

            // listFunctor :: (a -> b) -> List a -> List b
            // listFunctor f Empty = Empty
            // listFunctor f (x :- xs) = f x :- listFunctor f xs

            // lFVal :: List Int
            // lFVal = listFunctor (* 2) (1 :- 2 :- 3 :- 4 :- Empty)

            //Haskellとの違い
            //Haskellの関数の評価とマッチ: "func a b"のように"()"を書かない。結合的にだめな時に"()"を付けるだけ
            //Haskellの型推論: 型推論がすごすぎて型を指定しなくてもいいためC#のジェネリックの型を指定しなくてもいい
            //Haskellの値を生成?: 値コンストラクタに引数を並べるだけなので"new"とか特別なキーワードがない(Pythonもない)
            //Haskellはメンバアクセスがない: Haskellはマッチを利用して変数からメンバの値を取り出している
            //Haskellの直和型は継承構造ではなく、デフォルト機能

            //Haskellの直和型は、1つの型の中で"|"で区切り直和型を構成している
                //C#は型のダウンキャストを利用して複数の型で、型を要素とし直和型を構成している。 (継承構造をポリモーフィズムと見るか直和型(ダウンキャスト)と見るか)
            //Haskellのポリモーフィズムは型クラスという機能をつかい、classでAbstractしたものをinstanceで型に実装する (Haskellはダウンキャスト禁止)
                //C#では型,interfaceどうしが継承可能で、基底クラスのoverrideによって実装する
            
            //Haskellは"データ/data/型"と"関数/型クラスの関数"が完全に分離して定義している (型を対象とし関数(射)で繋げている)
                //C#のローカル変数は関数内で状態を持ち、フィールド変数は複数の関数間で状態を持つ

            //Haskellは、射(関数)の繋がりによってEndoを表現している(予測可能(参照透過性))
                //C#は、関数を呼ぶ順序によってEndoを表現している。そして、その呼び出す順序は外部の入力と条件分岐により予測不能
                //大体クラスという単位で時間的、Endoを構成している
                //フィールド変数への状態の更新、"="(代入)には警戒する

            //List_Functor:状態を更新するかも知れない記号"="(代入記号)が無い

            //Haskellは関数または定数を定義するとき、｢定数｣ :: 型, ｢関数名｣ :: 型 -> 型 -> 型 というように
            //定数の型、関数に与える引数と戻り値の型を考える (｢定数｣も0引数を取る関数と考えれる)
            //この様に関数は何を引数にとり最終的に何を返すのか考える (カリー化により引数を少なく与えると残りの引数を必要とする新しい関数を返すので、引数を与える順番も重要)
            //そして、これ以外に外部から情報を取り入れる術がなく、関数は式を引数に適用し一つの結果を返す