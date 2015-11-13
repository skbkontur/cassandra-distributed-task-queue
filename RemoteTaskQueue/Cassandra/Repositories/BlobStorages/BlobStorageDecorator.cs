using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class BlobStorageDecorator<T> : IBlobStorage<T>
    {
        public BlobStorageDecorator(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName, string timeBasedColumnFamilyName)
        {
            blobStorage = new BlobStorage<T>(parameters, serializer, globalTime, columnFamilyName);
            timeBasedBlobStorage = new TimeBasedBlobStorage<T>(parameters, serializer, globalTime, timeBasedColumnFamilyName);
        }

        public BlobWriteResult Write([NotNull] string id, T element)
        {
            TimeGuid timeGuidId;
            if(TimeGuid.TryParse(id, out timeGuidId) && timeBasedBlobStorage.Write(timeGuidId, element) == BlobWriteResult.Success)
                return BlobWriteResult.Success;

            return blobStorage.Write(id, element);
        }

        public IBlobsWriteResult Write([NotNull] IEnumerable<KeyValuePair<string, T>> elements)
        {
            var splitResult = KeyValuePairsSplit(elements);

            if(splitResult.TimeBasedBlobItems.Any())
            {
                var writeResult = timeBasedBlobStorage.Write(splitResult.TimeBasedBlobItems);
                if(!writeResult.IsSuccess)
                {
                    splitResult.BlobItems.AddRange(splitResult.TimeBasedBlobItems
                                                              .Where((timeBasedElement, index) => writeResult.OutOfSizeLimitBlobIndexes.Contains(index))
                                                              .Select(timeBasedElement => new KeyValuePair<string, T>(timeBasedElement.Key.ToGuid().ToString(), timeBasedElement.Value)));
                }
            }

            if(splitResult.BlobItems.Any())
                blobStorage.Write(splitResult.BlobItems);

            return SuccessBlobsWriteResult.Instance;
        }

        public T Read([NotNull] string id)
        {
            TimeGuid timeGuidId;
            return TimeGuid.TryParse(id, out timeGuidId) ? timeBasedBlobStorage.Read(timeGuidId) : blobStorage.Read(id);
        }

        public Dictionary<string, T> Read([NotNull] IEnumerable<string> ids)
        {
            var splitResult = Split(ids);
            var dictionary = blobStorage.Read(splitResult.BlobItems);
            foreach(var pair in timeBasedBlobStorage.Read(splitResult.TimeBasedBlobItems))
                dictionary.Add(pair.Key.ToGuid().ToString(), pair.Value);

            return dictionary;
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            return blobStorage.ReadAll(batchSize).Concat(timeBasedBlobStorage.ReadAll(batchSize));
        }

        public IEnumerable<KeyValuePair<string, T>> ReadAllWithIds(int batchSize = 1000)
        {
            return blobStorage.ReadAllWithIds(batchSize).Concat(timeBasedBlobStorage.ReadAllWithIds(batchSize).Select(x => new KeyValuePair<string, T>(x.Key.ToGuid().ToString(), x.Value)));
        }

        public void Delete([NotNull] string id, long timestamp)
        {
            TimeGuid timeGuidId;
            if(TimeGuid.TryParse(id, out timeGuidId))
                timeBasedBlobStorage.Delete(timeGuidId, timestamp);
            else
                blobStorage.Delete(id, timestamp);
        }

        public void Delete([NotNull] IEnumerable<string> ids, long? timestamp)
        {
            var splitResult = Split(ids);
            if(splitResult.TimeBasedBlobItems.Any())
                timeBasedBlobStorage.Delete(splitResult.TimeBasedBlobItems, timestamp);

            if(splitResult.BlobItems.Any())
                blobStorage.Delete(splitResult.BlobItems, timestamp);
        }

        private static SplitResult<KeyValuePair<TimeGuid, T>, KeyValuePair<string, T>> KeyValuePairsSplit([NotNull] IEnumerable<KeyValuePair<string, T>> items)
        {
            var result = new SplitResult<KeyValuePair<TimeGuid, T>, KeyValuePair<string, T>>();
            foreach(var item in items)
            {
                var id = item.Key;
                TimeGuid timeGuidId;
                if(TimeGuid.TryParse(id, out timeGuidId))
                    result.TimeBasedBlobItems.Add(new KeyValuePair<TimeGuid, T>(timeGuidId, item.Value));
                else
                    result.BlobItems.Add(item);
            }
            return result;
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