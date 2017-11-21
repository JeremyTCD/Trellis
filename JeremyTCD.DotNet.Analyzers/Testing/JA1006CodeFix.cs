using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1006CodeFixProvider)), Shared]
    public class JA1006CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();

            if (!diagnostic.Properties.ContainsKey(Constants.NoCodeFix))
            {
                string newDataMethodName = diagnostic.Properties[JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DataMethodNameProperty];
                context.RegisterCodeFix(
                    CodeAction.Create(
                    string.Format(Strings.JA1006_CodeFix_Title, newDataMethodName),
                    cancellationToken => GetTransformedDocumentAsync(newDataMethodName, context.Document, diagnostic, cancellationToken),
                    nameof(JA1006CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(string newDataMethodName, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            TestClassContext testClassContext = await TestClassContextFactory.TryCreateAsync(document).ConfigureAwait(false);

            // Renamed method
            MethodDeclarationSyntax oldDataMethodDeclaration = testClassContext.
                CompilationUnit.
                FindNode(diagnostic.Location.SourceSpan) as MethodDeclarationSyntax;
            string oldDataMethodName = oldDataMethodDeclaration.Identifier.ValueText;
            MethodDeclarationSyntax newDataMethodDeclaration = oldDataMethodDeclaration.WithIdentifier(SyntaxFactory.Identifier(newDataMethodName).WithTriviaFrom(oldDataMethodDeclaration.Identifier));
            documentEditor.ReplaceNode(oldDataMethodDeclaration, newDataMethodDeclaration);

            // Rename references to method
            IEnumerable<IdentifierNameSyntax> referenceIdentifiers = testClassContext.
                GetDescendantNodes<IdentifierNameSyntax>().
                Where(i => i.ToString() == oldDataMethodName);
            SyntaxNode newIdentifier = syntaxGenerator.IdentifierName(newDataMethodName);

            foreach (IdentifierNameSyntax referenceIdentifier in referenceIdentifiers)
            {
                documentEditor.ReplaceNode(referenceIdentifier, newIdentifier);
            }

            return documentEditor.GetChangedDocument();
        }
    }
}
