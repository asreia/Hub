using System;
namespace スコープと寿命{
    namespace ID{
        public class ID{//クラス名とそのメンバ名は名前が衝突する(同じ名前は∫Tructor∫として使われるため)
            public class ID0{
                public static int ID0_0 = 0;
                public static int ID0_1()=> 1;
            }
            public static int ID1 = 3;
            public static int ID2()=> 4;
        }
    }
    public class Prog {
        public static void Func(bool n,out int m){m = 0;}
        public static void M() {
              //global::名前空間.クラス.メンバ(クラス).メンバ
            _ = global::スコープと寿命.ID.ID.ID0.ID0_0;
            _ = global::スコープと寿命.ID.ID.ID0.ID0_1();
             //global::名前空間.クラス.メンバ
            _ = global::スコープと寿命.ID.ID.ID1;
            _ = global::スコープと寿命.ID.ID.ID2();
              //∫Complex∫とそのメンバ間で名前が衝突する以外は各層(ID.ID)で衝突しないと思う

            /*_ = ID0;*/
            int ID0 = 0;
            {
                int ID1 = 0;
                _ = ID0 = ID1;
                {
                    int ID2 = 0;
                    _ = ID0 = ID1 = ID2;
                }
                 _ = ID0 = ID1 /*= ID2*/;
            }
             _ = ID0 /*= ID1 = ID2*/; //int ID0,ID1,ID2;
            //基本的に変数のスコープはブロック"{}"内のみ
            //変数の寿命は宣言より下からブロックの終わり"}"まで(readする前にwriteされている事(確実な初期化))

            int IDf0 = 0;
            for(int IDf1 = 0;IDf1 < 1;_ = IDf0 is int IDf2){//関数とif文以外のisの変数定義のスコープはその"()"内
                int IDf3 = 0;//ループ内の変数定義は使い回しではない
                _ = IDf0 = IDf1 /*= IDf2*/ = IDf3;//IDf1(forのループ変数)は使い回し。foreachは使い回しではない
                IDf1 = 1;
            }

            //ローカル関数
            int c = 4;//呼び出し前にキャプチャされる変数を初期化していればいい
            // ローカル関数は宣言より前で使える     //つまり、Python式
            var y = f0(2);
            int f0(int x) => x * c;

            //ラムダ式
            Func<string, int> f1 = s => int.TryParse(s, out var x) ? x : -1;
            //s => {int.Try..}のようにラムダ式の本体は{ブロック}で囲まれるためスコープもブロック内
            f1("123");    //↑Linqでよく使われる
            //Console.WriteLine(x); // ここで x は使えない

            //関数とif文
            int IDF0 = 0;
            Func(IDF0 is int n, out int refn);if(IDF0 is int m){}
            _ = n = refn = m;
        }
    }
}