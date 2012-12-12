using SKBKontur.Catalogue.AccessControl.LocalImplementation;
using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.ColumnFamilyRegistration;
using SKBKontur.Catalogue.Core.CommonBusinessObjects.Parties;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Storage.ColumnFamilyRegistration
{
    public class SerializeToBlobStorageRegistry : AbstractColumnFamilyRegistryByType, ISerializeToBlobStorageRegistry
    {
        public SerializeToBlobStorageRegistry(
            ICassandraCoreSettings cassandraCoreSettings)
            : base(cassandraCoreSettings)
        {
            RegisterLocal<Party>("Party");
            RegisterLocal<PartySynonym>("PartySynonym");
            RegisterLocal<UserStorageElement>("UserStorageElement");
            RegisterLocal<AccessRuleStorageElement>("AccessRuleStorageElement");
            RegisterLocal<FtpUser>("FtpUser");
        }
    }
}