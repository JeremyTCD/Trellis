using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1011IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1011TestableClass.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1011TestableClass());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1011TestableClass(), new JA1011CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForNonTestClass()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1011TestableClass.DiagnosticId,
                Message = string.Empty,
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test.cs", 5, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsForTestsClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_CreatesTestClass()
        {
            _codeFixVerifier.VerifyFilesAdded(_sourcesHelper.GetSourcesFolder(), "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1011.CreatesTestClass");
        }
    }
}
