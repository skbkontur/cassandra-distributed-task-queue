using System.Collections.Generic;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class SnaphotConverter
    {
        public SnaphotConverter(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public bool TryConvert(CounterControllerSnapshotStorage.SnapshotData snapshotData, out CounterControllerSnapshot result)
        {
            if(snapshotData.Version == 1)
            {
                var decompress = SnapshotStorageUtils.Decompress(snapshotData.Data);
                var snapshotV1 = serializer.Deserialize<CounterControllerSnapshotV1>(decompress);

                result = HackConvert(snapshotV1);
                return true;
            }

            result = null;
            return false;
        }

        private static CounterControllerSnapshot HackConvert(CounterControllerSnapshotV1 snapshotV1)
        {
            if(snapshotV1 == null)
                return null;

            var result = new CounterControllerSnapshot()
                {
                    ControllerTicks = snapshotV1.CountollerTicks
                };
            var counterSnapshotV1 = snapshotV1.CounterSnapshot;
            if(counterSnapshotV1 != null)
            {
                result.CounterSnapshot = new CompositeCounterSnapshot()
                    {
                        TotalSnapshot = HackConvert(counterSnapshotV1.TotalSnapshot)
                    };
                if(counterSnapshotV1.Snapshots != null)
                    result.CounterSnapshot.Snapshots = counterSnapshotV1.Snapshots.ToDictionary(pair => pair.Key, pair => HackConvert(pair.Value));
            }
            return null;
        }

        private static ProcessedTasksCounter.CounterSnapshot HackConvert(CounterSnapshotV1 totalSnapshot)
        {
            if(totalSnapshot == null)
                return null;
            var map = new Dictionary<string, TaskState>();
            var counts = new int[TaskStateHelpers.statesCount];
            const TaskState fakeState = TaskState.InProcess;
            if(totalSnapshot.NotFinishedTasks != null)
            {
                foreach(var notFinishedTask in totalSnapshot.NotFinishedTasks)
                    map[notFinishedTask] = fakeState; //NOTE hack. we do not know exact state
            }
            counts[(int)fakeState] = map.Count;
            return new ProcessedTasksCounter.CounterSnapshot(map, totalSnapshot.CountCalculatedTime, totalSnapshot.Count, counts);
        }

        private readonly ISerializer serializer;

// ReSharper disable MemberCanBePrivate.Global
        public class CounterControllerSnapshotV1
        {
            public long CountollerTicks { get; set; }
            public CompositeCounterSnapshotV1 CounterSnapshot { get; set; }
        }

        public class CompositeCounterSnapshotV1
        {
            public Dictionary<string, CounterSnapshotV1> Snapshots { get; set; }
            public CounterSnapshotV1 TotalSnapshot { get; set; }
        }

        public class CounterSnapshotV1
        {
            public string[] NotFinishedTasks { get; private set; }

            public long CountCalculatedTime { get; private set; }
            public int Count { get; private set; }
        }

        // ReSharper restore MemberCanBePrivate.Global
    }
}