using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator.generator;

[Generator]
public class ProxyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;

        // Find all records implementing IRequest
        var records = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes())
            .OfType<RecordDeclarationSyntax>()
            .Where(r => r.BaseList?.Types.Any(t => t.ToString() == "IRequest") == true)
            .ToList();

        if (!records.Any()) return;
        var classSource = GenerateProxyClass(records);
        context.AddSource("FooBarProxy", SourceText.From(classSource, Encoding.UTF8));
    }

    private string GenerateProxyClass(List<RecordDeclarationSyntax> records)
    {
        var methods = string.Join("\n", records.Select(record =>
        {
            var recordName = record.Identifier.Text;
            return $@"
public void Execute{recordName}({recordName} request)
{{
    _mediator.Execute(request);
}}";
        }));

        var classCode = $@"
namespace SourceGenerator
{{
    public class FooBarProxy
    {{
        private IMediator _mediator;

        public FooBarProxy(IMediator mediator)
        {{
            _mediator = mediator;
        }}

        {methods}
    }}
}}";

        var syntaxTree = CSharpSyntaxTree.ParseText(classCode);
        var formattedNode = syntaxTree.GetRoot().NormalizeWhitespace();
        return formattedNode.ToFullString();
    }
}