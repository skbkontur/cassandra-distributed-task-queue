using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Commons
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Вычисляет хеш для строки, который никогда не меняется (в т.ч. не зависит от окружения)
        /// </summary>
        public static int GetPersistentHashCode(this string value)
        {
            var s = new char[value.Length + 1];
            Array.Copy(value.ToCharArray(), s, value.Length);
            s[value.Length] = (char)0;

            var hash1 = 5381;
            var hash2 = hash1;

            var index = 0;
            int c;
            while ((c = s[index]) != 0)
            {
                hash1 = ((hash1 << 5) + hash1) ^ c;
                c = s[index + 1];
                if (c == 0)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ c;
                index += 2;
            }
            return hash1 + (hash2 * 1566083941);
        }
    }
}