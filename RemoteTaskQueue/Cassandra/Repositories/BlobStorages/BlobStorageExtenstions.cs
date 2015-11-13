using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public static class BlobStorageExtenstions
    {
        public static T[] ReadQuiet<T, TId>(this IBlobStorage<T, TId> storage, [NotNull] TId[] ids)
        {
            var result = new T[ids.Length];
            var dictionary = storage.Read(ids);
            for(var i = 0; i < ids.Length; i++)
            {
                if(dictionary.ContainsKey(ids[i]))
                    result[i] = dictionary[ids[i]];
            }
            return result;
        }
    }
}