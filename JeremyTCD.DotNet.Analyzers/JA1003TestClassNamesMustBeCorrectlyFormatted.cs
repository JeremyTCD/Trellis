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
    public class JA1003TestClassNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1003TestClassNamesMustBeCorrectlyFormatted"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1003";

        private const string Title = "Test class names must be correctly formatted.";
        private const string NonExistentClassMessage = "Test class name must begin with class under test; \"{0}\" is not an existing class' name.";
        private const string InvalidSuffixMessage = "Test class name must end with \"UnitTests\", \"IntegrationTests\" or \"EndToEndTests\".";
        private const string Description = "A test class's name is incorrectly formatted.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor InvalidSuffixDescriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, InvalidSuffixMessage, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);
        
        private static readonly DiagnosticDescriptor NonExistentClassDescriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, NonExistentClassMessage, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(InvalidSuffixDescriptor, NonExistentClassDescriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(HandleVariableDeclaration, SyntaxKind.CompilationUnit);
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

            // Invalid suffix
            if (!className.EndsWith("UnitTests") && !className.EndsWith("IntegrationTests") && !className.EndsWith("EndToEndTests"))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidSuffixDescriptor, classDeclaration.Identifier.GetLocation()));
                return;
            }

            // Non existent class 
            string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            if (!Exists(classUnderTestName, context.Compilation.GlobalNamespace))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidSuffixDescriptor, classDeclaration.Identifier.GetLocation(), classUnderTestName));
            }
        }

        public bool Exists(string classUnderTestName, INamespaceOrTypeSymbol symbol)
        {
            if (symbol is ITypeSymbol)
            {
                if (symbol.Name.EndsWith(classUnderTestName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            IEnumerable<INamespaceOrTypeSymbol> symbols = (symbol as INamespaceSymbol).GetMembers();

            foreach (INamespaceOrTypeSymbol child in symbols)
            {
                if (Exists(classUnderTestName, child))
                {
                    return true;
                }
            }

            return false;
        }
    }
}