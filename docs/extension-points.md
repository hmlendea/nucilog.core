# Extension Points

Contracts and patterns for extending NuciLog.Core. Each extension point is a deliberate architectural boundary where consumer code integrates with the library.

## 📑 Table of Contents

- [Concrete Log Sink](#concrete-log-sink)
- [Structured Field Keys](#structured-field-keys)
- [Domain Operations](#domain-operations)
- [Domain Operation Statuses](#domain-operation-statuses)
- [Extension Composition](#extension-composition)

---

## Concrete Log Sink

### Contract

**File:** `Logger.cs`
**Method:** `protected abstract void WriteLog(LogLevel level, Func<string> logMessage)`

### Signature

```csharp
protected abstract void WriteLog(
    LogLevel level,           // Selected severity (Fatal=0 ... Verbose=5)
    Func<string> logMessage   // Lazy formatter — invokes LogMessageBuilder.Build
);
```

### Responsibilities

The sink implementation **owns entirely**:

| Responsibility | Description |
|----------------|-------------|
| **Level filtering** | Decide whether to process based on `level` (e.g., `if (level <= LogLevel.Info)`) |
| **Formatter invocation** | Call `logMessage()` **iff** emitting; `NullLogger` never calls it |
| **Destination policy** | Console, file, network, database, external service, etc. |
| **Formatting decoration** | Prefix timestamp, add correlation IDs, transform record, etc. |
| **Buffering/batching** | Accumulate records, flush on interval or count |
| **Error handling** | Catch/translate exceptions from formatter or destination |
| **Concurrency** | Synchronise access to destination; `Logger` provides no locking |
| **Resource lifecycle** | Open/close connections, dispose resources, handle app shutdown |

### Minimal Implementation

```csharp
public sealed class ConsoleLogger : Logger
{
    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        // Optional: filter by level
        if (level > LogLevel.Info) return;

        // Formatting occurs HERE (lazy)
        string line = logMessage();

        // Destination policy
        Console.WriteLine(line);
    }
}
```

### Advanced Implementation Patterns

#### Level-Filtered with Configuration

```csharp
public sealed class ConfigurableLogger : Logger
{
    private readonly LogLevel _minimumLevel;

    public ConfigurableLogger(LogLevel minimumLevel = LogLevel.Info)
    {
        _minimumLevel = minimumLevel;
    }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        if (level > _minimumLevel) return;  // Higher ordinal = lower severity
        string line = logMessage();
        // ... emit
    }
}
```

#### Async Sink with Buffering

```csharp
public sealed class BufferedLogger : Logger, IDisposable
{
    private readonly BlockingCollection<string> _queue = new(1000);
    private readonly Task _worker;
    private readonly CancellationTokenSource _cts = new();

    public BufferedLogger()
    {
        _worker = Task.Run(ProcessQueue, _cts.Token);
    }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        if (!_queue.TryAdd(logMessage()))
        {
            // Backpressure policy: drop, block, or fallback
        }
    }

    private async Task ProcessQueue()
    {
        foreach (var line in _queue.GetConsumingEnumerable(_cts.Token))
        {
            await EmitAsync(line);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _queue.CompleteAdding();
        _worker.Wait();
    }
}
```

#### Multi-Destination Sink

```csharp
public sealed class MultiLogger : Logger
{
    private readonly IReadOnlyList<Logger> _sinks;

    public MultiLogger(params Logger[] sinks)
    {
        _sinks = sinks;
    }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        string line = logMessage();  // Format once
        foreach (var sink in _sinks)
        {
            // Each sink re-uses formatted string
            sink.GetType()
                .GetMethod("WriteLog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(sink, new object[] { level, new Func<string>(() => line) });
        }
    }
}
```

### Critical Invariants

| Invariant | Enforcement |
|-----------|-------------|
| **Formatter called at most once per log call** | Sink controls invocation; library calls `WriteLog` once |
| **Formatter called on consumer's thread (unless sink defers)** | `WriteLog` is synchronous; sink may capture delegate |
| **No library dependency on sink** | `Logger` knows nothing of concrete sinks |
| **`NullLogger` is valid sink** | `WriteLog` empty body; zero cost when logging disabled |

### Thread Safety

- `Logger` instance state (`SourceContext`) is **not thread-safe**
- Sink **must** implement its own synchronisation if shared across threads
- `WriteLog` may be called concurrently; sink must handle reentrancy

---

## Structured Field Keys

### Contract

**File:** `LogInfoKey.cs`
**Class:** `LogInfoKey` (extensible via protected constructors)

### Extension Pattern

```csharp
public sealed class AppLogKey : LogInfoKey
{
    // Non-sensitive keys
    public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
    public static LogInfoKey CorrelationId => new AppLogKey(nameof(CorrelationId));
    public static LogInfoKey RequestPath => new AppLogKey(nameof(RequestPath));

    // Sensitive keys — values will be masked
    public static LogInfoKey AccessToken => new AppLogKey(nameof(AccessToken), true);
    public static LogInfoKey ApiKey => new AppLogKey(nameof(ApiKey), true);
    public static LogInfoKey Password => new AppLogKey(nameof(Password), true);
    public static LogInfoKey ConnectionString => new AppLogKey(nameof(ConnectionString), true);

    // Required: public constructor for non-sensitive
    public AppLogKey(string name) : base(name) { }

    // Required: private constructor for sensitive
    private AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}
```

### Constructor Accessibility

| Constructor | Access | Use For |
|-------------|--------|---------|
| `LogInfoKey(string name)` | `protected` | Non-sensitive keys (`IsSensitive = false`) |
| `LogInfoKey(string name, bool isSensitive)` | `protected` | Sensitive keys (`IsSensitive = true`) |

**Consumers must subclass** — cannot instantiate `LogInfoKey` directly.

### Key Identity and Equality

- **Equality:** `LogInfoKey.Equals` → case-sensitive `Name` comparison
- **Hash code:** `Name.GetHashCode()`
- **Implication:** Two keys with same `Name` are **equal**, even if different types or `IsSensitive` values
- **De-duplication:** In `LogMessageBuilder`, last value wins for duplicate names; first key's `IsSensitive` determines masking (but see [Record Construction](record-construction.md#edge-cases-and-verified-behaviours) for per-input processing nuance)

### Reserved Internal Names

**Do not use these names** for custom keys — they are used by `LogMessageBuilder` for internal fields:

| Reserved Name | Internal Key | Purpose |
|---------------|--------------|---------|
| `SourceContext` | `LogInfoKey.SourceContext` | Logger source type (not formatted) |
| `Operation` | `LogInfoKey.Operation` | Operation name |
| `OperationStatus` | `LogInfoKey.OperationStatus` | Operation status (upper-cased) |
| `Message` | `LogInfoKey.Message` | Log message text |
| `Exception` | `LogInfoKey.Exception` | Exception runtime type |
| `ExceptionMessage` | `LogInfoKey.ExceptionMessage` | Exception message |
| `StackTrace` | `LogInfoKey.StackTrace` | Exception stack trace |

**Collision behaviour:** If a custom key uses a reserved name, it will be grouped with the internal field during de-duplication, with unpredictable results.

### Sensitivity Masking

When `IsSensitive == true` and value is non-vacant:
- **Masked in formatted output only** — `LogInfo.Value` property retains original string
- **Algorithm:** First 4 chars + `*****` + last 2 chars (see [Record Construction](record-construction.md#sensitive-value-masking))
- **No encryption** — masking is visual only; original value exists in `LogInfo` object and delegate capture

### Usage

```csharp
logger.Info(
    Operation.UserLogin,
    OperationStatus.Success,
    "User authenticated",
    new LogInfo(AppLogKey.UserId, 12345),
    new LogInfo(AppLogKey.CorrelationId, "req-abc"),
    new LogInfo(AppLogKey.AccessToken, "Bearer secret-token-xyz"));  // Masked in output
```

---

## Domain Operations

### Contract

**File:** `Operation.cs`
**Class:** `Operation` (extensible via protected constructor)

### Extension Pattern

```csharp
public sealed class BusinessOperation : Operation
{
    public static Operation DataImport => new BusinessOperation(nameof(DataImport));
    public static Operation DataExport => new BusinessOperation(nameof(DataExport));
    public static Operation UserRegistration => new BusinessOperation(nameof(UserRegistration));
    public static Operation PaymentProcessing => new BusinessOperation(nameof(PaymentProcessing));
    public static Operation ReportGeneration => new BusinessOperation(nameof(ReportGeneration));

    private BusinessOperation(string name) : base(name) { }
}
```

### Constructor

| Constructor | Access | Description |
|-------------|--------|-------------|
| `Operation(string name)` | `protected` | Creates operation with given name |

### Built-in Operations

| Property | Name | Typical Use |
|----------|------|-------------|
| `Operation.Unknown` | `"Unknown"` | Fallback |
| `Operation.StartUp` | `"StartUp"` | Application/service startup |
| `Operation.ShutDown` | `"ShutDown"` | Application/service shutdown |

### Formatting

- Rendered as: `Operation＝{Name}` (original case preserved)
- Position: First field in record (if present)
- No sanitisation applied to operation name

### Usage

```csharp
logger.Info(
    BusinessOperation.DataImport,
    OperationStatus.Started,
    "Import initiated",
    new LogInfo(AppLogKey.CorrelationId, "imp-789"));
```

**Output:** `Operation＝DataImport͵OperationStatus＝STARTED͵Message＝Import initiated͵CorrelationId＝imp-789`

---

## Domain Operation Statuses

### Contract

**File:** `OperationStatus.cs`
**Class:** `OperationStatus` (extensible via protected constructor)

### Extension Pattern

```csharp
public sealed class BusinessStatus : OperationStatus
{
    public static OperationStatus Pending => new BusinessStatus(nameof(Pending));
    public static OperationStatus Validating => new BusinessStatus(nameof(Validating));
    public static OperationStatus Processing => new BusinessStatus(nameof(Processing));
    public static OperationStatus Completed => new BusinessStatus(nameof(Completed));
    public static OperationStatus Failed => new BusinessStatus(nameof(Failed));
    public static OperationStatus RolledBack => new BusinessStatus(nameof(RolledBack));

    private BusinessStatus(string name) : base(name) { }
}
```

### Constructor

| Constructor | Access | Description |
|-------------|--------|-------------|
| `OperationStatus(string name)` | `protected` | Creates status with given name |

### Built-in Statuses

| Property | Name | Typical Use |
|----------|------|-------------|
| `OperationStatus.Unknown` | `"Unknown"` | Fallback |
| `OperationStatus.Started` | `"Started"` | Operation begun |
| `OperationStatus.Success` | `"Success"` | Completed successfully |
| `OperationStatus.Failure` | `"Failure"` | Failed |
| `OperationStatus.InProgress` | `"InProgress"` | Ongoing |

### Formatting

- Rendered as: `OperationStatus＝{Name.ToUpper()}` (**always upper-cased**)
- Position: Second field in record (if present, after Operation)
- Upper-casing applied in `LogMessageBuilder.Build`

### Usage

```csharp
logger.Info(
    BusinessOperation.PaymentProcessing,
    BusinessStatus.Processing,
    "Processing payment",
    new LogInfo(AppLogKey.TransactionId, "txn-456"));
```

**Output:** `Operation＝PaymentProcessing͵OperationStatus＝PROCESSING͵Message＝Processing payment͵TransactionId＝txn-456`

---

## Extension Composition

### Typical Consumer Setup

```csharp
// 1. Define domain keys
public sealed class AppLogKey : LogInfoKey
{
    public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
    public static LogInfoKey AccessToken => new AppLogKey(nameof(AccessToken), true);
    public AppLogKey(string name) : base(name) { }
    private AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}

// 2. Define domain operations
public sealed class AppOperation : Operation
{
    public static Operation ApiRequest => new AppOperation(nameof(ApiRequest));
    public static Operation DbMigration => new AppOperation(nameof(DbMigration));
    private AppOperation(string name) : base(name) { }
}

// 3. Define domain statuses
public sealed class AppStatus : OperationStatus
{
    public static OperationStatus Retrying => new AppStatus(nameof(Retrying));
    public static OperationStatus CircuitOpen => new AppStatus(nameof(CircuitOpen));
    private AppStatus(string name) : base(name) { }
}

// 4. Implement concrete sink
public sealed class AppLogger : Logger
{
    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        if (level > LogLevel.Debug) return;  // Configurable filter
        string line = logMessage();
        // Send to Seq, Elasticsearch, file, etc.
    }
}

// 5. Compose and use
ILogger logger = new AppLogger();
logger.SetSourceContext<Program>();

logger.Info(
    AppOperation.ApiRequest,
    AppStatus.Retrying,
    "Retrying API call",
    new LogInfo(AppLogKey.UserId, 42),
    new LogInfo(AppLogKey.AccessToken, "secret"));  // Masked
```

### Extension Dependency Graph

```
Consumer Application
        │
        ├─── AppLogKey ──────────────────┐
        ├─── AppOperation ───────────────┤  Extend library base classes
        ├─── AppStatus ──────────────────┤
        └─── AppLogger (extends Logger) ─┘  Implement WriteLog
                    │
                    ▼
            NuciLog.Core
            (Logger base, LogMessageBuilder,
             LogInfo, LogInfoKey, Operation,
             OperationStatus, LogLevel)
```

### Versioning Considerations

| Extension Point | Binary Compatibility | Source Compatibility |
|-----------------|---------------------|---------------------|
| `LogInfoKey` subclass | ✅ Adding static properties | ✅ |
| `Operation` subclass | ✅ Adding static properties | ✅ |
| `OperationStatus` subclass | ✅ Adding static properties | ✅ |
| `Logger` subclass | ✅ Override `WriteLog` | ✅ |
| `ILogger` implementation | ❌ Must implement all 240 overloads | ❌ Use `Logger` base |

**Recommendation:** Always extend `Logger`, never implement `ILogger` directly.

---

## Summary: Extension Point Checklist

| Extension | Base Class | Key Method/Constructor | Must Override |
|-----------|------------|------------------------|---------------|
| Concrete sink | `Logger` | `WriteLog(LogLevel, Func<string>)` | Yes (abstract) |
| Field keys | `LogInfoKey` | `protected LogInfoKey(name)` / `protected LogInfoKey(name, bool)` | Subclass with static properties |
| Operations | `Operation` | `protected Operation(name)` | Subclass with static properties |
| Statuses | `OperationStatus` | `protected OperationStatus(name)` | Subclass with static properties |

All extension points are **opt-in** — the library functions fully with only built-in types.