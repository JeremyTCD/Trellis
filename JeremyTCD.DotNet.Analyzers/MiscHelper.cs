using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public static bool IsBuiltInType(QualifiedNameSyntax qualifiedName)
        {
            string fullyQualifiedName = qualifiedName.ToString();

            return fullyQualifiedName.Equals("global::System.Boolean") ||
                fullyQualifiedName.Equals("global::System.Byte") ||
                fullyQualifiedName.Equals("global::System.SByte") ||
                fullyQualifiedName.Equals("global::System.Char") ||
                fullyQualifiedName.Equals("global::System.Decimal") ||
                fullyQualifiedName.Equals("global::System.Double") ||
                fullyQualifiedName.Equals("global::System.Single") ||
                fullyQualifiedName.Equals("global::System.Int32") ||
                fullyQualifiedName.Equals("global::System.UInt32") ||
                fullyQualifiedName.Equals("global::System.Int64") ||
                fullyQualifiedName.Equals("global::System.UInt64") ||
                fullyQualifiedName.Equals("global::System.Object") ||
                fullyQualifiedName.Equals("global::System.Int16") ||
                fullyQualifiedName.Equals("global::System.UInt16") ||
                fullyQualifiedName.Equals("global::System.String");
        }

        public static bool IsBuiltInType(ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Boolean ||
                type.SpecialType == SpecialType.System_Byte ||
                type.SpecialType == SpecialType.System_SByte ||
                type.SpecialType == SpecialType.System_Char ||
                type.SpecialType == SpecialType.System_Decimal ||
                type.SpecialType == SpecialType.System_Double ||
                type.SpecialType == SpecialType.System_Single ||
                type.SpecialType == SpecialType.System_Int32 ||
                type.SpecialType == SpecialType.System_UInt32 ||
                type.SpecialType == SpecialType.System_Int64 ||
                type.SpecialType == SpecialType.System_UInt64 ||
                type.SpecialType == SpecialType.System_Object ||
                type.SpecialType == SpecialType.System_Int16 ||
                type.SpecialType == SpecialType.System_UInt16 ||
                type.SpecialType == SpecialType.System_String;
        }
    }
}
