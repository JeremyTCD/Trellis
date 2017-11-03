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
    public class JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1202_Title,
                Strings.JA1202_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1202_Description,
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

            // Get class declaration
            InterfaceDeclarationSyntax interfaceDeclaration = compilationUnit.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
            if (interfaceDeclaration == null)
            {
                return;
            }

            // Return if not a factory interface
            if (!FactoryHelper.IsFactoryType(interfaceDeclaration))
            {
                return;
            }

            // Get type that factory creates
            ITypeSymbol producedType = FactoryHelper.GetProducedType(interfaceDeclaration, context.Compilation.GlobalNamespace);
            if (producedType == null || producedType.TypeKind != TypeKind.Interface)
            {
                return;
            }

            // Get all methods named Create
            IEnumerable<MethodDeclarationSyntax> interfaceMethodDeclarations = interfaceDeclaration.
                DescendantNodes().
                OfType<MethodDeclarationSyntax>().
                Where(m => m.Identifier.ValueText == "Create");

            // Check if any return the produced type
            if (interfaceMethodDeclarations.Count() > 0 &&
                semanticModel.GetSymbolInfo(interfaceMethodDeclarations.ToArray()[0].ReturnType).Symbol == producedType)
            {
                return;
            }

            // Add diagnostic
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, interfaceDeclaration.Identifier.GetLocation()));
        }
    }
}