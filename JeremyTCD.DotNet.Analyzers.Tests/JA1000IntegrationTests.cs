using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1000IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1000UnitTestMethodsMustUseInterfaceMocksForDummies());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1000UnitTestMethodsMustUseInterfaceMocksForDummies(), new JA1000CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCompilationDoesNotContainTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfTestClassIsNotUnitTestClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfClassUnderTestDoesNotExist()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticForDeclarationsThatAreNotInATestMethod()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticForClassUnderTestDeclarations()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticForMockDeclarations()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticForDeclarationsOfTypesThatOnlyImplementCoreFrameworkInterfaces()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWhenAllConditionsAreSatisfied()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.DiagnosticId,
                Message = Strings.JA1000_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 29) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.InterfaceIdentifierProperty, "IShouldBeMocked"},
                    { JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.VariableIdentifierProperty, "shouldBeMocked" }
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticWithNoCodeFixPropertyIfExpressionWithNonNullArgumentsIsUsed()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.DiagnosticId,
                Message = Strings.JA1000_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 29) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.InterfaceIdentifierProperty, "IShouldBeMocked"},
                    { JA1000UnitTestMethodsMustUseInterfaceMocksForDummies.VariableIdentifierProperty, "shouldBeMocked" },
                    { Constants.NoCodeFix, null }
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_AddsMoqUsingDirectiveMockRepositoryVariableUpdatesLocalVariable()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_IfMockRepositoryVariableAlreadyExistsUsesItInsteadOfAddingOne()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
