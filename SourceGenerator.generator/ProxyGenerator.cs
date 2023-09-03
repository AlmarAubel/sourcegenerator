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
        // var records = compilation.SyntaxTrees
        //     .SelectMany(tree => tree.GetRoot().DescendantNodes())
        //     .OfType<RecordDeclarationSyntax>()
        //     .Where(r => r.BaseList?.Types.Any(t => t.ToString() == "IRequest") == true)
        //     .ToList();

        var records = compilation.SyntaxTrees
            .SelectMany(tree => GetAllRecords(tree.GetRoot()))
            .Where(r => r.BaseList?.Types.Any(t => t.ToString() == "IRequest") == true)
            .ToList();

        if (!records.Any()) return;
        var classSource = GenerateProxyClass(records, compilation);
        context.AddSource("FooBarProxy", SourceText.From(classSource, Encoding.UTF8));
    }


    private IEnumerable<RecordDeclarationSyntax> GetAllRecords(SyntaxNode node)
    {
        foreach (var child in node.ChildNodes())
        {
            if (child is RecordDeclarationSyntax record)
            {
                yield return record;
            }

            foreach (var nestedRecord in GetAllRecords(child))
            {
                yield return nestedRecord;
            }
        }
    }

    private string GenerateProxyClass(List<RecordDeclarationSyntax> records, Compilation compilation)
    {
        var methods = string.Join("\n", records.Select(record =>
        {
            var recordName = record.Identifier.Text;
            var enclosingTypeName = (record.Parent as ClassDeclarationSyntax)?.Identifier.Text;

            // Adjust the method name to include the enclosing type's name if present
            var methodName = enclosingTypeName != null ? $"{enclosingTypeName}_{recordName}" : recordName;

            var recordSymbol = compilation.GetSemanticModel(record.SyntaxTree).GetDeclaredSymbol(record) as INamedTypeSymbol;
            var constructorParameters = recordSymbol?.InstanceConstructors.FirstOrDefault()?.Parameters;
            var parameterDeclarations = string.Join(", ", constructorParameters?.Select(p => $"{p.Type} {p.Name}") ?? Enumerable.Empty<string>());
            var parameterValues = string.Join(", ", constructorParameters?.Select(p => p.Name) ?? Enumerable.Empty<string>());
            var nestedRecordName = enclosingTypeName != null ? $"{enclosingTypeName}.{recordName}" : recordName;
            return $$"""
                     public void Execute{{methodName}}({{parameterDeclarations}})
                     {
                         var request = new {{nestedRecordName}}({{parameterValues}});
                         _mediator.Execute(request);
                     }
                     """;
        }));

        var classCode = $$"""
                          namespace SourceGenerator
                          {
                              public partial class FooBarProxy
                              {
                                  private IMediator _mediator;
                          
                                  public FooBarProxy(IMediator mediator)
                                  {
                                      _mediator = mediator;
                                  }
                          
                                  {{methods}}
                              }
                          }
                          """;

        var syntaxTree = CSharpSyntaxTree.ParseText(classCode);
        var formattedNode = syntaxTree.GetRoot().NormalizeWhitespace();
        return formattedNode.ToFullString();
    }
}