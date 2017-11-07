// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1200FactoryClassNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1200FactoryClassNamesMustBeCorrectlyFormatted).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1200_Title,
                Strings.JA1200_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1200_Description,
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
            FactoryClassContext factoryClassContext = FactoryClassContextFactory.TryCreate(context);
            // Impossible to verify validity of factory class name
            if (factoryClassContext == null || 
                factoryClassContext.FactoryInterfaceContext == null ||
                factoryClassContext.FactoryInterfaceContext.ProducedInterface == null)
            {
                return;
            }

            // Factory class name is valid
            if(factoryClassContext.ProducedClass != null && 
               factoryClassContext.ProducedClass.AllInterfaces.Contains(factoryClassContext.FactoryInterfaceContext.ProducedInterface))
            {
                return;
            }

            // Produced class does not exist or produced class does not implement factory interface's produced interface
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, factoryClassContext.ClassDeclaration.Identifier.GetLocation()));
        }
    }
}