using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace IfBrackets;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseResultValueWithoutCheck : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SB0003";
    private const string Title = "Check IsSuccess or IsError before accessing Value from result object";
    private const string MessageFormat = "Accessing Value without checking IsSuccess or IsError can result in an error";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, helpLinkUri: "https://www.youtube.com/watch?v=8r8D8RLxvkA");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);


    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        if (memberAccess.Name.ToString() != "Value") return;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
        var memberSymbol = symbolInfo.Symbol as IPropertySymbol;
        
        if (memberSymbol == null || memberSymbol.ContainingType.ToString() != "CSharpFunctionalExtensions.Result<int, string>") return;
        
        // Get the enclosing block (e.g., the method or property body)
        var enclosingBlock = memberAccess.Ancestors().FirstOrDefault(a => a is BlockSyntax);
        
        if (enclosingBlock == null) return;
        
        var dataFlow = context.SemanticModel.AnalyzeDataFlow(enclosingBlock);
        if (!dataFlow.Succeeded) return;
        // Check if the nullable variable is always assigned a value before it's accessed
        if (dataFlow.AlwaysAssigned.Contains(memberSymbol))
        {
            return; // It's safe, no need to report a diagnostic
        }

        // Check if the nullable variable is checked for HasValue or null before it's accessed
        var checks = enclosingBlock.DescendantNodes()
            .OfType<IfStatementSyntax>()
            .Where(ifStatement =>
                ifStatement.Condition.ToString().Contains(memberAccess.Expression + ".IsSuccess") ||
                ifStatement.Condition.ToString().Contains(memberAccess.Expression + ".IsFailure"))
            .Where(ifStatement=>ContainsTerminatingStatement(ifStatement.Statement))
            .ToList();

        if (checks.Any()) return;
 
        var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), memberAccess.Expression);
        context.ReportDiagnostic(diagnostic);
    }

    private bool ContainsTerminatingStatement(StatementSyntax statement)
    {
        // Check for return or throw statements directly within the provided statement
        if (statement is ReturnStatementSyntax || statement is ThrowStatementSyntax)
            return true;

        // If the statement is a block, check its child statements
        if (statement is BlockSyntax block)
        {
            foreach (var childStatement in block.Statements)
            {
                if (childStatement is ReturnStatementSyntax || childStatement is ThrowStatementSyntax)
                    return true;
            }
        }

        return false;
    }
    public static string GetFullLineOfSymbol(ISymbol symbol)
    {
        var declaringSyntaxReference = symbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (declaringSyntaxReference == null)
            return null;

        var syntaxTree = declaringSyntaxReference.SyntaxTree;
        var span = declaringSyntaxReference.Span;
        var lineSpan = syntaxTree.GetLineSpan(span);
        var lineNumber = lineSpan.StartLinePosition.Line; // or lineSpan.EndLinePosition.Line
        var textLine = syntaxTree.GetText().Lines[lineNumber];
        return textLine.ToString();
    }
}