using System;
using System.Collections.Generic;
using System.Linq;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static ArchUnitNET.Fluent.Slices.SliceRuleDefinition;

namespace NuciLog.Core.ArchitectureTests
{
    [TestFixture]
    public sealed class LayerDependencyTests
    {
        private static string CoreNamespace => "NuciLog.Core";

        private static string ExternalAspCorePattern => "^Microsoft\\.AspNetCore.*";

        [Test]
        public void GivenProductionAssembly_WhenLoaded_ThenItContainsTheDocumentedNamespace()
        {
            IReadOnlyList<string> namespaces = ProductionArchitecture.Model.Namespaces
                .Select(namespaceDefinition => namespaceDefinition.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(namespaces, Is.Not.Empty, "The production architecture must declare namespaces.");
            Assert.That(namespaces, Is.EquivalentTo(new[] { CoreNamespace }));
        }

        [Test]
        public void GivenTheDocumentedNamespace_WhenSelected_ThenItContainsAtLeastOneType()
        {
            IObjectProvider<IType> coreTypes = Types().That().ResideInNamespace(CoreNamespace);

            Assert.That(coreTypes.GetObjects(ProductionArchitecture.Model), Is.Not.Empty);
        }

        [Test]
        public void GivenProductionTypes_WhenTheyDependOnAspNetCore_ThenNoDependencyIsAllowed()
        {
            IArchRule rule = Types()
                .That().Are(ProductionArchitecture.ProductionTypes)
                .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(ExternalAspCorePattern))
                .Because("the library is a logging abstraction that must stay independent of ASP.NET Core");

            rule.Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenItsNamespacesAreSliced_ThenNoDependencyCycleExists()
        {
            SliceRule rule = Slices()
                .Matching("NuciLog.Core.(**)")
                .Should().BeFreeOfCycles();

            rule.Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenItsTopLevelNamespacesAreSliced_ThenNoDependencyCycleExists()
        {
            SliceRule rule = Slices()
                .Matching("NuciLog.Core.(*)")
                .Should().BeFreeOfCycles();

            rule.Check(ProductionArchitecture.Model);
        }

        [TestCase("Microsoft.AspNetCore")]
        [TestCase("Microsoft.EntityFrameworkCore")]
        [TestCase("Amazon.DynamoDBv2")]
        [TestCase("Dapper")]
        [TestCase("MediatR")]
        public void GivenProductionTypes_WhenTheyUseInfrastructureLibraries_ThenNoDependencyIsAllowed(
            string infrastructureNamespace)
        {
            IObjectProvider<IType> infrastructureTypes = Types()
                .That()
                .ResideInNamespaceMatching($"^{infrastructureNamespace.Replace(".", "\\.")}.*");

            IArchRule rule = Types()
                .That().Are(ProductionArchitecture.ProductionTypes)
                .Should().NotDependOnAny(infrastructureTypes)
                .Because($"the production library must not depend on {infrastructureNamespace}");

            rule.Check(ProductionArchitecture.Model);
        }
    }
}
