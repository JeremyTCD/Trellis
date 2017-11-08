using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1013IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed(), 
            new JA1013CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForLoneResultVariableIfItsNameIsNotResult()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.DiagnosticId,
                Message = Strings.JA1013_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 12, 17) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.CorrectVariableNameProperty, "result"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForResultVariablesIfTheirNamesDoNotEndWithResult()
        {
            DiagnosticResult firstDiagnostic = new DiagnosticResult
            {
                Id = JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.DiagnosticId,
                Message = Strings.JA1013_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 12, 17) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.CorrectVariableNameProperty, "invalidNameResult"},
                    }
            };

            DiagnosticResult secondDiagnostic = new DiagnosticResult
            {
                Id = JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.DiagnosticId,
                Message = Strings.JA1013_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 13, 17) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1013TestMethodResultLocalVariableNamesMustBeCorrectlyNamed.CorrectVariableNameProperty, "resultResult"},
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), firstDiagnostic, secondDiagnostic);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfResultVariableIsCorrectlyNamed()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesNameOfResultVariableWithResult()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
