using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    
    [StructLayout(LayoutKind.Explicit)]
    struct ExplStr{[FieldOffset(0)]int i; [FieldOffset(4)]long l; [FieldOffset(12)]byte b;}
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