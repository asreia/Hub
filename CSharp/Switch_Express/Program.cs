
using System;
using System.Collections.Generic;
using System.Collections;

public class SuperThings{}
public class Things : SuperThings, IEnumerable<int>{
    public IEnumerator<int> GetEnumerator(){yield return 0;}
    IEnumerator IEnumerable.GetEnumerator(){return GetEnumerator();}
    //変数
    public int a = 1, b = 2;
    public (int, int) c = (3,4);
    //getプロパティ
    public int p => a + b;
    //List
    public List<int> list = new List<int>{4,5,6}; 
    //分解代入
    // public void Deconstruct(out int n1, out int n2, ValueTuple<int, int> n34) => (n1, n2, n34) = (a, b, c); 
    public void Deconstruct(out int n1, out int n2) => (n1, n2) = (a, b); 
    //インデクサ
    public int this[int n]{get{return list[n];} set{list[n] = value;}}
    //コレクション初期化子
    public void Add(int n){
        list.Add(n);
    }
}

public class M
{
    public static void Main()
    {
        (int a, (int b, int c)) = (1, (2, 3));
        var things1 = new Things(); 
        string n1 = new Things() switch //値でもいい
        {
            //プロパティマッチ
                //Haskellの値構成子の引数のマッチに似ている
            Things{a: 1, b: not 3 and 2 and _ and var abc and >= 1, c: (2,3)} => "{a: 1}",
            {b: not not not not (not (not (not (not (not (not 2))))))} => "{b: 2}",
            {c: (3,4)} => "{c: (3,4)}",
            //メンバアクセス
            {list.Count: 3} => "{list.Count: 2}",
            //メンバのプロパティマッチ
            {list: {Count: 2}} => "{list: {Count: 3}}",
            {list: {Count: _}} => "{list: {Count: 3}}",
            //位置パターン(Deconstruct(タプル分解代入))
            // (1,2,(3,4)) => "(1,2,3)", //入れ子タプルは無理だった
            // (1,2,new ValueTuple<int,int>(3,4)) => "(1,2,3)",
            Things(1,2) => "(1,2)",
            (1,4) => "(1,4)",
            //値マッチ
              {} => "not null", //nullチェックはするのでnullではない
            null => "null",
            //型マッチ
            // {d: 4} => 2,
            // (2,4,6) and {d: >=6} => 4,
            //     _ => 10
        };
        Console.WriteLine(n1);
        SuperThings st = new Things();
        string n2 = st switch
        {
            Things t and {b: 2} => "Things t" + t.ToString(),
            SuperThings => "SuperThings",
            var v => " var v" //nullでも受けるので"v"はnullになりうる
        };
        Console.WriteLine(n2);
        int i = 4;
        string n3 = i switch
        {
            >= 0 and < 10 => ">= 0",
            <= 20 or 40 => "<= 20 or 40"
        };
        Console.WriteLine(n3);
        static string Func<T>(T t) => t switch{ int => "abc" };
        int i1 = 2;
        _ = i1 is int n4;
        void Fis<T>(T a){if(a is Things n5){Console.WriteLine(n5);}else{Console.WriteLine("else")/*($"{n5}")*/;}}//elseにはn5のスコープが無い
        // void Fis<T>(T a){if(a is Things n5 || n5 == n5){Console.WriteLine();}else{Console.WriteLine("else")/*($"{n5}")*/;}}//elseにはn5のスコープが無い
        Fis(4);Fis(new Things());
        // _ = 2 as int; //asは参照型でないとだめ
        Console.WriteLine(n4);
        //＄switch式＝❰⟪｢変数｣¦∫Lit∫⟫ switch{｡⟦,┃～⟧❰∫Pattern∫ => ∫式∫❱｡}❱
        //＄is式＝❰⟪｢変数｣¦∫Lit∫⟫ is ∫Pattern∫ ｢変数定義｣❱ //is式はboolを返す。
            //｢変数定義｣にはis式がtrueの時⟪｢変数｣¦∫Lit∫⟫が入り、falseの時nullが入る?(nullが入る時の部分のスコープがないようになってる?)。
            //が、スコープがis式が属している式とif文のthenブロックしかない

        //＄Pattern＝⟪Constant_P¦Property_P¦Positional_P¦Type_P¦Var_P¦Discard_P¦Relational_P¦Pattern_C_and_or¦Pattern_C_not¦Pattern_C_()⟫
        //＄Constant_P＝❰∫Lit∫❱
        //＄Property_P＝％❰∫Type∫❱❰｡{｡⟦,┃～⟧❰｢メンバ｣: ∫Pattern∫❱｡}｡❱
        //＄Positional_P＝％❰∫Type∫❱❰｡(｡⟦,┃～⟧❰∫Pattern∫❱｡)｡❱
        //＄Type_P＝❰∫Type∫ ＠❰｢変数定義｣❱❱                                //Haskellでは型でパターンマッチするのは許されない。でも、これが直和型の代わりにはなりうる
        //＄Var_P＝❰var ｢変数定義｣❱ //nullでも受けるので"v"はnullになりうる //多相変数的な
        //＄Discard_P＝⟪var _¦_⟫ //"var _"はis,caseのみ
        //＄Relational_P＝❰⟪<¦<=¦>¦>=⟫ ∫Pattern∫❱                     //ガード的な
        //＄Pattern_C_and_or＝❰∫Pattern∫ ⟪and¦or⟫ ∫Pattern∫❱
        //＄Pattern_C_not＝❰not ∫Pattern∫❱
        //＄Pattern_C_()＝❰｡(｡⟦ ┃2～⟧❰∫Pattern∫❱｡)｡❱     //単に主にand,orの結合性を解決
        //List_Pは配列かインデクサをもつ様な型にマッチする。"..∫Pattern∫"はList_Pにマッチ。`..`はその間を捨てる。
        //`..`は2つ以上書くとエラー(多分どう範囲を区切ればいいか分からない)。"..var arr"はその区間のコピーになってしまうので`Span<T>`だと効率がいい
        //＄List_P＝❰｡[｡⟦,┃～⟧⟪∫Pattern∫¦..¦..∫Pattern∫⟫｡]｡❱ //List_PはC#11の機能で現在プレビュー
        //whenいらなくね?(andで置き換え可能)
        
            // [1,2,3,4] = "[1,2,3,4]" //'リスト パターン' は現在、プレビュー段階であり、*サポートされていません*。
        Person person = new Person("namae", new DateTime(1800, 12, 31));
        Console.WriteLine(person);
        D.M();
    }
    record Person(string name, DateTime birthday); //レコード型はC# 9.0 //↓の様なちょクワガタに進化して欲しい！https://ufcpp.net/study/csharp/datatype/record/

