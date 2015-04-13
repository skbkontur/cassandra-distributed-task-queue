using System;

using NUnit.Framework;

using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.UnitTests
{
    public class TestFormat : CoreTestBase
    {
        [Test]
        public void Test1()
        {
            Console.WriteLine(DateTime.Now.Ticks);
            DateTime dt = new DateTime(635645450983024781L);
            Console.WriteLine(dt.ToString(@"\m\o\n\i\t\o\r\i\n\g\-\i\n\d\e\x\-yyyy.MM.dd\-\o\l\d"));
        }
    }
}