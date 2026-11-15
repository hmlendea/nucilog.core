using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
            : this(key, value.ToString(format, CultureInfo.InvariantCulture)) { }

        public LogInfo(LogInfoKey key, DateTimeOffset value, string format)
            : this(key, value.ToString(format, CultureInfo.InvariantCulture)) { }

        private string GetStringValue(object rawValue)
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
                return dateTimeValue.ToString("o", CultureInfo.InvariantCulture);
            }

            if (rawValue is DateTimeOffset dateTimeOffsetValue)
            {
                return dateTimeOffsetValue.ToString("o", CultureInfo.InvariantCulture);
            }

            if (rawValue is TimeSpan timeSpanValue)
            {
                return timeSpanValue.ToString("c", CultureInfo.InvariantCulture);
            }

            if (rawValue is Enum enumValue)
            {
                return enumValue.ToString("G");
            }

            if (rawValue is string[] stringArrayValue)
            {
                return string.Join(";", stringArrayValue);
            }

            if (rawValue is IDictionary dictionaryValue)
            {
                StringBuilder builder = new();

                foreach (DictionaryEntry entry in dictionaryValue)
                {
                    builder
                        .Append(GetStringRepresentation(entry.Key))
                        .Append('=')
                        .Append(GetStringRepresentation(entry.Value))
                        .Append(';');
                }

                return builder.ToString();
            }

            if (rawValue is IEnumerable enumerableValue)
            {
                if (rawValue
                    .GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x =>
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == typeof(IDictionary<,>)) is not null)
                {
                    StringBuilder builder = new();

                    foreach (var item in enumerableValue)
                    {
                        if (item is null)
                        {
                            continue;
                        }

                        Type itemType = item.GetType();
                        object key = itemType.GetProperty("Key")?.GetValue(item);
                        object value = itemType.GetProperty("Value")?.GetValue(item);

                        builder
                            .Append(GetStringRepresentation(key))
                            .Append('=')
                            .Append(GetStringRepresentation(value))
                            .Append(';');
                    }

                    return builder.ToString();
                }

                return string.Join(";", enumerableValue.Cast<object>());
            }

            return rawValue.ToString();
        }

        private string GetStringRepresentation(object rawValue)
        {
            string stringValue = Convert.ToString(rawValue);

            if (stringValue is null)
            {
                return string.Empty;
            }

            return stringValue;
        }
    }
}
