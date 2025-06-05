namespace NuciLog.Core
{
    public class OperationStatus
    {
        protected OperationStatus(string name) => Name = name;

        public string Name { get; protected set; }

        public static OperationStatus Unknown => new(nameof(Unknown));

        public static OperationStatus Started => new(nameof(Started));

        public static OperationStatus Success => new(nameof(Success));

        public static OperationStatus Failure => new(nameof(Failure));

        public static OperationStatus InProgress => new(nameof(InProgress));
    }
}
