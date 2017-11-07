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

        public static FactoryInterfaceContext TryCreate(SemanticModel semanticModel, CompilationUnitSyntax compilationUnit)
        {
            InterfaceDeclarationSyntax interfaceDeclaration = compilationUnit.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
            if (interfaceDeclaration == null)
            {
                return null;
            }

            if (!FactoryHelper.IsFactoryType(interfaceDeclaration))
            {
                return null;
            }

            return new FactoryInterfaceContext(semanticModel, compilationUnit, interfaceDeclaration);
        }
    }
}
