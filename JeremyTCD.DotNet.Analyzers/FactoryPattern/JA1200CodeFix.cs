using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1200CodeFixProvider)), Shared]
    public class JA1200CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1200FactoryClassNamesMustBeCorrectlyFormatted.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();
            string expectedClassName = diagnostic.Properties[JA1200FactoryClassNamesMustBeCorrectlyFormatted.ExpectedClassNameProperty];

            context.RegisterCodeFix(
                CodeAction.Create(
                string.Format(Strings.JA1200_CodeFix_Title, expectedClassName),
                cancellationToken => GetTransformedDocumentAsync(expectedClassName, context.Document, diagnostic, cancellationToken),
                nameof(JA1200CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Solution> GetTransformedDocumentAsync(string expectedClassName, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            FactoryClassContext factoryClassContext = await FactoryClassContextFactory.
                TryCreateAsync(document).ConfigureAwait(false);

            Solution oldSolution = document.Project.Solution;

            return Renamer.RenameSymbolAsync(oldSolution, factoryClassContext.ClassSymbol, expectedClassName, oldSolution.Workspace.Options).Result;
        }
    }
}
