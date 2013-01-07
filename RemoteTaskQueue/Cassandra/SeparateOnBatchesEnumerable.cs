using System.Collections;
using System.Collections.Generic;

namespace RemoteQueue.Cassandra
{
    public class SeparateOnBatchesEnumerable<T> : IEnumerable<T[]>
    {
        public SeparateOnBatchesEnumerable(IEnumerable<T> items, int batchSize)
        {
            this.items = items;
            this.batchSize = batchSize;
        }

        public IEnumerator<T[]> GetEnumerator()
        {
            return new SeparateOnBatchesEnumerator<T>(items, batchSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly IEnumerable<T> items;
        private readonly int batchSize;
    }
}