using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1007CodeFixProvider)), Shared]
    public class JA1007CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId);

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
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1007_CodeFix_Title,
                    cancellationToken => GetTransformedSolutionAsync(context.Document, diagnostic, cancellationToken),
                    nameof(JA1007CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Solution> GetTransformedSolutionAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            TestClassContext testClassContext = await TestClassContextFactory.TryCreateAsync(document).ConfigureAwait(false);

            // Parse fix data
            string fixData = diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty];
            string[] fixes = fixData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string fix in fixes)
            {
                // Get data
                string[] data = fix.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // If existing wrongly named test method exists, rename it
                if (data.Length == 4)
                {
                    // TODO assumes that test methods are never overloaded
                    MethodDeclarationSyntax existingWronglyNamedTestMethodDeclaration = testClassContext.
                        TestMethodDeclarations.
                        Where(m => m.Identifier.ValueText == data[0]).
                        First();

                    MethodDeclarationSyntax newTestMethodDeclaration = existingWronglyNamedTestMethodDeclaration.
                        WithIdentifier(SyntaxFactory.Identifier(data[1]));

                    documentEditor.ReplaceNode(existingWronglyNamedTestMethodDeclaration, newTestMethodDeclaration);
                }
                else
                {
                    // Otherwise, create a new test method
                    MethodDeclarationSyntax newTestMethodDeclaration = TestingHelper.CreateExceptionTestMethod(
                        data[1],
                        testClassContext.ClassUnderTestName,
                        data[0],
                        testClassContext.ClassUnderTestMethods.First(m => m.Name == data[2]), // TODO does not handle overloaded methods
                        syntaxGenerator);

                    documentEditor.AddMember(testClassContext.ClassDeclaration, newTestMethodDeclaration);
                }
            }

            Document newDocument = documentEditor.GetChangedDocument();
            SemanticModel newSemanticModel = await newDocument.GetSemanticModelAsync().ConfigureAwait(false);
            TestClassContext newTestClassContext = await TestClassContextFactory.TryCreateAsync(newDocument).ConfigureAwait(false);

            Document newClassUnderTestDocument = newDocument.Project.Solution.GetDocument(newTestClassContext.ClassUnderTestDeclaration.SyntaxTree);
            SemanticModel newClassUnderTestSemanticModel = await newClassUnderTestDocument.GetSemanticModelAsync().ConfigureAwait(false);

            // Get correct order
            MemberDeclarationSyntax[] orderedTestClassMembers = TestingHelper.
                OrderTestClassMembers(
                    newTestClassContext,
                    newClassUnderTestSemanticModel).
                Cast<MemberDeclarationSyntax>().
                ToArray();

            // Fix trivia
            SyntaxHelper.FixMemberTrivia(orderedTestClassMembers);

            // Create new syntax root
            CompilationUnitSyntax newCompilationUnit = newTestClassContext.CompilationUnit.
                ReplaceNode(
                    newTestClassContext.ClassDeclaration,
                    newTestClassContext.ClassDeclaration.WithMembers(SyntaxFactory.List(orderedTestClassMembers)));

            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newCompilationUnit);
        }
    }
}