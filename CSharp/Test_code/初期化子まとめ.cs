using System;

namespace 初期化子{
    using System.Collections.Generic;
    using System.Collections;
    using static System.Console;
    public class C:IEnumerable<int>{
        public IEnumerator<int> GetEnumerator(){return default;}
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
        public static void M() {//初期化子はコンストラクタの後にアクセス可能なメンバに対して初期化する
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
                //Addメソッドが2引数の時↑のように書ける
        }
    }
}