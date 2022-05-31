# SharpLab全コード

## クラス

### [Init.cs]============================================================================================

```C#
using System;
namespace Init{    //＄Recur=❰SF->SC->IF->∫Recur∫->IC❱
    public class Super{
        static int snum = 2;//5:Super静的フィールド(SF)初期化
        int num = 3;//7:Superフィールド(IF)初期化
        static Super(){
            Console.WriteLine($"5:Super静的フィールド初期化");
            Console.WriteLine($"6:Super静的コンストラクタ");
        }
        public Super(){
            Console.WriteLine($"7:Superフィールド初期化");
            Console.WriteLine($"8:System.Objectコンストラクタ呼び出し(SF->SC->IF->IC)");
            Console.WriteLine($"9:Superコンストラクタ");
        }
    }
    public class Sub:Super{
        public static int snum = 0;//1:Sub静的フィールド(SF)初期化
        int num = 1;//3:Subフィールド(IF)初期化
        static Sub(){
            Console.WriteLine($"1:Sub静的フィールド初期化");
            Console.WriteLine($"2:Sub静的コンストラクタ");

            //Subコンストラクタに書きたいが初期化中に呼べないのでここに書く
            Console.WriteLine($"3:Subフィールド初期化");
            Console.WriteLine($"4:Superの発射");
        }
        public Sub(){

            Console.WriteLine($"10:Subコンストラクタ");
        }
    }
    public class Launch{
        public static void Main(){
            //_ = Sub.snum; //SF->SCはクラスに初めてアクセスするとき実行される
            Console.WriteLine($"0:Subの発射");
            new Sub();
        }
    }
}
```

### [アクセスの仕方とnew]============================================================================================

```C#
using System;
public class B{public virtual void oBF(){} public void BF(){} public static void sBF(){}}
public class D : B{public override void oBF(){} public new void BF(){} public new static void sBF(){}}
public class DD : D{public override void oBF(){} public new void BF(){} public new static void sBF(){}}
public class C {    //動的な型: {override → abstract,virtual} 静的な型,静的関数: {new → なし}
    public void M() {
        //インスタンスの静的な型に基づくcall instance
        new D().BF();
        ((B)new D()).BF();
        ((B)new DD()).BF();
        
        //インスタンスの動的な型に基づくcallvirt instance
        new D().oBF();
        ((B)new D()).oBF();
        ((B)new DD()).oBF();
        
        //インスタンス不要。静的関数 call
        //✖❰new D().sBF();❱
        D.sBF();
        DD.sBF();
        B.sBF();
    }
}
```

### [ILとメソッドの種類]============================================================================================

```C#
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
    //＄メソッドの種類=⟪＠❰newslot❱ ＠❰abstract❱ ＠❰virtual❱ instance❱｡¦｡static⟫
    //＄静的メソッド=❰∫メソッドの種類∫⊃❰static❱❱
    //＄公儀のインスタンスメソッド=❰∫メソッドの種類∫⊃❰｡＠❰newslot❱ ＠❰abstract❱ ＠❰virtual❱ instance❱｡❱
    //＄インスタンスメソッド=❰∫公儀のインスタンスメソッド∫⊃❰instance❱❱
    //＄公儀の仮想メソッド=❰∫公儀のインスタンスメソッド∫⊃❰＠❰newslot❱ ＠❰abstract❱ virtual instance❱❱
    //＄抽象メソッド=❰∫公儀の仮想メソッド∫⊃❰newslot abstract｡ virtual instance❱❱
    //＄仮想メソッド=❰∫公儀の仮想メソッド∫⊃❰newslot｡ virtual instance❱❱
    //＄上書メソッド=❰∫公儀の仮想メソッド∫⊃❰｡virtual instance❱❱   『override
    //∫メソッドの種類∫⊃❰⏎
    //    ∫静的メソッド∫¦⏎
    //    ∫公儀のインスタンスメソッド∫⊃❰⏎
    //        ∫インスタンスメソッド∫¦⏎
    //        ∫公儀の仮想メソッド∫⊃❰⏎
    //            ∫抽象メソッド∫¦⏎
    //            ∫仮想メソッド∫¦⏎
    //            ∫上書メソッド∫¦⏎
    //        ❱
    //    ❱
    //❱
}
```

### [仮想関数テーブル](Vtable)============================================================================================

```C#
using System;
using C = System.Console;

class C0{
    public virtual void VF0(){C.WriteLine("C0::VF0");}
    public virtual void VF1(){C.WriteLine("C0::VF1");}
}

class C1 : C0{
    public override void VF0(){C.WriteLine("C1::VF0");}
    public new virtual void VF1(){C.WriteLine("C1::VF1");}
}
class C2 : C1{
    public new virtual void VF0(){C.WriteLine("C2::VF0");}
    public new virtual void VF1(){C.WriteLine("C2::VF1");}
}
class C3 : C2{
    public override void VF0(){C.WriteLine("C3::VF0");}
    public override void VF1(){C.WriteLine("C3::VF1");}
}

class M{
    public static void Main(){
        C3 c3 = new C3();
        
        c3.VF0();       //=>C3::VF0//C2(virtual)にキャスト//C2::VF0 : C3:: VF0
        ((C0)c3).VF0(); //=>C1::VF0                      //C0::VF0 : C1:: VF0
        ((C1)c3).VF0(); //=>C1::VF0//C0(virtual)にキャスト//C0::VF0 : C1:: VF0
        ((C2)c3).VF0(); //=>C3::VF0                      //C2::VF0 : C3:: VF0
        ((C3)c3).VF0(); //=>C3::VF0//C2(virtual)にキャスト//C2::VF0 : C3:: VF0
        
        C.WriteLine();
        
        c3.VF1();       //=>C3::VF1//C2(virtual)にキャスト//C2::VF1 : C3::VF1
        ((C0)c3).VF1(); //=>C0::VF1                      //C0::VF1 : C0::VF1
        ((C1)c3).VF1(); //=>C1::VF1                      //C1::VF1 : C1::VF1
        ((C2)c3).VF1(); //=>C3::VF1                      //C2::VF1 : C3::VF1
        ((C3)c3).VF1(); //=>C3::VF1//C2(virtual)にキャスト//C2::VF1 : C3::VF1
        
        //暗黙的にnewslotにキャストされ、Vtableにより呼ばれるメソッドが決まる。
        //動的な型以上でnewslotの数だけ、そのメソッドについてのVtableに登録されるスロットが増える。
        //newslotがポリモーフィズムの区切りになっている。
        
        //Vtable
        //new C3 -> {
        //              VF0:[
        //                  {C0::VF0 : C1::VF0},
        //                  {C2::VF0 : C3::VF0}
        //              ],  
        //              VF1:[
        //                  {C0::VF1 : C0::VF1},
        //                  {C1::VF1 : C1::VF1},
        //                  {C2::VF1 : C3::VF1}
        //              ]
        //          }
    }
}
```

### [CS1540とアクセス]============================================================================================

```C#
using System;
namespace CS1540{
    public abstract class C0{
        protected abstract void aFunc();
        protected void iFunc(){
            Console.WriteLine("C0");
        }
        protected static void sFunc(){
            Console.WriteLine("C0");
        }
    }
    public class C1 : C0 {
        protected override void aFunc(){
            Console.WriteLine("C1");
            ((C2)new C1()).aFunc();
        }
    }
    public class C2 : C1{}
    
    public class C3 : C0{
        protected override void aFunc(){
            Console.WriteLine("C3");
        }
        public static void Main(){ 
            //CS1540: アクセス元より上の静的な型のインスタンス(基底クラス⟪のインスタンス¦へのキャスト⟫)からアクセスできない
            //{仮にnew C3() のC3がC1だとprotectedで完全に違反している}
            //((C0)new C3()).aFunc();//CS1540 //コンパイラが見えているのは C0.aFunc()呼び出し元:C3
                                               //C0.aFunc()はcallvirtでC3::aFunc()を呼ぶか分からない
            new C3().aFunc();//上(C0)にキャストしなければ呼べる
            //呼び出し元(C3(Main))より上の型でキャストすると
            //実行時に動的な型が何か分からずcallvirtで違反するか分からない
            //コンパイラは事前に動的な型を予測できない。
            //呼び出し元以下ならばcallvirtによる下りの分岐は呼び出し元以下なので違反しない。
            //呼び出し元と静的な型(C0)の間にキャストが入るとそこで分岐して戻ってこないイメージ
            //(new C1()).iFunc();
            //インスタンスメソッドでもエラーでるのは、アクセス元とアクセス先が共通の継承の親になるとは限らないから?
                //要するにprotectedは静的な型でそのprotectedが記述されてる型より下から呼ばないといけない?
                //[呼び出し"先"の型]<-[呼び出し"元"の型]<=[静的な型]<=[動的な型]
                //↓のように静的な型を介して分岐してしまうのを防ぐ
                //|```````| <- [呼び出し"先"の型]<=[動的な型]
                //|静的な型| <- [呼び出し"元"の型]
                //のときprotectedに違反する?
            C1.sFunc();
            //静的メソッドは◯
        }
    }
}

                //⟪V¦T¦B⟫⟪0¦1¦2⟫⟪S¦I¦V⟫
                //V := private, T := protected, B := public
                //0 := 基底, 1 := 0の派生, 2 := 1の派生
                //S := static, I := instance, V := virtual
//private(自身のクラス内からのみアクセスできる)=======================================================
//--static----------------------------------------------------------------------------------------
public class V0S    {private static void F0(){V2S.F0(); V1S.F0(); V0S.F0();}}

public class V1S:V0S{private static void F1(){V2S.F1(); V1S.F1();          }}

public class V2S:V1S{private static void F2(){V2S.F2();                    }}

//--instance----------------------------------------------------------------------------------------
//派生のクラスのインスタンスから自身のクラス内のメソッドは呼べる
public class V0I    {private void F0(){new V2I().F0();  new V1I().F0();  new V0I().F0();  }}

public class V1I:V0I{private void F1(){new V2I().F1();  new V1I().F1();                   }}

public class V2I:V1I{private void F2(){new V2I().F2();                                    }}

//--virtual----------------------------------------------------------------------------------------
//CS0621: 仮想(virtual)メンバーまたは抽象(abstract)メンバーをprivateにすることはできません
//public class V0V    {private virtual  void V(){}}
//public class V1V:V0V{private override void V(){}}
//public class V2V:V1V{private override void V(){}}


