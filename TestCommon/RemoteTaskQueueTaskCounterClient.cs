using System;

using JetBrains.Annotations;

using RemoteTaskQueue.TaskCounter;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace TestCommon
{
    public class RemoteTaskQueueTaskCounterClient : HttpClientBase
    {
        public RemoteTaskQueueTaskCounterClient(
            [NotNull] IDomainTopologyFactory domainTopologyFactory, 
            [NotNull] IMethodDomainFactory methodDomainFactory, 
            [NotNull] IHttpServiceClientConfiguration configuration)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
        }

        public TaskCount GetProcessingTaskCount()
        {
            return Method("GetProcessingTaskCount").InvokeOnRandomReplica().ThanReturn<TaskCount>();
        }

        public void RestartProcessingTaskCounter(DateTime? fromTime)
        {
            Method("RestartProcessingTaskCounter").SendToEachReplica(DomainConsistencyLevel.All, fromTime);
        }

        [NotNull]
        protected override IHttpServiceClientConfiguration DoGetConfiguration([NotNull] IHttpServiceClientConfiguration defaultConfiguration)
        {
            return defaultConfiguration.WithTimeout(TimeSpan.FromSeconds(15));
        }

        [NotNull]
        protected override string GetDefaultTopologyFileName()
        {
            return "remoteTaskQueueTaskCounterService";
        }
    }
}