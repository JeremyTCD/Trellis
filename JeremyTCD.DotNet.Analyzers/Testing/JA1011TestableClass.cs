using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1011TestableClass : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1011TestableClass).Substring(0, 6);

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                string.Empty,
                string.Empty,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Hidden,
                true,
                string.Empty,
                string.Empty);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.CompilationUnit);
            context.EnableConcurrentExecution();
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            TestClassContext testClassContext = TestClassContextFactory.TryCreate(context);
            if (testClassContext != null)
            {
                return;
            }

            ClassDeclarationSyntax classDeclaration = context.Node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if(classDeclaration == null)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation()));
        }
    }
}
