﻿using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    public class WithDefaultSerializer : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<ISerializer>().UseInstances(TestRtqSettings.MergeOnReadInstance);
        }
    }
}