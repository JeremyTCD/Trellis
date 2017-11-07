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
            FactoryInterfaceContext factoryInterfaceContext = FactoryInterfaceContextFactory.TryCreate(context);
            if (factoryInterfaceContext == null || factoryInterfaceContext.ProducedInterface == null)
            {
                return;
            }

            // Check if any create methods exist
            if (factoryInterfaceContext.CreateMethods.Count() > 0)
            {
                return;
            }

            // Add diagnostic
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, factoryInterfaceContext.InterfaceDeclaration.Identifier.GetLocation()));
        }
    }
}