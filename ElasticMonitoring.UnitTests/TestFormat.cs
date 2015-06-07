using System;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.UnitTests
{
    public class TestFormat : CoreTestBase
    {
        [Test]
        public void TestFormatSimple()
        {
            Assert.AreEqual(@"\a\b\s\-dd.mm\x\x", IndexNameConverter.ConvertToDateTimeFormat("abs-{dd.mm}xx"));
            Assert.AreEqual(@"\a\b\s\-dd.mm\x\x\-YYYY\-\s", IndexNameConverter.ConvertToDateTimeFormat("abs-{dd.mm}xx-{YYYY}-s"));
            Assert.AreEqual(@"dd.mm", IndexNameConverter.ConvertToDateTimeFormat("{dd.mm}"));
            Assert.AreEqual(@"dd.mm\z", IndexNameConverter.ConvertToDateTimeFormat("{dd.mm}z"));
            Assert.AreEqual(@"\x\z", IndexNameConverter.ConvertToDateTimeFormat("x{}z"));
            Assert.AreEqual(@"", IndexNameConverter.ConvertToDateTimeFormat("{}"));
        }

        [Test]
        public void TestFormatBad()
        {
            RunMethodWithException<NotSupportedException>(() => IndexNameConverter.ConvertToDateTimeFormat("z{"), "'{}' not balanced. Missing '}'");
            RunMethodWithException<NotSupportedException>(() => IndexNameConverter.ConvertToDateTimeFormat("z{x{}}"), "Inner '{}' not supported. position 3");
            RunMethodWithException<NotSupportedException>(() => IndexNameConverter.ConvertToDateTimeFormat("z}{"), "'{}' not balanced. Unexpected '{'  at position 1");
        }

        private static void RunMethodWithException<TE>(Action a, string msg) where TE : Exception
        {
            try
            {
                a();
                Assert.Fail("no exception");
            }
            catch(TE e)
            {
                Assert.AreEqual(msg, e.Message);
            }
        }
    }
}