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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            SemanticModel semanticModel = documentEditor.SemanticModel;

            ExpressionSyntax existingExpression = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as ExpressionSyntax;
            // TODO assumes expression's first argument list is the relevant one
            IEnumerable<ArgumentSyntax> existingExpressionArguments = existingExpression.DescendantNodes().OfType<ArgumentListSyntax>().First().Arguments;
            INamedTypeSymbol classUnderTest = documentEditor.
                SemanticModel.
                Compilation.
                GetTypeByMetadataName(diagnostic.Properties[JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty]);
            IMethodSymbol classUnderTestConstructor = classUnderTest.Constructors.OrderByDescending(c => c.Parameters.Count()).First();
            IEnumerable<IParameterSymbol> classUnderTestConstructorParameters = classUnderTestConstructor.Parameters;
            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            if (diagnostic.Properties.ContainsKey(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty))
            {
                string methodName = diagnostic.Properties[JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty];
                // TODO more robust way to determine if method returns a mock
                bool createMockCreateMethod = methodName.StartsWith("CreateMock");

                // Add a mock repository field declaration if necessary
                if (createMockCreateMethod && TestingHelper.GetMockRepositoryFieldDeclaration(compilationUnit, semanticModel) == null)
                {
                    documentEditor.InsertMembers(classDeclaration, 0, new[] { TestingHelper.CreateMockRepositoryFieldDeclaration(syntaxGenerator) });
                }

                MethodDeclarationSyntax newCreateMethodDeclaration = CreateMethodDeclaration(
                    classUnderTest,
                    syntaxGenerator,
                    methodName,
                    classUnderTestConstructorParameters,
                    createMockCreateMethod);

                MethodDeclarationSyntax existingCreateMethodDeclaration = classDeclaration.
                    DescendantNodes().
                    OfType<MethodDeclarationSyntax>().
                    Select(m => semanticModel.GetDeclaredSymbol(m) as IMethodSymbol).
                    Where(m =>
                    {
                        if (m == null || !string.Equals(m.Name, methodName))
                        {
                            return false;
                        }

                        if (!createMockCreateMethod && m.ReturnType == classUnderTest)
                        {
                            return true;
                        }

                        if (createMockCreateMethod &&
                            m.ReturnType.OriginalDefinition == semanticModel.Compilation.GetTypeByMetadataName("Moq.Mock`1") &&
                            (m.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault() == classUnderTest)
                        {
                            return true;
                        }

                        return false;
                    }).
                    FirstOrDefault()?.
                    DeclaringSyntaxReferences.
                    FirstOrDefault()?.
                    GetSyntax() as MethodDeclarationSyntax;

                // TODO check if a method with the same name and return type already exists (move to testinghelper), if it does, replace it, otherwise, add the new method to
                // the test class
                if (existingCreateMethodDeclaration != null)
                {
                    documentEditor.ReplaceNode(existingCreateMethodDeclaration, newCreateMethodDeclaration);
                }
                else
                {
                    documentEditor.AddMember(compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First(), newCreateMethodDeclaration);
                }
            }

            if (diagnostic.Properties.ContainsKey(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.InvocationInvalidProperty))
            {
                InvocationExpressionSyntax newExpression = CreateMethodInvocation(
                    diagnostic.Properties[JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty],
                    existingExpressionArguments,
                    syntaxGenerator,
                    classUnderTestConstructorParameters);
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

        // TODO mock repository return expressions
        private static MethodDeclarationSyntax CreateMethodDeclaration(INamedTypeSymbol classUnderTestSymbol,
            SyntaxGenerator syntaxGenerator, string methodName, IEnumerable<IParameterSymbol> classUnderTestConstructorParameters,
            bool createMockCreateMethod)
        {
            IEnumerable<SyntaxNode> parameterSyntaxes = classUnderTestConstructorParameters.
                Select(p => syntaxGenerator.ParameterDeclaration(p, syntaxGenerator.DefaultExpression(p.Type)));

            IEnumerable<SyntaxNode> resultExpressionArguments = parameterSyntaxes.
                Select(p => syntaxGenerator.Argument(syntaxGenerator.IdentifierName((p as ParameterSyntax).Identifier.ValueText)));

            SyntaxNode classUnderTestCreation = createMockCreateMethod ?
                TestingHelper.CreateMockRepositoryCreateInvocationExpression(syntaxGenerator, classUnderTestSymbol.Name, "_mockRepository", resultExpressionArguments)
                : syntaxGenerator.ObjectCreationExpression(classUnderTestSymbol, resultExpressionArguments);

            SyntaxNode returnStatement = syntaxGenerator.ReturnStatement(classUnderTestCreation);

            SyntaxNode classUnderTestName = syntaxGenerator.TypeExpression(classUnderTestSymbol);

            return syntaxGenerator.MethodDeclaration(
                   methodName,
                   parameters: parameterSyntaxes,
                   returnType: createMockCreateMethod ? syntaxGenerator.GenericName("Mock", classUnderTestName) : classUnderTestName,
                   accessibility: Accessibility.Private,
                   statements: new[] { returnStatement }) as MethodDeclarationSyntax;
        }
    }
}
