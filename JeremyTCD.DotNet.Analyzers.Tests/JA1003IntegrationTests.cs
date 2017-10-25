using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1003IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1003TestMethodNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1003TestMethodNamesMustBeCorrectlyFormatted());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodNameBeginsWithUnderscore()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1003TestMethodNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1003_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 8, 21) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }


        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodNameDoesNotBeginWithNameOfMemberFromClassUnderTest()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1003TestMethodNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1003_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 8, 21) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodNameHasNoUnderscore()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1003TestMethodNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1003_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 8, 21) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfClassUnderTestDoesNotExist()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfTestMethodNamesAreCorrectlyFormatted()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }
    }
}
