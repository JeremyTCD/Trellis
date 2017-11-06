// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1006TestDataMethodNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1006TestDataMethodNamesMustBeCorrectlyFormatted).Substring(0, 6);
        public const string DataMethodNameProperty = "DataMethodNameProperty";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1006_Title,
                Strings.JA1006_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1006_Description,
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
            TestClassContext testClassContext = TestClassContextFactory.TryCreate(context);
            if (testClassContext == null)
            {
                return;
            }

            foreach (KeyValuePair<MethodDeclarationSyntax, MethodDeclarationSyntax> kvp in 
                testClassContext.TestMethodDeclarationsAndTheirDataMethodDeclarations)
            {
                string testMethodName = kvp.Key.Identifier.ValueText;
                string dataMethodName = kvp.Value.Identifier.ValueText;
                string expectedDataMethodName = $"{testMethodName}_Data";

                if (dataMethodName.Equals(expectedDataMethodName))
                {
                    continue;
                }

                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder.Add(DataMethodNameProperty, expectedDataMethodName);
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, kvp.Value.Identifier.GetLocation(), builder.ToImmutable()));
            }
        }
    }
}