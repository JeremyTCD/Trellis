// Copyright (c) JeremyTCD. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="JA1009MockRepositoryCreateMustBeUsedInsteadOfMockConstructor"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "JA1009";

        private const string Title = "MockRepository.Create must be used instead of Mock<T>'s constructor.";
        private const string MessageFormat = "Use MockRepository.Create instead of Mock<T>().";
        private const string Description = "Mock<T>() used instead of MockRepository.Create.";
        private const string HelpLink = "";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Testing", DiagnosticSeverity.Warning, true, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.CompilationUnit);
            context.EnableConcurrentExecution();
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)context.Node;
            SemanticModel semanticModel = context.SemanticModel;

            // Return if not in a test class
            if (!TestingHelper.ContainsTestClass(compilationUnit))
            {
                return;
            }

            // Find object creation expressions
            IEnumerable<ObjectCreationExpressionSyntax> objectCreationExpressions = compilationUnit.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
            foreach(ObjectCreationExpressionSyntax objectCreationExpression in objectCreationExpressions)
            {
                if(semanticModel.GetTypeInfo(objectCreationExpression).Type.OriginalDefinition?.ToDisplayString() == "Moq.Mock<T>")
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, objectCreationExpression.GetLocation()));
                }
            }
        }
    }
}