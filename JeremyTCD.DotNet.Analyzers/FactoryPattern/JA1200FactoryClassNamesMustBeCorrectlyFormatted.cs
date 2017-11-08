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
    public class JA1200FactoryClassNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1200FactoryClassNamesMustBeCorrectlyFormatted).Substring(0, 6);
        public const string ExpectedClassNameProperty = "ExpectedClassNameProperty";

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

            // Check if all create methods return the same type
            ITypeSymbol producedType = null;
            foreach (MethodDeclarationSyntax methodDeclaration in factoryClassContext.CreateMethodDeclarations)
            {
                // Get return statement
                IEnumerable<ReturnStatementSyntax> returnStatements = methodDeclaration.DescendantNodes().OfType<ReturnStatementSyntax>();
                if (returnStatements.Count() == 0)
                {
                    continue;
                }

                foreach (ReturnStatementSyntax returnStatement in returnStatements)
                {
                    // Get return type (note this differs from declared return type since we are returning a concrete implementation)
                    ITypeSymbol returnType = factoryClassContext.SemanticModel.GetTypeInfo(returnStatement.Expression).Type;
                    if (returnType == null)
                    {
                        continue; // Incomplete create method
                    }
                    else if (returnType.TypeKind == TypeKind.Interface)
                    {
                        return; // Assume that factory produces multiple concrete types
                    }
                    else if (producedType == null)
                    {
                        producedType = returnType;
                    }
                    else if (returnType != producedType)
                    {
                        return; // Factory produces multiple concrete types. Factory name cannot be deduced from available information.
                    }
                }
            }
            if (producedType == null)
            {
                return;
            }

            // Factory class name is valid
            string expectedClassName = null;
            if (producedType != null)
            {
                expectedClassName = $"{producedType?.Name}Factory";
                if (factoryClassContext.ClassName.Equals(expectedClassName))
                {
                    return;
                }

                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder.Add(ExpectedClassNameProperty, expectedClassName);
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, factoryClassContext.ClassDeclaration.Identifier.GetLocation(), builder.ToImmutable()));
            }

        }
    }
}