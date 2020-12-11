using System;
using System.IO;
using System.Linq;
using CsvGenerator;
using Sprache;
using Xunit;
using Xunit.Abstractions;

namespace Tests {
    public class UnitTest1 {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper o) => _output = o;

        private static string GetResource(string path) =>
            File.ReadAllText(Path.Combine("../../../../../resource", path));

        [Fact]
        public void Csv() {
            var text = GetResource("csv/A.csv");
            var csv = CsvParser.Csv.Parse(text);

            _output.WriteLine(string.Empty);

            var data = csv.Skip(1);
            foreach (var item in data) {
                var l = item.ToList();
                _output.WriteLine($"{l[0]}-{l[1]}-{l[2]}");
            }
        }

        [Fact]
        public void Head() {
            var text = GetResource("csv/A.csv");
            var heads = CsvParser.Header(text).ToList();
            foreach (var item in heads) {
                _output.WriteLine(item);
            }
            Assert.Equal("Id", heads[0]);
            Assert.Equal("FirstName", heads[1]);
            Assert.Equal("LastName", heads[2]);
        }
    }
}
