using System;
namespace Struct_matome{
    public interface I0{}public interface I1{}
    public struct S0{}
    public class C0/*:S0*/{}//構造体はsealedされている
    public struct S:/*C0,*/I0,I1{//クラスも継承不可。interfaceは多重継承可能(キャスト時はボックス化が起きる)
        //int n0 = 9;//多分、引数なしコンストラクタはフィールドをゼロ初期化するのでメンバ変数に初期化値を代入できない
        public int n1;
        public int p0{get;}//構造体はスタックに積まれるのでデータサイズは16バイト以下が最適
        public readonly int r0;
        //public S(){}//構造体の引数なしコンストラクタはC#によってフィールドをゼロ初期化するという機能で固定されている
        public S(int m){
            //Func();//CS0188:全てのフィールドメンバが割り当てられるまでthis(メソッド?)を使えない
            n1 = m;//↓,↓↓引数ありコンストラクタは全てのフィールドメンバの初期化を義務付けられる
            p0 = m;//構造体の?コンストラクタのget-onlyプロパティへの代入(メソッド)はバッキングフェールドに直接代入する
            r0 = m;//readonlyはコンストラクタのみ初期化可能なのでおけ
            Func();//全て割り当てれば使える
        }
        public void Func(){}
    }

    public class C {
        public static void M() {
            S s0 = new S();//フィールドゼロ初期化
            S s1 = new S(5);//引数ありは全て割り当てされている
            Console.WriteLine($"s0: {s0.n1}, {s0.p0}, {s0.r0}");
            Console.WriteLine($"s0: {s1.n1}, {s1.p0}, {s1.r0}");
        }
    }
}