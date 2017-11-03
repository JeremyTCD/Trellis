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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            SemanticModel semanticModel = documentEditor.SemanticModel;

            // Get interface declaration
            InterfaceDeclarationSyntax interfaceDeclaration = compilationUnit.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First();

            // Get factory interface
            ITypeSymbol factoryInterface = semanticModel.GetDeclaredSymbol(interfaceDeclaration) as ITypeSymbol;

            // Get produced interface
            ITypeSymbol producedInterface = FactoryHelper.GetProducedType(interfaceDeclaration, semanticModel.Compilation.GlobalNamespace);

            // Get produced interface declaration
            InterfaceDeclarationSyntax producedInterfaceDeclaration = producedInterface.DeclaringSyntaxReferences.First().GetSyntax() as InterfaceDeclarationSyntax;

            // Check if interface has any methods that return produced interface, if it does, rename them to create
            IEnumerable<IMethodSymbol> methodsThatReturnProducedInterface = factoryInterface.
                GetMembers().
                OfType<IMethodSymbol>().
                Where(m => m.ReturnType == producedInterface);

            if (methodsThatReturnProducedInterface.Count() > 0)
            {
                foreach (IMethodSymbol method in methodsThatReturnProducedInterface)
                {
                    MethodDeclarationSyntax methodDeclaration = method.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax;
                    // TODO methods with the same arguments
                    documentEditor.ReplaceNode(methodDeclaration, methodDeclaration.WithIdentifier(SyntaxFactory.Identifier("Create")));
                }
            }
            else
            {
                SyntaxTrivia indentationTrivia = SyntaxHelper.GetIndentationTrivia();

                // Create method declaration
                MethodDeclarationSyntax createMethodDeclaration = FactoryHelper.
                    CreateCreateMethodDeclaration(producedInterfaceDeclaration, syntaxGenerator).
                    WithLeadingTrivia(indentationTrivia, indentationTrivia);

                // If no methods that return produced interface, create a new create method
                documentEditor.AddMember(interfaceDeclaration, createMethodDeclaration);
            }

            return documentEditor.GetChangedDocument();
        }
    }
}
