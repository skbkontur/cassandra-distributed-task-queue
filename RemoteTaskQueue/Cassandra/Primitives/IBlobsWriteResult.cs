using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Primitives
{
    public interface IBlobsWriteResult
    {
        bool IsSuccess { get; }
        HashSet<int> OutOfSizeLimitBlobIndexes { get; }
    }
}