// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests).Substring(0, 6);
        public const string FixDataProperty = "FixDataProperty";
        public const string TestClassFullyQualifiedNameProperty = "TestClassFullyQualifiedNameProperty";
        public const string ClassUnderTestNameProperty = "ClassUnderTestNameProperty";
        public const string ClassUnderTestFullyQualifiedNameProperty = "ClassUnderTestFullyQualifiedNameProperty";

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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;
            SemanticModel semanticModel = context.SemanticModel;

            // Return if document contains test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Get unit test class
            ClassDeclarationSyntax testClassDeclaration = compilationUnit.
                DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault() as ClassDeclarationSyntax;
            if (testClassDeclaration == null)
            {
                return;
            }

            // Return if not unit test class
            if (!TestingHelper.IsUnitTestClass(testClassDeclaration))
            {
                return;
            }

            // Get class under test
            ITypeSymbol classUnderTest = TestingHelper.
                GetClassUnderTest(testClassDeclaration, semanticModel.Compilation.GlobalNamespace);
            if (classUnderTest == null)
            {
                return;
            }

            // Get unit test class
            ITypeSymbol unitTestClass = semanticModel.GetDeclaredSymbol(testClassDeclaration) as ITypeSymbol;
            if (unitTestClass == null)
            {
                return;
            }

            // Get test methods
            IEnumerable<IMethodSymbol> testMethods = TestingHelper.
                GetMethodMembers(unitTestClass).
                Where(m => TestingHelper.IsTestMethod(m));

            // Get test method declarations
            IEnumerable<MethodDeclarationSyntax> testMethodDeclarations = testMethods.
                Select(m => m.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax).
                Where(m => m != null);

            // Create map of methods that test methods test
            Dictionary<MethodDeclarationSyntax, IMethodSymbol> methodsThatTestMethodsTest = new Dictionary<MethodDeclarationSyntax, IMethodSymbol>();
            foreach(MethodDeclarationSyntax testMethodDeclaration in testMethodDeclarations)
            {
                methodsThatTestMethodsTest.Add(testMethodDeclaration, TestingHelper.GetMethodUnderTest(testMethodDeclaration, classUnderTest, semanticModel));
            }

            // Create dictionary for speeding up second pass
            Dictionary<XmlElementSyntax, Tuple<string, string>> exceptionElementValues = new Dictionary<XmlElementSyntax, Tuple<string, string>>();

            // Iterate over methods in main class
            string fixData = string.Empty;
            foreach (IMethodSymbol methodUnderTest in TestingHelper.GetMethodMembers(classUnderTest))
            {
                MethodDeclarationSyntax methodUnderTestDeclaration = methodUnderTest.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                if (methodUnderTestDeclaration == null)
                {
                    continue;
                }

                // Get exception elements that are not empty
                List<XmlElementSyntax> exceptionElements = TestingHelper.GetMethodExceptionXmlElements(methodUnderTestDeclaration, methodUnderTest);

                // Find all tests for the method under test
                List<MethodDeclarationSyntax> methodUnderTestTestMethods = testMethodDeclarations.
                    Where(m => methodsThatTestMethodsTest[m] == methodUnderTest).ToList();

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
                    string testMethodName = $"{methodUnderTest.Name}_Throws{exceptionName}{exceptionDescription}";

                    // Exception outcome has test method
                    MethodDeclarationSyntax exceptionOutcomeTestMethodDeclaration = methodUnderTestTestMethods.
                        FirstOrDefault(m => m.Identifier.ValueText == testMethodName);
                    if (exceptionOutcomeTestMethodDeclaration != null)
                    {
                        exceptionElements.RemoveAt(i);
                        methodUnderTestTestMethods.Remove(exceptionOutcomeTestMethodDeclaration);
                        continue;
                    }

                    exceptionElementValues.Add(exceptionElement, new Tuple<string, string>(exceptionName, testMethodName));
                }

                // Add diagnostics
                foreach (XmlElementSyntax exceptionElement in exceptionElements)
                {
                    string exceptionName = exceptionElementValues[exceptionElement].Item1;
                    string correctTestMethodName = exceptionElementValues[exceptionElement].Item2;
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

                    fixData += correctTestMethodName;
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
                builder.Add(TestClassFullyQualifiedNameProperty, unitTestClass.ToDisplayString());
                builder.Add(ClassUnderTestNameProperty, classUnderTest.Name.ToString());
                builder.Add(ClassUnderTestFullyQualifiedNameProperty, classUnderTest.ToDisplayString());
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, testClassDeclaration.Identifier.GetLocation(), builder.ToImmutable()));
            }
        }
    }
}