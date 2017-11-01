using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class TestingHelper
    {
        public static FieldDeclarationSyntax CreateMockRepositoryFieldDeclaration(SyntaxGenerator syntaxGenerator)
        {
            SyntaxNode argumentMemberAccessExpression = syntaxGenerator.MemberAccessExpression(
                    syntaxGenerator.IdentifierName("MockBehavior"),
                    syntaxGenerator.IdentifierName("Default"));
            SyntaxNode constructorArgument = syntaxGenerator.Argument(argumentMemberAccessExpression);
            SyntaxNode typeIdentifier = syntaxGenerator.IdentifierName("MockRepository");
            SyntaxNode objectCreationExpression = syntaxGenerator.ObjectCreationExpression(typeIdentifier, constructorArgument);

            return (FieldDeclarationSyntax)syntaxGenerator.FieldDeclaration("_mockRepository", typeIdentifier, Accessibility.Private, DeclarationModifiers.ReadOnly,
                initializer: objectCreationExpression);
        }

        public static InvocationExpressionSyntax CreateMockRepositoryCreateInvocationExpression(SyntaxGenerator syntaxGenerator,
            string typeName, string mockRepositoryVariableName, IEnumerable<SyntaxNode> arguments = null)
        {
            SyntaxNode typeIdentifier = syntaxGenerator.IdentifierName(typeName);
            SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(
                syntaxGenerator.IdentifierName(mockRepositoryVariableName),
                syntaxGenerator.GenericName("Create", typeIdentifier));

            return (InvocationExpressionSyntax)syntaxGenerator.InvocationExpression(memberAccessExpression, arguments ?? new SyntaxNode[] { });
        }

        public static VariableDeclarationSyntax GetMockRepositoryFieldDeclaration(CompilationUnitSyntax compilationUnit, SemanticModel semanticModel)
        {
            // TODO check for default name _mockRepository
            // TODO could be declared as a property as well
            return compilationUnit.
                DescendantNodes().
                OfType<VariableDeclarationSyntax>().
                Where(v => semanticModel.GetTypeInfo(v.Type).Type.ToString() == "Moq.MockRepository").
                FirstOrDefault();
        }

        public static bool ContainsTestClass(CompilationUnitSyntax compilationUnit)
        {
            return compilationUnit.Usings.Any(u => string.Equals(u.Name.ToString(), "Xunit", StringComparison.Ordinal));
        }

        public static bool IsUnitTestClass(ClassDeclarationSyntax testClassDeclaration)
        {
            return testClassDeclaration.Identifier.ValueText.EndsWith("UnitTests");
        }

        public static IEnumerable<IMethodSymbol> GetMethodMembers(ITypeSymbol targetClass)
        {
            return targetClass.GetMembers().Where(m => m is IMethodSymbol).Cast<IMethodSymbol>();
        }

        public static bool IsIntegrationTestClass(ClassDeclarationSyntax testClassDeclaration)
        {
            return testClassDeclaration.Identifier.ValueText.EndsWith("IntegrationTests");
        }

        public static bool IsEndToEndTestClass(ClassDeclarationSyntax testClassDeclaration)
        {
            return testClassDeclaration.Identifier.ValueText.EndsWith("EndToEndTests");
        }

        public static IEnumerable<MethodDeclarationSyntax> GetTestMethodDeclarations(CompilationUnitSyntax compilationUnit, SemanticModel semanticModel)
        {
            return compilationUnit.
                DescendantNodes().
                OfType<MethodDeclarationSyntax>().
                Where(m => IsTestMethod(m, semanticModel));
        }

        public static bool IsTestMethod(SyntaxNode methodDeclaration, SemanticModel semanticModel)
        {
            if (!(methodDeclaration is MethodDeclarationSyntax))
            {
                return false;
            }

            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            return IsTestMethod(methodSymbol);
        }

        public static bool IsTestDataMethod(SyntaxNode methodDeclaration)
        {
            // TODO more robust way to identify data methods
            return methodDeclaration is MethodDeclarationSyntax && (methodDeclaration as MethodDeclarationSyntax).Identifier.ValueText.EndsWith("_Data");
        }

        public static bool IsTestMethod(IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(a => a.ToString() == "Xunit.FactAttribute" || a.ToString() == "Xunit.TheoryAttribute");
        }

        public static ITypeSymbol GetClassUnderTest(ClassDeclarationSyntax testClassDeclaration, INamespaceSymbol globalNamespace)
        {
            string testClassName = testClassDeclaration.Identifier.ValueText;
            string classUnderTestName = testClassName.
                Replace("UnitTests", string.Empty).
                Replace("IntegrationTests", string.Empty).
                Replace("EndToEndTests", string.Empty);

            if (testClassName == classUnderTestName)
            {
                return null;
            }

            IEnumerable<ITypeSymbol> types = SymbolHelper.GetTypeSymbols(classUnderTestName, globalNamespace);

            // More than one types with the same name (from different namespaces)
            if (types.Count() > 1)
            {
                string classNamespaceName = testClassDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();

                foreach (ITypeSymbol type in types)
                {
                    if (classNamespaceName.StartsWith(type.ContainingNamespace.ToString()))
                    {
                        return type;
                    }
                }
            }

            return types.FirstOrDefault();
        }

        public static bool IsMockSetupMethod(IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
            {
                return false;
            }

            string methodDisplayString = methodSymbol.OriginalDefinition.ToDisplayString();

            return methodDisplayString == "Moq.Mock<T>.Setup<TResult>(System.Linq.Expressions.Expression<System.Func<T, TResult>>)" ||
                methodDisplayString == "Moq.Mock<T>.Setup(System.Linq.Expressions.Expression<System.Action<T>>)";
        }

        public static bool IsMockRepositoryVerifyAllMethod(IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
            {
                return false;
            }

            return methodSymbol.ToDisplayString() == "Moq.MockFactory.VerifyAll()";
        }

        public static bool IsMockRepositoryCreateMethod(IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
            {
                return false;
            }

            return methodSymbol.OriginalDefinition.ToDisplayString() == "Moq.MockFactory.Create<T>()";
        }

        public static List<SyntaxNode> OrderTestClassMembers(ClassDeclarationSyntax testClassDeclaration, ClassDeclarationSyntax classUnderTestDeclaration,
            SemanticModel testClassSemanticModel)
        {
            return OrderTestClassMembers(testClassDeclaration.ChildNodes(), classUnderTestDeclaration, testClassSemanticModel);
        }

        public static List<SyntaxNode> OrderTestClassMembers(IEnumerable<SyntaxNode> testClassMembers, ClassDeclarationSyntax classUnderTestDeclaration,
            SemanticModel testClassSemanticModel)
        {
            IEnumerable<MethodDeclarationSyntax> testClassMethodDeclarations = testClassMembers.OfType<MethodDeclarationSyntax>();

            // TODO property, indexer tests
            IEnumerable<MethodDeclarationSyntax> classUnderTestMethodDeclarations = classUnderTestDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>();

            // TODO not very efficient, also does not handle properties etc
            List<SyntaxNode> result = new List<SyntaxNode>();
            result.AddRange(testClassMembers.OfType<FieldDeclarationSyntax>().OrderBy(f => f.Declaration.Variables.First().Identifier.ValueText));
            result.AddRange(testClassMembers.OfType<ConstructorDeclarationSyntax>());

            IEnumerable<MethodDeclarationSyntax> testAndDataMethodDeclarations = testClassMethodDeclarations.
                Where(m => IsTestMethod(m, testClassSemanticModel) || IsTestDataMethod(m));
            if (testAndDataMethodDeclarations.Count() > 0)
            {
                if (classUnderTestMethodDeclarations.Count() == 0)
                {
                    result.AddRange(testAndDataMethodDeclarations);
                }
                else
                {
                    foreach (MethodDeclarationSyntax classUnderTestMethod in classUnderTestMethodDeclarations)
                    {
                        string testMethodPrefix = $"{classUnderTestMethod.Identifier.ValueText}_";
                        IEnumerable<MethodDeclarationSyntax> classUnderTestMethodTestAndDataMethodDeclarations = testAndDataMethodDeclarations.
                            Where(t => t.Identifier.ValueText.StartsWith(testMethodPrefix));
                        IEnumerable<MethodDeclarationSyntax> classUnderTestMethodDataMethodDeclarations = classUnderTestMethodTestAndDataMethodDeclarations.
                            Where(t => IsTestDataMethod(t));
                        IEnumerable<MethodDeclarationSyntax> classUnderTestMethodTestMethodDeclarations = classUnderTestMethodTestAndDataMethodDeclarations.
                            Except(classUnderTestMethodDataMethodDeclarations);

                        foreach (MethodDeclarationSyntax classUnderTestMethodTestMethodDeclaration in classUnderTestMethodTestMethodDeclarations)
                        {
                            result.Add(classUnderTestMethodTestMethodDeclaration);
                            MethodDeclarationSyntax dataMethodDeclaration = classUnderTestMethodDataMethodDeclarations.
                                FirstOrDefault(c => c.Identifier.ValueText == classUnderTestMethodTestMethodDeclaration.Identifier.ValueText + "_Data");
                            if (dataMethodDeclaration != null)
                            {
                                result.Add(dataMethodDeclaration);
                            }
                        }
                    }
                }
            }

            result.AddRange(testClassMembers.
                OfType<MethodDeclarationSyntax>().
                Except(testAndDataMethodDeclarations).
                OrderBy(m => m.Identifier.ValueText));
            result.AddRange(testClassMembers.OfType<ClassDeclarationSyntax>().OrderBy(c => c.Identifier.ValueText));

            if (result.Count() != testClassMembers.Count())
            {
                result.AddRange(testClassMembers.Except(result));
            }

            return result;
        }

        public static MethodDeclarationSyntax CreateCreateMethodDeclaration(INamedTypeSymbol classUnderTest,
            SyntaxGenerator syntaxGenerator,
            bool createMockCreateMethod)
        {
            IMethodSymbol classUnderTestConstructor = classUnderTest.Constructors.OrderByDescending(c => c.Parameters.Count()).First();
            IEnumerable<IParameterSymbol> classUnderTestConstructorParameters = classUnderTestConstructor.Parameters;

            IEnumerable<SyntaxNode> parameterSyntaxes = classUnderTestConstructorParameters.
                Select(p => syntaxGenerator.ParameterDeclaration(p, syntaxGenerator.DefaultExpression(p.Type)));

            IEnumerable<SyntaxNode> resultExpressionArguments = parameterSyntaxes.
                Select(p => syntaxGenerator.Argument(syntaxGenerator.IdentifierName((p as ParameterSyntax).Identifier.ValueText)));

            SyntaxNode classUnderTestCreation = createMockCreateMethod ?
                CreateMockRepositoryCreateInvocationExpression(syntaxGenerator, classUnderTest.Name, "_mockRepository", resultExpressionArguments)
                : syntaxGenerator.ObjectCreationExpression(classUnderTest, resultExpressionArguments);

            SyntaxNode returnStatement = syntaxGenerator.ReturnStatement(classUnderTestCreation);

            SyntaxNode classUnderTestName = syntaxGenerator.TypeExpression(classUnderTest);

            return syntaxGenerator.MethodDeclaration(
                   createMockCreateMethod ? $"CreateMock{classUnderTest.Name}" : $"Create{classUnderTest.Name}",
                   parameters: parameterSyntaxes,
                   returnType: createMockCreateMethod ? syntaxGenerator.GenericName("Mock", classUnderTestName) : classUnderTestName,
                   accessibility: Accessibility.Private,
                   statements: new[] { returnStatement }) as MethodDeclarationSyntax;
        }

        /*
         * [Fact]
         * public void <MethodName>_<Description>()
         * {
         *      // Arrange
         *      ClassUnderTest testSubject = CreateClassUnderTest();
         *
         *      // Act
         *      testSubject.<MethodName>(<default values>);
         *      
         *      // Assert
         *      _mockRepository.VerifyAll();
         * }
         */
        public static MethodDeclarationSyntax CreateTestMethod(
            string classUnderTestName,
            string testMethodName,
            IMethodSymbol methodUnderTest,
            SyntaxGenerator syntaxGenerator)
        {
            SyntaxTrivia indentationTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "    ");
            SyntaxTrivia endOfLineTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");

            // TODO assumes create method exists
            SyntaxNode testSubjectLocalDeclaration = syntaxGenerator.
                LocalDeclarationStatement(
                    SyntaxFactory.IdentifierName(classUnderTestName),
                    "testSubject",
                    syntaxGenerator.InvocationExpression(SyntaxFactory.IdentifierName($"Create{classUnderTestName}"))).
                 WithLeadingTrivia(
                    indentationTrivia,
                    SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Arrange"),
                    endOfLineTrivia,
                    indentationTrivia);

            IEnumerable<SyntaxNode> methodUnderTestArgumentSyntaxes = methodUnderTest.
                Parameters.
                Select(p => syntaxGenerator.Argument(syntaxGenerator.DefaultExpression(p.Type)));

            SyntaxNode methodUnderTestInvocation = syntaxGenerator.
                InvocationExpression(syntaxGenerator.MemberAccessExpression(
                    SyntaxFactory.IdentifierName("testSubject"),
                    methodUnderTest.Name),
                    methodUnderTestArgumentSyntaxes).
                 WithLeadingTrivia(
                    endOfLineTrivia,
                    indentationTrivia,
                    SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Act"),
                    endOfLineTrivia,
                    indentationTrivia);

            SyntaxNode verifyAllInvocation = CreateMockRepositoryVerifyAllExpression(syntaxGenerator).
                 WithLeadingTrivia(
                    endOfLineTrivia,
                    indentationTrivia,
                    SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Assert"),
                    endOfLineTrivia,
                    indentationTrivia);

            MethodDeclarationSyntax methodDeclaration = syntaxGenerator.
                MethodDeclaration(testMethodName,
                    accessibility: Accessibility.Public,
                    statements: new[] { testSubjectLocalDeclaration, methodUnderTestInvocation, verifyAllInvocation }) as MethodDeclarationSyntax;

            return methodDeclaration.AddAttributeLists(syntaxGenerator.Attribute("Fact") as AttributeListSyntax);
        }

        /*
         * [Fact]
         * public void <MethodName>_<Description>()
         * {
         *      // Arrange
         *      ClassUnderTest testSubject = CreateClassUnderTest();
         *
         *      // Act and assert
         *      <Exception> result = Assert.Throws<<Exception>>(() => testSubject.<MethodName>(<default values>));
         *      _mockRepository.VerifyAll();
         * }
         */
        public static MethodDeclarationSyntax CreateExceptionTestMethod(
            string exceptionName,
            string classUnderTestName,
            string testMethodName,
            IMethodSymbol methodUnderTest,
            SyntaxGenerator syntaxGenerator)
        {
            SyntaxTrivia indentationTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "    ");
            SyntaxTrivia endOfLineTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");

            // TODO assumes create method exists
            SyntaxNode testSubjectLocalDeclaration = syntaxGenerator.
                LocalDeclarationStatement(
                    SyntaxFactory.IdentifierName(classUnderTestName),
                    "testSubject",
                    syntaxGenerator.InvocationExpression(SyntaxFactory.IdentifierName($"Create{classUnderTestName}"))).
                 WithLeadingTrivia(
                    indentationTrivia,
                    SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Arrange"),
                    endOfLineTrivia,
                    indentationTrivia);

            IEnumerable<SyntaxNode> methodUnderTestArgumentSyntaxes = methodUnderTest.
                Parameters.
                Select(p => syntaxGenerator.Argument(syntaxGenerator.DefaultExpression(p.Type)));

            SyntaxNode methodUnderTestInvocation = syntaxGenerator.
                InvocationExpression(syntaxGenerator.MemberAccessExpression(
                    SyntaxFactory.IdentifierName("testSubject"),
                    methodUnderTest.Name),
                    methodUnderTestArgumentSyntaxes);

            SyntaxNode lambdaExpression = syntaxGenerator.VoidReturningLambdaExpression(methodUnderTestInvocation);

            SyntaxNode throwsMemberAccessExpression = syntaxGenerator.
                MemberAccessExpression(
                    SyntaxFactory.IdentifierName("Assert"),
                    syntaxGenerator.GenericName("Throws", SyntaxFactory.IdentifierName(exceptionName)));

            SyntaxNode throwsInvocation = syntaxGenerator.InvocationExpression(throwsMemberAccessExpression, lambdaExpression);

            SyntaxNode resultLocalDeclaration = syntaxGenerator.
                LocalDeclarationStatement(
                    SyntaxFactory.IdentifierName(exceptionName),
                    "result",
                    throwsInvocation).
                 WithLeadingTrivia(
                    endOfLineTrivia,
                    indentationTrivia,
                    SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Act and assert"),
                    endOfLineTrivia,
                    indentationTrivia);

            SyntaxNode verifyAllInvocation = CreateMockRepositoryVerifyAllExpression(syntaxGenerator).
                 WithLeadingTrivia(indentationTrivia);

            MethodDeclarationSyntax methodDeclaration = syntaxGenerator.
                MethodDeclaration(testMethodName,
                    accessibility: Accessibility.Public,
                    statements: new[] { testSubjectLocalDeclaration, resultLocalDeclaration, verifyAllInvocation }) as MethodDeclarationSyntax;

            return methodDeclaration.AddAttributeLists(syntaxGenerator.Attribute("Fact") as AttributeListSyntax);
        }

        public static ExpressionStatementSyntax CreateMockRepositoryVerifyAllExpression(SyntaxGenerator syntaxGenerator)
        {
            // TODO assumes that a MockRepository instances named _mockRepository exists
            SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(syntaxGenerator.IdentifierName("_mockRepository"), "VerifyAll");
            SyntaxNode invocationExpression = syntaxGenerator.InvocationExpression(memberAccessExpression);
            return syntaxGenerator.ExpressionStatement(invocationExpression) as ExpressionStatementSyntax;
        }

        public static List<XmlElementSyntax> GetMethodExceptionXmlElements(MethodDeclarationSyntax methodDeclaration,
            IMethodSymbol method)
        {
            // Get documentation trivia
            DocumentationCommentTriviaSyntax documentationCommentTrivia = DocumentationHelper.GetDocumentCommentTrivia(methodDeclaration);
            if (documentationCommentTrivia == null)
            {
                return new List<XmlElementSyntax>();
            }
            XmlNodeSyntax inheritdocNode = DocumentationHelper.GetXmlNodeSyntaxes(documentationCommentTrivia, "inheritdoc").FirstOrDefault();
            if (inheritdocNode != null)
            {
                documentationCommentTrivia = DocumentationHelper.GetInheritedDocumentCommentTrivia(method);
                if (documentationCommentTrivia == null)
                {
                    return new List<XmlElementSyntax>();
                }
            }

            // Return exception elements that are not empty
            return DocumentationHelper.
                GetXmlNodeSyntaxes(documentationCommentTrivia, "exception").
                Where(x => x is XmlElementSyntax).
                Cast<XmlElementSyntax>().
                ToList();
        }

        public static XmlCrefAttributeSyntax GetXmlElementCrefAttribute(XmlElementSyntax xmlElement)
        {
            return xmlElement.
                        StartTag.
                        Attributes.
                        Where(a => a is XmlCrefAttributeSyntax).
                        Cast<XmlCrefAttributeSyntax>().
                        FirstOrDefault();
        }
    }
}
