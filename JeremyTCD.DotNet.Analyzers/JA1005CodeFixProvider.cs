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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1004CodeFixProvider)), Shared]
    public class JA1005CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1005NewMustNotBeUsedToInstantiateClassUnderTestInTestMethods.DiagnosticId);

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
                        nameof(JA1005CodeFixProvider),
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(JA1005CodeFixProvider)),
                        diagnostic);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            INamedTypeSymbol classUnderTestSymbol = semanticModel.
                Compilation.
                GetTypeByMetadataName(diagnostic.Properties[JA1005NewMustNotBeUsedToInstantiateClassUnderTestInTestMethods.ClassUnderTestFullyQualifiedNameProperty]);
            IMethodSymbol classUnderTestConstructor = classUnderTestSymbol.Constructors.OrderByDescending(c => c.Parameters.Count()).First();
            IEnumerable<IParameterSymbol> classUnderTestConstructorParameters = classUnderTestConstructor.Parameters;
            string createMethodName = diagnostic.Properties[JA1005NewMustNotBeUsedToInstantiateClassUnderTestInTestMethods.CreateMethodNameProperty];

            // Create create method if it does not exist
            IEnumerable<MethodDeclarationSyntax> methodDeclarations = compilationUnit.
                DescendantNodes().
                OfType<MethodDeclarationSyntax>().
                Where(m => m.Identifier.ValueText == createMethodName);
            MethodDeclarationSyntax createMethodDeclaration = methodDeclarations.FirstOrDefault(m => (semanticModel.GetDeclaredSymbol(m) as IMethodSymbol).ReturnType == classUnderTestSymbol);
            // TODO logic to ensure that create method parameters are valid (different analyzer?)
            if (createMethodDeclaration == null)
            {
                // create method
                createMethodDeclaration = CreateMethodDeclaration(classUnderTestSymbol, syntaxGenerator, createMethodName, classUnderTestConstructor, 
                    classUnderTestConstructorParameters);
                documentEditor.AddMember(compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First(), createMethodDeclaration);
            }

            // TODO don't add any parameters if class under test has multiple constructors
            // TODO if new is used in an argument
            // Replace object creation expression with invocation expression
            ObjectCreationExpressionSyntax objectCreationExpression = compilationUnit.FindNode(diagnostic.Location.SourceSpan) as ObjectCreationExpressionSyntax;
            List<SyntaxNode> createMethodArguments = new List<SyntaxNode>();
            int numArguments = objectCreationExpression.ArgumentList.Arguments.Count();
            bool anySkipped = false;

            for(int i = 0; i< numArguments; i++)
            {
                ExpressionSyntax expression = objectCreationExpression.ArgumentList.Arguments[i].Expression;
                if (expression.ToString() != "null")
                {
                    SyntaxNode argument = anySkipped ? syntaxGenerator.Argument(createMethodDeclaration.ParameterList.Parameters[i].Identifier.ValueText, RefKind.None, expression) :
                        syntaxGenerator.Argument(RefKind.None, expression);
                    createMethodArguments.Add(argument);
                }
                else
                {
                    anySkipped = true;
                }
            }

            documentEditor.ReplaceNode(objectCreationExpression, syntaxGenerator.InvocationExpression(syntaxGenerator.IdentifierName(createMethodName), createMethodArguments));

            return documentEditor.GetChangedDocument();
        }

        private static MethodDeclarationSyntax CreateMethodDeclaration(INamedTypeSymbol classUnderTestSymbol,
            SyntaxGenerator syntaxGenerator, string methodName, IMethodSymbol constructor, IEnumerable<IParameterSymbol> parameters)
        {
            IEnumerable<SyntaxNode> parameterSyntaxes = parameters.
                Select(p => syntaxGenerator.ParameterDeclaration(p, SyntaxFactory.IdentifierName("null")));

            SyntaxNode classUnderTestCreation = syntaxGenerator.
                ObjectCreationExpression(
                    classUnderTestSymbol,
                    parameterSyntaxes.Select(p => syntaxGenerator.Argument(syntaxGenerator.IdentifierName((p as ParameterSyntax).Identifier.ValueText))));

            SyntaxNode returnStatement = syntaxGenerator.ReturnStatement(classUnderTestCreation);

            return syntaxGenerator.MethodDeclaration(
                   methodName,
                   parameters: parameterSyntaxes,
                   returnType: syntaxGenerator.TypeExpression(classUnderTestSymbol),
                   accessibility: Accessibility.Private,
                   statements: new[] { returnStatement }) as MethodDeclarationSyntax;
        }
    }
}
