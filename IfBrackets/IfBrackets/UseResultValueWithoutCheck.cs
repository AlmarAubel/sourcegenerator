using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IfBrackets;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseResultValueWithoutCheck : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SB0003";
    private const string Title = "Check IsSuccess or IsError before accessing Value from result object";
    private const string MessageFormat = "Accessing Value without checking IsSuccess or IsError can result in an error";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "https://www.youtube.com/watch?v=8r8D8RLxvkA");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);


    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;
        var typeSymbol = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;

        if (memberAccess.Name.ToString() == "Value" && IsOfType(typeSymbol, "CSharpFunctionalExtensions", "Result"))
        {
            var containingMethod = memberAccess.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            
            
            if (containingMethod == null)
                return;

            if (memberAccess.Expression is IdentifierNameSyntax variableName &&
                !HasSuccessOrErrorCheck(memberAccess, variableName,context.SemanticModel))
            {
                var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private bool IsOfType(ITypeSymbol type, string namespaceName, string typeName)
    {
        return type?.ContainingNamespace.ToString() == namespaceName && type.Name == typeName;
    }


    private bool HasSuccessOrErrorCheck(	MemberAccessExpressionSyntax method, IdentifierNameSyntax variable, SemanticModel semanticModel)
    {
        // Dit is een simplistische controle, dit kan nog verder verbeterd worden.
        var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(method);

        if (!dataFlowAnalysis.Succeeded) return false;
        
        // Check if the variable is ever checked for IsSuccess or IsError before the access
        foreach (var reference in dataFlowAnalysis.ReadInside)
        {
            if (reference.Name == "IsSuccess" || reference.Name == "IsError")
            {
                foreach (var location in reference.Locations)
                {
                    var refSyntax = location.SourceTree?.GetRoot().FindNode(location.SourceSpan);
                    var memberAccess = refSyntax?.Parent as MemberAccessExpressionSyntax;

                    if (memberAccess?.Expression is IdentifierNameSyntax identifier &&
                        identifier.Identifier.Text == variable.Identifier.Text)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}