using System;

namespace defaultまとめ{
    public struct Struct{int n;}
    class Program{
        static int F(int n) => default; //戻り値おけ
        static void M(string[] args){
            //defaultは既定値(0のようなもの)で初期化する
            _ = default(int);
            int n = default;  //ターゲットの型を見て型推論する
            float f;
            f = default;      //宣言時で無くてもいい//どんな型の変数でもdefaultを突っ込める?
            _ = n != default; //!=,==演算子の片方のオペランドの型を見て型推論する
            _ = n == default; //!=,==以外の演算子は多分使用不可
            _ = default == n;
            _ = default != n;
            var v = F(default);//引数おけ
            Span<int> span = default; //Span<T>の中のref(&)はnull?
            Struct str = default; // == new Struct() (構造体の場合、同じILを生成する)
            //var v1 = default;//型情報が両方ないので無理
            // 初期化せずにフィールドを読んでみる(既定値が入っている)
            var a = new DefaultValues();
            Console.WriteLine(a.i);
            Console.WriteLine(a.x);
            Console.WriteLine((int)a.c); // '\0' (ヌル文字)は表示できないので数値化して表示
            Console.WriteLine(a.b); // False
            Console.WriteLine(a.s == null); // null は表示できないので比較で。True になる
        }
    }

    class DefaultValues
    {
        public int i;
        public double x;
        public char c;
        public bool b;
        public string s;
    }
}