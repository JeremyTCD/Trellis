using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class SyntaxHelper
    {
        public static SyntaxNode SimplifyQualifiedNames(SyntaxNode node)
        {
            foreach (QualifiedNameSyntax qualifiedName in node.DescendantNodes().OfType<QualifiedNameSyntax>())
            {
                if (!MiscHelper.IsBuiltInType(qualifiedName))
                {
                    node = node.ReplaceNode(qualifiedName, qualifiedName.Right);
                }
            }

            return node;
        }

        public static List<SyntaxNode> SortUsings(List<SyntaxNode> usingDirectives)
        {
            return usingDirectives.OrderBy(n =>
            {
                string name = (n as UsingDirectiveSyntax).Name.ToString();
                return name.StartsWith("System") ? "_" : name; // System namespaces should come first
            }).ToList();
        }

        public static void TryInsertUsing(CompilationUnitSyntax compilationUnit, string namespaceName, DocumentEditor documentEditor)
        {
            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == namespaceName))
            {
                documentEditor.InsertBefore(compilationUnit.Usings[0], new[] { SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(namespaceName)) });
            }
        }

        public static SyntaxTrivia GetEndOfLineTrivia(SyntaxNode syntaxNode = null)
        {
            if (syntaxNode == null)
            {
                return SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");
            }

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

        public static SyntaxTrivia GetIndentationTrivia(SyntaxNode syntaxNode = null)
        {
            if (syntaxNode == null)
            {
                return SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "    ");
            }

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

        public static void FixMemberTrivia(MemberDeclarationSyntax[] members)
        {
            SyntaxTrivia endOfLineTrivia = GetEndOfLineTrivia();
            SyntaxTrivia indentation = GetIndentationTrivia(members[0]);

            for (int i = 0; i < members.Length; i++)
            {
                MemberDeclarationSyntax member = members[i];
                if (i == 0)
                {
                    // Remove any leading new line trivia
                    members[i] = member.
                        WithLeadingTrivia(indentation).
                        WithTrailingTrivia(endOfLineTrivia);
                }
                else
                {
                    members[i] = member.
                        WithLeadingTrivia(endOfLineTrivia, indentation).
                        WithTrailingTrivia(endOfLineTrivia);
                }
            }
        }
    }
}
