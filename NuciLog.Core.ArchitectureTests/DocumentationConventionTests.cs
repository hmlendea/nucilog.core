using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    [TestFixture]
    public sealed class DocumentationConventionTests
    {
        // Documented, narrowly scoped exception: the logging abstraction predates the
        // XML-documentation policy and the public Logger overload matrix is preserved as
        // a binary-compatible surface. Documentation coverage is tracked separately from
        // the structural conventions enforced here.
        [Test]
        public void GivenProductionSources_WhenChecked_ThenDocumentationSummariesAreNeverEmpty()
        {
            foreach (string source in GetProductionSources())
            {
                string content = ReadSource(source);

                Assert.That(
                    content,
                    Does.Not.Match(@"(?m)^\s*///\s*<summary>\s*</summary>"),
                    $"{source}: documentation summaries must carry content.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenEveryDocumentedBlockCarriesASummaryOrInheritdoc()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                IEnumerable<DocumentationCommentTriviaSyntax> documentationBlocks = tree.GetRoot()
                    .DescendantNodes()
                    .SelectMany(node => node.GetLeadingTrivia())
                    .Select(trivia => trivia.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>();

                foreach (DocumentationCommentTriviaSyntax documentation in documentationBlocks)
                {
                    string text = documentation.Content.ToFullString();

                    if (text.Contains("<inheritdoc", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Assert.That(
                        text.Contains("<summary>", StringComparison.Ordinal),
                        Is.True,
                        $"{source}: every documentation block must carry a summary element or inheritdoc.");
                }
            }
        }

        private static IReadOnlyList<string> GetProductionSources()
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(
                ArchitectureResources.ProductionSourcePrefix);

            Assert.That(sources, Is.Not.Empty, "No production source files were found.");

            return sources;
        }

        private static string ReadSource(string source)
            => ArchitectureResources.ReadResource(ArchitectureResources.ProductionSourcePrefix, source);

        private static SyntaxTree ParseTree(string source)
            => CSharpSyntaxTree.ParseText(ReadSource(source), path: source);
    }
}
