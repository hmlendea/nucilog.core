[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucilog.core)](https://github.com/hmlendea/nucilog.core/releases/latest)
[![Build Status](https://github.com/hmlendea/nucilog.core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucilog.core/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# NuciLog.Core

NuciLog.Core is a lightweight logging abstraction for .NET applications that want predictable, structured log messages without coupling application code directly to a specific logging backend.

## 📑 Table of Contents

- [Capabilities](#-capabilities)
- [Usage](#-usage)
- [Installation](#-installation)
- [Concepts](#concepts)
- [Development](#-development)
- [Project Structure](#-project-structure)
- [Architecture](#-architecture)
- [Contributing](#-contributing)
- [Security](#-security)
- [Project Engagement](#-project-engagement)

## ✨ Capabilities

- `Verbose`, `Debug`, `Info`, `Warn`, `Error`, and `Fatal` log levels
- Operation and operation status tracking for semantic context
- Structured `key=value` log output with lazy message formatting
- Sensitive structured detail masking through `LogInfoKey`
- Exception enrichment with exception type, message, and stack trace
- Predictable output formatting for downstream processing
- No-op `NullLogger` implementation for disabled logging scenarios
- Fully extensible via custom `Logger` implementations

## 🚀 Usage

`Logger` is an abstract base class. To use the library, create a concrete implementation and write the final log line to your preferred destination.

### Custom Logger Implementation

```csharp
using System;
using NuciLog.Core;

public sealed class ConsoleLogger : Logger
{
	protected override void WriteLog(LogLevel level, Func<string> logMessage)
	{
		Console.WriteLine($"[{level}] {logMessage()}");
	}
}

public sealed class AppLogKey : LogInfoKey
{
	public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
	public static LogInfoKey CorrelationId => new AppLogKey(nameof(CorrelationId));
	public static LogInfoKey AccessToken => new AppLogKey(nameof(AccessToken), true);

	public AppLogKey(string name)
		: base(name) { }

	private AppLogKey(string name, bool isSensitive)
		: base(name, isSensitive) { }
}
```

### Example Usage

```csharp
using NuciLog.Core;

ILogger logger = new ConsoleLogger();
logger.SetSourceContext<Program>();

logger.Info(
	Operation.StartUp,
	OperationStatus.Started,
	"Initialising service",
	new LogInfo(AppLogKey.CorrelationId, "req-42"));

logger.Info(
	Operation.StartUp,
	OperationStatus.Success,
	new LogInfo(AppLogKey.UserId, 1234));

try
{
	throw new InvalidOperationException("Configuration file is invalid");
}
catch (Exception ex)
{
	logger.Error(
		Operation.StartUp,
		OperationStatus.Failure,
		"Service failed to start",
		ex,
		new LogInfo(AppLogKey.CorrelationId, "req-42"));
}
```

### Example Output

```text
2026-03-23T11:22:33.1090023+02:00||INFO|Operation＝StartUp͵OperationStatus＝STARTED͵Message＝Initialising service͵CorrelationId＝req-42
2026-03-23T11:22:33.1090123+02:00||INFO|Operation＝StartUp͵OperationStatus＝SUCCESS͵UserId＝1234
2026-03-23T11:22:33.1090223+02:00||ERROR|Operation＝StartUp͵OperationStatus＝FAILURE͵Message＝Service failed to start͵CorrelationId＝req-42͵Exception＝System.InvalidOperationException͵ExceptionMessage＝Configuration file is invalid
```

## 📦 Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciLog.Core)

### Package Manager Installation

```bash
dotnet add package NuciLog.Core
```

Or, via the Package Manager Console:

```powershell
Install-Package NuciLog.Core
```

## Concepts

### Logger

`Logger` provides the implementation for a large set of convenience overloads exposed by `ILogger`. All overloads eventually resolve to:

```csharp
protected abstract void WriteLog(LogLevel level, Func<string> logMessage)
```

This keeps application code simple while leaving the final output destination under your control.

### Log Levels

The available log levels are, from least to most severe:

- `Verbose`
- `Debug`
- `Info`
- `Warn`
- `Error`
- `Fatal`

### Operations and Statuses

Operations provide semantic context for application lifecycle or business actions.

Built-in operations:

- `Unknown`
- `StartUp`
- `ShutDown`

Built-in operation statuses:

- `Unknown`
- `Started`
- `Success`
- `Failure`
- `InProgress`

You can introduce your own domain-specific operations or statuses by deriving from `Operation` or `OperationStatus`.

### Structured Details

Structured fields are added through `LogInfo` instances:

```csharp
logger.Info(
	"User authenticated",
	new LogInfo(AppLogKey.UserId, 1234),
	new LogInfo(AppLogKey.CorrelationId, "req-42"));
```

Each `LogInfo` contains:

- a `LogInfoKey`
- a stringified value

Values can be created from:

- `string`
- `object`
- `DateTime` with a custom format
- `DateTimeOffset` with a custom format

When using the `object` overload, common values are normalised as follows:

- `null` becomes an empty string
- `DateTime` and `DateTimeOffset` use round-trip (`o`) format
- `TimeSpan` uses constant (`c`) format
- Enums use general (`G`) format
- Arrays and enumerables are joined with `;`
- Dictionaries are rendered as `key=value;key2=value2;`

To define custom keys, derive from `LogInfoKey` and expose strongly named static members, as shown in the usage example. Use the protected `LogInfoKey(string name, bool isSensitive)` constructor for keys whose values are sensitive. Sensitive values are masked when non-empty by displaying the first four characters, five stars, and the last two characters; empty and whitespace values are omitted.

### Output Format

Generated log lines use `key＝value` pairs separated by `͵`.

The output order is:

1. `Operation`
2. `OperationStatus`
3. message and custom structured fields
4. exception-related fields

Example:

```text
Operation＝StartUp͵OperationStatus＝SUCCESS͵Message＝Ready͵UserId＝1234
```

Important formatting rules:

- empty or whitespace values are omitted
- duplicate keys are collapsed so the last value wins
- if the final value of a duplicate key is empty, that key is removed
- values for sensitive keys are masked by displaying the first four characters, five stars, and the last two characters
- line breaks inside values are converted to `\n`
- field separators use look-alike unicode characters (`＝` U+FF1D for `=`, `͵` U+0375 for `,`) so values are never modified
- when logging an exception without a message, `Message＝An exception has occurred.` is added automatically

## API Overview

The most commonly used members are:

- `ILogger` for the logging contract
- `Logger` for the base implementation
- `NullLogger` for a no-op logger
- `LogInfo` for structured details
- `LogInfoKey` for defining structured field names
- `Operation` and `OperationStatus` for contextual logging

Common overload patterns exist for each log level:

- message only
- operation only
- operation with status
- message with exception
- operation with message
- operation with status and message
- any of the above with `params LogInfo[]`
- any of the above with `IEnumerable<LogInfo>`

Examples:

```csharp
logger.Debug("Cache warmup started");
logger.Info(Operation.StartUp, OperationStatus.Started);
logger.Warn(Operation.ShutDown, "Shutdown taking longer than expected");
logger.Error("Request failed", ex);
logger.Fatal(Operation.ShutDown, OperationStatus.Failure, "Host terminated", ex);
```

## Source Context

The logger exposes source context through:

```csharp
logger.SetSourceContext<Program>();
logger.SetSourceContext(typeof(Program));
```

Source context is stored on the logger instance; at present, it is not automatically emitted as part of the generated log message.

## Null Logger

Use `NullLogger` when you need an `ILogger` implementation that safely ignores all log calls:

```csharp
ILogger logger = new NullLogger();
```

This is useful for tests, optional integrations, or disabled logging pipelines.

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

Clone the repository and restore dependencies:

```bash
git clone https://github.com/hmlendea/nucilog.core.git
cd nucilog.core
dotnet restore
```

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

## 🗂️ Project Structure

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `NuciLog.Core` | Library | Core logging abstraction, types, and public API |
| `NuciLog.Core.UnitTests` | Tests | Comprehensive unit tests for all log levels and functionality |

### Directories

| Directory | Purpose |
|-----------|---------|
| `NuciLog.Core/` | Library source code |
| `NuciLog.Core.UnitTests/` | Unit test suite with organised test classes per log level |
| `.github/workflows/` | GitHub Actions workflows for build and release automation |

## 🏗️ Architecture

See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Add tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/nucilog.core/issues) for problems or suggestions

## 🔒 Security

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nucilog.core/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!
