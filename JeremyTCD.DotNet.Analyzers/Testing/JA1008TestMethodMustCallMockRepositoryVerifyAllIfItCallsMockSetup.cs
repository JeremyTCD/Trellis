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
            TestClassContext testClassContext = TestClassContextFactory.TryCreate(context);
            if (testClassContext == null)
            {
                return;
            }

            foreach(MethodDeclarationSyntax testMethodDeclaration in testClassContext.TestMethodDeclarations)
            {
                IEnumerable <IMethodSymbol> invokedMethods = testMethodDeclaration.
                    DescendantNodes().
                    OfType<InvocationExpressionSyntax>().
                    Select(i => testClassContext.SemanticModel.GetSymbolInfo(i).Symbol as IMethodSymbol);

                if(invokedMethods.Any(i => TestingHelper.IsMockSetupMethod(i)) &&
                    !invokedMethods.Any(i => TestingHelper.IsMockRepositoryVerifyAllMethod(i)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, testMethodDeclaration.Identifier.GetLocation()));
                }
            }
        }
    }
}