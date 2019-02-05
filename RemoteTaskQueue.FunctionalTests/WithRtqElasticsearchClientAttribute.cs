using System;
using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteTaskQueue.Monitoring.Storage;

namespace RemoteTaskQueue.FunctionalTests
{
    public class WithRtqElasticsearchClientAttribute : GroboTestSuiteWrapperAttribute
    {
        public override void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IRtqElasticsearchClient>().UseInstances(new RtqElasticsearchClient(new Uri("http://localhost:9205")));
        }
    }
}