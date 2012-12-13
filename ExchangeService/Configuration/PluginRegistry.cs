using System;

using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace ExchangeService.Configuration
{
    public class PluginRegistry
    {
        public static Type remoteTaskQueuePlugin = typeof(RemoteTaskQueuePlugin);
    }
}