using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    /// <summary>
    /// Class for turning strings into documents and getting the diagnostics on them
    /// All methods are static
    /// </summary>
    public static class DiagnosticHelper
    {
        public static IEnumerable<Diagnostic> GetDiagnostics(IEnumerable<string> files, DiagnosticAnalyzer analyzer)
        {
            return GetDiagnosticsByDocument(files, analyzer).Values.SelectMany(d => d);
        }

        public static Dictionary<Document, List<Diagnostic>> GetDiagnosticsByDocument(IEnumerable<string> files, DiagnosticAnalyzer analyzer)
        {
            IEnumerable<Document> documents = InfrastructureHelper.CreateDocuments(files);
            var projects = new HashSet<Project>();
            foreach (var document in documents)
            {
                projects.Add(document.Project);
            }

            Dictionary<Document, List<Diagnostic>> result = new Dictionary<Document, List<Diagnostic>>();
            foreach (var project in projects)
            {
                CompilationWithAnalyzers compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
                // TODO should check for compiler diagnostics first since they can affect analyzer diagnostics
                IEnumerable<Diagnostic> diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    foreach (Document document in documents)
                    {
                        var tree = document.GetSyntaxTreeAsync().Result;
                        if (tree == diagnostic.Location.SourceTree)
                        {
                            if (result.TryGetValue(document, out List<Diagnostic> existingDiagnostics))
                            {
                                existingDiagnostics.Add(diagnostic);
                            }
                            else
                            {
                                result.Add(document, new List<Diagnostic> { diagnostic });
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}

