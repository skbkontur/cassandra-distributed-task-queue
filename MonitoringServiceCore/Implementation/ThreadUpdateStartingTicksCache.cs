using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    class ThreadUpdateStartingTicksCache : IThreadUpdateStartingTicksCache
    {
        public ThreadUpdateStartingTicksCache()
        {
            dictionary = new Dictionary<string, long>();
        }

        public void Add(string guid, long startingTicks)
        {
            lock(dictionary)
            {
                dictionary.Add(guid, startingTicks);
            }
        }

        public void Remove(string guid)
        {
            lock(dictionary)
            {
                dictionary.Remove(guid);
            }
        }

        public long GetMinimum()
        {
            lock(dictionary)
            {
                return dictionary.Values.Min();
            }
        }

        private readonly Dictionary<string, long> dictionary = new Dictionary<string, long>();
    }
}