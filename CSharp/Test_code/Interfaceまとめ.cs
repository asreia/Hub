using System;
interface Inter0{//interfaceはフィールドが持てない以外はabstractと同じ
    //int n;//メンバ変数は仮想にならないのでインスタス変数を宣言するとエラー
    /*％❰public❱*/ /*％❰abstract❱*/ void IAFunc();//メソッド定義は∫仮/イ/静∫を省略すると仮想メソッドに
    /*％❰public❱*/ /*％❰virtual❱*/ void IVFunc(){}//なるのでインスタスメソッドにはならない様になっている
    sealed void ISFunc(){}//sealedを付けるとインスタスメソッドになる
    private /*not❰virtual❱*/ void IPriFunc(){}//privateな仮想メソッドは無いのでインスタスメソッドになる
    protected void IProFunc(){}//protectedはCS1540と普通の実装はpublicという事からinterface内のみ使用可能
}
interface Inter1 : Inter0{//↓interface内ならキャスト無しでそのまま呼べる
    void Inter0.IAFunc(){IVFunc();IProFunc();}//interfaceのオーバーライドは明示的実装のみ
    abstract void Inter0.IVFunc();//再抽象化
}
abstract class Prog : Inter1{
    public void IVFunc(){}//普通の実装はpublicを必ず書く//Inter0にキャストできる以外は定義通りの機能になる
    void Inter0.IVFunc(){}//明示的と普通が両方ある場合、明示的が仮想メソッドになる
    
    public void prog(){
        IVFunc();               //普通の方が呼ばれる
        ((Inter0)this).IVFunc();//明示的の方が呼ばれる
        ((Inter0)this).IAFunc();//interfaceのデフォルト実装または明示的実装はinterfaceにキャストする必要がある
        ((Inter0)this).ISFunc();
        ((Inter1)this).ISFunc();//キャスト先がinterfaceであればInter0でも1でもいいみたい
        //((Inter0)this).IProFunc();//CS1540
    }
}

