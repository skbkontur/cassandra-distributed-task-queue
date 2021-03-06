﻿using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    public enum EpsilonEnum
    {
        Alpha = 0,
        Beta = 1,
        Gamma = 2,
        Delta = 3,
        Epsilon = 4,
    }

    [RtqTaskName("EpsilonTaskData")]
    public class EpsilonTaskData : ITaskDataWithTopic
    {
        public EpsilonTaskData(EpsilonEnum epsilonEnum)
        {
            EpsilonEnum = epsilonEnum;
        }

        public EpsilonEnum EpsilonEnum { get; set; }
    }
}