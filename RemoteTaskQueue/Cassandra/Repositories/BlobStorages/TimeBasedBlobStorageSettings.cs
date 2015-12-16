using System;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TimeBasedBlobStorageSettings
    {
        public TimeBasedBlobStorageSettings([NotNull] string keyspaceName, [NotNull] string largeBlobsCfName, [NotNull] string regularBlobsCfName)
        {
            KeyspaceName = keyspaceName;
            LargeBlobsCfName = largeBlobsCfName;
            RegularBlobsCfName = regularBlobsCfName;
        }

        [NotNull]
        public string KeyspaceName { get; private set; }

        [NotNull]
        public string LargeBlobsCfName { get; private set; }

        [NotNull]
        public string RegularBlobsCfName { get; private set; }

        /// <summary>
        ///     размер строки взяли 50mb
        ///     интенсивность записи взяли за 20 000 в минуту
        ///     6 минут tickPartition
        ///     10 - значение splittingFactor
        ///     blobSizeLimit = 50 *1024*1024/(6*20 000/10) примерно 4kb
        ///     если надо больше можно уменьшить tickPartition
        /// </summary>
        public const int RegularBlobSizeLimit = 4 * 1024;

        /// <summary>
        ///     пока сделали константой, в будущем можно будет сделать CF внутри которой хранить по тикам значение splittingFactor,
        ///     при старте загружать все в cache и по таймеру обновлять вновь добавленные.
        /// </summary>
        public const int SplittingFactor = 10;

        public static readonly long TickPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}