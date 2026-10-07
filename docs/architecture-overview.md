# Architecture Overview

This document provides a high-level architectural decomposition of NuciLog.Core, derived from implementation evidence. It complements the root `ARCHITECTURE.md` by organising the same verified facts into a navigable reference for maintainers and contributors.

## 🎯 System Purpose

NuciLog.Core is a **.NET class library** (`net10.0`) that supplies a logging abstraction. It standardises:
- Six severity levels (`Verbose` through `Fatal`)
- Optional operation context (`Operation`, `OperationStatus`)
- Structured key-value details (`LogInfo`, `LogInfoKey`)
- Exception enrichment (type, message, stack trace)
- Predictable textual record format with sensitive-value masking

**It does not** provide filtering, transport, persistence, presentation, or any concrete log destination. Those responsibilities belong to a consumer-derived `Logger` subclass.

## 🌐 System Context

```
┌─────────────────────────────────────────────────────────────────┐
│                    NuciLog.Core System Boundary                 │
│  ┌─────────────┐  ┌──────────────────┐  ┌───────────────────┐  │
│  │   ILogger   │  │     Logger       │  │ LogMessageBuilder │  │
│  │  (contract) │◄─┤ (normalisation)  │──►│  (formatting)     │  │
│  └──────┬──────┘  └────────┬─────────┘  └─────────┬─────────┘  │
│         │                  │                       │            │
│         │                  ▼                       │            │
│         │         ┌─────────────────┐              │            │
│         │         │  WriteLog hook  │              │            │
│         │         │ (abstract)      │              │            │
│         │         └────────┬────────┘              │            │
│         │                  │                       │            │
│         └──────────────────┼───────────────────────┘            │
│                            ▼                                    │
│                   ┌─────────────────┐                           │
│                   │   NullLogger    │                           │
│                   │  (no-op impl)   │                           │
│                   └─────────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
         ▲                              ▲
         │                              │
    ┌────┴────┐                  ┌───────┴───────┐
    │Consumer │                  │Consumer Sink  │
    │Application│                 │(Logger subclass)│
    └─────────┘                  └───────────────┘
```

**External boundaries:**
| Boundary | Owner | Responsibility |
|----------|-------|----------------|
| Consuming application | Application developer | Composition, logger lifetime, source data, API invocation |
| Concrete sink | Application developer | `WriteLog` implementation, destination policy, filtering, transport |
| .NET runtime | Microsoft | Loads assembly, supplies BCL (collections, LINQ, text, regex) |
| Persistent stores | Sink implementation | Any retention, consistency, expiry |

## 🏗️ Architectural Style

**Library/SDK with Template Method sink hook**

| Layer | Components | Key Characteristic |
|-------|------------|-------------------|
| Public contract | `ILogger`, `Logger`, `LogLevel`, `Operation`, `OperationStatus`, `LogInfo`, `LogInfoKey` | Source- and binary-compatible surface |
| Normalisation | `Logger` (base class) | Owns 100+ overloads → canonical per-level call; no I/O |
| Formatting | `LogMessageBuilder` (static) | Stateless transformation; invoked only when sink calls delegate |
| Sink boundary | `WriteLog(LogLevel, Func<string>)` | Protected abstract; transfers control without core→sink dependency |
| Null object | `NullLogger` | Ignores formatter; zero allocation when logging disabled |

## 🧩 Component Responsibilities

| Component | Responsibility | Key Dependencies | Lifetime |
|-----------|----------------|------------------|----------|
| `ILogger` | Consumer contract: source context + 6 level families | `Type`, `Operation`, `OperationStatus`, `Exception`, `LogInfo` | External |
| `Logger` | Overload normalisation, detail combination, level selection, source context retention, lazy delegate dispatch | `ILogger`, `LogMessageBuilder`, LINQ, public data types | Caller-owned instance; mutable `SourceContext` |
| `LogMessageBuilder` | Field ordering, enrichment, sanitisation, de-duplication, omission, serialisation | `LogInfo`, `LogInfoKey`, `Operation`, `OperationStatus`, LINQ, `Regex` | Process-static; compiled regex |
| `LogInfo` | Named structured value + object→string conversion | `LogInfoKey`, collections, reflection, `StringBuilder` | Transient carrier |
| `LogInfoKey` | Field identity, reserved internal names, sensitivity classification | `IEquatable<LogInfoKey>` | Consumer-extendable; static built-ins create fresh instances |
| `Operation` / `OperationStatus` | Extensible operation context with built-ins | None beyond BCL | Consumer-extendable; static built-ins create fresh instances |
| `LogLevel` | Severity identifier (separate from payload) | None | Value copied per call |
| `NullLogger` | No-op sink | `Logger` | Caller-owned |

