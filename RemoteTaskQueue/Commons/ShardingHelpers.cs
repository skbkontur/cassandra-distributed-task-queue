using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Commons
{
    internal static class ShardingHelpers
    {
        public static int GetShard(int hashCode, int shardsCount)
        {
            if (shardsCount <= 0)
                throw new InvalidOperationException($"ShardsCount should be positive: {shardsCount}");

            var longAbsHashCode = Math.Abs((long)hashCode);
            return (int)(longAbsHashCode % shardsCount);
        }
    }
}