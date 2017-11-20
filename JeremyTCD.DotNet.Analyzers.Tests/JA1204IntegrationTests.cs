using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1204IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1204FactoryProducableClass.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1204FactoryProducableClass());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1204FactoryProducableClass(), new JA1204CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForNonFactoryClass()
        {
           DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1204FactoryProducableClass.DiagnosticId,
                Message = string.Empty,
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("NonFactoryClass.cs", 3, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticForFactoryClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_CreatesFactoryClass()
        {
            _codeFixVerifier.VerifyFilesAdded(_sourcesHelper.GetSourcesFolder(), "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1204.CreatesTestClass");
        }
    }
}
