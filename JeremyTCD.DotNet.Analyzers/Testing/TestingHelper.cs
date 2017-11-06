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
            string typeName, IEnumerable<SyntaxNode> arguments = null)
        {
            SyntaxNode typeIdentifier = syntaxGenerator.IdentifierName(typeName);
            SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(
                syntaxGenerator.IdentifierName("_mockRepository"),
                syntaxGenerator.GenericName("Create", typeIdentifier));

            return (InvocationExpressionSyntax)syntaxGenerator.InvocationExpression(memberAccessExpression, arguments ?? new SyntaxNode[] { });
        }

        public static VariableDeclarationSyntax GetMockRepositoryVariableDeclaration(CompilationUnitSyntax compilationUnit, SemanticModel semanticModel)
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

        public static IMethodSymbol GetMethodUnderTest(MethodDeclarationSyntax testMethodDeclaration, ITypeSymbol classUnderTest,
            SemanticModel testClassSemanticModel)
        {
            // Find all member access expressions that are children of invocation expressions
            IEnumerable<MemberAccessExpressionSyntax> memberAccessExpressions = testMethodDeclaration.
                DescendantNodes().
                OfType<InvocationExpressionSyntax>().
                SelectMany(i => i.ChildNodes().OfType<MemberAccessExpressionSyntax>());

            // Find all expressions that are on the class under test
            IEnumerable<MemberAccessExpressionSyntax> classUnderTestMemberAccessExpressions = memberAccessExpressions.
                Where(m =>
                {
                    ISymbol symbol = testClassSemanticModel.GetSymbolInfo(m.Expression).Symbol;
                    return (symbol as ILocalSymbol)?.Type == classUnderTest || (symbol as IPropertySymbol)?.Type == classUnderTest;
                });
            if (classUnderTestMemberAccessExpressions.Count() == 1)
            {
                return testClassSemanticModel.GetSymbolInfo(classUnderTestMemberAccessExpressions.Single().Name).Symbol as IMethodSymbol;
            }

            // If there is more than one, filter out those that are within setup invocations
            foreach (MemberAccessExpressionSyntax memberAccessExpression in classUnderTestMemberAccessExpressions)
            {
                if (!memberAccessExpression.
                    Parent.
                    Ancestors().
                    OfType<InvocationExpressionSyntax>().
                    Any(i =>
                    {
                        MemberAccessExpressionSyntax childMemberAccessExpression = i.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
                        return childMemberAccessExpression != null && childMemberAccessExpression.Name.ToString() == "Setup";
                    }))
                {
                    return testClassSemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;
                }
            }

            return null;
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

        public static List<SyntaxNode> OrderTestClassMembers(
            TestClassContext testClassContext,
            SemanticModel classUnderTestSemanticModel)
        {
            // TODO not very efficient, also does not handle properties etc
            List<SyntaxNode> result = new List<SyntaxNode>();
            result.AddRange(testClassContext.MemberDeclarations.OfType<FieldDeclarationSyntax>().OrderBy(f => f.Declaration.Variables.First().Identifier.ValueText));
            result.AddRange(testClassContext.MemberDeclarations.OfType<ConstructorDeclarationSyntax>());

            if (testClassContext.TestMethods.Count() > 0)
            {
                if (testClassContext.ClassUnderTestMethods.Count() == 0)
                {
                    foreach (MethodDeclarationSyntax testMethodDeclaration in testClassContext.TestMethodDeclarations)
                    {
                        result.Add(testMethodDeclaration);
                        if (testClassContext.
                            TestMethodDeclarationsAndTheirDataMethodDeclarations.
                            TryGetValue(testMethodDeclaration, out MethodDeclarationSyntax dataMethodDeclaration))
                        {
                            result.Add(dataMethodDeclaration);
                        }
                    }
                }
                else
                {
                    foreach (IMethodSymbol methodUnderTest in testClassContext.ClassUnderTestMethods)
                    {
                        SyntaxNode methodUnderTestDeclaration = methodUnderTest.DeclaringSyntaxReferences.First().GetSyntax();

                        IEnumerable<MethodDeclarationSyntax> testMethodDeclarations = testClassContext.
                            TestMethodDeclarationsAndTheirMethodUnderTests.
                            Where(tuple => tuple.Item2?.DeclaringSyntaxReferences.First().GetSyntax() == methodUnderTestDeclaration).
                            Select(tuple => tuple.Item1);

                        foreach (MethodDeclarationSyntax testMethodDeclaration in testMethodDeclarations)
                        {
                            result.Add(testMethodDeclaration);
                            if (testClassContext.
                                TestMethodDeclarationsAndTheirDataMethodDeclarations.
                                TryGetValue(testMethodDeclaration, out MethodDeclarationSyntax dataMethodDeclaration))
                            {
                                result.Add(dataMethodDeclaration);
                            }
                        }
                    }
                }
            }

            result.AddRange(testClassContext.
                MemberDeclarations.
                Except(result).
                OfType<MethodDeclarationSyntax>().
                OrderBy(m => m.Identifier.ValueText));
            result.AddRange(testClassContext.MemberDeclarations.OfType<ClassDeclarationSyntax>().OrderBy(c => c.Identifier.ValueText));

            if (result.Count() != testClassContext.MemberDeclarations.Count())
            {
                result.AddRange(testClassContext.MemberDeclarations.Except(result));
            }

            return result;
        }

        public static MethodDeclarationSyntax GetDataMethodDeclaration(MethodDeclarationSyntax testMethodDeclaration,
            TestClassContext testClassContext, SemanticModel semanticModel)
        {
            AttributeSyntax attributeSyntax = testMethodDeclaration.
                DescendantNodes().
                OfType<AttributeSyntax>().
                Where(a => semanticModel.GetSymbolInfo(a).Symbol.ToDisplayString() == "Xunit.MemberDataAttribute.MemberDataAttribute(string, params object[])").
                FirstOrDefault();
            if (attributeSyntax  == null)
            {
                return null;
            }
            InvocationExpressionSyntax attributeSyntaxFirstArgumentExpression = attributeSyntax.
                ArgumentList.
                Arguments.
                FirstOrDefault()?.Expression as InvocationExpressionSyntax;
            if (attributeSyntaxFirstArgumentExpression == null || attributeSyntaxFirstArgumentExpression.Expression.ToString() != "nameof")
            {
                return null;
            }
            IdentifierNameSyntax dataMethodIdentifier = attributeSyntaxFirstArgumentExpression.
                ArgumentList.
                Arguments.
                FirstOrDefault()?.Expression as IdentifierNameSyntax;
            if (dataMethodIdentifier == null)
            {
                return null;
            }
            string dataMethodName = dataMethodIdentifier.ToString();
            return testClassContext.
                MethodDeclarations.
                Where(m => m.Identifier.ValueText == dataMethodName).
                FirstOrDefault();
        }

        public static MethodDeclarationSyntax CreateCreateMethodDeclaration(INamedTypeSymbol classUnderTest,
            SyntaxGenerator syntaxGenerator,
            bool createMockCreateMethod,
            IEnumerable<IParameterSymbol> classUnderTestConstructorParameters = null)
        {
            if (classUnderTestConstructorParameters == null)
            {
                classUnderTestConstructorParameters = classUnderTest.
                    Constructors.
                    OrderByDescending(c => c.Parameters.Count()).
                    First().
                    Parameters;
            }

            IEnumerable<SyntaxNode> parameterSyntaxes = classUnderTestConstructorParameters.
                Select(p =>
                {
                    SyntaxNode defaultExpression = syntaxGenerator.DefaultExpression(p.Type);

                    return (p.DeclaringSyntaxReferences.First().GetSyntax() as ParameterSyntax).
                        WithDefault(SyntaxFactory.EqualsValueClause(defaultExpression as ExpressionSyntax));
                });

            IEnumerable<SyntaxNode> resultExpressionArguments = parameterSyntaxes.
                Select(p => syntaxGenerator.Argument(syntaxGenerator.IdentifierName((p as ParameterSyntax).Identifier.ValueText)));

            SyntaxNode classUnderTestCreation = createMockCreateMethod ?
                CreateMockRepositoryCreateInvocationExpression(syntaxGenerator, classUnderTest.Name, resultExpressionArguments)
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

        public static List<SyntaxNode> CreateMissingUsingDirectives(TestClassContext testClassContext)
        {
            return CreateMissingUsingDirectives(
                   testClassContext.ClassUnderTestMainConstructor.Parameters.Select(p => p.Type.ContainingNamespace),
                   testClassContext.ClassDeclaration,
                   testClassContext.CompilationUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault());
        }

        public static List<SyntaxNode> CreateMissingUsingDirectives(
            IEnumerable<INamespaceSymbol> namespaceSymbols,
            ClassDeclarationSyntax classDeclaration,
            NamespaceDeclarationSyntax namespaceDeclaration)
        {
            List<SyntaxNode> result = new List<SyntaxNode>();

            IEnumerable<string> existingImportedNamespaces = classDeclaration.
                DescendantNodes().
                OfType<UsingDirectiveSyntax>().
                Select(u => u.Name.ToString());

            string currentNamespace = namespaceDeclaration?.Name.ToString() ?? string.Empty;

            foreach (INamespaceSymbol namespaceSymbol in namespaceSymbols)
            {
                string namespaceToImport = namespaceSymbol.ToDisplayString();
                if (!existingImportedNamespaces.Contains(namespaceToImport) &&
                    !currentNamespace.StartsWith(namespaceToImport))
                {
                    result.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(namespaceSymbol.ToDisplayString())));
                }
            }

            return result;
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
