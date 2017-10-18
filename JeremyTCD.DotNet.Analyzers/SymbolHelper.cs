using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class SymbolHelper
    {
        // TODO does not handle case where there are multiple classes with the same non-qualified name
        public static ITypeSymbol TryGetTypeSymbol(string className, INamespaceOrTypeSymbol namespaceSymbol)
        {
            if (namespaceSymbol is ITypeSymbol)
            {
                // TODO does not handle nested types
                if (namespaceSymbol.Name == className)
                {
                    return namespaceSymbol as ITypeSymbol;
                }
                else
                {
                    return null;
                }
            }

            IEnumerable<INamespaceOrTypeSymbol> symbols = (namespaceSymbol as INamespaceSymbol).GetMembers();

            foreach (INamespaceOrTypeSymbol child in symbols)
            {
                ITypeSymbol result = TryGetTypeSymbol(className, child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
