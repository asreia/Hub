
using System;
using System.Collections.Generic;
using System.Collections;
public class C:IEnumerable<int>{
            public IEnumerator<int> GetEnumerator(){yield return 0;}//←↓は、使われていない
        IEnumerator IEnumerable.GetEnumerator(){return GetEnumerator();}
    public int a;
    public int b,c;
    public int d => a + b;
    public int[] arr = {1,2,3,4};
    public void Deconstruct(out int n1, out int n2, out int n3) => (n1, n2, n3) = (a, b, c);
    public int this[int n]{get{return arr[n];} set{arr[n] = value;}}
    public int cnt = 0; 
    public void Add(int n){
        switch(cnt){
            case 0:
                a = n;
                break;
            case 1:
                b = n;
                break;
            case 2:
                c = n;
                break;
        }
        cnt++;
    }
}

public class M
{
    public static void Main()
    {
        int n = new C{2,4,6} switch{
            {d: 4} => 2,
            (2,4,6) and {d: >=4} => 4,
                _ => 10
        };
        string n1 = new C{2,4,5}.arr switch{
            // [1,2,3,4] = "[1,2,3,4]" //'リスト パターン' は現在、プレビュー段階であり、*サポートされていません*。
            _ => "abc"
        };
        Console.WriteLine(n);
        
        if(new C{2,4,6} is {d: 6} c)
        {
            Console.WriteLine(c.a);
        }
    }
}