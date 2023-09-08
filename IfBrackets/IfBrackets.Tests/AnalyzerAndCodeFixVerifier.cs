using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Reflection;

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
            TestState.AdditionalReferences.AddRange(GetReferences());
        }


        private ImmutableArray<MetadataReference> GetReferences()
        {
            return new[]
                {
                    typeof(object), // System.Private.CoreLib
                    typeof(Console), // System
                    typeof(Uri), // System.Private.Uri
                    typeof(Enumerable), // System.Linq
                    typeof(CSharpCompilation), // Microsoft.CodeAnalysis.CSharp
                    typeof(Compilation), // Microsoft.CodeAnalysis
                }.Select(type => type.GetTypeInfo().Assembly.Location)
                .Append(GetSystemAssemblyPathByName("System.Globalization.dll"))
                .Append(GetSystemAssemblyPathByName("System.Text.RegularExpressions.dll"))
                .Append(GetSystemAssemblyPathByName("System.Runtime.Extensions.dll"))
                .Append(GetSystemAssemblyPathByName("System.Data.Common.dll"))
                .Append(GetSystemAssemblyPathByName("System.Threading.Tasks.dll"))
                .Append(GetSystemAssemblyPathByName("System.Runtime.dll"))
                .Append(GetSystemAssemblyPathByName("System.Reflection.dll"))
                .Append(GetSystemAssemblyPathByName("System.Xml.dll"))
                .Append(GetSystemAssemblyPathByName("System.Xml.XDocument.dll"))
                .Append(GetSystemAssemblyPathByName("System.Private.Xml.Linq.dll"))
                .Append(GetSystemAssemblyPathByName("System.Linq.Expressions.dll"))
                .Append(GetSystemAssemblyPathByName("System.Collections.dll"))
                .Append(GetSystemAssemblyPathByName("netstandard.dll"))
                .Append(GetSystemAssemblyPathByName("System.Xml.ReaderWriter.dll"))
                .Append(GetSystemAssemblyPathByName("System.Private.Xml.dll"))
                .Select(location => (MetadataReference)MetadataReference.CreateFromFile(location))
                .ToImmutableArray();

            string GetSystemAssemblyPathByName(string assemblyName)
            {
                var root = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
                return System.IO.Path.Combine(root, assemblyName);
            }
        }
    }
}