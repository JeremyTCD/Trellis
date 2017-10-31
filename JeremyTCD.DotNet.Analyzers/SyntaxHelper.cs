using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class SyntaxHelper
    {
        public static void TryInsertUsing(CompilationUnitSyntax compilationUnit, string namespaceName, DocumentEditor documentEditor)
        {
            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == namespaceName))
            {
                documentEditor.InsertBefore(compilationUnit.Usings[0], new[] { SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(namespaceName)) });
            }
        }

        public static SyntaxTrivia GetEndOfLineTrivia(SyntaxNode syntaxNode)
        {
            SyntaxTrivia endOfLineTrivia = syntaxNode.
                DescendantTokens().
                SelectMany(token => token.TrailingTrivia).
                FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            if (endOfLineTrivia.Equals(default(SyntaxTrivia)))
            {
                endOfLineTrivia = SyntaxFactory.CarriageReturnLineFeed;
            }

            return endOfLineTrivia;
        }

        public static SyntaxTrivia GetIndentationTrivia(SyntaxNode syntaxNode)
        {
            SyntaxTrivia indentation = syntaxNode.
                GetLeadingTrivia().
                Where(t => t.Kind() == SyntaxKind.WhitespaceTrivia).
                FirstOrDefault();

            if (indentation.Equals(default(SyntaxTrivia)))
            {
                indentation = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "    ");
            }

            return indentation;
        }
    }
}
