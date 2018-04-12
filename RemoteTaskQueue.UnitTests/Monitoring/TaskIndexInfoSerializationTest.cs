using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using NUnit.Framework;

using RemoteTaskQueue.Monitoring.Indexer;
using RemoteTaskQueue.Monitoring.Storage.Writing;

namespace RemoteTaskQueue.UnitTests.Monitoring
{
    public class TaskIndexInfoSerializationTest
    {
        [Test]
        public void Simple()
        {
            var source = new TaskIndexedInfo(new MetaIndexedInfo {Name = "zzz"}, "exc", new Data
                {
                    A = 1,
                    S = "222"
                });
            Check(source, source);
        }

        [Test]
        public void TruncateStrings()
        {
            var source = new TaskIndexedInfo(new MetaIndexedInfo {Name = new string('q', 502)}, new string('e', 2049), new Data
                {
                    A = 1,
                    S = new string('z', 501)
                });
            var expected = new TaskIndexedInfo
                {
                    Meta = new MetaIndexedInfo {Name = new string('q', 500)},
                    ExceptionInfo = new string('e', 2048),
                    Data = new Dictionary<string, object>
                        {
                            {
                                new string('q', 502), new Data
                                    {
                                        A = 1,
                                        S = new string('z', 500)
                                    }
                            }
                        }
                };
            Check(source, expected);
        }

        [Test]
        public void IgnoreBinaryFields()
        {
            var source = new TaskIndexedInfo(new MetaIndexedInfo {Name = "zzz"}, string.Empty, new Data2
                {
                    B = 1,
                    BA = new byte[] {1, 2, 3, 4},
                    O = "qxx",
                    OO = new object[] {1, "zzz"}
                });
            Check(source, new TaskIndexedInfo(new MetaIndexedInfo {Name = "zzz"}, string.Empty, new Data2
                {
                    B = 1,
                    BA = null,
                    O = "qxx",
                    OO = new object[] {1, "zzz"}
                }));
        }

        private void Check(TaskIndexedInfo source, TaskIndexedInfo expected)
        {
            Assert.That(Serialize(source), Is.EqualTo(Serialize(expected)));
        }

        private string Serialize(object obj)
        {
            var sb = new StringBuilder();
            jsonSerializer.Serialize(new StringWriter(sb), obj);
            return sb.ToString();
        }

        private readonly JsonSerializer jsonSerializer = JsonSerializer.Create(new RtqElasticsearchIndexerSettings().JsonSerializerSettings);

        private class Data
        {
            public int A { get; set; }
            public string S { get; set; }
        }

        private class Data2
        {
            public byte B { get; set; }
            public byte[] BA { get; set; }
            public object O { get; set; }
            public object[] OO { get; set; }
        }
    }
}