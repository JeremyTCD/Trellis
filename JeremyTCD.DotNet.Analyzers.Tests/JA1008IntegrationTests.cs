using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1008IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup(), new JA1008CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateADiagnosticIfTestMethodCallsSetupTAndVerifyAll()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateADiagnosticIfTestMethodCallsSetupAndVerifyAll()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodCallsSetupTButDoesNotCallVerifyAll()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup.DiagnosticId,
                Message = Strings.JA1008_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 21) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodCallsSetupButDoesNotCallVerifyAll()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1008TestMethodMustCallMockRepositoryVerifyAllIfItCallsMockSetup.DiagnosticId,
                Message = Strings.JA1008_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 21) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_AddsMockRepositoryVerifyAll()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
