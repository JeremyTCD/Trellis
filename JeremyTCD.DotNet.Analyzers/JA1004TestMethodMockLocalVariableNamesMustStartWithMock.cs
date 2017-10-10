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
    public class JA1004TestMethodMockLocalVariableNamesMustStartWithMock : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1004TestMethodMockLocalVariableNamesMustStartWithMock"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1004";

        private const string Title = "Test method mock local variable names must start with \"mock\".";
        private const string MessageFormat = "Mock local variable {0}'s name must start with \"mock\".";
        private const string Description = "A test method mock local varaible's name does not start with \"mock\".";
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

            // Find invocations of Mock<T>.Setup
            INamedTypeSymbol mockType = context.Compilation.GetTypeByMetadataName("Moq.Mock`1");

            IEnumerable<LocalDeclarationStatementSyntax> localDeclarations = compilationUnit.
                DescendantNodes().
                OfType<LocalDeclarationStatementSyntax>();

            foreach(LocalDeclarationStatementSyntax localDeclaration in localDeclarations)
            {
                GenericNameSyntax genericName = localDeclaration.Declaration.Type as GenericNameSyntax;
                if(genericName == null)
                {
                    continue;
                }

                if (!genericName.Identifier.ValueText.StartsWith("Mock"))
                {
                    continue;
                }

                if(context.
                    SemanticModel.
                    GetTypeInfo(compilationUnit.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().First().Declaration.Type).
                    Type.
                    OriginalDefinition != mockType)
                {
                    continue;
                }

                // Get all occurences of the local variable within its method, if it ever calls a method named "setup", its own name must begin with mock
            }

            // TODO consider other setup methods
            //IEnumerable<ISymbol> setupMethods = mockType.GetMembers("Setup");
            //IEnumerable<InvocationExpressionSyntax> invocations = compilationUnit.DescendantNodes().OfType<InvocationExpressionSyntax>();

            //foreach(InvocationExpressionSyntax invocation in invocations)
            //{
            //    SimpleNameSyntax memberName = (invocation.Expression as MemberAccessExpressionSyntax)?.Name;
            //    if (memberName.Identifier.ValueText == "Setup")
            //    {
            //        ISymbol memberSymbol = context.SemanticModel.GetDeclaredSymbol(memberName);

            //    }
            //}

            // Add diagnostics          
            //ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            //string className = classDeclaration.Identifier.ToString();
            //string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            //if (classUnderTestName == className || SymbolHelper.TryGetSymbol(classUnderTestName, context.Compilation.GlobalNamespace) == null)
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation(), classUnderTestName));
            //}
        }
    }
}