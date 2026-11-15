namespace NuciLog.Core.UnitTests.Helpers
{
    internal sealed class CustomValue(string value)
    {
        public override string ToString() => $"custom-{value}";
    }
}
