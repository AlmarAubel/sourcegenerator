using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace IfBrackets.Tests;

public static class AnalyzerAndCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>
            .Diagnostic(diagnosticId);
    }

    public static async Task VerifyCodeFixAsync(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
    {
        var test = new CodeFixTest(source, fixedSource, expected);
        await test.RunAsync(CancellationToken.None);
    }

    private class CodeFixTest : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
    {
        public CodeFixTest(
            string source,
            string fixedSource,
            params DiagnosticResult[] expected)
        {
            TestCode = source;
            FixedCode = fixedSource;
            ExpectedDiagnostics.AddRange(expected);

            ReferenceAssemblies = new ReferenceAssemblies(
                "net6.0",
                new PackageIdentity("CSharpFunctionalExtensions", "2.40.1"),
                Path.Combine("ref", "net6.0"));


            //TestState.AdditionalReferences.Add(typeof(EnumGenerationAttribute).Assembly);
        }
    }
}

public static class AnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);
    }

    public static async Task VerifyAnalyzerAsync(
        string source,
        params DiagnosticResult[] expected)
    {
        var test = new AnalyzerTest(source, expected);
        await test.RunAsync(CancellationToken.None);
    }

    private class AnalyzerTest : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    {
        public AnalyzerTest(
            string source,
            params DiagnosticResult[] expected)
        {
            TestCode = source;
            ExpectedDiagnostics.AddRange(expected);
           
            ReferenceAssemblies = new ReferenceAssemblies(
                "net6.0",
                new PackageIdentity("CSharpFunctionalExtensions", "2.4.1"),
                Path.Combine("ref", "net6.0"));
            var systemReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location); // For System
            TestState.AdditionalReferences.Add(systemReference);
            TestState.AdditionalReferences.Add(typeof(string).Assembly);
            TestState.AdditionalReferences.Add(typeof(Console).Assembly);
        }
    }
}