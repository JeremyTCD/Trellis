using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with codefixes.
    /// Contains methods used to verify correctness of codefixes
    /// </summary>
    public class CodeFixVerifier
    {
        private readonly DiagnosticAnalyzer _diagnosticAnalyzer;
        private readonly CodeFixProvider _codeFixProvider;

        public CodeFixVerifier(DiagnosticAnalyzer diagnosticAnalyzer, CodeFixProvider codeFixProvider)
        {
            _diagnosticAnalyzer = diagnosticAnalyzer;
            _codeFixProvider = codeFixProvider;
        }

        /// <summary>
        /// General verifier for codefixes.
        /// Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
        /// Then gets the string after the codefix is applied and compares it with the expected result.
        /// Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="language">The language the source code is in</param>
        /// <param name="analyzer">The analyzer to be applied to the source code</param>
        /// <param name="codeFixProvider">The codefix to be applied to the code wherever the relevant Diagnostic is found</param>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        public void VerifyFix(string sourcesFolder)
        {
            IEnumerable<string> beforeFiles = Directory.GetFiles($"{sourcesFolder}/Before");
            Dictionary<Document, List<Diagnostic>> diagnostics = DiagnosticHelper.GetDiagnosticsByDocument(beforeFiles, _diagnosticAnalyzer);

            foreach (KeyValuePair<Document, List<Diagnostic>> keyValuePair in diagnostics)
            {
                Document document = keyValuePair.Key;

                FixAllProvider fixAllProvider = _codeFixProvider.GetFixAllProvider();
                FixMultipleDiagnosticProvider fixMultipleDiagnosticProvider = new FixMultipleDiagnosticProvider(keyValuePair.Value);
                FixAllContext fixAllContext = new FixAllContext(
                    document, 
                    _codeFixProvider, 
                    FixAllScope.Document, 
                    _codeFixProvider.GetType().Name,
                    _codeFixProvider.FixableDiagnosticIds,
                    fixMultipleDiagnosticProvider, 
                    CancellationToken.None);
                // Need to use custom assembly cause of this issue: https://github.com/dotnet/roslyn/issues/22710
                CodeAction codeAction = fixAllProvider.GetFixAsync(fixAllContext).Result;
                document = CodeFixHelper.ApplyFix(document, codeAction);

                //after applying all of the code fixes, compare the resulting string to the inputted one
                string result = InfrastructureHelper.GetStringFromDocument(document);
                string expected = File.ReadAllText($"{sourcesFolder}/After/{document.Name}").Replace(".After", ".Before");
                Assert.Equal(expected, result);
            }
        }
    }
}