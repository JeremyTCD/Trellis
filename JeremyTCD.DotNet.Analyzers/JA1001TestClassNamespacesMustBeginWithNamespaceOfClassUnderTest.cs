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
    public class JA1001TestClassNamespacesMustBeginWithNamespaceOfClassUnderTest : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1001TestClassNamespacesMustBeginWithNamespaceOfClassUnderTest"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1001";

        private const string Title = "Test class namespaces must begin with namespace of class under test.";
        private const string MessageFormat = "Test class namespace does not begin with namespace of class under test.";
        private const string Description = "A test class' namespace does not begin with namespace of class under test.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(HandleVariableDeclaration, SyntaxKind.CompilationUnit);
        }

        private void HandleVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;

            // Return if not in a test class
            if(!compilationUnit.Usings.Any(u => u.Name.ToString() == "Xunit"))
            {
                return;
            }

            // Add diagnostic
            NamespaceDeclarationSyntax namespaceDeclaration = compilationUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            string className = classDeclaration.Identifier.ToString();
            string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            IEnumerable<ISymbol> symbols = context.SemanticModel.LookupSymbols(0, name: classUnderTestName);

            if(!symbols.Any(s => namespaceDeclaration.Name.ToString().StartsWith(s.ContainingNamespace.ToDisplayString())))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, namespaceDeclaration.Name.GetLocation()));
            }
        }
    }
}