namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public interface ICurrentMetaProvider
    {
        long NowTicks { get; }
        void Subscribe(IMetaConsumer c);
        void Unsubscribe(IMetaConsumer c);
        void Start();
        void Stop();
        MetaProviderSnapshot GetSnapshotOrNull(int maxLength);
    }
}