using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1001IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1001TestClassNamespacesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1001TestClassNamespacesMustBeCorrectlyFormatted());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1001TestClassNamespacesMustBeCorrectlyFormatted(), new JA1001CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWhenAllConditionsAreSatisfied()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1001TestClassNamespacesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1001_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 3, 11) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1001TestClassNamespacesMustBeCorrectlyFormatted.CorrectNamespaceProperty, "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1001.CreatesDiagnosticWhenAllConditionsAreSatisfied.Tests"},
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWithNoCodeFixPropertyIfClassUnderTestIsUnknown()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1001TestClassNamespacesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1001_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 3, 11) },
                Properties = new Dictionary<string, string>()
                {
                    { Constants.NoCodeFix, null }
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_UpdatesNamespace()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
