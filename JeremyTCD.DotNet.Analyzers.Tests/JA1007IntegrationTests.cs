using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1007IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests(), new JA1007CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfExceptionOutcomesAreNotTested()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId,
                Message = Strings.JA1007_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty,
                        ";DoSomething_ThrowsExceptionOnEveryCall,Exception,DoSomething;" +
                        "DoSomething_ThrowsArgumentExceptionOnceInAwhile,ArgumentException,DoSomething;" +
                        "DoSomethingElse_ThrowsInvalidOperationExceptionOnceInABlueMoon,InvalidOperationException,DoSomethingElse"},
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty,
                        "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1007.CreatesDiagnosticIfExceptionOutcomesAreNotTested.Tests.ClassUnderTestUnitTests" },
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty,
                        "ClassUnderTest"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfExceptionOutcomesAreTestedByIncorrectlyNamedMethods()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId,
                Message = Strings.JA1007_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty,
                        ";DoSomething_InvalidDescription,DoSomething_ThrowsExceptionOnEveryCall,Exception,DoSomething"},
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty,
                        "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1007.CreatesDiagnosticIfExceptionOutcomesAreTestedByIncorrectlyNamedMethods.Tests.ClassUnderTestUnitTests" },
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty,
                        "ClassUnderTest"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticsIfExceptionOutcomesInheritedFromImplementationsAreNotTested()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId,
                Message = Strings.JA1007_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty,
                        ";DoSomething_ThrowsExceptionOnEveryCall,Exception,DoSomething"},
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty,
                        "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1007.CreatesDiagnosticsIfExceptionOutcomesInheritedFromImplementationsAreNotTested.Tests.ClassUnderTestUnitTests" },
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty,
                        "ClassUnderTest"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticsIfExceptionOutcomesInheritedFromAbstractClassesAreNotTested()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId,
                Message = Strings.JA1007_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty,
                        ";DoSomething_ThrowsExceptionOnEveryCall,Exception,DoSomething"},
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty,
                        "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1007.CreatesDiagnosticsIfExceptionOutcomesInheritedFromAbstractClassesAreNotTested.Tests.ClassUnderTestUnitTests" },
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty,
                        "ClassUnderTest"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticsIfExceptionOutcomesInheritedFromBaseClassesAreNotTested()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId,
                Message = Strings.JA1007_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty,
                        ";DoSomething_ThrowsExceptionOnEveryCall,Exception,DoSomething"},
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty,
                        "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1007.CreatesDiagnosticsIfExceptionOutcomesInheritedFromBaseClassesAreNotTested.Tests.ClassUnderTestUnitTests" },
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty,
                        "ClassUnderTest"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_NormalizesExceptionDescriptions()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.DiagnosticId,
                Message = Strings.JA1007_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.FixDataProperty,
                        ";DoSomething_ThrowsExceptionIfTestIntIs0Object,Exception,DoSomething"},
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.TestClassFullyQualifiedNameProperty,
                        "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1007.NormalizesExceptionDescriptions.Tests.ClassUnderTestUnitTests" },
                    { JA1007DocumentedExceptionOutcomesMustHaveMatchingUnitTests.ClassUnderTestNameProperty,
                        "ClassUnderTest"}
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfExceptionOutcomesAreTested()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_AddsTestMethodForExceptionOutcomeIfOneDoesNotAlreadyExist()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_RenamesTestMethodForExceptionOutcomeIfItIsIncorrectlyNamed()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_FixesMultipleExceptionOutcomeTestingIssuesAtOnce()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
