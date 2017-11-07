using Microsoft.CodeAnalysis;
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
                return _classSymbolGetAttempted ? _classSymbol : (_classSymbol = SemanticModel.GetDeclaredSymbol(ClassDeclaration) as INamedTypeSymbol);
            }
        }

        private INamedTypeSymbol _factoryInterface;
        private bool _factoryInterfaceGetAttempted;
        public INamedTypeSymbol FactoryInterface
        {
            get
            {
                if(ClassSymbol == null)
                {
                    return null;
                }

                return _factoryInterfaceGetAttempted ? 
                    _factoryInterface : (_factoryInterface = ClassSymbol.Interfaces.Where(i => FactoryHelper.IsFactoryType(i)).FirstOrDefault());
            }
        }

        //private INamedTypeSymbol _producedInterface;
        //private bool _producedInterfaceGetAttempted;
        //public INamedTypeSymbol ProducedInterface
        //{
        //    get
        //    {
        //        return _producedInterfaceGetAttempted ? _producedInterface :
        //            (_producedInterface = FactoryHelper.GetProducedInterface(ClassDeclaration, Compilation.GlobalNamespace) as INamedTypeSymbol);
        //    }
        //}

        //private INamedTypeSymbol _factoryInterface;
        //private bool _factoryInterfaceGetAttempted;
        //public INamedTypeSymbol FactoryInterface
        //{
        //    get
        //    {
        //        return _factoryInterfaceGetAttempted ? _factoryInterface :
        //            (_factoryInterface = FactoryHelper.GetFactoryInterface());
        //    }
        //}

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
