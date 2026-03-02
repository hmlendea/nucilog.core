using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NuciLog.Core
{
    public static partial class LogMessageBuilder
    {
        private static readonly Regex NewLineMatchingRegex = new(@"\r\n|\r|\n", RegexOptions.Compiled);

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
                logMessage += $"{LogInfoKey.Operation.Name}={operation.Name},";
            }

            if (operationStatus is not null)
            {
                logMessage += $"{LogInfoKey.OperationStatus.Name}={operationStatus.Name.ToUpper()},";
            }

            IEnumerable<LogInfo> processedDetails = GetProcessedLogInfoList(message, logInfos, exception);

            if (processedDetails is not null)
            {
                foreach (LogInfo detail in processedDetails)
                {
                    logMessage += $"{detail.Key.Name}={detail.Value},";
                }
            }

            if (logMessage.EndsWith(','))
            {
                return logMessage[..^1];
            }

            return logMessage;
        }

        static IEnumerable<LogInfo> GetProcessedLogInfoList(string message, IEnumerable<LogInfo> logInfos, Exception ex)
        {
            List<LogInfo> processedLogInfos = [];

            if (!string.IsNullOrWhiteSpace(message))
            {
                processedLogInfos.Add(new(
                    LogInfoKey.Message,
                    SanitiseLogInfoValue(message)));
            }
            else if (ex is not null)
            {
                processedLogInfos.Add(new(
                    LogInfoKey.Message,
                    "An exception has occurred"));
            }

            if (logInfos is not null)
            {
                foreach (LogInfo logInfo in logInfos)
                {
                    if (logInfo.Value is string stringValue)
                    {
                        processedLogInfos.Add(new(
                            logInfo.Key,
                            SanitiseLogInfoValue(stringValue)));
                    }
                    else
                    {
                        processedLogInfos.Add(logInfo);
                    }
                }
            }

            if (ex is not null)
            {
                processedLogInfos.Add(new(
                    LogInfoKey.Exception,
                    ex.GetType()));
                processedLogInfos.Add(new(
                    LogInfoKey.ExceptionMessage,
                    SanitiseLogInfoValue(ex.Message)));
                processedLogInfos.Add(new(
                    LogInfoKey.StackTrace,
                    SanitiseLogInfoValue(ex.StackTrace)));
            }

            return processedLogInfos
                .GroupBy(x => x.Key)
                .Select(g => new LogInfo(g.First().Key, g.Last().Value))
                .Where(x => !string.IsNullOrWhiteSpace(x.Value));
        }

        private static string SanitiseLogInfoValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            string sanitisedValue = value;

            sanitisedValue = NewLineMatchingRegex.Replace(sanitisedValue, "\\n");
            sanitisedValue = sanitisedValue.Replace(",", "͵");

            return sanitisedValue;
        }
    }
}
