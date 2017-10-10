using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1000CodeFixProvider)), Shared]
    public class JA1001JA1004CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1001TestClassNamespacesMustBeginWithNamespaceOfClassUnderTest.DiagnosticId, JA1004TestClassNamespacesMustEndWithTests.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    nameof(JA1001JA1004CodeFixProvider),
                    cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                    nameof(JA1001JA1004CodeFixProvider)),
                diagnostic);

            return Task.FromResult(default(object));
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            // TODO multiple classes declared in document
            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            string className = classDeclaration.Identifier.ToString();
            string classUnderTestName = className.Replace("UnitTests", "").Replace("IntegrationTests", "").Replace("EndToEndTests", "");
            // TODO multiple symbols match class under test's name
            ISymbol symbol = documentEditor.SemanticModel.LookupSymbols(0, name: classUnderTestName).First();
            // TODO don't offer fix if class under test is invalid
            if(symbol == null)
            {
                return document;
            }

            // Updated namespace
            SyntaxNode oldQualifiedName = compilationUnit.FindNode(diagnostic.Location.SourceSpan);
            SyntaxNode classUnderTestNamespace = (symbol.ContainingNamespace.DeclaringSyntaxReferences.First().GetSyntax() as NamespaceDeclarationSyntax).Name.WithoutTrailingTrivia();
            SyntaxNode newQualifiedName = syntaxGenerator.QualifiedName(classUnderTestNamespace, syntaxGenerator.IdentifierName("Tests"));
            documentEditor.ReplaceNode(oldQualifiedName, newQualifiedName);

            return documentEditor.GetChangedDocument();
        }
    }
}
