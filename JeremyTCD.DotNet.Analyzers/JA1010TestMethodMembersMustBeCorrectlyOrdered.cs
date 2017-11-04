using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1010TestMethodMembersMustBeCorrectlyOrdered : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1010TestMethodMembersMustBeCorrectlyOrdered).Substring(0, 6);
        public const string InterfaceIdentifierProperty = "InterfaceIdentifierProperty";
        public const string VariableIdentifierProperty = "VariableIdentifierProeprty";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId,
                Strings.JA1010_Title,
                Strings.JA1010_MessageFormat,
                Strings.CategoryName_Testing,
                DiagnosticSeverity.Warning,
                true,
                Strings.JA1010_Description,
                "");

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
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;
            SemanticModel testClassSemanticModel = context.SemanticModel;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Get test class declaration
            ClassDeclarationSyntax testClassDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (testClassDeclaration == null)
            {
                return;
            }

            // Get class under test declaration
            ITypeSymbol classUnderTest = TestingHelper.GetClassUnderTest(testClassDeclaration, testClassSemanticModel.Compilation.GlobalNamespace);
            if (classUnderTest == null)
            {
                return;
            }
            ClassDeclarationSyntax classUnderTestDeclaration = classUnderTest.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;
            if (classUnderTestDeclaration == null)
            {
                return;
            }
            SemanticModel classUnderTestSemanticModel = context.Compilation.GetSemanticModel(classUnderTestDeclaration.SyntaxTree);

            // Get correct order
            IEnumerable<SyntaxNode> orderedTestClassMembers = TestingHelper.OrderTestClassMembers(
                testClassDeclaration, 
                classUnderTestDeclaration,
                testClassSemanticModel, 
                classUnderTest, 
                classUnderTestSemanticModel);

            // Create diagnostic
            if (!orderedTestClassMembers.SequenceEqual(testClassDeclaration.ChildNodes()))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, testClassDeclaration.Identifier.GetLocation()));
            }
        }
    }
}
