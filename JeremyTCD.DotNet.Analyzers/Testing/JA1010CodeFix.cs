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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1010CodeFixProvider)), Shared]
    public class JA1010CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1010TestMethodMembersMustBeCorrectlyOrdered.DiagnosticId);

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
                        nameof(JA1010CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1010CodeFixProvider)),
                        diagnostic);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            TestClassContext testClassContext = await TestClassContextFactory.TryCreateAsync(document).ConfigureAwait(false);

            // Get test class declaration
            Document classUnderTestDocument = document.Project.Solution.GetDocument(testClassContext.ClassUnderTestDeclaration.SyntaxTree);
            SemanticModel classUnderTestSemanticModel = await classUnderTestDocument.GetSemanticModelAsync().ConfigureAwait(false);

            // Get correct order
            MemberDeclarationSyntax[] orderedTestClassMembers = TestingHelper.
                OrderTestClassMembers(
                    testClassContext,
                    classUnderTestSemanticModel).
                Cast<MemberDeclarationSyntax>().ToArray();

            // Fix trivia
            SyntaxHelper.FixMemberTrivia(orderedTestClassMembers);

            // Re-construct test class with methods in order consistent with class under test methods
            ClassDeclarationSyntax newTestClassDeclaration = testClassContext.
                ClassDeclaration.
                WithMembers(SyntaxFactory.List(orderedTestClassMembers));

            documentEditor.ReplaceNode(testClassContext.ClassDeclaration, newTestClassDeclaration);

            return documentEditor.GetChangedDocument();
        }
    }
}
