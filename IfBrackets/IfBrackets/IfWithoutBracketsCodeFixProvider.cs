using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace IfBrackets;
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IfWithoutBracketsCodeFixProvider)), Shared]
public class IfWithoutBracketsCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(IfStatementAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var statement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add brackets",
                createChangedDocument: c => AddBracketsAsync(context.Document, statement, c),
                equivalenceKey: "Add brackets"),
            diagnostic);
    }

    private async Task<Document> AddBracketsAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var newStatement = SyntaxFactory.Block(ifStatement.Statement);
        var newRoot = root.ReplaceNode(ifStatement.Statement, newStatement);

        return document.WithSyntaxRoot(newRoot);
    }
}