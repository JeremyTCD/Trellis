using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1012IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted(), 
            new JA1012CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForTestSubjectsIfTheirNamesDoNotEndWithTestSubject()
        {
            DiagnosticResult firstDiagnostic = new DiagnosticResult
            {
                Id = JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1012_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 28) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty, "invalidNameTestSubject"},
                    }
            };

            DiagnosticResult secondDiagnostic = new DiagnosticResult
            {
                Id = JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1012_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 12, 34) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty, "testSubjectTestSubject"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), firstDiagnostic, secondDiagnostic);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForLoneClassUnderTestInstanceIfItsNameIsNotTestSubject()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1012_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 28) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty, "testSubject"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForLoneMockOfClassUnderTestIfItsNameIsNotTestSubject()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1012_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 34) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1012TestMethodTestSubjectLocalVariableNamesMustBeCorrectlyFormatted.CorrectVariableNameProperty, "testSubject"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfLoneClassUnderTestInstanceIsCorrectlyNamed()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfLoneMockOfClassUnderTestInstanceIsCorrectlyNamed()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesNameOfClassUnderTestWithTestSubject()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesNameOfMockClassUnderTestWithTestSubject()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
