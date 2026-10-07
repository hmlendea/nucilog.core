# Source Map

File-to-responsibility mapping for rapid navigation of the NuciLog.Core codebase.

## 📑 Table of Contents

- [Project Structure](#project-structure)
- [Core Library (NuciLog.Core)](#core-library-nucilogcore)
- [Unit Tests (NuciLog.Core.UnitTests)](#unit-tests-nucilogcoreunittests)
- [Documentation (docs/)](#documentation-docs)
- [Cross-Reference Index](#cross-reference-index)

---

## Project Structure

```
nucilog.core/
├── NuciLog.Core/                    # Core library
│   ├── NuciLog.Core.csproj
│   ├── ILogger.cs                   # Public contract (240 overloads)
│   ├── Logger.cs                    # Abstract base (normalisation + dispatch)
│   ├── LogMessageBuilder.cs         # Stateless formatter (Build, sanitise, mask, de-dup)
│   ├── LogInfo.cs                   # Value carrier (object→string conversion)
│   ├── LogInfoKey.cs                # Field identity + sensitivity
│   ├── Operation.cs                 # Domain operation (extensible)
│   ├── OperationStatus.cs           # Domain status (extensible)
│   ├── LogLevel.cs                  # Severity enum (Fatal=0...Verbose=5)
│   └── NullLogger.cs                # No-op implementation
│
├── NuciLog.Core.UnitTests/          # Test project
│   ├── NuciLog.Core.UnitTests.csproj
│   ├── Helpers/
│   │   ├── TestLogger.cs            # Captures LastLogLine, LastLogLevel
│   │   ├── TestLogInfoKey.cs        # Test keys (sensitive + non-sensitive)
│   │   └── TestLogValues.cs         # Shared expected values
│   ├── LoggerTests1Verbose.cs       # Verbose level (40+ overloads)
│   ├── LoggerTests2Debug.cs         # Debug level (40+ overloads)
│   ├── LoggerTests3Info.cs          # Info level (40+ overloads)
│   ├── LoggerTests4Warn.cs          # Warn level (40+ overloads)
│   ├── LoggerTests5Error.cs         # Error level (40+ overloads)
│   ├── LoggerTests6Fatal.cs         # Fatal level (40+ overloads)
│   ├── LogInfoTests.cs              # LogInfo construction, conversion
│   └── LogMessageBuilderTests.cs    # Builder pipeline, masking, de-dup
│
├── docs/                            # Documentation
│   ├── README.md                    # Documentation index
│   ├── architecture-overview.md     # High-level architecture
│   ├── public-api-reference.md      # Complete member reference
│   ├── record-construction.md       # Grammar, pipeline, masking, de-dup
│   ├── extension-points.md          # Sink, keys, operations, statuses
│   ├── runtime-execution-flows.md   # Call sequences, async patterns
│   ├── data-model-and-transformations.md # Value conversion, pipeline
│   ├── testing-and-verification.md  # Test org, coverage, contracts
│   ├── security-and-privacy.md      # Masking, data flow, compliance
│   ├── compatibility-and-versioning.md # TFM, versioning, compat
│   └── source-map.md                # This file
│
├── NuciLog.slnx                     # Solution file
├── README.md                        # Repository overview
├── PRIVACY.md                       # Privacy policy
├── SECURITY.md                      # Security policy
├── ARCHITECTURE.md                  # Verified architecture (source of truth)
└── ROADMAP.md                       # Future plans
```

---

## Core Library (NuciLog.Core)

### ILogger.cs

**Purpose:** Public consumer contract — all 240 overloads across 6 levels.

**Key Members:**
- `SourceContext` property
- `SetSourceContext<T>()`, `SetSourceContext(Type)`
- 6 level groups × 40 overloads each:
  - `Verbose`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`
- Each group: Operation-only, Message-only, Operation+Message, Operation+Status+Message, with Exception, with LogInfos (params, IEnumerable, both)

**Navigation:**
- Start here to understand the public API surface
- Every overload delegates to `Logger` canonical method

### Logger.cs

**Purpose:** Abstract base implementing overload normalisation and canonical dispatch.

**Key Members:**
- `SourceContext` property (settable)
- `SetSourceContext<T>()`, `SetSourceContext(Type)` (virtual)
- 6 canonical methods (one per level):
  ```csharp
  void Verbose(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>, params LogInfo[])
  void Debug(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>, params LogInfo[])
  void Info(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>, params LogInfo[])
  void Warn(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>, params LogInfo[])
  void Error(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>, params LogInfo[])
  void Fatal(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>, params LogInfo[])
  ```
- `protected abstract void WriteLog(LogLevel level, Func<string> logMessage)`

**Navigation:**
- Canonical methods = single call site per level
- `WriteLog` = extension point for concrete sinks
- Overload forwarding chain: specific → canonical → WriteLog

### LogMessageBuilder.cs

**Purpose:** Stateless record construction, sanitisation, masking, de-duplication.

**Key Members:**
- `public static string Build(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>)`
- `private static IEnumerable<LogInfo> GetProcessedLogInfoList(string, IEnumerable<LogInfo>, Exception)`
- `private static string ProcessLogInfoValue(LogInfo)`
- `private static string ObfuscateLogInfoValue(string)`
- `private static string SanitiseLogInfoValue(string)`
- `private static string SanitiseStackTrace(string)`
- Constants: `ObfuscatedLogInfoValuePrefixLength=4`, `MaskLength=5`, `SuffixLength=2`, `MaskCharacter='*'`
- `NewLineMatchingRegex` for newline sanitisation

**Navigation:**
- `Build` = entry point, called by `Logger` canonical methods via lazy delegate
- `GetProcessedLogInfoList` = pipeline orchestrator
- Processing order: Message → Consumer LogInfos → Exception enrichment → De-dup → Omit vacant

### LogInfo.cs

**Purpose:** Value carrier with object→string conversion at construction.

**Key Members:**
- `Key` (LogInfoKey), `Value` (string) — both readonly
- Constructors:
  - `LogInfo(LogInfoKey, string)`
  - `LogInfo(LogInfoKey, object)` → calls `GetStringValue`
  - `LogInfo(LogInfoKey, DateTime, string format)`
  - `LogInfo(LogInfoKey, DateTimeOffset, string format)`
- `private string GetStringValue(object)` — type-specific formatting

**Navigation:**
- `GetStringValue` = conversion logic for all supported types
- `Value` is always string — conversion happens at construction, not formatting

### LogInfoKey.cs

**Purpose:** Field identity + sensitivity classification.

**Key Members:**
- `Name` (string), `IsSensitive` (bool) — both protected set
- Constructors:
  - `protected LogInfoKey(string name)` → `IsSensitive=false`
  - `protected LogInfoKey(string name, bool isSensitive)`
- `Equals`/`GetHashCode` by `Name` only
- Internal static keys: `SourceContext`, `Operation`, `OperationStatus`, `Message`, `Exception`, `ExceptionMessage`, `StackTrace`

**Navigation:**
- Consumers must subclass — cannot instantiate directly
- `IsSensitive` drives masking in `LogMessageBuilder.ProcessLogInfoValue`
- Equality by `Name` enables de-duplication but creates sensitivity collision edge case

### Operation.cs

**Purpose:** Extensible domain context.

**Key Members:**
- `Name` (string) — protected set
- `protected Operation(string name)`
- Built-ins: `Unknown`, `StartUp`, `ShutDown`

**Navigation:**
- Subclass for domain operations
- Rendered as `Operation＝{Name}` (original case)

### OperationStatus.cs

**Purpose:** Extensible domain status.

**Key Members:**
- `Name` (string) — protected set
- `protected OperationStatus(string name)`
- Built-ins: `Unknown`, `Started`, `Success`, `Failure`, `InProgress`

**Navigation:**
- Subclass for domain statuses
- Rendered as `OperationStatus＝{Name.ToUpper()}` (always upper-cased)

### LogLevel.cs

**Purpose:** Severity enumeration.

**Key Members:**
```csharp
enum LogLevel { Fatal = 0, Error = 1, Warn = 2, Info = 3, Debug = 4, Verbose = 5 }
```

**Navigation:**
- Ordinal = severity (lower = more severe)
- Sink filtering: `if (level > minimumLevel) return;`

### NullLogger.cs

**Purpose:** No-op implementation.

**Key Members:**
- `protected override void WriteLog(LogLevel level, Func<string> logMessage) { }`

**Navigation:**
- Valid sink — formatter never invoked, zero cost
- Use for testing or disabling logging

---

## Unit Tests (NuciLog.Core.UnitTests)

### Helpers/TestLogger.cs

**Purpose:** Test sink capturing formatted output.

**Key Members:**
- `LastLogLine` (string) — result of `logMessage()`
- `LastLogLevel` (LogLevel) — level passed to `WriteLog`
- `WriteLog` invokes formatter and stores both

**Navigation:**
- Used in all `LoggerTests*` classes
- Verifies level dispatch and formatted output

### Helpers/TestLogInfoKey.cs

**Purpose:** Test keys with known names and sensitivity.

**Key Members:**
- `TestKey` (non-sensitive)
- `TestKey2` (non-sensitive, different name)
- `SensitiveTestKey` (sensitive, same name as TestKey for de-dup tests)

**Navigation:**
- Used in `LogMessageBuilderTests` for masking/de-dup verification
- `SensitiveTestKey` name = `TestKey` → tests sensitivity collision

### Helpers/TestLogValues.cs

**Purpose:** Shared expected values.

**Key Members:**
- `DefaultExceptionLogMessage` = "An exception has occurred."

**Navigation:**
- Used in exception enrichment tests

### LoggerTests1Verbose.cs through LoggerTests6Fatal.cs

**Purpose:** Per-level overload verification.

**Structure:** Each file tests all 40 overloads for its level.

**Navigation:**
- Tests follow `Given{Precondition}_When{Action}_Then{ExpectedOutcome}` naming
- Verify `LastLogLevel` and `LastLogLine` content
- Canonical path coverage via `TestLogger`

### LogInfoTests.cs

**Purpose:** `LogInfo` construction and value conversion.

**Coverage:**
- All `GetStringValue` type branches
- Null handling
- Format overloads (DateTime, DateTimeOffset)
- Edge cases (empty collections, nested dictionaries)

### LogMessageBuilderTests.cs

**Purpose:** Builder pipeline verification.

**Coverage:**
- Sanitisation (newlines, tabs)
- Masking algorithm (prefix/suffix lengths, vacant check)
- De-duplication (first key, last value, vacant omission)
- Exception enrichment (3 fields, stack trace sanitisation)
- Sensitivity collision edge case
- Delimiter handling (fullwidth equals, Greek numeral)
- Trailing delimiter trim

---

## Documentation (docs/)

| File | Purpose | Source Files Referenced |
|------|---------|------------------------|
| `README.md` | Documentation index | All |
| `architecture-overview.md` | High-level architecture | `ARCHITECTURE.md`, all core files |
| `public-api-reference.md` | Complete member reference | All core files |
| `record-construction.md` | Grammar, pipeline, masking, de-dup | `LogMessageBuilder.cs`, `LogInfo.cs`, `LogInfoKey.cs` |
| `extension-points.md` | Sink, keys, operations, statuses | `Logger.cs`, `LogInfoKey.cs`, `Operation.cs`, `OperationStatus.cs` |
| `runtime-execution-flows.md` | Call sequences, async patterns | `Logger.cs`, `LogMessageBuilder.cs`, `ILogger.cs` |
| `data-model-and-transformations.md` | Value conversion, pipeline | `LogInfo.cs`, `LogMessageBuilder.cs`, `LogInfoKey.cs` |
| `testing-and-verification.md` | Test org, coverage, contracts | All test files |
| `security-and-privacy.md` | Masking, data flow, compliance | `LogMessageBuilder.cs`, `LogInfoKey.cs`, `LogInfo.cs` |
| `compatibility-and-versioning.md` | TFM, versioning, compat | `.csproj`, all public APIs |
| `source-map.md` | This file | All |

---

## Cross-Reference Index

### By Concept

| Concept | Primary File | Secondary Files |
|---------|--------------|-----------------|
| **Public API** | `ILogger.cs` | `Logger.cs` |
| **Overload normalisation** | `Logger.cs` | `ILogger.cs` |
| **Canonical dispatch** | `Logger.cs` (canonical methods) | `LogMessageBuilder.cs` |
| **Lazy formatter** | `Logger.cs` (delegate) | `LogMessageBuilder.cs` |
| **Sink extension** | `Logger.cs` (`WriteLog`) | `NullLogger.cs` |
| **Record formatting** | `LogMessageBuilder.cs` (`Build`) | `LogInfo.cs`, `LogInfoKey.cs` |
| **Value conversion** | `LogInfo.cs` (`GetStringValue`) | `LogMessageBuilder.cs` |
| **Sanitisation** | `LogMessageBuilder.cs` (`SanitiseLogInfoValue`, `SanitiseStackTrace`) | |
| **Sensitive masking** | `LogMessageBuilder.cs` (`ProcessLogInfoValue`, `ObfuscateLogInfoValue`) | `LogInfoKey.cs` |
| **De-duplication** | `LogMessageBuilder.cs` (`GetProcessedLogInfoList` GroupBy) | `LogInfoKey.cs` (Equality) |
| **Exception enrichment** | `LogMessageBuilder.cs` (exception block in `GetProcessedLogInfoList`) | |
| **Field keys** | `LogInfoKey.cs` | `LogMessageBuilder.cs` (internal keys) |
| **Operations** | `Operation.cs` | `LogMessageBuilder.cs` (rendering) |
| **Statuses** | `OperationStatus.cs` | `LogMessageBuilder.cs` (upper-casing) |
| **Levels** | `LogLevel.cs` | `Logger.cs` (canonical methods) |
| **Testing** | `TestLogger.cs` | All `LoggerTests*.cs` |
| **Test keys** | `TestLogInfoKey.cs` | `LogMessageBuilderTests.cs` |

### By File → Responsibility

| File | Primary Responsibility |
|------|------------------------|
| `ILogger.cs` | Public contract definition |
| `Logger.cs` | Overload normalisation, canonical dispatch, sink abstraction |
| `LogMessageBuilder.cs` | Formatting pipeline (sanitise, mask, de-dup, enrich) |
| `LogInfo.cs` | Value object + object→string conversion |
| `LogInfoKey.cs` | Field identity + sensitivity flag |
| `Operation.cs` | Domain operation (extensible) |
| `OperationStatus.cs` | Domain status (extensible) |
| `LogLevel.cs` | Severity enumeration |
| `NullLogger.cs` | No-op sink |
| `TestLogger.cs` | Test sink (capture output) |
| `TestLogInfoKey.cs` | Test keys (sensitive + non-sensitive) |
| `TestLogValues.cs` | Shared test constants |
| `LoggerTests*.cs` | Per-level overload verification |
| `LogInfoTests.cs` | Value conversion verification |
| `LogMessageBuilderTests.cs` | Pipeline verification |

### By Extension Point → Files

| Extension Point | Base File | Consumer Pattern |
|-----------------|-----------|------------------|
| **Concrete sink** | `Logger.cs` (`WriteLog`) | Subclass `Logger`, override `WriteLog` |
| **Field keys** | `LogInfoKey.cs` | Subclass, add static properties, use protected constructors |
| **Operations** | `Operation.cs` | Subclass, add static properties, use protected constructor |
| **Statuses** | `OperationStatus.cs` | Subclass, add static properties, use protected constructor |

---

## Quick Navigation Commands

### Find All Overloads for a Level

```bash
grep -n "void Info" NuciLog.Core/ILogger.cs
grep -n "void Info" NuciLog.Core/Logger.cs
```

### Find Masking Logic

```bash
grep -n "ObfuscateLogInfoValue\|IsSensitive" NuciLog.Core/LogMessageBuilder.cs
grep -n "IsSensitive" NuciLog.Core/LogInfoKey.cs
```

### Find De-duplication Logic

```bash
grep -n "GroupBy" NuciLog.Core/LogMessageBuilder.cs
```

### Find Value Conversion

```bash
grep -n "GetStringValue" NuciLog.Core/LogInfo.cs
```

### Find Exception Enrichment

```bash
grep -n "Exception" NuciLog.Core/LogMessageBuilder.cs | head -20
```

### Find Delimiter Characters

```bash
grep -n "＝\|͵" NuciLog.Core/LogMessageBuilder.cs
```

### Find Test for Specific Behaviour

```bash
grep -rn "GivenDuplicatedLogInfosWithASensitiveFinalValue" NuciLog.Core.UnitTests/
grep -rn "SanitiseStackTrace" NuciLog.Core.UnitTests/
```

---

## Symbol Index

### Public Types

| Symbol | File | Kind |
|--------|------|------|
| `ILogger` | `ILogger.cs` | Interface |
| `Logger` | `Logger.cs` | Abstract class |
| `LogMessageBuilder` | `LogMessageBuilder.cs` | Static class |
| `LogInfo` | `LogInfo.cs` | Sealed class |
| `LogInfoKey` | `LogInfoKey.cs` | Class (extensible) |
| `Operation` | `Operation.cs` | Class (extensible) |
| `OperationStatus` | `OperationStatus.cs` | Class (extensible) |
| `LogLevel` | `LogLevel.cs` | Enum |
| `NullLogger` | `NullLogger.cs` | Sealed class |

### Key Methods

| Symbol | File | Signature |
|--------|------|-----------|
| `Logger.WriteLog` | `Logger.cs` | `protected abstract void WriteLog(LogLevel, Func<string>)` |
| `LogMessageBuilder.Build` | `LogMessageBuilder.cs` | `public static string Build(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>)` |
| `LogMessageBuilder.GetProcessedLogInfoList` | `LogMessageBuilder.cs` | `private static IEnumerable<LogInfo> GetProcessedLogInfoList(string, IEnumerable<LogInfo>, Exception)` |
| `LogInfo.GetStringValue` | `LogInfo.cs` | `private string GetStringValue(object)` |
| `LogInfoKey.Equals` | `LogInfoKey.cs` | `public bool Equals(LogInfoKey)` |
| `LogInfoKey.GetHashCode` | `LogInfoKey.cs` | `public override int GetHashCode()` |

### Internal Keys (LogMessageBuilder)

| Key | Purpose |
|-----|---------|
| `LogInfoKey.SourceContext` | Logger source type (not formatted) |
| `LogInfoKey.Operation` | Operation name |
| `LogInfoKey.OperationStatus` | Operation status (upper-cased) |
| `LogInfoKey.Message` | Log message text |
| `LogInfoKey.Exception` | Exception runtime type |
| `LogInfoKey.ExceptionMessage` | Exception message |
| `LogInfoKey.StackTrace` | Exception stack trace |

---

## Change Impact Map

| File Changed | Affects |
|--------------|---------|
| `ILogger.cs` | All consumers (source + binary) |
| `Logger.cs` canonical methods | All sinks (binary), consumers using canonical directly (source) |
| `Logger.cs` overloads | Consumers using specific overloads (source) |
| `LogMessageBuilder.Build` | All formatted output (behavioural) |
| `LogMessageBuilder` sanitisation/masking | All formatted output (behavioural) |
| `LogInfo.GetStringValue` | All `LogInfo(object)` constructions (behavioural) |
| `LogInfoKey` constructors | All custom key subclasses (source + binary) |
| `Operation` constructor | All custom operation subclasses (source + binary) |
| `OperationStatus` constructor | All custom status subclasses (source + binary) |
| `LogLevel` values | All level filtering logic (behavioural) |
| `NullLogger` | None (leaf implementation) |

---

## Summary

| Layer | Files | Responsibility |
|-------|-------|----------------|
| **Contract** | `ILogger.cs` | Public API surface |
| **Core** | `Logger.cs`, `LogMessageBuilder.cs`, `LogInfo.cs`, `LogInfoKey.cs`, `Operation.cs`, `OperationStatus.cs`, `LogLevel.cs` | Implementation |
| **Sink** | `NullLogger.cs` | Reference implementation |
| **Test** | `TestLogger.cs`, `TestLogInfoKey.cs`, `TestLogValues.cs`, `LoggerTests*.cs`, `LogInfoTests.cs`, `LogMessageBuilderTests.cs` | Verification |
| **Docs** | `docs/*.md` | Externalised knowledge |

**Navigation principle:** Start at `ILogger.cs` for API, `Logger.cs` for dispatch, `LogMessageBuilder.cs` for formatting, `LogInfo.cs`/`LogInfoKey.cs` for data model.