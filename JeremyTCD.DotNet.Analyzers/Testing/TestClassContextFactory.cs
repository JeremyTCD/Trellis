using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    public class TestClassContextFactory
    {
        public static async Task<TestClassContext> TryCreateAsync(Document document)
        {
            return TryCreate(await document.GetSemanticModelAsync().ConfigureAwait(false), (await document.GetSyntaxRootAsync().ConfigureAwait(false)) as CompilationUnitSyntax);
        }

        public static TestClassContext TryCreate(SyntaxNodeAnalysisContext context)
        {
            CompilationUnitSyntax compilationUnit = context.Node as CompilationUnitSyntax;
            if(compilationUnit == null)
            {
                return null;
            }

            return TryCreate(context.SemanticModel, compilationUnit);
        }

        public static TestClassContext TryCreate(SemanticModel semanticModel, CompilationUnitSyntax compilationUnit)
        {
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return null;
            }

            ClassDeclarationSyntax classDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            if(classDeclaration == null)
            {
                return null;
            }

            return new TestClassContext(semanticModel, compilationUnit, classDeclaration);
        }
    }
}
