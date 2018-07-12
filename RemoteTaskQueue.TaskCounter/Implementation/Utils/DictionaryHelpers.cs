using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteTaskQueue.TaskCounter.Implementation.Utils
{
    internal static class DictionaryHelpers
    {
        public static void DeleteWhere<TK, TV>(this IDictionary<TK, TV> d, Func<KeyValuePair<TK, TV>, bool> needRemove)
        {
            var keys = d.Where(needRemove).Select(x => x.Key).ToArray();
            DeleteByKeys(d, keys);
        }

        private static void DeleteByKeys<TK, TV>(this IDictionary<TK, TV> d, IEnumerable<TK> keys)
        {
            foreach (var key in keys)
                d.Remove(key);
        }
    }
}