using System;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    [TaskName("FakePeriodicTaskData")]
    public class FakePeriodicTaskData : ITaskData
    {
        public static TimeSpan RerunAfter { get { return TimeSpan.FromMilliseconds(100); } }
    }
}