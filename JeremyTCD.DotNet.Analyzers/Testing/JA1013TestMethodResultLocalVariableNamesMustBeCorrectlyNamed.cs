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
    public class JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed).Substring(0, 6);
        public const string CorrectVariableNameProperty = "CorrectVariableNameProperty";

        private static readonly DiagnosticDescriptor Descriptor =
                    new DiagnosticDescriptor(DiagnosticId,
                        Strings.JA1013_Title,
                        Strings.JA1013_MessageFormat,
                        Strings.CategoryName_Testing,
                        DiagnosticSeverity.Warning,
                        true,
                        Strings.JA1013_Description,
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
            if (testClassContext == null || testClassContext.ClassUnderTest == null)
            {
                return;
            }

            // Find test subject variable names
            // Find variables that are instantiated using invocations on test subjects
            foreach (MethodDeclarationSyntax methodDeclaration in testClassContext.TestMethodDeclarations)
            {
                List<LocalDeclarationStatementSyntax> testSubjectDeclarations = TestingHelper.GetTestSubjectDeclarations(testClassContext, methodDeclaration);
                if(testSubjectDeclarations.Count == 0)
                {
                    continue;
                }

                IEnumerable<LocalDeclarationStatementSyntax> resultDeclarations = TestingHelper.
                    GetResultVariableDeclarations(methodDeclaration, testClassContext, testSubjectDeclarations);

                if (resultDeclarations.Count() == 1)
                {
                    VariableDeclaratorSyntax variableDeclarator = resultDeclarations.First().Declaration.Variables.First();

                    if (variableDeclarator.Identifier.ValueText != "result")
                    {
                        ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder.Add(CorrectVariableNameProperty, "result");
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            variableDeclarator.Identifier.GetLocation(),
                            builder.ToImmutable()));
                    }

                    continue;
                }

                foreach (LocalDeclarationStatementSyntax resultDeclaration in resultDeclarations)
                {
                    VariableDeclaratorSyntax variableDeclarator = resultDeclaration.Declaration.Variables.First();

                    if (!variableDeclarator.Identifier.ValueText.EndsWith("Result"))
                    {
                        ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder.Add(CorrectVariableNameProperty, $"{variableDeclarator.Identifier.ValueText}Result");
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            variableDeclarator.Identifier.GetLocation(),
                            builder.ToImmutable()));
                    }
                }
            }
        }
    }
}