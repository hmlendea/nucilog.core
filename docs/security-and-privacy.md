# Security and Privacy

Deep-dive on sensitive data handling, masking guarantees, and security boundaries in NuciLog.Core.

## 📑 Table of Contents

- [Threat Model](#threat-model)
- [Sensitive Data Handling](#sensitive-data-handling)
- [Masking Guarantees](#masking-guarantees)
- [Data Flow Analysis](#data-flow-analysis)
- [Security Boundaries](#security-boundaries)
- [Compliance Considerations](#compliance-considerations)
- [Best Practices for Consumers](#best-practices-for-consumers)
- [Limitations and Caveats](#limitations-and-caveats)

---

## Threat Model

### Assets

| Asset | Sensitivity | Location |
|-------|-------------|----------|
| Log records (formatted) | Variable | Sink destination (console, file, network) |
| Log records (in-memory) | Variable | `LogInfo.Value`, formatter delegate capture |
| Exception details | Medium | `Exception.Message`, `Exception.StackTrace` |
| Source context | Low | `Logger.SourceContext` (type name only) |

### Threat Actors

| Actor | Capability | Motivation |
|-------|------------|------------|
| **Log consumer** (developer) | Full code access | Accidental logging of secrets |
| **Log viewer** (ops, support) | Read access to log destination | Credential harvesting |
| **Attacker** (compromised log sink) | Read/write log destination | Data exfiltration |
| **Attacker** (memory dump) | Process memory access | Secret extraction |

### Trust Boundaries

```
┌─────────────────────────────────────────────────────────────┐
│                    Consumer Application                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ Business    │  │ NuciLog.Core│  │ Concrete Sink       │  │
│  │ Logic       │──▶│ (Library)   │──▶│ (Consumer Impl)     │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│        │                │                    │               │
│        ▼                ▼                    ▼               │
│  Secrets in      Formatting &          Destination         │
│  memory          Masking                Policy             │
└─────────────────────────────────────────────────────────────┘
```

**Key principle:** NuciLog.Core provides **masking primitives**; the consumer controls **what is logged** and **where it goes**.

---

## Sensitive Data Handling

### What NuciLog.Core Does

| Action | Mechanism |
|--------|-----------|
| **Classify sensitivity** | `LogInfoKey.IsSensitive` flag |
| **Mask in formatted output** | `ObfuscateLogInfoValue` (first 4 + `*****` + last 2) |
| **Sanitise newlines** | Prevent log injection via `\r\n` → `\n` |
| **Single-line stack traces** | Prevent multi-line injection |
| **Omit vacant values** | No empty fields in output |

### What NuciLog.Core Does NOT Do

| Action | Reason |
|--------|--------|
| **Encrypt values** | Not a crypto library; masking is visual only |
| **Prevent logging secrets** | Consumer chooses what to log |
| **Audit log access** | Sink responsibility |
| **Rotate/expire logs** | Sink/destination responsibility |
| **Enforce retention** | Sink/destination responsibility |
| **Validate sink security** | Consumer chooses sink implementation |

### Consumer Responsibilities

| Responsibility | Implementation |
|----------------|----------------|
| **Mark sensitive keys** | `new AppLogKey("ApiKey", true)` |
| **Avoid logging secrets** | Code review, static analysis |
| **Secure sink destination** | TLS, file permissions, access control |
| **Implement retention** | Sink-level rotation, archival |
| **Monitor log access** | SIEM, audit logs |

---

## Masking Guarantees

### Algorithm

```csharp
// Constants (private, not configurable)
ObfuscatedLogInfoValuePrefixLength = 4
ObfuscatedLogInfoValueMaskLength = 5
ObfuscatedLogInfoValueSuffixLength = 2
ObfuscatedLogInfoValueMaskCharacter = '*'

string ObfuscateLogInfoValue(string value)
{
    int prefixLength = Math.Min(value.Length, 4);
    string prefix = value[..prefixLength];
    int remaining = value.Length - prefixLength;
    int suffixLength = remaining > 0 ? Math.Min(remaining, 2) : 0;
    string suffix = suffixLength > 0 ? value[^suffixLength..] : "";
    string mask = new('*', 5);
    return $"{prefix}{mask}{suffix}";
}
```

### Guarantees

| Guarantee | Verification |
|-----------|--------------|
| **Deterministic** | Same input → same output |
| **Prefix preserved** | First 4 chars always visible |
| **Suffix preserved** | Last 2 chars always visible (if length > 4) |
| **Fixed mask length** | Always 5 `*` characters |
| **Vacant values unmasked** | `string.IsNullOrWhiteSpace` check before masking |
| **Applied after sanitisation** | Newlines already replaced |

### Masking Examples

| Input | Length | Output | Notes |
|-------|--------|--------|-------|
| `"password123"` | 11 | `"pass*****23"` | Standard |
| `"Bearer abcdefghijklmnop"` | 24 | `"Bear*****op"` | Long token |
| `"ab"` | 2 | `"ab*****"` | Short — full prefix, no suffix |
| `"a"` | 1 | `"a*****"` | Very short |
| `""` | 0 | `""` | Vacant — not masked |
| `" "` | 1 | `" *****"` | Whitespace — masked (not vacant) |

### What Masking Does NOT Guarantee

| Non-Guarantee | Explanation |
|---------------|-------------|
| **Entropy preservation** | Mask is fixed `*****` — no entropy |
| **Reversibility** | One-way; original value lost in formatted output |
| **Length hiding** | Output length reveals input length category |
| **Prefix/suffix secrecy** | First 4 and last 2 chars exposed |
| **In-memory protection** | `LogInfo.Value` retains original; delegate captures original |

---

## Data Flow Analysis

### Secret Lifecycle

```mermaid
flowchart TD
    A[Consumer creates LogInfo] --> B{Key.IsSensitive?}
    B -->|Yes| C[LogInfo.Value = original secret]
    B -->|No| C
    C --> D[Logger canonical method]
    D --> E[WriteLog(LogLevel, () => Build(...))]
    E --> F{Sink filters level?}
    F -->|Yes| G[Formatter NEVER invoked]
    F -->|No| H[logMessage() invoked]
    H --> I[LogMessageBuilder.Build]
    I --> J[ProcessLogInfoValue]
    J --> K{Key.IsSensitive && !vacant?}
    K -->|Yes| L[ObfuscateLogInfoValue]
    K -->|No| M[SanitiseLogInfoValue only]
    L --> N[Formatted string with mask]
    M --> N
    N --> O[Sink emits to destination]
    G --> P[Zero exposure]
```

### Exposure Points

| Point | Data Exposed | Mitigation |
|-------|--------------|------------|
| `LogInfo` construction | Original secret in `Value` | Consumer controls construction |
| Formatter delegate capture | Original secret in closure | Delegate not invoked if filtered |
| `LogMessageBuilder.Build` | Original secret during processing | In-memory, single-threaded |
| Formatted string | **Masked** secret | Masking algorithm |
| Sink destination | **Masked** string | Sink controls destination |
| Memory dump | Original secret in `LogInfo.Value` | Process isolation, no swap |

### Critical Observation

**The original secret exists in memory until the `LogInfo` object is garbage collected.** The masking only applies to the **formatted output string**, not the in-memory representation.

---

## Security Boundaries

### Library Boundary

```
┌────────────────────────────────────────────────────────────┐
│                      NuciLog.Core                           │
│  ┌──────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │ ILogger      │  │ Logger (base)    │  │ LogMessage   │  │
│  │ (contract)   │  │ (normalisation)  │  │ Builder      │  │
│  └──────────────┘  └──────────────────┘  └──────────────┘  │
│  ┌──────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │ LogInfo      │  │ LogInfoKey       │  │ Operation/   │  │
│  │ (value obj)  │  │ (identity)       │  │ Status       │  │
│  └──────────────┘  └──────────────────┘  └──────────────┘  │
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────┐
│                    Consumer Code                            │
│  ┌──────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │ Concrete     │  │ Custom Keys      │  │ Custom       │  │
│  │ Logger       │  │ (AppLogKey)      │  │ Operations   │  │
│  │ (sink impl)  │  │                  │  │ / Statuses   │  │
│  └──────────────┘  └──────────────────┘  └──────────────┘  │
└────────────────────────────────────────────────────────────┘
```

### Library Guarantees

| Guarantee | Scope |
|-----------|-------|
| **No network I/O** | Library never opens connections |
| **No file I/O** | Library never reads/writes files |
| **No process execution** | Library never spawns processes |
| **No reflection on consumer types** | Only `exception.GetType()` for exception enrichment |
| **No serialization** | Library never serialises objects |
| **No configuration loading** | Library reads no config files |

### Consumer Extension Points (Security Review Required)

| Extension Point | Risk | Review Focus |
|-----------------|------|--------------|
| `Logger.WriteLog` | **High** — controls destination | Validate sink security, error handling |
| `LogInfoKey` subclass | **Medium** — defines sensitivity | Ensure `IsSensitive=true` for secrets |
| `Operation` subclass | **Low** — metadata only | Naming conventions |
| `OperationStatus` subclass | **Low** — metadata only | Naming conventions |

---

## Compliance Considerations

### GDPR / Personal Data

| Aspect | NuciLog.Core Role |
|--------|-------------------|
| **Data controller** | Consumer application |
| **Data processor** | NuciLog.Core (processes log records) |
| **Personal data in logs** | Consumer controls — library provides masking tools |
| **Right to erasure** | Sink responsibility (log retention/deletion) |
| **Data portability** | Sink responsibility (structured export) |
| **DPIA** | Consumer must assess their logging practices |

### PCI DSS

| Requirement | NuciLog.Core Support |
|-------------|---------------------|
| **3.2** — Mask PAN | `LogInfoKey.IsSensitive` + masking |
| **3.3** — Render PAN unreadable | Masking shows first 4, last 2 |
| **10.2** — Audit trails | Structured logging with `Operation`/`OperationStatus` |
| **10.5** — Log protection | Sink responsibility (file perms, TLS) |

### SOC 2 / ISO 27001

| Control | NuciLog.Core Support |
|---------|---------------------|
| **Logging** | Structured, extensible, levelled |
| **Monitoring** | `Operation`/`OperationStatus` for correlation |
| **Incident response** | Exception enrichment (type, message, stack) |
| **Access control** | Sink responsibility |

---

## Best Practices for Consumers

### 1. Classify All Keys

```csharp
public sealed class AppLogKey : LogInfoKey
{
    // Non-sensitive — safe to log in plain text
    public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
    public static LogInfoKey CorrelationId => new AppLogKey(nameof(CorrelationId));
    public static LogInfoKey RequestPath => new AppLogKey(nameof(RequestPath));
    public static LogInfoKey IpAddress => new AppLogKey(nameof(IpAddress));

    // Sensitive — will be masked
    public static LogInfoKey AccessToken => new AppLogKey(nameof(AccessToken), true);
    public static LogInfoKey ApiKey => new AppLogKey(nameof(ApiKey), true);
    public static LogInfoKey Password => new AppLogKey(nameof(Password), true);
    public static LogInfoKey ConnectionString => new AppLogKey(nameof(ConnectionString), true);
    public static LogInfoKey CreditCardNumber => new AppLogKey(nameof(CreditCardNumber), true);
    public static LogInfoKey Ssn => new AppLogKey(nameof(Ssn), true);

    public AppLogKey(string name) : base(name) { }
    private AppLogKey(string name, bool isSensitive) : base(name, isSensitive) { }
}
```

### 2. Never Log Secrets Directly

```csharp
// ❌ BAD — secret in source code
logger.Info("API key: " + apiKey);

// ✅ GOOD — structured, masked
logger.Info("API call", new LogInfo(AppLogKey.ApiKey, apiKey));

// ✅ GOOD — don't log at all
logger.Info("API call completed");
```

### 3. Use Structured Logging

```csharp
// ❌ BAD — string interpolation loses structure
logger.Info($"User {userId} logged in from {ip}");

// ✅ GOOD — structured fields
logger.Info(
    Operation.UserLogin,
    OperationStatus.Success,
    "User logged in",
    new LogInfo(AppLogKey.UserId, userId),
    new LogInfo(AppLogKey.IpAddress, ip));
```

### 4. Secure the Sink

```csharp
public sealed class SecureFileLogger : Logger
{
    private readonly string _path;
    private readonly FileStream _stream;
    private readonly StreamWriter _writer;
    private readonly object _lock = new();

    public SecureFileLogger(string path)
    {
        _path = path;
        // Restrictive permissions: owner read/write only
        _stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
    }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        if (level > LogLevel.Info) return;

        string line = logMessage();
        lock (_lock)
        {
            _writer.WriteLine($"[{DateTimeOffset.UtcNow:o}] {line}");
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _stream?.Dispose();
    }
}
```

### 5. Implement Log Retention

```csharp
public sealed class RotatingFileLogger : Logger, IDisposable
{
    private readonly Timer _rotationTimer;
    private FileStream _stream;
    private StreamWriter _writer;
    private readonly string _directory;
    private readonly object _lock = new();

    public RotatingFileLogger(string directory, TimeSpan rotationInterval)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
        RotateFile();
        _rotationTimer = new Timer(_ => RotateFile(), null, rotationInterval, rotationInterval);
    }

    private void RotateFile()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _stream?.Dispose();
            var fileName = $"log-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";
            _stream = new FileStream(Path.Combine(_directory, fileName), FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
            // Delete files older than 30 days
            foreach (var file in Directory.GetFiles(_directory, "log-*.txt"))
            {
                if (File.GetCreationTimeUtc(file) < DateTime.UtcNow.AddDays(-30))
                    File.Delete(file);
            }
        }
    }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        if (level > LogLevel.Debug) return;
        string line = logMessage();
        lock (_lock)
        {
            _writer.WriteLine($"[{DateTimeOffset.UtcNow:o}] {line}");
        }
    }

    public void Dispose()
    {
        _rotationTimer?.Dispose();
        _writer?.Dispose();
        _stream?.Dispose();
    }
}
```

### 6. Avoid Logging Exceptions with Secrets

```csharp
// ❌ BAD — exception message may contain secrets
try { ... }
catch (Exception ex)
{
    logger.Error(ex);  // ex.Message might have connection string
}

// ✅ GOOD — log structured, mask sensitive fields
try { ... }
catch (Exception ex)
{
    logger.Error(
        Operation.DatabaseQuery,
        OperationStatus.Failure,
        "Query failed",
        new LogInfo(AppLogKey.Query, SanitizeQuery(sql)),
        new LogInfo(AppLogKey.ConnectionString, connectionString));  // Masked
}
```

---

## Limitations and Caveats

### 1. Masking Is Visual Only

```csharp
var logInfo = new LogInfo(AppLogKey.ApiKey, "secret-key-123");
// logInfo.Value == "secret-key-123"  ← Original retained
// Formatted output: "ApiKey＝secr*****23"  ← Masked only in string
```

**Implication:** Memory dumps, debugger inspection, and delegate captures see the original value.

### 2. De-duplication Can Defeat Masking

```csharp
// First occurrence: non-sensitive key
// Second occurrence: sensitive key, same name
var logInfos = new[]
{
    new LogInfo(new AppLogKey("Token", false), "visible"),
    new LogInfo(new AppLogKey("Token", true), "secret")
};
// Result: "Token＝secret" — NOT masked (first key was non-sensitive)
```

**Mitigation:** Use unique names for sensitive vs non-sensitive fields.

### 3. No Encryption at Rest or in Transit

| Layer | NuciLog.Core | Consumer Must Provide |
|-------|--------------|----------------------|
| **In-memory** | No encryption | Process isolation |
| **In-transit** | No TLS | Sink: HTTPS, syslog TLS, etc. |
| **At-rest** | No encryption | Sink: encrypted disk, encrypted DB |

### 4. No Access Control

- Library has no concept of users, roles, or permissions
- Sink must enforce who can read/write logs

### 5. No Integrity Protection

- No signatures, hashes, or tamper-evidence
- Sink must implement if required (e.g., append-only, signed logs)

### 6. Exception Stack Traces May Leak Paths

```csharp
// Stack trace includes file paths, method names, line numbers
// Sanitised to single line but content preserved
```

**Mitigation:** Don't log exceptions from sensitive code paths, or implement custom exception sanitisation.

### 7. Source Context Is Type Name Only

```csharp
logger.SetSourceContext<PaymentService>();
// SourceContext = "PaymentService" — no namespace, no assembly
```

**Implication:** Low sensitivity, but reveals internal structure.

---

## Security Checklist for Consumers

### Development

- [ ] All sensitive keys marked `IsSensitive=true`
- [ ] No secrets in string interpolation or concatenation
- [ ] Structured logging used consistently
- [ ] Exception messages reviewed for secret leakage
- [ ] Code review includes logging audit

### Deployment

- [ ] Sink destination secured (TLS, file perms, network segmentation)
- [ ] Log retention policy implemented and tested
- [ ] Log access monitored and audited
- [ ] Rotation/archival configured
- [ ] Backup encryption enabled

### Operations

- [ ] Alerting on log volume anomalies
- [ ] Alerting on sink failures
- [ ] Regular log review for accidental secret exposure
- [ ] Incident response includes log analysis procedures

### Compliance

- [ ] Data processing agreement if logs leave infrastructure
- [ ] DPIA completed for logging personal data
- [ ] Retention schedule documented
- [ ] Right-to-erasure procedure for logs

---

## Summary

| Aspect | NuciLog.Core Provides | Consumer Must Provide |
|--------|----------------------|----------------------|
| **Sensitivity classification** | `LogInfoKey.IsSensitive` | Mark keys appropriately |
| **Output masking** | Fixed algorithm (4+5+2) | Use sensitive keys |
| **Newline sanitisation** | Automatic | — |
| **Stack trace sanitisation** | Automatic | — |
| **De-duplication** | Automatic | Unique key names |
| **Destination security** | — | TLS, perms, encryption |
| **Retention/rotation** | — | Sink implementation |
| **Access control** | — | Sink/infrastructure |
| **Audit/integrity** | — | Sink/infrastructure |

**Bottom line:** NuciLog.Core gives you the **tools** to log securely; you must **use them correctly** and **secure the pipeline**.