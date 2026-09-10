using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NuciLog.Core
{
    public static partial class LogMessageBuilder
    {
        private static readonly Regex NewLineMatchingRegex = new(@"\r\n|\r|\n", RegexOptions.Compiled);

        private static char ObfuscatedLogInfoValueMaskCharacter => '*';

        private static int ObfuscatedLogInfoValueMaskLength => 5;

        private static int ObfuscatedLogInfoValuePrefixLength => 4;

        private static int ObfuscatedLogInfoValueSuffixLength => 2;

        public static string Build(
            Operation operation,
            OperationStatus operationStatus,
            string message,
            Exception exception,
            params LogInfo[] logInfos)
            => Build (operation, operationStatus, message, exception, logInfos?.ToList());

        public static string Build(
            Operation operation,
            OperationStatus operationStatus,
            string message,
            Exception exception,
            IEnumerable<LogInfo> logInfos)
        {
            string logMessage = string.Empty;

            if (operation is not null)
            {
                logMessage += $"{LogInfoKey.Operation.Name}＝{operation.Name}͵";
            }

            if (operationStatus is not null)
            {
                logMessage += $"{LogInfoKey.OperationStatus.Name}＝{operationStatus.Name.ToUpper()}͵";
            }

            IEnumerable<LogInfo> processedDetails = GetProcessedLogInfoList(message, logInfos, exception);

            if (processedDetails is not null)
            {
                foreach (LogInfo detail in processedDetails)
                {
                    logMessage += $"{detail.Key.Name}＝{detail.Value}͵";
                }
            }

            if (logMessage.EndsWith('͵'))
            {
                return logMessage[..^1];
            }

            return logMessage;
        }

        static IEnumerable<LogInfo> GetProcessedLogInfoList(
            string message,
            IEnumerable<LogInfo> logInfos,
            Exception exception)
        {
            List<LogInfo> processedLogInfos = [];

            if (!string.IsNullOrWhiteSpace(message))
            {
                processedLogInfos.Add(new(
                    LogInfoKey.Message,
                    SanitiseLogInfoValue(message)));
            }
            else if (exception is not null)
            {
                processedLogInfos.Add(new(
                    LogInfoKey.Message,
                    "An exception has occurred."));
            }

            if (logInfos is not null)
            {
                foreach (LogInfo logInfo in logInfos)
                {
                    processedLogInfos.Add(new(
                        logInfo.Key,
                        ProcessLogInfoValue(logInfo)));
                }
            }

            if (exception is not null)
            {
                processedLogInfos.Add(new(
                    LogInfoKey.Exception,
                    exception.GetType()));
                processedLogInfos.Add(new(
                    LogInfoKey.ExceptionMessage,
                    SanitiseLogInfoValue(exception.Message)));
                processedLogInfos.Add(new(
                    LogInfoKey.StackTrace,
                    SanitiseStackTrace(exception.StackTrace)));
            }

            return processedLogInfos
                .GroupBy(x => x.Key)
                .Select(g => new LogInfo(g.First().Key, g.Last().Value))
                .Where(x => !string.IsNullOrWhiteSpace(x.Value));
        }

        private static string ProcessLogInfoValue(LogInfo logInfo)
        {
            string processedValue = SanitiseLogInfoValue(logInfo.Value);

            if (logInfo.Key.IsSensitive && !string.IsNullOrWhiteSpace(processedValue))
            {
                return ObfuscateLogInfoValue(processedValue);
            }

            return processedValue;
        }

        private static string ObfuscateLogInfoValue(string value)
        {
            int prefixLength = Math.Min(value.Length, ObfuscatedLogInfoValuePrefixLength);
            string prefix = value[..prefixLength];
            int remainingValueLength = value.Length - prefixLength;
            int suffixLength = 0;

            if (remainingValueLength > 0)
            {
                suffixLength = Math.Min(remainingValueLength, ObfuscatedLogInfoValueSuffixLength);
            }

            string suffix = string.Empty;

            if (suffixLength > 0)
            {
                suffix = value[^suffixLength..];
            }

            string mask = new(ObfuscatedLogInfoValueMaskCharacter, ObfuscatedLogInfoValueMaskLength);

            return $"{prefix}{mask}{suffix}";
        }

        private static string SanitiseLogInfoValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            string sanitisedValue = value;

            sanitisedValue = NewLineMatchingRegex.Replace(sanitisedValue, "\\n");

            return sanitisedValue;
        }

        private static string SanitiseStackTrace(string stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
            {
                return stackTrace;
            }

            return SanitiseLogInfoValue(stackTrace)
                .Replace("\\n", string.Empty)
                .Replace("\\t", " ")
                .Trim();
        }
    }
}
