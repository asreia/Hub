using System;

namespace イテレータまとめ{
    using System.Collections.Generic;
    using System.Collections;
    
    public class EasyIter{//foreach文はIEnumerableを実装しなくても動く。ダックタイピング？
        public EasyIter GetEnumerator()=> this;
        int counter = 0;
        public bool MoveNext()=> counter++ < 3? true : false;
        public int Current{get{return counter;}}
    }

    public class Iter<T>:IEnumerator<T>{//IEnumerator<T>はMoveNext,Current(ジェネ,非ジェネ),Reset,Disposeを実装
        T[] table{get;}
        int counter = -1;
        public Iter(T[] table)=> this.table = table;
        
        public bool MoveNext(){//MoveNext
            if(counter < table.Length - 1){
                counter++;
                return true;
            }else return false;
        }
        
        public T Current{get{return table[counter];}}//Currentジェネ
        
        object IEnumerator.Current{get{return Current;}}//Current非ジェネ
        
        public void Reset(){counter = -1;}
        public void Dispose(){}
    }

    public class Table<T>:IEnumerable<T>{//IEnumerable<T>はGetEnumeratorのジェネと非ジェネを実装する
        T[] table{get;}
        public Table(T[] table)=> this.table = table;
        
        public IEnumerator<T> GetEnumerator(){//ジェネ
            return new Iter<T>(table);
            //yield return default(T); //コンパイル結果: https://ufcpp.net/study/csharp/sp2_iterator.html
            //戻り値がIEnumera❰ble¦tor❱＠❰<T>❱であるメソッドに、
            //yield ❰return ∫式∫¦break❱を書くとイテレータメソッドになる。
            //メソッド内で使われる変数は、ローカル変数:メンバ変数として持つ、フィールド:this経由アクセス
            //静的変数:直接、というふうにイテレータメソッドが返すオブジェクトが必要な時に素直にアクセスする
        }
        
        IEnumerator IEnumerable.GetEnumerator(){//非ジェネ//IEnumerable<T>にデフォルト実装できそうな気がする
            IEnumerator enumerator = (IEnumerator)GetEnumerator();
            return enumerator; //IEnumerator<T>をIEnumeratorにアップキャスト！
        }     
    }

    public class C {
        public static void M() {
            Console.WriteLine("EasyIterのforeach");
            EasyIter eiter = new EasyIter();
            foreach(int n in eiter){Console.WriteLine(n);}
            
            Console.WriteLine("\nTableのforeach");
            Table<int> table = new Table<int>(new int[]{1, 2, 3});
            foreach(int n in table){
                Console.WriteLine(n);
            }
            
            Console.WriteLine("\nTableのiterを手動で回す");
            IEnumerator<int> iter = table.GetEnumerator();
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine("iter.Reset()");
            iter.Reset();
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            Console.WriteLine(iter.MoveNext());
            Console.WriteLine(iter.Current);
            
        }
    }
}