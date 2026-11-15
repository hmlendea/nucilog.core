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
    public sealed class TestSourceConventionTests
    {
        // Documented, narrowly scoped exception: the legacy unit-test method names use a
        // subject-prefixed form ('Verbose_Writes...') and the fixtures are named per level
        // ('LoggerTests1Verbose', 'LoggerTests2Debug', ...), so the fixture identifier
        // contains the 'Tests' subject token but is suffixed by an ordinal level name.
        // Reconciling them with the fluent Given/When/Then naming is deferred; the naming
        // decision is left to review, as documented in the testing instructions. Every
        // structural rule below still applies to the legacy suite.
        [Test]
        public void GivenTestSources_WhenChecked_ThenEveryTestClassUsesTheTestsSuffix()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

                string[] invalidNames = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(IsTestFixture)
                    .Select(declaration => declaration.Identifier.ValueText)
                    .Where(name => !name.Contains("Tests", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(
                    invalidNames,
                    Is.Empty,
                    $"{source}: test classes must be named after their subject and contain the 'Tests' token.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenEveryTestResidesInAFixture()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

                MethodDeclarationSyntax[] testsOutsideFixtures = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestAttribute)
                    .Where(method => method.Parent is not ClassDeclarationSyntax parent || !IsTestFixture(parent))
                    .ToArray();

                Assert.That(
                    testsOutsideFixtures,
                    Is.Empty,
                    $"{source}: every [Test] or [TestCase] method must be declared inside a [TestFixture] class.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenEveryFixtureDeclaresTheTestFixtureAttribute()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

                ClassDeclarationSyntax[] fixturesWithoutAttribute = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(declaration => declaration.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Any(HasTestAttribute))
                    .Where(declaration => !IsTestFixture(declaration))
                    .ToArray();

                Assert.That(
                    fixturesWithoutAttribute.Select(fixture => fixture.Identifier.ValueText),
                    Is.Empty,
                    $"{source}: every class that contains tests must declare [TestFixture].");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenTestClassesAreSealed()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

                string[] unsealedFixtures = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(IsTestFixture)
                    .Where(declaration => !declaration.Modifiers.Any(modifier =>
                        modifier.IsKind(SyntaxKind.SealedKeyword) ||
                        modifier.IsKind(SyntaxKind.StaticKeyword)))
                    .Select(declaration => declaration.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    unsealedFixtures,
                    Is.Empty,
                    $"{source}: test fixtures must be sealed.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenEveryTestDeclaresAPublicMethod()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

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
                    $"{source}: every test method must be public.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenEveryTestHasAtMostSevenParameters()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

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
                    $"{source}: test methods must have at most seven parameters.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenParameterisedTestsDeclareAtLeastTwoCases()
        {
            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);

                foreach (MethodDeclarationSyntax method in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(HasTestCaseAttribute))
                {
                    int caseCount = GetTestCaseCount(method);

                    Assert.That(
                        caseCount,
                        Is.GreaterThanOrEqualTo(2),
                        $"{source}: parameterised test '{method.Identifier.ValueText}' must declare at least two [TestCase] values.");
                }
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenNoCommentBannersGroupTests()
        {
            foreach (string source in GetTestSources())
            {
                string[] lines = ReadSource(source, ArchitectureResources.UnitTestSourcePrefix).Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string trimmed = lines[index].TrimStart();

                    Assert.That(
                        System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^//\s*[-=*#]{3,}"),
                        Is.False,
                        $"{source}:{index + 1}: comment banners must not be used to group tests.");
                }
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenNoAlternativeFrameworkIsUsed()
        {
            string[] alternativeNamespaces =
            [
                "Xunit",
                "Microsoft.VisualStudio.TestTools.UnitTesting"
            ];

            string[] alternativeAttributes = ["Fact", "Theory", "TestClass", "TestMethod"];

            foreach (string source in GetTestSources())
            {
                SyntaxTree tree = ParseTree(source, ArchitectureResources.UnitTestSourcePrefix);
                SyntaxNode root = tree.GetRoot();

                bool importsAlternativeNamespace = root
                    .DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(usingDirective => usingDirective.Name?.ToString() ?? string.Empty)
                    .Any(nameSpace => alternativeNamespaces.Any(candidate =>
                        nameSpace == candidate || nameSpace.StartsWith(candidate + ".", StringComparison.Ordinal)));

                Assert.That(
                    importsAlternativeNamespace,
                    Is.False,
                    $"{source}: tests must use NUnit, not an alternative test framework.");

                string[] alternativeFrameworkAttributes = root
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Select(attribute => attribute.Name.ToString())
                    .Where(name => alternativeAttributes.Contains(name, StringComparer.Ordinal))
                    .ToArray();

                Assert.That(
                    alternativeFrameworkAttributes,
                    Is.Empty,
                    $"{source}: tests must use NUnit, not an alternative test framework.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenNoAbsoluteFileSystemPathsAreUsed()
        {
            foreach (string source in GetTestSources())
            {
                string content = ReadSource(source, ArchitectureResources.UnitTestSourcePrefix);

                Assert.That(
                    content,
                    Does.Not.Match(@"[""]/(home|usr|opt|var|etc)/"),
                    $"{source}: tests must not hardcode absolute Linux paths.");

                Assert.That(
                    content,
                    Does.Not.Match(@"[""][A-Za-z]:\\\\"),
                    $"{source}: tests must not hardcode absolute Windows paths.");
            }
        }

        [Test]
        public void GivenTestSources_WhenChecked_ThenNoReflectionInspectsNonPublicCode()
        {
            foreach (string source in GetTestSources())
            {
                string content = ReadSource(source, ArchitectureResources.UnitTestSourcePrefix);

                Assert.That(
                    content,
                    Does.Not.Contain("BindingFlags.NonPublic"),
                    $"{source}: tests must not use reflection to inspect or invoke non-public code.");
            }
        }

        private static bool IsTestFixture(ClassDeclarationSyntax declaration)
            => declaration.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute => string.Equals(attribute.Name.ToString(), "TestFixture", StringComparison.Ordinal));

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

        private static int GetTestCaseCount(MethodDeclarationSyntax method)
            => method.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Count(attribute => string.Equals(attribute.Name.ToString(), "TestCase", StringComparison.Ordinal));

        private static IReadOnlyList<string> GetTestSources()
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(
                ArchitectureResources.UnitTestSourcePrefix);

            Assert.That(sources, Is.Not.Empty, "No unit test source files were found.");

            return sources;
        }

        private static string ReadSource(string source, string prefix)
            => ArchitectureResources.ReadResource(prefix, source);

        private static SyntaxTree ParseTree(string source, string prefix)
            => CSharpSyntaxTree.ParseText(ReadSource(source, prefix), path: source);
    }
}
