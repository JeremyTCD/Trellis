// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class FactoryHelper
    {
        public static bool IsFactoryType(TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.Identifier.ValueText.EndsWith("Factory");
        }

        public static IEnumerable<MethodDeclarationSyntax> GetCreateMethods(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.
                DescendantNodes().
                OfType<MethodDeclarationSyntax>().
                Where(m => m.Identifier.ValueText == "Create");
        }

        public static ITypeSymbol GetProducedType(TypeDeclarationSyntax typeDeclaration, INamespaceSymbol globalNamespace)
        {
            string factoryName = typeDeclaration.Identifier.ValueText;
            int lastIndexOfFactory = factoryName.LastIndexOf("Factory");
            string producedTypeName = factoryName.Substring(0, lastIndexOfFactory == -1 ? factoryName.Length : lastIndexOfFactory);

            IEnumerable<ITypeSymbol> types = SymbolHelper.GetTypeSymbols(producedTypeName, globalNamespace);

            // More than one types with the same name (from different namespaces)
            string producedTypeNamespaceName = typeDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();

            foreach (ITypeSymbol type in types)
            {
                if (producedTypeNamespaceName.Equals(type.ContainingNamespace.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }

            return null;
        }

        public static MethodDeclarationSyntax CreateCreateMethodDeclaration(InterfaceDeclarationSyntax producedInterfaceDeclaration, SyntaxGenerator syntaxGenerator)
        {
            return syntaxGenerator.
                MethodDeclaration("Create", returnType: SyntaxFactory.IdentifierName(producedInterfaceDeclaration.Identifier)) as MethodDeclarationSyntax;
        }
    }
}