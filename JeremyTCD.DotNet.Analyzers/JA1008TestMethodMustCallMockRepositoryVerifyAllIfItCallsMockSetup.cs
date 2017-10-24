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
    public class JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1008_Title,
                Strings.JA1008_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1008_Description,
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
            SemanticModel semanticModel = context.SemanticModel;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Find test methods
            IEnumerable<MethodDeclarationSyntax> testMethodDeclarations = compilationUnit.
                DescendantNodes().
                OfType<MethodDeclarationSyntax>().
                Where(m => TestingHelper.IsTestMethod(m, semanticModel));
            if (testMethodDeclarations.Count() == 0)
            {
                return;
            }

            foreach(MethodDeclarationSyntax testMethodDeclaration in testMethodDeclarations)
            {
                IEnumerable <IMethodSymbol> invokedMethods = testMethodDeclaration.
                    DescendantNodes().
                    OfType<InvocationExpressionSyntax>().
                    Select(i => semanticModel.GetSymbolInfo(i).Symbol as IMethodSymbol);

                if(invokedMethods.Any(i => TestingHelper.IsMockSetupMethod(i)) &&
                    !invokedMethods.Any(i => TestingHelper.IsMockRepositoryVerifyAllMethod(i)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, testMethodDeclaration.Identifier.GetLocation()));
                }
            }
        }
    }
}