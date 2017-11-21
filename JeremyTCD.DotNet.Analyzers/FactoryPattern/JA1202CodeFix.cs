using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1202CodeFixProvider)), Shared]
    public class JA1202CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();
            Document document = context.Document;
            FactoryInterfaceContext factoryInterfaceContext = FactoryInterfaceContextFactory.TryCreateAsync(document).Result;

            // Check if interface has any methods that return produced interface, if it does, rename them to create
            IEnumerable<IMethodSymbol> methodsThatReturnProducedInterface = factoryInterfaceContext.
                Methods.
                Where(m => m.ReturnType == factoryInterfaceContext.ProducedInterface);

            if (methodsThatReturnProducedInterface.Count() > 0)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    string.Format(Strings.JA1202_CodeFix_Title_RenameMethods, factoryInterfaceContext.ProducedInterface.Name),
                    cancellationToken => RenameExistingMethodsAsync(methodsThatReturnProducedInterface, document, cancellationToken),
                    $"{nameof(JA1202CodeFixProvider)}RenameMethods"),
                    diagnostic);
            }
            else
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1202_CodeFix_Title_CreateMethod,
                    cancellationToken => CreateCreateMethodAsync(factoryInterfaceContext, document, cancellationToken),
                    $"{nameof(JA1202CodeFixProvider)}CreateMethod"),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> CreateCreateMethodAsync(
            FactoryInterfaceContext factoryInterfaceContext,
            Document document,
            CancellationToken cancellationToken)
        {
            DocumentEditor documentEditor = DocumentEditor.CreateAsync(document).Result;
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            SyntaxTrivia indentationTrivia = SyntaxHelper.GetIndentationTrivia();

            // Create method declaration
            MethodDeclarationSyntax createMethodDeclaration = FactoryHelper.
                CreateCreateMethodDeclaration(factoryInterfaceContext.ProducedInterfaceDeclaration, syntaxGenerator).
                WithLeadingTrivia(indentationTrivia, indentationTrivia);

            // If no methods that return produced interface, create a new create method
            documentEditor.AddMember(factoryInterfaceContext.InterfaceDeclaration, createMethodDeclaration);

            return documentEditor.GetChangedDocument();
        }

        private static async Task<Solution> RenameExistingMethodsAsync(
            IEnumerable<IMethodSymbol> methodsThatReturnProducedInterface,
            Document document,
            CancellationToken cancellationToken)
        {
            Solution solution = document.Project.Solution;

            for(int i = 0; i < methodsThatReturnProducedInterface.Count(); i++)
            {
                IMethodSymbol method = methodsThatReturnProducedInterface.ElementAt(i);
                if (i != 0)
                {
                    // Solution has changed, retrieve new symbol
                    document = solution.GetDocument(document.Id);
                    string methodDisplayString = method.ToDisplayString();
                    method = document.
                        GetSemanticModelAsync().
                        Result.
                        Compilation.
                        GetTypeByMetadataName(method.ContainingType.ToDisplayString()).
                        GetMembers().
                        FirstOrDefault(m => m.ToDisplayString().Equals(methodDisplayString, StringComparison.OrdinalIgnoreCase)) as IMethodSymbol;
                }

                // TODO methods that initially have different names but the same arguments
                // TODO after solution changes, does method symbol need to be re-looked up?
                solution = Renamer.RenameSymbolAsync(solution, method, "Create", solution.Workspace.Options).Result;
            }

            return solution;
        }
    }
}
