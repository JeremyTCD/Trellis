using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1201IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfInterfaceIsNotAFactoryInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryInterfaceNameBeginsWithAnExistingInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFactoryInterfaceNameDoesNotBeginWithAnExistingInterface()
         {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1201_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ExistingTypeFactory.cs", 3, 22) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFactoryInterfaceNameDoesNotBeginWithAnExistingType()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1201FactoryInterfaceNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1201_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("INonExistentTypeFactory.cs", 3, 22) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }
    }
}
