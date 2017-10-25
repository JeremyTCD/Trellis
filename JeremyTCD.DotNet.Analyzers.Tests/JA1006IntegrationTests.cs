using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1006IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1006TestDataMethodNamesMustBeCorrectlyFormatted());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1006TestDataMethodNamesMustBeCorrectlyFormatted(), new JA1006CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfDataMethodNameDoesNotBeginWithAssociatedTestMethodName()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1006_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Tests.cs", 14, 45) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DataMethodNameProperty, "TestMethod_Data"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfDataMethodNameDoesNotEndWithUnderscoreData()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1006_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Tests.cs", 14, 45) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1006TestDataMethodNamesMustBeCorrectlyFormatted.DataMethodNameProperty, "TestMethod_Data"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfDataMethodNameIsCorrectlyFormatted()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_RenamesDataMethodAndItsReferences()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
