using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Primitives
{
    public class BlobsWriteResult : IBlobsWriteResult
    {
        public BlobsWriteResult()
        {
            OutOfSizeLimitBlobIndexes = new HashSet<int>();
        }

        public bool IsSuccess { get { return OutOfSizeLimitBlobIndexes.Count == 0; } }
        public HashSet<int> OutOfSizeLimitBlobIndexes { get; private set; }
    }
}