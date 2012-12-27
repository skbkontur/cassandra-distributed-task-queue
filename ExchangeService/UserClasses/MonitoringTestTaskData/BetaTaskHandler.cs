using System;
using System.Threading;

using GroBuf;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class BetaTaskHandler : TaskHandler<BetaTaskData>
    {
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly ISerializer serializer;

        public BetaTaskHandler(IHandleTaskCollection handleTaskCollection, ISerializer serializer)
        {
            this.handleTaskCollection = handleTaskCollection;
            this.serializer = serializer;
        }

        protected override HandleResult HandleTask(BetaTaskData taskData)
        {
            while (taskData.IsProcess)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Hello World");
                taskData = serializer.Deserialize<BetaTaskData>(handleTaskCollection.GetTask(taskData.OwnTaskId).Data);
            }
            return new HandleResult
            {
                FinishAction = FinishAction.Finish
            };
        }
    }
}