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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SemanticModel semanticModel = documentEditor.SemanticModel;
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            // Get test class declaration
            ClassDeclarationSyntax oldTestClassDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            ITypeSymbol classUnderTest = TestingHelper.GetClassUnderTest(oldTestClassDeclaration, semanticModel.Compilation.GlobalNamespace);
            ClassDeclarationSyntax classUnderTestDeclaration = classUnderTest.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;

            // Get correct order
            MemberDeclarationSyntax[] orderedTestClassMembers = TestingHelper.
                OrderTestClassMembers(oldTestClassDeclaration, classUnderTestDeclaration, semanticModel).
                Cast<MemberDeclarationSyntax>().ToArray(); ;

            // Fix line gap
            SyntaxTrivia endOfLineTrivia = SyntaxHelper.GetEndOfLineTrivia(compilationUnit);
            SyntaxTrivia indentation = SyntaxHelper.GetIndentationTrivia(orderedTestClassMembers[0]);

            for (int i = 0; i < orderedTestClassMembers.Length; i++)
            {
                MemberDeclarationSyntax member = orderedTestClassMembers[i];
                if (i == 0)
                {
                    // Remove any leading new line trivia
                    orderedTestClassMembers[i] = member.
                        WithLeadingTrivia(indentation).
                        WithTrailingTrivia(endOfLineTrivia);
                }
                else
                {
                    orderedTestClassMembers[i] = member.
                        WithLeadingTrivia(endOfLineTrivia, indentation).
                        WithTrailingTrivia(endOfLineTrivia);
                }
            }

            // Re-construct test class with methods in order consistent with class under test methods
            ClassDeclarationSyntax newTestClassDeclaration = oldTestClassDeclaration.
                WithMembers(SyntaxFactory.List(orderedTestClassMembers));

            documentEditor.ReplaceNode(oldTestClassDeclaration, newTestClassDeclaration);

            return documentEditor.GetChangedDocument();
        }
    }
}
