using System.IO;
using System.Text;

using Newtonsoft.Json;

using NUnit.Framework;

using RemoteTaskQueue.Monitoring.Storage.Writing;
using RemoteTaskQueue.Monitoring.Storage.Writing.Contracts;

using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.UnitTests
{
    public class TaskWtriterSerializationTest : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            jsonSerializer = JsonSerializer.Create(TaskWriterJsonSettings.GetSerializerSettings());
            jsonDeserializer = JsonSerializer.Create();
        }

        [Test]
        public void TestStupid()
        {
            var info = new TaskIndexedInfo<Data>(new MetaIndexedInfo() {Name = "zzz"}, "exc", new Data()
                {
                    A = 1, S = "222"
                });
            Check(info, info);
        }

        [Test]
        public void TestCutStringsExceptExceptionInfo()
        {
            var info = new TaskIndexedInfo<Data>(new MetaIndexedInfo() {Name = new string('q', 502)}, new string('e', 2049), new Data()
                {
                    A = 1, S = new string('z', 501)
                });
            Check(info, new TaskIndexedInfo<Data>(new MetaIndexedInfo() {Name = new string('q', 500)}, new string('e', 2048), new Data()
                {
                    A = 1,
                    S = new string('z', 500)
                }));
        }

        [Test]
        public void TestThrowBadTypes()
        {
            var info = new TaskIndexedInfo<Data2>(new MetaIndexedInfo() {Name = "zzz"}, null, new Data2()
                {
                    B = 1,
                    BA = new byte[] {1},
                    M = GetMock<IMock>(),
                    O = "qxx",
                    OO = new object[] {1, "zzz"}
                });
            Check(info, new TaskIndexedInfo<Data2>(new MetaIndexedInfo() {Name = "zzz"}, null, new Data2()
                {
                    B = 1,
                }));
        }

        private void Check<T>(T source, T expected)
        {
            var stringBuilder = new StringBuilder();
            jsonSerializer.Serialize(new StringWriter(stringBuilder), source);

            var deserialize = jsonDeserializer.Deserialize<T>(new JsonTextReader(new StringReader(stringBuilder.ToString())));
            deserialize.AssertEqualsTo(expected);
        }

// ReSharper disable MemberCanBePrivate.Global
        public interface IMock
// ReSharper restore MemberCanBePrivate.Global
        {
        }

        private JsonSerializer jsonSerializer;
        private JsonSerializer jsonDeserializer;

        private class Data
        {
            public int A { get; set; }
            public string S { get; set; }
        }

        private class Data2
        {
            public byte B { get; set; }
            public byte[] BA { get; set; }
            public object[] OO { get; set; }
            public object O { get; set; }
            public IMock M { get; set; }
        }
    }
}