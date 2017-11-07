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
    public class JA1204FactoryClassCreateMethodsMustReturnProducedType : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1204FactoryClassCreateMethodsMustReturnProducedType).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1204_Title,
                Strings.JA1204_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1204_Description,
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
            if (factoryClassContext == null || factoryClassContext.FactoryInterfaceContext == null)
            {
                return;
            }

            foreach (MethodDeclarationSyntax methodDeclaration in factoryClassContext.CreateMethodDeclarations)
            {
                // Get return statement
                ReturnStatementSyntax returnStatement = methodDeclaration.Body.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
                if (returnStatement == null)
                {
                    continue;
                }

                // Get return type (note this differs from declared return type since we are returning a concrete implementation)
                ITypeSymbol returnType = factoryClassContext.SemanticModel.GetTypeInfo(returnStatement.Expression).Type;
                if (returnType == null)
                {
                    continue;
                }

                // Compare with produced type
                if (returnType != factoryClassContext.ProducedClass)
                {
                    // Add diagnostic
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.Identifier.GetLocation()));
                }
            }
        }
    }
}