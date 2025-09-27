using System;
//`public static ref int Srefarg(ref Str str){return ref str.num;}//structのref来たものの中身をrefで返せる`  
//https://ufcpp.net/study/csharp/sp_ref.html?p=2 //refを返す
namespace Struct_ref{
    //`public static ref int Srefarg(ref Str str){return ref str.num;}//structのref来たものの中身をrefで返せる`  
    //https://ufcpp.net/study/csharp/sp_ref.html?p=2 //refを返す
    public struct Str{public ref Str Ret_this(ref Str @this) => ref @this;}//@thisは通るのにthisはだめ。なぜだ       
    public class C {
        public void M() {
            Str str = new Str();
            ref Str r_str = ref str.Ret_this(ref str);//↓の状況を許すためstruct直下のメンバまたは自身(this(値型))は参照を返せない事にした。
        }                                                 //引数経由だと値型の参照渡しルールで安全な参照戻り値かコンパイラがチェックできる。
    }                                                         //そもそも、structのthisがrefなのは this = new Str();するとコピーが起こるから
    public class D{public int n;}
    public class R{public ref int Ret_D(){return ref (new D()).n;}}//参照型(ヒープ)のメンバの参照を返せる(ヒープならスコープを抜けてもヒープから削除されない)

    //struct S{public readonly ref int Y => ref _value[0];}//(C# 8.0)readonly refはただreadonly structをメソッド単位でreadonlyにして防衛的コピーを防いでいるだけだった
    //struct S{∫修飾子∫⇒❰public readonly❱ ∫戻り値∫⇒❰ref int❱ Y => ref _value[0];}
}