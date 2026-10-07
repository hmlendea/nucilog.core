# Public API Reference

Complete reference for NuciLog.Core public types and members, derived from source code. This document covers the consumer-facing contract (`ILogger`, `Logger`, and public data types).

## 📑 Table of Contents

- [ILogger Interface](#ilogger-interface)
- [Logger Base Class](#logger-base-class)
- [LogLevel Enumeration](#loglevel-enumeration)
- [Operation Class](#operation-class)
- [OperationStatus Class](#operationstatus-class)
- [LogInfoKey Class](#loginfokey-class)
- [LogInfo Class](#loginfo-class)
- [LogMessageBuilder Static Class](#logmessagebuilder-static-class)
- [NullLogger Class](#nulllogger-class)

---

## ILogger Interface

**File:** `ILogger.cs`
**Namespace:** `NuciLog.Core`
**Purpose:** Defines the consumer contract for logging operations and source context management.

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `SourceContext` | `Type` | `get` | The type associated with this logger instance, typically the declaring type of the consumer class. Null initially. |

### Methods — Source Context

| Method | Signature | Description |
|--------|-----------|-------------|
| `SetSourceContext<T>()` | `void SetSourceContext<T>()` | Sets `SourceContext` to `typeof(T)`. Non-virtual; calls `SetSourceContext(typeof(T))`. |
| `SetSourceContext(Type)` | `void SetSourceContext(Type type)` | Virtual. Sets `SourceContext` to the supplied `Type`. Overridable for custom context logic. |

### Methods — Logging Overload Families

Each of the six levels (`Verbose`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`) exposes an identical overload matrix. The table below documents the pattern; replace `Level` with the desired level name.

| Overload | Parameters | Purpose |
|----------|------------|---------|
| 1 | `Operation operation` | Log operation only |
| 2 | `Operation operation, params LogInfo[] logInfos` | Operation + array of details |
| 3 | `Operation operation, IEnumerable<LogInfo> logInfos` | Operation + enumerable of details |
| 4 | `Operation operation, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + two detail sequences (combined) |
| 5 | `Operation operation, Exception exception` | Operation + exception |
| 6 | `Operation operation, Exception exception, params LogInfo[] logInfos` | Operation + exception + array details |
| 7 | `Operation operation, Exception exception, IEnumerable<LogInfo> logInfos` | Operation + exception + enumerable details |
| 8 | `Operation operation, Exception exception, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + exception + two detail sequences |
| 9 | `Operation operation, OperationStatus operationStatus` | Operation + status |
| 10 | `Operation operation, OperationStatus operationStatus, Exception exception` | Operation + status + exception |
| 11 | `Operation operation, OperationStatus operationStatus, params LogInfo[] logInfos` | Operation + status + array details |
| 12 | `Operation operation, OperationStatus operationStatus, IEnumerable<LogInfo> logInfos` | Operation + status + enumerable details |
| 13 | `Operation operation, OperationStatus operationStatus, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + status + two detail sequences |
| 14 | `Operation operation, OperationStatus operationStatus, Exception exception, params LogInfo[] logInfos` | Operation + status + exception + array details |
| 15 | `Operation operation, OperationStatus operationStatus, Exception exception, IEnumerable<LogInfo> logInfos` | Operation + status + exception + enumerable details |
| 16 | `Operation operation, OperationStatus operationStatus, Exception exception, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + status + exception + two detail sequences |
| 17 | `string message` | Message only |
| 18 | `string message, params LogInfo[] logInfos` | Message + array details |
| 19 | `string message, IEnumerable<LogInfo> logInfos` | Message + enumerable details |
| 20 | `string message, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Message + two detail sequences |
| 21 | `string message, Exception exception` | Message + exception |
| 22 | `string message, Exception exception, params LogInfo[] logInfos` | Message + exception + array details |
| 23 | `string message, Exception exception, IEnumerable<LogInfo> logInfos` | Message + exception + enumerable details |
| 24 | `string message, Exception exception, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Message + exception + two detail sequences |
| 25 | `Operation operation, string message` | Operation + message |
| 26 | `Operation operation, string message, params LogInfo[] logInfos` | Operation + message + array details |
| 27 | `Operation operation, string message, IEnumerable<LogInfo> logInfos` | Operation + message + enumerable details |
| 28 | `Operation operation, string message, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + message + two detail sequences |
| 29 | `Operation operation, string message, Exception exception` | Operation + message + exception |
| 30 | `Operation operation, string message, Exception exception, params LogInfo[] logInfos` | Operation + message + exception + array details |
| 31 | `Operation operation, string message, Exception exception, IEnumerable<LogInfo> logInfos` | Operation + message + exception + enumerable details |
| 32 | `Operation operation, string message, Exception exception, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + message + exception + two detail sequences |
| 33 | `Operation operation, OperationStatus operationStatus, string message` | Operation + status + message |
| 34 | `Operation operation, OperationStatus operationStatus, string message, params LogInfo[] logInfos` | Operation + status + message + array details |
| 35 | `Operation operation, OperationStatus operationStatus, string message, IEnumerable<LogInfo> logInfos` | Operation + status + message + enumerable details |
| 36 | `Operation operation, OperationStatus operationStatus, string message, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + status + message + two detail sequences |
| 37 | `Operation operation, OperationStatus operationStatus, string message, Exception exception` | Operation + status + message + exception |
| 38 | `Operation operation, OperationStatus operationStatus, string message, Exception exception, params LogInfo[] logInfos` | Operation + status + message + exception + array details |
| 39 | `Operation operation, OperationStatus operationStatus, string message, Exception exception, IEnumerable<LogInfo> logInfos` | Operation + status + message + exception + enumerable details |
| 40 | `Operation operation, OperationStatus operationStatus, string message, Exception exception, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos` | Operation + status + message + exception + two detail sequences |

**Total per level:** 40 overloads
**Total across 6 levels:** 240 overloads

### Overload Normalisation Rules (Implemented in `Logger`)

1. **Null detail sequences are bypassed** — `null` `IEnumerable<LogInfo>` or `params LogInfo[]` contributes nothing.
2. **Two populated sequences are combined** — When both `IEnumerable<LogInfo>` and `params LogInfo[]` are non-null and non-empty, they are combined via LINQ `Union` (preserves order of first sequence, then appends unique items from second).
3. **Canonical dispatch** — All overloads ultimately invoke the most parameterised canonical form per level, which calls `WriteLog(LogLevel, Func<string>)`.

---

## Logger Base Class

**File:** `Logger.cs`
**Namespace:** `NuciLog.Core`
**Inheritance:** `ILogger` (abstract)
**Purpose:** Implements overload normalisation, source context retention, and canonical dispatch to `WriteLog`.

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `SourceContext` | `Type` | `get` (private `set`) | Current source context type. Null initially. |

### Methods — Source Context

| Method | Signature | Description |
|--------|-----------|-------------|
| `SetSourceContext<T>()` | `public void SetSourceContext<T>()` | Sets `SourceContext = typeof(T)`. Non-virtual. |
| `SetSourceContext(Type)` | `public virtual void SetSourceContext(Type type)` | Virtual. Sets `SourceContext = type`. Overridable. |

### Methods — Logging (Canonical Forms)

Each level has one canonical overload that all others forward to. Signature pattern:

```csharp
protected void Verbose(
    Operation operation,
    OperationStatus operationStatus,
    string message,
    Exception exception,
    IEnumerable<LogInfo> logInfos,
    LogInfo[] extraLogInfos = null)
```

The canonical method:
1. Combines `logInfos` and `extraLogInfos` (if both non-null) via `Union`
2. Creates a `Func<string>` capturing all formatting inputs
3. Calls `WriteLog(LogLevel.Verbose, formatter)`

**Six canonical methods exist** (one per level), differing only in the `LogLevel` passed to `WriteLog`.

### Abstract Method (Sink Hook)

| Method | Signature | Description |
|--------|-----------|-------------|
| `WriteLog` | `protected abstract void WriteLog(LogLevel level, Func<string> logMessage)` | **Must be implemented by concrete sinks.** Receives the selected severity and a lazy formatter. The sink decides whether, when, and where to invoke `logMessage()`. |

### Behavioural Notes

- **No filtering** — `Logger` does not filter by level; the sink receives every call.
- **No formatting unless invoked** — The `Func<string>` is lazy; `NullLogger` and filtering sinks incur zero formatting cost.
- **Synchronous dispatch** — `WriteLog` is called synchronously on the consumer's thread.
- **Exception propagation** — Exceptions from `WriteLog` or the formatter propagate to the caller untranslated.
- **Thread safety** — `SourceContext` is mutable instance state with no synchronisation. Concurrent `SetSourceContext` and logging calls may race. Sink implementations own their concurrency model.

---

## LogLevel Enumeration

**File:** `LogLevel.cs`
**Namespace:** `NuciLog.Core`
**Purpose:** Identifies log severity independently from the formatted payload.

### Values (Descending Severity)

| Value | Name | Ordinal | Typical Use |
|-------|------|---------|-------------|
| 0 | `Fatal` | Highest | Process-terminating or unrecoverable failure |
| 1 | `Error` | | Recoverable operation failure |
| 2 | `Warn` | | Unexpected but handled condition |
| 3 | `Info` | | Significant operational event |
| 4 | `Debug` | | Diagnostic detail for development |
| 5 | `Verbose` | Lowest | High-volume tracing |

**Note:** `LogLevel` is **not** included in the formatted record string. It is passed separately to `WriteLog` so sinks can filter before formatting.

---

## Operation Class

**File:** `Operation.cs`
**Namespace:** `NuciLog.Core`
**Purpose:** Represents an extensible operation context (e.g., `StartUp`, `ShutDown`, domain-specific operations).

### Constructor

| Constructor | Access | Description |
|-------------|--------|-------------|
| `Operation(string name)` | `protected` | Creates an operation with the given name. Consumers extend this class. |

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Name` | `string` | `get` (protected `set`) | The operation name. |

### Built-in Static Properties

| Property | Returns | Description |
|----------|---------|-------------|
| `Operation.Unknown` | `new Operation(nameof(Unknown))` | Fallback/unknown operation |
| `Operation.StartUp` | `new Operation(nameof(StartUp))` | Application/service startup |
| `Operation.ShutDown` | `new Operation(nameof(ShutDown))` | Application/service shutdown |

### Extension Pattern

```csharp
public sealed class MyOperations : Operation
{
    public static Operation DataImport => new MyOperations(nameof(DataImport));
    public static Operation UserLogin => new MyOperations(nameof(UserLogin));

    private MyOperations(string name) : base(name) { }
}
```

---

## OperationStatus Class

**File:** `OperationStatus.cs`
**Namespace:** `NuciLog.Core`
**Purpose:** Represents an extensible operation status (e.g., `Started`, `Success`, `Failure`).

### Constructor

| Constructor | Access | Description |
|-------------|--------|-------------|
| `OperationStatus(string name)` | `protected` | Creates a status with the given name. Consumers extend this class. |

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Name` | `string` | `get` (protected `set`) | The status name. |

### Built-in Static Properties

| Property | Returns | Description |
|----------|---------|-------------|
| `OperationStatus.Unknown` | `new OperationStatus(nameof(Unknown))` | Fallback/unknown status |
| `OperationStatus.Started` | `new OperationStatus(nameof(Started))` | Operation has begun |
| `OperationStatus.Success` | `new OperationStatus(nameof(Success))` | Operation completed successfully |
| `OperationStatus.Failure` | `new OperationStatus(nameof(Failure))` | Operation failed |
| `OperationStatus.InProgress` | `new OperationStatus(nameof(InProgress))` | Operation is ongoing |

### Formatting Behaviour

In `LogMessageBuilder`, the status name is **upper-cased** (`Name.ToUpper()`) when rendered.

---

## LogInfoKey Class

**File:** `LogInfoKey.cs`
**Namespace:** `NuciLog.Core`
**Implements:** `IEquatable<LogInfoKey>`
**Purpose:** Supplies name-based structured-field identity, reserved internal field names, and key-level sensitivity classification.

### Constructors

| Constructor | Access | Description |
|-------------|--------|-------------|
| `LogInfoKey(string name)` | `protected` | Creates a key with `IsSensitive = false`. |
| `LogInfoKey(string name, bool isSensitive)` | `protected` | Creates a key with the specified sensitivity. |

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Name` | `string` | `get` (protected `set`) | The structured log field name. |
| `IsSensitive` | `bool` | `get` (protected `set`) | When `true`, the associated value is masked in formatted output. |

### Equality

- **Equality:** Two keys are equal iff their `Name` strings are equal (case-sensitive).
- **Hash code:** Based on `Name.GetHashCode()`.

### Built-in Static Properties (Internal Fields)

| Property | Returns | Used For |
|----------|---------|----------|
| `LogInfoKey.SourceContext` | `new LogInfoKey(nameof(SourceContext))` | Internal: logger source context type |
| `LogInfoKey.Operation` | `new LogInfoKey(nameof(Operation))` | Internal: operation name |
| `LogInfoKey.OperationStatus` | `new LogInfoKey(nameof(OperationStatus))` | Internal: operation status (upper-cased) |
| `LogInfoKey.Message` | `new LogInfoKey(nameof(Message))` | Internal: log message text |
| `LogInfoKey.Exception` | `new LogInfoKey(nameof(Exception))` | Internal: exception runtime type |
| `LogInfoKey.ExceptionMessage` | `new LogInfoKey(nameof(ExceptionMessage))` | Internal: exception message (sanitised) |
| `LogInfoKey.StackTrace` | `new LogInfoKey(nameof(StackTrace))` | Internal: exception stack trace (sanitised) |

### Extension Pattern

```csharp
public sealed class AppLogKey : LogInfoKey
{
    public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
    public static LogInfoKey CorrelationId => new AppLogKey(nameof(CorrelationId));
    public static LogInfoKey AccessToken => new AppLogKey(nameof(AccessToken), true); // sensitive

    public AppLogKey(string name) : base(name) { }
    private AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}
```

---

## LogInfo Class

**File:** `LogInfo.cs`
**Namespace:** `NuciLog.Core`
**Purpose:** Captures one named structured value and converts supported object forms into strings at construction time.

### Constructors

| Constructor | Description |
|-------------|-------------|
| `LogInfo(LogInfoKey key, string value)` | Direct string value. |
| `LogInfo(LogInfoKey key, object value)` | Converts `value` to string via conversion rules below. |
| `LogInfo(LogInfoKey key, DateTime value, string format)` | Formats `DateTime` with custom format string. |
| `LogInfo(LogInfoKey key, DateTimeOffset value, string format)` | Formats `DateTimeOffset` with custom format string. |

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Key` | `LogInfoKey` | `get` | The field key (identity + sensitivity). |
| `Value` | `string` | `get` | The string representation (read-only after construction). |

### Object → String Conversion Rules (`GetStringValue`)

| Input Type | Output Format |
|------------|---------------|
| `null` | `string.Empty` |
| `string` | As-is |
| `DateTime` | Round-trip (`"o"`) — e.g., `2026-04-21T10:30:45.0000000Z` |
| `DateTimeOffset` | Round-trip (`"o"`) |
| `TimeSpan` | Constant (`"c"`) — e.g., `1.10:30:45` |
| `Enum` | General (`"G"`) — name or numeric value |
| `string[]` | Elements joined by `;` — e.g., `a;b;c` |
| `IDictionary` (non-generic) | Each entry as `key=value;` concatenated — e.g., `key1=11;key2=;` |
| `IDictionary<TKey, TValue>` (generic) | Each entry as `key=value;` concatenated |
| `IEnumerable` (non-dictionary, non-string-array) | Elements joined by `;` — e.g., `1;2;3` |
| Other | `ToString()` result |

**Note:** Conversion occurs at `LogInfo` construction, not at formatting time. The `Value` property is a plain string thereafter.

---

## LogMessageBuilder Static Class

**File:** `LogMessageBuilder.cs`
**Namespace:** `NuciLog.Core`
**Purpose:** Stateless transformation of canonical inputs into the delimited structured record.

### Public Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `Build` | `static string Build(Operation, OperationStatus, string, Exception, params LogInfo[])` | Convenience overload converting `params` to list. |
| `Build` | `static string Build(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>)` | Canonical builder. Returns formatted record or empty string. |

### Record Grammar

| Element | Specification |
|---------|---------------|
| Field form | `Name＝Value` |
| Assignment | `＝` (U+FF1D, FULLWIDTH EQUALS SIGN) |
| Separator | `͵` (U+0375, GREEK LOWER NUMERAL SIGN) |
| Terminator | None (no trailing separator, no newline) |
| Field order | 1. Operation (if present)<br>2. OperationStatus (if present, upper-cased)<br>3. Message (if present) or default exception message<br>4. Custom `LogInfo` fields (in processed order, de-duplicated)<br>5. Exception fields: `Exception`, `ExceptionMessage`, `StackTrace` (if exception present) |
| Omission | Null operation/status; vacant (null/empty/whitespace) processed values |
| De-duplication | By `LogInfoKey.Name` (case-sensitive); last value wins, first position retained |
| Sensitivity masking | Non-vacant values with `Key.IsSensitive == true` → first 4 chars + `*****` + last 2 chars |

### Sanitisation Rules

| Input | Transformation |
|-------|----------------|
| Embedded newlines (CRLF, CR, LF) | Replaced with literal `\n` (two characters: backslash + n) |
| Stack trace newlines | Removed entirely (after `\n` replacement) |
| Stack trace tabs (`\t`) | Replaced with space |
| Stack trace | Trimmed |
| Vacant final values | Omitted from record |

### Masking Algorithm (`ObfuscateLogInfoValue`)

```csharp
prefix = value[0..min(4, value.Length)]
remaining = value.Length - prefix.Length
suffix = remaining > 0 ? value[^min(2, remaining)..] : ""
mask = "*****" (5 asterisks)
result = prefix + mask + suffix
```

**Examples:**
- `"solar-token-613"` → `"sola*****13"`
- `"613"` → `"613*****"` (no overlap; prefix consumes all, suffix empty)
- `"ab"` → `"ab*****"`
- `""` or whitespace → omitted entirely (not masked)

---

## NullLogger Class

**File:** `NullLogger.cs`
**Namespace:** `NuciLog.Core`
**Inheritance:** `Logger` (sealed)
**Purpose:** No-op implementation for disabled logging scenarios.

### Implementation

```csharp
protected override void WriteLog(LogLevel level, Func<string> logMessage)
{
    // Intentionally empty — does not invoke formatter, emits nothing
}
```

### Behaviour

- Inherits all `Logger` overloads and `SourceContext` management
- `WriteLog` ignores both parameters
- Zero allocation, zero formatting, zero I/O
- Suitable for: testing, disabled logging configurations, performance baselines

---

## Usage Example

```csharp
using NuciLog.Core;

// Consumer defines domain keys
public sealed class AppLogKey : LogInfoKey
{
    public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
    public static LogInfoKey AccessToken => new AppLogKey(nameof(AccessToken), true); // masked

    public AppLogKey(string name) : base(name) { }
    private AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}

// Consumer implements concrete sink
public sealed class FileLogger : Logger
{
    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        if (level <= LogLevel.Info) // Example: filter
        {
            string line = logMessage(); // Formatting occurs HERE
            File.AppendAllText("app.log", line + Environment.NewLine);
        }
    }
}

// Usage
ILogger logger = new FileLogger();
logger.SetSourceContext<Program>();

logger.Info(
    Operation.StartUp,
    OperationStatus.Started,
    "Service initialising",
    new LogInfo(AppLogKey.CorrelationId, "req-42"),
    new LogInfo(AppLogKey.AccessToken, "secret-token-123")); // Will be masked
```

**Output:**
```
Operation＝StartUp͵OperationStatus＝STARTED͵Message＝Service initialising͵CorrelationId＝req-42͵AccessToken＝secr*****23
```