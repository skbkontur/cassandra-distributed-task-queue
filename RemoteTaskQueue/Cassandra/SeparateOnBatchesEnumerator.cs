using System.Collections;
using System.Collections.Generic;

namespace RemoteQueue.Cassandra
{
    public class SeparateOnBatchesEnumerator<T> : IEnumerator<T[]>
    {
        public SeparateOnBatchesEnumerator(IEnumerable<T> items, int batchSize)
        {
            this.items = items;
            this.batchSize = batchSize;
            Reset();
        }

        public void Dispose()
        {
            itemEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            while(true)
            {
                var list = new List<T>();
                for(var i = 0; i < batchSize && itemEnumerator.MoveNext(); ++i)
                    list.Add(itemEnumerator.Current);
                if(list.Count == 0)
                    return false;
                currentBatch = list.ToArray();
                return true;
            }
        }

        public void Reset()
        {
            itemEnumerator = items.GetEnumerator();
            currentBatch = null;
        }

        public T[] Current { get { return currentBatch; } }

        object IEnumerator.Current { get { return Current; } }

        private IEnumerator<T> itemEnumerator;
        private T[] currentBatch;
        private readonly IEnumerable<T> items;
        private readonly int batchSize;
    }
}