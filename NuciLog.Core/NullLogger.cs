using System;

namespace NuciLog.Core
{
    public sealed class NullLogger : Logger
    {
        protected override void WriteLog(
            LogLevel level,
            Func<string> logMessage)
        { }
    }
}
