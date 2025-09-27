using System;

//global::名前空間1.名前空間2...名前空間n.∫Complex∫1.∫Complex∫2...∫Complex∫n.メンバ1.メンバ2...メンバn
//名前空間（name space）とは、∫Complex∫を種類ごとに分けて管理するための機構です。
//===================================================================================================
namespace 基本的な使い方{
    //extern alias X; //外部エイリアス//"csc /r:X=Lib.dll Test.cs" とコンパイルするとLib.dllのglobalを
                    //Xとして、X::❰＃Lib.dll内の要素＃❱と指せる。あとnamespaceの先頭である必要がある
    using A;
    using A.B;
    using Alias = A.B.CB;//エイリアス//名前空間~∫Complex∫に別名を与える
    using Alias1 = A.B;
    using static A.B.CB; //∫Complex∫まで省略し静的メンバを直接書ける
                         //usingと区別する理由はnamespaceと同名の∫Complex∫を定義して破壊しないようにするため
    namespace usingの使用{
         class usingの使用{//階層違いで同名の識別子を使用可能
             void F(){
                 _ = new CA(); //using A;によりA.CAのAを省略できる
                 _ = new CB(); //using A.B;によりA.B.CBのA.Bを省略できる
                 _ = new Alias(); //using Alias = A.B.CB;によりA.B.CBの別名Aliasを使った
                                  //Aliasがクラスだと出てくるのでクラスの別名だと気づきにくい
                 _ = new Alias1.CB(); _ = new Alias1::CB(); //"::"で直前がエイリアスであることを明示できる
                 _ = CBの静的メンバ; //using static A.B.CB;により∫Complex∫まで省略し静的メンバを直接書ける
                 _ = new global::基本的な使い方.A.B.CB(); //完全修飾名//globalもエイリアス
             }
         }   
    }
    namespace A{
        //using C;//CS0246//↓↓↓にも書いてあるがnamespace A{}"直下"にnamespace C{}が無いのでエラーになる
        class CA{}
        namespace B{
            class CB{public static int CBの静的メンバ = 0;}
            class X{}
            namespace C{
                
            }
        }
    }
    namespace A.B{//ネストになっているnamespaceをA.Bというふうに書く事ができる
        //class X{} //CS0101: 名前空間「基本的な使い方.A.B」には、すでに「X」の定義が含まれています
            //namespace A{namespace B{"ココ"}} と namespace A.B{"ココ"} のnamespaceは同じnamespaceです
    }
    namespace B{class X{}}//namespace A.B{}とnamespace B{}は別のnamespaceなのでclass X{}を定義できる
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
            static void M(){
                //{namespaceのブロック内}: 直接 ↔ エイリアス → using
                //    ↓
                //{namespaceのブロック外}: 直接 ↔ エイリアス → using
                Lib.F();
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