//protected(アクセス元から上にアクセス(継承下から自身か基底クラスのみにアクセスできる))==================
//--static----------------------------------------------------------------------------------------
public class T0S    {protected static void F0(){T2S.F0(); T1S.F0(); T0S.F0();}}

public class T1S:T0S{protected static void F1(){T2S.F0(); T1S.F0(); T0S.F0();
                                                T2S.F1(); T1S.F1();          }}

public class T2S:T1S{protected static void F2(){T2S.F0(); T1S.F0(); T0S.F0();
                                                T2S.F1(); T1S.F1();          
                                                T2S.F2();                    }}

//--instance----------------------------------------------------------------------------------------
//CS1540: 基底のクラスのインスタンスから呼べない(派生クラスのインスタンスを保持し呼ぶことはできる)
public class T0I    {protected void F0(){new T2I().F0(); new T1I().F0(); new T0I().F0();}}

public class T1I:T0I{protected void F1(){new T2I().F0(); new T1I().F0();
                                         new T2I().F1(); new T1I().F1();                }}

public class T2I:T1I{protected void F2(){new T2I().F0();
                                         new T2I().F1();
                                         new T2I().F2();                                }}

//--virtual----------------------------------------------------------------------------------------
//オーバーライドにより基底クラスから派生クラスのメソッドが呼ばれる
public class T0V    {protected virtual  void V(){new T2V().V(); new T1V().V(); new T0V().V();}}

public class T1V:T0V{protected override void V(){new T2V().V(); new T1V().V();               }}

public class T2V:T1V{protected override void V(){new T2V().V();                              }}


//public(どこの継承の部分からでもアクセスできる(どこからでもアクセス可能))==============================
//--static----------------------------------------------------------------------------------------
public class B0S    {public static void F0(){B2S.F0(); B1S.F0(); B0S.F0();
                                             B2S.F1(); B1S.F1();          
                                             B2S.F2();                    }}

public class B1S:B0S{public static void F1(){B2S.F0(); B1S.F0(); B0S.F0();
                                             B2S.F1(); B1S.F1();          
                                             B2S.F2();                    }}

public class B2S:B1S{public static void F2(){B2S.F0(); B1S.F0(); B0S.F0();
                                             B2S.F1(); B1S.F1();          
                                             B2S.F2();                    }}

//--instance----------------------------------------------------------------------------------------
public class B0I    {public void F0(){new B2I().F0(); new B1I().F0(); new B0I().F0();
                                      new B2I().F1(); new B1I().F1();
                                      new B2I().F2();                                }}

public class B1I:B0I{public void F1(){new B2I().F0(); new B1I().F0(); new B0I().F0();
                                      new B2I().F1(); new B1I().F1();
                                      new B2I().F2();                                }}

public class B2I:B1I{public void F2(){new B2I().F0(); new B1I().F0(); new B0I().F0();
                                      new B2I().F1(); new B1I().F1();
                                      new B2I().F2();                                }}

//--virtual----------------------------------------------------------------------------------------
public class B0V    {public virtual  void V(){new B2V().V(); new B1V().V(); new B0V().V();}}

public class B1V:B0V{public override void V(){new B2V().V(); new B1V().V(); new B0V().V();}}

public class B2V:B1V{public override void V(){new B2V().V(); new B1V().V(); new B0V().V();}}

```

## インターフェース

### [interface]============================================================================================

```C#
using System;

interface Inter0{//interfaceはフィールドが持てない以外はabstractと同じ
    //int n;//メンバ変数は仮想にならないのでインスタス変数を宣言するとエラー
    static int num = 4;
    public static void sfunc(){}
    int IntRet();
    int IntRetPulas1(){return IntRet() + 1;}
    /*％❰public❱*/ /*％❰abstract❱*/ void IAFunc();//メソッド定義は∫仮/イ/静∫を省略すると仮想メソッドに
    /*％❰public❱*/ /*％❰virtual❱*/ void IVFunc(){}//なるのでインスタスメソッドにはならない様になっている
    sealed void ISFunc(){}//sealedを付けるとインスタスメソッドになる
    private /*✖❰virtual❱*/ void IPriFunc(){}//privateな仮想メソッドは無いのでインスタスメソッドになる
    protected void IProFunc(){}//protectedはCS1540と普通の実装はpublicという事からinterface内のみ使用可能
}

interface Inter1 : Inter0{//↓interface内ならキャスト無しでそのまま呼べる
    void Inter0.IAFunc(){IVFunc();IProFunc();}//interfaceのオーバーライドは明示的実装のみ
    abstract void Inter0.IVFunc();//再抽象化
}

class Prog : Inter1{
    public int IntRet(){return 2;}
    public void IVFunc(){}//普通の実装はpublicを必ず書く//Inter0にキャストできる以外は定義通りの機能になる
    void Inter0.IVFunc(){}//明示的と普通が両方ある場合、明示的が仮想メソッドになる
    
    public void prog(){
        Console.WriteLine(((Inter0)this).IntRetPulas1());
        IVFunc();               //普通の方が呼ばれる
        ((Inter0)this).IVFunc();//明示的の方が呼ばれる
        ((Inter0)this).IAFunc();//interfaceのデフォルト実装または明示的実装はinterfaceにキャストする必要がある
        ((Inter0)this).ISFunc();
        ((Inter1)this).ISFunc();//キャスト先がinterfaceであればInter0でも1でもいいみたい
        //((Inter0)this).IProFunc();//CS1540
        Inter0.sfunc();
    }
    public static void Main(){(new Prog()).prog();}
}
//追記:要約:interfaceはフィールドが持てない代わりに多重継承できる
//         interfaceでメソッドを書くとデフォルト(％)で public と 仮想メソッド になる
//         static変数 と staticメソッド はクラスと構造体と変わらない
//         publicなメソッドだけinterface以外でオーバーライドできる
//         interfaceを含めた全ての明示的実装と、interface以外でオーバーライド(普通の実装)をされていない
//           interfaceの仮想メソッドと、interfaceのインスタンスメソッド、をinterface以外で呼ぶ場合はキャストが必要
//           つまり、interfaceのメソッドを呼ぶ時、interface以外で通常の実装をしているメソッド以外はキャストが必要
//           キャストが必要なのは多重継承で、呼び分けが必要なため?

```

## 構造体

### [構造体]============================================================================================

```C#
using System;
namespace Struct_matome{
    public interface I0{}public interface I1{}
    public struct S0{}
    public class C0/*:S0*/{}//構造体はsealedされている
    public struct S:/*C0,*/I0,I1{//クラスも継承不可。interfaceは多重継承可能(キャスト時はボックス化が起きる)
        //int n0 = 9;//多分、引数なしコンストラクタはフィールドをゼロ初期化するのでメンバ変数に初期化値を代入できない
                        //↑C# 10.0 よりdefaultとnew()を区別する事により引数なしctorを呼べる様になり、
        public int n1;    //その他にもメンバ変数代入初期化など色々な制約が解除される。
        public int p0{get;}//構造体はスタックに積まれるのでデータサイズは16バイト以下が最適
        public readonly int r0;
        //public S(){}//構造体の引数なしコンストラクタはC#によってフィールドをゼロ初期化するという機能で固定されている
        public S(int m){
            //Func();//CS0188:全てのフィールドメンバが割り当てられるまでthis(メソッド?)を使えない
            n1 = m;//↓,↓↓引数ありコンストラクタは全てのフィールドメンバの初期化を義務付けられる
            p0 = m;//構造体の?コンストラクタのget-onlyプロパティへの代入(メソッド)はバッキングフェールドに直接代入する
            r0 = m;//readonlyはコンストラクタのみ初期化可能なのでおけ
            Func();//全て割り当てれば呼べる
        }
        public void Func(){}
    }

    public class C {
        public static void Main() {
            S s0 = new S();//フィールドゼロ初期化
            S s1 = new S(5);//引数ありは全て割り当てされている
            Console.WriteLine($"s0: {s0.n1}, {s0.p0}, {s0.r0}");
            Console.WriteLine($"s0: {s1.n1}, {s1.p0}, {s1.r0}");
        }
    }
}
```

### [構造体はメンバのrefを返せない]============================================================================================

```C#
    using System;
//`public static ref int Struct_ref_arg(ref S s){return ref s.num;}//structのref来たものの中身をrefで返せる`
//https://ufcpp.net/study/csharp/sp_ref.html?p=2 //refを返す



namespace Struct_ref{
    
    public static class Ext{public static void ExtRef_arg<T>(this ref T o) where T : struct {}}
       //これは別の問題? refでclassを渡すと勝手に違うオブジェクトにすり替えられるのを防ぐ?(classのthisはreadonly)

    public struct S{public ref S Ret_this(ref S @this) => ref @this;    public int sx;}
                                                               //CS8170:@thisは通るのにthisはだめ。
    public class C {
        public int cx;
        public void M() {
            S s = new S();
            ref S rs = ref s.Ret_this(ref s);    s.sx = 4; C c = new C(); c.cx = 4;
            //↓↓の状況を許すためstruct直下のメンバまたは自身(this(値型))は参照(ref)を返せない事にした。
              //引数経由だと値型の参照渡しルールで安全な参照戻り値かコンパイラがチェックできる。
                //そもそも、structのthisがrefなのは this = new S();するとコピーが起こるから(？)
        }         //↑ufcpp:構造体内では、フィールドの読み書きのために、実はthisが参照扱いになっています。
    }                //{s.sx = 4;}はILでは、{[0] valuetype Struct_ref.S s} => {ldloca.s 0『アドレスld』}
                     // => {ldc.i4.4} => {stfld int32 Struct_ref.S::num} となっている
                     //クラスの場合は{ldloc.2『値ld(ヒープへの参照)』}となっている
    public class D{public int n;}
    public class R{public ref int Ret_D(){return ref new D().n;}}
                     //参照型(ヒープ)のメンバの参照を返せる(ヒープならスコープを抜けてもヒープから削除されない)
                     //参照型の方が寿命が長いため、値型は参照型に合わせるために直下の参照を返せないことにした？
}
//struct S{public readonly ref int Y => ref _value[0];}
    //(C# 8.0)readonly refはただreadonly structをメソッド単位でreadonlyにして防衛的コピーを防いでいるだけだった
    //struct S{∫修飾子∫⊃❰public readonly❱ ∫戻り値∫⊃❰ref int❱ Y => ref _value[0];}

