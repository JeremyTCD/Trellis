// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    /// <summary>
    /// A test method's dummy local variable is not a mock of its type's interface.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs if a test method contains a dummy local variable that is not a mock of its type's interface.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1000TestMethodsMustUseInterfaceMocksForDummies : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1000DerivedMembersMustInheritDocumentation"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1000";
        public const string InterfaceIdentifierProperty = "InterfaceIdentifierProperty";
        public const string VariableIdentifierProperty = "VariableIdentifierProeprty";

        private const string Title = "Test methods must use interface mocks for dummies.";
        private const string MessageFormat = "Dummy local variable should be a mock of its interface.";
        private const string Description = "A test method's dummy local variable is not a mock of its type's interface.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(HandleVariableDeclaration, SyntaxKind.LocalDeclarationStatement);
            context.EnableConcurrentExecution();
        }

        private void HandleVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            LocalDeclarationStatementSyntax localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

            // Return if not in a test class
            if (!localDeclaration.Ancestors().OfType<CompilationUnitSyntax>().First().Usings.Any(u => u.Name.ToString() == "Xunit"))
            {
                return;
            }

            // Return if local variable is a stub
            if (localDeclaration.Declaration.Variables.First().Identifier.ToString().StartsWith("stub"))
            {
                return;
            }

            // Return if method has no theory or fact attribute
            MethodDeclarationSyntax methodDeclaration = localDeclaration.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDeclaration == null || !methodDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.ToString() == "Fact" || a.ToString() == "Theory")))
            {
                return;
            }

            // Return if theory or fact attribute is not from the Xunit namespace
            IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null || !methodSymbol.GetAttributes().Any(a => a.ToString().StartsWith("Xunit.")))
            {
                return;
            }


            ClassDeclarationSyntax testClassDeclaration = localDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if(testClassDeclaration == null)
            {
                return;
            }
            string classUnderTestName = testClassDeclaration.Identifier.ValueText.
                Replace("UnitTests", string.Empty).
                Replace("IntegrationTests", string.Empty).
                Replace("EndToEndTests", string.Empty);
            ISymbol classUnderTestSymbol = SymbolHelper.TryGetSymbol(classUnderTestName, context.Compilation.GlobalNamespace);

            // Add diagnostic if local variable's type has an interface in the assembly under test
            foreach (VariableDeclaratorSyntax variableDeclarator in localDeclaration.Declaration.Variables)
            {
                EqualsValueClauseSyntax initializer = variableDeclarator.Initializer;
                if (initializer == null)
                {
                    continue;
                }

                ITypeSymbol typeSymbol = context.SemanticModel.GetTypeInfo(initializer.Value).Type;
                if (typeSymbol == classUnderTestSymbol || typeSymbol == null)
                {
                    continue;
                }

                INamedTypeSymbol interfaceSymbol = typeSymbol.Interfaces.FirstOrDefault(i => i.ContainingAssembly == classUnderTestSymbol.ContainingAssembly);

                if (interfaceSymbol != null)
                {
                    ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder.Add(InterfaceIdentifierProperty, interfaceSymbol.Name);
                    builder.Add(VariableIdentifierProperty, variableDeclarator.Identifier.ToString());
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableDeclarator.Identifier.GetLocation(), builder.ToImmutable()));
                }
            }
        }
    }
}