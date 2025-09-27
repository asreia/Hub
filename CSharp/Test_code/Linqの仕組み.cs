using System;

namespace Linqの仕組み{
    using System.Collections.Generic;
    //using System.Linq;

    public static class Ext{//IEnumerableと取ってIEnumerableを返す//モナドに似ている
        public static IEnumerable<T> Select_<T>(this IEnumerable<T> source, Func<T, T> filter){
            foreach (var x in source){//ここで引数のIEnumerableの要素が取り出される
                yield return filter(x);//遅延的に出力できる
            }
        }    
    }

    public static class Program {
        public static void M() {
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