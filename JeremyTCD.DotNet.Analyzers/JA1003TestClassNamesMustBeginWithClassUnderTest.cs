// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1003TestClassNamesMustBeginWithClassUnderTest : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1003TestClassNamesMustBeginWithClassUnderTest"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1003";

        private const string Title = "Test class names must be correctly formatted.";
        private const string MessageFormat = "Test class name must begin with class under test; {0} is not the name of a testable class.";
        private const string Description = "A test class's name is incorrectly formatted.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(HandleVariableDeclaration, SyntaxKind.CompilationUnit);
            context.EnableConcurrentExecution();
        }

        private void HandleVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;

            // Return if not in a test class
            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == "Xunit"))
            {
                return;
            }

            // Add diagnostics          
            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            string className = classDeclaration.Identifier.ToString();
            string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            if (classUnderTestName == className || SymbolHelper.TryGetTypeSymbol(classUnderTestName, context.Compilation.GlobalNamespace) == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation(), classUnderTestName));
            }
        }
    }
}