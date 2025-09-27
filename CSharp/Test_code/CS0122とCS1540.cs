using System;
namespace CS1540{
    public abstract class D{
        protected abstract void aFunc();
        protected void vFunc(){
            Console.WriteLine("D");
        }
        protected static void sFunc(){
            Console.WriteLine("D");
        }
    }
    public class C1 : D {
        protected override void aFunc(){
            Console.WriteLine("C1");
        }
    }
    public class C0 : D{
        protected override void aFunc(){
            Console.WriteLine("C0");
        }
        public static void M(){
            //((D)new C0()).aFunc();//CS1540
            //呼び出し元より上の型でキャストすると実行時に動的な型が何か分からずcallvirtで違反するか分からない
            //コンパイラは事前に動的な型を予測できない。
            //呼び出し元以下ならばcallvirtによる下りの分岐は呼び出し元以下なので違反しない。
            //呼び出し元とprotectedの間にキャストが入るとそこで分岐して戻ってこないイメージ
            //(new C1()).vFunc();
            //仮想メソッド以外でもエラるのは謎。
            C1.sFunc();
            //静的メソッドは◯
        }
    }
}
//CS0122 静的メソッド
public class C0S{protected static void P0(){/*C2S.M();C1S.P1();*/C0S.P0();}}
public class C1S:C0S{protected static void P1(){/*C2S.M();*/C1S.P1();C0S.P0();}}
public class C2S:C1S{protected static void M(){C2S.M();C1S.P1();C0S.P0();}}

//CS0122とCS1540 インスタンスメソッド
public class C0I{protected void P0(){/*new C2I().M();new C1I().P1();*/new C0I().P0();}}
public class C1I:C0I{protected void P1(){/*new C2I().M()*/;new C1I().P1();/*new C0I().P0();*/}}
public class C2I:C1I{protected void M(){new C2I().M();/*new C1I().P1();new C0I().P0();*/}}

//CS1540 仮想メソッド (下を呼び出してるように見えるがcallvirtにより自分を呼び出していて違反にならない)
public class C0V{protected virtual void V(){new C2V().V();new C1V().V();new C0V().V();}}
public class C1V:C0V{protected override void V(){new C2V().V();new C1V().V();/*new C0V().V();*/}}
public class C2V:C1V{protected override void V(){new C2V().V();/*new C1V().V();new C0V().V();*/}}