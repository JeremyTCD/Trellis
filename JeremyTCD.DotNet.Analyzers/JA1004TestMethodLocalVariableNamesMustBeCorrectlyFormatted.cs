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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Look for local variables of type Mock<T>
            IEnumerable<LocalDeclarationStatementSyntax> localDeclarations = compilationUnit.
                DescendantNodes().
                OfType<LocalDeclarationStatementSyntax>();
            if (localDeclarations.Count() == 0)
            {
                return;
            }

            ClassDeclarationSyntax testClassDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            if (testClassDeclaration == null)
            {
                return;
            }

            ISymbol classUnderTest = TestingHelper.GetClassUnderTest(testClassDeclaration, context.Compilation.GlobalNamespace);
            INamedTypeSymbol mockGenericType = context.Compilation.GetTypeByMetadataName("Moq.Mock`1");
            foreach (LocalDeclarationStatementSyntax localDeclaration in localDeclarations)
            {
                SyntaxToken variableToken = localDeclaration.Declaration.Variables.First().Identifier;
                string variableName = variableToken.ValueText;
                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                ITypeSymbol variableType = context.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type).Type;

                if (variableType == classUnderTest)
                {
                    if (variableName != "testSubject")
                    {
                        builder.Add(CorrectVariableNameProperty, "testSubject");
                    }
                    else
                    {
                        continue; // Must be "testSubject"
                    }
                }
                else if (variableType.OriginalDefinition == mockGenericType) // Variable is of type Mock<T>
                {
                    // Variable is mock of type under test
                    if ((variableType as INamedTypeSymbol).TypeArguments.First() == classUnderTest)
                    {
                        if (variableName != "testSubject")
                        {
                            builder.Add(CorrectVariableNameProperty, "testSubject");
                        }
                        else
                        {
                            continue; // Must be "testSubject"
                        }
                    }
                    else
                    {
                        // Variable has its behaviour verified
                        if (localDeclaration.FirstAncestorOrSelf<MethodDeclarationSyntax>().
                            DescendantNodes().
                            OfType<MemberAccessExpressionSyntax>().
                            Any(m => (m.Expression as IdentifierNameSyntax)?.Identifier.ValueText == variableName && m.Name.Identifier.ValueText == "Setup"))
                        {
                            if (!variableName.StartsWith("mock"))
                            {
                                builder.Add(
                                    CorrectVariableNameProperty, 
                                    variableName.StartsWith("dummy") ? variableName.Replace("dummy", "mock") : $"mock{variableName.FirstCharUpper()}");
                            }
                            else
                            {
                                continue; // Must start with "mock"
                            }
                        }
                        else if (!variableName.StartsWith("dummy"))
                        {
                            builder.Add(CorrectVariableNameProperty, 
                                variableName.StartsWith("mock") ? variableName.Replace("mock", "dummy") : $"dummy{variableName.FirstCharUpper()}");
                        }
                        else
                        {
                            continue; // Must start with "dummy"
                        }
                    }
                }
                else // TODO Not class under test or a mock
                {
                    // Must not be "testSubject"
                    // Must not start with "mock"
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableToken.GetLocation(), builder.ToImmutable(), variableName));
            }
        }
    }
}