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
    public sealed class NullHandlingConventionTests
    {
        [Test]
        public void GivenProductionSources_WhenChecked_ThenNullComparisonsUsePatternMatching()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                BinaryExpressionSyntax[] equalityComparisons = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BinaryExpressionSyntax>()
                    .Where(expression => expression.IsKind(SyntaxKind.EqualsExpression) ||
                        expression.IsKind(SyntaxKind.NotEqualsExpression))
                    .Where(expression => IsNullLiteral(expression.Left) || IsNullLiteral(expression.Right))
                    .ToArray();

                Assert.That(
                    equalityComparisons,
                    Is.Empty,
                    $"{source}: use 'is null' / 'is not null' instead of '== null' / '!= null'.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoNullCoalescingOperatorIsUsed()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                BinaryExpressionSyntax[] coalescing = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BinaryExpressionSyntax>()
                    .Where(expression => expression.IsKind(SyntaxKind.CoalesceExpression))
                    .ToArray();

                Assert.That(
                    coalescing,
                    Is.Empty,
                    $"{source}: the null-coalescing operator '??' is not allowed.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoNullForgivingExpressionsAreUsed()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                PostfixUnaryExpressionSyntax[] nullForgivingExpressions = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<PostfixUnaryExpressionSyntax>()
                    .Where(expression => expression.IsKind(SyntaxKind.SuppressNullableWarningExpression))
                    .ToArray();

                Assert.That(
                    nullForgivingExpressions,
                    Is.Empty,
                    $"{source}: the null-forgiving operator '!' is not allowed.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoUnsafeTypeCastsAreUsed()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                CastExpressionSyntax[] castsToReferenceTypes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<CastExpressionSyntax>()
                    .Where(cast => cast.Type is IdentifierNameSyntax identifier &&
                        char.IsUpper(identifier.Identifier.ValueText[0]) &&
                        !IsValueOrPermittedType(identifier.Identifier.ValueText))
                    .ToArray();

                Assert.That(
                    castsToReferenceTypes,
                    Is.Empty,
                    $"{source}: prefer 'as' for casts that may fail instead of a direct cast.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenInstanceEqualsIsNotUsedInNullOrLiteralComparisons()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                InvocationExpressionSyntax[] literalInstanceEqualsCalls = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.ValueText == "Equals" &&
                        invocation.ArgumentList.Arguments.Count == 1 &&
                        IsNullLiteral(memberAccess.Expression))
                    .ToArray();

                Assert.That(
                    literalInstanceEqualsCalls,
                    Is.Empty,
                    $"{source}: prefer static Equals(a, b) or string.Equals(a, b) over instance '.Equals()'.");
            }
        }

        private static bool IsNullLiteral(ExpressionSyntax expression)
            => expression.IsKind(SyntaxKind.NullLiteralExpression);

        private static bool IsValueOrPermittedType(string typeName)
        {
            string[] permittedTypes = ["string", "object", "Exception", "Type", "Func", "Action"];

            return permittedTypes.Contains(typeName, StringComparer.Ordinal);
        }

        private static IReadOnlyList<string> GetProductionSources()
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(
                ArchitectureResources.ProductionSourcePrefix);

            Assert.That(sources, Is.Not.Empty, "No production source files were found.");

            return sources;
        }

        private static SyntaxTree ParseTree(string source)
            => CSharpSyntaxTree.ParseText(
                ArchitectureResources.ReadResource(ArchitectureResources.ProductionSourcePrefix, source),
                path: source);
    }
}
