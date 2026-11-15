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
    public sealed class ArchitectureTestSourceConventionTests
    {
        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryFileDeclaresOneTypeWithAMatchingName()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                BaseTypeDeclarationSyntax[] declarations = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .ToArray();

                Assert.That(
                    declarations,
                    Has.Length.EqualTo(1),
                    $"{source}: architecture test files must declare exactly one type.");

                Assert.That(
                    declarations[0].Identifier.ValueText,
                    Is.EqualTo(GetFileStem(source)),
                    $"{source}: the declared type must match the file name.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryTestHasAPublicSignature()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] nonPublicTests = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestAttribute)
                    .Where(method => !method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)))
                    .Select(method => method.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    nonPublicTests,
                    Is.Empty,
                    $"{source}: every architecture test method must be public.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryTestHasAtMostSevenParameters()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] oversizedTests = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestAttribute)
                    .Where(method => method.ParameterList.Parameters.Count > 7)
                    .Select(method => method.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    oversizedTests,
                    Is.Empty,
                    $"{source}: architecture tests must have at most seven parameters.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenParameterisedTestsDeclareAtLeastTwoCases()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                foreach (MethodDeclarationSyntax method in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestCaseAttribute))
                {
                    int caseCount = method.AttributeLists
                        .SelectMany(attributeList => attributeList.Attributes)
                        .Count(attribute => string.Equals(attribute.Name.ToString(), "TestCase", StringComparison.Ordinal));

                    Assert.That(
                        caseCount,
                        Is.GreaterThanOrEqualTo(2),
                        $"{source}: the parameterised test '{method.Identifier.ValueText}' must declare at least two cases.");
                }
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryTypeRuleHasADescription()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] undescribedRules = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestAttribute)
                    .Where(method => method.DescendantNodes()
                        .OfType<VariableDeclarationSyntax>()
                        .Any(declaration => declaration.Type.ToString() == "IArchRule"))
                    .Where(method => !method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation => invocation.Expression is MemberAccessExpressionSyntax access &&
                            access.Name.Identifier.ValueText is "As" or "Because"))
                    .Select(method => method.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    undescribedRules,
                    Is.Empty,
                    $"{source}: every type-based architecture rule must carry an 'As' or 'Because' description.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenTheyUseBlockNamespaces()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                Assert.That(
                    tree.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>(),
                    Is.Empty,
                    $"{source}: architecture test files must use block namespaces.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryFixtureIsNUnitAndSealed()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                foreach (ClassDeclarationSyntax fixture in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(HasTests))
                {
                    Assert.That(
                        HasFixtureAttribute(fixture),
                        Is.True,
                        $"{source}: the fixture '{fixture.Identifier.ValueText}' must declare [TestFixture].");

                    Assert.That(
                        fixture.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.SealedKeyword)),
                        Is.True,
                        $"{source}: the fixture '{fixture.Identifier.ValueText}' must be sealed.");

                    Assert.That(
                        fixture.Identifier.ValueText.EndsWith("Tests", StringComparison.Ordinal),
                        Is.True,
                        $"{source}: the fixture '{fixture.Identifier.ValueText}' must end with 'Tests'.");
                }
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenNoAlternativeFrameworkUsageExists()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                Assert.That(
                    UsesAlternativeFramework(tree),
                    Is.False,
                    $"{source}: architecture tests must use NUnit, not an alternative test framework.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryRuleFailsWhenItsSelectorMatchesNothing()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                string content = ReadSource(source);

                if (!content.Contains("GetObjects(", StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.That(
                    content,
                    Does.Contain("Is.Not.Empty"),
                    $"{source}: every selector-backed rule must fail when its selection is empty.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenEveryTestDeclaresAnAssertion()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                List<string> assertionFreeTests = [];

                foreach (MethodDeclarationSyntax method in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestAttribute))
                {
                    bool hasAssertion = method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation => invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                            (memberAccess.Expression.ToString() == "Assert" ||
                             memberAccess.Name.Identifier.ValueText == "Check" ||
                             memberAccess.Name.Identifier.ValueText == "HasNoViolations"));

                    if (!hasAssertion)
                    {
                        assertionFreeTests.Add(method.Identifier.ValueText);
                    }
                }

                Assert.That(
                    assertionFreeTests,
                    Is.Empty,
                    $"{source}: every architecture test must assert -> {string.Join(", ", assertionFreeTests)}.");
            }
        }

        [Test]
        public void GivenArchitectureTestSources_WhenChecked_ThenNoTestNestsHelperTypes()
        {
            foreach (string source in GetArchitectureTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                BaseTypeDeclarationSyntax[] nestedTypes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .Where(declaration => declaration.Parent is TypeDeclarationSyntax)
                    .ToArray();

                Assert.That(
                    nestedTypes.Select(declaration => declaration.Identifier.ValueText),
                    Is.Empty,
                    $"{source}: helper types must be top-level classes in separate files, never nested types.");
            }
        }

        private static bool HasFixtureAttribute(ClassDeclarationSyntax fixture)
            => fixture.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute => string.Equals(attribute.Name.ToString(), "TestFixture", StringComparison.Ordinal));

        private static bool HasTests(ClassDeclarationSyntax fixture)
            => fixture.Members
                .OfType<MethodDeclarationSyntax>()
                .Any(HasTestAttribute);

        private static bool HasTestAttribute(MethodDeclarationSyntax method)
            => method.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute =>
                    string.Equals(attribute.Name.ToString(), "Test", StringComparison.Ordinal) ||
                    string.Equals(attribute.Name.ToString(), "TestCase", StringComparison.Ordinal));

        private static bool HasTestCaseAttribute(MethodDeclarationSyntax method)
            => method.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute => string.Equals(attribute.Name.ToString(), "TestCase", StringComparison.Ordinal));

        private static bool UsesAlternativeFramework(SyntaxTree tree)
        {
            string[] alternativeNamespaces =
            [
                "Xunit",
                "Microsoft.VisualStudio.TestTools.UnitTesting",
                "Microsoft.VisualStudio.TestPlatform"
            ];

            string[] alternativeAttributes = ["Fact", "Theory", "TestClass", "TestMethod"];

            SyntaxNode root = tree.GetRoot();

            bool importsAlternativeNamespace = root
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(usingDirective => usingDirective.Name?.ToString() ?? string.Empty)
                .Any(nameSpace => alternativeNamespaces.Any(candidate =>
                    nameSpace == candidate || nameSpace.StartsWith(candidate + ".", StringComparison.Ordinal)));

            if (importsAlternativeNamespace)
            {
                return true;
            }

            return root
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Select(attribute => attribute.Name.ToString())
                .Any(name => alternativeAttributes.Contains(name, StringComparer.Ordinal));
        }

        private static IReadOnlyList<string> GetArchitectureTestSources()
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(
                ArchitectureResources.ArchitectureTestSourcePrefix);

            Assert.That(sources, Is.Not.Empty, "No architecture test source files were found.");

            return sources;
        }

        private static string ReadSource(string source)
            => ArchitectureResources.ReadResource(ArchitectureResources.ArchitectureTestSourcePrefix, source);

        private static SyntaxTree ParseTree(string source)
            => CSharpSyntaxTree.ParseText(ReadSource(source), path: source);

        private static string GetFileStem(string source)
        {
            string fileName = source.Substring(source.LastIndexOf('/') + 1);

            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }
    }
}
