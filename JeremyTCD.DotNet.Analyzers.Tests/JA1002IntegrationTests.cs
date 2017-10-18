using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1002IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1002TestClassNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1002TestClassNamesMustBeCorrectlyFormatted());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfTestClassNamesAreCorrectlyFormatted()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWhenTestClassNameSuffixIsInvalid()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1002TestClassNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1002_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestInvalidSuffix.cs", 5, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWhenClassUnderTestIsInvalid()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1002TestClassNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1002_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("InvalidClassUnitTests.cs", 5, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }
    }
}