    // record Shape //Shapeは構造を持たない基底クラス
    // {
    //     Rect(float height, float width) |
    //     Triangle(float height, float width) |
    //     Circle(float radius)
    // }
    // record Maybe<T> //基底クラスが型引数を持ち派生がそれを使う。ILレベルで更新が必要そう..いや単に継承先から型引数を渡すだけ(Just<T>(T t) : Maybe<T>)
    //{
    //     Just(T t) |
    //     Nothing()
    //}
    //これなら可能
    record Maybe();
    record Just<T>(T t) : Maybe; //C#はメンバには必ず名前が必要。Haskellは無い(マッチで位置でアクセスしてる)。これもタプルみたいにItem1とかできれば消せる
    record Nothing() : Maybe;
    record Shape();
    record Rect(float height, float width) : Shape;
    record Triangle(float height, float width): Shape;
    record Circle(float radius) : Shape;
    //Maybe Shape
    class D
    {
        public static void M()
        {
            //Haskellとの違い
            //Haskellの関数の評価とマッチ: "func a b"のように"()"を書かない。結合的にだめな時に"()"を付けるだけ
            //Haskellの型推論: 型推論がすごすぎて型を指定しなくてもいいためC#のジェネリックの型を指定しなくてもいい
            //Haskellの値を生成?: 値コンストラクタに引数を並べるだけなので"new"とか特別なキーワードがない(Pythonもない)
            //Haskellはメンバアクセスがない: Haskellはマッチを利用して変数からメンバの値を取り出している
            //Haskellの直和型は継承構造ではなく、デフォルト機能

            //maybe_Shape = Just (Rect 1.2 2.2)
            Maybe maybe_Shape = new Just<Shape>(new Rect(1.2f, 2.2f));
            // Maybe maybe_Shape = Just(Rect(1.2f, 2.2f)); //new,ジェネリック を除去してみた
            (float, float)? shape = maybe_Shape switch
            {   //Just (Rect height width) = Just (height, width) //Haskellに"t,height,width"が現れないのは順番(位置)でアクセスしてるから?"and"もない
                Just<Shape> and {t: Rect and {height: float h, width: float w}} => (h, w), //プロパティアクセスは省略できない
                // Just {Rect {float h, float w}} => (h, w),  //型推論できるとしてジェネリック,and,プロパティ名 を除去してみた。だいぶHaskellに近くなった
                Just<Shape> and {t: Circle c} => (c.radius, 0.0f),
                Nothing => null
            };
            Console.WriteLine(shape);
        }
    }
}