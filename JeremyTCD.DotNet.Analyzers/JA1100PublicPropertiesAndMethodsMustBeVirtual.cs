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
    public class JA1100PublicPropertiesAndMethodsMustBeVirtual : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1100PublicPropertiesAndMethodsMustBeVirtual).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1100_Title,
                Strings.JA1100_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1100_Description,
                "");

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(HandleMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandlePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.EnableConcurrentExecution();
        }

        private void HandlePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            PropertyDeclarationSyntax propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            Handle(context, propertyDeclaration.Modifiers, propertyDeclaration.Identifier, propertyDeclaration.ExplicitInterfaceSpecifier != null);
        }

        private void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;
            Handle(context, methodDeclaration.Modifiers, methodDeclaration.Identifier, methodDeclaration.ExplicitInterfaceSpecifier != null);
        }

        private void Handle(SyntaxNodeAnalysisContext context, SyntaxTokenList modifiers, SyntaxToken identifier, bool hasExplicitInterfaceSpecifier)
        {
            if (hasExplicitInterfaceSpecifier)
            {
                return;
            }

            if (context.Node.Parent is InterfaceDeclarationSyntax)
            {
                return;
            }

            // Return if file is a designer.cs file generated from a resx file
            if (GeneratedCodeHelper.IsDesigner(context.Node.SyntaxTree.FilePath))
            {
                return;
            }

            if (TestingHelper.ContainsTestClass(context.Node.FirstAncestorOrSelf<CompilationUnitSyntax>()))
            {
                return;
            }

            if(!modifiers.Any(s => s.ValueText == "public"))
            {
                return;
            }

            if(modifiers.Any(s => s.ValueText == "virtual" || s.ValueText == "abstract" || s.ValueText == "static" || s.ValueText == "override"))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation()));
        }
    }
}