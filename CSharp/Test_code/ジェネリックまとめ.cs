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
        public static void M(){
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