using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1004CodeFixProvider)), Shared]
    public class JA1100CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1100PublicPropertiesAndMethodsMustBeVirtual.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Properties.ContainsKey(Constants.NoCodeFix))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                        nameof(JA1100CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1100CodeFixProvider)),
                        diagnostic);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);

            MemberDeclarationSyntax oldMemberDeclaration = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as MemberDeclarationSyntax;
            SyntaxToken virtualSyntaxToken = SyntaxFactory.Token(SyntaxKind.VirtualKeyword);
            MemberDeclarationSyntax newMemberDeclaration = null;

            if (oldMemberDeclaration is PropertyDeclarationSyntax)
            {
                newMemberDeclaration = (oldMemberDeclaration as PropertyDeclarationSyntax).AddModifiers(virtualSyntaxToken);
            }
            else if (oldMemberDeclaration is MethodDeclarationSyntax)
            {
                newMemberDeclaration = (oldMemberDeclaration as MethodDeclarationSyntax).AddModifiers(virtualSyntaxToken);
            }
            else
            {
                return document;
            }

            documentEditor.ReplaceNode(oldMemberDeclaration, newMemberDeclaration);

            return documentEditor.GetChangedDocument();
        }
    }
}
