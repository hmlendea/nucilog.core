using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuciLog.Core
{
    public sealed class LogInfo
    {
        public LogInfoKey Key { get; }

        public string Value { get; }

        public LogInfo(LogInfoKey key, string value)
        {
            Key = key;
            Value = value;
        }

        public LogInfo(LogInfoKey key, object value)
        {
            Key = key;
            Value = GetStringValue(value);
        }

        public LogInfo(LogInfoKey key, DateTime value, string format)
            : this(key, value.ToString(format)) { }

        public LogInfo(LogInfoKey key, DateTimeOffset value, string format)
            : this(key, value.ToString(format)) { }

        string GetStringValue(object rawValue)
        {
            if (rawValue is null)
            {
                return string.Empty;
            }

            if (rawValue is string stringValue)
            {
                return stringValue;
            }

            if (rawValue is DateTime dateTimeValue)
            {
                return dateTimeValue.ToString("o");
            }

            if (rawValue is DateTimeOffset dateTimeOffsetValue)
            {
                return dateTimeOffsetValue.ToString("o");
            }

            if (rawValue is TimeSpan timeSpanValue)
            {
                return timeSpanValue.ToString("c");
            }

            if (rawValue is Enum enumValue)
            {
                return enumValue.ToString("G");
            }

            if (rawValue is IFormattable formattableValue)
            {
                return formattableValue.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
            }

            if (rawValue is string[] stringArrayValue)
            {
                return string.Join(";", stringArrayValue);
            }

            if (rawValue is IDictionary dictionaryValue)
            {
                var builder = new StringBuilder();

                foreach (DictionaryEntry entry in dictionaryValue)
                {
                    builder
                        .Append(Convert.ToString(entry.Key) ?? string.Empty)
                        .Append('=')
                        .Append(Convert.ToString(entry.Value) ?? string.Empty)
                        .Append(';');
                }

                return builder.ToString();
            }

            Type dictionaryInterface = rawValue
                .GetType()
                .GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    (x.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                     x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));

            if (dictionaryInterface is not null && rawValue is IEnumerable genericDictionaryValue)
            {
                StringBuilder builder = new();

                foreach (var item in genericDictionaryValue)
                {
                    if (item is null)
                    {
                        continue;
                    }

                    Type itemType = item.GetType();
                    object key = itemType.GetProperty("Key")?.GetValue(item);
                    object value = itemType.GetProperty("Value")?.GetValue(item);

                    builder
                        .Append(Convert.ToString(key) ?? string.Empty)
                        .Append('=')
                        .Append(Convert.ToString(value) ?? string.Empty)
                        .Append(';');
                }

                return builder.ToString();
            }

            if (rawValue is IEnumerable enumerableValue)
            {
                return string.Join(";", enumerableValue.Cast<object>());
            }

            return rawValue.ToString();
        }
    }
}
