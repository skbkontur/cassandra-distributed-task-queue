using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class BlobStorageDecorator<T> : IBlobStorage<T>
    {
        public BlobStorageDecorator(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName, string orderedColumnFamilyName)
        {
            blobStorage = new BlobStorage<T>(parameters, serializer, globalTime, columnFamilyName);
            orderedBlobStorage = new OrderedBlobStorage<T>(parameters, serializer, globalTime, orderedColumnFamilyName);
        }

        public void Write(string id, T element)
        {
            if(IsTimeGuid(id))
                orderedBlobStorage.Write(id, element);
            else
                blobStorage.Write(id, element);
        }

        public void Write(KeyValuePair<string, T>[] elements)
        {
            var parts = Split(elements, element => element.Key);

            if(parts.Item1.Any())
                orderedBlobStorage.Write(parts.Item1);

            if(parts.Item2.Any())
                blobStorage.Write(parts.Item2);
        }

        public T Read(string id)
        {
            return IsTimeGuid(id) ? orderedBlobStorage.Read(id) : blobStorage.Read(id);
        }

        public T[] Read(string[] ids)
        {
            var parts = Split(ids, id => id);
            return orderedBlobStorage.Read(parts.Item1)
                                     .Concat(blobStorage.Read(parts.Item2))
                                     .ToArray();
        }

        public T[] ReadQuiet(string[] ids)
        {
            var parts = Split(ids, id => id);
            var orderedEntities = orderedBlobStorage.ReadQuiet(parts.Item1);
            var orderedEntitiesMap = parts.Item1.Zip(orderedEntities, (id, entity) => new KeyValuePair<string, T>(id, entity)).ToDictionary(x => x.Key, x => x.Value);

            var entities = blobStorage.ReadQuiet(parts.Item2);
            var entitiesMap = parts.Item2.Zip(entities, (id, entity) => new KeyValuePair<string, T>(id, entity)).ToDictionary(x => x.Key, x => x.Value);

            var result = new T[ids.Length];
            for(var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                if(orderedEntitiesMap.ContainsKey(id))
                {
                    result[i] = orderedEntitiesMap[id];
                    continue;
                }
                if(entitiesMap.ContainsKey(id))
                    result[i] = entitiesMap[id];
            }

            return result;
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            return blobStorage.ReadAll(batchSize).Union(orderedBlobStorage.ReadAll(batchSize));
        }

        public IEnumerable<KeyValuePair<string, T>> ReadAllWithIds(int batchSize = 1000)
        {
            return blobStorage.ReadAllWithIds(batchSize).Union(orderedBlobStorage.ReadAllWithIds(batchSize));
        }

        public void Delete(string id, long timestamp)
        {
            if(IsTimeGuid(id))
                orderedBlobStorage.Delete(id, timestamp);
            else
                blobStorage.Delete(id, timestamp);
        }

        public void Delete(string[] ids, long? timestamp)
        {
            var parts = Split(ids, id => id);
            if(parts.Item1.Any())
                orderedBlobStorage.Delete(parts.Item1, timestamp);

            if(parts.Item2.Any())
                blobStorage.Delete(parts.Item2, timestamp);
        }

        private static bool IsTimeGuid(string input)
        {
            Guid guid;
            return Guid.TryParse(input, out guid) && TimeGuidFormatter.GetVersion(guid) == GuidVersion.TimeBased;
        }

        private static Tuple<TItem[], TItem[]> Split<TItem>(IEnumerable<TItem> items, Func<TItem, string> getId)
        {
            var orderedBlobIds = new List<TItem>();
            var blobIds = new List<TItem>();
            foreach(var item in items)
            {
                var id = getId(item);
                if(IsTimeGuid(id))
                    orderedBlobIds.Add(item);
                else
                    blobIds.Add(item);
            }

            return new Tuple<TItem[], TItem[]>(orderedBlobIds.ToArray(), blobIds.ToArray());
        }

        private readonly IBlobStorage<T> blobStorage;
        private readonly IBlobStorage<T> orderedBlobStorage;
    }
}