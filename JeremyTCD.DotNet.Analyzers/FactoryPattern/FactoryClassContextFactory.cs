using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    public class FactoryClassContextFactory
    {
        public static async Task<FactoryClassContext> TryCreateAsync(Document document)
        {
            return TryCreate(await document.GetSemanticModelAsync().ConfigureAwait(false), (await document.GetSyntaxRootAsync().ConfigureAwait(false)) as CompilationUnitSyntax);
        }

        public static FactoryClassContext TryCreate(SyntaxNodeAnalysisContext context)
        {
            CompilationUnitSyntax compilationUnit = context.Node as CompilationUnitSyntax;
            if(compilationUnit == null)
            {
                return null;
            }

            return TryCreate(context.SemanticModel, compilationUnit);
        }

        public static FactoryClassContext TryCreate(SemanticModel semanticModel, CompilationUnitSyntax compilationUnit)
        {
            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
            {
                return null;
            }

            if (!FactoryHelper.IsFactoryType(classDeclaration))
            {
                return null;
            }

            return new FactoryClassContext(semanticModel, compilationUnit, classDeclaration);
        }
    }
}
