using System;
using System.Collections.Generic;

using NUnit.Framework;
using NuciLog.Core.UnitTests.Helpers;

namespace NuciLog.Core.UnitTests
{
    public sealed class LoggerTests2Debug
    {
        TestLogger logger;

        [SetUp]
        public void SetUp() => logger = new TestLogger();

        [Test]
        public void Debug_OperationIsPopulated_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;

            string expectedLogLine = $"Operation={operation.Name}";

            logger.Debug(operation);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;

            string expectedLogLine = $"Operation={operation.Name}";

            logger.Debug(operation, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            LogInfo[] logInfos =
            [
                new(TestLogInfoKey.TestKey, "teeestValue"),
                new(TestLogInfoKey.TestKey2, "teeestValue2")
            ];

            string expectedLogLine = $"Operation={operation.Name},{logInfos[0].Key.Name}={logInfos[0].Value},{logInfos[1].Key.Name}={logInfos[1].Value}";

            logger.Debug(operation, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Operation={operation.Name},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Operation={operation.Name},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine = $"Operation={operation.Name},{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndExceptionAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name}," +
                $"Message=An exception has occurred," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatus_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}";

            logger.Debug(operation, status);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, status, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}";

            logger.Debug(operation, status, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{logInfos.Key.Name}={logInfos.Value}";

            logger.Debug(operation, status, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, status, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, status, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation, status, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()}," +
                $"Message=An exception has occurred," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, status, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndExceptionAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndExceptionAndLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_Message_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Debug(operation: null, message: message);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndException_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine = $"Message={message},Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Debug(operation: null, message: message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine = $"Message={message},{logInfos.Key.Name}={logInfos.Value}";

            logger.Debug(operation: null, message: message, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndNullLogInfos_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Debug(operation: null, message: message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation: null, message: message, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";

            string expectedLogLine = $"Message={message}";

            logger.Debug(operation: null, message: message, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation: null, message: message, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation: null, message: message, logInfos: logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine = $"Message={message},{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation: null, message: message, logInfos: logInfos, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine = $"Message={message},Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndNullLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_MessageAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation: null, message: message, exception: ex, logInfos: logInfos, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessage_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Debug(operation, null, message);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Debug(operation, null, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}";

            logger.Debug(operation, null, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Debug(operation, null, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, null, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},Message={message}";

            logger.Debug(operation, null, message, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation, null, message, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, null, message, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation, null, message, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            string message = "testudo";
            Exception ex = new();
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, null, message, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndMessageAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, null, message, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessage_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Debug(operation, status, message);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndException_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, status, message, ex);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Debug(operation, status, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            LogInfo logInfos = new(TestLogInfoKey.TestKey, "teeest");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{logInfos.Key.Name}={logInfos.Value}";

            logger.Debug(operation, status, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Debug(operation, status, message, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, status, message, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";

            string expectedLogLine = $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}";

            logger.Debug(operation, status, message, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation, status, message, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest";

            logger.Debug(operation, status, message, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndLogInfosAndExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            IEnumerable<LogInfo> logInfos = [new(TestLogInfoKey.TestKey, "teeest")];
            LogInfo extraLogInfos = new(TestLogInfoKey.TestKey2, "teeest2");

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"{TestLogInfoKey.TestKey.Name}=teeest,{TestLogInfoKey.TestKey2.Name}=teeest2";

            logger.Debug(operation, status, message, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, status, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndNullLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, status, message, ex, logInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, message, ex, logInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndNullLogInfosAndNullExtraLogInfos_LogsCorrectly()
        {
            Operation operation = Operation.StartUp;
            OperationStatus status = OperationStatus.Started;
            string message = "testudo";
            Exception ex = new();

            string expectedLogLine =
                $"Operation={operation.Name},OperationStatus={status.Name.ToUpper()},Message={message}," +
                $"Exception={ex.GetType()},ExceptionMessage={ex.Message}";

            logger.Debug(operation, status, message, ex, logInfos: null, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndNullLogInfosAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, message, ex, logInfos: null, extraLogInfos: extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndLogInfosAndNullExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, message, ex, logInfos, extraLogInfos: null);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }

        [Test]
        public void Debug_OperationAndOperationStatusAndMessageAndExceptionAndLogInfosAndExtraLogInfos_LogsCorrectly()
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

            logger.Debug(operation, status, message, ex, logInfos, extraLogInfos);

            Assert.That(logger.LastLogLevel, Is.EqualTo(LogLevel.Debug));
            Assert.That(logger.LastLogLine, Is.EqualTo(expectedLogLine));
        }
    }
}
