using System;

//オーバーロード解決//https://ufcpp.net/study/csharp/structured/miscoverloadresolution/
       //↓に書いてないことはラムダ式、インスタンスと静的メソッド呼び分け、型制約違いの呼び分け

//クラス内
    //型完全一致 → option → option+params → params
    //↓
    //ジェネリック → option → option+params → params
    //↓
    //継承関係 → option → option+params → params
//外部
    //インスタンス
        //型完全一致~ →ジェネリック~ → 継承関係~
    //↓
    //拡張メソッド
        //型完全一致~ →ジェネリック~ → 継承関係~

//クラス内
namespace 型{
    // A → B → C の型階層
    // IDisposable インターフェイスを実装
    // C には int への暗黙的型変換あり
    class A : IDisposable { public void Dispose() { } }
    class B : A, IDisposable { }
    class C : B, IDisposable{
        public static implicit operator int(C x) => 0;
    }
    class Program{
        // 上から順に候補になる。
        // 上の方を消さないと、下の方が呼ばれることはない。

        // 「そのもの」が当然1番一致度高い
        static void M(C x) => Console.WriteLine("C");

        // 次がジェネリックなやつ。型変換が要らないので一致度が高いという扱い。
        static void M<T>(T x) => Console.WriteLine("generic");

        // 基底クラスは、階層が近い方が優先。この場合 B が先で、A が後
        static void M(B x) => Console.WriteLine("B");
        static void M(A x) => Console.WriteLine("A");

        // 次に、インターフェイス、暗黙的型変換が同率??。
        // (構造体の時の ValueType と違って、クラスは明確に基底クラスが上。)
        // この2つが同時に候補になってると ambiguous エラー
        static void M(int x) => Console.WriteLine("int");
        //////M(int x)(型変換)は継承関係の型と競合してあいまいエラー
        static void M(IDisposable x) => Console.WriteLine("IDisposable");
        
        // 最後が object。
        static void M(object x) => Console.WriteLine("object");
        static void Method(){
            // M のオーバーロードがいくつかある中、C を引数にして呼び出す
            M(new C());
        }
    }
}
namespace 個数{
    class Program{
        // これが最優先
        static void M() => Console.WriteLine("void");
        // 次がこれ。既定値を与えたもの
        static void M(int x = 0) => Console.WriteLine("int x = 0");
        // 最後がこれ。params
        static void M(params int[] x) => Console.WriteLine("params int[]");
        static void Method(){
            M();
        }
    }
}
namespace 型と個数{
    public class D{
    }
    public class C:D {
        //public void F(C c){Console.WriteLine("C:one");}//型が同じなのでoptionと衝突する
        public void F(C c = null){Console.WriteLine("C:option");}//型の一致が優先
        public void F(C c = null, params C[] cs){Console.WriteLine("C:option + params");}
        public void F(params C[] cs){Console.WriteLine("C:params");}
        //public void F(D d){Console.WriteLine("D:one");}//型違い
        public void F(D c = null){Console.WriteLine("D:option");}
        public void F(D c = null, params C[] cs){Console.WriteLine("D:option + params");}
        public void F(params D[] cs){Console.WriteLine("D:params");}
        public static void M() {
            C c = new C();
            c.F(c);
        }
    }
}
//外部
namespace インスタンスか拡張と型と個数{
    class A{
        public void M(params object[] os) => Console.WriteLine("instance");
    }
    static class Extensions{
        public static void M(this A a,A a1) => Console.WriteLine("extension");
    }
    class Program{
        static void Method(){
            new A().M(new A());//instanceが呼ばれる
        }
    }
}