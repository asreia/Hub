public enum E_List : int{Empty, Cons}
public record List<a>();
public record Cons<a>(a a_,List<a> l) : List<a>; //コンストラクターの引数を満たして呼ばないと行けないのでオブジェクト初期化("{}")を使っても意味ない
public record Empty<a>() : List<a>;

public class M
{
    //ジェネリック、delegate(Func)、ラムダ式、switch式、Deconstruct、再帰関数、expression-bodied
    public static Func<List<a>,List<b>> List_Functor<a,b>(Func<a,b> f) => list =>
    list switch
    {
        Empty<a> => new Empty<b>(),
        Cons<a> and var (x, xs) => new Cons<b>(f(x), List_Functor(f)(xs))
    };
    // public static string List_stringShow<T>(List<T> list)
    // {
    //     switch(list.v)
    //     {
    //         case E_List.Empty :
    //             return ".";
    //         case E_List.Cons :
    //             T x = list.a_;
    //             List<T> xs = list.l;
    //             return x + "_" + List_stringShow(xs); //再帰
    //         default:
    //             throw new Exception("non-exhaustive(List_stringShow)");
    //     }
    // }
    public static void Main()
    {
        _ = new Cons<int>(1, new List<int>());
        Cons<int> list_int =
            new Cons<int>(1,
            new Cons<int>(2,
            new Cons<int>(3,
            new Cons<int>(4,
            new Cons<int>(5,
            new Empty<int>(
            ))))));

        // Func<List<int>,List<int>> List_ToMyStr = List_Functor<int,int>(x => x * 2);
        // Console.WriteLine(List_stringShow(List_ToMyStr(list_int)));

    }
}