namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public interface IMetasLoader
    {
        void Reset(long startTicks);
        void Load(IMetaConsumer metaConsumer, long endTicks);
        void CancelLoadingAsync();
    }
}