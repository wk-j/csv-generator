using System;
using System.IO;

namespace CsvApp {
    [CsvGenerator(Template = "A.csv")]
    partial class A {

    }

    [CsvGenerator(Template = "B.csv")]
    partial class B {

    }

    class Program {
        static void Main(string[] args) {
            var csv = File.ReadAllText("resource/csv/A.csv");
            var data = ALoader.Parse(csv);
            foreach (var item in data) {
                Console.WriteLine("{0,10} {1,10} {2,10}", item.Id, item.FirstName, item.LastName);
            }
        }
    }
}