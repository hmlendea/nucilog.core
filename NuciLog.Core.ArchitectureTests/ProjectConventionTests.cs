using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    [TestFixture]
    public sealed class ProjectConventionTests
    {
        private static string ProductionProjectPath => "NuciLog.Core/NuciLog.Core.csproj";

        private static string UnitTestProjectPath => "NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj";

        private static string ArchitectureTestProjectPath => "NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj";

        [TestCase("NuciLog.Core/NuciLog.Core.csproj")]
        [TestCase("NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj")]
        [TestCase("NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj")]
        public void GivenProjects_WhenLoaded_ThenTheRequiredPropertiesAreDeclared(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            Assert.That(
                GetProperty(project, "TargetFramework"),
                Is.EqualTo("net10.0"),
                $"{projectPath}: the target framework must be net10.0.");

            Assert.That(
                GetProperty(project, "ImplicitUsings"),
                Is.EqualTo("disable"),
                $"{projectPath}: implicit usings must be disabled.");

            Assert.That(
                GetProperty(project, "LangVersion"),
                Is.EqualTo("latest"),
                $"{projectPath}: the latest language version must be used.");
        }

        // Scoped exception: the production and unit-test projects remain nullable-oblivious.
        // Enabling nullable references would surface roughly nine hundred pre-existing
        // nullability warnings across the preserved overload matrix, which is a separate,
        // deliberately deferred migration. The new architecture test project is written
        // nullable-aware and therefore declares Nullable=enable.
        [Test]
        public void GivenArchitectureTestProject_WhenLoaded_ThenNullableReferencesAreEnabled()
        {
            XDocument project = LoadProject(ArchitectureTestProjectPath);

            Assert.That(
                GetProperty(project, "Nullable"),
                Is.EqualTo("enable"),
                "The architecture test project must enable nullable references.");
        }

        [TestCase("NuciLog.Core/NuciLog.Core.csproj")]
        [TestCase("NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj")]
        [TestCase("NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj")]
        public void GivenProjects_WhenLoaded_ThenPackageReferencesAreAlphabetical(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            string[] packages = project.Descendants("PackageReference")
                .Select(reference => GetAttributeValue(reference, "Include"))
                .ToArray();

            Assert.That(packages, Is.Not.Empty, $"{projectPath}: no package references were found.");
            Assert.That(
                packages,
                Is.EqualTo(packages.OrderBy(package => package, StringComparer.Ordinal).ToArray()),
                $"{projectPath}: package references must be ordered alphabetically.");
        }

        [TestCase("NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj")]
        [TestCase("NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj")]
        public void GivenTestProjects_WhenLoaded_ThenTheTestFlagsAreDeclared(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            Assert.That(
                GetProperty(project, "IsTestProject"),
                Is.EqualTo("true"),
                $"{projectPath}: the test project flag must be declared.");

            Assert.That(
                GetProperty(project, "ExcludeFromCodeCoverage"),
                Is.EqualTo("true"),
                $"{projectPath}: the code coverage exclusion flag must be declared.");

            Assert.That(
                GetProperty(project, "IsPackable"),
                Is.EqualTo("false"),
                $"{projectPath}: test projects must not be packable.");
        }

        [Test]
        public void GivenUnitTestProject_WhenLoaded_ThenTheTestFlagIsDeclared()
        {
            XDocument project = LoadProject(UnitTestProjectPath);

            Assert.That(
                GetProperty(project, "IsTestProject"),
                Is.EqualTo("true"),
                "The unit test project must declare IsTestProject.");
        }

        [Test]
        public void GivenProductionProject_WhenLoaded_ThenItDoesNotReferenceTestProjects()
        {
            XDocument project = LoadProject(ProductionProjectPath);

            Assert.That(
                GetProjectReferences(project).Where(reference =>
                    reference.Contains("Tests", StringComparison.OrdinalIgnoreCase)),
                Is.Empty,
                "The production project must not reference test projects.");
        }

        [Test]
        public void GivenProductionProject_WhenLoaded_ThenItDoesNotDeclareFriendAssemblies()
        {
            XDocument project = LoadProject(ProductionProjectPath);

            Assert.That(
                project.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "InternalsVisibleTo", StringComparison.Ordinal)),
                Is.Empty,
                "The production project must not declare InternalsVisibleTo.");
        }

        [Test]
        public void GivenUnitTestProject_WhenLoaded_ThenItReferencesTheProductionProject()
        {
            XDocument project = LoadProject(UnitTestProjectPath);

            Assert.That(
                GetProjectReferences(project),
                Has.Some.EqualTo("../NuciLog.Core/NuciLog.Core.csproj"),
                "The unit test project must reference the production project.");
        }

        [Test]
        public void GivenArchitectureTestProject_WhenLoaded_ThenItReferencesTheProductionProject()
        {
            XDocument project = LoadProject(ArchitectureTestProjectPath);

            Assert.That(
                GetProjectReferences(project),
                Has.Some.EqualTo("../NuciLog.Core/NuciLog.Core.csproj"),
                "The architecture test project must reference the production project.");
        }

        [TestCase("NuciLog.Core/NuciLog.Core.csproj")]
        [TestCase("NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj")]
        [TestCase("NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj")]
        public void GivenProjects_WhenLoaded_ThenOnlyASingleTargetFrameworkIsDeclared(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            Assert.That(
                project.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "TargetFrameworks", StringComparison.Ordinal)),
                Is.Empty,
                $"{projectPath}: multi-targeting is not used; declare a single TargetFramework.");
        }

        [TestCase("NuciLog.Core/NuciLog.Core.csproj")]
        [TestCase("NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj")]
        [TestCase("NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj")]
        public void GivenProjects_WhenLoaded_ThenNoProjectDeclaresFriendAssemblies(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            Assert.That(
                project.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "InternalsVisibleTo", StringComparison.Ordinal) ||
                    string.Equals(
                        GetAttributeValue(element, "Include"),
                        "System.Runtime.CompilerServices.InternalsVisibleToAttribute",
                        StringComparison.Ordinal)),
                Is.Empty,
                $"{projectPath}: no project may declare InternalsVisibleTo.");
        }

        [TestCase("NuciLog.Core/NuciLog.Core.csproj")]
        [TestCase("NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj")]
        [TestCase("NuciLog.Core.ArchitectureTests/NuciLog.Core.ArchitectureTests.csproj")]
        public void GivenProjects_WhenLoaded_ThenEveryPackageReferenceDeclaresAVersion(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            foreach (XElement reference in project.Descendants("PackageReference"))
            {
                string version = GetAttributeValue(reference, "Version");
                XElement? versionElement = reference.Element("Version");

                Assert.That(
                    version.Length > 0 || versionElement is not null,
                    Is.True,
                    $"{projectPath}: the package '{GetAttributeValue(reference, "Include")}' must declare a version.");
            }
        }

        [Test]
        public void GivenProductionProject_WhenLoaded_ThenItsPackageMetadataIsComplete()
        {
            XDocument project = LoadProject(ProductionProjectPath);

            Assert.That(GetProperty(project, "Description"), Is.Not.Empty, "The package description must be declared.");
            Assert.That(GetProperty(project, "Authors"), Is.Not.Empty, "The package authors must be declared.");
            Assert.That(
                GetProperty(project, "PackageLicenseExpression"),
                Is.EqualTo("GPL-3.0-or-later"),
                "The package licence must be GPL-3.0-or-later.");
            Assert.That(GetProperty(project, "RepositoryUrl"), Is.Not.Empty, "The repository URL must be declared.");
            Assert.That(GetProperty(project, "PackageTags"), Is.Not.Empty, "The package tags must be declared.");
            Assert.That(GetProperty(project, "Version"), Is.Not.Empty, "The package version must be declared.");
        }

        private static XDocument LoadProject(string relativePath)
            => XDocument.Parse(
                ArchitectureResources.ReadResource(
                    ArchitectureResources.ProjectFilePrefix,
                    relativePath));

        private static string[] GetProjectReferences(XDocument project)
            => project.Descendants("ProjectReference")
                .Select(reference => GetAttributeValue(reference, "Include"))
                .Select(reference => reference.Replace('\\', '/'))
                .ToArray();

        private static string GetAttributeValue(XElement element, string attributeName)
        {
            XAttribute? attribute = element.Attribute(attributeName);

            if (attribute is null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }

        private static string GetProperty(XDocument project, string name)
        {
            XElement? element = project.Descendants()
                .FirstOrDefault(candidate => string.Equals(candidate.Name.LocalName, name, StringComparison.Ordinal));

            if (element is null)
            {
                return string.Empty;
            }

            return element.Value;
        }
    }
}
