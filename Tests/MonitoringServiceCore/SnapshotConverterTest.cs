using System;
using System.Collections.Generic;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters;
using SKBKontur.Catalogue.TestCore;

namespace RemoteQueue.Tests.MonitoringServiceCore
{
    public class SnapshotConverterTest : CoreTestBase
    {
        [Test]
        public void TestCurrentVersion()
        {
            var internalSnapshot = new InternalSnapshot(new MetaProvider.MetaProviderSnapshot(128923, 238923, new Dictionary<string, long>() {{"z", 1}}, new Dictionary<string, long>() {{"q", 2}}
                                                            ), new ProcessedTasksCounter.CounterSnapshot(new HashSet<string>() {{"x"}, {"y"}}, 3478, 38));

            var bytes = snapshotConverter.ConvertToBytes(internalSnapshot);
            Console.WriteLine("grobuf: {0} length: {1}", serializer.GetSize(internalSnapshot), bytes.Length);
            //note content is gzipped
            Assert.That(bytes.Length < serializer.GetSize(internalSnapshot));

            var convertFromBytes = snapshotConverter.ConvertFromBytes(SnapshotConverter.CurrentVersion, bytes);
            convertFromBytes.AssertEqualsToWithXmlSerializer(internalSnapshot);
        }

        [Test]
        public void TestV1()
        {
            const int version = 1;
            var internalSnapshot = new InternalSnapshot(new MetaProvider.MetaProviderSnapshot(128923, 238923, new Dictionary<string, long>() {{"z", 22}}, new Dictionary<string, long>() {{"q", 2}}
                                                            ), new ProcessedTasksCounter.CounterSnapshot(new HashSet<string>() {{"x"}, {"y"}}, 3478, 38));
            var v1 = new InternalSnapshotV1() {Version = version, CounterSnapshot = internalSnapshot.CounterSnapshot, MetaProviderSnapshot = internalSnapshot.MetaProviderSnapshot};
            var bytes = serializer.Serialize(v1);

            var convertFromBytes = snapshotConverter.ConvertFromBytes(version, bytes);
            convertFromBytes.AssertEqualsToWithXmlSerializer(internalSnapshot);
        }

        [Test]
        public void TestV1Zero()
        {
            const int version = 1;
            var internalSnapshot = new InternalSnapshot(new MetaProvider.MetaProviderSnapshot(128923, 238923, new Dictionary<string, long>() {{"z", 22}}, new Dictionary<string, long>() {{"q", 2}}
                                                            ), new ProcessedTasksCounter.CounterSnapshot(new HashSet<string>() {{"x"}, {"y"}}, 3478, 38));
            var v1 = new InternalSnapshotV1() {Version = version, CounterSnapshot = internalSnapshot.CounterSnapshot, MetaProviderSnapshot = internalSnapshot.MetaProviderSnapshot};
            var bytes = serializer.Serialize(v1);

            var convertFromBytes = snapshotConverter.ConvertFromBytes(0, bytes);
            convertFromBytes.AssertEqualsToWithXmlSerializer(internalSnapshot);
        }

        public override void SetUp()
        {
            base.SetUp();
            snapshotConverter = new SnapshotConverter(serializer);
        }

        private SnapshotConverter snapshotConverter;

        private class InternalSnapshotV1
        {
            public int Version { get; set; }
            public MetaProvider.MetaProviderSnapshot MetaProviderSnapshot { get; set; }
            public ProcessedTasksCounter.CounterSnapshot CounterSnapshot { get; set; }
        }
    }
}