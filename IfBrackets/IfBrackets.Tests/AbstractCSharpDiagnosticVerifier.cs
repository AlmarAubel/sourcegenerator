using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing;
using Roslynator.Testing.CSharp;
using Roslynator.Testing.CSharp.Xunit;

namespace IfBrackets.Tests;

public abstract class AbstractCSharpDiagnosticVerifier<TAnalyzer, TFixProvider> : XunitDiagnosticVerifier<TAnalyzer, TFixProvider>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TFixProvider : CodeFixProvider, new()
{
    public abstract DiagnosticDescriptor Descriptor { get; }

    public override CSharpTestOptions Options => CSharpTestOptions.Default;

    public async Task VerifyDiagnosticAsync(
        string source,
        IEnumerable<string> additionalFiles = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var code = TestCode.Parse(source);

        var data = new DiagnosticTestData(
            Descriptor,
            code.Value,
            code.Spans,
            code.AdditionalSpans
           // //additionalFiles: AdditionalFile.CreateRange(additionalFiles)
           );

        await VerifyDiagnosticAsync(
            data,
            options: options,
            cancellationToken: cancellationToken);
    }

    public async Task VerifyDiagnosticAsync(
        string source,
        string sourceData,
        IEnumerable<string> additionalFiles = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var code = TestCode.Parse(source, sourceData);

        var data = new DiagnosticTestData(
            Descriptor,
            source,
            code.Spans,
            code.AdditionalSpans);
            //additionalFiles: AdditionalFile.CreateRange(additionalFiles));

        await VerifyDiagnosticAsync(
            data,
            options: options,
            cancellationToken: cancellationToken);
    }

    internal async Task VerifyDiagnosticAsync(
        string source,
        TextSpan span,
        IEnumerable<string> additionalFiles = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var data = new DiagnosticTestData(
            Descriptor,
            source,
            ImmutableArray.Create(span));
            //additionalFiles: AdditionalFile.CreateRange(additionalFiles));

        await VerifyDiagnosticAsync(
            data,
            options: options,
            cancellationToken: cancellationToken);
    }

    internal async Task VerifyDiagnosticAsync(
        string source,
        IEnumerable<TextSpan> spans,
        IEnumerable<string> additionalFiles = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var data = new DiagnosticTestData(
            Descriptor,
            source,
            spans
            //additionalFiles: AdditionalFile.CreateRange(additionalFiles)
            );

        await VerifyDiagnosticAsync(
            data,
            options: options,
            cancellationToken: cancellationToken);
    }

    public async Task VerifyNoDiagnosticAsync(
        string source,
        string sourceData,
        IEnumerable<string> additionalFiles = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var code = TestCode.Parse(source, sourceData);

        var data = new DiagnosticTestData(
            Descriptor,
            code.Value,
            spans: null,
            code.AdditionalSpans);
            //AdditionalFile.CreateRange(additionalFiles));

        await VerifyNoDiagnosticAsync(
            data,
            options: options,
            cancellationToken);
    }

    public async Task VerifyNoDiagnosticAsync(
        string source,
        IEnumerable<string> additionalFiles = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var data = new DiagnosticTestData(
            Descriptor,
            source,
            spans: null);
            //additionalFiles: AdditionalFile.CreateRange(additionalFiles));

        await VerifyNoDiagnosticAsync(
            data,
            options: options,
            cancellationToken);
    }

   
    public async Task VerifyDiagnosticAndNoFixAsync(
        string source,
        IEnumerable<(string source, string expectedSource)> additionalFiles = null,
        string equivalenceKey = null,
        TestOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var code = TestCode.Parse(source);

        var data = new DiagnosticTestData(
            Descriptor,
            code.Value,
            code.Spans,
            additionalSpans: code.AdditionalSpans,
            //additionalFiles: AdditionalFile.CreateRange(additionalFiles),
            equivalenceKey: equivalenceKey);

        await VerifyDiagnosticAndNoFixAsync(data, options, cancellationToken);
    }

   
}