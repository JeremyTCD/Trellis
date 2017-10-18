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
        public void VerifyFix(string sourcesFolder, int codeFixIndex = 0, bool allowNewCompilerDiagnostics = true)
        {
            IEnumerable<string> beforeFiles = Directory.GetFiles($"{sourcesFolder}/Before");
            Dictionary<Document, List<Diagnostic>> diagnostics = DiagnosticHelper.GetDiagnosticsByDocument(beforeFiles, _diagnosticAnalyzer);
            //var compilerDiagnostics = GetCompilerDiagnostics(documents);

            foreach (KeyValuePair<Document, List<Diagnostic>> keyValuePair in diagnostics)
            {
                Document document = keyValuePair.Key;

                foreach (Diagnostic diagnostic in keyValuePair.Value)
                {
                    List<CodeAction> codeActions = new List<CodeAction>();
                    CodeFixContext context = new CodeFixContext(document, diagnostic, (codeAction, _) => codeActions.Add(codeAction), CancellationToken.None);
                    _codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                    if (!codeActions.Any())
                    {
                        break;
                    }

                    document = CodeFixHelper.ApplyFix(document, codeActions.ElementAt(codeFixIndex));

                    //diagnostics = GetDocumentDiagnostics(analyzer, new[] { document });

                    //var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                    //check if applying the code fix introduced any new compiler diagnostics
                    //if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                    //{
                    //    // Format and get the compiler diagnostics again so that the locations make sense in the output
                    //    document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                    //    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                    //    Assert.True(false,
                    //        string.Format("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                    //            string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                    //            document.GetSyntaxRootAsync().Result.ToFullString()));
                    //}

                    //check if there are analyzer diagnostics left after the code fix
                    //if (!keyValuePair.Any())
                    //{
                    //    break;
                    //}
                }

                //after applying all of the code fixes, compare the resulting string to the inputted one
                string result = InfrastructureHelper.GetStringFromDocument(document);
                string expected = File.ReadAllText($"{sourcesFolder}/After/{document.Name}").Replace(".After", ".Before");
                Assert.Equal(expected, result);
            }
        }
    }
}