using Microsoft.CodeAnalysis;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1009IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor(), new JA1009CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfNewIsUsedToInstantiateAMock()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor.DiagnosticId,
                Message = Strings.JA1009_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 48) }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void CodeFixProvider_ReplacesNewExpressionsWithMockRepositoryCreate()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