```

## 初期化子

### [初期化子]============================================================================================

```C#
using System;

namespace 初期化子{
    using System.Collections.Generic;
    using System.Collections;
    using static System.Console;
    public class C:IEnumerable<int>{
        public IEnumerator<int> GetEnumerator(){return default;}//←↓は、使われていない
        IEnumerator IEnumerable.GetEnumerator(){return GetEnumerator();}
        public void Add(int n){WriteLine("Add:" + n);}
        public C(){WriteLine("引数なし");}
        public C(int m){n = p_get = p_getset = m; WriteLine("引数あり:" + m);}
        private int[] arr = new int[2];
        public int this[int i]{get{return arr[i];}set{arr[i] = value;}}
        public int n;
        public int p_get{get;}
        public int p_getset{get;set;}
    }
    public static class Program {
        public static void Main() {//初期化子はコンストラクタの後にアクセス可能なメンバに対して初期化する
            //==============================================================================
            C c0 = new C{n = 4, p_getset = 4, [0] = 4, [1] = 4};//オブジェクトとインデックス初期化子
            //new C(~){~}の"()"を省略すると引数なしコンストラクタが呼ばれる
            WriteLine($"c0.n:{c0.n}, c0.p_get:{c0.p_get}, c0.p_getset:{c0.p_getset}, c0[0]:{c0[0]}, c0[1]:{c0[1]}");
            //↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            C c1 = new C();
            c1.n = 4;
            c1.p_getset = 4;
            c1[0] = 4;
            c1[1] = 4;
            WriteLine($"c1.n:{c1.n}, c1.p_get:{c1.p_get}, c1.p_getset:{c1.p_getset}, c1[0]:{c1[0]}, c1[1]:{c1[1]}");
            //==============================================================================
                C c2 = new C(4){1, 2, 3};//コレクション初期化子//↑オブジェクトとインデックス初期化子と混ぜれない
            //==============================================================================
            List<int> l0 = new List<int>{1, 2, 3};//コレクション初期化子
            //IEnumerableを実装して、Addメソッドを持つクラスでできる
            //↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            List<int> l1 = new List<int>();
            l1.Add(1);
            l1.Add(2);
            l1.Add(3);
            //==============================================================================
                var map = new Dictionary<string, int>{{ "One", 1 },{ "Two", 2 },{ "Three", 3 }};
                //Addメソッドが2引数の時↑のように書ける(n引数のAddメソッドも作れる)
        }
    }
}
```

## ジェネリック

### [ジェネリック]============================================================================================

```C#
using System;
namespace Generic_matome{
    public interface Inter<in I, out O>{//変性はinterfaceとdelegateのみ可能
        static O Os;static I Is;//静的変数なら変性は無効?
        public O Om{get;} //共変(out)はreadonly的に使う
        public I Im{set;} //反変(in)はwriteonly的に使う
    }
    public class C <T, I, O>:Inter<I,O>{
        public O Om{get;}
        public I _Im;
        public I Im{set{_Im = value;}}
        public delegate O2 DFunc<in I1, out O2>(I1 i);//in:反変、out:共変
        public string Method<U,W>(U u, W w)//＄型仮引数=❰<class U, valuetype ~ W>❱(!!U u,!!W w)
            where U: class
            where W: struct=> u.ToString();
        public void M(){
            Inter<string,object> c0 = new C<int,object,string>();//
            //共変(out)は継承の向きobject←stringと代入の向きobject=stringが一致している。反変(in)は一致しない
            string s = Method("abc", 123);//::Method＄型実引数=❰<string, int32>❱(!!0, !!1)
            //引数の∫Lit∫から型が推論されている。引数から全ての型仮引数を指定子ないと推論されない
            Console.WriteLine(s);
        }
    }
    public class Prog{
        public static void Main(){
            var c =new C<int,object,string>();
            c.M();
            //配列はclassだけど特別に共変
            string[] str = { "A", "B", "C" };
            object[] obj = str;
        }   
    }
            //-------------------------------------------------------------------------------
                //[()] <= [静的] = (動的)。代入は静的な型(接点)に動的な型(obj)を内包する操作でもある。
                //変性は変性の付いたターゲット変数に代入される時、変性の効果がある。
                //アップキャスト的に代入するのは正常
                //値型は変性できない。         
            //-------------------------------------------------------------------------------
}
```

### [ジェネリック変性]============================================================================================

```C#
using System;

//変性はジェネリックのアップキャストを可能にするもの
//変性のinは静的な型から入ってきた型をすぐ動的な型にアップキャストする(静的-in->動的)
//ontは動的な型から出ていく時に静的な型にアップキャストする(静的<-out-動的)
//変性は入出力時にアップキャストが入る機能?
//変性された型に代入する時は、入出力時にアップキャストになるような動的な型を入れれて制約が緩くなる方向になる
//変性された型を使う時は、アップキャストが入るので制約が入る方向になる?(静的⇔動的自動アップキャスト機能?)
//変性されたinterfaceの型に統一しList<Inter<In,Out>> list 見たいのが作れる?
//delegateも同じだと思う

public class Base{
    public virtual void VFunc(){Console.Write("VFunc");}
}
public class SubA : Base{
    public override void VFunc(){Console.Write("OFunc");}
}
public interface Inter<in I, out O>{
    public O o{get;}
    public O IFunc(I i);
    //インスタンスメソッドは動的な型がBase(clsBS)で静的な型がSubA(interSB)という状態が起こりうり、
    //動的と静的が逆になっていて呼べないしインターフェースだと定義できない。ので変性はインターフェース?
    //なのでOverrideされたメソッドを呼ぶしかない。その入出力時にアップキャストが入る。
}
public class Cls<I, O> : Inter<I, O> where I: Base where O: Base{
    public O o{get;}
    public Cls(O o){this.o = o;}
    public O IFunc(I i){//SubAからBaseにアップキャストが発生している?
        i.VFunc();
        Console.Write(" i:"+i.GetType());
        Console.Write(" I:" + typeof(I) + " ");
        o.VFunc();
        Console.Write(" o:"+o.GetType());
        Console.WriteLine(" O:" + typeof(O));
        return o;
    }
}
public class Cls : Inter<Base, SubA> {//Cls非ジェネ版
    public SubA o{get;}
    public Cls(SubA o){this.o = o;}
    public SubA IFunc(Base i){//SubAからBaseにアップキャストが発生している?
        i.VFunc();
        Console.Write(" i:"+i.GetType());
        Console.Write(" I:" + typeof(Base) + " ");
        o.VFunc();
        Console.Write(" o:"+o.GetType());
        Console.WriteLine(" O:" + typeof(SubA));
        return o;
    }
}
public class C {
    public static void Main() {
        //var clsBS = new Cls<Base, SubA>(new SubA());
        var clsBS = new Cls(new SubA());//Cls非ジェネ版
        clsBS.IFunc(new Base());
        clsBS.IFunc(new SubA());
        
        Inter<SubA, Base> interSB = clsBS;
        //interSB.IFunc(new Base());//「Base」から「SubA」に変換できません
        Base b = interSB.IFunc(new SubA());
        //SubA s = interSB.IFunc(new SubA());//「Base」を「SubA」に暗黙的に変換することはできません。
        
        
    }
}
```

## スコープと寿命

### [スコープと寿命]============================================================================================

```C#
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
        public static void Main() {
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
                IDf1 = 1;    //使い回せるか?と言うのは、action += () => WriteLine(IDF~);の様にキャプチャした時、
            }                    //IDF~が1ループ毎にキャプチャするか共有なのか、という事みたい。
                                 //https://ufcpp.net/study/csharp/start/st_scope/?p=2#for-loop-variable
                                 //ループ毎に変数宣言されるかと言う意味ではない、そもそもILコードに
                                 //変数を宣言する命令はない。キャプチャしない限り、使い回しの有無の差は無いと思う

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
```

## オーバーロード解決

### [オーバーロード解決]============================================================================================

```C#
using System;

