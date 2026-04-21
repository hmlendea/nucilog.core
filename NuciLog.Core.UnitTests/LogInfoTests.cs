using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using NUnit.Framework;
using NuciLog.Core.UnitTests.Helpers;

namespace NuciLog.Core.UnitTests
{
    public sealed class LogInfoTests
    {
        [Test]
        public void Given_StringConstructor_When_CreatingLogInfo_Then_KeyAndValueAreStored()
        {
            LogInfoKey key = TestLogInfoKey.TestKey;
            LogInfo sut = new(key, "abc");

            Assert.That(sut.Key, Is.EqualTo(key));
            Assert.That(sut.Value, Is.EqualTo("abc"));
        }

        [Test]
        public void Given_NullObject_When_CreatingLogInfo_Then_ValueIsEmptyString()
        {
            LogInfo sut = new(TestLogInfoKey.TestKey, (object)null);

            Assert.That(sut.Value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Given_StringObject_When_CreatingLogInfo_Then_ValueIsSameString()
        {
            LogInfo sut = new(TestLogInfoKey.TestKey, (object)"text-value");

            Assert.That(sut.Value, Is.EqualTo("text-value"));
        }

        [Test]
        public void Given_DateTimeObject_When_CreatingLogInfo_Then_ValueUsesRoundTripFormat()
        {
            DateTime value = new(2026, 4, 21, 10, 30, 45, DateTimeKind.Utc);
            LogInfo sut = new(TestLogInfoKey.TestKey, value);

            Assert.That(sut.Value, Is.EqualTo(value.ToString("o")));
        }

        [Test]
        public void Given_DateTimeOffsetObject_When_CreatingLogInfo_Then_ValueUsesRoundTripFormat()
        {
            DateTimeOffset value = new(2026, 4, 21, 10, 30, 45, TimeSpan.FromHours(2));
            LogInfo sut = new(TestLogInfoKey.TestKey, value);

            Assert.That(sut.Value, Is.EqualTo(value.ToString("o")));
        }

        [Test]
        public void Given_TimeSpanObject_When_CreatingLogInfo_Then_ValueUsesConstantFormat()
        {
            TimeSpan value = TimeSpan.FromMinutes(95);
            LogInfo sut = new(TestLogInfoKey.TestKey, value);

            Assert.That(sut.Value, Is.EqualTo(value.ToString("c")));
        }

        [Test]
        public void Given_EnumObject_When_CreatingLogInfo_Then_ValueUsesGeneralFormat()
        {
            LogInfo sut = new(TestLogInfoKey.TestKey, TestEnum.Second);

            Assert.That(sut.Value, Is.EqualTo("Second"));
        }

        [Test]
        public void Given_StringArray_When_CreatingLogInfo_Then_ValuesAreJoinedBySemicolon()
        {
            string[] values = ["a", "b", "c"];
            LogInfo sut = new(TestLogInfoKey.TestKey, values);

            Assert.That(sut.Value, Is.EqualTo("a;b;c"));
        }

        [Test]
        public void Given_NonGenericDictionary_When_CreatingLogInfo_Then_EntriesAreRenderedAsKeyValuePairs()
        {
            Hashtable dictionary = new()
            {
                { "key1", 11 },
                { "key2", null }
            };

            LogInfo sut = new(TestLogInfoKey.TestKey, dictionary);

            Assert.That(sut.Value, Is.EqualTo("key1=11;key2=;"));
        }

        [Test]
        public void Given_GenericDictionaryOfStringAndInt_When_CreatingLogInfo_Then_EntriesAreRenderedAsKeyValuePairs()
        {
            Dictionary<string, int> dictionary = new()
            {
                { "key1", 1 },
                { "key2", 2 }
            };

            LogInfo sut = new(TestLogInfoKey.TestKey, dictionary);

            Assert.That(sut.Value, Is.EqualTo("key1=1;key2=2;"));
        }

        [Test]
        public void Given_NonDictionaryEnumerable_When_CreatingLogInfo_Then_ItemsAreJoinedBySemicolon()
        {
            int[] values = [1, 2, 3];
            LogInfo sut = new(TestLogInfoKey.TestKey, values);

            Assert.That(sut.Value, Is.EqualTo("1;2;3"));
        }

        [Test]
        public void Given_UnsupportedObject_When_CreatingLogInfo_Then_ToStringResultIsUsed()
        {
            LogInfo sut = new(TestLogInfoKey.TestKey, new CustomValue("hello"));

            Assert.That(sut.Value, Is.EqualTo("custom-hello"));
        }

        [Test]
        public void Given_DateTimeConstructorWithFormat_When_CreatingLogInfo_Then_ValueUsesProvidedFormat()
        {
            DateTime value = new(2026, 4, 21, 10, 30, 45, DateTimeKind.Utc);
            LogInfo sut = new(TestLogInfoKey.TestKey, value, "yyyy-MM-dd");

            Assert.That(sut.Value, Is.EqualTo("2026-04-21"));
        }

        [Test]
        public void Given_DateTimeOffsetConstructorWithFormat_When_CreatingLogInfo_Then_ValueUsesProvidedFormat()
        {
            DateTimeOffset value = new(2026, 4, 21, 10, 30, 45, TimeSpan.Zero);
            LogInfo sut = new(TestLogInfoKey.TestKey, value, "yyyy-MM-dd");

            Assert.That(sut.Value, Is.EqualTo("2026-04-21"));
        }

        private sealed class CustomValue(string value)
        {
            public override string ToString() => $"custom-{value}";
        }

        private enum TestEnum
        {
            First,
            Second
        }
    }
}
