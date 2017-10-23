using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class JA1005IntegrationTests
    {
        private readonly SourcesHelper _sourcesHelper = new SourcesHelper(JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId);
        private readonly DiagnosticVerifier _diagnosticVerifier = new DiagnosticVerifier(new JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod());
        private readonly CodeFixVerifier _codeFixVerifier = new CodeFixVerifier(new JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod(), new JA1005CodeFixProvider());

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForMockTestSubjectsInstantiatedUsingMockRepositoryCreate()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 13, 48) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.InvocationInvalidProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateMockClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticForTestSubjectsInstantiatedInTestMethodsUsingAnInvocation.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForMockTestSubjectsInstantiatedUsingNew()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 11, 48) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.InvocationInvalidProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateMockClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticForMockTestSubjectsInstantiatedUsingNew.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticForTestSubjectsInstantiatedUsingNew()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.InvocationInvalidProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticForTestSubjectsInstantiatedInTestMethodsUsingNew.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodHasInvalidNumberOfParameters()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodHasInvalidNumberOfParameters.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodHasInvalidParameterNames()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodHasInvalidParameterNames.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodHasInvalidParameterOrder()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodHasInvalidParameterOrder.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodHasInvalidParameterTypes()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodHasInvalidParameterTypes.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodHasNonOptionalParameters()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodHasNonOptionalParameters.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodReturnExpressionHasInvalidArgumentOrder()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 13, 48) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateMockClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodReturnExpressionHasInvalidArgumentOrder.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodReturnExpressionHasInvalidArguments()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 10, 42) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodReturnExpressionHasInvalidArguments.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_CreatesDiagnosticIfCreateMethodReturnExpressionHasInvalidNumberOfArguments()
        {
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.DiagnosticId,
                Message = Strings.JA1005_MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("ClassUnderTestUnitTests.cs", 13, 48) },
                Properties = new Dictionary<string, string>()
                    {
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.NoValidCreateMethodProperty, null},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.CreateMethodNameProperty, "CreateMockClassUnderTest"},
                        { JA1005TestSubjectMustBeInstantiatedInAValidCreateMethod.ClassUnderTestFullyQualifiedNameProperty,
                            "JeremyTCD.DotNet.Analyzers.Tests.Sources.JA1005.CreatesDiagnosticIfCreateMethodReturnExpressionHasInvalidNumberOfArguments.ClassUnderTest" }
                    }
            };

            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder(), expected);
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCreateMethodWithValidInvocationExpressionIsUsed()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void DiagnosticAnalyzer_DoesNotCreateDiagnosticsIfCreateMethodWithValidNewReturnExpressionIsUsed()
        {
            _diagnosticVerifier.VerifyDiagnostics(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_AddsCreateMethodIfOneDoesNotAlreadyExist()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_AddsMockCreateMethodIfOneDoesNotAlreadyExist()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesCreateMethodIfAValidOneDoesNotAlreadyExist()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }

        [Fact]
        public void CodeFixProvider_ReplacesMockCreateMethodIfOneDoesNotAlreadyExist()
        {
            _codeFixVerifier.VerifyFix(_sourcesHelper.GetSourcesFolder());
        }
    }
}
