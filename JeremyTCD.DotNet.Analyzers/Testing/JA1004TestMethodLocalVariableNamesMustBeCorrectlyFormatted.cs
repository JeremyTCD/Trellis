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
    public class JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted).Substring(0, 6);
        public const string CorrectVariableNameProperty = "CorrectVariableNameProperty";

        private static readonly DiagnosticDescriptor Descriptor =
                    new DiagnosticDescriptor(DiagnosticId,
                        Strings.JA1004_Title,
                        Strings.JA1004_MessageFormat,
                        Strings.CategoryName_Testing,
                        DiagnosticSeverity.Warning,
                        true,
                        Strings.JA1004_Description,
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

            INamedTypeSymbol mockGenericType = context.Compilation.GetTypeByMetadataName("Moq.Mock`1");
            foreach (MethodDeclarationSyntax methodDeclaration in testClassContext.
                TestMethodDeclarations)
            {
                List<LocalDeclarationStatementSyntax> testSubjectDeclarations = TestingHelper.GetTestSubjectDeclarations(testClassContext, methodDeclaration);

                foreach (LocalDeclarationStatementSyntax localDeclaration in methodDeclaration.
                    DescendantNodes().
                    OfType<LocalDeclarationStatementSyntax>().
                    Except(testSubjectDeclarations))
                {
                    SyntaxToken variableToken = localDeclaration.Declaration.Variables.First().Identifier;
                    string variableName = variableToken.ValueText;
                    ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                    ITypeSymbol variableType = context.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type).Type;

                    if (variableType.OriginalDefinition == mockGenericType) // Variable is of type Mock<T>
                    {
                        // Variable has its behaviour verified
                        if (methodDeclaration.
                            DescendantNodes().
                            OfType<MemberAccessExpressionSyntax>().
                            Any(m => (m.Expression as IdentifierNameSyntax)?.Identifier.ValueText == variableName && m.Name.Identifier.ValueText == "Setup"))
                        {
                            if (variableName.StartsWith("mock"))
                            {
                                continue; // Correctly starts with "mock"
                            }
                            builder.Add(
                                CorrectVariableNameProperty,
                                variableName.StartsWith("dummy") ? variableName.Replace("dummy", "mock") : $"mock{variableName.FirstCharUpper()}");
                        }
                        else if (variableName.StartsWith("dummy"))
                        {
                            continue; // Correctly starts with "dummy"
                        }
                        else
                        {
                            builder.Add(CorrectVariableNameProperty,
                                variableName.StartsWith("mock") ? variableName.Replace("mock", "dummy") : $"dummy{variableName.FirstCharUpper()}");
                        }

                    }
                    else
                    {
                        // TODO must not start with mock
                        continue;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableToken.GetLocation(), builder.ToImmutable(), variableName));
                }
            }
        }
    }
}