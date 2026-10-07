# Compatibility and Versioning

Binary compatibility, source compatibility, target framework strategy, and versioning model for NuciLog.Core.

## 📑 Table of Contents

- [Target Framework Strategy](#target-framework-strategy)
- [Versioning Model](#versioning-model)
- [Binary Compatibility](#binary-compatibility)
- [Source Compatibility](#source-compatibility)
- [Packaging Model](#packaging-model)
- [Dependency Management](#dependency-management)
- [Breaking Change Policy](#breaking-change-policy)
- [Migration Guides](#migration-guides)

---

## Target Framework Strategy

### Current Target

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

### Single TFM Rationale

| Factor | Decision |
|--------|----------|
| **Simplicity** | One build artifact, one test matrix |
| **Modern APIs** | Access to latest .NET 10 features (if needed) |
| **No legacy burden** | No netstandard2.0, net462, net6.0 compatibility shims |
| **Performance** | Optimised for single runtime |

### Future Multi-TFM Considerations

If multi-targeting becomes necessary:

```xml
<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
```

**Compatibility requirements for older TFMs:**
- `net8.0`: Requires `System.Text.Json` package reference (built-in in 9.0+)
- `net6.0`/`net7.0`: Requires `Nullable` context polyfill
- `netstandard2.0`: Significant API surface reduction (no `ILogger` overloads with `params` + `IEnumerable`)

**Current stance:** Single TFM (`net10.0`) until explicit demand for older runtimes.

---

## Versioning Model

### Semantic Versioning (SemVer 2.0)

```
MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]
```

| Component | Incremented When |
|-----------|------------------|
| **MAJOR** | Breaking changes to public API |
| **MINOR** | New functionality, backward compatible |
| **PATCH** | Bug fixes, backward compatible |
| **PRERELEASE** | Alpha, beta, RC builds |
| **BUILD** | CI metadata (commit hash, build number) |

### Version Examples

| Version | Meaning |
|---------|---------|
| `1.0.0` | First stable release |
| `1.1.0` | New extension point, new built-in key |
| `1.1.1` | Bug fix in masking algorithm |
| `2.0.0` | Breaking change (e.g., `LogInfoKey` constructor signature) |
| `1.2.0-alpha.1` | Pre-release for next minor |
| `1.2.0-beta.3` | Beta release |
| `1.2.0-rc.1` | Release candidate |

### Release Cadence

| Release Type | Frequency | Version Bump |
|--------------|-----------|--------------|
| **Patch** | As needed | PATCH |
| **Minor** | Quarterly | MINOR |
| **Major** | Annually or as needed | MAJOR |
| **Pre-release** | Per minor cycle | PRERELEASE |

---

## Binary Compatibility

### Definition

**Binary compatibility** = Existing compiled consumer assemblies work without recompilation against new library version.

### Compatibility Matrix

| Change | Binary Compatible? | Version Bump |
|--------|-------------------|--------------|
| Add new `LogInfoKey` static property | ✅ Yes | MINOR |
| Add new `Operation` static property | ✅ Yes | MINOR |
| Add new `OperationStatus` static property | ✅ Yes | MINOR |
| Add new `ILogger` overload | ✅ Yes | MINOR |
| Add new `Logger` virtual method | ✅ Yes | MINOR |
| Change `LogInfoKey` constructor accessibility | ❌ No | MAJOR |
| Change `Operation` constructor accessibility | ❌ No | MAJOR |
| Change `OperationStatus` constructor accessibility | ❌ No | MAJOR |
| Remove `ILogger` overload | ❌ No | MAJOR |
| Change `LogLevel` enum values | ❌ No | MAJOR |
| Change `LogMessageBuilder.Build` signature | ❌ No | MAJOR |
| Change `WriteLog` signature | ❌ No | MAJOR |
| Change `LogInfo` constructor signatures | ❌ No | MAJOR |
| Change `LogInfo.Value` type | ❌ No | MAJOR |

### Safe Changes (MINOR)

```csharp
// Adding built-in keys — safe
internal static LogInfoKey NewInternalKey => new(nameof(NewInternalKey));

// Adding virtual method to Logger — safe
public virtual void Flush() { }

// Adding new ILogger overload — safe (consumers don't implement ILogger directly)
void Info(Operation operation, string message, object extraContext);
```

### Breaking Changes (MAJOR)

```csharp
// Changing constructor accessibility — breaks subclasses
// Before: protected LogInfoKey(string name)
// After:  public LogInfoKey(string name)  // BREAKING

// Removing ILogger overload — breaks consumers using that overload
// void Info(Operation operation);  // REMOVED — BREAKING

// Changing LogLevel values — breaks level filtering logic
// enum LogLevel { Fatal = 0, Error = 100, ... }  // BREAKING
```

### Binary Compatibility Testing

```bash
# Build consumer against v1.0.0
dotnet build ConsumerApp.csproj

# Replace NuciLog.Core.dll with v1.1.0 (no rebuild)
# Run consumer — should work

# Replace with v2.0.0 — may fail with MissingMethodException
```

---

## Source Compatibility

### Definition

**Source compatibility** = Existing consumer source code compiles without changes against new library version.

### Compatibility Matrix

| Change | Source Compatible? | Version Bump |
|--------|-------------------|--------------|
| Add new `LogInfoKey` static property | ✅ Yes | MINOR |
| Add new `Operation` static property | ✅ Yes | MINOR |
| Add new `OperationStatus` static property | ✅ Yes | MINOR |
| Add new `ILogger` overload | ✅ Yes | MINOR |
| Add new `Logger` virtual method | ✅ Yes | MINOR |
| Add new `LogInfo` constructor | ✅ Yes | MINOR |
| Change `LogInfoKey` constructor accessibility | ❌ No | MAJOR |
| Change `Operation` constructor accessibility | ❌ No | MAJOR |
| Change `OperationStatus` constructor accessibility | ❌ No | MAJOR |
| Remove `ILogger` overload | ❌ No | MAJOR |
| Change `LogLevel` enum values | ⚠️ Maybe* | MAJOR |
| Change `LogMessageBuilder.Build` signature | ❌ No | MAJOR |
| Change `WriteLog` signature | ❌ No | MAJOR |
| Change `LogInfo.Value` type | ❌ No | MAJOR |
| Rename public type/member | ❌ No | MAJOR |

*Changing enum values: Source compatible if consumer uses names (`LogLevel.Info`), not values (`3`).

### Source Compatibility Testing

```bash
# Update package reference
dotnet add ConsumerApp.csproj package NuciLog.Core --version 1.1.0

# Build — should succeed
dotnet build ConsumerApp.csproj
```

---

## Packaging Model

### NuGet Package

```xml
<PropertyGroup>
  <PackageId>NuciLog.Core</PackageId>
  <Version>1.0.0</Version>
  <Authors>NuciLog Contributors</Authors>
  <Description>Structured logging abstraction for .NET</Description>
  <PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/nucilog/nucilog.core</PackageProjectUrl>
  <RepositoryUrl>https://github.com/nucilog/nucilog.core</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

### Package Contents

```
NuciLog.Core.1.0.0.nupkg
├── lib/
│   └── net10.0/
│       ├── NuciLog.Core.dll
│       └── NuciLog.Core.xml          # XML documentation
├── NuciLog.Core.1.0.0.nuspec
└── [Content_Types].xml
```

### Symbol Package

```
NuciLog.Core.1.0.0.snupkg
├── lib/
│   └── net10.0/
│       └── NuciLog.Core.pdb          # Portable PDB with embedded source
```

### Source Link

Enabled via:
```xml
<PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
```

**Effect:** Debugger steps into library source from GitHub.

---

## Dependency Management

### Direct Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| *None* | — | Zero external dependencies |

### Development Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `NUnit` | 4.2.2 | Test framework |
| `NUnit3TestAdapter` | 4.6.0 | VS Test integration |
| `Microsoft.NET.Test.Sdk` | 17.11.1 | Test runner |
| `Microsoft.SourceLink.GitHub` | 8.0.0 | Source Link |

### Dependency Policy

| Policy | Rule |
|--------|------|
| **Zero runtime dependencies** | Library has no `PackageReference` with `PrivateAssets != All` |
| **No transitive dependencies** | Consumers get only `NuciLog.Core.dll` |
| **Framework APIs only** | Uses only `System.*` namespaces |
| **No native dependencies** | Pure managed code |

---

## Breaking Change Policy

### Classification

| Severity | Examples | Process |
|----------|----------|---------|
| **Source-breaking** | Removed overload, renamed type | MAJOR version, migration guide |
| **Binary-breaking** | Changed method signature, removed virtual | MAJOR version, migration guide |
| **Behavioural** | Changed masking algorithm, delimiter | MINOR (if opt-in) or MAJOR (if default) |
| **Security** | Fixed vulnerability | PATCH (if no API change) or MINOR/MAJOR |

### Deprecation Process

1. **Mark obsolete** with `ObsoleteAttribute` and message
2. **Keep for one MINOR cycle** (e.g., deprecated in 1.2, removed in 2.0)
3. **Document migration** in release notes and migration guide

```csharp
[Obsolete("Use Info(Operation, OperationStatus, string, IEnumerable<LogInfo>) instead. Will be removed in 2.0.")]
public void Info(Operation operation, string message, params LogInfo[] logInfos)
{
    // ... implementation
}
```

### Breaking Change Checklist

Before releasing a MAJOR version:

- [ ] All breaking changes documented in `CHANGELOG.md`
- [ ] Migration guide created (`docs/migration-v{major}.md`)
- [ ] Obsolete members marked in previous MINOR
- [ ] Tests updated for new API
- [ ] Samples updated
- [ ] Documentation updated

---

## Migration Guides

### Template: Migration to v{MAJOR}.0

```markdown
# Migration Guide: NuciLog.Core v{MAJOR}.0

## Breaking Changes

### 1. {Change Title}

**Before (v{MAJOR-1}.x):**
```csharp
// Old code
```

**After (v{MAJOR}.0):**
```csharp
// New code
```

**Reason:** {Why the change was made}

**Impact:** {Who is affected}

---

### 2. {Change Title}
...

## Non-Breaking Changes

### New Features
- {Feature}

### Bug Fixes
- {Fix}

## Upgrade Steps

1. Update package: `dotnet add package NuciLog.Core --version {MAJOR}.0.0`
2. Fix compiler errors (see breaking changes above)
3. Run tests
4. Verify log output format
```

### Example: Hypothetical v2.0 Migration

```markdown
# Migration Guide: NuciLog.Core v2.0.0

## Breaking Changes

### 1. LogInfoKey Constructors Made Public

**Before (v1.x):**
```csharp
public sealed class AppLogKey : LogInfoKey
{
    public AppLogKey(string name) : base(name) { }  // protected base
    private AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}
```

**After (v2.0):**
```csharp
public sealed class AppLogKey : LogInfoKey
{
    public AppLogKey(string name) : base(name) { }  // public base
    public AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}
```

**Reason:** Enable direct instantiation without subclassing (simpler for simple cases).

**Impact:** All custom `LogInfoKey` subclasses must change `private` constructor to `public`.

---

### 2. LogLevel Enum Values Changed

**Before:** `Fatal=0, Error=1, Warn=2, Info=3, Debug=4, Verbose=5`
**After:** `Fatal=100, Error=200, Warn=300, Info=400, Debug=500, Verbose=600`

**Reason:** Align with common logging frameworks (Serilog, NLog, Microsoft.Extensions.Logging).

**Impact:** Level filtering logic using numeric comparison breaks.

**Fix:** Use enum names, not values:
```csharp
// Before
if ((int)level > 3) ...

// After
if (level > LogLevel.Info) ...
```

## Upgrade Steps

1. `dotnet add package NuciLog.Core --version 2.0.0`
2. Change `private` → `public` in `LogInfoKey` subclass constructors
3. Replace numeric `LogLevel` comparisons with enum comparisons
4. Run tests
```

---

## Compatibility Summary

| Dimension | Current State | Policy |
|-----------|---------------|--------|
| **Target Framework** | `net10.0` (single) | Multi-TFM on demand |
| **Versioning** | SemVer 2.0 | MAJOR.MINOR.PATCH |
| **Binary Compatibility** | Preserved for MINOR/PATCH | Tested via consumer rebuild |
| **Source Compatibility** | Preserved for MINOR/PATCH | Tested via consumer rebuild |
| **Dependencies** | Zero runtime deps | Maintained |
| **Breaking Changes** | MAJOR only | Deprecation cycle |
| **Symbols** | Source Link + snupkg | Enabled |
| **License** | GPL-3.0-or-later | Fixed |

---

## Version History

| Version | Date | Type | Notes |
|---------|------|------|-------|
| `1.0.0` | TBD | MAJOR | Initial release |

---

## Future Considerations

### Potential MAJOR Changes (v2.0)

1. **Align `LogLevel` with industry standard** (Fatal=100, Error=200, etc.)
2. **Public `LogInfoKey` constructors** (simpler extension)
3. **`ILogger` as abstract class** (reduce overload burden)
4. **Structured logging with `IReadOnlyList<LogInfo>`** (immutability)
5. **Async `WriteLogAsync`** (first-class async sinks)

### Potential MINOR Changes (v1.x)

1. **Additional built-in keys** (CorrelationId, TraceId, SpanId)
2. **Additional built-in operations** (HttpRequest, DatabaseQuery)
3. **Additional built-in statuses** (Retrying, CircuitOpen)
4. **`LogMessageBuilder` extensibility hooks** (custom sanitisation)
5. **Source generator for `ILogger` overloads** (reduce boilerplate)

---

## Verification Checklist

Before each release:

- [ ] Version number follows SemVer
- [ ] `CHANGELOG.md` updated
- [ ] Package builds without warnings
- [ ] Symbol package generated
- [ ] Source Link verified (debugger steps into source)
- [ ] Binary compatibility test passes (if MINOR/PATCH)
- [ ] Source compatibility test passes (if MINOR/PATCH)
- [ ] All tests pass on `net10.0`
- [ ] Documentation updated
- [ ] Migration guide created (if MAJOR)