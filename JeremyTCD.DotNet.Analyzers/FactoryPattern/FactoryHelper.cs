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
            return IsFactoryType(typeDeclaration.Identifier.ValueText);
        }

        public static bool IsFactoryType(string typeName)
        {
            return typeName.EndsWith("Factory");
        }

        public static ITypeSymbol GetProducedClass(ClassDeclarationSyntax factoryClassDeclaration, INamespaceSymbol globalNamespace)
        {
            string factoryClassName = factoryClassDeclaration.Identifier.ValueText;
            int lastIndexOfFactory = factoryClassName.LastIndexOf("Factory");
            string producedClassName = factoryClassName.Substring(0, lastIndexOfFactory == -1 ? factoryClassName.Length : lastIndexOfFactory);

            if (factoryClassName == producedClassName)
            {
                return null;
            }

            IEnumerable<ITypeSymbol> types = SymbolHelper.GetTypeSymbols(producedClassName, globalNamespace);

            // More than one types with the same name (from different namespaces)
            if (types.Count() > 1)
            {
                string classNamespaceName = factoryClassDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();

                foreach (ITypeSymbol type in types)
                {
                    if (classNamespaceName.Equals(type.ContainingNamespace.ToString()))
                    {
                        return type;
                    }
                }
            }

            return types.FirstOrDefault();
        }

        public static ITypeSymbol GetProducedInterface(InterfaceDeclarationSyntax factoryInterfaceDeclaration, INamespaceSymbol globalNamespace)
        {
            string factoryName = factoryInterfaceDeclaration.Identifier.ValueText;
            int lastIndexOfFactory = factoryName.LastIndexOf("Factory");
            string producedInterfaceName = factoryName.Substring(0, lastIndexOfFactory == -1 ? factoryName.Length : lastIndexOfFactory);

            IEnumerable<ITypeSymbol> interfaceTypes = SymbolHelper.
                GetTypeSymbols(producedInterfaceName, globalNamespace).
                Where(t => t.TypeKind == TypeKind.Interface);
            int numInterfaceTypes = interfaceTypes.Count();

            if(numInterfaceTypes == 1)
            {
                return interfaceTypes.First();
            }
            if(numInterfaceTypes == 0)
            {
                return null;
            }

            // More than one types with the same name (from different namespaces)
            string factoryInterfaceNamespaceName = factoryInterfaceDeclaration.
                FirstAncestorOrSelf<NamespaceDeclarationSyntax>().
                Name.
                ToString();

            foreach (ITypeSymbol interfaceType in interfaceTypes)
            {
                if (factoryInterfaceNamespaceName.Equals(interfaceType.ContainingNamespace.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return interfaceType;
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