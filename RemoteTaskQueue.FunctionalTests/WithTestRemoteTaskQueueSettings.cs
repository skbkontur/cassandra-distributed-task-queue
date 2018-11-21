﻿using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteQueue.Settings;

using RemoteTaskQueue.FunctionalTests.Common;

namespace RemoteTaskQueue.FunctionalTests
{
    public class WithTestRemoteTaskQueueSettings : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<TestRemoteTaskQueueSettings>();
        }
    }
}