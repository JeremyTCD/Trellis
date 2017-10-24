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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1009CodeFixProvider)), Shared]
    public class JA1009CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor.DiagnosticId);

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
                        nameof(JA1009CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1009CodeFixProvider)),
                        diagnostic);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            SemanticModel semanticModel = documentEditor.SemanticModel;

            // Get test class
            ClassDeclarationSyntax testClassDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            // Check if mock repository field exists
            VariableDeclarationSyntax mockRepositoryVariableDeclaration = TestingHelper.GetMockRepositoryFieldDeclaration(compilationUnit, semanticModel);
            if (mockRepositoryVariableDeclaration == null)
            {
                FieldDeclarationSyntax mockRepositoryFieldDeclaration = TestingHelper.CreateMockRepositoryFieldDeclaration(syntaxGenerator);
                documentEditor.InsertMembers(testClassDeclaration, 0, new[] { mockRepositoryFieldDeclaration });
                mockRepositoryVariableDeclaration = mockRepositoryFieldDeclaration.Declaration;
            }

            // Replace object creation expression
            ObjectCreationExpressionSyntax oldExpression = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as ObjectCreationExpressionSyntax;
            InvocationExpressionSyntax newExpression = TestingHelper.
                CreateMockRepositoryCreateInvocationExpression(
                    syntaxGenerator,
                    (oldExpression.Type as GenericNameSyntax).TypeArgumentList.Arguments.First().ToString(),
                    mockRepositoryVariableDeclaration.Variables.First().Identifier.ValueText,
                    oldExpression.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault()?.Arguments);
            documentEditor.ReplaceNode(oldExpression, newExpression);

            return documentEditor.GetChangedDocument();
        }
    }
}
