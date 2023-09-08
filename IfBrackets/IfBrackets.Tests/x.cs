// using System;
// using System.Collections.Generic;
// using System.Collections.Immutable;
// using System.Linq;
// using System.Reflection;
// using System.Text;
// using System.Threading;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CodeActions;
// using Microsoft.CodeAnalysis.CodeFixes;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Diagnostics;
// using Microsoft.CodeAnalysis.Simplification;
// using Microsoft.CodeAnalysis.Testing;
// using Microsoft.CodeAnalysis.Text;
//
// namespace IfBrackets.Tests;
//
// /// <summary>
//     /// Superclass of all Unit Tests for DiagnosticAnalyzers
//     /// </summary>
//     public static class DiagnosticVerifier
//     {
//         static DiagnosticVerifier()
//         {
//             References = new[]
//             {
//                 typeof(object), // System.Private.CoreLib
//                 typeof(Console), // System
//                 typeof(Uri), // System.Private.Uri
//                 typeof(Enumerable), // System.Linq
//                 typeof(CSharpCompilation), // Microsoft.CodeAnalysis.CSharp
//                 typeof(Compilation), // Microsoft.CodeAnalysis
//             }.Select(type => type.GetTypeInfo().Assembly.Location)
//             .Append(GetSystemAssemblyPathByName("System.Globalization.dll"))
//             .Select(location => (MetadataReference)MetadataReference.CreateFromFile(location))
//             .ToImmutableArray();
//
//             DefaultFilePathPrefix = "Test";
//             CSharpDefaultFileExt = "cs";
//             VisualBasicDefaultExt = "vb";
//             TestProjectName = "TestProject";
//
//             string GetSystemAssemblyPathByName(string assemblyName)
//             {
//                 var root = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
//                 return System.IO.Path.Combine(root, assemblyName);
//             }
//         }
//         // based on http://code.fitness/post/2017/02/using-csharpscript-with-netstandard.html
//         public static string GetSystemAssemblyPathByName(string assemblyName)
//         {
//             var root = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
//             return System.IO.Path.Combine(root, assemblyName);
//         }
//
//         private static readonly ImmutableArray<MetadataReference> References;
//
//         private static readonly string DefaultFilePathPrefix;
//         private static readonly string CSharpDefaultFileExt;
//         private static readonly string VisualBasicDefaultExt;
//         private static readonly string TestProjectName;
//              
//
//         #region  Get Diagnostics
//
//         /// <summary>
//         /// Given classes in the form of strings, their language, and an IDiagnosticAnlayzer to apply to it, return the diagnostics found in the string after converting it to a document.
//         /// </summary>
//         /// <param name="sources">Classes in the form of strings</param>
//         /// <param name="language">The language the source classes are in</param>
//         /// <param name="analyzer">The analyzer to be run on the sources</param>
//         /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
//         private static Diagnostic[] GetSortedDiagnostics(string[] sources, string language, params DiagnosticAnalyzer[] analyzers)
//         {
//             return GetSortedDiagnosticsFromDocuments(analyzers, GetDocuments(sources, language));
//         }
//
//         /// <summary>
//         /// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
//         /// The returned diagnostics are then ordered by location in the source document.
//         /// </summary>
//         /// <param name="analyzers">The analyzer to run on the documents</param>
//         /// <param name="documents">The Documents that the analyzer will be run on</param>
//         /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
//         private static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer[] analyzers, Document[] documents)
//         {
//             var projects = new HashSet<Project>();
//             foreach (var document in documents)
//             {
//                 projects.Add(document.Project);
//             }
//
//             var diagnostics = new List<Diagnostic>();
//             foreach (var project in projects)
//             {
//                 var compilation = project.GetCompilationAsync().Result;
//                 var compilationWithAnalyzers = compilation
//                     .WithOptions(compilation.Options
//                         .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
//                         {
//                             ["CS1701"] = ReportDiagnostic.Suppress, // Binding redirects
//                             ["CS1702"] = ReportDiagnostic.Suppress,
//                             ["CS1705"] = ReportDiagnostic.Suppress,
//                             ["CS8019"] = ReportDiagnostic.Suppress // TODO: Unnecessary using directive
//                         }))
//                     .WithAnalyzers(ImmutableArray.Create(analyzers));
//                 var relevantDiagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
//
//                 var allDiagnostics = compilationWithAnalyzers.GetAllDiagnosticsAsync().Result;
//                 var other = allDiagnostics.Except(relevantDiagnostics).ToArray();
//
//                 var code = documents[0].GetSyntaxRootAsync().Result.ToFullString();
//
//                 other.Should().BeEmpty("there should be no error diagnostics that are not related to the test.{0}code: {1}", Environment.NewLine, code);
//
//                 foreach (var diag in relevantDiagnostics)
//                 {
//                     if (diag.Location == Location.None || diag.Location.IsInMetadata)
//                     {
//                         diagnostics.Add(diag);
//                     }
//                     else
//                     {
//                         for (int i = 0; i < documents.Length; i++)
//                         {
//                             var document = documents[i];
//                             var tree = document.GetSyntaxTreeAsync().Result;
//                             if (tree == diag.Location.SourceTree)
//                             {
//                                 diagnostics.Add(diag);
//                             }
//                         }
//                     }
//                 }
//             }
//
//             var results = SortDiagnostics(diagnostics);
//             diagnostics.Clear();
//             return results;
//         }
//
//         /// <summary>
//         /// Sort diagnostics by location in source document
//         /// </summary>
//         /// <param name="diagnostics">The list of Diagnostics to be sorted</param>
//         /// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
//         private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
//         {
//             return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
//         }
//
//         #endregion
//
//         #region Set up compilation and documents
//         /// <summary>
//         /// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
//         /// </summary>
//         /// <param name="sources">Classes in the form of strings</param>
//         /// <param name="language">The language the source code is in</param>
//         /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
//         private static Document[] GetDocuments(string[] sources, string language)
//         {
//             if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
//             {
//                 throw new ArgumentException("Unsupported Language");
//             }
//
//             var project = CreateProject(sources, language);
//             var documents = project.Documents.ToArray();
//
//             if (sources.Length != documents.Length)
//             {
//                 throw new SystemException("Amount of sources did not match amount of Documents created");
//             }
//
//             return documents;
//         }
//
//         /// <summary>
//         /// Create a Document from a string through creating a project that contains it.
//         /// </summary>
//         /// <param name="source">Classes in the form of a string</param>
//         /// <param name="language">The language the source code is in</param>
//         /// <returns>A Document created from the source string</returns>
//         private static Document CreateDocument(string source, string language = LanguageNames.CSharp)
//         {
//             return CreateProject(new[] { source }, language).Documents.First();
//         }
//
//         /// <summary>
//         /// Create a project using the inputted strings as sources.
//         /// </summary>
//         /// <param name="sources">Classes in the form of strings</param>
//         /// <param name="language">The language the source code is in</param>
//         /// <returns>A Project created out of the Documents created from the source strings</returns>
//         private static Project CreateProject(string[] sources, string language = LanguageNames.CSharp)
//         {
//             string fileNamePrefix = DefaultFilePathPrefix;
//             string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;
//
//             var projectId = ProjectId.CreateNewId(debugName: TestProjectName);
//
//             var solution = new AdhocWorkspace()
//                 .CurrentSolution
//                 .AddProject(projectId, TestProjectName, TestProjectName, language)
//                 .AddMetadataReferences(projectId, References);
//
//             int count = 0;
//             foreach (var source in sources)
//             {
//                 var newFileName = fileNamePrefix + count + "." + fileExt;
//                 var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
//                 solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
//                 count++;
//             }
//             return solution.GetProject(projectId);
//         }
//         #endregion
//
//         #region Verifier wrappers
//
//         /// <summary>
//         /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
//         /// Note: input a DiagnosticResult for each Diagnostic expected
//         /// </summary>
//         /// <param name="source">A class in the form of a string to run the analyzer on</param>
//         /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
//         public static void VerifyCSharpDiagnostic<TDiagnosticAnalyzer>(string source, params DiagnosticResult[] expected) where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
//         {
//             VerifyDiagnostics(new[] { source }, LanguageNames.CSharp, new TDiagnosticAnalyzer(), expected);
//         }
//
//         /// <summary>
//         /// Called to test a VB DiagnosticAnalyzer when applied on the single inputted string as a source
//         /// Note: input a DiagnosticResult for each Diagnostic expected
//         /// </summary>
//         /// <param name="source">A class in the form of a string to run the analyzer on</param>
//         /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source</param>
//         public static void VerifyBasicDiagnostic<TDiagnosticAnalyzer>(string source, params DiagnosticResult[] expected) where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
//         {
//             VerifyDiagnostics(new[] { source }, LanguageNames.VisualBasic, new TDiagnosticAnalyzer(), expected);
//         }
//
//         /// <summary>
//         /// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source
//         /// Note: input a DiagnosticResult for each Diagnostic expected
//         /// </summary>
//         /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
//         /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
//         public static void VerifyCSharpDiagnostic<TDiagnosticAnalyzer>(string[] sources, params DiagnosticResult[] expected) where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
//         {
//             VerifyDiagnostics(sources, LanguageNames.CSharp, new TDiagnosticAnalyzer(), expected);
//         }
//
//         /// <summary>
//         /// Called to test a VB DiagnosticAnalyzer when applied on the inputted strings as a source
//         /// Note: input a DiagnosticResult for each Diagnostic expected
//         /// </summary>
//         /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
//         /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
//         public static void VerifyBasicDiagnostic<TDiagnosticAnalyzer>(string[] sources, params DiagnosticResult[] expected) where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
//         {
//             VerifyDiagnostics(sources, LanguageNames.VisualBasic, new TDiagnosticAnalyzer(), expected);
//         }
//
//         public static void VerifyCSharpDiagnosticUsingAllAnalyzers(string source, params DiagnosticResult[] expected)
//         {
//             var analyzers = CreateAllAnalyzers();
//             var diagnostics = GetSortedDiagnostics(new[] { source }, LanguageNames.CSharp, analyzers);
//             VerifyDiagnosticResults(diagnostics, analyzers, expected);
//         }
//
//         public static void VerifyCSharpDiagnosticUsingAllAnalyzers(string[] sources, params DiagnosticResult[] expected)
//         {
//             var analyzers = CreateAllAnalyzers();
//             var diagnostics = GetSortedDiagnostics(sources, LanguageNames.CSharp, analyzers);
//             VerifyDiagnosticResults(diagnostics, analyzers, expected);
//         }
//
//         /// <summary>
//         /// General method that gets a collection of actual diagnostics found in the source after the analyzer is run,
//         /// then verifies each of them.
//         /// </summary>
//         /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
//         /// <param name="language">The language of the classes represented by the source strings</param>
//         /// <param name="analyzer">The analyzer to be run on the source code</param>
//         /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
//         private static void VerifyDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
//         {
//             var diagnostics = GetSortedDiagnostics(sources, language, analyzer);
//             VerifyDiagnosticResults(diagnostics, analyzer, expected);
//         }
//
//         #endregion
//
//         #region Actual comparisons and verifications
//         /// <summary>
//         /// Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array of expected results.
//         /// Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the DiagnosticResult match the actual diagnostic.
//         /// </summary>
//         /// <param name="actualResults">The Diagnostics found by the compiler after running the analyzer on the source code</param>
//         /// <param name="analyzer">The analyzer that was being run on the sources</param>
//         /// <param name="expectedResults">Diagnostic Results that should have appeared in the code</param>
//         private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
//         {
//             VerifyDiagnosticResults(actualResults, new[] { analyzer }, expectedResults);
//         }
//
//         private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer[] analyzers, params DiagnosticResult[] expectedResults)
//         {
//             int expectedCount = expectedResults.Length;
//             int actualCount = actualResults.Count();
//
//             if (expectedCount != actualCount)
//             {
//                 string diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(analyzers, actualResults.ToArray()) : "    NONE.";
//
//                 throw new AssertionFailedException(
//                     string.Format("Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n", expectedCount, actualCount, diagnosticsOutput));
//             }
//
//             for (int i = 0; i < expectedResults.Length; i++)
//             {
//                 var actual = actualResults.ElementAt(i);
//                 var expected = expectedResults[i];
//
//                 if (expected.Line == -1 && expected.Column == -1)
//                 {
//                     if (actual.Location != Location.None)
//                     {
//                         throw new AssertionFailedException(
//                             string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
//                             FormatDiagnostics(analyzers, actual)));
//                     }
//                 }
//                 else
//                 {
//                     VerifyDiagnosticLocation(analyzers, actual, actual.Location, expected.Locations.First());
//                     var additionalLocations = actual.AdditionalLocations.ToArray();
//
//                     if (additionalLocations.Length != expected.Locations.Length - 1)
//                     {
//                         throw new AssertionFailedException(
//                             string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n    {2}\r\n",
//                                 expected.Locations.Length - 1, additionalLocations.Length,
//                                 FormatDiagnostics(analyzers, actual)));
//                     }
//
//                     for (int j = 0; j < additionalLocations.Length; ++j)
//                     {
//                         VerifyDiagnosticLocation(analyzers, actual, additionalLocations[j], expected.Locations[j + 1]);
//                     }
//                 }
//
//                 if (actual.Id != expected.Id)
//                 {
//                     throw new AssertionFailedException(
//                         string.Format("Expected diagnostic id to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
//                             expected.Id, actual.Id, FormatDiagnostics(analyzers, actual)));
//                 }
//
//                 if (actual.Severity != expected.Severity)
//                 {
//                     throw new AssertionFailedException(
//                         string.Format("Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
//                             expected.Severity, actual.Severity, FormatDiagnostics(analyzers, actual)));
//                 }
//
//                 if (actual.GetMessage() != expected.Message)
//                 {
//                     throw new AssertionFailedException(
//                         string.Format("Expected diagnostic message to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
//                             expected.Message, actual.GetMessage(), FormatDiagnostics(analyzers, actual)));
//                 }
//             }
//         }
//
//         /// <summary>
//         /// Helper method to VerifyDiagnosticResult that checks the location of a diagnostic and compares it with the location in the expected DiagnosticResult.
//         /// </summary>
//         /// <param name="analyzers">The analyzer that was being run on the sources</param>
//         /// <param name="diagnostic">The diagnostic that was found in the code</param>
//         /// <param name="actual">The Location of the Diagnostic found in the code</param>
//         /// <param name="expected">The DiagnosticResultLocation that should have been found</param>
//         private static void VerifyDiagnosticLocation(DiagnosticAnalyzer[] analyzers, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
//         {
//             var actualSpan = actual.GetLineSpan();
//
//             (actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")))
//                 .Should().BeTrue(string.Format("Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
//                     expected.Path, actualSpan.Path, FormatDiagnostics(analyzers, diagnostic)));
//
//             var actualLinePosition = actualSpan.StartLinePosition;
//
//             // Only check line position if there is an actual line in the real diagnostic
//             if (actualLinePosition.Line > 0)
//             {
//                 if (actualLinePosition.Line + 1 != expected.Line)
//                 {
//                     throw new AssertionFailedException(string.Format("Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
//                         expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(analyzers, diagnostic)));
//                 }
//             }
//
//             // Only check column position if there is an actual column position in the real diagnostic
//             if (actualLinePosition.Character > 0)
//             {
//                 if (actualLinePosition.Character + 1 != expected.Column)
//                 {
//                     throw new AssertionFailedException(
//                         string.Format("Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
//                             expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(analyzers, diagnostic)));
//                 }
//             }
//         }
//         #endregion
//
//         #region Formatting Diagnostics
//         /// <summary>
//         /// Helper method to format a Diagnostic into an easily readable string
//         /// </summary>
//         /// <param name="analyzers">The analyzer that this verifier tests</param>
//         /// <param name="diagnostics">The Diagnostics to be formatted</param>
//         /// <returns>The Diagnostics formatted as a string</returns>
//         private static string FormatDiagnostics(DiagnosticAnalyzer[] analyzers, params Diagnostic[] diagnostics)
//         {
//             var builder = new StringBuilder();
//             for (int i = 0; i < diagnostics.Length; ++i)
//             {
//                 builder.AppendLine("// " + diagnostics[i]);
//
//                 foreach (var analyzer in analyzers)
//                 {
//                     var analyzerType = analyzer.GetType();
//                     foreach (var rule in analyzer.SupportedDiagnostics)
//                     {
//                         if (rule != null && rule.Id == diagnostics[i].Id)
//                         {
//                             var location = diagnostics[i].Location;
//                             if (location == Location.None)
//                             {
//                                 builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
//                             }
//                             else
//                             {
//                                 location.IsInSource.Should().BeTrue($"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");
//
//                                 string resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs") ? "GetCSharpResultAt" : "GetBasicResultAt";
//                                 var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;
//
//                                 builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
//                                     resultMethodName,
//                                     linePosition.Line + 1,
//                                     linePosition.Character + 1,
//                                     analyzerType.Name,
//                                     rule.Id);
//                             }
//
//                             if (i != diagnostics.Length - 1)
//                             {
//                                 builder.Append(',');
//                             }
//
//                             builder.AppendLine();
//                             break;
//                         }
//                     }
//                 }
//
//             }
//             return builder.ToString();
//         }
//         #endregion
//
//         private static DiagnosticAnalyzer[] CreateAllAnalyzers()
//         {
//             var assembly = typeof(Constants).Assembly;
//             var analyzersTypes = assembly.GetTypes()
//                 .Where(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type));
//             var analyzers = analyzersTypes.Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type));
//
//             return analyzers.ToArray();
//         }
//     }