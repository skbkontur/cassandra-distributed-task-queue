﻿using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBase;
using SKBKontur.Catalogue.ClientLib.HttpClientBase.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public class ExchangeServiceClient : HttpClientBase, IExchangeServiceClient
    {
        public ExchangeServiceClient(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration configuration)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
        }

        public void Start()
        {
            CreateMethodDomain("Start").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void Stop()
        {
            CreateMethodDomain("Stop").SendToEachReplica(DomainConsistencyLevel.All);
        }

        protected override IHttpServiceClientConfiguration GetConfiguration()
        {
            return base.GetConfiguration().WithTimeout(TimeSpan.FromSeconds(30));
        }

        protected override string GetDefaultTopologyFileName()
        {
            return "exchangeServiceTopology";
        }
    }
}