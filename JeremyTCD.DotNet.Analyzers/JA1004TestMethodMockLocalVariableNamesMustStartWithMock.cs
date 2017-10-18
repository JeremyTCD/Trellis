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
    public class JA1004TestMethodMockLocalVariableNamesMustStartWithMock : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1004TestMethodMockLocalVariableNamesMustStartWithMock"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1004";

        private const string Title = "Test method mock local variable names must start with \"mock\".";
        private const string MessageFormat = "Mock local variable name \"{0}\" must begin with \"mock\".";
        private const string Description = "A test method mock local varaible's name does not start with \"mock\".";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

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

            // Find invocations of Mock<T>.Setup
            IEnumerable<LocalDeclarationStatementSyntax> localDeclarations = compilationUnit.
                DescendantNodes().
                OfType<LocalDeclarationStatementSyntax>();
            if(localDeclarations.Count() == 0)
            {
                return;
            }

            INamedTypeSymbol lazyMockType = null;
            foreach(LocalDeclarationStatementSyntax localDeclaration in localDeclarations)
            {
                GenericNameSyntax genericName = localDeclaration.Declaration.Type as GenericNameSyntax;
                if(genericName == null)
                {
                    continue;
                }

                if (!genericName.Identifier.ValueText.StartsWith("Mock"))
                {
                    continue;
                }

                if(lazyMockType == null)
                {
                     lazyMockType = context.Compilation.GetTypeByMetadataName("Moq.Mock`1");
                }
                if(context.
                    SemanticModel.
                    GetTypeInfo(localDeclaration.Declaration.Type).
                    Type.
                    OriginalDefinition != lazyMockType)
                {
                    continue;
                }

                // Get all occurences of the local variable within its method, if it ever calls a method named "setup", its own name must begin with mock
                // TODO multiple variables in declaration
                SyntaxToken variableToken = localDeclaration.Declaration.Variables.First().Identifier;
                string variableName = variableToken.ValueText;
                MethodDeclarationSyntax methodDeclaration = localDeclaration.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if(methodDeclaration.
                    DescendantNodes().
                    OfType<MemberAccessExpressionSyntax>().
                    Any(m => (m.Expression as IdentifierNameSyntax)?.Identifier.ValueText == variableName && m.Name.Identifier.ValueText == "Setup") &&
                    !variableName.StartsWith("mock"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableToken.GetLocation(), variableName));
                }
            }
        }
    }
}