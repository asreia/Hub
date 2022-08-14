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
    static string ListShow<a>(List<a> list) =>
    list switch{
        Empty<a> => ".",
        Cons<a> and var (x, xs) => x + "_" + ListShow(xs)
    };
    static void Main()
    {
        Cons<int> list_int =
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
<<<<<<< HEAD
<<<<<<< HEAD
=======
//test
>>>>>>> f5c68b5 (test1)
=======
//変更
>>>>>>> f5c68b5 (変更)
