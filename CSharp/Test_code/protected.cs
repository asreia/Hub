using System;
namespace prot{
    public class C0{
        public void MeC0(){
            //((C1)new C3()).PM();
        }
    }
    public class C1:C0{
        protected virtual void PM(){Console.WriteLine($"{nameof(PM)}_{nameof(C1)}: ");}
        public void MeC1(){
            ((C2)new C3()).PM();
        }
    }
    public class C2:C1{
        //protected override void PM(){Console.WriteLine($"{nameof(PM)}_{nameof(C2)}: ");}
        public void MeC2(){
            ((C3)new C3()).PM();
        }
    }
    public class C3:C2{
        //protected override void PM(){Console.WriteLine($"{nameof(PM)}_{nameof(C3)}: ");}
        public void MeC3(){
            ((C3)new C3()).PM();
        }
    }
    public class Start{
        public static void M(){
            new C3().MeC0();
            new C3().MeC1();
            new C3().MeC2();
            new C3().MeC3();
        }
    }
}