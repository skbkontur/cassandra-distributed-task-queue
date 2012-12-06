using System;
using System.Security.Cryptography;
using System.Text;

using RemoteQueue.Settings;

namespace RemoteQueue.Handling
{
    public class ShardingManager : IShardingManager
    {
        private readonly IExchangeSchedulableRunnerSettings settings;

        public ShardingManager(IExchangeSchedulableRunnerSettings settings)
        {
            this.settings = settings;
        }

        public bool IsSituableTask(string taskId)
        {
            return Math.Abs(GetHash(taskId) % settings.ShardsCount) == (settings.ShardIndex - 1);
        }

        private byte GetHash(string s)
        {
            byte[] hash;
            using (var sha1 = new SHA1Managed())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(s ?? ""));
            }
            return hash[0];
        }
    }
}