using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1201CodeFixProvider)), Shared]
    public class JA1201CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
            CompilationUnitSyntax compilationUnit = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            FactoryInterfaceContext factoryInterfaceContext = FactoryInterfaceContextFactory.TryCreate(semanticModel, compilationUnit);

            // Factory interface has no valid produced interface, manually retrieve create methods
            IEnumerable<IMethodSymbol> createMethods = factoryInterfaceContext.
                InterfaceSymbol.
                GetMembers().
                OfType<IMethodSymbol>().
                Where(m => m.Name == "Create");
            if (createMethods.Count() == 0)
            {
                return Task.CompletedTask;
            }

            // Get type returned by create methods
            ITypeSymbol producedInterface = null;
            foreach (IMethodSymbol createMethod in createMethods)
            {
                if (createMethod.ReturnType == null || createMethod.ReturnType.TypeKind != TypeKind.Interface)
                {
                    return Task.CompletedTask;
                }

                if (producedInterface == null)
                {
                    producedInterface = createMethod.ReturnType;
                }
                else if (producedInterface != createMethod.ReturnType)
                {
                    return Task.CompletedTask;
                }
            }

            string expectedFactoryInterfaceName = $"{producedInterface.Name}Factory";
            Diagnostic diagnostic = context.Diagnostics.First();
            context.RegisterCodeFix(
                CodeAction.Create(
                    string.Format(Strings.JA1201_CodeFix_Title, expectedFactoryInterfaceName),
                    cancellationToken => GetTransformedDocumentAsync(
                        expectedFactoryInterfaceName,
                        factoryInterfaceContext,
                        document,
                        cancellationToken),
                nameof(JA1201CodeFixProvider)),
                diagnostic
                );

            return Task.CompletedTask;
        }

        private static async Task<Solution> GetTransformedDocumentAsync(
            string expectedFactoryInterfaceName,
            FactoryInterfaceContext factoryInterfaceContext,
            Document document,
            CancellationToken cancellationToken)
        {
            Solution oldSolution = document.Project.Solution;

            return Renamer.
                RenameSymbolAsync(oldSolution, factoryInterfaceContext.InterfaceSymbol, expectedFactoryInterfaceName, oldSolution.Workspace.Options).Result;
        }
    }
}
