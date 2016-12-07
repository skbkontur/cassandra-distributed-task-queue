using System;
using System.Linq;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.UnitTests
{
    public class SearchIndexNameFactoryTest : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            indexNameFormat = IndexNameConverter.ConvertToDateTimeFormat("zzz-{yyyy.MM.dd}");
        }

        [Test]
        public void TestStupid()
        {
            Check("zzz-2010.01.01,zzz-2010.01.02", Date(2010, 01, 01), Date(2010, 01, 02));
            Check("zzz-2010.03.03", Date(2010, 03, 03), Date(2010, 03, 03));
        }

        [Test]
        public void TestYearWildcard()
        {
            Check("zzz-2010.*.*", Date(2010, 01, 01), Date(2010, 12, 31));
            Check("zzz-2010.*.*,zzz-2011.01.01", Date(2010, 01, 01), Date(2011, 01, 01));
            Check("zzz-2009.12.31,zzz-2010.*.*,zzz-2011.01.01", Date(2009, 12, 31), Date(2011, 01, 01));
            Check("zzz-2009.12.31,zzz-2010.*.*,zzz-2011.*.*,zzz-2012.01.01", Date(2009, 12, 31), Date(2012, 01, 01));
        }

        [Test]
        public void TestMonthWildcard()
        {
            Check("zzz-2010.01.*,zzz-2010.02.01", Date(2010, 01, 01), Date(2010, 02, 01));
            Check("zzz-2010.02.*,zzz-2010.03.01", Date(2010, 02, 01), Date(2010, 03, 01));
        }

        [Test]
        public void TestHard()
        {
            Check("zzz-2010.11.29,zzz-2010.11.30,zzz-2010.12.*,zzz-2011.*.*,zzz-2012.01.*,zzz-2012.02.01,zzz-2012.02.02", Date(2010, 11, 29), Date(2012, 02, 02));
            Check("zzz-2010.02.*", Date(2010, 02, 01), Date(2010, 02, 28));
            Check(string.Join(",", Enumerable.Range(1, 27).Select(x => string.Format("zzz-2010.02.{0:D2}", x))), Date(2010, 02, 01), Date(2010, 02, 27));
            Check(string.Join(",", Enumerable.Range(25, 31 - 25 + 1).Select(x => string.Format("zzz-2010.01.{0:D2}", x))) + ",zzz-2010.02.01,zzz-2010.02.02", Date(2010, 01, 25), Date(2010, 02, 2));
        }

        private static long Date(int year, int month, int day)
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc).Ticks;
        }

        private void Check(string exp, long start, long end)
        {
            Assert.AreEqual(exp, SearchIndexNameFactory.GetIndexForTimeRange(start, end, indexNameFormat));
            Assert.AreEqual(exp, SearchIndexNameFactory.GetIndexForTimeRange(start + TimeSpan.FromHours(1).Ticks, end, indexNameFormat));
            Assert.AreEqual(exp, SearchIndexNameFactory.GetIndexForTimeRange(start, end + TimeSpan.FromHours(1).Ticks, indexNameFormat));
        }

        private string indexNameFormat;
    }
}