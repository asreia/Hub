using System;
namespace Init{
    public class Super{
        static int snum = 1;//3:Super静的フィールド初期化
        int num = 3;//7:Superフィールド初期化
        static Super(){
            Console.WriteLine($"3:Super静的フィールド初期化");
            Console.WriteLine($"4:Super静的コンストラクタ");

            //Subコンストラクタに書きたいが初期化中に呼べないのでここに書く
            Console.WriteLine($"5:Subフィールド初期化");
            Console.WriteLine($"6:Superの発射");
        }
        public Super(){
            Console.WriteLine($"7:Superフィールド初期化");
            Console.WriteLine($"8:Superコンストラクタ");
        }
    }
    public class Sub:Super{
        static int snum = 0;//1:Sub静的フィールド初期化
        int num = 2;//5:Subフィールド初期化
        static Sub(){
            Console.WriteLine($"1:Sub静的フィールド初期化");
            Console.WriteLine($"2:Sub静的コンストラクタ");
        }
        public Sub(){

            Console.WriteLine($"9:Subコンストラクタ");
        }
    }
    public class Launch{
        public static void M(){
            Console.WriteLine($"0:Subの発射");
            new Sub();
        }
    }
}