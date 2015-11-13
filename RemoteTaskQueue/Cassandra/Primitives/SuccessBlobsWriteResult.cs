using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Primitives
{
    public class SuccessBlobsWriteResult : IBlobsWriteResult
    {
        private SuccessBlobsWriteResult()
        {
        }

        public static SuccessBlobsWriteResult Instance { get { return SingletonInitializer.instance; } }
        public bool IsSuccess { get { return true; } }
        public HashSet<int> OutOfSizeLimitBlobIndexes { get { return null; } }

        private class SingletonInitializer
        {
            static SingletonInitializer()
            {
            }

            internal static readonly SuccessBlobsWriteResult instance = new SuccessBlobsWriteResult();
        }
    }
}