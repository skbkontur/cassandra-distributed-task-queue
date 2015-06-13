using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Client
{
    public class RemoteTaskQueueTaskCounterClient : HttpClientBase, IRemoteTaskQueueTaskCounterClient
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