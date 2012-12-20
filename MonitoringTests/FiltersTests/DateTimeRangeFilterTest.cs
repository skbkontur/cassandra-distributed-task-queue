using NUnit.Framework;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class DateTimeRangeFilterTest : FiltersTestBase
    {
        [Test]
        public void Test()
         {
             CreateUser("user", "psw");
             var tasksListPage = Login("user", "psw");
         }
    }
}