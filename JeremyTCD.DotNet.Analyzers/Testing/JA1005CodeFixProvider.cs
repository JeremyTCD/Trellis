using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1005CodeFixProvider)), Shared]
    public class JA1005CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId);

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
                        nameof(JA1005CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1005CodeFixProvider)),
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

            ExpressionSyntax existingExpression = testClassContext.CompilationUnit.FindNode(diagnostic.Location.SourceSpan) as ExpressionSyntax;
            // TODO assumes expression's first argument list is the relevant one
            IEnumerable<ArgumentSyntax> existingExpressionArguments = existingExpression.DescendantNodes().OfType<ArgumentListSyntax>().First().Arguments;

            if (diagnostic.Properties.ContainsKey(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty))
            {
                string createMethodName = diagnostic.Properties[JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty];
                // TODO more robust way to determine if method returns a mock
                bool createMockCreateMethod = createMethodName.StartsWith("CreateMock");

                // Add a mock repository field declaration if necessary
                if (createMockCreateMethod && testClassContext.MockRepositoryVariableDeclaration == null)
                {
                    documentEditor.
                        InsertMembers(testClassContext.ClassDeclaration, 0, new[] { TestingHelper.CreateMockRepositoryFieldDeclaration(syntaxGenerator) });
                }

                // Add newly required usings
                List<SyntaxNode> newUsingDirectives = TestingHelper.CreateMissingUsingDirectives(testClassContext);
                documentEditor.InsertMembers(testClassContext.CompilationUnit, 0, newUsingDirectives);

                MethodDeclarationSyntax newCreateMethodDeclaration = TestingHelper.CreateCreateMethodDeclaration(
                    testClassContext.ClassUnderTest,
                    syntaxGenerator,
                    createMockCreateMethod,
                    testClassContext.ClassUnderTestMainConstructor.Parameters);

                MethodDeclarationSyntax existingCreateMethodDeclaration = testClassContext.
                    Methods.
                    Where(m =>
                    {
                        if (!string.Equals(m.Name, createMethodName))
                        {
                            return false;
                        }

                        if (!createMockCreateMethod && m.ReturnType == testClassContext.ClassUnderTest)
                        {
                            return true;
                        }

                        if (createMockCreateMethod &&
                            m.ReturnType.OriginalDefinition == testClassContext.SemanticModel.Compilation.GetTypeByMetadataName("Moq.Mock`1") &&
                            (m.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault() == testClassContext.ClassUnderTest)
                        {
                            return true;
                        }

                        return false;
                    }).
                    FirstOrDefault()?.
                    DeclaringSyntaxReferences.
                    FirstOrDefault()?.
                    GetSyntax() as MethodDeclarationSyntax;

                // Check if a method with the same name and return type already exists, if it does, replace it, otherwise, add the new method to
                // the test class
                if (existingCreateMethodDeclaration != null)
                {
                    documentEditor.ReplaceNode(existingCreateMethodDeclaration, newCreateMethodDeclaration);
                }
                else
                {
                    documentEditor.AddMember(testClassContext.ClassDeclaration, newCreateMethodDeclaration);
                }
            }

            if (diagnostic.Properties.ContainsKey(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.InvocationInvalidProperty))
            {
                InvocationExpressionSyntax newExpression = CreateMethodInvocation(
                    diagnostic.Properties[JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty],
                    existingExpressionArguments,
                    syntaxGenerator,
                    testClassContext.ClassUnderTestMainConstructor.Parameters);
                documentEditor.ReplaceNode(existingExpression, newExpression);
            }

            return documentEditor.GetChangedDocument();
        }

        private static InvocationExpressionSyntax CreateMethodInvocation(string createMethodName, IEnumerable<ArgumentSyntax> arguments,
            SyntaxGenerator syntaxGenerator, IEnumerable<IParameterSymbol> classUnderTestConstructorParameters)
        {
            List<SyntaxNode> createMethodArguments = new List<SyntaxNode>();
            bool skippedAny = false;
            for (int i = 0; i < arguments.Count(); i++)
            {
                ExpressionSyntax expression = arguments.ElementAt(i).Expression;
                // TODO check for default for the specific type instead (syntaxGenerator.DefaultExpression(ITypeSymbol)
                if (expression.ToString() != "null")
                {
                    SyntaxNode argument = skippedAny ? syntaxGenerator.Argument(classUnderTestConstructorParameters.ElementAt(i).Name, RefKind.None, expression) :
                        syntaxGenerator.Argument(RefKind.None, expression);
                    createMethodArguments.Add(argument);
                }
                else
                {
                    skippedAny = true;
                }
            }

            return syntaxGenerator.InvocationExpression(syntaxGenerator.IdentifierName(createMethodName), createMethodArguments) as InvocationExpressionSyntax;
        }
    }
}
