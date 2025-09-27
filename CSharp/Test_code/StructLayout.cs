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
    [StructLayout(LayoutKind./*❰％*/Sequential/*❱*/)]
    struct SequStr{int i; long l; byte b;}//一番大きい型のサイズで宣言順に配置される

    [StructLayout(LayoutKind.Auto)]
    struct AutoStr{int i; long l; byte b;}//一番大きい型のサイズから詰めて配置される

    //[StructLayout(LayoutKind.Explicit)] //全て手動(FieldOffset(int offset))で配置する(重ねてunion見たいな使い方もできる)
    //struct ExplStr{[FieldOffset(0)]int i; [FieldOffset(4)]long l; [FieldOffset(12)]byte b;}
        //許可されていない明示的レイアウトと言われてエラー。pcだと通る

    //クラス
    [StructLayout(LayoutKind.Sequential)]
    class SequCls{int i; long l; byte b;}

    [StructLayout(LayoutKind./*❰％*/Auto/*❱*/)]
    class AutoCls{int i; long l; byte b;}

    //[StructLayout(LayoutKind.Explicit)]
    //class ExplCls{[FieldOffset(0)]int i; [FieldOffset(4)]long l; [FieldOffset(12)]byte b;}

    public static class Program {
        public static void Main_() {
            Console.WriteLine("🌄");
            //構造体
            SequStr ss = new SequStr();
            //Inspect.Stack(ss);
            AutoStr @as = new AutoStr();
            //Inspect.Stack(@as);
            //クラス
            SequCls sc = new SequCls();
            //Inspect.Heap(sc);
            AutoCls ac = new AutoCls();
            //Inspect.Heap(ac);
        }
    }
}