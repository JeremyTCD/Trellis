using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Properties.ContainsKey(Constants.NoCodeFix))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                        nameof(JA1007CodeFixProvider),
                        cancellationToken => GetTransformedSolutionAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1007CodeFixProvider)),
                        diagnostic);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task<Solution> GetTransformedSolutionAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            // Get unit test class
            ITypeSymbol unitTestClass = semanticModel.
                Compilation.
                GetTypeByMetadataName(diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty]);

            // Get class under test name
            string classUnderTestName = diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty];

            // Get unit test class syntax root
            ClassDeclarationSyntax unitTestClassDeclaration = unitTestClass.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;
            SyntaxTree unitTestClassSyntaxTree = unitTestClassDeclaration.SyntaxTree;

            // Get unit test class document
            Document unitTestClassDocument = document.Project.GetDocument(unitTestClassSyntaxTree);
            DocumentEditor unitTestClassDocumentEditor = await DocumentEditor.CreateAsync(unitTestClassDocument).ConfigureAwait(false);
            SyntaxGenerator unitTestClassSyntaxGenerator = SyntaxGenerator.GetGenerator(unitTestClassDocument);

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
                    MethodDeclarationSyntax existingWronglyNamedTestMethodDeclaration = unitTestClassDeclaration.
                        DescendantNodes().
                        OfType<MethodDeclarationSyntax>().
                        Where(m => m.Identifier.ValueText == data[0]).
                        First();

                    MethodDeclarationSyntax newTestMethodDeclaration = existingWronglyNamedTestMethodDeclaration.
                        WithIdentifier(SyntaxFactory.Identifier(data[1]));

                    unitTestClassDocumentEditor.ReplaceNode(existingWronglyNamedTestMethodDeclaration, newTestMethodDeclaration);
                }
                else
                {
                    // Otherwise, create a new test method
                    MethodDeclarationSyntax newTestMethodDeclaration = CreateExceptionOutcomeTestMethod(
                        data[1],
                        classUnderTestName,
                        data[0],
                        data[2],
                        unitTestClassSyntaxGenerator);

                    unitTestClassDocumentEditor.InsertMembers(unitTestClassDeclaration, 0, new[] { newTestMethodDeclaration });
                }
            }

            // Replace document that contains test class
            unitTestClassDocument = unitTestClassDocumentEditor.GetChangedDocument();
            CompilationUnitSyntax unitTestClassCompilationUnit = await unitTestClassDocument.
                GetSyntaxRootAsync()
                .ConfigureAwait(false) as CompilationUnitSyntax;

            return document.Project.Solution.WithDocumentSyntaxRoot(unitTestClassDocument.Id, unitTestClassCompilationUnit);
        }

        private static MethodDeclarationSyntax CreateExceptionOutcomeTestMethod(string exceptionName,
            string mainClassName,
            string testMethodName,
            string methodUnderTestName,
            SyntaxGenerator syntaxGenerator)
        {
            // TODO assumes create method exists
            SyntaxNode testSubjectLocalDeclaration = syntaxGenerator.
                LocalDeclarationStatement(
                    SyntaxFactory.IdentifierName(mainClassName),
                    "testSubject",
                    syntaxGenerator.InvocationExpression(SyntaxFactory.IdentifierName($"Create{mainClassName}")));

            SyntaxNode throwsMemberAccessExpression = syntaxGenerator.
                MemberAccessExpression(
                    SyntaxFactory.IdentifierName("Assert"),
                    syntaxGenerator.GenericName("Throws", SyntaxFactory.IdentifierName(exceptionName)));

            // TODO does not provide default arguments
            SyntaxNode lambdaExpression = syntaxGenerator.
                VoidReturningLambdaExpression(
                    syntaxGenerator.InvocationExpression(
                        syntaxGenerator.MemberAccessExpression(
                            SyntaxFactory.IdentifierName("testSubject"), methodUnderTestName)));

            SyntaxNode throwsInvocation = syntaxGenerator.
                InvocationExpression(throwsMemberAccessExpression, lambdaExpression);

            SyntaxNode resultLocalDeclaration = syntaxGenerator.
                LocalDeclarationStatement(
                    SyntaxFactory.IdentifierName(exceptionName),
                    "result",
                    throwsInvocation);

            MethodDeclarationSyntax methodDeclaration = syntaxGenerator.
                MethodDeclaration(testMethodName,
                    accessibility: Accessibility.Public,
                    statements: new[] { testSubjectLocalDeclaration, resultLocalDeclaration }) as MethodDeclarationSyntax;

            return methodDeclaration.AddAttributeLists(syntaxGenerator.Attribute("Fact") as AttributeListSyntax);
        }
    }
}