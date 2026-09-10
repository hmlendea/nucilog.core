using System;

namespace NuciLog.Core
{
    public class LogInfoKey : IEquatable<LogInfoKey>
    {
        /// <summary>
        /// Gets whether the values logged for this key contain sensitive information.
        /// </summary>
        public bool IsSensitive { get; protected set; }

        /// <summary>
        /// Gets the structured log field name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="LogInfoKey"/> class.
        /// </summary>
        protected LogInfoKey(string name)
            : this(name, false) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="LogInfoKey"/> class.
        /// </summary>
        protected LogInfoKey(string name, bool isSensitive)
        {
            Name = name;
            IsSensitive = isSensitive;
        }

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
