using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.ColumnFamilyRegistration;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Storage.ColumnFamilyRegistration
{
    public sealed class SerializeToRowsStorageRegistry : AbstractColumnFamilyRegistryByType, ISerializeToRowsStorageRegistry
    {
        public SerializeToRowsStorageRegistry(
            ICassandraCoreSettings cassandraCoreSettings)
            : base(cassandraCoreSettings)
        {
        }
    }
}