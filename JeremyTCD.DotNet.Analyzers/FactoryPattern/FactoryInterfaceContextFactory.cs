using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    public class FactoryInterfaceContextFactory
    {
        public static async Task<FactoryInterfaceContext> TryCreateAsync(Document document)
        {
            return TryCreate(await document.GetSemanticModelAsync().ConfigureAwait(false), (await document.GetSyntaxRootAsync().ConfigureAwait(false)) as CompilationUnitSyntax);
        }

        public static FactoryInterfaceContext TryCreate(SyntaxNodeAnalysisContext context)
        {
            CompilationUnitSyntax compilationUnit = context.Node as CompilationUnitSyntax;
            if(compilationUnit == null)
            {
                return null;
            }

            return TryCreate(context.SemanticModel, compilationUnit);
        }

        public static FactoryInterfaceContext TryCreate(SemanticModel semanticModel, INamedTypeSymbol interfaceSymbol)
        {
            CompilationUnitSyntax compilationUnit = interfaceSymbol.
                DeclaringSyntaxReferences.
                FirstOrDefault()?.
                GetSyntax().
                SyntaxTree.
                GetRoot() as CompilationUnitSyntax;
            if (compilationUnit == null)
            {
                return null;
            }

            return TryCreate(semanticModel, compilationUnit, interfaceSymbol);
        }

        public static FactoryInterfaceContext TryCreate(SemanticModel semanticModel, CompilationUnitSyntax compilationUnit, INamedTypeSymbol interfaceSymbol = null)
        {
            InterfaceDeclarationSyntax interfaceDeclaration = interfaceSymbol == null ?
                compilationUnit.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault() :
                interfaceSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as InterfaceDeclarationSyntax;
            if (interfaceDeclaration == null)
            {
                return null;
            }

            if (!FactoryHelper.IsFactoryType(interfaceDeclaration))
            {
                return null;
            }

            return new FactoryInterfaceContext(semanticModel, compilationUnit, interfaceDeclaration, interfaceSymbol);
        }
    }
}
