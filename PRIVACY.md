# Privacy and Personal Data

NuciLog.Core is a .NET logging library that provides structured logging infrastructure. It does not collect, process, transmit, or store personal data. Applications that integrate NuciLog.Core control what data they log.

**Information reviewed:** 2026-10-07

## 📑 Table of Contents

- What This Document Covers
- Self-Hosted Deployments
- Data We Handle
- Processing and Use
- Storage, Retention, and Deletion
- External Processing and Integrations
- Document Changes
- Contact

## 🔎 What This Document Covers

This document describes how NuciLog.Core at https://github.com/hmlendea/nucilog handles personal data. It covers the library behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

NuciLog.Core is a .NET library distributed as a NuGet package. Application developers integrate it into their own applications and deploy those applications as they choose. The library maintainers do not operate any instances of applications using NuciLog.Core.

Instance operators (application developers) control their application's configuration, local storage, logs, backups, access controls, retention, and request handling. The library does not send any data from the application to the project maintainers or external services. There is no telemetry, update checks, crash reports, or monitoring integrations built into the library.

## 📥 Data We Handle

### Data Provided to the Application

NuciLog.Core does not request or require any personal data. Applications using the library may choose to log personal data through the provided logging APIs.

### Data Generated or Collected by the Application

NuciLog.Core does not generate or collect any personal data automatically. The library provides structured logging APIs (`LogInfo`, `LogInfoKey`) that applications use to create log entries. Any personal data in logs is placed there by the application code.

### Data Received from Integrations

NuciLog.Core has no built-in integrations and receives no data from third parties.

## 🧭 Processing and Use

The library provides logging infrastructure for these verified functions:
- Structured log message formatting — Application-provided log data (operation, status, message, exception, key-value details)
- Sensitive value masking — Values associated with `LogInfoKey` instances where `IsSensitive` is `true`

## 🗄️ Storage, Retention, and Deletion

NuciLog.Core does not store any data. Log output is written by the concrete `Logger` implementation provided by the application (e.g., to console, file, or external logging service). Storage, retention, and deletion are entirely controlled by the application developer and their chosen logging backend.

## 🔗 External Processing and Integrations

NuciLog.Core has no built-in external data transfers, integrations, or third-party processors.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | N/A | N/A | N/A |

## 🛡️ Data Protection and Security

NuciLog.Core provides a `LogInfoKey.IsSensitive` property that, when set to `true`, causes the library to mask the associated value in formatted log output (replacing it with a fixed-length masked string). This helps application developers avoid accidentally logging sensitive values such as tokens, passwords, or personal identifiers.

The library does not implement encryption, access controls, or network security. Application developers are responsible for securing their logging infrastructure, log storage, log transport, and access to logs.

## 🔄 Document Changes

Update this document when library data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/nucilog/blob/main/PRIVACY.md.

## 📬 Contact

For questions about library data handling, contact the project maintainers via GitHub issues at https://github.com/hmlendea/nucilog/issues. For a self-hosted application using NuciLog.Core, contact the application operator. Include the application name and deployment context; do not send passwords, access tokens, or other secrets.