using System;

namespace NuciLog.Core
{
    public class LogInfoKey : IEquatable<LogInfoKey>
    {
        public string Name { get; protected set; }

        protected LogInfoKey(string name) => Name = name;

        public bool Equals(LogInfoKey other) => Name == other.Name;

        public override bool Equals(object other)
        {
            if (other is LogInfoKey)
            {
                return Equals(other as LogInfoKey);
            }

            return false;
        }

        public override int GetHashCode() => Name.GetHashCode();

        internal static LogInfoKey SourceContext => new(nameof(SourceContext));

        internal static LogInfoKey Operation => new(nameof(Operation));

        internal static LogInfoKey OperationStatus => new(nameof(OperationStatus));

        internal static LogInfoKey Message => new(nameof(Message));

        internal static LogInfoKey Exception => new(nameof(Exception));

        internal static LogInfoKey ExceptionMessage => new(nameof(ExceptionMessage));

        internal static LogInfoKey StackTrace => new(nameof(StackTrace));
    }
}
