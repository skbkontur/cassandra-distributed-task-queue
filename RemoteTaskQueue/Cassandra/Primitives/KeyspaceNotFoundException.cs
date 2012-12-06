using System;

namespace RemoteQueue.Cassandra.Primitives
{
    public class KeyspaceNotFoundException : Exception
    {
        public KeyspaceNotFoundException(string message)
            : base(message)
        {
        }
    }
}