//追記: オーバーロード解決は名前空間の優先度の解決の後にある。
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
        public static void Method(){
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
        //public void F(C c){Console.WriteLine("C one");}//型が同じなのでoptionと衝突する
        public void F(C c = null){Console.WriteLine("option");}//型の一致が優先
        public void F(C c = null, params C[] cs){Console.WriteLine("option + params");}
        public void F(params C[] cs){Console.WriteLine("params");}
        //public void F(D d){Console.WriteLine("D one");}//型違い
        public void F(D c = null){Console.WriteLine("option");}
        public void F(D c = null, params C[] cs){Console.WriteLine("option + params");}
        public void F(params D[] cs){Console.WriteLine("params");}
        
        public static void Main() {
            C c = new C();
            c.F(c);
            
            型.Program.Method();
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
```

## 型の決定と型推論

## デリゲート、ラムダ式

### [デリゲート代入テスト]============================================================================================

```C#
using System;
using System.Collections.Generic;

//FuncとActionの実装
namespace System{
    delegate TResult Func_<in T, out TResult>(T arg);
    delegate TResult Func_<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    delegate void Action_<in T>(T arg);
    delegate void Action_<in T1, in T2>(T1 arg1, T2 arg2);
}

namespace デリゲート代入テスト{
    public static class Ext{
        public static void ExtFunc(this I m){Console.WriteLine("ExtFunc");}
    }
    
    public interface I{
        public void IFunc(){Console.WriteLine("Interface(仮想メソッド)");}
        public sealed void ISealedFunc(){Console.WriteLine("Interface(Sealed(==instance))");}
        public static void ISFunc(){Console.WriteLine("Interface(static)");}
    }
    
    public partial class Program{
        static partial void ParFunc(){Console.WriteLine("ParFunc");}
    }
    public class Base{
        public virtual void VFunc(){Console.WriteLine("Base:Vfunc");}
    }
    public partial class Program:Base,I {
        public override void VFunc(){Console.WriteLine("Program:Vfunc");}
        static partial void ParFunc();
        public string Expr()=>"Expr";
        public static Program operator+(Program m0, Program m1){
            Console.WriteLine("operator+");
            return default;
        }
        public int P{get;}
        //public delegate void F();
        public IEnumerator<int> Iter(){Console.WriteLine("Iter");yield break;}
        
        public void Method(){
            //◎仮想メソッドおけ//呼べるようになった→sharplabだと仮想メソッドは不可(Unbreakable.AssemblyGuardException:検証できないポインター操作を実行します。)
            Action DVF = this.VFunc;
            DVF();
            //✖プロパティ不可(関数として表現できない)
            //Func<int> DP = P;
            //◎interfaceの仮想メソッドおけ
            Action DIF = ((I)this).IFunc;
            DIF();
            //◎interfaceのsealed(インスタンスメソッド)おけ
            Action DISealedF = ((I)this).ISealedFunc;
            DISealedF();
            //◎interfaceの静的メソッドはおけ
            Action DISF = I.ISFunc;
            DISF();
            //✖コンストラクタは型と認識され不可
            //Action a2 = Program;
            //◎イテレーターおけ
            Func<IEnumerator<int>> FEnum_i =Iter;
            FEnum_i().MoveNext();
            //◎拡張メソッドおけ
            Action IExt = ((I)this).ExtFunc;
            IExt();
            //◎演算子のオーバーロードはラムダ式を噛ませればいける//それは、ラムダ式なのでは..
            Func<Program,Program,Program> Plus = (p0,p1) => p0 + p1;
            Plus(new Program(),new Program());
            //◎expression-bodiedおけ
            Func<string> Ex = Expr;
            Console.WriteLine(Ex());
            //◎partial(部分メソッド)おけ
            Action Par = ParFunc;
            Par();
        }
        public static void Main() {
            new Program().Method();
        }
    }
}
```

## ローカルメソッド

### [ローカルメソッド]============================================================================================

```C#
using System;
namespace ローカルメソッド{
    
    //デリゲート型で変数にメソッドを束縛するとメソッド内で使われる変数(オブジェクト)はメソッドを束縛した変数の
    //寿命が終わるまでメソッド内で使われる変数の寿命が伸びてしまう。(ローカル変数はクラスのフィールドに昇格する)
    
    //ローカルメソッドは∫イ/静∫(書かなくても自動的に決定される)、アクセスなし
    //よってラムダ式でもローカルメソッドと大体おなじ振る舞い
    
    public class Class{
        //====================================================================================
        class キャプチャなし{
            public void Method0(){
                NoCap();
                int shadow = 123;
                /*％❰static❱*/ void NoCap(){
                    int localMethod = 4;
                    int shadow = 123; //シャドーイング(8.0)外の識別子と同名の識別子を定義すると別々の識別子になる
                }
            }
            //↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕単にstaticメソッドを中に入れただけ↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            public void Method0_(){
                NoCap_();
                int shadow = 123;
            }
            
            internal static void NoCap_(){
                int localMethod_ = 4;
                int shadow = 123;
            }
        }
        //====================================================================================
        class フィールドメンバのキャプチャ{
            public int field = 2;
            public void Method1(){
                Field();
                /*✖❰static❱*/ void Field(){//staticを明示的に付けると静的ローカルメソッドと言って、
                    field = 4;                //静的メンバ変数以外のキャプチャを明示的に禁止することができる
                }                           //(オブジェクトの寿命を伸ばさない)
            }
            //↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕単にインスタンスメソッドを中に入れただけ↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            public int field_ = 2;
            public void Method1_(){
                Field_();
            }
            
            private void Field_(){
                field_ = 4;
            }
        }
        //====================================================================================
        class ローカル変数のキャプチャ{
            int field = 2;
            public Action Method2(){//Actionをvoidにして外部に漏れない場合、struct Displayになる
                int local = 3;        //漏れる場合、class Displayになる(なので漏れない場合、安心して書ける)
                LocVal();
                return LocVal;
                /*✖❰static❱*/ void LocVal(){
                    local = 4;
                    field = 4;
                }
            }
            //↕↕↕↕↕↕↕↕ローカル変数の状態をブロック(Method2)から抜けても保持するため、↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            //thisとローカル変数とローカルメソッドをメンバに持つクラスを定義しそのインスタンスを生成して、↕↕↕↕↕↕
            //インスタンス経由でローカル変数とフィールドの変数を扱う。↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            public Action Method2_(){                //漏れる場合、ローカル変数をヒープに飛ばしボクシングする
                Display disp = new Display();
                disp.@this = this;
                disp.local_ = 3;
                disp.LocVal_();
                return new Action(disp.LocVal_);
            }
            
            int field_ = 2;
            private sealed class Display{    
                public int local_;
                public ローカル変数のキャプチャ @this;
                internal void LocVal_(){
                    local_ = 4;
                    @this.field_ = 4;
                }
            }

        //====================================================================================
        }
    }
}
    
```

## refのルール

### [refのルール]============================================================================================

```C#
using System;

namespace refルール{
    public class C{
        /*
        void M(ref int x){
            // クロージャに使えない
            Action<int> a = i => x = i;
            void f(int i) => x = i;
        }
        // イテレーターの引数に使えない
        IEnumerable Iterator(ref int x){
            yield break;
        }
        // 非同期メソッドの引数に使えない
        async Task Async(ref int x){
            await Task.Delay(1);
        }
        */

        //In引数===============================================================================================
        public ref readonly int InFunc(in int val = 100/*既定値可能*/){
            //n = 100;//readonlyなので書き換え不可
            return ref val;//in == ref readonlyなのでref readonlyはref readonlyとして返せる
        }

        //Out引数===============================================================================================
        public ref int OutFunc(out int val /*= 4//既定値不可*/){//out == 代入が必要なref
            val = 100;//必ず代入が必要
            return ref val;
        }

        //ref引数===============================================================================================
        public ref int RefFunc(ref int refArg){
             refArg += 100;
             return ref refArg;   
        }

        //refルール===============================================================================================
        public ref int RefRuleFunc(ref int refArg){
             int loc = 100;
             //refArg = ref loc;//関数内で生成した変数を引数の参照に再代入できない
             ref int refLoc = ref loc;
             //return ref refLoc;//関数内で生成した変数を参照で返せない
             return ref RefFunc(ref refArg);//引数で渡した参照をそのまま参照で返す関数ならおけ   
        }

        //戻り値がrefの関数への代入================================================================================
        public int field = 100;
        public ref int RetFieldFunc()=> ref field;//クラスはフィールドメンバ変数の参照を返せる

        //構造体の中身のrefを返す==================================================================================
        public ref int TupleFunc(ref (int A, int B) s){
            s.A += 100;
            return ref s.A;
        }

        //参照型(ヒープ)を介すれば値型のメンバのrefを返せる===========================================================
        private struct Struct{public int A, B;}//クラスでもおけ
        //構造体だけどc.StructFunc(){@struct.A}のように参照型のクラスCを介しているので参照を返せる
        //(クラスC(ヒープ)の中に構造体Struct(値型)がある)
        private Struct @struct = new Struct();
        public ref int StructFunc(){
            @struct.A += 100;
            return ref @struct.A;
        }
    }

    public static class Program {
        public static void Main() {
            {
                Console.WriteLine("ref代入===============================================================================================");
                int loc0 = 1;
                ref int refLoc = ref loc0;//==========refLoc0への代入
                refLoc++;
                Console.WriteLine($"loc0:{loc0}, refLoc:{refLoc}");

                Console.WriteLine("\n"+"ref再代入==============================================================================================");
                int Loc1 = 2;
                refLoc = ref Loc1;//ref再代入//=======refLoc0への再代入
                Loc1++;
                Console.WriteLine($"loc0:{loc0}, Loc1:{Loc1}, refLoc:{refLoc}(再代入)");
            }
            {
                Console.WriteLine("\n"+"ref readonly代入===========================================================================================");
                int loc2 = 4;
                ref readonly int refReadLoc0 = ref loc2;
                //refReadLoc0 = 4;//readonlyへの代入不可
                ref readonly int refReadLoc1 = ref refReadLoc0;
                //参照で繋がっているのでref readonlyはref readonlyを維持して代入する
                Console.WriteLine($"loc2:{loc2}, refReadLoc0:{refReadLoc0}, refReadLoc1:{refReadLoc1}");
            }
            {
                Console.WriteLine("\n"+"クラスCを使用|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
                var c = new C();

                {Console.WriteLine("\n"+"In引数===============================================================================================");
                    int inVal = 8;    
                    //in引数は参照渡しも値渡しも可能(IFunc(⟪in ∫vari∫¦∫vari∫¦∫式∫¦Ø⟫)『最後は既定値使用』)
                    ref readonly int retVal0 = ref c.InFunc(in inVal/*in*/);//=> 8
                    ref readonly int retVal1 = ref c.InFunc(inVal/*in省略可能*/);//=> 8
                    ref readonly int retVal2 = ref c.InFunc(1024/*∫Lit∫を渡すとrefでは無くなる*/);//=> 1024
                    ref readonly int retVal3 = ref c.InFunc(/*既定値使用*/);//=> 100
                    Console.WriteLine($"inVal:{inVal}, retVal0(in):{retVal0}, retVal1(in省略可能):{retVal1}, retVal2(∫Lit∫):{retVal2}, retVal3(既定値使用):{retVal3}");
                }
                {Console.WriteLine("\n"+"Out引数===============================================================================================");
                    int outVal0 = 16;     //Out引数は参照渡しして呼び出し先で必ず代入される
                    ref int retVal0 = ref c.OutFunc(out outVal0);//=> 100
                    //int outVal1;               ←があるのと同じ↓
                    ref int retVal1 = ref c.OutFunc(out int outVal1);//=> 100
                    Console.WriteLine($"outVal0:{outVal0}, retVal0:{retVal0}, retVal1:{retVal1}");
                }
                {Console.WriteLine("\n"+"ref引数===============================================================================================");
                    int refVal = 32;     //ref引数は単にIn,Out引数の制限が無い版
                    ref int returnVal = ref c.RefFunc(ref refVal);//=> 132 
                    Console.WriteLine($"refVal:{refVal}, returnVal:{returnVal}");
                }
                {Console.WriteLine("\n"+"refルール===============================================================================================");
                    int val = 64;
                    ref int retVal = ref c.RefRuleFunc(ref val);//=> 164
                    Console.WriteLine($"val:{val}, retVal:{retVal}");
                }
                {Console.WriteLine("\n"+"戻り値がrefの関数への代入================================================================================");
                    int val = 128;
                    Console.WriteLine($"c.field:{c.field}(c.RetFieldFunc() = val;前)");
                    c.RetFieldFunc() = val;
                    Console.WriteLine($"c.field:{c.field}(c.RetFieldFunc() = val;後)");
                }
                {Console.WriteLine("\n"+"構造体の中身のrefを返す==================================================================================");
                    (int A, int B) tuple = (256, 512);
                    Console.WriteLine($"tuple:{tuple}");
                    ref int retA = ref c.TupleFunc(ref tuple);
                    Console.WriteLine($"tuple:{tuple}, retA:{retA}");//=>retA:356//構造体のrefの中身のrefを返せる
                }
                {Console.WriteLine("\n"+"参照型(ヒープ)を介すれば値型のメンバのrefを返せる===========================================================");
                    ref int refStruct = ref c.StructFunc();
                    Console.WriteLine($"@struct.A:{refStruct}");
                }
            }
        }
    }
}
```

## readonly struct

### [readonly構造体]============================================================================================

```C#
using System;

namespace readonly構造体{
    public struct S{
        public int A;
        public int B;
        public void M()=> A = B = 4;
        public static int @static;
        public readonly void RM() => @static = 4; //public readonly void RM1()=> A = B = 4;
        public readonly void RM2(){@static = 4; M();}
        //8.0//readonlyメソッドはメソッド内のフィールドへのアクセスをreadonlyとして扱い防衛的コピーをメソッド単位で防ぐ
            //しかし、メソッドの中でreadonlyメソッドでは無いメソッドを呼ぶと防衛的コピーが発生するreadonly構造体
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
        public static void Main() {

            //readonlyなメンバの値型(構造体)にその内部を変更しうるメソッドを呼ぼうとすると
            //防衛的コピー((t=s).M())が起きる
            s.M(); Console.WriteLine($"s.A:{s.A}, s.B:{s.B}");
            
            rs.M1();//RSはreadonly structのため防衛的コピーは発生しない

            //readonly struct や readonlyメソッドは防衛的コピーを防ぐだけで副作用は発生しない
            //コピーコストを無くすために付けれるなら付けたほうがいいと思う
            //readonly変数 から ⟪NoReadonlyMethod(8.0)¦NoReadonlyStructMethod⟫ を呼ぶ時防衛的コピーが発生する
        }
    }
}
```

## イテレータ

### [イテレータ]============================================================================================

```C#
using System;

namespace イテレータまとめ{
    using System.Collections.Generic;
    using System.Collections;
    
