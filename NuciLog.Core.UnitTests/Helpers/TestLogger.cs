using System;

namespace NuciLog.Core.UnitTests.Helpers
{
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
}
