using System;

namespace readonly構造体{
    public struct S{
        public int A;
        public int B;
        public void M()=> A = B = 4;
        //public readonly void RM()=> A = B = 4;
        //8.0//readonlyメソッドはメソッド内のフィールドへのアクセスをreadonlyとして扱い防衛的コピーをメソッド単位で防ぐ
    }
    public readonly struct RS{
        public readonly int A;    //readonly structは全てのフィールドメンバ変数をreadonlyにすることを義務付け
        public readonly int B;    //thisもref readonly RS this になる
        //public void M()=> A = B = 4;
        public void M1(){}
    }
    public static class Program {
        static readonly S s = new S();
        static readonly RS rs = new RS();
        public static void M() {
            s.M();
            Console.WriteLine($"s.A:{s.A}, s.B:{s.B}");
            //readonlyなメンバの値型(構造体)にその内部を変更しうるメソッドを呼ぼうとすると
            //防衛的コピー((t=s).M())が起きる
                S s1 = new S();  S t;  (t=s1).M();  Console.WriteLine($"s1.A:{s1.A}, s1.B:{s1.B}");
            rs.M1();//RSはreadonly structのため防衛的コピーは発生しない

            //readonly struct や readonlyメソッドは防衛的コピーを防ぐだけで副作用は発生しない
            //コピーコストを無くすために付けれるなら付けたほうがいいと思う
            //イメージ的に階層的に readonly.readonly.readonly または No_readonly.No_readonly.No_readonlyと
            //なるなら防衛的コピーは発生しないが
            //readonly.readonly.No_readonlyとなるなら防衛的コピーは発生すると思う
        }
    }
}