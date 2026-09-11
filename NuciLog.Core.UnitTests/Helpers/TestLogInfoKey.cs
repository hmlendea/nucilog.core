namespace NuciLog.Core.UnitTests.Helpers
{
    public sealed class TestLogInfoKey : LogInfoKey
    {
        public static LogInfoKey TestKey => new TestLogInfoKey(nameof(TestKey));

        public static LogInfoKey TestKey2 => new TestLogInfoKey(nameof(TestKey2));

        public static LogInfoKey SensitiveTestKey => new TestLogInfoKey(nameof(SensitiveTestKey), true);

        public TestLogInfoKey(string name)
            : base(name) { }

        private TestLogInfoKey(string name, bool isSensitive)
            : base(name, isSensitive) { }
    }
}
