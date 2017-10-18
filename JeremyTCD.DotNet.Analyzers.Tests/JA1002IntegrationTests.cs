using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1002IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1002TestClassNamesMustEndWithAValidSuffix.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1002TestClassNamesMustEndWithAValidSuffix());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfTestClassNamesHaveValidSuffixes()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWhenAllConditionsAreSatisfied()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1002TestClassNamesMustEndWithAValidSuffix.DiagnosticId,
                Message = Strings.JA1002_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("TestInvalidSuffix.cs", 5, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }
    }
}
