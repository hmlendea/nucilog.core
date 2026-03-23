[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Build Status](https://github.com/hmlendea/nucilog.core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucilog.core/actions/workflows/dotnet.yml) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/nucilog.core)](https://github.com/hmlendea/nucilog.core/releases/latest)

# NuciLog.Core

NuciLog.Core is a lightweight logging abstraction for .NET applications that want predictable, structured log messages without coupling application code directly to a specific logging backend.

The library focuses on three things:

- a consistent set of log levels
- operation-aware logging
- structured `key=value` log output

It is designed to be extended by implementing a custom logger sink, while the base `Logger` class handles overload normalization and message construction.

# Features

- `Verbose`, `Debug`, `Info`, `Warn`, `Error`, and `Fatal` log levels
- operation and operation status tracking
- structured log details through `LogInfo`
- exception enrichment with exception type, message, and stack trace
- predictable output formatting for downstream processing
- no-op `NullLogger` implementation for disabled logging scenarios

# Target Framework

The package currently targets `.NET 10.0`.

# Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciLog.Core)

**.NET CLI**:
```bash
dotnet add package NuciLog.Core
```

**Package Manager**:
```powershell
Install-Package NuciLog.Core
```

# Quick Start

`Logger` is an abstract base class. To use the library, create a concrete implementation and write the final log line to your preferred destination.

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

public sealed class AppLogKey(string name) : LogInfoKey(name)
{
	public static LogInfoKey UserId => new AppLogKey(nameof(UserId));
	public static LogInfoKey CorrelationId => new AppLogKey(nameof(CorrelationId));
}
```

Example usage:

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

Possible output:

```text
2026-03-23T11:22:33.1090023+02:00||INFO|Operation=StartUp,OperationStatus=STARTED,Message=Initialising service,CorrelationId=req-42
2026-03-23T11:22:33.1090123+02:00||INFO|Operation=StartUp,OperationStatus=SUCCESS,UserId=1234
2026-03-23T11:22:33.1090223+02:00||INFO|Operation=StartUp,OperationStatus=FAILURE,Message=Service failed to start,CorrelationId=req-42,Exception=System.InvalidOperationException,ExceptionMessage=Configuration file is invalid
```

# Concepts

## Logger

`Logger` provides the implementation for the large set of convenience overloads exposed by `ILogger`. All overloads eventually resolve to:

```csharp
protected abstract void WriteLog(LogLevel level, Func<string> logMessage)
```

This keeps application code simple while leaving the final output destination under your control.

## Log Levels

The available log levels are:

- `Verbose`
- `Debug`
- `Info`
- `Warn`
- `Error`
- `Fatal`

## Operations and Statuses

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

## Structured Details

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

To define custom keys, derive from `LogInfoKey` and expose strongly named static members, as shown in the quick start example.

# Output Format

Generated log lines use comma-separated `key=value` pairs.

The output order is:

1. `Operation`
2. `OperationStatus`
3. message and custom structured fields
4. exception-related fields

Example:

```text
Operation=StartUp,OperationStatus=SUCCESS,Message=Ready,UserId=1234
```

Important formatting rules:

- empty or whitespace values are omitted
- duplicate keys are collapsed so the last value wins
- if the final value of a duplicate key is empty, that key is removed
- line breaks inside values are converted to `\n`
- commas inside values are replaced with `͵` (unicode look-alike character) to preserve parsing
- when logging an exception without a message, `Message=An exception has occurred.` is added automatically

# API Overview

The most commonly used members are:

- `ILogger` for the logging contract
- `Logger` for the base implementation
- `NullLogger` for a no-op logger
- `LogInfo` for structured details
- `LogInfoKey` for defining structured field names
- `Operation` and `OperationStatus` for contextual logging

Common overload patterns are available for each log level:

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

# Source Context

The logger exposes source context through:

```csharp
logger.SetSourceContext<Program>();
logger.SetSourceContext(typeof(Program));
```

This sets the `SourceContext` property on the logger instance. At the moment, source context is stored on the logger but is not automatically emitted as part of the generated log message.

# Null Logger

Use `NullLogger` when you need an `ILogger` implementation that safely ignores all log calls:

```csharp
ILogger logger = new NullLogger();
```

This is useful for tests, optional integrations, or disabled logging pipelines.

# Development

Build the solution:

```bash
dotnet build NuciLog.sln
```

Run the tests:

```bash
dotnet test NuciLog.sln
```

The test suite covers:

- log level dispatch for all severity levels
- structured message building
- exception formatting
- duplicate key handling
- sanitisation of commas and new lines

# License

Licensed under GNU GPL v3. See `LICENSE` for details.
