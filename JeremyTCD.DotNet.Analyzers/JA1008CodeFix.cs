﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1008CodeFixProvider)), Shared]
    public class JA1008CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup.DiagnosticId);

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
                        nameof(JA1008CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1008CodeFixProvider)),
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

            MethodDeclarationSyntax oldMethodDeclaration = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as MethodDeclarationSyntax;
            ExpressionStatementSyntax newMockRepositoryVerifyAllExpression = CreateMockRepositoryVerifyAllExpression(syntaxGenerator);
            MethodDeclarationSyntax newMethodDeclaration = oldMethodDeclaration.AddBodyStatements(newMockRepositoryVerifyAllExpression);

            documentEditor.ReplaceNode(oldMethodDeclaration, newMethodDeclaration);

            return documentEditor.GetChangedDocument();
        }

        private static ExpressionStatementSyntax CreateMockRepositoryVerifyAllExpression(SyntaxGenerator syntaxGenerator)
        {
            // TODO assumes that a MockRepository instances named _mockRepository exists
            SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(syntaxGenerator.IdentifierName("_mockRepository"), "VerifyAll");
            SyntaxNode invocationExpression = syntaxGenerator.InvocationExpression(memberAccessExpression);
            return syntaxGenerator.ExpressionStatement(invocationExpression) as ExpressionStatementSyntax;
        }
    }
}
