using Microsoft.CodeAnalysis;
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
            string typeName, string mockRepositoryVariableName)
        {
            SyntaxNode typeIdentifier = syntaxGenerator.IdentifierName(typeName);
            SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(
                syntaxGenerator.IdentifierName(mockRepositoryVariableName),
                syntaxGenerator.GenericName("Create", typeIdentifier));

            return (InvocationExpressionSyntax)syntaxGenerator.InvocationExpression(memberAccessExpression);
        }

        public static bool ContainsTestClass(CompilationUnitSyntax compilationUnit)
        {
            return compilationUnit.Usings.Any(u => string.Equals(u.Name.ToString(), "Xunit", StringComparison.Ordinal));
        }

        public static bool IsUnitTestClass(ClassDeclarationSyntax testClassDeclaration)
        {
            return testClassDeclaration.Identifier.ValueText.EndsWith("UnitTests");
        }

        public static bool IsIntegrationTestClass(ClassDeclarationSyntax testClassDeclaration)
        {
            return testClassDeclaration.Identifier.ValueText.EndsWith("IntegrationTests");
        }

        public static bool IsEndToEndTestClass(ClassDeclarationSyntax testClassDeclaration)
        {
            return testClassDeclaration.Identifier.ValueText.EndsWith("EndToEndTests");
        }

        public static bool IsTestMethod(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            return IsTestMethod(methodSymbol);
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

            List<ITypeSymbol> types = new List<ITypeSymbol>();
            SymbolHelper.TryGetTypeSymbol(classUnderTestName, globalNamespace, types);

            // More than one types with the same name (from different namespaces)
            if(types.Count() > 1)
            {
                string classNamespaceName = testClassDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();

                foreach(ITypeSymbol type in types)
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
    }
}
