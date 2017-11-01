using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class SymbolHelper
    {
        public static IEnumerable<ITypeSymbol> GetTypeSymbols(string className, INamespaceOrTypeSymbol namespaceSymbol)
        {
            List<ITypeSymbol> result = new List<ITypeSymbol>();
            TryGetTypeSymbols(className, namespaceSymbol, result);

            return result;
        }

        private static void TryGetTypeSymbols(string className, INamespaceOrTypeSymbol namespaceSymbol, List<ITypeSymbol> result)
        {
            if (namespaceSymbol is ITypeSymbol)
            {
                // TODO does not handle nested types
                if (namespaceSymbol.Name == className)
                {
                    result.Add(namespaceSymbol as ITypeSymbol);
                }
                return;
            }

            IEnumerable<INamespaceOrTypeSymbol> symbols = (namespaceSymbol as INamespaceSymbol).GetMembers();

            foreach (INamespaceOrTypeSymbol child in symbols)
            {
                TryGetTypeSymbols(className, child, result);
            }

            return;
        }
    }
}
