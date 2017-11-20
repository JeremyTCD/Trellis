using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeremyTCD.DotNet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JA1204CodeFixProvider)), Shared]
    public class JA1204CodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(JA1204FactoryProducableClass.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document producedClassDocument = context.Document;
            SemanticModel producedClassSemanticModel = producedClassDocument.GetSemanticModelAsync().Result;
            CompilationUnitSyntax producedClassCompilationUnit = producedClassDocument.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            ClassDeclarationSyntax producedClassDeclaration = producedClassCompilationUnit.
                DescendantNodes().
                FirstOrDefault(s => s is ClassDeclarationSyntax) as ClassDeclarationSyntax;
            INamedTypeSymbol producedClass = producedClassSemanticModel.GetDeclaredSymbol(producedClassDeclaration) as INamedTypeSymbol;
            string producedInterfaceName = $"I{producedClass.Name}";
            string factoryInterfaceName = $"I{producedClass.Name}Factory";
            string factoryClassName = $"{producedClass.Name}Factory";

            // Find interface type
            INamedTypeSymbol producedInterface = SymbolHelper.
                GetTypeSymbols(producedInterfaceName, producedClass.ContainingNamespace).FirstOrDefault() as INamedTypeSymbol;

            // Find factory interface type
            INamedTypeSymbol factoryInterface = SymbolHelper.
                GetTypeSymbols(factoryInterfaceName, producedClass.ContainingNamespace).FirstOrDefault() as INamedTypeSymbol;

            // Find factory class type
            INamedTypeSymbol factoryClass = SymbolHelper.
                GetTypeSymbols(factoryClassName, producedClass.ContainingNamespace).FirstOrDefault() as INamedTypeSymbol;

            // Add code actions
            if (producedInterface == null || factoryInterface == null || factoryClass == null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                    Strings.JA1204_CodeFix_Title_CreateFactoryInfrastructure,
                    cancellationToken => CreateFactoryInfrastructure(
                        producedClassDocument,
                        producedClassSemanticModel,
                        producedClass,
                        producedClassDeclaration,
                        producedInterface,
                        factoryInterface,
                        factoryClass,
                        producedInterfaceName,
                        factoryInterfaceName,
                        factoryClassName,
                        cancellationToken),
                    nameof(JA1204CodeFixProvider)),
                    context.Diagnostics.First());
            }

            return Task.CompletedTask;
        }

        private static async Task<Solution> CreateFactoryInfrastructure(
            Document producedClassDocument,
            SemanticModel producedClassSemanticModel,
            INamedTypeSymbol producedClass,
            ClassDeclarationSyntax producedClassDeclaration,
            INamedTypeSymbol producedInterface,
            INamedTypeSymbol factoryInterfaceType,
            INamedTypeSymbol factoryClassType,
            string producedInterfaceName,
            string factoryInterfaceName,
            string factoryClassName,
            CancellationToken cancellationToken)
        {
            SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(producedClassDocument.Project);
            Project project = producedClassDocument.Project;
            Solution solution = project.Solution;

            // Create produced interface if necessary
            if (producedInterface == null)
            {
                DocumentId documentId = DocumentId.CreateNewId(project.Id);
                CompilationUnitSyntax syntaxRoot = CreateProducedInterfaceSyntaxRoot(
                    producedInterfaceName,
                    producedClass,
                    syntaxGenerator);
                solution = solution.AddDocument(documentId, producedInterfaceName, syntaxRoot);
            }

            // Create factory interface if necessary
            if(factoryInterfaceType == null)
            {
                DocumentId documentId = DocumentId.CreateNewId(project.Id);
                CompilationUnitSyntax syntaxRoot = CreateFactoryInterfaceSyntaxRoot(
                    factoryInterfaceName,
                    producedInterfaceName,
                    producedClass,
                    syntaxGenerator);
                solution = solution.AddDocument(documentId, factoryInterfaceName, syntaxRoot);
            }

            // Create factory class if necessary
            if(factoryClassType == null)
            {
                DocumentId documentId = DocumentId.CreateNewId(project.Id);
                CompilationUnitSyntax syntaxRoot = CreateFactoryClassSyntaxRoot(
                    factoryClassName,
                    factoryInterfaceName,
                    producedInterfaceName,
                    producedClass,
                    syntaxGenerator);
                solution = solution.AddDocument(documentId, factoryClassName, syntaxRoot);
            }

            return solution;
        }

        private static CompilationUnitSyntax CreateProducedInterfaceSyntaxRoot(
            string producedInterfaceName,
            INamedTypeSymbol producedClass,
            SyntaxGenerator syntaxGenerator)
        {
            // TODO does not handle generic type arguments
            HashSet<INamespaceSymbol> namespaces = new HashSet<INamespaceSymbol>();
            List<SyntaxNode> memberDeclarations = new List<SyntaxNode>();
            foreach (ISymbol member in producedClass.GetMembers())
            {
                // TODO handle IEventSymbols and indexers properly
                if (member is IPropertySymbol)
                {
                    IPropertySymbol property = member as IPropertySymbol;
                    SyntaxNode propertyDeclaration = syntaxGenerator.PropertyDeclaration(property);
                    propertyDeclaration = SyntaxHelper.SimplifyQualifiedNames(propertyDeclaration);
                    memberDeclarations.Add(propertyDeclaration);
                    if (!MiscHelper.IsBuiltInType(property.Type))
                    {
                        namespaces.Add(property.Type.ContainingNamespace);
                    }
                }
                else if (member is IMethodSymbol)
                {
                    IMethodSymbol method = member as IMethodSymbol;
                    if (method.MethodKind == MethodKind.Ordinary)
                    {
                        SyntaxNode methodDeclaration = syntaxGenerator.MethodDeclaration(method);
                        methodDeclaration = SyntaxHelper.SimplifyQualifiedNames(methodDeclaration);
                        memberDeclarations.Add(methodDeclaration);
                        if (!MiscHelper.IsBuiltInType(method.ReturnType))
                        {
                            namespaces.Add(method.ReturnType.ContainingNamespace);
                        }
                        foreach (IParameterSymbol parameterSymbol in method.Parameters)
                        {
                            if (!MiscHelper.IsBuiltInType(parameterSymbol.Type))
                            {
                                namespaces.Add(parameterSymbol.Type.ContainingNamespace);
                            }
                        }
                    }
                }
            }

            InterfaceDeclarationSyntax interfaceDeclaration = syntaxGenerator.
                InterfaceDeclaration(
                    producedInterfaceName,
                    accessibility: Accessibility.Public,
                    members: memberDeclarations) as InterfaceDeclarationSyntax;
            NamespaceDeclarationSyntax namespaceDeclaration = syntaxGenerator.
                NamespaceDeclaration(
                    producedClass.ContainingNamespace.ToDisplayString(),
                    interfaceDeclaration) as NamespaceDeclarationSyntax;

            List<SyntaxNode> nodes = TestingHelper.CreateMissingUsingDirectives(namespaces, interfaceDeclaration, namespaceDeclaration);
            nodes = SyntaxHelper.SortUsings(nodes);
            nodes.Add(namespaceDeclaration);

            return syntaxGenerator.CompilationUnit(nodes) as CompilationUnitSyntax;
        }

        private static CompilationUnitSyntax CreateFactoryInterfaceSyntaxRoot(
            string factoryInterfaceName,
            string producedInterfaceName,
            INamedTypeSymbol producedClass,
            SyntaxGenerator syntaxGenerator)
        {
            SyntaxNode returnType = syntaxGenerator.IdentifierName(producedInterfaceName);
            SyntaxNode methodDeclaration = syntaxGenerator.MethodDeclaration("Create", returnType: returnType);

            InterfaceDeclarationSyntax interfaceDeclaration = syntaxGenerator.
                InterfaceDeclaration(
                    factoryInterfaceName,
                    accessibility: Accessibility.Public,
                    members: new[] { methodDeclaration }) as InterfaceDeclarationSyntax;
            NamespaceDeclarationSyntax namespaceDeclaration = syntaxGenerator.
                NamespaceDeclaration(
                    producedClass.ContainingNamespace.ToDisplayString(),
                    interfaceDeclaration) as NamespaceDeclarationSyntax;

            return syntaxGenerator.CompilationUnit(namespaceDeclaration) as CompilationUnitSyntax;
        }

        private static CompilationUnitSyntax CreateFactoryClassSyntaxRoot(
            string factoryClassName,
            string factoryInterfaceName,    
            string producedInterfaceName,
            INamedTypeSymbol producedClass,
            SyntaxGenerator syntaxGenerator)
        {
            IMethodSymbol mainConstructor = producedClass.Constructors.OrderByDescending(c => c.Parameters.Count()).First();
            IEnumerable<SyntaxNode> argumentSyntaxes = mainConstructor.
                Parameters.
                Select(p =>
                {
                    SyntaxNode defaultExpression = syntaxGenerator.DefaultExpression(p.Type);
                    return syntaxGenerator.Argument(defaultExpression);
                });

            SyntaxNode objectCreation = syntaxGenerator.ObjectCreationExpression(producedClass, argumentSyntaxes);
            SyntaxNode returnStatement = syntaxGenerator.ReturnStatement(objectCreation);
            SyntaxNode returnType = syntaxGenerator.IdentifierName(producedInterfaceName);
            SyntaxNode methodDeclaration = syntaxGenerator.MethodDeclaration(
                "Create", 
                returnType: returnType, 
                accessibility: Accessibility.Public, 
                statements: new[] { returnStatement });
            SyntaxNode interfaceType = syntaxGenerator.IdentifierName(factoryInterfaceName);
            ClassDeclarationSyntax classDeclaration = syntaxGenerator.
                ClassDeclaration(
                    factoryClassName,
                    accessibility: Accessibility.Public,
                    members: new[] { methodDeclaration },
                    interfaceTypes: new[] { interfaceType }) as ClassDeclarationSyntax;
            NamespaceDeclarationSyntax namespaceDeclaration = syntaxGenerator.
                NamespaceDeclaration(
                    producedClass.ContainingNamespace.ToDisplayString(),
                    classDeclaration) as NamespaceDeclarationSyntax;

            return syntaxGenerator.CompilationUnit(namespaceDeclaration) as CompilationUnitSyntax;
        }
    }
}
