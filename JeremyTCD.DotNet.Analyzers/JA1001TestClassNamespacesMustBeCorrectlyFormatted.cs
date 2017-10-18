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
    public class JA1001TestClassNamespacesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1001CodeFixProvider).Substring(0, 6);
        public const string CorrectNamespaceProperty = "CorrectNamespaceProperty";

        private const string Title = "Test class namespaces must be correctly formatted.";
        private const string MessageFormat = "Test class namespace is not in the format \"<NamespaceOfClassUnderTest>.Tests\".";
        private const string Description = "A test class's namespace is incorrectly formatted.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1001_Title,
                Strings.JA1001_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1001_Description,
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

            // Add diagnostic
            NamespaceDeclarationSyntax namespaceDeclaration = compilationUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
            ClassDeclarationSyntax classDeclaration = namespaceDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            string className = classDeclaration.Identifier.ValueText;
            string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
            ITypeSymbol classUnderTestSymbol;
            string correctNamespace;

            if (classUnderTestName == className ||
                (classUnderTestSymbol = SymbolHelper.TryGetTypeSymbol(classUnderTestName, context.Compilation.GlobalNamespace)) == null)
            {
                builder.Add(Constants.NoCodeFix, null);
            }
            else if ((correctNamespace = $"{classUnderTestSymbol.ContainingNamespace.ToDisplayString()}.Tests") != namespaceDeclaration.Name.ToString())
            {
                builder.Add(CorrectNamespaceProperty, correctNamespace);
            }
            else
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, namespaceDeclaration.Name.GetLocation(), builder.ToImmutable()));
        }
    }
}