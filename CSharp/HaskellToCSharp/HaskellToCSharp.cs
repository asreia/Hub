//dotnet new consoleしてプロジェクトファイル(.csproj)を作らなくてもOmniSharpがコード補完した(作らないと補完が効かない場合も発生した)
//デバッグ方法(独自にVSCodeを立ち上げないといけない)
    //C#ソースコードを空のフォルダに入れてVSCodeを起動 (空フォルダに名前を付けコードを"Program.cs"にコピペでもいいかも)
    //ターミナル(PowerShell)で"dotnet new console"というコマンドを実行する(と、C#のプロジェクトが作られる)
    //プロジェクトが作られた時に生成された"Program.cs"を削除する
    //(しなくてもいいみたい)"dotnet run"でビルド＆実行して動くことを確かめる
    //VSCodeを再起動する
    //再起動直後に右下に表示が出るのでYseをクリックする(と、デバッグ構成を自動でつくる)
        //(Required assets to build and debug are missing from 'XXXXX'. Add them?)
    //デバッグできる(.NET Core Launch (console)という名前)
using System;
using System.Runtime.InteropServices;

//ジェネリック型は明示的(Explicit)なレイアウトを持つことができないエラー
//[StructLayout(LayoutKind.Explicit)]
public enum E_Maybe : int{Nothing, Just}
public class Maybe<a> //struct_ok
{
    //[FieldOffset(0)]
    public E_Maybe v;
    //[FieldOffset(4)]
    public a a_;
}
//[StructLayout(LayoutKind.Explicit)]
public enum E_Either : int{Left, Right}
public class Either<a, b> //struct_ok
{
    public class C{}
    public Either<a, b>.C c; //Either<a, b>の中のCである事を示すためにEither<a, b>は必要か
    //[FieldOffset(0)]
    public E_Either v;
    //[FieldOffset(4)]
    public a a_;
    //[FieldOffset(4)]
    public b b_;
}
//[StructLayout(LayoutKind.Explicit)]
public enum E_List : int{Empty, Cons}
public class/*✖❰struct❱*/ List<a>
{
    //[FieldOffset(0)]
    public E_List v;
    //[FieldOffset(4)]
    public a a_;
    //[FieldOffset(12)]
    public List<a> l;  //CS0523:structで再帰的定義はやばいよ 
    
