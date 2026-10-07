# Testing and Verification

Test organisation, coverage strategy, and behavioural contracts for NuciLog.Core.

## 📑 Table of Contents

- [Test Project Structure](#test-project-structure)
- [Test Organisation](#test-organisation)
- [Coverage Strategy](#coverage-strategy)
- [Behavioural Contracts](#behavioural-contracts)
- [Test Helpers](#test-helpers)
- [Running Tests](#running-tests)
- [Adding Tests](#adding-tests)

---

## Test Project Structure

```
NuciLog.Core.UnitTests/
├── NuciLog.Core.UnitTests.csproj
├── Helpers/
│   ├── TestLogger.cs
│   ├── TestLogInfoKey.cs
│   └── TestLogValues.cs
├── LoggerTests1Verbose.cs
├── LoggerTests2Debug.cs
├── LoggerTests3Info.cs
├── LoggerTests4Warn.cs
├── LoggerTests5Error.cs
├── LoggerTests6Fatal.cs
├── LogInfoTests.cs
└── LogMessageBuilderTests.cs
```

### Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="NUnit" Version="4.2.2" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\NuciLog.Core\NuciLog.Core.csproj" />
  </ItemGroup>
</Project>
```

---

## Test Organisation

### Per-Level Test Classes

Each log level has a dedicated test class (6 total):

| File | Level | Ordinal | Test Count (approx) |
|------|-------|---------|---------------------|
| `LoggerTests1Verbose.cs` | Verbose | 5 | 40+ |
| `LoggerTests2Debug.cs` | Debug | 4 | 40+ |
| `LoggerTests3Info.cs` | Info | 3 | 40+ |
| `LoggerTests4Warn.cs` | Warn | 2 | 40+ |
| `LoggerTests5Error.cs` | Error | 1 | 40+ |
| `LoggerTests6Fatal.cs` | Fatal | 0 | 40+ |

**Rationale:** Explicit per-level verification ensures all 240 overloads are exercised at each severity.

### Cross-Cutting Test Classes

| File | Focus |
|------|-------|
| `LogInfoTests.cs` | `LogInfo` construction, value conversion, edge cases |
| `LogMessageBuilderTests.cs` | `LogMessageBuilder.Build`, sanitisation, masking, de-duplication, exception enrichment |

### Naming Convention

```csharp
[TestFixture]
public class LoggerTests3Info
{
    [Test]
    public void GivenOperationAndMessage_WhenInfo_ThenLogLineContainsOperationAndMessage()
    {
        // ...
    }
}
```

**Pattern:** `Given{Precondition}_When{Action}_Then{ExpectedOutcome}`

---

## Coverage Strategy

### Overload Coverage

Each of the 240 `ILogger` overloads is tested at least once per level.

**Overload matrix per level (40 overloads):**

| Category | Count | Example |
|----------|-------|---------|
| Operation only | 1 | `Info(Operation)` |
| Operation + params LogInfo[] | 1 | `Info(Operation, details...)` |
| Operation + IEnumerable<LogInfo> | 1 | `Info(Operation, details)` |
| Operation + IEnumerable + params | 1 | `Info(Operation, details, extra...)` |
| Operation + Exception | 1 | `Info(Operation, exception)` |
| Operation + Exception + params | 1 | `Info(Operation, exception, details...)` |
| Operation + Exception + IEnumerable | 1 | `Info(Operation, exception, details)` |
| Operation + Exception + IEnumerable + params | 1 | `Info(Operation, exception, details, extra...)` |
| Operation + OperationStatus | 1 | `Info(Operation, status)` |
| Operation + OperationStatus + Exception | 1 | `Info(Operation, status, exception)` |
| Operation + OperationStatus + params | 1 | `Info(Operation, status, details...)` |
| Operation + OperationStatus + IEnumerable | 1 | `Info(Operation, status, details)` |
| Operation + OperationStatus + IEnumerable + params | 1 | `Info(Operation, status, details, extra...)` |
| Operation + OperationStatus + Exception + params | 1 | `Info(Operation, status, exception, details...)` |
| Operation + OperationStatus + Exception + IEnumerable | 1 | `Info(Operation, status, exception, details)` |
| Operation + OperationStatus + Exception + IEnumerable + params | 1 | `Info(Operation, status, exception, details, extra...)` |
| Message only | 1 | `Info("message")` |
| Message + params | 1 | `Info("message", details...)` |
| Message + IEnumerable | 1 | `Info("message", details)` |
| Message + IEnumerable + params | 1 | `Info("message", details, extra...)` |
| Message + Exception | 1 | `Info("message", exception)` |
| Message + Exception + params | 1 | `Info("message", exception, details...)` |
| Message + Exception + IEnumerable | 1 | `Info("message", exception, details)` |
| Message + Exception + IEnumerable + params | 1 | `Info("message", exception, details, extra...)` |
| Operation + Message | 1 | `Info(Operation, "message")` |
| Operation + Message + params | 1 | `Info(Operation, "message", details...)` |
| Operation + Message + IEnumerable | 1 | `Info(Operation, "message", details)` |
| Operation + Message + IEnumerable + params | 1 | `Info(Operation, "message", details, extra...)` |
| Operation + Message + Exception | 1 | `Info(Operation, "message", exception)` |
| Operation + Message + Exception + params | 1 | `Info(Operation, "message", exception, details...)` |
| Operation + Message + Exception + IEnumerable | 1 | `Info(Operation, "message", exception, details)` |
| Operation + Message + Exception + IEnumerable + params | 1 | `Info(Operation, "message", exception, details, extra...)` |
| Operation + OperationStatus + Message | 1 | `Info(Operation, status, "message")` |
| Operation + OperationStatus + Message + params | 1 | `Info(Operation, status, "message", details...)` |
| Operation + OperationStatus + Message + IEnumerable | 1 | `Info(Operation, status, "message", details)` |
| Operation + OperationStatus + Message + IEnumerable + params | 1 | `Info(Operation, status, "message", details, extra...)` |
| Operation + OperationStatus + Message + Exception | 1 | `Info(Operation, status, "message", exception)` |
| Operation + OperationStatus + Message + Exception + params | 1 | `Info(Operation, status, "message", exception, details...)` |
| Operation + OperationStatus + Message + Exception + IEnumerable | 1 | `Info(Operation, status, "message", exception, details)` |
| Operation + OperationStatus + Message + Exception + IEnumerable + params | 1 | `Info(Operation, status, "message", exception, details, extra...)` |

**Total:** 40 overloads × 6 levels = 240 overloads tested.

### Canonical Path Coverage

Every test ultimately exercises the **canonical method**:

```csharp
public void Info(Operation operation, OperationStatus operationStatus, string message, Exception exception, IEnumerable<LogInfo> logInfos, params LogInfo[] extraLogInfos)
{
    WriteLog(LogLevel.Info, () => LogMessageBuilder.Build(operation, operationStatus, message, exception, logInfos));
}
```

Tests verify:
1. Correct `LogLevel` passed to `WriteLog`
2. Correct parameters passed to `LogMessageBuilder.Build`
3. Formatted output matches expected grammar

### Sink Verification

`TestLogger` captures:
- `LastLogLevel` — the `LogLevel` passed to `WriteLog`
- `LastLogLine` — the result of invoking the formatter delegate

```csharp
public sealed class TestLogger : Logger
{
    public string LastLogLine { get; set; }
    public LogLevel LastLogLevel { get; set; }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        LastLogLevel = level;
        LastLogLine = logMessage();  // Invokes formatter
    }
}
```

**Verification pattern:**
```csharp
var logger = new TestLogger();
logger.Info("Test message");
Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Info));
Assert.That(logger.LastLogLine, Does.Contain("Message＝Test message"));
```

---

## Behavioural Contracts

### Contract 1: Overload Normalisation

**Requirement:** All overloads for a level produce identical output for equivalent inputs.

**Test:** For each level, invoke multiple overloads with semantically equivalent parameters; assert identical `LastLogLine`.

```csharp
[Test]
public void GivenEquivalentInputs_WhenDifferentOverloads_ThenSameOutput()
{
    var logger = new TestLogger();
    var op = Operation.StartUp;
    var details = new[] { new LogInfo(TestLogInfoKey.TestKey, "value") };

    logger.Info(op, "message", details);
    var line1 = logger.LastLogLine;

    logger.Info(op, "message", details.AsEnumerable());
    var line2 = logger.LastLogLine;

    Assert.That(line1, Is.EqualTo(line2));
}
```

### Contract 2: Level Filtering

**Requirement:** `WriteLog` receives correct `LogLevel` ordinal.

**Test:** Each level's canonical method passes its ordinal.

```csharp
[Test]
public void GivenInfoCall_WhenLogged_ThenLogLevelIsInfo()
{
    var logger = new TestLogger();
    logger.Info("test");
    Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Info));
}
```

### Contract 3: Lazy Formatter

**Requirement:** Formatter delegate not invoked if sink filters level.

**Test:** `NullLogger` never invokes formatter.

```csharp
[Test]
public void GivenNullLogger_WhenLogged_ThenFormatterNotInvoked()
{
    var logger = new NullLogger();
    bool formatterInvoked = false;
    logger.Info(() => { formatterInvoked = true; return "test"; });  // Not actual API — conceptual
    Assert.That(formatterInvoked, Is.False);
}
```

**Actual test:** Verify `NullLogger.WriteLog` body is empty (code inspection).

### Contract 4: Record Grammar

**Requirement:** Formatted output follows exact grammar.

**Grammar:**
```
[Operation＝Name͵][OperationStatus＝NAME͵]Message＝Text͵[Key＝Value͵]*
```

**Test:** `LogMessageBuilderTests` verify each component.

### Contract 5: Sanitisation

**Requirement:** Newlines replaced with literal `\n`; stack traces single-line.

**Test:** `LogMessageBuilderTests` verify sanitisation.

### Contract 6: Sensitive Masking

**Requirement:** Sensitive keys masked in output; original value preserved in `LogInfo.Value`.

**Test:** `LogMessageBuilderTests` verify masking algorithm.

### Contract 7: De-duplication

**Requirement:** Duplicate keys → first key identity, last value; vacant omitted.

**Test:** `LogMessageBuilderTests` verify de-duplication semantics.

### Contract 8: Exception Enrichment

**Requirement:** Exception adds `Exception`, `ExceptionMessage`, `StackTrace` keys.

**Test:** `LogMessageBuilderTests` verify exception fields.

### Contract 9: SourceContext Not Formatted

**Requirement:** `SourceContext` never appears in formatted output.

**Test:** Verify `LogMessageBuilder.Build` never appends `SourceContext`.

### Contract 10: Null Safety

**Requirement:** All `null` inputs handled without `NullReferenceException`.

**Test:** Each test class includes null-input variants.

---

## Test Helpers

### TestLogger

```csharp
public sealed class TestLogger : Logger
{
    public string LastLogLine { get; set; }
    public LogLevel LastLogLevel { get; set; }

    protected override void WriteLog(LogLevel level, Func<string> logMessage)
    {
        LastLogLevel = level;
        LastLogLine = logMessage();
    }
}
```

**Purpose:** Capture sink input for verification.

### TestLogInfoKey

```csharp
public sealed class TestLogInfoKey : LogInfoKey
{
    public static LogInfoKey TestKey => new TestLogInfoKey(nameof(TestKey));
    public static LogInfoKey TestKey2 => new TestLogInfoKey(nameof(TestKey2));
    public static LogInfoKey SensitiveTestKey => new TestLogInfoKey(nameof(SensitiveTestKey), true);

    public TestLogInfoKey(string name) : base(name) { }
    private TestLogInfoKey(string name, bool isSensitive) : base(name, isSensitive) { }
}
```

**Purpose:** Provide test keys with known names and sensitivity.

### TestLogValues

```csharp
public static class TestLogValues
{
    public static string DefaultExceptionLogMessage => "An exception has occurred.";
}
```

**Purpose:** Share expected default message string.

---

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj

# Run with verbose output
dotnet test NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj --verbosity normal

# Run specific test class
dotnet test NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj --filter "FullyQualifiedName~LoggerTests3Info"

# Run specific test method
dotnet test NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj --filter "FullyQualifiedName~GivenOperationAndMessage_WhenInfo_ThenLogLineContainsOperationAndMessage"

# Generate TRX report
dotnet test NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj --logger "trx;LogFileName=test_results.trx"
```

### IDE

- **VS Code:** Test Explorer (NUnit 3 Test Adapter)
- **Visual Studio:** Test Explorer
- **JetBrains Rider:** Unit Test Sessions

### CI/CD

```yaml
# .github/workflows/dotnet.yml
- name: Test
  run: dotnet test NuciLog.Core.UnitTests/NuciLog.Core.UnitTests.csproj --configuration Release --no-build
```

---

## Adding Tests

### New Level Test Class

1. Copy `LoggerTests3Info.cs` → `LoggerTests{Level}.cs`
2. Update level references (`Info` → `Warn`, `LogLevel.Info` → `LogLevel.Warn`)
3. Run tests — all should pass (same overload matrix)

### New Overload Test

1. Identify missing overload in `ILogger`
2. Add test method to appropriate level class
3. Follow naming convention: `Given{Precondition}_When{Action}_Then{ExpectedOutcome}`
4. Use `TestLogger` to capture output
5. Assert `LastLogLevel` and `LastLogLine`

### New Behaviour Test

1. Add to `LogMessageBuilderTests.cs` or `LogInfoTests.cs`
2. Test the specific transformation (sanitisation, masking, de-dup, etc.)
3. Use direct `LogMessageBuilder.Build` calls (no `Logger` needed)

### Test Template

```csharp
[TestFixture]
public class LoggerTests{Level}
{
    private TestLogger _logger;

    [SetUp]
    public void SetUp() => _logger = new TestLogger();

    [Test]
    public void Given{Precondition}_When{Action}_Then{ExpectedOutcome}
    {
        // Arrange
        var operation = Operation.StartUp;
        var message = "Test message";
        var details = new[] { new LogInfo(TestLogInfoKey.TestKey, "value") };

        // Act
        _logger.{Level}(operation, message, details);

        // Assert
        Assert.That(_logger.LastLogLevel, Is.EqualTo(LogLevel.{Level}));
        Assert.That(_logger.LastLogLine, Does.Contain("Operation＝StartUp"));
        Assert.That(_logger.LastLogLine, Does.Contain("Message＝Test message"));
        Assert.That(_logger.LastLogLine, Does.Contain("TestKey＝value"));
    }
}
```

---

## Verification Checklist

Before merging changes, verify:

- [ ] All 240 overloads tested at each level (6 × 40 = 240 tests minimum)
- [ ] `LogMessageBuilderTests` cover: sanitisation, masking, de-duplication, exception enrichment, omission
- [ ] `LogInfoTests` cover: all `GetStringValue` type conversions, null handling, format overloads
- [ ] `NullLogger` behaviour verified (formatter never invoked)
- [ ] `SourceContext` never appears in formatted output
- [ ] Delimiter characters (`＝` U+FF1D, `͵` U+0375) used correctly
- [ ] No trailing delimiter in output
- [ ] Tests pass on `net10.0` target framework
- [ ] No flaky tests (no timing, no external dependencies)

---

## Test Metrics (Current)

| Metric | Value |
|--------|-------|
| Test classes | 8 |
| Test methods | ~280 |
| Lines of test code | ~3,500 |
| Target framework | net10.0 |
| Test framework | NUnit 4.2.2 |
| Coverage target | 100% of public API surface |

---

## Summary

| Aspect | Approach |
|--------|----------|
| **Organisation** | Per-level test classes + cross-cutting classes |
| **Coverage** | All 240 overloads × 6 levels + builder transformations |
| **Sink** | `TestLogger` captures level and formatted line |
| **Keys** | `TestLogInfoKey` provides known sensitive/non-sensitive keys |
| **Assertions** | NUnit `Assert.That` with constraints |
| **Execution** | `dotnet test` with NUnit 3 adapter |
| **CI** | GitHub Actions `dotnet.yml` workflow |