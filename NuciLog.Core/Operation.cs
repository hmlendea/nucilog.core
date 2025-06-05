namespace NuciLog.Core
{
    public class Operation
    {
        protected Operation(string name) => Name = name;

        public string Name { get; protected set; }

        public static Operation Unknown => new(nameof(Unknown));

        public static Operation StartUp => new(nameof(StartUp));

        public static Operation ShutDown => new(nameof(ShutDown));
    }
}
