using System;
namespace MethodKind{
    public abstract class Super {
        public static void S(){}     //静的メソッド                : static
        public void I(){}            //インスタンスメソッド         : instance
        public abstract void A();    //抽象(仮想)メソッド          : newslot abstract virtual instance
        public virtual void V(){}    //仮想メソッド                : newslot virtual instance
    }
    //newslotは多分、各生成可能なクラスに一つずつあるVtableに新しい仮想メソッドへのポインタ追加する事だと思う..
    public class Sub:Super{
        /*Vtable* _VtPtr;*/           //仮想メソッドへのポインタのクラスのようなもの(静的に定まっている)
        public override void A(){}   //オーバーライド(仮想)メソッド : virtual instance
    }
    //＄メソッドの種類=❰❰＠❰newslot❱ ＠❰abstract❱ ＠❰virtual❱ instance❱¦static❱
    //＄静的メソッド=❰∫メソッドの種類∫⇒❰static❱❱
    //＄公儀のインスタンスメソッド=❰∫メソッドの種類∫⇒❰＠❰newslot❱ ＠❰abstract❱ ＠❰virtual❱ instance❱❱
    //＄インスタンスメソッド=❰∫公儀のインスタンスメソッド∫⇒❰instance❱❱
    //＄公儀の仮想メソッド=❰∫公儀のインスタンスメソッド∫∠❰⸨＠❰virtual❱⸩❱⇒❰⟪❰virtual❱⟫❱
    //＄抽象メソッド=❰∫公儀の仮想メソッド∫∠❰⸨＠❰newslot❱ ＠❰abstract❱⸩❱⇒❰⟪❰newslot abstract❱⟫❱❱
    //＄仮想メソッド=❰∫公儀の仮想メソッド∫∠❰⸨＠❰newslot❱ ＠❰abstract❱⸩❱⇒❰⟪❰newslot❱⟫❱❱
    //＄上書メソッド=❰∫公儀の仮想メソッド∫∠❰⸨＠❰newslot❱ ＠❰abstract❱⸩❱⇒❰⟪⟫❱❱
    //∫メソッドの種類∫⇒❰✖⏎
    //    ∫静的メソッド∫¦✖⏎
    //    ∫公儀のインスタンスメソッド∫⇒❰✖⏎
    //        ∫インスタンスメソッド∫¦✖⏎
    //        ∫公儀の仮想メソッド∫⇒❰✖⏎
    //            ∫抽象メソッド∫¦✖⏎
    //            ∫仮想メソッド∫¦✖⏎
    //            ∫上書メソッド∫¦✖⏎
    //        ❱
    //    ❱
    //❱
}