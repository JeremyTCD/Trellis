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
    public class JA1005NewMustNotBeUsedToInstantiateClassUnderTestInTestMethods : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1005NewMustNotBeUsedToInstantiateClassUnderTestInTestMethods"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1005";
        public const string ClassUnderTestFullyQualifiedNameProperty = "ClassUnderTestFullyQualifiedName";
        public const string CreateMethodNameProperty = "CreateMethodName";

        private const string Title = "New must not be used to instantiate class under test in test methods.";
        private const string MessageFormat = "New must not be used to instantiate class under test in test methods, use a \"Create{0}\" method that returns \"{0}\" instead.";
        private const string Description = "A test method uses new to instantiate class under test.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

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
            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == "Xunit"))
            {
                return;
            }

            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            string className = classDeclaration.Identifier.ToString();
            string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            if (classUnderTestName == className)
            {
                return;
            }

            ITypeSymbol classUnderTestSymbol = TestingHelper.GetClassUnderTest(classDeclaration, context.Compilation.GlobalNamespace);
            if (classUnderTestSymbol == null)
            {
                return;
            }

            IEnumerable<ObjectCreationExpressionSyntax> classUnderTestCreationExpressions = compilationUnit.
                DescendantNodes().
                OfType<ObjectCreationExpressionSyntax>().
                Where(o => context.SemanticModel.GetTypeInfo(o).Type == classUnderTestSymbol);
            if(classUnderTestCreationExpressions.Count() == 0)
            {
                return;
            }

            ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder.Add(ClassUnderTestFullyQualifiedNameProperty, classUnderTestSymbol.ToDisplayString());
            builder.Add(CreateMethodNameProperty, $"Create{classUnderTestName}");
            ImmutableDictionary<string, string> properties = builder.ToImmutable();

            foreach (ObjectCreationExpressionSyntax creationExpression in classUnderTestCreationExpressions)
            {
                IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(creationExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>());
                if(!methodSymbol.GetAttributes().Any(a => a.ToString() == "Xunit.FactAttribute" || a.ToString() == "Xunit.TheoryAttribute"))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, creationExpression.GetLocation(), properties, classUnderTestName));
            }
        }

    }
}