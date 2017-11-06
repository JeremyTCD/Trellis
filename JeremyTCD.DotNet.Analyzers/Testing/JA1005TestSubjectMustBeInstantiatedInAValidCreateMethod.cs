// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod).Substring(0, 6);
        public const string ClassUnderTestFullyQualifiedNameProperty = "ClassUnderTestFullyQualifiedNameProperty";
        public const string CreateMethodNameProperty = "CreateMethodNameProperty";
        public const string InvocationInvalidProperty = "InvocationInvalidProperty";
        public const string NoValidCreateMethodProperty = "NoValidCreateMethodProperty";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1005_Title,
                Strings.JA1005_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1005_Description,
                "");

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.CompilationUnit);
            context.EnableConcurrentExecution();
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            TestClassContext testClassContext = TestClassContextFactory.TryCreate(context);
            if (testClassContext == null || testClassContext.ClassUnderTest == null)
            {
                return;
            }

            // Return if class under test is an abstract class (no constructor arguments)
            if (testClassContext.ClassUnderTest.IsAbstract)
            {
                return;
            }

            // Create create method names
            string createClassUnderTestMethodName = $"Create{testClassContext.ClassUnderTestName}";
            string createMockClassUnderTestMethodName = $"CreateMock{testClassContext.ClassUnderTestName}";
            string classUnderTestFullyQualifiedName = testClassContext.ClassUnderTest.ToDisplayString();
            string mockClassUnderTestFullyQualifiedName = $"Moq.Mock<{classUnderTestFullyQualifiedName}>";

            // Get expressions
            IEnumerable<(SyntaxNode, string)> testSubjectInstantiationExpressions = testClassContext.
                TestMethodDeclarations.
                SelectMany(m => m.
                    DescendantNodes().
                    Where(n => n is ObjectCreationExpressionSyntax || n is InvocationExpressionSyntax).
                    Select(n => {
                        ITypeSymbol type = context.SemanticModel.GetTypeInfo(n).Type;
                        string typeFullyQualifiedName = type.ToDisplayString();
                        string expectedInvocationName = typeFullyQualifiedName == classUnderTestFullyQualifiedName ? createClassUnderTestMethodName :
                            typeFullyQualifiedName == mockClassUnderTestFullyQualifiedName ? createMockClassUnderTestMethodName : null;
                        return (n, expectedInvocationName);
                    }).
                    Where(n => n.Item2 != null));
            if(testSubjectInstantiationExpressions.Count() == 0)
            {
                return;
            }

            // Check if valid create methods exist
            bool validCreateMethodExists = CheckValidCreateMethodExist(
                testClassContext.ClassDeclaration,
                createClassUnderTestMethodName,
                classUnderTestFullyQualifiedName,
                testClassContext);
            bool validMockCreateMethodExists = CheckValidCreateMethodExist(
                testClassContext.ClassDeclaration,
                createMockClassUnderTestMethodName,
                mockClassUnderTestFullyQualifiedName,
                testClassContext);

            foreach ((SyntaxNode expression, string expectedInvocationName) in testSubjectInstantiationExpressions)
            {
                // Check if required create method is valid and exists
                bool requiredValidCreateMethodExists = expectedInvocationName.Equals(createClassUnderTestMethodName, StringComparison.OrdinalIgnoreCase) ? 
                    validCreateMethodExists : validMockCreateMethodExists;

                // Check if invocation is correct
                // TODO assumes create method is not overloaded (it should not be, all parameters are optional and map to the constructor)
                // TODO assumes parameters are valid
                bool correctInvocation = (expression as InvocationExpressionSyntax)?.Expression.ToString() == expectedInvocationName;
                if (correctInvocation && requiredValidCreateMethodExists)
                {
                    continue;
                }

                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder.Add(ClassUnderTestFullyQualifiedNameProperty, classUnderTestFullyQualifiedName);
                builder.Add(CreateMethodNameProperty, expectedInvocationName);
                if (!correctInvocation)
                {
                    builder.Add(InvocationInvalidProperty, null);
                }
                if (!validCreateMethodExists)
                {
                    builder.Add(NoValidCreateMethodProperty, null);
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, expression.GetLocation(), builder.ToImmutable()));
            }
        }

        /// <summary>
        /// Correct name, return type, parameters and return expression.
        /// </summary>
        /// <param name="createMethod"></param>
        /// <param name="classUnderTestSymbol"></param>
        /// <returns></returns>
        public bool CheckValidCreateMethodExist(
            ClassDeclarationSyntax classDeclarationSyntax, 
            string expectedName, 
            string returnTypeFullyQualifiedName, 
            TestClassContext testClassContext)
        {
            IMethodSymbol createMethod = testClassContext.
                GetDescendantNodes<MethodDeclarationSyntax>().
                Select(m => testClassContext.SemanticModel.GetDeclaredSymbol(m) as IMethodSymbol).
                Where(m => m != null && m.ReturnType.ToDisplayString() == returnTypeFullyQualifiedName && m.Name == expectedName).
                FirstOrDefault();
            if (createMethod == null)
            {
                return false;
            }

            // Ensure that parameters match and that create method parameters are optional
            IEnumerable<IParameterSymbol> createMethodParameters = createMethod.Parameters;
            IEnumerable<IParameterSymbol> classUnderTestConstructorParameters = testClassContext.ClassUnderTestMainConstructor.Parameters;
            if (createMethodParameters.Count() != classUnderTestConstructorParameters.Count())
            {
                return false;
            }
            for (int i = 0; i < createMethodParameters.Count(); i++)
            {
                IParameterSymbol createMethodParameter = createMethodParameters.ElementAt(i);
                IParameterSymbol constructorParameter = classUnderTestConstructorParameters.ElementAt(i);

                if (!createMethodParameter.Type.Equals(constructorParameter.Type) ||
                    createMethodParameter.Name != constructorParameter.Name ||
                    !createMethodParameter.IsOptional)
                {
                    return false;
                }
            }

            // Return expression must be either new ClassUnderTest or mockrepository.create<classundertest>
            MethodDeclarationSyntax createMethodDeclaration = createMethod.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax;
            ReturnStatementSyntax returnStatement = createMethodDeclaration.Body.Statements.FirstOrDefault() as ReturnStatementSyntax;
            if (returnStatement == null ||
                createMethod.ReturnType == testClassContext.ClassUnderTest && !(returnStatement.Expression is ObjectCreationExpressionSyntax) ||
                createMethod.ReturnType != testClassContext.ClassUnderTest && !(returnStatement.Expression is InvocationExpressionSyntax)) // TODO ensure that MockRepository.Create is called
            {
                return false;
            }

            // Create method parameters must be passed as arguments to return expression
            IEnumerable<ArgumentSyntax> returnExpressionArguments = returnStatement.DescendantNodes().OfType<ArgumentListSyntax>().First().Arguments;
            if (returnExpressionArguments.Count() != createMethodParameters.Count())
            {
                return false;
            }
            for (int i = 0; i < createMethodParameters.Count(); i++)
            {
                ArgumentSyntax argument = returnExpressionArguments.ElementAt(i);
                IParameterSymbol createMethodParameter = createMethodParameters.ElementAt(i);

                if (argument.Expression.ToString() != createMethodParameter.Name)
                {
                    return false;
                }
            }

            return true;
        }
    }
}