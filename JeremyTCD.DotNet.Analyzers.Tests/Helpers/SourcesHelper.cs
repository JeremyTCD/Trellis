using System.IO;
using System.Runtime.CompilerServices;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public class SourcesHelper
    {
        private readonly string _diagnosticAnalyzerID;

        public SourcesHelper(string diagnosticAnalyzerID)
        {
            _diagnosticAnalyzerID = diagnosticAnalyzerID;
        }

        public string GetSourcesFolder([CallerMemberName] string testMethodName = "")
        {
            string folderName = testMethodName.Replace("DiagnosticAnalyzer_", "").Replace("CodeFixProvider_", "");

            return Path.Combine(Directory.GetCurrentDirectory(),
                $"../../../../../JeremyTCD.DotNet.Analyzers.Tests.Sources/JeremyTCD.DotNet.Analyzers.Tests.Sources/{_diagnosticAnalyzerID.Substring(0, 6)}/{folderName}");
        }
    }
}
