using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class SyntaxHelper
    {
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
