using System;
public interface test{protected internal partial interface test1<T>{public const int n = 4;}}
public interface I1{
 protected void ipFunc();
}
public interface I0 : I1{
    //protected void ipFunc(){Console.WriteLine($"{nameof(ipFunc)}_{nameof(C3)}: ");}
    void I1.ipFunc(){Console.WriteLine($"{nameof(I0.ipFunc)}_{nameof(I0)}: explicit");}
    void ipproc(){ipFunc();}
    public void iImpFunc(){Console.WriteLine($"{nameof(iImpFunc)}_{nameof(I0)}: ");} 
    public void iFunc();
}                        //interfaceのprotectedはinterfaceまで。
public abstract class C3 : I0{
    //void I1.ipFunc(){Console.WriteLine($"ipFunc_{nameof(C3)}: explicit");}
    //void I1.ipFunc(){}
    public virtual void iFunc(){Console.WriteLine($"{nameof(iFunc)}_{nameof(C3)}: ");} 
    void I0.iFunc(){Console.WriteLine($"{nameof(I0.iFunc)}_{nameof(C3)}: explicit");}
    //public void iImpFunc(){Console.WriteLine($"{nameof(I0.iImpFunc)}_{nameof(C3)}: ");}
    void I0.iImpFunc(){Console.WriteLine($"{nameof(I0.iImpFunc)}_{nameof(C3)}: explicit");}
    //void I0.iFunc(){Console.WriteLine($"{nameof(I0.iFunc)}_{nameof(C3)}: explicit");} 
    public int num = 3;
    public static int snum = 3;
    public abstract void vFunc();//{Console.WriteLine($"{nameof(vFunc)}_{nameof(C3)}: ");} 
    public static void sFunc(){Console.WriteLine($"{nameof(sFunc)}_{nameof(C3)}:");}
}
public class C2 : C3 , I0{
    public new int iFunc()=>0; //∫Signatur∫が同じならばコンパイラはメソッドを絶対に区別できない?
    void I0.iFunc(){Console.WriteLine($"{nameof(I0.iFunc)}_{nameof(C2)}: explicit");}
    void I0.iImpFunc(){Console.WriteLine($"{nameof(I0.iImpFunc)}_{nameof(C2)}: explicit");} 
    //public override void iImpFunc(){}
        
    //public override void iFunc(){Console.WriteLine($"{nameof(iFunc)}_{nameof(C2)}: ");}
    //public void iFunc(){Console.WriteLine($"{nameof(iFunc)}_{nameof(C2)}: ");}//仮想とインスタンス違いでオーバーロードはできない(シグネチャが同じだから)
    public new int num = 2;
    public new static int snum = 2;
    public override void vFunc(){Console.WriteLine($"{nameof(vFunc)}_{nameof(C2)}: ");}
    public new static void sFunc(){Console.WriteLine($"{nameof(sFunc)}_{nameof(C2)}:");}
}
public class C1 : C2{
    //public override void iFunc(){Console.WriteLine($"{nameof(iFunc)}_{nameof(C1)}: ");}
    public new int num = 1;
    public new static int snum = 1;
    public new virtual void vFunc(){Console.WriteLine($"{nameof(vFunc)}_{nameof(C1)}: ");}
    public new static void sFunc(){Console.WriteLine($"{nameof(sFunc)}_{nameof(C1)}:");}
}
public class C0 : C1 ,I0{
    //public override void iFunc(){Console.WriteLine($"{nameof(iFunc)}_{nameof(C0)}: ");}
    public new int num = 0;
    public new static int snum = 0;
    public override void vFunc(){Console.WriteLine($"{nameof(vFunc)}_{nameof(C0)}: ");}
    public new static void sFunc(){Console.WriteLine($"{nameof(sFunc)}_{nameof(C0)}:");}
    static void M(string[] args){
        //((C3)new C0()).vFunc();
        //Other.other();
        //Console.WriteLine(new C3().num);
        //Console.WriteLine(C2.snum);
        //C0.sFunc();
        
    }
}
public class Other : Object{
    static bool @is<T, U>(T obj)where U: class=> obj as U != null;
    public static void other(){
        //((C3)new C0()).iImpFunc();
        Console.WriteLine(@is<C1,I0>(new C1()));
    }
}

