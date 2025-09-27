using System;
namespace Assign_test{
    public class C {
        public static void M() {
            int n0, n1 = 2;
            n0 = n1 + (n1 = 4);
            Console.WriteLine($"n0:{n0}\nn1:{n1}");//=>n0:6 n1:4//式を評価してから代入している
        }
    }
}