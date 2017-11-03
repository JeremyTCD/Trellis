using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class MiscHelper
    {
        public static string FirstCharUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            char[] sChars = str.ToCharArray();
            sChars[0] = char.ToUpper(sChars[0]);

            return new string(sChars);
        }

        public static string RemoveNonAlphaNumericCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            char[] arr = str.ToCharArray();

            arr = Array.FindAll(arr, c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c));
            return new string(arr);
        }

        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var tokens = str.Split(new[] { " ", "-", "." }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                tokens[i] = token.FirstCharUpper();
            }

            return string.Join("", tokens);
        }

        public static IMethodSymbol FindImplementedMethod(IMethodSymbol methodSymbol)
        {
            foreach(INamedTypeSymbol implementedInterface in methodSymbol.ContainingType.Interfaces)
            {
                IEnumerable<IMethodSymbol> implementedMethods = implementedInterface.GetMembers().OfType<IMethodSymbol>();
                foreach (IMethodSymbol method in implementedMethods)
                {
                    if(methodSymbol.ContainingType.FindImplementationForInterfaceMember(method) == methodSymbol)
                    {
                        return method;
                    }
                }
            }

            return null;
        }
    }
}
