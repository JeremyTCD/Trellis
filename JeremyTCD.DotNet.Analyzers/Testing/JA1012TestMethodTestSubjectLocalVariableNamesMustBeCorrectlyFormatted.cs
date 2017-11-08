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
    public class JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted).Substring(0, 6);
        public const string CorrectVariableNameProperty = "CorrectVariableNameProperty";

        private static readonly DiagnosticDescriptor Descriptor =
                    new DiagnosticDescriptor(DiagnosticId,
                        Strings.JA1012_Title,
                        Strings.JA1012_MessageFormat,
                        Strings.CategoryName_Testing,
                        DiagnosticSeverity.Warning,
                        true,
                        Strings.JA1012_Description,
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

            foreach (MethodDeclarationSyntax methodDeclaration in testClassContext.TestMethodDeclarations)
            {
                List<LocalDeclarationStatementSyntax> testSubjectDeclarations = TestingHelper.GetTestSubjectDeclarations(testClassContext, methodDeclaration);

                if (testSubjectDeclarations.Count() == 1)
                {
                    VariableDeclaratorSyntax variableDeclarator = testSubjectDeclarations.First().Declaration.Variables.First();

                    if (variableDeclarator.Identifier.ValueText != "testSubject")
                    {
                        ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder.Add(CorrectVariableNameProperty, "testSubject");
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            variableDeclarator.Identifier.GetLocation(),
                            builder.ToImmutable()));
                    }

                    continue;
                }

                foreach (LocalDeclarationStatementSyntax localDeclaration in TestingHelper.GetTestSubjectDeclarations(testClassContext, methodDeclaration))
                {
                    VariableDeclaratorSyntax variableDeclarator = localDeclaration.Declaration.Variables.First();

                    if (!variableDeclarator.Identifier.ValueText.EndsWith("TestSubject"))
                    {
                        ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder.Add(CorrectVariableNameProperty, $"{variableDeclarator.Identifier.ValueText}TestSubject");
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