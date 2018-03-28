using System;
using System.Linq;

using NUnit.Framework;

using RemoteTaskQueue.Monitoring.Storage.Search;

namespace RemoteTaskQueue.UnitTests.Monitoring
{
    [TestFixture]
    public class SearchIndexNameFactoryTest
    {
        [Test]
        public void TestStupid()
        {
            Check("rtq-2010.01.01,rtq-2010.01.02", Date(2010, 01, 01), Date(2010, 01, 02));
            Check("rtq-2010.03.03", Date(2010, 03, 03), Date(2010, 03, 03));
        }

        [Test]
        public void TestYearWildcard()
        {
            Check("rtq-2010.*.*", Date(2010, 01, 01), Date(2010, 12, 31));
            Check("rtq-2010.*.*,rtq-2011.01.01", Date(2010, 01, 01), Date(2011, 01, 01));
            Check("rtq-2009.12.31,rtq-2010.*.*,rtq-2011.01.01", Date(2009, 12, 31), Date(2011, 01, 01));
            Check("rtq-2009.12.31,rtq-2010.*.*,rtq-2011.*.*,rtq-2012.01.01", Date(2009, 12, 31), Date(2012, 01, 01));
        }

        [Test]
        public void TestMonthWildcard()
        {
            Check("rtq-2010.01.*,rtq-2010.02.01", Date(2010, 01, 01), Date(2010, 02, 01));
            Check("rtq-2010.02.*,rtq-2010.03.01", Date(2010, 02, 01), Date(2010, 03, 01));
        }

        [Test]
        public void TestHard()
        {
            Check("rtq-2010.11.29,rtq-2010.11.30,rtq-2010.12.*,rtq-2011.*.*,rtq-2012.01.*,rtq-2012.02.01,rtq-2012.02.02", Date(2010, 11, 29), Date(2012, 02, 02));
            Check("rtq-2010.02.*", Date(2010, 02, 01), Date(2010, 02, 28));
            Check(string.Join(",", Enumerable.Range(1, 27).Select(x => $"rtq-2010.02.{x:D2}")), Date(2010, 02, 01), Date(2010, 02, 27));
            Check(string.Join(",", Enumerable.Range(25, 31 - 25 + 1).Select(x => $"rtq-2010.01.{x:D2}")) + ",rtq-2010.02.01,rtq-2010.02.02", Date(2010, 01, 25), Date(2010, 02, 2));
        }

        private static long Date(int year, int month, int day)
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc).Ticks;
        }

        private static void Check(string exp, long start, long end)
        {
            Assert.AreEqual(exp, SearchIndexNameFactory.GetIndexForTimeRange(start, end));
            Assert.AreEqual(exp, SearchIndexNameFactory.GetIndexForTimeRange(start + TimeSpan.FromHours(1).Ticks, end));
            Assert.AreEqual(exp, SearchIndexNameFactory.GetIndexForTimeRange(start, end + TimeSpan.FromHours(1).Ticks));
        }
    }
}