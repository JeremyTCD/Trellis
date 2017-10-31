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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1000CodeFixProvider)), Shared]
    public class JA1000CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.Single();

            if (!diagnostic.Properties.ContainsKey(Constants.NoCodeFix))
            { 
                context.RegisterCodeFix(
                    CodeAction.Create(
                    nameof(JA1000CodeFixProvider),
                    cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                    nameof(JA1000CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            // Add using for moq if required
            SyntaxHelper.TryInsertUsing(compilationUnit, "Moq", documentEditor);

            // Add mock repository if required
            VariableDeclarationSyntax mockRepositoryVariableDeclaration = TestingHelper.GetMockRepositoryFieldDeclaration(compilationUnit, documentEditor.SemanticModel);
            if (mockRepositoryVariableDeclaration == null)
            {
                FieldDeclarationSyntax mockRepositoryFieldDeclaration = TestingHelper.CreateMockRepositoryFieldDeclaration(syntaxGenerator);
                ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
                documentEditor.InsertMembers(classDeclaration, 0, new[] { mockRepositoryFieldDeclaration });
                mockRepositoryVariableDeclaration = mockRepositoryFieldDeclaration.Declaration;
            }

            // Replace local variable declaration statement (do this first since it relies on the diagnostic location)
            LocalDeclarationStatementSyntax oldDummyVariableLocalDeclarationStatement = compilationUnit.
                FindNode(diagnostic.Location.SourceSpan).
                FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
            LocalDeclarationStatementSyntax newDummyVariableLocalDeclarationStatement = CreateDummyVariableLocalDeclarationStatement(
                syntaxGenerator,
                diagnostic.Properties[JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.VariableIdentifierProperty],
                diagnostic.Properties[JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.InterfaceIdentifierProperty],
                mockRepositoryVariableDeclaration.Variables.First().Identifier.ValueText);
            documentEditor.ReplaceNode(oldDummyVariableLocalDeclarationStatement, newDummyVariableLocalDeclarationStatement);

            // Update references to local variable
            MethodDeclarationSyntax testMethodDeclaration = oldDummyVariableLocalDeclarationStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            IEnumerable<IdentifierNameSyntax> dummyVariableIdentifierNames = testMethodDeclaration.
                DescendantNodes().
                OfType<IdentifierNameSyntax>().
                Where(i => i.Identifier.ToString() == diagnostic.Properties[JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.VariableIdentifierProperty]);
            foreach (IdentifierNameSyntax identifierName in dummyVariableIdentifierNames)
            {
                // TODO doesn't work if identifierName is part of an argument with argument name stated
                SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(
                    identifierName,
                    syntaxGenerator.IdentifierName("Object"));
                documentEditor.ReplaceNode(identifierName, memberAccessExpression);
            }

            return documentEditor.GetChangedDocument();
        }

        private static LocalDeclarationStatementSyntax CreateDummyVariableLocalDeclarationStatement(SyntaxGenerator syntaxGenerator, string variableName,
            string interfaceName, string mockRepositoryVariableName)
        {
            SyntaxNode invocationExpression = TestingHelper.CreateMockRepositoryCreateInvocationExpression(syntaxGenerator, interfaceName, mockRepositoryVariableName);
            SyntaxNode interfaceIdentifier = syntaxGenerator.IdentifierName(interfaceName);
            SyntaxNode genericName = syntaxGenerator.GenericName("Mock", interfaceIdentifier);

            return (LocalDeclarationStatementSyntax)syntaxGenerator.LocalDeclarationStatement(genericName, variableName, invocationExpression);
        }
    }
}
