using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class BlobStorageDecorator<T> : IBlobStorage<T>
    {
        public BlobStorageDecorator(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName, string timeBasedColumnFamilyName)
        {
            blobStorage = new BlobStorage<T>(parameters, serializer, globalTime, columnFamilyName);
            timeBasedBlobStorage = new TimeBasedBlobStorage<T>(parameters, serializer, globalTime, timeBasedColumnFamilyName);
        }

        public void Write([NotNull] string id, T element)
        {
            blobStorage.Write(id, element);

            TimeGuid timeGuid;
            if(TimeGuid.TryParse(id, out timeGuid))
                timeBasedBlobStorage.Write(timeGuid, element);
        }

        public bool TryWrite([NotNull] T element, out string id)
        {
            TimeGuid timeGuidId;
            if(!timeBasedBlobStorage.TryWrite(element, out timeGuidId))
                return blobStorage.TryWrite(element, out id);

            id = timeGuidId.ToGuid().ToString();
            blobStorage.Write(id, element);
            return true;
        }

        [CanBeNull]
        public T Read([NotNull] string id)
        {
            TimeGuid timeGuidId;
            return TimeGuid.TryParse(id, out timeGuidId) ? timeBasedBlobStorage.Read(timeGuidId) : blobStorage.Read(id);
        }

        public Dictionary<string, T> Read([NotNull] string[] ids)
        {
            var splitResult = Split(ids);
            var dictionary = blobStorage.Read(splitResult.BlobItems.ToArray());
            foreach(var pair in timeBasedBlobStorage.Read(splitResult.TimeBasedBlobItems.ToArray()))
                dictionary.Add(pair.Key.ToGuid().ToString(), pair.Value);

            return dictionary;
        }

        public IEnumerable<KeyValuePair<string, T>> ReadAll(int batchSize = 1000)
        {
            return blobStorage.ReadAll(batchSize);
        }

        public void Delete([NotNull] string id, long timestamp)
        {
            blobStorage.Delete(id, timestamp);

            TimeGuid timeGuidId;
            if(TimeGuid.TryParse(id, out timeGuidId))
                timeBasedBlobStorage.Delete(timeGuidId, timestamp);
        }

        public void Delete([NotNull] IEnumerable<string> ids, long? timestamp)
        {
            ids = ids.ToArray();
            blobStorage.Delete(ids, timestamp);

            var splitResult = Split(ids);
            if(splitResult.TimeBasedBlobItems.Any())
                timeBasedBlobStorage.Delete(splitResult.TimeBasedBlobItems, timestamp);
        }

        private static SplitResult<TimeGuid, string> Split([NotNull] IEnumerable<string> ids)
        {
            var result = new SplitResult<TimeGuid, string>();
            foreach(var id in ids)
            {
                TimeGuid timeGuidId;
                if(TimeGuid.TryParse(id, out timeGuidId))
                    result.TimeBasedBlobItems.Add(timeGuidId);
                else
                    result.BlobItems.Add(id);
            }
            return result;
        }

        private readonly BlobStorage<T> blobStorage;
        private readonly TimeBasedBlobStorage<T> timeBasedBlobStorage;

        private class SplitResult<TTimeBasedBlobItem, TBlobItem>
        {
            public SplitResult()
            {
                TimeBasedBlobItems = new List<TTimeBasedBlobItem>();
                BlobItems = new List<TBlobItem>();
            }

            public List<TTimeBasedBlobItem> TimeBasedBlobItems { get; private set; }
            public List<TBlobItem> BlobItems { get; private set; }
        }
    }
}