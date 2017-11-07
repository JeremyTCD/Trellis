using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1204FactoryClassCreateMethodsMustReturnProducedTypeIntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1204FactoryClassCreateMethodsMustReturnProducedType.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1204FactoryClassCreateMethodsMustReturnProducedType());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassDoesNotImplementAFactoryInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfCompilationDoesNotContainAFactoryClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfCreateMethodsReturnProducedClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfACreateMethodDoesNotReturnTheProducedClass()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1204FactoryClassCreateMethodsMustReturnProducedType.DiagnosticId,
                Message = Strings.JA1204_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ProducedClassFactory.cs", 5, 35) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }
    }
}
