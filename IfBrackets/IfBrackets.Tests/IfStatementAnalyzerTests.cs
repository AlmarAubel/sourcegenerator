using System.Threading.Tasks;
using IfBrackets;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<IfBrackets.IfStatementAnalyzer,
        IfBrackets.IfWithoutBracketsCodeFixProvider>;
using Xunit;

public class IfStatementTests
{
    [Fact]
    public async Task IfStatementWithoutBracketsOnDifferentLine_ShouldWarn()
    {
        var testCode = @"
using System;
class TestClass
{
    void TestMethod()
    {
        if (true)
            Console.WriteLine(""True"");
    }
}";

        var expected = Verifier.Diagnostic(IfStatementAnalyzer.DiagnosticId)
            .WithLocation(8, 13)
            .WithMessage("If statement without brackets can lead to confusion");

        await Verifier.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task IfStatementWithoutBracketsOnDifferentLine_ShouldFix()
    {
        var testCode = @"
class TestClass
{
    void TestMethod()
    {
        if (true)
            Console.WriteLine(""True"");
    }
}";

        var fixedCode = @"
class TestClass
{
    void TestMethod()
    {
        if (true)
        {
            Console.WriteLine(""True"");
        }
    }
}";

        var expected = Verifier.Diagnostic(IfStatementAnalyzer.DiagnosticId)
            .WithLocation(7, 13)
            .WithMessage("If statement without brackets can lead to confusion");

        await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
    }
}