using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Sprache;

namespace CsvGenerator {
    [Generator]
    internal class TypeGenerator : ISourceGenerator {
        private const string AttributeText = @"
using System;
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class CsvAttribute : Attribute {
    public string Template { set;get;}
}
";

        private AdditionalText GetJsonSettings(GeneratorExecutionContext context, string fileName) =>
            context.AdditionalFiles
                .FirstOrDefault(x => x.Path.EndsWith(fileName));

        private List<INamedTypeSymbol> GetClassSymbals(GeneratorExecutionContext context, SyntaxReceiver receiver) {
            var options = (context.Compilation as CSharpCompilation)
                .SyntaxTrees[0].Options as CSharpParseOptions;

            var tree = CSharpSyntaxTree.ParseText(SourceText.From(AttributeText, Encoding.UTF8), options);
            var compilation = context.Compilation.AddSyntaxTrees(tree);
            var attributeSymbol = compilation.GetTypeByMetadataName("CsvAttribute");
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

        private SourceText CreateAppSettingsSource(INamedTypeSymbol classSymbol, GeneratorExecutionContext context) {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var att = classSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == "CsvAttribute");
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

            return SourceText.From(classDefinition, Encoding.UTF8);
        }

        public void Execute(GeneratorExecutionContext context) {
            InjectAttribute(context);

            if (context.SyntaxReceiver is not SyntaxReceiver receiver) {
                return;
            }

            var symbols = GetClassSymbals(context, receiver);
            foreach (var item in symbols) {
                var source = CreateAppSettingsSource(item, context);
                context.AddSource(item.Name + ".Csv.g.cs", source);
            }
        }

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private static void InjectAttribute(GeneratorExecutionContext context) {
            context.AddSource("CsvAttribute.g.cs", SourceText.From(AttributeText, Encoding.UTF8));
        }
    }
}