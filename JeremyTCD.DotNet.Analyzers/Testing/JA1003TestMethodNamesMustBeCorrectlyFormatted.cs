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
    public class JA1003TestMethodNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1003TestMethodNamesMustBeCorrectlyFormatted).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1003_Title,
                Strings.JA1003_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1003_Description,
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

            // Get test methods
            foreach ((MethodDeclarationSyntax testMethodDeclaration, IMethodSymbol methodUnderTest) in testClassContext.
                TestMethodDeclarationsAndTheirMethodUnderTests)
            {
                if (methodUnderTest == null)
                {
                    // Check if test method name starts with the name of any method in the test class
                    if (testClassContext.ClassUnderTestMethods.
                        Any(m => testMethodDeclaration.Identifier.ValueText.StartsWith($"{m.Name}_")))
                    {
                        continue;
                    }
                }
                else if (testMethodDeclaration.Identifier.ValueText.StartsWith($"{methodUnderTest.Name}_"))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, testMethodDeclaration.Identifier.GetLocation()));
            }

        }
    }
}