﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public class FactoryClassContext
    {
        private INamedTypeSymbol _classSymbol;
        private bool _classSymbolGetAttempted;
        public INamedTypeSymbol ClassSymbol
        {
            get
            {
                if (_classSymbolGetAttempted)
                {
                    return _classSymbol;
                }

                _classSymbolGetAttempted = true;
                return _classSymbol = SemanticModel.GetDeclaredSymbol(ClassDeclaration) as INamedTypeSymbol;
            }
        }

        private INamedTypeSymbol _producedClass;
        private bool _producedClassGetAttempted;
        public INamedTypeSymbol ProducedClass
        {
            get
            {
                if (!_producedClassGetAttempted)
                {
                    _producedClassGetAttempted = true;
                    return _producedClass = FactoryHelper.GetProducedClass(ClassDeclaration, Compilation.GlobalNamespace) as INamedTypeSymbol;
                }

                return _producedClass;
            }
        }

        private FactoryInterfaceContext _factoryInterfaceContext;
        private bool _factoryInterfaceContextGetAttempted;
        public FactoryInterfaceContext FactoryInterfaceContext
        {
            get
            {
                if (_factoryInterfaceContextGetAttempted)
                {
                    return _factoryInterfaceContext;
                }

                _factoryInterfaceContextGetAttempted = true;
                INamedTypeSymbol factoryInterface = ClassSymbol.Interfaces.Where(i => FactoryHelper.IsFactoryType(i.Name)).FirstOrDefault();
                if(factoryInterface == null)
                {
                    return null;
                }

                return  _factoryInterfaceContext = FactoryInterfaceContextFactory.TryCreate(SemanticModel, factoryInterface);
            }
        }

        public CompilationUnitSyntax CompilationUnit { get; }
        public Compilation Compilation { get; }
        public SemanticModel SemanticModel { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }

        public FactoryClassContext(SemanticModel semanticModel,
            CompilationUnitSyntax compilationUnit,
            ClassDeclarationSyntax classDeclaration)
        {
            SemanticModel = semanticModel;
            CompilationUnit = compilationUnit;
            ClassDeclaration = classDeclaration;
            Compilation = semanticModel.Compilation;
        }
    }
}