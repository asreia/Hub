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
        public static void M(){
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