using System;

namespace CsvApp {
    [Csv(Template = "A.csv")]
    partial class A {

    }

    [Csv(Template = "B.csv")]
    partial class B {

    }

    class Program {
        static void Main(string[] args) {
            var a = new A();
            a.FirstName = "a";
            a.LastName = "l";
            a.Id = "1";

            var b = new B();
        }
    }
}
