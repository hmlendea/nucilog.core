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
    /// Enforces the unit-test conventions that are decidable from source: NUnit constraint
    /// assertions, no alternative frameworks, permitted assertion forms, deterministic and
    /// portable test data, and one fixture-local declaration per file.
    /// </summary>
    [TestFixture]
    public sealed class UnitTestConventionTests
    {
        private static string[] ForbiddenAssertions =>
        [
            "Assert.AreEqual",
            "Assert.IsTrue",
            "Assert.IsFalse",
            "Assert.IsNull",
            "Assert.IsNotNull",
            "Assert.Greater",
            "Assert.Less",
            "Assert.That(Is.True)",
            "Assert.That(Is.False)",
            "Is.EqualTo(string.Empty)"
        ];

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenOnlyNUnitConstraintAssertionsAreUsed()
        {
            foreach (string source in GetUnitTestSources())
            {
                string content = ReadSource(source);

                foreach (string forbidden in ForbiddenAssertions)
                {
                    Assert.That(
                        content,
                        Does.Not.Contain(forbidden + "("),
                        $"{source}: use Assert.That with NUnit constraints instead of {forbidden}.");
                }
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenNoInstanceEqualsAssertionIsUsed()
        {
            foreach (string source in GetUnitTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                InvocationExpressionSyntax[] instanceEquals = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.ValueText == "Equals" &&
                        invocation.ArgumentList.Arguments.Count == 1)
                    .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax access &&
                        access.Expression.ToString() != "object")
                    .Where(invocation => ((MemberAccessExpressionSyntax)invocation.Expression).Expression.ToString() != "Assert")
                    .ToArray();

                Assert.That(
                    instanceEquals,
                    Is.Empty,
                    $"{source}: compare with Is.EqualTo, never with instance Equals().");
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenNoFormatProviderlessDateTimeFormattingIsUsed()
        {
            foreach (string source in GetUnitTestSources())
            {
                string content = ReadSource(source);

                Assert.That(
                    content,
                    Does.Not.Match(@"\.ToString\(""(o|c|G|yyyy-MM-dd)""\)"),
                    $"{source}: date-time formatting must pass CultureInfo.InvariantCulture.");
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenNoEnvironmentDependentApisAreUsed()
        {
            string[] forbiddenApis =
            [
                "DateTime.Now",
                "DateTime.UtcNow",
                "DateTime.Today",
                "new Random(",
                "Thread.Sleep(",
                "Environment.CurrentDirectory",
                "Environment.MachineName",
                "Environment.UserName",
                "Environment.GetEnvironmentVariable(",
                "CultureInfo.CurrentCulture",
                "CultureInfo.CurrentUICulture",
                "TimeZoneInfo.Local"
            ];

            foreach (string source in GetUnitTestSources())
            {
                string content = ReadSource(source);

                foreach (string forbidden in forbiddenApis)
                {
                    Assert.That(
                        content,
                        Does.Not.Contain(forbidden),
                        $"{source}: unit tests must stay deterministic and portable; {forbidden} is not allowed.");
                }
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenNoNestedTypesAreDeclared()
        {
            foreach (string source in GetUnitTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] nestedTypes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .Where(declaration => declaration.Parent is TypeDeclarationSyntax)
                    .Select(declaration => declaration.Identifier.ValueText)
                    .ToArray();

                Assert.That(
                    nestedTypes,
                    Is.Empty,
                    $"{source}: dummy implementations must be top-level internal classes in separate files, never nested types.");
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenEveryFileDeclaresOneTypeWithAMatchingName()
        {
            foreach (string source in GetUnitTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                BaseTypeDeclarationSyntax[] declarations = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .ToArray();

                Assert.That(
                    declarations,
                    Has.Length.EqualTo(1),
                    $"{source}: test files must declare exactly one type.");

                Assert.That(
                    declarations[0].Identifier.ValueText,
                    Is.EqualTo(GetFileStem(source)),
                    $"{source}: the declared type must match the file name.");
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenEverySetupMethodUsesTheDocumentedName()
        {
            foreach (string source in GetUnitTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] invalidSetUpNames = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.AttributeLists
                        .SelectMany(attributeList => attributeList.Attributes)
                        .Any(attribute => string.Equals(attribute.Name.ToString(), "SetUp", StringComparison.Ordinal)))
                    .Select(method => method.Identifier.ValueText)
                    .Where(name => !string.Equals(name, "SetUp", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(
                    invalidSetUpNames,
                    Is.Empty,
                    $"{source}: setup methods must be declared as '[SetUp] public void SetUp()'.");
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenEveryTestSignatureUsesGivenWhenThenNaming()
        {
            foreach (string source in GetUnitTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] nonFluentNames = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.AttributeLists
                        .SelectMany(attributeList => attributeList.Attributes)
                        .Any(attribute => string.Equals(attribute.Name.ToString(), "Test", StringComparison.Ordinal)))
                    .Select(method => method.Identifier.ValueText)
                    .Where(name => !name.StartsWith("Given", StringComparison.Ordinal) &&
                        !name.StartsWith("Build_", StringComparison.Ordinal) &&
                        !name.StartsWith("Verbose_", StringComparison.Ordinal) &&
                        !name.StartsWith("Debug_", StringComparison.Ordinal) &&
                        !name.StartsWith("Info_", StringComparison.Ordinal) &&
                        !name.StartsWith("Warn_", StringComparison.Ordinal) &&
                        !name.StartsWith("Error_", StringComparison.Ordinal) &&
                        !name.StartsWith("Fatal_", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(
                    nonFluentNames,
                    Is.Empty,
                    $"{source}: test methods must use the documented naming form.");
            }
        }

        [Test]
        public void GivenUnitTestSources_WhenChecked_ThenNoReusableArrayLiteralIsPassedToAssertions()
        {
            foreach (string source in GetUnitTestSources())
            {
                SyntaxTree tree = ParseTree(source);

                ArrayCreationExpressionSyntax[] arrayLiterals = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ArrayCreationExpressionSyntax>()
                    .Where(array => array.Initializer is not null)
                    .Where(array => array.Parent is ArgumentSyntax)
                    .ToArray();

                Assert.That(
                    arrayLiterals,
                    Is.Empty,
                    $"{source}: reusable expected values must be declared in private static readonly fields.");
            }
        }

        private static IReadOnlyList<string> GetUnitTestSources()
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(
                ArchitectureResources.UnitTestSourcePrefix);

            Assert.That(sources, Is.Not.Empty, "No unit test source files were found.");

            return sources;
        }

        private static string ReadSource(string source)
            => ArchitectureResources.ReadResource(ArchitectureResources.UnitTestSourcePrefix, source);

        private static SyntaxTree ParseTree(string source)
            => CSharpSyntaxTree.ParseText(ReadSource(source), path: source);

        private static string GetFileStem(string source)
        {
            string fileName = source.Substring(source.LastIndexOf('/') + 1);

            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }
    }
}