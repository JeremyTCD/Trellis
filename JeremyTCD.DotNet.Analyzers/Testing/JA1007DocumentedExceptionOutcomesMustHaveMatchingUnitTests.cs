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
    public class JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests).Substring(0, 6);
        public const string FixDataProperty = "FixDataProperty";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1007_Title,
                Strings.JA1007_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1007_Description,
                "");

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.CompilationUnit);
            context.EnableConcurrentExecution();
        }

        // TODO unit test project has reference to main project but not vice versa, therefore unit test class always null
        private void Handle(SyntaxNodeAnalysisContext context)
        {
            TestClassContext testClassContext = TestClassContextFactory.TryCreate(context);
            if (testClassContext == null || testClassContext.ClassUnderTest == null)
            {
                return;
            }

            // Return if not unit test class
            if (!testClassContext.IsUnitTestClass)
            {
                return;
            }

            // Iterate over methods in main class
            string fixData = string.Empty;
            foreach (IMethodSymbol methodUnderTest in testClassContext.ClassUnderTestMethods)
            {
                MethodDeclarationSyntax methodUnderTestDeclaration = methodUnderTest.
                    DeclaringSyntaxReferences.
                    FirstOrDefault()?.
                    GetSyntax() as MethodDeclarationSyntax;
                if (methodUnderTestDeclaration == null)
                {
                    continue;
                }

                // Get exception elements that are not empty
                List<XmlElementSyntax> exceptionElements = TestingHelper.GetMethodExceptionXmlElements(methodUnderTestDeclaration, methodUnderTest);
                if(exceptionElements.Count() == 0)
                {
                    continue;
                }

                // Find all tests for the method under test
                List<MethodDeclarationSyntax> methodUnderTestTestMethods = testClassContext.GetMethodUnderTestTestMethodDeclarations(methodUnderTest);

                // Create dictionary for speeding up second pass
                Dictionary<XmlElementSyntax, (string, string)> exceptionElementValues = new Dictionary<XmlElementSyntax, (string, string)>();

                // Filter out exception elements that do not have cref attributes or that have valid test methods
                int numExceptionElements = exceptionElements.Count();
                for (int i = numExceptionElements - 1; i > -1; i--)
                {
                    XmlElementSyntax exceptionElement = exceptionElements[i];
                    XmlCrefAttributeSyntax xmlCrefAttribute = TestingHelper.GetXmlElementCrefAttribute(exceptionElement);
                    if (xmlCrefAttribute == null)
                    {
                        exceptionElements.RemoveAt(i);
                        continue;
                    }

                    string exceptionName = xmlCrefAttribute.Cref.ToString();
                    string exceptionDescription = DocumentationHelper.GetNodeContentsAsNormalizedString(exceptionElement).
                        RemoveNonAlphaNumericCharacters().
                        ToTitleCase().
                        Replace("Thrown", "");
                    string expectedTestMethodName = $"{methodUnderTest.Name}_Throws{exceptionName}{exceptionDescription}";

                    // Exception outcome has test method
                    MethodDeclarationSyntax exceptionOutcomeTestMethodDeclaration = methodUnderTestTestMethods.
                        FirstOrDefault(m => m.Identifier.ValueText == expectedTestMethodName);
                    if (exceptionOutcomeTestMethodDeclaration != null)
                    {
                        exceptionElements.RemoveAt(i);
                        methodUnderTestTestMethods.Remove(exceptionOutcomeTestMethodDeclaration);
                        continue;
                    }

                    exceptionElementValues.Add(exceptionElement, (exceptionName, expectedTestMethodName));
                }

                // Add diagnostics
                foreach (XmlElementSyntax exceptionElement in exceptionElements)
                {
                    string exceptionName = exceptionElementValues[exceptionElement].Item1;
                    string expectedTestMethodName = exceptionElementValues[exceptionElement].Item2;
                    fixData += ";";

                    // Check if there are any remaining method under test test methods that call Assert.Throws<Exception>
                    IEnumerable<MethodDeclarationSyntax> IncorrectlyNamedTestMethods = methodUnderTestTestMethods.
                    Where(m => m.
                        DescendantNodes().
                        OfType<MemberAccessExpressionSyntax>().
                        Any(ma => ma.ToString() == $"Assert.Throws<{exceptionName}>"));
                    if (IncorrectlyNamedTestMethods.Count() == 1)
                    {
                        // TODO if multiple exception elements point to the same "Incorrectly" named method, code fix results will be invalid
                        fixData += IncorrectlyNamedTestMethods.First().Identifier.ValueText;
                        fixData += ',';
                    }

                    fixData += expectedTestMethodName;
                    fixData += ',';
                    fixData += exceptionName;
                    fixData += ',';
                    fixData += methodUnderTest.Name;
                }

            }

            if (!string.IsNullOrWhiteSpace(fixData))
            {
                ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                builder.Add(FixDataProperty, fixData);
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, testClassContext.ClassDeclaration.Identifier.GetLocation(), builder.ToImmutable()));
            }
        }
    }
}