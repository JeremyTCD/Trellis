using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1100IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1100PublicPropertiesAndMethodsMustBeVirtual.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1100PublicPropertiesAndMethodsMustBeVirtual());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1100PublicPropertiesAndMethodsMustBeVirtual(), new JA1100CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForAbstractMembers()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForDeclarationsWithExplicitInterfaceSpecifier()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForDesignerFiles()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForInterfaceMembers()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForNonPublicDeclarations()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForOverrideMembers()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForStaticMembers()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForTestClassMembers()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForVirtualMembers()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForMethodDeclarationWhenAllConditionsAreSatisfied()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1100PublicPropertiesAndMethodsMustBeVirtual.DiagnosticId,
                Message = Strings.JA1100_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("TestClass.cs", 5, 21) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForPropertyDeclarationWhenAllConditionsAreSatisfied()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1100PublicPropertiesAndMethodsMustBeVirtual.DiagnosticId,
                Message = Strings.JA1100_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("TestClass.cs", 5, 23) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_AddsVirtualModifierToMethodDeclaration()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_AddsVirtualModifierToPropertyDeclaration()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
