using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1010IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1010TestClassMembersMustBeCorrectlyOrdered());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1010TestClassMembersMustBeCorrectlyOrdered(), new JA1010CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFieldComesAfterConstructor()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFieldComesAfterMethod()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFieldComesAfterNestedClass()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfMethodComesAfterNestedClass()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodComesAfterHelperMethod()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfConstructorComesAfterMethod()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfConstructorComesAfterNestedClass()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfTestMethodsAreIncorrectlyOrdered()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 6, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfDataMethodDoesNotComeAfterTestMethod()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1010TestClassMembersMustBeCorrectlyOrdered.DiagnosticId,
                Message = Strings.JA1010_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 7, 18) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfMembersAreCorrectlyOrdered()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReordersMembers()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
