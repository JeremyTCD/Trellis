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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1011CodeFixProvider)), Shared]
    public class JA1011CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1011TestableClass.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document mainDocument = context.Document;
            Project mainProject = mainDocument.Project;
            Solution solution = mainProject.Solution;

            // Check if test project exists
            string mainProjectName = mainProject.Name;
            string testProjectName = $"{mainProjectName}.Tests";
            Project testProject = solution.Projects.FirstOrDefault(p => p.Name == testProjectName);

            if (testProject == null)
            {
                return Task.CompletedTask;
            }

            // Add code actions
            Diagnostic diagnostic = context.Diagnostics.First();
            CompilationUnitSyntax mainCompilationUnit = mainDocument.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            ClassDeclarationSyntax mainClassDeclaration = mainCompilationUnit.FindNode(diagnostic.Location.SourceSpan) as ClassDeclarationSyntax;
            string mainClassName = mainClassDeclaration.Identifier.ValueText;
            string unitTestClassName = $"{mainClassName}UnitTests";
            if (!testProject.Documents.Any(d => d.Name == $"{unitTestClassName}.cs"))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1011_CodeFix_Title_CreateUnitTestClass,
                    cancellationToken => CreateUnitTestClass(solution,
                        testProject,
                        mainDocument,
                        testProjectName,
                        unitTestClassName,
                        "UnitTests",
                        diagnostic,
                        cancellationToken),
                    $"{nameof(JA1011CodeFixProvider)}UnitTestClass"),
                    diagnostic);
            }

            string integrationTestClassName = $"{mainClassName}IntegrationTests.cs";
            if (!testProject.Documents.Any(d => d.Name == integrationTestClassName))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1011_CodeFix_Title_CreateIntegrationTestClass,
                    cancellationToken => CreateUnitTestClass(solution,
                        testProject,
                        mainDocument,
                        testProjectName,
                        integrationTestClassName,
                        "IntegrationTests",
                        diagnostic,
                        cancellationToken),
                    $"{nameof(JA1011CodeFixProvider)}IntegrationTestClass"),
                    diagnostic);
            }

            string endToEndTestClassName = $"{mainClassName}EndToEndTests.cs";
            if (!testProject.Documents.Any(d => d.Name == endToEndTestClassName))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1011_CodeFix_Title_CreateEndToEndTestClass,
                    cancellationToken => CreateUnitTestClass(solution,
                        testProject,
                        mainDocument,
                        testProjectName,
                        endToEndTestClassName,
                        "EndToEndTests",
                        diagnostic,
                        cancellationToken),
                    $"{nameof(JA1011CodeFixProvider)}EndToEndTestClass"),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Solution> CreateUnitTestClass(Solution solution,
            Project testProject,
            Document mainDocument,
            string namespaceName,
            string testClassName,
            string folderName,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(testProject);
            SemanticModel semanticModel = await mainDocument.GetSemanticModelAsync().ConfigureAwait(false);
            CompilationUnitSyntax compilationUnit = await mainDocument.GetSyntaxRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            // Get test class
            ClassDeclarationSyntax classUnderTestDeclaration = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as ClassDeclarationSyntax;
            INamedTypeSymbol classUnderTest = semanticModel.GetDeclaredSymbol(classUnderTestDeclaration) as INamedTypeSymbol;

            DocumentId documentId = DocumentId.CreateNewId(testProject.Id);
            CompilationUnitSyntax syntaxRoot = CreateTestClassSyntaxRoot(
                testProject,
                namespaceName,
                testClassName,
                classUnderTestDeclaration,
                classUnderTest,
                syntaxGenerator,
                semanticModel);

            return solution.AddDocument(documentId, testClassName, syntaxRoot, folders: new[] { folderName });
        }

        // Better to create more than is required and just leave unused statements be
        private static CompilationUnitSyntax CreateTestClassSyntaxRoot(
            Project testProject,
            string namespaceName,
            string testClassName,
            ClassDeclarationSyntax classUnderTestDeclaration,
            INamedTypeSymbol classUnderTest,
            SyntaxGenerator syntaxGenerator,
            SemanticModel semanticModel)
        {
            List<SyntaxNode> classMembers = new List<SyntaxNode>();
            HashSet<INamespaceSymbol> namespacesToImport = new HashSet<INamespaceSymbol>();

            classMembers.Add(TestingHelper.CreateMockRepositoryFieldDeclaration(syntaxGenerator));

            // Test method template for each method in class under test
            int descriptionPlaceholder = 0;
            foreach (MethodDeclarationSyntax methodUnderTestDeclaration in classUnderTestDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                string methodUnderTestName = methodUnderTestDeclaration.Identifier.ValueText;
                IMethodSymbol methodUnderTest = semanticModel.GetDeclaredSymbol(methodUnderTestDeclaration) as IMethodSymbol;

                classMembers.Add(TestingHelper.CreateTestMethod(
                    classUnderTest.Name,
                    $"{methodUnderTestName}_{descriptionPlaceholder++}",
                    methodUnderTest,
                    syntaxGenerator));

                // Check if method has exception documentation
                List<XmlElementSyntax> exceptionElements = TestingHelper.GetMethodExceptionXmlElements(methodUnderTestDeclaration, methodUnderTest);

                // Create a test method for each exception documentation
                foreach (XmlElementSyntax exceptionElement in exceptionElements)
                {
                    XmlCrefAttributeSyntax xmlCrefAttribute = TestingHelper.GetXmlElementCrefAttribute(exceptionElement);
                    if (xmlCrefAttribute == null)
                    {
                        continue;
                    }

                    string exceptionName = xmlCrefAttribute.Cref.ToString();
                    string exceptionDescription = DocumentationHelper.GetNodeContentsAsNormalizedString(exceptionElement).
                        RemoveNonAlphaNumericCharacters().
                        ToTitleCase().
                        Replace("Thrown", "");
                    string testMethodName = $"{methodUnderTest.Name}_Throws{exceptionName}{exceptionDescription}";

                    classMembers.Add(TestingHelper.CreateExceptionTestMethod(exceptionName, classUnderTest.Name, testMethodName, methodUnderTest, syntaxGenerator));

                    // Add using directive for exception
                    ITypeSymbol exceptionType = SymbolHelper.GetTypeSymbols(exceptionName, semanticModel.Compilation.GlobalNamespace).FirstOrDefault();
                    if (exceptionType != null)
                    {
                        namespacesToImport.Add(exceptionType.ContainingNamespace);
                    }
                }
            }

            classMembers.Add(TestingHelper.CreateCreateMethodDeclaration(classUnderTest, syntaxGenerator, false));
            classMembers.Add(TestingHelper.CreateCreateMethodDeclaration(classUnderTest, syntaxGenerator, true));

            SyntaxNode classDeclaration = syntaxGenerator.ClassDeclaration(
                testClassName,
                accessibility: Accessibility.Public,
                members: classMembers);

            SyntaxNode namespaceDeclaration = syntaxGenerator.NamespaceDeclaration(namespaceName, classDeclaration);

            List<SyntaxNode> nodes = new List<SyntaxNode>();
            nodes.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Moq")));
            nodes.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Xunit")));
            foreach(INamespaceSymbol namespaceSymbol in namespacesToImport)
            {
                nodes.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(namespaceSymbol.ToDisplayString())));
            }
            nodes = nodes.OrderBy(n => (n as UsingDirectiveSyntax).Name.ToString()).ToList();
            nodes.Add(namespaceDeclaration);

            return syntaxGenerator.CompilationUnit(nodes) as CompilationUnitSyntax;
        }
    }
}
