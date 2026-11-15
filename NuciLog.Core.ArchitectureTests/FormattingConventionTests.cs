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
    public sealed class FormattingConventionTests
    {
        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoTwoConsecutiveBlankLinesExist()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source),
                    Does.Not.Match(@"\n[ \t]*\n[ \t]*\n"),
                    $"{source}: two or more consecutive blank lines are not allowed.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenEachFileEndsWithExactlyOneLineFeed()
        {
            foreach (string source in GetProductionSources())
            {
                string content = ReadSource(source);

                Assert.That(
                    content.EndsWith('\n'),
                    Is.True,
                    $"{source}: the file must end with a line feed.");

                Assert.That(
                    content.EndsWith("\n\n", StringComparison.Ordinal),
                    Is.False,
                    $"{source}: the file must end with exactly one line feed.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoCarriageReturnCharactersAreUsed()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source),
                    Does.Not.Contain("\r"),
                    $"{source}: line endings must use a single LF, not CRLF.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoTrailingWhitespaceExists()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source),
                    Does.Not.Match(@"[ \t]+(\r?\n)"),
                    $"{source}: trailing whitespace is not allowed.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoTabsAreUsedForIndentation()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source),
                    Does.Not.Match(@"(?m)^[ \t]*\t"),
                    $"{source}: tabs must not be used for indentation.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoBlankLinePrecedesClosingDelimiters()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source),
                    Does.Not.Match(@"\n[ \t]*\n[ \t]*[}\])]"),
                    $"{source}: no blank line may precede a closing delimiter.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenExactlyOneBlankLinePrecedesTheNamespace()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source),
                    Does.Not.Match(@"\n[ \t]*\n[ \t]*\n[ \t]*namespace "),
                    $"{source}: exactly one blank line must precede the namespace declaration.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenUsingDirectivesAreAlphabetisedWithinGroups()
        {
            foreach (string source in GetProductionSources())
            {
                foreach (IReadOnlyList<string> group in GetUsingGroups(source))
                {
                    string[] sorted = group.OrderBy(name => name, StringComparer.Ordinal).ToArray();

                    Assert.That(
                        group,
                        Is.EqualTo(sorted),
                        $"{source}: using directives must be alphabetised within each group.");
                }
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenIndentationUsesFourSpaceMultiples()
        {
            foreach (string source in GetProductionSources())
            {
                string[] lines = ReadSource(source).Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string line = lines[index];

                    if (line.Trim().Length == 0)
                    {
                        continue;
                    }

                    int leadingSpaces = line.Length - line.TrimStart(' ').Length;

                    Assert.That(
                        leadingSpaces % 4,
                        Is.EqualTo(0),
                        $"{source}:{index + 1}: indentation must use multiples of four spaces.");
                }
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoByteOrderMarkIsPresent()
        {
            foreach (string source in GetProductionSources())
            {
                Assert.That(
                    ReadSource(source).StartsWith('\uFEFF'),
                    Is.False,
                    $"{source}: the file must not start with a byte-order mark.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenMembersAreSeparatedByExactlyOneBlankLine()
        {
            foreach (string source in GetProductionSources())
            {
                string[] lines = ReadSource(source).Split('\n');

                for (int index = 1; index < lines.Length; index++)
                {
                    string current = lines[index].Trim();
                    string previous = lines[index - 1].Trim();

                    bool currentIsMemberStart =
                        current.StartsWith("public ", StringComparison.Ordinal) ||
                        current.StartsWith("private ", StringComparison.Ordinal) ||
                        current.StartsWith("protected ", StringComparison.Ordinal) ||
                        current.StartsWith("internal ", StringComparison.Ordinal) ||
                        current.StartsWith("/// ", StringComparison.Ordinal);

                    bool previousIsClosingBrace = previous == "}";

                    if (currentIsMemberStart && previousIsClosingBrace)
                    {
                        Assert.That(
                            lines[index - 1].Length == 0 || lines[index - 1].Trim().Length == 0,
                            Is.True,
                            $"{source}:{index + 1}: members must be separated by a blank line.");
                    }
                }
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenMemberAttributesArePlacedOnTheirOwnLines()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                AttributeListSyntax[] inlineAttributes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<AttributeListSyntax>()
                    .Where(attributeList => attributeList.Parent is MemberDeclarationSyntax)
                    .Where(attributeList => attributeList.GetLocation().GetLineSpan().StartLinePosition.Line ==
                        ((MemberDeclarationSyntax)attributeList.Parent!).GetLocation().GetLineSpan().StartLinePosition.Line)
                    .ToArray();

                Assert.That(
                    inlineAttributes,
                    Is.Empty,
                    $"{source}: member attributes must be placed on their own lines.");
            }
        }

        private static IReadOnlyList<IReadOnlyList<string>> GetUsingGroups(string source)
        {
            string[] lines = ReadSource(source)
                .Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .ToArray();

            List<IReadOnlyList<string>> groups = [];
            List<string> currentGroup = [];

            foreach (string line in lines)
            {
                bool isUsingDirective = line.StartsWith("using ", StringComparison.Ordinal) && line.EndsWith(';');

                if (isUsingDirective)
                {
                    currentGroup.Add(GetUsingDirectiveName(line));
                    continue;
                }

                if (currentGroup.Count > 0)
                {
                    groups.Add(currentGroup);
                    currentGroup = [];
                }
            }

            if (currentGroup.Count > 0)
            {
                groups.Add(currentGroup);
            }

            return groups;
        }

        private static string GetUsingDirectiveName(string usingDirective)
            => usingDirective.Replace("using ", string.Empty, StringComparison.Ordinal).TrimEnd(';');

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
