using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1202IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod(), new JA1202CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfInterfaceIsNotAFactoryInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryInterfaceHasAValidCreateMethod()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFactoryInterfaceHasNoCreateMethods()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod.DiagnosticId,
                Message = Strings.JA1202_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("IProducedInterfaceFactory.cs", 3, 22) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFactoryInterfaceOnlyHasInvalidCreateMethods()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1202FactoryInterfaceMustHaveAtLeastOneValidCreateMethod.DiagnosticId,
                Message = Strings.JA1202_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("IProducedInterfaceFactory.cs", 3, 22) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_AddsCreateMethodIfNoExitingMethodReturnsTheProducedInterface()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder(), $"{nameof(JA1202CodeFixProvider)}CreateMethod");
        }

        [Fact]
        public void CodeFixProvider_RenamesMethodsThatReturnTheProducedIntefacesToCreate()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder(), $"{nameof(JA1202CodeFixProvider)}RenameMethods");
        }
    }
}
