// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace JeremyTCD.DotNet.Analyzers
{
    // The issue with creating dummy implementations of abstract classes just for testing is that if the abstract classes change, many test classes will need to be updated
    // combine with 1006
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1007MocksMustBeUsedToTestAbstractClasses : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1007MocksMustBeUsedToTestAbstractClasses"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1007";
        public const string ClassUnderTestFullyQualifiedNameProperty = "ClassUnderTestFullyQualifiedName";
        public const string CreateMethodNameProperty = "CreateMethodName";

        private const string Title = "Test class must have create method for class under test.";
        private const string MessageFormat = "Test class must have a \"Create{0}\" method that returns \"{0}\" or \"Mock<{0}>\".";
        private const string Description = "A test class does not have a create method for the class under test.";
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