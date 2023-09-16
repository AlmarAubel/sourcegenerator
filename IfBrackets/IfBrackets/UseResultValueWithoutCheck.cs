using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace IfBrackets;

//https://github.com/dotnet/roslyn-analyzers/blob/0b21b2163220669981f682e58a8ddcdc9a839774/src/Utilities.UnitTests/FlowAnalysis/Analysis/PropertySetAnalysis/PropertySetAnalysisTests.cs
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseResultValueWithoutCheck : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SB0003";
    private const string Title = "Check IsSuccess or IsError before accessing Value from result object";
    private const string MessageFormat = "Accessing Value without checking IsSuccess or IsError can result in an error";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
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

        // Check if the Value variable is checked is succes and returned before accessing it
        var checks = enclosingBlock.DescendantNodes()
            .OfType<IfStatementSyntax>()
            .Where(ifStatement =>
                ifStatement.Condition.ToString().Contains(memberAccess.Expression + ".IsSuccess") ||
                ifStatement.Condition.ToString().Contains(memberAccess.Expression + ".IsFailure"))
            .Where(ifStatement => ContainsTerminatingStatement(ifStatement.Statement))
            .ToList();

        if (checks.Any()) return;

        //Check if accessed inside if statement
        var enclosingControlStructures = memberAccess.Ancestors().Where(a => a is IfStatementSyntax or ConditionalExpressionSyntax);
       

        var checksSucces = enclosingControlStructures
            .Where(structure =>
                (structure is IfStatementSyntax ifStatement && WillExecuteWhenIsSuccess(ifStatement.Condition)) ||
                (structure is ConditionalExpressionSyntax ternary && WillExecuteWhenIsSuccess(ternary.Condition)))
            .ToList();

        if (checksSucces.Any()) return;

        var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), memberAccess.Expression);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool WillExecuteWhenIsSuccess(ExpressionSyntax condition)
    {
        if (condition is BinaryExpressionSyntax binaryExpression)
        {
            switch (binaryExpression.OperatorToken.Kind())
            {
                case SyntaxKind.AmpersandAmpersandToken:
                    return WillExecuteWhenIsSuccess(binaryExpression.Left) || WillExecuteWhenIsSuccess(binaryExpression.Right);
                case SyntaxKind.BarBarToken:
                    return WillExecuteWhenIsSuccess(binaryExpression.Left) && WillExecuteWhenIsSuccess(binaryExpression.Right);
            }
        }
        else if (condition is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.ToString() == "IsSuccess")
        {
            return true;
        }
        else if (condition is PrefixUnaryExpressionSyntax prefixUnary && prefixUnary.Operand.ToString().Contains("IsSuccess"))
        {
            return false; // This means we found a !IsSuccess, so we return false.
        }
        else if (condition is ConditionalExpressionSyntax ternary)
        {
            return WillExecuteWhenIsSuccess(ternary.Condition);
        }

        return false;
    }

    private bool ContainsTerminatingStatement(StatementSyntax statement)
    {
        // Check for return or throw statements directly within the provided statement
        if (statement is ReturnStatementSyntax or ThrowStatementSyntax)
            return true;

        // If the statement is a block, check its child statements
        if (statement is BlockSyntax block)
        {
            return block.Statements.Any(childStatement => childStatement is ReturnStatementSyntax or ThrowStatementSyntax);
        }

        return false;
    }
}