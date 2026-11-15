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
    /// Enforces the collection, mapping, serialisation, logging, exception, and
    /// culture-sensitivity conventions that the production library is subject to.
    /// </summary>
    [TestFixture]
    public sealed class ApiConventionTests
    {
        private static string[] CollectionTypes => ["List<", "IList<", "IReadOnlyList<", "HashSet<", "Dictionary<"];

        [Test]
        public void GivenProductionSources_WhenChecked_ThenCollectionSignaturesUseEnumerableAbstractions()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                IEnumerable<string> concreteCollections = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ParameterSyntax>()
                    .Select(parameter => parameter.Type?.ToString() ?? string.Empty)
                    .Where(typeText => CollectionTypes.Any(collection => typeText.StartsWith(collection, StringComparison.Ordinal)))
                    .Concat(tree.GetRoot()
                        .DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Select(method => method.ReturnType.ToString())
                        .Where(typeText => CollectionTypes.Any(collection => typeText.StartsWith(collection, StringComparison.Ordinal))))
                    .ToArray();

                Assert.That(
                    concreteCollections,
                    Is.Empty,
                    $"{source}: collection parameters and returns must use IEnumerable<T>.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoMappingExtensionClassesAreDeclared()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] mappingTypes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .Select(declaration => declaration.Identifier.ValueText)
                    .Where(name => name.EndsWith("MappingExtensions", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(
                    mappingTypes,
                    Is.Empty,
                    $"{source}: the library declares no mapping extension classes.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenDateTimeFormattingUsesInvariantCulture()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                InvocationExpressionSyntax[] dateFormattingCalls = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.ValueText == "ToString")
                    .Where(invocation => invocation.ArgumentList.Arguments.Count > 0)
                    .Where(invocation => invocation.ToString().Contains("format", StringComparison.Ordinal))
                    .Where(invocation => !invocation.ToString().Contains("InvariantCulture", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(
                    dateFormattingCalls,
                    Is.Empty,
                    $"{source}: date-time formatting with a format string must pass CultureInfo.InvariantCulture.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoJsonSerialisationAttributesAreApplied()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                AttributeSyntax[] jsonAttributes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Where(attribute => attribute.Name.ToString().EndsWith("JsonPropertyName", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(
                    jsonAttributes,
                    Is.Empty,
                    $"{source}: the library exposes no API DTOs, so no JSON serialisation attributes are expected.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenAllThrownExceptionsAreBclTypes()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] customExceptionThrows = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ThrowStatementSyntax>()
                    .Select(throwStatement => throwStatement.Expression?.ToString() ?? string.Empty)
                    .Where(expression => expression.StartsWith("new ", StringComparison.Ordinal))
                    .Where(expression => !IsBclException(expression))
                    .ToArray();

                Assert.That(
                    customExceptionThrows,
                    Is.Empty,
                    $"{source}: only base class library exceptions may be thrown.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoNestedConditionalExpressionsAreDeclared()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                ConditionalExpressionSyntax[] nestedTernaries = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ConditionalExpressionSyntax>()
                    .Where(expression => expression.DescendantNodes().OfType<ConditionalExpressionSyntax>().Any())
                    .ToArray();

                Assert.That(
                    nestedTernaries,
                    Is.Empty,
                    $"{source}: conditional expressions must not be nested.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoRepeatedArrayArgumentsAreAllocated()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] repeatedArrayLiterals = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ArrayCreationExpressionSyntax>()
                    .Where(array => array.Initializer is not null)
                    .Select(array => array.ToString())
                    .GroupBy(text => text, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToArray();

                Assert.That(
                    repeatedArrayLiterals,
                    Is.Empty,
                    $"{source}: repeatedly passed array literals must be stored in static readonly fields.");
            }
        }

        private static bool IsBclException(string expression)
        {
            string[] permitted = ["Exception", "ArgumentNullException", "ArgumentException", "InvalidOperationException", "KeyNotFoundException", "NotSupportedException"];

            return permitted.Any(candidate => expression.Contains(candidate, StringComparison.Ordinal));
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
