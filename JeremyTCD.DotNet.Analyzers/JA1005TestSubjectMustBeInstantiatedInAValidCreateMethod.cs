// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            INamedTypeSymbol classUnderTestType = TestingHelper.GetClassUnderTest(classDeclaration, context.Compilation.GlobalNamespace) as INamedTypeSymbol;
            if (classUnderTestType == null)
            {
                return;
            }

            // Return if class under test is an abstract class (no constructor arguments)
            if (classUnderTestType.IsAbstract)
            {
                return;
            }

            // Find all test method class under test or mock class under test local declarations
            // TODO if declaration and assignment are separate
            string classUnderTestName = classUnderTestType.Name.ToString();
            string mockClassUnderTestTypeName = $"Moq.Mock<{classUnderTestType.ToDisplayString()}>";
            string createClassUnderTestMethodName = $"Create{classUnderTestName}";
            string createMockClassUnderTestMethodName = $"CreateMock{classUnderTestName}";
            IEnumerable<MethodDeclarationSyntax> testMethodDeclarations = TestingHelper.
                GetTestMethodDeclarations(compilationUnit, context.SemanticModel);

            IEnumerable<ExpressionSyntax> testMethodExpressions = testMethodDeclarations.
                SelectMany(m => m.DescendantNodes().OfType<ExpressionSyntax>());

            Dictionary<(string, ITypeSymbol), bool> CreateMethodStates = new Dictionary<(string, ITypeSymbol), bool>();
            foreach (ExpressionSyntax expression in testMethodExpressions)
            {
                bool isObjectCreationExpression = expression is ObjectCreationExpressionSyntax;
                bool isInvocationExpression = expression is InvocationExpressionSyntax;

                // Continue if expression is not an object creation expression or an invocation expression
                if (!isObjectCreationExpression && !isInvocationExpression)
                {
                    continue;
                }

                // Check if expression returns class under test or a mock of it
                ITypeSymbol returnType = context.SemanticModel.GetTypeInfo(expression).Type;
                bool returnsClassUnderTest = returnType == classUnderTestType;
                bool returnsMockClassUnderTest = string.Equals(returnType.ToDisplayString(), mockClassUnderTestTypeName);
                if (!returnsClassUnderTest && !returnsMockClassUnderTest)
                {
                    continue;
                }

                // Check if valid create method exists
                string createMethodName = returnsClassUnderTest ? createClassUnderTestMethodName : createMockClassUnderTestMethodName;
                if (!CreateMethodStates.TryGetValue((createMethodName, returnType), out bool validCreateMethodExists))
                {
                    validCreateMethodExists = CheckValidCreateMethodExist(classDeclaration, createMethodName, returnType, classUnderTestType,
                    context.SemanticModel);

                    CreateMethodStates.Add((createMethodName, returnType), validCreateMethodExists);
                }

                // Check if invocation is correct
                // TODO assumes create method is not overloaded (it should not be, all parameters are optional and map to the constructor)
                // TODO assumes parameters are valid
                bool correctInvocation = (expression as InvocationExpressionSyntax)?.Expression.ToString() == createMethodName;
                if (correctInvocation && validCreateMethodExists)
                {
                    continue;
                }

                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder.Add(ClassUnderTestFullyQualifiedNameProperty, classUnderTestType.ToDisplayString());
                builder.Add(CreateMethodNameProperty, returnType == classUnderTestType ? createClassUnderTestMethodName : createMockClassUnderTestMethodName);
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
        public bool CheckValidCreateMethodExist(ClassDeclarationSyntax classDeclarationSyntax, string expectedName, ITypeSymbol expectedReturnType, 
            INamedTypeSymbol classUnderTestSymbol, SemanticModel semanticModel)
        {
            IMethodSymbol createMethod = classDeclarationSyntax.
                DescendantNodes().
                OfType<MethodDeclarationSyntax>().
                Select(m => semanticModel.GetDeclaredSymbol(m) as IMethodSymbol).
                Where(m => m != null && m.ReturnType == expectedReturnType && m.Name == expectedName).
                FirstOrDefault();
            if (createMethod == null)
            {
                return false;
            }

            IEnumerable<IParameterSymbol> createMethodParameters = createMethod.Parameters;

            // Create method and constructor parameters must be identical
            IMethodSymbol classUnderTestConstructor = classUnderTestSymbol.Constructors.OrderByDescending(c => c.Parameters.Count()).First();
            IEnumerable<IParameterSymbol> classUnderTestConstructorParameters = classUnderTestConstructor.Parameters;
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
                createMethod.ReturnType == classUnderTestSymbol && !(returnStatement.Expression is ObjectCreationExpressionSyntax) ||
                createMethod.ReturnType != classUnderTestSymbol && !(returnStatement.Expression is InvocationExpressionSyntax)) // TODO ensure that MockRepository.Create is called
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