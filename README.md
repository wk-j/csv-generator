## Csv Typ Generator for C# #

[![Actions](https://github.com/wk-j/csv-generator/workflows/NuGet/badge.svg)](https://github.com/wk-j/csv-generator/actions)
[![NuGet](https://img.shields.io/nuget/v/wk.CsvGenerator.svg)](https://www.nuget.org/packages/wk.CsvGenerator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/wk.CsvGenerator.svg)](https://www.nuget.org/packages/wk.CsvGenerator)

## Usage

1. Install package

```bash
dotnet add package wk.CsvGenerator
```

2. Include CSV template as additional file in `.csproj`

```xml
  <ItemGroup>
    <AdditionalFiles Include="B.csv" />
  </ItemGroup>
```

3. Create partial class and place `CsvGeneratorAttribute`
4. Parse CSV with loader utility

```csharp
using System;
using System.IO;

namespace MyApp {
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
```
