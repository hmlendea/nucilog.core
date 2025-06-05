using System;
using System.Collections.Generic;

using NUnit.Framework;
using NuciLog.Core.UnitTests.Helpers;

namespace NuciLog.Core.UnitTests
{
    public sealed class LoggerTests1Verbose
    {
        TestLogger logger;

        [SetUp]
        public void SetUp()
        {
            logger = new TestLogger();
        }

        [Test]
        public void Verbose_OperationIsPopulated_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;

            string expectedLogLine = $"Operation={operation.Name}";

            logger.Verbose(operation);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;

            string expectedLogLine = $"Operation={operation.Name}";

            logger.Verbose(operation, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            LogInfo[] logInfos =
            [
                new(TestLogInfoKey.TestKey, "teeestValue"),
                new(TestLogInfoKey.TestKey2, "teeestValue2")
            ];

            string expectedLogLine = $"Operation={operation.Name},{logInfos[0].Key.Name}={logInfos[0].Value},{logInfos[1].Key.Name}={logInfos[1].Value}";

            logger.Verbose(operation, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Operation={operation.Name},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Operation={operation.Name},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine = $"Operation={operation.Name},{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndExceptionAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatus_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}";

            logger.Verbose(operation, status);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}";

            logger.Verbose(operation, status, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{logInfos.Key.Name}={logInfos.Value}";

            logger.Verbose(operation, status, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, status, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, status, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation, status, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndExceptionAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_Message_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Verbose(operation: null, message: message);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndException_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine = $"Message={message},Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Verbose(operation: null, message: message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine = $"Message={message},{logInfos.Key.Name}={logInfos.Value}";

            logger.Verbose(operation: null, message: message, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndNullLogInfos_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Verbose(operation: null, message: message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation: null, message: message, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Verbose(operation: null, message: message, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation: null, message: message, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation: null, message: message, logInfos: logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation: null, message: message, logInfos: logInfos, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine = $"Message={message},Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndNullLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_MessageAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation: null, message: message, exception: ex, logInfos: logInfos, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessage_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Verbose(operation, null, message);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Verbose(operation, null, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}";

            logger.Verbose(operation, null, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Verbose(operation, null, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, null, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Verbose(operation, null, message, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation, null, message, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, null, message, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation, null, message, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndMessageAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, null, message, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessage_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Verbose(operation, status, message);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Verbose(operation, status, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}";

            logger.Verbose(operation, status, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Verbose(operation, status, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, status, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Verbose(operation, status, message, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation, status, message, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Verbose(operation, status, message, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Verbose(operation, status, message, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Verbose_OperationAndOperationStatusAndMessageAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Verbose(operation, status, message, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Verbose));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }
    }
}
