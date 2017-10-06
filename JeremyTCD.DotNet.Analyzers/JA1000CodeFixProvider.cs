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
            ImmutableArray.Create(JA1000TestMethodsMustUseInterfaceMocksForDummies.DiagnosticId);

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
                    nameof(JA1000CodeFixProvider),
                    cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                    nameof(JA1000CodeFixProvider)),
                diagnostic);

            return Task.FromResult(default(object));
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            // TODO publish package locally, use in commandline
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            // Add using for moq if required
            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == "Moq"))
            {
                documentEditor.InsertBefore(compilationUnit.Usings[0], new[] { SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Moq")) });
            }

            // Add mock repository if required
            VariableDeclarationSyntax mockRepositoryVariableDeclarataion = compilationUnit.
                DescendantNodes().
                OfType<VariableDeclarationSyntax>().
                Where(v => documentEditor.SemanticModel.GetTypeInfo(v.Type).Type.ToString() == "Moq.MockRepository").
                FirstOrDefault();
            if (mockRepositoryVariableDeclarataion == null)
            {
                FieldDeclarationSyntax mockRepositoryFieldDeclaration = CreateMockRepositoryFieldDeclaration(syntaxGenerator);
                ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
                documentEditor.InsertMembers(classDeclaration, 0, new[] { mockRepositoryFieldDeclaration });
            }

            // Replace local variable declaration statement (do this first since it relies on the diagnostic location)
            LocalDeclarationStatementSyntax oldDummyVariableLocalDeclarationStatement = compilationUnit.
                FindNode(diagnostic.Location.SourceSpan).
                FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
            LocalDeclarationStatementSyntax newDummyVariableLocalDeclarationStatement = CreateDummyVariableLocalDeclarationStatement(
                syntaxGenerator,
                diagnostic.Properties[JA1000TestMethodsMustUseInterfaceMocksForDummies.VariableIdentifierProperty],
                diagnostic.Properties[JA1000TestMethodsMustUseInterfaceMocksForDummies.InterfaceIdentifierProperty]);
            documentEditor.ReplaceNode(oldDummyVariableLocalDeclarationStatement, newDummyVariableLocalDeclarationStatement);

            // Update references to local variable
            MethodDeclarationSyntax testMethodDeclaration = oldDummyVariableLocalDeclarationStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            IEnumerable<IdentifierNameSyntax> dummyVariableIdentifierNames = testMethodDeclaration.
                DescendantNodes().
                OfType<IdentifierNameSyntax>().
                Where(i => i.Identifier.ToString() == diagnostic.Properties[JA1000TestMethodsMustUseInterfaceMocksForDummies.VariableIdentifierProperty]);
            foreach (IdentifierNameSyntax identifierName in dummyVariableIdentifierNames)
            {
                SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(
                    identifierName,
                    syntaxGenerator.IdentifierName("Object"));
                documentEditor.ReplaceNode(identifierName, memberAccessExpression);
            }

            return documentEditor.GetChangedDocument();
        }

        private static LocalDeclarationStatementSyntax CreateDummyVariableLocalDeclarationStatement(SyntaxGenerator syntaxGenerator, string variableName,
            string interfaceName)
        {
            SyntaxNode interfaceIdentifier = syntaxGenerator.IdentifierName(interfaceName);
            SyntaxNode memberAccessExpression = syntaxGenerator.MemberAccessExpression(
                syntaxGenerator.IdentifierName("_mockRepository"),
                syntaxGenerator.GenericName("Create", interfaceIdentifier));
            SyntaxNode invocationExpression = syntaxGenerator.InvocationExpression(memberAccessExpression);
            SyntaxNode genericName = syntaxGenerator.GenericName("Mock", interfaceIdentifier);

            return (LocalDeclarationStatementSyntax)syntaxGenerator.LocalDeclarationStatement(genericName, variableName, invocationExpression);
        }

        private static FieldDeclarationSyntax CreateMockRepositoryFieldDeclaration(SyntaxGenerator syntaxGenerator)
        {
            SyntaxNode argumentMemberAccessExpression = syntaxGenerator.MemberAccessExpression(
                    syntaxGenerator.IdentifierName("MockBehavior"),
                    syntaxGenerator.IdentifierName("Default"));
            SyntaxNode constructorArgument = syntaxGenerator.Argument(argumentMemberAccessExpression);
            SyntaxNode typeIdentifier = syntaxGenerator.IdentifierName("MockRepository");
            SyntaxNode objectCreationExpression = syntaxGenerator.ObjectCreationExpression(typeIdentifier, constructorArgument);

            return (FieldDeclarationSyntax)syntaxGenerator.FieldDeclaration("_mockRepository", typeIdentifier, initializer: objectCreationExpression);
        }
    }
}
