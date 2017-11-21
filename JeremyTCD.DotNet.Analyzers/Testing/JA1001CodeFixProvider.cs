using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1001CodeFixProvider)), Shared]
    public class JA1001CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1001TestClassNamespacesMustBeCorrectlyFormatted.DiagnosticId);

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
                string correctNamespace = diagnostic.Properties[JA1001TestClassNamespacesMustBeCorrectlyFormatted.CorrectNamespaceProperty];
                context.RegisterCodeFix(
                    CodeAction.Create(
                        string.Format(Strings.JA1001_CodeFix_Title, correctNamespace),
                        cancellationToken => GetTransformedDocumentAsync(correctNamespace, context.Document, diagnostic, cancellationToken),
                        nameof(JA1001CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(string correctNamespace, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            // Update namespace
            SyntaxNode oldQualifiedName = compilationUnit.FindNode(diagnostic.Location.SourceSpan);
            SyntaxNode newQualifiedName = CreateQualifiedName(
                syntaxGenerator,
                correctNamespace.Split('.'));
            documentEditor.ReplaceNode(oldQualifiedName, newQualifiedName);

            return documentEditor.GetChangedDocument();
        }

        public static QualifiedNameSyntax CreateQualifiedName(SyntaxGenerator syntaxGenerator, string[] names)
        {
            int numNames = names.Length;
            if (numNames < 2)
            {
                return null;
            }

            SyntaxNode result = syntaxGenerator.IdentifierName(names[0]);
            for (int i = 1; i < numNames; i++)
            {
                result = syntaxGenerator.QualifiedName(result, syntaxGenerator.IdentifierName(names[i]));
            }

            return result as QualifiedNameSyntax;
        }
    }
}
