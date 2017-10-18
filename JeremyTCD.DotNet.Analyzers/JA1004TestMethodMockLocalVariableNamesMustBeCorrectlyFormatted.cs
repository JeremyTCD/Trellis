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
    public class JA1004TestMethodMockLocalVariableNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1004TestMethodMockLocalVariableNamesMustBeCorrectlyFormatted).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
                    new DiagnosticDescriptor(DiagnosticId,
                        Strings.JA1004_Title,
                        Strings.JA1004_MessageFormat,
                        Strings.CategoryName_Testing,
                        DiagnosticSeverity.Warning,
                        true,
                        Strings.JA1004_Description,
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

            // Look for local variables of type Mock<T>
            IEnumerable<LocalDeclarationStatementSyntax> localDeclarations = compilationUnit.
                DescendantNodes().
                OfType<LocalDeclarationStatementSyntax>();
            if (localDeclarations.Count() == 0)
            {
                return;
            }

            INamedTypeSymbol mockGenericType = context.Compilation.GetTypeByMetadataName("Moq.Mock`1");
            if (mockGenericType == null)
            {
                return;
            }

            foreach (LocalDeclarationStatementSyntax localDeclaration in localDeclarations)
            {
                // Skip if variable is not of type Mock<T>
                GenericNameSyntax genericName = localDeclaration.Declaration.Type as GenericNameSyntax;
                if (genericName == null ||
                    !genericName.Identifier.ValueText.StartsWith("Mock") ||
                    context.SemanticModel.GetTypeInfo(genericName).Type.OriginalDefinition != mockGenericType)
                {
                    continue;
                }

                // TODO skip if mock is of class under test

                // Get all occurences of the local variable within its method, if it ever calls a method named "setup", its own name must begin with mock
                SyntaxToken variableToken = localDeclaration.Declaration.Variables.First().Identifier;
                string variableName = variableToken.ValueText;
                MethodDeclarationSyntax methodDeclaration = localDeclaration.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (methodDeclaration.
                    DescendantNodes().
                    OfType<MemberAccessExpressionSyntax>().
                    Any(m => (m.Expression as IdentifierNameSyntax)?.Identifier.ValueText == variableName && m.Name.Identifier.ValueText == "Setup") &&
                    !variableName.StartsWith("mock"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableToken.GetLocation(), variableName));
                }
                // TODO if setup not called, name should begin with dummy
            }
        }
    }
}