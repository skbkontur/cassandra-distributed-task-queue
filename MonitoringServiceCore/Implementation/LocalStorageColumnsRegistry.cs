using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.Indexes;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageColumnsRegistry : LocalStorageColumnsRegistryBase
    {
        public LocalStorageColumnsRegistry(ISqlDataTypeMapper sqlDataTypeMapper, IPropertiesExtracter propertiesExtracter)
            : base(sqlDataTypeMapper, propertiesExtracter)
        {
            Register(x => x.AllProperties(), new List<Expression<Func<MonitoringTaskMetadata, object>>[]>
                {
                    new Expression<Func<MonitoringTaskMetadata, object>>[]
                        {
                            x => x.MinimalStartTicks,
                            x => x.Id
                        }
                });
        }
    }
}