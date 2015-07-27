using System;

using Kontur.Tracing;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using Kontur.Tracing.EdiVersion;
using Trace = Kontur.Tracing.EdiVersion.Trace;
using TraceContext = Kontur.Tracing.EdiVersion.TraceContext;

using RemoteQueue.Tracing;


namespace RemoteQueue.Handling
{
    internal class RemoteTask : IRemoteTask
    {
        public RemoteTask(IHandleTaskCollection handleTaskCollection, Task task, IGlobalTime globalTime)
        {
            this.handleTaskCollection = handleTaskCollection;
            this.task = task;
            this.globalTime = globalTime;
            if (!Trace.IsInitialized)
                Trace.Initialize(new TracingConfigurationProvider());
        }

        public string Queue()
        {
            return Queue(TimeSpan.FromTicks(0));
        }

        public string Queue(TimeSpan delay)
        {
            var traceContext = TraceContext.Current.IsActive ? Trace.CreateChildContext(task.Meta.Name) : Trace.CreateRootContext(task.Meta.Name);
            task.Meta.ContextId = traceContext.ContextId;
            task.Meta.TraceId = traceContext.TraceId;
            task.Meta.IsActive = traceContext.IsActive;
            traceContext.RecordTimepoint(Timepoint.ClientSend);
            using (var publishContext = Trace.CreateChildContext("Publish"))
            {
                publishContext.RecordTimepoint(Timepoint.ServerReceive);
                Publish(delay);
                publishContext.RecordTimepoint(Timepoint.ServerSend);
            }
            Trace.FinishCurrentContext();
            return Id;
        }

        private void Publish(TimeSpan delay)
        {
            var delayTicks = Math.Max(delay.Ticks, 0);
            //task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, globalTime.UpdateNowTicks() + delayTicks) + 1;
            task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, DateTime.UtcNow.Ticks + delayTicks) + 1;
            handleTaskCollection.AddTask(task);
        }

        public string Id { get { return task.Meta.Id; } }

        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly Task task;
        private readonly IGlobalTime globalTime;
    }
}