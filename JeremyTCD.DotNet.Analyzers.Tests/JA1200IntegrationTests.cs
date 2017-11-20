using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1200IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1200FactoryClassNamesMustBeCorrectlyFormatted.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1200FactoryClassNamesMustBeCorrectlyFormatted());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1200FactoryClassNamesMustBeCorrectlyFormatted(), 
            new JA1200CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfFactoryClassNameIsIncorrectlyFormatted()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1200FactoryClassNamesMustBeCorrectlyFormatted.DiagnosticId,
                Message = Strings.JA1200_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ProducedClassFactory.cs", 3, 18) },
                Properties = new Dictionary<string, string>()
                {
                    { JA1200FactoryClassNamesMustBeCorrectlyFormatted.ExpectedClassNameProperty, "ProducedClassFactory" }
                }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfCompilationDoesNotContainAFactoryClass()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassDoesNotImplementAFactoryInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassHasNoCreateMethodThatProducesConcreteTypes()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassHasCreateMethodsThatProduceInterfaces()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassNameIsCorrectlyFormatted()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassHasACreateMethodThatProducesMultipleConcreteTypes()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfImplementedFactoryInterfaceDoesNotHaveAProducedInterface()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticIfFactoryClassHasCreateMethodsThatProduceDifferentConcreteTypes()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_RenamesFactoryClass()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
