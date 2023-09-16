// Copyright (c) Josef Pihrt and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;

namespace IfBrackets.Tests;

public sealed class DummyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => throw new NotSupportedException();

    public override Task RegisterCodeFixesAsync(CodeFixContext context) => throw new NotSupportedException();

    public override FixAllProvider GetFixAllProvider() => throw new NotSupportedException();
}


public readonly struct AdditionalFile
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdditionalFile"/>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="expectedSource"></param>
    public AdditionalFile(string source, string expectedSource = null)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        ExpectedSource = expectedSource;
    }

    /// <summary>
    /// Gets a source code.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets expected source code.
    /// </summary>
    public string ExpectedSource { get; }

 
    private string DebuggerDisplay => Source;

    public static ImmutableArray<AdditionalFile> CreateRange(IEnumerable<string> additionalFiles)
    {
        return additionalFiles?.Select(f => new AdditionalFile(f)).ToImmutableArray()
               ?? ImmutableArray<AdditionalFile>.Empty;
    }

    public static ImmutableArray<AdditionalFile> CreateRange(IEnumerable<(string source, string expectedSource)> additionalFiles)
    {
        return additionalFiles?.Select(f => new AdditionalFile(f.source, f.expectedSource)).ToImmutableArray()
               ?? ImmutableArray<AdditionalFile>.Empty;
    }
}