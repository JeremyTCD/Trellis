using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class DocumentationHelper
    {
        public static string GetNodeContentsAsNormalizedString(SyntaxNode syntaxNode)
        {
            string result = string.Empty;
            // TODO should recurse incase child nodes have their own children and so on
            foreach (SyntaxNode node in syntaxNode.ChildNodes())
            {
                if (node is XmlElementStartTagSyntax || node is XmlElementEndTagSyntax)
                {
                    continue;
                }

                if (node is XmlTextSyntax)
                {
                    foreach (SyntaxToken syntaxToken in (node as XmlTextSyntax).TextTokens)
                    {
                        if(syntaxToken.Kind() == SyntaxKind.XmlTextLiteralToken)
                        {
                            result += syntaxToken.ToString();
                        }
                    }
                }
                else if (node is XmlEmptyElementSyntax)
                {
                    XmlEmptyElementSyntax xmlEmptyElement = node as XmlEmptyElementSyntax;
                    string elementName = xmlEmptyElement.Name.ToString();
                    if (elementName == "paramref")
                    {
                        XmlNameAttributeSyntax xmlNameAttribute = xmlEmptyElement.Attributes.FirstOrDefault(a => a is XmlNameAttributeSyntax) as XmlNameAttributeSyntax;
                        string nameValue = xmlNameAttribute?.Identifier.ToString();
                        if (!string.IsNullOrWhiteSpace(nameValue))
                        {
                            result += nameValue;
                        }
                    }
                    else if (elementName == "see")
                    {
                        XmlCrefAttributeSyntax xmlCrefAttribute = xmlEmptyElement.Attributes.FirstOrDefault(a => a is XmlCrefAttributeSyntax) as XmlCrefAttributeSyntax;
                        string crefValue = xmlCrefAttribute?.Cref.ToString();
                        if (!string.IsNullOrWhiteSpace(crefValue))
                        {
                            result += crefValue;
                        }
                    }
                }
            }

            return result;
        }

        public static DocumentationCommentTriviaSyntax GetDocumentCommentTrivia(SyntaxNode syntaxNode)
        {
            return syntaxNode.
                GetLeadingTrivia().
                Select(s => s.GetStructure()).
                OfType<DocumentationCommentTriviaSyntax>().
                FirstOrDefault();
        }

        public static DocumentationCommentTriviaSyntax GetInheritedDocumentCommentTrivia(IMethodSymbol method)
        {
            DocumentationCommentTriviaSyntax result = null;
            do
            {
                // TODO try getting overriden method first (simple override or override of abstract method)
                method = method.OverriddenMethod ?? MiscHelper.FindImplementedMethod(method);
                if (method == null)
                {
                    return null;
                }
                SyntaxNode implementedMethodDeclaration = method.DeclaringSyntaxReferences.First().GetSyntax();

                result = GetDocumentCommentTrivia(implementedMethodDeclaration);
            } while (GetXmlNodeSyntaxes(result, "inheritdoc").Any());

            return result;
        }

        public static IEnumerable<XmlNodeSyntax> GetXmlNodeSyntaxes(DocumentationCommentTriviaSyntax documentationCommentTriviaSyntax,
            string nodeName = null)
        {
            return documentationCommentTriviaSyntax.
                ChildNodes().
                Cast<XmlNodeSyntax>().
                Where(x =>
                {
                    if (nodeName == null)
                    {
                        return true;
                    }

                    if (x is XmlElementSyntax)
                    {
                        return (x as XmlElementSyntax).StartTag.Name.ToString().Equals(nodeName);
                    }
                    if (x is XmlEmptyElementSyntax)
                    {
                        return (x as XmlEmptyElementSyntax).Name.ToString().Equals(nodeName);
                    }

                    return false;
                });
        }
    }
}
