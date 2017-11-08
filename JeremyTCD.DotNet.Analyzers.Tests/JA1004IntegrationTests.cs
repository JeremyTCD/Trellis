using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1004IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted(), new JA1004CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForMockWithBehaviourIfItsNameDoesNotStartWithMock()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = string.Format(Strings.JA1004_MessageFormat, "classWithBehaviour"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 39) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty, "mockClassWithBehaviour"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForMockWithoutBehaviourIfItsNameDoesStartWithDummy()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = string.Format(Strings.JA1004_MessageFormat, "classWithNoBehaviour"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 41) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1004TestMethodLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty, "dummyClassWithNoBehaviour"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_AppendsMockToNameOfClassWithBehaviour()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesDummyWithMockInNameOfClassWithBehaviour()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_AppendsDummyToNameOfClassWithNoBehaviour()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesMockWithDummyInNameOfClassWithNoBehaviour()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesReferencesToVariableWithoutAlteringTrivia()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