    public class EasyIter{//foreach文はIEnumerableを実装しなくても動く。ダックタイピング？
        public EasyIter GetEnumerator()=> this;
        int counter = 0;
        public bool MoveNext()=> counter++ < 3? true : false;
        public int Current{get{return counter;}}
    }

    public class Iter<T>:IEnumerator<T>{//IEnumerator<T>はMoveNext,Current(ジェネ,非ジェネ),Reset,Disposeを実装
        T[] table{get;}
        int counter = -1;
        public Iter(T[] table)=> this.table = table;
        
        public bool MoveNext(){//IEnumerator(非ジェネ)のメソッド
            if(counter < table.Length - 1){
                counter++;
                return true;
            }else return false;
        }
        
        public T Current{get{return table[counter];}}//Currentジェネ
        
        object IEnumerator.Current{get{return Current;}}//Current非ジェネ
        
        public void Reset(){counter = -1;}//IEnumerator(非ジェネ)のメソッド
        public void Dispose(){}
    }

    public class Table<T>:IEnumerable<T>{//IEnumerable<T>はGetEnumeratorのジェネと非ジェネを実装する
        T[] table{get;}
        public Table(T[] table)=> this.table = table;
        
        public IEnumerator<T> GetEnumerator(){//ジェネ
            return new Iter<T>(table);
            //yield return default(T); //コンパイル結果: https://ufcpp.net/study/csharp/sp2_iterator.html
            //戻り値がIEnumera⟪ble¦tor⟫＠❰<T>❱であるメソッドに、
            //yield ⟪return ∫式∫¦break⟫を書くとイテレータメソッドになる。
            //メソッド内で使われる変数は、ローカル変数:メンバ変数として持つ、フィールド:this経由アクセス
            //静的変数:直接、というふうにイテレータメソッドが返すオブジェクトが必要な時に素直にアクセスする
        }
        
        IEnumerator IEnumerable.GetEnumerator(){//非ジェネ//IEnumerable<T>にデフォルト実装できそうな気がする
            IEnumerator enumerator = (IEnumerator)GetEnumerator();
            return enumerator; //IEnumerator<T>をIEnumeratorにアップキャスト！
        }     
    }

    public class C {
        public static void Main() {
            Console.WriteLine("EasyIterのforeach");
            EasyIter eiter = new EasyIter();
            foreach(int n in eiter){Console.WriteLine(n);}
            
            Console.WriteLine("\nTableのforeach");
            Table<int> table = new Table<int>(new int[]{1, 2, 3});
            foreach(int n in table){
                Console.WriteLine(n);
            }
            
            Console.WriteLine("\nTableのiterを手動で回す");
            IEnumerator<int> iter = table.GetEnumerator();
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine("iter.Reset()");
            iter.Reset();
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            
        }
    }
}
```

- Linq

### [Linqの仕組み]============================================================================================

```C#
using System;

namespace Linqの仕組み{
    using System.Collections.Generic;
    //using System.Linq;

    public static class Ext{//IEnumerableと取ってIEnumerableを返す
        public static IEnumerable<T> Select_<T>(this IEnumerable<T> source, Func<T, T> filter){
            foreach (var x in source){//ここで引数のIEnumerableの要素が取り出される
                yield return filter(x);//遅延的に出力できる
            }
        }//Select_はfilterをIEnumerableに持ち上げた関手(ファンクター)
    }

    public static class Program {
        public static void Main() {
            List<int> list = new List<int>{1, 2, 3, 4};

            Console.WriteLine("デフォルト");
            foreach(int n in list){
                Console.WriteLine(n);
            }

            Console.WriteLine("\n自作Linqの拡張Select_メソッド");
            foreach(int n in list.Select_(n => n * 100)){
                Console.WriteLine(n); //this IEnumerable<int> sourceによってSelect_<int>に型推論され省略可能
            }
        }
    }
}
```

## usingステートメント

### [usingステートメント]============================================================================================

```C#
using System;
namespace usingステートメントまとめ{
    //========================================================================================
    class _{
        void __(){//usingは例外処理の糖衣構文
            //==========================================================================
            using(Resource r0 = new Resource("")){//using (⟪∫式∫¦obj as IDisposable⟫){}でもいい
                /*リソースに対する操作*/
            }
            //↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            Resource r1 = /*式*/ new Resource("");
            try{
                /*リソースに対する操作*/
            }
            finally{
                if(r1 != null)((IDisposable)r1).Dispose();
            }
            //==========================================================================
            
            using(new _() as IDisposable){}//using(❰＃nullかIDisposableのインスタンスを返す∫式∫＃❱){}
            //ref構造体だけ特別にIDisposableを継承しなくてもパターンベース(ダックタイピング?)で使える
        }
    }
    
    //========================================================================================
    
    public class Resource:IDisposable{
        private bool alive{get;set;} = true;
        public string name{get;}
        public Resource(string name){this.name = name;}
        public void Alive(){if(alive)Console.WriteLine(name + "、生存なう");
                            else Console.WriteLine(name + "脂肪");}
                                                        //↑死んだ後はスコープから外れてるから言えない
        void IDisposable.Dispose(){Console.WriteLine(name + "、昇天なう");alive = false;}
    }

