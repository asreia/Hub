using System;



Console.WriteLine(new Person("namae", default).Func());



record Person(string Name, DateTime Birthday)
{
    public Person() : this("", default(DateTime)) { }
    public string Func() //関数が書けない訳ではない
    {
        Console.WriteLine("Func");
        return "abc";
    }
}
