using System;

namespace CsvApp {
    [CsvGenerator(Template = "A.csv")]
    partial class A {

    }

    [CsvGenerator(Template = "B.csv")]
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