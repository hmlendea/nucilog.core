using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    [TestFixture]
    public sealed class CompiledConventionTests
    {
        private static Assembly ProductionAssembly => ProductionArchitecture.Assembly;

        [Test]
        public void GivenProductionTypes_WhenChecked_ThenNoTypeExposesPublicFields()
        {
            FieldInfo[] publicFields = ProductionAssembly.GetTypes()
                .Where(type => !type.IsEnum)
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(field => !field.IsSpecialName)
                .Where(field => !IsCompilerGenerated(field))
                .ToArray();

            Assert.That(
                publicFields,
                Is.Empty,
                "Production types must expose properties, never public fields.");
        }

        [Test]
        public void GivenProductionTypes_WhenEligibleClassesAreChecked_ThenTheyAreSealed()
        {
            Type[] unsealedClasses = ProductionAssembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && !type.Name.Contains('<'))
                .Where(type => type.Namespace is not null && type.Namespace.StartsWith("NuciLog.Core", StringComparison.Ordinal))
                .Where(type => !IsExtensibleDomainModel(type))
                .Where(type => !type.IsSealed)
                .ToArray();

            Assert.That(
                unsealedClasses,
                Is.Empty,
                "Every non-static, non-abstract production class must be sealed, except extensible domain models and the abstract base.");
        }

        [Test]
        public void GivenLogger_WhenInspected_ThenItIsAbstractAndImplementsILogger()
        {
            Type logger = typeof(Logger);

            Assert.That(logger.IsAbstract, Is.True, "Logger must be abstract.");
            Assert.That(logger.IsSealed, Is.False, "Logger must be extendable.");
            Assert.That(typeof(ILogger).IsAssignableFrom(logger), Is.True, "Logger must implement ILogger.");
            Assert.That(logger.Namespace, Is.EqualTo("NuciLog.Core"));
        }

        [Test]
        public void GivenLogger_WhenInspected_ThenItDeclaresTheWriteLogHook()
        {
            MethodInfo? writeLog = typeof(Logger).GetMethod(
                "WriteLog",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.That(writeLog, Is.Not.Null, "Logger must declare the WriteLog hook.");
            Assert.That(writeLog!.IsAbstract, Is.True, "WriteLog must be abstract.");
            Assert.That(writeLog.IsFamily, Is.True, "WriteLog must remain protected.");
            Assert.That(writeLog.GetParameters(), Has.Length.EqualTo(2));
            Assert.That(writeLog.GetParameters()[0].ParameterType, Is.EqualTo(typeof(LogLevel)));
            Assert.That(writeLog.GetParameters()[1].ParameterType, Is.EqualTo(typeof(Func<string>)));
        }

        [Test]
        public void GivenNullLogger_WhenInspected_ThenItIsASealedNoOpSink()
        {
            Assert.That(typeof(NullLogger).IsSealed, Is.True, "NullLogger must be sealed.");
            Assert.That(typeof(NullLogger).BaseType, Is.EqualTo(typeof(Logger)));
            Assert.That(typeof(NullLogger).Namespace, Is.EqualTo("NuciLog.Core"));
        }

        [Test]
        public void GivenLogMessageBuilder_WhenInspected_ThenItIsAStaticUtility()
        {
            Type builder = typeof(LogMessageBuilder);

            Assert.That(builder.IsAbstract && builder.IsSealed, Is.True, "LogMessageBuilder must be static.");
            Assert.That(builder.Namespace, Is.EqualTo("NuciLog.Core"));
        }

        [Test]
        public void GivenLogLevelEnum_WhenInspected_ThenItPinsTheDocumentedSeverityContract()
        {
            Type logLevel = typeof(LogLevel);

            Assert.That(logLevel.IsEnum, Is.True, "LogLevel must be an enum.");
            Assert.That(
                Enum.GetNames(logLevel),
                Is.EquivalentTo(new[] { "Fatal", "Error", "Warn", "Info", "Debug", "Verbose" }));

            Assert.That((int)Enum.Parse(logLevel, "Fatal"), Is.EqualTo(0));
            Assert.That((int)Enum.Parse(logLevel, "Error"), Is.EqualTo(1));
            Assert.That((int)Enum.Parse(logLevel, "Warn"), Is.EqualTo(2));
            Assert.That((int)Enum.Parse(logLevel, "Info"), Is.EqualTo(3));
            Assert.That((int)Enum.Parse(logLevel, "Debug"), Is.EqualTo(4));
            Assert.That((int)Enum.Parse(logLevel, "Verbose"), Is.EqualTo(5));
        }

        [Test]
        public void GivenLogInfo_WhenInspected_ThenItIsASealedImmutableCarrier()
        {
            Type logInfo = typeof(LogInfo);

            Assert.That(logInfo.IsSealed, Is.True, "LogInfo must be sealed.");
            Assert.That(logInfo.GetProperty("Key")!.CanWrite, Is.False, "LogInfo.Key must be read-only.");
            Assert.That(logInfo.GetProperty("Value")!.CanWrite, Is.False, "LogInfo.Value must be read-only.");
        }

        [Test]
        public void GivenLogInfoKey_WhenInspected_ThenItImplementsEquatableAndHidesConstructors()
        {
            Type key = typeof(LogInfoKey);

            Assert.That(typeof(IEquatable<LogInfoKey>).IsAssignableFrom(key), Is.True);
            Assert.That(
                key.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                Is.Empty,
                "LogInfoKey must not expose public constructors; it is extended through subclassing.");
        }

        [Test]
        public void GivenOperationAndOperationStatus_WhenInspected_ThenTheyAreExtensibleDomainValues()
        {
            Assert.That(typeof(Operation).IsAbstract, Is.False, "Operation must be extendable.");
            Assert.That(typeof(Operation).IsSealed, Is.False, "Operation must be extendable.");
            Assert.That(typeof(OperationStatus).IsAbstract, Is.False, "OperationStatus must be extendable.");
            Assert.That(typeof(OperationStatus).IsSealed, Is.False, "OperationStatus must be extendable.");

            Assert.That(
                typeof(Operation).GetProperty("Name")!.PropertyType,
                Is.EqualTo(typeof(string)),
                "Operation.Name must expose the operation identifier.");

            Assert.That(
                typeof(OperationStatus).GetProperty("Name")!.PropertyType,
                Is.EqualTo(typeof(string)),
                "OperationStatus.Name must expose the status identifier.");

            ConstructorInfo[] operationConstructors = typeof(Operation)
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.That(
                operationConstructors.Where(constructor => constructor.IsFamily),
                Is.Not.Empty,
                "Operation must expose a protected constructor so consumers can derive new operations.");

            ConstructorInfo[] statusConstructors = typeof(OperationStatus)
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.That(
                statusConstructors.Where(constructor => constructor.IsFamily),
                Is.Not.Empty,
                "OperationStatus must expose a protected constructor so consumers can derive new statuses.");
        }

        [Test]
        public void GivenProductionTypes_WhenInspected_ThenCustomExceptionTypesAreAbsent()
        {
            Type[] exceptions = ProductionAssembly.GetTypes()
                .Where(type => typeof(Exception).IsAssignableFrom(type) && type != typeof(Exception))
                .ToArray();

            Assert.That(
                exceptions,
                Is.Empty,
                "The library must use base class library exceptions and declare no custom exception types.");
        }

        private static bool IsExtensibleDomainModel(Type type)
            => type == typeof(Operation) ||
                type == typeof(OperationStatus) ||
                type == typeof(LogInfoKey);

        private static bool IsCompilerGenerated(FieldInfo field)
            => field.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
                field.DeclaringType is not null &&
                field.DeclaringType.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }
}
