using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

public  class IfStatementsTests
{
    [Fact]
    public  void Test()
    {
        var code = @"
        public class Test
        {
            public void Method()
            {
                int a = 0;
                var idFromDbResult = new { IsSuccess = true };
                if (1 > a && idFromDbResult.IsSuccess)
                {
                    // Do something
                }
            }
        }";

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var ifStatement = root.DescendantNodes().OfType<IfStatementSyntax>().First();
        var condition = ifStatement.Condition;

        bool willExecuteWhenIsSuccessTrue = WillExecuteWhenIsSuccessTrue(condition);
        Console.WriteLine($"Will the body execute when IsSuccess is true? {willExecuteWhenIsSuccessTrue}");
    }

    static bool WillExecuteWhenIsSuccessTrue(ExpressionSyntax condition)
    {
        if (condition is BinaryExpressionSyntax binaryExpression)
        {
            switch (binaryExpression.OperatorToken.Kind())
            {
                case SyntaxKind.AmpersandAmpersandToken:
                    return WillExecuteWhenIsSuccessTrue(binaryExpression.Left) || WillExecuteWhenIsSuccessTrue(binaryExpression.Right);
                case SyntaxKind.BarBarToken:
                    return WillExecuteWhenIsSuccessTrue(binaryExpression.Left) && WillExecuteWhenIsSuccessTrue(binaryExpression.Right);
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

        return false;
    }
}