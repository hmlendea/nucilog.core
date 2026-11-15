namespace NuciLog.Core
{
    public class Operation
    {
        public string Name { get; protected set; }

        public static Operation Unknown => new(nameof(Unknown));

        public static Operation StartUp => new(nameof(StartUp));

        public static Operation ShutDown => new(nameof(ShutDown));

        protected Operation(string name) => Name = name;
    }
}