    public static class Program {
        public static void Main() {
            try{
                Console.WriteLine($"using {nameof(Resource)} resource0 = new {nameof(Resource)}(resource0);");
                using Resource resource0 = new Resource(nameof(resource0));
                
                //変数に代入しなくてもIDisposeは呼ばれる。ILでは、代入しない場合でも適当な変数が作られる
                Console.WriteLine("using (new Resource(\"変数に代入しない\")){Console.WriteLine(\"処理\");}");
                using(new Resource("変数に代入しない")){Console.WriteLine("処理");}
                
                //nullだった場合、finallyでnull判定されIDisposeは呼ばれない。
                using(RetNullDisp()){} static IDisposable RetNullDisp(){return null;}
                
                //using変数宣言//ブロック{}がスコープ全体になるので注意
                resource0.Alive();
                Console.WriteLine($"using({nameof(Resource)} resource1 = new {nameof(Resource)}(resource1)){{");
                using(Resource resource1 = new Resource(nameof(resource1))){
                    resource0.Alive();
                    resource1.Alive();
                }
                Console.WriteLine("}");
                resource0.Alive();
                //throw new Exception();//AliveもDisposeも呼ばれているはずだけど表示されない
            }
            catch{
                Console.WriteLine("例外が発生しました");
            }
        }
    }
    //========================================================================================
}
```

## dafault(T)

### [default]============================================================================================

```C#
using System;

namespace defaultまとめ{
    public struct Struct{int n;}
    class Program{
        static int F(int n) => default; //戻り値おけ
        static void Main(string[] args){
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
```

## ref構造体

### [ref構造体]============================================================================================

```C#
using System;

namespace ref構造体まとめ{
    //public struct Struct{ref_Struct_1 str1;} //普通の構造体はref構造体を持てない
    public class Class{public int n; public string s; /*ref_Struct_2 str2;*/}
    public readonly ref struct ref_Struct_2{                        //↑クラスもだめ
        readonly Span<int> span;
        public void Assign_this(ref_Struct_2 rs2){}//=> this = rs2;
    }
    //↓はAssign_thisで中のref(&)を書き換える危険性があるが↑のreadonly refは無いのでメソッド呼び出しが安全でコンパイルが通る
    public ref struct ref_Struct_1{
        public void Assign_this(ref_Struct_1 rs1)=> this = rs1;
        ref_Struct_2 str2;
        Class cls;
        string s;        //ref構造体は参照型が置けない訳ではない
    }
    public ref struct ref_Struct_0{
        ref_Struct_1 str1;    //ref構造体はref構造体のメンバに置ける。そして再帰的にそうである必要があるため
        Span<int> span;       //ref構造体は常にスタックに存在する。Span<T>構造体もref構造体でそのメンバに
                              //ref T Reference;のようなメンバを特別に持っていて、ref(&)の参照型は
                              //常にスタックに存在しなければならないが、ref構造体は常にスタックに存在する
                              //という性質によって合法になっている。        ↑「stack-only 型」
                              //ref構造体はref(&)を持っているのでrefルールの制約がある
    }

    public class C {
        // ちゃんと「メモリ確保」があったかどうかを見てる
        // 同じようなコードでもこれは OK (default だと何も確保しない)
        private static Span<int> Success1(){
            Span<int> x = default;
            //ref int n = default;//ref(&)はできない
            return x; //Span<T>(ref)だけどdefaultだから返せる
        }
        
        public void M() {
        
        //謎エラー https://twitter.com/aetos382/status/1227278725722460160
        //CS8353：タイプ'Span <int>'のstackalloc式の結果は、包含メソッドの外部に公開される可能性があるため、このコンテキストでは使用できません
        //Span<int> span = default; //Span<T>はdefault以外のstackallocなどで初期化しないとエラーになる
        Span<int> span = stackalloc int[2];//ref構造体の中にSapn<T>がある場合、.ctor内で初期化しないといけない
        span = default;         //初期化以外でdefaultを入れてもエラーにならない(謎)
        span = stackalloc int[4];
        }
    }
}
```

- Sapn構造体

### [SpanT]============================================================================================

```C#
using System;

namespace SpanTまとめ{
    public struct Struct{public int n;}
    public static class Program {
        public static void Main() {
            //基本的な使い方
            int[] arr = new int[10]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            Span<int> span = arr.AsSpan(); //AsSpanは拡張メソッド(static Span<T> AsSpan<T>(this T[]? array);)
            span = span.Slice(2, 6);

            Console.Write("for: ");
            for(int i = 0; i < span.Length; i++) Console.Write(span[i] + ","); //=> for: 2,3,4,5,6,7,
            Console.Write("\nfoeach: ");
            foreach(int n in span) Console.Write(n + ",");                     //=> foeach: 2,3,4,5,6,7,
            
            span = span.Slice(2, 4);//4以上はArgOutOfRangeExp。多分、Sliceした時にアクセスできる範囲が縮小した
            Console.Write("\nfor: ");
            for(int i = 0; i < span.Length; i++) Console.Write(span[i] + ","); //=> for: 4,5,6,7,
            
            Console.WriteLine();
            

            //ReadOnlySpan<T>とSpan<T>
            ReadOnlySpan<int> rspan; //ref readonly T this[int index]
            Span<int> span1;         //ref T this[int index]
            

            //色々なSpan<T>の作り方 //大体、配列(System.Array(参照型))かstackalloc の二択
            _ = new Span<int>(new int[10], 2, 6);

            int[] d = new int[6]{0, 2, 3, 4, 5, 6};
            Span<int> sd = d.AsSpan();      //⟪var¦Span<int>⟫ sd = d＠❰.AsSpan()❱;
            var sd1 = d.AsSpan(); //型推論
            ReadOnlySpan<int> sd2 = d; //暗黙的型変換演算子により変換される(implicit operator ＠❰ReadOnly❱Span<T>)
            //色々入る
            ReadOnlySpan<char> span_str = "abcdefghigklmn".AsSpan(2, 6); //文字列はReadOnlySpan<char>
            Span<int> span_stackalloc = stackalloc int[10]; //unsafe外でint*型だけど入る
            Span<Struct> Span_Struct = new Struct[10].AsSpan(2, 6); //∫Complex∫おけ

            
            unsafe{  //Span<T>のイメージ(struct Span<T>{ref T ref_; int length;})
                int* array = stackalloc int[10]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

                //本当にメモリのアドレスと長さのペア
                ulong ref_ = (ulong)&array[0] + sizeof(int) * 2;
                int length = sizeof(int) * 6;

                Console.Write("ref_ < ref_+length: ");
                for(ulong i = ref_; i < ref_ + (ulong)length; i += sizeof(int)) Console.Write(*(int*)i + ",");
                //3000以上の変な所にアクセスするとエラる          //=> ref_ < ref_+length: 2,3,4,5,6,7,
                    //CLR / System.AccessViolationException: 保護されたメモリの読み取りまたは書き込みを試みました。 
                    //これは多くの場合、他のメモリが破損していることを示しています。

                Console.Write("\nspan_: ");
                Span<int> span_ = new Span<int>((void*)ref_, length / sizeof(int));
                foreach(int n in span_) Console.Write(n + ",");//=> span_: 2,3,4,5,6,7,

                Console.WriteLine();
            }
        }
    }
}
```

## ポインタ操作

### [ポインタ]============================================================================================

```C#
using System;

namespace ポインタまとめ{ //https://ufcpp.net/study/csharp/sp_unsafe.html
    unsafe public interface IRef{
        public static int n;
        public static int* nptr;
    }
    unsafe public class Ref{
        public int n;
        public int* nptr;            //∫Complex∫はポインタを持てる
    }
    unsafe public struct Val<T>where T: unmanaged{ //unmanaged制約を付ける事で値型扱いできる(8.0で再帰的に)
        public int n;
        public int* nptr;
        public fixed byte fb[4]; //固定長バッファ(値型の基本型のみ)//stackallocの固定長版?
    }
    public class Prog{
        public static void Main(){
            unsafe{//∫Complex∫やメソッドやブロックにunsafeを付けれる。(unsafeを付けたブロック内は全てポインタ操作可能)
                    //それとunsefeを実行するにはコンパイル時に/unsafeオプションを付ける必要がある。
                    //ポインタ操作演算子: &,*,+,-,->,[],fixed(~){},sizeof(∫Type∫⇒∫ValueType∫),stackalloc
                    //ポインタは何処にでも存在できるが値型(スタック)しか指せない(操作できない)
                    //構造体,値型,アンマネージ型,スタック ¦¦ クラス,参照型,マネージ型,ヒープ
                ulong ul = 18_446_744_073_709_551_615;//2^64-1
                byte* bptr = (byte*)&ul; //ulongのアドレスをbyte*に強制変換//[1] uint8* bptr//&
                Console.WriteLine($"bptr[sizeof(ulong)-1]: {bptr[sizeof(ulong)-1]}");
                Console.WriteLine($"bptr[sizeof(ulong)-1] == *(bptr + sizeof(ulong)-1): {bptr[sizeof(ulong)-1] == *(bptr + sizeof(ulong)-1)}");//[],+,*)
                
                int i;
                int* iptr = &i;
                int** iptrptr = &iptr;
                Console.WriteLine($"&iptr: {(ulong)&iptr}, iptr: {(ulong)iptr}, *iptr: {(ulong)*iptr}, **iptrptr: {(ulong)**iptrptr}");

                ref int ri = ref i; //[4] int32& ri, [13] int32& pinned, [12] int32* riptr
                fixed(int* riptr = &ri){}//参照型(ref)の先が値型(アンマネージ型)の場合、fixedで固定すれば操作できる(ヒープには無い気がする)
                    //riptr = pinned = &riをしている(pinnedに参照されていると動かされない?)
                
                string s = default;
                ref string rs = ref s;
                //fixed(string* rsptr = &rs){} //参照型(ref)の先がクラスの場合はポインタ操作不可
                fixed(char* c = rs){} //string(参照型)の中のchar(値型)を固定すればポインタ操作可能
                                      //fixedでstringの中のcharのアドレスが入るようコンパイラの補助がある
                                      //fixedはGetPinnableReference()で、どのアドレスを返すか(入れるか)ユーザー定義できる
                                          // fixed (∫Type∫* p = &a.GetPinnableReference()) に展開される。
                
                Ref r = new Ref();
                //Ref* rptr; _ = &r; //クラスはポインタ操作不可

                Val<int> v = new Val<int>();
                Val<int>* vptr = &v; //構造体はポインタ操作可能
                Console.WriteLine($"vptr->n == (*vptr).n && (*vptr).n == v.n: {vptr->n == (*vptr).n && (*vptr).n == v.n}");//->

                    //int* nptr = &r.n;
                fixed(int* nptr = &r.n){} //参照型(ヒープ)の中の値型をポインタ操作したい場合はヒープのアドレスをfixedで固定する必要がある

                int[] iarr = new int[]{1, 2, 3, 4}; //配列は参照型    //↓これもコンパイラの補助がある?
                fixed(int* iarrptr = iarr){} //C,C++と同じでiarrは配列全体のアドレスを指す?   ←&[1,2,3,4]   ↓&[1],2,3,4
                fixed(int* iarrptr = &iarr[0]){} //参照型なので↑↑と同じようにできる//要素が一つも無い場合(new int[0]の時)エラーがでる
                
                int* si = stackalloc int[]{1, 2, 3, 4};//多分一次元配列のみ。スタックに配列を作る(構造体のようなもの?)
                Console.WriteLine($"si[1]: {si[1]}");
            }
        }
    }
}
```

## 名前空間

### [名前空間]============================================================================================

```C#
using System;
//https://ufcpp.net/study/csharp/sp_namespace.html
//global::名前空間1.名前空間2...名前空間n.∫Complex∫1.∫Complex∫2...∫Complex∫n.メンバ1.メンバ2...メンバn
//名前空間（name space）とは、∫Complex∫を種類ごとに分けて管理するための機構です。
//===================================================================================================
namespace 見えてる範囲{
    //識別子(クラスとか)もusingもスコープはそのnamespaceブロック({})の全体(変数のスコープと同じ)
    //using A.B.C;はちょうどそのnamespaceだけ見える(見えると言うより省略できるだけ)
        //namespaceの途中まで省略できるわけでは無い(✖❰using A.B; new C.Class()❱)
    //↓は↑にまとめた。適当にぐるぐる考えた見たいだから分かりにくい↓
    namespace A_mieru{
        class CA{
            void F(){new CA();new B.CB();new B.C.CC();}
        }
        namespace B{
            class CB{
                void F(){new CA();new CB();new C.CC();}
                         //new C.CB_();}//外側から内側のusing A_using.B_;は見えていない*/
            }
            namespace C{
                using A_using.B_;
                class CC{
                    void F(){new CA();new CB();new CC();
                             new CB_();
                             /*new CC_();外部のusing C_;は見えていない*/
                             /*new CA_();外部の内側は見えていない(局所的)*/}
                }
            }
        }
    }
    namespace G{class CG{}}
    namespace A_using{//usingは外部から内部のある階層を局所的に見ることができる(usingは見えてない)。
        using B_;
        using B_.C_;//A_mieruの様に今の階層の位置から名前空間を繋げる
        class CA_{
            void F(){new CA_();new CB_();new CC_();/*new CG();*/
                    }//new C_.CC_();}//using B_;を利用してnew C_.CC_();とすることはできない。*/
        }
        namespace B_{
            using C_;
            using G;
            class CB_{
                void F(){new CA_();new CB_();new CC_();new CG();}
            }
            namespace C_{
                class CC_{
            //usingは変数のスコープの様にその名前空間ブロック内のみ有効{using G; ここは有効{ここも有効}}
                    void F(){new CA_();new CB_();new CC_();new CG();}
                }
            }
        }
    }
}
//===================================================================================================
namespace 基本的な使い方{
    //extern alias X; //外部エイリアス//"csc /r:X=Lib.dll Test.cs" とコンパイルするとLib.dllのglobalを
                    //Xとして、X::❰＃Lib.dll内の要素＃❱と指せる。あとnamespaceの先頭である必要がある
    using A;
    using A.B;
    using Alias = A.B.ClassB;//エイリアス//名前空間~∫Complex∫に別名を与える
    using Alias1 = A.B;
    using static A.B.ClassB; //∫Complex∫まで省略し静的メンバを直接書ける
                         //usingと区別する理由はnamespaceと同名の∫Complex∫を定義して破壊しないようにするため
    namespace usingの使用{
         class usingの使用{//階層違いで同名の識別子を使用可能
             void F(){
                 _ = new ClassA(); //using A;によりA.ClassAのAを省略できる
                 _ = new ClassB(); //using A.B;によりA.B.ClassBのA.Bを省略できる
                 _ = new Alias(); //using Alias = A.B.ClassB;によりA.B.ClassBの別名Aliasを使った
                                  //Aliasがクラスだと出てくるのでクラスの別名だと気づきにくい
                 _ = new Alias1.ClassB(); _ = new Alias1::ClassB(); //"::"で直前がエイリアスであることを明示できる
                 _ = ClassBの静的メンバ; //using static A.B.ClassB;により∫Complex∫まで省略し静的メンバを直接書ける
                 _ = new global::基本的な使い方.A.B.ClassB(); //完全修飾名//globalもエイリアス
             }
         }   
    }
    namespace A{
        //using C;//CS0246//↓↓↓にも書いてあるがnamespace A{}"直下"にnamespace C{}が無いのでエラーになる
        class ClassA{}
        namespace B{
            class ClassB{public static int ClassBの静的メンバ = 0;}
            class ClassX{}
            namespace C{
                
            }
        }
    }
    namespace A.B{//ネストになっているnamespaceをA.Bというふうに書く事ができる
        //class ClassX{} //CS0101: 名前空間「基本的な使い方.A.B」には、すでに「ClassX」の定義が含まれています
            //namespace A{namespace B{"ココ"}} と namespace A.B{"ココ"} のnamespaceは同じnamespaceです
    }
    namespace B{class ClassX{}}//namespace A.B{}とnamespace B{}は別のnamespaceなのでclass ClassX{}を定義できる
}
//===================================================================================================
namespace usingとnamespace{
    namespace usingのnamespaceの存在{//namespace内で存在しないnamespace名や∫Complex∫をusingすることはできない
                                    //これによってusing System;とか書けると言うことは、そのnamespaceが定義
                                    //されたアセンブリがコンパイル時に読まれている事を意味していると思う
        
        //using 存在しないnamespace名 //CS0246:タイプまたは名前空間名 '存在しないnamespace名'が見つかりませんでした
    }
    namespace 順序{//namespace内でusing.., namespace..の順序でないとだめ(externエイリアス宣言を除いて)
        namespace おけ{
            using _;
            namespace _{} 
        }
        namespace だめ{
            namespace _{} //↓多分、namespaceの先頭(extern aliasの次)でないとだめ
            //using _; //CS1529: namespaceで定義されている他のすべての要素の前に置く必要があります
        }
    }
}
//===================================================================================================
namespace namespaceの優先度{
    using static System.Console;
    using A;
    using Lib = C.Lib;
    class Lib {public static void F() => WriteLine("global");}
    namespace A{class Lib{public static void F() => WriteLine("A");}}
    namespace B{class Lib{public static void F() => WriteLine("B");}}
    namespace C{class Lib{public static void F() => WriteLine("C");}}
    namespace MyApp{
        using B; 
        class Lib {public static void F() => WriteLine("MyApp");}
        class Program{
            static void Main(){
                //{namespaceのブロック内}: 直接 ↔ エイリアス → using
                //    ↓
                //{namespaceのブロック外}: 直接 ↔ エイリアス → using
                Lib.F();//=> MyApp
                // ちゃんと呼び分けたければフルネームで書く
                A.Lib.F();
                B.Lib.F();
                C.Lib.F();
                MyApp.Lib.F();
                global::namespaceの優先度.Lib.F();
            }
        }
    }
}
//===================================================================================================
```

## 例外処理

### [例外処理まとめ]============================================================================================

```C#
using System;

//===========================================================================================
public class ExprThrow{//throw式//↓これ以外の文脈でthrow式を書くことはできません。
    // 式形式メンバーの中( => の直後)
    static void A() => throw new NotImplementedException();

    static string B(object obj){
        // null 合体演算子(??)の後ろ
        var s = obj as string ?? throw new ArgumentException(nameof(obj));

        // 条件演算子(?:)の条件以外の部分
        return s.Length == 0 ? "empty" :
            s.Length < 5 ? "short" :
            throw new InvalidOperationException("too long");
    }   
}
//===========================================================================================
public static class Program {
    public static int X(){/*if(false)*/ throw new Exception("くらえ");}
        //例外をthrowすることが確実な場合は戻り値が任意
    public static void RethrowKind(){
        try{
            X();
        }
        catch (Exception e){//Xメソッドで起きた例外を再スローする
            throw; //catchした例外のインスタンス(e)を触らずにそのまま再スロー(スタックトレースが書き換わらない)
                    //IL: rethrow
            //throw new Exception("例外を例外で包む", e); //catchした例外(e)を生成した新たな例外に包んで返す
                                                    //IL: throw//eはInnerExceptionプロパティで取り出せる
            //throw e; //IL: throw//スタックトレースが上書きされて本来の例外の発生源が消えてしまう
        }
    }
    public static void ExcProc() {//例外処理でメソッドの入れ子を崩すと↓こんなイメージ
        try{
            try{
                RethrowKind();
                
            }
            catch{
                throw;
            }
        }
        catch(Exception e)　when(e.Message == "くらえ"){//when句でさらに条件で例外を選択することができる
            Console.WriteLine("ぐはぁ");
            throw;
        }
        catch(Exception e)　when(e.Message == "例外を例外で包む"){//when句でさらに条件で例外を選択することができる
            Console.WriteLine("↓の " + e.Message);
            //Console.WriteLine(e.InnerException.Message);
            //InnerExceptionの参照がsharplabだとうまく動かない
            throw;
        }
        finally{
            Console.WriteLine("このまま例外を通すわけにはッ！(finally)");
        }
    }
    public static void Main(){ExcProc();}
}
//===========================================================================================
```

## インライン化

## ポインタとref

### [Pointer_test]============================================================================================

```C#
using System;
using System.Runtime.CompilerServices;
namespace Pointer_test{
    //クラスの場合c.d0は(*c).d0でc->d0だった
    //クラスは
    //[＄c=❰[addr]❱] -> [＄*c=❰[＄(*c).d0=❰[d0]❱,＄(*c).d1=❰[d1]❱,..]❱] (c.~とすると(*c).~になる所はrefみたいで、cは*cにならずcのまま)
    //構造体は
    //[＄s=❰[＄s.d0=❰[d0]❱,＄s.d1=❰[d1]❱,..]❱]
    //かな?

    //S s = new S(); と S* ps = &s; と C c = new C(); で
    //❰s¦c❱.d, (*ps).d, ps->d を試したが
    //全て ldfldのみ だった (代入の場合は stfldのみ)
    //❰ld¦st❱fld は obj と *obj を区別せず受け取り、中で場合分けをしている?

    //ref ∫Type∫ r = ref a ⇔ ∫Type∫* r = &a (ref a(aは非ref) ⇔ &a)
    //r ⇔ *r
    //なし ⇔ r
    //なし ⇔ &r
    //↑↑と↑は"r"が出現すると"*r"に解釈されるので表現できない。つまりrefは自分自身のアドレスとデータ(参照先)を表現できない
        //そもそもIL的にO型は不透明でポインタ操作ができない
    //よって"ref r"をポインタで表すと、ref r ⇔ ref (*r) ⇔ &(*r) ⇔ r    (IL: ldloc r)
    //"ref a(aは非ref)"だと、ref a ⇔ &a になるかな?                     (IL: ldloca a)
    //refは自分自身を表現できず、"ref r"は初期化時のみ使用できるため、構文的にrefの参照先を共有することはできてもrefはrefを指せない

    //ref再代入は r1 = ref r0 でなく ref r1 = ref r0 であるべき?("r1"は"*r1"になってしまう)
        //でも、"ref r1"は&(*r1)で最後が"&"で&∫vari∫は定数なので ❰定数❱ = ~ になってしまうため
        //ターゲット側にrefがある場合はr1はr1のままであると言う解釈にした?

    //https://www.youtube.com/watch?v=3NMUJdZIdQM&list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&index=1  
    class D{}
    class C{
        public float d0 = 123.456f;
    }
    class Main_{
        unsafe public static void Main(){
            C c = new C();
            ref C rc = ref c;
            rc = ref c;
            //var arc = &rc; //(CS0208)マネージ型 ('❰❰❰❰C❱❱❱❱') のアドレスの取得、サイズの取得、またはそのマネージ型へのポインターの宣言が実行できません [C:\python学習メモ\==C#学習メ
            void** cp = (void**)Unsafe.AsPointer(ref rc);
            float* fp = (float*)((byte*)*cp + 8);           //c.d0 ⇔ (*c).d0 ⇔ c->d0 (c == rc, d0 == fp)
             /* ldloc.s cp    :cp
                ldind.i       :*cp
                ldc.i4.8      :*cp   8
                add           :*cp + 8
                stloc.s fp    :fp = *cp + 8
                //キャストが無くなっている*/
            Console.WriteLine($"*fp: {*fp}, c.d0: {c.d0}");
            *fp = 11.22f;
            Console.WriteLine($"*fp: {*fp}, c.d0: {c.d0}");
            c.d0 = 22.33f;
            Console.WriteLine($"*fp: {*fp}, c.d0: {c.d0}");

            //動的な型?(sharplabのtype handle)を変えようとしたがうまくいかなかった
            Console.WriteLine($"c.GetType().ToString(): {c.GetType().ToString()}, c.GetType(): {c.GetType()}, c: {c}");
            D d = new D();
            uint* td = (uint*)(*(byte**)Unsafe.AsPointer(ref d) + 4);
            uint* tc = (uint*)((byte*)fp - 4);
            Console.WriteLine($"tc: {*tc}, td: {*td}, sizeof(uint): {sizeof(uint)}");
            *tc = *td;
            Console.WriteLine($"c.GetType().ToString(): {c.GetType().ToString()}, c.GetType(): {c.GetType()}, c: {c}");
            //int (*pa)[];
            //int* ap[] = new int*[4];
        }
    }
}
/*sharplab
using System;
using System.Runtime.CompilerServices;
public class D{public int d = 1234;}
public struct S{public int d;}
unsafe public class C {
    public void M() {
        int n = 0;
        int* p = &n;
        ref int r0 = ref n;
        ref int r1 = ref r0;
        int* p1 = p;
        
        D d = new D();
        void** cp = (void**)Unsafe.AsPointer(ref d);
        float* fp = (float*)((byte*)*cp + 8);
        
        S s = new S();
        S* ps = &s;
        int m0 = s.d;
        int m1 = ps->d;
        int m2 = (*ps).d;
        int m3 = d.d;
        ;
        s.d = m0;
        ps->d = m0;
        (*ps).d = m0;
        d.d = m0;
    }
}
*/
```

### [Pointer_test補足]============================================================================================

```C#
using System;
using System.Runtime.CompilerServices; //効かない

class Class{public int x; public void F(){}}
struct Struct{public int x; public void F(){}}

class M{
    static void Main(){
        Class c = new Class();
        Struct @struct = new Struct();
        int a;
        a = c.x;
        a = @struct.x; //メソッド呼び出し(s.F())はldloca.s 1なのにldfldはldloc.1でアドレスをldしていない
        c.F();
        @struct.F();


        unsafe{/* //using System.Runtime.CompilerServices;が効かない
            int x = 1;
            void* pointer = Unsafe.AsPointer(ref x);
            *(int*)pointer = 2;

            Console.WriteLine(x); // 2 になってる

            ref int r = ref Unsafe.AsRef<int>(pointer);
            r = 3;

            Console.WriteLine(*(int*)pointer); // 3 になってる
        */}
    }
}

```

## StructLayout

### [StructLayout]============================================================================================

```C#
/*
  SharpLab tools in Run mode:
    • value.Inspect()
    • Inspect.Heap(object)
    • Inspect.Stack(value)
    • Inspect.MemoryGraph(value1, value2, …)
*/
//https://youtu.be/3NMUJdZIdQM?list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&t=1185
//https://ufcpp.net/study/csharp/interop/memorylayout/#explicit-layout
//クラスのフィールドデータ部分(headerとtype handle以外)と
//構造体のフィールドデータ部分(それ以外ない)の違いは
//構造体がLayoutKind.Sequentialで、クラスがLayoutKind.Autoの違いだけ
//つまりクラスは構造体をLayoutKind.Autoにした構造体を含んでいる(構造体(Auto)⊂クラス)
namespace StructLayout{
    using System;
    using System.Runtime.InteropServices;

    //構造体
    [StructLayout(LayoutKind./*％❰*/Sequential/*❱*/)]
    struct SequStr{int i; long l; byte b;}//一番大きい型のサイズで宣言順に配置される

    [StructLayout(LayoutKind.Auto)]
    struct AutoStr{int i; long l; byte b;}//一番大きい型のサイズから詰めて配置される

    [StructLayout(LayoutKind.Explicit)] //全て手動(FieldOffset(int offset))で配置する(重ねてunion見たいな使い方もできる)
    struct ExplStr{[FieldOffset(0)]int i; [FieldOffset(0)]long l; [FieldOffset(0)]byte b;}
        //共用体の様になるはずだけどInspect.Stack(es);の表示がおかしい
    
    [StructLayout(LayoutKind.Sequential, Pack = 4/*⟪0～7┃2^i⟫*/)]
    struct SequPackStr{int i; long l; byte b;}//Packはアライメントを指定する
        //アライメントとは、データの先頭位置(アドレス)を指定した倍数の間隔の位置に揃えて配置しCPUが読みやすくする

    //クラス
    [StructLayout(LayoutKind.Sequential)]
    class SequCls{int i; long l; byte b;}

    [StructLayout(LayoutKind./*％❰*/Auto/*❱*/)]
    class AutoCls{int i; long l; byte b;}

    [StructLayout(LayoutKind.Explicit)]
    class ExplCls{[FieldOffset(0)]int i; [FieldOffset(4)]long l; [FieldOffset(12)]byte b;}
    
    [StructLayout(LayoutKind.Auto, Pack = 8)]
    class SequPackCls{int i; long l; byte b;}

    public static class Program {
        public static void Main() {
            //Inspectは重い
            //構造体
            SequStr ss = new SequStr();
            Inspect.Stack(ss);
            AutoStr @as = new AutoStr();
            Inspect.Stack(@as);
            ExplStr es = new ExplStr();
            Inspect.Stack(es);
            SequPackStr sps = new SequPackStr();
            Inspect.Stack(sps);
            
            //クラス
            SequCls sc = new SequCls();
            Inspect.Heap(sc);
            AutoCls ac = new AutoCls();
            Inspect.Heap(ac);
        }
    }
}
```

## ポインタクラス

### [Pointer<T>]============================================================================================

```C#
using System;
//int** ptr ⇔ Pointer<Pointer<int>> ptr
//**ptr ⇔ *(*ptr.p).p
unsafe struct Pointer<T> where T : unmanaged{public T* p;}
public class C {
    unsafe public void M() {
        Pointer<Pointer<int>> ptr = new Pointer<Pointer<int>>();
        _ = *(*ptr.p).p; //p、初期化してない(nullかな?)
    }
}
```

## 一貫性のないアクセシビリティ

### [CS0052：一貫性のないアクセシビリティ]============================================================================================

```C#
//CS0052：一貫性のないアクセシビリティ：フィールドタイプ'Field'はフィールド'Class.field'よりもアクセスしにくい
//ClassのfieldがFieldよりもアクセスし易いため、Fieldを使わなくてもClass経由でFieldより緩いアクセスができてしまう
internal class Field{}
public class Class{
    public Field field;
}
```

## UTF-16とUTF-8まとめ

### [UTF-16とUTF-8まとめ]============================================================================================

```C#
using System;
public class C {
    unsafe public static void Main() {
        char c = '￿'; //char型(2byte)はUnicodeと一致(2^16(0~65535)まで(サロゲートの領域も文字としている?))
        System.Console.WriteLine((int)c);
        string s = "😃";//string型はUTF-16 //Unicodeにはサロゲートという4バイトで表す為の穴が空いている
                        //下位サロゲート: D800 - DBFF (1024) 1024 * 1024 = 2^20
                        //上位サロゲート: DC00 - DFFF (1024) 2^20 + 2^16 - 1 = 0x10FFFF(0x0-0x10FFFF)
        fixed(char* cp = s){//"😃" == 0xD83D 0xDE03 == 55357 56835
            System.Console.WriteLine(((int)cp[0]).ToString() + " " + ((int)cp[1]).ToString());
            
            //UTF-16の4バイト文字"😃"をchar型に入れて表示してみる
            char[] ac = new char[2];
            ac[0] = cp[0]; ac[1] = cp[1]; ac[0] = (char)0xD83D; ac[1] = (char)0xDE03;
            System.Console.WriteLine(ac);
        }
        //UTF-8は、上位ビットに符号を持ちそれによって何バイト必要とする文字か決まる
        //1byte(7bit)__U+007F   0XXX_XXXX
        //2byte(11bit)_U+07FF   110X_XXXX 10XX_XXXX
        //3byte(16bit)_U+FFFF   1110_XXXX 10XX_XXXX 10XX_XXXX
        //4byte(21bit)_U+10FFFF 1111_0XXX 10XX_XXXX 10XX_XXXX 10XX_XXXX
    }
}
```

## 型付き参照(TypedReference)まとめ

### [型付き参照(TypedReference)まとめ]============================================================================================

```C#
using System;

class Program
{
    static void Main(string[] args)
    {
        //型付き参照
        int x = 10;
        TypedReference r = __makeref(x); // x の参照を作る

        __refvalue(r, int) = 99; // 参照元の x も書き換わる

        Console.WriteLine(x); // 99
        
        System.Type t = __reftype(r);
        Console.WriteLine(t);
       
        //ref版
        int x1 = 10;
        ref int r1 = ref x1; // x の参照を作る

        r1 = 99; // 参照元の x も書き換わる

        Console.WriteLine(x1); // 99
        
        System.Type t1 = r1.GetType();
        Console.WriteLine(t1);
        
        //__arglistは、配列引数(paramの様なもの)
        arglistFunc(__arglist(1, "aaa", 'x', 1.5)); // 呼び出し側にも __arglist を書く
    }
    
    static void arglistFunc(__arglist) // 仮引数のところに __arglist を書く
    {
        // 中身のとりだしには ArgIterator 構造体を使う
        ArgIterator argumentIterator = new ArgIterator(__arglist);
        while (argumentIterator.GetRemainingCount() > 0)
        {
            object value = null;

            TypedReference r = argumentIterator.GetNextArg(); // 可変個引数から要素取り出し
            System.Type t = __reftype(r); // TypedReference から、元の型を取得

            // 型(t)で分岐して、__refvalue(r,型) で値の取り出し
            if (t == typeof(int)) value = __refvalue(r, int);
            else if (t == typeof(char)) value = __refvalue(r, char);
            else if (t == typeof(double)) value = __refvalue(r, double);
            else value = __refvalue(r, string);

            Console.WriteLine(t.Name + ": " + value);
        }
    }
}
```
