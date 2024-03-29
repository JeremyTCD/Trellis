﻿using Microsoft.CodeAnalysis;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1009CodeFixProvider)), Shared]
    public class JA1009CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();

            if (!diagnostic.Properties.ContainsKey(Constants.NoCodeFix))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1009_CodeFix_Title,
                    cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                    nameof(JA1009CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            TestClassContext testClassContext = await TestClassContextFactory.TryCreateAsync(document).ConfigureAwait(false);

            // Check if mock repository field exists
            if (testClassContext.MockRepositoryVariableDeclaration == null)
            {
                FieldDeclarationSyntax mockRepositoryFieldDeclaration = TestingHelper.
                    CreateMockRepositoryFieldDeclaration(syntaxGenerator);
                documentEditor.InsertMembers(testClassContext.ClassDeclaration, 0, new[] { mockRepositoryFieldDeclaration });
            }

            // Replace object creation expression
            ObjectCreationExpressionSyntax oldExpression = testClassContext.
                CompilationUnit.
                FindNode(diagnostic.Location.SourceSpan) as ObjectCreationExpressionSyntax;
            InvocationExpressionSyntax newExpression = TestingHelper.
                CreateMockRepositoryCreateInvocationExpression(
                    syntaxGenerator,
                    (oldExpression.Type as GenericNameSyntax).TypeArgumentList.Arguments.First().ToString(),
                    oldExpression.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault()?.Arguments).
                WithTriviaFrom(oldExpression);
            documentEditor.ReplaceNode(oldExpression, newExpression);

            return documentEditor.GetChangedDocument();
        }
    }
}
