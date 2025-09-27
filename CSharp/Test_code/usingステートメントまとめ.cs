using System;
namespace usingステートメントまとめ{
    //========================================================================================
    class _{
        void __(){//usingは例外処理の糖衣構文
            //==========================================================================
            using(Resource r0 = new Resource("")){//using (❰∫式∫¦obj as IDisposable❱){}でもいい
                /*リソースに対する操作*/
            }
            //↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕↕
            Resource r1 = /*式*/ new Resource("");
            try{
                /*リソースに対する操作*/
            }
            finally{
                if(r1 != null)((IDisposable)r1).Dispose();
            }
            //==========================================================================
            
            using(new _() as IDisposable){}//using(❰＃nullかIDisposableのインスタンスを返す∫式∫＃❱){}
                                                //nullだった場合はDisposeが呼ばれないだけかな
            //ref構造体だけ特別にIDisposableを継承しなくてもパターンベース(ダックタイピング?)で使える
        }
    }
    
    //========================================================================================
    
    public class Resource:IDisposable{
        private bool alive{get;set;} = true;
        public string name{get;}
        public Resource(string name){this.name = name;}
        public void Alive(){if(alive)Console.WriteLine(name + "、生存なう");
                            else Console.WriteLine(name + "、し、死んでる！？");}
                                                        //↑死んだ後はスコープから外れてるから言えない
        void IDisposable.Dispose(){Console.WriteLine(name + "、昇天なう");alive = false;}
    }

    public static class Program {
        public static void M() {
            try{
                Console.WriteLine("🌄");
                Console.WriteLine($"using {nameof(Resource)} resource0 = new {nameof(Resource)}(resource0);");
                using Resource resource0 = new Resource(nameof(resource0));
                //using変数宣言//ブロック{}がスコープ全体になるので注意
                resource0.Alive();
                Console.WriteLine($"using({nameof(Resource)} resource1 = new {nameof(Resource)}(resource1)){{");
                using(Resource resource1 = new Resource(nameof(resource1))){
                    resource0.Alive();
                    resource1.Alive();
                }
                Console.WriteLine("}");
                resource0.Alive();
                //throw new Exception();//AliveもDisposeも呼ばれているはずだけど表示されない
            }
            catch{
                Console.WriteLine("例外が発生しました");
            }
        }
    }
    //========================================================================================
}