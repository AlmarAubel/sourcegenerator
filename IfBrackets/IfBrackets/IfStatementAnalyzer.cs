using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IfBrackets;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IfStatementAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SB0001";
    private const string Title = "If statement without brackets";
    private const string MessageFormat = "If statement without brackets can lead to confusion";
    private const string Category = "Syntax";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri:"https://www.youtube.com/watch?v=8r8D8RLxvkA");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.Statement is BlockSyntax)
            return;

        var ifKeywordLine = ifStatement.IfKeyword.GetLocation().GetLineSpan().StartLinePosition.Line;
        var statementLine = ifStatement.Statement.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (ifKeywordLine != statementLine)
        {
            var diagnostic = Diagnostic.Create(Rule, ifStatement.Statement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}