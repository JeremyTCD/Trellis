using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace JeremyTCD.DotNet.Analyzers.Tests
{
    public static class InfrastructureHelper
    {
        private static readonly MetadataReference CollectionsReference = MetadataReference.CreateFromFile(typeof(IEnumerable).Assembly.Location);
        private static readonly MetadataReference GenericCollectionsReference = MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location);
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference LinqReference = MetadataReference.CreateFromFile(typeof(Expression<>).Assembly.Location);
        private static readonly MetadataReference XunitReference = MetadataReference.CreateFromFile(typeof(FactAttribute).Assembly.Location);
        private static readonly MetadataReference MoqReference = MetadataReference.CreateFromFile(typeof(Mock).Assembly.Location);
        private static readonly MetadataReference SystemRuntimeReference = MetadataReference.CreateFromFile("C:/Program Files/dotnet/shared/Microsoft.NETCore.App/2.0.0/System.Runtime.dll");

        /// <summary>
        /// Given a document, turn it into a string based on the syntax root
        /// </summary>
        /// <param name="document">The Document to be converted to a string</param>
        /// <returns>A string containing the syntax of the Document after formatting</returns>
        public static string GetStringFromDocument(Document document)
        {
            return document.GetTextAsync().Result.ToString();
        }

        /// <summary>
        /// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
        /// </summary>
        /// <param name="files">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
        public static Document[] CreateDocuments(IEnumerable<string> files, string projectName)
        {
            var project = CreateProject(files, projectName);
            var documents = project.Documents.ToArray();

            if (files.Count() != documents.Length)
            {
                throw new Exception("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        /// <summary>
        /// Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="files">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Project created out of the Documents created from the source strings</returns>
        public static Project CreateProject(IEnumerable<string> files, string projectName)
        {
            string testProjectName = $"{projectName}.Tests";
            ProjectId projectId = ProjectId.CreateNewId(debugName: projectName);
            ProjectId testProjectId = ProjectId.CreateNewId(debugName: testProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, LinqReference)
                .AddMetadataReference(projectId, XunitReference)
                .AddMetadataReference(projectId, SystemRuntimeReference)
                .AddMetadataReference(projectId, CollectionsReference)
                .AddMetadataReference(projectId, GenericCollectionsReference)
                .AddMetadataReference(projectId, MoqReference)
                .AddProject(testProjectId, testProjectName, testProjectName, LanguageNames.CSharp)
                .AddProjectReference(testProjectId, new ProjectReference(projectId))
                .AddMetadataReference(testProjectId, CorlibReference)
                .AddMetadataReference(testProjectId, LinqReference)
                .AddMetadataReference(testProjectId, XunitReference)
                .AddMetadataReference(testProjectId, SystemRuntimeReference)
                .AddMetadataReference(testProjectId, CollectionsReference)
                .AddMetadataReference(testProjectId, GenericCollectionsReference)
                .AddMetadataReference(testProjectId, MoqReference);

            int count = 0;
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                var documentId = DocumentId.CreateNewId(projectId, debugName: fileName);
                solution = solution.AddDocument(documentId, fileName, SourceText.From(File.ReadAllText(file)));
                count++;
            }
            return solution.GetProject(projectId);
        }
    }
}
