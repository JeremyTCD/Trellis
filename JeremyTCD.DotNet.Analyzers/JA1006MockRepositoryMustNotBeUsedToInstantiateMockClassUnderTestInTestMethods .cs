// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1006MockRepositoryMustNotBeUsedToInstantiateMockClassUnderTestInTestMethods : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1006MockRepositoryMustNotBeUsedToInstantiateMockClassUnderTestInTestMethods"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1006";
        public const string ClassUnderTestFullyQualifiedNameProperty = "ClassUnderTestFullyQualifiedName";
        public const string CreateMethodNameProperty = "CreateMethodName";

        private const string Title = "MockRepository must not be used to instantiate mock class under test in test methods.";
        private const string MessageFormat = "Use a \"Create{0}\" method that returns \"{0}\".";
        private const string Description = "A test method uses MockRepository to instantiate mock class under test.";
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
            //CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;

            //// Return if not in a test class
            //if (!compilationUnit.Usings.Any(u => u.Name.ToString() == "Xunit"))
            //{
            //    return;
            //}

            //ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            //string className = classDeclaration.Identifier.ToString();
            //string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            //if (classUnderTestName == className)
            //{
            //    return;
            //}

            //ITypeSymbol classUnderTestSymbol = SymbolHelper.TryGetSymbol(classUnderTestName, context.Compilation.GlobalNamespace);
            //if (classUnderTestSymbol == null)
            //{
            //    return;
            //}

            //string createMethodName = $"Create{classUnderTestName}";
            //MethodDeclarationSyntax createMethod = classDeclaration.
            //    DescendantNodes().
            //    OfType<MethodDeclarationSyntax>().
            //    Where(m => m.Identifier.ValueText == createMethodName).
            //    FirstOrDefault();
            //if (createMethod != null)
            //{
            //    INamedTypeSymbol createReturnType = context.SemanticModel.GetTypeInfo(createMethod.ReturnType).Type as INamedTypeSymbol;
            //    if ((!classUnderTestSymbol.IsAbstract && createReturnType == classUnderTestSymbol) ||
            //        string.Equals(createReturnType.ToDisplayString(), $"Moq.Mock<{classUnderTestSymbol.ToDisplayString()}>"))
            //    {
            //        return;
            //    }
            //}

            //ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
            //builder.Add(ClassUnderTestFullyQualifiedNameProperty, classUnderTestSymbol.ToDisplayString());
            //builder.Add(CreateMethodNameProperty, createMethodName);
            //context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation(), builder.ToImmutable(), classUnderTestName));
        }

    }
}