using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1010TestClassMembersMustBeCorrectlyOrdered : DiagnosticAnalyzer
    {
        public static string DiagnosticId = nameof(JA1010TestClassMembersMustBeCorrectlyOrdered).Substring(0, 6);
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
            TestClassContext testClassContext = TestClassContextFactory.TryCreate(context);
            if (testClassContext == null || testClassContext.ClassUnderTest == null)
            {
                return;
            }

            // TODO null when project is built 
            if (testClassContext.ClassUnderTestDeclaration == null)
            {
                return;
            }

            // Get class under test semantic model
            Compilation compilation = testClassContext.Compilation;
            if (!testClassContext.Compilation.ContainsSyntaxTree(testClassContext.ClassUnderTestDeclaration.SyntaxTree))
            {
                compilation = testClassContext.Compilation.AddSyntaxTrees(testClassContext.ClassUnderTestDeclaration.SyntaxTree);
            }
            SemanticModel classUnderTestSemanticModel = compilation.
                GetSemanticModel(testClassContext.ClassUnderTestDeclaration.SyntaxTree);

            // Get correct order
            IEnumerable<SyntaxNode> orderedTestClassMembers = TestingHelper.OrderTestClassMembers(
                testClassContext,
                classUnderTestSemanticModel);

            // Create diagnostic
            if (!orderedTestClassMembers.SequenceEqual(testClassContext.MemberDeclarations))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, testClassContext.ClassDeclaration.Identifier.GetLocation()));
            }
        }
    }
}
