﻿using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [WithDefaultSerializer, WithTestRtqSettings, WithNoProfiling]
    public class WithTestRemoteTaskQueue : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IRtqTaskDataRegistry>().UseType<TestTaskDataRegistry>();
        }
    }
}