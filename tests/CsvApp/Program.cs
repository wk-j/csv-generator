using System;
using System.IO;

namespace CsvApp {
    [CsvGenerator(Template = "A.csv")]
    partial class A {

    }

    [CsvGenerator(Template = "B.csv")]
    partial class B {

    }

    [CsvGenerator(Template = "D.csv")]
    partial class D {

    }

    class Program {
        static void A() {
            var csv = File.ReadAllText("resource/csv/A.csv");
            var data = ALoader.Parse(csv);
            foreach (var item in data) {
                Console.WriteLine("{0,10} {1,10} {2,10}", item.Id, item.FirstName, item.LastName);
            }
        }

        static void D() {
            var csv = File.ReadAllText("resource/csv/D.csv");
            var data = DLoader.Parse(csv);
            foreach (var item in data) {
                Console.WriteLine("{0,10} {1,10} {2,10} {3,10}",
                    item.Company,
                    item.ContractCreateDate,
                    item.Name,
                    item.FileName);
            }
        }

        static void Main(string[] args) {
            D();
        }
    }
}