## 🔄 Runtime Flow Summary

```
Consumer calls ILogger overload
        │
        ▼
Logger normalises & combines arguments
        │
        ▼
Logger.WriteLog(level, lazyFormatter)  ──► Sink override
        │                                      │
        │         ┌────────────────────────────┘
        │         ▼
        │  Sink invokes formatter (or not)
        │         │
        │         ▼
        │  LogMessageBuilder.Build(...)
        │         │
        │         ▼
        │  Formatted record returned
        │         │
        ▼         ▼
   Return    Sink applies destination policy
```

**Critical invariant**: Formatting occurs **only** when the sink invokes the delegate. `NullLogger` and level-filtering sinks incur zero formatting cost.

## 📦 Dependency Direction

```
Consumer Application
        │
        ▼
┌───────────────────┐
│   NuciLog.Core    │  ◄─── No dependencies on consumer code
│  (library only)   │
└───────────────────┘
        │
        ▼
.NET 10 Runtime (BCL only)
```

**No external NuGet dependencies** (except `Microsoft.SourceLink.GitHub` for build-time symbol publishing).

## 🔌 Extension Points

| Extension Point | Contract | Consumer Responsibility |
|-----------------|----------|------------------------|
| Concrete log sink | `protected override void WriteLog(LogLevel, Func<string>)` | Implement destination policy; decide when/if to invoke formatter |
| Structured field keys | Extend `LogInfoKey` (protected constructors) | Define domain-specific keys; set `IsSensitive` for masking |
| Domain operations | Extend `Operation` (protected constructor) | Define business operation names |
| Domain statuses | Extend `OperationStatus` (protected constructor) | Define business status names |

## 📋 Cross-Cutting Concerns

| Concern | Implementation |
|---------|----------------|
| **Security/Privacy** | `LogInfoKey.IsSensitive` → masks value (first 4 chars + 5★ + last 2 chars); no encryption, access control, or network security in core |
| **Error handling** | No exception translation or retry; failures propagate from boundary that executes them (formatter, sink, or consumer) |
| **Observability** | None built-in; sink owns all emission |
| **Concurrency** | `Logger` instance state (`SourceContext`) is not thread-safe; sink owns its own concurrency model |
| **Resource use** | Zero allocation when formatter not invoked; `LogInfo` converts at construction; delegate captures references |

## 📍 Source Map

| File | Primary Responsibility |
|------|----------------------|
| `ILogger.cs` | Public consumer contract |
| `Logger.cs` | Overload normalisation, canonical dispatch, `SourceContext` |
| `LogMessageBuilder.cs` | Record construction, sanitisation, masking, de-duplication |
| `LogInfo.cs` | Value carrier + object→string conversion |
| `LogInfoKey.cs` | Field identity + sensitivity |
| `Operation.cs` / `OperationStatus.cs` | Extensible domain context |
| `LogLevel.cs` | Severity enumeration |
| `NullLogger.cs` | No-op implementation |

## 🔗 Related Documents

- [Public API Reference](public-api-reference.md) — Complete member-level reference
- [Record Construction](record-construction.md) — Grammar, formatting pipeline, sanitisation rules
- [Extension Points](extension-points.md) — Sink, key, operation, status contracts
- [Runtime Execution Flows](runtime-execution-flows.md) — Detailed call sequences
- [Data Model and Transformations](data-model-and-transformations.md) — Value conversion and builder processing
- [Testing and Verification](testing-and-verification.md) — Test organisation and coverage
- [Security and Privacy](security-and-privacy.md) — Masking, data handling, boundaries
- [Compatibility and Versioning](compatibility-and-versioning.md) — TFM, packaging, compatibility
- [Source Map](source-map.md) — File-to-responsibility index