    public List<b> Functor<b>(Func<a,b> f)
    {
        return Functor_(this, f);
        //型パラメーター 'b' は、外部メソッド 'List<a>.Functor<b>(Func<a, b>)' の型パラメーターと同じ名前です
        static List<b> Functor_/*％❰<b>❱*/(List<a> list, Func<a,b> f)
        {
            switch(list.v)
            {
                case E_List.Empty :
                    return new List<b>{v = E_List.Empty};
                case E_List.Cons :
                    a x = list.a_;
                    var newList = new List<b>{v = E_List.Cons, a_ = f(x)};
                    List<a> xs = list.l;
                    newList.l = Functor_(xs, f); //再帰
                    return newList;
                default:
                    throw new Exception("non-exhaustive(Functor_)");
            }
        }
    }
}
public class M
{
    //Func<a,b> -> Func<List<a>,List<b>> List関手！
    public static Func<List<a>,List<b>> List_Functor<a,b>(Func<a,b> f) => list =>
    {
        switch(list.v)
        {
            case E_List.Empty :
                return new List<b>{v = E_List.Empty};
            case E_List.Cons :
                a x = list.a_;
                List<a> xs = list.l;
                return new List<b>{v = E_List.Cons, a_ = f(x), l = List_Functor(f)(xs)};
            default:
                throw new Exception("non-exhaustive(List_Functor)");
        }
    };
    public static Maybe<int> ESum(Either<string, List<int>> eitherList)
    {
        switch(eitherList.v) //パターンマッチ
        {
            case E_Either.Left :
                string s = eitherList.a_; //値取り出し
                Console.WriteLine(s); //副作用
                return new Maybe<int>{v = E_Maybe.Nothing};
            case E_Either.Right :
                List<int> xs = eitherList.b_; //値取り出し
                return new Maybe<int>{v = E_Maybe.Just, a_ = Sum(xs)}; // Sum呼び出し
            default:
                throw new Exception("non-exhaustive(ESum)");
        }
        static int Sum(List<int> list)
        {
            switch(list.v)
            {
                case E_List.Empty :
                    return 0;
                case E_List.Cons :
                    int x = list.a_;
                    List<int> xs = list.l;
                    return x + Sum(xs); //再帰
                default:
                    throw new Exception("non-exhaustive(Sum)");
            }
        }
    }
    public static string Maybe_intShow(Maybe<int> maybe_int)
    {
        switch(maybe_int.v)
        {
            case E_Maybe.Nothing :
                return "Nothing";
            case E_Maybe.Just :
                return maybe_int.a_.ToString();
            default :
                throw new Exception("non-exhaustive(Maybe_intShow)");
        }
    }
    public static string List_stringShow(List<string> list)
    {
        switch(list.v)
        {
            case E_List.Empty :
                return "￥n";
            case E_List.Cons :
                string x = list.a_;
                List<string> xs = list.l;
                return x + "～" + List_stringShow(xs); //再帰
            default:
                throw new Exception("non-exhaustive(List_stringShow)");
        }
    }
    //演算子のオーバーロードはジェネリックにはできなかった
    //public static Func<a,c> operator*<a,b,c>(Func<b,c> g, Func<a,b> f){
    public static Func<a,c> Composition<a,b,c>(Func<b,c> g, Func<a,b> f) => a => g(f(a));//簡単！
    public static void Main()
    {
        Either<string,List<int>> eitherList1 = 
            new Either<string,List<int>>{v = E_Either.Right, b_ = 
                new List<int>{v = E_List.Cons, a_ = 1, l = 
                new List<int>{v = E_List.Cons, a_ = 2, l = 
                new List<int>{v = E_List.Cons, a_ = 3, l = 
                new List<int>{v = E_List.Cons, a_ = 4, l = 
                new List<int>{v = E_List.Empty,
                }}}}}
            };
        Either<string,List<int>> eitherList2 = 
            new Either<string,List<int>>{v = E_Either.Left, a_ =
                "Leftだよ"
            };
        //一発でうまくいった。時間はめちゃくちゃかかったが状態を持たないと言うのはバグを起こしにくいのかも知れない
        Console.WriteLine(Maybe_intShow(ESum(eitherList1)));
        //Composition、関数から型推論できなかった
        Console.WriteLine(
            Composition<Either<string, List<int>>,Maybe<int>,string>(Maybe_intShow, ESum)(eitherList1)
        );
        Console.WriteLine(Maybe_intShow(ESum(eitherList2)));
        
        List<int> list_int =
            new List<int>{v = E_List.Cons, a_ = 1, l = 
            new List<int>{v = E_List.Cons, a_ = 2, l = 
            new List<int>{v = E_List.Cons, a_ = 3, l = 
            new List<int>{v = E_List.Cons, a_ = 4, l = 
            new List<int>{v = E_List.Cons, a_ = 5, l = 
            new List<int>{v = E_List.Empty,
            }}}}}};
        //List<a>.Functor(ToMyStr)にListを与えたもの
        List<string> list_str = list_int.Functor(ToMyStr);
        Console.WriteLine(List_stringShow(list_str));
        //List_FunctorでToMyStrをListに持ち上げ！
        Func<List<int>,List<string>> List_ToMyStr = List_Functor<int,string>(ToMyStr);
        Console.WriteLine(List_stringShow(List_ToMyStr(list_int)));
        static string ToMyStr(int n)
        {
            switch(n)
            {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "木";
                case 4:
                    return "four";
                default :
                    return "N";
            }
        }
    }
}