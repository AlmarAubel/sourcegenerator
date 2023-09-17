﻿using Microsoft.CodeAnalysis;

namespace SourceGenerator.generator;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Find the main method
        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

        // Build up the source code
        string source = $$"""
                          // <auto-generated/>
                          using System;

                          namespace {{mainMethod.ContainingNamespace.ToDisplayString()}};
                          
                          //{{mainMethod.ContainingType.Name}}
                          public static class Almar
                          {
                             public static void HelloFrom(string name) =>
                                  Console.WriteLine($"Generator says: Hi from '{name}'");
                          }
                          """;
        var typeName = mainMethod.ContainingType.Name;

        // Add the source code to the compilation
        context.AddSource($"almar.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}

