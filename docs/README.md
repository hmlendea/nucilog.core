# NuciLog.Core Documentation

This directory contains implementation-grounded documentation for NuciLog.Core, a .NET logging abstraction library. Each document is derived from source code, tests, and architectural evidence.

## 📑 Documentation Index

| Document | Purpose |
|----------|---------|
| [Architecture Overview](architecture-overview.md) | High-level architectural decomposition, component relationships, and design principles |
| [Public API Reference](public-api-reference.md) | Complete reference for `ILogger`, `Logger`, and public data types |
| [Record Construction](record-construction.md) | Detailed specification of the log record grammar, formatting pipeline, and sanitisation rules |
| [Extension Points](extension-points.md) | Contracts for concrete sinks, custom field keys, domain operations, and statuses |
| [Runtime Execution Flows](runtime-execution-flows.md) | End-to-end call sequences from consumer invocation to sink emission |
| [Data Model and Transformations](data-model-and-transformations.md) | `LogInfo` value conversion, `LogInfoKey` sensitivity, and builder processing |
| [Testing and Verification](testing-and-verification.md) | Test organisation, coverage strategy, and behavioural contracts verified by tests |
| [Security and Privacy](security-and-privacy.md) | Sensitive value masking, data handling, and security boundary analysis |
| [Compatibility and Versioning](compatibility-and-versioning.md) | Binary/source compatibility, target framework, and packaging model |
| [Source Map](source-map.md) | File-to-responsibility mapping for rapid navigation |

## 🎯 Repository Purpose

NuciLog.Core is a lightweight logging abstraction for .NET applications that standardises six log levels (`Verbose`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`), optional operation context, structured key-value details, exception enrichment, and a predictable textual record format — while delegating filtering, transport, persistence, and presentation to a consumer-owned sink implementation.

## 🏗️ Architectural Style

The library implements a **library/SDK architecture** centred on:
- **Public facade**: `ILogger` defines the consumer contract
- **Template Method base**: `Logger` normalises a large overload matrix into canonical per-level calls
- **Stateless formatter**: `LogMessageBuilder` transforms canonical inputs into a delimited structured string
- **Sink hook**: `protected abstract void WriteLog(LogLevel, Func<string>)` transfers control to a consumer-derived subclass
- **Null object**: `NullLogger` provides a no-op implementation

## 🔑 Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Overload normalisation in base class | Eliminates duplication across six levels × many parameter combinations |
| Lazy formatter delegate | Avoids formatting cost when sink filters by level or discards (e.g., `NullLogger`) |
| `LogInfoKey.IsSensitive` masking | Application-controlled sensitive data protection without library schema awareness |
| Fullwidth equals (`＝`) and Greek numeral (`͵`) delimiters | Unambiguous parsing; avoids collision with common log content |
| No persistent state in core | Library owns only transient transformation data; `SourceContext` is the sole instance state |
| `net10.0` target framework | Current .NET version at time of development; single-TFM simplicity |

## 📦 Packaging and Distribution

- **NuGet package**: `NuciLog.Core` (GPL-3.0-or-later)
- **Repository**: https://github.com/hmlendea/nucilog
- **Symbols**: Published as `.snupkg` via SourceLink
- **Build**: `dotnet pack` from `NuciLog.Core.csproj`

## 🔗 Related Root Documents

- [ARCHITECTURE.md](../ARCHITECTURE.md) — Verified current architecture (source of truth for architectural claims)
- [README.md](../README.md) — User-facing overview, usage examples, and project engagement
- [SECURITY.md](../SECURITY.md) — Vulnerability reporting and disclosure policy
- [PRIVACY.md](../PRIVACY.md) — Personal data handling description