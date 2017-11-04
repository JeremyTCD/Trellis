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

        private static async Task<Solution> GetTransformedSolutionAsync(Document unitTestClassDocument, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SemanticModel unitTestClassSemanticModel = await unitTestClassDocument.GetSemanticModelAsync().ConfigureAwait(false);
            CompilationUnitSyntax unitTestClassCompilationUnit = await unitTestClassDocument.GetSyntaxRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            // Get unit test class
            ITypeSymbol unitTestClass = unitTestClassSemanticModel.
                Compilation.
                GetTypeByMetadataName(diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty]);

            // Get unit test class declaration
            ClassDeclarationSyntax unitTestClassDeclaration = unitTestClass.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;
            SyntaxTree unitTestClassSyntaxTree = unitTestClassDeclaration.SyntaxTree;

            // Get class under test
            string classUnderTestName = diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty];
            ITypeSymbol classUnderTest = unitTestClassSemanticModel.
                Compilation.
                GetTypeByMetadataName(diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestFullyQualifiedNameProperty]);

            // Get class under test declaration
            ClassDeclarationSyntax classUnderTestDeclaration = classUnderTest.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;

            // Get class under test semantic model
            SemanticModel classUnderTestSemanticModel = unitTestClassSemanticModel.Compilation.GetSemanticModel(classUnderTestDeclaration.SyntaxTree);

            // Get unit test class document
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
                    MethodDeclarationSyntax newTestMethodDeclaration = TestingHelper.CreateExceptionTestMethod(
                        data[1],
                        classUnderTestName,
                        data[0],
                        classUnderTest.GetMembers().OfType<IMethodSymbol>().First(m => m.Name == data[2]), // TODO does not handle overloaded methods
                        unitTestClassSyntaxGenerator);

                    unitTestClassDocumentEditor.AddMember(unitTestClassDeclaration, newTestMethodDeclaration);
                }
            }

            Document newUnitTestDocument = unitTestClassDocumentEditor.GetChangedDocument();
            SemanticModel newUnitTestClassSemanticModel = await newUnitTestDocument.GetSemanticModelAsync().ConfigureAwait(false);
            CompilationUnitSyntax newUnitTestClassCompilationUnit = await newUnitTestDocument.GetSyntaxRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;
            ClassDeclarationSyntax newUnitTestClassDeclaration = newUnitTestClassCompilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            ITypeSymbol newClassUnderTest = newUnitTestClassSemanticModel.
                Compilation.
                GetTypeByMetadataName(diagnostic.Properties[JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestFullyQualifiedNameProperty]) as ITypeSymbol;
            ClassDeclarationSyntax newClassUnderTestDeclaration = newClassUnderTest.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;
            SemanticModel newClassUnderTestSemanticModel = newUnitTestClassSemanticModel.Compilation.GetSemanticModel(newClassUnderTestDeclaration.SyntaxTree);

            // Get correct order
            MemberDeclarationSyntax[] orderedTestClassMembers = TestingHelper.
                OrderTestClassMembers(
                    newUnitTestClassDeclaration, 
                    newClassUnderTestDeclaration, 
                    newUnitTestClassSemanticModel, 
                    newClassUnderTest,
                    newClassUnderTestSemanticModel).
                Cast<MemberDeclarationSyntax>().
                ToArray();

            // Fix trivia
            SyntaxHelper.FixMemberTrivia(orderedTestClassMembers);

            // Create new syntax root
            newUnitTestClassCompilationUnit = newUnitTestClassCompilationUnit.
                ReplaceNode(newUnitTestClassDeclaration, newUnitTestClassDeclaration.WithMembers(SyntaxFactory.List(orderedTestClassMembers)));

            return unitTestClassDocument.Project.Solution.WithDocumentSyntaxRoot(unitTestClassDocument.Id, newUnitTestClassCompilationUnit);
        }
    }
}