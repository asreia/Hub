using System;

//===========================================================================================
public class ExprThrow{//throw式//↓これ以外の文脈でthrow式を書くことはできません。
    // 式形式メンバーの中( => の直後)
    static void A() => throw new NotImplementedException();

    static string B(object obj){
        // null 合体演算子(??)の後ろ
        var s = obj as string ?? throw new ArgumentException(nameof(obj));

        // 条件演算子(?:)の条件以外の部分
        return s.Length == 0 ? "empty" :
            s.Length < 5 ? "short" :
            throw new InvalidOperationException("too long");
    }   
}
//===========================================================================================
public static class Program {
    public static int X(){/*if(false)*/ throw new Exception("くらえ");}
        //例外をthrowすることが確実な場合は戻り値が任意
    public static void RethrowKind(){
        try{
            X();
        }
        catch (Exception e){//Xメソッドで起きた例外を再スローする
            throw; //catchした例外のインスタンス(e)を触らずにそのまま再スロー(スタックトレースが書き換わらない)
                    //IL: rethrow
            //throw new Exception("例外を例外で包む", e); //catchした例外(e)を生成した新たな例外に包んで返す
                                                    //IL: throw//eはInnerExceptionプロパティで取り出せる
            //throw e; //IL: throw//スタックトレースが上書きされて本来の例外の発生源が消えてしまう
        }
    }
    public static void ExcProc() {//例外処理でメソッドの入れ子を崩すと↓こんなイメージ
        try{
            try{
                RethrowKind();
                
            }
            catch{
                throw;
            }
        }
        catch(Exception e)　when(e.Message == "くらえ"){//when句でさらに条件で例外を選択することができる
            Console.WriteLine("ぐはぁ");
            throw;
        }
        catch(Exception e)　when(e.Message == "例外を例外で包む"){//when句でさらに条件で例外を選択することができる
            Console.WriteLine("↓の " + e.Message);
            Console.WriteLine(e.InnerException.Message);
            //InnerExceptionの参照がsharplabだとうまく動かない
            throw;
        }
        finally{
            Console.WriteLine("このまま例外を通すわけにはッ！(finally)");
        }
    }
    public static void M(){ExcProc();}
}
//===========================================================================================