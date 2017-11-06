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
    /// <summary>
    /// A test method's dummy local variable is not a mock of its type's interface.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs if a test method contains a dummy local variable that is not a mock of its type's interface.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1000UnitTestMethodsMustUseInterfaceMocksForDummies : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1000UnitTestMethodsMustUseInterfaceMocksForDummies).Substring(0, 6);
        public const string InterfaceIdentifierProperty = "InterfaceIdentifierProperty";
        public const string VariableIdentifierProperty = "VariableIdentifierProeprty";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1000_Title,
                Strings.JA1000_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1000_Description,
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

            // Ensure that class is a unit test class
            if (!testClassContext.IsUnitTestClass)
            {
                return;
            }

            foreach (VariableDeclaratorSyntax variableDeclarator in testClassContext.
                TestMethodDeclarations.
                SelectMany(m => m.DescendantNodes().
                    OfType<LocalDeclarationStatementSyntax>().
                    SelectMany(l => l.Declaration.Variables))
                )
            {
                EqualsValueClauseSyntax initializer = variableDeclarator.Initializer;
                if (initializer == null)
                {
                    continue;
                }

                // Do not create diagnostic for class under test declarations
                ITypeSymbol typeSymbol = context.SemanticModel.GetTypeInfo(initializer.Value).Type;
                if (typeSymbol == null || typeSymbol == testClassContext.ClassUnderTest || typeSymbol.OriginalDefinition?.ToDisplayString() == "Moq.Mock<T>")
                {
                    continue;
                }

                // Do not create diagnostic for types that only implement core framework interfaces
                INamedTypeSymbol interfaceSymbol = typeSymbol.
                    Interfaces.
                    FirstOrDefault(i => !i.ToDisplayString().StartsWith("System."));

                if (interfaceSymbol != null)
                {
                    ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder.Add(InterfaceIdentifierProperty, interfaceSymbol.Name);
                    builder.Add(VariableIdentifierProperty, variableDeclarator.Identifier.ToString());

                    // Don't offer codefix if object creation expression with non null arguments is used 
                    ObjectCreationExpressionSyntax objectCreationExpression = variableDeclarator.
                        DescendantNodes().
                        OfType<ObjectCreationExpressionSyntax>().
                        FirstOrDefault();
                    if (objectCreationExpression != null && objectCreationExpression.ArgumentList?.Arguments.Any(a => a.ToString() != "null") == true)
                    {
                        builder.Add(Constants.NoCodeFix, null);
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableDeclarator.Identifier.GetLocation(), builder.ToImmutable()));
                }

            }
        }
    }
}