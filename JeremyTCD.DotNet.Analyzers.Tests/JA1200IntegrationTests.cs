using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1200IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1200FactoryClassNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1200FactoryClassNamesMustBeCorrectlyFormatted());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfCompilationDoesNotContainAFactoryClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassDoesNotImplementAFactoryInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassNameIsCorrectlyFormatted()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfImplementedFactoryInterfaceDoesNotHaveAProducedInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfProducedClassDoesNotExist()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1200FactoryClassNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1200_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("NonExistentClassFactory.cs", 3, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfProducedClassDoesNotImplementProducedInterface()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1200FactoryClassNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1200_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ProducedClassFactory.cs", 3, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

    }
}
