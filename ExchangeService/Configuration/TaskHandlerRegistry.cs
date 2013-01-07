using System;

using ExchangeService.UserClasses;
using ExchangeService.UserClasses.MonitoringTestTaskData;

using RemoteQueue.UserClasses;

namespace ExchangeService.Configuration
{
    public class TaskHandlerRegistry : TaskHandlerRegistryBase
    {
        public TaskHandlerRegistry(Func<FakeFailTaskHandler> createFakeFailTaskHandler,
                                   Func<FakePeriodicTaskHandler> createFakePeriodicTaskHandler,
                                   Func<SimpleTaskHandler> createSimpleTaskHandler,
                                   Func<ByteArrayTaskDataHandler> createByteArrayTaskDataHandler,
                                   Func<FileIdTaskDataHandler> createFileIdTaskDataHandler,
                                   Func<AlphaTaskHandler> createAlphaTaskHandler,
                                   Func<BetaTaskHandler> createBetaTaskHandler,
                                   Func<DeltaTaskHandler> createDeltaTaskHandler)
        {
            Register(createFakeFailTaskHandler);
            Register(createFakePeriodicTaskHandler);
            Register(createSimpleTaskHandler);
            Register(createByteArrayTaskDataHandler);
            Register(createFileIdTaskDataHandler);
            Register(createAlphaTaskHandler);
            Register(createBetaTaskHandler);
            Register(createDeltaTaskHandler);
        }
    }
}