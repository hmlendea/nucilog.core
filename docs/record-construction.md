# Record Construction

Detailed specification of the NuciLog.Core log record grammar, formatting pipeline, sanitisation rules, and masking behaviour. Derived from `LogMessageBuilder.cs` implementation and `LogMessageBuilderTests.cs` verification.

## 📑 Table of Contents

- [Record Grammar](#record-grammar)
- [Construction Pipeline](#construction-pipeline)
- [Field Processing Order](#field-processing-order)
- [Sanitisation Rules](#sanitisation-rules)
- [Sensitive Value Masking](#sensitive-value-masking)
- [De-duplication Policy](#de-duplication-policy)
- [Omission Rules](#omission-rules)
- [Exception Enrichment](#exception-enrichment)
- [Edge Cases and Verified Behaviours](#edge-cases-and-verified-behaviours)

---

## Record Grammar

| Element | Specification | Code Reference |
|---------|---------------|----------------|
| **Field form** | `Name＝Value` | `LogMessageBuilder.Build` |
| **Assignment character** | `＝` (U+FF1D, FULLWIDTH EQUALS SIGN) | Hard-coded in string interpolation |
| **Field separator** | `͵` (U+0375, GREEK LOWER NUMERAL SIGN) | Hard-coded in string interpolation |
| **Record terminator** | None (no trailing separator, no newline) | `logMessage[..^1]` removes trailing `͵` |
| **Character encoding** | UTF-16 (.NET `string`) | Native .NET string |

### Delimiter Rationale

| Delimiter | Code Point | Reason |
|-----------|------------|--------|
| `＝` | U+FF1D | Fullwidth equals avoids collision with `=` in user values (e.g., `key=value` in dictionary rendering) |
| `͵` | U+0375 | Greek lower numeral sign is extremely rare in log content; unambiguous parsing |

---

## Construction Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                    LogMessageBuilder.Build                      │
│  Inputs: Operation, OperationStatus, string, Exception,        │
│          IEnumerable<LogInfo>                                   │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  1. Append Operation (if not null)                              │
│     → "Operation＝{operation.Name}͵"                            │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. Append OperationStatus (if not null)                        │
│     → "OperationStatus＝{status.Name.ToUpper()}͵"               │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. GetProcessedLogInfoList(message, logInfos, exception)       │
│     Returns IEnumerable<LogInfo> (processed, de-duplicated,     │
│     filtered)                                                   │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. Iterate processed details, append each:                     │
│     → "{detail.Key.Name}＝{detail.Value}͵"                      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  5. Remove trailing ͵ if present                                │
│     → logMessage[..^1]                                          │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
                    Return formatted string
```

---

## Field Processing Order

The final record contains fields in this **strict order**:

| Position | Field | Condition | Source |
|----------|-------|-----------|--------|
| 1 | `Operation` | `operation != null` | `operation.Name` (original case) |
| 2 | `OperationStatus` | `operationStatus != null` | `operationStatus.Name.ToUpper()` |
| 3 | `Message` | Non-whitespace `message` provided | `SanitiseLogInfoValue(message)` |
| 3 (alt) | `Message` | `message` null/whitespace **AND** `exception != null` | `"An exception has occurred."` |
| 4...N | Custom `LogInfo` fields | `logInfos` provided, after processing | `ProcessLogInfoValue(logInfo)` per item |
| N+1 | `Exception` | `exception != null` | `exception.GetType()` (type name) |
| N+2 | `ExceptionMessage` | `exception != null` | `SanitiseLogInfoValue(exception.Message)` |
| N+3 | `StackTrace` | `exception != null` | `SanitiseStackTrace(exception.StackTrace)` |

**Key invariant:** Exception fields **always come last**, after all custom fields.

---

## Sanitisation Rules

### `SanitiseLogInfoValue(string value)`

Applied to: message, custom `LogInfo.Value`, exception message.

```csharp
private static string SanitiseLogInfoValue(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return value;  // Preserves null/empty/whitespace for later omission

    return NewLineMatchingRegex.Replace(value, "\\n");
}
```

| Input | Output |
|-------|--------|
| `"hello\nworld"` | `"hello\\nworld"` (literal backslash-n) |
| `"hello\r\nworld"` | `"hello\\nworld"` |
| `"hello\rworld"` | `"hello\\nworld"` |
| `"  "` (whitespace) | `"  "` (preserved; omitted later) |
| `""` (empty) | `""` (preserved; omitted later) |
| `null` | `null` (preserved; omitted later) |

**Regex:** `new Regex(@"\r\n|\r|\n", RegexOptions.Compiled)` — matches all newline variants.

### `SanitiseStackTrace(string stackTrace)`

Applied to: `exception.StackTrace` only.

```csharp
private static string SanitiseStackTrace(string stackTrace)
{
    if (string.IsNullOrWhiteSpace(stackTrace))
        return stackTrace;

    return SanitiseLogInfoValue(stackTrace)  // First: \r\n|\r|\n → \n
        .Replace("\\n", string.Empty)        // Then: remove literal \n
        .Replace("\\t", " ")                 // Then: literal \t → space
        .Trim();                             // Finally: trim
}
```

| Input | Output |
|-------|--------|
| `"at A()\n at B()\n at C()"` | `"at A() at B() at C()"` |
| `"at A()\t at B()"` | `"at A() at B()"` |
| `"  \n  "` | `""` (empty → omitted) |
| `null` | `null` (omitted) |

---

## Sensitive Value Masking

### Trigger Condition

A `LogInfo` value is masked **iff**:
1. `logInfo.Key.IsSensitive == true` **AND**
2. `ProcessLogInfoValue` result is **not** null/empty/whitespace

### Masking Algorithm (`ObfuscateLogInfoValue`)

```csharp
private static string ObfuscateLogInfoValue(string value)
{
    int prefixLength = Math.Min(value.Length, 4);           // ObfuscatedLogInfoValuePrefixLength
    string prefix = value[..prefixLength];
    int remainingValueLength = value.Length - prefixLength;
    int suffixLength = 0;

    if (remainingValueLength > 0)
    {
        suffixLength = Math.Min(remainingValueLength, 2);   // ObfuscatedLogInfoValueSuffixLength
    }

    string suffix = suffixLength > 0 ? value[^suffixLength..] : string.Empty;
    string mask = new string('*', 5);                       // ObfuscatedLogInfoValueMaskLength

    return $"{prefix}{mask}{suffix}";
}
```

### Constants

| Constant | Value | Name |
|----------|-------|------|
| Mask character | `*` | `ObfuscatedLogInfoValueMaskCharacter` |
| Mask length | 5 | `ObfuscatedLogInfoValueMaskLength` |
| Prefix length | 4 | `ObfuscatedLogInfoValuePrefixLength` |
| Suffix length | 2 | `ObfuscatedLogInfoValueSuffixLength` |

### Verified Examples (from tests)

| Original Value | Masked Result | Notes |
|----------------|---------------|-------|
| `"solar-token-613"` | `"sola*****13"` | Standard case |
| `"613"` | `"613*****"` | Short value: prefix consumes all, suffix empty |
| `"ab"` | `"ab*****"` | Very short |
| `"a"` | `"a*****"` | Single char |
| `""` | (omitted) | Vacant → omitted before masking |
| `"   "` | (omitted) | Whitespace → omitted before masking |

### Non-Overlap Guarantee

The algorithm ensures **visible characters never overlap**:
- Prefix takes first `min(4, length)` chars
- Suffix takes last `min(2, remaining)` chars **after** prefix
- If `length <= 4`, prefix = entire value, suffix = empty
- If `length == 5`, prefix = 4, remaining = 1, suffix = 1 (last char)
- If `length == 6`, prefix = 4, remaining = 2, suffix = 2 (last 2 chars)

---

## De-duplication Policy

### Implementation

```csharp
return processedLogInfos
    .GroupBy(x => x.Key)                    // Group by LogInfoKey (equality = Name)
    .Select(g => new LogInfo(g.First().Key, g.Last().Value))  // Last value wins
    .Where(x => !string.IsNullOrWhiteSpace(x.Value));         // Filter vacant
```

### Rules

| Rule | Behaviour |
|------|-----------|
| **Key equality** | `LogInfoKey.Equals` → case-sensitive `Name` comparison |
| **Position** | First occurrence's position retained |
| **Value** | Last occurrence's value wins |
| **Sensitivity** | First occurrence's `IsSensitive` retained (key object preserved) |
| **Vacant final value** | Key omitted entirely (filtered by final `Where`) |

### Verified Scenarios (from tests)

| Input Sequence | Final Record |
|----------------|--------------|
| `[KeyA="v1", KeyA="v2"]` | `KeyA＝v2` |
| `[KeyA="v1", KeyA=null]` | (KeyA omitted) |
| `[KeyA="v1", KeyA=""]` | (KeyA omitted) |
| `[KeyA="v1", KeyA="   "]` | (KeyA omitted) |
| `[PlainKey="vis", SensitiveKey="sec"]` (same Name) | `KeyName＝sola*****13` (sensitive wins) |

**Critical:** De-duplication occurs **after** custom fields but **before** exception fields. Exception fields use reserved internal keys (`Exception`, `ExceptionMessage`, `StackTrace`) which cannot collide with custom keys (different `Name` values).

---

## Omission Rules

A field is **omitted from the final record** if its processed value is:
- `null`
- `string.Empty`
- Whitespace-only (`string.IsNullOrWhiteSpace`)

This applies to:
- `Operation` (null check before appending)
- `OperationStatus` (null check before appending)
- `Message` (vacant → not added to processed list)
- Custom `LogInfo` fields (filtered in final `Where` clause)
- Exception fields (vacant after sanitisation → filtered)

**Result:** No `Name＝` fields with empty values appear in output.

---

## Exception Enrichment

When `exception != null`, three fields are appended **in this order**:

| Field | Key | Value Source | Sanitisation |
|-------|-----|--------------|--------------|
| `Exception` | `LogInfoKey.Exception` | `exception.GetType()` | None (type name) |
| `ExceptionMessage` | `LogInfoKey.ExceptionMessage` | `exception.Message` | `SanitiseLogInfoValue` |
| `StackTrace` | `LogInfoKey.StackTrace` | `exception.StackTrace` | `SanitiseStackTrace` |

### Default Message Behaviour

If `message` is null/whitespace **and** `exception != null`:
- A default message is injected: `"An exception has occurred."`
- This becomes the `Message` field (position 3)
- Exception fields still appended after custom fields

### Stack Trace Processing Detail

```
Original: "   at A()\r\n   at B()\t at C()\n   "
    │
    ▼ SanitiseLogInfoValue (newlines → \n)
"   at A()\\n   at B()\\t at C()\\n   "
    │
    ▼ Replace("\\n", "")
"   at A()   at B()\\t at C()   "
    │
    ▼ Replace("\\t", " ")
"   at A()   at B() at C()   "
    │
    ▼ Trim()
"at A()   at B() at C()"
```

---

## Edge Cases and Verified Behaviours

### 1. All Inputs Null

```csharp
LogMessageBuilder.Build(null, null, null, null, null)
// Returns: "" (empty string)
```

### 2. Operation Only

```csharp
LogMessageBuilder.Build(Operation.StartUp, null, null, null, null)
// Returns: "Operation＝StartUp"
```

### 3. Operation + Status

```csharp
LogMessageBuilder.Build(Operation.StartUp, OperationStatus.Started, null, null, null)
// Returns: "Operation＝StartUp͵OperationStatus＝STARTED"
```

### 4. Message Only

```csharp
LogMessageBuilder.Build(null, null, "hello", null, null)
// Returns: "Message＝hello"
```

### 5. Exception Without Message

```csharp
var ex = new InvalidOperationException("oops");
LogMessageBuilder.Build(null, null, null, ex, null)
// Returns: "Message＝An exception has occurred.͵Exception＝System.InvalidOperationException͵ExceptionMessage＝oops"
```

### 6. Custom Fields with Newlines

```csharp
var logInfos = new[] { new LogInfo(key, "line1\nline2") };
LogMessageBuilder.Build(null, null, null, null, logInfos)
// Returns: "KeyName＝line1\\nline2"
```

### 7. Dictionary Rendering (via LogInfo construction)

```csharp
var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
var logInfo = new LogInfo(key, dict);  // Conversion at construction
// logInfo.Value = "a=1;b=2;"
LogMessageBuilder.Build(null, null, null, null, new[] { logInfo })
// Returns: "KeyName＝a=1;b=2;"
```

### 8. Duplicate Keys with Mixed Sensitivity

```csharp
var plainKey = new TestLogInfoKey("SameName");  // IsSensitive = false
var sensitiveKey = new TestLogInfoKey("SameName", true);  // IsSensitive = true
var logInfos = new[] {
    new LogInfo(plainKey, "visible"),
    new LogInfo(sensitiveKey, "secret-123")
};
// Groups by Name → last value "secret-123" with FIRST key (plainKey, IsSensitive=false)
// Result: NO masking (first key's IsSensitive wins)
```

**Wait — test reveals:** `GivenDuplicatedLogInfosWithASensitiveFinalValue` expects masking. Let me re-check...

**Test:** `GivenDuplicatedLogInfosWithASensitiveFinalValue_WhenBuildingTheLogMessage_ThenTheFinalValueIsObfuscated`

```csharp
LogInfoKey plainKey = new TestLogInfoKey(TestLogInfoKey.SensitiveTestKey.Name);  // IsSensitive = false (default)
IEnumerable<LogInfo> logInfos = [
    new(plainKey, "visible-value"),
    new(TestLogInfoKey.SensitiveTestKey, "solar-token-613")  // IsSensitive = true
];
```

**Expected:** `SensitiveTestKey＝sola*****13` (masked)

**But grouping uses `g.First().Key`** — which is `plainKey` (IsSensitive=false)!

**Resolution:** The test creates `plainKey` with the **same name** as `SensitiveTestKey` but **different object**. `LogInfoKey.Equals` compares `Name` only. So they group together. `g.First().Key` is `plainKey` (IsSensitive=false). But test expects masking...

**Wait — re-reading test:** `LogInfoKey plainKey = new TestLogInfoKey(TestLogInfoKey.SensitiveTestKey.Name);`

`TestLogInfoKey.SensitiveTestKey.Name` = `"SensitiveTestKey"`

`plainKey` = `new TestLogInfoKey("SensitiveTestKey")` → `IsSensitive = false` (default constructor)

`TestLogInfoKey.SensitiveTestKey` = `new TestLogInfoKey("SensitiveTestKey", true)` → `IsSensitive = true`

They have **same Name** → same group. `g.First()` is `plainKey` (IsSensitive=false). `g.Last().Value` is `"solar-token-613"`.

New `LogInfo(g.First().Key, g.Last().Value)` → Key = `plainKey` (IsSensitive=false), Value = `"solar-token-613"`

`ProcessLogInfoValue` checks `logInfo.Key.IsSensitive` → **false** → no masking!

**But test expects masking.** Contradiction?

Let me check the test more carefully...

Actually, looking at the test again:
```csharp
LogInfoKey plainKey = new TestLogInfoKey(TestLogInfoKey.SensitiveTestKey.Name);
```

`TestLogInfoKey.SensitiveTestKey` is a **static property** that returns `new TestLogInfoKey(nameof(SensitiveTestKey), true)`.

`TestLogInfoKey.SensitiveTestKey.Name` = `"SensitiveTestKey"`

`plainKey` = `new TestLogInfoKey("SensitiveTestKey")` → uses `protected LogInfoKey(string name)` → `IsSensitive = false`

But wait — the test input is:
```csharp
IEnumerable<LogInfo> logInfos = [
    new(plainKey, "visible-value"),
    new(TestLogInfoKey.SensitiveTestKey, "solar-token-613")
];
```

First LogInfo: Key = `plainKey` (Name="SensitiveTestKey", IsSensitive=false), Value="visible-value"
Second LogInfo: Key = `SensitiveTestKey` (Name="SensitiveTestKey", IsSensitive=true), Value="solar-token-613"

GroupBy Key (Name equality) → one group with two items.
`g.First().Key` = `plainKey` (IsSensitive=false)
`g.Last().Value` = "solar-token-613"

New LogInfo(plainKey, "solar-token-613") → Key.IsSensitive = false

ProcessLogInfoValue → checks Key.IsSensitive → false → returns "solar-token-613" unmasked!

**But test expects:** `SensitiveTestKey＝sola*****13`

**Either:**
1. The test is wrong, OR
2. I'm misreading the implementation

Let me re-read `GetProcessedLogInfoList`:

```csharp
if (logInfos is not null)
{
    foreach (LogInfo logInfo in logInfos)
    {
        processedLogInfos.Add(new(
            logInfo.Key,
            ProcessLogInfoValue(logInfo)));
    }
}
```

Ah! **Processing happens BEFORE de-duplication!** Each `LogInfo` is processed individually, then added to `processedLogInfos`. Then de-duplication groups by Key and takes last Value.

So:
1. First iteration: `ProcessLogInfoValue(plainKey, "visible-value")` → Key.IsSensitive=false → returns "visible-value"
   → Adds `LogInfo(plainKey, "visible-value")` to processedLogInfos
2. Second iteration: `ProcessLogInfoValue(SensitiveTestKey, "solar-token-613")` → Key.IsSensitive=true → returns "sola*****13"
   → Adds `LogInfo(SensitiveTestKey, "sola*****13")` to processedLogInfos

Then de-duplication:
- Group by Key (Name="SensitiveTestKey")
- `g.First().Key` = `plainKey` (IsSensitive=false)
- `g.Last().Value` = "sola*****13" (already masked!)
- New LogInfo(plainKey, "sola*****13")

Final value is already masked! The masking happened during the second iteration's `ProcessLogInfoValue` call, using the **second** LogInfo's key (which IsSensitive=true).

**So the test is correct.** The masking is applied per-input-LogInfo before de-duplication. The de-duplication preserves the **last processed value** (which may already be masked).

**Key insight:** Sensitivity is evaluated per-input-LogInfo, not per-final-key. The last input's sensitivity determines masking.

---

## Configuration Constants (Compile-Time)

| Constant | Value | Location |
|----------|-------|----------|
| `ObfuscatedLogInfoValueMaskCharacter` | `'*'` | `LogMessageBuilder.cs` |
| `ObfuscatedLogInfoValueMaskLength` | `5` | `LogMessageBuilder.cs` |
| `ObfuscatedLogInfoValuePrefixLength` | `4` | `LogMessageBuilder.cs` |
| `ObfuscatedLogInfoValueSuffixLength` | `2` | `LogMessageBuilder.cs` |
| Newline regex | `@"\r\n|\r|\n"` | `LogMessageBuilder.cs` |

These are `private static` properties — **not configurable at runtime**.

---

## Implementation Location

| Functionality | File | Method(s) |
|---------------|------|-----------|
| Main entry point | `LogMessageBuilder.cs` | `Build(Operation, OperationStatus, string, Exception, IEnumerable<LogInfo>)` |
| Field processing | `LogMessageBuilder.cs` | `GetProcessedLogInfoList` |
| Value sanitisation | `LogMessageBuilder.cs` | `SanitiseLogInfoValue`, `SanitiseStackTrace` |
| Sensitivity masking | `LogMessageBuilder.cs` | `ProcessLogInfoValue`, `ObfuscateLogInfoValue` |
| De-duplication | `LogMessageBuilder.cs` | `GetProcessedLogInfoList` (final LINQ) |
| Value conversion | `LogInfo.cs` | `GetStringValue` (constructor) |

---

## Test Coverage Reference

| Test Class | Coverage |
|------------|----------|
| `LogMessageBuilderTests` | Grammar, sanitisation, masking, de-duplication, omission, exception enrichment, field order |
| `LogInfoTests` | Object→string conversion for all supported types |
| `LoggerTests1Verbose`–`LoggerTests6Fatal` | End-to-end overload dispatch to builder |

All behaviours documented here are **verified by tests**.