using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using Assembly = System.Reflection.Assembly;

namespace NuciLog.Core.ArchitectureTests
{
    internal static class ProductionArchitecture
    {
        internal static Assembly Assembly { get; } = typeof(ILogger).Assembly;

        internal static Architecture Model { get; } = new ArchLoader()
            .LoadAssemblies(Assembly)
            .Build();

        internal static IObjectProvider<IType> ProductionTypes { get; } = Types()
            .That()
            .ResideInAssembly(Assembly)
            .As("NuciLog.Core production types");
    }
}
