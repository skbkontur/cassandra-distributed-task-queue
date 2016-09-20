using System;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    [TaskName("FakePeriodicTaskData")]
    public class FakePeriodicTaskData : ITaskData
    {
        public FakePeriodicTaskData(TimeSpan? rerunAfter = null)
        {
            RerunAfter = rerunAfter ?? DefaultRerunAfter;
        }

        public TimeSpan RerunAfter { get; private set; }

        public static TimeSpan DefaultRerunAfter { get { return TimeSpan.FromMilliseconds(100); } }
    }
}