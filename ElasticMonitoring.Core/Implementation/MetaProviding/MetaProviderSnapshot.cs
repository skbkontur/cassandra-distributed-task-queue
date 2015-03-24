using System.Collections.Generic;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class MetaProviderSnapshot
    {
        public MetaProviderSnapshot(long lastUpdateTicks, long startTicks, Dictionary<string, long> notReadEvents, Dictionary<string, long> readEvents)
        {
            if(notReadEvents != null) NotReadEvents = new Dictionary<string, long>(notReadEvents);
            if(readEvents != null) ReadEvents = new Dictionary<string, long>(readEvents);
            LastUpdateTicks = lastUpdateTicks;
            StartTicks = startTicks;
        }

        public long LastUpdateTicks { get; private set; }
        public long StartTicks { get; set; }

        public Dictionary<string, long> NotReadEvents { get; private set; }
        public Dictionary<string, long> ReadEvents { get; private set; }
    }
}