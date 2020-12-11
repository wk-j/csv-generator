using System;
using System.IO;

namespace MyLib {
    [CsvGenerator(Template = "B.csv")]
    public partial class Industry { }

    public class Program {
        public static void Main() {
            var csv = File.ReadAllText("resource/csv/B.csv");
            var data = IndustryLoader.Parse(csv);

            foreach (var item in data) {
                Console.WriteLine($"{item.Year} {item.VariableCategory,50}");
            }
        }
    }
}