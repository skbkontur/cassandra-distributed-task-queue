using System;

using FluentAssertions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Utils;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.UnitTests.Monitoring
{
    public class UtcTicksDateConverterTest
    {
        [Test]
        public void TestSimple()
        {
            var d = new DateTime(2010, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
            var d2 = new DateTime(2013, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
            var c1 = new C1 {UtcTicks = d.Ticks, UtcTicksNullable = d2.Ticks};
            var s = JsonConvert.SerializeObject(c1);
            JsonConvert.DeserializeObject<C1>(s).Should().BeEquivalentTo(c1);
            var c2 = new C2()
                {
                    UtcTicks = d,
                    UtcTicksNullable = d2
                };
            JsonConvert.DeserializeObject<C2>(s).Should().BeEquivalentTo(c2);
            JsonConvert.DeserializeObject<C1>(JsonConvert.SerializeObject(c2)).Should().BeEquivalentTo(c1);
        }

        [Test]
        public void TestFormat()
        {
            var d = new DateTime(635633850005256502, DateTimeKind.Utc);
            var d2 = new DateTime(2013, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
            var c1 = new C1 {UtcTicks = d.Ticks, UtcTicksNullable = d2.Ticks};
            var s = JsonConvert.SerializeObject(c1);
            JsonConvert.DeserializeObject<C1>(s).Should().BeEquivalentTo(c1);
            Assert.AreEqual("{\"UtcTicks\":\"2015-03-31T07:50:00.5256502Z\",\"UtcTicksNullable\":\"2013-01-02T03:04:05.006Z\"}", s);
        }

        [Test]
        public void TestNull()
        {
            var c1 = new C1 {UtcTicks = 0, UtcTicksNullable = null};
            var s = JsonConvert.SerializeObject(c1);
            JsonConvert.DeserializeObject<C1>(s).Should().BeEquivalentTo(c1);
            var c2 = new C2()
                {
                    UtcTicks = new DateTime(0, DateTimeKind.Utc),
                    UtcTicksNullable = null
                };
            JsonConvert.DeserializeObject<C2>(s).Should().BeEquivalentTo(c2);
            JsonConvert.DeserializeObject<C1>(JsonConvert.SerializeObject(c2)).Should().BeEquivalentTo(c1);
        }

        [Test]
        public void TestNullDt()
        {
            var c1 = new C1 {UtcTicks = 0, UtcTicksNullable = null};
            var s = JsonConvert.SerializeObject(c1);
            JsonConvert.DeserializeObject<C1>(s).Should().BeEquivalentTo(c1);
            var c3 = new C3()
                {
                    UtcTicks = 0,
                    UtcTicksNullable = 0
                };
            JsonConvert.DeserializeObject<C3>(s).Should().BeEquivalentTo(c3);
            JsonConvert.DeserializeObject<C1>(JsonConvert.SerializeObject(c3)).Should().BeEquivalentTo(new C1() {UtcTicks = 0, UtcTicksNullable = 0});
        }

        [Test]
        public void TestZero()
        {
            var c1 = new C1 {UtcTicksNullable = 0};
            var s = JsonConvert.SerializeObject(c1);
            JsonConvert.DeserializeObject<C1>(s).Should().BeEquivalentTo(c1);
            var c2 = new C2()
                {
                    UtcTicks = new DateTime(0, DateTimeKind.Utc),
                    UtcTicksNullable = new DateTime(0, DateTimeKind.Utc),
                };
            JsonConvert.DeserializeObject<C2>(s).Should().BeEquivalentTo(c2);
            JsonConvert.DeserializeObject<C1>(JsonConvert.SerializeObject(c2)).Should().BeEquivalentTo(c1);
        }

        [Test]
        public void TestBadValues()
        {
            var c1 = new C1 {UtcTicks = -1, UtcTicksNullable = DateTime.MaxValue.Ticks + 1};
            var s = JsonConvert.SerializeObject(c1);
            JsonConvert.DeserializeObject<C1>(s).Should().BeEquivalentTo(new C1 {UtcTicks = DateTime.MinValue.Ticks, UtcTicksNullable = DateTime.MaxValue.Ticks});
            JsonConvert.DeserializeObject<C2>(s).Should().BeEquivalentTo(new C2()
                {
                    UtcTicks = DateTime.MinValue,
                    UtcTicksNullable = DateTime.MaxValue
                });
        }

        private class C1
        {
            [JsonConverter(typeof(UtcTicksJsonConverter))]
            public long UtcTicks { get; set; }

            [JsonConverter(typeof(UtcTicksJsonConverter))]
            public long? UtcTicksNullable { get; set; }
        }

        private class C2
        {
            [JsonConverter(typeof(IsoDateTimeConverter))]
            public DateTime UtcTicks { get; set; }

            [JsonConverter(typeof(IsoDateTimeConverter))]
            public DateTime? UtcTicksNullable { get; set; }
        }

        private class C3
        {
            [JsonConverter(typeof(UtcTicksJsonConverter))]
            public long? UtcTicks { get; set; }

            [JsonConverter(typeof(UtcTicksJsonConverter))]
            public long UtcTicksNullable { get; set; }
        }
    }
}