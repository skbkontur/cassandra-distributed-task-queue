using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.ColumnFamilyRegistration
{
    public class StoragePlugin
    {
        public static Type serializeToBlobStorageRegistry = typeof(SerializeToBlobStorageRegistry);
        public static Type serializeToRowsStorageRegistry = typeof(SerializeToRowsStorageRegistry);
        public static Type simpleColumnFamilyRegistry = typeof(SimpleColumnFamilyRegistry);
    }
}