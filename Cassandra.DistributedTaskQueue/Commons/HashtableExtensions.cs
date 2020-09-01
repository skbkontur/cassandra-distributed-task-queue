using System;
using System.Collections;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Commons
{
    internal static class HashtableExtensions
    {
        [NotNull]
        public static TValue GetOrAddThreadSafely<TKey, TValue>([NotNull] this Hashtable hashtable, [NotNull] TKey key, [NotNull] Func<TKey, TValue> valueFactory)
        {
            var value = (TValue)hashtable[key];
            if (value == null)
            {
                lock (hashtable.SyncRoot)
                {
                    value = (TValue)hashtable[key];
                    if (value == null)
                    {
                        value = valueFactory(key);
                        hashtable.Add(key, value);
                    }
                }
            }
            return value;
        }
    }
}