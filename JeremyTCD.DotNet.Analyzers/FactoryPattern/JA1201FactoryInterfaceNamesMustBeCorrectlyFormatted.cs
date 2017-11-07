// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1201_Title,
                Strings.JA1201_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1201_Description,
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
            if (factoryInterfaceContext == null || factoryInterfaceContext.ProducedInterface != null)
            {
                return;
            }

            // Add diagnostic
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, 
                factoryInterfaceContext.InterfaceDeclaration.Identifier.GetLocation()));
        }
    }
}