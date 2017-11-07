using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JeremyTCD.DotNet.Analyzers
{
    public class TestClassContext
    {
        private string _className;
        public string ClassName
        {
            get
            {
                return _className ?? (_className = ClassDeclaration.Identifier.ValueText);
            }
        }

        private string _classUnderTestName;
        public string ClassUnderTestName
        {
            get
            {
                if (ClassUnderTest == null)
                {
                    return null;
                }

                return _classUnderTestName ?? (_classUnderTestName = ClassUnderTest.Name.ToString());
            }
        }

        private INamedTypeSymbol _classUnderTest;
        private bool _classUnderTestGetAttempted;
        public INamedTypeSymbol ClassUnderTest
        {
            get
            {
                if (!_classUnderTestGetAttempted)
                {
                    _classUnderTestGetAttempted = true;
                    return _classUnderTest = TestingHelper.GetClassUnderTest(ClassDeclaration, Compilation.GlobalNamespace) as INamedTypeSymbol;
                }

                return _classUnderTest;
            }
        }

        private ClassDeclarationSyntax _classUnderTestDeclaration;
        public ClassDeclarationSyntax ClassUnderTestDeclaration
        {
            get
            {
                if(ClassUnderTest == null)
                {
                    return null;
                }

                // TODO DeclaringSyntaxReferences returns an empty IEnumerable when project is build using msbuild
                return _classUnderTestDeclaration ?? 
                    (_classUnderTestDeclaration = ClassUnderTest.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax);
            }
        }

        private IMethodSymbol _classUnderTestMainConstructor;
        private bool _classUnderTestMainConstructorGetAttempted;
        public IMethodSymbol ClassUnderTestMainConstructor
        {
            get
            {
                if (ClassUnderTest == null)
                {
                    return null;
                }

                if (!_classUnderTestMainConstructorGetAttempted)
                {
                    _classUnderTestMainConstructorGetAttempted = true;
                    return _classUnderTestMainConstructor = ClassUnderTest.Constructors.OrderByDescending(c => c.Parameters.Count()).First();
                }

                return _classUnderTestMainConstructor;
            }
        }

        private IEnumerable<IMethodSymbol> _classUnderTestMethods;
        public IEnumerable<IMethodSymbol> ClassUnderTestMethods
        {
            get
            {
                if (ClassUnderTest == null)
                {
                    return null;
                }

                return _classUnderTestMethods ?? (_classUnderTestMethods = ClassUnderTest.GetMembers().OfType<IMethodSymbol>().
                    Where(m => m.MethodKind != MethodKind.Constructor));
            }
        }

        private IEnumerable<MethodDeclarationSyntax> _classUnderTestMethodDeclarations;
        public IEnumerable<MethodDeclarationSyntax> ClassUnderTestMethodDeclarations
        {
            get
            {
                if (ClassUnderTest == null)
                {
                    return null;
                }

                return _classUnderTestMethodDeclarations ?? 
                    (_classUnderTestMethodDeclarations = ClassUnderTestMethods.Select(m => m.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax));
            }
        }

        private IEnumerable<SyntaxNode> _descendantNodes;
        public IEnumerable<SyntaxNode> DescendantNodes
        {
            get
            {
                return _descendantNodes ?? (_descendantNodes = ClassDeclaration.DescendantNodes());
            }
        }

        private bool? _isUnitTestClass;
        public bool IsUnitTestClass
        {
            get
            {
                return _isUnitTestClass ?? ((_isUnitTestClass = TestingHelper.IsUnitTestClass(ClassDeclaration)) == true);
            }
        }

        private bool? _isIntegrationTestClass;
        public bool IsIntegrationTestClass
        {
            get
            {
                return _isIntegrationTestClass ?? ((_isIntegrationTestClass = TestingHelper.IsIntegrationTestClass(ClassDeclaration)) == true);
            }
        }

        private bool? _isEndToEndTestClass;
        public bool IsEndToEndTestClass
        {
            get
            {
                return _isEndToEndTestClass ?? ((_isEndToEndTestClass = TestingHelper.IsEndToEndTestClass(ClassDeclaration)) == true);
            }
        }

        private IEnumerable<SyntaxNode> _memberDeclarations;
        public IEnumerable<SyntaxNode> MemberDeclarations
        {
            get
            {
                if (_memberDeclarations != null)
                {
                    return _memberDeclarations;
                }

                return _memberDeclarations = ClassDeclaration.ChildNodes();
            }
        }

        private IEnumerable<IMethodSymbol> _methods;
        public IEnumerable<IMethodSymbol> Methods
        {
            get
            {
                if (_methods != null)
                {
                    return _methods;
                }

                return _methods = MethodsAndTheirDeclarations.
                    Select(tuple => tuple.Item1);
            }
        }

        private IEnumerable<(IMethodSymbol, MethodDeclarationSyntax)> _methodsAndTheirDeclarations;
        public IEnumerable<(IMethodSymbol, MethodDeclarationSyntax)> MethodsAndTheirDeclarations
        {
            get
            {
                if (_methodsAndTheirDeclarations != null)
                {
                    return _methodsAndTheirDeclarations;
                }

                return _methodsAndTheirDeclarations = GetDescendantNodes<MethodDeclarationSyntax>().
                        Select(m => (SemanticModel.GetDeclaredSymbol(m) as IMethodSymbol, m));
            }
        }

        private IEnumerable<IMethodSymbol> _testMethods;
        public IEnumerable<IMethodSymbol> TestMethods
        {
            get
            {
                if (_testMethods != null)
                {
                    return _testMethods;
                }

                return _testMethods = TestMethodsAndTheirDeclarations.
                    Select(tuple => tuple.Item1);
            }
        }

        private IEnumerable<MethodDeclarationSyntax> _testMethodDeclarations;
        public IEnumerable<MethodDeclarationSyntax> TestMethodDeclarations
        {
            get
            {
                if (_testMethodDeclarations != null)
                {
                    return _testMethodDeclarations;
                }

                return _testMethodDeclarations = TestMethodsAndTheirDeclarations.
                    Select(tuple => tuple.Item2);
            }
        }

        private IEnumerable<(IMethodSymbol, MethodDeclarationSyntax)> _testMethodsAndTheirDeclarations;
        public IEnumerable<(IMethodSymbol, MethodDeclarationSyntax)> TestMethodsAndTheirDeclarations
        {
            get
            {
                if (_testMethodsAndTheirDeclarations != null)
                {
                    return _testMethodsAndTheirDeclarations;
                }

                return _testMethodsAndTheirDeclarations = GetDescendantNodes<MethodDeclarationSyntax>().
                        Select(m => (SemanticModel.GetDeclaredSymbol(m) as IMethodSymbol, m)).
                        Where(tuple => tuple.Item1 != null && TestingHelper.IsTestMethod(tuple.Item1));
            }
        }

        private IEnumerable<MethodDeclarationSyntax> _methodDeclarations;
        public IEnumerable<MethodDeclarationSyntax> MethodDeclarations
        {
            get
            {
                if (_methodDeclarations != null)
                {
                    return _methodDeclarations;
                }

                return _methodDeclarations = MethodsAndTheirDeclarations.
                    Select(tuple => tuple.Item2);
            }
        }

        private IDictionary<MethodDeclarationSyntax, MethodDeclarationSyntax> _TestMethodDeclarationsAndTheirDataMethodDeclarations;
        public IDictionary<MethodDeclarationSyntax, MethodDeclarationSyntax> TestMethodDeclarationsAndTheirDataMethodDeclarations
        {
            get
            {
                if (_TestMethodDeclarationsAndTheirDataMethodDeclarations != null)
                {
                    return _TestMethodDeclarationsAndTheirDataMethodDeclarations;
                }

                ITypeSymbol memberDataAttribute = Compilation.GetTypeByMetadataName("Xunit.MemberDataAttribute");

                return _TestMethodDeclarationsAndTheirDataMethodDeclarations = TestMethodDeclarations.
                    Select(m => (m, TestingHelper.GetDataMethodDeclaration(m, this, SemanticModel))).
                    Where(tuple => tuple.Item2 != null).
                    ToDictionary(
                       tuple => tuple.Item1,
                       tuple => tuple.Item2);
            }
        }

        private IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> _testMethodDeclarationsAndTheirMethodUnderTests;
        public IEnumerable<(MethodDeclarationSyntax, IMethodSymbol)> TestMethodDeclarationsAndTheirMethodUnderTests
        {
            get
            {
                if (ClassUnderTest == null)
                {
                    return null;
                }

                if (_testMethodDeclarationsAndTheirMethodUnderTests != null)
                {
                    return _testMethodDeclarationsAndTheirMethodUnderTests;
                }

                return _testMethodDeclarationsAndTheirMethodUnderTests = TestMethodsAndTheirDeclarations.
                    Select(tuple => (tuple.Item2, TestingHelper.GetMethodUnderTest(tuple.Item2, ClassUnderTest, SemanticModel)));
            }
        }

        private VariableDeclarationSyntax _mockRepositoryFieldDeclaration;
        private bool _mockRepositoryFieldDeclarationGetAttempted;
        public VariableDeclarationSyntax MockRepositoryVariableDeclaration
        {
            get
            {
                if (!_mockRepositoryFieldDeclarationGetAttempted && _mockRepositoryFieldDeclaration == null)
                {
                    _mockRepositoryFieldDeclarationGetAttempted = true;
                    return _mockRepositoryFieldDeclaration = TestingHelper.GetMockRepositoryVariableDeclaration(CompilationUnit, SemanticModel);
                }

                return _mockRepositoryFieldDeclaration;
            }
        }

        private INamedTypeSymbol _classSymbol;
        private bool _classSymbolGetAttempted;
        public INamedTypeSymbol ClassSymbol
        {
            get
            {
                return _classSymbolGetAttempted ? _classSymbol : (_classSymbol = SemanticModel.GetDeclaredSymbol(ClassDeclaration) as INamedTypeSymbol);
            }
        }

        public CompilationUnitSyntax CompilationUnit { get; }
        public Compilation Compilation { get; }
        public SemanticModel SemanticModel { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }

        public TestClassContext(SemanticModel semanticModel,
            CompilationUnitSyntax compilationUnit,
            ClassDeclarationSyntax classDeclaration)
        {
            SemanticModel = semanticModel;
            CompilationUnit = compilationUnit;
            ClassDeclaration = classDeclaration;
            Compilation = semanticModel.Compilation;
        }

        public IEnumerable<T> GetDescendantNodes<T>() where T : SyntaxNode
        {
            return DescendantNodes.OfType<T>();
        }

        public List<MethodDeclarationSyntax> GetMethodUnderTestTestMethodDeclarations(IMethodSymbol methodUnderTest)
        {
            return TestMethodDeclarationsAndTheirMethodUnderTests.
                Where(tuple => tuple.Item2 == methodUnderTest).
                Select(tuple => tuple.Item1).
                ToList();
        }
    }
}
