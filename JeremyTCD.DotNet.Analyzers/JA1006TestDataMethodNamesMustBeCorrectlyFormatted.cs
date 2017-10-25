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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;
            SemanticModel semanticModel = context.SemanticModel;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Get test methods
            IEnumerable<MethodDeclarationSyntax> testMethodDeclarations = TestingHelper.GetTestMethodDeclarations(compilationUnit, semanticModel);
            foreach (MethodDeclarationSyntax testMethodDeclaration in testMethodDeclarations)
            {
                // Skip if test method does not have a MemberDataAttribute
                // TODO assumes that methods have at most one MemberDataAttribute
                AttributeSyntax memberDataAttribute = testMethodDeclaration.
                    DescendantNodes().
                    OfType<AttributeSyntax>().
                    Where(a => semanticModel.GetSymbolInfo(a).Symbol.ToDisplayString() == "Xunit.MemberDataAttribute.MemberDataAttribute(string, params object[])").
                    FirstOrDefault();
                if(memberDataAttribute == null)
                {
                    continue;
                }

                // Skip if MemberDataAttribute takes something other than nameof(<DataMethod>) as its first argument
                InvocationExpressionSyntax memberDataAttributeFirstArgumentExpression = memberDataAttribute.
                    ArgumentList.
                    Arguments.
                    FirstOrDefault()?.Expression as InvocationExpressionSyntax;
                // TODO is there a more robust way to ensure that expression is an invocation of the built in function nameof?
                if(memberDataAttributeFirstArgumentExpression == null || memberDataAttributeFirstArgumentExpression.Expression.ToString() != "nameof")
                {
                    continue;
                }

                // Get name of method passed to nameof
                IdentifierNameSyntax dataMethodIdentifier = memberDataAttributeFirstArgumentExpression.
                    ArgumentList.
                    Arguments.
                    FirstOrDefault()?.Expression as IdentifierNameSyntax;

                // Skip if data method name is valid
                string dataMethodName = dataMethodIdentifier.ToString();
                string testMethodName = testMethodDeclaration.Identifier.ValueText;
                if (dataMethodName.EndsWith("_Data") && dataMethodName.Replace("_Data", "") == testMethodName)
                {
                    continue; 
                }

                // Get MethodDeclaration for data method
                MethodDeclarationSyntax dataMethodDeclaration = compilationUnit.
                    DescendantNodes().
                    OfType<MethodDeclarationSyntax>().
                    Where(m => m.Identifier.ValueText == dataMethodName).
                    FirstOrDefault();
                if(dataMethodDeclaration == null)
                {
                    continue;
                }

                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder.Add(DataMethodNameProperty, $"{testMethodName}_Data");
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, dataMethodDeclaration.Identifier.GetLocation(), builder.ToImmutable()));
            }

        }
    }
}