using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    /// <summary>
    /// Enforces the line-splitting and indentation conventions: opening braces on their
    /// own line, braced and single-line <c>else</c> clauses, continuation operators at
    /// line end, one statement per line, and continuation indentation.
    /// </summary>
    [TestFixture]
    public sealed class IndentationConventionTests
    {
        [Test]
        public void GivenProductionSources_WhenChecked_ThenOpeningBracesOccupyTheirOwnLine()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] inlineTypeBraces = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .Where(declaration => declaration.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line ==
                        declaration.GetLocation().GetLineSpan().StartLinePosition.Line)
                    .Select(declaration => declaration.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    inlineTypeBraces,
                    Is.Empty,
                    $"{source}: a type's opening brace must occupy its own line.");

                string[] inlineMethodBraces = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Body is not null)
                    .Where(method => method.Body!.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line ==
                        method.GetLocation().GetLineSpan().StartLinePosition.Line)
                    .Select(method => method.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    inlineMethodBraces,
                    Is.Empty,
                    $"{source}: a method's opening brace must occupy its own line.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenElseClausesAreBracedAndKeepElseIfOnOneLine()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                ElseClauseSyntax[] braceLessElse = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ElseClauseSyntax>()
                    .Where(clause => clause.Statement is not BlockSyntax && clause.Statement is not IfStatementSyntax)
                    .ToArray();

                Assert.That(
                    braceLessElse,
                    Is.Empty,
                    $"{source}: every 'else' clause must use braces.");

                IfStatementSyntax[] splitElseIf = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<IfStatementSyntax>()
                    .Where(statement => statement.Parent is ElseClauseSyntax)
                    .Where(statement => statement.IfKeyword.GetLocation().GetLineSpan().StartLinePosition.Line !=
                        ((ElseClauseSyntax)statement.Parent!).ElseKeyword.GetLocation().GetLineSpan().StartLinePosition.Line)
                    .ToArray();

                Assert.That(
                    splitElseIf,
                    Is.Empty,
                    $"{source}: 'else if' must stay on one line.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenContinuationOperatorsArePlacedAtLineEnd()
        {
            foreach (string source in GetProductionSources())
            {
                string[] lines = ReadSource(source).Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string trimmed = lines[index].TrimStart();

                    Assert.That(
                        trimmed.StartsWith("&&", StringComparison.Ordinal) ||
                        trimmed.StartsWith("||", StringComparison.Ordinal),
                        Is.False,
                        $"{source}:{index + 1}: a continuation operator must be placed at the end of the previous line.");
                }
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoTwoStatementsShareALine()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                BlockSyntax[] multiStatementBlocks = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BlockSyntax>()
                    .Where(block => block.Statements.Count > 1)
                    .Where(block => block.Statements
                        .Zip(block.Statements.Skip(1), (first, second) =>
                            first.GetLocation().GetLineSpan().StartLinePosition.Line ==
                            second.GetLocation().GetLineSpan().StartLinePosition.Line)
                        .Any(shareLine => shareLine))
                    .ToArray();

                Assert.That(
                    multiStatementBlocks,
                    Is.Empty,
                    $"{source}: statements must not share a line.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenContinuationLinesIndentByFourSpaces()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] misalignedArrows = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ArrowExpressionClauseSyntax>()
                    .Where(arrow => arrow.GetLocation().GetLineSpan().StartLinePosition.Line !=
                        arrow.Parent!.GetLocation().GetLineSpan().StartLinePosition.Line)
                    .Where(arrow => arrow.GetLocation().GetLineSpan().StartLinePosition.Character !=
                        ((SyntaxNode)arrow.Parent!).GetLocation().GetLineSpan().StartLinePosition.Character + 4)
                    .Select(arrow => arrow.ToString())
                    .ToArray();

                Assert.That(
                    misalignedArrows,
                    Is.Empty,
                    $"{source}: a continuation '=>' must be indented four spaces past its signature.");
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
