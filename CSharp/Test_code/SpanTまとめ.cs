using System;

namespace SpanTまとめ{
    public struct Struct{public int n;}
    public static class Program {
        public static void M() {
            //基本的な使い方
            int[] arr = new int[10]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            Span<int> span = arr.AsSpan(); //AsSpanは拡張メソッド(static Span<T> AsSpan<T>(this T[]? array);)
            span = span.Slice(2, 6);

            Console.Write("for: ");
            for(int i = 0; i < span.Length; i++) Console.Write(span[i] + ","); //=> for: 2,3,4,5,6,7,
            Console.Write("\nfoeach: ");
            foreach(int n in span) Console.Write(n + ",");                     //=> foeach: 2,3,4,5,6,7,
            Console.WriteLine();
            

            //ReadOnlySpan<T>とSpan<T>
            ReadOnlySpan<int> rspan; //ref readonly T this[int index]
            Span<int> span1;         //ref T this[int index]
            

            //色々なSpan<T>の作り方
            _ = new Span<int>(new int[10], 2, 6);

            int[] d = new int[6]{0, 2, 3, 4, 5, 6};
            Span<int> sd = d.AsSpan();      //❰var¦Span<int>❱ sd = d＠❰.AsSpan()❱;
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