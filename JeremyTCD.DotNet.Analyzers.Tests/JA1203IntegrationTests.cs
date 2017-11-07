using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1203IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1203FactoryClassMustImplementFactoryInterface.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1203FactoryClassMustImplementFactoryInterface());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassImplementsAFactoryInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFactoryClassDoesNotImplementAFactoryInterface()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1203FactoryClassMustImplementFactoryInterface.DiagnosticId,
                Message = Strings.JA1203_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("InvalidFactory.cs", 3, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }
    }
}
