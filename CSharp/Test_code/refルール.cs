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
        public ref readonly int InFunc(in int n = 100/*既定値可能*/){
            //n = 100;//readonlyなので書き換え不可
            return ref n;//in == ref readonlyなのでref readonlyはref readonlyとして返せる
        }

        //Out引数===============================================================================================
        public ref int OutFunc(out int n /*= 4//既定値不可*/){//out == 代入が必要なref
            n = 100;//必ず代入が必要
            return ref n;
        }

        //ref引数===============================================================================================
        public ref int RefFunc(ref int rnArg){
             rnArg += 100;
             return ref rnArg;   
        }

        //refルール===============================================================================================
        public ref int RefRuleFunc(ref int rnArg){
             int nLoc = 100;
             //rnArg = ref nLoc;//関数内で生成した変数を引数の参照に再代入できない
             ref int rnLoc = ref nLoc;
             //return ref rnLoc;//関数内で生成した変数を参照で返せない
             return ref RefFunc(ref rnArg);//引数で渡した参照をそのまま参照で返す関数ならおけ   
        }

        //戻り値がrefの関数への代入================================================================================
        public int fn = 100;
        public ref int RetfnFunc()=> ref fn;//クラスはフィールドメンバ変数の参照を返せる

        //構造体の中身のrefを返す==================================================================================
        public ref int TupleFunc(ref (int A, int B) s){
            s.A += 100;
            return ref s.A;
        }

        //参照型(ヒープ)を介すれば値型のメンバのrefを返せる===========================================================
        private struct D{public int A, B;}//クラスでもおけ
        //構造体だけどc.DFunc(){d.A}のように参照型のクラスCを介しているので参照を返せる
        //(クラスC(ヒープ)の中に構造体D(値型)がある)
        private D d = new D();
        public ref int DFunc(){
            d.A += 100;
            return ref d.A;
        }
    }

    public static class Program {
        public static void M() {
            {
                //ref代入===============================================================================================
                int nLoc0 = 1;
                ref int rnLoc0 = ref nLoc0;//==========rnLoc0への代入
                rnLoc0++;
                Console.WriteLine($"nLoc0:{nLoc0}, rnLoc0:{rnLoc0}");

                //ref再代入==============================================================================================
                int nLoc1 = 2;
                rnLoc0 = ref nLoc1;//ref再代入//=======rnLoc0への再代入
                nLoc1++;
                Console.WriteLine($"nLoc0:{nLoc0}, nLoc1:{nLoc1}, rnLoc0:{rnLoc0}(再代入)");
            }

            //ref readonly代入===========================================================================================
            int nLoc2 = 4;
            ref readonly int rrnLoc0 = ref nLoc2;
            //rrnLoc0 = 4;//readonlyへの代入不可
            ref readonly int rrnLoc1 = ref rrnLoc0;
            //参照で繋がっているのでref readonlyはref readonlyを維持して代入する
            Console.WriteLine($"nLoc2:{nLoc2}, rrnLoc0:{rrnLoc0}, rrnLoc1:{rrnLoc1}");

            {//クラスCを使用
                var c = new C();

                //In引数===============================================================================================
                int nIn0 = 8;    
                //in引数は参照渡しも値渡しも可能(IFunc(❰in ∫vari∫¦∫vari∫¦∫式∫¦❰❱❱)❰/;最後は既定値使用)
                ref readonly int rrnIn0 = ref c.InFunc(in nIn0);//=> 8
                ref readonly int rrnIn1 = ref c.InFunc(nIn0/*in省略可能*/);//=> 8
                ref readonly int rrnIn2 = ref c.InFunc(1024/*∫Lit∫を渡すとrefでは無くなる*/);//=> 1024
                ref readonly int rrnIn3 = ref c.InFunc(/*既定値使用*/);//=> 6
                Console.WriteLine($"nIn0:{nIn0}, rrnIn0:{rrnIn0}, rrnIn1:{rrnIn1}, rrnIn2:{rrnIn2}, rrnIn3:{rrnIn3}");

                //Out引数===============================================================================================
                int nOut0 = 16;     //Out引数は参照渡しして呼び出し先で必ず代入される
                ref int rnOut0 = ref c.OutFunc(out nOut0);//=> 100
                //int nOut1;               ←があるのと同じ↓
                ref int rnOut1 = ref c.OutFunc(out int nOut1);//=> 100
                Console.WriteLine($"nOut0:{nOut0}, rnOut0:{rnOut0}, rnOut1:{rnOut1}");
                
                //ref引数===============================================================================================
                int nRef0 = 32;     //ref引数は単にIn,Out引数の制限が無い版
                ref int rnRef0 = ref c.RefFunc(ref nRef0);//=> 132 
                Console.WriteLine($"nRef0:{nRef0}, rnRef0:{rnRef0}");

                //refルール===============================================================================================
                int nRule0 = 64;
                ref int rnRule0 = ref c.RefRuleFunc(ref nRule0);//=> 164
                Console.WriteLine($"nRule0:{nRule0}, rnRule0:{rnRule0}");

                //戻り値がrefの関数への代入================================================================================
                int nRet0 = 128;
                Console.WriteLine($"c.fn:{c.fn}(c.RetfnFunc() = nRet0;前)");
                c.RetfnFunc() = nRet0;
                Console.WriteLine($"c.fn:{c.fn}(c.RetfnFunc() = nRet0;後)");

                //構造体の中身のrefを返す==================================================================================
                (int A, int B) t = (256, 512);
                Console.WriteLine($"t:{t}");
                ref int rA = ref c.TupleFunc(ref t);//=> 356//構造体のrefの中身のrefを返せる
                Console.WriteLine($"t:{t}, rA:{rA}");

                //参照型(ヒープ)を介すれば値型のメンバのrefを返せる===========================================================
                ref int rDa = ref c.DFunc();
                Console.WriteLine($"D:{rDa}");
            }
        }
    }
}