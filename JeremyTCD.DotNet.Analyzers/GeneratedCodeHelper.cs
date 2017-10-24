using System;

namespace JeremyTCD.DotNet.Analyzers
{
    public static class GeneratedCodeHelper
    {
        public static bool IsDesigner(string filePath)
        {
            return filePath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
