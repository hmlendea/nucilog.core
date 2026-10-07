# Data Model and Transformations

Complete specification of the data structures, value conversions, and transformation pipeline in NuciLog.Core.

## 📑 Table of Contents

- [Core Types](#core-types)
- [LogInfo Value Conversion](#loginfo-value-conversion)
- [LogInfoKey Sensitivity Mechanics](#loginfokey-sensitivity-mechanics)
- [Builder Processing Pipeline](#builder-processing-pipeline)
- [De-duplication Semantics](#de-duplication-semantics)
- [Exception Enrichment](#exception-enrichment)
- [Edge Cases and Verified Behaviours](#edge-cases-and-verified-behaviours)

---

## Core Types

### Type Hierarchy

```
LogInfoKey (extensible)
    ├── Built-in: SourceContext, Operation, OperationStatus, Message, Exception, ExceptionMessage, StackTrace
    └── Consumer: AppLogKey, etc.

Operation (extensible)
    ├── Built-in: Unknown, StartUp, ShutDown
    └── Consumer: BusinessOperation, etc.

OperationStatus (extensible)
    ├── Built-in: Unknown, Started, Success, Failure, InProgress
    └── Consumer: BusinessStatus, etc.

LogLevel (enum)
    Fatal=0, Error=1, Warn=2, Info=3, Debug=4, Verbose=5

LogInfo (sealed)
    Key: LogInfoKey
    Value: string (always string — converted at construction)

Logger (abstract)
    SourceContext: Type
    WriteLog(LogLevel, Func<string>): abstract

LogMessageBuilder (static)
    Build(...): string
    GetProcessedLogInfoList(...): IEnumerable<LogInfo>
```

### Immutability

| Type | Mutable? | Notes |
|------|----------|-------|
| `LogInfoKey` | No | `Name`, `IsSensitive` set at construction |
| `Operation` | No | `Name` set at construction |
| `OperationStatus` | No | `Name` set at construction |
| `LogInfo` | No | `Key`, `Value` set at construction |
| `LogLevel` | N/A | Enum |
| `Logger` | Yes | `SourceContext` can be changed via `SetSourceContext` |

---

## LogInfo Value Conversion

### Construction Overloads

```csharp
// String value — direct assignment
new LogInfo(key, "literal string")

// Object value — converted via GetStringValue
new LogInfo(key, someObject)

// DateTime with format
new LogInfo(key, dateTime, "yyyy-MM-dd")

// DateTimeOffset with format
new LogInfo(key, dateTimeOffset, "yyyy-MM-dd")
```

### GetStringValue Algorithm

```csharp
string GetStringValue(object rawValue)
{
    if (rawValue is null) return string.Empty;

    if (rawValue is string s) return s;

    if (rawValue is DateTime dt) return dt.ToString("o");           // ISO 8601 round-trip
    if (rawValue is DateTimeOffset dto) return dto.ToString("o");   // ISO 8601 round-trip
    if (rawValue is TimeSpan ts) return ts.ToString("c");           // Constant format
    if (rawValue is Enum e) return e.ToString("G");                 // General format (name or value)

    if (rawValue is string[] arr) return string.Join(";", arr);

    if (rawValue is IDictionary dict)
    {
        var sb = new StringBuilder();
        foreach (DictionaryEntry entry in dict)
        {
            sb.Append(Convert.ToString(entry.Key) ?? string.Empty)
              .Append('=')
              .Append(Convert.ToString(entry.Value) ?? string.Empty)
              .Append(';');
        }
        return sb.ToString();
    }

    if (rawValue is IEnumerable enumerable)
    {
        // Generic IDictionary<,> handling
        if (rawValue.GetType().GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)) is not null)
        {
            var sb = new StringBuilder();
            foreach (var item in enumerable)
            {
                if (item is null) continue;
                var keyProp = item.GetType().GetProperty("Key");
                var valProp = item.GetType().GetProperty("Value");
                sb.Append(Convert.ToString(keyProp?.GetValue(item)) ?? string.Empty)
                  .Append('=')
                  .Append(Convert.ToString(valProp?.GetValue(item)) ?? string.Empty)
                  .Append(';');
            }
            return sb.ToString();
        }

        // Generic IEnumerable<T>
        var sb = new StringBuilder();
        foreach (var item in enumerable)
        {
            sb.Append(Convert.ToString(item) ?? string.Empty).Append(';');
        }
        return sb.ToString();
    }

    // Fallback: ToString()
    return rawValue.ToString();
}
```

### Conversion Examples

| Input Type | Input Value | Output String |
|------------|-------------|---------------|
| `string` | `"hello"` | `"hello"` |
| `int` | `42` | `"42"` |
| `DateTime` | `2024-01-15T10:30:00Z` | `"2024-01-15T10:30:00.0000000Z"` |
| `DateTimeOffset` | `2024-01-15T10:30:00+02:00` | `"2024-01-15T10:30:00.0000000+02:00"` |
| `TimeSpan` | `1.02:30:00` | `"1.02:30:00"` |
| `DayOfWeek` | `DayOfWeek.Monday` | `"Monday"` |
| `string[]` | `["a", "b", "c"]` | `"a;b;c"` |
| `Dictionary<string, int>` | `{"x": 1, "y": 2}` | `"x=1;y=2;"` |
| `List<int>` | `[1, 2, 3]` | `"1;2;3;"` |
| `null` | `null` | `""` (empty string) |

### Key Properties

- **Always returns string** — `LogInfo.Value` is `string`, never `object`
- **Null → empty string** — never `null`
- **No culture** — uses invariant/default formatting
- **No escaping** — semicolons in values are not escaped (consumer responsibility)
- **Dictionary order** — iteration order (not guaranteed for `IDictionary`)

---

## LogInfoKey Sensitivity Mechanics

### Definition

```csharp
public class LogInfoKey : IEquatable<LogInfoKey>
{
    public bool IsSensitive { get; protected set; }
    public string Name { get; protected set; }

    protected LogInfoKey(string name) : this(name, false) { }
    protected LogInfoKey(string name, bool isSensitive)
    {
        Name = name;
        IsSensitive = isSensitive;
    }

    public bool Equals(LogInfoKey other) => Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
}
```

### Sensitivity Classification

| Key Type | `IsSensitive` | Masking Applied |
|----------|---------------|-----------------|
| Built-in keys | `false` | Never |
| Consumer non-sensitive | `false` | Never |
| Consumer sensitive | `true` | When value non-vacant |

### Masking Algorithm

```csharp
private static string ObfuscateLogInfoValue(string value)
{
    int prefixLength = Math.Min(value.Length, 4);  // ObfuscatedLogInfoValuePrefixLength
    string prefix = value[..prefixLength];
    int remainingValueLength = value.Length - prefixLength;
    int suffixLength = 0;

    if (remainingValueLength > 0)
    {
        suffixLength = Math.Min(remainingValueLength, 2);  // ObfuscatedLogInfoValueSuffixLength
    }

    string suffix = string.Empty;
    if (suffixLength > 0)
    {
        suffix = value[^suffixLength..];
    }

    string mask = new('*', 5);  // ObfuscatedLogInfoValueMaskLength, ObfuscatedLogInfoValueMaskCharacter

    return $"{prefix}{mask}{suffix}";
}
```

### Masking Examples

| Original Value | Length | Masked Output |
|----------------|--------|---------------|
| `"secret"` | 6 | `"secr*****et"` |
| `"Bearer token123"` | 15 | `"Bear*****23"` |
| `"ab"` | 2 | `"ab*****"` |
| `"a"` | 1 | `"a*****"` |
| `""` | 0 | `""` (not masked — vacant) |
| `" "` | 1 | `" *****"` (whitespace preserved, then masked) |

### Masking Invariants

| Invariant | Description |
|-----------|-------------|
| **Only in formatted output** | `LogInfo.Value` property retains original string |
| **Applied after sanitisation** | Newlines already replaced with `\n` |
| **Vacant values not masked** | `string.IsNullOrWhiteSpace` check before masking |
| **No encryption** | Visual masking only; original value in `LogInfo` object |
| **Delegate capture** | Original value captured in lambda passed to `WriteLog` |

---

## Builder Processing Pipeline

### Build Method Signature

```csharp
public static string Build(
    Operation operation,
    OperationStatus operationStatus,
    string message,
    Exception exception,
    IEnumerable<LogInfo> logInfos)
```

### Pipeline Stages

```mermaid
flowchart TD
    A[Build called] --> B{operation != null?}
    B -->|Yes| C[Append "Operation＝{Name}͵"]
    B -->|No| D{operationStatus != null?}
    C --> D
    D -->|Yes| E[Append "OperationStatus＝{Name.ToUpper()}͵"]
    D -->|No| F[GetProcessedLogInfoList]
    E --> F
    F --> G[For each detail: Append "Key＝Value͵"]
    G --> H{Ends with ͵?}
    H -->|Yes| I[Trim trailing ͵]
    H -->|No| J[Return as-is]
    I --> J
```

### GetProcessedLogInfoList

```csharp
static IEnumerable<LogInfo> GetProcessedLogInfoList(
    string message,
    IEnumerable<LogInfo> logInfos,
    Exception exception)
{
    List<LogInfo> processedLogInfos = [];

    // 1. Message key
    if (!string.IsNullOrWhiteSpace(message))
    {
        processedLogInfos.Add(new(LogInfoKey.Message, SanitiseLogInfoValue(message)));
    }
    else if (exception is not null)
    {
        processedLogInfos.Add(new(LogInfoKey.Message, "An exception has occurred."));
    }

    // 2. Consumer logInfos
    if (logInfos is not null)
    {
        foreach (LogInfo logInfo in logInfos)
        {
            processedLogInfos.Add(new(logInfo.Key, ProcessLogInfoValue(logInfo)));
        }
    }

    // 3. Exception enrichment
    if (exception is not null)
    {
        processedLogInfos.Add(new(LogInfoKey.Exception, exception.GetType()));
        processedLogInfos.Add(new(LogInfoKey.ExceptionMessage, SanitiseLogInfoValue(exception.Message)));
        processedLogInfos.Add(new(LogInfoKey.StackTrace, SanitiseStackTrace(exception.StackTrace)));
    }

    // 4. De-duplication + omit empty
    return processedLogInfos
        .GroupBy(x => x.Key)
        .Select(g => new LogInfo(g.First().Key, g.Last().Value))
        .Where(x => !string.IsNullOrWhiteSpace(x.Value));
}
```

### ProcessLogInfoValue

```csharp
private static string ProcessLogInfoValue(LogInfo logInfo)
{
    string processedValue = SanitiseLogInfoValue(logInfo.Value);

    if (logInfo.Key.IsSensitive && !string.IsNullOrWhiteSpace(processedValue))
    {
        return ObfuscateLogInfoValue(processedValue);
    }

    return processedValue;
}
```

### Sanitisation

```csharp
private static string SanitiseLogInfoValue(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return value;

    // Replace all newline variants with literal \n
    return NewLineMatchingRegex.Replace(value, "\\n");
    // Regex: \r\n|\r|\n
}

private static string SanitiseStackTrace(string stackTrace)
{
    if (string.IsNullOrWhiteSpace(stackTrace)) return stackTrace;

    return SanitiseLogInfoValue(stackTrace)
        .Replace("\\n", string.Empty)  // Remove literal \n
        .Replace("\\t", " ")           // Replace literal \t with space
        .Trim();
}
```

### Sanitisation Examples

| Input | Sanitised |
|-------|-----------|
| `"line1\nline2"` | `"line1\\nline2"` |
| `"line1\r\nline2"` | `"line1\\nline2"` |
| `"line1\rline2"` | `"line1\\nline2"` |
| `"tab\there"` | `"tab\\there"` |
| `"  spaced  "` | `"  spaced  "` (whitespace preserved) |

---

## De-duplication Semantics

### Algorithm

```csharp
processedLogInfos
    .GroupBy(x => x.Key)           // Group by LogInfoKey (equality = Name)
    .Select(g => new LogInfo(
        g.First().Key,             // First key's identity (including IsSensitive)
        g.Last().Value))           // Last value in input order
    .Where(x => !string.IsNullOrWhiteSpace(x.Value));  // Omit vacant
```

### Key Behaviours

| Behaviour | Description |
|-----------|-------------|
| **Equality by Name** | `LogInfoKey.Equals` compares `Name` only — different types with same name are equal |
| **First key wins** | The `Key` object from the first occurrence is retained (including its `IsSensitive`) |
| **Last value wins** | The `Value` from the last occurrence is retained |
| **Order preserved** | Input order determines "first" and "last" |
| **Vacant omitted** | After de-dup, entries with whitespace-only values are removed |

### De-duplication Examples

#### Example 1: Same key, different values

```csharp
new LogInfo(key, "first"),
new LogInfo(key, "second")
```

**Result:** One entry with `key` (first key's `IsSensitive`) and value `"second"`.

#### Example 2: Same name, different sensitivity

```csharp
new LogInfo(new AppLogKey("Token", false), "visible"),
new LogInfo(new AppLogKey("Token", true), "secret")
```

**Result:** One entry with first key (`IsSensitive=false`), value `"secret"` — **not masked** because first key was non-sensitive.

**Note:** This is a known edge case — see [Edge Cases](#edge-cases-and-verified-behaviours).

#### Example 3: Built-in + consumer collision

```csharp
new LogInfo(LogInfoKey.Message, "consumer message"),
// ... later, exception adds LogInfoKey.ExceptionMessage
```

**Result:** Consumer's `Message` value retained if it appears last; exception's `ExceptionMessage` is separate key.

---

## Exception Enrichment

### Added Fields

When `exception != null`, three fields are appended **after** consumer `logInfos`:

| Key | Value Source | Sanitisation |
|-----|--------------|--------------|
| `Exception` | `exception.GetType()` | `ToString()` → sanitised |
| `ExceptionMessage` | `exception.Message` | `SanitiseLogInfoValue` |
| `StackTrace` | `exception.StackTrace` | `SanitiseStackTrace` |

### Ordering

```
[Operation] [OperationStatus] [Message] [Consumer LogInfos...] [Exception] [ExceptionMessage] [StackTrace]
```

### Default Message

If `message` is null/whitespace **and** exception present:
- `Message` key gets value `"An exception has occurred."` (from `TestLogValues.DefaultExceptionLogMessage`)

### Stack Trace Sanitisation

```csharp
SanitiseStackTrace(stackTrace)
    = SanitiseLogInfoValue(stackTrace)
        .Replace("\\n", "")      // Remove literal \n
        .Replace("\\t", " ")     // Replace literal \t with space
        .Trim();
```

**Effect:** Stack trace becomes single-line, tab-normalised, trimmed.

---

## Edge Cases and Verified Behaviours

### 1. Vacant Value Omission

```csharp
logger.Info("Message", new LogInfo(key, ""), new LogInfo(key2, "   "));
```

**Result:** Both omitted — `Where(x => !string.IsNullOrWhiteSpace(x.Value))` removes them.

### 2. Null Message with Exception

```csharp
logger.Info(exception: new InvalidOperationException("Failed"));
```

**Result:** `Message＝An exception has occurred.` + exception fields.

### 3. Null Message without Exception

```csharp
logger.Info(message: null);
```

**Result:** No `Message` key at all (omitted by `Where`).

### 4. Sensitivity Determined by First Key

```csharp
// Test: GivenDuplicatedLogInfosWithASensitiveFinalValue
var logInfos = new[]
{
    new LogInfo(TestLogInfoKey.TestKey, "first-value"),      // Non-sensitive
    new LogInfo(TestLogInfoKey.SensitiveTestKey, "secret")   // Sensitive, same name
};
```

**Result:** Value `"secret"` retained, **not masked** — first key (`TestKey`) was non-sensitive.

**Reason:** De-duplication groups by `Name`; `g.First().Key` is the non-sensitive key.

### 5. Sensitivity Determined by Last Key (if first is sensitive)

```csharp
var logInfos = new[]
{
    new LogInfo(TestLogInfoKey.SensitiveTestKey, "secret"),  // Sensitive
    new LogInfo(TestLogInfoKey.TestKey, "visible")           // Non-sensitive, same name
};
```

**Result:** Value `"visible"` retained, **masked** — first key was sensitive.

### 6. Built-in Key Collision

```csharp
logger.Info("Message", new LogInfo(LogInfoKey.Message, "custom"));
```

**Result:** Consumer's `Message` value used (appears last in processing order).

### 7. Empty LogInfos

```csharp
logger.Info(Array.Empty<LogInfo>());
logger.Info((IEnumerable<LogInfo>)null);
```

**Result:** No consumer fields added; only `Message` (and exception if present).

### 8. SourceContext Not Formatted

```csharp
logger.SetSourceContext<Program>();
logger.Info("Test");
```

**Result:** `SourceContext` key **never appears** in formatted output — it's internal metadata only.

### 9. Delimiter Characters

| Character | Unicode | Name | Purpose |
|-----------|---------|------|---------|
| `＝` | U+FF1D | Fullwidth Equals Sign | Key-value separator |
| `͵` | U+0375 | Greek Lower Numeral Sign | Field delimiter |

**Rationale:** Unambiguous parsing — these characters are extremely unlikely to appear in log values.

### 10. Trailing Delimiter Trim

```csharp
if (logMessage.EndsWith('͵'))
{
    return logMessage[..^1];
}
```

**Result:** No trailing `͵` in final output.

---

## Summary: Transformation Checklist

| Stage | Input | Output | Key Operation |
|-------|-------|--------|---------------|
| Construction | `object` | `string` | `GetStringValue` — type-specific formatting |
| Sanitisation | `string` | `string` | Newlines → `\n`; stack trace → single-line |
| Masking | `string` | `string` | First 4 + `*****` + last 2 (if sensitive & non-vacant) |
| De-duplication | `List<LogInfo>` | `IEnumerable<LogInfo>` | Group by Key.Name; first key, last value |
| Omission | `IEnumerable<LogInfo>` | `IEnumerable<LogInfo>` | Remove whitespace-only values |
| Formatting | `IEnumerable<LogInfo>` | `string` | `Key＝Value͵` with fullwidth/Greek delimiters |

**Invariant:** `LogInfo.Value` is always a string; original object never escapes construction.