using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Sprache;

[assembly: InternalsVisibleToAttribute("Tests")]

namespace CsvGenerator {
    [Generator]
    internal class TypeGenerator : ISourceGenerator {
        private const string AttributeName = "CsvGeneratorAttribute";
        private static string AttributeText = $@"
using System;
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class {AttributeName} : Attribute {{
    public string Template {{ set; get; }}
}}
";

        private AdditionalText GetJsonSettings(GeneratorExecutionContext context, string fileName) =>
            context.AdditionalFiles
                .FirstOrDefault(x => x.Path.EndsWith(fileName));

        private List<INamedTypeSymbol> GetClassSymbals(GeneratorExecutionContext context, SyntaxReceiver receiver) {
            var options = (context.Compilation as CSharpCompilation)
                .SyntaxTrees[0].Options as CSharpParseOptions;

            var tree = CSharpSyntaxTree.ParseText(SourceText.From(AttributeText, Encoding.UTF8), options);
            var compilation = context.Compilation.AddSyntaxTrees(tree);
            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeName);
            var classSymbals = new List<INamedTypeSymbol>();

            foreach (var item in receiver.CandidateClasses) {
                var model = compilation.GetSemanticModel(item.SyntaxTree);
                var classSymbal = model.GetDeclaredSymbol(item);
                if (classSymbal.GetAttributes().Any(x => x.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default))) {
                    classSymbals.Add(classSymbal);
                }
            }
            return classSymbals;
        }

        private SourceText CreateLoaderSource(INamedTypeSymbol classSymbol, IEnumerable<string> headers) {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            var sb = new StringBuilder();
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Sprache;");
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"--internal static class {className}Loader {{");
            sb.AppendLine($"----internal static IEnumerable<{className}> Parse(string csvContent) {{");
            sb.AppendLine($"------var csv = CsvGenerator.CsvParser.Csv.Parse(csvContent).Skip(1);");
            sb.AppendLine($"------foreach(var item in csv) {{");
            sb.AppendLine($"--------var x = new {className}();");

            foreach (var (item, i) in headers.Select((x, i) => (x, i))) {
                sb.AppendLine($"--------x.{item} = item.ElementAt({i});");
            }

            sb.AppendLine($"--------yield return x;");
            sb.AppendLine($"------}}");
            sb.AppendLine($"----}}");
            sb.AppendLine($"--}}");
            sb.AppendLine($"}}");

            var classDefinition = sb.ToString().Replace("--", "  ");
            return SourceText.From(classDefinition, Encoding.UTF8);
        }

        private IEnumerable<(string, SourceText)> CreateSources(INamedTypeSymbol classSymbol, GeneratorExecutionContext context) {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            var att = classSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == AttributeName);
            var templateAttribute = att.NamedArguments[0];
            var templateValue = templateAttribute.Value.Value as string;

            var settingFile = GetJsonSettings(context, templateValue);
            var csv = settingFile.GetText().ToString();
            var headers = CsvParser.Header(csv);

            var sb = new StringBuilder();
            sb.AppendLine($"namespace {namespaceName} {{");
            sb.AppendLine($"--partial class {classSymbol.Name} {{");

            foreach (var item in headers) {
                sb.AppendLine($"----public string {item} {{ set; get; }}");
            }

            sb.AppendLine("--}");
            sb.AppendLine("}");

            var classDefinition = sb.ToString().Replace("--", "  ");

            yield return (className, SourceText.From(classDefinition, Encoding.UTF8));
            yield return (className + "Loader", CreateLoaderSource(classSymbol, headers));
        }

        public void Execute(GeneratorExecutionContext context) {
            InjectAttribute(context);
            InjectResourceClass(context);

            if (context.SyntaxReceiver is not SyntaxReceiver receiver) {
                return;
            }

            var symbols = GetClassSymbals(context, receiver);

            foreach (var item in symbols) {
                var sources = CreateSources(item, context);
                foreach (var (name, source) in sources) {
                    context.AddSource(name + ".Csv.g.cs", source);
                }
            }
        }

        private void InjectResourceClass(GeneratorExecutionContext context) {
            var asm = Assembly.GetCallingAssembly();
            var name = Array.Find(asm.GetManifestResourceNames(), x => x.EndsWith("CsvParser.cs"));
            using var stream = asm.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var source = SourceText.From(text, Encoding.UTF8);
            context.AddSource("CsvParser.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private static void InjectAttribute(GeneratorExecutionContext context) {
            context.AddSource("CsvAttribute.g.cs", SourceText.From(AttributeText, Encoding.UTF8));
        }
    }
}