using System;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class GeneratedCodeHelper
    {
        public static bool IsGenerated(string filePath)
        {
            return filePath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
