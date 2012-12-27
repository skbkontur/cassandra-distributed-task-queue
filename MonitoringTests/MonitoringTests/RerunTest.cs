using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.MonitoringTests
{
    public class RerunTest : MonitoringFunctionalTestBase
    {
        [Test]
        public void Test()
        {
            CreateUser("user","psw");
        }
    }
}