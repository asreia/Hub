using System;
namespace デリゲート代入テスト{
    using System.Collections.Generic;
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
            Action DVF = this.VFunc;
            DVF();
            //◎仮想メソッドおけ//sharplabだと仮想メソッドは不可(Unbreakable.AssemblyGuardException:検証できないポインター操作を実行します。)
            //Func<int> DP = P;
            //✖プロパティ不可(関数として表現できない)
            Action DIF = ((I)this).IFunc;
            DIF();
            //◎interfaceの仮想メソッドおけ//sharplabだとinterfaceの仮想メソッドも不可
            Action DISealedF = ((I)this).ISealedFunc;
            DISealedF();
            //◎interfaceのsealed(インスタンスメソッド)はおけ
            Action DISF = I.ISFunc;
            DISF();
            //◎interfaceの静的メソッドはおけ
            //Action a2 = Program;
            //✖コンストラクタは型と認識され不可
            Func<IEnumerator<int>> FEnum_i =Iter;
            FEnum_i().MoveNext();
            //◎イテレーターおけ
            Action IExt = ((I)this).ExtFunc;
            IExt();
            //◎拡張メソッドおけ
            Program m = (new Program()) + (new Program());
            Func<Program,Program,Program> Plus = (p0,p1) => p0 + p1;
            Plus((new Program()),(new Program()));
            //◎演算子のオーバーロードはラムダ式を噛ませればいける
            Func<string> Ex = Expr;
            Console.WriteLine(Ex());
            //◎expression-bodiedおけ
            Action Par = ParFunc;
            Par();
            //◎partial(部分メソッド)おけ
        }
        public static void M() {
            new Program().Method();
        }
    }
}