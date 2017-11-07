using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    public class FactoryInterfaceContext
    {
        private INamedTypeSymbol _interfaceSymbol;
        private bool _interfaceSymbolGetAttempted;
        public INamedTypeSymbol InterfaceSymbol
        {
            get
            {
                return _interfaceSymbolGetAttempted ? _interfaceSymbol : 
                    (_interfaceSymbol = SemanticModel.GetDeclaredSymbol(InterfaceDeclaration) as INamedTypeSymbol);
            }
        }

        private IEnumerable<IMethodSymbol> _methods;
        public IEnumerable<IMethodSymbol> Methods
        {
            get
            {
                if(InterfaceSymbol == null)
                {
                    return null;
                }

                return _methods ?? (InterfaceSymbol.GetMembers().OfType<IMethodSymbol>());
            }
        }

        private INamedTypeSymbol _producedInterface;
        private bool _producedInterfaceGetAttempted;
        public INamedTypeSymbol ProducedInterface
        {
            get
            {
                return _producedInterfaceGetAttempted ? _producedInterface :
                    (_producedInterface = FactoryHelper.GetProducedInterface(InterfaceDeclaration, Compilation.GlobalNamespace) as INamedTypeSymbol);
            }
        }

        private InterfaceDeclarationSyntax _producedInterfaceDeclaration;
        private bool _producedInterfaceDeclarationGetAttempted;
        public InterfaceDeclarationSyntax ProducedInterfaceDeclaration
        {
            get
            {
                return _producedInterfaceDeclarationGetAttempted ? _producedInterfaceDeclaration :
                    (_producedInterfaceDeclaration = ProducedInterface.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as InterfaceDeclarationSyntax);
            }
        }

        private IEnumerable<SyntaxNode> _descendantNodes;
        public IEnumerable<SyntaxNode> DescendantNodes
        {
            get
            {
                return _descendantNodes ?? (_descendantNodes = InterfaceDeclaration.DescendantNodes());
            }
        }

        private IEnumerable<MethodDeclarationSyntax> _methodDeclarations;
        public IEnumerable<MethodDeclarationSyntax> MethodDeclarations
        {
            get
            {
                return _methodDeclarations ?? (_methodDeclarations = GetDescendantNodes<MethodDeclarationSyntax>());
            }
        }

        private IEnumerable<IMethodSymbol> _createMethods;
        public IEnumerable<IMethodSymbol> CreateMethods
        {
            get
            {
                return _createMethods ?? (Methods.
                    Where(m => m.ReturnType == ProducedInterface && m.Name == "Create"));
            }
        }

        public CompilationUnitSyntax CompilationUnit { get; }
        public Compilation Compilation { get; }
        public SemanticModel SemanticModel { get; }
        public InterfaceDeclarationSyntax InterfaceDeclaration { get; }

        public FactoryInterfaceContext(SemanticModel semanticModel,
            CompilationUnitSyntax compilationUnit,
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            SemanticModel = semanticModel;
            CompilationUnit = compilationUnit;
            InterfaceDeclaration = interfaceDeclaration;
            Compilation = semanticModel.Compilation;
        }

        public IEnumerable<T> GetDescendantNodes<T>() where T : SyntaxNode
        {
            return DescendantNodes.OfType<T>();
        }
    }
}
