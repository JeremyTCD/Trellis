using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1004CodeFixProvider)), Shared]
    public class JA1004CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId);

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
                string correctVariableName = diagnostic.Properties[JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty];
                context.RegisterCodeFix(
                    CodeAction.Create(
                    string.Format(Strings.JA1004_CodeFix_Title, correctVariableName),
                    cancellationToken => GetTransformedDocumentAsync(correctVariableName, context.Document, diagnostic, cancellationToken),
                    nameof(JA1004CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(string correctVariableName, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            VariableDeclaratorSyntax oldVariableDeclarator = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as VariableDeclaratorSyntax;
            string oldVariableName = oldVariableDeclarator.Identifier.ValueText;
            string newVariableName = correctVariableName;
            VariableDeclaratorSyntax newVariableDeclarator = oldVariableDeclarator.WithIdentifier(SyntaxFactory.Identifier(newVariableName));

            // DocumentEditor does not work for VariableDeclartorSyntax - https://github.com/dotnet/roslyn/issues/8154
            compilationUnit = compilationUnit.ReplaceNode(oldVariableDeclarator, newVariableDeclarator);
            document = document.WithSyntaxRoot(compilationUnit);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            MethodDeclarationSyntax methodDeclaration = compilationUnit.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<MethodDeclarationSyntax>();
            foreach (IdentifierNameSyntax identifierName in methodDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (identifierName.Identifier.ValueText == oldVariableName)
                {
                    documentEditor.ReplaceNode(identifierName, SyntaxFactory.IdentifierName(newVariableName).WithTriviaFrom(identifierName));
                }
            }

            return documentEditor.GetChangedDocument();
        }
    }
}
