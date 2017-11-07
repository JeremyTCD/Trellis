using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
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
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Properties.ContainsKey(Constants.NoCodeFix))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                        nameof(JA1202CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1202CodeFixProvider)),
                        diagnostic);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            FactoryInterfaceContext factoryInterfaceContext = await FactoryInterfaceContextFactory.
                TryCreateAsync(document).ConfigureAwait(false);

            // Check if interface has any methods that return produced interface, if it does, rename them to create
            IEnumerable<IMethodSymbol> methodsThatReturnProducedInterface = factoryInterfaceContext.
                Methods.
                Where(m => m.ReturnType == factoryInterfaceContext.ProducedInterface);

            if (methodsThatReturnProducedInterface.Count() > 0)
            {
                foreach (IMethodSymbol method in methodsThatReturnProducedInterface)
                {
                    MethodDeclarationSyntax methodDeclaration = method.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax;
                    // TODO methods that initially have different names but the same arguments
                    documentEditor.ReplaceNode(methodDeclaration, methodDeclaration.WithIdentifier(SyntaxFactory.Identifier("Create")));
                }
            }
            else
            {
                SyntaxTrivia indentationTrivia = SyntaxHelper.GetIndentationTrivia();

                // Create method declaration
                MethodDeclarationSyntax createMethodDeclaration = FactoryHelper.
                    CreateCreateMethodDeclaration(factoryInterfaceContext.ProducedInterfaceDeclaration, syntaxGenerator).
                    WithLeadingTrivia(indentationTrivia, indentationTrivia);

                // If no methods that return produced interface, create a new create method
                documentEditor.AddMember(factoryInterfaceContext.InterfaceDeclaration, createMethodDeclaration);
            }

            return documentEditor.GetChangedDocument();
        }
    }
}
