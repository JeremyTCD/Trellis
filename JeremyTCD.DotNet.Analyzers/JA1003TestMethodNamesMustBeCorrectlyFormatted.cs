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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;
            SemanticModel semanticModel = context.SemanticModel;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Get class under test members
            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
            {
                return;
            }
            ITypeSymbol classUnderTest = TestingHelper.GetClassUnderTest(classDeclaration, context.Compilation.GlobalNamespace);
            if (classUnderTest == null)
            {
                return;
            }
            IEnumerable<ISymbol> classUnderTestMembers = classUnderTest.GetMembers();

            // Get test methods
            IEnumerable<MethodDeclarationSyntax> testMethodDeclarations = TestingHelper.GetTestMethodDeclarations(compilationUnit, semanticModel);
            foreach (MethodDeclarationSyntax testMethodDeclaration in testMethodDeclarations)
            {
                string testMethodName = testMethodDeclaration.Identifier.ValueText;
                int underscoreIndex = testMethodName.IndexOf('_');
                // If no underscore or it is the first character, name is incorrectly formatted
                if (underscoreIndex > 1)
                {
                    string expectedMemberName = testMethodName.Substring(0, underscoreIndex);
                    if (classUnderTestMembers.Any(m => m.Name == expectedMemberName))
                    {
                        continue;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, testMethodDeclaration.Identifier.GetLocation()));
            }

        }
    }
}