using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    /// <summary>
    /// Enforces the interface, enumeration, and exception conventions on the compiled
    /// production surface: interface naming and pairing, enumeration placement, and
    /// exception accessibility and naming.
    /// </summary>
    [TestFixture]
    public sealed class InterfaceAndEnumerationConventionTests
    {
        private static Assembly ProductionAssembly => ProductionArchitecture.Assembly;

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenInterfacesUseTheIPrefixAndPascalCase()
        {
            string[] invalidNames = ProductionAssembly.GetTypes()
                .Where(type => type.IsInterface)
                .Select(type => type.Name)
                .Where(name => !name.StartsWith('I') || name.Length < 2 || !char.IsUpper(name[1]))
                .ToArray();

            Assert.That(
                invalidNames,
                Is.Empty,
                "Interface names must use the 'I' prefix followed by PascalCase.");
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenTheLoggerContractIsColocatedWithItsImplementation()
        {
            Type loggerInterface = typeof(ILogger);
            Type loggerImplementation = typeof(Logger);

            Assert.That(
                loggerInterface.Namespace,
                Is.EqualTo(loggerImplementation.Namespace),
                "ILogger must be colocated with Logger in the same namespace.");
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenNoInterfacesNamespaceIsDeclared()
        {
            string[] interfacesNamespaces = ProductionAssembly.GetTypes()
                .Select(type => type.Namespace ?? string.Empty)
                .Where(nameSpace => nameSpace.EndsWith(".Interfaces", StringComparison.Ordinal))
                .Distinct()
                .ToArray();

            Assert.That(
                interfacesNamespaces,
                Is.Empty,
                "No '.Interfaces' namespace is allowed; interfaces are colocated with their implementations.");
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenEnumerationsResideInDomainNamespace()
        {
            Type[] enums = ProductionAssembly.GetTypes()
                .Where(type => type.IsEnum)
                .ToArray();

            Assert.That(enums, Is.Not.Empty, "The production assembly must declare at least one enumeration.");

            string[] misplacedEnums = enums
                .Where(type => type.Namespace != "NuciLog.Core")
                .Select(type => type.Name)
                .ToArray();

            Assert.That(
                misplacedEnums,
                Is.Empty,
                "Enumerations must reside in the domain namespace, never a dedicated '.Enumerations' or '.Enums' namespace.");
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenNoCustomExceptionsAreDeclared()
        {
            Type[] customExceptions = ProductionAssembly.GetTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type) && type != typeof(Exception))
                .ToArray();

            Assert.That(
                customExceptions,
                Is.Empty,
                "The library must use base class library exceptions and declare no custom exception types.");
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenExtensibleDomainValuesExposeProtectedConstructors()
        {
            Type[] extensibleTypes = [typeof(Operation), typeof(OperationStatus), typeof(LogInfoKey)];

            foreach (Type type in extensibleTypes)
            {
                ConstructorInfo[] constructors = type.GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                Assert.That(
                    constructors.Where(constructor => constructor.IsFamily),
                    Is.Not.Empty,
                    $"{type.Name} must expose a protected constructor so consumers can derive new values.");
            }
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenEqualityConsistencyHoldsForLogInfoKey()
        {
            Type key = typeof(LogInfoKey);

            PropertyInfo? name = key.GetProperty("Name");

            Assert.That(name, Is.Not.Null, "LogInfoKey must expose Name.");
            Assert.That(name!.PropertyType, Is.EqualTo(typeof(string)), "LogInfoKey.Name must be a string.");
            Assert.That(
                typeof(IEquatable<>).MakeGenericType(key).IsAssignableFrom(key),
                Is.True,
                "LogInfoKey must implement IEquatable<LogInfoKey>.");
        }

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenPublicDataObjectsAreSealed()
        {
            Assert.That(typeof(LogInfo).IsSealed, Is.True, "LogInfo must be sealed.");
        }
    }
}
